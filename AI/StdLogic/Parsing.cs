using System;

public static class Parsing
{
    // messages
    public const string sAiWarning = "Warning";
    public const string sAiEmpty = "";
    public const string sAiNothing = "empty";
    public static IErrorLog errorlog;

    public static void AiMessage(int type, string title, string frm, params object[] va_list)
    {
        if (errorlog == null) return;

        string cur;

        if (title != null)
        {
            cur = string.Format("{0}{1}{2}", title, title != "" ? " : ":sAiEmpty, frm);
        }
        else cur = frm;

        errorlog.printVMessage(type, cur, va_list);
    }
    public static void FillUnitDataFromRespawnInfo(ref UNIT_DATA dt, RespawnInfo info)
    {
        dt.Org = info.mOrg;
        dt.CodedName = info.ObjectName;
        dt.Layout1 = info.Layout1Name;
        dt.Layout2 = info.Layout2Name;
        dt.Layout3 = info.Layout3Name;
        dt.Layout4 = info.Layout4Name;
        dt.Angle = info.mRespawnAngle;
    }

    public static void FillRespawnInfoFromUnitData(out RespawnInfo info, UNIT_DATA dt)
    {
        info = new RespawnInfo();
        info.mOrg = dt.Org;
        info.ObjectName = dt.CodedName;
        info.Layout1Name = dt.Layout1;
        info.Layout2Name = dt.Layout2;
        info.Layout3Name = dt.Layout3;
        info.Layout4Name = dt.Layout4;
        info.mRespawnAngle = dt.Angle;
    }

    public static void FillSpawnDataFromUnitData(UNIT_DATA data, ref UnitSpawnData sp)
    {
        // разбираем строку - установку оружия
        sp.ObjectName = data.CodedName;
        sp.Layout1Name = data.Layout1;
        sp.Layout2Name = data.Layout2;
        sp.Layout3Name = data.Layout3;
        sp.Layout4Name = data.Layout4;
    }


    public static void openErrorLog(IErrorLog con)
    {
        errorlog = con;
    }

    public static void closeErrorLog()
    {
        errorlog = null;
    }

    internal static void FillUnitDataFromSpawnData(UnitSpawnData data, ref UNIT_DATA sp)
    {
        // разбираем строку - установку оружия
        sp.CodedName = (uint) data.ObjectName;
        sp.Layout1 = (uint) data.Layout1Name;
        sp.Layout2 = (uint) data.Layout2Name;
        sp.Layout3 = (uint) data.Layout3Name;
        sp.Layout4 = (uint) data.Layout4Name;
    }
}
