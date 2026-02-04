using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Класс описания крупных летающих и плавающих юнитов
/// </summary>
public class CARRIER_DATA : OBJECT_DATA
{
    /// <summary>
    /// TRUE=морской юнит
    /// </summary>
    public bool IsSeaUnit;                   // TRUE=морской юнит
                                             // линейные значения
    /// <summary>
    /// максимальная горизонтальная скорость
    /// </summary>
    public float MaxSpeedZ;                   // максимальная горизонтальная скорость
    /// <summary>
    /// максимальная вертикальная скорость
    /// </summary>
    public float MaxSpeedY;                   // максимальная вертикальная скорость
    /// <summary>
    /// Обратная горизонтальная скорость
    /// 1.f/MaxSpeedZ 
    /// </summary>
    public float OOMaxSpeedZ;                 // 1.f/MaxSpeedZ

    /// <summary>
    /// максимальное горизонтальное ускорение
    /// </summary>
    public float MaxAccelZ;                   // максимальное горизонтальное ускорение
    /// <summary>
    /// максимальное вертикальное ускорение
    /// </summary>
    public float MaxAccelY;                   // максимальное вертикальное ускорение

    /// <summary>
    /// максимальная угловая скорость поворота в горизонтальной плоскости
    /// </summary>
    public float ASpeedX;                     // максимальная угловая скорость поворота в горизонтальной плоскости
    /// <summary>
    /// максимальная угловая скорость поворота в вертикальной плоскости
    /// </summary>
    public float ASpeedY;                     // максимальная угловая скорость поворота в вертикальной плоскости

    /// <summary>
    /// максимальное угловое ускорение в горизонтальной плоскости
    /// </summary>
    public float AAccelX;                     // максимальное угловое ускорение в горизонтальной плоскости
    /// <summary>
    /// максимальное угловое ускорение в вертикальной плоскости
    /// </summary>
    public float AAccelY;                     // максимальное угловое ускорение в вертикальной плоскости

    /// <summary>
    /// максимальный угол тангажа (поворот вокруг оси "Вправо")
    /// </summary>
    public float PitchLimit;                  // максимальный угол наклона

    public delegate void CarrierLoadCallbacks(Stream stream);

    public static void insertSeaCarrierData(READ_TEXT_STREAM st)
    {
        CARRIER_DATA cData = new CARRIER_DATA(st.GetNextItem(), true);
        StormLog.LogMessage("Loading sea carrier " + cData.FullName + " [OK]",StormLog.logPriority.DEBUG);
        InsertObjectData(cData, st.LineNumber()).Load(st);
    }
    public static void insertAirCarrierData(READ_TEXT_STREAM st)
    {
        CARRIER_DATA cData = new CARRIER_DATA(st.GetNextItem(), false);
        StormLog.LogMessage("Loading air carrier " + cData.FullName + " [OK]",StormLog.logPriority.DEBUG);
        InsertObjectData(cData, st.LineNumber()).Load(st);
    }

    public CARRIER_DATA(string name, bool sea_unit) : base(name)
    {
        IsSeaUnit = sea_unit;
        Flags |= sea_unit ? OC_SEASHIP : OC_AIRSHIP;

        MaxSpeedZ = 0;
        MaxAccelZ = 0;
        ASpeedX = 0;
        AAccelX = 0;

        MaxSpeedY = 0;
        MaxAccelY = 0;
        ASpeedY = 0;
        AAccelY = 0;
        OOMaxSpeedZ = 0;

        PitchLimit = 0;
    }

    public override void ProcessToken(READ_TEXT_STREAM st, string value)
    {
        do
        {
            if (st.LoadFloatC(ref MaxSpeedZ, "MaxSpeedZ", Storm.Math.KPH2MPS)) continue;
            if (st.LoadFloat(ref MaxAccelZ, "MaxAccelZ")) continue;
            if (st.LoadFloatC(ref ASpeedX, "ASpeedX", Storm.Math.GRD2RD)) continue;
            if (st.LoadFloatC(ref AAccelX, "AAccelX", Storm.Math.GRD2RD)) continue;
            if (!IsSeaUnit)
            {
                if (st.LoadFloatC(ref MaxSpeedY, "MaxSpeedY", Storm.Math.KPH2MPS)) continue;
                if (st.LoadFloat(ref MaxAccelY, "MaxAccelY")) continue;
                if (st.LoadFloatC(ref ASpeedY, "ASpeedY", Storm.Math.GRD2RD)) continue;
                if (st.LoadFloatC(ref AAccelY, "AAccelY", Storm.Math.GRD2RD)) continue;
                if (st.LoadFloat(ref PitchLimit, "PitchLimit")) continue;
            }

            ////TODO! Реализовать работу авианосца с ангаром!
            //string tmp = string.Empty;
            //if (st.Load_String(ref tmp, "PointForStart")) continue;
            //if (st.Load_String(ref tmp, "PointForCraft")) continue;
            //if (st.Load_String(ref tmp, "PointForINT")) continue;

            base.ProcessToken(st, value);
        } while (false);
    }

    public override void Reference(OBJECT_DATA referenceData)
    {
        base.Reference(referenceData);
        if (referenceData.GetClass() != OC_AIRSHIP && referenceData.GetClass() != OC_SEASHIP)
            return;

        // CRAFT_DATA
        CARRIER_DATA rr = (CARRIER_DATA)referenceData;
        IsSeaUnit = rr.IsSeaUnit;
        MaxSpeedZ = rr.MaxSpeedZ;
        MaxSpeedY = rr.MaxSpeedY;
        MaxAccelZ = rr.MaxAccelZ;
        MaxAccelY = rr.MaxAccelY;
        ASpeedX = rr.ASpeedX;
        ASpeedY = rr.ASpeedY;
        AAccelX = rr.AAccelX;
        AAccelY = rr.AAccelY;
        PitchLimit = rr.PitchLimit;
    }

    public override void MakeLinks()
    {
        base.MakeLinks();
        if (MaxSpeedZ > 0.001)
            OOMaxSpeedZ = 1f / MaxSpeedZ;
        PitchLimit = Mathf.Cos(Storm.Math.GRD2RD(90f - PitchLimit));
    }

    public static void loadCarrierData(IMappedDb db)
    {
        //Stream st = GameDataHolder.GetResource<Stream>(db, "Carriers");
        //Debug.Log("Data size: " + st.Length);

        //LoadUtils lu = new LoadUtils();
        string[] keys = { "AirCarrier", "SeaCarrier" };
        LoadUtils.InsertCall[] calls = { insertAirCarrierData, insertSeaCarrierData };

        LoadUtils.parseMultiData(db, "carrier", "Carriers", "Carriers.txt", "[STORM CARRIERS DATA FILE V1.0]", keys, calls);
        //st.Close();


        //Debug.Log("DatasSubjObj count: " + ObjDatasHolder.DatasSubjObj.Count);
    }
    //public static void loadCarrierData(PackType db)
    //{
    //    //Stream st = GameDataHolder.GetResource<Stream>(db, "Carriers");
    //    //Debug.Log("Data size: " + st.Length);

    //    //LoadUtils lu = new LoadUtils();
    //    string[] keys = { "AirCarrier", "SeaCarrier" };
    //    LoadUtils.InsertCall[] calls = { insertAirCarrierData, insertSeaCarrierData };

    //    LoadUtils.parseMultiData(db, "carrier", "Carriers", "Carriers.txt", "[STORM CARRIERS DATA FILE V1.0]", keys, calls);
    //    //st.Close();


    //    //Debug.Log("DatasSubjObj count: " + ObjDatasHolder.DatasSubjObj.Count);
    //}

}


