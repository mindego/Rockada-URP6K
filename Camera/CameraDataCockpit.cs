using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using static HudData;
using static HudDataColors;
using static VType;
using static WpnDataDefines;
using crc32 = System.UInt32;
using DWORD = System.UInt32;

enum VIEWPORT_MODE { VIEWPORT_OFF, VIEWPORT_TARGET, VIEWPORT_MISSILE, VIEWPORT_REAR };

/// <summary>
/// CameraDataCockpit - странный класс (да, в исходниках так и написано)
/// </summary>
public class CameraDataCockpit : BaseData, CommLink
{
    public static float MIN_CAMERA_PITCH = Storm.Math.GRD2RD(-45);
    public static float MAX_CAMERA_PITCH = Storm.Math.GRD2RD(80);
    public static float MAX_CAMERA_YAW = Storm.Math.GRD2RD(155);

    public const float HudDeviceMin = .02f;
    public const float HudDeviceMax = .2f;
    const float CrossSizeLim = 0.9999619f;  // 89.5 degrees

    const float gsLampRadius = .008f;

    const float gsSituationLampX = .734f + gsLampRadius;
    const float gsSituationLampY = .263f + gsLampRadius;

    const float gsObjectivesLampX = gsSituationLampX;
    const float gsObjectivesLampY = .333f + gsLampRadius;

    const float gsMenuLampX = gsSituationLampX;
    const float gsMenuLampY = .403f + gsLampRadius;

    const float gsAutopilotLampX = gsSituationLampX;
    const float gsAutopilotLampY = .473f + gsLampRadius;

    const float gsCameraShakeMax = 0.01745329f;
    const float gsCameraShakeDamageC = gsCameraShakeMax * .005f;
    const float gsCameraShakeSpeedC = gsCameraShakeMax * .01f;
    const float gsCameraShakeDecayC = gsCameraShakeMax * .5f;

    const float gsMinAlt = 50;
    const float gsHudOpacitySpeed = .5f;

    // приборы
    private IHUD pHud;
    HudDevice pCompass;
    HudDevice pAltimeter;
    HudDevice pSpeedmeter;
    HudDevice pRadar;
    HudDevice pThreat;
    HudDevice pWeapon;
    HudDevice pHorizon;
    HudDevice pDamage;
    HudDevice pEnergy;
    HudDevice pEnergyScale;
    HudDevice pRecticles;

    HudDevice pViewPort;
    public CamViewPort mCamViewPort;
    public float mCameraAspect2;
    public MATRIX mCamera2;
    public int mHUDViewportMode;
    public bool myGlobalShowToma;

    public bool mCam2Active;

    public void updateViewPort()
    {
        //TODO реализовать отрисовку минимонитора. Возможно, в rendered texture?
    }
    public void enableViewPort(bool viewPort, bool hudDevice, int signChar = 0)
    {
        mCam2Active = viewPort;
        pViewPort.Hide(!hudDevice);
        HudViewPortData hvpd = (HudViewPortData)pViewPort.GetData();

        //hvpd.mMarkText[0] = signChar;
        hvpd.mMarkText = new string(new char[] { (char)signChar });
        hvpd.mEmpty = hudDevice && !viewPort;
    }
    public bool isViewPortEnabled() { return mCam2Active; }

    protected HudRecticleData pCross;
    protected HudRecticleData pMouse;
    protected HudRecticleData pWptPointer;
    protected HudRecticleData pWptPointerArrow;
    protected HudRecticleData pTargetBox;
    protected HudRecticleData pTargetBoxArrow;
    protected HudRecticleData pAimCue;
    protected HudRecticleData pSuggestedTargetBox;
    protected HudRecticleData pSuggestedTargetBoxArrow;
    protected HudRecticleData pMissileCue;
    protected HudRecticleData pAutopilotLamp;
    protected HudRecticleData pSituationLamp;
    protected HudRecticleData pObjectivesLamp;
    protected HudRecticleData pMenuLamp;
    protected int TargetsEnumerOptions;
    protected CameraRadarAndHudEnumer pTargetsEnumer;
    protected CameraData rData;
    protected float CrossSize;
    protected float CrossSizeMin;
    protected float CrossSizeMax;
    protected float CrossSizeSpd;
    protected float RadarRange;

    const float YMAX = 0.75f;
    //const float YMAX = 1.00f;
    const float XMAX = 1.00f;
    const float CENTER = 0.5f;
    const float OFFSET = 0.375f;

    private bool ProjectRecticle(ref HudRecticleData pData, Vector3 Org, float R, float Rscr = 1)
    {
        //Debug.Log("projecting pData: " + pData + " " + pData.GetHashCode().ToString("X8") + " " + (pData.x, pData.y) + " " + Org + "/" + rData.myCamera.Org);

        Vector3 Dif = Org - rData.myCamera.Org;

        float d = Vector3.Dot(rData.myCamera.Dir, Dif);
        if (Mathf.Abs(d) < .1f) //Если направление на ректикл слишком далеко от направления камеры
        {
            pData.x = (Vector3.Dot(Dif, rData.myCamera.Right) > 0 ? XMAX : 0);
            //pData.y = (Vector3.Dot(Dif, rData.myCamera.Right) > 0 ? 0.75f : 0);
            pData.y = (Vector3.Dot(Dif, rData.myCamera.Right) > 0 ? YMAX : 0);
            pData.setRadius((R == 0 ? HudDeviceMin : HudDeviceMax) * Rscr);
            return true;
        }
        d = rData.CameraAspect / d;
        pData.x = 0.5f + (Vector3.Dot(rData.myCamera.Right, Dif)) * Mathf.Abs(d);
        pData.y = 0.375f - (Vector3.Dot(rData.myCamera.Up, Dif)) * Mathf.Abs(d);
        //pData.y = 0.5f - (Vector3.Dot(rData.myCamera.Up, Dif)) * Mathf.Abs(d);
        pData.setRadius(Mathf.Clamp(R * Mathf.Abs(d) * .5f, HudDeviceMin, HudDeviceMax) * Rscr);
        if (d < 0) //если ректикл "сзади" камеры
        {
            pData.x = (pData.x > 0.5f ? XMAX : 0);
            return false;
        }
        // Закомментировано, т.к. эта ветка всё равно выполнится при пропуске предыдущего if
        //else
        //{
        return (pData.x > (0 - pData.w)) &&
                (pData.x < (XMAX + pData.w)) &&
                (pData.y > (0 - pData.h)) &&
                (pData.y < (YMAX + pData.h));
        //}
    }

    // углы поворота и тряска камеры
    private float AngleX, AngleXDelta;
    private float AngleY, AngleYDelta;
    private float HitShakeValue;
    public float GetAngleX() { return AngleX + AngleXDelta; }
    public float GetAngleY() { return AngleY + AngleYDelta; }
    public void Smooth(float cax, float cay, float spd)
    {
        cax = Mathf.Clamp(cax, MIN_CAMERA_PITCH, MAX_CAMERA_PITCH);
        cay = Mathf.Clamp(cay, -MAX_CAMERA_YAW, MAX_CAMERA_YAW);
        if (spd == 0) { AngleX = cax; AngleY = cay; return; }
        AngleX += Mathf.Clamp(cax - AngleX, -spd, spd);
        AngleY += Mathf.Clamp(cay - AngleY, -spd, spd);
    }
    public void GetPosAndR(ref float x, ref float y, ref float r, float xa, float ya, float ra)
    {
        //Matrix m = rData.myCamera; // так камеру колбасит.
        MATRIX m = new MATRIX(rData.myCamera);
        m.TurnRightPrec((AngleY + AngleYDelta - ya));
        m.TurnUpPrec((AngleX + AngleXDelta - xa));
        float d = Vector3.Dot(rData.myCamera.Dir, m.Dir);
        if (d < 0.1f)
        {
            x = -1f;
            y = -1f;
            r = 0;

            //В метод передаются указатели, поэтому сравнивать x/y/r с 0 (или null) нет смысла
            //if (x != 0) x = -1f;
            //if (y != 0) y = -1f;
            //if (r != 0) r = 0;
        }
        else
        {
            d = rData.CameraAspect / d;
            //if (x != 0) x = 0.5f - (Vector3.Dot(rData.myCamera.Right, m.Dir)) * d;
            //if (y != 0) y = 0.375f + (Vector3.Dot(rData.myCamera.Up, m.Dir)) * d;
            //if (r != 0) r = ra * d;
            x = 0.5f - (Vector3.Dot(rData.myCamera.Right, m.Dir)) * d;
            y = 0.375f + (Vector3.Dot(rData.myCamera.Up, m.Dir)) * d;
            //y = 0.5f + (Vector3.Dot(rData.myCamera.Up, m.Dir)) * d;
            r = ra * d;
        }
    }
    public void AddDamageShake(float d)
    {
        HitShakeValue += d * gsCameraShakeDamageC;
        if (HitShakeValue > gsCameraShakeMax) HitShakeValue = gsCameraShakeMax;
    }
    // own
    private SceneVisualizer rVisual;
    private BaseCraft pCraft;
    private IWave mpShakeSound;
    private IWave mpLowAltSound;
    private float mRadarRange;
    private float mHudOpacity;
    private BaseWeaponSlot myCurrentWS;

    public void SetRadarRange(float r)
    {
        mRadarRange = Mathf.Clamp(r, 500f, 20000f);
    }
    public float GetRadarRange() { return mRadarRange; }

    // создание/удаление
    public CameraDataCockpit(BaseScene s, IHUD h, CameraData d) : base(s, 0x68822B1C) // CameraDataCockpit?
    {
        rVisual = s.GetSceneVisualizer();
        pHud = h;
        rData = d;
        pCraft = null;
        mRadarRange = 5000;
        myCurrentWS = null;
        //TODO Реализовать создание данных кабинной камеры
        pCompass = pHud.CreateCompass(iCompas);
        //pViewPort   =pHud.CreateViewPort   (iViewPort);
        //pAltimeter = pHud.CreateVscale(iAltimeter);
        //pSpeedmeter = pHud.CreateVscale(iSpeedmeter);
        pThreat = pHud.CreateBar(iThreat);
        pWeapon = pHud.CreateWeapon(iWeapon);
        pHorizon = pHud.CreateAviaHorizon(iHorizon);
        pDamage = pHud.CreateDamage(iDamage);
        pEnergy = pHud.CreateBar(iEnergy);
        pEnergyScale = pHud.CreateBar(iEnergyScale);

        pRecticles = pHud.CreateRecticles(iRecticles);
        pWptPointer = ((HudRecticlesData)pRecticles.GetData()).AddItem(HudRecticleData.TYPE.CURRENTWPT, HudDeviceMin * .35f, HudDeviceMin * .35f, HUDCOLOR_NORMAL);
        pWptPointerArrow = ((HudRecticlesData)pRecticles.GetData()).AddItem(HudRecticleData.TYPE.ARROW, HudDeviceMin * .35f, HudDeviceMin * .35f, HUDCOLOR_NORMAL);
        pCross = ((HudRecticlesData)pRecticles.GetData()).AddItem(HudRecticleData.TYPE.GUNSIGHT, 0, 0, HUDCOLOR_GUNSIGN);
        //pMouse = ((HudRecticlesData*)pRecticles.GetData()).AddItem(RecticleData::ROMB, HudDeviceMin * .5, HudDeviceMin * .5);
        pTargetBox = ((HudRecticlesData)pRecticles.GetData()).AddItem(HudRecticleData.TYPE.CURRENTTARGET);
        pTargetBoxArrow = ((HudRecticlesData)pRecticles.GetData()).AddItem(HudRecticleData.TYPE.ARROW, HudDeviceMin * .5f, HudDeviceMin * .5f);
        pAimCue = ((HudRecticlesData)pRecticles.GetData()).AddItem(HudRecticleData.TYPE.RING);
        //pSuggestedTargetBox = ((HudRecticlesData*)pRecticles.GetData()).AddItem(RecticleData::ANIMETARGET);
        //pSuggestedTargetBoxArrow = ((HudRecticlesData*)pRecticles.GetData()).AddItem(RecticleData::ANIMEARROW, HudDeviceMin * .4, HudDeviceMin * .4);
        //pMissileCue = ((HudRecticlesData)pRecticles.GetData()).AddItem(HudRecticleData.TYPE.MISSILECONTROL);
        pAutopilotLamp = ((HudRecticlesData)pRecticles.GetData()).AddItem(HudRecticleData.TYPE.LAMP_A, gsLampRadius, gsLampRadius);
        pAutopilotLamp.x = gsAutopilotLampX;
        pAutopilotLamp.y = gsAutopilotLampY;
        pSituationLamp = ((HudRecticlesData)pRecticles.GetData()).AddItem(HudRecticleData.TYPE.LAMP_T, gsLampRadius, gsLampRadius);
        pSituationLamp.x = gsSituationLampX;
        pSituationLamp.y = gsSituationLampY;
        pObjectivesLamp = ((HudRecticlesData)pRecticles.GetData()).AddItem(HudRecticleData.TYPE.LAMP_O, gsLampRadius, gsLampRadius);
        pObjectivesLamp.x = gsObjectivesLampX;
        pObjectivesLamp.y = gsObjectivesLampY;
        pMenuLamp = ((HudRecticlesData)pRecticles.GetData()).AddItem(HudRecticleData.TYPE.LAMP_M, gsLampRadius, gsLampRadius);
        pMenuLamp.x = gsMenuLampX;
        pMenuLamp.y = gsMenuLampY;
        MapData md;
        md.Name = rScene.GetGameData().mpGameMapName;
        md.MapSizeX = rScene.GetGameData().mGameMapSizeX;
        md.MapSizeY = rScene.GetGameData().mGameMapSizeZ;
        md.SizeX = rScene.GetTerrain().GetXSize();
        md.SizeY = rScene.GetTerrain().GetZSize();
        pRadar = pHud.CreateMap(iRadar, md);
        TargetsEnumerOptions = (rScene.GetGameData().mShowFriendlyName ? 1 : 0) |
            ((rScene.GetGameData().mShowFriendlyType ? 1 : 0) << 1) |
            ((rScene.GetGameData().mShowFriendlyDist ? 1 : 0) << 2) |
            ((rScene.GetGameData().mShowEnemyName ? 1 : 0) << 4) |
            ((rScene.GetGameData().mShowEnemyType ? 1 : 0) << 5) |
            ((rScene.GetGameData().mShowEnemyDist ? 1 : 0) << 6);
        pTargetsEnumer = new CameraRadarAndHudEnumer(rScene, (HudRadarData)pRadar.GetData(), (HudRecticlesData)pRecticles.GetData(), rData);

        // other data
        InitCross();

        AngleX = 0; AngleXDelta = 0;
        AngleY = 0; AngleYDelta = 0;

        HitShakeValue = 0;
        mpShakeSound = rScene.GetRadioEnvironment().CreateWave(0, "CockpitShake");
        mpLowAltSound = rScene.GetRadioEnvironment().CreateWave(0, "LowAlt");

        // register
        rScene.GetCommandsApi().RegisterCommand("cl_icons_friendly", this, 0, "switch friendly icons mode");
        rScene.GetCommandsApi().RegisterCommand("cl_icons_enemy", this, 0, "switch enemy icons mode");
        rScene.GetCommandsApi().RegisterCommand("cm_radar_range", this, 1, "set radar range");
        rScene.GetCommandsApi().RegisterCommand("cl_switch_vs", this, 1, "set HUD viewport mode");

        // set default HUD viewport mode
        mHUDViewportMode = (int)VIEWPORT_MODE.VIEWPORT_OFF;

        // register toma commands
        rScene.GetCommandsApi().RegisterVariable("hud_tomat_x", this, VAR_FLOAT, "set toma(t) x coord");
        rScene.GetCommandsApi().RegisterVariable("hud_tomat_y", this, VAR_FLOAT, "set toma(t) y coord");
        rScene.GetCommandsApi().RegisterVariable("hud_tomat_w", this, VAR_FLOAT, "set toma(t) width");
        rScene.GetCommandsApi().RegisterVariable("hud_tomat_h", this, VAR_FLOAT, "set toma(t) height");
        rScene.GetCommandsApi().RegisterVariable("hud_tomao_x", this, VAR_FLOAT, "set toma(o) x coord");
        rScene.GetCommandsApi().RegisterVariable("hud_tomao_y", this, VAR_FLOAT, "set toma(o) y coord");
        rScene.GetCommandsApi().RegisterVariable("hud_tomao_w", this, VAR_FLOAT, "set toma(o) width");
        rScene.GetCommandsApi().RegisterVariable("hud_tomao_h", this, VAR_FLOAT, "set toma(o) height");
        rScene.GetCommandsApi().RegisterVariable("hud_tomam_x", this, VAR_FLOAT, "set toma(m) x coord");
        rScene.GetCommandsApi().RegisterVariable("hud_tomam_y", this, VAR_FLOAT, "set toma(m) y coord");
        rScene.GetCommandsApi().RegisterVariable("hud_tomam_w", this, VAR_FLOAT, "set toma(m) width");
        rScene.GetCommandsApi().RegisterVariable("hud_tomam_h", this, VAR_FLOAT, "set toma(m) height");
        rScene.GetCommandsApi().RegisterVariable("hud_tomaa_x", this, VAR_FLOAT, "set toma(a) x coord");
        rScene.GetCommandsApi().RegisterVariable("hud_tomaa_y", this, VAR_FLOAT, "set toma(a) y coord");
        rScene.GetCommandsApi().RegisterVariable("hud_tomaa_w", this, VAR_FLOAT, "set toma(a) width");
        rScene.GetCommandsApi().RegisterVariable("hud_tomaa_h", this, VAR_FLOAT, "set toma(a) height");
        rScene.GetCommandsApi().RegisterCommand("hud_toma", this, 1, "show/hide toma");
        myGlobalShowToma = true;

        // init
        SetCraft(null, true);
    }

    private void InitCross()
    {
        CrossSizeMin = .035f;
        CrossSizeMax = .05f;
        CrossSizeSpd = .1f;

        rScene.GetCommandsApi().RegisterVariable("hud_cross_min", this, VAR_FLOAT, "set min cross size");
        rScene.GetCommandsApi().RegisterVariable("hud_cross_max", this, VAR_FLOAT, "set max cross size");
        rScene.GetCommandsApi().RegisterVariable("hud_cross_speed", this, VAR_FLOAT, "set speed for cross size change");
        CrossSize = CrossSizeMax;
    }
    ~CameraDataCockpit()
    {
        Dispose();
    }

    public void Dispose()
    {
        rScene.GetCommandsApi().UnRegister(this);
        if (mpLowAltSound != null) mpLowAltSound.Release();
        if (mpShakeSound != null) mpShakeSound.Release();
        pTargetsEnumer.Dispose();
        pHud.ReleaseDevice(pViewPort);
        pHud.ReleaseDevice(pCompass);
        pHud.ReleaseDevice(pAltimeter);
        pHud.ReleaseDevice(pSpeedmeter);
        pHud.ReleaseDevice(pThreat);
        pHud.ReleaseDevice(pWeapon);
        pHud.ReleaseDevice(pRadar);
        pHud.ReleaseDevice(pHorizon);
        pHud.ReleaseDevice(pDamage);
        pHud.ReleaseDevice(pEnergy);
        pHud.ReleaseDevice(pEnergyScale);
        pHud.ReleaseDevice(pRecticles);
    }
    public void SetCraft(BaseCraft c, bool Force = false)
    {
        bool return_by_mikha = pCraft == c && Force == false;

        pCraft = c;
        updateViewPort();
        if (return_by_mikha) return;


        HitShakeValue = 0;
        if (pCraft == null)
        {

            if (mpShakeSound != null) mpShakeSound.Stop();
            if (mpLowAltSound != null) mpLowAltSound.Stop();

            pCompass.Hide(true);
            //pAltimeter.Hide(true);
            //pSpeedmeter.Hide(true);
            pRadar.Hide(true);
            pThreat.Hide(true);
            pWeapon.Hide(true);
            pHorizon.Hide(true);
            pDamage.Hide(true);
            pEnergy.Hide(true);
            pEnergyScale.Hide(true);
            pRecticles.Hide(true);

        }
        else
        {

            UpdateHudWeapon();
            UpdateHudDamage();
            // размер прицела - max
            CrossSize = CrossSizeMax;
            mHudOpacity = (pCraft.IsInSF() == true ? 0 : 1);
        }
    }

    /// <summary>
    /// апдейт оружия
    /// в оригинале - часть метода SetCraft()
    /// </summary>
    private void UpdateHudWeapon()
    {
        HudWeaponData d = (HudWeaponData)pWeapon.GetData();
        BaseWeaponSlot s;
        // чистим старый список и составляем новый
        d.Items.Free();
        if (pCraft.GetWeaponSystem() != null)
        {
            for (s = pCraft.GetWeaponSystem().GetFirstWS(); s != null; s = s.GetNext())
                d.Items.AddToTail(new WeaponData(0, s.GetData().DescriptionShort != null ? s.GetData().DescriptionShort : s.GetData().FullName, 0, false, 0));
        }
    }

    /// <summary>
    /// апдейт повреждометра
    /// </summary>
    private void UpdateHudDamage()
    {
        HudDamageData d = (HudDamageData)pDamage.GetData();
        d.SetBitmapName(pCraft.Dt().DamageBitmap);
        d.ClearList();
        BaseSubobj s;
        for (s = pCraft.GetRoot(); s != null; s = s.Next())
        {
            if (s.GetData().GetClass() != SUBOBJ_DATA.SC_CRAFT_PART) continue;
            CRAFT_PART_DATA cpd = (CRAFT_PART_DATA)s.GetData();
            d.CreateNewPartdata(cpd.DamageXY, cpd.DamageUV, cpd.FullName);
        }
    }
    public void Update(float scale, bool LookBehind)
    {
        //TODO реализовать апдейт данных камеры
        Asserts.AssertBp(pCraft != null);


        pCraft.updateAngles();

        //Обновление разных HudDeviceов для удобства вынесены в отдельные методы
        //TODO Требуется реализовать пропущенные

        // готовим глобальные переменные
        iMovementSystemCraft m = (iMovementSystemCraft)pCraft.GetInterface(iMovementSystemCraft.ID);
        IBaseCraftController cc = (IBaseCraftController)rVisual.GetPlayerInterface().GetInterface(IBaseCraftController.ID);
        WeaponSystemForBaseCraft w = pCraft.GetWeaponSystem();
        iContact pTarget = (w != null ? w.GetTarget() : null);
        iContact pSuggestedTarget = (cc != null ? cc.GetSuggestedTarget() : null);
        BaseWeaponSlot ws = (w != null ? w.GetCurrentWS(w.GetWeapon()) : null);
        if (ws != null && ws.GetWpnData().Type != WT_MISSILE) myCurrentWS = ws = null;
        MATRIX pos = new MATRIX(pCraft.GetPosition());
        float delta_y = m.GetDeltaY();
        // тряска

        // звук от низкой высоты
        UpdateLowAltitudeSound(delta_y);

        // прозрачность HUD-а
        // viewport
        // готовим воспомогательные переменные

        Vector3 Tgt = Vector3.zero;
        Vector3 TgtSpd = Vector3.zero;
        int ptr_color = HUDCOLOR_NORMAL;
        float Dist = 0;
        // указатель WPT

        UpdateWaypointDisplay(cc, ref Tgt, ref TgtSpd, ref Dist);

        //Установка цвета
        if (pTarget != null)
        {
            if (pTarget.GetSideCode() != pCraft.GetSideCode())
                ptr_color = pTarget.GetSideCode() == 0 ? HUDCOLOR_NEUTRAL : HUDCOLOR_VELIAN;
            Tgt = pTarget.GetOrg();
            TgtSpd = pTarget.GetSpeed();
        }
        // компас
        UpdateCompassDisplay(LookBehind, pos, Tgt);
        // высотомер
        // спидометр
        // авиагоризонта (sic!)
        UpdateAviaHorDisplay(LookBehind);
        // радар
        UpdateRadarDisplay(LookBehind, pos);
        // отметки от целей на радаре и HUD
        UpdateRadarAndHUDTargetsDisplay(cc, pTarget, pSuggestedTarget);
        // захват ракеты
        // рекомендуемая цель
        // захват цели и упреждение
        UpdateTargettingDisplay(scale, pTarget, ws, w);
        // индикатор угрозы
        UpdateThreatDisplay(LookBehind);
        // оружие
        UpdateWeaponDisplay(LookBehind);
        // повреждометр
        UpdateDamageDisplay(LookBehind);
        // индикатор энергии
        UpdateEnergyDisplay(LookBehind);
        // лампы
        UpdateTOMADisplay(LookBehind, cc);
        // прицел
        UpdateGunSight(LookBehind);
        // мышиный указатель
    }

    private void UpdateTOMADisplay(bool LookBehind, IBaseCraftController cc)
    {
        pAutopilotLamp.Hide(true); pAutopilotLamp.opacity = mHudOpacity;
        pSituationLamp.Hide(true); pSituationLamp.opacity = mHudOpacity;
        pObjectivesLamp.Hide(true); pObjectivesLamp.opacity = mHudOpacity;
        pMenuLamp.Hide(true); pMenuLamp.opacity = mHudOpacity;
        if (cc != null && LookBehind == false && myGlobalShowToma)
        {
            pAutopilotLamp.Hide(false);
            pSituationLamp.Hide(false);
            pObjectivesLamp.Hide(false);
            pMenuLamp.Hide(false);
            DWORD[] State2Colors = { HUDCOLOR_DISABLED, HUDCOLOR_NORMAL, HUDCOLOR_DANGER };
            pAutopilotLamp.colour = State2Colors[cc.GetAutopilotState()];
            pSituationLamp.colour = State2Colors[cc.GetSituationState()];
            pObjectivesLamp.colour = State2Colors[rVisual.GetPlayerInterface().GetObjectivesState()];
            pMenuLamp.colour = State2Colors[rVisual.GetPlayerInterface().GetMenuState()];
        }

    }
    private void UpdateAviaHorDisplay(bool LookBehind)
    {
        pHorizon.Hide(LookBehind);
        if (LookBehind == false)
        {
            HudHorizonData d = (HudHorizonData)pHorizon.GetData();
            d.opacity = mHudOpacity;
            //float t = Storm.Math.RD2GRD(Storm.Math.fov(rData.CameraAspect));
            //if (d.fov!=t) { d.fov=t; d.changed=true; }
            d.roll = Storm.Math.RD2GRD(pCraft.GetRollAngle());
            d.pitch = Storm.Math.RD2GRD(pCraft.GetPitchAngle());
            float discard = 0;
            GetPosAndR(ref d.x, ref d.y, ref discard, 0, 0, 0);
        }
    }
    private void UpdateRadarDisplay(bool LookBehind, MATRIX pos)
    {
        pRadar.Hide(LookBehind);
        if (LookBehind == false)
        {
            HudRadarData d = (HudRadarData)pRadar.GetData();
            d.opacity = mHudOpacity;
            d.changed = true;
            d.org = pos.Org;
            d.angle = pCraft.GetHeadingAngle();
            d.range = mRadarRange;
        }

    }
    private void UpdateThreatDisplay(bool LookBehind)
    {
        pThreat.Hide(true);
        if (LookBehind == false && rScene.GetGameData().mShowThreat == true)
        {
            pThreat.Hide(false);
            HudBarData d = (HudBarData)pThreat.GetData();
            d.opacity = mHudOpacity;
            d.value = pCraft.GetThreatF() * .5f;
            d.changed = true;
            iContact c = rScene.GetContact(pCraft.GetThreatHandle());
            string txt = (c != null ? c.GetTypeName() : null);
            if (txt == null) d.text = "NO THREAT";
            else d.text = string.Format("THREAT: {0}", txt);
        }
    }

    private void UpdateEnergyDisplay(bool LookBehind)
    {
        pEnergy.Hide(LookBehind);
        pEnergyScale.Hide(LookBehind);
        if (LookBehind == false)
        {
            HudBarData d = (HudBarData)pEnergy.GetData();
            d.opacity = mHudOpacity;
            d.value = pCraft.GetBattaryCharge() / pCraft.Dt().BatteryCharge;
            d.changed = true;
            HudBarData p = (HudBarData)pEnergyScale.GetData();
            p.opacity = mHudOpacity;
        }
    }
    private void UpdateRadarAndHUDTargetsDisplay(IBaseCraftController cc, iContact pTarget, iContact pSuggestedTarget)
    {
        pRecticles.Hide(false);
        pTargetsEnumer.mShowFriendlyName = (TargetsEnumerOptions & 0x0001) != 0;
        pTargetsEnumer.mShowFriendlyType = (TargetsEnumerOptions & 0x0002) != 0;
        pTargetsEnumer.mShowFriendlyDist = (TargetsEnumerOptions & 0x0004) != 0;
        pTargetsEnumer.mShowEnemyName = ((TargetsEnumerOptions & 0x0010) != 0) && (rScene.GetGameData().mAllowEnemyName == true);
        pTargetsEnumer.mShowEnemyType = ((TargetsEnumerOptions & 0x0020) != 0) && (rScene.GetGameData().mAllowEnemyType == true);
        pTargetsEnumer.mShowEnemyDist = ((TargetsEnumerOptions & 0x0040) != 0) && (rScene.GetGameData().mAllowEnemyDist == true);
        pTargetsEnumer.Enumerate((iContact)pCraft.GetInterface(iContact.ID), pTarget, pSuggestedTarget, (iSensors)pCraft.GetInterface(iSensors.ID));
        if (cc != null)
        {
            Vector3 org = Vector3.zero;
            if (WaypointSuggester.getWaypoint(cc, ref org) != 0)
                pTargetsEnumer.addWaypoint(org);
        }
    }
    private void UpdateWaypointDisplay(IBaseCraftController cc, ref Vector3 Tgt, ref Vector3 TgtSpd, ref float Dist)
    {
        pWptPointer.hide = true;
        pWptPointerArrow.hide = true;
        if (cc != null)
        {
            bool show = false;
            switch (WaypointSuggester.getWaypoint(cc, ref Tgt))
            {
                case 1:
                    {
                        Dist = (Tgt - pCraft.GetOrg()).magnitude;
                        TgtSpd = cc.GetAutopiloLeader().GetSpeed();
                        pWptPointer.textup = null;
                        pWptPointer.text = null;
                        show = true;
                        break;
                    }
                case 2:
                    {
                        Dist = (Tgt - pCraft.GetOrg()).magnitude;
                        pWptPointer.textup = "WPT";
                        if (Dist > 1000f)
                            //TODO разобраться с форматированием
                            //Sprintf(pWptPointer.text, "%.1fkm", Dist * .001f);
                            pWptPointer.text = string.Format("{0:F2}km", Dist * .001);
                        else
                            //Sprintf(pWptPointer.text, "%dm", int(Dist));
                            pWptPointer.text = string.Format("{0:D}m", (int)Dist);
                        show = true;
                        break;
                    }
            }
            if (show == true)
            {
                if (ProjectRecticle(ref pWptPointer, Tgt, 0, .35f) == true)
                {
                    pWptPointer.hide = false;
                }
                else
                {
                    pWptPointerArrow.hide = false;
                    pWptPointerArrow.x = pWptPointer.x;
                    pWptPointerArrow.y = pWptPointer.y;
                }
            }
        }
    }

    private void UpdateCompassDisplay(bool LookBehind, MATRIX pos, Vector3 Tgt, int ptr_color = HUDCOLOR_NORMAL)
    {
        pCompass.Hide(LookBehind);
        if (LookBehind == false)
        {
            HudCompasData d = (HudCompasData)pCompass.GetData();
            d.opacity = mHudOpacity;
            d.angle = Storm.Math.RD2GRD(pCraft.GetHeadingAngle());
            if (d.angle < 0) d.angle += 360f;
            float t = Storm.Math.RD2GRD(Storm.Math.fov(rData.CameraAspect));
            if (d.angle_view != t) { d.angle_view = t; d.changed = true; }
            // угол на цель
            if (Storm.Math.NormaFAbs(Tgt) > 0)
            {
                d.colour_pointer = ptr_color;
                d.pointer = Storm.Math.RD2GRD(Mathf.Atan2(Tgt.x - pos.Org.x, Tgt.z - pos.Org.z));
                if (d.pointer < 0) d.pointer += 360f;
                d.state |= (int)HUDSTATE_POINTER;
                Assert.IsTrue(d.pointer <= 360f);
            }
            else
            {
                d.pointer = 0;
                d.state &= ~(int)HUDSTATE_POINTER;
            }
        }

    }
    private void UpdateWeaponDisplay(bool LookBehind)
    {
        pWeapon.Hide(LookBehind);
        if (LookBehind) return;

        WeaponSystemForBaseCraft w = pCraft.GetWeaponSystem();
        HudWeaponData d = (HudWeaponData)pWeapon.GetData();
        d.opacity = mHudOpacity;
        BaseWeaponSlot s;
        WeaponData wd;
        // обновляем список
        for (s = w.GetFirstWS(), wd = d.Items.Head(); (s != null && wd != null); s = s.GetNext(), wd = wd.Next())
        {
            int load;
            float progr;
            s.GetFullStatus(out wd.colour, out load, out progr);
            if (load < 0)
            {
                wd.value = progr;
                wd.tape = true ? 1 : 0; //Согласен, странное. Но так больше соответствует исходному коду. Возможно, при рефакторинге можно будет просто назначать 1/0
            }
            else
            {
                wd.value = load;
                wd.tape = false ? 1 : 0;
            }
            if (s.GetGroup() == w.GetWeapon())
            {
                wd.box = true;
                wd.current = (s.GetWpnData().IsChained == false || s == w.GetCurrentWS(w.GetWeapon()));
            }
            else
            {
                wd.box = false;
                wd.current = false;
            }
        }

    }
    private void UpdateDamageDisplay(bool LookBehind)
    {
        // повреждометр
        pDamage.Hide(LookBehind);
        if (LookBehind == false)
        {
            HudDamageData d = (HudDamageData)pDamage.GetData();
            d.opacity = mHudOpacity;
            // обновляем список
            BaseSubobj s;
            DamageData curr_part = d.List.Head();
            Assert.IsNotNull(curr_part);
            for (s = pCraft.GetRoot(); s != null; s = s.Next())
            {
                if (s.GetData().GetClass() != SUBOBJ_DATA.SC_CRAFT_PART) continue;
                int idx = s.GetState();
                //  [sNormal..sScratched)=GREEN, [sScratched..sDamaged)=YELLOW, [sDamaged, sKilled)=RED  [sKilled..)=GRAY
                if (idx < State2Life.sScratched) curr_part.colour = HUDCOLOR_NORMAL;
                else if (idx < State2Life.sDamaged) curr_part.colour = HUDCOLOR_MEDIUM;
                else if (idx < State2Life.sKilled) curr_part.colour = HUDCOLOR_DANGER;
                else curr_part.colour = HUDCOLOR_DISABLED;
                // перевоы
                curr_part = curr_part.Next();
            }
        }
    }
    private void UpdateLowAltitudeSound(float delta_y)
    {
        if (mpLowAltSound != null)
        {
            if (delta_y < gsMinAlt)
            {
                int vol = (int)Mathf.Clamp(rVisual.GetSceneConfig().s_vhudspeach * ((gsMinAlt - delta_y) / gsMinAlt), 0, rVisual.GetSceneConfig().s_veffects);
                mpLowAltSound.Play(true, vol);
            }
            else
            {
                mpLowAltSound.Stop();
            }
        }
    }

    private void UpdateGunSight(bool LookBehind)
    {
        pCross.hide = true;
        if (LookBehind == false)
        {
            pCross.hide = false;
            float distcardR = 0;
            GetPosAndR(ref pCross.x, ref pCross.y, ref distcardR, 0, 0, 0);
            pCross.setRadius(CrossSize);
        }

    }
    private void UpdateTargettingDisplay(float scale, iContact pTarget, BaseWeaponSlot ws, WeaponSystemForBaseCraft w)
    {
        // захват цели и упреждение
        {
            float TgtCrossSize = CrossSizeMax;
            pTargetBox.hide = true;
            pTargetBoxArrow.hide = true;
            pAimCue.hide = true;
            if (pTarget != null)
            {
                pTargetsEnumer.mShowEnemyDist = true;
                pTargetsEnumer.mShowEnemyType = true;
                pTargetsEnumer.mShowFriendlyDist = true;
                pTargetsEnumer.mShowFriendlyType = true;
                pTargetsEnumer.PrintContactInfo(ref pTargetBox, pTarget, (pTarget.GetSideCode() == pCraft.GetSideCode()));
                if (ProjectRecticle(ref pTargetBox, pTarget.GetOrg(), pTarget.GetRadius()) == true)
                {
                    pTargetBox.hide = false;
                    if (pTargetBox.textup == null) pTargetBox.textup = "";
                    int l = pTargetBox.textup.Length;
                    //if (l > 0) { pTargetBox.textup[l] = ' '; l++; }
                    if (l > 0) { pTargetBox.textup += ' '; l++; }
                    pTargetBox.textup += string.Format("{0}%", (int)(pTarget.GetCondition() * 100f + .49f));
                }
                else
                {
                    pTargetBoxArrow.colour = pTargetBox.colour;
                    pTargetBoxArrow.hide = false;
                    pTargetBoxArrow.x = pTargetBox.x;
                    pTargetBoxArrow.y = pTargetBox.y;
                }
                // апдейт автонаведения
                //Debug.LogFormat("rScene.GetGameData().mShowLead {0} ws {1}" ,rScene.GetGameData().mShowLead,ws==null? "null":ws);
                if (ws == null && rScene.GetGameData().mShowLead == true && ProjectRecticle(ref pAimCue, w.GetTargetOrg(), pTarget.GetRadius(), .5f) == true)
                {
                    pAimCue.hide = false;
                    pAimCue.opacity = mHudOpacity;
                    pAimCue.colour = (w.GetTargetDist() <= w.GetRange() ? pTargetBox.colour : HUDCOLOR_DISABLED);
                    if (w.GetAim() > CrossSizeLim) TgtCrossSize = CrossSizeMin;
                }
            }
            //размер крестика прицела
            CrossSize += Mathf.Clamp(TgtCrossSize - CrossSize, -CrossSizeSpd * scale, CrossSizeSpd * scale);
        }

    }


    // от CommLink
    public virtual void OnCommand(uint code, string arg1, string arg2)
    {
        switch (code)
        {
            case 0x4CB283C4:  // cl_icons_friendly
                TargetsEnumerOptions += 0x0001;
                TargetsEnumerOptions &= 0xFFF7;
                return;
            case 0x4E49BC00:  // cl_icons_enemy
                TargetsEnumerOptions += 0x0010;
                TargetsEnumerOptions &= 0xFF7F;
                return;
            case 0x43958EB0:
                {// hud_toma
                    crc32 crc = Hasher.HshString(arg1);
                    switch (crc)
                    {
                        case 0xF649D637: // on
                            myGlobalShowToma = true;
                            break;
                        case 0xD443A2BC: // off
                            myGlobalShowToma = false;
                            break;
                    }
                    return;
                }
            case 0xDF983A20: // cl_switch_vs
                {
                    crc32 crc = Hasher.HshString(arg1);
                    switch (crc)
                    {
                        case 0x6C16C390: // TargetView
                            mHUDViewportMode = (int)VIEWPORT_MODE.VIEWPORT_TARGET;
                            break;
                        case 0x00E6B9A4: // MissileView
                            mHUDViewportMode = (int)VIEWPORT_MODE.VIEWPORT_MISSILE;
                            break;
                        case 0x6B610393: // RearView
                            mHUDViewportMode = (int)VIEWPORT_MODE.VIEWPORT_REAR;
                            break;
                        case 0xEC0EE45C: // Off
                        default:
                            mHUDViewportMode = (int)VIEWPORT_MODE.VIEWPORT_OFF;
                            break;
                    }
                    break;
                }
            case 0x8FE77112:  // cm_radar_range
                if (arg1[0] == '-' || arg1[0] == '+') SetRadarRange(RadarRange + float.Parse(arg1));
                else SetRadarRange(float.Parse(arg1));
# if _DEBUG
                rScene.Message("RadarRange=%f", RadarRange);
#endif
                return;
        }
    }
    public virtual object OnVariable(uint v_id, object data)
    {
        //float data = (float)datag;
        switch (v_id)
        {
            case 0xD03E5444:  // hud_tomat_x
                if (data != null) setValidValue(out pSituationLamp.x, (float)data, 1f, pSituationLamp.w);
                return pSituationLamp.x;
            case 0xA73964D2:  // hud_tomat_y
                if (data != null) setValidValue(out pSituationLamp.y, (float)data, 1f, pSituationLamp.h);
                return pSituationLamp.y;
            case 0x408149D5:  // hud_tomat_w
                if (data != null) setValidValueD(out pSituationLamp.w, pSituationLamp.x, (float)data, .5f);
                return pSituationLamp.w;
            case 0xCD894420:  // hud_tomat_h
                if (data != null) setValidValueD(out pSituationLamp.h, pSituationLamp.y, (float)data, .375f);
                return pSituationLamp.h;
            case 0xC04D18D5:  // hud_tomao_x
                if (data != null) setValidValue(out pObjectivesLamp.x, (float)data, 1f, pObjectivesLamp.w);
                return pObjectivesLamp.x;
            case 0xB74A2843:  // hud_tomao_y
                if (data != null) setValidValue(out pObjectivesLamp.y, (float)data, 1f, pObjectivesLamp.h);
                return pObjectivesLamp.y;
            case 0x50F20544:  // hud_tomao_w
                if (data != null) setValidValueD(out pObjectivesLamp.w, pObjectivesLamp.x, (float)data, .5f);
                return pObjectivesLamp.w;
            case 0xDDFA08B1:  // hud_tomao_h
                if (data != null) setValidValueD(out pObjectivesLamp.h, pObjectivesLamp.y, (float)data, .375f);
                return pObjectivesLamp.h;
            case 0xC3C9CCBB:  // hud_tomam_x
                if (data != null) setValidValue(out pMenuLamp.x, (float)data, 1f, pMenuLamp.w);
                return pMenuLamp.x;
            case 0xB4CEFC2D:  // hud_tomam_y
                if (data != null) setValidValue(out pMenuLamp.y, (float)data, .75f, pMenuLamp.h);
                return pMenuLamp.y;
            case 0x5376D12A:  // hud_tomam_w
                if (data != null) setValidValueD(out pMenuLamp.w, pMenuLamp.x, (float)data, .5f);
                return pMenuLamp.w;
            case 0xDE7EDCDF:  // hud_tomam_h
                if (data != null) setValidValueD(out pMenuLamp.h, pMenuLamp.y, (float)data, .375f);
                return pMenuLamp.h;
            case 0xCAD335DF: // hud_tomaa_x
                if (data != null) setValidValue(out pAutopilotLamp.x, (float)data, 1f, pAutopilotLamp.w);
                return pAutopilotLamp.x;
            case 0xBDD40549:  // hud_tomaa_y
                if (data != null) setValidValue(out pAutopilotLamp.y, (float)data, .75f, pAutopilotLamp.h);
                return pAutopilotLamp.y;
            case 0x5A6C284E: // hud_tomaa_w
                if (data != null) setValidValueD(out pAutopilotLamp.w, pAutopilotLamp.x, (float)data, .5f);
                return pAutopilotLamp.w;
            case 0xD76425BB: // hud_tomaa_h
                if (data != null) setValidValueD(out pAutopilotLamp.h, pAutopilotLamp.y, (float)data, .375f);
                return pAutopilotLamp.h;

            case 0xB8755713: // hud_cross_min
                if (data != null) CrossSizeMin = (float)data;
                return CrossSizeMin;
            case 0x8478684A: // hud_cross_max
                if (data != null) CrossSizeMax = (float)data;
                return CrossSizeMax;
            case 0x3821D904: // hud_cross_speed
                if (data != null) CrossSizeSpd = (float)data;
                return CrossSizeSpd;

        }
        return null;
    }

    private void setValidValue(out float coord, float value, float ub, float adder, float mult = 1)
    {
        /*float var=mult*value;
        bool b=(0 <= var && var <= ub);
        if (0 <= var && var <= ub)
            coord=var+adder;
        return b;*/
        coord = value;
    }

    private void setValidValueD(out float dim, float modificator, float value, float ub)
    {
        /*float prev=dim;
        if (setValidValue(dim,value,ub,0,.5f))
            modificator+=dim-prev;*/
        dim = value;
    }
}



