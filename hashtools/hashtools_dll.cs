using DWORD = System.UInt32;
using static DLLEmulation;
using static ProductDefs;
using static DBDef;

public class hashtools_dll : DLLEmulation
{
    public static ILog hlog = null;
    public static ClDllData cl_dll_data = null;

    public static int DllMain()
    {
        return DllMain(null, DLL_PROCESS_ATTACH, null);
    }
    public static int DllMain(HMODULE hDll, DWORD dwReason, LPVOID aborted)
    {
        switch (dwReason)
        {
            case DLL_PROCESS_ATTACH:
                //HeapInit();
                hlog = LogFactory.CreateLOG("HashTools");
                hlog.OpenLogFile();
                //hlog->OpenLogWindow();
                cl_dll_data = new ClDllData();
                cl_dll_data.Initialize();
                break;
            case DLL_PROCESS_DETACH:
                cl_dll_data.Release();
                hlog.Release();
                //HeapDone(hDll);
                break;
        }
        return 1;
    }
}

public class ClDllData
{
    const string DataDb = "boxes.dat";
    const string FilesError = "Sharing access violation or mssing datafile \"{0}\"\n"+
                                   "Plase, verify installation procedure and Your SourceSafe status !";

    IMappedDb files;

    public CollisionData GetClData(ObjId _id)
    {
        return files.GetBlock(_id).Convert<CollisionData>();
    }

    public string GetName(ObjId _id)
    {
        return files.CompleteObjId(_id);
    }

    public void Initialize()
    {
        files = IMappedDb.CreateMappedDb(IMappedDb.DBFORMAT_NAKED);
    }
    public bool Open()
    {
        if (!files.IsOpened() && files.Open(GetPI().Fs().getHddFile(DataDb)) != DB_OK)
        {
            string buffer;
            buffer = string.Format(FilesError, DataDb);

            //MessageBox(0, string, "HashTolls.dll initialization error", MB_ICONERROR);
            UnityEngine.Debug.LogError("HashTolls.dll initialization error" + "\n"+buffer);
            return false;
        }
        return true;
    }
    public void Release()
    {
        files.Release();
    }
};
