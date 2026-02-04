using static HudData;
using UnityEngine;
using static HudDataColors;

class HudPrim : IHUDObject, IHUDObjectData
{
    IHUDObjectData hobj;
    IBill Bill;
    public IBObject Line;
    HudColors Colors;
    BillStyle MyStyle;
    bool UseStyle;
    Matrix2D M;
    bool UseMatrix;

    float x, y, h, w;
    int colour;
    float opacity;
    int state;
    bool changed;

    Texture2D myTexture;

    public HudPrim(IBill _Bill, ITexturesDB _TexturesDB, HudColors colors, float _opacity, float[] Texels)
    {
        hobj = new HUDObjectImpl();
        Bill = _Bill;
        Colors = colors;
        colour = HUDCOLOR_NORMAL;
        opacity = _opacity;
        x = y = h = w = state = 0;
        Line = Sq.Create(Bill, Colors.colors[colour], opacity, x, y, x + w, y + h, Texels);
        SetHide(false);
        changed = false;
        UseStyle = false;
        UseMatrix = false;

        Line.name = "HudPrim " + GetHashCode().ToString("X8");
        //todo Ну не должно здесь переключсться
        myTexture = _TexturesDB.CreateTexture(Hasher.HshString("hud"));
    }
    public void SetXY(float _x, float _y)
    {
        //Debug.Log("SetXY " + _x + ":" + _y);
        x = _x;
        y = _y;
        changed = true;
    }
    public void SetWH(float _w, float _h)
    {
        //Debug.Log("SetWH " + _w + ":" + _h);
        w = _w;
        h = _h;
        changed = true;
    }
    public void SetColour(int c, float _opacity)
    {
        if (c < HUDCOLOR_MAX)
        {
            colour = c;
            opacity = _opacity;
            changed = true;
        }
    }
    public void SetState(int st)
    {
        state = st;
        changed = true;
    }
    public  void SetStyle(BillStyle HStyle)
    {
        if (HStyle != null)
        {
            UseStyle = true;
            MyStyle.blend = HStyle.blend;
            MyStyle.force = HStyle.force;
            MyStyle.write = HStyle.write;
        }
        else
        {
            UseStyle = false;
        }
    }
    public void SetMatrix(Matrix2D _M)
    {
        if (_M!=null)
        {
            UseMatrix = true;
            M = _M; //TODO - Возможно, правильнее копировать
        }
        else
        {
            UseMatrix = false;
        }
    }


    public virtual void Update(float f)
    {
        if (!changed) return;
        Line.Lock();
        Sq.Set(ref Line, 0, Bill, x, y, x + w, y + h);

        Color32 myColor = Colors.colors[colour];// * opacity;
        //Color.a*=opacity;
        Line.SetVertC(0, 4, myColor, Color.black);
        Line.UnLock();
    }
    public virtual void BeginDraw()
    {
        if (UseMatrix) Bill.PushTrasform(M);

        Bill.SetTexture(myTexture);
    }
    public virtual void Draw()
    {
        if ((state & HUDSTATE_OFF)!=0) return;
        if (UseStyle)
            Bill.SetStyle(MyStyle);
        Bill.Draw(Line);
    }
    public virtual void EndDraw()
    {
        if (UseMatrix) Bill.PopTrasform();
    }
    public virtual HudDeviceData GetData()
    {
        return null;
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

    ~HudPrim()
    {
        Dispose();
    }

    public void Dispose()
    {
        Line.Release();
    }
}