using static HudData;
using UnityEngine;

class HudDamage : IHUDObject
{
    //IHUDObject
    IHUDObjectData hudobj;

    Matrix2D M = new Matrix2D();
    IBill Bill;
    HudDamageData Data;
    //IObject Texture;
    Texture2D Texture;
    ITexturesDB TexturesDB;

    public HudDamage(IBill _Bill, ITexturesDB _TexturesDB, HudDamageData _Data)
    {
        hudobj = new HUDObjectImpl();
        Bill = _Bill;
        TexturesDB = _TexturesDB;
        Data = _Data;
        Texture = null;
        Data.changed = true;
        
    }
    public void Update(float f)
    {
        //Debug.Log("Updating " + this);
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        if (Data.changed)
        {
            M.Identity();
            M.Move(Data.x, Data.y);
            M.Scale(Data.w / 256f, Data.h / 256f);
        }

        if (Data.NewTexture)
        {
            Data.NewTexture = false;
            //SafeRelease(Texture);
            Texture = TexturesDB.CreateTexture(Hasher.HshString(Data.Texture));
            for (DamageData I = Data.List.Head(); I != null; I = I.Next())
            {
                if (I.BillObj == null)
                {
                    I.BillObj = Sq.Create(Bill, Data.colors.colors[Data.colour], Data.opacity, I.xy[0], I.xy[1], I.xy[0] + I.xy[2], I.xy[1] + I.xy[3], I.txy[0], I.txy[1], I.txy[2], I.txy[3]);
                    I.BillObj.name = I.name;
                    //Debug.Log("Created I.BillObj " + I.BillObj.name + " " + new Vector4(I.txy[0], I.txy[1], I.txy[2], I.txy[3]) + " " + ((BObject) I.BillObj).GetTexRect()) ;
                    Debug.Log("Created I.BillObj " + I.BillObj.name + " " + new Vector4(I.xy[0], I.xy[1], I.xy[2], I.xy[3]));
                }

            }
        }
    }
    public void BeginDraw()
    {
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        Bill.SetTexture(Texture);
        Bill.PushTrasform(M);
    }
    public void Draw()
    {

        if ((Data.state & HUDSTATE_OFF) != 0) return;
        for (DamageData I = Data.List.Head(); I != null; I = I.Next())
        {
            Asserts.AssertBp(I.BillObj != null);
            Color32 myColor = Data.colors.colors[I.colour];//TODO Корректно рисовать прозрачность * Data.opacity;
            //Color.a*=Data.opacity;
            //TODO корректно устанавливать прозрачность для Bill
            I.BillObj.Lock();
            I.BillObj.SetVertC(0, 4, myColor, Color.black);
            I.BillObj.UnLock();

            Bill.Draw(I.BillObj);
        }
    }
    public void EndDraw() {
        if ((Data.state & HUDSTATE_OFF)!=0) return;
        Bill.PopTrasform();
    }
    public HudDeviceData GetData()
    {
        return Data;
    }

    public void Dispose()
    {
        Texture = null;
    }

    public bool IsHidden()
    {
        return hudobj.IsHidden();
    }

    public void SetHide(bool off)
    {
        hudobj.SetHide(off);
    }

    public void SetTree(HUDTree tree)
    {
        hudobj.SetTree(tree);
    }

    public HUDTree GetTree()
    {
        return hudobj.GetTree();
    }

    ~HudDamage()
    {
        Dispose();
    }
}
