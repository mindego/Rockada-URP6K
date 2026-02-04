using System.Linq;
using UnityEngine;
using DWORD = System.UInt32;

public enum RoadType
{
    Road,
    Bridge,
    Tunnel
};

/// <summary>
/// data struct
/// </summary>
public class ROADDATA : STORM_DATA
{
    public static StormDataHolder<ROADDATA> Datas = new StormDataHolder<ROADDATA>("road");

    public const string c_Bridge = "Bridge";
    public const string c_Tunnel = "Tunnel";

    // internal part
    public ROADDATA(string n) : base(n)
    {
        Width = 20;
        EntranceObject = 0xFFFFFFFF;
        SectionObject = 0xFFFFFFFF;
        SectionScript = 0xFFFFFFFF;
        SectionTexture = 0xFFFFFFFF;
        SectionMaterial = 0xFFFFFFFF;
        EntranceHeight = 0f;
        EntranceDeltaLow = 0f;
        EntranceDeltaHigh = 0f;
        SectionWidth = 0f;
        Type = RoadType.Road;
        Show = true;
        Color = 0;
    }

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("Name: " + FullName);
        sb.AppendLine("Width: " + Width);
        sb.AppendLine("Type: " + Type);
        return sb.ToString();
    }

    public override void ProcessToken(READ_TEXT_STREAM st, string c)
    {
        string buf = "";
        do
        {
            if (st.LoadUInt(ref Color, "Color")) continue;
            if (st.LoadBool(ref Show, "Show")) continue;
            if (st.LoadFloat(ref Width, "Width")) continue;
            if (st.Load_String(ref buf, "Type")) continue;
            if (st.LoadFloat(ref EntranceHeight, "EntranceHeight")) continue;
            if (st.LoadFloat(ref EntranceDeltaLow, "EntranceDeltaLow")) continue;
            if (st.LoadFloat(ref EntranceDeltaHigh, "EntranceDeltaHigh")) continue;
            if (st.LoadFloat(ref SectionWidth, "SectionWidth")) continue;
            if (st.LdHS(ref EntranceObject, "EntranceObject")) continue;
            if (st.LdHS(ref SectionObject, "SectionObject")) continue;
            if (st.LdHS(ref SectionScript, "SectionScript")) continue;
            if (st.LdHS(ref SectionTexture, "SectionTexture")) continue;
            if (st.LdHS(ref SectionMaterial, "SectionMaterial")) continue;
            if (st.Recognize("Reference"))
            {
                Reference(GetByName(st.GetNextItem()));
                return;
            }
            base.ProcessToken(st, c);
        } while (false);

        if (buf == c_Bridge) Type = RoadType.Bridge;
        if (buf == c_Tunnel) Type = RoadType.Tunnel;
    }
    public void Reference(ROADDATA data_ref)
    {
        Type = data_ref.Type;
        Width = data_ref.Width;
        EntranceObject = data_ref.EntranceObject;
        SectionObject = data_ref.SectionObject;
        SectionScript = data_ref.SectionScript;
        SectionTexture = data_ref.SectionTexture;
        SectionMaterial = data_ref.SectionMaterial;
        Show = data_ref.Show;

    }
    // data section
    public RoadType Type;
    public float Width;
    public uint EntranceObject;
    public uint SectionObject;
    public uint SectionScript;
    public uint SectionTexture;
    public uint SectionMaterial;
    public float SectionWidth;
    public float EntranceHeight;
    public float EntranceDeltaLow;
    public float EntranceDeltaHigh;
    public DWORD Color;
    public bool Show;
    // data access
    public static ROADDATA GetByName(string Name, bool MustExist = true)
    {
        //return Datas.GetItem(Name, 0, MustExist);
        uint Code = Constants.THANDLE_INVALID;
        if (Name != "") Code = Hasher.HshString(Name);
        foreach (ROADDATA data in Datas)
        {
            if (data.Name == Code) return data;
        }
        if (MustExist) throw new System.Exception($"Could not get ROADDATA {Name}");
        return null;
    }
    public static ROADDATA GetByCode(uint Code, bool MustExist = true)
    {
        foreach (ROADDATA data in Datas)
        {
            if (data.Name == Code) return data;
            //Debug.Log((Code.ToString("X8"),data.FullName, data.Name.ToString("X8"), Hasher.CodeString(data.FullName).ToString("X8")));
        }
        if (MustExist) throw new System.Exception($"Could not get ROADDATA {Code}");
        return null;
    }
    public static ROADDATA GetFirstItem()
    {
        return Datas.First();
    }
    public static int nItems()
    {
        return Datas.Count;
    }

    public static void insertRoadData(READ_TEXT_STREAM st)
    {
        ROADDATA rData = new ROADDATA(st.GetNextItem());
        StormLog.LogMessage("Loading road " + rData.FullName + " [OK]",StormLog.logPriority.DEBUG);
        Datas.InsertItem(rData, st.LineNumber()).Load(st);
    }

    //public static void loadRoadData(int code, PackType db)
    //{
    //    //LoadUtils lu = new LoadUtils();
    //    switch (code)
    //    {
    //        case 0:
    //            LoadUtils.parseData(db, Datas.GetItemName(), "Roads", "Roads.txt", "[STORM ROAD DATA FILE V1.0]", "Road", insertRoadData);
    //            return;
    //    }

    //}
    public static void loadRoadData(int code, IMappedDb db)
    {
        //LoadUtils lu = new LoadUtils();
        switch (code)
        {
            case 0:
                LoadUtils.parseData(db, Datas.GetItemName(), "Roads", "Roads.txt", "[STORM ROAD DATA FILE V1.0]", "Road", insertRoadData);
                return;
        }

    }
}