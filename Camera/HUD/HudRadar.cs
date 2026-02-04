using static HudData;
using UnityEngine;

public class HudRadar : IHUDObject, IHUDObjectData
{
    IHUDObjectData hudobj = new HUDObjectImpl();

    float myPrevtime;
    bool myWptDrawStatus;
    Matrix2D M;
    IBill Bill;
    float linewidth;

    IBObject Tank;
    IBObject Craft;
    IBObject Static;
    IBObject Ring;
    IBObject Black;
    IBObject AirShip;
    IBObject SeaShip;
    IBObject Turet;
    IBObject Rocket;
    IBObject Waypoint;
    IBClipper Clipper;

    HudRadarData Data;
    ICompositeMap Map;
    //IObject Texture;
    Texture2D Texture;


    public void DrawObject(RadarData rd)
    {
        //Debug.Log("Drawing RadarData: " + rd);
        IBObject myobject;

        switch (rd.Type)
        {
            case RadarData.TYPE.LINE:
                myobject = Static;
                Sq.SetAsLine(Static, 0, Bill, rd.X1, rd.Z1, rd.X2, rd.Z2, linewidth, true);
                break;
            case RadarData.TYPE.STATIC:
                //                Debug.Log("Drawing Static RadarData: " + rd);
                myobject = Static;
                {
                    float MinX = rd.MinX;
                    float MinZ = rd.MinZ;
                    float MaxX = rd.MaxX;
                    float MaxZ = rd.MaxZ;
                    float static_width = Data.linewidth * Data.range;
                    float static_width2 = 2f * static_width;
                    if ((MaxX - MinX) < static_width2)
                    {
                        MaxX += static_width;
                        MinX -= static_width;
                    }
                    if ((MaxZ - MinZ) < static_width2)
                    {
                        MaxZ += static_width;
                        MinZ -= static_width;
                    }
                    Sq.Set(ref Static, 0, Bill, MinX, MinZ, MaxX, MaxZ, true);
                }
                break;
            case RadarData.TYPE.GPS:
                myobject = Ring;
                //Sq::Set(Ring,0,Bill,rd.MinX,rd.MinZ,rd.MaxX,rd.MaxZ,true);
                break;
            case RadarData.TYPE.WAYPOINT:
                if (!myWptDrawStatus) return;
                myobject = Waypoint;
                break;
            case RadarData.TYPE.CRAFT:
                myobject = Craft;
                break;
            case RadarData.TYPE.TANK:
                myobject = Tank;
                break;
            case RadarData.TYPE.WEAPON:
                myobject = Tank;
                break;
            case RadarData.TYPE.SEASHIP:
                myobject = SeaShip;
                break;
            case RadarData.TYPE.AIRSHIP:
                myobject = AirShip;
                break;
            case RadarData.TYPE.TURET:
                myobject = Turet;
                break;
            case RadarData.TYPE.ROCKET:
                myobject = Rocket;
                break;
            default:
                myobject = Tank;//Ring;
                break;

        }

        Color color = Data.colors.colors[rd.ColorIndex];
        //Debug.Log("rd.ColorIndex " + rd.ColorIndex + " color " + color);

        if (rd.Type != RadarData.TYPE.LINE)
        {

            const float ALT_DELTA = 500f;
            const float ALT_DELTA2 = (ALT_DELTA + ALT_DELTA);
            const float COLOR_DELTA = 1f;
            const float COLOR_ALT_COEFF = COLOR_DELTA / (ALT_DELTA2);
            float delta = Mathf.Clamp(rd.Org.y - Data.org.y + ALT_DELTA, 300f, ALT_DELTA2) * COLOR_ALT_COEFF;
            //color.Median(color, delta, new Color(color.a, 0, 0, 0), 1 - delta);
            color = Storm.Math.Median(color, delta, new Color(0, 0, 0, color.a), 1 - delta);
        }
        //Debug.Log("Color " + color + " for " + rd);
        //color = color * Data.opacity;
        myobject.Lock();
        myobject.SetVertC(0, 4, color, Color.black);
        myobject.UnLock();
        myobject.name = rd.Type.ToString();
        if (rd.Type == RadarData.TYPE.LINE)
        {
            Bill.Draw(myobject);
        }
        else
        {
            //Debug.Log("Drawing myobject RadarData: " + myobject + " "+ rd.Org);
            Matrix2D Mo = new Matrix2D();
            Mo.Identity();
            Mo.Move(rd.Org.x, rd.Org.z);
            Mo.Rotate(-rd.Angle);
            Bill.PushTrasform(Mo);
            Bill.Draw(myobject);
            Bill.PopTrasform();
        }
        //GetLog().Message("Object Org(%f,%f,%f)\n",rd.Org.x,rd.Org.y,rd.Org.z);
    }


    public HudRadar(IBill _Bill, ITexturesDB _TexturesDB, HudRadarData _Data, HUDTexelData _TexelData, MapData md)
    {
        Bill = _Bill;
        Data = _Data;
        Clipper = null;
        myWptDrawStatus = true;
        myPrevtime = 0;

        Tank = Sq.Create(Bill, Data.colors.human, Data.opacity, 0, 0, 1, 1, _TexelData.Tank);
        Craft = Sq.Create(Bill, Data.colors.human, Data.opacity, 0, 0, 1, 1, _TexelData.Craft);
        Static = Sq.Create(Bill, Data.colors.human, Data.opacity, 0, 0, 1, 1, _TexelData.Point);
        SeaShip = Sq.Create(Bill, Data.colors.human, Data.opacity, 0, 0, 1, 1, _TexelData.SeaShip);
        AirShip = Sq.Create(Bill, Data.colors.human, Data.opacity, 0, 0, 1, 1, _TexelData.AirShip);
        Turet = Sq.Create(Bill, Data.colors.human, Data.opacity, 0, 0, 1, 1, _TexelData.Turet);
        Ring = Sq.Create(Bill, Data.colors.human, Data.opacity, 0, 0, 1, 1, _TexelData.BigRing);
        Black = Sq.Create(Bill, Data.colors.disabled, Data.opacity, 0, 0, 1, 1, _TexelData.Point/*PointBlack*/);
        Rocket = Sq.Create(Bill, Data.colors.human, Data.opacity, 0, 0, 1, 1, _TexelData.Rocket);
        Waypoint = Sq.Create(Bill, Data.colors.human, Data.opacity, 0, 0, 1, 1, _TexelData.TriangleFull);
        Data.changed = true;
        Map = Bill.CreateMap(md, _TexturesDB);
        Texture = _TexturesDB.CreateTexture(_TexelData.TextureName);

        M = new Matrix2D();
    }
    public virtual void Update(float time_scale)
    {
        if ((Data.state & HUDSTATE_OFF) != 0) return;

        myPrevtime -= time_scale;
        if (myPrevtime < 0)
        {
            myPrevtime = myWptDrawStatus ? Data.wpt_period_off : Data.wpt_period_on;
            myWptDrawStatus = !myWptDrawStatus;
        }

        if (Data.changed)
        {
            Data.changed = false;
            M.Identity();
            float dx = Data.w * 0.5f;
            float dy = Data.h * 0.5f;
            float cx = Data.x + dx;
            float cy = Data.y + dy;
            float scale;
            if (dx < dy) scale = dx / Data.range;//*sqrt((dx*dx)/(dy*dy)+1.));
            else scale = dy / Data.range;//*sqrt((dy*dy)/(dx*dx)+1.));

            M.Move(cx, cy);
            M.Scale(scale, -scale);
            M.Move(-Data.org.x, -Data.org.z);
            M.Rotate(Data.org.x, Data.org.z, Data.angle);
            //SafeRelease(Clipper);
            {
                Vector2[] Clip = new Vector2[4] { new Vector2(cx - dx, cy - dy), new Vector2(cx + dx, cy - dy), new Vector2(cx + dx, cy + dy), new Vector2(cx - dx, cy + dy) };
                //foreach (var c in Clip)
                //{
                //    Debug.Log("Clip: " + c);
                //}
                Clipper = Bill.CreateClipper(4, Clip);
            }
            Sq.Set(ref Black, 0, Bill, Data.org.x - 2 * Data.range, Data.org.z - 2 * Data.range, Data.org.x + 2 * Data.range, Data.org.z + 2 * Data.range, true);

            float iconsizex = Data.iconsize * Data.range;
            float iconsizey = iconsizex * 2.3125f; // dy/dх = 2.3125 для всех иконок кроме waypoint
            linewidth = Data.linewidth * Data.range;
            Sq.Set(ref Tank, 0, Bill, -iconsizex, iconsizey, iconsizex, -iconsizey, true);
            Sq.Set(ref Craft, 0, Bill, -iconsizex, iconsizey, iconsizex, -iconsizey, true);
            Sq.Set(ref Turet, 0, Bill, -iconsizex, iconsizey, iconsizex, -iconsizey, true);
            Sq.Set(ref AirShip, 0, Bill, -iconsizex, iconsizey, iconsizex, -iconsizey, true);
            Sq.Set(ref SeaShip, 0, Bill, -iconsizex, iconsizey, iconsizex, -iconsizey, true);
            Sq.Set(ref Rocket, 0, Bill, -iconsizex, iconsizey, iconsizex, -iconsizey, true);
            //Т.к. WayPoint и Ring имеет другой форм фактор,то iconsizey для него равен iconsizeх
            float wpt_iconsize = Data.wpt_size * Data.range;
            Sq.Set(ref Waypoint, 0, Bill, -wpt_iconsize, wpt_iconsize, wpt_iconsize, -wpt_iconsize, true);
            float ringsize = iconsizey;
            Sq.Set(ref Ring, 0, Bill, -ringsize, ringsize, ringsize, -ringsize, true);
            /*
            Ring.Lock();
            Sq.Set(Ring,0,Bill,-iconsizex,iconsizey,iconsizex,-iconsizey);
            Ring.UnLock();
            */
        }
    }
    public virtual void BeginDraw()
    {
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        //float scr[4]={.1,.6,.5,-.5};
        //if ((Data.state & HUDSTATE_POINTER)!=0) Bill.SetClipping(Clipper);
        Bill.PushTrasform(M);
    }
    public virtual void Draw()
    {
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        Bill.SetStyle(new BillStyle(BlendMode.BLEND_BLEND, false, true));
        if ((Data.state & HUDSTATE_MAP) != 0)
        {
            //float range=Data->range*4;
            float range = Data.range * Mathf.Sqrt(1f + Mathf.Pow(Data.w > Data.h ? Data.w / Data.h : Data.h / Data.w, 2)) * 2f;
            Color32 Color = Data.colors.colors[Data.colour];// * Data.opacity; //TODO вернуть прозрачность радара
            //TODO Вернуть отрисовку радарной карты
            //Map.Draw(Data.org.x, Data.org.z, range, range, Color);
            Bill.SetTexture(Texture);
        }
        else
        {
            Bill.SetTexture(Texture);
            Bill.Draw(Black);
        }
        Bill.SetStyle(new BillStyle(BlendMode.BLEND_ADD, false, true));
        for (RadarData I = Data.Items.Head(); I != null; I = I.Next())
            DrawObject(I);
    }
    public virtual void EndDraw()
    {
        if ((Data.state & HUDSTATE_OFF) != 0) return;
        Bill.SetClipping(null);
        Bill.PopTrasform();
    }
    public virtual HudDeviceData GetData()
    {
        return Data;
    }

    public void Dispose()
    {
        //IMemory.SafeRelease(Texture);
        Texture = null;
        Map.Release();
        Waypoint.Release();
        Rocket.Release();
        Black.Release();
        Ring.Release();
        Static.Release();
        Craft.Release();
        Tank.Release();
        SeaShip.Release();
        AirShip.Release();
        Turet.Release();

        IMemory.SafeRelease(Clipper);
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

    ~HudRadar() { Dispose(); }

}
