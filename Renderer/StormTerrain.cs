using geombase;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Assertions;
using static D3DEMULATION;
using static GL_STRIDE_FIELD;
using static GL_VB_TYPE;
using static renderer_dll;
using static TerrainDefs;
using static TMType;
using crc32value = System.UInt32;
using DWORD = System.UInt32;
using TerrainPipe = ITPipe<BoxRObj>;
using WORD = System.UInt16;

/// <summary>
/// Переименовано из Terrain во избежание конфликта с одноименным классом Unity
/// </summary>
public partial class StormTerrain : iTerrain
{
    const int max_scripts = 256;

    Scene scene;

    TERRAIN_DATA Data;

    TerrainCfg cur_cfg;
    ObjId cur_cfgid;

    TerrainStateCfg cur_statecfg;
    ObjId cur_statecfgid;

    D3DMATERIAL7 GroundLoadMaterial;

    MSurface WaterSurface;
    D3DMATERIAL7 WaterDetailMaterial;
    TexMap WaterDetailTexMap;
    //IShader WaterDetailShader;

    MSurface[] Surfaces = new MSurface[max_scripts];
    int n_msurfaces;
    int detail_surface;//detail surface index

    TerrainMtlCfg cur_mtlcfg;
    ObjId cur_mtlcfgid;
    float TerrainTime;
    public StormTerrain(TERRAIN_DATA td, string collisions, int size)
    {
        Debug.Log("Creating Terrain object");
        Data = td;

        temp_geometry.pdata = new GLVertices();
        temp_geometry.pdata.vb = null;//it's not api-supporded vb!
        temp_geometry.pdata.format_gl_2_d3d(GLField(glsfPOSITION) | GLField(glsfNORMAL) | GLField(glsfDIFFUSE) | GLField(glsfSPECULAR) | GLField(glsfTEXTURE0));
        Asserts.AssertBp(temp_geometry.pdata.d3dformat == GroundVertex.FVF);
        //Предположительно - 256 (16*16) GroundVertex'ов. Возможно, их лучше и хранить в lpdata?
        //temp_geometry.pdata.lpdata = new char[40 * 256];
        //temp_geometry.pdata.lpdata = new object[40 * 256];
        //temp_geometry.pdata.lpdata = new GroundVertex[256];
        temp_geometry.pdata.lpdata = Alloca.ANewN<GroundVertex>(256);

        temp_geometry.pdata.Set(glvtBASIC_SOA);
        temp_geometry.pdata.SetAOSData(ref temp_geometry.pdata.lpdata, temp_geometry.pdata.vtx_size);


        bxclamp_max = Data.Boxes.pager.SizeXPages() * BOXES_PAGE_SIZE - 1;
        bzclamp_max = Data.Boxes.pager.SizeZPages() * BOXES_PAGE_SIZE - 1;
        bxclamp_min = 0;
        bzclamp_min = 0;

        LoadSize = size;

        VBoxes = new TerrainObjects(this, size);
    }

    private int GLField(GL_STRIDE_FIELD i)
    {
        return 1 << (int)i;
    }
    private int GLField(int i)
    {
        return 1 << i;
    }

    public bool Initialize(Scene _scene, ObjId config)
    {
        scene = _scene;

        if (!VBoxes.Initialize(d3d))
        {
            Log.Message("Error : Can't initialize terrain data hash...\n");
            return false;
        }

        TerrainCfg cfg = dll_data.LoadFile<TerrainCfg>(config);
        if (cfg == null)
        {
            Log.Message("Error : Invalid Terrain Config " + config.ToString() + " , applying default...\n");
            cfg = dll_data.LoadFile<TerrainCfg>("default#terr");
            Assert.IsNotNull(cfg);
        }

        InitializeState(cfg.state);

        if (!InitializeMaterials(cfg.material))
        {
            Log.Message("Error : Terrain failed to initialize materials!\n");
            return false;
        }

        if (!InitializeFeatures(cfg.features))
        {

            return false;
        }
        CreateRenderPipe();
        cur_cfg = cfg;
        cur_cfgid = config;
        return true;
    }

    public void Invalidate()
    {
        for (int y = 0; y < LoadSize; y++)
        {
            for (int x = 0; x < LoadSize; x++)
            {
                //TODO Корректно реализовать инвалидацию VBoxes при отрисовке террайна
                // VBoxes.data[y][x].Invalidate();
                // VBoxes.mFeatures[y][x].Invalidate();
            }
        }
    }

    TerrExport mExporter;
    public IMeshExporter CreateMeshExporter()
    {
        if (mExporter == null)
        {
            mExporter = new TerrExport();
            if (!mExporter.Initialize(this))
            {
                mExporter.Release();
                mExporter = null;
                Asserts.AssertBp(null);
            }
        }

        mExporter.AddRef();
        return mExporter;
    }

    bool InitializeFeatures(ObjId config)
    {
        return true;
    }
    #region state
    //state
    public void InitializeState(ObjId config)
    {
        TerrainStateCfg cfg = dll_data.LoadFile<TerrainStateCfg>(config);
        if (cfg == null)
        {
            Log.Message("Error : Invalid TerrainState Config, applying default...\n");
            cfg = dll_data.LoadFile<TerrainStateCfg>("default#terrstate");
            Assert.IsNotNull(cfg);
        }

        cur_statecfg = cfg;
        cur_statecfgid = config;

        Debug.Log("Terrain state loaded: " + cfg);
        Invalidate();
    }

    public ObjId GetStateConfig()
    {
        return cur_statecfgid;
    }

    public void ApplyStateConfig(ObjId config)
    {
        TerrainStateCfg cfg = dll_data.LoadFile<TerrainStateCfg>(config);
        if (cfg == null)
        {
            Log.Message("Error : Can't apply invalid TerrainState Config...\n");
            return;
        }

        bool need_invalidate = false;
        if (cfg.StaticLightingMode != cur_statecfg.StaticLightingMode)
        {
            need_invalidate = true;
        }
        if (cfg.texture_scale != cur_statecfg.texture_scale)
        {
            need_invalidate = true;
        }

        cur_statecfg = cfg;
        cur_statecfgid = config;

        if (need_invalidate) Invalidate();
    }
    #endregion

    #region materials
    //materials
    bool InitializeMaterials(ObjId config)
    {
        TerrainMtlCfg cfg = dll_data.LoadFile<TerrainMtlCfg>(config);
        if (cfg == null)
        {
            Log.Message("Error : Invalid TerrainMtl Config, applying default...\n");
            cfg = dll_data.LoadFile<TerrainMtlCfg>("default#terrmtl");
            Assert.IsNotNull(cfg);
        }

        int i;

        GroundLoadMaterial = dll_data.LoadMaterial("GroundStatic");
        //GroundLoadRS = dll_data.CreateRS("basealignboxbstate");

        //if (d3d.caps.use_embm)
        //{
        //    WaterTexMap = CreateTexMap("tex_map#waterbump", 0);
        //    if (!WaterTexMap)
        //    {
        //        Log->Message("Error : Can't load water texmap...\n");
        //        return false;
        //    }
        //}
        //else
        //{
        WaterDetailMaterial = dll_data.LoadMaterial("WaterDetail");
        WaterDetailTexMap = CreateTexMap("tex_map#water_detail", 0);
        //WaterDetailShader = CreateShader("shader#water_detail");
        //}
        //WaterShader = CreateShader("shader#water_bump");
        //if (!WaterShader) return false;

        //detail_shader = CreateShader("detail_shader");

        //configurable
        for (i = 0; i < Data.Header.nMaterials; ++i)
        {
            Surfaces[i] = new MSurface();
            Surfaces[i].texture = dll_data.LoadTexture(cfg.GroundSurfaces[i].texture);
            Surfaces[i].material = dll_data.LoadMaterial(cfg.GroundSurfaces[i].material);
        }

        detail_surface = i;
        if (Surfaces[detail_surface] == null) Surfaces[detail_surface] = new MSurface();
        Surfaces[detail_surface].texture = dll_data.LoadTexture(cfg.DetailSurface.texture);
        Surfaces[detail_surface].material = dll_data.LoadMaterial(cfg.DetailSurface.material);

        //MStates[0] = dll_data.CreateRS(cfg->main_rs);
        //MStates[1] = dll_data.CreateRS(cfg->blend_rs);
        detail_state = 2;
        //MStates[detail_state] = dll_data.CreateRS(cfg->detail_rs);

        WaterSurface = new MSurface();
        WaterSurface.texture = dll_data.LoadTexture(cfg.WaterSurface.texture);
        WaterSurface.material = dll_data.LoadMaterial(cfg.WaterSurface.material);
        //WaterRS = dll_data.CreateRS("basetwater");
        //WaterDetailRS = dll_data.CreateRS("detailtwater");

        cur_mtlcfg = cfg;
        cur_mtlcfgid = config;
        return true;
    }

    ObjId GetMtlConfig()
    {
        return cur_mtlcfgid;
    }

    bool ApplyMtlConfig(ObjId config)
    {
        TerrainMtlCfg cfg = dll_data.LoadFile<TerrainMtlCfg>(config);
        if (cfg == null)
        {
            Log.Message("Error : Can't apply invalid TerrainMtl Config...\n");
            return false;
        }

        int i, j;

        for (i = 0; i < Data.Header.nMaterials; ++i)
        {
            //TODO возможно, тут и далее в ConfigureTexture и ConfigureMaterial нужен ref Surfaces
            ConfigureTexture(ref Surfaces[i].texture, cfg.GroundSurfaces[i].texture, ref cur_mtlcfg.GroundSurfaces[i].texture);
            ConfigureMaterial(ref Surfaces[i].material, cfg.GroundSurfaces[i].material, ref cur_mtlcfg.GroundSurfaces[i].material);
        }

        detail_surface = i;
        ConfigureTexture(ref Surfaces[detail_surface].texture, cfg.DetailSurface.texture, ref cur_mtlcfg.DetailSurface.texture);
        ConfigureMaterial(ref Surfaces[detail_surface].material, cfg.DetailSurface.material, ref cur_mtlcfg.DetailSurface.material);

        //ConfigureRS(&MStates[0], cfg->main_rs, cur_mtlcfg.main_rs);
        //ConfigureRS(&MStates[1], cfg->blend_rs, cur_mtlcfg.blend_rs);
        //detail_state = 2;
        //ConfigureRS(&MStates[2], cfg->detail_rs, cur_mtlcfg.detail_rs);

        ConfigureTexture(ref WaterSurface.texture, cfg.WaterSurface.texture, ref cur_mtlcfg.WaterSurface.texture);
        ConfigureMaterial(ref WaterSurface.material, cfg.WaterSurface.material, ref cur_mtlcfg.WaterSurface.material);
        //ConfigureRS      ( &WaterRS, cfg->water_rs, cur_mtlcfg.water_rs );

        cur_mtlcfg = cfg;
        cur_mtlcfgid = config;
        return true;
    }


    void FreeMaterials()
    {

        //SafeRelease(detail_shader);
        //SafeRelease(WaterShader);
        //SafeRelease(WaterDetailShader);
        //SafeRelease(WaterTexMap);
        SafeRelease(WaterDetailTexMap);
        //SafeRelease(WaterRS);
        //SafeRelease(WaterDetailRS);
        //WaterSurface.material
        SafeRelease(WaterSurface.texture);
        //SafeRelease(GroundLoadRS);
        //GroundLoadMaterial

        //SafeRelease(MStates[2]);
        //SafeRelease(MStates[1]);
        //SafeRelease(MStates[0]);

        //Surfaces[detail_surface].material
        SafeRelease(Surfaces[detail_surface].texture);

        for (int i = 0; i != Data.Header.nMaterials; ++i)
        {
            //Surfaces[i].material
            SafeRelease(Surfaces[i].texture);
        }
    }
    #endregion

    //TODO возможно, стоит перенести как статический класс в texmapdata
    TexMap CreateTexMap(ObjId id, int i)
    {
        TexMapData data = dll_data.LoadFile<TexMapData>(id);
        if (data == null)
        {
            Log.Message("Error : Can't load tex map : {0}\n", id.name);
            return null;
        }
        Debug.Log("CreateTexMap: " + data);
        switch (data.type)
        {
            case TMT_STANDARD:
                {
                    return CreateStdTexMap(dll_data.LoadFile<StdTexMapData>(id), id);
                    //return CreateStdTexMap((StdTexMapData)data);

                }
        }
        ;
        return null;
    }

    TexMap CreateStdTexMap(StdTexMapData data, ObjId id)
    {
        Debug.Log("CreateStdTexMap:" + data);
        switch (data.animode)
        {
            case TMAniMode.TMA_STATIC:
                {
                    //StaticStdTexMap tex_map = new StaticStdTexMap(&d3d);
                    StaticStdTexMap tex_map = new StaticStdTexMap(null);
                    data = dll_data.LoadFile<StaticStdTexMapData>(id);
                    if (!tex_map.Initialize((StaticStdTexMapData)data))
                    {
                        tex_map.Release();
                        return null;
                    }
                    Debug.Log("tex_map:" + tex_map);
                    return tex_map;
                }
            case TMAniMode.TMA_SLIDED:
                {
                    //SlidedStdTexMap tex_map = new SlidedStdTexMap(&d3d);
                    SlidedStdTexMap tex_map = new SlidedStdTexMap(null);
                    data = dll_data.LoadFile<SlidedStdTexMapData>(id);
                    if (!tex_map.Initialize((SlidedStdTexMapData)data))
                    {
                        tex_map.Release();
                        return null;
                    }
                    Debug.Log("tex_map:" + tex_map);
                    return tex_map;
                }
        }

        return null;
    }
    public static void ConfigureTexture(ref Texture2D ptexture, crc32value new_tex, ref crc32value cur_tex)
    {
        if (cur_tex != new_tex)
        {
            //SafeRelease(ptexture);
            ptexture = dll_data.LoadTexture(cur_tex = new_tex);
        }
    }

    public void ConfigureMaterial(ref D3DMATERIAL7 pmaterial, crc32value new_mtl, ref crc32value cur_mtl)
    {
        if (cur_mtl != new_mtl)
        {
            pmaterial = dll_data.LoadMaterial(cur_mtl = new_mtl);
        }
    }
    private void SafeRelease(object o)
    {
        //STUB!
        return;
    }
    public void Update(float scale)
    {
        //в исходном коде на c++ здесь анимируется вода
        TerrainTime += scale;
        //if (WaterTexMap!=null)
        //    WaterTexMap.Query<iUpdatable>().Update(scale);

        if (WaterDetailTexMap != null)
            ((iUpdatable)WaterDetailTexMap.Query(iUpdatable.ID)).Update(scale);
    }

    float Time;
    int frame;
    float water_freq;

    int boxes_drawn;
    int boxes_clipped;

    public void DrawGround()
    {
        DrawGroundChunked();
        //DrawGroundStorm();
    }

    /// <summary>
    /// Отрисовка террайна при поможи Terrain Unity
    /// </summary>
    public void DrawGroundChunked()
    {
        //if (!StormUnityRendererTerrain.inited) StormUnityRendererTerrain.Init(Data, bxclamp_min, bxclamp_max, bzclamp_min, bzclamp_max);
        //StormUnityRendererTerrain.DrawGroundChunks();
        if (StormUnityRenderer.suTerrain == null) StormUnityRenderer.Init(); //TODO нужно инициализировать отдельно, а не здесь!
        if (!StormUnityRenderer.suTerrain.inited) StormUnityRenderer.suTerrain.Init(Data, bxclamp_min, bxclamp_max, bzclamp_min, bzclamp_max);
        StormUnityRenderer.suTerrain.DrawGroundChunks();
    }
    /// <summary>
    /// Отрисовка террайна с использованием кода "Шторма"
    /// </summary>
    public void DrawGroundStorm()
    {
        Prof prof;
        frame++;

        //dprintf( "Terrain Ground Visualisation : \n");

        boxes_drawn = 0;
        boxes_clipped = 0;

        m_RenderPipe.Start();

        //initing material list
        //mfreei = 0;
        //for (int i = 0; i < max_mstates; ++i)
        //{
        //    mcount[i] = 0;
        //    for (int j = 0; j < max_scripts; ++j)
        //        mlist[i][j] = 0;
        //}

        //preloading ground boxes
        //dprintf( "Preload Stage... \n");

        //d3d.SetMaterial(GroundLoadMaterial);
        //GroundLoadRS->Apply();

        //Engine::SetGlobalLight(Engine::scene->objects.g_light->gl_light);

        //Engine::light_pipe.ToActivate(Engine::scene->objects.g_light->gl_light, lpGLOBAL);
        //Engine::light_pipe.UpdateActivity();
        int enumed_boxes;
        {
            ERasterizer<StormTerrain> gdenumer = new ERasterizer<StormTerrain>(this, LoadGroundBox);
            //ERasterizer<StormTerrain> gdenumer = new ERasterizer<StormTerrain>(this, LoadGroundBoxChunked);

            prof = new Prof();
            prof.Reset();
            prof.AddRef();
            prof.Start();
            enumed_boxes = frame_raster.Rasterize(gdenumer);
            prof.End();
            Log.Message("Terrain : in {0} clocks enumed {1}  boxes.\n", prof.Avrg(), enumed_boxes);
        }
        VBoxes.vbs.UnlockAll();

        float detail_tcscale = 4;
        //D3DMATRIX mat;
        //b_zero(&mat, sizeof(D3DMATRIX));
        //mat._11 = GetDetailScale();
        //mat._22 = GetDetailScale();
        //d3d.Device().SetTransform(D3DTRANSFORMSTATE_TEXTURE0, &mat);

        //LightActivator la(&Engine::light_pipe);
        //Engine::light_pipe.ToActivate(Engine::scene.objects.g_light.gl_light, lpGLOBAL);
        //Engine::light_pipe.UpdateActivity();

        prof = new Prof();
        prof.Start();
        m_RenderPipe.Flush();
        prof.End();
        Log.Message("Terrain : Flushed in {0} ticks.", prof.Time());
    }

    void AlignBox(TerrainBox dst)
    {
        temp_geometry.pdata.Lock(0);

        if (GetStaticLightingMode() != TerrainCfg.TSLM_EEI)
        {
            //filling   color2 with the real-time emissive color (static shadows)
            //filling   color1 with the real-time diffuse color
            Vector3 color0 = new Vector3(
              Surfaces[dst.msurfacei[0]].material.diffuse.r,
              Surfaces[dst.msurfacei[0]].material.diffuse.g,
              Surfaces[dst.msurfacei[0]].material.diffuse.b);

            Vector3 color1 = (dst.mstatei[1] == -1) ? color0 : new Vector3(
              Surfaces[dst.msurfacei[1]].material.diffuse.r,
              Surfaces[dst.msurfacei[1]].material.diffuse.g,
              Surfaces[dst.msurfacei[1]].material.diffuse.b);


            //Stride dstc = new Stride(temp_geometry.pdata.data[(int)glsfDIFFUSE].ptr, temp_geometry.pdata.data[(int)glsfDIFFUSE].stride);
            //Stride dsts = new Stride(temp_geometry.pdata.data[(int)glsfSPECULAR].ptr, temp_geometry.pdata.data[(int)glsfSPECULAR].stride);
            //Stride dff = new Stride(temp_geometry.pdata.data[(int)glsfDIFFUSE].ptr, temp_geometry.pdata.data[(int)glsfDIFFUSE].stride);
            //for (int i = 0; i < temp_geometry.num; ++i)
            //{
            //    SVec4 c = new SVec4((int)dff.Ref<DWORD>(i));
            //    Vector3 mc = (color1 - color0) * c.a + color0 * 255;
            //    dstc.Ref<DWORD>(i) =
            //      new SVec4(c.a, (int)(c.r * Storm.Math.OO256 * mc.r), (int)(c.g * Storm.Math.OO256 * mc.g), (int)(c.b * Storm.Math.OO256 * mc.b)).PackARGBMax();
            //    dsts.Ref<DWORD>(i) =
            //      new SVec4(c.a, (int)(0), (int)(0), (int)(0)).PackARGBMax();
            //}
            //
        }
        else
        {
            //GLStride.CopyBytes(0, temp_geometry.pdata.data[(int)glsfDIFFUSE], 0, temp_geometry.pdata.data[(int)glsfSPECULAR], temp_geometry.num, 3);
        }

        dst.vb.Lock();

        dst.num_vtx = temp_geometry.num;

        Matrix34f m = dst.locus.GetTransformInv();
        //В оригинале здесь берётся указатель, так что изменения в массиве должны как-то отображаться и в самом буфере вершин.
        //GroundVertex[] dst_vb = dst.vb.vbuffer.GetDataFromIndex<GroundVertex>(dst.start_vtx);
        GroundVertex[] dst_vb = new GroundVertex[dst.num_vtx];
        GroundVertex[] src_vb = temp_geometry.pdata.lpdata as GroundVertex[];

        for (int i = 0; i != dst.num_vtx; ++i)
        {
            Debug.LogFormat("Probing [{0}] from src_vb as GroundVertex", src_vb == null ? "null" : src_vb[i]);
            GroundVertex vtx = new GroundVertex();
            vtx.color = src_vb[i].color;
            vtx.specular = src_vb[i].specular;
            vtx.tc = src_vb[i].tc;
            vtx.norm = src_vb[i].norm;
            vtx.pos = m.TransformPoint(src_vb[i].pos);
            dst_vb[i] = vtx;
            Debug.LogFormat("Saving [{0}] to dst_vb as GroundVertex @{1}+{2} of {3}", dst_vb == null ? "null" : dst_vb[i], dst.start_vtx, i,dst.num_vtx);
        }
        //В оригинале здесь берётся указатель, так что изменения в массиве должны как-то отображаться и в самом буфере вершин.
        dst.vb.vbuffer.SaveDataFromIndex<GroundVertex>(dst_vb, dst.start_vtx);

        temp_geometry.pdata.Unlock();

        /*
        for(int i=0;i<25;++i){
          VECTOR pos=*(VECTOR*)(temp_geometry.pdata.data[glsfPOSITION][i]);
          if( (pos.y<dst.lo) || (pos.y>dst.hi) ){
            AssertBp(0);
            Log.Message("Terrain : Error - bad box bounds : %f %f (y=%f)\n",
              dst.lo,dst.hi,pos.y);
          }
        } */
    }


    void LoadBoxPosition(ref GLStride dst, T_VBOX v_box, int sx, int sz, float ptx, float ptz)
    {
        int x, z;
        for (z = 0; z < 5; z++)
        {
            for (x = 0; x < 5; x++)
            {
                //Debug.LogFormat("dst {0} lod {1} lod.back_vidx[{2}][{3}] {4}", dst==null? "null": dst,lod==null?"null":lod, x,z,lod.back_vidx[x][z]);
                dst[lod.back_vidx[x][z]] =
                  //VECTOR( SQUARE_SIZE*2+ptx,GetHeight(sx+x,sz+z), SQUARE_SIZE*2+ptz);
                  new Vector3(x * SQUARE_SIZE + ptx, GetHeight(sx + x, sz + z), z * SQUARE_SIZE + ptz);
                //VECTOR( x*SQUARE_SIZE,GetHeight(sx+x,sz+z), z*SQUARE_SIZE);
            }
        }
    }

    void LoadBoxMesh(ref GLTopology mesh, T_VBOX v_box, int sx, int sz)
    {
        int x, z;

        mesh.num_tris = 96;

        int i = 0;
        for (z = 0; z < 4; ++z)
            for (x = 0; x < 4; i += 6, ++x)
            {
                lod.SetSqTris(ref mesh.tris, x, z, 1, (Data.Squares.pager.Get(sx + x, sz + z).Flag & SQF_SIMMETRY) != 0, i);
            }
    }

    public void DrawWater()
    {
        MaterialStorage.StaticWaterMaterial.SetTexture("_NormalMap", WaterDetailTexMap.GetTexture());

        //MaterialStorage.StaticWaterMaterial.mainTexture = WaterDetailTexMap.GetTexture();
        //float mindistance = 1E6f;
        //Vector3 LocalCoordinates;
        ////List<tmpTerrainChunkRemake> UpdatedChunks = new List<tmpTerrainChunkRemake>();
        //Vector3 distancePivot;
        //Vector2 flatPivot;
        //foreach (StormWaterChunk chunk in th.TerrainChunks)
        //{
        //    LocalCoordinates = chunk.WorldPosition - Engine.EngineCamera.Org;
        //    //float distance = new Vector2(LocalCoordinates.x+CENTEROFFSET,LocalCoordinates.z+CENTEROFFSET).magnitude;
        //    distancePivot = LocalCoordinates + new Vector3(TERRAINCENTEROFFSET, 0, TERRAINCENTEROFFSET);
        //    flatPivot = new Vector2(distancePivot.x, distancePivot.z);
        //    float distance = flatPivot.magnitude;
        //    if (distance < mindistance) mindistance = distance;

        //    if (distance > DRAW_DISTANCE) chunk.Undraw();
        //    if (distance <= DRAW_DISTANCE)
        //    {
        //        chunk.Draw();
        //        //UpdatedChunks.Add(chunk);
        //        //await Task.Yield();
        //        //LocalCoordinates = chunk.WorldPosition - pCameraData.myCamera.Org;
        //        chunk.TerrainGameObject.transform.position = Engine.ToCameraReference(LocalCoordinates);
        //        //GameObject tmpObject= GameObject.CreatePrimitive(PrimitiveType.Cube);
        //        //tmpObject.name = chunk.TerrainGameObject.name + " coord";
        //        //tmpObject.transform.position = LocalCoordinates + new Vector3(CENTEROFFSET,0,CENTEROFFSET);

        //    }
        //}
    }
    RasterizeData frame_raster;
    public void DrawWaterOld()
    {
        //{
        //    //preloading water boxes
        //    ERasterizer<StormTerrain> wlenumer = new ERasterizer<StormTerrain>(this, LoadWaterBox);

        //    frame_raster.Rasterize(wlenumer);
        //}

        //{
        //    ERasterizer<StormTerrain> wdenumer = new(this, DrawWaterBox);
        //    frame_raster.Rasterize(wdenumer);
        //}

    }

    int LoadSize;
    bool m_Visible;
    static int MaxRenderLine = 1024;
    static int[] AreaX0 = new int[MaxRenderLine];
    static int[] AreaX1 = new int[MaxRenderLine];
    static int[] Flag = new int[MaxRenderLine];
    public void Prepare()
    {
        int VisMin, VisMax;
        //int basebx = Mathf.FloorToInt((Engine.EngineCamera.Org.x - Engine.UnityCamera.farClipPlane) / BOX_SIZE),
        //    basebz = Mathf.FloorToInt((Engine.EngineCamera.Org.z - Engine.UnityCamera.farClipPlane) / BOX_SIZE);
        int basebx = Mathf.FloorToInt((Engine.EngineCamera.Org.x - CAMERA.end_dist) / BOX_SIZE),
            basebz = Mathf.FloorToInt((Engine.EngineCamera.Org.z - CAMERA.end_dist) / BOX_SIZE);

        m_Visible = Engine.ClipArea(basebx * BOX_SIZE, basebz * BOX_SIZE,
                          10000, 10000, out VisMin, out VisMax,
                          ref AreaX0, ref AreaX1,
                          OO_BOX_SIZE, LoadSize, LoadSize);
        if (m_Visible)
        {
            Assert.IsTrue(VisMin >= 0);
            Assert.IsTrue(VisMax < LoadSize);
            for (int iz = VisMin; iz <= VisMax; ++iz)
            {
                Assert.IsTrue(AreaX0[iz] >= 0 && AreaX0[iz] < LoadSize);
                Assert.IsTrue(AreaX1[iz] >= 0 && AreaX1[iz] < LoadSize);
                AreaX0[iz] += basebx;
                AreaX1[iz] += basebx;
            }

            int[] AreaX0trim = new int[AreaX0.Length - VisMin];
            int[] AreaX1trim = new int[AreaX1.Length - VisMin];
            Array.Copy(AreaX0, VisMin, AreaX0trim, 0, AreaX0trim.Length);
            Array.Copy(AreaX1, VisMin, AreaX1trim, 0, AreaX1trim.Length);
            //frame_raster = RasterizeData(basebz + VisMin, VisMax - VisMin + 1, AreaX0 + VisMin, AreaX1 + VisMin);
            frame_raster = new RasterizeData(basebz + VisMin, VisMax - VisMin + 1, AreaX0trim, AreaX1trim);
        }
    }

    public void Release() { }

    int bxclamp_max;
    int bzclamp_max;
    int bxclamp_min;
    int bzclamp_min;
    bool LoadWaterBox(int ix, int iz)
    {
        int BOXES_CLAMP = 1;

        int clamped_x = Storm.Math.ClampIntWrapped(ix, bxclamp_min, bxclamp_max, BOXES_CLAMP);
        int clamped_z = Storm.Math.ClampIntWrapped(iz, bzclamp_min, bzclamp_max, BOXES_CLAMP);

        int index = Storm.Math.Index2D(ix, iz);

        WaterBox Box = GetWaterBox(ix, iz);
        T_VBOX box = Data.VBoxes.pager.Get(clamped_x, clamped_z);

        if (Data.Boxes.pager.Get(clamped_x, clamped_z).Lo > box.water_hi) { Box.suzerland = -1; return true; }

        if (Box.index != index)
        {

            float
              lo = box.water_lo * HeightScale,
              hi = box.water_hi * HeightScale;
            float
              ptx = ix * BOX_SIZE,
              ptz = iz * BOX_SIZE;

            Vector3 center = new Vector3(ptx + BOX_SIZE * .5f, (lo + hi) * .5f, ptz + BOX_SIZE * .5f);
            float r = Mathf.Sqrt(Mathf.Pow(BOX_SIZE, 2) * 2 + Mathf.Pow(hi - lo, 2)) * .5f;

            Box.suzerland = Engine.EngineCamera.SphereVisibleEx(center, r);

            if (Box.suzerland == -1) return true;

            Box.center = center;
            Box.radius = r;
            Box.index = (uint)index;

            LoadWaterSpline(ref Box, clamped_x, clamped_z, ptx, ptz);
        }
        else
        {
            Box.suzerland = Engine.EngineCamera.SphereVisibleEx(Box.center, Box.radius);
            if (Box.suzerland == -1) return true;
        }

        float c_dist = (Engine.EngineCamera.Org - Box.center).magnitude;
        //Box.detail = ((c_dist - Box.radius) < GetDetailEnd()) ? 1 : 0;
        Box.detail = 1;

        return true;
    }

    public bool DrawWaterBox(int real_x, int real_z)
    {
        WaterBox Box = GetWaterBox(real_x, real_z);
        return DrawWaterBoxMesh(Box, real_x, real_z);
    }

    public bool DrawWaterBoxMesh(WaterBox Box, int x, int z)
    {
        GameObject gobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gobj.name = ("Water box " + x + ":" + z);
        gobj.transform.position = new Vector3(x, 0, z);
        GameObject.Destroy(gobj, 5);
        return true;
    }

    void LoadWaterSpline(ref WaterBox Box, int bx, int bz, float x, float z)
    {
        float[,] h ={
            {
                Data.VBoxes.pager.Get(bx, bz).water_level * HeightScale,
                Data.VBoxes.pager.Get(bx + 1, bz).water_level * HeightScale},
            {
                Data.VBoxes.pager.Get(bx, bz + 1).water_level * HeightScale,
                Data.VBoxes.pager.Get(bx + 1, bz + 1).water_level * HeightScale},
            };
        //Box.SplineData.Zero();
        //Box.SplineData.raw[0][0] = x; Box.SplineData.raw[1][0] = h[0][0]; Box.SplineData.raw[2][0] = z; Box.SplineData.raw[3][0] = 1;
        //Box.SplineData.raw[0][1] = x + BOX_SIZE; Box.SplineData.raw[1][1] = h[0][1]; Box.SplineData.raw[2][1] = z; Box.SplineData.raw[3][1] = 1;
        //Box.SplineData.raw[0][2] = x; Box.SplineData.raw[1][2] = h[1][0]; Box.SplineData.raw[2][2] = z + BOX_SIZE; Box.SplineData.raw[3][2] = 1;
        Box.SplineData[0, 3] = x + BOX_SIZE; Box.SplineData[1, 3] = h[1, 1]; Box.SplineData[2, 3] = z + BOX_SIZE; Box.SplineData[3, 3] = 1;
        Box.SplineData[0, 0] = x; Box.SplineData[1, 0] = h[0, 0]; Box.SplineData[2, 0] = z; Box.SplineData[3, 0] = 1;
        Box.SplineData[0, 1] = x + BOX_SIZE; Box.SplineData[1, 1] = h[0, 1]; Box.SplineData[2, 1] = z; Box.SplineData[3, 1] = 1;
        Box.SplineData[0, 2] = x; Box.SplineData[1, 2] = h[1, 0]; Box.SplineData[2, 2] = z + BOX_SIZE; Box.SplineData[3, 2] = 1;
        Box.SplineData[0, 3] = x + BOX_SIZE; Box.SplineData[1, 3] = h[1, 1]; Box.SplineData[2, 3] = z + BOX_SIZE; Box.SplineData[3, 3] = 1;
    }

    TerrainObjects VBoxes;
    WaterBox GetWaterBox(int ix, int iz) { return VBoxes.water[iz & (LoadSize - 1), ix & (LoadSize - 1)]; }
    TerrainBox GetGroundBox(int ix, int iz) { return VBoxes.data[iz & (LoadSize - 1), ix & (LoadSize - 1)]; }
}

public class WaterBox : aBox
{
    public int flags;
    //Matrix4f SplineData;
    public float[,] SplineData = new float[4, 4];
};

public class aBox
{
    public Vector3 center; // boundingsphere
    public float radius; // boundingsphere
    public float hi, lo;
    public int suzerland;
    public int detail;
    public int size;
    public uint index;
    public ITerrainLocus locus;
    public void Invalidate()
    {
        index = 0x80008000; locus = null;
    }
};

public interface ITerrainLocus : IObject
{
    public DWORD GetIndex();
    public Matrix34f GetTransform();
    public Matrix34f GetTransformInv();
};

public delegate bool TerrainAction(int x, int y);

public class MSurface
{
    public Texture2D texture;
    public D3DMATERIAL7 material;
};
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TMDFlags : Flags
{
    //TODO в исходника объявлено хитрым способом:
    //    /**********************************************/
    //    //FLAGS stuff
    //    /**********************************************/
    //    static const DWORD F_MIPMAPPED = 0x0001;
    //    static const DWORD F_MIPMAPPED_ALLOWED = F_MIPMAPPED << 16;
    //    static const DWORD F_RGB = 0x0002;
    //    static const DWORD F_RGB_ALLOWED = F_RGB << 16;
    //    static const DWORD F_BUMPDUDV = 0x0004;
    //    static const DWORD F_BUMPDUDV_ALLOWED = F_BUMPDUDV << 16;
    //    static const DWORD F_ALPHA = 0x0008;
    //    static const DWORD F_ALPHA_ALLOWED = F_ALPHA << 16;
    //    static const DWORD F_LUMINANCE = 0x0010;
    //    static const DWORD F_LUMINANCE_ALLOWED = F_LUMINANCE << 16;

    //#define DEFINE_FLAGMETHODS(CLASS,NAMESUFF,FLAG) \
    //    void CLASS::Set##NAMESUFF(bool state) { Set(FLAG,state);  }\
    // bool CLASS::Get##NAMESUFF()           { return Get(FLAG); }\
    // void CLASS::Set##NAMESUFF##_ALLOWED(bool state) { Set(FLAG##_ALLOWED,state);  }\
    // bool CLASS::Get##NAMESUFF##_ALLOWED()           { return Get(FLAG##_ALLOWED); }


    //DEFINE_FLAGMETHODS(TMDFlags, RGB, F_RGB)
    //   DEFINE_FLAGMETHODS(TMDFlags, ALPHA, F_ALPHA)
    //   DEFINE_FLAGMETHODS(TMDFlags, LUMINANCE, F_LUMINANCE)
    //   DEFINE_FLAGMETHODS(TMDFlags, MIPMAPPED, F_MIPMAPPED)
    //   DEFINE_FLAGMETHODS(TMDFlags, BUMPDUDV, F_BUMPDUDV)
    ///**********************************************/

    //    /**********************************************/
    //    //TYPE stuff
    //    /**********************************************/

    const DWORD F_MIPMAPPED = 0x0001;
    const DWORD F_MIPMAPPED_ALLOWED = F_MIPMAPPED << 16;
    const DWORD F_RGB = 0x0002;
    const DWORD F_RGB_ALLOWED = F_RGB << 16;
    const DWORD F_BUMPDUDV = 0x0004;
    const DWORD F_BUMPDUDV_ALLOWED = F_BUMPDUDV << 16;
    const DWORD F_ALPHA = 0x0008;
    const DWORD F_ALPHA_ALLOWED = F_ALPHA << 16;
    const DWORD F_LUMINANCE = 0x0010;
    const DWORD F_LUMINANCE_ALLOWED = F_LUMINANCE << 16;

    public void SetRGB(bool state) { Set((int)F_RGB, state); }
    public bool GetRGB() { return Get((int)F_RGB) != 0; }
    public void SetRGB_ALLOWED(bool state) { Set((int)F_RGB_ALLOWED, state); }
    public bool GetRGB_ALLOWED() { return Get((int)F_RGB_ALLOWED) != 0; }
    public void SetALPHA(bool state) { Set((int)F_ALPHA, state); }
    public bool GetALPHA() { return Get((int)F_ALPHA) != 0; }
    public void SetALPHA_ALLOWED(bool state) { Set((int)F_ALPHA_ALLOWED, state); }
    public bool GetALPHA_ALLOWED() { return Get((int)F_ALPHA_ALLOWED) != 0; }
    public void SetLUMINANCE(bool state) { Set((int)F_LUMINANCE, state); }
    public bool GetLUMINANCE() { return Get((int)F_LUMINANCE) != 0; }
    public void SetLUMINANCE_ALLOWED(bool state) { Set((int)F_LUMINANCE_ALLOWED, state); }
    public bool GetLUMINANCE_ALLOWED() { return Get((int)F_LUMINANCE_ALLOWED) != 0; }
    public void SetMIPMAPPED(bool state) { Set((int)F_MIPMAPPED, state); }
    public bool GetMIPMAPPED() { return Get((int)F_MIPMAPPED) != 0; }
    public void SetMIPMAPPED_ALLOWED(bool state) { Set((int)F_MIPMAPPED_ALLOWED, state); }
    public bool GetMIPMAPPED_ALLOWED() { return Get((int)F_MIPMAPPED_ALLOWED) != 0; }
    public void SetBUMPDUDV(bool state) { Set((int)F_BUMPDUDV, state); }
    public bool GetBUMPDUDV() { return Get((int)F_BUMPDUDV) != 0; }
    public void SetBUMPDUDV_ALLOWED(bool state) { Set((int)F_BUMPDUDV_ALLOWED, state); }
    public bool GetBUMPDUDV_ALLOWED() { return Get((int)F_BUMPDUDV_ALLOWED) != 0; }

};

public partial class StormTerrain //PatchPrepare
{
    private const float TEXTURE_SIZE = 1.0f;

    void LoadBoxTC(ref GLStride dst, T_VBOX v_box, int bx, int bz)
    {
        int x, z;
        FVec2 dTex = new FVec2(TEXTURE_SIZE / (GetTextureScale() * SQUARES_IN_BOX),
                             TEXTURE_SIZE / (GetTextureScale() * SQUARES_IN_BOX));
        FVec2 BaseTex = new FVec2(
                            (bx % GetTextureScale()) * (TEXTURE_SIZE / GetTextureScale()),
                            (bz % GetTextureScale()) * (TEXTURE_SIZE / GetTextureScale())
                            );
        for (z = 0; z < 5; z++)
        {
            for (x = 0; x < 5; x++)
            {
                (dst[lod.back_vidx[x][z]]) = dTex * (new FVec2(x, z)) + BaseTex;
            }
        }
    }
    void LoadBoxVertices(ref GLGeometry vert, T_VBOX v_box, int bx, int bz, float ptx, float ptz)
    {
        vert.num = 25;
        int
          sx = bx * SQUARES_IN_BOX,
          sz = bz * SQUARES_IN_BOX;

        vert.pdata.Lock(0);
        LoadBoxPosition(ref vert.pdata.data[(int)glsfPOSITION], v_box, sx, sz, ptx, ptz);
        LoadBoxNormal(ref vert.pdata.data[(int)glsfNORMAL], v_box, sx, sz);
        LoadBoxColor(ref vert.pdata.data[(int)glsfDIFFUSE], v_box, bx, bz);
        LoadBoxTC(ref vert.pdata.data[(int)glsfTEXTURE0], v_box, bx, bz);
        vert.pdata.Unlock();
    }

    void LoadBoxNormal(ref GLStride dst, T_VBOX v_box, int sx, int sz)
    {
        int x, z;
        for (z = 0; z < 5; z++)
        {
            for (x = 0; x < 5; x++)
            {
                dst[lod.back_vidx[x][z]] = GetNormal(sx + x, sz + z);
            }
        }
    }
    void LoadBoxMaterial(ref TerrainBox dst, T_VBOX v_box)
    {

        //materials establish
        Asserts.AssertBp(v_box.material[0] < Data.Header.nMaterials);

        dst.msurfacei[0] = v_box.material[0];
        dst.mstatei[0] = 0;

        dst.vscripti = 0;
        dst.ascripti = 0;

        if (v_box.material[1] != VBF_NO_LAYER)
        {
            dst.msurfacei[1] = v_box.material[1];
            dst.mstatei[1] = 1;
        }
        else
        {
            dst.msurfacei[1] = -1;
            dst.mstatei[1] = -1;
        }

        dst.msurfacei[2] = detail_surface;
        dst.mstatei[2] = detail_state;
    }
    void LoadBoxColor(ref GLStride dst, T_VBOX v_box, int bx, int bz)
    {

        switch (GetStaticLightingMode())
        {
            case TerrainCfg.TSLM_NORMAL:
                LoadBoxColorNormal(ref dst, v_box, bx, bz);
                break;
            case TerrainCfg.TSLM_EEI:
                LoadBoxColorEEI(ref dst, v_box, bx, bz);
                break;
        }
        ;
    }

    void LoadBoxColorNormal(ref GLStride dst, T_VBOX v_box, int bx, int bz)
    {
        LoadColor(0, ref dst, v_box, bx, bz);
    }

    void LoadBoxColorEEI(ref GLStride dst, T_VBOX v_box, int bx, int bz)
    {
        int x, z;
        int
          sx = bx * SQUARES_IN_BOX,
          sz = bz * SQUARES_IN_BOX;
        uint mask = 1;
        for (z = 0; z < 5; z++)
        {
            for (x = 0; x < 5; mask <<= 1, x++)
            {
                int f = ((GetFlag(sx + x, sz + z) & terrpdef.PASS_ANGLE_MASK) != 0) ? 0 : 255;
                dst[lod.back_vidx[x][z]] = new SVec4(((v_box.blend & mask) != 0 ? 255 : 0), 255 - f, 255 - f, f).PackARGB();
            }
        }
    }

    void LoadColor(int light_id, ref GLStride dst, T_VBOX v_box, int bx, int bz)
    {
        //TODO Проверить необходимость загрузки пре-рендереных теней
        //        int x, z;

        //        T_LIGHT lg = new[2][2];

        //        lg[0][0] = Data.Lights[light_id].pager.Get(bx, bz);
        //        lg[1][0] = Data.Lights[light_id].pager.Get(bx + 1, bz);
        //        lg[0][1] = Data.Lights[light_id].pager.Get(bx, bz + 1);
        //        lg[1][1] = Data.Lights[light_id].pager.Get(bx + 1, bz + 1);


        //#define get_color( src ) \
        //        c = (SVec4(255, 255, 255) * ((src & 3) * 21)) >> 6; \
        //  c.a = ((v_box->blend & mask) ? 255 : 0);


        //        unsigned int mask = 1;
        //        SVec4 c;
        //        for (z = 0; z < 4; ++z)
        //        {
        //            for (x = 0; x < 4; lg[0][0] >>= 2, mask <<= 1, ++x)
        //            {
        //                get_color(lg[0][0]);
        //                *(DWORD*)(dst[lod.back_vidx[x][z]]) = c.PackARGBMax();
        //            }

        //            get_color(lg[1][0]);
        //            *(DWORD*)(dst[lod.back_vidx[x][z]]) = c.PackARGBMax();
        //            lg[1][0] >>= 2 * 4;
        //            mask <<= 1;
        //        }

        //        for (x = 0; x < 4; lg[0][1] >>= 2, mask <<= 1, ++x)
        //        {
        //            get_color(lg[0][1]);
        //            *(DWORD*)(dst[lod.back_vidx[x][z]]) = c.PackARGBMax();
        //        }

        //        get_color(lg[1][1]);
        //        *(DWORD*)(dst[lod.back_vidx[x][z]]) = c.PackARGBMax();


        //#undef GetColor
    }
}

public partial class StormTerrain //TerrainImpl
{
    GLGeometry temp_geometry=new GLGeometry();
    TerrainPipe m_RenderPipe;
    T_LOD lod=new T_LOD();
    int detail_state;//detail state index

    void CreateRenderPipe()
    {
        Asserts.AssertBp(m_RenderPipe == null);
        BoxPipe pipe = new BoxPipe();
        if (!pipe.Initialize(this))
        {
            pipe.Release();
            m_RenderPipe = null;
        }
        m_RenderPipe = pipe;
    }
    int GetStaticLightingMode() { return (int)cur_statecfg.StaticLightingMode; }
    int GetTextureScale() { return cur_statecfg.texture_scale; }

    float GetDetailScale() { return cur_statecfg.detail_scale; }
    float GetDetailStartDist() { return cur_statecfg.detail_start; }
    float GetDetailEnd() { return cur_statecfg.detail_end; }
    float GetDetailEnabled() { return cur_statecfg.detail_enabled; }
    int GetWaterLod() { return cur_statecfg.water_lod; }
    int GetFlag(int x, int z)
    {
        return Data.Squares.pager.GetCl(x, z).Flag;
    }

    float GetHeight(int x, int z)
    {
        return Data.Squares.pager.GetCl(x, z).Height * HeightScale;
    }
}

public partial class StormTerrain //Aux for chunked
{
    public bool LoadGroundBoxChunked(int ix, int iz)
    {
        //DrawGroundChunks
        //Debug.LogFormat("Drawing chunk @[{0}:{1}]", ix, iz);

        //int clamped_x = Storm.Math.ClampIntWrapped(ix, bxclamp_min, bxclamp_max, BOXES_CLAMP);
        //int clamped_z = Storm.Math.ClampIntWrapped(iz, bzclamp_min, bzclamp_max, BOXES_CLAMP);

        //int index = Storm.Math.Index2D(ix, iz);
        //Debug.LogFormat("Drawing chunk @[{0}:{1}] index {2}", ix, iz,index);

        //Ленивая инициализация - не лучший способ, Но в процессе отладки - ничего так.
        //if (!StormUnityRendererTerrain.inited) StormUnityRendererTerrain.Init(Data, bxclamp_min, bxclamp_max, bzclamp_min, bzclamp_max);
        //StormUnityRendererTerrain.DrawBox(ix, iz);
        //return true;

        return true;
    }
}
public partial class StormTerrain //TerrainMain
{
    void AddToMList(TerrainBox Box)
    {

        m_RenderPipe.Add(new BoxRObj(Box, 0));

        if ((Box.mstatei[1] != -1) && (Box.mstatei[0] != Box.mstatei[1]))
        {
            m_RenderPipe.Add(new BoxRObj(Box, 1));
        }

        if (GetDetailEnabled() != 0 && Box.detail != 0)
        {
            m_RenderPipe.Add(new BoxRObj(Box, 2));
        }
    }
    bool LoadGroundBox(int ix, int iz)
    {


        int clamped_x = Storm.Math.ClampIntWrapped(ix, bxclamp_min, bxclamp_max, BOXES_CLAMP),
                  clamped_z = Storm.Math.ClampIntWrapped(iz, bzclamp_min, bzclamp_max, BOXES_CLAMP);

        int
          index = Storm.Math.Index2D(ix, iz);

        TerrainBox Box = GetGroundBox(ix, iz);

        //AssertBp(index!=0x00350025);

        if (Box.index != index)
        {
            T_BOX box = Data.Boxes.pager.Get(clamped_x, clamped_z);

            float
              lo = box.Lo * HeightScale,
              hi = box.Hi * HeightScale;

            float
              ptx = ix * BOX_SIZE,
              ptz = iz * BOX_SIZE;

            Vector3 center = new Vector3(ptx + BOX_SIZE * .5f, (lo + hi) * .5f, ptz + BOX_SIZE * .5f);
            float r = Mathf.Sqrt(Mathf.Pow(BOX_SIZE, 2) * 2f + Mathf.Pow((hi - lo), 2)) * .5f;

            //Box.suzerland=Engine::Camera.SphereVisibleEx( center, r);
            //    Box.suzerland=Engine::Camera.SphereVisibleEx( center, r);
            //    if ( Box.suzerland==-1 ) return true;

            //    if(Box.suzerland){
            PLANE[] pln = new PLANE[1];
            Box.suzerland = Engine.EngineCamera.BoxVisible(lo, hi, center.x, center.z, BOX_SIZE, r, ref pln);
            //    }
            if (Box.suzerland == -1) return true;

            Asserts.AssertBp(hi >= lo);
            if (scene.GetHideUnderwater())
            {
                T_VBOX vbox = Data.VBoxes.pager.Get(clamped_x, clamped_z);

                Asserts.AssertBp(vbox.water_lo <= vbox.water_level);
                Asserts.AssertBp(vbox.water_hi >= vbox.water_level);

                if (vbox.water_lo * HeightScale > hi)
                {
                    Asserts.AssertBp(vbox.water_lo * HeightScale >= lo);
                    Asserts.AssertBp(vbox.water_hi * HeightScale >= hi);
                    return true;
                }
            }

            Box.lo = lo;
            Box.hi = hi;
            Box.center = center;
            Box.radius = r;
            Box.index = (uint)index;

            int
              sx = clamped_x * SQUARES_IN_BOX,
              sz = clamped_z * SQUARES_IN_BOX;

            Box.locus = VBoxes.m_Loci.CreateTerrainLocus(ix, iz);


            T_VBOX v_box = Data.VBoxes.pager.Get(clamped_x, clamped_z);
            LoadBoxVertices(ref temp_geometry, v_box, clamped_x, clamped_z, ptx, ptz);
            GLTopology top = new GLTopology(0, Box.idxs);
            LoadBoxMesh(ref top, v_box, sx, sz);
            Box.num_idx = top.num_tris;
            LoadBoxMaterial(ref Box, v_box);
            //applying static shadows
            //На самом деле - нет. Их рисует сам движок Unity
            //      SSApplier ss(
            //        Stride(temp_geometry.pdata.data[glsfPOSITION].ptr, temp_geometry.pdata.data[glsfPOSITION].stride ),
            //Stride(temp_geometry.pdata.data[glsfDIFFUSE].ptr, temp_geometry.pdata.data[glsfDIFFUSE].stride) );
            //      Sphere s(Box.center, BOX_SIZE);
            //      scene.objects.hasher.EnumSphere(s, ROObjectId(ROFID_STATICSHADOW), ss);

            AlignBox(Box);
        }
        else
        {
            //    Box.suzerland=Engine::Camera.SphereVisibleEx( Box.center, Box.radius);
            //    if ( Box.suzerland==-1 ) return true;
            //    if ( Box.suzerland ){
            PLANE[] pln = new PLANE[1];
            Box.suzerland = Engine.EngineCamera.BoxVisible(Box.lo, Box.hi, Box.center.x, Box.center.z, BOX_SIZE, Box.radius, ref pln);
            //    }
            if (Box.suzerland == -1) return true;

            if (scene.GetHideUnderwater())
            {
                T_VBOX vbox = Data.VBoxes.pager.Get(clamped_x, clamped_z);
                if (vbox.water_lo * HeightScale > Box.hi)
                    return true;
            }

        }
        float c_dist = (Engine.EngineCamera.Org - Box.center).magnitude;
        Box.detail = ((c_dist - Box.radius) < GetDetailEnd()) ? 1 : 0;

        AddToMList(Box);

        return true;
    }
}


public partial class StormTerrain : iTerrain //terrainutil.cpp
{
    public const int MaxExportDist = 2000;

    public geombase.Rect GetRect(float[] area)
    {
        return new geombase.Rect(
          (int)(area[0] * OO_SQUARE_SIZE - .5f),
          (int)(area[1] * OO_SQUARE_SIZE - .5f),
          (int)(area[2] * OO_SQUARE_SIZE + .5f),
          (int)(area[3] * OO_SQUARE_SIZE + .5f)
          );
    }

    public geombase.Rect ClipRect(geombase.Rect inRect)
    {
        geombase.Rect res = new geombase.Rect();
        res.x0 = Mathf.Clamp(inRect.x0, 1, Data.Squares.pager.SizeXPages() * SQUARES_PAGE_SIZE - 2);
        res.x1 = Mathf.Clamp(inRect.x1, 1, Data.Squares.pager.SizeXPages() * SQUARES_PAGE_SIZE - 2);
        res.y0 = Mathf.Clamp(inRect.y0, 1, Data.Squares.pager.SizeZPages() * SQUARES_PAGE_SIZE - 2);
        res.y1 = Mathf.Clamp(inRect.y1, 1, Data.Squares.pager.SizeZPages() * SQUARES_PAGE_SIZE - 2);
        return res;
    }

    public int RectVtxCount(geombase.Rect rect) { return (rect.x1 - rect.x0 + 1) * (rect.y1 - rect.y0 + 1); }
    public int RectTriCount(geombase.Rect rect) { return (rect.x1 - rect.x0) * (rect.y1 - rect.y0) * 2 * 3; }

    public Vector3 GetNormal(int x, int z)
    {
        Vector3 V = new Vector3(
          GetHeight(x - 1, z) -
          GetHeight(x + 1, z),
            (2 * SQUARE_SIZE),
            GetHeight(x, z - 1) -
            GetHeight(x, z + 1)
        );
        V.Normalize();
        return V;
    }

    public int GetLight(int x, int z)
    {
        //int bx = x / SQUARES_IN_BOX, bz = z / SQUARES_IN_BOX;
        //T_LIGHT lg = ((Data.Lights[0].pager.Get(bx, bz)) >> (8 * (z - bz * SQUARES_IN_BOX))) >> (2 * (x - bx * SQUARES_IN_BOX));
        //return (lg & 3) * 85;
        return 0; //TODO Стоит или извлекать данные о тенях,если оно генерируется Unity?
    }

    public void ExportVertices(geombase.Rect rect, ref Stride<Vector3> dest)
    {
        int i = 0;
        for (int z = rect.y0; z <= rect.y1; z++)
        {
            for (int x = rect.x0; x <= rect.x1; i++, x++)
            {
                Vector3 v = new Vector3(x * SQUARE_SIZE, GetHeight(x, z), z * SQUARE_SIZE);
                //dest.Ref<Vector3>(i) = v;
                dest[i] = v;
            }
        }
    }

    public void ExportNormals(geombase.Rect rect, ref Stride<Vector3> dest)
    {
        int i = 0;
        for (int z = rect.y0; z <= rect.y1; z++)
        {
            for (int x = rect.x0; x <= rect.x1; i++, x++)
            {
                //dest.Ref<Vector3>(i) = GetNormal(x, z);
                dest[i] = GetNormal(x, z);
            }
        }
    }

    //exports lightmap
    public void ExportColors(geombase.Rect rect, ref Stride<uint> dest)
    {
        int i = 0;
        for (int z = rect.y0; z <= rect.y1; z++)
        {
            for (int x = rect.x0; x <= rect.x1; i++, x++)
            {
                //dest.Ref<int>(i) = GetLight(x, z);
                //dest.SetRef(i)
                dest[i] = (uint) GetLight(x, z);
            }
        }
    }

    public void ExportTriangles(geombase.Rect rect, ref WORD[] dest)
    {
        int xdim = (rect.x1 - rect.x0 + 1);
        int destIndex = 0;
        for (int z = 0; z < rect.y1 - rect.y0; z++)
        {
            for (int x = 0; x < rect.x1 - rect.x0; destIndex += 6, x++)
            {
                T_SQUARE b = Data.Squares.pager.Get(x + rect.x0, z + rect.y0);
                if ((b.Flag & SQF_SIMMETRY) != 0)
                {
                    dest[destIndex + 0] = (WORD)((x) + (z) * xdim);
                    dest[destIndex + 2] = (WORD)((x + 1) + (z + 1) * xdim);
                    dest[destIndex + 1] = (WORD)((x) + (z + 1) * xdim);
                    dest[destIndex + 3] = (WORD)((x) + (z) * xdim);
                    dest[destIndex + 5] = (WORD)((x + 1) + (z) * xdim);
                    dest[destIndex + 4] = (WORD)((x + 1) + (z + 1) * xdim);
                }
                else
                {
                    dest[destIndex + 0] = (WORD)((x) + (z) * xdim);
                    dest[destIndex + 2] = (WORD)((x + 1) + (z) * xdim);
                    dest[destIndex + 1] = (WORD)((x) + (z + 1) * xdim);
                    dest[destIndex + 3] = (WORD)((x + 1) + (z + 1) * xdim);
                    dest[destIndex + 5] = (WORD)((x) + (z + 1) * xdim);
                    dest[destIndex + 4] = (WORD)((x + 1) + (z) * xdim);
                }
            }
        }
    }

    public int GetMaxRasterSize(float R)
    {
        return 1024;
    }

    public int GetMaxRasterVertices(RasterizeData r)
    {
        return 1024;
    }

    public int GetMaxRasterIndices(RasterizeData r)
    {
        return 3096;
    }

    public RasterizeData GetRaster(Matrix34f Cam, int[] aleft, int[] aright)
    {
        int basex = (int)Mathf.Floor((Cam.pos.x - MaxExportDist) * OO_SQUARE_SIZE),
            basez = (int)Mathf.Floor((Cam.pos.z - MaxExportDist) * OO_SQUARE_SIZE);

        int min, max;
        Engine.ClipTube(Cam, out min, out max, aleft, aright, OO_SQUARE_SIZE, 254, 254, basex, basez);
        for (int i = min; i <= max; ++i)
        {
            aleft[i] += basex;
            aright[i] += basex;
        }

        //TODO Возможно, лучше ArraySegment?
        int tmpaleftsize = aleft.Length - min;
        int[] tmpaleft = new int[tmpaleftsize];
        int tmparightsize = aright.Length - min;
        int[] tmparight = new int[tmparightsize];
        Array.Copy(aleft, min, tmpaleft, 0, tmpaleftsize);
        Array.Copy(aright, min, tmparight, 0,tmparightsize);
        //return new RasterizeData(min + basez, max - min, aleft + min, aright + min);
        return new RasterizeData(min + basez, max - min, tmpaleft, tmparight);
    }

    public void ExportRaster(RasterizeData r, ref Stride<Vector3> pos, int n_vtx, WORD[] tris, int n_idx)
    {
        int x, z;

        int[] idx0 = new int[256];
        int[] idx1 = new int[256];
        int[] idn = new int[256];
        int prevleft = r.left[0], prevright = r.right[0] + ((r.left[0] == r.right[0]) ? 1 : 0);
        int i = 0;
        for (z = r.starty; z <= r.starty + r.nlines; ++z)
        {
            int right = r.right[z - r.starty] + ((r.left[z - r.starty] == r.right[z - r.starty]) ? 1 : 0);
            int startx = (prevleft < r.left[z - r.starty]) ? prevleft : r.left[z - r.starty];
            int endx = (prevright > right) ? prevright : right;
            Asserts.AssertBp(startx < endx);
            idx0[z - r.starty] = i + r.left[z - r.starty] - startx;
            idx1[z - r.starty] = i + prevleft - startx;
            idn[z - r.starty] = right - r.left[z - r.starty];
            for (x = startx; x <= endx; i++, ++x)
            {
                Vector3 v = new Vector3(x * SQUARE_SIZE, GetHeight(x, z), z * SQUARE_SIZE);
                //pos.Ref<Vector3>(i) = v;
                pos[i] = v;
            }
            prevleft = r.left[z - r.starty]; prevright = right;
        }
        n_vtx = i;

        WORD[] t = tris;
        int tIndex = 0;
        for (z = r.starty; z < r.starty + r.nlines; ++z)
        {
            int i0 = idx0[z - r.starty];
            int i1 = idx1[z - r.starty + 1];
            for (x = r.left[z - r.starty]; x < r.left[z - r.starty] + idn[z - r.starty]; tIndex += 6, i0++, i1++, ++x)
            {
                Asserts.AssertBp(i0 + 1 < i && i0 >= 0 && i1 + 1 < i && i1 >= 0);
                T_SQUARE b = Data.Squares.pager.Get(x, z);
                if ((b.Flag & SQF_SIMMETRY) != 0)
                {
                    t[tIndex+0] = (WORD) (i0);
                    t[tIndex + 2] = (WORD)( i1 + 1);
                    t[tIndex + 1] = (WORD)( i1);
                    t[tIndex + 3] = (WORD)(i0);
                    t[tIndex + 5] = (WORD)(i0 + 1);
                    t[tIndex + 4] = (WORD)(i1 + 1);
                }
                else
                {
                    t[tIndex + 0] = (WORD)(i0);
                    t[tIndex + 2] = (WORD)(i0 + 1);
                    t[tIndex + 1] = (WORD)(i1);
                    t[tIndex + 3] = (WORD)(i1 + 1);
                    t[tIndex + 5] = (WORD)(i1);
                    t[tIndex + 4] = (WORD)(i0 + 1);
                }
            }
        }
        //n_idx = t - tris;
        n_idx = tIndex;
    }
}
