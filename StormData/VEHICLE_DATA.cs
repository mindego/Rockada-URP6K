using UnityEngine;

public class VEHICLE_DATA : OBJECT_DATA
{
    //consts
    public const int MAX_TERRAIN_TYPE_COUNT = 8;

    // internal part
    public VEHICLE_DATA(string n) : base(n)
    {
        Flags |= OC_VEHICLE;
        MaxSpeed = Storm.Math.KPH2MPS(80);
        MaxAccel = 3;
        MaxAngleSpeed = Storm.Math.GRD2RD(30);
        Dust = null;
        HipAngle = Storm.Math.GRD2RD(30);
        ThighHeight = 1;
        for (int i = 0; i < MAX_TERRAIN_TYPE_COUNT; i++)
            SpeedCoeff[i] = 1f;

    }
    public override void ProcessToken(READ_TEXT_STREAM st,string c)
    {
        do
        {
            if(st.LoadFloatC(ref MaxSpeed, "MaxSpeed", Storm.Math.KPH2MPS)) continue;
            if (st.LoadFloat(ref MaxAccel, "Accel")) continue; ;
            if (st.LoadFloatC(ref MaxAngleSpeed, "AngleSpeed", Storm.Math.GRD2RD)) continue; ;
            if (st.LdHST<DUST_DATA>(ref Dust, "Dust")) continue; ;
            if (st.LoadFloatC(ref HipAngle, "HipAngle", Storm.Math.GRD2RD)) continue; ;
            if (st.LoadFloat(ref ThighHeight, "ThighHeight")) continue; ;
            if (st.LoadFloat(ref SpeedCoeff[0], "SpeedCoeff[0]")) continue; ;
            if (st.LoadFloat(ref SpeedCoeff[1], "SpeedCoeff[1]")) continue; ;
            if (st.LoadFloat(ref SpeedCoeff[2], "SpeedCoeff[2]")) continue; ;
            if (st.LoadFloat(ref SpeedCoeff[3], "SpeedCoeff[3]")) continue; ;
            if (st.LoadFloat(ref SpeedCoeff[4], "SpeedCoeff[4]")) continue; ;
            if (st.LoadFloat(ref SpeedCoeff[5], "SpeedCoeff[5]")) continue; ;
            if (st.LoadFloat(ref SpeedCoeff[6], "SpeedCoeff[6]")) continue; ;
            if (st.LoadFloat(ref SpeedCoeff[7], "SpeedCoeff[7]")) continue; ;
            base.ProcessToken(st, c);
        } while (false);

    }
    public override void Reference(OBJECT_DATA dataref)
    {
        base.Reference(dataref);

        if (dataref.GetClass() != OBJECT_DATA.OC_VEHICLE) return;
        // VEHICLE_DATA
        VEHICLE_DATA rr = (VEHICLE_DATA) dataref;
        MaxSpeed = rr.MaxSpeed;
        MaxAccel = rr.MaxAccel;
        MaxAngleSpeed = rr.MaxAngleSpeed;
        OO_MaxSpeed = rr.OO_MaxSpeed;
        Dust = rr.Dust;
        HipAngle = rr.HipAngle;
        ThighHeight = rr.ThighHeight;
        for (int i = 0; i < MAX_TERRAIN_TYPE_COUNT; i++)
            SpeedCoeff[i] = rr.SpeedCoeff[i];

    }
    public override void MakeLinks()
    {
        base.MakeLinks();
        OO_MaxSpeed = 1.0f / MaxSpeed;
        StormDataHDR.RSLV<DUST_DATA>(ref Dust);
    }
    // data section
    public float MaxSpeed, MaxAccel, MaxAngleSpeed, OO_MaxSpeed;
    public float HipAngle, ThighHeight;
    public DUST_DATA Dust;
    public float[] SpeedCoeff = new float[MAX_TERRAIN_TYPE_COUNT];

    public static void insertVehicleData(READ_TEXT_STREAM st)
    {
        VEHICLE_DATA vData = new VEHICLE_DATA(st.GetNextItem());
        StormLog.LogMessage("Loading vehicle " + vData.FullName + " [OK]",StormLog.logPriority.DEBUG);
        OBJECT_DATA.InsertObjectData(vData, st.LineNumber()).Load(st);
    }

    //public static void loadVehicleData(PackType db)
    //{
    //    //LoadUtils lu = new LoadUtils();
    //    LoadUtils.parseData(db, "vehicle", "Vehicles", "Vehicles.txt", "[STORM VEHICLES DATA FILE V1.1]", "Vehicle", insertVehicleData);
    //}
    public static void loadVehicleData(IMappedDb db)
    {
        //LoadUtils lu = new LoadUtils();
        LoadUtils.parseData(db, "vehicle", "Vehicles", "Vehicles.txt", "[STORM VEHICLES DATA FILE V1.1]", "Vehicle", insertVehicleData);
    }
}