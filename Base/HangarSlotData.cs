using UnityEngine;

public class HangarSlotData
{
    public HangarSlotInfo myInfo;
    public Vector3 myTakeoffOrg;
    public Vector3 myLandOrg;
    public bool myHaveLand;
    public bool myHaveTakeoff;

    public void setLandOrg(Vector3 org) { myLandOrg=org;myHaveLand=true; }
    public void setTakeoffOrg(Vector3 org) { myTakeoffOrg = org; myHaveTakeoff = true; }

    public HangarSlotData()
    {
        myInfo = null;
        myTakeoffOrg = Vector3.zero;
        myLandOrg = Vector3.zero;
        myHaveLand = false;
        myHaveTakeoff = false;
    }
};