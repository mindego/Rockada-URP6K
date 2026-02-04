using static HudData;
using UnityEngine;

class HudCompas : IHUDObject, IHUDObjectData
{
    IHUDObjectData hobj = new HUDObjectImpl();
    Matrix2D M = new Matrix2D();
    //RectTransform M;
    Vector2 TextXY;
    IBill Bill;

    BillStyle HUDstyle;
    //IFont* DigFont;
    Texture2D Texture;
    IBObject Pointer;
    IBObject PointerR;
    IBObject PointerL;
    IBObject Scale;
    IBObject Box;
    IBObject TScale;
    IBClipper Clipper;

    IBClipper ViewAngleClipper;
    HudCompasData Data;

    void ReCalc()
    {
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        if (!Data.changed) return;
        Data.changed = false;

        M.Identity();
        M.Move(Data.x, Data.y);
        M.Scale(Data.w / Data.viewcompasangle, Data.h);
        //SafeRelease(Clipper);
        //SafeRelease(ViewAngleClipper);
        {
            //4
            Vector2[] Clip = new Vector2[] { new Vector2(-Data.viewcompasangle / 2, 0), new Vector2(Data.viewcompasangle / 2, 0), new Vector2(Data.viewcompasangle / 2, 1), new Vector2(-Data.viewcompasangle / 2, 1) };
            Clipper = Bill.CreateClipper(4, Clip);
        }
        {
            Vector2[] Clip = { new Vector2(-Data.angle_view / 2, 0), new Vector2(Data.angle_view / 2, 0), new Vector2(Data.angle_view / 2, 1), new Vector2(-Data.angle_view / 2, 1) };
            ViewAngleClipper = Bill.CreateClipper(4, Clip);
        }
    }


    public HudCompas(IBill _Bill, ITexturesDB _TexturesDB, BillStyle style, HudCompasData _Data, HUDTexelData TexelData)
    {
        HUDstyle = style;
        Bill = _Bill;
        Data = _Data;
        Data.angle = 0;
        Data.pointer = 0;
        Texture = _TexturesDB.CreateTexture(TexelData.TextureName);

        Clipper = null;
        ViewAngleClipper = null;

        Pointer = Sq.Create(Bill, Data.colors.colors[Data.colour_pointer], Data.opacity, -2, 0.7f, 2, 1, TexelData.TargetUp);
        Pointer.name = "TargetUp";
        PointerL = Sq.Create(Bill, Data.colors.colors[Data.colour_pointer], Data.opacity, -2, 0.7f, 2, 1, TexelData.TargetLeft);
        PointerL.name = "TargetLeft";
        PointerR = Sq.Create(Bill, Data.colors.colors[Data.colour_pointer], Data.opacity, -2, 0.7f, 2, 1, TexelData.TargetRight);
        PointerL.name = "TargetRight";
        Scale = Sq.Create(Bill, Data.colors.colors[Data.colour], Data.opacity, 0, 0.4f, 30, .7f, TexelData.CompasScale);
        Scale.name = "CompasScale";
        Box = Sq.Create(Bill, Data.colors.colors[Data.colour], Data.opacity, -12, 0, 12, 0.8f, TexelData.CompasBox);
        Box.name = "CompasBox";
        //DigFont = Bill.CreateFont("Digits", 640./ 7.* 6., 480./ 12.* 0.8);
        /////////////////////////////////
        //Шрифт для системы коорлинат: X-градусы[-90,90], Y-[0,1] 
        //
        //640x480 предположительный размер экрана для которого размер пикселя равен текселю
        //12- высота цифры в текселях
        //0.8 - размер цифры по Y
        // размер по Х в текселях и градусах =7


        //TextXY = new Vector2(-DigFont.Width("000") / 2, -(DigFont.Height() - 0.8) / 2);

        TScale = Bill.Create(12 * 4, 12 * 2); // Teкстовая 

        float bX = -4; //
        float eX = 4;  // Размер числовых значений на шкале в градусах(единицах шкалы)
        float bX2 = -2;//                                                                 
        float eX2 = 2; // Размер символьных значений на шкале в градусах(единицах шкалы)    

        float bY = 0.1f;
        float eY = 0.3f;

        TScale.Lock();
        for (int i = 0; i < 12; i++)
        {
            if ((i % 3) == 0) Sq.Add(TScale, i, Bill, Data.colors.colors[Data.colour], Data.opacity, (float)i * 30 + bX2, bY, (float)i * 30 + eX2, eY, TexelData.CompasAzimut[i]);      // N
            else Sq.Add(TScale, i, Bill, Data.colors.colors[Data.colour], Data.opacity, (float)i * 30 + bX, bY, (float)i * 30 + eX, eY, TexelData.CompasAzimut[i]);  // 030
        }
        TScale.UnLock();
        Data.changed = true;
        ReCalc();
    }
    public virtual void Update(float scale)
    {
        //Data.angle+=10.*scale;
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        while (Data.angle >= 359.5) Data.angle -= 360f;
        while (Data.angle < -0.5) Data.angle += 360f;
        Color32 Color_pointer = Data.colors.colors[Data.colour_pointer];// * Data.opacity;
        Color32 myColor = Data.colors.colors[Data.colour];// * Data.opacity;
        //Color*=Data.opacity; //.a*=
        //Color_pointer*=Data.opacity; //.a*=
        Pointer.Lock();
        Pointer.SetVertC(0, 4, Color_pointer, Color.black);
        Pointer.UnLock();
        PointerL.Lock();
        PointerL.SetVertC(0, 4, Color_pointer, Color.black);
        PointerL.UnLock();
        PointerR.Lock();
        PointerR.SetVertC(0, 4, Color_pointer, Color.black);
        PointerR.UnLock();
        Scale.Lock();
        Scale.SetVertC(0, 4, myColor, Color.black);
        Scale.UnLock();
        Box.Lock();
        Box.SetVertC(0, 4, myColor, Color.black);
        Box.UnLock();
        TScale.Lock();
        TScale.SetVertC(0, 12 * 4, myColor, Color.black);
        TScale.UnLock();
    }
    public virtual void BeginDraw()
    {
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        ReCalc();
        Bill.PushTrasform(M);
    }
    public virtual void Draw()
    {
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        BillStyle B = new BillStyle(HUDstyle.blend, true, true);
        float A;

        Matrix2D Ma = new Matrix2D();
        Bill.SetTexture(Texture);
        if ((Data.state & HUDSTATE_POINTER) != 0)
        {//Draw Pointer
            IBObject pPointer;
            Ma.Identity();
            float Delta = Data.pointer - Data.angle;
            float MaxDelta = 0;
            if ((Data.state & 4) != 0) MaxDelta = Data.angle_view / 2;
            if ((Data.state & 8) != 0) MaxDelta = Data.viewcompasangle / 2;
            while (Delta >= 180f) Delta -= 360f;
            while (Delta < -180f) Delta += 360f;
            if (Delta > MaxDelta)
            {
                Ma.Move(MaxDelta, 0);
                pPointer = PointerR;
            }
            else if (Delta < -MaxDelta)
            {
                Ma.Move(-(MaxDelta), 0);
                pPointer = PointerL;
            }
            else
            {
                Ma.Move(Delta, 0);
                pPointer = Pointer;
            }
            pPointer.name = "Pointer";
            Bill.PushTrasform(Ma);
            Bill.Draw(pPointer);
            Bill.PopTrasform();
        }
        if ((Data.state & HUDSTATE_DIGITAL) != 0)
        {//Draw Digital
            Box.name = "Digital";
            Bill.SetStyle(B);
            Bill.Draw(Box);
            Color32 myColor = Data.colors.colors[Data.colour];// * Data.opacity;
            //DigFont.Printf(Color, TextXY, "%03d", int(Data.angle));
            Bill.SetStyle(new BillStyle(HUDstyle.blend, false, false));
        }

        if ((Data.state & HUDSTATE_VIEW_SCALE) != 0)
        {//Draw view Scale
            Bill.SetClipping(ViewAngleClipper);
            Ma.Identity();
            Ma.Move(-Data.angle, 0);
            Bill.PushTrasform(Ma);
            for (A = -360f; A < 360f + Data.angle_view / 2 + 30f; A += 30f)
            {
                if (A < Data.angle - Data.angle_view / 2 - 30f) continue;
                if (A > Data.angle + Data.angle_view / 2 + 30f) break;
                Scale.name = "Scale " + A;
                Ma.Identity();
                Ma.Move(A, 0);
                Bill.PushTrasform(Ma);
                Bill.Draw(Scale);
                Bill.PopTrasform();
            }
            Bill.PopTrasform();

        }
        if ((Data.state & HUDSTATE_SCALE) != 0)
        {//Draw Big Scale
            Bill.SetClipping(Clipper);
            Ma.Identity();
            Ma.Move(-Data.angle, 0);
            Bill.PushTrasform(Ma);
            for (A = -360f; A < 360f + Data.viewcompasangle / 2 + 30f; A += 30f)
            {
                if (A < Data.angle - Data.viewcompasangle / 2 - 30f) continue;
                if (A > Data.angle + Data.viewcompasangle / 2 + 30f) break;
                Scale.name = "Big Scale " + A;
                Ma.Identity();
                Ma.Move(A, 0);
                Bill.PushTrasform(Ma);
                Bill.Draw(Scale);
                Bill.PopTrasform();
            }
            Bill.PopTrasform();
        }

        if ((Data.state & (HUDSTATE_VIEW_SCALE | HUDSTATE_SCALE)) != 0)
        {//Draw Text for Scale
            Ma.Identity();
            Ma.Move(-Data.angle, 0);
            Bill.PushTrasform(Ma);
            TScale.name = "Text for scale";
            if (Data.angle - Data.viewcompasangle / 2 < 0)
            {
                Ma.Identity();
                Ma.Move(-360, 0);
                Bill.PushTrasform(Ma);
                Bill.Draw(TScale);
                Bill.PopTrasform();
            }
            Bill.Draw(TScale);
            if (Data.angle + Data.viewcompasangle / 2 > 360)
            {
                Ma.Identity();
                Ma.Move(360, 0);
                Bill.PushTrasform(Ma);
                Bill.Draw(TScale);
                Bill.PopTrasform();
            }
            Bill.PopTrasform();
        }
        Bill.SetClipping(null);
    }
    public virtual HudDeviceData GetData()
    {
        return Data;

    }
    public virtual void EndDraw()
    {
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        Bill.SetStyle(HUDstyle);
        Bill.PopTrasform();
    }
    ~HudCompas()
    {
        Dispose();
    }

    public void Dispose()
    {
        //DigFont->Release();
        //SafeRelease(ViewAngleClipper);
        //SafeRelease(Clipper);

        //TScale->Release();
        //Box->Release();
        //Scale->Release();
        //Pointer->Release();
        //PointerL->Release();
        //PointerR->Release();
        //SafeRelease(Texture);
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
}