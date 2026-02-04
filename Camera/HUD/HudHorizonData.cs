using UnityEngine;
using static HudData;
using DWORD = System.UInt32;

public class HudHorizonData : HudDeviceData
{
    public float pitch;
    public float roll;

    public float text_x;
    public float text_dy;
    public float fontsize;

    int var_text_x;
    int var_text_dy;
    int var_fontsize;


    public HudHorizonData(BaseScene pScene, DWORD iname = iHorizon, string sname = sHorizon) : base(pScene, iname, sname)
    {
        //wsprintf(Buff,"hud_%s_""text_x", sname); var_text_x=rScene.GetCommandsApi()->RegisterVariable(Buff, this, VAR_FLOAT)
        //wsprintf(Buff, "hud_%s_""text_dy", sname); var_text_dy = rScene.GetCommandsApi()->RegisterVariable(Buff, this, VAR_FLOAT)
        //wsprintf(Buff,"hud_%s_""fontsize",sname); var_fontsize=rScene.GetCommandsApi()->RegisterVariable(Buff,this,VAR_FLOAT)
        string Buff;
        Buff = string.Format("hud_{0}_text_x", sname); var_text_x = (int)rScene.GetCommandsApi().RegisterVariable(Buff, this, VType.VAR_FLOAT);
        Buff = string.Format("hud_{0}_text_dy", sname); var_text_dy = (int)rScene.GetCommandsApi().RegisterVariable(Buff, this, VType.VAR_FLOAT);
        Buff = string.Format("hud_{0}_fontsize", sname); var_fontsize = (int)rScene.GetCommandsApi().RegisterVariable(Buff, this, VType.VAR_FLOAT);

    }
    ~HudHorizonData() { }
    public override object OnVariable(uint code, object data)
    {
        { if (code == var_text_x) { if (data != null) { text_x = (float)data; changed = true; } return text_x; } };
        { if (code == var_text_dy) { if (data != null) { text_dy = (float)data; changed = true; } return text_dy; } };
        { if (code == var_fontsize) { if (data != null) { fontsize = (float)data; changed = true; } return fontsize; } };
        return base.OnVariable(code, data);

    }

};

public class HudAviaHor : IHUDObject
{
    //Эмуляция множественного наследования
    IHUDObjectData hobj = new HUDObjectImpl();

    Matrix2D M = new Matrix2D();
    IBill Bill;
    float k1;
    float k2;

    BillStyle HUDstyle;
    IFont StormFont;
    IBObject BAviaLine1;
    IBObject BAviaLine2;
    IBClipper Clipper;

    HudHorizonData Data;



    public HudAviaHor(IBill _Bill, ITexturesDB _TexturesDB, BillStyle style, HudHorizonData _Data, HUDTexelData TexelData)
    {
        Bill = _Bill;
        Data = _Data;
        StormFont = Bill.CreateFont("Small", 10, 10);

        {
            //FVec2 Clip[4]={FVec2(x-w,0),FVec2(ViewCompasAngle,0),FVec2(ViewCompasAngle,1),FVec2(-ViewCompasAngle,1)};
            //Clipper=Bill.CreateClipper(4,Clip);
        }

        BAviaLine1 = Bill.Create(4, 2);
        BAviaLine1.Lock();
        {
            Sq.Add(BAviaLine1, 0, Bill, Data.colors.colors[Data.colour], Data.opacity, 1, 1, 1, 1, TexelData.AviaHor1);
            float dx = TexelData.AviaHor1[2] - TexelData.AviaHor1[0];
            float dy = TexelData.AviaHor1[3] - TexelData.AviaHor1[1];
            k1 = dy / dx * .5f;
        }
        BAviaLine1.UnLock();
        BAviaLine2 = Bill.Create(4, 2);
        BAviaLine2.Lock();
        {
            Sq.Add(BAviaLine2, 0, Bill, Data.colors.colors[Data.colour], Data.opacity, 1, 1, 1, 1, TexelData.AviaHor2);
            float dx = TexelData.AviaHor2[2] - TexelData.AviaHor2[0];
            float dy = TexelData.AviaHor2[3] - TexelData.AviaHor2[1];
            k2 = dy / dx * .5f;
        }
        BAviaLine2.UnLock();
    }
    public void Update(float scale)
    {
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        if (Data.changed)
        {
            Data.changed = false;
            BAviaLine1.Lock();
            {
                Sq.Set(ref BAviaLine1, 0, Bill, -Data.w / 2.0f, -k1 * Data.w,
                    +Data.w / 2.0f, k1 * Data.w);
                BAviaLine1.SetVertC(0, 4, Data.colors.colors[Data.colour], Color.black);
            }
            BAviaLine1.UnLock();

            BAviaLine2.Lock();
            {
                Sq.Set(ref BAviaLine2, 0, Bill, -Data.w / 2.0f, -k2 * Data.w,
                    +Data.w / 2.0f, k2 * Data.w);
                BAviaLine2.SetVertC(0, 4, Data.colors.colors[Data.colour], Color.black);
            }
            BAviaLine2.UnLock();
            M.Identity();
            M.Move(Data.x, Data.y);
            StormFont.SetWH(Data.fontsize, Data.fontsize);
        }

    }
    public void BeginDraw()
    {
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        //BillStyle B = new BillStyle(BLEND_ADDA,true,true);
        Bill.PushTrasform(M);
        //  Bill->SetClipping(Clipper);
        //Bill->SetStyle(B);

    }
    public void Draw()
    {
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        Color color = Data.colors.colors[Data.colour];
        color *= Data.opacity;
        BAviaLine1.Lock();
        BAviaLine1.SetVertC(0, 4, color, Color.black);
        BAviaLine1.UnLock();
        BAviaLine2.Lock();
        BAviaLine2.SetVertC(0, 4, color, Color.black);
        BAviaLine2.UnLock();

        Bill.Draw(BAviaLine2);
        Matrix2D M1 = new Matrix2D();
        M1.Identity();
        float craft_y = -Data.pitch / 90.0f * Data.h / 2.0f;
        M1.Move(0, craft_y);
        M1.Rotate(Storm.Math.GRD2RD(Data.roll));
        Bill.PushTrasform(M1);
        Bill.Draw(BAviaLine1);
        Bill.PopTrasform();
        string Buff;
        //wsprintf(Buff, "%d\"", int((Data.pitch)));
        Buff = string.Format("{0}\"", (int)Data.pitch);
        StormFont.Puts(color, new Vector2(Data.text_x, craft_y + Data.text_dy), Buff);
    }
    public void EndDraw()
    {
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        Bill.SetClipping(null);
        Bill.PopTrasform();
        Bill.SetStyle(HUDstyle);
    }
    public HudDeviceData GetData() { return Data; }
    ~HudAviaHor() { Dispose(); }
    public void Dispose()
    {
        BAviaLine1.Release();
        BAviaLine2.Release();
        StormFont.Release();
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