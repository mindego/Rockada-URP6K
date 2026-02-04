using static HudData;
public class HudDesktop : IHUDObject
{
    //От IHUDObject
    HUDObjectImpl hudobj;
    HudDesktopData Data;
    Matrix2D MInit = new Matrix2D();
    IBill Bill;


    public HudDesktop(IBill _Bill, ITexturesDB _TexturesDB, HudDesktopData _Data)
    {
        hudobj = new HUDObjectImpl();
        Bill = _Bill;
        Data = _Data;
        Data.CurrSize = 0;
        SetHide(false);
        //Hide = false;
    }
    public void Update(float scale) {
        if ((Data.state & HUDSTATE_OFF)!=0) return;
        if (Data.speed > 0)
        {
            if (Data.CurrSize != 1f)
                Data.CurrSize += scale * Data.speed;
            if (Data.CurrSize > 1f) Data.CurrSize = 1f;
            Data.changed = true;
        }
        if (Data.speed < 0)
        {
            if (Data.CurrSize != 0)
                Data.CurrSize += scale * Data.speed;
            if (Data.CurrSize < 0f) Data.CurrSize = 0f;
            Data.changed = true;
        }

        if (Data.changed)
        {
            Data.changed = false;
            float qx = 5f;
            MInit.Identity();
            MInit.Move(Data.x, Data.y);
            MInit.Scale(Data.w, Data.h / 0.75f);

            MInit.Move(0.5f, .375f);
            MInit.Scale(((qx - 1f) / qx + Data.CurrSize / qx), Data.CurrSize);
            MInit.Move(-0.5f, -.375f);
            //UnityEngine.Debug.Log("Desktop transform:" + MInit);
        }
        //M.Rotate(0.5,0.75/2,GRD2RD(1));
    }
    public void BeginDraw() {
        if ((Data.state & HUDSTATE_OFF)!=0) return;
        //Matrix2D M;
        //M.Identity();
        Bill.PushTrasform(MInit);
    }
    public void Draw() { }
    public void EndDraw() {
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        Bill.PopTrasform();
    }
    public HudDeviceData GetData() { return Data; }

    public void Dispose()
    {
        throw new System.NotImplementedException();
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

    ~HudDesktop() { }


}
