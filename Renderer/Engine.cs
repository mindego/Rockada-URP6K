using geombase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using static Clip;
using static D3DEMULATION;
using static HashFlags;
using static renderer_dll;
using static RoFlags;
using static UnityEngine.Mesh;
using CPointsList = AList<CPOINT>;
using DWORD = System.UInt32;
using lstacktype = System.Collections.Generic.Stack<Engine.LStacker>;
using ScanLine = System.ValueTuple<int, int>;
using static D3DRENDERSTATETYPE;

public class CAMERA : MATRIX
{
    public const float SceneNearZ = 0.1f, SceneFarZ = 1.0f;
    public static float start_dist, end_dist;
    private Vector3 floatingOrigin;
    public CAMERA(Vector3 org) : base(org)
    {
        floatingOrigin = org;
    }

    public int SphereVisibleEx(Vector3 v, float r)
    {
        return 1;
    }

    internal int BoxVisible(float lo, float hi, float x, float z, float size, float r, ref PLANE[] dst_planes)
    {
        //Vector3 V = new Vector3(x, (hi + lo) * .5f, z);
        //float half_s = size * .5f;

        //int n_planes = 0;
        //Vector3 Vmin = new Vector3(x - half_s, lo, z - half_s);
        //for (int i = 0; i < 6; i++)
        //{
        //    float temp = planes[i].d - planes[i].n * V;
        //    if (temp < -r)
        //        return -1;
        //    if (temp < r)
        //    {

        //        Vector3 Vouter = Vmin;
        //        Vector3 Vinner = Vmin;
        //        Vector3 n = planes[i].n;
        //        float d = planes[i].d;

        //        if (n.x > 0) Vouter.x += size; else Vinner.x += size;
        //        if (n.y > 0) Vouter.y = hi; else Vinner.y = hi;
        //        if (n.z > 0) Vouter.z += size; else Vinner.z += size;

        //        if (d < n * Vinner) return -1;
        //        if (d < n * Vouter)
        //            n_planes++;
        //        //if ( planes[i].d-planes[i].n*Vin  <0 )  
        //        //   dst_planes[]=planes[i];
        //    }
        //}

        //return n_planes;
        Vector3 V = new Vector3(x, (hi + lo) * .5f, z);
        //Debug.LogFormat("Is visible box in [{0}", V);

        return 1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stormCamera">Matrix камеры</param>
    /// <param name="bounds"></param>
    /// <param name="sceneNearZ"></param>
    /// <param name="sceneFarZ"></param>
    /// <param name="aspect"></param>
    /// <param name="near_dist"></param>
    /// <param name="view_dist"></param>
    /// <param name="dir">Направление глобального света</param>

    internal void Init(MATRIX stormCamera, float[] bounds, object sceneNearZ, object sceneFarZ, float aspect, float _start_dist, float _end_dist, Vector3 dir)
    {

        UpdateFloatingOrigin(stormCamera);
        Engine.SetView(stormCamera.Org);
        this.Set(stormCamera);
        end_dist = _end_dist;
        start_dist = _start_dist;

        // a = cot( fovh / 2) / 2
        //(MATRIX &) * this = M;
        //ViewPort.Set(Vpt, start_z, end_z);
        //Aspect = a * ViewPort.hres;


        //start_dist = _start_dist;
        //end_dist = _end_dist;
        //CalcPlanes();

        //GlDir = Glight;

        //Engine::SetProjectionPersp(
        //    a * 2.f, 2.f * a * ViewPort.hres / ViewPort.vres,
        //    start_dist, end_dist);

        //Matrix34f vm = MathConvert::FromLocus(*this);
        //Engine::SetView(vm);

        //d3d.ReCalcZScales(start_dist, end_dist, start_z, end_z);
    }

    private void UpdateFloatingOrigin(MATRIX stormCamera)
    {
        Vector3 Diff = stormCamera.Org - floatingOrigin;

        if (Diff.sqrMagnitude > Mathf.Pow(Engine.UnityCamera.farClipPlane / 4, 2)) // чтобы не извлекать корни
        {
            floatingOrigin = stormCamera.Org;
            Engine.UnityCamera.transform.position = Vector3.zero;
            //GameObject gobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //gobj.name = "Floating origin " + floatingOrigin;
            //gobj.transform.position = Diff;
            //GameObject.Destroy(gobj, 10);
        }
        else
        {
            Engine.UnityCamera.transform.position = Diff;
            //Engine.UnityCamera.transform.position = floatingOrigin + Diff;
        }
        //Engine.UnityCamera.transform.position = Vector3.zero;
        //Rotating Unity camera
        Engine.UnityCamera.gameObject.transform.rotation = Quaternion.LookRotation(stormCamera.Dir, stormCamera.Up);
    }
    private void UpdateFloatingOriginCampos(MATRIX stormCamera)
    {
        Vector2 flatPosition = new Vector2(Engine.UnityCamera.gameObject.transform.position.x, Engine.UnityCamera.gameObject.transform.position.z);
        Vector3 Diff = stormCamera.Org - this.Org;

        if (flatPosition.sqrMagnitude > Mathf.Pow(Engine.UnityCamera.farClipPlane / 4, 2)) // чтобы не извлекать корни
        {
            floatingOrigin = Engine.UnityCamera.transform.position;
            Engine.UnityCamera.transform.position = new Vector3(0, stormCamera.Org.y, 0);
            GameObject gobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gobj.name = "Floating origin " + floatingOrigin;
            gobj.transform.position = floatingOrigin;
            GameObject.Destroy(gobj, 10);
        }
        else
        {
            Engine.UnityCamera.transform.position += Diff;
            //Engine.UnityCamera.transform.position = floatingOrigin + Diff;
        }
        //Engine.UnityCamera.transform.position = Vector3.zero;
        //Rotating Unity camera
        Engine.UnityCamera.gameObject.transform.rotation = Quaternion.LookRotation(stormCamera.Dir, stormCamera.Up);
    }

}

public static partial class Engine //Renderer
{
    // Statistics
    public static int FacetsDrawed, VerticesDrawed;
    public static int ObjectsDrawed, ShadersApplied;
    public static int ParticlesDrawed;
    public static int BillVerticesDrawed;
    public static int BillFacetsDrawed;

    internal static void SetEnvMap(int stage)
    {
        d3d.SetTexture(scene.GetEnvMap(stage), stage);
    }

    public static SVec4 GetColor(Vector3 v, int a = 255)
    {
        //return new SVec4(a, int(v.r * 255), int(v.g * 255), int(v.b * 255));
        return new SVec4(a, (int)(v.x * 255), (int)(v.y * 255), (int)(v.z * 255));
    }

    public static SVec4 GetSVec4(Vector4 v)
    {
        //union {
        //           struct { t x, y, z, w; };
        //           struct { t r, g, b, a; };
        //      t raw[4];
        //};
        //return new SVec4(v.a, v.r, v.g, v.b);
        return new SVec4((int)v.w, (int)v.x, (int)v.y, (int)v.z);
    }


    public static Matrix4f env_transform;

    // stdmacros
    public static iRS rs_default,
                 rs_meo1,
                 rs_meo2,
                 rs_d3d1,
                 rs_d3d2,
                 rs_part1,
                 rs_part2,
                 rs_part3,
                 decal_mainRS,
                 light_mainRS,
                 shadow_mainRS,
                 laser_rs;

    public static string CurrentRenderingStage {
        set
        {
            switch (value)
            {
                case "std_transp_none":
                case "std_transp_add":
                case "std_transp_blend":
                    CurrentRenderingStageTransparancy = value;
                    break;
                case "std_tss_diffuse":
                case "std_tss_diffuse_no_texture":
                    CurrentRenderingStageTexture = value;
                    break;
                default:
                    CurrentRenderingStageTransparancy = "ERROR TRANSP RENDER STAGE";
                    CurrentRenderingStageTexture = "ERROR TSS RENDER STAGE"; ;
                    break;
            }
        }
    }
    public static string CurrentRenderingStageTransparancy { get; internal set; }
    public static string CurrentRenderingStageTexture { get; internal set; }
}

public static partial class Engine //EngineState
{
    public static SVec4 GetFogColor()
    {
        return scene.objects.fog.color;
    }

    public static float GetFogStartDist()
    {
        return scene.objects.fog.fog_d;
    }

    public static float GetFogEndDist()
    {
        return scene.objects.fog.max_d;
    }

    public static float GetViewDist()
    {
        return scene.GetViewDistance();
    }

    public static int GetLod(float radius, float distance)
    {
        return scene.GetLod(radius, distance);
    }
}
public static partial class Engine //scenedraw
{
    public static void SetFogState(float start, float end, DWORD color)
    {
        HRESULT hr;
        d3d.FogColor(color);
        hr = d3d.Device().SetRenderState(D3DRENDERSTATE_FOGSTART, (DWORD)start);
        hr = d3d.Device().SetRenderState(D3DRENDERSTATE_FOGEND, (DWORD)end);
    }
}
public static partial class Engine //EngData
{
    const string EngineCantLoadSS = "Renderer can`t load {0} {1}\n";
    const string EngineCantLoadSP = "Renderer can`t load {0} {1}\n";
    const string DefaultName = "default";
    const DWORD DEFAULT_CRC_CODE = 0x1CA1FF20;

    public static void FlushDB()
    {
        sRObjPipe.Reset();
        sRObjEffectsPipe.Reset();

        mFpoImages.Flush();
        mFpoShaders.Flush();
    }

    private static FpoImageFactory mFpoImages;
    private static FpoShaderFactory mFpoShaders;
    public static IShader CreateFpoShader(FpoShaderId id)
    {
        Debug.LogFormat("CreateFpoShader using {0}", id.mMaterialData);
        //mFpoShaders.SetData(id.mMaterialData);
        mFpoShaders.SetData(id);
        return mFpoShaders.CreateObject(id);

    }
    
}

public static partial class Engine //Clip.cpp
{
    public static bool ClipTube(Matrix34f Cam, out int min, out int max, int[] aleft, int[] aright, float oO_SQUARE_SIZE, int v1, int v2, int basex, int basez)
    {
        //TODO Обдумать и реализовать вырезание трубы.
        min = 0; max = 1;
        return true;
    }
    //    float ox = Cam.pos.x, oz = Cam.pos.z;
    //    float dx = ox + Cam.tm.raw[2].x, dz = oz + Cam.tm.raw[2].z;
    //    float drx = ox - Cam.tm.raw[2].x, drz = oz - Cam.tm.raw[2].z;
    //    float rx = Cam.tm.raw[0].x, rz = Cam.tm.raw[0].z;
    //    float ux = Cam.tm.raw[1].x, uz = Cam.tm.raw[1].z;

    //    CPOINT[] CPoints = new CPOINT[2 * 8];
    //    int[] CNext = new int[2 * 8];
    //    int[] CPrev = new int[2 * 8];
    //    CPointsList SourceList = new CPointsList(CPoints   , CNext   , CPrev   ),
    //          ResultList = new CPointsList(CPoints, CNext, CPrev,8);

    //    AddVectorToList(SourceList, new Vector3(drx + rx + ux, 0, drz + rz + uz));
    //    AddVectorToList(SourceList, new Vector3(drx + rx - ux, 0, drz + rz - uz));
    //    AddVectorToList(SourceList, new Vector3(drx - rx - ux, 0, drz - rz - uz));
    //    AddVectorToList(SourceList, new Vector3(drx - rx + ux, 0, drz - rz + uz));
    //    AddVectorToList(SourceList, new Vector3(dx + rx + ux, 0, dz + rz + uz));
    //    AddVectorToList(SourceList, new Vector3(dx + rx - ux, 0, dz + rz - uz));
    //    AddVectorToList(SourceList, new Vector3(dx - rx - ux, 0, dz - rz - uz));
    //    AddVectorToList(SourceList, new Vector3(dx - rx + ux, 0, dz - rz + uz));

    //    Convex c;

    //    MakeConvexList(c, SourceList);

    //    float[] x = new float[8], z = new float[8];

    //    int num = c.size();
    //    min = 0;max = 0;
    //    if (num < 3) return false;

    //    int i = num - 1;
    //    for (Convex.iterator it = c.begin(); c.end() != it; i--, ++it)
    //    {
    //        Vector2f p = *it;
    //        x[i] = p.x * oo_ssize - basex; z[i] = p.y * oo_ssize - basez;
    //    }

    //    bool ret = MakeScanlines(num, min, max, x, z, LeftBound, RightBound, sizex, sizez);
    //    //DEBUG::Message("Make Time =%d \n",LocalClock(0)-Time);
    //    return ret;
    //}
}
public static partial class Engine //RObjPipe
{
    public static IRObjPipe sRObjPipe = null;
    public static IRObjPipe sRObjEffectsPipe = null;

    public static bool sApplyRObjLayer = true;
}
public static partial class Engine //EngineWaterGround
{
    //VBPipe* Engine::DLightPipe=0;
    static Texture2D LightTexture = null;
    static Texture2D DecalTexture = null;
    static Texture2D ShadowTexture = null;
    public static Texture2D LaserTexture = null;
}
public static partial class Engine //const
{
    public const int MAX_LIGHTS = 1024;
    public const int MAX_ALIGHTS = 8;
}

public static partial class Engine //variables
{
    #region SceneDraw
    public static int UseD3DPipe = 1;
    public static int DynamicLighting = 1;
    public static int DrawBill = 1;
    public static bool scene_z_split = false;
    #endregion SceneDraw


    public static LIGHT[] AllLightsEx = new LIGHT[4096];


    //  temp section
    static float SkyOow;

    // stdmacros
    //pRS Engine::rs_default,
    //             Engine::rs_meo1,
    //             Engine::rs_meo2,
    //             Engine::rs_d3d1,
    //             Engine::rs_d3d2,
    //             Engine::rs_part1,
    //             Engine::rs_part2,
    //             Engine::rs_part3,
    //             Engine::decal_mainRS,
    //             Engine::light_mainRS,
    //             Engine::shadow_mainRS,
    //             Engine::laser_rs; 

    public static LightManager light_pipe;
    static D3DMATERIAL7 shadow_material;
    static D3DMATERIAL7 decal_material;
    static D3DMATERIAL7 light_material;
    static D3DMATERIAL7 fpo_def_material;
    static D3DMATERIAL7 fpo_defglass_material;
}

public static partial class Engine //LightStack
{
    static lstacktype l_stack = new lstacktype();

    public static LIGHT[] GetLights(out int num)
    {
        Assert.IsTrue(l_stack.Count != 0);
        LStacker stacker = l_stack.Peek();

        num = stacker.num;
        return stacker.lights;
    }

    static void InitLightStack()
    {
        Assert.IsTrue(l_stack.Count == 0);
    }
    static void DoneLightStack()
    {
        Assert.IsTrue(l_stack.Count == 0);
    }

    public static void PushLights(LIGHT[] lights, int num)
    {
        l_stack.Push(new LStacker(lights, num));
    }

    public static void PopLights()
    {
        Assert.IsTrue(l_stack.Count != 0);
        l_stack.Pop();
    }

    public class LStacker
    {
        public LIGHT[] lights;
        public int num;
        public LStacker() { }
        ~LStacker() { }
        public LStacker(LIGHT[] _lights, int _num)
        {
            lights = _lights;
            num = _num;
        }
    };
}



public static partial class Engine
{
    #region unity addon
    public static Camera UnityCamera = Camera.main;
    public static readonly Vector3 FarFarAway = new Vector3(0, -4000, 0);// Место "за картой" для размещения всякого при инициализации
    public static Dictionary<uint, GameObject> UnitCache = new Dictionary<uint, GameObject>();
    //private static GameObject templateHolder = new GameObject("Templates holder");
    #endregion
    static int current_frame;
    /// <summary>
    /// В исходниках - просто Camera
    /// </summary>
    public static CAMERA EngineCamera = new CAMERA(Vector3.zero);
    static float aspect;
    public static Scene scene;

    public static int HashMax, HashMin;
    public static int[] HashAreaX0, HashAreaX1;

    //private static TerrainHolder th;

    private static int mProjectionPersp = 0;
    private static Matrix4f mProjTransform;
    internal static void SetProjectionPersp(int wa, float ha, float mind, float maxd)
    {
        //mProjectionPersp = true ? 1:0;
        //mProjTransform = CreatePerspectiveProjection(wa, ha, mind, maxd);
        //mPerspectiveVolume = PerspectiveVolume::PerspectiveVolume(wa, ha, mind, maxd);

        //d3d.SetTransform(D3DTRANSFORMSTATE_PROJECTION, (D3DMATRIX*)&mProjTransform);
    }





    //settings
    //TODO Реализовать загрузку из конфига рендерера
    static bool write_log;
    static bool draw_ground_features = true;
    public static bool draw_terrain = true;
    public static bool draw_water = true;
    public static bool draw_sky;
    static bool draw_objects;
    static bool draw_particles = true;

    //  temp section
    static bool HashVisible = true;

    /// <summary>
    /// Отрисовка земли и декалей на ней (включая дороги)
    /// </summary>
    public static void DrawGround()
    {
        scene.objects.terrain.DrawGround();

        if (!draw_ground_features) return;

        RasterizeData raster = GetRasterizeData();
        EDrawDecals dr = new EDrawDecals();
        int drawn;
        if (HashVisible)
        {
            drawn = scene.objects.hasher.EnumPoly(raster, ROObjectId(RoFlags.ROFID_DECAL), dr);
            //EngineDebug.DebugConsole("Decals drawn:" + drawn);
        }
    }


    public static void DrawWater()
    {
        scene.objects.terrain.DrawWater();



    }

    public static void DrawObjects(bool Effects)
    {
        bool mDrawHash = true;
        bool mDrawList = true;

        if (mDrawHash)
            DrawHash(Effects);

        if (mDrawList)
            DrawList(Effects);

        //if (Effects)
        //{
        //    Engine::GetRObjEffectsPipe()->Flush();
        //}
        //else
        //{
        //    Engine::GetRObjPipe()->Flush();
        //}
    }



    public static void DrawList(bool effects)
    {
        //Глобальное освещение уже установлено.
        //SetGlobalLight(scene.objects.g_light.gl_light); 


        for (var p = scene.objects.objects_list.Head(); p != null; p = scene.objects.objects_list.Next(p))
        {

            IHashObject r = p.myobject;
            if (r.MatchFlags(ROObjectId(ROFID_PARTICLE)))
            {
                if (effects && draw_particles)
                {
                    //Debug.Log("Drawing r as particle" + r);
                    drawParticle((RO)r);
                }
                else if (r.MatchFlags(ROObjectId(ROFID_FPO)))
                {
                    //Debug.Log("Drawing r as fpo" + r);
                    drawFPO((RO)r, effects);
                }
            }
            else if (!effects && p.myobject.MatchGroup(OF_GROUP_TMT))
            {
                //Debug.Log("Drawing r as TMT: " + r + " " + p.myobject);
                drawTMT(p.myobject);
            }
        }
    }

    private static void drawTMT(IHashObject myobject)
    {
        //LightAdder la(Engine::light_pipe, Engine::AllLightsEx );
        //if (Engine::DynamicLighting)
        //{
        //    Sphere s(object->tm.pos, object->HashRadius());
        //    scene->objects.hasher->EnumSphere(s, ROObjectId(ROFID_LIGHT), &la);
        //}
        //PushLights(Engine::AllLightsEx, la.numlights);
        DrawTMT((TmTree)myobject);
        //PopLights();
    }

    private static void DrawTMT(TmTree myobject)
    {
        PushWorldTransform(myobject.tm);

        do
        {
            if (myobject.MatchFlags(TMTObjectId(TmTree.TMTID_FPO)))
            {
                StormUnityRenderer.DrawFpo((Fpo)myobject);
                //DrawFpo((Fpo)myobject);
                break;
            }
        } while (false);

        PopWorldTransform();
    }



    //private static void DrawFpo(Fpo myobject)
    //{
    //    if (!Engine.draw_objects) return;

    //    DrawFpoNode(myobject);

    //    for (var i = myobject.Begin(); i != null; i = i.Next())
    //        DrawTMT(i);
    //    //DrawTMT(i.Node());
    //}

    //private static void DrawFpoNode(Fpo myobject)
    //{

    //    IRData prd = myobject.r_images[myobject.image];

    //    if (prd != null)
    //    {
    //        IDrawable image = (IDrawable)prd.Query(IDrawable.ID);
    //        if (image != null)
    //        {
    //            //DrawFpoImage(image);
    //            image.Release();
    //        }
    //    }
    //}

    //public static void DrawFpoImage(IDrawable image)
    //{
    //    //PushBoundedLights(sph); 

    //    IRenderGroup rgroup = image.CreateRenderGroup();
    //    if (rgroup != null)
    //    {
    //        int n_groups = rgroup.GetNumGroups();
    //        for (int g = 0; g < n_groups; ++g)
    //        {
    //            IRenderable renderable = rgroup.GetRenderable(g);
    //            IShader shader = rgroup->GetShader(g);
    //            if (shader)
    //            {
    //                if (renderable)
    //                {
    //                    IShader::Desc desc = shader->GetDesc();
    //                    IRObjPipe* pipe = desc.IsSolid() ?
    //                      Engine::GetRObjPipe() :
    //                    Engine::GetRObjEffectsPipe();
    //                    for (int l = 0; l != desc.NumLayers(); ++l)
    //                    {
    //                        ILayer* layer = shader->GetLayer(l);
    //                        Asserts.AssertBp(layer);
    //                        pipe->Add(RObj(l, layer, renderable));
    //                        layer->Release();
    //                    }
    //                    renderable->Release();
    //                }
    //                shader->Release();
    //            }
    //        }
    //        rgroup.Release();
    //    }
    //    //PopLights();
    //}


    static void DrawPARTNode(PARTICLE_SYSTEM part)
    {
        //Matrix34f m;
        //Matrix34f wv = Engine::GetWorldViewTransform();

        //wv.GetReciprocal(m);
        //CAMERA NewCam; *(MATRIX*)&NewCam = MathConvert::ToLocus(m);

        //d3d.Validate("DrawPARTNode");
        //ClipStatus clip_status = GetClipStatus();
        //if (clip_status.ClipIntersection)
        //    part->EDrawMainPartial(0, 0, NewCam);
        //else
        //    part->EDrawMainFull(0, 0, NewCam);

        part.EDrawMainFull(0, 0, null);
        StormUnityRenderer.DrawParticle(part);
    }
    public static void drawParticle(RO r)
    {
        r.SetFlag(ROF_DRAWED);
        //StormUnityRenderer.DrawParticle(r);
        DrawPARTNode((PARTICLE_SYSTEM)r);
    }





    static void drawFPO(RO r, bool effects)
    {
        //LightAdder la(Engine::light_pipe, Engine::AllLightsEx );
        //if (Engine::DynamicLighting && scene->objects.hasher)
        //{
        //    Sphere s(r->Org, r->HashRadius);
        //    scene->objects.hasher->EnumSphere(s, ROObjectId(ROFID_LIGHT), &la);
        //}
        //PushLights(Engine::AllLightsEx, la.numlights);

        Debug.Log("Drawing FPO" + r);
        drawRO(r, effects);

        //PopLights();
    }

    static void drawRO(RO r, bool effects)
    {
        Debug.Log("Drawing RO" + r);
        r.SetFlag(ROF_DRAWED);
        if (r.Parent != null)
        {
            //MATRIX m = MWorld;
            //r->Parent->MatrixToWorld(m);
            //PushWorldTransform(MathConvert::FromLocus(m));
        }
        DrawROEx(r, null, 0, effects ? 1 : 0);
        if (r.Parent != null)
        {
            //    PopWorldTransform();
        }
    }

    public static void DrawROEx(RO r, LIGHT[] dlights, int ndlights, int Eff)
    {
        d3d.Validate("DrawROEx");
        //Debug.Log(string.Format("Drawing DrawROEx RO {0} lights {1} Eff {2}",r,ndlights,Eff));
        StormUnityRenderer.DrawFPO((FPO)r);

        //DrawRONode(r, dlights, ndlights, Eff);



        //if ((Eff==0) || (Eff!=0 && r.GetFlag(ROF_ST_TANSP)!=0))
        //{
        //    for (RO o = r.SubObjects; o!=null; o = o.Next)
        //        if (o.GetFlag(ROF_NONHASH_OBJECT)==0)
        //            DrawROEx(o, dlights, ndlights, Eff);
        //}
    }

    /// <summary>
    /// В оригинале - функция "вырезает" область карты, попадающей в поле зрения камеры
    /// </summary>
    /// <param name="BaseX"></param>
    /// <param name="BaseZ"></param>
    /// <param name="ylow"></param>
    /// <param name="yhi"></param>
    /// <param name="bmin"></param>
    /// <param name="bmax"></param>
    /// <param name="Result0"></param>
    /// <param name="Result1"></param>
    /// <param name="ooboxsize"></param>
    /// <param name="SizeX"></param>
    /// <param name="SizeY"></param>
    /// <returns></returns>
    //internal static bool ClipArea(float BaseX, float BaseZ, int ylow, int yhi, out int bmin, out int bmax, out int[] Result0, out int[] Result1, float ooboxsize, int SizeX, int SizeY)
    //{
    //    List<Vector3> bounds = new List<Vector3>();
    //    float[] x = new float[32], y = new float[32];

    //    Vector3 dd = new Vector3(EngineCamera.Org.x - BaseX, 0, EngineCamera.Org.z - BaseZ);//-Camera.Dir*SQUARE_SIZE;
    //    bounds.Add(dd + Vector3.forward * UnityCamera.farClipPlane + Vector3.right * UnityCamera.farClipPlane); //Правый-верхний угол блока
    //    bounds.Add(dd + Vector3.forward * UnityCamera.farClipPlane - Vector3.right * UnityCamera.farClipPlane);//Левый-верхний угол блока
    //    bounds.Add(dd - Vector3.forward * UnityCamera.farClipPlane - Vector3.right * UnityCamera.farClipPlane);//Левый-нижний угол блока
    //    bounds.Add(dd - Vector3.forward * UnityCamera.farClipPlane + Vector3.right * UnityCamera.farClipPlane);//Правиый-нижний угол блока

    //    int num = bounds.Count;
    //    int i = num - 1;
    //    foreach (Vector3 p in bounds)
    //    {
    //        x[i] = p.x * ooboxsize; y[i] = p.y * ooboxsize;
    //    }

    //    bool ret = MakeScanlines(num, out bmin, out bmax, x, y, out Result0, out Result1, SizeX, SizeY);

    //    //TODO Реализовать корректно вырезание активной области из карты


    //    return ret;
    //}

    //internal static bool MakeScanlines(int n, out int min, out int max, float[] x, float[] y, out int[] Area0, out int[] Area1, int SizeX, int SizeY)
    //{
    //    float[] X = new float[n];
    //    float[] Y = new float[n];

    //    {
    //        for (int i = 0; i != n; ++i)
    //        {
    //            X[i] = x[n - 1 - i];
    //            Y[i] = y[n - 1 - i];
    //        }
    //    }

    //    Raster2D r = new Raster2D(X, Y, n);

    //    min = r.Start();
    //    int end = Mathf.Max(
    //        Mathf.Min(
    //            r.End() + 1,
    //            SizeY),
    //        0);

    //    for (; min < 0; ++min) r.Next();

    //    if (min >= end)
    //    {
    //        max = 0;
    //        Area0 = new int[0];
    //        Area1 = new int[0];
    //        return false;
    //    }

    //    max = end - 1;

    //    Area0 = new int[end];
    //    Area1 = new int[end];
    //    for (int i = min; i != end; ++i)
    //    {
    //        ScanLine s = r.GetScanline();

    //        Area0[i] = Mathf.Max(Mathf.Min(s.Item1, SizeX - 1), 0);
    //        Area1[i] = Mathf.Max(Mathf.Min(s.Item2, SizeX - 1), 0);

    //        r.Next();
    //    }

    //    return true;
    //}

    public static bool DumpHashTexture = false;
    public static void DrawHash(bool Effects)
    {
        if (!HashVisible) return;

        //SetFogState(GetFogStartDist(), GetFogEndDist(), Effects ? 0 : GetFogColor().PackRGB()); //Туман пока не нужен
        //SetGlobalLight(scene->objects.g_light->gl_light); //Глобальное освещение устанавливается глобально для сцены.

        RasterizeData r = GetRasterizeData();
        //VisualizeRasterizator(r);
        DrawFpoParticle dr = new DrawFpoParticle(Effects ? 1 : 0, scene.objects.hasher, 1, null);
        scene.objects.hasher.EnumPoly(r, OF_GROUP_TMT | OF_GROUP_RENDER | OF_USER_MASK, dr);

        if (DumpHashTexture)
        {
            Debug.Log("Dumping texture. Raster: " + r);
            DumpHashTexture = false;
            VisualizeEnumerator en = new VisualizeEnumerator(Effects ? 1 : 0, scene.objects.hasher, 1, null);
            scene.objects.hasher.EnumPoly(r, OF_GROUP_TMT | OF_GROUP_RENDER | OF_USER_MASK, en);
            en.SaveTexture(String.Format("EnumerationVisualization{0}x{1}.png", r.starty, r.nlines));
        }
    }

    private class VisualizeEnumerator : HashEnumer
    {
        int effects;   // 1 or 0 only
        IHash hasher;
        int layer;
        Texture2D HashTexture;
        //int textureWidth = 1920, textureHeight = 1080;
        const int textureWidth = 5376, textureHeight = 6016; //Это Continent
        const int terrainWidth = textureWidth * 64, terrainHeight = textureHeight * 64;

        //LightManager* light;
        public VisualizeEnumerator(int e, IHash hs, int lr, object l = null)
        {
            effects = e;
            hasher = hs;
            layer = lr;
            //light = l;
            HashTexture = prepTexture();
        }

        private Texture2D prepTexture()
        {
            Texture2D myTexture = new Texture2D(textureWidth, textureHeight);
            var pixels = myTexture.GetPixels();
            Color defaultColor = Color.black;
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = defaultColor;
            }
            myTexture.SetPixels(pixels);

            GlobalHasher myHasher = ((GlobalHasher)hasher);
            var myRect = myHasher.GetRect();
            Debug.Log("Rect: " + myRect + " of " + hasher);
            int xOffset = (int)EngineCamera.Org.x / 64;
            int yOffset = (int)EngineCamera.Org.z / 64;

            for (int i = myRect.y0; i < myRect.y1; i++)
            {
                myTexture.SetPixel(myRect.x0 + xOffset, i + yOffset, Color.green);
                myTexture.SetPixel(myRect.x1 + xOffset, i + yOffset, Color.green);
            }
            for (int i = myRect.x0; i < myRect.x1; i++)
            {
                myTexture.SetPixel(i + xOffset, myRect.y0 + yOffset, Color.green);
                myTexture.SetPixel(i + xOffset, myRect.y1 + yOffset, Color.green);
            }
            myTexture.Apply();
            return myTexture;
        }

        public bool ProcessElement(HMember p)
        {
            FPO myFPO = (FPO)p.Object();
            int posx = (int)(textureWidth * (myFPO.Org.x / terrainWidth));
            int posy = (int)(textureHeight * (myFPO.Org.z / terrainHeight));
            HashTexture.SetPixel(posx, posy, Color.yellow);
            //Debug.Log(string.Format("Dumping pixel {0}:{1} -> {2}:{3}", myFPO.Org.x, myFPO.Org.z, posx, posy));

            return true;
        }

        public void SaveTexture(string TextureName = "debug.png")
        {

            HashTexture.Apply();
            Debug.Log("Saving texture as  " + TextureName);
            File.WriteAllBytes(TextureName, HashTexture.EncodeToPNG());
        }
    }
    private static void VisualizeRasterizator(RasterizeData r)
    {
        if (!DumpHashTexture) return;
        DumpHashTexture = false;

        Debug.Log("Raster: " + r);
        //int width = 1920, height = 1080;
        //int terrainWidth = 400000, terrainHeight = 400000;
        //Texture2D texture = new Texture2D(width, height);
        //Debug.Log(string.Format("Dumping {0} items", hm.Count));
        //foreach (HMember item in hm)
        //{
        //    if (item.Object().GetFlag(RoFlags.ROFID_FPO) == 0) continue;
        //    FPO myFpo = (FPO)item.Object();
        //    int posx = (int)(width * (myFpo.Org.x / terrainWidth));
        //    int posy = (int)(height * (myFpo.Org.x / terrainHeight));
        //    texture.SetPixel(posx, posy, Color.yellow);
        //    Debug.Log(string.Format("Dumping pixel {0}:{1}", posx, posy));
        //}
        //texture.Apply();
        //File.WriteAllBytes("debug.jpg", texture.EncodeToJPG());
    }

    static RasterizeData GetRasterizeData()
    {
        //Debug.Log(scene.objects.protohash.GetRect());
        //return new RasterizeData(scene.objects.protohash.GetRect().y0 + HashMin,HashMax - HashMin + 1, HashAreaX0[] + HashMin, HashAreaX1[] + HashMin);
        return new RasterizeData(scene.objects.protohash.GetRect().y0 + HashMin, HashMax - HashMin + 1, HashAreaX0, HashAreaX1);
        //Debug.Log("Rasterizing at" + scene.objects.protohash.GetRect());
        // return new RasterizeData();
    }
    public static IDrawable CreateFpoImage(ObjId id)
    {
        //FpoGraphData data = StormRendererData.GetFpoGraph(id);
        FpoGraphData data = dll_data.GetFpoGraph(id);
        Debug.LogFormat("FpoGraphData {0} loaded: {1}", id, data == null ? "NOT" : "\n"+data);
        if (data == null)
            return null;
        mFpoImages.SetData(data);
        Debug.LogFormat("Creating via factory: {0}",id);
        IDrawable result = mFpoImages.CreateObject(id);
        return result;
    }



    public static void Init(IRendererConfig rc)
    {
        //TODO Реализовать загрузку из конфига рендерера
        write_log = true;
        draw_ground_features = true;
        draw_terrain = true;
        draw_water = true;
        draw_sky = true;
        draw_objects = true;
        draw_particles = true;
        scene_z_split = false; //из scenedraw.cpp

        //write_log = rc->GetWriteLog();

        //draw_ground_features = rc->GetDRAWGROUNDFEATURES();
        //draw_terrain = rc->GetDRAWTERRAIN();
        //draw_water = rc->GetDRAWWATER();
        //draw_sky = rc->GetDRAWSKY();
        //draw_objects = rc->GetDRAWOBJECTS();
        //draw_particles = rc->GetDRAWPARTICLES();
        //scene_z_split = rc->GetZSPLITSCENE();

        current_frame = 0;

        mFpoShaders = new FpoShaderFactory();
        mFpoImages = new FpoImageFactory();


        InitStack();

        //gl_spline = new GLSpline;

        //InitMeoPipe();
        //FpoPipe = d3d.vbmanager->CreateVBPipe(8192, D3DFVF_VERTEX, VBF_DRAWABLE);
        //WaterPipe = d3d.vbmanager->CreateVBPipe(2048, D3DFVF_XYZ | D3DFVF_NORMAL | D3DFVF_DIFFUSE | D3DFVF_TEX2, VBF_DRAWABLE);

        //DLightPipe = d3d.vbmanager->CreateVBPipe(1024, D3DFVF_VERTEX | D3DFVF_DIFFUSE, VBF_DRAWABLE);
        //TLPipe = d3d.vbmanager->CreateVBPipe(2048, D3DFVF_TLVERTEX, VBF_DRAWABLE);

        // dprintf( " Loading RenderStates...\n");
        rs_default = dll_data.CreateRS("default");
        rs_meo1 = dll_data.CreateRS("meo_solid");
        rs_meo2 = dll_data.CreateRS("meo_transparent");
        rs_d3d1 = dll_data.CreateRS("d3d_solid");
        rs_d3d2 = dll_data.CreateRS("d3d_transparent");
        rs_part1 = dll_data.CreateRS("paticle_atexture");
        rs_part2 = dll_data.CreateRS("paticle_adda");
        rs_part3 = dll_data.CreateRS("paticle_atextureinv");
        decal_mainRS = dll_data.CreateRS("decal_main");
        light_mainRS = dll_data.CreateRS("terrain_light");
        shadow_mainRS = dll_data.CreateRS("shadow_main");

        //laser_rs = dll_data.CreateRS("laser");

        // dprintf( " Loaded RenderStates...\n");

        shadow_material = dll_data.LoadMaterial("GroundShadow");
        decal_material = dll_data.LoadMaterial("GroundDecal");
        light_material = dll_data.LoadMaterial("GroundLight");
        fpo_def_material = dll_data.LoadMaterial("FPO_Material");
        fpo_defglass_material = dll_data.LoadMaterial("FPOGlass_Material");



        LightTexture = dll_data.LoadTexture("shade");
        DecalTexture = dll_data.LoadTexture("vfx1");
        ShadowTexture = dll_data.LoadTexture("shade");
        LaserTexture = dll_data.LoadTexture("laser");

        // SmallMap.Init();

        int ml = MAX_LIGHTS;
        int mal = d3d.MaxLights(); mal = mal > MAX_ALIGHTS ? MAX_ALIGHTS : mal < 0 ? MAX_ALIGHTS : mal;

        //TODO - как-то неправильно создавать так.
        light_pipe = new LightManager();
        light_pipe.Create(ml, mal);

        sRObjPipe = new RObjPipe();
        sRObjEffectsPipe = new RObjPipe();

        //DEBUG_MSG(" Initing Rendering Engine Stage finished...\n");
        Debug.Log(" Initing Rendering Engine Stage finished...\n");
    }


    public static void InitMissionScene(Scene _scene, float start_dist, float end_dist, float near_z, float far_z)
    {
        current_frame++;

        scene = _scene;
        aspect = scene.aspect;

        //Camera.Init(scene.camera, scene.bounds, near_z, far_z, aspect, start_dist, end_dist, scene.objects.g_light.dir);
        EngineCamera.Init(scene.SceneCamera, scene.bounds, near_z, far_z, aspect, start_dist, end_dist, Vector3.down);
        //HashVisible = scene.objects.protohash !=null? ClipHash(scene.objects.protohash) : false;
        HashVisible = scene.objects.protohash != null ? true : false;
        //Остальное отбрасываем или устанавливаем для Camera.main
    }

    public static void DoneMissionScene()
    {
        //DoneStack();

        scene = null;
        //light_pipe.EndFrame();
        //DoneLightStack();

        StormUnityRenderer.GameObjectsCleanup();
    }

    private static bool ClipHash(ProtoHash h)
    {
        bool res = ClipArea(h.GetBaseX(), h.GetBaseY(),
          10000, 10000, out HashMin, out HashMax, ref HashAreaX0, ref HashAreaX1,
          h.GetOOSquareSize(), h.GetSizeX(), h.GetSizeY());
        if (res)
        {
            Assert.IsTrue(HashMin >= 0);
            Assert.IsTrue(HashMax < h.GetSizeY());
            int add = h.GetRect().x0;
            for (int i = HashMin; i <= HashMax; ++i)
            {
                HashAreaX0[i] += add;
                HashAreaX1[i] += add;
            }
        }
        return res;
    }

    static Vector3 vn0, vn1, vn2, vn3,
                    vf0, vf1, vf2, vf3;


    private static void CalcualeClippingPoints(float BaseX, float BaseZ)
    {
        Vector3 rd, ud, dd, tmp;
        rd = Vector3.zero;//Camera.Right*SQUARE_SIZE;
        ud = Vector3.zero;//Camera.Up   *SQUARE_SIZE;
        dd = new Vector3(EngineCamera.Org.x - BaseX, 0, EngineCamera.Org.z - BaseZ);//-Camera.Dir*SQUARE_SIZE;

        //tmp = Vector3.zero;
        tmp = Vector3.one;
        //tmp.z = .5 / CAMERA::Aspect;
        //tmp.x = CAMERA::ViewPort.hres * tmp.z;
        //tmp.y = CAMERA::ViewPort.vres * tmp.z;
        /*
        RDEBUG Rd("RENDERER::CalcualeClippingPoints");
        TRACEVAR(CAMERA::Aspect, %f);
        TRACEVAR(CAMERA::ViewPort.hres, %f);
        TRACEVAR(CAMERA::ViewPort.vres, %f);*/
        tmp.z = 1;
        //tmp *= CAMERA::end_dist;//+SQUARE_SIZE;
        tmp *= UnityCamera.farClipPlane; //TODO читать из конфига, а не самой камеры.

        vn0 = dd - rd - ud; vf0 = vn0 + (EngineCamera.Dir * tmp.z - EngineCamera.Right * tmp.x - EngineCamera.Up * tmp.y);
        vn1 = dd - rd + ud; vf1 = vn1 + (EngineCamera.Dir * tmp.z - EngineCamera.Right * tmp.x + EngineCamera.Up * tmp.y);
        vn2 = dd + rd + ud; vf2 = vn2 + (EngineCamera.Dir * tmp.z + EngineCamera.Right * tmp.x + EngineCamera.Up * tmp.y);
        vn3 = dd + rd - ud; vf3 = vn3 + (EngineCamera.Dir * tmp.z + EngineCamera.Right * tmp.x - EngineCamera.Up * tmp.y);
    }

    //private bool MakeScanlines(int n, out int min, out int max, float[] x, float[] y, int[] Area0, int[] Area1, int SizeX, int SizeY)
    //{

    //    float[] X = new float[n];
    //    float[] Y = new float[n];
    //    {
    //        for (int i = 0; i != n; ++i)
    //        {
    //            X[i] = x[n - 1 - i];
    //            Y[i] = y[n - 1 - i];
    //        }
    //    }

    //    Raster2D r(X, Y, n );

    //    min = r.Start();

    //    int end = std::_MAX(std::_MIN(r.End() + 1, SizeY), 0);

    //    for (min; min < 0; ++min) r.Next();

    //    if (min >= end)
    //        return false;

    //    max = end - 1;

    //    for (int i = min; i != end; ++i)
    //    {
    //        ScanLine s = r.GetScanline();

    //        Area0[i] = std::_MAX(std::_MIN(s.first, SizeX - 1), 0);
    //        Area1[i] = std::_MAX(std::_MIN(s.second, SizeX - 1), 0);

    //        r.Next();
    //    }

    //    return true;
    //}

    /// <summary>
    /// Вариация-заглушка для Unity. Реальная функция - в Clip.cpp
    /// </summary>
    /// <param name="n"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="Area0">Массив "левых" границ области</param>
    /// <param name="Area1">Массив "правых" границ области</param>
    /// <param name="SizeX"></param>
    /// <param name="SizeY"></param>
    /// <returns></returns>
    private static bool MakeScanlines(int n, out int min, out int max, float[] x, float[] y, ref int[] Area0, ref int[] Area1, int SizeX, int SizeY)
    {

        min = 0;
        max = n - 1;

        for (int i = min; i != n; ++i)
        {
            Area0[i] = 0;
            Area1[i] = i;
            //Debug.Log(i);
        }

        return true;
    }
    public static bool ClipArea(float BaseX, float BaseZ, float ylow, float yhi, out int bmin, out int bmax, ref int[] Result0, ref int[] Result1, float ooboxsize, int SizeX, int SizeY)
    {
        bmin = 0;
        bmax = 0;
        //Debug.LogFormat("BaseX: {0} BaseZ: {1} yhi/lo {2}/{3}",BaseX,BaseZ,ylow,yhi);

        float[] x = new float[32], y = new float[32];

        int num = 32;
        for (int i = 0; i < 32; i++)
        {
            x[i] = i;
            y[i] = i;
        }

        bool ret = MakeScanlines(num, out bmin, out bmax, x, y, ref Result0, ref Result1, SizeX, SizeY);
        return ret;
    }

    public static bool ClipAreaDebug(float BaseX, float BaseZ, float ylow, float yhi, out int bmin, out int bmax, ref int[] Result0, ref int[] Result1, float ooboxsize, int SizeX, int SizeY)
    {
        bmin = 0; bmax = 0;
        Debug.Log(string.Format("ClipArea BaseX {0} BaseZ {1}", BaseX, BaseZ));
        CalcualeClippingPoints(BaseX, BaseZ);
        CPOINT[] CPoints = new CPOINT[8 * 8];
        int[] CNext = new int[8 * 8];
        int[] CPrev = new int[8 * 8];

        CPointsList TopList = new CPointsList(CPoints, CNext, CPrev);
        CPointsList BottomList = new CPointsList(CPoints, CNext, CPrev, 8);
        CPointsList RightList = new CPointsList(CPoints, CNext, CPrev, 16);
        CPointsList LeftList = new CPointsList(CPoints, CNext, CPrev, 24);
        CPointsList l = new CPointsList(CPoints, CNext, CPrev, 32);

        float[] x = new float[32], y = new float[32];

        // left list
        AddVectorToList(LeftList, vn0);
        AddVectorToList(LeftList, vf0);
        AddVectorToList(LeftList, vf1);
        AddVectorToList(LeftList, vn1);
        // top list
        AddVectorToList(TopList, vn1);
        AddVectorToList(TopList, vf1);
        AddVectorToList(TopList, vf2);
        AddVectorToList(TopList, vn2);
        // right list
        AddVectorToList(RightList, vn2);
        AddVectorToList(RightList, vf2);
        AddVectorToList(RightList, vf3);
        AddVectorToList(RightList, vn3);
        // bottom list
        AddVectorToList(BottomList, vn3);
        AddVectorToList(BottomList, vf3);
        AddVectorToList(BottomList, vf0);
        AddVectorToList(BottomList, vn0);

        // clipping all planes with max plane

        //int Time=LocalClock(0);
        ClipListWithPlane(LeftList, 0, -1, 0, yhi);
        ClipListWithPlane(TopList, 0, -1, 0, yhi);
        ClipListWithPlane(RightList, 0, -1, 0, yhi);
        ClipListWithPlane(BottomList, 0, -1, 0, yhi);

        // clipping all planes with min plane
        ClipListWithPlane(LeftList, 0, 1, 0, ylow);
        ClipListWithPlane(TopList, 0, 1, 0, ylow);
        ClipListWithPlane(RightList, 0, 1, 0, ylow);
        ClipListWithPlane(BottomList, 0, 1, 0, ylow);
        //DEBUG::Message("Clip Time =%d \n",LocalClock(0)-Time);

        Convex c = new Convex();

        AddVectorToList(l, vn0);
        AddVectorToList(l, vf0);
        AddVectorToList(l, vf1);
        AddVectorToList(l, vf2);
        AddVectorToList(l, vf3);

        MakeConvexList(c, l);

        int num = c.size();
        if (num < 3)
        {
            Asserts.AssertBp(0);
            return false;
        }

        return true;
    }

    internal static Vector3 GetZeroPosition()
    {
        return UnityCamera.gameObject.transform.position - EngineCamera.Org;
    }
    internal static Vector3 ToCameraReference(Vector3 coordinates)
    {
        //return coordinates - UnityCamera.gameObject.transform.position;
        //return /*UnityCamera.gameObject.transform.position -*/ coordinates;
        //return UnityCamera.gameObject.transform.position + coordinates;

        //return UnityCamera.gameObject.transform.position + coordinates - EngineCamera.Org;
        //if (scene == null) return Vector3.zero;
        //return UnityCamera.gameObject.transform.position + coordinates - scene.StormCamera.Org;

        return GetZeroPosition() + coordinates;
    }
}
class FpoShaderFactory : IndexFactory<FpoShaderId, FpoShader>
{
    public void SetData(FpoShaderId data)
    {
        mData = data.mMaterialData;
    }
    public override bool InitializeObject(FpoShader myObject)
    {
        return myObject.Initialize(mData);
    }
    public override void DestroyObject(FpoShader myObject)
    {
        myObject.Destroy();
        myObject.Dispose();
    }
    protected MaterialData mData;
};

class FpoImageFactory : IndexFactory<DWORD, FpoImage>, IDisposable
{
    public void SetData(FpoGraphData data)
    {
        mData = data;
    }
    public override bool InitializeObject(FpoImage myObject)
    {
        Debug.LogFormat("Trying to initialize object {0} using data {1}",myObject,mData);
        //return base.InitializeObject(myObject);
        //return myObject.Initialize(mGroupPool, mNoClipPool, mClipPool, mData);
        return myObject.Initialize(null, null, null, mData);
    }
    public override void DestroyObject(FpoImage myObject)
    {
        myObject.Destroy();
        base.DestroyObject(myObject); //TODO сомнительное. Нужно ли вызывать метод освобождления FpoImage ещё раз?
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    protected FpoGraphData mData;
    //NotePool<FpoRenderGroup> mGroupPool;
    //NotePool<FpoRenderableNoClip> mNoClipPool;
    //NotePool<FpoRenderableClip> mClipPool;

};

//TODO! Реализовать TWrapObject!
/// <summary>
/// В оригинале - наследуется от I
/// Таким образом, получается унаследованный от I объект, к которому добавляется ещё экземпляр N
/// </summary>
/// <typeparam name="N"></typeparam>
/// <typeparam name="I"></typeparam>
public class TWrapObject<N, I> where N : IObject
{
    I obj2;


    public TWrapObject(N myObject)
    {
        mObject = myObject;
    }

    public void AddRef()
    {
        mObject.AddRef();
    }

    public int Release()
    {
        return mObject.Release();
    }

    public object Query(uint cls_id)
    {
        return mObject.Query(cls_id);
    }
    protected N mObject;
}
public struct EngineVisualMesh
{
    public Mesh myMesh;
    public Material[] materials;
}

public class PSShowInEditor : MonoBehaviour
{
    public bool alive;
    public bool isPlaying;
    public bool isStopped;
}


//public class Raster2D
//{

//    public Raster2D(float[] x, float[] y, int n)
//    {
//        m_X = x;
//        m_Y = y;
//        m_N = n;

//        (int, int) min_r = FindExtremumFloatIndex(y, n, "less");
//        //m_minLeft = min_r.Item1 - m_Y[0];
//        //m_minRight = min_r.Item2 - m_Y[0];
//        m_minLeft = min_r.Item1;
//        m_minRight = min_r.Item2;

//        (int, int) max_r = FindExtremumFloatIndex(y, n, "greater");
//        //m_maxLeft = max_r.Item1 - m_Y[0];
//        //m_maxRight = max_r.Item2 - m_Y[0];
//        m_maxLeft = max_r.Item1;
//        m_maxRight = max_r.Item2 ;

//        m_Contour = RasterContour<ElemItB, ElemItF>(
//          ElemItB(m_X, m_Y, n, m_minLeft), ElemItB(m_X, m_Y, n, m_maxLeft),
//          ElemItF(m_X, m_Y, n, m_minRight), ElemItF(m_X, m_Y, n, m_maxRight));
//    }
//    (float, float) FindExtremumFloat(float[] range, int lastindex, string pr)
//    {

//        (int, int) index = FindExtremumFloatIndex(range, lastindex, pr);
//        /*if (index.Item1 < range.Length - 1 && index.Item2 < range.Length -1)*/ 
//        return (range[index.Item1], range[index.Item2]);

//    }

//    (int, int) FindExtremumFloatIndex(float[] range, int lastindex, string pr)
//    {
//        int res = 0;
//        for (int i = 0; i < lastindex; i++)
//        {
//            switch (pr)
//            {
//                case "less":
//                    if (range[i] < range[res]) res = i;
//                    break;
//                case "greater":
//                    if (range[i] > range[res]) res = i;
//                    break;
//                default:
//                    break;
//            }
//        }
//        return (res, res);
//    }

//    public int Start()
//    {
//        return m_Contour.GetY();
//    }

//    public int End()
//    {
//        return m_Contour.GetEndY();
//    }

//    public (int, int) GetScanline()
//    {
//        return *m_Contour;
//    }

//    public void Next()
//    {
//        ++m_Contour;
//    }

//    protected float[] m_X;
//    protected float[] m_Y;
//    protected int m_N;

//    int m_minLeft;
//    int m_minRight;
//    int m_maxLeft;
//    int m_maxRight;

//    RasterContour<ElemItB, ElemItF> m_Contour;
//};

public static partial class Engine //EngineStack
{
    static int mWorldTransformApplied = 0;
    static Matrix34f mViewTransform;
    static Vector3 mViewPosition;
    internal static void InitStack()
    {
        //AssertBp(world_transform.IsEmpty());
        //AssertBp(world_transform_reciprocal.IsEmpty());
        mWorldTransformApplied = 0;

        //AssertBp(mClipStack.IsEmpty());
    }

    private static Matrix4f CreatePerspectiveProjection(float wa, float ha, float mind, float maxd)
    {
        float Q = maxd / (maxd - mind);
        Matrix4f projection = new Matrix4f();
        projection.Zero();
        Vector4 tmp = projection[0];
        tmp[0] = wa;
        projection[0] = tmp;
        tmp = projection[1];
        tmp[1] = ha;
        projection[1] = tmp;
        tmp = projection[2];
        tmp[2] = Q;
        tmp[3] = 1;
        projection[2] = tmp;
        tmp = projection[3];
        tmp[2] = -Q * mind;
        projection[3] = tmp;

        return projection;
    }

    public static void DoneStack()
    {
        //AssertBp(mClipStack.IsEmpty());

        //AssertBp(world_transform.IsEmpty());
        //AssertBp(world_transform_reciprocal.IsEmpty());
    }
    public static ClipStatus SphereVisible(Sphere sphere)
    {
        ClipStatus cs = new ClipStatus(0, (uint)ClipStatusFlags.CS_FRUSTRUM);
        if (mProjectionPersp != 0)
        {
            Debug.LogError("mProjectionPersp not implemented yet");
            return cs;
        }
        //return mPerspectiveVolume.CheckSphereVisibility(sphere);
        else
            return cs;
    }

    /// <summary>
    /// Функция временная.
    /// </summary>
    /// <param name="CamLocation"></param>
    public static void SetView(Vector3 CamLocation)
    {
        mViewPosition = CamLocation;
    }
    public static void SetView(ref Matrix34f CamLocation)
    {
        //AssertBp(world_transform.IsEmpty());
        //  AssertBp(world_transform_reciprocal.IsEmpty());

        CamLocation.tm.GetTranspose(ref mViewTransform.tm);
        mViewPosition = CamLocation.pos;
        mViewTransform.pos = Vector3.zero;

        Matrix4f m = new Matrix4f(); mViewTransform.GetMatrix4f(ref m);
        //d3d.SetTransform(D3DTRANSFORMSTATE_VIEW, (D3DMATRIX*)&m );

        //NotifyWorldViewChange();
    }

    public static Vector3 WorldToEngineWorld(Vector3 p)
    {
        return p - mViewPosition;
    }

    static Stack<Matrix34f> world_transform = new Stack<Matrix34f>();
    static Stack<Matrix34f> world_transform_reciprocal = new Stack<Matrix34f>();
    public static void PushWorldTransform(Matrix34f tr)
    {
        world_transform.Push(world_transform.Count == 0 ? new Matrix34f(tr.tm, tr.pos - mViewPosition) : tr);

        NotifyWorldViewChange();
        //mWorldTransformApplied = false;
        mWorldTransformApplied = 0;
    }

    public static void PopWorldTransform()
    {
        world_transform.Pop();
        NotifyWorldViewChange();
        //mWorldTransformApplied = false;
        mWorldTransformApplied = 0;
    }

    static Matrix34f LastAppliedTransform;

    public static void ApplyWorldTransform()
    {
        Matrix4f m = new Matrix4f();
        if (mWorldTransformApplied == 0)
        {

            (
#if _DEBUG
      LastAppliedTransform=
#endif
              GetWorldTransform()).GetMatrix4f(ref m);
            //d3d.SetTransform(D3DTRANSFORMSTATE_WORLD, (D3DMATRIX*)&m);
            mWorldTransformApplied = 1;// true;
        }
    }

    public static Matrix34f GetWorldTransform()
    {
        Matrix34f m = new Matrix34f();
        if (world_transform.Count == 0)
        {
            m.Identity();
            m.pos = -mViewPosition;
        }
        else
            m = world_transform.Peek();
        return m;
    }

    public static void ApplyWorldTransform(Matrix34f tm)
    {
        Matrix4f m = new Matrix4f();
        tm.GetMatrix4f(ref m);
        //d3d.SetTransform(D3DTRANSFORMSTATE_WORLD, (D3DMATRIX*)&m);
        //mWorldTransformApplied = false;
        mWorldTransformApplied = 0;
    }

    static bool mWorldViewValid = false;
    static void NotifyWorldViewChange()
    {
        mWorldViewValid = false;
    }
}

public class CPOINT
{
    public Vector3 Org;
    public float d;
    public CPOINT() { }
    public CPOINT(Vector3 O, float dd)
    {
        Org = O;
        d = dd;
    }

    public override string ToString()
    {
        return "CPOINT Org:" + Org + " d:" + d;
    }
};

public static class Clip
{
    //public static bool MakeScanlines(int n, out int min, out int max, float[] x, float[] y, int[] Area0, int[] Area1, int SizeX, int SizeY)
    //{

    //    float[] X = new float[n];
    //    float[] Y = new float[n];
    //    {
    //        for (int i = 0; i != n; ++i)
    //        {
    //            X[i] = x[n - 1 - i];
    //            Y[i] = y[n - 1 - i];
    //        }
    //    }

    //    Raster2D r = new Raster2D(X, Y, n );

    //    min = r.Start();

    //    int end = std::_MAX(std::_MIN(r.End() + 1, SizeY), 0);

    //    for (min; min < 0; ++min) r.Next();

    //    if (min >= end)
    //        return false;

    //    max = end - 1;

    //    for (int i = min; i != end; ++i)
    //    {
    //        ScanLine s = r.GetScanline();

    //        Area0[i] = std::_MAX(std::_MIN(s.first, SizeX - 1), 0);
    //        Area1[i] = std::_MAX(std::_MIN(s.second, SizeX - 1), 0);

    //        r.Next();
    //    }

    //    return true;
    //}
    public static void MakeConvexList(Convex convex, CPointsList l)
    {
        int p0, p1;
        for (p0 = l.Head(); l.Counter() != 0; p0 = p1)
        {
            p1 = l.NextElemCicled(p0);

            CPOINT pt = l.Data[l.Extract(p0)];
            convex.insert(new Vector2(pt.Org.x, pt.Org.z));

            p0 = p1;
        }
    }
    public static void AddVectorToList(CPointsList l, Vector3 v)
    {
        l.AddTail(new CPOINT(v, 0));
    }

    public static CPOINT ClipCPoint(CPOINT pp, CPOINT pm)
    {
        return new CPOINT((pm.Org * pp.d - pp.Org * pm.d) / (pp.d - pm.d), 0);
    }
    public static void ClipListWithPlane(CPointsList l, float A, float B, float C, float D)
    {
        int p, ppw, pmw, pp = -1, pm = -1;

        // getting distances and searching for "-" point
        for (p = l.Head(); ; p = l.NextElem(p))
        {
            l.Data[p].d = l.Data[p].Org.x * A + l.Data[p].Org.y * B + l.Data[p].Org.z * C + D;
            if (l.Data[p].d < 0) pm = p; else pp = p;
            if (p == l.Tail()) break;
        }

        if (pm == -1) return;      // all points are in front, nothing to do
        if (pp == -1)
        {            // all points are behind
            l.Empty();           // remove all points
            return;
        }

        // now we must set pm and pp this way:
        // +++----------++
        // pm^^pmw   ppw^^pp

        for (pp = pm; l.Data[pp].d < 0; pp = l.NextElemCicled(pp)) { }
        ;
        ppw = l.PrevElemCicled(pp);
        for (; l.Data[pm].d < 0; pm = l.PrevElemCicled(pm)) { }
        ;
        pmw = l.NextElemCicled(pm);

        // creating CPOINT between [pm;pm+1]
        if (l.Data[pm].d > 0)
            p = l.AddAfter(pm, ClipCPoint(l.Data[pm], l.Data[pmw]));

        // creating CPOINT between [pp;pp-1]
        if (l.Data[pp].d > 0)
            l.AddBefore(pp, ClipCPoint(l.Data[pp], l.Data[ppw]));

        // now list looks this way:
        // +++0-------0+++
        //  pm^     pp^

        // deleting all points in ]pm;pp[
        for (p = l.NextElemCicled(p); p != pp;)
        {
            ppw = l.NextElemCicled(p);
            l.Extract(p);
            p = ppw;
        }
    }
}
