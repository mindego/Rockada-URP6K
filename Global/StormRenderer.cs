using UnityEngine;

//public class StormRendererData
//{
//    const string ParticlesDb = "particles.dat";
//    const  string RDataDb = "rdata.dat";
//    const  string RSatesDb = "dxm.dat";
//    const  string MaterialsDb = "materials.dat";
//    const string MeshesDb = "mesh.dat";
//    // const  string FilesPath = "Graphics\\";
//    const string FilesPath = "Graphics/";
//    const string FilesError = "Sharing access violation or mssing datafile \"{0}{1}\"\n" + 
//                                    "Plase, verify installation procedure and Your SourceSafe status !";

//    private static StormRendererData instance;

//    public static StormRendererData GetInstance()
//    {
//        if (instance == null)
//        {
//            instance = new StormRendererData();
//            instance.Initialize();
//            instance.Open(false);
//        }
        
//        return instance;
//    }
//    public IMappedDb files;

//    IMappedDb rstates;
//    public IMappedDb materials;
//    public IMappedDb meshes;
//    public IMappedDb particles;

//    bool can_write;

//    TexturesDB textures;
//    TexturesDB myFpoTextures;

//    public void Initialize()
//    {
//        files = IMappedDb.CreateMappedDb(IMappedDb.DBFORMAT_NAKED);
//        //rstates = CreateMappedDb(*(int*)"DXMC");
//        //materials = IMappedDb.CreateMappedDb("DMAT");
//        materials = IMappedDb.CreateMappedDb(IMappedDb.DBFORMAT_NAKED);
//        meshes = IMappedDb.CreateMappedDb(IMappedDb.DBFORMAT_NAKED);
//        //particles = IMappedDb.CreateMappedDb("PARS");
//        particles = IMappedDb.CreateMappedDb(IMappedDb.DBFORMAT_NAKED);


//        //        mTextureDataBases = new TextureDataBases(false);
//        //textures = CreateTexturesDB("Graphics\\textures.dat");
//        //textures = CreateTexturesDB("Graphics/textures.dat");
//        textures = CreateTexturesDB("Graphics\\textures.dat");

//    }

//    public static TexturesDB CreateTexturesDB(string path)
//    {
//        TexturesDB db = new TexturesDB();
//        if (!db.Initialize(path)) throw new System.Exception(string.Format(FilesError,path));

//        return db;
//    }

//    bool openFpoTextures(string path)
//    {
//        myFpoTextures = path != "" ? CreateTexturesDB(path) : textures;
//        return myFpoTextures !=null;
//    }

//    public bool OpenBase(IMappedDb db, string name, ref bool write)
//    {
//        string refpath = FilesPath + name;
//        Debug.Log("Opening " + name);
//        if (db.Open(ProductDefs.GetPI().getHddFile(refpath), write) != DBDef.DB_OK)
//        {
//            if (write)
//            {
//                write = false;

//                if (db.Open(ProductDefs.GetPI().getHddFile(refpath), false) == DBDef.DB_OK)
//                    return true;

//            }
//            Debug.Log(string.Format(refpath, FilesError, FilesPath, name));
//            //MessageBox(0, string, "Renderer.dll initialization error", MB_ICONERROR);
//            return false;
//        }
//        return true;
//    }

//    public bool Open(bool _CanWrite)
//    {
//        can_write = _CanWrite;
//        bool _false = false;
//        //return
//        //    OpenBase(files, RDataDb, ref can_write) &&
//        //    OpenBase(materials, MaterialsDb, ref can_write) &&
//        //    OpenBase(rstates, RSatesDb, ref _false) &&
//        //    OpenBase(meshes, MeshesDb, ref _false) &&
//        //    OpenBase(particles, ParticlesDb, ref _false);
//        return
//            OpenBase(files, RDataDb, ref can_write) &&
//            OpenBase(materials, MaterialsDb, ref can_write) &&
//            OpenBase(meshes, MeshesDb, ref _false) &&
//            OpenBase(particles, ParticlesDb, ref _false);
//    }

//    void Close()
//    {
//        particles.Close();
//        meshes.Close();
//        //rstates.Close();
//        materials.Close();
//        files.Close();
//    }

//    public static Storm.MeshData GetFpoMesh(ObjId id)
//    {
//        return GetInstance().meshes.GetBlock(id).Convert<Storm.MeshData>();
//    }

//    public static FpoGraphData GetFpoGraph(ObjId id)
//    {
//        return GetInstance().meshes.GetBlock(id).Convert<FpoGraphData>();
//    }

//    public StormMesh GetStormMesh(ObjId id)
//    {
//        return meshes.GetBlock(id).Convert<StormMesh>();
//    }

//    private const int TFF_BUMP = 0x00000001;

//    public Texture2D loadTexture(TexturesDB mydb, ObjId id, bool bump)
//    {
//        //# ifdef _DEBUG
//        //        cstr name = base->GetMappedDb()->CompleteObjId(id).name;
//        //    ::GetLog().Message("Texture:%s", name ? name : "<unknown>");
//        //#endif
//        mydb.Flags().Set(TFF_BUMP, bump);
//        Texture2D myobject = mydb.CreateTexture(id.name != "" ? Hasher.HshString(id.name) : id.obj_id);
//        mydb.Flags().Set(TFF_BUMP, false);
//        //return myobject ? (Texture*)object->Query<Texture>() : 0;
//        return myobject;
//    }

//    public Texture2D LoadTexture(ObjId id, bool bump = false) { return loadTexture(textures, id, bump); }
//    public Texture2D loadFpoTexture(ObjId id, bool bump = false) { return loadTexture(myFpoTextures, id, bump); }



//}
