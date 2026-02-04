using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
/// <summary>
/// Описание структуры турелей
/// </summary>
public class TURRET_DATA : SUBOBJ_DATA
{
    public TURRET_DATA(string name) : base(name)
    {
        SetFlag(SC_TURRET);
        SpeedX = 60;
        SpeedY = 30;
        MinX = -5;
        MaxX = 60;
        MinY = -Mathf.PI;
        MaxY = Mathf.PI;
        WeaponData = null;
        myCraftTurret = 0;
        UnitDataIndex = -1;

        //Storm original mode
        //myTurretPauseTimeBase = 1f;
        //myTurretPauseTimeDelta = .0f;
        //myTurretFireTimeBase = 3f;
        //myTurretFireTimeDelta = .0f;
    }
    public override void ProcessToken(READ_TEXT_STREAM st,string value)
    {
        do
        {
            if (st.LoadFloatC(ref SpeedY, "TurretSpeed", Storm.Math.GRD2RD)) continue;
            if (st.LoadFloatC(ref SpeedX, "GunSpeed", Storm.Math.GRD2RD))continue;
            if (st.LoadFloatC(ref MinY, "TurretMin", Storm.Math.GRD2RD))continue;
            if (st.LoadFloatC(ref MaxY, "TurretMax", Storm.Math.GRD2RD))continue;
            if (st.LoadFloatC(ref MinX, "GunMin", Storm.Math.GRD2RD))continue;
            if (st.LoadFloatC(ref MaxX, "GunMax", Storm.Math.GRD2RD))continue;

            //st.LoadFloatC(ref SpeedY, "TurretSpeed");
            //st.LoadFloatC(ref SpeedX, "GunSpeed");
            //st.LoadFloatC(ref MinY, "TurretMin");
            //st.LoadFloatC(ref MaxY, "TurretMax");
            //st.LoadFloatC(ref MinX, "GunMin");
            //st.LoadFloatC(ref MaxX, "GunMax");
            if (st.LoadInt(ref myCraftTurret, "CraftTurret")) continue;
            if (st.LdHST<WPN_DATA>(ref WeaponData, "Weapon")) continue;
            if (st.Recognize("Gun")) {
                //GunNamesList.Add(CodeString(f.GetNextItem()));  //TODO! Возможно важно
                GunNamesList.Add(st.GetNextItem());
                continue; 
            }

            //st.Recognize("Turret") PARSE_ERROR;
            if (st.Recognize("Turret")) throw new Exception("Turret in turret not supported");

            
            //Storm Original data load:
            //if (st.LoadFloat(ref myTurretPauseTimeBase, "PauseTimeBase")) continue;
            //if (st.LoadFloat(ref myTurretPauseTimeDelta, "PauseTimeDelta")) continue;
            //if (st.LoadFloat(ref myTurretFireTimeBase, "FireTimeBase")) continue;
            //if (st.LoadFloat(ref myTurretFireTimeDelta, "FireTimeDelta")) continue;

            base.ProcessToken(st, value);

        } while (false);
    }
    public override void Reference(SUBOBJ_DATA data)
    {
        base.Reference(data);
        // TURRET_DATA
        if (data.GetClass() != SC_TURRET) return;
        TURRET_DATA rr = (TURRET_DATA) data;
        SpeedX = rr.SpeedX;
        SpeedY = rr.SpeedY;
        MinX = rr.MinX;
        MaxX = rr.MaxX;
        MinY = rr.MinY;
        MaxY = rr.MaxY;
        myCraftTurret = rr.myCraftTurret;
        WeaponData = rr.WeaponData;

    }
    public override void MakeLinks()
    {
        base.MakeLinks();
        StormDataHDR.RSLV<WPN_DATA>(ref WeaponData);
        if (WeaponData == null || (WeaponData.Type != WpnDataDefines.WT_GUN && WeaponData.Type != WpnDataDefines.WT_PLASMA && WeaponData.Type != WpnDataDefines.WT_MISSILE && WeaponData.Type != WpnDataDefines.WT_HTGR))
            stormdata_dll.StructError("turret", FullName);
        else
                if (WeaponData.Type != WpnDataDefines.WT_MISSILE && WeaponData.Type != WpnDataDefines.WT_HTGR && GunNamesList.Count == 0)
            stormdata_dll.StructError("turret", FullName);

    }
    // data section
    public float SpeedX, SpeedY;
    public float MinX, MaxX, MinY, MaxY;
    public int myCraftTurret;
    public WPN_DATA WeaponData;
    public List<string> GunNamesList = new List<string>();

    //Storm Original mode
    //public float myTurretPauseTimeBase;
    //public float myTurretPauseTimeDelta;
    //public float myTurretFireTimeBase;
    //public float myTurretFireTimeDelta;

    private MakeLinkStageData lateData=new MakeLinkStageData();
    public struct MakeLinkStageData
    {
        public string WeaponData;
    }
    public static void insertTurretData(READ_TEXT_STREAM st)
    {
        TURRET_DATA tData = new TURRET_DATA(st.GetNextItem());
        StormLog.LogMessage("Loading Turret " + tData.FullName + " [OK]", StormLog.logPriority.DEBUG);
        SUBOBJ_DATA.InsertSubobjData(tData, st.LineNumber()).Load(st);
    }

    //public static void loadTurretData(PackType db)
    //{
    //    LoadUtils.parseData(db, "turret", "Turrets", "Turrets.txt", "[STORM TURRET DATA FILE V1.1]", "Turret", insertTurretData);
    //}
    public static void loadTurretData(IMappedDb db)
    {
        LoadUtils.parseData(db, "turret", "Turrets", "Turrets.txt", "[STORM TURRET DATA FILE V1.1]", "Turret", insertTurretData);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(base.ToString());
        sb.AppendLine("Guns:");
        foreach (string Gun in GunNamesList)
        {
            sb.AppendLine("Gun: " + Gun);
        }

        //sb.AppendLine(base.ToString());
        return sb.ToString();
    }
}