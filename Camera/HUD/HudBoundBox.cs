public class HudBoundBox : IHUDObject, IHUDObjectData
{
    private IHUDObjectData hobj;
    protected IBill Bill;
    protected ITexturesDB TexturesDB;
    protected HUDTexelData TexelData;
    protected HudColors colors;
    HudPrim[] Line = new HudPrim[4];
    HudPrim[] Angle = new HudPrim[4];

    float x, y, w, h;
    float linew;
    float maxboundsize;
    bool uselines;
    float bh, bw;        //bound h,w
    float tbh, tbw;      //bound h,w on texture;
    float tlw;          //line w on texture;
    int colour;
    bool changed;
    float opacity;

    public HudBoundBox(IBill _Bill, ITexturesDB _TexturesDB, HUDTexelData _TexelData, HudColors _colors, float _opacity)
    {
        hobj = new HUDObjectImpl();

        Bill = _Bill;
        TexturesDB = _TexturesDB;
        TexelData = _TexelData;
        colors = _colors;
        opacity = _opacity;
        changed = true;
        Hide = false;
        uselines = true;
        //Line[0]=Line[1]=Line[2]=Line[3]=0;
        //Angle[0]=Angle[1]=Angle[2]=Angle[3]=0;
        Line[0] = new HudPrim(Bill, TexturesDB, colors, opacity, TexelData.LineTop);
        Line[0].Line.name = "LineTop";
        Line[1] = new HudPrim(Bill, TexturesDB, colors, opacity, TexelData.LineLeft);
        Line[1].Line.name = "LineLeft";
        Line[2] = new HudPrim(Bill, TexturesDB, colors, opacity, TexelData.LineBottom);
        Line[2].Line.name = "LineBottom";
        Line[3] = new HudPrim(Bill, TexturesDB, colors, opacity, TexelData.LineRight);
        Line[3].Line.name = "LineRight";

        Angle[0] = new HudPrim(Bill, TexturesDB, colors, opacity, TexelData.TargetNW);
        Angle[0].Line.name = "TargetNW";
        Angle[1] = new HudPrim(Bill, TexturesDB, colors, opacity, TexelData.TargetSW);
        Angle[1].Line.name = "TargetSW";
        Angle[2] = new HudPrim(Bill, TexturesDB, colors, opacity, TexelData.TargetSE);
        Angle[2].Line.name = "TargetSE";
        Angle[3] = new HudPrim(Bill, TexturesDB, colors, opacity, TexelData.TargetNE);
        Angle[3].Line.name = "TargetNE";
        tbh = TexelData.TargetNW[1] - TexelData.TargetNW[3];
        tbw = TexelData.TargetNW[0] - TexelData.TargetNW[2];
        tlw = TexelData.LineTop[1] - TexelData.LineTop[3];
    }
    public void SetXY(float _x, float _y)
    {
        x = _x;
        y = _y;
        changed = true;
    }
    public void SetWH(float _w, float _h) {
        h = _h / 2;
        w = _w / 2;
        changed = true;
    }
    public void SetColour(int c, float _opacity)
    {
        colour = c;
        opacity = _opacity;
        changed = true;
    }
    public void UseLine(bool ul)
    {
        uselines = ul;
        changed = true;
    }
    public void SetLineW(float lw)
    {
        linew = lw;
        changed = true;
    }
    void SetSomething()
    {
        changed = true;
    }
    public virtual void Update(float scale)
    {
        if (!changed) return;
        //if(!Tree) return;
        changed = false;
        bh = tbh * linew / tlw;
        bw = tbw * linew / tlw;
        if ((bh > h) || (bw > w))
        {
            float lw1 = tlw * h / tbh;
            float lw2 = tlw * w / tbw;
            linew = lw1 < lw2 ? lw1 : lw2;
            bh = tbh * linew / tlw;
            bw = tbw * linew / tlw;
        }
        Angle[0].SetXY(x - w, y - h);
        Angle[0].SetWH(bw, bh);
        Angle[0].SetColour(colour, opacity);
        Angle[0].Update(scale);

        Angle[1].SetXY(x - w, y + h - bh);
        Angle[1].SetWH(bw, bh);
        Angle[1].SetColour(colour, opacity);
        Angle[1].Update(scale);

        Angle[2].SetXY(x + w - bw, y + h - bh);
        Angle[2].SetWH(bw, bh);
        Angle[2].SetColour(colour, opacity);
        Angle[2].Update(scale);

        Angle[3].SetXY(x + w - bw, y - h);
        Angle[3].SetWH(bw, bh);
        Angle[3].SetColour(colour, opacity);
        Angle[3].Update(scale);

        Line[0].SetXY(x - w + bw, y - h);
        Line[0].SetWH(2f* (w - bw), linew);
        Line[0].SetColour(colour, opacity);
        Line[0].Update(scale);

        Line[1].SetXY(x - w, y - h + bh);
        Line[1].SetWH(linew, 2f* (h - bh));
        Line[1].SetColour(colour, opacity);
        Line[1].Update(scale);

        Line[2].SetXY(x - w + bw, y + h - linew);
        Line[2].SetWH(2f* (w - bw), linew);
        Line[2].SetColour(colour, opacity);
        Line[2].Update(scale);

        Line[3].SetXY(x + w - linew, y - h + bh);
        Line[3].SetWH(linew, 2f* (h - bh));
        Line[3].SetColour(colour, opacity);
        Line[3].Update(scale);
    }
    public virtual void BeginDraw() { }
    public virtual void Draw()
    {
        for (int i = 0; i < 4; ++i) Angle[i].Draw();
        if (uselines)
        {
            for (int i = 0; i < 4; ++i) Line[i].Draw();
        }
    }
    public virtual void EndDraw() { }
    public virtual HudDeviceData GetData()
    {
        return null;
    }

    public bool Hide
    {
        get
        {
            return hobj.IsHidden();
        }
        set
        {
            SetHide(value);
        }
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
        throw new System.NotImplementedException();
    }

    ~HudBoundBox()
    {

    }

}