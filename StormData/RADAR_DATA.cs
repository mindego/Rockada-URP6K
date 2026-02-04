using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// data struct
public class RADAR_DATA : SUBOBJ_DATA
{
    public RADAR_DATA(string name) : base(name)
    {
        SetFlag(SC_RADAR);
        SensorRange = 20000;
        CornerSpeed = Storm.Math.GRD2RD(90f);
    }
    public override void ProcessToken(READ_TEXT_STREAM st,string value)
    {
        do
        {
            if (st.LoadFloat(ref SensorRange, "SensorRange")) continue;
            if (st.LoadFloatC(ref CornerSpeed, "CornerSpeed", Storm.Math.GRD2RD)) continue;
            base.ProcessToken(st, value);
        } while (false);
    }
    public override void Reference(SUBOBJ_DATA ReferenceData)
    {
        base.Reference(ReferenceData);
        // RADAR_DATA
        if (ReferenceData.GetClass() != SC_RADAR) return;
        RADAR_DATA rr = (RADAR_DATA) ReferenceData;
        SensorRange = rr.SensorRange;
        CornerSpeed = rr.CornerSpeed;

    }
    // data section
    public float SensorRange;
    public float CornerSpeed;

    //// ****************************************************************************
    //// data list перенесён в LoadUtils и возвращён назад.
    ////extern SUBOBJ_DATA InsertSubobjData(SUBOBJ_DATA*, int);

    //public void insertRadarData(READ_TEXT_STREAM st)
    //{
    //    InsertSubobjData(new RADAR_DATA(st.GetNextItem()), f.LineNumber())->Load(f);
    //}

    //void loadRadarData(IMappedDb* db)
    //{
    //    parseData(db, "radar", "Radars", "Radars.txt", "[STORM RADAR DATA FILE V1.1]", "Radar", insertRadarData);
    //}
    public static void insertRadarData(READ_TEXT_STREAM st)
    {
        RADAR_DATA rData = new RADAR_DATA(st.GetNextItem());
        StormLog.LogMessage("Loading radar " + rData.FullName + " [OK]", StormLog.logPriority.DEBUG);
        InsertSubobjData(rData, st.LineNumber()).Load(st);
    }

    //public static void loadRadarData(PackType db)
    //{
    //    LoadUtils.parseData(db, "radar", "Radars", "Radars.txt", "[STORM RADAR DATA FILE V1.1]", "Radar", insertRadarData);
    //}
    public static void loadRadarData(IMappedDb db)
    {
        LoadUtils.parseData(db, "radar", "Radars", "Radars.txt", "[STORM RADAR DATA FILE V1.1]", "Radar", insertRadarData);
    }
};
