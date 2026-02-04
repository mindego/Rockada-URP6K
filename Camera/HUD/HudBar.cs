using static HudData;
using UnityEngine;

public class HudBar : IHUDObject
{
    //IHUDObject
    IHUDObjectData hudobj;

    Matrix2D M;
    IBill Bill;

    IBObject Scale;
    IFont Font;
    Color TextColor;

    IBClipper Clipper;

    HudBarData Data;
    Texture2D Texture;
    HUDTexelData TexelData;
    void Indi(float value, Color Color, Color ColorOff)
    {
        float[] Texels = TexelData.Bar[Data.scale];
        float x2 = Data.x + Data.w;
        float y2 = Data.y + Data.h;
        if (Data.vert)
        {
            float yv = Data.y + Data.h * (1 - value);
            float tyv = Texels[1] + (Texels[3] - Texels[1]) * (1 - value);
            Sq.Add(Scale, 0, Bill, ColorOff, Data.opacity,
              Data.x, Data.y, x2, yv,
              Texels[0], Texels[1], Texels[2], tyv);
            Sq.Add(Scale, 1, Bill, Color, Data.opacity,
              Data.x, yv, x2, y2,
              Texels[0], tyv, Texels[2], Texels[3]);
        }
        else
        {
            float xv = Data.x + Data.w * (value);
            float tyv = Texels[1] + (Texels[3] - Texels[1]) * (value);
            Sq.AddR(Scale, 0, Bill, Color, Data.opacity,
              Data.x, Data.y, xv, y2,
              Texels[0], Texels[1], Texels[2], tyv);
            Sq.AddR(Scale, 1, Bill, ColorOff, Data.opacity,
              xv, Data.y, x2, y2,
              Texels[0], tyv, Texels[2], Texels[3]);
        }
    }
    public HudBar(IBill _Bill, ITexturesDB _TexturesDB, HudBarData _Data, HUDTexelData _TexelData)
    {
        hudobj = new HUDObjectImpl();

        Bill = _Bill;
        Data = _Data;
        TexelData = _TexelData;
        Bill = _Bill;
        Font = Bill.CreateFont("Small");
        Scale = Bill.Create(2 * 4, 2 * 2);
        Texture = _TexturesDB.CreateTexture(TexelData.TextureName);
        TextColor = Data.colors.normal;// * Data.opacity;

        M = new Matrix2D();
        M.Identity();
    }
    public void Update(float scale)
    {
        //TODO Восстановить прозрачность элемента HUD
        if ((Data.state & HUDSTATE_OFF) != 0) return;

        //Data.value=scale;
        float value = (Data.state & HUDSTATE_BAR) != 0 ? (float)((int)(Data.value * TexelData.BarCount[Data.scale])) / TexelData.BarCount[Data.scale] : Data.value;
        float lowmax = (Data.state & HUDSTATE_BAR) != 0 ? (float)((int)(Data.lowmax * TexelData.BarCount[Data.scale])) / TexelData.BarCount[Data.scale] : Data.lowmax;
        float hightmin = (Data.state & HUDSTATE_BAR) != 0 ? (float)((int)(Data.hightmin * TexelData.BarCount[Data.scale])) / TexelData.BarCount[Data.scale] : Data.hightmin;

        Color OffColor = Data.colors.colors[Data.colour_off];// * Data.opacity;
        //value = min(1, max(0, value));
        value = Mathf.Min(1, Mathf.Max(0, value));
        if (value < lowmax)
        {
            Data.colour = (int)Data.colour_low;
        }
        else if (value < hightmin)
        {
            Data.colour = (int)Data.colour_normal;
        }
        else
        {
            Data.colour = (int)Data.colour_hight;
        }
        TextColor = Data.colors.colors[Data.colour];// * Data.opacity;
        Scale.Lock();
        Indi(value, TextColor, OffColor);
        Scale.UnLock();
    }
    public void BeginDraw()
    {
        Bill.SetTexture(Texture);
    }
    public void Draw()
    {
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        Bill.Draw(Scale);
        //if (Data.text!=null && Data.text[0]!='\0')
        if (Data.text != null && Data.text.Length > 0)
        {
            if (Data.vert)
            {
                Vector2 TextXY = new Vector2(Data.x - Font.Height(), Data.y + Data.h);
                M.Identity();
                M.Rotate(TextXY.x, TextXY.y, Storm.Math.GRD2RD(-90));
                Bill.PushTrasform(M);
                Font.Puts(TextColor, TextXY, Data.text);
                Bill.PopTrasform();
            }
            else
            {
                Vector2 TextXY = new Vector2(Data.x, Data.y - Font.Height());
                //Font.Puts(Data.colors.normal * Data.opacity, TextXY, Data.text);
                Font.Puts(Data.colors.normal, TextXY, Data.text);

            }
        }
    }
    public void EndDraw() { }
    public HudDeviceData GetData()
    {
        return Data;
    }
    ~HudBar()
    {
        Dispose();
    }
    public void Dispose()
    {
        //SafeRelease(Texture);
        Texture = null;
        Scale.Release();
        Font.Release();
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
}