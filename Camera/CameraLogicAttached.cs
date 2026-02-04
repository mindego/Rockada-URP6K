using System;
using UnityEngine;
using DWORD = System.UInt32;
public class CameraDataAttached : BaseData
{
    // данные для Attached-режимов
    public const uint CAMERA_DATA_ATTACHED = 0x470CEBBD;  // CameraDataAttached
    public const uint CAMERA_DATA_ATTACHED_XZ = 0x9F6FC3BF;  // CameraDataAttachedXZ

    public float CameraAngleX;
    public float CameraAngleY;
    public float Scale;
    //CameraDataAttached(BaseScene * s, DWORD c) : BaseData(s, c),CameraAngleX(GRD2RD(-35)),CameraAngleY(0),Scale(1.25) { }

    public CameraDataAttached(BaseScene s, DWORD c) : base(s, c)
    {
        CameraAngleX = Storm.Math.GRD2RD(-35); 
        CameraAngleY = 0;
        Scale = 1.25f;
    }
};

//public class CameraLogicCockpit : CameraLogicAttached
//{
//    public static uint CockpitCode = Hasher.CodeString("cockpit");

//    // от CameraLogic
//    public CameraLogicCockpit(CameraLogic prev, DWORD c) : base(prev, c)
//    {
//        Code = c;
//        rOwnData = rVisual.GetCameraDataCockpit();
//        LastHandle = Constants.THANDLE_INVALID;
//        pCockpit = null;
//        rVisual.GetScene().showSfgs(true);

//    }
//    /// <summary>
//    /// обновиться
//    /// </summary>
//    /// <param name="scale"></param>
//    public override void Update(float scale)
//    {
//        BaseCraft pCraft = GetCraft(rData.GetRef());
//        rOwnData.SetCraft(pCraft);

//        if (pCraft == null)
//        {
//            ResetLastCraft();
//            LastHandle = Constants.THANDLE_INVALID;
//            mDesiredConfig = rVisual.GetSceneNormalConfig();
//            mDesiredInfra = false;
//            base.Update(scale);
//            return;
//        }

//        // если поменялся крафт
//        if (LastHandle != pCraft.GetHandle())
//        {
//            ResetLastCraft();
//            LastHandle = pCraft.GetHandle();
//            pCockpit = (Wedge.FPO)pCraft.GetFpo().GetSubObject(CockpitCode);

//        }
//        // ставим имидж кабины
//        if (pCockpit != null)
//        {
//            //if (pCockpit.SetImage(3, RoFlags.FSI_EQUAL_LINKS, (void*)pCockpit.Link, false) == 0)
//            if (pCockpit.SetImage(3, (int)RoFlags.FSI_EQUAL_LINKS, pCockpit.Link!=null, false) == 0)
//            {
//                pCockpit = null;
//                ResetLastCraft();
//            }
//        }
//        //TODO: Закончить!

//    }
//    /// <summary>
//    /// имя режима
//    /// </summary>
//    /// <returns></returns>
//    public override string GetName()
//    {
//        return (Code == CameraDefines.iCmCockpit ? "cockpit" : "cockpit_tracking");
//    }
//    public override CameraLogic GetCameraLogic(DWORD code)
//    {
//        return (code == CameraDefines.iCmCockpit || code == CameraDefines.iCmCockpitTracking) ? this : base.GetCameraLogic(code);
//    }
//    public override void Draw2(float[] ViewPort)
//    {
//        if (rOwnData.mCam2Active)
//        {
//            float[] ViewPort2 = new float[4];
//            rOwnData.mCamViewPort.convertFrom(ViewPort, out ViewPort2);

//            rVisual.GetSceneApi().SetCamera(rOwnData.mCamera2);
//            rVisual.GetSceneApi().SetViewport(rOwnData.mCameraAspect2, ViewPort2);
//            rVisual.GetSceneApi().SetVision(SCENE_VISION.SV_INFRA);
//            rVisual.GetSceneApi().Draw();
//        }

//    }


//    // own
//    protected DWORD LastHandle;
//    protected DWORD Code;
//    protected CameraDataCockpit rOwnData;
//    protected Wedge.FPO pCockpit;
//    protected BaseCraft GetCraft(DWORD h)
//    {
//        iContact c = rScene.GetContact(h);
//        return (c != null ? c.GetInterface<BaseCraft>() : null);
//    }
//    protected void ResetLastCraft()
//    {
//        if (LastHandle == Constants.THANDLE_INVALID) return;
//        BaseCraft pCraft = GetCraft(LastHandle);
//        if (pCraft == null) return;
//        pCraft.GetRoot().ResetImage();

//    }
//    protected void SetVision(int vis_mode) { }


//};

///// <summary>
///// данные для радара и карты
///// </summary>
//public class CameraDataMap : BaseData, CommLink
//{
//    // defines команд и перменных
//    public const string sCmMapRange = "cm_map_range";
//    public const uint iCmMapRange = 0xFDC18019;

//    private float MapRange;

//    public float GetMapRange() { return MapRange; }
//    public void SetMapRange(float r)
//    {
//        MapRange = Mathf.Clamp(r, 500f, 100000f);
//    }
//    public CameraDataMap(BaseScene s) : base(s, CameraDefines.CAMERA_MAP_DATA)
//    {
//        MapRange = 20000f;
//        rScene.GetCommandsApi().RegisterCommand(sCmMapRange, this, 1, "set map range");
//    }
//    ~CameraDataMap()
//    {
//        rScene.GetCommandsApi().UnRegister(this);
//    }

//    // от CommLink
//    public void OnCommand(uint code, string arg1, string arg2)
//    {
//        switch (code)
//        {
//            case iCmMapRange:
//                if (arg1.StartsWith('-') || arg1.StartsWith('+')) SetMapRange(MapRange + float.Parse(arg1));
//                else SetMapRange(float.Parse(arg1));
//                //# ifdef _DEBUG
//                //                rScene.Message("MapRange=%f", MapRange);
//                //#endif
//                return;
//        }

//    }
//};

//public struct CamViewPort
//{
//    public float x, y, w, h;
//    public void convertFrom(float[] ParentVP, out float[] DestVp)
//    {
//        DestVp = new float[4];
//        float x_scale = ParentVP[2];
//        float y_scale = ParentVP[3];
//        DestVp[0] = ParentVP[0] + x * x_scale;
//        DestVp[1] = ParentVP[1] + y * y_scale;
//        DestVp[2] = w * x_scale;
//        DestVp[3] = h * y_scale;
//    }

//};

//public enum Viewports { VIEWPORT_OFF, VIEWPORT_TARGET, VIEWPORT_MISSILE, VIEWPORT_REAR };

///// <summary>
///// CameraDataCockpit - странный класс
///// </summary>
//public class CameraDataCockpit : BaseData, CommLink
//{
//    public static readonly float MIN_CAMERA_PITCH = Storm.Math.GRD2RD(-45);
//    public static readonly float MAX_CAMERA_PITCH = Storm.Math.GRD2RD(80);
//    public static readonly float MAX_CAMERA_YAW = Storm.Math.GRD2RD(155);

//    public const float HudDeviceMin = .02f;
//    public const float HudDeviceMax = .2f;

//    public const float CrossSizeLim = 0.9999619f;  // 89.5 degrees

//    public const float gsLampRadius = .008f;

//    public const float gsSituationLampX = .734f + gsLampRadius;
//    public const float gsSituationLampY = .263f + gsLampRadius;

//    public const float gsObjectivesLampX = gsSituationLampX;
//    public const float gsObjectivesLampY = .333f + gsLampRadius;

//    public const float gsMenuLampX = gsSituationLampX;
//    public const float gsMenuLampY = .403f + gsLampRadius;

//    public const float gsAutopilotLampX = gsSituationLampX;
//    public const float gsAutopilotLampY = .473f + gsLampRadius;

//    public const float gsCameraShakeMax = 0.01745329f;
//    public const float gsCameraShakeDamageC = gsCameraShakeMax * .005f;
//    public const float gsCameraShakeSpeedC = gsCameraShakeMax * .01f;
//    public const float gsCameraShakeDecayC = gsCameraShakeMax * .5f;

//    public const float gsMinAlt = 50;
//    public const float gsHudOpacitySpeed = .5f;


//    // приборы
//    private IHUD pHud;
//    private HudDevice pCompass;
//    private HudDevice pAltimeter;
//    private HudDevice pSpeedmeter;
//    private HudDevice pRadar;
//    private HudDevice pThreat;
//    private HudDevice pWeapon;
//    private HudDevice pHorizon;
//    private HudDevice pDamage;
//    private HudDevice pEnergy;
//    private HudDevice pEnergyScale;
//    private HudDevice pRecticles;

//    HudDevice pViewPort;
//    public CamViewPort mCamViewPort;
//    public float mCameraAspect2;
//    public Storm.MyPos mCamera2;
//    public int mHUDViewportMode;
//    public bool myGlobalShowToma;

//    public bool mCam2Active;

//    public void updateViewPort()
//    {
//        if (pCraft == null)
//            enableViewPort(false, false);
//        else
//            switch (mHUDViewportMode)
//            {
//                case (int)Viewports.VIEWPORT_MISSILE:
//                    {
//                        // выставляем камеру на ракету
//                        ProjectileMissile missile = pCraft.getActiveMissile();
//                        if (missile != null)
//                        {
//                            mCamera2 = missile.GetMatrix();
//                            mCamera2.Org += mCamera2.Dir * 3;
//                            //mCameraAspect2 = aspect(Mathf.PI * .5f);
//                            enableViewPort(true, true, 'M');
//                        }
//                        else
//                            enableViewPort(false, true, 'M');

//                    }
//                    break;

//                case (int)Viewports.VIEWPORT_TARGET:
//                    {
//                        WeaponSystemForBaseCraft w = pCraft.GetWeaponSystem();
//                        iContact pTarget = w!=null ? w.GetTarget() : null;
//                        if (pTarget != null)
//                        {
//                            // выставляем камеру на цель
//                            Storm.MyPos pos = new Storm.MyPos(pCraft.GetPosition());

//                            Vector3 to_target = pTarget.GetOrg() - pos.Org;
//                            Vector3 dir = to_target; dir.Normalize();
//                            mCamera2.SetHorizontal(dir);
//                            mCamera2.Org = pos.Org + mCamera2.Dir * (pCraft.GetRadius() * 1.1f);

//                            mCameraAspect2 = Storm.Math.SphereAspect2(to_target.sqrMagnitude, pTarget.GetRadius() * 1.0f);
//                            mCameraAspect2 = Mathf.Max(mCameraAspect2, .875f);

//                            // rScene.CriticalMessage( "mCameraAspect2 = %f", mCameraAspect2);

//                            enableViewPort(true, true, 'T');
//                        }
//                        else
//                            enableViewPort(false, true, 'T');
//                    }
//                    break;
//                case (int)Viewports.VIEWPORT_REAR:
//                    {
//                        // выставляем камеру взад
//                        Storm.MyPos pos = new Storm.MyPos(pCraft.GetPosition());
//                        Vector3 d = pCraft.Dt().BackViewDelta;
//                        mCamera2.Org = pos.Org + (pos.Dir * d.z + pos.Right * d.x + pos.Up * d.y);
//                        mCamera2.Dir = -pos.Dir;
//                        mCamera2.Up = pos.Up;
//                        mCamera2.Right = -pos.Right;
//                        mCameraAspect2 = Storm.Math.aspect(Mathf.PI * .5f);
//                        enableViewPort(true, true, 'R');
//                    }
//                    break;

//                case (int)Viewports.VIEWPORT_OFF:

//                    enableViewPort(false, false);
//                    break;

//            }


//    }
//    public void enableViewPort(bool viewPort, bool hudDevice, int signChar = 0)
//    {
//        mCam2Active = viewPort;
//        pViewPort.Hide(!hudDevice);
//        HudViewPortData hvpd = (HudViewPortData)pViewPort.GetData();

//        hvpd.mMarkText[0] = signChar ==0 ? '\0':'+';
//        hvpd.mEmpty = hudDevice && !viewPort;

//    }
//    public bool isViewPortEnabled() { return mCam2Active; }


//    protected RecticleData pCross;
//    protected RecticleData pMouse;
//    protected RecticleData pWptPointer;
//    protected RecticleData pWptPointerArrow;
//    protected RecticleData pTargetBox;
//    protected RecticleData pTargetBoxArrow;
//    protected RecticleData pAimCue;
//    protected RecticleData pSuggestedTargetBox;
//    protected RecticleData pSuggestedTargetBoxArrow;
//    protected RecticleData pMissileCue;
//    protected RecticleData pAutopilotLamp;
//    protected RecticleData pSituationLamp;
//    protected RecticleData pObjectivesLamp;
//    protected RecticleData pMenuLamp;
//    protected int TargetsEnumerOptions;
//    protected CameraRadarAndHudEnumer pTargetsEnumer;
//    protected CameraData rData;
//    protected float CrossSize;
//    protected float CrossSizeMin;
//    protected float CrossSizeMax;
//    protected float CrossSizeSpd;
//    protected float RadarRange;
//    private bool ProjectRecticle(RecticleData pData, Vector3 Org, float R, float Rscr = 1)
//    { return true; }

//    // углы поворота и тряска камеры
//    private float AngleX, AngleXDelta;
//    private float AngleY, AngleYDelta;
//    private float HitShakeValue;
//    public float GetAngleX() { return AngleX + AngleXDelta; }
//    public float GetAngleY() { return AngleY + AngleYDelta; }
//    public void Smooth(float tx, float ty, float spd) { }
//    public void GetPosAndR(float x, float y, float r, float xa, float ya, float ra)
//    {
//        Storm.MyPos m = rData.myCamera;
//        m.TurnRightPrec((AngleY + AngleYDelta - ya));
//        m.TurnUpPrec((AngleX + AngleXDelta - xa));
//        float d = Vector3.Dot(rData.myCamera.Dir,m.Dir);
//        if (d < 0.1f)
//        {
//            if (x != 0) x = -1f;
//            if (y != 0) y = -1f;
//            if (r != 0) r = 0;
//        }
//        else
//        {
//            d = rData.CameraAspect / d;
//            if (x != 0) x = 0.5f - Vector3.Dot(rData.myCamera.Right,m.Dir) * d;
//            if (y != 0) y = 0.375f + Vector3.Dot(rData.myCamera.Up, m.Dir) * d;
//            if (r != 0) r = ra * d;
//        }

//    }
//    public void AddDamageShake(float d)
//    {
//        HitShakeValue += d * gsCameraShakeDamageC;
//        if (HitShakeValue > gsCameraShakeMax) HitShakeValue = gsCameraShakeMax;
//    }
//    // own
//    private SceneVisualizer rVisual;
//    private BaseCraft pCraft;
//    private IWave mpShakeSound;
//    private IWave mpLowAltSound;
//    private float mRadarRange;
//    private float mHudOpacity;
//    private BaseWeaponSlot myCurrentWS;

//    public void SetRadarRange(float r)
//    {
//        mRadarRange = Mathf.Clamp(r, 500f, 20000f);
//    }
//    public float GetRadarRange() { return mRadarRange; }

//    // создание/удаление
//    public CameraDataCockpit(BaseScene s, IHUD h, CameraData d) : base(s, 0x68822B1C)
//    {
//        rVisual = (s.GetSceneVisualizer());
//        pHud = (h);
//        rData = d;
//            pCraft = null;
//        mRadarRange = (5000);
//        myCurrentWS = null;
//            pCompass = pHud.CreateCompass(HudDataDefines.iCompas);
//            pViewPort = pHud.CreateViewPort(HudDataDefines.iViewPort);
//            pAltimeter = pHud.CreateVscale(HudDataDefines.iAltimeter);
//            pSpeedmeter = pHud.CreateVscale(HudDataDefines.iSpeedmeter);
//            pThreat = pHud.CreateBar(HudDataDefines.iThreat);
//            pWeapon = pHud.CreateWeapon(HudDataDefines.iWeapon);
//            pHorizon = pHud.CreateAviaHorizon(HudDataDefines.iHorizon);
//            pDamage = pHud.CreateDamage(HudDataDefines.iDamage);
//            pEnergy = pHud.CreateBar(HudDataDefines.iEnergy);
//            pEnergyScale = pHud.CreateBar(HudDataDefines.iEnergyScale);
//            pRecticles = pHud.CreateRecticles(HudDataDefines.iRecticles);
//            pWptPointer = ((HudRecticlesData)pRecticles.GetData()).AddItem(RecticleData.TYPE.CURRENTWPT, HudDeviceMin * .35f, HudDeviceMin * .35f, HudDataColors.HUDCOLOR_NORMAL);
//            pWptPointerArrow = ((HudRecticlesData)pRecticles.GetData()).AddItem(RecticleData.TYPE.ARROW, HudDeviceMin * .35f, HudDeviceMin * .35f, HudDataColors.HUDCOLOR_NORMAL);
//            pCross = ((HudRecticlesData)pRecticles.GetData()).AddItem(RecticleData.TYPE.GUNSIGHT, 0, 0, HudDataColors.HUDCOLOR_GUNSIGN);
//            pMouse = ((HudRecticlesData)pRecticles.GetData()).AddItem(RecticleData.TYPE.ROMB, HudDeviceMin * .5f, HudDeviceMin * .5f);
//            pTargetBox = ((HudRecticlesData)pRecticles.GetData()).AddItem(RecticleData.TYPE.CURRENTTARGET);
//            pTargetBoxArrow = ((HudRecticlesData)pRecticles.GetData()).AddItem(RecticleData.TYPE.ARROW, HudDeviceMin * .5f, HudDeviceMin * .5f);
//            pAimCue = ((HudRecticlesData)pRecticles.GetData()).AddItem(RecticleData.TYPE.RING);
//            pSuggestedTargetBox = ((HudRecticlesData)pRecticles.GetData()).AddItem(RecticleData.TYPE.ANIMETARGET);
//        pSuggestedTargetBoxArrow = ((HudRecticlesData)pRecticles.GetData()).AddItem(RecticleData.TYPE.ANIMEARROW, HudDeviceMin * .4f, HudDeviceMin * .4f);
//            pMissileCue = ((HudRecticlesData)pRecticles.GetData()).AddItem(RecticleData.TYPE.MISSILECONTROL);
//            pAutopilotLamp = ((HudRecticlesData)pRecticles.GetData()).AddItem(RecticleData.TYPE.LAMP_A, gsLampRadius, gsLampRadius);
//            pAutopilotLamp.x = gsAutopilotLampX;
//            pAutopilotLamp.y = gsAutopilotLampY;
//            pSituationLamp = ((HudRecticlesData)pRecticles.GetData()).AddItem(RecticleData.TYPE.LAMP_T, gsLampRadius, gsLampRadius);
//            pSituationLamp.x = gsSituationLampX;
//            pSituationLamp.y = gsSituationLampY;
//            pObjectivesLamp = ((HudRecticlesData)pRecticles.GetData()).AddItem(RecticleData.TYPE.LAMP_O, gsLampRadius, gsLampRadius);
//            pObjectivesLamp.x = gsObjectivesLampX;
//            pObjectivesLamp.y = gsObjectivesLampY;
//            pMenuLamp = ((HudRecticlesData)pRecticles.GetData()).AddItem(RecticleData.TYPE.LAMP_M, gsLampRadius, gsLampRadius);
//            pMenuLamp.x = gsMenuLampX;
//            pMenuLamp.y = gsMenuLampY;
//        MapData md = new();
//            md.Name = rScene.GetGameData().mpGameMapName;
//            md.MapSizeX = rScene.GetGameData().mGameMapSizeX;
//            md.MapSizeY = rScene.GetGameData().mGameMapSizeZ;
//            md.SizeX = rScene.GetTerrain().GetXSize();
//            md.SizeY = rScene.GetTerrain().GetZSize();
//            pRadar = pHud.CreateMap(HudDataDefines.iRadar, md);
//        TargetsEnumerOptions = (rScene.GetGameData().mShowFriendlyName? 1:0) |
//    ((rScene.GetGameData().mShowFriendlyType ? 1:0) << 1) |
//    ((rScene.GetGameData().mShowFriendlyDist ? 1 : 0) << 2) |
//    ((rScene.GetGameData().mShowEnemyName ? 1 : 0) << 4) |
//    ((rScene.GetGameData().mShowEnemyType ? 1 : 0) << 5) |
//    ((rScene.GetGameData().mShowEnemyDist ? 1 : 0) << 6);
//        pTargetsEnumer = new CameraRadarAndHudEnumer(rScene, (HudRadarData)pRadar.GetData(), (HudRecticlesData)pRecticles.GetData(), rData);
//        // other data
//        CrossSizeMin = .035f;
//        CrossSizeMax = .05f;
//        CrossSizeSpd = .1f;

//        // register cross commands
//        rScene.GetCommandsApi().RegisterVariable("hud_cross_min", this, VType.VAR_FLOAT, "set min cross size");
//        rScene.GetCommandsApi().RegisterVariable("hud_cross_max", this, VType.VAR_FLOAT, "set max cross size");
//        rScene.GetCommandsApi().RegisterVariable("hud_cross_speed", this, VType.VAR_FLOAT, "set speed for cross size change");

//        CrossSize = CrossSizeMax;
//        AngleX = 0; AngleXDelta = 0;
//        AngleY = 0; AngleYDelta = 0;
//        HitShakeValue = 0;
//        mpShakeSound = rScene.GetRadioEnvironment().CreateWave(0, "CockpitShake");
//        mpLowAltSound = rScene.GetRadioEnvironment().CreateWave(0, "LowAlt");


//        // register
//        rScene.GetCommandsApi().RegisterCommand("cl_icons_friendly", this, 0, "switch friendly icons mode");
//        rScene.GetCommandsApi().RegisterCommand("cl_icons_enemy", this, 0, "switch enemy icons mode");
//        rScene.GetCommandsApi().RegisterCommand("cm_radar_range", this, 1, "set radar range");
//        rScene.GetCommandsApi().RegisterCommand("cl_switch_vs", this, 1, "set HUD viewport mode");
//        // set default HUD viewport mode
//        mHUDViewportMode = (int) Viewports.VIEWPORT_OFF;

//        // register toma commands
//        rScene.GetCommandsApi().RegisterVariable("hud_tomat_x", this, VType.VAR_FLOAT, "set toma(t) x coord");
//        rScene.GetCommandsApi().RegisterVariable("hud_tomat_y", this, VType.VAR_FLOAT, "set toma(t) y coord");
//        rScene.GetCommandsApi().RegisterVariable("hud_tomat_w", this, VType.VAR_FLOAT, "set toma(t) width");
//        rScene.GetCommandsApi().RegisterVariable("hud_tomat_h", this, VType.VAR_FLOAT, "set toma(t) height");
//        rScene.GetCommandsApi().RegisterVariable("hud_tomao_x", this, VType.VAR_FLOAT, "set toma(o) x coord");
//        rScene.GetCommandsApi().RegisterVariable("hud_tomao_y", this, VType.VAR_FLOAT, "set toma(o) y coord");
//        rScene.GetCommandsApi().RegisterVariable("hud_tomao_w", this, VType.VAR_FLOAT, "set toma(o) width");
//        rScene.GetCommandsApi().RegisterVariable("hud_tomao_h", this, VType.VAR_FLOAT, "set toma(o) height");
//        rScene.GetCommandsApi().RegisterVariable("hud_tomam_x", this, VType.VAR_FLOAT, "set toma(m) x coord");
//        rScene.GetCommandsApi().RegisterVariable("hud_tomam_y", this, VType.VAR_FLOAT, "set toma(m) y coord");
//        rScene.GetCommandsApi().RegisterVariable("hud_tomam_w", this, VType.VAR_FLOAT, "set toma(m) width");
//        rScene.GetCommandsApi().RegisterVariable("hud_tomam_h", this, VType.VAR_FLOAT, "set toma(m) height");
//        rScene.GetCommandsApi().RegisterVariable("hud_tomaa_x", this, VType.VAR_FLOAT, "set toma(a) x coord");
//        rScene.GetCommandsApi().RegisterVariable("hud_tomaa_y", this, VType.VAR_FLOAT, "set toma(a) y coord");
//        rScene.GetCommandsApi().RegisterVariable("hud_tomaa_w", this, VType.VAR_FLOAT, "set toma(a) width");
//        rScene.GetCommandsApi().RegisterVariable("hud_tomaa_h", this, VType.VAR_FLOAT, "set toma(a) height");
//        rScene.GetCommandsApi().RegisterCommand("hud_toma", this, 1, "show/hide toma");
//        myGlobalShowToma = true;



//        // init
//        SetCraft(null, true);

//    }
//    ~CameraDataCockpit() { }
//    public void SetCraft(BaseCraft pCraft, bool Force = false) { }
//    public void Update(float scale, bool LookBehind) { }

//    // от CommLink
//    public void OnCommand(int i, string arg1, string arg2) { }
//    public object OnVariable(int i, object o) { return default; }
//};

//public class CameraLogicMap : CameraLogic
//{
//    public HudDevice pRadar;
//    public bool TrackingLost;
//    public CameraDataMap rMapData;
//    CameraRadarEnumer pTargetsEnumer;
//    public CameraLogicMap(CameraLogic prev) : base(prev)
//    {
//        rMapData = rScene.GetSceneVisualizer().GetCameraDataMap();
//        TrackingLost = false;
//        MapData md = new();
//        md.Name = rScene.GetGameData().mpGameMapName;
//        md.MapSizeX = rScene.GetGameData().mGameMapSizeX;
//        md.MapSizeY = rScene.GetGameData().mGameMapSizeZ;
//        md.SizeX = rScene.GetTerrain().GetXSize();
//        md.SizeY = rScene.GetTerrain().GetZSize();
//        pRadar = rVisual.GetHud().CreateMap(HudDataDefines.iMap, md);
//        pRadar.Hide(false);
//        pTargetsEnumer = new CameraRadarEnumer((HudRadarData)pRadar.GetData());

//    }
//    //~CameraLogicMap();
//    /// <summary>
//    /// обновиться
//    /// </summary>
//    /// <param name=""></param>
//    public override void Update(float scale)
//    {
//        iContact u = rData.GetContact();
//        iContact t = null;
//        if (u != null)
//        {
//            iWeaponSystemDedicated w = u.GetInterface<iWeaponSystemDedicated>();
//            if (w != null) t = w.GetTarget();
//        }
//        // двигаем камеру
//        if (rData.GetCameraRightLeft() != 0)
//        {
//            TrackingLost = true;
//            rData.myCamera.Org.x += ((rMapData.GetMapRange() * .25f * scale) * (rData.GetCameraRightLeft() / rData.GetCameraAngleSpeed()));
//        }
//        if (rData.GetCameraUpDown() != 0)
//        {
//            TrackingLost = true;
//            rData.myCamera.Org.z += ((rMapData.GetMapRange() * .25f * scale) * (rData.GetCameraUpDown() / rData.GetCameraAngleSpeed()));
//        }
//        if (rData.GetCameraMoveZ() != 0)
//            rMapData.SetMapRange(rMapData.GetMapRange() * (1f - scale * (rData.GetCameraMoveZ() / rData.GetCameraSpeed())));
//        if (TrackingLost == false && u != null)
//            if (u != null) rData.myCamera.Org = u.GetOrg();
//        HudRadarData d = (HudRadarData)pRadar.GetData();
//        d.range = rMapData.GetMapRange();
//        d.org = rData.myCamera.Org;
//        d.angle = 0;
//        d.changed = true;
//        pTargetsEnumer.Enumerate(u, t, (u!=null ? u.GetInterface<iSensors>() : null));

//        PlayerInterface pi;
//        if ((pi = rVisual.GetPlayerInterface())!=null)
//        {
//            BaseCraftController cc = pi.GetInterface<BaseCraftController>();
//            Vector3 org = Vector3.zero;
//            if (cc != null && getWaypoint(cc, ref org)!=0)
//                pTargetsEnumer.addWaypoint(org);
//        }
//        rVisual.Get3DSound().SetCamera(rData.myCamera, Vector3.zero, SoundApiDefines.SM_EXTERN);
//        /*
//          // The Sample of set waypoints and line between waypoints
//          d.AddRadarLine(0,0,0,5000,0);
//          d.AddRadarLine(0,5000,0,10000,5000);
//          d.AddRadarLine(0,10000,5000,10000,10000);

//          d.AddDynamicRadarItem(RadarData::WAYPOINT,0,VECTOR(0,0,0),0);
//          d.AddDynamicRadarItem(RadarData::WAYPOINT,0,VECTOR(5000,0,0),0);
//          d.AddDynamicRadarItem(RadarData::WAYPOINT,0,VECTOR(10000,0,5000),0);
//          d.AddDynamicRadarItem(RadarData::WAYPOINT,0,VECTOR(10000,0,10000),0);
//          d.AddDynamicRadarItem(RadarData::WAYPOINT,0,VECTOR(20000,0,20000),0);
//        */
//    }

//    public int getWaypoint(BaseCraftController cc, ref Vector3 org)
//    {
//        iContact ldr;
//        if (cc.isShowedWaypoint())
//        {
//            org = cc.GetAutopilotOrg();
//            return 2;
//        }
//        if ((ldr = cc.GetAutopiloLeader()) != null) {
//            org = ldr.GetOrg();
//            return 1;
//        }
//        else if (!cc.IsPaused())
//        {
//            org = cc.GetAutopilotOrg();
//            return 2; 
//        }
//        return 0;
//    }


//    /// <summary>
//    /// отрисовать сцену в очке
//    /// </summary>
//    /// <param name="Viewport"></param>
//    public override void Draw(float[] Viewport)
//    {
//        rVisual.GetHud().Draw(Viewport);
//    }
//    /// <summary>
//    /// имя режима
//    /// </summary>
//    /// <returns></returns>
//    public override string GetName()
//    {
//        return "map";

//    }
//    public override CameraLogic GetCameraLogic(DWORD code)
//    {
//        return (code == CameraDefines.iCmMap ? this : base.GetCameraLogic(code));
//    }
//};



//public class CameraLogicTactical : CameraLogicAttached
//{
//    new private DWORD Code;
//    public CameraLogicTactical(CameraLogic prev, DWORD code) : base(prev, CameraDefines.iCmAttached)
//    {

//    }
//    /// <summary>
//    /// обновиться
//    /// </summary>
//    /// <param name=""></param>
//    public override void Update(float scale)
//    {
//        // определяем первый юнит
//        iContact u1 = rData.GetContact();
//        if (u1 == null || u1.GetState() == iSensorsDefines.CS_DEAD) { base.Update(scale); return; }
//        // определяем второй юнит как видимую цель первого
//        iContact u2 = null;
//        iWeaponSystemDedicated w = u1.GetInterface<iWeaponSystemDedicated>();
//        if (w != null)
//        {
//            u2 = w.GetTarget();
//            if (u2 !=null && (u2.GetState() == iSensorsDefines.CS_DEAD || u2.GetAge() > .0f))
//                u2 = null;
//        }
//        // запоминаем старый Ref
//        DWORD OldRef = rData.GetRef();
//        // определяем углы для CameraLogicAttached
//        if (u2 != null)
//        {
//            // обратный режим
//            if (Code == CameraDefines.iCmTacticalInversed) { iContact tmp = u1; u1 = u2; u2 = tmp; }
//            rData.SetRef(u1.GetHandle());
//            // определяем углы для CameraLogicAttached
//            Vector3 delta = u2.GetOrg() - u1.GetOrg();
//            if (Mathf.Abs(delta.x) + Mathf.Abs(delta.z) > 0)
//            {
//                pAttachedData.CameraAngleY = Mathf.Atan2(delta.x, delta.z);
//                pAttachedData.CameraAngleX = Mathf.Asin(delta.y / delta.magnitude);
//            }
//            pAttachedData.Scale = 2f;
//            // чтобы не реагировать на клавиатуру
//            scale = 0;
//        }
//        // поворачиваем камеру
//        base.Update(scale);
//        // восстанавливаем старый Ref
//        rData.SetRef(OldRef);

//    }
//    /// <summary>
//    /// имя режима
//    /// </summary>
//    /// <returns></returns>
//    public override string GetName()
//    {
//        return (Code == CameraDefines.iCmTactical ? "tactical" : "tactical_inversed");
//    }
//    public override CameraLogic GetCameraLogic(DWORD code)
//    {
//        return ((code == CameraDefines.iCmTactical || code == 0x6A95BBB7) ? this : base.GetCameraLogic(code));
//    }
//};

//public class CameraLogicTracking : CameraLogicFree
//{
//    private DWORD Code;
//    private float OldAspect;
//    public CameraLogicTracking(CameraLogic prev, DWORD code) : base(prev)
//    {
//        Code = code;

//        CameraLogicTracking cc = (CameraLogicTracking)prev.GetCameraLogic(code);
//        OldAspect = (cc != null ? cc.OldAspect : rData.CameraAspect);
//    }
//    //~CameraLogicTracking();
//    /// <summary>
//    /// обновиться
//    /// </summary>
//    /// <param name="scale"></param>
//    public override void Update(float scale)
//    {
//        iContact u = rData.GetContact();
//        if (u != null && u.GetState() != iSensorsDefines.CS_DEAD)
//        {
//            bool NeedReset = false;
//            Vector3 delta = u.GetOrg() - rData.myCamera.Org;
//            float d = delta.magnitude;
//            // проверка расстояния
//            if (d < u.GetRadius() || d > Mathf.Min(u.GetRadius() * 50, 1000)) NeedReset = true;
//            else
//            {
//                // проверка прямой видимости
//                delta /= d;
//                TraceInfo res = rScene.TraceLine(new Storm.Line(rData.myCamera.Org, delta, d - u.GetRadius()), u.GetHMember());
//                if (res.count!=0) NeedReset = true;
//            }
//            // переустановка
//            if (NeedReset)
//            {
//                Vector3 dir = u.GetSpeed();
//                float dist = dir.magnitude;
//                if (dist < 1) { dir.Set(0, 0, 1); dist = 1; }
//                dir *= cfn(dist * 3, u.GetRadius() * 2, Mathf.Min(u.GetRadius() * 40, 800)) / dist;
//                dist = (RandomGenerator.norm_rand()) * Mathf.PI * .5f;
//                dir += (u.GetUp() * Mathf.Cos(dist) + u.GetRight() * Mathf.Sin(dist)) * (u.GetRadius() * 2);
//                dist = dir.magnitude;
//                if (dist > 0)
//                {
//                    dir /= dist;
//                    Vector3 org = u.GetOrg();
//                    TraceInfo res = rScene.TraceLine(new Storm.Line(org, dir, dist), u.GetHMember());
//                    if (res.count > 0 && res.results[0].dist > 100)
//                    {
//                        dist = res.results[0].dist - 2f;
//                        res.count = 0;
//                    }
//                    if (res.count == 0)
//                    {
//                        rData.myCamera.Org = org + dir * dist;
//                        delta = -dir;
//                        d = dist;
//                        NeedReset = false;
//                    }
//                }
//            }
//            if (NeedReset == false)
//            {
//                // поворот и проецирование
//                rData.myCamera.SetHorizontal(delta);
//                if (Code == CameraDefines.iCmTV)
//                {
//                    rData.CameraAspect = Storm.Math.SphereAspect(d, u.GetRadius());
//                    if (rData.CameraAspect < OldAspect) rData.CameraAspect = OldAspect;
//                }
//                // блокируем перемещения от клавиатуры
//                scale = 0;
//            }
//        }
//        else
//        {
//            rData.CameraAspect = OldAspect;
//        }
//        base.Update(scale);

//    }
//    /// <summary>
//    /// имя режима
//    /// </summary>
//    /// <returns></returns>
//    public override string GetName()
//    {
//        return (Code == CameraDefines.iCmTracking ? "tracking" : "tv");
//    }
//    public override CameraLogic GetCameraLogic(DWORD code)
//    {
//        return ((code == CameraDefines.iCmTracking || code == CameraDefines.iCmTV) ? this : base.GetCameraLogic(code));
//    }

//    public float cfn(float v, float mn, float mx)
//    {
//        if (v < mn) v = mn;
//        if (v > mx) v = mx;
//        return v;
//    }

//};

//public class CameraLogicAuto : CameraLogic
//{
//    public const float SMALL_PERIOD = 2f;
//    public const float NORMAL_PERIOD = 4f;
//    public const float LARGE_PERIOD = 6f;

//    private CameraLogic pCameraLogic;
//    private float NextCheckTime;
//    public CameraLogicAuto(CameraLogic prev) : base(prev)
//    {
//        pCameraLogic = new CameraLogic(rVisual);
//        NextCheckTime = rScene.GetTime() + SMALL_PERIOD;
//    }
//    ~CameraLogicAuto()
//    {
//        pCameraLogic = null;
//    }
//    /// <summary>
//    /// обновиться
//    /// </summary>
//    /// <param name="scale"></param>
//    public override void Update(float scale)
//    {
//        //TODO: Реализовать!

//    }
//    /// <summary>
//    /// отрисовать сцену в очке
//    /// </summary>
//    /// <param name=""></param>
//    public override void Draw(float[] Viewport)
//    {
//        pCameraLogic.Draw(Viewport);
//    }
//    //// имя режима
//    public override string GetName()
//    {
//        return "auto";
//    }
//    public override CameraLogic GetCameraLogic(DWORD code)
//    {
//        if (code == CameraDefines.iCmAuto) return this;
//        return pCameraLogic.GetCameraLogic(code);

//    }
//};

/// <summary>
/// приделаный режим камеры
/// </summary>
public class CameraLogicAttached : CameraLogic
{
    // приделаный режим камеры
    //public const uint iCmAttached = 0x97589D77;  // "attached"
    //public const uint iCmAttachedXZ = 0xC0375182;  // "attached_xz"
    //public const uint iCmOrbite = 0x942FB67E;  // "orbite"
    private DWORD Code;

    CameraDataAttached pAttachedData;
    public CameraLogicAttached(CameraLogic v, DWORD c) : base(v) {
        Code = c;

        // устанавливаем дополнительные данные
        if (Code == CameraDefines.iCmAttached)
        {
            pAttachedData = (CameraDataAttached)rScene.GetBaseData(CameraDataAttached.CAMERA_DATA_ATTACHED, false);
            if (pAttachedData==null)
                pAttachedData = new CameraDataAttached(rScene, CameraDataAttached.CAMERA_DATA_ATTACHED);
        }
        else
        {
            pAttachedData = (CameraDataAttached)rScene.GetBaseData(CameraDataAttached.CAMERA_DATA_ATTACHED_XZ, false);
            if (pAttachedData==null)
                pAttachedData = new CameraDataAttached(rScene, CameraDataAttached.CAMERA_DATA_ATTACHED_XZ);
        }
    }

    public override void Update(float scale)
    {
        iContact r = rData.GetContact();
        if (r==null)
        {
            base.Update(scale);
            return;
        }
        rVisual.Get3DSound().SetCamera(rData.myCamera, r.GetSpeed(), (int)SoundApiDefines.SM_EXTERN);
        pAttachedData.Scale = Mathf.Clamp(pAttachedData.Scale - rData.GetCameraMoveZ() * scale / r.GetRadius(), 1.01f, 500f);
        if (Code != CameraDefines.iCmOrbite)
        {
            pAttachedData.CameraAngleY += rData.GetCameraRightLeft() * scale;
            pAttachedData.CameraAngleX += rData.GetCameraUpDown() * scale;
        }
        else
        {
            pAttachedData.CameraAngleY += rData.GetCameraAngleSpeed() * scale;
        }
        //if (pAttachedData.CameraAngleY > Storm.Math.PI) pAttachedData.CameraAngleY -= Storm.Math.PI_2; //TODO При необходимости перевода градусов в радианы - иметь в виду!
        //else
        //  if (pAttachedData.CameraAngleY < -Storm.Math.PI) pAttachedData.CameraAngleY += Storm.Math.PI2;
        if (pAttachedData.CameraAngleY > 180) pAttachedData.CameraAngleY -= 360; //TODO При необходимости перевода градусов в радианы - иметь в виду!
        else
          if (pAttachedData.CameraAngleY < -180) pAttachedData.CameraAngleY += 360;

        //pAttachedData.CameraAngleX = Mathf.Clamp(pAttachedData.CameraAngleX, -Storm.Math.GRD2RD(75), Storm.Math.GRD2RD(75));
        pAttachedData.CameraAngleX = Mathf.Clamp(pAttachedData.CameraAngleX, -75, 75);
        if (Code == CameraDefines.iCmAttachedXZ)
        {
            rData.myCamera.Dir = r.GetDir();
            rData.myCamera.Up = r.GetUp();
            rData.myCamera.Right = r.GetRight();
        }
        else
        {
            rData.myCamera.Dir = Vector3.forward;
            rData.myCamera.Up = Vector3.up;
            rData.myCamera.Right = Vector3.right;
        }
        rData.myCamera.TurnRightPrec(pAttachedData.CameraAngleY);
        rData.myCamera.TurnUpPrec(pAttachedData.CameraAngleX);
        rData.myCamera.Org = ((BaseContact)r).GetUnit().GetOrg();
        rData.myCamera.Org -= rData.myCamera.Dir * (pAttachedData.Scale * r.GetRadius());
        rData.myCamera.Org += rData.myCamera.Up * (pAttachedData.Scale * r.GetRadius() * .25f);
    }

    public override string GetName()
    {
        switch (Code)
        {
            case CameraDefines.iCmAttached:
                return "attached";
            case CameraDefines.iCmAttachedXZ:
                return "attached_xz";
            default:
                return "orbite";
        }
    }

    /// <summary>
    /// смена режимов камеры
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public override CameraLogic GetCameraLogic(DWORD code)
    {
        return (code == CameraDefines.iCmAttached || code == CameraDefines.iCmAttachedXZ || code == CameraDefines.iCmOrbite) ? this : base.GetCameraLogic(code);
    }
}
