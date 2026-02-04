using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering.HighDefinition;
using static MaterialStorage;
using static renderer_dll;
using static RoFlags;
using static UnityEngine.ParticleSystem;
using DWORD = System.UInt32;
using System;
using UnityEngine.Rendering;

public class ParticleSystemContainerSlim
{
    private PARTICLE_SYSTEM pss;
    private PARTICLE_DATA pd;
    private GameObject myGameObject;
    private ParticleSystem m_ParticleSystem;
    private Particle[] m_Particles;

    public ParticleSystemContainerSlim(PARTICLE_SYSTEM _pss)
    {
        pss = _pss;
        pd = pss.Data;
        m_Particles = new Particle[pd.MaxParts];
        myGameObject = GenerateGameObject();
    }

    private GameObject GenerateGameObject()
    {
        //GameObject res=GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject res = new GameObject();
        res.name = "Particle Spawner: " + pss.TextName;
        res.name += " BirthType " + pd.BirthType;

        m_ParticleSystem = res.AddComponent<ParticleSystem>();


        ParticleSystemRenderer rend = m_ParticleSystem.GetComponent<ParticleSystemRenderer>();

        Material material;
        switch (pd.DrawType)
        {
            case ParticlesDefines.DrawType.PD_ADDA:
                material = new Material(StaticPD_ADDA);
                break;
            case ParticlesDefines.DrawType.PD_ATEXTURE:
                material = new Material(StaticPD_ATEXTURE);
                break;
            default:
                material = new Material(StaticPD_ADDA);
                break;

        }

        //Texture2D fulltexture = renderer_dll.dll_data.LoadTexture(pd.GetTextureName());
        //Texture2D texture = GetClippedTexture(d3d.GetTexture(), pd.Mapping);
        Texture2D texture = StormUnityRenderer.GetClippedTexture(d3d.GetTexture(), pd.Mapping);
        material.mainTexture = texture;
        rend.sharedMaterial = material;

        MainModule main = m_ParticleSystem.main;
        main.startSpeed = 0;
        main.maxParticles = pd.MaxParts;
        main.simulationSpace = pss.isLocal() ? ParticleSystemSimulationSpace.Local : ParticleSystemSimulationSpace.World;
        
        var emission = m_ParticleSystem.emission;
        emission.rateOverTime = 0;
        return res;
    }

    public void Update()
    {
        UpdateTransform();
        UpdateVisual();
    }
    private void UpdateTransform()
    {
        myGameObject.transform.position = Engine.ToCameraReference(pss.Org);
        myGameObject.transform.rotation = Quaternion.LookRotation(pss.Dir, pss.Up);
    }
    private void UpdateVisual()
    {

        A_PARTICLE Pa = pss.Par[pss.Younger];
        int currentIndex = pss.Younger;
        int MaxP = pss.Data.MaxParts - 1;

        int particles_num = m_ParticleSystem.particleCount;
        int part_delta = pss.Num - particles_num;

        for (int j = 0; j < part_delta; j++)
        {
            EmitParams emitParams = new EmitParams();
            emitParams.position = Vector3.zero;
            emitParams.velocity = Vector3.zero;
            m_ParticleSystem.Emit(emitParams, 1);
        }

        particles_num = m_ParticleSystem.GetParticles(m_Particles);

        //Pa = pss.Par[pss.Younger];
        //currentIndex = pss.Younger;
        for (int j = pss.Num; j != 0; --j)
        {
            if (j >= particles_num) continue;
            int idx = (int)Mathf.Clamp(Pa.LivedFor, 0, 255);
            float size = pd.Size[idx];
            if (size > PARTICLE_SYSTEM.maxsize) size = PARTICLE_SYSTEM.maxsize;

            Color32 color = IndexToColor(pd.Color[idx]);

            Particle UnityParticle = m_Particles[j];

            UnityParticle.startSize = size;
            UnityParticle.startColor = color;

            //UnityParticle.position = !pss.isLocal() ? Engine.ToCameraReference(Pa.Org) : Engine.ToCameraReference(myGameObject.transform.positionmyGameObject.transform.rotation * (pss.Org - Pa.Org);
            //UnityParticle.position = !pss.isLocal() ? pss.Org - Pa.Org : myGameObject.transform.rotation * Pa.Org;
            UnityParticle.position = Pa.Org;
            //UnityParticle.velocity = Pa.Speed;

            m_Particles[j] = UnityParticle;

            currentIndex = (int)Mathf.Repeat(currentIndex - 1, MaxP);

            //Debug.Log("currentIndex " + currentIndex);
            Pa = pss.Par[currentIndex];
        }

        m_ParticleSystem.SetParticles(m_Particles, particles_num);
    }

    private static Dictionary<uint, Color32> partColors = new Dictionary<uint, Color32>();
    private Color32 IndexToColor(DWORD colorValue)
    {
        if (!partColors.ContainsKey(colorValue))
        {
            byte a = (byte)((colorValue >> 24) & 0xFF);
            byte r = (byte)((colorValue >> 16) & 0xFF);
            byte g = (byte)((colorValue >> 8) & 0xFF);
            byte b = (byte)((colorValue) & 0xFF);
            partColors[colorValue] = new Color32(r, g, b, a);
        }


        return partColors[colorValue];
    }
}
public static partial class StormUnityRenderer //Mesh (for terrain) 
{
    private static StormUnityRendererIndexedVB SURindexedVB;
    public static void DrawIndexedMesh(IDirect3DVertexBuffer7 vb, int num_vtx, ushort[] idxs, int num_idx, int start_vtx)
    {
        if (SURindexedVB == null) SURindexedVB = new StormUnityRendererIndexedVB();
        SURindexedVB.Draw(vb, num_vtx, idxs, num_idx, start_vtx);
    }
}
public static partial class StormUnityRenderer //Particles2
{
    private static Dictionary<int, ParticleSystemContainerSlim> currentParticleSystems = new Dictionary<int, ParticleSystemContainerSlim>();
    public static void DrawParticleSystem(PARTICLE_SYSTEM ps, PARTICLE_SYSTEM.GetColor get_color, bool clip)
    {
        int hash = ps.GetHashCode();
        if (!currentParticleSystems.ContainsKey(hash)) CreateParticleSystemContainer(ps);

        ParticleSystemContainerSlim particleSystem = currentParticleSystems[hash];
        if (particleSystem == null) return;

        particleSystem.Update();

    }


    private static void CreateParticleSystemContainer(PARTICLE_SYSTEM ps)
    {
        ParticleSystemContainerSlim cont = new ParticleSystemContainerSlim(ps);
        currentParticleSystems.Add(ps.GetHashCode(), cont);
    }
}
public static partial class StormUnityRenderer //Particles
{
    private static Dictionary<DWORD, Texture2D> ParticleTexturesCache = new Dictionary<DWORD, Texture2D>();

    private static StormUnityRendererParticles partRenderer;
    public static void DrawParticle(RO r) //TODO Перенести ленивую инициализацию в нормальную
    {
        if (r == null) return;
        if (partRenderer == null) partRenderer = new StormUnityRendererParticles();

        partRenderer.Draw(r);

    }
    public static GameObject PS2Unity(RO pemiiter)
    {
        return PS2UnitySlim(pemiiter);
    }
    public static GameObject PS2UnitySlim(RO pemitter)
    {
        PARTICLE_SYSTEM pss = (PARTICLE_SYSTEM)pemitter;
        PARTICLE_DATA pd = pss.Data;

        //GameObject tmp = GameObjectFactory.GetGameObject(GameObjectFactory.GameObjectType.PARTICLE);
        GameObject tmp = GameObjectFactory.GetGameObject<PARTICLE_SYSTEM>(pemitter);
        tmp.name = pemitter.TextName + " " + pemitter.GetHashCode().ToString("X8");


        ParticleSystem ps = tmp.GetComponent<ParticleSystem>();
        ps.name = pemitter.TextName;
        ParticleSystemRenderer rend = ps.GetComponent<ParticleSystemRenderer>();

        Material material;
        switch (pd.DrawType)
        {
            case ParticlesDefines.DrawType.PD_ADDA:
                material = new Material(StaticPD_ADDA);
                break;
            case ParticlesDefines.DrawType.PD_ATEXTURE:
                material = new Material(StaticPD_ATEXTURE);
                break;
            default:
                material = new Material(StaticPD_ADDA);
                break;

        }
        //new Material(StaticPD_ADDA);
        Texture2D fulltexture = renderer_dll.dll_data.LoadTexture(pd.GetTextureName());
        //Texture2D texture = GetClippedTexture(fulltexture, pd.Mapping);
        //material.mainTexture = texture;
        material.mainTexture = fulltexture;
        //material.mainTextureOffset = new Vector2(pd.Mapping[2], pd.Mapping[3]);
        material.mainTextureOffset = new Vector2(pd.Mapping[0], pd.Mapping[1]);
        material.mainTextureScale = new Vector2(pd.Mapping[2] - pd.Mapping[0], pd.Mapping[3]-pd.Mapping[1]);

        rend.sharedMaterial = material;

        var main = ps.main;
        main.startSpeed = 0;
        main.maxParticles = pd.MaxParts;
        main.simulationSpace = pss.isLocal() ? ParticleSystemSimulationSpace.Local : ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        return tmp;
    }


    public static Texture2D GetClippedTexture(Texture2D input, float[] Mapping)
    {
        uint crc = Hasher.HshString(input.name + Mapping[0] + Mapping[1] + Mapping[2] + Mapping[3]);
        if (!ParticleTexturesCache.ContainsKey(crc)) ParticleTexturesCache.Add(crc, CutTexture(input, Mapping));

        return ParticleTexturesCache[crc];
    }

    private static Texture2D CutTexture(Texture2D input, float[] Mapping)
    {
        return CutTextureAsTexture(input, Mapping);
    }
    private static Texture2D CutTextureAsTexture(Texture2D input, float[] Mapping)
    {
        int textureWidth = (int)((Mapping[2] - Mapping[0]) * input.width);
        int textureHeight = (int)((Mapping[3] - Mapping[1]) * input.height);
        //Debug.Log(string.Format("{0}x{1}", textureWidth, textureWidth));
        int textureX = (int)(Mapping[0] * input.width);
        int textureY = (int)(Mapping[1] * input.height);
        //Texture2D texture = new Texture2D(textureWidth, textureHeight, input.format, false);
        Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.Alpha8, false);

        Color pixel;
        Color correctedpixel;
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                pixel = input.GetPixel(textureX + x, input.height - textureY - y);
                correctedpixel.a = pixel.a;
                //correctedpixel.r = correctedpixel.g = correctedpixel.b = pixel.a;
                //texture.SetPixel(x, y, correctedpixel);
                texture.SetPixel(x, y, pixel);
                //Debug.Log("Pixel " + texture.GetPixel(x, y));
            }
        }


        texture.name = input.name;
        texture.Apply();
        return texture;
    }

    private static Texture2D CutTextureAsArray(Texture2D input, float[] Mapping)
    {
        int textureWidth = (int)((Mapping[2] - Mapping[0]) * input.width);
        int textureHeight = (int)((Mapping[3] - Mapping[1]) * input.height);
        //Debug.Log(string.Format("{0}x{1}", textureWidth, textureWidth));
        int textureX = (int)(Mapping[0] * input.width);
        int textureY = (int)(Mapping[1] * input.height);
        Texture2D texture = new Texture2D(textureWidth, textureHeight, input.format, false);

        Color pixel;
        Color correctedpixel;

        Color[] pixelsIn = input.GetPixels();
        Color[] pixelsOut = new Color[textureHeight * textureWidth];

        for (int y = 0; y < texture.height; y++)
        {
            int offsetOut = y * textureHeight;
            int offsetIn = textureY * input.width;
            for (int x = 0; x < texture.width; x++)
            {
                //pixel = input.GetPixel(textureX + x, input.height - textureY - y);
                pixel = pixelsIn[textureX + x + offsetIn];
                correctedpixel.a = pixel.a;
                correctedpixel.r = correctedpixel.g = correctedpixel.b = pixel.a;
                pixelsOut[x * offsetOut] = pixel;
            }
        }


        texture.name = input.name;
        texture.Apply();
        return texture;
    }

}

public static partial class StormUnityRenderer //common
{
    public static Stack<Transform> transformStack=new Stack<Transform>();
    public static Transform GetCurrentTransform()
    {
        if (transformStack.Count == 0) return null;
        return transformStack.Peek();
    }
    public static Transform PushTransform(Transform transform)
    {
        transformStack.Push(transform);
        return transform;
    }

    public static Transform PopTransform()
    {
        return transformStack.Pop();
    }

    public static void GameObjectsCleanup()
    {
        //FPOObjectsCleanup();
        //EffectsObjectsCleanup();

        if (fpoRenderer != null) fpoRenderer.Cleanup(); //TODO - убрать проверку после реализации корректной инициализации рендерера объектов
        if (partRenderer != null) partRenderer.Cleanup();
    }

    /// <summary>
    /// Теоретически - находится ли объект myRO в в поле обзора камеры. Практически - проверяется наличие его в передней полусфере камеры.
    /// </summary>
    /// <param name="Org">точка в игровом пространстве</param>
    /// <param name="radius">радиус объёкта в игровом пространстве</param>
    /// <returns></returns>
    //public static bool FrustrumCulling(RO myRO)
    public static bool FrustrumCulling(Vector3 Org,float radius=0)
    {
        Vector3 localpos = Org - Engine.EngineCamera.Org;
        //Отбрасывам точки дальше радиуса обзора
        if (localpos.sqrMagnitude> Mathf.Pow(Engine.UnityCamera.farClipPlane,2)) return false;
        //Отбрасывам точки сзади камеры
        //localpos+= Engine.EngineCamera.Dir * radius;
        //if (Vector3.Dot(localpos.normalized, Engine.EngineCamera.Dir) < 0) return false;
        return true;
    }

    public static bool FrustrumCulling(RO myRO)
    {
        if (myRO == null) return false;
        RO top = myRO.Top();
        return FrustrumCulling(top.Org, top.MaxRadius);
    }

}
public static partial class StormUnityRenderer //FPO
{
    private static IStormUnityRendererFPO fpoRenderer;

    public static GameObject DrawFPO(FPO myFPO, GameObject parent = null)
    {
        //return DrawFPOGameObject(myFPO, parent);
        // return DrawFPOMesh(myFPO, parent); //Отрисовка меша без создания GameObject
        //return DrawFPOimages2GameObject(myFPO);
        //return DrawFPOGameObject2(myFPO); //Старый вариант рендеринга непосредственно из ресурсных файлов
        return DrawFPOGameObjectUniform(myFPO); //Более новый, унифицированный вариант
    }

    public static GameObject DrawFPOGameObjectUniform(FPO myFPO)
    {
        if (fpoRenderer == null) fpoRenderer = new StormUnityManagerFPO(); //TODO перенести из ленивой инициализации в общую
        fpoRenderer.Draw(myFPO);
        return null;
    }

    public static GameObject DrawFPOMesh(FPO myFPO, GameObject parent = null)
    {
        if (fpoRenderer == null) fpoRenderer = new StormUnityRendererFPOMesh(); //TODO перенести из ленивой инициализации в общую
        fpoRenderer.Draw(myFPO);
        return null;
    }

    public static GameObject DrawFPOGameObject2(FPO myFPO)
    {
        if (fpoRenderer == null) fpoRenderer = new StormUnityRendererFPOGameObject(); //TODO перенести из ленивой инициализации в общую
        fpoRenderer.Draw(myFPO);
        return null;
    }
}

public static partial class StormUnityRenderer //Lasers
{
    private static Dictionary<int, GameObject> lasers = new Dictionary<int, GameObject>();
    private static Dictionary<int, GameObject> lasers_prev = new Dictionary<int, GameObject>();
    public static void DrawLaser(Laser laser)
    {
        if (!FrustrumCulling((laser.GetStart() + laser.GetEnd()) / 2)) return;
        GameObject laserGobj = GetLaserGameObject(laser);
        if (laserGobj == null) return;
        LineRenderer lr = laserGobj.GetComponent<LineRenderer>();
        laserGobj.transform.position = Engine.ToCameraReference(laser.GetStart());
        lr.SetPosition(0, Engine.ToCameraReference(laser.GetStart()));
        lr.SetPosition(1, Engine.ToCameraReference(laser.GetEnd()));
    }

    //TODO! Так делать не нужно! Нужно использовать единый подход к рисованию
    public static void UnDrawLaser(Laser laser)
    {
        int id = laser.GetHashCode();
        if (!lasers.ContainsKey(id)) return;
        GameObject gobj = lasers[id];
        GameObject.Destroy(gobj);

    }

    private static GameObject GetLaserGameObject(Laser laser)
    {
        int id = laser.GetHashCode();

        if (!lasers.ContainsKey(id))
        {
            GameObject gobj = new GameObject("laser");
            LineRenderer lr = gobj.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.generateLightingData = true;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Material mat = new Material(MaterialStorage.DefaultTransparentUnlit);
            //mat.mainTexture = RotateTexture(d3d.GetTexture());
            UpdateMaterialHDRP(ref mat, laser.GetColor());

            lr.material = mat;
            Color lasColor = laser.GetColor();
            lr.startColor = lasColor;
            lr.endColor = lasColor;
            lr.startWidth = laser.start_thickness;
            lr.endWidth = laser.finish_thickness;
            lr.positionCount = 2;
            lasers.Add(id, gobj);
        }

        return lasers[id];
    }

    private static void UpdateMaterialHDRP(ref Material mat, Color laserColor)
    {
        if (mat == null) return;
        mat.mainTexture = RotateTexture(d3d.GetTexture());
        mat.SetTexture("_EmissiveColorMap", MaskTexture);
        mat.color = laserColor;

        HDMaterial.SetEmissiveColor(mat, laserColor);
        HDMaterial.ValidateMaterial(mat);
    }

    public static Texture2D MaskTexture;

    private static Texture2D RotateTexture(Texture2D tex)
    {
        if (MaskTexture != null) return MaskTexture;
        MaskTexture = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, true);
        Color[] pixels = tex.GetPixels();
        Color[] pixels_out = new Color[pixels.Length];
        Color CurrentColor;
        for (int y = 0; y < tex.width; y++)
        {
            int line = y * tex.width;
            for (int x = 0; x < tex.height; x++)
            {
                CurrentColor = pixels[line + x];
                CurrentColor.a = CurrentColor.r;
                pixels_out[x * tex.height + y] = CurrentColor;
                //MaskTexture.SetPixel(y, x, new Color(0,0,0,CurrentColor.a));
                //MaskTexture.SetPixel(y, x, CurrentColor);
            }
        }

        MaskTexture.SetPixels(pixels_out);
        MaskTexture.Apply();

        return MaskTexture;
    }
}

public static partial class StormUnityRenderer //Fpo
{
    private static Dictionary<int, GameObject> fpos = new Dictionary<int, GameObject>();
    public static void DrawFpo(TmTree myfpo)
    {
        //Debug.Log("Draw as myfpo: " + myfpo + " @ " + myfpo.tm.pos);
        GameObject projectile;
        int id = myfpo.GetHashCode();


        if (!fpos.ContainsKey(id))
        {
            projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //projectile.name = "fpoobject " + id.ToString("X8") + myfpo.tm.pos;
            projectile.name = string.Format("fpoobject {0} {1}", ((Fpo)myfpo).Name, id.ToString("X8"));
            fpos.Add(id, projectile);
        }
        projectile = fpos[id];
        if (projectile == null) return;

        //projectile.transform.position = Engine.ToCameraReference(myfpo.tm.pos);
        projectile.transform.position = Engine.UnityCamera.transform.position + myfpo.tm.pos;
    }

    public static void UnDrawFPO(TmTree myfpo)
    {
        int id = myfpo.GetHashCode();
        if (!fpos.ContainsKey(id)) return;

        GameObject.Destroy(fpos[id]);
    }
}
public static partial class StormUnityRenderer //sky
{
    private static Dictionary<int, GameObject> planets = new Dictionary<int, GameObject>();
    private static GameObject PlanetsHolder;
    private static GameObject SkyHolder;
    internal static void DrawPlanet(Sky.PLANET_8 planet, CAMERA cam, float PLANET_Z)
    {
        GameObject gobj;
        if (planet.texture != null) gobj = GetPlanet(planet);

    }
    private static GameObject GetPlanet(Sky.PLANET_8 planet)
    {
        int hash = planet.GetHashCode();
        if (!planets.ContainsKey(hash))
        {
            planets.Add(hash, CreatePlanet(planet, hash));
            planets[hash].transform.parent = PlanetsHolder.transform;
        }
        return planets[hash];
    }

    private static GameObject CreatePlanet(Sky.PLANET_8 planet, int hash = 0)
    {
        if (hash == 0) hash = planet.GetHashCode();
        var d = planet.cur_cfg;
        GameObject gobjplanet = GameObject.Instantiate(MaterialStorage.MoonPrefab);
        gobjplanet.name = "Planet " + hash.ToString("X8") + " " + planet.cur_cfg.size;
        HDAdditionalLightData moonlight = gobjplanet.GetComponent<HDAdditionalLightData>();
        moonlight.surfaceTexture = planet.texture;
        //Vector3 tmpColor = planet.cur_cfg.color[0];
        //Vector3 tmpColor = GetColor((d.color[1] + d.color[2]) * .5f);
        moonlight.surfaceTint = GetColor((d.color[1] + d.color[2]) * .5f);
        moonlight.angularDiameter = planet.cur_cfg.size;
        moonlight.diameterOverride = planet.cur_cfg.size;
        //всегда полнолуние, т.к. сама текстура в шторме обкусана
        moonlight.moonPhase = 180;
        Vector3 Org = StormTransform.Globus(planet.cur_cfg.a, planet.cur_cfg.h);
        Vector3 Left = Vector3.Cross(Org, Vector3.up); Left *= (planet.cur_cfg.size * Storm.Math.OO256) / Left.magnitude;
        Vector3 Up = Vector3.Cross(Left, Org); Up *= (planet.cur_cfg.size * Storm.Math.OO256) / Up.magnitude;
        gobjplanet.transform.rotation = Quaternion.LookRotation(-Org, -Up);

        //Debug.Log("Planet created " + gobjplanet.name + " dir " + Org + " size " + planet.cur_cfg.size+ " OO256 "+ Storm.Math.OO256 + " magnitude " + Vector3.Cross(Org, Vector3.up).magnitude + " up " + Up);
        Debug.Log(d);
        return gobjplanet;
    }

    private static Color GetColor(Vector3 v, int a = 1)
    {
        return new Color(v[0], v[1], v[2], a);
    }
    private static GameObject sun;
    public static void InitSkyUnity(Sky skyobject, GLight globalLight)
    {
        if (SkyHolder == null) SkyHolder = new GameObject("Sky");
        if (PlanetsHolder == null) PlanetsHolder = new GameObject("Planets");
        PlanetsHolder.transform.parent = SkyHolder.transform;

        Light sunLight;

        if (sun == null)
        {
            var lights = GameObject.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in lights)
            {
                if (l.type == LightType.Directional)
                {
                    l.gameObject.SetActive(false);
                }
            }

            sun = GameObject.Instantiate(MaterialStorage.SunPrefab);
            sun.name = "Sun";
            sun.transform.parent = SkyHolder.transform;

        }
        sunLight = sun.GetComponent<Light>();
        sunLight.color = new Color(globalLight.v_color.x, globalLight.v_color.y, globalLight.v_color.z);
        sun.transform.rotation = Quaternion.LookRotation(-globalLight.dir);
        Debug.Log(string.Format("Sun [{0}] color is set: {1}, rotation {2}/ {3}", sun.name, sunLight.color, sun.transform.rotation, globalLight.dir));
    }

    /// <summary>
    /// Генерация единого GameObject'а, содержащего полный FPO (Со всеми subobjects)
    /// </summary>
    /// <param name="fdata"></param>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static GameObject fdata2gameObjectFull(FpoData fdata, string name = "unknown", GameObject parent = null)
    {
        //if (fdata.images[0].graph == 0xFFFFFFFF) return null;
        GameObject gobj = new GameObject(name);
        //GameObject gobj = DrawGameObjectPool.objectPool.Get();
        gobj.name = name + " NO GRAPH";

        FpoGraphData fpoGraphData;
        if (fdata.images[0].graph == 0xFFFFFFFF)
        {
            fpoGraphData = default;
        }
        else
        {
            fpoGraphData = dll_data.meshes.GetBlock(fdata.images[0].graph).Convert<FpoGraphData>();
        }
        //FpoGraphData fpoGraphData = StormRendererData.GetFpoGraph(fdata.images[0].graph);

        if (fpoGraphData != null)
        {
            if (fpoGraphData.GetLod(0) != 0x00000000)
            {
                gobj.name = name;
                //StormMesh graph = GameDataHolder.GetResource<StormMesh>(PackType.MeshDB, fpoGraphData.GetLod(0));
                uint id = fpoGraphData.GetLod(0);

                ObjId tmpId = new ObjId(id);
                dll_data.meshes.CompleteObjId(tmpId);
                gobj.name = tmpId.name + " " + name;
                //if (!DrawGameObjectPool.stormmeshcache.ContainsKey(id))
                //{
                //    DrawGameObjectPool.stormmeshcache.Add(id, Renderer.sdr.GetStormMesh(id));
                //}
                //StormMesh graph = DrawGameObjectPool.stormmeshcache[id];
                StormMesh graph = dll_data.GetStormMesh(id);

                if (!DrawGameObjectPool.meshcache.ContainsKey(id))
                {
                    DrawGameObjectPool.meshcache.Add(id, StormMeshImport.ExtractMesh(graph));
                    //Debug.Log("ID added" + id.ToString("X8") + " size " + DrawGameObjectPool.meshcache.Count);
                }
                Mesh tmpMesh = DrawGameObjectPool.meshcache[id];

                MeshRenderer mr;
                if (!gobj.TryGetComponent<MeshRenderer>(out mr))
                {
                    mr = gobj.AddComponent<MeshRenderer>();
                }

                mr.materials = StormMeshImport.GetMaterials(graph);


                MeshFilter mf;
                if (!gobj.TryGetComponent<MeshFilter>(out mf))
                {
                    mf = gobj.AddComponent<MeshFilter>();
                }
                if (tmpMesh != null) mf.mesh = tmpMesh;
            }
        }
        if (parent != null)
        {
            gobj.transform.parent = parent.transform;
        }
        //gobj.name = name;
        //gobj.transform.localScale *= 2 * fdata.images[1].radius;

        Vector3 fixedpos;
        fixedpos = fdata.pos.org;
        fixedpos.y *= -1;
        gobj.transform.localPosition = fixedpos;

        Vector3 FPOUp = fdata.pos.e2;
        Vector3 FPOLeft = fdata.pos.e1;

        FPOUp.y *= -1;
        FPOLeft.y *= -1;

        Vector3 FPODir = Vector3.Cross(FPOUp, FPOLeft);

        //if (parent != null) gobj.transform.localRotation = Quaternion.LookRotation(FPODir, FPOUp);
        gobj.transform.localRotation = Quaternion.LookRotation(FPODir, FPOUp * -1);

        FpoData f;
        //for (f=fdata.GetSubData();f!=null;f=f.GetSubData())
        //{
        //    await Task.Yield();
        //    await fdata2gameObject(f, "subobj", gobj);
        //}
        f = fdata.GetSubData();
        if (f != null)
        {
            //await Task.Yield();
            string subobjname = f.name.ToString("X8");
            fdata2gameObjectFull(f, "subobj " + subobjname, gobj);
        }

        f = fdata.GetNextData();
        if (f != null)
        {
            //await Task.Yield();
            fdata2gameObjectFull(f, "next " + f.name.ToString("X8"), parent);
        }

        foreach (SlotData sld in fdata.slots)
        {
            GameObject slot = new GameObject();
            slot.name = "Slot " + sld.name.ToString("X8") + " " + sld.slot_id;

            Vector3 slotOrg = sld.pos.org;
            slotOrg.y *= -1;
            Vector3 slotUp = sld.pos.e2;
            slotUp.y *= -1;
            Vector3 slotDir = sld.pos.GetE3();
            //slotDir.y *= -1;

            slot.transform.parent = gobj.transform;
            slot.transform.localPosition = slotOrg;
            slot.transform.localRotation = Quaternion.LookRotation(slotDir);

        }
        //for (f = fdata.GetNextData(); f != null; f = f.GetNextData())
        //{
        //    await Task.Yield();
        //    await fdata2gameObject(f, "next", parent);
        //}

        return gobj;

    }


}

public static partial class StormUnityRenderer //Terrain
{
    public static StormUnityRendererTerrain suTerrain;

    public static void Init()
    {
        suTerrain = new StormUnityRendererTerrain();
    }

}
