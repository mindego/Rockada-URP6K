using static HudData;
using UnityEngine;
using static HudDataColors;
using static HudRecticleData;
using System;

public class HUD : IHUD
{
    bool On;
    BaseScene Scene;
    IBill Bill;
    ITexturesDB TexturesDB;
    BillStyle style;
    HUDTree GD;


    public HUD(BaseScene rScene)
    {
        Scene = rScene;
        style = new BillStyle(BlendMode.BLEND_TRANSA, false, true);
        Bill = Scene.GetRendererApi().CreateBill();
        TexturesDB = Bill.CreateTexturesDB(Scene.getSV().getTexturesDbName());
        On = true;
        RegisterAllData();
        HudDesktopData DesktopData = (HudDesktopData)Scene.GetBaseData(iDesktop, true);
        GD = new HUDTree(new HudDesktop(Bill, TexturesDB, DesktopData));
    }


    private void RegisterAllData()
    {
        //Для удобства - вынесены в отдельные методы
        RegisterDesktopData();
        RegisterTexelData();
        //Compas default data
        RegisterCompasData();
        //Altimeter default data
        RegisterAltiData();
        //Speedmeter default data
        RegisterSpeedData();
        RegisterGunSightData();
        RegisterLockGunSightData();
        // Radar default data
        RegisterRadarData();
        // Map default data
        RegisterMData();
        // Threat Bar default data
        RegisterIData();
        // Weapon default data
        RegisterWeaponData();
        // Target box default data
        RegisterTargetBoxData();
        // Horizon default data
        RegisterHorizonData();
        // Damage default data
        RegisterDamageData();
        // Energy default data;
        RegisterEnergyData();
        RegisterEnergyScaleData();
        // Test??? data
        RegisterTestData();
        // Log data
        RegisterLogData();
        // Ai menu
        RegisterAiMenuData();
        // Ring data:
        RegisterRingData();
        //RecticlesData
        RegisterRecticlesData();
        //FPSData
        RegisterFPSData();
        //LabelData
        RegisterLabelData();
        //VPData
        RegisterVPData();


        //RegisterGunSightData();
    }

    private void RegisterCompasData()
    {
        HudCompasData CompasData = (HudCompasData)Scene.GetBaseData(iCompas, false);
        if (CompasData == null)
        {
            CompasData = new HudCompasData(Scene);
            CompasData.changed = true;
            CompasData.angle_view = 57;
            CompasData.viewcompasangle = 180;
            CompasData.colour = HUDCOLOR_NORMAL;
            CompasData.colour_pointer = HUDCOLOR_VELIAN;
            CompasData.x = 0.5f;
            CompasData.y = 0.02f;
            CompasData.w = .4f;
            CompasData.h = 25f / 640f;
            CompasData.state = (int)(HUDSTATE_DIGITAL | HUDSTATE_VIEW_SCALE | HUDSTATE_SCALE | HUDSTATE_POINTER);
            CompasData.colors = (HudColors)Scene.GetBaseData(iColor, false);
            if (CompasData.colors == null)
            {
# if _DEBUG
                Scene.Message("Compas colors not find");
#endif //_DEBUG
                CompasData.colors = new HudColors(Scene, iColor, sColor);
            }
# if _DEBUG
            Scene.Message("Compas complete");
#endif
        }
    }
    private void RegisterVPData()
    {
        Debug.Log("Stub RegisterVPData()");
    }
    private void RegisterLabelData()
    {
        Debug.Log("Stub RegisterLabelData()");
    }
    private void RegisterFPSData()
    {
        Debug.Log("Stub RegisterFPSData()");
    }
    private void RegisterRingData()
    {
        Debug.Log("Stub RegisterRingData()");
    }
    private void RegisterAiMenuData()
    {
        Debug.Log("Stub RegisterAiMenuData()");
    }
    private void RegisterLogData()
    {
        Debug.Log("Stub RegisterLogData()");
    }
    private void RegisterTestData()
    {
        Debug.Log("Stub RegisterTestData()");
    }
    private void RegisterEnergyScaleData()
    {
        HudBarData EnergyData = (HudBarData)Scene.GetBaseData(iEnergyScale, false);
        if (EnergyData == null)
        {
            EnergyData = new HudBarData(Scene, iEnergyScale, sEnergyScale);
            EnergyData.changed = true;
            EnergyData.colors = (HudColors)Scene.GetBaseData(iColor, false);
            if (EnergyData.colors == null) EnergyData.colors = new HudColors(Scene, iColor, sColor);
            EnergyData.colour = HUDCOLOR_NORMAL;
            EnergyData.x = 0.025f;
            EnergyData.y = 0f;
            EnergyData.w = .03f;
            EnergyData.h = .2f;
            EnergyData.state = (int)HUDSTATE_TEST;
            EnergyData.hightmin = 0.5f;
            EnergyData.lowmax = 0.25f;
            EnergyData.colour_hight = HUDCOLOR_NORMAL;
            EnergyData.colour_low = HUDCOLOR_DANGER;
            EnergyData.colour_normal = HUDCOLOR_MEDIUM;
            EnergyData.colour_off = HUDCOLOR_DISABLED;
            EnergyData.scale = 2;
            EnergyData.SetText("");
            EnergyData.value = 1;
            EnergyData.vert = true;
        }
    }
    private void RegisterEnergyData()
    {
        HudBarData EnergyData = (HudBarData)Scene.GetBaseData(iEnergy, false);
        if (EnergyData == null)
        {
            EnergyData = new HudBarData(Scene, iEnergy, sEnergy);
            EnergyData.changed = true;
            EnergyData.colors = (HudColors)Scene.GetBaseData(iColor, false);
            if (EnergyData.colors == null) EnergyData.colors = new HudColors(Scene, iColor, sColor);
            EnergyData.colour = HUDCOLOR_NORMAL;
            EnergyData.x = 0.075f;
            EnergyData.y = 0f;
            EnergyData.w = .03f;
            EnergyData.h = .2f;
            EnergyData.state = (int)HUDSTATE_TEST;
            EnergyData.hightmin = 0.5f;
            EnergyData.lowmax = 0.2f;
            EnergyData.colour_hight = HUDCOLOR_NORMAL;
            EnergyData.colour_low = HUDCOLOR_DANGER;
            EnergyData.colour_normal = HUDCOLOR_MEDIUM;
            EnergyData.colour_off = HUDCOLOR_DISABLED;
            EnergyData.scale = 0;
            EnergyData.SetText("");
            EnergyData.value = 1;
            EnergyData.vert = true;
        }
    }
    private void RegisterHorizonData()
    {
        HudHorizonData HData = (HudHorizonData)Scene.GetBaseData(iHorizon, false);
        if (HData == null)
        {
            HData = new HudHorizonData(Scene, iHorizon, sHorizon);
            HData.changed = true;
            HData.colors = (HudColors)Scene.GetBaseData(iColor, false);
            if (HData.colors == null) HData.colors = new HudColors(Scene, iColor, sColor);
            HData.colour = HUDCOLOR_HORIZON;
            HData.x = 0.5f;
            //HData.y = 0.375f; //в оригинале
            HData.y = 0.5f;
            HData.w = .3f;
            HData.h = .3f;
            HData.state = 0;
            HData.pitch = 0;
            HData.roll = 0;
            HData.text_dy = 0;
            HData.text_x = HData.w * .5f;
            HData.fontsize = 1;
        }
    }
    private void RegisterTargetBoxData()
    {
        HudTargetBoxData TData = (HudTargetBoxData)Scene.GetBaseData(iTarget, false);
        if (TData == null)
        {
            TData = new HudTargetBoxData(Scene, iTarget, sTarget);
            TData.changed = true;
            TData.colors = (HudColors)Scene.GetBaseData(iColor, false);
            if (TData.colors == null) TData.colors = new HudColors(Scene, iColor, sColor);
            TData.colour = HUDCOLOR_NORMAL;
            TData.x = 0.5f;
            TData.y = 0.375f;
            TData.w = .3f;
            TData.h = .3f;
            TData.wb = .06f;
            TData.minsize = 0.05f;
            TData.maxsize = .5f;
            TData.state = (int)(HUDSTATE_BAR | HUDSTATE_TEXT);
            TData.text = "Target:\n not found";
        }
    }
    private void RegisterIData()
    {
        HudBarData IData = (HudBarData)Scene.GetBaseData(iThreat, false);
        if (IData == null)
        {
            IData = new HudBarData(Scene, iThreat, sThreat);
            IData.x = 0.775f;
            IData.y = 0.725f;
            IData.w = 0.2f;
            IData.h = 0.015f;
            IData.colour = HUDCOLOR_NEUTRAL;
            IData.colors = (HudColors)Scene.GetBaseData(iColor, false);
            if (IData.colors == null) IData.colors = new HudColors(Scene, iColor, sColor);
            IData.state = 0;//HUDSTATE_BAR;
            IData.changed = true;
            IData.hightmin = 0.5f;
            IData.lowmax = 0.3f;
            IData.colour_hight = HUDCOLOR_DANGER;
            IData.colour_low = HUDCOLOR_NORMAL;
            IData.colour_normal = HUDCOLOR_MEDIUM;
            IData.colour_off = HUDCOLOR_DISABLED;
            IData.scale = 0;
            IData.SetText("THREAT");
            IData.value = 1;
            IData.vert = false;
        }
    }
    /// <summary>
    /// Map default data
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    /// <exception cref="System.Exception"></exception>
    private void RegisterMData()
    {
        HudRadarData MData = (HudRadarData)Scene.GetBaseData(iMap, false);
        if (MData == null)
        {
            MData = new HudRadarData(Scene, iMap, sMap);
            MData.x = 0;
            MData.y = 0;
            MData.w = 1;
            MData.h = .75f;
            MData.angle = 0;
            MData.colour = HUDCOLOR_RADAR;
            MData.colors = (HudColors)Scene.GetBaseData(iColor, false);
            if (MData.colors == null) MData.colors = new HudColors(Scene, iColor, sColor);
            MData.iconsize = 0.025f;
            MData.wpt_size = 0.025f;
            MData.wpt_period_off = 1;
            MData.wpt_period_on = 1;
            MData.org = Vector3.zero;
            MData.range = 50000;
            MData.state = -2;
            MData.linewidth = MData.iconsize * .2f;
            MData.changed = true;

        }
    }
    private void RegisterSpeedData()
    {
        Debug.Log("Stub RegisterSpeedData(.)(.)");
    }
    private void RegisterAltiData()
    {
        Debug.Log("Stub RegisterAltiData()");
    }
    private void RegisterLockGunSightData()
    {
        Debug.Log("Stub RegisterLockGunSightData()");
    }
    private void RegisterRecticlesData()
    {
        HudRecticlesData RecticData = (HudRecticlesData)Scene.GetBaseData(iRecticles, false);
        if (RecticData == null)
        {
            RecticData = new HudRecticlesData(Scene);
            RecticData.changed = true;
            RecticData.colors = (HudColors)Scene.GetBaseData(iColor, false);
            if (RecticData.colors == null) RecticData.colors = new HudColors(Scene, iColor, sColor);
            RecticData.state = (int)HUDSTATE_TEST;
            RecticData.x = 0f;
            RecticData.y = 0f;
            RecticData.colour = HUDCOLOR_NORMAL;
            RecticData.speed = 1;
        }
    }
    private void RegisterRadarData()
    {
        HudRadarData RData = (HudRadarData)Scene.GetBaseData(iRadar, false);
        if (RData == null)
        {
            RData = new HudRadarData(Scene);
            RData.x = 0.775f;
            RData.y = 0.525f;
            RData.h = 0.2f;
            RData.w = 0.2f;
            RData.angle = 0;
            RData.colour = HUDCOLOR_RADAR;
            RData.colors = (HudColors)Scene.GetBaseData(iColor, false);
            if (RData.colors == null) RData.colors = new HudColors(Scene, iColor, sColor);
            RData.iconsize = 0.07f;
            RData.wpt_size = 0.07f;
            RData.wpt_period_off = 1;
            RData.wpt_period_on = 1;
            RData.org = Vector3.zero;
            RData.range = 50000;
            RData.state = -2;
            RData.linewidth = RData.iconsize * .2f;
            RData.changed = true;
        }

    }
    private void RegisterDesktopData()
    {
        HudDesktopData DesktopData = (HudDesktopData)Scene.GetBaseData(iDesktop, false);
        if (DesktopData == null)
        {
            DesktopData = new HudDesktopData(Scene, iDesktop, sDesktop);
            DesktopData.changed = true;
            DesktopData.x = 0;
            DesktopData.y = 0;
            DesktopData.w = 1;
            //DesktopData.h = .75f; //так в исходниках
            DesktopData.h = 1;
            DesktopData.state = 0;
            DesktopData.speed = 1;
            DesktopData.CurrSize = 0;
        }
    }

    private void RegisterTexelData()
    {
        HUDTexelData TexelData = (HUDTexelData)Scene.GetBaseData(HUD_TEXEL_DATA, false);
        if (TexelData == null) TexelData = new HUDTexelData(Scene, HUD_TEXEL_DATA);

    }


    private void RegisterWeaponData()
    {
        HudWeaponData WData = (HudWeaponData)Scene.GetBaseData(iWeapon, false);
        if (WData == null)
        {
            WData = new HudWeaponData(Scene, iWeapon, sWeapon);
            WData.changed = true;
            WData.colors = (HudColors)Scene.GetBaseData(iColor, false);
            if (WData.colors == null) WData.colors = new HudColors(Scene, iColor, sColor);
            WData.colour = HUDCOLOR_NORMAL;
            WData.boxcolour = HUDCOLOR_NORMAL;
            WData.scalesize = 0.1f;
            WData.spacing = 0.005f;
            WData.step = 0.027f;
            WData.state = 0;
            WData.x = 0.775f;
            WData.y = 0.001f;
            WData.w = 0.2f;
            WData.h = 0;
            WData.fontx = 1.5f;
            WData.fonty = 1;
        }
    }
    private void RegisterGunSightData()
    {
        HudDeviceData GunSightData = (HudDeviceData)Scene.GetBaseData(iRecticle, false);
        if (GunSightData == null)
        {
            GunSightData = new HudDeviceData(Scene, iRecticle, sRecticle);
            GunSightData.changed = true;
            GunSightData.colour = HUDCOLOR_NORMAL;
            GunSightData.x = 0.5f;    //center
            GunSightData.y = 0.75f / 2;//center
            GunSightData.h = 0.1f;
            GunSightData.w = 0.1f;
            GunSightData.state = 0;
            GunSightData.colors = (HudColors)Scene.GetBaseData(iColor, false);
            if (GunSightData.colors == null) GunSightData.colors = new HudColors(Scene, iColor, sColor);
        }
    }

    private void RegisterDamageData()
    {
        HudDamageData DData = (HudDamageData)Scene.GetBaseData(iDamage, false);
        if (DData == null)
        {
            DData = new HudDamageData(Scene, iDamage, sDamage);
            DData.changed = true;
            DData.colors = (HudColors)Scene.GetBaseData(iColor, false);
            if (DData.colors == null) DData.colors = new HudColors(Scene, iColor, sColor);
            DData.colour = HUDCOLOR_NORMAL;
            DData.x = 0.025f;
            DData.y = 0.525f;
            DData.w = .2f;
            DData.h = .2f;
            DData.state = (int)HUDSTATE_TEST;
        }
    }


    public HudDevice CreateAviaHorizon(uint h)
    {
        HUDTexelData TexelData = (HUDTexelData)Scene.GetBaseData(HUD_TEXEL_DATA, true);
        HudHorizonData HData = (HudHorizonData)Scene.GetBaseData(h, true);
        return new HudDevice(GD.Insert(new HudAviaHor(Bill, TexturesDB, style, HData, TexelData)));
    }

    public HudDevice CreateBar(uint h)
    {
        HUDTexelData TexelData = (HUDTexelData)Scene.GetBaseData(HUD_TEXEL_DATA, true);
        HudBarData IData = (HudBarData)Scene.GetBaseData(h, true);
        return new HudDevice(GD.Insert(new HudBar(Bill, TexturesDB, IData, TexelData)));
    }

    public HudDevice CreateCompass(uint h)
    {
        HUDTexelData TexelData = (HUDTexelData)Scene.GetBaseData(HUD_TEXEL_DATA, true);
        HudCompasData CompasData = (HudCompasData)Scene.GetBaseData(h, true);
        return new HudDevice(GD.Insert(new HudCompas(Bill, TexturesDB, style, CompasData, TexelData)));
    }

    public HudDevice CreateDamage(uint h)
    {
        HudDamageData DData = (HudDamageData)Scene.GetBaseData(h, true);
        return new HudDevice(GD.Insert(new HudDamage(Bill, TexturesDB, DData)));
    }

    public HudDevice CreateFPSmeter(uint h)
    {
        throw new System.NotImplementedException();
    }

    public HudDevice CreateFrame(uint h)
    {
        throw new System.NotImplementedException();
    }

    public HudDevice CreateLabel(uint h)
    {
        throw new System.NotImplementedException();
    }

    public HudDevice CreateLabelForEEI(string sName, uint iName, float _x, float _y, int color_index, float font_size, ushort aligned)
    {
        throw new System.NotImplementedException();
    }

    public HudDevice CreateRecticle(uint d, int type)
    {
        throw new System.NotImplementedException();
    }

    public HudDevice CreateMap(uint h, MapData md)
    {
        HUDTexelData TexelData = (HUDTexelData)Scene.GetBaseData(HUD_TEXEL_DATA, true);
        HudRadarData RData = (HudRadarData)Scene.GetBaseData(h, true);
        if (RData == null) throw new System.Exception("Can not find HudRadarData in BaseData");
        return new HudDevice(GD.Insert(new HudRadar(Bill, TexturesDB, RData, TexelData, md)));
    }

    public HudDevice CreateRecticles(uint h)
    {
        HUDTexelData TexelData = (HUDTexelData)Scene.GetBaseData(HUD_TEXEL_DATA, true);
        HudRecticlesData RData = (HudRecticlesData)Scene.GetBaseData(h, true);
        return new HudDevice(GD.Insert(new HudRecticles(Bill, TexturesDB, style, RData, TexelData)));
    }

    public HudDevice CreateRing(uint h)
    {
        throw new System.NotImplementedException();
    }

    public HudDevice CreateTable(uint h)
    {
        throw new System.NotImplementedException();
    }

    public HudDevice CreateTableForEEI(string sName, uint iName, float _x, float _y, float _w, float _h, int columns)
    {
        throw new System.NotImplementedException();
    }

    public HudDevice CreateTarget(uint h)
    {
        HUDTexelData TexelData = (HUDTexelData)Scene.GetBaseData(HUD_TEXEL_DATA, true);
        HudTargetBoxData TData = (HudTargetBoxData)Scene.GetBaseData(h, true);
        return new HudDevice(GD.Insert(new HudTargetBox(Bill, TexturesDB, TData, TexelData)));
    }

    public HudDevice CreateViewPort(uint h)
    {
        throw new System.NotImplementedException();
    }

    public HudDevice CreateVscale(uint d)
    {
        throw new System.NotImplementedException();
    }

    public HudDevice CreateWeapon(uint h)
    {
        HUDTexelData TexelData = (HUDTexelData)Scene.GetBaseData(HUD_TEXEL_DATA, true);
        HudWeaponData WData = (HudWeaponData)Scene.GetBaseData(h, true);
        return new HudDevice(GD.Insert(new HudWeapon(Bill, TexturesDB, WData, TexelData)));
        //return new HudDevice(new HUDTree(new HudWeapon(Bill, TexturesDB, WData, TexelData)));
    }

    public void Draw(float[] ViewPort)
    {
        //if (HudCanvasHolder.activeSelf != On) HudCanvasHolder.SetActive(On);
        //if (!On) return;
        //GD.Draw();

        Bill.Begin();
        Matrix2D M = new Matrix2D();
        if (ViewPort != null)
        {
            M.Identity();
            M.Scale(1, 0.75f);
            M.Move(ViewPort[0], ViewPort[1]);
            M.Scale(ViewPort[2], ViewPort[3]);
            M.Scale(1, 1 / .75f);
            Bill.PushTrasform(M);
        }
        Bill.SetStyle(style);
        GD.Draw();
        if (ViewPort != null)
        {
            Bill.PopTrasform();
        }
        Bill.End();

    }

    public void Hide(bool off)
    {
        On = !off;
    }

    public void ReleaseDevice(HudDevice Dev)
    {
        Dev.Dispose();
    }

    public void Update(float Scale)
    {
        GD.Update(Scale);
    }
}

public interface IBClipper : IObject
{
};

public class Matrix34f
{
    public Vector3 pos;
    public Matrix3f tm = new Matrix3f();

    public Matrix34f() { }
    public Matrix34f(Matrix3f _tm, Vector3 _pos)
    {
        tm = _tm;
        pos = _pos;
    }

    public void Identity()
    {
        tm.Identity();
        pos = Vector3.zero;
    }

    public void Multiply(Matrix34f a, Matrix34f b)
    {
        //pos = a.pos * b.tm + b.pos;
        pos = Matrix3f.Vector3Matrix3fMultiply(a.pos, b.tm);
        tm.Multiply(a.tm, b.tm);
    }

    void Transform(Vector4 src, out Vector4 dst, bool transpose = false)
    {
        Vector3 tmpVector = new Vector3(src.x, src.y, src.z);
        if (transpose)
        {
            //dst = new Vector4(tm * Vector3f(src.x, src.y, src.z), Vector4f(pos, 1) * src);
            tmpVector = Matrix3f.Matrix3fVector3Multiply(tm, tmpVector);
            dst = new Vector4(tmpVector.x, tmpVector.y, tmpVector.z, Vector4.Dot(new Vector4(pos.x, pos.y, pos.z, 1), src));
        }
        else
        {
            tmpVector = Matrix3f.Vector3Matrix3fMultiply(new Vector3(src.x, src.y, src.z), tm) + pos * src.w;
            dst = new Vector4(tmpVector.x, tmpVector.y, tmpVector.z, src.w);
        }
    }
    public Vector3 TransformPoint(Vector3 v, bool direct = true) { return direct ? Matrix3f.Vector3Matrix3fMultiply(v, tm) + pos : Matrix3f.Matrix3fVector3Multiply(tm, (v - pos)); }
    Vector3 TransformVector(Vector3 v, bool direct = true) { return direct ? Matrix3f.Vector3Matrix3fMultiply(v, tm) : Matrix3f.Matrix3fVector3Multiply(tm, v); }

    void Move(Vector3 v)
    {
        pos += v;
    }
    void Scale(Vector3 v)
    {

        tm.Scale(v);
    }
    void Rotate(Vector3 v, float a)
    {
        Matrix3f r = new Matrix3f();
        r.MakeRotation(v, a);
        r = r.MultiplyThis(tm);
        tm = r;
    }

    public void MakeRotation(Vector3 axis, float angle)
    {
        tm.MakeRotation(axis, angle);
        pos = Vector3.zero;
    }
    public void GetReciprocal(ref Matrix34f m)
    {
        // p*this = p*tm+pos ==> p*tm*m.tm+pos*m.tm+m.pos=p ==> m.pos=-pos*m.tm, m.tm = (tm)^(-1)
        tm.GetTranspose(ref m.tm);
        m.pos = -(Matrix3f.Vector3Matrix3fMultiply(pos, m.tm));
    }
    public void GetMatrix4f(ref Matrix4f m)
    {
        m.raw[0] = new Vector4(tm[0].x, tm[0].y, tm[0].z, 0);
        m.raw[1] = new Vector4(tm[1].x, tm[1].y, tm[1].z, 0);
        m.raw[2] = new Vector4(tm[2].x, tm[2].y, tm[2].z, 0);
        m.raw[3] = new Vector4(pos.x, pos.y, pos.z, 1);
    }
};

public class Matrix4f
{
    public Vector4[] raw = new Vector4[4];

    public void Identity()
    {
        raw[0].Set(1, 0, 0, 0);
        raw[1].Set(0, 1, 0, 0);
        raw[2].Set(0, 0, 1, 0);
        raw[3].Set(0, 0, 0, 1);
    }

    public void Zero()
    {
        for (int i = 0; i < 4; ++i)
            raw[i].Set(0, 0, 0, 0);
    }

    internal D3DMATRIX GetD3dmatrix()
    {
        //float _11, _12, _13, _14;
        //float _21, _22, _23, _24;
        //float _31, _32, _33, _34;
        //float _41, _42, _43, _44;
        D3DMATRIX res = new D3DMATRIX();
        //TODO создать корректное преобразование Matrix4f в D3DMatrix
        return res;
    }

    public Vector4 this[int i]
    {
        get
        {
            return raw[i];
        }
        set
        {
            raw[i] = value;
        }
    }
}
public class Matrix2D
{
    Mat2D direct, reciprocal;

    public override string ToString()
    {
        string res = this.GetType().ToString() + "\n";
        res += string.Format("Direct Up {0} Right {1}\n", direct.Up, direct.Right);
        res += "Direct  Position: " + direct.Org + "\n";
        res += string.Format("Reciprocal Up {0} Right {1}\n", reciprocal.Up, reciprocal.Right);
        res += "Reciprocal Position: " + reciprocal.Org + "\n";
        //res += "Scale: " + direct.Scale;
        return res;
    }

    public void Identity()
    {
        direct.Identity();
        reciprocal.Identity();
    }

    public Matrix2D Move(float x, float y)
    {
        Matrix2D m = CreateShift(x, y);
        Multiply(m);
        return this;
    }

    public Matrix2D Scale(float sx, float sy)
    {
        //transform.localScale = new Vector2(w, h);
        //scale = new Vector2(w * 640, h * 480);
        Matrix2D m = CreateScale(sx, sy);
        Multiply(m);
        return this;
    }
    public Matrix2D Rotate(float x, float y, float a)
    {
        Matrix2D m = CreateRotation(x, y, a);
        Multiply(m);
        return this;
    }

    public Matrix2D Rotate(float a)
    {
        Matrix2D m = CreateRotation(a);
        Multiply(m);
        return this;
    }

    private static Matrix2D CreateRotation(float x, float y, float a)
    {
        Matrix2D m = new Matrix2D();
        m.Multiply(CreateShift(x, y), CreateRotation(a)).Multiply(CreateShift(-x, -y));
        return m;
    }

    static Matrix2D CreateRotation(float a)
    {
        Matrix2D m = new Matrix2D();
        Vector2 sincos = Vector2.zero;
        Storm.Math.SinCos(a, out sincos.x, out sincos.y);
        m.direct.Org = new Vector2(0, 0);
        m.direct.Right = new Vector2(sincos.y, sincos.x);
        m.direct.Up = new Vector2(-sincos.x, sincos.y);
        m.reciprocal.Org = new Vector2(0, 0);
        m.reciprocal.Right = new Vector2(sincos.y, -sincos.x);
        m.reciprocal.Up = new Vector2(sincos.x, sincos.y);

        return m;
    }

    private static Matrix2D CreateShift(float x, float y)
    {
        Matrix2D m = new Matrix2D();
        m.direct.Org = new Vector2(x, y);
        m.direct.Right = new Vector2(1, 0);
        m.direct.Up = new Vector2(0, 1);
        m.reciprocal.Org = new Vector2(-x, -y);
        m.reciprocal.Right = new Vector2(1, 0);
        m.reciprocal.Up = new Vector2(0, 1);
        return m;
    }

    private static Matrix2D CreateScale(float sx, float sy)
    {
        Matrix2D m = new Matrix2D();
        m.direct.Org = new Vector3(0, 0);
        m.direct.Right = new Vector3(sx, 0);
        m.direct.Up = new Vector3(0, sy);
        m.reciprocal.Org = new Vector3(0, 0);
        m.reciprocal.Right = new Vector3(1 / sx, 0);
        m.reciprocal.Up = new Vector3(0, 1 / sy);
        return m;
    }

    Matrix2D Multiply(Matrix2D m0)
    {
        direct.Multiply(m0.direct);
        Mat2D m = m0.reciprocal; m.Multiply(reciprocal); reciprocal = m;
        return this;
    }

    Matrix2D Multiply(Matrix2D m0, Matrix2D m1)
    {
        direct.Multiply(m0.direct, m1.direct);
        reciprocal.Multiply(m1.reciprocal, m0.reciprocal);
        return this;
    }

    public Vector3 Transform(Vector3 src, bool direct = true, bool transpose = false)
    {
        return (direct ? this.direct : this.reciprocal).Transform(src, transpose);
    }

    public Vector2 TransformVector(Vector2 pos, bool direct = true, bool transpose = false)
    {
        return (direct ? this.direct : this.reciprocal).TransformVector(pos, transpose);
    }
    public Vector2 TransformPoint(Vector2 pos, bool direct = true)
    {
        return (direct ? this.direct : this.reciprocal).TransformPoint(pos);
    }

}

public struct Mat2D
{
    public Vector2 Org, Right, Up;
    public void Identity()
    {
        Org = new Vector2(0, 0);
        Up = new Vector2(0, 1);
        Right = new Vector2(1, 0);
    }
    public Mat2D Multiply(Mat2D m0, Mat2D m1)
    {
        Org = m0.Right * m1.Org.x + m0.Up * m1.Org.y + m0.Org;
        Up = m0.Right * m1.Up.x + m0.Up * m1.Up.y;
        Right = m0.Right * m1.Right.x + m0.Up * m1.Right.y;
        return this;
    }
    public Mat2D Multiply(Mat2D m1)
    {
        Vector2 _Org = Right * m1.Org.x + Up * m1.Org.y + Org;
        Vector2 _Up = Right * m1.Up.x + Up * m1.Up.y;
        Vector2 _Right = Right * m1.Right.x + Up * m1.Right.y;
        Org = _Org;
        Right = _Right;
        Up = _Up;
        return this;
    }
    public Vector3 Transform(Vector3 src, bool transpose = false)
    {
        Vector2 v2;
        if (transpose)
        {
            v2 = TransformVector(new Vector2(src.x, src.y), true);
            float zVec = Vector3.Dot(new Vector3(Org.x, Org.y, 1), src);
            return new Vector3(v2.x, v2.y, zVec);
        }
        else
        {
            v2 = TransformVector(new Vector2(src.x, src.y), false);
            Vector2 tVec = v2 + Org * src.z;
            return new Vector3(tVec.x, tVec.y, src.z);
        }
    }
    public Vector2 TransformVector(Vector2 pos, bool transpose = false)
    {
        if (transpose)
        {
            return new Vector2(Right.x, Up.x) * pos.x + new Vector2(Right.y, Up.y) * pos.y;
        }
        else
        {
            return Right * pos.x + Up * pos.y;
        }
    }
    public Vector2 TransformPoint(Vector2 pos)
    {
        return TransformVector(pos) + new Vector2(Org.x, Org.y);
    }
};
