using geombase;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static IParamList;
using static UnityEditor.PlayerSettings;
using crc32 = System.UInt32;
using DWORD = System.UInt32;
//using EffectFlare = EffectLF<ILenzFlare2, VisDescFlare, EffectFlareData>;
//using EffectLight = EffectLF<ILight, VisDescLight, EffectLightData>;
//using NonHashList = System.Collections.Generic.List<HashObjectCont>;
using NonHashList = TLIST<HashObjectCont>;

public partial class SceneVisualizer: IKeepParticle
{
    ParticleHolder myHolder; //TODO реализовать разные партиклы

    public IKeepParticle getParticleKeeper() { return this; }

    public void keepParticle(PARTICLE_SYSTEM p)
    {
        p.Die();
        myHolder.addParticle(p);
    }
}
public partial class SceneVisualizer //Animation
{
    internal void AddCameraDamageShake(uint v, float delta)
    {
        //TODO реализовать тряску камеры от повреждений
        return;
    }

    internal IVisualDebris CreateVisualDebris(DEBRIS_DATA _data, BaseDebris _body, bool in_hash)
    {
        if (m_VisualizerConfig.v_debrises != 0)
            return new VisualDebris(this, _data, _body, in_hash);
        return null;
    }


}
public partial class SceneVisualizer //constants
{
    public const string sCmMode = "cm_mode";
    public const uint iCmMode = 0x606AAAD0;
    public const string sCmConfigure = "configure_scene";
    public const uint iCmConfigure = 0xE5B2913F;
    public const string sCmShoot = "cm_shoot";
    public const uint iCmShoot = 0x65661B6A;
    public const string sCmRoute = "cm_route";
    public const uint iCmRoute = 0x17E6C7AD;
    public const string sVRoadInfo = "v_roadinfo";
    public const uint iVRoadInfo = 0x9C95A529;
    public const string sVHashInfo = "v_hashinfo";
    public const uint iVHashInfo = 0xAA3FBD06;

}
public partial class SceneVisualizer
{
    protected I3DSound sound;

    public StormPath CreatePath(Vector3 start, Vector3 end, FVec4 c, float _lifetime)
    {
        return new StormPath(this, start, end, c, _lifetime);
    }

    public VisualizerConfig GetSceneConfig()
    {
        return m_VisualizerConfig;
    }

    float myDarkness;
    public float getDarkness() { return myDarkness; }



    //Scene config
    const string sVisualConfigsName = "VisualConfigs.dat";
    iUnifiedVariableDB GetSceneConfigDB()
    {
        return UniVarUtils.CreateUnifiedVariableDB(Constants.UNIVARS_VERSION, ProductDefs.GetPI().getHddFile(sVisualConfigsName), true);
    }
    void LoadSceneConfig(string name)
    {
        rScene.Message("Loading visual config \"{0}\"", name);
        iUnifiedVariableDB vis_db;
        iUnifiedVariableContainer root;
        if ((vis_db = GetSceneConfigDB()) != null)
            if ((root = vis_db.GetRootTpl<iUnifiedVariableContainer>()) != null)
                m_SceneConfigCtr = root.openContainer(name);

        if (m_SceneConfigCtr == null)
            throw new Exception(string.Format("Can't open VisConfig. \"{0}\"", name));


        m_SceneConfigCtr.getFloat("Darkness", ref myDarkness, 1);
    }
    iUnifiedVariableContainer m_SceneConfigCtr;

    //Visualizer preferences
    iUnifiedVariableContainer m_FlaresCtr;

    // возня с envcfg
    protected crc32 mSceneNormalCofig;
    protected crc32 mSceneNvConfig;
    protected crc32 mSceneSfgConfig;

    protected crc32 mCurrentConfig;
    protected bool mCurrentInfra;
    protected string myFpoTexturesPath;
    public crc32 GetSceneNormalConfig() { return mSceneNormalCofig; }
    public crc32 GetSceneNvConfig() { return mSceneNvConfig; }
    public crc32 GetSceneSfgConfig() { return mSceneSfgConfig; }
    public string getTexturesDbName() { return myFpoTexturesPath; }

    iRadioEnvironment mpRadioEnvironment;
    public iRadioEnvironment GetRadioEnvironment() { return mpRadioEnvironment; }

    PlayerInterface pPlayer;
    public PlayerInterface GetPlayerInterface() { return pPlayer; }
    public PlayerInterface SetPlayerInterface(PlayerInterface pNew = null)
    {
        if (pNew == null)
        {
            if (rScene.IsClient()) pNew = new PlayerInterfaceRemote(rScene, null);
            else pNew = new PlayerInterfaceLocal(rScene, null);
        }
        pNew.Init(pPlayer);
        if (pPlayer != null) pPlayer.Dispose();
        pPlayer = pNew;
        mpRadioEnvironment = pPlayer;
        return pPlayer;
    }

}

public partial class SceneVisualizer //inline
{
    #region non hash
    //    inline int SceneVisualizer::GetDetailStage(const VECTOR& Org) { 
    //  CameraData& cd=GetCameraData();
    //    float a = (fabs(Org.x - cd.Camera.Org.x) + fabs(Org.z - cd.Camera.Org.z)) * .5f;
    //  if (a<500.f ) return DETAIL_STAGE_FULL;
    //  if (a<3000.f) return DETAIL_STAGE_HALF;
    //  return (a<cd.GetCameraRange()? DETAIL_STAGE_QUARTER:DETAIL_STAGE_NONE);
    //}

    internal Dust CreateDust(DUST_DATA dust, FPO r, SLOT_DATA sld)
    {
        //TODO Реализовать прикремление пыли
        return null;
        //throw new NotImplementedException();
    }
    //inline Dust *SceneVisualizer::CreateDust (const DUST_DATA *d, RO *_attach_to,const SLOT_DATA *sl_d) {
    //    if (m_VisualizerConfig.v_dust)
    //        return new Dust(this, d, _attach_to, sl_d);
    //    return 0;
    //}

    //inline Path *SceneVisualizer::CreatePath (const VECTOR& start,const VECTOR& end, FVec4 c,float _lifetime) {
    //    return new Path(this, start, end, c, _lifetime);
    //}

    //inline IVisualDebris *SceneVisualizer::CreateVisualDebris (const DEBRIS_DATA *_data, class BaseDebris *_body, bool in_hash) {
    //    if (m_VisualizerConfig.v_debrises)
    //        return new VisualDebris(this, _data, _body, in_hash);
    //    return 0;
    //}
    #endregion non hash
    #region hash
    public HMember CreateHM(IHashObject r)
    {
        return rScene.CreateHM(r);
    }

    public void DeleteHM(HMember h)
    {
        rScene.RemoveHM(h);
    }

    public HMember UpdateHM(HMember h)
    {
        return rScene.UpdateHM(h);
    }

    public HMember RemoveHM(HMember h)
    {
        return rScene.RemoveHM(h);
    }

    public HMember ConstructHM(IHashObject r)
    {
        return rScene.ConstructHM(r);
    }
    public FPO CreateFPO(string Name)
    {
        return rScene.CreateFPO(Name);
    }

    public FPO CreateFPO(uint Name)
    {
        return rScene.CreateFPO(Name);
    }

    public Fpo CreateFPO2(ObjId id)
    {
        return rScene.CreateFPO2(id);
    }

    public float GetViewDist()
    {
        return Engine.UnityCamera.farClipPlane;
    }

    public RendererApi etRendererApi()
    {
        return rScene.GetRendererApi();
    }

    public ISound GetSoundApi()
    {
        return rScene.GetSoundApi();
    }
    public CommandsApi GetCommandsApi()
    {
        return rScene.GetCommandsApi();
    }
    public EInput GetInputApi()
    {
        return rScene.GetInputApi();
    }
    public I3DSound Get3DSound()
    {
        return sound;
    }

    public bool SkipDebris(float prob)
    {
        Debug.LogFormat("m_VisualizerConfig.v_debrises_density {0} prob {1}", m_VisualizerConfig.v_debrises_density, prob);
        //throw new NotImplementedException();
        Asserts.AssertBp(prob >= -0 && prob <= 1);
        //return ((m_VisualizerConfig.v_debrises_density < Storm.Math.PRECISION) || (prob < Storm.Math.PRECISION)) ? true
        //  : ((m_VisualizerConfig.v_debrises_density > 1 - Storm.Math.PRECISION) || (prob > 1 - Storm.Math.PRECISION)) ? false
        //  : (Mathf.Pow(prob, 1 / m_VisualizerConfig.v_debrises_density - 1) < RandomGenerator.Rand01());
        if ((m_VisualizerConfig.v_debrises_density < Storm.Math.PRECISION) || (prob < Storm.Math.PRECISION)) return true;
        if ((m_VisualizerConfig.v_debrises_density > 1 - Storm.Math.PRECISION) || (prob > 1 - Storm.Math.PRECISION)) return false;

        return Mathf.Pow(prob, 1 / m_VisualizerConfig.v_debrises_density - 1) < RandomGenerator.Rand01();
    }
    #endregion hash
}

public partial class SceneVisualizer : IAnimationSupport
{

    public IAnimationSupport getIAnimationSupport() { return this; }


    public PARTICLE_SYSTEM createParticle(crc32 name) { return CreateParticle(name, .0f); }
    public ILight createLight(VisLightData data) { return CreateLight(data); }
    //ILenzFlare2* SceneVisualizer::createFlare(cstr name) { return CreateFlare(name); }
    public FPO createFpo(string name) { return CreateFPO(name); }

    public void registerObject(IHashObject ps) { AddNonHashObject(ps); }
    public void unregisterObject(IHashObject ps) { SubNonHashObject(ps); }

    public HMember registerHashed(IHashObject r) { return ConstructHM(r); }
    public void unregisterHashed(HMember hm) { DeleteHM(hm); }
    public void updateHashed(HMember hm) { UpdateHM(hm); }

    public IAnimation createAnimation(crc32 name, IAnimationServer srv, AnimationData data)
    {
        switch (name)
        {
            case AnimationParams.iPulseEmitter:
                //return createObject < PulseEmitter, IAnimationServer *,const AnimationData&> (srv, *data);
                return new PulseEmitter(srv, data);
        }
        rScene.Message("Can't create animation {0}", name);
        return null;
    }

    public static IEffect createParticleEffect(EffectBaseData dt, IAnimationServer srv)
    {
        return new EffectParticle((EffectParticleData)dt, srv);
        //return createObject < EffectParticle,const EffectParticleData&> (dt, srv);
    }

    //#include "Animation\EffectLight.h"
    public static IEffect createLightEffect(EffectBaseData dt, IAnimationServer srv)
    {
        return new EffectLight((EffectLightData)dt, srv);
        //return createObject<EffectLight, EffectLightData const&> (dt, srv);
    }
    public static IEffect createFlareEffect(EffectBaseData dt, IAnimationServer srv)
    {
        return null; //STUB! флары будет рисовать движок Unity
        //return new EffectFlare((EffectFlareData)dt, srv);
        //return createObject<EffectFlare, EffectFlareData const&> (dt, srv);
    }

    //#include "Animation\EffectFpo.h"
    public static IEffect createFpoEffect(EffectBaseData dt, IAnimationServer srv)
    {

        return new EffectFpo((EffectFpoData)dt, srv);
        //   return createObject < EffectFpo, EffectFpoData const&> (dt, srv);
    }
}

public partial class SceneVisualizer
{
    //HUD 
    protected IHUD pHud;
    public IHUD GetHud() { return pHud; }
    // переменная логика камеры
    protected DWORD CameraMode;
    protected CameraData pCameraData;
    protected CameraDataMap pCameraDataMap;
    protected CameraDataCockpit pCameraDataCockpit;
    protected CameraLogic pCameraLogic;
    public DWORD GetCameraMode() { return CameraMode; }
    public CameraLogic GetCameraLogic() { return pCameraLogic; }
    public CameraData GetCameraData() { return pCameraData; }
    public CameraDataMap GetCameraDataMap() { return pCameraDataMap; }
    public CameraDataCockpit GetCameraDataCockpit() { return pCameraDataCockpit; }

    public bool SetCameraLogic(DWORD code)
    {
        CameraLogic n = CameraLogic.Create(pCameraLogic, code);
        Debug.Log("Setting CameraLogic: " + code.ToString("X8") + " : " + n);
        if (n == null) return false;
        pCameraLogic.Dispose();
        pCameraLogic = n;
        CameraMode = code;
        return true;

    }
}

public class VisualizerConfig
{
    // visuals
    public int v_brass;//Brasses.int

    public int v_debrises; //Debrises.int
    public float v_debrises_density;

    public int v_decals;//Decals.int

    public int v_dust;//Dust.int

    public int v_light; //"Dynamic Lightning.int"
    public int v_dshadows; //"Dynamic Shadows.int"
    public int v_flares;//Flares.int
    public int v_HideUnderwater;//"Hide Underwater.int"

    public float v_objects_detail;//"Objects Detail.flt"
    public float v_particles_detail;//"Particle Detail.flt"
    public float v_random_objects_density;//"Random Objects Density.flt"

    public int v_recoil;//"Shoot Effects.int"
    public int v_sshadows;//"Static Shadows.int"

    public float r_range;//"View Range.flt"

    public int s_veffects;//"InGame Effects Volume.int"
    public int s_vhudspeach;//"HUD Speech Volume.int"
    public int s_vradio;    //"Radio Environment Volume.int"

    public int soundDetail;

    public int s_vcraftengine;


    public void LoadFrom(iUnifiedVariableContainer cfg_ctr)
    {
        Debug.Log("Loading Visualizer config " + cfg_ctr);
        iUnifiedVariableContainer opt = cfg_ctr.openContainer("Video");
        Debug.Log("Loading from Video container: " + opt);
        //UniVarContainer opt = (UniVarContainer) cfg_ctr.openContainer("Video");


        v_dshadows = v_sshadows = opt.getInt("Shadows");

        v_recoil = v_decals = v_dust = v_brass = opt.getInt("Effects");

        v_flares = v_light = 1;

        v_HideUnderwater = opt.getInt("TransparentWater") == 0 ? 0 : 1;

        r_range = opt.getFloat("Range");
        Debug.Log("r_range: "+ r_range);

        v_debrises_density = opt.getFloat("DebrisesDetail");
        v_debrises = v_debrises_density > 0 ? 1 : 0;


        v_random_objects_density = 1;

        v_objects_detail = opt.getFloat("ObjectsDetail");
        v_particles_detail = opt.getFloat("EffectsDetail");
        Debug.Log("Loaded from Video container: " + opt);
        opt = cfg_ctr.openContainer("Sound");

        s_veffects = opt.getInt("GameVolume");

        s_vcraftengine = opt.getInt("GameCraftEngineVolume");

        s_vhudspeach = opt.getInt("GameHudVolume");

        s_vradio = opt.getInt("GameRadioVolume");

        soundDetail = 2 - opt.getInt("GameSoundDetail");
    }
};

public partial class SceneVisualizer
{
    public void CreateVisualExplosion(EXPLOSION_DATA _expldata, Vector3 _org, Vector3 _dir, Vector3 _speed, DWORD id = 0)
    {
        float d = _expldata.VDist2;// * 100;
        float MaxD2 = Mathf.Pow(pCameraData.GetCameraRange(), 2);

        if (d > MaxD2) d = MaxD2;
        Vector3 v = _org - pCameraData.myCamera.Org;

        if (v.sqrMagnitude < d)
        {
            Debug.Log("Creating visual explosion " + _expldata + " @ " + _org + " " + v.sqrMagnitude + " < " + d + "?" + (v.sqrMagnitude < d));
            new Explosion(this, _expldata, _org, _dir, _speed, id);
        }
    }

    internal PARTICLE_SYSTEM CreateParticle(uint _name, float _speed)
    {
        IParticleData pd = GetSceneApi().GetParticleData(null, _name);
        Debug.LogFormat("ParticleData #{0} {1}", _name.ToString("X8"), pd);
        if (pd == null)
        {
# if _DEBUG
            rScene.Message(DParticleMissed, _name);
#endif
            return null;
        }
        PARTICLE_SYSTEM ps = new PARTICLE_SYSTEM(pd, _speed);
        pd.Release();

        ps.TextName = Hasher.StringHsh(_name);
        return ps;
    }

    public ILight CreateLight(VisLightData _ld)
    {

        if (m_VisualizerConfig.v_light != 0)
        {
            ILight light = GetSceneApi().CreateLight(new LightData());
            //light.SetColor(_ld.mColor);
            light.SetColor(new Color(_ld.mColor.x, _ld.mColor.y, _ld.mColor.z));
            light.SetIntensity(_ld.mIntensity);
            light.SetRadius(_ld.mRadius);
            return light;
        }
        return null;
    }
}
public partial class SceneVisualizer : CommLink
{
    public BaseScene rScene;
    SceneApi pScene;

    Vector3 prevCamPosition;

    List<RSection> RSectionsList;
    List<EnumPosition> vis_pos = new List<EnumPosition>();
    VisualizerEnumer vis_enumer;
    NonHashList cNonHashList = new NonHashList();

    VisualizerConfig m_VisualizerConfig;

    RoadNetData rd;

    //public List<VisualBaseActor> VisualActorsList = new List<VisualBaseActor>();
    public TLIST<VisualBaseActor> VisualActorsList = new TLIST<VisualBaseActor>();
    public List<DisplayedGameObject> DisplayedActors = new List<DisplayedGameObject>();


    //STUB
    //FpoBuilder fpoBuilder = new FpoBuilder();
    //iRadioEnvironment GetRadioEnvironment() { return mpRadioEnvironment; }
    public BaseScene GetScene() { return rScene; }

    public SceneApi GetSceneApi() { return pScene; }

    public object OnVariable(uint code, object data)
    {
        int tmp;
        //Debug.Log("SceneVisualizer processing Vars");
        switch (code)
        {
            case iCmMode:
                string logicName = pCameraLogic.GetName();
                if (data == null) return logicName;
                if (logicName == "cockpit" && (string)data == "cockpit")
                {
                    SetCameraLogic(Hasher.HshString("fps"));
                    return pCameraLogic.GetName();
                }
                SetCameraLogic(Hasher.HshString((string)data));
                return pCameraLogic.GetName();
                //if (data != null) SetCameraLogic(Hasher.HshString((string)data)); 
                //return pCameraLogic.GetName();
        }
        return null;
    }

    public void Initialize()
    {
        rScene.Message("Initializing SceneVisualizer");
        myHolder = new ParticleHolder(getIAnimationSupport());

        // загружам visual config
        LoadSceneConfig(rScene.GetGameData().mpVisConfigName == null ? "VisDefault" : rScene.GetGameData().mpVisConfigName);

        // создаем visualizer config

        m_VisualizerConfig = new VisualizerConfig();
        Debug.Log("config: " + rScene.GetOptions());
        m_VisualizerConfig.LoadFrom(rScene.GetOptions());
        rScene.Message("Loading scene");

        // загружаем имена configs
        mSceneNormalCofig = UniVarUtils.GetStringValueCrc(m_SceneConfigCtr, "SceneConfigNormal", Hasher.HshString("default#scene"));
        mSceneNvConfig = UniVarUtils.GetStringValueCrc(m_SceneConfigCtr, "SceneConfigNVision", mSceneNormalCofig);
        mSceneSfgConfig = UniVarUtils.GetStringValueCrc(m_SceneConfigCtr, "SceneConfigSfg", mSceneNormalCofig);
        myFpoTexturesPath = m_SceneConfigCtr.getString("TextureDbName");
        if (myFpoTexturesPath == null)
            myFpoTexturesPath = "Graphics\\Textures.dat";
        //Debug.Log("myFpoTexturesPath" + myFpoTexturesPath);
        SceneData sd = new SceneData();
        sd.config = mSceneNormalCofig;
        sd.TerrainData = rScene.GetTerrain();
        sd.hasher = rScene.GetHash();
        sd.objects = cNonHashList;
        sd.fpo_textures_path = myFpoTexturesPath;
        rScene.Message("\tInitializing Scene");
        pScene = rScene.GetRendererApi().CreateScene(sd);

        if (pScene == null)
        {
            throw new Exception("renderer scene creation failed");
        }

        rScene.Message("\tInitializing environment");
        InitEnvironment();


        // Init sound
        rScene.Message("\tInitializing sound");
        sound = rScene.GetSoundApi().Create3D(m_VisualizerConfig.s_veffects, true);

        sound.SetVolume(m_VisualizerConfig.s_veffects);
        // устанавливаем приоритеты звука
        sound.SetSceneDetail(m_VisualizerConfig.soundDetail);

        //Test - strategic camere
        //Debug.Log("\tInitializing CameraData");
        //pCameraData = new CameraData(rScene);
        //Debug.Log("\tInitializing CameraLogic");
        //pCameraLogic = new CameraLogicStrategic(this);
        //SetPlayerInterface(null);

        //Real game
        // HUD
        pHud = IHUD.CreateHUD(rScene);
        //устанавливаем режимы камеры
        rScene.Message("\tустанавливаем режимы камеры");
        pCameraData = new CameraData(rScene);

        SetPlayerInterface(null);
        pCameraDataMap = new CameraDataMap(rScene);
        pCameraDataCockpit = new CameraDataCockpit(rScene, pHud, pCameraData);
        pCameraLogic = new CameraLogic(this);

        //if (m_SceneConfigCtr!=null)
        //    if (weather_cfg = m_SceneConfigCtr.openContainer("Weather"))
        //    m_pFeatureWorld->ApplyConfig(weather_cfg);


        // регистрируем свои команды
        CommandsApi cmd = rScene.GetCommandsApi();
        cmd.RegisterVariable(sCmMode, this, VType.VAR_TEXT);

        // debug commands
        cmd.RegisterCommand(sCmShoot, this, 1, "cm_shoot <explosion>");
        cmd.RegisterCommand(sCmRoute, this, 1, "cm_route <use_roads flag>");
        cmd.RegisterCommand(sCmConfigure, this, 0, "Configure Your Best Scene");

        // info
        cmd.RegisterCommand(sVRoadInfo, this, 0, "info about roads");
        cmd.RegisterCommand(sVHashInfo, this, 0, "info about hash");

        // HUD CONFIG
        //if (m_SceneConfigCtr)
        //{
        //    CPtr HudConfigName = m_SceneConfigCtr->getString("HudConfigName");
        //    if (HudConfigName)
        //    {
        //        execDlc<getRootFile>(cmd, "Profiles\\Common", HudConfigName, false);
        //        execDlc<getRootFile>(cmd, rScene.GetGameData()->myProfileName, HudConfigName, true);
        //    }
        //}

    }

    public void AddNonHashObject(IHashObject o)
    {
        Asserts.Assert(o != null);
        cNonHashList.AddToTail(new HashObjectCont(o));
        Debug.Log(o.GetType().ToString() + " " + o.GetHashCode().ToString("X8") + " added to cNonHashList, total: " + cNonHashList.Counter());
    }

    private HashObjectCont FindObject(NonHashList list, IHashObject obj)
    {
        //for (HashObjectCont* head = list.Head(); head; head = list.Next(head))
        //    if (head->object== obj) return head;
        //return 0;
        //foreach (HashObjectCont head in list)
        //{
        //    if (head.myobject == obj) return head;
        //}
        for (HashObjectCont head = list.Head(); head != null; head = list.Next(head))
            if (head.myobject == obj) return head;
        return null;
    }

    public void SubNonHashObject(IHashObject obj)
    {
        HashObjectCont o = FindObject(cNonHashList, obj);
        if (o != null)
        {
            //cNonHashList.Remove(o);
            cNonHashList.Sub(o);
            o.Dispose();
        }
        obj.Dispose();
    }

    public void Draw(float[] Viewport)
    {
        //if (scene_config && scene_config->IsApplayed())
        //{
        //    pScene->ApplyConfig(scene_config->GetConfig());
        //    setCameraDist();
        //}
        //GetSceneApi()->SetVision(mCurrentInfra ? SV_INFRA : SV_NORMAL);
        pCameraLogic.Draw(Viewport);

    }

    public void Update(float scale)
    {
        float sc = scale * rScene.GetTimeScale();
        // Player Interface
        pPlayer.Process(scale);
        // update actors

        VisualBaseActor next = null;
        for (VisualBaseActor a = VisualActorsList.Head(); a != null; a = next)
        {
            next = VisualActorsList.Next(a);
            if (a.Update(sc)) continue;
            a.Dispose();
        }

        // update environment
        pCameraLogic.Update(scale);
        UpdateEnvironment();
        pScene.Update(sc); //Отрисовка сцены
        sound.Update(scale); //Обновление 3D звука
        pHud.Update(scale); //Обновление HUD
                            //m_pFeatureWorld.Update(sc);//Реализовать по готовности фичи на карте
                            //myHolder.update(sc); //Реализовать по готовности партиклы
    }


    private void InitEnvironment()
    {
        vis_enumer = new VisualizerEnumer(this);
        vis_pos.Add(new EnumPosition());
    }

    private Dictionary<int, DisplayedGameObject> LoadedRoads = new Dictionary<int, DisplayedGameObject>();
    private Dictionary<int, GameObject> LoadedRoadLinks = new Dictionary<int, GameObject>();

    //private List<BaseActor> GetClosestActors()
    //{
    //    float dX = rScene.GetTerrain().GetXSize() / 50;
    //    float dZ = rScene.GetTerrain().GetZSize() / 50;

    //    List<BaseActor> closestActors = new List<BaseActor>();
    //    for (int z = 0; z < 50; z++)
    //    {
    //        for (int x = 0; x < 50; x++)
    //        {
    //            Vector3 pos = new Vector3(dX * x, 0, dZ * z);
    //            if (Vector3.Distance(pos, pCameraData.myCamera.Org) > Camera.main.farClipPlane) continue;

    //            int cellIndex = z * 50 + x;
    //            closestActors.AddRange(sceneCells[cellIndex].Actors);
    //        }
    //    }
    //    return closestActors;
    //}
    //private List<NodeData> GetClosestNodes()
    //{
    //    List<NodeData> nodeDatas = new List<NodeData>();
    //    List<NodeData> closestNodes = new List<NodeData>();
    //    float dX = rScene.GetTerrain().GetXSize() / 50;
    //    float dZ = rScene.GetTerrain().GetZSize() / 50;
    //    for (int z = 0; z < 50; z++)
    //    {
    //        for (int x = 0; x < 50; x++)
    //        {
    //            Vector3 pos = new Vector3(dX * x, 0, dZ * z);
    //            //Debug.Log((x,z,pos, pCameraData.myCamera.Org, Vector3.Distance(pos, pCameraData.myCamera.Org), DRAW_DISTANCE));
    //            if (Vector3.Distance(pos, pCameraData.myCamera.Org) > Camera.main.farClipPlane) continue;

    //            int cellIndex = z * 50 + x;
    //            //Debug.Log("Hit! " + cellIndex);
    //            closestNodes.AddRange(sceneCells[cellIndex].RoadNodes);
    //        }
    //    }
    //    //foreach (NodeData node in rd.Nodes)
    //    foreach (NodeData node in closestNodes)
    //    {
    //        Vector3 LeveledPosition = node.head.org + (rScene.GetGroundLevel(node.head.org) * Vector3.up);
    //        //Vector3 LocalCoordinates = (node.head.org + (rScene.GetGroundLevel(node.head.org) * Vector3.up) * TerrainDefs.HeightScale) - pCameraData.myCamera.Org;
    //        Vector3 LocalCoordinates = LeveledPosition - pCameraData.myCamera.Org;
    //        float distance = LocalCoordinates.magnitude;
    //        if (distance > DRAW_DISTANCE) continue;
    //        nodeDatas.Add(node);
    //    }
    //    return nodeDatas;
    //}

    private void UpdateEnvironment()
    {
        // update roads
        //vis_pos.Head()->new_org     = pCameraData.myCamera.Org;
        //vis_pos.Head()->new_radius  = GetCameraData().GetCameraRange();

        vis_pos[0].new_org = pCameraData.myCamera.Org;
        vis_pos[0].new_radius = GetCameraData().GetCameraRange();
        //TODO вернуть отрисовку дорог на место
        //Debug.Log(DataHasherDefines.RSObjectId(DataHasherDefines.RS_ALL_ROADS_RELATED).ToString("X8"));
        rScene.GetDataHasher().EnumCrosses(ref vis_pos, DataHasherDefines.RSObjectId(DataHasherDefines.RS_ALL_ROADS_RELATED), vis_enumer);
    }
    //private void UpdateEnvironmentLinks()
    //{

    //    //Выбираем узлы в области видимости
    //    //nodeDatas = await GetClosestNodes();
    //    //return;
    //    //foreach (NodeData node in rd.Nodes)
    //    //{
    //    //    Vector3 LeveledPosition = node.head.org + (rScene.GetGroundLevel(node.head.org) * Vector3.up);
    //    //    //Vector3 LocalCoordinates = (node.head.org + (rScene.GetGroundLevel(node.head.org) * Vector3.up) * TerrainDefs.HeightScale) - pCameraData.myCamera.Org;
    //    //    Vector3 LocalCoordinates = LeveledPosition - pCameraData.myCamera.Org;
    //    //    float distance = LocalCoordinates.magnitude;
    //    //    if (distance > DRAW_DISTANCE) continue;
    //    //    nodeDatas.Add(node);
    //    //}

    //    List<NodeData> nodeDatas = GetClosestNodes();

    //    //Рисуем линки для каждого видимого узла
    //    foreach (NodeData node in nodeDatas)
    //    {
    //        Vector3 LeveledPosition = node.head.org + (rScene.GetGroundLevel(node.head.org) * Vector3.up);
    //        Vector3 LocalCoordinates = LeveledPosition - pCameraData.myCamera.Org;
    //        if (Vector3.Distance(pCameraData.myCamera.Org, LeveledPosition) > DRAW_DISTANCE) continue;
    //        int hash = node.GetHashCode();
    //        GameObject gobj;

    //        if (LoadedRoads.ContainsKey(hash))
    //        {
    //            gobj = LoadedRoads[hash].obj;
    //        }
    //        else
    //        {
    //            gobj = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //            gobj.name = "Road node " + hash.ToString("X8");
    //            foreach (int linkIndex in node.links)
    //            {
    //                gobj.name += " " + linkIndex;
    //            }

    //            //Debug.Log("Node " + hash.ToString("X8") + " " + node.head.org +" " + LeveledPosition + " " + rScene.GetGroundLevel(node.head.org));
    //            DisplayedGameObject road = new DisplayedGameObject(
    //                );
    //            road.Org = node.head.org;
    //            road.obj = gobj;
    //            LoadedRoads.Add(hash, road);
    //        }
    //        gobj.transform.position = LocalCoordinates;

    //        foreach (int linkIndex in node.links)
    //        {
    //            if (linkIndex >= rd.Links.Length)
    //            {
    //                Debug.Log(string.Format("Out of index {1}/{0}", rd.Nodes.Length, linkIndex));
    //                continue;
    //            }
    //            LinkData Neighbour = rd.Links[linkIndex];
    //            NodeData node1 = rd.Nodes[Neighbour.node1];
    //            NodeData node2 = rd.Nodes[Neighbour.node2];

    //            Vector3 Node1LeveledPosition = node1.head.org + (rScene.GetGroundLevel(node1.head.org) * Vector3.up);
    //            Vector3 Node2LeveledPosition = node2.head.org + (rScene.GetGroundLevel(node2.head.org) * Vector3.up);
    //            Debug.DrawLine(Node1LeveledPosition - pCameraData.myCamera.Org, Node2LeveledPosition - pCameraData.myCamera.Org);

    //            //Vector3 NeigbourLocalCoordinates = NeighbourLeveledPosition - pCameraData.myCamera.Org;

    //            //hash = Neighbour.GetHashCode();
    //            hash = (int)Hasher.HshString(Neighbour.node1 + " + " + Neighbour.node2);
    //            //Vector3 LocalProjectorCoordinates= (LocalCoordinates + NeigbourLocalCoordinates) / 2;
    //            Vector3 ProjectorPosition = (Node2LeveledPosition + Node1LeveledPosition) / 2;
    //            //ProjectorPosition += rScene.GetGroundLevel(ProjectorPosition)* 10 * Vector3.up;

    //            //Vector3 dir = NeighbourLeveledPosition - LeveledPosition;
    //            Vector3 dir = Node2LeveledPosition - Node1LeveledPosition;
    //            float yOffsetMax = Mathf.Max(Node1LeveledPosition.y, Node2LeveledPosition.y);
    //            float yOffsetMin = Mathf.Min(Node1LeveledPosition.y, Node2LeveledPosition.y);
    //            float yOffset = yOffsetMax - yOffsetMin;
    //            ProjectorPosition.y = yOffsetMax;
    //            if (Vector3.Distance(pCameraData.myCamera.Org, ProjectorPosition) > DRAW_DISTANCE) continue;

    //            Vector3 ProjectorLocalCoordinates = ProjectorPosition - pCameraData.myCamera.Org;
    //            if (LoadedRoads.ContainsKey(hash))
    //                gobj = LoadedRoads[hash].obj;
    //            else
    //            {
    //                gobj = new GameObject();
    //                ROADDATA road = ROADDATA.GetByCode(Neighbour.roaddata);
    //                gobj.name = "Decal projector " + road.FullName;

    //                DisplayedGameObject Link = new DisplayedGameObject
    //                {
    //                    Org = ProjectorPosition,
    //                    obj = gobj
    //                };
    //                LoadedRoads.Add(hash, Link);
    //                DecalProjector dp = gobj.AddComponent<DecalProjector>();
    //                dp.drawDistance = Camera.main.farClipPlane;
    //                dp.size = Vector3.forward * 200 + Vector3.up * dir.magnitude + Vector3.right * road.Width;
    //                //Material tmpMaterial = GameDataHolder.GetResource<Material>(PackType.MaterialsDB, road.SectionMaterial);
    //                Material tmpMaterial = new Material(Shader.Find("Shader Graphs/Decal"));
    //                Texture2D tmpTexture = GameDataHolder.GetResource<Texture2D>(PackType.TexturesDB, road.SectionTexture);

    //                //tmpMaterial.SetTexture(road.FullName, tmpTexture);
    //                tmpMaterial.SetTexture("Base_Map", tmpTexture);
    //                //Debug.Log(tmpMaterial.GetTexturePropertyNames());
    //                dp.material = tmpMaterial;
    //                dp.uvScale = new Vector2(1, dir.magnitude / road.Width);
    //                //dp.material = GameDataHolder.GetResource<Material>(PackType.MaterialsDB, "Water#water");
    //            }
    //            gobj.transform.position = ProjectorLocalCoordinates;
    //            gobj.transform.rotation = Quaternion.LookRotation(Vector3.down, dir);
    //            //Neighbour.head.org;
    //            //Debug.DrawLine(LocalCoordinates, NeigbourLocalCoordinates);


    //            //NodeData Neighbour = rd.Nodes[neighbourIndex];
    //            //Vector3 NeighbourLeveledPosition = Neighbour.head.org + (rScene.GetGroundLevel(node.head.org) * Vector3.up);
    //            //Vector3 NeigbourLocalCoordinates = NeighbourLeveledPosition - pCameraData.myCamera.Org;
    //            ////Neighbour.head.org;
    //            //Debug.DrawLine(LocalCoordinates, NeigbourLocalCoordinates);
    //        }
    //    }
    //    List<int> NodesToDelete = new List<int>();
    //    foreach (KeyValuePair<int, DisplayedGameObject> kvp in LoadedRoads)
    //    {
    //        //if (kvp.Value.transform.position.magnitude <= DRAW_DISTANCE) continue;
    //        if (Vector3.Distance(kvp.Value.Org, pCameraData.myCamera.Org) < DRAW_DISTANCE) continue;

    //        NodesToDelete.Add(kvp.Key);
    //    }

    //    foreach (int index in NodesToDelete)
    //    {
    //        GameObject gobj = LoadedRoads[index].obj;
    //        gobj.name = gobj.name + " deleted";
    //        Debug.Log("Deleting " + gobj.name);
    //        LoadedRoads.Remove(index);
    //        GameObject.Destroy(gobj);
    //    }
    //}

    public class DisplayedGameObject
    {
        public Vector3 Org;
        public BaseActor actor;
        public GameObject obj;
    }

    private void LoadRoads()
    {
        Stream st = rScene.GetGameData().mpCampaign.openStream("RoadNet");
        RoadNetDataHead hd = StormFileUtils.ReadStruct<RoadNetDataHead>(st);
        rd = new RoadNetData(hd);
        rd.head = hd;
        for (int i = 0; i < hd.links_count; i++)
        {
            rd.LinksI[i] = StormFileUtils.ReadStruct<int>(st, st.Position);
        }
        for (int i = 0; i < hd.nodes_count; i++)
        {
            rd.NodesI[i] = StormFileUtils.ReadStruct<int>(st, st.Position);
        }
        for (int i = 0; i < hd.visuals_count; i++)
        {
            rd.VisualsI[i] = StormFileUtils.ReadStruct<int>(st, st.Position);
        }


        for (int i = 0; i < hd.links_count; i++)
        {
            rd.Links[i] = StormFileUtils.ReadStruct<LinkData>(st, st.Position);
        }

        for (int i = 0; i < hd.nodes_count; i++)
        {
            NodeDataHead nhd = StormFileUtils.ReadStruct<NodeDataHead>(st, st.Position);
            NodeData nd = new NodeData();
            nd.head = nhd;
            nd.links = new int[nhd.link_count];
            for (int j = 0; j < nhd.link_count; j++)
            {
                nd.links[j] = StormFileUtils.ReadStruct<int>(st, st.Position);
            }
            rd.Nodes[i] = nd;
        }


        for (int i = 0; i < hd.visuals_count; i++)
        {
            VisualDataHead vdh = StormFileUtils.ReadStruct<VisualDataHead>(st, st.Position);
            VisualData vd = new VisualData();
            vd.head = vdh;
            vd.vectors = new Vector3[vdh.vector_count];
            for (int j = 0; j < vdh.vector_count; j++)
            {
                vd.vectors[j] = StormFileUtils.ReadStruct<Vector3>(st, st.Position);
            }
            rd.Visuals[i] = vd;
        }
        //StormFileUtils.SaveXML<RoadNetData>("Debug.xml", rd);
        st.Close();
    }

        /// <summary>
    /// интерфейс с игрой
    /// </summary>
    /// <param name="i"></param>
    public SceneVisualizer(BaseScene i)
    {
        rScene = i;
        vis_enumer = null;
        //scene_config = null;
        pCameraLogic = null;
        pCameraData = null;
        pCameraDataCockpit = null;
        mpRadioEnvironment = null;
        //pCameraDataMap = null;
        pScene = null;
        pHud = null;
        sound = null;
        //m_pFeatureScene = null;
        // m_pFeatureManager = null;
        //m_pFeatureWorld(0) = null;
        pPlayer = null;
        //myHolder = null; //TODO реализовать разные партиклы
        myDarkness = 1;

    }



    public void ProcessRSection(IVisualPart p, bool to_delete)
    {
        if (to_delete)
        {
            RSection d = p.GetVisual();
            d.Dispose();
            //delete d;
        }
        else
            CreateRSection(p);
    }
    public RSection CreateRSection(IVisualPart part)
    {
        ROADDATA data = part.Data();
        //AssertBp(data);
        if (data.Type == RoadType.Road)
            return new RDecal(this, part, RSectionsList);
        else
            return new RBuilding(rScene, part, RSectionsList);
    }
}

//SceneVisualizer.inl
public partial class SceneVisualizer
{
    public int GetDetailStage(Vector3 Org)
    {
        //CameraData& cd=GetCameraData();
        //  float a = (fabs(Org.x - cd.Camera.Org.x) + fabs(Org.z - cd.Camera.Org.z)) * .5f;
        //if (a<500.f ) return DETAIL_STAGE_FULL;
        //if (a<3000.f) return DETAIL_STAGE_HALF;
        //return (a<cd.GetCameraRange()? DETAIL_STAGE_QUARTER:DETAIL_STAGE_NONE);
        return BaseScene.DETAIL_STAGE_FULL; //TODO Модельки всё равно не очень детальные. Но при необходимости можно и реализовать.
    }
}
