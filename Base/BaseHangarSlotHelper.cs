using DWORD = System.UInt32;

public class BaseHangarSlotHelper : ISlotEnum
{
    private BaseHangar rHangar;
    private HangarData rData;
    public virtual bool ProcessSlot(SLOT_DATA sld, int slot_id, FPO r)
    {
        DWORD cn = Hasher.HshString(new string(sld.Name));
        // ищем соответствующее имя
        if (cn == rData.myBF.myTakeoffSlot)
            rHangar.myBFSlot.setTakeoffOrg(sld.Org);
        if (cn == rData.myBF.myLandSlot)
            rHangar.myBFSlot.setLandOrg(sld.Org);
        if (cn == rData.myInt.myTakeoffSlot)
            rHangar.myIntSlot.setTakeoffOrg(sld.Org);
        if (cn == rData.myInt.myLandSlot)
            rHangar.myIntSlot.setLandOrg(sld.Org);
        if (cn == rData.myVehicle.myTakeoffSlot)
            rHangar.myVehicleSlot.setTakeoffOrg(sld.Org);
        if (cn == rData.myVehicle.myLandSlot)
            rHangar.myVehicleSlot.setLandOrg(sld.Org);
        if (cn == rData.BaseSlot)
            rHangar.setBaseOrg(sld.Org);
        return true;
    }
    public BaseHangarSlotHelper(BaseHangar h, HangarData d) {
        rHangar = h;
        rData = d;
    }
};