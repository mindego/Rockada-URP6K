/*===========================================================================*\
|  This file contains main DLL_API functions                                  |
\*===========================================================================*/
using UnityEngine;
using MadInput=EInput;

public class mctrls
{
    public const int MCTRLS_VERSION = 150032;
    public const int MP_Last = 16;

    const string DllInitError = "Controls.dll initialization error";
    const string RecordError = "Cann't find record \"{0}\" in datafile \"{1}\"";
    const string FilesError = "Sharing access violation or mssing datafile \"{0}\"";
    const string MouseError = "Cann't find mouse device";
    const string ObjectsPath = "Graphics\\ctrlobj.dat";
    const string TexturesPath = "Graphics\\ctrltex.dat";
    const string TexBriefPath = "Graphics\\textures.dat";

    static IMappedDb files;

    private static mctrls instance;
    LOG Log;
    public static mctrls getInstance()
    {
        if (instance == null)
        {
            instance = new mctrls();
            instance.DllMain();
        }
            
            return instance;
    }

    private void DllMain()
    {
        Log = (LOG) LogFactory.CreateLOG("MControls");
        Log.OpenLogFile();
        Initialize();
    }

    public int Version()
    {
        return MCTRLS_VERSION;
    }

    public static ICtrlDialog CreateMDialog(int _version, IBill _b, MadInput _i, string _m, int _id, Vector4[] palette)
    {//palette[MP_Last]
        if (instance == null) instance = getInstance();
        if (_version != MCTRLS_VERSION)
        {
            Asserts.Assert(false);
            return null;
        }
        return Open(false) ? new Dialog(_b, _i, _m, palette, _id) : null;
    }


    public static bool OpenBase(IMappedDb _db, string _name, bool CanWrite)
    {
        string str;
        //  _addstr(str, _name);
        if (_db.Open(ProductDefs.GetPI().getHddFile(_name), CanWrite) != DBDef.DB_OK)
        {
            str = string.Format(FilesError, _name);
            //MessageBox(0, str, DllInitError, MB_ICONERROR);
            Debug.Log(str);
            return false;
        }
        return true;
    }

    string GetName(ObjId _id)
    {
        return files.CompleteObjId(_id);
    }

    void Initialize()
    {
        files = IMappedDb.CreateMappedDb(IMappedDb.DBFORMAT_NAKED);
    }
    static bool  Open(bool CanWrite)
    {
        if (files.IsOpened()) return true;
        return OpenBase(files, ObjectsPath, CanWrite);
    }
    void Close()
    {
        files.Close();
    }
    void Release()
    {
        files.Release();
    }
}

public class Dialog : ICtrlDialog
{
    private IBill b;
    private MadInput i;
    private string m;
    private Vector4[] palette;
    private int id;

    public Dialog(IBill b, MadInput i, string m, Vector4[] palette, int id) 
    {
        this.b = b;
        this.i = i;
        this.m = m;
        this.palette = palette;
        this.id = id;
    }

    public void EnableMouse(bool on)
    {
        Cursor.visible = on;
        //Возможно, тут правильнее CursorLockMode.Locked

    }
}