using UnityEngine;
using static HudData;

public class HudWeapon : IHUDObject
{
    //IHUDObject
    HUDObjectImpl hudobj;
    //Своё.

    Matrix2D M = new Matrix2D();
    IBill Bill;
    IFont StormFont;
    IBObject Box;
    IBObject Bar;
    IBObject Current;

    HudWeaponData Data;
    //IObject Texture;
    Texture2D Texture;
    HUDTexelData TexelData;
    //const float font_scale = 1 / 640.0f;
    float FontHeight; //= 4f * font_scale; //TODO необходимо учитывать размер шрифта, а не брать его из константы
    void DrawObject(WeaponData wd)
    {
        //TODO Восстановить прозрачность HUD
        Color color = Data.colors.colors[wd.colour];
        color *= Data.opacity;
        Color Color2 = Data.colors.disabled;
        Color2 *= Data.opacity;
        //Color2.a/=4;
        Color boxcolor = Data.colors.colors[Data.boxcolour];
        boxcolor *= Data.opacity;
        //Закомментировано в самом коде Шторма
        //color.a*=Data.opacity;
        //Color2.a*=Data.opacity;
        //boxcolor.a*=Data.opacity;
        if (wd.box)
        {
            Box.Lock();
            Box.SetVertC(0, 4, boxcolor, Color.black);
            Box.UnLock();
            Bill.Draw(Box);
        }
        if (wd.current)
        {
            Current.Lock();
            Current.SetVertC(0, 4, color, Color.black);
            Current.UnLock();
            Bill.Draw(Current);
        }
        StormFont.Puts(color, new Vector2(Data.spacing, Data.spacing + Data.fonty / 640f), wd.text);
        if (wd.tape == 0)
        {
            //char Buff[32];
            //wsprintf(Buff, "%d", int(wd.value));
            //Font.Puts(color, FVec2(Data.w - Data.spacing - Font.Width(Buff), Data.spacing + Data.fonty / 640.), Buff);
            string Buff = ((int)wd.value).ToString();
            StormFont.Puts(color, new Vector2(Data.w - Data.spacing - StormFont.Width(Buff), Data.spacing + Data.fonty / 640f), Buff);
        }
        else
        {
            //TODO реализовать корректную отрисовку заряда энергооружия
            float part = wd.value;
            float x1 = Data.w - Data.scalesize - Data.spacing;
            float y1 = Data.spacing;
            float x2 = Data.w - Data.spacing;
            float y2 = Data.spacing + FontHeight;
            float xc = x1 + (x2 - x1) * part;
            float tx1 = TexelData.WBar[0] / 256f;
            float ty1 = TexelData.WBar[1] / 256f;
            float tx2 = TexelData.WBar[2] / 256f;
            float ty2 = TexelData.WBar[3] / 256f;
            Bar.name = "Bar " + (y1, y2) + " " + (ty1, ty2);

            float txc = (TexelData.WBar[0] + (TexelData.WBar[2] - TexelData.WBar[0]) * part) / 256f;
            //float txc1=(TexelData.WBar[0]+(TexelData.WBar[2]-TexelData.WBar[0])*part)/256.;

            //Bar.SetFace(0, 0, 1, 2);
            //Bar.SetFace(1, 1, 2, 3);
            //Bar.SetFace(2, 4, 5, 6);
            //Bar.SetFace(3, 5, 6, 7);
            Bar.Lock();


            { //Оригинал
                Bar.SetVertO(0, new Vector2(x1, y1), new Vector2(tx1, ty1));// |
                Bar.SetVertO(1, new Vector2(x1, y2), new Vector2(tx1, ty2));// |
                                                                            // |
                Bar.SetVertO(2, new Vector2(xc, y1), new Vector2(txc, ty1));// |
                Bar.SetVertO(3, new Vector2(xc, y2), new Vector2(txc, ty2));// |_ Первый bar
                Bar.SetVertO(4, new Vector2(xc, y1), new Vector2(txc, ty1));// |
                Bar.SetVertO(5, new Vector2(xc, y2), new Vector2(txc, ty2));// |
                                                                            // |
                Bar.SetVertO(6, new Vector2(x2, y1), new Vector2(tx2, ty1));// |
                Bar.SetVertO(7, new Vector2(x2, y2), new Vector2(tx2, ty2));// |_ Второй bar
            }

            Bar.SetVertC(0, 4, color, Color.black);
            Bar.SetVertC(4, 4, Color2, Color.black);

            Bar.UnLock();
            Bill.Draw(Bar);
        }
    }
    public HudWeapon(IBill _Bill, ITexturesDB _TexturesDB, HudWeaponData _Data, HUDTexelData _TexelData)
    {
        hudobj = new HUDObjectImpl();
        Bill = _Bill;

        Data = _Data;
        TexelData = _TexelData;
        StormFont = Bill.CreateFont("Small", Data.fontx, Data.fonty);

        FontHeight = StormFont.Height();

        Texture = _TexturesDB.CreateTexture(TexelData.TextureName);
        //Box = Sq.Create(Bill, Data.colors.human, Data.opacity, 0, 0, Data.w, Data.spacing * 2 + Font.Height(), TexelData.WBox);
        Box = Sq.Create(Bill, Data.colors.human, Data.opacity, 0, 0, Data.w, Data.spacing * 2 + FontHeight, TexelData.WBox);
        Bar = Bill.Create(4 * 2, 2 * 2);
        Box.name = "Box";
        Bar.Lock();
        Bar.SetFace(0, 0, 1, 2);
        Bar.SetFace(1, 1, 2, 3);
        Bar.SetFace(2, 4, 5, 6);
        Bar.SetFace(3, 5, 6, 7);
        Bar.name = "Bar";
        Bar.UnLock();
        Current = Bill.Create(4, 2);
        Current.name = "Current";
        Texture = _TexturesDB.CreateTexture(TexelData.TextureName);
        Data.Items.Free();
        Data.changed = true;
    }
    public void Update(float scale)
    {
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        if (Data.changed)
        {
            Box.Lock();
            Sq.Add(Box, 0, Bill, Data.colors.human, Data.opacity, 0, 0, Data.w, Data.spacing * 2 + FontHeight, TexelData.WBox);
            Box.UnLock();
            float size_2 = FontHeight / 4;
            float center = FontHeight / 2 + Data.spacing;

            Current.Lock();
            Sq.Add(Current, 0, Bill, Data.colors.human, Data.opacity, -2 * size_2 - Data.spacing, center - size_2, -Data.spacing, center + size_2, TexelData.Select);
            Current.UnLock();
            StormFont.SetWH(Data.fontx, Data.fonty);
        }
    }
    public void BeginDraw() { }
    public void Draw()
    {
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        float y = Data.y;

        Bill.SetTexture(Texture);

        for (WeaponData I = Data.Items.Head(); I != null; I = I.Next())
        {
            M.Identity();
            M.Move(Data.x, y);
            Bill.PushTrasform(M);
            DrawObject(I);
            Bill.PopTrasform();
            y += Data.step;
        }
    }
    public void EndDraw() { }
    public HudDeviceData GetData()
    {
        return Data;
    }


    public void Dispose()
    {
        Data.Items.Free();
        //SafeRelease(Texture);
        Texture = null;
        Current.Release();
        Bar.Release();
        Box.Release();
        //Font.Release();
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

    ~HudWeapon()
    {
        Dispose();
    }
}
