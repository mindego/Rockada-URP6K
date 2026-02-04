using System;
using System.Collections.Generic;
using UnityEngine;
using static CppFunctionEmulator;
using static D3DEMULATION;
using static renderer_dll;
using DWORD = System.UInt32;
//struct VBufferObject : TRefMem2<VBufferImpl> {};
using VBufferObject = VBufferImpl;
using crc32 = System.UInt32;
using System.IO;

/// <summary>
/// Main dll functions
/// Эмуляция загрузки dll
/// </summary>
public static class renderer_dll
{
    public static LOG Log = null, StdOut = null;
    public static RendererDllData dll_data;
    public static Cd3d d3d;

    public static StormUnityRendererD3D d3dunity;

    public static bool DllMain()
    {
        return Attach();
    }

    public static bool Attach()
    {
        Debug.Log("Renderer loading ...");
        Log = (LOG)LogFactory.CreateLOG("Renderer");
        dll_data = new RendererDllData();
        dll_data.Initialize();
        return true;
    }

    public static bool Detach()
    {
        dll_data.Release();
        Log.Release();
        return true;
    }

    public static uint Version()
    {
        return RendererApi.RENDERER_VERSION;
    }
}


public class RendererDllData
{
    const string ParticlesDb = "particles.dat";
    const string RDataDb = "rdata.dat";
    const string RSatesDb = "dxm.dat";
    const string MaterialsDb = "materials.dat";
    const string MeshesDb = "mesh.dat";
    const string FilesPath = "Graphics\\";
    const string FilesError = "Sharing access violation or mssing datafile \"{0}{1}\"\n" +
                                    "Plase, verify installation procedure and Your SourceSafe status !";

    public IMappedDb files;

    IMappedDb rstates;
    public IMappedDb materials;
    public IMappedDb meshes;
    public IMappedDb particles;

    bool can_write;

    TexturesDB textures;
    TexturesDB myFpoTextures;

    public T LoadFile<T>(ObjId id) where T : class, IStormImportable<T>, new()
    {
        try
        {
            return files.GetBlock(id).Convert<T>();
        }
        catch
        {
            Debug.Log("Failed to load file: " + id.ToString());
            throw;
        }
    }
    public void Initialize()
    {
        files = IMappedDb.CreateMappedDb(IMappedDb.DBFORMAT_NAKED);
        //rstates = CreateMappedDb(*(int*)"DXMC");
        //materials = IMappedDb.CreateMappedDb("DMAT");
        materials = IMappedDb.CreateMappedDb(IMappedDb.DBFORMAT_NAKED);
        meshes = IMappedDb.CreateMappedDb(IMappedDb.DBFORMAT_NAKED);
        //particles = IMappedDb.CreateMappedDb("PARS");
        particles = IMappedDb.CreateMappedDb(IMappedDb.DBFORMAT_NAKED);


        mTextureDataBases = new TextureDataBases(false);
        //textures = CreateTexturesDB("Graphics\\textures.dat");
        //textures = CreateTexturesDB("Graphics/textures.dat");
        textures = CreateTexturesDB("Graphics\\textures.dat");

    }

    public TexturesDB CreateTexturesDB(string path)
    {
        //TexturesDB db = new TexturesDB();
        //if (!db.Initialize(path)) throw new System.Exception(string.Format(FilesError, path));

        //return db;

        //string _path = StrDown(StrDup(path));
        string _path = path;
        mTextureDataBases.SetData(_path);
        TexturesDB db = mTextureDataBases.CreateObject(Hasher.HshString(_path));
        //delete _path;
        return db;
    }

    public bool openFpoTextures(string path)
    {
        if (path == null) path = "";

        myFpoTextures = path != "" ? CreateTexturesDB(path) : textures;
        return myFpoTextures != null;
    }

    public bool OpenBase(IMappedDb db, string name, ref bool write)
    {
        string refpath = FilesPath + name;
        Debug.Log("Opening " + name);
        if (db.Open(ProductDefs.GetPI().getHddFile(refpath), write) != DBDef.DB_OK)
        {
            if (write)
            {
                write = false;

                if (db.Open(ProductDefs.GetPI().getHddFile(refpath), false) == DBDef.DB_OK)
                    return true;

            }
            Debug.Log(string.Format(refpath, FilesError, FilesPath, name));
            //MessageBox(0, string, "Renderer.dll initialization error", MB_ICONERROR);
            return false;
        }
        return true;
    }

    public bool Open(bool _CanWrite)
    {
        can_write = _CanWrite;
        bool _false = false;
        //return
        //    OpenBase(files, RDataDb, ref can_write) &&
        //    OpenBase(materials, MaterialsDb, ref can_write) &&
        //    OpenBase(rstates, RSatesDb, ref _false) &&
        //    OpenBase(meshes, MeshesDb, ref _false) &&
        //    OpenBase(particles, ParticlesDb, ref _false);
        return
            OpenBase(files, RDataDb, ref can_write) &&
            OpenBase(materials, MaterialsDb, ref can_write) &&
            OpenBase(meshes, MeshesDb, ref _false) &&
            OpenBase(particles, ParticlesDb, ref _false);
    }

    void Close()
    {
        particles.Close();
        meshes.Close();
        //rstates.Close();
        materials.Close();
        files.Close();
    }

    public MeshData GetFpoMesh(ObjId id)
    {
        return meshes.GetBlock(id).Convert<MeshData>();
    }

    public FpoGraphData GetFpoGraph(ObjId id)
    {
        return meshes.GetBlock(id).Convert<FpoGraphData>();
    }

    public PARTICLE_DATA GetParticleData(ObjId id)
    {
        return particles.GetBlock(id).Convert<PARTICLE_DATA>();
    }

    public StormMesh GetStormMesh(ObjId id)
    {
        return meshes.GetBlock(id).Convert<StormMesh>();
    }

    private const int TFF_BUMP = 0x00000001;

    public Texture2D loadTexture(TexturesDB mydb, ObjId id, bool bump)
    {
        //# ifdef _DEBUG
        //        cstr name = base->GetMappedDb()->CompleteObjId(id).name;
        //    ::GetLog().Message("Texture:%s", name ? name : "<unknown>");
        //#endif
        mydb.Flags().Set(TFF_BUMP, bump);
        Texture2D myobject = mydb.CreateTexture(id.name != null ? Hasher.HshString(id.name) : id.obj_id);
        mydb.Flags().Set(TFF_BUMP, false);
        //return myobject ? (Texture*)object->Query<Texture>() : 0;
        return myobject;
    }

    public Texture2D LoadTexture(ObjId id, bool bump = false) { return loadTexture(textures, id, bump); }
    public Texture2D loadFpoTexture(ObjId id, bool bump = false) { Debug.LogFormat("loadFpoTexture {0} bump {1}",id,bump); return loadTexture(myFpoTextures, id, bump); }

    TextureDataBases mTextureDataBases;
    public void Release()
    {
        myFpoTextures = null;
        textures = null;
        SafeRelease(files);
        SafeRelease(materials);
        SafeRelease(rstates);
        SafeRelease(meshes);
        SafeRelease(particles);
        if (mTextureDataBases != null) mTextureDataBases.Destroy();
        //delete mTextureDataBases;
    }

    //    D3DMATERIAL7 DllData::DefaultD3DM={
    //        1,1,1,1,
    //        0,0,0,0,
    //        0,0,0,0,
    //        0,0,0,0,
    //        1
    //};
    D3DMATERIAL7 DefaultD3DM
    {
        get
        {
            D3DMATERIAL7 res = new D3DMATERIAL7();

            res.diffuse = new D3DCOLORVALUE(1, 1, 1, 1);
            res.ambient = new D3DCOLORVALUE(0, 0, 0, 0);
            res.specular = new D3DCOLORVALUE(0, 0, 0, 0);
            res.emissive = new D3DCOLORVALUE(0, 0, 0, 0);
            res.power = 1;

            return res;
        }
    }


#if _DEBUG
    public D3DMATERIAL7 LoadMaterial(ObjId id)
    {
        D3DMATERIAL7 m = materials.GetBlock(id).Convert<D3DMATERIAL7>();
        string MaterialWarning = "Warning: Can't Load D3DMATERIAL7 ";
        if (m != null)
        {
            renderer_dll.Log.Message("Loaded D3DMATERIAL7 {0}\n", renderer_dll.dll_data.materials.CompleteObjId(id).name);
        }
        else
        {
            renderer_dll.Log.Message(id.name != null ? "%s%s\n" : "%s%s 0x%p\n", MaterialWarning, id.name != null ? id.name : "<by code>", id.obj_id);
        }

        return m != null ? m : DefaultD3DM;
    }
#else
    public D3DMATERIAL7 LoadMaterial(ObjId id)
    {
        D3DMATERIAL7 m = materials.GetBlock(id).Convert<D3DMATERIAL7>();
        int hash = m.GetHashCode();
        if (!MaterialNames.ContainsKey(hash)) MaterialNames.Add(hash, id.name);
        return m != null ? m : DefaultD3DM;
    }
#endif
    /// <summary>
    /// Возможно, стоит вынести в отдельный статический класс и использовать его для хранения разных строк.
    /// </summary>
    public static Dictionary<int,string> MaterialNames = new Dictionary<int,string>();
    private void SafeRelease(IMappedDb db)
    {
        if (db != null) db.Release();
    }

    Dictionary<string, iRS> rsCache = new Dictionary<string, iRS>();
    internal iRS CreateRS(ObjId id)
    {
        iRS renderstate_stub;
        if (rsCache.TryGetValue(id.name, out renderstate_stub))
        {
            return renderstate_stub;
        }

        renderstate_stub = new RS(id.name);
        rsCache.Add(id.name,renderstate_stub);
        return renderstate_stub;

    }

    internal byte[] GetFpoVert(ObjId id)
    {
        //return meshes.GetBlock(id).Ptr();
        //throw new System.NotImplementedException();
        return meshes.GetBlock(id).buffer;

        //byte[] data = meshes.GetBlock(id).buffer;
        //string debugfname = "FpoVert#" + id.name + "#"+id.crc32().ToString("X8");
        //FileStream fs = new FileStream(debugfname, FileMode.Create);
        //fs.Write(data);
        //fs.Close();

        //return data;
    }
}

public class RS : iRS
{
    string name;
    int mRefCount;
    public RS(string _name)
    {
        name = _name;
        mRefCount = 0;
    }
    public void AddRef()
    {
        mRefCount++;
    }

    public HRESULT Apply()
    {
        Engine.CurrentRenderingStage = name;
        return HRESULT.S_OK;
    }

    public int Release()
    {
        if (--mRefCount > 0) return mRefCount;
        //TODO somehow clean
        return 0;
    }
}
public class TextureDataBases : IndexFactory<crc32,TexturesDB>, IDisposable
{
    public TextureDataBases(bool persist) : base(persist) { }
    public void SetData(string path)
    {
        mData = path;
    }
    public override bool InitializeObject(TexturesDB myObject)
    {
        //return base.InitializeObject(myObject);
        return myObject.Initialize(mData);
    }
    public override void DestroyObject(TexturesDB myobject)
    {
        //object->Destroy();
        //delete object;
        myobject.Destroy();
        myobject.Dispose();
    }
    protected string mData;
    public void Dispose()
    {

    }
};

public class VBufferImpl : VBuffer
{
    private IDirect3D7 d3d;
    VBManager vbmanager;

    public VBufferImpl()
    {
        d3d = null;
    }
    public bool Initialize(VBManager _vbmanager, IDirect3D7 _d3d, int size, DWORD _Caps, DWORD _FVF = D3DFVF_TLVERTEX)
    {
        Asserts.AssertBp(vb==null);
        d3d = _d3d;
        vbmanager = _vbmanager;
        d3d.AddRef();

        FVF = _FVF;
        Caps = _Caps;

        D3DVERTEXBUFFERDESC VbDesc;

        //VbDesc.dwSize = sizeof(D3DVERTEXBUFFERDESC);
        VbDesc.dwSize = (uint) D3DVERTEXBUFFERDESC.GetSize(); //4 DWORD 4 bytes each
        VbDesc.dwCaps = Caps;
        VbDesc.dwFVF = FVF;
        VbDesc.dwNumVertices = (uint) size;

        HRESULT hr = d3d.CreateVertexBuffer(VbDesc, out vb, 0);
        return SUCCEEDED(hr);
    }
};
public class VBManager : iVBManager
{

    int max_pipes;
    VBPipeImpl[] pipes;
    IDirect3D7 lpD3D;
    IDirect3DDevice7 lpDevice;
    //std::set<VBuffer*> vbuffers;
    List<VBuffer> vbuffers = new List<VBuffer>();

    DWORD DrawableTLVBMemoryType;
    DWORD DrawableUnTVBMemoryType;
    ~VBManager() { }

    //DWORD GetVBCaps(int needed_size, DWORD FVF, DWORD flags);

    //VBPipeImpl CreateNewVBPipe(int needed_size, DWORD FVF, DWORD flags);

    public VBManager()
    {
        lpDevice = null;
        lpD3D = null;
        pipes = new VBPipeImpl[max_pipes = 8];
        for (int i = 0; i < max_pipes; ++i)
            pipes[i] = null;
    }
    public bool Initialize(IDirect3DDevice7 _lpDevice)
    {
        lpDevice = _lpDevice;
        Debug.LogFormat("_lpDevice {0}!", _lpDevice == null ? "null" : _lpDevice);
        lpDevice.AddRef();

        HRESULT hr = lpDevice.GetDirect3D(out lpD3D);

        if (FAILED(hr))
        {
            Log.Message("Error : VBManager can't get Direct3D!!!");
            return false;
        }

        D3DDEVICEDESC7 Caps;
        hr = lpDevice.GetCaps(out Caps);

        //DrawableTLVBMemoryType = ((Caps.dwDevCaps & D3DDEVCAPS_HWRASTERIZATION) && d3d.caps.use_hw_raster) ? 0 : D3DVBCAPS_SYSTEMMEMORY;
        //Log->Message("Transformed Vertex is in %s memory.\n",
        //  (DrawableTLVBMemoryType & D3DVBCAPS_SYSTEMMEMORY) ? "System" : "Device");
        //DrawableUnTVBMemoryType = ((Caps.dwDevCaps & D3DDEVCAPS_HWTRANSFORMANDLIGHT) && d3d.caps.use_hw_tnl) ? 0 : D3DVBCAPS_SYSTEMMEMORY;
        //Log->Message("UnTransformed Vertex is in %s memory.\n",
        //  (DrawableUnTVBMemoryType & D3DVBCAPS_SYSTEMMEMORY) ? "System" : "Device");

        return true;
    }
    public void restore()
    {
        //for (std::set<VBuffer*>::iterator It = vbuffers.begin(); It != vbuffers.end(); ++It)
        //{
        //    VBuffer* vb = (*It);
        //    vb->invalidate();
        //}
    }

    //interface with user

    public VBPipe CreateVBPipe(int needed_size, DWORD FVF, DWORD flags)
    {
        //STUB
        return null;
    }

    void LogVertexBufferCaps(DWORD Caps)
    {
        Log.Message("Creating Vertex Buffer : {0} memory, {1}, {2}.\n",
          (Caps & D3DVBCAPS_SYSTEMMEMORY) != 0 ? "System" : "Device",
          (Caps & D3DVBCAPS_WRITEONLY) != 0 ? "Writeonly" : "Readable",
          (Caps & D3DVBCAPS_DONOTCLIP) != 0 ? "Not clippable" : "clippable"
          );
    }

    public VBuffer CreateVBuffer(int needed_size, DWORD FVF, DWORD flags)
    {
        //TODO Полностью реализовать создание VBuffer
        //VBuffer vb = new VBuffer();

        //return vb;
        DWORD Caps = GetVBCaps(needed_size, FVF, flags);
        LogVertexBufferCaps(Caps);

        VBufferImpl vb = new VBufferObject();
        
        if (!vb.Initialize(this, lpD3D, needed_size, Caps, FVF))
        {
            Log.Message("VBManager : Can't create VBuffer.");
            vb.Release();
            return null;
        }
        //vbuffers.insert(vb);
        vbuffers.Add(vb);
        return vb;
    }

    //TODO! Перенести константы в отдельное место
    public const uint VBF_DRAWABLE = 0x00000001;
    public const uint VBF_READABLE = 0x00000002;
    public const uint VBF_CLIPPED = 0x00000004;
    private DWORD GetVBCaps(int needed_size, DWORD FVF, DWORD flags)
    {

        DWORD MemoryType = ((flags & VBF_DRAWABLE) == 0) ? D3DVBCAPS_SYSTEMMEMORY :
        (((FVF & D3DFVF_POSITION_MASK) != 0) & (~D3DFVF_XYZRHW) != 0)
          ? DrawableUnTVBMemoryType : DrawableTLVBMemoryType;

        DWORD AccessType =
          ((flags & VBF_READABLE) != 0 ? 0 : D3DVBCAPS_WRITEONLY) |
          ((flags & VBF_CLIPPED) != 0 ? 0 : D3DVBCAPS_DONOTCLIP);

        return AccessType | MemoryType;
    }

    //iVBManager CreateVBManager(IDirect3DDevice7 lpD3D)
    //{
    //    VBManager manager = new VBManager();
    //    if (manager.Initialize(lpD3D))
    //        return manager;
    //    Log.Message("VBManager : Can't create VBManager.");
    //    manager.Release();
    //    return null;
    //}

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }
};
