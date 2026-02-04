using System.IO;
using UnityEngine;
using DWORD = System.UInt32;
public class _PI : ProductInfo, ProductInfo.IFileSystem
{
    //string mRootPath;
    //public static string mHddPath = "E://Unity/Wedge/Echelon Island Defence/Data";
    //public static string mHddPathWW = "E://Unity/Wedge/Echelon Island Defence/DataWW";
    //public static string dataDir = mHddPath;

    public const string STORMWW_PATH = "E:\\\\SteamLibrary\\steamapps\\common\\Echelon Wind Warriors";
    const string DataFmt = "{0}\\{1}\\";
    const string NoResource = "";

    DWORD mProductID;
    DWORD mLangID;

    DWORD mVersion;
    DWORD mPubVersion;

    DWORD mProductState; // 1 - installed, 2 - from CD

    string mRootFile;
    string mHddFile;
    string mCdFile;

    //HINSTANCE hResources;

    string mStrVersion;
    string mLanguage;

    string mRevisionBrief;
    string mRevisionFull;

    string mTitle;
    string mRegKeyConfig;

    string mRootPath;
    string mHddPath;
    string mCdPath;

    public _PI(string defPath= STORMWW_PATH)
    {
        mVersion = ProductDefs.PRODUCT_VERSION;
        //hResources(res)
        //Storm.CRC32 Crc32;
        //initProdict(Crc32);

        initPath(defPath);
        //initInstall(Crc32);
    }

    
    void initPath(string defPath)
    {
        //GetModuleFileName(0, mRootPath, sizeof(mRootPath));
        //mRootPath = Application.dataPath;
        mRootPath = defPath;
        //mRootFile = getFnDir(mRootPath); //*mRootFile = 0;
        mRootFile = Path.GetFileName(mRootPath);
        mHddFile = formDataPath(ref mHddPath, mRootPath);
        mCdFile = formDataPath(ref mCdPath, null);

        Debug.Log(this);
    }

    public override string ToString()
    {
        return string.Format("mRootPath {0}\nmRootFile {1}\nmHddFile {2}\nmCdFile {3}", mRootPath, mRootFile, mHddFile, mCdFile);
    }
    string formDataPath(ref string Dest, string Root)
    {
        //return Root ? Dest + wsprintf(Dest, DataFmt, Root, DataFolder) : (*Dest = 0, Dest);
        if (Root == null || Root == "") return Dest;
        Dest = string.Format(DataFmt, Root, ProductInfo.DataFolder);
        return Path.GetFileName(Dest);
    }

    public string CompanyName() { return ProductDefs.Company; }

    public string ProductName() { return ProductDefs.Product; }
    public DWORD ProductID() { return mProductID; }


    public string Language() { return mLanguage; }
    public DWORD LangID() { return mLangID; }


    public string Title() { return mTitle; }

    public DWORD Version() { return mVersion; }
    public DWORD VersionMajor() { return mVersion / 100; }
    public DWORD VersionMinor() { return mVersion % 100; }

    public DWORD VersionPublisher() { return mPubVersion; }

    public string StrVersion() { return mStrVersion; }

    public string RevisionBrief() { return mRevisionBrief; }
    public string RevisionFull() { return mRevisionFull; }

    public DWORD GetProductState() { return mProductState; }

    public string RegKeyConfig() { return mRegKeyConfig; }
    public string RegKeyInstall() { return mRegKeyConfig; }

    public string getRootFile(string fn)
    {      // main executable path
        Asserts.Assert(mRootPath != fn);
        //strcopy(mRootFile, fn);
        return mRootPath + fn;
    }

    public string getHddFile(string fn)
    {       // install location + DataFolder
        Asserts.Assert(mHddPath != fn);
        //strcopy(mHddFile, fn);
        return mHddPath + fn;
    }

    public string getCdFile(string fn)
    {        // install source + DataFolder
        Asserts.Assert(mCdPath != fn);
        //strcopy(mCdFile, fn); 
        return mCdPath + fn;
    }

    public virtual ProductInfo.IFileSystem Fs() { return this; }

    public void InstallProduct(string CdDir, string HddDir)
    {
        throw new System.NotImplementedException();
    }

    public ProductInfo.IGameSpyKeys getGS()
    {
        throw new System.NotImplementedException();
    }
}

public static class ProductDefs
{
    public const string Company = "MADia & mindego";
    public const string Product = "Echelon 1.5 Unity";
    //#else
    //    const char Product[] = "Echelon15Demo";
    public const string LangDll = "Data\\WinRes.dat";

    public const int PRODUCT_VERSION = 110;

    public const uint CD_CHEAT_PHRASE = 0x5A74D0D5; // "CrackMeAgain"

    public static _PI pi;
    public static _PI GetPI()
    {
        if (pi == null) pi = new _PI();
        return pi;
    }

    public static _PI InitPI(string path)
    {
        if (pi == null) pi = new _PI(path);
        return pi;
    }
    public static string getRootFile(string fn = null)
    {
        return GetPI().Fs().getRootFile(fn);
    }

    public static string getHddFile(string fn = null)
    {
        return GetPI().Fs().getHddFile(fn);
    }

    public static string getCdFile(string fn = null)
    {
        return GetPI().Fs().getCdFile(fn);
    }

}

/// <summary>
/// This interface must support backward compatibility for version number
/// </summary>
public interface ProductInfo
{

    public const int PRODUCT_STATE_INSTALL = 1;
    public const int PRODUCT_STATE_INSTALLCD = 2;
    public const int PRODUCT_STATE_DEMO = 3;

    public const int PRODUCT_INFO_VERSION = 1;

    //GLOBAL
    public const string PInfoDll = "Product.dll";

    public const string DataFolder = "Data";
    public const string SaveGamesDir = "Saved Games";
    public const string GameConfigs = "configs.cfg";

    // general    
    public string CompanyName();       // "Madia"
    public string ProductName();       // "Echelon"
    public DWORD ProductID();         // Crc32(ProductName());

    public string Language();          // "English"
    public DWORD LangID();            // Crc32(Language);

    public string Title();             // "Echelon Alpha"

    public DWORD Version();           // 103           // read from registry
    public DWORD VersionMajor();      // Version/100   // read from or init value
    public DWORD VersionMinor();      // Version%100   // read from or init value

    public DWORD VersionPublisher();  // ToInt($PublisherVersion)

    public string StrVersion();        // "%d.%02d",Version/100+Version%100

    public string RevisionBrief();     // $RevisionBrief
    public string RevisionFull();      // $RevisionFull

    // Data ...
    public void InstallProduct(string CdDir, string HddDir); // notify install success
    public DWORD GetProductState();

    public string RegKeyInstall();     // "Software\\MS\\WIN\\CV\\UN\\Echelon"
    public string RegKeyConfig();      // "Software\\Madia\\Echelon"


    // resource related
    //public HINSTANCE GetResourceDLL();
    //public string LoadString(UINT StrID, char* Buffer, int Size);

    // Localized message boxes ...
    //public int MsgBox(HWND hWnd, UINT TitleId, UINT FmtId, UINT Type = MB_ICONERROR, ...);

    interface IFileSystem
    {
        public string getRootFile(string fn = null);      // main executable path
        public string getHddFile(string fn = null);       // install location + DataFolder
        public string getCdFile(string fn = null);        // install source + DataFolder
    };

    public IFileSystem Fs();

    interface IGameSpyKeys
    {
        public string getGameName();
        public string getSecretKey();
    };

    public IGameSpyKeys getGS();
};