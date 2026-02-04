using System;
using DWORD = System.UInt32;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

/// <summary>
/// UnitDataTable (ex-EffData)
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)] 
public class UnitDataTable : IStormImportable<UnitDataTable>
{
    private int type_count;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    private UnitData[] UDS;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    private UnitDataTableEntry[] UDTES;

    //public long Size() { return (sizeof(int) + type_count * sizeof(UnitData) + type_count * type_count * sizeof(UnitDataTableEntry)); }
    public int Size() { return sizeof(int) + type_count * Marshal.SizeOf<UnitData>() + type_count*type_count * Marshal.SizeOf<UnitDataTableEntry>(); }
    // access to unit datas
    //public UnitData[] GetUD() { return (UnitData*)((char*)this + sizeof(int)); }
    public UnitData[] GetUD() { return UDS; }
    public UnitData GetUD(int x) { return GetUD()[x]; }
    // access to unit datas table
    public UnitDataTableEntry[] GetUDTE() { return UDTES; }
    public UnitDataTableEntry GetUDTE(int x, int y) { return GetUDTE()[y * type_count + x]; }
    // general access
    public static UnitDataTable pUnitDataTable;
    public static int GetIdxByName(string name)
    {
        for (int i = 0; i < pUnitDataTable.type_count; i++)
            //if (StriCmp(name, pUnitDataTable->GetUD(i).Name) == 0)
            if (name == pUnitDataTable.GetUD(i).getName())
                return i;
        return -1;
    }
    
    public static int GetIdxByType(DWORD t)
    {
        for (int i = 0; i < pUnitDataTable.type_count; i++)
        {
            //var UD = pUnitDataTable.GetUD(i);
            //UnityEngine.Debug.Log(string.Format("Examining {0} {1} {2} vs {3} {4}", i,UD.getName(), UD.Type.ToString("X8"),t.ToString("X8"),(t == (uint)UD.Type).ToString()));
            if (t == (uint)pUnitDataTable.GetUD(i).Type)
                return i;
        }
        return -1;
    }


    //public static UnitDataTable GetUnitDataTable(Stream stream)
    //{
    //    if (!stream.CanRead) return null;
    //    stream.Seek(0, SeekOrigin.Begin);

    //    UnitDataTable pUnitDataTable = StormFileUtils.ReadStruct<UnitDataTable>(stream);
    //    //Debug.Log(pUnitDataTable.type_count);

    //    pUnitDataTable.UDS = new UnitData[pUnitDataTable.type_count];
    //    for (int i = 0; i < pUnitDataTable.type_count; i++)
    //    {
    //        pUnitDataTable.UDS[i] = StormFileUtils.ReadStruct<UnitData>(stream, stream.Position);
    //    }

    //    //Debug.Log("[" + pUnitDataTable.UDS[0].getName() + "]");
    //    ///Debug.Log("[" + pUnitDataTable.UDS[pUnitDataTable.type_count - 1].getName() + "]");

    //    pUnitDataTable.UDTES = new UnitDataTableEntry[pUnitDataTable.type_count * pUnitDataTable.type_count];
    //    for (int y = 0; y < pUnitDataTable.type_count; y++)
    //    {
    //        for (int x = 0; x < pUnitDataTable.type_count; x++)
    //        {
    //            pUnitDataTable.UDTES[y * pUnitDataTable.type_count + x] = StormFileUtils.ReadStruct<UnitDataTableEntry>(stream, stream.Position);
    //        }
    //    }

    //    //Debug.Log(pUnitDataTable.GetUDTE(5,5));
    //    return pUnitDataTable;
    //}
    public UnitDataTable Import(Stream stream)
    {
        if (!stream.CanRead) return null;
        stream.Seek(0, SeekOrigin.Begin);

        UnitDataTable pUnitDataTable = StormFileUtils.ReadStruct<UnitDataTable>(stream);
        //Debug.Log(pUnitDataTable.type_count);

        pUnitDataTable.UDS = new UnitData[pUnitDataTable.type_count];
        for (int i = 0; i < pUnitDataTable.type_count; i++)
        {
            pUnitDataTable.UDS[i] = StormFileUtils.ReadStruct<UnitData>(stream, stream.Position);
        }

        //Debug.Log("[" + pUnitDataTable.UDS[0].getName() + "]");
        ///Debug.Log("[" + pUnitDataTable.UDS[pUnitDataTable.type_count - 1].getName() + "]");

        pUnitDataTable.UDTES = new UnitDataTableEntry[pUnitDataTable.type_count * pUnitDataTable.type_count];
        for (int y = 0; y < pUnitDataTable.type_count; y++)
        {
            for (int x = 0; x < pUnitDataTable.type_count; x++)
            {
                pUnitDataTable.UDTES[y * pUnitDataTable.type_count + x] = StormFileUtils.ReadStruct<UnitDataTableEntry>(stream, stream.Position);
            }
        }

        //Debug.Log(pUnitDataTable.GetUDTE(5,5));
        return pUnitDataTable;
    }
};


/// <summary>
/// UnitData table entry (ex-UnitTypeInfo )
/// </summary>
public struct UnitDataTableEntry
{
    public float power;
    public float importance;
};

public class UnitDataDefines
{
    public const int UT_TOTAL = 5;
    // UnitData::Type - codes
    public const uint UT_ON_SURFACE = 0x05133238;
    public const uint UT_FLYING = 0xB5CB853C;
    public const uint UT_FLYING_AGILE = 0x248AC77E;
    public const uint UT_FLYING_FAST = 0x047FFE10;
    public const uint UT_MISSILE = 0xD3678788;

    // UnitData::Type - names
    public const string UTC_ON_SURFACE = "on_surface";
    public const string UTC_FLYING = "flying";
    public const string UTC_FLYING_AGILE = "flying_agile";
    public const string UTC_FLYING_FAST = "flying_fast";
    public const string UTC_MISSILE = "missile";
}
/// <summary>
/// UnitData struct (ex-UnitTypeData)
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UnitData
{
    //char Name[48];
    //public string Name;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
    public char[] Name;
    public float SensorRadius;     // 0...100000
    public float SensorVisibility; // 0...100000
    public int Type;

    public string getName()
    {
        return new string(Name).Trim('\0');
    }
};
public class UnitDataDBHelper
{
    const string UnitDataTableName = "UnitDatasTable";
    public static void LoadInitDataTable(PackType pd)
    {
        ResourcePack rp = new ResourcePack();
        rp.Open(ProductDefs.GetPI().getHddFile("gdata.dat"));
        LoadUnitDataTable(rp);
        rp.Close();
    }
    public static void LoadUnitDataTable(IMappedDb db)
    {
        stormdata_dll.LoadingMessage("UnitDataTable");

        // грузим файл

        MemBlock bl = db.GetBlock(new ObjId(UnitDataTableName));
        UnitDataTable.pUnitDataTable = bl.Convert<UnitDataTable>();
        //UnitDataTable.pUnitDataTable = UnitDataTable.GetUnitDataTable(bl.myStream);
        bl.myStream.Close();

        if (UnitDataTable.pUnitDataTable == null)
            throw new IOException(UnitDataTableName);

        //LogAppendSucceeded();

        // считаем CRC
        //STORM_DATA::crc ^= Crc32.Code(bl.Ptr(), bl.size);
    }

    void ReleaseUnitDataTable()
    {
        UnitDataTable.pUnitDataTable = null;
    }


}

