using UnityEngine;
using UnityEngine.Assertions;

public class HudRecticle : IHUDObject, IHUDObjectData
{
    IHUDObjectData hobj;
    IBill Bill;
    ITexturesDB TexturesDB;
    HudRecticleData Data;
    HUDTexelData TexelData;
    HudColors colors;
    IFont StormFont;
    public HudRecticle(IBill _Bill, ITexturesDB _TexturesDB, HudRecticleData _Data, HUDTexelData _TexelData, HudColors _colors)
    {
        hobj = new HUDObjectImpl();
        Bill = _Bill;
        TexturesDB = _TexturesDB;
        Data = _Data;
        TexelData = _TexelData;
        colors = _colors;
        StormFont = Bill.CreateFont("Small");
        SetHide(false);
        Data.Dev = this;
    }
    public virtual void Update(float scale)
    {
        const float o75 = .75f;
        Assert.IsNotNull(GetTree());
        switch (Data.Type)
        {
            case HudRecticleData.TYPE.LAMP_A:
            case HudRecticleData.TYPE.LAMP_T:
            case HudRecticleData.TYPE.LAMP_O:
            case HudRecticleData.TYPE.LAMP_M:
                {
                    HUDTree tPrim = GetTree().In();
                    HudPrim Prim;
                    if (tPrim != null)
                    {
                        Prim = (HudPrim)tPrim.GetData();
                    }
                    else
                    {
                        float[] Tex = null; ;
                        string name="Unknown lamp";
                        switch (Data.Type)
                        {
                            case HudRecticleData.TYPE.LAMP_T: Tex = TexelData.LampT; name = "TacticalLamp"; break;
                            case HudRecticleData.TYPE.LAMP_O: Tex = TexelData.LampO; name = "ObjectiveLamp"; break;
                            case HudRecticleData.TYPE.LAMP_M: Tex = TexelData.LampM; name = "MessageLamp"; break;
                            case HudRecticleData.TYPE.LAMP_A: Tex = TexelData.LampA; name = "AutopilotLamp"; break;
                        }
                        Assert.IsNotNull(Tex);

                        Prim = new HudPrim(Bill, TexturesDB, colors, Data.opacity, Tex);
                        Prim.Line.name = name;
                        Tree.Insert(Prim);
                    };
                    Prim.SetXY(Data.x - Data.w, Data.y - Data.h);
                    Prim.SetWH(2 * Data.w, 2 * Data.h);
                    Prim.SetColour((int)Data.colour, Data.opacity);
                }
                break;
            case HudRecticleData.TYPE.GUNSIGHT:
                {
                    HUDTree tPrim = Tree.In();
                    HudPrim Prim;
                    if (tPrim != null)
                    {
                        Prim = (HudPrim)tPrim.GetData();
                    }
                    else
                    {
                        Prim = new HudPrim(Bill, TexturesDB, colors, Data.opacity, TexelData.GunSight);
                        Tree.Insert(Prim);
                        Prim.Line.name = "GunSight";
                    };
                    Prim.SetXY(Data.x - Data.w, Data.y - Data.h);
                    Prim.SetWH(2 * Data.w, 2 * Data.h);
                    Prim.SetColour((int)Data.colour, Data.opacity);
                }
                break;
            case HudRecticleData.TYPE.RING:
                {
                    HUDTree tPrim = Tree.In();
                    HudPrim Prim;
                    if (tPrim != null)
                    {
                        Prim = (HudPrim)tPrim.GetData();
                    }
                    else
                    {
                        Prim = new HudPrim(Bill, TexturesDB, colors, Data.opacity, TexelData.BigRing);
                        Tree.Insert(Prim);
                    }
                    Prim.SetXY(Data.x - Data.w, Data.y - Data.h);
                    Prim.SetWH(2 * Data.w, 2 * Data.h);
                    Prim.SetColour((int)Data.colour, Data.opacity);
                }
                break;
            case HudRecticleData.TYPE.SIMPLETARGET:
                {
                    HUDTree tPrim = Tree.In();
                    Assert.IsNull(tPrim); // skip this safety
                    if (tPrim != null)
                    {
                        tPrim.Sub();
                    }
                }
                break;
            case HudRecticleData.TYPE.CURRENTTARGET:
                {
                    HUDTree tPrim = Tree.In();
                    HudBoundBox Prim;
                    if (tPrim != null)
                    {
                        Prim = (HudBoundBox)tPrim.GetData();
                    }
                    else
                    {
                        Prim = new HudBoundBox(Bill, TexturesDB, TexelData, colors, Data.opacity);
                        Prim.UseLine(false);
                        Tree.Insert(Prim);
                    }
                    Prim.SetXY(Data.x, Data.y);
                    Prim.SetWH(2 * Data.w, 2 * Data.h);
                    Prim.SetColour((int)Data.colour, Data.opacity);
                    Prim.SetLineW(.005f);
                }
                break;
            case HudRecticleData.TYPE.ANIMETARGET:
                {
                    HUDTree tPrim = Tree.In();
                    HudBoundBox Prim;
                    if (tPrim != null)
                    {
                        Prim = (HudBoundBox)tPrim.GetData();
                    }
                    else
                    {
                        Prim = new HudBoundBox(Bill, TexturesDB, TexelData, colors, Data.opacity);
                        Tree.Insert(Prim);
                    }
                    Prim.SetXY(Data.x, Data.y);
                    float awh = 2 * Data.w * (Data.stage * (1 - o75) + o75);
                    Prim.SetWH(awh, awh);
                    Prim.SetColour((int)Data.colour, Data.opacity);
                    Prim.SetLineW(.005f);
                }
                break;
            case HudRecticleData.TYPE.CURRENTWPT:
                {
                    HUDTree tPrim = Tree.In();
                    HudPrim Prim;
                    if (tPrim != null)
                    {
                        Prim = (HudPrim)tPrim.GetData();
                    }
                    else
                    {
                        Prim = new HudPrim(Bill, TexturesDB, colors, Data.opacity, TexelData.TriangleFull);
                        Tree.Insert(Prim);
                    }
                    TurnArrow(Prim);
                    Prim.SetWH(2 * Data.w, 2 * Data.h);
                    Prim.SetColour((int)Data.colour, Data.opacity);
                    Prim.Line.name = "Waypoint";
                }
                break;
            case HudRecticleData.TYPE.ARROW:
                {
                    HUDTree tPrim = Tree.In();
                    HudPrim Prim;
                    if (tPrim != null)
                    {
                        Prim = (HudPrim)tPrim.GetData();
                    }
                    else
                    {
                        Prim = new HudPrim(Bill, TexturesDB, colors, Data.opacity, TexelData.TriangleFull);
                        Tree.Insert(Prim);
                    }
                    TurnArrow(Prim);
                    Prim.SetWH(2 * Data.w, 2 * Data.h);
                    Prim.SetColour((int)Data.colour, Data.opacity);
                }
                break;
            case HudRecticleData.TYPE.ANIMEARROW:
                {
                    HUDTree tPrim = Tree.In();
                    HudPrim Prim;
                    if (tPrim != null)
                    {
                        Prim = (HudPrim)tPrim.GetData();
                    }
                    else
                    {
                        Prim = new HudPrim(Bill, TexturesDB, colors, Data.opacity, TexelData.Triangle);
                        Tree.Insert(Prim);
                    }
                    TurnArrow(Prim);
                    float awh = 2 * Data.w * (Data.stage * (1 - o75) + o75);
                    Prim.SetWH(awh, awh);
                    Prim.SetColour((int)Data.colour, Data.opacity);
                }
                break;
            case HudRecticleData.TYPE.ANIMEWPT:
                {
                    HUDTree tPrim = Tree.In();
                    HudPrim Prim;
                    //HudPrim *RingPrim;
                    if (tPrim != null)
                    {
                        Prim = (HudPrim)tPrim.GetData();
                        //RingPrim=(HudPrim *)tPrim.Next().GetData();
                    }
                    else
                    {
                        Prim = new HudPrim(Bill, TexturesDB, colors, Data.opacity, TexelData.TriangleFull);
                        Tree.Insert(Prim);
                        //RingPrim=new HudPrim(Bill,colors,TexelData.BigRing);
                        //Tree.Insert(RingPrim);
                    }
                    //Prim.SetXY(Data.x-Data.r/2,Data.y-Data.r/2);
                    //Prim.SetWH(Data.r,Data.r);
                    //Prim.SetColour(Data.colour);

                    float awh = 2 * Data.w * (Data.stage * (1 - o75) + o75);
                    Prim.SetXY(Data.x - awh / 2, Data.y - awh / 2);
                    Prim.SetWH(awh, awh);
                    Prim.SetColour((int)Data.colour, Data.opacity);

                }
                break;
            case HudRecticleData.TYPE.X:
                {
                    HUDTree tPrim = Tree.In();
                    HudRingPrim Prim;
                    if (tPrim != null)
                    {
                        Prim = (HudRingPrim)tPrim.GetData();
                    }
                    else
                    {
                        Prim = new HudRingPrim(Bill, TexturesDB, Data.w, Data.h * .9f, TexelData.Point);
                        Tree.Insert(Prim);
                        Prim.SetPercent(1f);
                    }
                    Prim.SetXY(Data.x, Data.y);
                    Prim.SetR(Data.w);
                    Prim.SetW(.003f);
                    Prim.SetColors(colors.colors[Data.colour], colors.disabled, Data.opacity);
                }

                break;
            case HudRecticleData.TYPE.MISSILECONTROL:
                {
                    HUDTree tPrim = Tree.In();
                    HudRingPrim Prim;
                    if (tPrim != null)
                    {
                        Prim = (HudRingPrim)tPrim.GetData();
                    }
                    else
                    {
                        Prim = new HudRingPrim(Bill, TexturesDB, Data.w, Data.h * .9f, TexelData.Point);
                        Tree.Insert(Prim);
                    }
                    Prim.SetXY(Data.x, Data.y);
                    Prim.SetR(Data.w);
                    Prim.SetW(.01f);
                    Prim.SetN(60);
                    Prim.SetPercent(Data.percent);
                    Prim.SetColors(colors.colors[Data.colour], colors.danger, Data.opacity);
                }
                break;
            case HudRecticleData.TYPE.ROMB:
                {
                    HUDTree tPrim = GetTree().In();
                    HudPrim Prim;
                    if (tPrim != null)
                    {
                        Prim = (HudPrim)tPrim.GetData();
                    }
                    else
                    {
                        Prim = new HudPrim(Bill, TexturesDB, colors, Data.opacity, TexelData.Romb);
                        Tree.Insert(Prim);
                    }
                    Prim.SetXY(Data.x - Data.w, Data.y - Data.h);
                    Prim.SetWH(2 * Data.w, 2 * Data.h);
                    Prim.SetColour((int)Data.colour, Data.opacity);
                }
                break;
            default:
                Assert.IsFalse(false);
                break;
        }
    }
    public virtual void BeginDraw() { }
    public virtual void Draw()
    {
        if (Data.hide) return;
        Color32 col = colors.colors[Data.colour]; //* Data->opacity;
        float wup = StormFont.Width(Data.textup) / 2;
        float wdn = StormFont.Width(Data.text) / 2;
        
        StormFont.Puts(col, new Vector2(Data.x - wup, Data.y - Data.h - StormFont.Height()), Data.textup);
        StormFont.Puts(col, new Vector2(Data.x - wdn, Data.y + Data.w), Data.text);
    }
    public virtual void EndDraw() { }
    public virtual HudDeviceData GetData()
    {
        return null;
    }
    ~HudRecticle()
    {
        Dispose();
    }
    public void Dispose()
    {
        StormFont.Release();
        //Data->Dev = 0;
        Data.Dev = null;
    }
    void TurnArrow(HudPrim Prim)
    {
        //TODO Реализовать указатель поворота
        //bool Up = false, Dn = false, Rt = false, Lt = false;

        //float _x = Data->x - Data->w;
        //float _y = Data->y - Data->h;
        //if (_x < Data->w) { _x = 0; Lt = true; }
        //if (_x > 1 - 2 * Data->w) { _x = 1 - 2 * Data->w; Rt = true; }
        //if (_y < Data->h) { _y = 0; Up = true; }
        //if (_y > .75 - 2 * Data->h) { _y = .75 - 2 * Data->h; Dn = true; }
        //Prim->SetXY(_x, _y);
        //_x += Data->w;
        //_y += Data->h;
        //if (Up || Dn || Rt || Lt)
        //{
        //    Matrix2D M;
        //    M.Identity();
        //    switch (Up + Dn * 2 + Rt * 4 + Lt * 8)
        //    {
        //        case 1://Up
        //            M.Rotate(_x, _y, GRD2RD(180));
        //            break;
        //        case 2://Dn
        //               //M.Rotate(_x,_y,RD2GRD(0));
        //            break;
        //        case 4://Rt
        //            M.Rotate(_x, _y, GRD2RD(-90));
        //            break;
        //        case 5://Up+Rt
        //            M.Rotate(_x, _y, GRD2RD(-90 - 45));
        //            break;
        //        case 6://Dn+Rt
        //            M.Rotate(_x, _y, GRD2RD(-45));
        //            break;
        //        case 8://Lt
        //            M.Rotate(_x, _y, GRD2RD(90));
        //            break;
        //        case 9://Up+Lt
        //            M.Rotate(_x, _y, GRD2RD(90 + 45));
        //            break;
        //        case 10://Dn+Lt
        //            M.Rotate(_x, _y, GRD2RD(45));
        //            break;
        //        default:
        //            Assert(0);
        //            break;
        //    }
        //    Prim->SetMatrix(&M);
        //}
        //else
        //{
        //    Prim->SetMatrix(0);
        //}
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
}
