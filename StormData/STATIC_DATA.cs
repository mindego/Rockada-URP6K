using System.Collections.Generic;
using UnityEngine;

public class STATIC_DATA : OBJECT_DATA
{

    public STATIC_DATA() : this("unnamed") { }
    public STATIC_DATA(string name) : base(name)
    {
        Flags |= OBJECT_DATA.OC_STATIC;
        //Delta.Set(0, 0, 0);
        Delta = Vector3.zero;
        Angle = 0;
        Avoidable = false;
    }

    public override void ProcessToken(READ_TEXT_STREAM st, string value)
    {
        do
        {
            if (st.LoadFloat(ref Delta.y, "DeltaY")) continue;
            if (st.LoadBool(ref Avoidable, "Avoidable")) continue;
            if (st.Recognize("Position"))
            {
                Delta.x = st.AtoF(st.GetNextItem());
                Delta.y = st.AtoF(st.GetNextItem());
                Delta.z = st.AtoF(st.GetNextItem());
                Angle = st.AtoF(st.GetNextItem());
                continue;

            }
            base.ProcessToken(st, value);
        } while (false);
    }

    public Vector3 Delta;
    public float Angle;
    public bool Avoidable;

    public static void loadStaticData(IMappedDb db)
    {
        LoadUtils.parseData(db, "static object", "Statics", "Statics.txt", "[STORM STATICS DATA FILE V1.1]", "Static", insertStaticData);
    }
    public static void insertStaticData(READ_TEXT_STREAM st)
    {
        STATIC_DATA sData = new STATIC_DATA(st.GetNextItem());
        StormLog.LogMessage("Loading building " + sData.FullName + " [OK]", StormLog.logPriority.DEBUG);
        InsertObjectData(sData, st.LineNumber()).Load(st);
    }
}

public class HANGAR_DATA : STATIC_DATA
{
    //SUB_HANGAR_DATA
    public HangarData myHangarData;
    public string mySoundName;
    public string myDoorName;
    public SUBOBJ_DATA HangarSubobj;
    public HANGAR_DATA(string name) : base(name)
    {

        SetFlag(OC_HANGAR);
        myHangarData = new HangarData();
        mySoundName = "Default";
        myDoorName = "Door";
    }

    public override void ProcessToken(READ_TEXT_STREAM st, string value)
    {
        do
        {
            //ДЛя СН
            //st.LdHS(ref myHangarData.BaseSlot, "BaseSlot");

            //st.LdHS(ref myHangarData.myBF.myTakeoffSlot, "BfTakeoffSlot");
            //st.LdHS(ref myHangarData.myBF.myLandSlot, "BfTouchdownSlot");
            //st.LoadFloatC(ref myHangarData.myBF.myTakeoffAngle, "BfTakeoffAngle", Storm.Math.GRD2RD);
            //st.LoadFloatC(ref myHangarData.myBF.myLandAngle, "BfTouchdownAngle", Storm.Math.GRD2RD);
            //st.LoadFloat(ref myHangarData.myBF.myTakeoffTime, "BfTakeoffTime");

            //st.LdHS(ref myHangarData.myInt.myTakeoffSlot, "IntTakeoffSlot");
            //st.LdHS(ref myHangarData.myInt.myLandSlot, "IntTouchdownSlot");
            //st.LoadFloatC(ref myHangarData.myInt.myTakeoffAngle, "IntTakeoffAngle", Storm.Math.GRD2RD);
            //st.LoadFloatC(ref myHangarData.myInt.myLandAngle, "IntTouchdownAngle", Storm.Math.GRD2RD);
            //st.LoadFloat(ref myHangarData.myInt.myTakeoffTime, "IntTakeoffTime");

            //st.LdHS(ref myHangarData.myVehicle.myTakeoffSlot, "VehicleDepartSlot");
            //st.LdHS(ref myHangarData.myVehicle.myLandSlot, "VehicleArriveSlot");

            //st.LdAS(ref mySoundName, "Sound");
            //st.LdAS(ref myDoorName, "Door");

            //base.ProcessToken(st, value);

            //КОнфиг СН
            //BaseSlot = "Appear"
            //BfTakeoffSlot = "BF_HVY"
            //BfTouchdownSlot = "BF_HVY"
            //IntTakeoffSlot = "IntTouch"
            //IntTouchdownSlot = "IntTouch"
            //VehicleDepartSlot = "TankT"
            //VehicleArriveSlot = "TankT"


            //Конфиг ОШ
            //PointForStart = "Appear"
            //PointForCraft = "BF_HVY"
            //PointForINT = "IntTouch"
            //PointForTank = "TankT"

            //Для ОШ
            if (st.LdHS(ref myHangarData.BaseSlot, "PointForStart")) continue;

            if (st.LdHS(ref myHangarData.myBF.myTakeoffSlot, "PointForCraft")) continue;
            if (st.LdHS(ref myHangarData.myBF.myLandSlot, "PointForCraft")) continue;
            if (st.LoadFloatC(ref myHangarData.myBF.myTakeoffAngle, "BfTakeoffAngle", Storm.Math.GRD2RD)) continue;
            if (st.LoadFloatC(ref myHangarData.myBF.myLandAngle, "BfTouchdownAngle", Storm.Math.GRD2RD)) continue;
            if (st.LoadFloat(ref myHangarData.myBF.myTakeoffTime, "BfTakeoffTime")) continue;

            if (st.LdHS(ref myHangarData.myInt.myTakeoffSlot, "PointForINT")) continue;
            if (st.LdHS(ref myHangarData.myInt.myLandSlot, "PointForINT")) continue;
            if (st.LoadFloatC(ref myHangarData.myInt.myTakeoffAngle, "IntTakeoffAngle", Storm.Math.GRD2RD)) continue;
            if (st.LoadFloatC(ref myHangarData.myInt.myLandAngle, "IntTouchdownAngle", Storm.Math.GRD2RD)) continue;
            if (st.LoadFloat(ref myHangarData.myInt.myTakeoffTime, "IntTakeoffTime")) continue;

            if (st.LdHS(ref myHangarData.myVehicle.myTakeoffSlot, "PointForTank")) continue;
            if (st.LdHS(ref myHangarData.myVehicle.myLandSlot, "PointForTank")) continue;

            if (st.LdAS(ref mySoundName, "Sound")) continue;
            if (st.LdAS(ref myDoorName, "Door")) continue;

            base.ProcessToken(st, value);
        } while (false);
        //TODO! В ОШ это фиксировано.
    }
    /// <summary>
    /// Загрузить данные об статических объектах-ангарах. Требуется только для ОШ, в СН ангары - отдельный подобъект обычного static объекта
    /// </summary>
    /// <param name="db"></param>
    public static void loadHangarData(IMappedDb db)
    {
        //LoadUtils lu = new LoadUtils();
        //Это для СН. В ОШ ангары описаны как отдельный тип зданий.
        LoadUtils.parseData(db, "static hangar object", "Statics", "Statics.txt", "[STORM STATICS DATA FILE V1.1]", "Hangar", insertHangarData);

        //string[] keys = { "Static", "Hangar" };
        //LoadUtils.InsertCall[] calls = { insertStaticData, insertStaticData };

        //LoadUtils.parseMultiData(db, "static object", "Statics", "Statics.txt", "[STORM STATICS DATA FILE V1.1]", keys, calls);
    }

    public static void insertHangarData(READ_TEXT_STREAM st)
    {
        HANGAR_DATA sData = new HANGAR_DATA(st.GetNextItem());

        var lineNumber = st.LineNumber();
        sData.Load(st);
        sData.mySoundName = "Default";
        sData.myDoorName = "Door";

        var mpRenderer = Renderer.CreateRenderer(null, new LOG(), null, RendererApi.RENDERER_VERSION);
        var mpFpoLoader = (IFpoLoader)FpoLoader.CreateFpoLoader(mpRenderer, null, "objects2.dat", "objects.dat");
        if (mpFpoLoader == null)
        {
            Debug.Log("FPO loader failed to load itself");
            return;
        }
        //Debug.Log(sData);
        //Debug.Log(sData.RootData);
        FPO myFPO = mpFpoLoader.CreateFPO(sData.RootData.FileName);

        foreach (var z in sData.RootData.SubobjDatas)
        {
            Debug.Log("Hangar Subobjs: " + z.FullName);
            FPO tmpFPO = (FPO)myFPO.GetSubObject(z.FullName);
            if (tmpFPO == null) continue;

            if (tmpFPO.GetSubObject(sData.myDoorName) != null) sData.HangarSubobj = z;
        }

        StormLog.LogMessage("Loading hangar building " + sData.FullName + " [OK]", StormLog.logPriority.DEBUG);
        //Debug.Log("Loading hangar building " + sData.FullName + " [OK]");
        //InsertObjectData(sData, st.LineNumber()).Load(st);
        InsertObjectData(sData, lineNumber);
    }

//    private Dictionary<string, string[]> SubhangarNames;
//    private void LoadSubhangarNames()
//    {
//        SubhangarNames.Add("Craft Hangar Big", new string[] { "Bunker" });
//        SubhangarNames.Add("Craft Hangar",  new string[] {"Bunker"});
//        SubhangarNames.Add("Tank Hangar", new string[] {  "Barak","Hangar");

//Tank Hangar Small
//V NULL-T Station
//V Craft Hangar Underwater
//V Craft Hangar
//V Craft Hangar Wide
//V Craft Hangar Big
//V Tank Hangar
//Multiplayer Start
//    }
}
