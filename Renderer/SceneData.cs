using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using static HashFlags;
using static LIGHTBHVR;
using static LIGHTTYPE;
using static renderer_dll;
using static RoFlags;
using crc32value = System.UInt32;
//using NonHashList = System.Collections.Generic.List<IHashObject>;
//using NonHashList = System.Collections.Generic.List<HashObjectCont>;
using NonHashList = TLIST<HashObjectCont>;
using WORD = System.UInt16;

public struct SceneData
{
    public TERRAIN_DATA TerrainData;
    public IHash hasher;
    ICollision collision;
    public NonHashList objects;
    //List<IHashObject> objects;
    public crc32value config;
    public string fpo_textures_path;
};

public struct SceneObjects
{
    public GLight g_light;
    public Fog fog;
    public IHash hasher;
    public ProtoHash protohash;
    public NonHashList objects_list;
    public iTerrain terrain;
    public Sky sky;
    public Laser lasers;

    //IClouds myClouds;
};


public interface SceneApi
{
    public void Draw();
    public void Update(float scale);

    public SceneData GetSceneData();

    // dynamic scene properties
    //public void SetViewport(float aspect, float bounds[4] );
    public void SetCamera(MATRIX StormCamera);
    public void SetViewport(float mCameraAspect2, float[] viewPort2) { }
    public void SetVision(SCENE_VISION sV_INFRA);
    public IParticleData GetParticleData(string Name, uint Code);
    public IDecal CreatePolyDecal(PolyDecalData pd);
    ILight CreateLight(LightData data);
    ILaser CreateLaser();
}

public struct LightData
{
    int unused;
};
public class Scene : SceneApi
{
    SceneData data;
    public SceneObjects objects;

    //settings
    float near_dist;
    float view_dist;

    /// <summary>
    /// В исходнихах - просто camera, переименовано во избежание конфликтов
    /// </summary>
    public MATRIX SceneCamera;
    float myTime;
    ITimer myTimer;
    public float aspect;
    public float[] bounds = new float[4];

    EnvCfg cur_cfg;
    ObjId cur_cfgid;

    bool m_HideUnderwater;


    float m_ParticleLod;
    float m_ParticleSize;

    protected ParticleDataFactory mParticleDataFactory;
    public Scene()
    {
        //mInfraColor = new Color32(100 * OO256, 1, 0, 1);
        //mInfraMapDensity(1),
        //mInfraMap(0),
        //mEnvMap(0),
        //terr_meshexp(0),
        //mStateControl(0),
        //mCmd(0),
        //mParticleDataFactory(0),
        //mShadowFactory(new ShadowFactory),
        //mLinearLod = 3;
        m_HideUnderwater = false;
        m_ParticleLod = 1;
        m_ParticleSize = 64;

        myTimer = StormTimer.CreateTimer();
        Engine.HashMin = 0;
        Engine.HashMax = 0;
        Engine.HashAreaX0 = new int[1024];
        Engine.HashAreaX1 = new int[1024];
    }

    public ILaser CreateLaser()
    {
        Laser l = new Laser();
        l.InsertIntoChain(ref objects.lasers);
        return l;
    }
    bool ApplyConfig(ObjId config)
    {
        throw new System.NotImplementedException();
        //    EnvCfg ec = dll_data.LoadFile<EnvCfg>(config);
        //    if (ec==null) return false;

        //    //ConfigureTexture(&mEnvMap, ec->sg_maps[0], cur_cfg.sg_maps[0]);

        //    objects.g_light.ApplyConfig(ec.g_light);
        //    //objects.fog.ApplyConfig(ec.fog);
        //    if (objects.terrain!=null)
        //        objects.terrain.ApplyConfig(ec.terrain);
        //    objects.sky.ApplyConfig(ec.sky);

        //    cur_cfg = ec;
        //    cur_cfgid = config;

        //    PostInit();
        // return true;
    }

    public IParticleData GetParticleData(string Name, uint Code)
    {

        uint code = Name != null ? Hasher.HshString(Name) : Code;

        mParticleDataFactory.SetData(code);

        return mParticleDataFactory.CreateObject(code);

    }

    Texture2D mEnvMap;
    public bool InitEnvironment(SceneData si)
    {
        EnvCfg ec = renderer_dll.dll_data.LoadFile<EnvCfg>(si.config);
        if (ec == null) ec = renderer_dll.dll_data.LoadFile<EnvCfg>("q#scene");
        if (ec == null) return false;
        mEnvMap = renderer_dll.dll_data.LoadTexture(ec.sg_maps[0]);

        InitGlobalLight(new GLight(), ec.g_light);

        //if (si.TerrainData != null && !InitTerrain(si.TerrainData, 64, 0xBAADF00D))
        if (si.TerrainData != null && !InitTerrain(si.TerrainData, 64, ec.terrain))
        {
            Debug.Log("Terrain Initialization failed!!!");
            return false;
        }

        //InitSky(new Sky(), Hasher.HshString("cloudy#env"));
        InitSky(new Sky(), ec.sky);

        cur_cfg = ec;
        cur_cfgid = si.config;
        return true;
    }

    public bool InitGlobalLight(GLight myobject, crc32value config)
    {
        myobject.ApplyConfig(config);
        objects.g_light = myobject;
        return true;
    }

    public bool InitParticles()
    {
        mParticleDataFactory = new ParticleDataFactory();
        return true;
    }


    public bool InitTerrain(TERRAIN_DATA td, int size, crc32value config)
    {
        //Terrain myTerrain = new Terrain(td, data.collisions, size);
        StormTerrain myTerrain = new StormTerrain(td, "data.collisions", size);
        if (!myTerrain.Initialize(this, config))
        {
            myTerrain.Release();
            return false;
        }
        objects.terrain = myTerrain;
        return true;

    }
    private void DrawEnvironment(bool Last, bool sun_visible)
    {
        if (objects.sky != null && Engine.draw_sky)
            objects.sky.Draw(Last, Engine.EngineCamera, sun_visible);
    }

    public void DrawObjects()
    {
        if (Engine.draw_terrain)
            Engine.DrawGround();
        Engine.DrawObjects(false); //TODO вернуть, при необхожимости, отрисовку объектов без эффектов

        if (Engine.draw_water)
            Engine.DrawWater();

        //if (camera.Org.y < hClouds)
        //{
        //    drawClouds();
        //    Engine::DrawObjects(true);
        //}
        //else
        //{
        //    Engine::DrawObjects(true);
        //    drawClouds();
        //}
        Engine.DrawObjects(true);
        DrawLasers();

    }

    void DrawLasers()
    {
        //Engine::ApplyWorldTransform();
        //Engine::rs_d3d2->Apply();
        //Engine::laser_rs->Apply();
        d3d.SetTexture(Engine.LaserTexture);
        //D3DPipe.Begin();
        for (Laser l = Engine.scene.objects.lasers; l != null; l = l.next)
        {
            //D3DPipe.Flush();
            l.Draw();
        }
        //D3DPipe.End();
    }

    public float GetViewDistance() { return view_dist; }
    public void Draw()
    {
        PARTICLE_SYSTEM.SetLod(m_ParticleLod);
        PARTICLE_SYSTEM.SetMax(m_ParticleSize);


        float med0_dist = (float)System.Math.Pow((double)(view_dist * Mathf.Pow(near_dist, 2)), 1 / 3);
        float[,] z_range = new float[3, 2] //for 0's is closest and 2's fartherst
        {
            { CAMERA.SceneNearZ, CAMERA.SceneNearZ + (CAMERA.SceneFarZ - CAMERA.SceneNearZ) * (1/ 3) },
            { CAMERA.SceneNearZ + (CAMERA.SceneFarZ - CAMERA.SceneNearZ) * (1/ 3), CAMERA.SceneNearZ + (CAMERA.SceneFarZ - CAMERA.SceneNearZ) * (2/ 3) },
            { CAMERA.SceneNearZ + (CAMERA.SceneFarZ - CAMERA.SceneNearZ) * (2/ 3), CAMERA.SceneFarZ }
        };

        float med1_dist = Mathf.Pow(med0_dist, 2) / near_dist;
        if (!Engine.scene_z_split)
        {
            Engine.InitMissionScene(this, near_dist, GetViewDistance(), CAMERA.SceneNearZ, CAMERA.SceneFarZ);
        }
        else
        {
            Engine.InitMissionScene(this, med1_dist, GetViewDistance(), z_range[2, 0], z_range[2, 1]);
        }
        //objects.fog.Prepare();

        bool sunvisible = true;
        //DrawEnvironment(false, sunvisible);

        if (objects.terrain != null)
            objects.terrain.Prepare();

        DrawObjects();

        if (Engine.scene_z_split)
        {

            Engine.InitMissionScene(this, med0_dist, med1_dist, z_range[1, 0], z_range[1, 1]);
            DrawObjects();

            Engine.InitMissionScene(this, near_dist, med0_dist, z_range[0, 0], z_range[0, 1]);
            DrawObjects();
        }
        //TODO передавать сюда objects.g_light->dir
        //Engine.EngineCamera.Init(SceneCamera, bounds, Engine.UnityCamera.nearClipPlane, Engine.UnityCamera.farClipPlane, aspect, Engine.UnityCamera.nearClipPlane, Engine.UnityCamera.farClipPlane, Vector3.down);
        Engine.EngineCamera.Init(SceneCamera, bounds, Engine.UnityCamera.nearClipPlane, Engine.UnityCamera.farClipPlane, aspect, Engine.UnityCamera.nearClipPlane, Engine.UnityCamera.farClipPlane, objects.g_light.dir);
        DrawEnvironment(false, sunvisible);
        //Draw UI
        //DrawUI();
        Engine.DoneMissionScene();
    }

    public bool Initialize(SceneData si, CommandsApi _cmd)
    {
        //Load FPO textures;
        if (!dll_data.openFpoTextures(si.fpo_textures_path))
        {
            StdOut.Message("Error : can't open fpo textures db \"{0}\"", si.fpo_textures_path != null ? si.fpo_textures_path : "default");
            return false;
        }

        data = si;
        InitObjectsHash(si);
        InitObjectsList(si);
        InitParticles();
        //InitFlares();

        if (!InitEnvironment(si)) return false;
        return PostInit();
    }

    private bool InitObjectsList(SceneData si)
    {
        objects.objects_list = si.objects;
        return true;
    }

    private bool InitObjectsHash(SceneData si)
    {
        if (si.hasher != null)
        {
            objects.hasher = si.hasher;
            objects.protohash = (ProtoHash)objects.hasher.Query(ProtoHash.ID);
            Asserts.AssertBp(objects.protohash != null);
        }
        else
        {
            objects.hasher = null;
            objects.protohash = null;
        }
        return true;

    }

    private bool PostInit()
    {
        //STUB
        return true;
    }

    private bool InitSky(Sky skyobject, crc32value config)
    {
        skyobject.Initialize(config);
        //skyobject.Initialize("cloudy#env");
        objects.sky = skyobject;
        StormUnityRenderer.InitSkyUnity(skyobject, objects.g_light);
        return true;
    }

    private GameObject sun;

    public void Update(float scale)
    {
        myTime += (float)myTimer.Update();
        if (objects.terrain != null) objects.terrain.Update(scale);
        if (objects.sky != null) objects.sky.Move(scale);
        //if (objects.myClouds != null) objects.myClouds.update(scale);

    }

    public SceneData GetSceneData()
    {
        return data;
    }

    public void SetCamera(MATRIX StormCamera)
    {
        //this.StormCamera = StormCamera;
        this.SceneCamera = new MATRIX(StormCamera);
    }

    public void SetViewport(float mCameraAspect2, float[] viewPort2)
    {
        aspect = mCameraAspect2;
        for (int i = 0; i < 4; ++i) bounds[i] = viewPort2[i];

        //mLinearLod.SetLodScale(aspect * d3d.Dx() * bounds[2]);
    }

    public void SetVision(SCENE_VISION sV_INFRA)
    {
        throw new System.NotImplementedException();
    }

    IMeshExporter terr_meshexp;
    IMeshExporter GetTerrainMesh()
    {
        if ((terr_meshexp == null) && objects.terrain != null)
        {
            terr_meshexp = objects.terrain.CreateMeshExporter();
        }
        if (terr_meshexp != null) terr_meshexp.AddRef();
        return terr_meshexp;
    }
    public IDecal CreatePolyDecal(PolyDecalData data)
    {
#warning Генерация PolyDecals (дороги и следы взрывов) отключена
        return null;

        //IMeshExporter exp = GetTerrainMesh();
        //if (exp == null) return null;

        //PolyDecal decal = new PolyDecal();
        //if (!decal.Initialize(exp, data))
        //{
        //    exp.Release();
        //    decal.Release();
        //    return null;
        //}
        //exp.Release();

        //IObject idecal = decal;
        //IDecal ret_decal = (IDecal)idecal.Query(IDecal.ID);
        ////IDecal ret_decal = idecal.Query<IDecal>(); //TODO Возможно, такой каст работать не будет.
        //decal.Release();

        //return ret_decal;

    }

    public ILight CreateLight(LightData data)
    {
        LightObject light = new LightObject();
        if (!light.Initialize(data))
        {
            light.Release();
            return null;
        }

        IObject ilight = light;
        ILight ret_light = (ILight)ilight.Query(ILight.ID);
        light.Release();

        return ret_light;
    }

    public void SetHideUnderwater(bool state)
    {
        m_HideUnderwater = state;
    }
    public bool GetHideUnderwater()
    {
        return m_HideUnderwater;
    }

    internal Texture2D GetEnvMap(int stage)
    {
        return mEnvMap;
    }

    internal int GetLod(float radius, float distance)
    {
        //TODO Ркализовать корректно получение LOD (или оставить как есть - в конце концов, для современной системы модельки не очень детальные
        //return mLinearLod.GetLod(radius, distance);
        return BaseScene.DETAIL_STAGE_FULL;
    }
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
class SceneGameInfo
{
    float max_distance;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    float[] reserved = new float[3];
};

public class GLight
{
    public Vector3 dir;
    public Vector3 v_color;
    Vector4 s_color;
    float diffuseBright;

    LIGHT gl_light;
    LIGHT sh_light;

    GLightCfg cur_cfg;
    ObjId cur_cfgid;

    public GLight()
    {
        gl_light = null;
        sh_light = null;
    }
    //~GLight()
    //{
    //    SafeRelease(gl_light);
    //    SafeRelease(sh_light);
    //}

    ObjId GetConfig() { return cur_cfgid; }


    public void ApplyConfig(ObjId config)
    {
        GLightCfg cfg = renderer_dll.dll_data.LoadFile<GLightCfg>(config);

        if (cfg == null)
        {
            Log.Message("Error : Invalid Light Config, applying default...\n");
            cfg = dll_data.LoadFile<GLightCfg>("default#glight");
            Assert.IsNotNull(cfg);
        }

        Vector3 ldir = StormTransform.Globus(cfg.a, cfg.h);
        dir = new Vector3(ldir.x, ldir.y, ldir.z);
        s_color = GetColor(cfg.diffuse);
        v_color.Set(cfg.diffuse.r, cfg.diffuse.g, cfg.diffuse.b);



        //diffuseBright = (color::grayScaleNTSC(cfg->diffuse) + color::grayScaleNTSC(cfg->ambient)) * .5f;

        LightDesc ld;

        //SafeRelease(gl_light);
        gl_light = LIGHT.Create(LT_DIRECTIONAL, LB_STATIC);
        //SafeRelease(sh_light);
        sh_light = LIGHT.Create(LT_DIRECTIONAL, LB_STATIC);

        gl_light.SetDiffuse(cfg.diffuse);
        gl_light.SetSpecular(cfg.specular);
        gl_light.SetAmbient(cfg.ambient);
        gl_light.SetDirection(-dir);

        sh_light.SetDiffuse(Color.black);
        sh_light.SetSpecular(Color.black);
        sh_light.SetAmbient(cfg.ambient);
        sh_light.SetDirection(-dir);

        cur_cfg = cfg;
        cur_cfgid = config;
    }

    public static Color GetColor(Vector3 v, int a = 255)
    {
        //return new Color(a, int(v.r * 255), int(v.g * 255), int(v.b * 255));
        return new Color(v.x, v.y, v.z, a / 255);
    }
    public static Color GetColor(Color v, int a = 255)
    {
        //return new Color(a, int(v.r * 255), int(v.g * 255), int(v.b * 255));
        return new Color(v.r, v.g, v.b, a / 255);
    }

};

public class LightDesc
{
    public Vector3 Diffuse;
    public Vector3 Ambient;
    public Vector3 Specular;
    public float R;
};

public class LightObject : IObject
{
    ~LightObject()
    {
        Dispose();
    }
    public void Dispose()
    {
        IMemory.SafeRelease(mLight);
    }
    public LightObject()
    {
        mLight = null;
        mEssence = new LightEss<LightObject>(this);
        mHashed = new THashed<LightObject>(OF_GROUP_RENDER | ROFID_LIGHT, this);
        mControl = new LightControl<LightObject>(this);
        mRefCount = 1;
    }

    public bool Initialize(LightData data)
    {
        mLight = LIGHT.Create();
        return true;
    }

    public void SetRadius(float r)
    {
        mLight.SetRadius(r);
    }

    public float GetRadius()
    {
        return mLight.GetRadius();
    }

    public void SetIntensity(float i)
    {
        mLight.SetIntensity(i);
    }

    public void SetColor(Color color)
    {
        mLight.SetColor(color);
    }

    public void SetPosition(Vector3 pos)
    {
        mLight.SetPosition(pos);
    }

    public void SetDirection(Vector3 dir)
    {
        mLight.SetDirection(dir);
    }

    public IHashObject GetHashObject()
    {
        return mHashed;
    }

    public geombase.Sphere GetBoundingSphere()
    {
        return mLight.GetBoundingSphere();
    }

    public LIGHT GetLIGHT()
    {
        return mLight;
    }


    public void AddRef()
    {
        mRefCount++;
    }
    public int Release()
    {
        mRefCount--;
        if (mRefCount != 0)
            return mRefCount;
        Dispose();
        return 0;
    }

    public object Query(uint id)
    {
        switch (id)
        {
            case ILight.ID:
                {
                    ILight ret = mControl;
                    ret.AddRef();
                    return ret;
                }
            case IHashObject.ID:
                {
                    IHashObject ret = mHashed;
                    return ret;
                }
            case ILightEss.ID:
                {
                    ILightEss ret = mEssence;
                    ret.AddRef();
                    return ret;
                }

        }
        ;
        return null;
    }
    private
  LIGHT mLight;

    LightEss<LightObject> mEssence;
    THashed<LightObject> mHashed;
    LightControl<LightObject> mControl;

    int mRefCount;
};

/// <summary>
/// Implementation of Light object renderer's interface 
/// </summary>
class LightControl<L> : ILight where L : LightObject
{

    public LightControl(L light)
    {
        mLight = light;
    }

    //ILight interface implementation
    public void SetRadius(float r)
    {
        mLight.SetRadius(r);
    }

    public float GetRadius()
    {
        return mLight.GetRadius();
    }

    public void SetIntensity(float i)
    {
        mLight.SetIntensity(i);
    }

    public void SetColor(Color color)
    {
        mLight.SetColor(color);
    }

    public void SetPosition(Vector3 pos)
    {
        mLight.SetPosition(pos);
    }

    public void SetDirection(Vector3 dir)
    {
        mLight.SetDirection(dir);
    }

    public IHashObject GetHashObject()
    {
        return mLight.GetHashObject();
    }

    public void AddRef()
    {
        mLight.AddRef();
    }

    public int Release()
    {
        return mLight.Release();
    }

    public object Query(uint cls_id)
    {
        return mLight.Query(cls_id);
    }

    private L mLight;
};

public class TerrainBox : aBox
{

    public int[] mstatei = new int[3];
    public int[] msurfacei = new int[3];

    public int vscripti;
    public int ascripti;

    public TerrainVB vb;
    public int start_vtx;
    public int num_vtx;

    public WORD[] idxs;
    public int num_idx;

    //public TerrainBox()
    //{
    //    //TODO здесь ошибка - vb должен быть откуда-то взят, а не генерироваться по месту
    //    vb = new TerrainVB();
    //}
};

public interface Invalidator
{
    public void invalidate();
};

public struct Fog
{
    public float max_d, fog_d, fog_c, oo_maxd;
    public Storm.Gradient gradient;
    public SVec4 color;
    public int v_enabled;
    public float v_height;
    public float v_density;
    FogCfg cur_cfg;
    ObjId cur_cfgid;


    ObjId GetConfig() { return cur_cfgid; }
    void ApplyConfig(ObjId config)
    {
        FogCfg cfg = dll_data.LoadFile<FogCfg>(config);
        if (cfg == null)
        {
            Log.Message("Error : Invalid Fog Config, applying default...\n");
            cfg = dll_data.LoadFile<FogCfg>("default#fog");
            Asserts.AssertBp(cfg);
        }

        gradient = cfg.color;

        cur_cfg = cfg;
        cur_cfgid = config;
    }


    void Prepare()
    {
        max_d = Engine.GetViewDist() * cur_cfg.end_dist;
        fog_d = Engine.GetViewDist() * cur_cfg.start_dist;
        oo_maxd = 1.0f / (max_d + 10);
        fog_c = 255 / (max_d - fog_d);
    }

    float Level(float d) { float f = (d - fog_d) * fog_c; return f < 0 ? 0 : f > 255 ? 255 : f; }
    void Set(Vector3 CDir, Vector3 LDir)
    {
        Vector3 cdir = new Vector3(CDir.x, CDir.y, CDir.z);
        Vector3 ldir = new Vector3(LDir.x, LDir.y, LDir.z);
        color = Engine.GetSVec4(gradient.GetColor(cdir, ldir, 0));
    }
};

public class FogCfg : IStormImportable<FogCfg>
{
    public float start_dist;
    public float end_dist;
    public Storm.Gradient color;

    public FogCfg Import(Stream st)
    {
        return StormFileUtils.ReadStruct<FogCfg>(st);
    }
};