using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using crc32 = System.UInt32;

public class FpoLoader : IFpoLoader
{
    private Dictionary<uint, FpoData> fpoDataCache = new Dictionary<crc32, FpoData>();
    private Dictionary<uint, MEO_DATA_HDR> MEODataCache = new Dictionary<crc32, MEO_DATA_HDR>();

    RendererApi render;
    ICollision collsn;
    IMappedDb fpodb;
    IMappedDb meodb;

    MeoDLdr old_ldr;

    public FpoLoader()
    {
        fpodb = null;
        collsn = null;
        meodb = null;
    }
    public bool Initialize(RendererApi r, ICollision c, string db_meo, string db_fpo2)
    {
        //    meodb = new DB("MEOS", DBDef.DATA_LOAD_DYNAMIC, old_ldr, (DbLdr)MeoDLdr::MeoDataLoad, 0);
        //    fpodb = CreateMappedDb(DBFORMAT_NAKED);
        //    if ((collsn = c)!=null) collsn.AddRef();
        //    render = r;

        //    return meodb.Open(ProductDefs.GetPI().getHddFile(db_meo)) == DBDef.DB_OK && fpodb.Open(ProductDefs.GetPI().getHddFile(db_fpo2 == DBDef.DB_OK;

        //TODO Обдумать и вернуть (при необходимости) на место загрузчик MEO
        //meodb = new DB("MEOS", DBDef.DATA_LOAD_DYNAMIC, old_ldr, null, null);
        meodb = IMappedDb.CreateMappedDb(IMappedDb.DBFORMAT_NAKED);
        fpodb = IMappedDb.CreateMappedDb(IMappedDb.DBFORMAT_NAKED);

        bool meostatus = meodb.Open(ProductDefs.GetPI().getHddFile(db_meo)) == DBDef.DB_OK;
        bool fpostatus = fpodb.Open(ProductDefs.GetPI().getHddFile(db_fpo2)) == DBDef.DB_OK;
        Debug.Log(string.Format("Loading meodb [{0}]", meostatus ? "success" : "failed"));
        Debug.Log(string.Format("Loading fpodb [{0}]", fpostatus ? "success" : "failed"));

        //if ((collsn = c) != null) collsn.AddRef();
        render = r;
        return meostatus && fpostatus;
    }

    public bool Initialize()
    {
        return true;
    }
    public void Destroy() { }

    public IRData GetData(ObjId id) { return null; }

    public void CreateFpoChilds(Fpo parent, FpoData data)
    {
        /*
          do if (!data->flags.Get(FDF_HIDDEN)) {

        Fpo * fpo = ::CreateFpo(data, render, collsn);

        parent->Attach(fpo);

        FpoData *sub_data = data->GetSubData();
        if (sub_data)
          CreateFpoChilds(fpo, sub_data);

      } while ( data=data->GetNextData() );
    */
    }

    public void LoadFPOGrphics(FPO obj)
    {
        Debug.Log("LoadFPOGrphics for " + obj);
        if (obj.fdata != null)
            for (int i = 0; i < 4; ++i)
            {
                crc32 rd_id = obj.fdata.images[i].graph;
                Debug.LogFormat("Loading LoadFPOGrphics[{0}] {1}",i,rd_id.ToString("X8"));
                if (rd_id != CRC32.CRC_NULL)
                    obj.r_images[i] = render.LoadTMTData(rd_id);
            }

        for (FPO o = (FPO)obj.SubObjects; o != null; o = (FPO)o.Next)
            LoadFPOGrphics(o);

    }

    public static Fpo CreateFpo(FpoData fd, RendererApi r, ICollision cl)
    {
        return new FpoObject(fd, r, cl);
    }
    // api
    public Fpo CreateFpo(ObjId id)
    {
        Fpo fpo = null;
        fpodb.CompleteObjId(ref id);
        FpoData fdata = fpodb.GetBlock(id).Convert<FpoData>();

        if (fdata != null)
        {
            fpo = CreateFpo(fdata, render, collsn);
            FpoData sub_data = fdata.GetSubData();
            if (sub_data != null)
                CreateFpoChilds(fpo, sub_data);
        }
        fpo.Name = id.name;
        if (render != null) render.InitFpo(fpo);
        return fpo;
    }

    public void CreateHiddenObjects(FPO obj, ICreateHidden crh)
    {
        if (render != null)
        {
            GrLodr grldr = new(this, crh);
            obj.CreateHiddenObjects(grldr);
        }
        else
            obj.CreateHiddenObjects(crh);
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }

    public static IFpoLoader CreateFpoLoader(RendererApi render, ICollision collision, string db_fpo2, string db_meo)
    {
        FpoLoader ldr = new FpoLoader();

        if (ldr.Initialize(render, collision, db_meo, db_fpo2))
            return ldr;

        ldr.Release();
        return null;
    }


    public FPO CreateFPO(ObjId id)
    {
        FpoData fdata;
        MEO_DATA_HDR meo;
        if (id.name != "" && id.name != null) id.obj_id = Hasher.HshString(id.name);
        if (id.name == "" || id.name == null) id = meodb.CompleteObjId(id);

        if (id.name == "" || id.name == null) Debug.Log("Failed to resolve ObjId " + id.obj_id.ToString("X8"));

        if (!MEODataCache.ContainsKey(id))
        {
            //meo = meodb.Load(id);
            var st = meodb.GetBlock(id).myStream;
            if (st == null) Debug.LogError("Failed to load " + id.ToString());
            //meo = MEOData.MEO_DATA_HDR.LoadMEO(meodb.GetBlock(id).myStream);
            meo = MEO_DATA_HDR.LoadMEO(st);
            Debug.Log("meo " + meo);
            if (meo == null) return null;
            MEODataCache.Add(id, meo);
        }

        if (!fpoDataCache.ContainsKey(id))
        {
            //fdata = FpoImport.GetFPOData(fpodb.GetBlock(id).myStream, 0);
            fdata = fpodb.GetBlock(id).Convert<FpoData>();
            //Debug.Log("fdata " + fdata);
            if (fdata == null) return null;
            fpoDataCache.Add(id, fdata);
        }

        meo = MEODataCache[id];
        fdata = fpoDataCache[id];


        if (meo != null && fdata != null)
        {
            FPO obj = new FPO(meo.MeoData[0], fdata);

            //Debug.Log(string.Format("FPO {0} created with flags {1}", obj, obj.GetFlags().ToString("X8")));
            //Debug.Log(obj.GetFlag(HashFlags.OF_GROUP_MASK).ToString("X8"));
            // handle error and destroy object if there one
            //if (obj.GetFlag(HashFlags.OF_GROUP_MASK) != HashFlags.OF_GROUP_MASK)
            if (false) //Да, стрёмно. Оключено просто для проверки
            {
                string msg = string.Format(
                        "Should be {1}, got {0}",
                        obj.GetFlag(HashFlags.OF_GROUP_MASK).ToString("X8"),
                        HashFlags.OF_GROUP_MASK.ToString("X8")
                        );
                Debug.Log(msg);
                obj.Release();
            }
            else
            {
                Debug.LogFormat("LoadFPOGrphics via id {0} for {1}",id,obj );
                obj.TextName = id.name;
                if (render != null)
                    LoadFPOGrphics(obj);
                Debug.LogFormat("LoadFPOGrphics via id {0} for {1} done", id, obj);
                return obj;
            }
        }


        return null;
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }
}
public class MeoDLdr : DbMaintenance
{
    public int MeoDataLoad(object[] Data, int Size) { return 0; }
    public void ObjectNotFreed(object Data, int Count) { }
};

public class DbMaintenance { };

public class DB
{
    //friend class DbResolve;
    bool dbType;

    DbMaintenance main;
    //DbLdr dpLoad, dpFree;

    string ID;
    uint nUnits;
    DbIndex diIndex;
    //    File mFile;
    //    union {
    //    struct {
    //      unsigned int* RefCount;
    //}
    //Dm;
    //struct {
    //      void* Data;
    //    } St;
    //  };

    //public :

    public DB(string id, bool Type, DbMaintenance m = null, DbLdr load = null, DbLdr free = null)
    {
        ID = id;
        dbType = Type;
        DbMaintenance main = m;
        //dpLoad = load;
        //dpFree = free;
    }
    //  DB(DbMaintenance * m = 0) : ID(0), dbType(true), main(m), dpLoad(0), dpFree(0), nUnits(0) { }
    //DB(int id, bool Type, DbMaintenance * m = 0, DbLdr load = 0, DbLdr free = 0) :
    //    ID(id), dbType(Type), main(m), dpLoad(load), dpFree(free),nUnits(0) { }

    //~DB() {
    //    if (nUnits) __asm int 3;
    //}

    //int Id() { return ID; }

    //MAD_TOOLS_API int Open(const char*);
    //MAD_TOOLS_API void Close(DbReminder);
    //MAD_TOOLS_API const void* Load  (unsigned int);
    //MAD_TOOLS_API void Free(const void*);
    //MAD_TOOLS_API int FreeAll(DbReminder);
    //MAD_TOOLS_API int DataSize(unsigned int) const;
    //MAD_TOOLS_API int Enumerate(DbEnum, DbMaintenance*);

    public int Open(string fname)
    {
        return DBDef.DB_OK;
    }

    //public MEOData.MEO_DATA_HDR Load(uint id)
    //{
    //    return MEOData.MEO_DATA_HDR.LoadMEO(id);
    //}

};

public interface RendererApi
{
    public const uint RENDERER_VERSION = 0x00020003;
    //Still a STUB!
    public SceneApi CreateScene(SceneData sd);
    //        public IBill* CreateBill()=0;

    // material editor
    public bool OpenMaterialEditor();

    //  public int WndProc(void* hwnd, int Msg, int wParam, int lParam)=0;
    //public void ScreenShot()=0;

    //public bool StartFrame()=0;                   // befor all
    //public void EndFrame()=0;                   // flip page
    //public void ClearScreen(SVec4 Color);

    // scene components
    public IRData LoadTMTData(ObjId id);
    void SetGamma(float gamma);
    bool StartFrame();
    IBill CreateBill();
    void ClearScreen(Vector4 vector4);
    void InitFpo(Fpo fpo);
    //public void InitFpo(interface Fpo *)=0;

    // configure
    //public float GetGamma()=0;
    //public void SetGamma(float )=0;

    //public PerfomanceInfo GetPerfomanceInfo()=0;
}

public class DbLdr
{
    //STUB

}

public class FpoObject : Fpo
{
    public FpoObject(FpoData fd, RendererApi r, ICollision cl)
    {
        data = fd;
        image = 0;
        link = null;

        data.pos.SetMatrix34(out tm);

        for (int i = 0; i < 4; ++i)
        {
            crc32 coll_id = data.images[i].collision;
            boxes[i] = (cl != null && coll_id != CRC32.CRC_NULL) ? cl.GetData(coll_id) : null;
            crc32 rend_id = data.images[i].graph;
            r_images[i] = (r != null && rend_id != CRC32.CRC_NULL) ? r.LoadTMTData(rend_id) : null;
        }
        SetCSphere();
    }

    public override object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case Fpo.ID: return GetFpo();
            default: return base.Query(cls_id);
        }
    }
    public Fpo GetFpo() { return this; }

    public override void Destroy()
    {
        // free render data
        for (int i = 0; i < 4; ++i)
        { //TODO корректно удалять имиджи
        }
        //SafeRelease(r_images[i]);
        //base.Destroy();
    }

    public override void Dispose()
    {
        Destroy();
    }
}

/// <summary>
/// Create hiddens
/// </summary>

struct GrLodr : ICreateHidden
{
    FpoLoader ldr;
    ICreateHidden usr;

    public GrLodr(FpoLoader fl, ICreateHidden u)
    {
        ldr = fl;
        usr = u;
    }

    public bool ProcessHidden(FPO parent, FPO hidden)
    {
        ldr.LoadFPOGrphics(hidden);
        return usr.ProcessHidden(parent, hidden);
    }
};