using UnityEngine;

public class HudRecticles : IHUDObject, IHUDObjectData
{
    IHUDObjectData hobj;
    HudRecticlesData Data;
    float stage;
    IBill Bill;
    ITexturesDB TexturesDB;
    BillStyle HUDstyle;
    HUDTexelData TexelData;
    public HudRecticles(IBill _Bill, ITexturesDB _TexturesDB, BillStyle _HUDstyle, HudRecticlesData _Data, HUDTexelData _TexelData)
    {
        HUDstyle = _HUDstyle;
        Bill = _Bill;
        TexturesDB = _TexturesDB;
        HUDstyle = _HUDstyle;
        Data = _Data;
        TexelData = _TexelData;

        hobj = new HUDObjectImpl();
    }
    public virtual void Update(float scale)
    {
        stage += scale;
        while (stage > Data.speed) stage -= Data.speed;
        for (HudRecticleData I = Data.Items.Head(); I != null; I = I.Next())
        {
            I.stage = Mathf.Abs(stage / Data.speed - .5f) * 2f;
            if (I.Dev == null) hobj.GetTree().Insert(new HudRecticle(Bill, TexturesDB, I, TexelData, Data.colors));
            I.Dev.SetHide(I.hide);
        }

    }
    public virtual void BeginDraw()
    {
        //Bill->SetStyle(BillStyle(
    }
    public virtual void Draw() { }
    public virtual void EndDraw() { }
    public virtual HudDeviceData GetData()
    {
        return Data;
    }

    public bool IsHidden()
    {
        return hobj.IsHidden();
    }

    public void SetHide(bool off)
    {
        hobj.SetHide(off);
    }

    public void SetTree(HUDTree tree)
    {
        hobj.SetTree(tree);
    }

    public HUDTree GetTree()
    {
        return hobj.GetTree();
    }

    public void Dispose()
    {
        Data.ClearList();
    }

    ~HudRecticles()
    {
        Dispose();
    }
}
