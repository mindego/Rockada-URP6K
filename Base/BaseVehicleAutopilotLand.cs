using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// BaseVehicleAutopilotLand - посадка
/// </summary>
class BaseVehicleAutopilotLand : BaseVehicleAutopilotMoveTo
{
    // для кверенья
    new public const uint ID = 0xE74D8521;
    public override object GetInterface(DWORD id)
    {
        switch (id)
        {
            case ID: return this;
            default: return base.GetInterface(id);
        }
    }

    // интерфейс с BaseVehicle
    public BaseVehicleAutopilotLand(BaseScene sc, BaseVehicle cr, iContact c, BaseHangar h) : base(sc, cr)
    {
        Hangar.setPtr(c);
        Stage = AutopilotLandingStage.lsApproach;

        // проставляемся ангару, чтобы никто не занял :)
        iContact cnt = (iContact)rOwner.GetInterface(iContact.ID);
        StartPos = h.GetPointFor(cnt, true);
        OnSetRoute(StartPos, true, 0);
        rOwner.SetThrust(1f);
        rOwner.SetTraceMode(true);
        mSetPredictions = false;
        mAwaitedForRouteToHangar = true;
        // route params
        rOwner.mPathDist = 10f;
        setTargetSpeed(rOwner.GetVehicleData().MaxSpeed);
        mpPredictionInfo = rOwner.LoadPrediction(Prediction.iPredictionRoad);
    }
    public override bool Move(float scale)
    {
        Hangar.Validate();
        BaseHangar h = null;
        if (Hangar.Ptr() != null && Hangar.Ptr().GetCondition() >= .0f) h = (BaseHangar)Hangar.Ptr().GetInterface(BaseHangar.ID);
        switch (Stage)
        {
            case AutopilotLandingStage.lsApproach: return ProcessApproach(h, scale);
            case AutopilotLandingStage.lsFinal: return ProcessFinal(h, scale);
            case AutopilotLandingStage.lsStartPoint: return ProcessStartPoint(h, scale);
            case AutopilotLandingStage.lsWaitForClose: return ProcessWaitForClose(h, scale);
            default: return ProcessWait(h, scale);
        }
    }
    public override bool IsOnTheFormation()
    {
        return true;
    }
    public override int GetState()
    {
        if (rOwner.pFPO == null) return iSensorsDefines.CS_DEAD;
        return (Hangar.Ptr() == null ? iSensorsDefines.CS_IN_GAME : iSensorsDefines.CS_ENTERING_HANGAR);
    }

    // свое
    protected  bool mAwaitedForRouteToHangar;
    readonly protected TContact Hangar = new TContact();
    protected Vector3 StartPos;
    protected float myCurrentDist;
    protected Prediction mpPred;
    private enum AutopilotLandingStage { lsApproach, lsFinal, lsStartPoint, lsWaitForClose, lsWaitForDoor }
    private AutopilotLandingStage Stage;
    bool ProcessApproach(BaseHangar h, float scale)
    {
        // проверки
        if (h != null)
        {
            if (mAwaitedForRouteToHangar)
                mAwaitedForRouteToHangar = mpCurrentRouteContainer==null;
            else if (rOwner.GetCondition() > 0f)
            {
                Vector3 df = mDest - rOwner.pFPO.Org;
                df.y = 0;
                float df_len = df.magnitude - Hangar.Ptr().GetRadius();
                if (df_len < 150f)
                {
                    // slow route params
                    rOwner.mPathDist = 1f;
                    setTargetSpeed(GetHangarSpeed());
                    mpPredictionInfo = rOwner.LoadPrediction(Prediction.iPredictionPlainDanger);
                    myCurrentDist = Norma2Plain(rOwner.pFPO.Org - StartPos);
                    Stage = AutopilotLandingStage.lsStartPoint;
                }
            }
        }
        return base.Move(scale);
    }
    bool ProcessFinal(BaseHangar h, float scale)
    {
        // проверки
        if (h!=null)
        {
            if (rOwner.GetCondition() > 0f)
            {
                float dist = Norma2Plain(StartPos - rOwner.pFPO.Org);
                if (dist <= myCurrentDist)
                    myCurrentDist = dist;
                else
                    if (h.isDoorOpened())
                {
                    h.CloseDoor();
                    setTargetSpeed(0f);
                    Stage = AutopilotLandingStage.lsWaitForClose;
                }
            }
        }
        mNowDest = StartPos;
        return rOwner.ProcessPhysic(scale);
    }
    bool ProcessStartPoint(BaseHangar h, float scale)
    {
        Vector3 dest = StartPos;
        VehicleControlsData data = getDataFromVectors(dest, rOwner.pFPO);
        rOwner.SetStick(data.myStick);
        if (data.myStick == 0f)
        {
            setTargetSpeed(GetHangarSpeed());
            float dist = Norma2Plain(rOwner.pFPO.Org - StartPos);
            if (dist <= myCurrentDist)
                myCurrentDist = dist;
            else
            {
                h.OpenDoor();    // открываем дверь
                setTargetSpeed(0f);
                StartPos = h.GetPointForStart();
                StartPos.y -= rOwner.pFPO.MinY();
                Stage = AutopilotLandingStage.lsWaitForDoor;
            }
        }
        else
            setTargetSpeed(0f);
        return rOwner.ProcessPhysic(scale);
    }
    bool ProcessWait(BaseHangar h, float scale)
    {
        if (h == null)
            Hangar.setPtr(null);
        else
        if (rOwner.GetCondition() > 0f && h.isDoorOpened() == true)
        {
            Vector3 dest = StartPos;
            VehicleControlsData data = getDataFromVectors(dest, rOwner.pFPO);
            rOwner.SetStick(data.myStick);
            if (data.myStick == 0f)
            {
                rOwner.SetTraceMode(false, true);
                setTargetSpeed(GetHangarSpeed()); //HANGAR_SPEED
                myCurrentDist = Norma2Plain(StartPos - rOwner.pFPO.Org);
                mNowDest = StartPos;
                Stage = AutopilotLandingStage.lsFinal;
            }
        }
        return rOwner.ProcessPhysic(scale);
    }

    private float GetHangarSpeed()
    {
        return rOwner.GetVehicleData().MaxSpeed * 0.3f;
    }

    public static float Norma2Plain(Vector3 vect1)
    {
        return Mathf.Pow(vect1.x, 2) + Mathf.Pow(vect1.z, 2);
    }
    bool ProcessWaitForClose(BaseHangar h, float scale)
    {
        return h != null ? !(h.isDoorClosed()) : false;
    }
};