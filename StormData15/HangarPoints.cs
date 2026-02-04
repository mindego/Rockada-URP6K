using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using crc32 = System.UInt32;

public static class HangarDefs
{
    public const int SLOT_CRAFT = 0;
    public const int SLOT_INT = 1;
    public const int SLOT_VEHICLE = 2;

    public const uint HANGARS_LAND_ONLY = 0x00000001;
    public const uint HANGARS_TAKEOFF_ONLY = 0x00000002;

    public const uint HANGARS_LAND_TAKEOFF = 0x00000003;

}

public class HangarSlotInfo
{
    public crc32 myTakeoffSlot;
    public crc32 myLandSlot;
    public float myTakeoffAngle;
    public float myLandAngle;
    public float myTakeoffTime;

    public HangarSlotInfo(uint myTakeoffSlot, uint myLandSlot, float myTakeoffAngle, float myLandAngle, float myTakeoffTime)
    {
        this.myTakeoffSlot = myTakeoffSlot;
        this.myLandSlot = myLandSlot;
        this.myTakeoffAngle = myTakeoffAngle;
        this.myLandAngle = myLandAngle;
        this.myTakeoffTime = myTakeoffTime;
    }

    public override string ToString()
    {
        return GetType() + myTakeoffSlot.ToString("X8");
    }
}
public class HangarData
{
    // data section
    public crc32 BaseSlot;
    public HangarSlotInfo myBF;
    public HangarSlotInfo myInt;
    public HangarSlotInfo myVehicle;

    public HangarData()
    {
        BaseSlot = CRC32.CRC_NULL;

        //HangarSlotInfo $ = { Storm.CRC32.CRC_NULL,Storm.CRC32.CRC_NULL,Storm.Math.GRD2RD(30f),Storm.Math.GRD2RD(30f),3f};
        //myBF = $;
        //myInt = $;
        //myVehicle = $;

        HangarSlotInfo tmp = new HangarSlotInfo(CRC32.CRC_NULL, CRC32.CRC_NULL, Storm.Math.GRD2RD(30f), Storm.Math.GRD2RD(30f), 3f);
        myBF = tmp;
        myInt = tmp;
        myVehicle = tmp;
    }

    public override string ToString()
    {
        return GetType().ToString() + "\n" +
            BaseSlot.ToString("X8") + "\n" +
            myBF + "\n" +
            myInt + "\n" +
            myVehicle;
    }
}
