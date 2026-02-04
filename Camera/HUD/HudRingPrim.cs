using UnityEngine;

public class HudRingPrim : IHUDObject, IHUDObjectData
{
    private IHUDObjectData hobj;
    float x, y;//,h,w;
    float r1;
    float r2;
    float w;
    float a1;
    float a2;
    int n;  // 0 if automatic
    int N;  //real n
    float tx;
    float ty;
    Color32 Color1;
    Color32 Color2;
    float percent;

    bool changed;
    IBill Bill;

    IBObject Mesh;
    Texture2D Texture;
    //Matrix2D M;

    public HudRingPrim(IBill _Bill, ITexturesDB _TexturesDB, float R1, float R2, float[] Point)
    {
        Bill = _Bill;
        tx = Point[0] / 256f;
        ty = Point[1] / 256f;
        Mesh = null;
        Texture = _TexturesDB.CreateTexture(Hasher.HshString("hud"));
        Color1 = new Color32(255, 0, 0, 128);
        Color2 = new Color32(0, 255, 0, 128);
        a1 = 0;
        a2 = Storm.Math.GRD2RD(360);
        Hide = false;
        n = 0;
    }
    public void SetR(float R)
    {
        r1 = R;
        r2 = r1 - w;
        changed = true;
    }
    public void SetW(float W)
    {
        w = W;
        r2 = r1 - w;
        changed = true;
    }
    public void SetBegin(float A1)
    {
        a1 = A1;
        changed = true;
    }
    public void SetEnd(float A2)
    {
        a2 = A2;
        changed = true;
    }
    public void SetXY(float X, float Y)
    {
        x = X;
        y = Y;
        changed = true;
    }
    public void SetPercent(float p)
    {
        percent = Mathf.Clamp(p, 0f, 1f);
    }
    public void SetN(int _N = 0)
    {
        n = _N;
        changed = true;
    }
    public void SetColors(Color32 C1, Color32 C2, float _opacity)
    {
        //TODO вернуть устрановку прозрачности
        Color1 = C1;//* opacity;
        Color2 = C2;//* opacity;
    }

    public virtual void Update(float scale)
    {
        if (!changed) return;
        changed = false;
        //TODO Реализовать отрисовку кольца
    }
    public virtual void BeginDraw()
    {
        if (a1 == a2) return;
        //Bill.PushTrasform(M);
    }

    public virtual void Draw()
    {
        if (a1 == a2) return;
        Bill.SetTexture(Texture);
        Bill.Draw(Mesh);
    }
    public virtual void EndDraw()
    {
        if (a1 == a2) return;
        //Bill.PopTrasform();
    }
    public virtual HudDeviceData GetData()
    {
        return null;
    }

    public bool IsHidden()
    {
        return hobj.IsHidden();
    }


    public bool Hide
    {
        get
        {
            return hobj.IsHidden();
        }
        set
        {
            hobj.SetHide(value);
        }
    }

    public HUDTree Tree
    {
        get
        {
            return hobj.GetTree();
        }
        set
        {
            hobj.SetTree(value);
        }
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
        throw new System.NotImplementedException();
    }

    ~HudRingPrim()
    {
        //Mesh->Release();
        //SafeRelease(Texture);
    }

}
