public class SUB_HANGAR_DATA : SUBOBJ_DATA
{

    public SUB_HANGAR_DATA(string n) : base(n)
    {
        SetFlag(SC_HANGAR);
        SetFlag(SF_DETACHED);
        mySoundName = "Default";
        myDoorName = "Door";
        myHangarData = new HangarData();
    }

    public override void ProcessToken(READ_TEXT_STREAM st,string value)
    {
        do
        {
            if(st.LdHS(ref myHangarData.BaseSlot, "BaseSlot")) continue;

            if (st.LdHS(ref myHangarData.myBF.myTakeoffSlot, "BfTakeoffSlot")) continue;
            if (st.LdHS(ref myHangarData.myBF.myLandSlot, "BfTouchdownSlot")) continue;
            if (st.LoadFloatC(ref myHangarData.myBF.myTakeoffAngle, "BfTakeoffAngle", Storm.Math.GRD2RD)) continue;
            if (st.LoadFloatC(ref myHangarData.myBF.myLandAngle, "BfTouchdownAngle", Storm.Math.GRD2RD)) continue;
            if (st.LoadFloat(ref myHangarData.myBF.myTakeoffTime, "BfTakeoffTime")) continue;

            if (st.LdHS(ref myHangarData.myInt.myTakeoffSlot, "IntTakeoffSlot")) continue;
            if (st.LdHS(ref myHangarData.myInt.myLandSlot, "IntTouchdownSlot")) continue;
            if (st.LoadFloatC(ref myHangarData.myInt.myTakeoffAngle, "IntTakeoffAngle", Storm.Math.GRD2RD)) continue;
            if (st.LoadFloatC(ref myHangarData.myInt.myLandAngle, "IntTouchdownAngle", Storm.Math.GRD2RD)) continue;
            if (st.LoadFloat(ref myHangarData.myInt.myTakeoffTime, "IntTakeoffTime")) continue;

            if (st.LdHS(ref myHangarData.myVehicle.myTakeoffSlot, "VehicleDepartSlot")) continue;
            if (st.LdHS(ref myHangarData.myVehicle.myLandSlot, "VehicleArriveSlot")) continue;

            if (st.LdAS(ref mySoundName, "Sound")) continue;
            if (st.LdAS(ref myDoorName, "Door")) continue;

            base.ProcessToken(st,value);
        } while (false);

    }
    public override void Reference(SUBOBJ_DATA referenceData)
    {
        base.Reference(referenceData);
        if (referenceData.GetClass() != SC_HANGAR) return;
        // HANGAR_DATA
        SUB_HANGAR_DATA rr = (SUB_HANGAR_DATA)referenceData;
        myHangarData = rr.myHangarData;
        mySoundName = rr.mySoundName;
        myDoorName = rr.myDoorName;

    }


    public HangarData myHangarData;
    public string mySoundName;
    public string myDoorName;

    
}