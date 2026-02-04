using System;
using System.IO;
using UnityEngine;
using static stormdata_dll;
using static Unity.Burst.Intrinsics.X86.Avx;
using DWORD = System.UInt32;
/// <summary>
/// Main dll functions
/// Эмуляция загрузки dll
/// </summary>
public class stormdata_dll : DLLEmulation
{

    public const string gspPackageName = "gdata.dat";
    public const string gspLocalizationName = "GameData.dat";
    public const string gspDbError = "ERROR: Can`t open DB \"{0}\"";

    public static IMappedDb DataDB;

    public static IMappedDb getGameData()
    {
        return DataDB;
    }

    public static iUnifiedVariableContainer gpDescriptionCtr = null;
    public static iUnifiedVariableContainer gpDescriptionShortCtr = null;

    public static void CantFindError(string ItemName, string Name, uint Code)
    {
        throw new Exception(string.Format("no {0} data {1} (\"{2}\")", ItemName, Code, Name != "" ? Name : "<N/A>"));
    }

    public static void DuplicateError(string ItemName, string Name)
    {
        throw new Exception(string.Format("{0} data \"{1}\" duplicates", ItemName, Name));
    }

    public static void StructError(string ItemName, string Name)
    {
        throw new Exception(string.Format("{0} data \"{1}\" has internal error", ItemName, Name));
    }

    public static void ParseError(string ItemName, string Name)
    {
        throw new Exception(string.Format("Parsing error: {0} data \"{1}\" ", ItemName, Name));
    }

    public static void LoadingMessage(string ItemName)
    {
        StormLog.LogMessage(string.Format("Loading {0} datas...", ItemName));
    }

    public static void LinkingMessage(string ItemName)
    {
        StormLog.LogMessage(string.Format("Linking {0} datas...", ItemName));
    }

    public static void ReleaseAll()
    {
        Debug.Log("Releasing datas");
        DUST_DATA.Datas.Clear();
        ROADDATA.Datas.Clear();
        EXPLOSION_DATA.Datas.Clear();
        DEBRIS_DATA.Datas.Clear();
        SUBOBJ_DATA.Datas.Clear();
        OBJECT_DATA.Datas.Clear();
        AnimationPackage.clearAnimationPackages();

    }
    public static bool LoadAll()
    {
        // open localization DBs
        iUnifiedVariableDB pDb = UniVarUtils.CreateUnifiedVariableDB(Constants.UNIVARS_VERSION, ProductDefs.GetPI().getHddFile(gspLocalizationName), true);

        if (pDb == null)
        {
            LogFactory.GetLog().Message(gspDbError, ProductDefs.GetPI().getHddFile(gspLocalizationName));
            return false;
        }

//        pDb.GetRoot();
        Debug.Log("localization DB loaded: " + pDb + " [ " + ProductDefs.GetPI().getHddFile(gspLocalizationName) + " ]");
        gpDescriptionCtr = pDb.GetVariableTpl<iUnifiedVariableContainer>("\\Root\\Descriptions Full");
        gpDescriptionShortCtr = pDb.GetVariableTpl<iUnifiedVariableContainer>("\\Root\\Descriptions Short");
        Debug.Log("localization descr DB loaded: " + (gpDescriptionCtr == null ? "Failed": gpDescriptionCtr));
        Debug.Log("localization descr DB loaded: " + (gpDescriptionShortCtr == null ? "Failed": gpDescriptionShortCtr));
        //Не загружается в СН и это нормально - \\Root\Descriptions присутствут только в ОШ


        DataDB = IMappedDb.CreateMappedDb(IMappedDb.DBFORMAT_NAKED);
        if (DataDB.Open(ProductDefs.GetPI().getHddFile(gspPackageName)) != DBDef.DB_OK)
        {
            //GetLog().Message(gspDbError, getHddFile(gspPackageName));
            //return false;
            Debug.Log(String.Format(gspDbError, ProductDefs.GetPI().getHddFile(gspPackageName)));
            return false;
        }

        StormLog.LogMessage("Loading Data:", StormLog.logPriority.WARNING);
        UnitDataDBHelper.LoadUnitDataTable(DataDB);
        AnimationPackage.loadAnimationPackages(DataDB);
        for (int i = 0; i < 2; i++)
        {
            DUST_DATA.loadDustData(i, DataDB);
            ROADDATA.loadRoadData(i, DataDB);
            EXPLOSION_DATA.loadExplData(i, DataDB);
            DEBRIS_DATA.loadDebrData(i, DataDB);
            SUBOBJ_DATA.loadSubobjData(i, DataDB);
            OBJECT_DATA.loadObjectData(i, DataDB);
        }
        StormLog.LogMessage("Loading data success !");
        return true;
    }

    //public static bool IsHangaring(int state, bool prev_state) //TODO перенести в более подходящее место (iSensors?)
    //{
    //    if (state != iSensorsDefines.CS_DEAD)
    //        return (state == iSensorsDefines.CS_ENTERING_HANGAR || state == iSensorsDefines.CS_LEAVING_HANGAR);
    //    return prev_state;

    //}

    public static void DllMain()
    {
        LoadAll();
    }

    //static STDT_LIST_HELPER<STRING_RECORD> Strings = new STDT_LIST_HELPER<STRING_RECORD>("Strings");
    public static StormDataHolder<STRING_RECORD> Strings = new StormDataHolder<STRING_RECORD>("Strings");
    public static string AddString(iUnifiedVariableContainer pCtr, DWORD Name)
    {
        if (pCtr == null) return null;
        iUnifiedVariableString pStr = pCtr.GetVariableTpl<iUnifiedVariableString>(null, Name);
        if (pStr == null) return null;
        STRING_RECORD r = Strings.InsertItem(new STRING_RECORD(pStr), 0);
        return r.FullName;
    }

    public static uint HshString(string n)
    {
        return (n == null ? 0 : Hasher.HshString(n)); //Возможно, правильнее тут 0xFFFFFFFF
    }
}

//public class STRING_RECORD : TLIST_ELEM<STRING_RECORD>
public class STRING_RECORD : STORM_DATA
{
    //public uint Name;
    //public string FullName;

    public STRING_RECORD(uint c,string n)
    {
        Name = c;
        FullName = n;
    }
    public STRING_RECORD(iUnifiedVariableString pStr)
    {
        //int i = pStr->StrLen() + 1;
        //FullName
        pStr.StrCpy(out FullName);
        Name = Hasher.HshString(FullName);
    }
    ~STRING_RECORD()
    {
        FullName = null;
    }

    
}

