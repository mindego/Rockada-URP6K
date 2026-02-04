using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// BaseVehicleAutopilotTakeoff - взлет
/// </summary>
class BaseVehicleAutopilotTakeoff : BaseVehicleAutopilotManual
{
    // для кверенья
    new public const uint ID = 0x8786C3FE;
    public override object GetInterface(DWORD id)
    {
        return (id == ID ? this : null);
    }

    // интерфейс с BaseVehicle
    public BaseVehicleAutopilotTakeoff(BaseScene bs, BaseVehicle cr, iContact c, BaseHangar h) : base(bs, cr)
    {
        Hangar.setPtr(c);
        // начальная стадия - ждать открытия двери
        Stage = AutopilotTakeOffStages.tsWaitForDoor;
        h.OpenDoor();
        // определяем координаты появления
        rOwner.pFPO.Org = h.GetPointForStart();
        rOwner.pFPO.Org.y -= rOwner.GetMinY();
        mNowDest = h.GetPointFor((iContact)rOwner.GetInterface(iContact.ID), false);
        mNowDest.y -= rOwner.GetMinY();
        // сколько осталось проехать до точки отрыва
        Vector3 tmp = mNowDest - rOwner.pFPO.Org;
        myCurrentDist = BaseVehicleAutopilotLand.Norma2Plain(tmp);
        // поворячиваемся по полосе
        rOwner.pFPO.Angles2Vectors(Mathf.Atan2(tmp.x, tmp.z), 0f, 0f);
        rOwner.SetThrust(0f);
        rOwner.setHangarFlag(true);
    }

    private float GetHangarSpeed()
    {
        return rOwner.GetVehicleData().MaxSpeed * 0.3f;
    }
    ~BaseVehicleAutopilotTakeoff()
    {
        Dispose();
    }

    public override void Dispose()
    {
        if (isDisposed) return;
        rOwner.setHangarFlag(false);
        base.Dispose();
        isDisposed= true;
    }

    public override bool Move(float scale)
    {
        Hangar.Validate();
        BaseHangar h = null;
        if (Hangar.Ptr() != null && Hangar.Ptr().GetCondition() >= .0f) h = (BaseHangar)Hangar.Ptr().GetInterface(BaseHangar.ID);
        switch (Stage)
        {
            case AutopilotTakeOffStages.tsWaitForDoor: return ProcessWait(h, scale);
            case AutopilotTakeOffStages.tsRun: return ProcessRun(h, scale);
            default: return ProcessReady(h, scale);
        }
    }
    public override int GetState()
    {
        if (rOwner.pFPO == null) return iSensorsDefines.CS_DEAD;
        return (Stage == AutopilotTakeOffStages.tsReady ? iSensorsDefines.CS_IN_GAME : iSensorsDefines.CS_LEAVING_HANGAR);
    }

    // свое
    protected readonly TContact Hangar = new TContact();
    protected float myCurrentDist;
    protected enum AutopilotTakeOffStages { tsWaitForDoor, tsRun, tsReady }
    protected AutopilotTakeOffStages Stage;
    bool ProcessWait(BaseHangar h, float scale)
    {
        if (h == null || rOwner.GetCondition() <= .0f) return false;
        if (h.isDoorOpened() == true)
        {
            rOwner.SetTraceMode(false, true);
            setTargetSpeed(GetHangarSpeed());
            Stage = AutopilotTakeOffStages.tsRun;
        }
        return true;
    }
    bool ProcessRun(BaseHangar h, float scale)
    {
        if (h == null || rOwner.GetCondition() <= .0f) return false;
        // устнавливаем управление
        rOwner.SetStick(0f);
        // компенсируем силу тяжести
        setTargetSpeed(mTargetSpeed);
        bool b = rOwner.ProcessPhysic(scale);
        float dist = BaseVehicleAutopilotLand.Norma2Plain(mNowDest - rOwner.pFPO.Org);
        if (dist <= myCurrentDist)
            myCurrentDist = dist;
        else
        {   // если разбег завершен
            // закрываем дверь ангара
            h.CloseDoor();
            // убираемся из ангара
            //Hangar = null;
            Hangar.setPtr(null);
            // начинаем отрыв
            rOwner.SetThrust(0f);
            Stage = AutopilotTakeOffStages.tsReady;
        }
        return b;
    }
    bool ProcessReady(BaseHangar h, float scale)
    {
        return base.Move(scale);
    }
};