using UnityEngine;
using crc32 = System.UInt32;

public class CRAFT_DATA : OBJECT_DATA
{
    // internal part
    public CRAFT_DATA(string n, bool plane) : base(n)
    {
        Flags |= OC_CRAFT;
        MainDuza = 0;
        DamageBitmap = string.Empty;
        ViewDelta.Set(0, 0, 0);
        BackViewDelta = ViewDelta;
        IsPlane = plane;
        W = 5000;
        TSC = .25f;
        if (IsPlane)
        {
            DragC.Set(0, 0, Storm.Math.KPH2MPS(800));
            MaxSpeed.Set(0, 0, Storm.Math.KPH2MPS(800));
            MinSpeed.Set(0, 0, 0);
            WingLiftC = Storm.Math.KPH2MPS(300);
            TailLiftC = Storm.Math.KPH2MPS(300);
            MaxASpeed.Set(Storm.Math.GRD2RD(20), Storm.Math.GRD2RD(15), Storm.Math.GRD2RD(70));
        }
        else
        {
            DragC.Set(Storm.Math.KPH2MPS(400), Storm.Math.KPH2MPS(200), Storm.Math.KPH2MPS(600));
            MaxSpeed.Set(0, Storm.Math.KPH2MPS(300), Storm.Math.KPH2MPS(450));
            MinSpeed.Set(0, 0, 0);
            MaxASpeed.Set(Storm.Math.GRD2RD(60), Storm.Math.GRD2RD(15), Storm.Math.GRD2RD(180));
        }
        MaxAAccel = MaxASpeed;
        YawC = 24;
        RollLimit = Storm.Math.GRD2RD(60);
        PitchLimit = Storm.Math.GRD2RD(90);
        Dust = null;
        // таблица коэфф. тяги от высоты
        Alt[0] = 0; ThrustC[0] = 1.2f;
        Alt[1] = 50; ThrustC[1] = 1f;
        Alt[2] = 1500; ThrustC[2] = .75f;
        Alt[3] = 2000; ThrustC[3] = .25f;
        // таблица коэфф. угловых значений от скорости
        Speed[0] = 0; CornerC[0] = .5f;
        Speed[1] = .5f; CornerC[1] = 1f;
        Speed[2] = .75f; CornerC[2] = 1f;
        Speed[3] = 1f; CornerC[3] = .5f;
        // хрень всякая
        BatteryCharge = 2000;
        BatteryRechargeC = 50;
        RegenerateC = 10;
        RegenerateSpeed = 3;
        // столкновения с землёй для ИИ
        AiGrndCollC = 10;
        AiGrndCollBase = 0;

        HoleObject = Constants.UndefinedID;
        HoleNumber = 0;
        HoleNumberDisp = 0;

    }
    public override void ProcessToken(READ_TEXT_STREAM st, string c)
    {
        do
        {
            if (st.LdHS(ref MainDuza, "Duza")) continue;
            if (st.LoadFloat(ref W, "Weight")) continue;
            if (IsPlane == true)
            {
                if (st.LoadFloatC(ref TailLiftC, "TailStallSpeed", Storm.Math.KPH2MPS)) continue;
                if (st.LoadFloatC(ref WingLiftC, "WingStallSpeed", Storm.Math.KPH2MPS)) continue;
            }
            else
            {
                if (st.LoadFloatC(ref MaxSpeed.x, "MaxSpeedX", Storm.Math.KPH2MPS)) continue;
                if (st.LoadFloatC(ref MaxSpeed.y, "MaxSpeedY", Storm.Math.KPH2MPS)) continue;
                if (st.LoadFloatC(ref MinSpeed.y, "MinSpeedY", Storm.Math.KPH2MPS)) continue;
                if (st.LoadFloatC(ref MinSpeed.z, "MinSpeedZ", Storm.Math.KPH2MPS)) continue;
                if (st.LoadFloatC(ref DragC.x, "FallSpeedX", Storm.Math.KPH2MPS)) continue;
                if (st.LoadFloatC(ref DragC.y, "FallSpeedY", Storm.Math.KPH2MPS)) continue;
            }
            if (st.LoadFloatC(ref MaxSpeed.z, "MaxSpeedZ", Storm.Math.KPH2MPS)) continue;
            if (st.LoadFloatC(ref DragC.z, "FallSpeedZ", Storm.Math.KPH2MPS)) continue;
            if (st.Recognize("ThrustCAtMaxSpeed"))
            {
                TSC = st.AtoF(st.GetNextItem()); if (TSC <= .0f || TSC > 1f) stormdata_dll.ParseError("thrustcatmaxspeed", c);
                return;
            }
            if (st.Recognize("ThrustCFromAlt"))
            {
                for (int idx = 0; idx < 4; idx++)
                {
                    Alt[idx] = st.AtoF(st.GetNextItem());
                    ThrustC[idx] = st.AtoF(st.GetNextItem());
                }
                return;
            }
            if (st.Recognize("CornerCFromSpeed"))
            {
                for (int idx = 0; idx < 4; idx++)
                {
                    Speed[idx] = st.AtoF(st.GetNextItem());
                    CornerC[idx] = st.AtoF(st.GetNextItem());
                }
                return;
            }
            if (st.LoadFloatC(ref MaxAAccel.x, "AAccelX", Storm.Math.GRD2RD)) continue;
            if (st.LoadFloatC(ref MaxAAccel.y, "AAccelY", Storm.Math.GRD2RD)) continue;
            if (st.LoadFloatC(ref MaxAAccel.z, "AAccelZ", Storm.Math.GRD2RD)) continue;
            if (st.LoadFloatC(ref MaxASpeed.x, "ASpeedX", Storm.Math.GRD2RD)) continue;
            if (st.LoadFloatC(ref MaxASpeed.y, "ASpeedY", Storm.Math.GRD2RD)) continue;
            if (st.LoadFloatC(ref MaxASpeed.z, "ASpeedZ", Storm.Math.GRD2RD)) continue;
            if (st.LoadFloatC(ref YawC, "YawSpeed", Storm.Math.GRD2RD)) continue;
            if (st.LoadFloatC(ref RollLimit, "RollLimit", Storm.Math.GRD2RD)) continue;
            if (st.LoadFloatC(ref PitchLimit, "PitchLimit", Storm.Math.GRD2RD)) continue;

            if (st.Recognize("ViewDelta"))
            {
                ViewDelta.x = st.AtoF(st.GetNextItem());
                ViewDelta.y = st.AtoF(st.GetNextItem());
                ViewDelta.z = st.AtoF(st.GetNextItem());
                BackViewDelta = ViewDelta;
                continue;
            }
            if (st.Recognize("BackViewDelta"))
            {
                BackViewDelta.x = st.AtoF(st.GetNextItem());
                BackViewDelta.y = st.AtoF(st.GetNextItem());
                BackViewDelta.z = st.AtoF(st.GetNextItem());
                continue;
            }

            if (st.LoadFloat(ref BatteryCharge, "BatteryCharge")) continue;
            if (st.LoadFloat(ref BatteryRechargeC, "BatteryRechargeC")) continue;
            if (st.LoadFloat(ref RegenerateC, "RegenerateC")) continue;
            if (st.LoadFloat(ref RegenerateSpeed, "RegenerateSpeed")) continue;

            if (st.LdAS(ref DamageBitmap, "DamageBitmap")) continue;
            if (st.LdHST<DUST_DATA>(ref Dust, "Dust")) continue;

            if (st.Recognize("Root"))
            {
                RootData = new CRAFT_PART_DATA(st.GetNextItem());
                RootData.Load(st);
                continue;
            }

            // столкновения с землёй для ИИ
            if (st.LoadFloat(ref AiGrndCollC, "AiGrndCollC")) continue;
            if (st.LoadFloat(ref AiGrndCollBase, "AiGrndCollBase")) continue;

            // Дырки в кабине
            if (st.LdHS(ref HoleObject, "HoleName")) continue;
            if (st.LoadInt(ref HoleNumber, "HoleNumber")) continue;
            if (st.LoadInt(ref HoleNumberDisp, "HoleNumberDisp")) continue;

            base.ProcessToken(st, c);
        } while (false);
    }
    public override void Reference(OBJECT_DATA dataref)
    {
        base.Reference(dataref);
        if (dataref.GetClass() != OC_CRAFT) return;
        // CRAFT_DATA
        CRAFT_DATA rr = (CRAFT_DATA)dataref;
        DamageBitmap = rr.DamageBitmap;
        IsPlane = rr.IsPlane;
        W = rr.W;
        MaxSpeed = rr.MaxSpeed;
        MinSpeed = rr.MinSpeed;
        DragC = rr.DragC;
        TP = rr.TP;
        TM = rr.TM;
        TSC = rr.TSC;
        WingLiftC = rr.WingLiftC;
        TailLiftC = rr.TailLiftC;
        RollLimit = rr.RollLimit;
        PitchLimit = rr.PitchLimit;
        YawC = rr.YawC;
        MaxASpeed = rr.MaxASpeed;
        MaxAAccel = rr.MaxAAccel;
        Dust = rr.Dust;
        ViewDelta = rr.ViewDelta;
        BackViewDelta = rr.BackViewDelta;
        BatteryCharge = rr.BatteryCharge;
        BatteryRechargeC = rr.BatteryRechargeC;
        RegenerateC = rr.RegenerateC;
        RegenerateSpeed = rr.RegenerateSpeed;
        MainDuza = rr.MainDuza;
        for (int i = 0; i < 4; i++)
        {
            Alt[i] = rr.Alt[i];
            ThrustC[i] = rr.ThrustC[i];
            Speed[i] = rr.Speed[i];
            CornerC[i] = rr.CornerC[i];
        }
        // столкновения с землёй для ИИ
        AiGrndCollC = rr.AiGrndCollC;
        AiGrndCollBase = rr.AiGrndCollBase;

        // Дырки в кабине
        HoleObject = rr.HoleObject;
        HoleNumber = rr.HoleNumber;
        HoleNumberDisp = rr.HoleNumberDisp;
    }
    public override void MakeLinks()
    {
        //Debug.Log("Making Links for " + FullName);
        base.MakeLinks();
        StormDataHDR.RSLV<DUST_DATA>(ref Dust);
        // аэродинамика
        if (IsPlane == true)
        {
            StallSpeed = WingLiftC;
            WingLiftC = W * STORM_DATA.GAcceleration / (Mathf.Pow(WingLiftC, 2) * Storm.Math.GRD2RD(15));
            TailLiftC = W * STORM_DATA.GAcceleration / (Mathf.Pow(TailLiftC, 2) * Storm.Math.GRD2RD(15));
        }
        else
        {
            DragC.x = GetDragC(W, DragC.x);
            DragC.y = GetDragC(W, DragC.y);
        }
        DragC.z = GetDragC(W, DragC.z);
        // физика - линейные значения
        MinSpeed.x = MaxSpeed.x;
        TSC = (1f - TSC) / MaxSpeed.z;
        TP.Set(GetThrust(DragC.x, TSC, MaxSpeed.x), GetThrust(DragC.y, TSC, MaxSpeed.y), GetThrust(DragC.z, TSC, MaxSpeed.z));
        TM.Set(GetThrust(DragC.x, TSC, MinSpeed.x), GetThrust(DragC.y, TSC, MinSpeed.y), GetThrust(DragC.z, TSC, MinSpeed.z));
        float tc = 1f / Storm.Math.NormaFAbs(TP);
        TC.Set(TP.x * tc, TP.y * tc, TP.z * tc);
        OOMaxSpeed = 1f / MaxSpeed.magnitude;
        // угловые значения
        YawC /= MaxSpeed.z;
        // таблица коэфф. тяги от высоты
        ThrustCd[0] = (ThrustC[1] - ThrustC[0]) / (Alt[1] - Alt[0]);
        ThrustCd[1] = (ThrustC[2] - ThrustC[1]) / (Alt[2] - Alt[1]);
        ThrustCd[2] = (ThrustC[3] - ThrustC[2]) / (Alt[3] - Alt[2]);
        // таблица коэфф. угловых значений от скорости
        CornerCd[0] = (CornerC[1] - CornerC[0]) / (Speed[1] - Speed[0]);
        CornerCd[1] = (CornerC[2] - CornerC[1]) / (Speed[2] - Speed[1]);
        CornerCd[2] = (CornerC[3] - CornerC[2]) / (Speed[3] - Speed[2]);

    }
    // data section
    /// <summary>
    /// имя битмапа, в котором хранятся картинки деталей
    /// </summary>
    public string DamageBitmap;
    /// <summary>
    /// TRUE=самолетная модель
    /// </summary>
    public bool IsPlane;
    /// <summary>
    /// вес
    /// </summary>
    public float W;
    // линейные значения
    /// <summary>
    /// макс. скорости по осям
    /// </summary>
    public Vector3 MaxSpeed;
    /// <summary>
    /// макс. отрицательные скорости по осям
    /// </summary>
    public Vector3 MinSpeed;
    /// <summary>
    /// базовый коэфф. сопротивления
    /// </summary>
    public const float BaseDragC = 2000f;
    /// <summary>
    /// коэфф. сопротивления
    /// </summary>
    public Vector3 DragC;
    /// <summary>
    /// макс. положительная тяга
    /// </summary>
    public Vector3 TP;
    /// <summary>
    /// макс. отрицательная тяга
    /// </summary>
    public Vector3 TM;
    /// <summary>
    /// коэфф. тяги
    /// </summary>
    public Vector3 TC;
    /// <summary>
    /// коэфф. падения тяги от скорости
    /// </summary>
    public float TSC;
    /// <summary>
    /// 1./макс. скорость
    /// </summary>
    public float OOMaxSpeed;
    // крылья и хвост
    /// <summary>
    /// скорость штопора
    /// </summary>
    public float StallSpeed;
    /// <summary>
    /// коэфф. подъемной силы
    /// </summary>
    public float WingLiftC;
    /// <summary>
    /// коэфф. подъемной силы
    /// </summary>
    public float TailLiftC;
    // таблицы
    /// <summary>
    /// высота
    /// </summary>
    public float[] Alt = new float[4];
    /// <summary>
    /// коэфф. тяги от высоты
    /// </summary>
    public float[] ThrustC = new float[4];
    /// <summary>
    /// наклон на отрезках
    /// </summary>
    public float[] ThrustCd = new float[3];
    public float ThrustCFromAlt(float alt)
    {
        if (alt < Alt[1]) return (alt - Alt[0]) * ThrustCd[0] + ThrustC[0];
        if (alt < Alt[2]) return (alt - Alt[1]) * ThrustCd[1] + ThrustC[1];
        alt = (alt - Alt[2]) * ThrustCd[2] + ThrustC[2];
        return (alt > 0 ? alt : 0);
    }
    /// <summary>
    /// скосрость (sic!)
    /// </summary>
    public float[] Speed = new float[4];
    /// <summary>
    /// коэфф. угловых значений от скорости
    /// </summary>
    public float[] CornerC = new float[4];
    /// <summary>
    /// наклон на отрезках
    /// </summary>
    public float[] CornerCd = new float[3];
    public float CornerCFromSpeed(float spd)
    {
        spd *= OOMaxSpeed;
        if (spd < Speed[1]) return (spd - Speed[0]) * CornerCd[0] + CornerC[0];
        if (spd < Speed[2]) return (spd - Speed[1]) * CornerCd[1] + CornerC[1];
        spd = (spd - Speed[2]) * CornerCd[2] + CornerC[2];
        return (spd > 0 ? spd : 0);

    }
    // угловые значения
    /// <summary>
    /// лимит крена при нормальном режиме
    /// </summary>
    public float RollLimit;
    /// <summary>
    /// лимит тангажа при нормальном режиме
    /// </summary>
    public float PitchLimit;
    /// <summary>
    /// коэфф. плоского поворота
    /// </summary>
    public float YawC;
    /// <summary>
    /// макс. угловыу (sic!) скорости
    /// </summary>
    public Vector3 MaxASpeed;
    /// <summary>
    /// макс. угловые ускорения
    /// </summary>
    public Vector3 MaxAAccel;
    // пыль
    public DUST_DATA Dust;
    // хрень всякая
    public Vector3 ViewDelta, BackViewDelta;
    public float BatteryCharge, BatteryRechargeC, RegenerateC, RegenerateSpeed;
    public uint MainDuza;


    // столкновения с землёй для ИИ
    public float AiGrndCollC;
    public float AiGrndCollBase;

    public float getDeepSpeed(float theradF, float life)
    {
        return AiGrndCollBase + AiGrndCollC * (1f - theradF * .5f) * life;
    }

    // Дырки в кабине
    public crc32 HoleObject;
    public int HoleNumber;
    public int HoleNumberDisp;

    public int getNumHoles(float rnd01)
    {
        return (int)(HoleNumber - HoleNumberDisp + rnd01 * 2 * HoleNumberDisp);
    }

    private float GetDragC(float w, float spd)
    {
        return (w * STORM_DATA.GAcceleration - CRAFT_DATA.BaseDragC) / Mathf.Pow(spd, 2);
    }

    private float GetThrust(float dc, float tsc, float spd)
    {
        return (spd > .0f ? (Mathf.Pow(spd, 2) * dc + CRAFT_DATA.BaseDragC) / (1f - tsc * spd) : .0f);
    }


    public static void insertCraftData(READ_TEXT_STREAM st)
    {
        CRAFT_DATA cData = new CRAFT_DATA(st.GetNextItem(), false);
        StormLog.LogMessage("Loading craft " + cData.FullName + " [OK]", StormLog.logPriority.DEBUG);
        InsertObjectData(cData, st.LineNumber()).Load(st);
    }

    public static void insertPlaneData(READ_TEXT_STREAM st)
    {
        CRAFT_DATA cData = new CRAFT_DATA(st.GetNextItem(), true);
        StormLog.LogMessage("Loading craft " + cData.FullName + " [OK]", StormLog.logPriority.DEBUG);
        InsertObjectData(cData, st.LineNumber()).Load(st);
    }

    //public static void loadCraftData(PackType db)
    //{
    //    //LoadUtils lu = new LoadUtils();
    //    string[] keys = { "Craft", "Plane" };
    //    LoadUtils.InsertCall[] calls = { insertCraftData, insertPlaneData };
    //    LoadUtils.parseMultiData(db, "craft", "Crafts", "Crafts.txt", "[STORM CRAFTS DATA FILE V1.1]", keys, calls);
    //}
    public static void loadCraftData(IMappedDb db)
    {
        //LoadUtils lu = new LoadUtils();
        string[] keys = { "Craft", "Plane" };
        LoadUtils.InsertCall[] calls = { insertCraftData, insertPlaneData };
        LoadUtils.parseMultiData(db, "craft", "Crafts", "Crafts.txt", "[STORM CRAFTS DATA FILE V1.1]", keys, calls);
    }

};
