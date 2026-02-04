using System;
using UnityEngine;
using DWORD = System.UInt32;

public class StdCarrierAi : StdHangarAi, IUnitAi
{
    new public const uint ID = 0xA25081A4;
    public const float CM_CARRIER_AWAIT_TIME = 5f;
    iMovementSystemCarrier mpEngine;
    iWeaponSystemTurrets mpTurrets;
    float mCheckFire;
    float mAiming;

    bool mOperationRequested;
    bool mPrevStopped;
    float mOperationTimer;
    float mOpenTimer;
    bool mCheckPause;
    bool mPauseRequested;

    Vector3 mDest;
    float mTime;
    bool mDestChanged;
    DWORD mCurrentFormation;
    float myRequestedSpeed;

    readonly TContact mLeader = new TContact();

    // StdHangerAi owerriders
    public override void handleTerminalsChange() { }

    public void Stopping(GROUP_DATA grp_data)
    {
        mpEngine.Pause(true);
        mOperationRequested = true;
        mOperationTimer = CM_CARRIER_AWAIT_TIME;
        // sending message
        if (!mpEngine.IsStopped())
        {
            mPrevStopped = false;
            RadioMessageInfo rmi = new RadioMessageInfo(5f,0.2f,true,false,true);
            mpEventDesigner.ProcessExternalEvent(AIGroupsEvents.INFO_CARRIERSTOP, 0, grp_data!=null ? grp_data.Callsign : null, rmi, mpData.Number + 1, null, false, EventType.etBase);
        }
    }

    public bool CarrierIdle(StateFlag flag, float scale)
    {
        if (flag != StateFlag.sfRun) return true;
        if (mOperationRequested)
        {    // if someone want to land
            if (mpEngine.IsStopped())
            {    // if engine stopped
                if (!mPrevStopped)
                {          // if just stopped
                    mPrevStopped = true;          // clear just stopped flag
                    mOpenTimer = 3f;           // start open buys timer
                }
                mOpenTimer -= scale;
                if (mOpenTimer < 0f)
                {   // if hangars is free and buys opened
                    if (ProcessQueue(scale) == 0)
                    {     // if no queue
                        ProcessQueue(scale);
                        mOperationTimer -= scale;        // dec operation timer
                        if (mOperationTimer < 0)
                        {        // if operation is complete 
                            mpEngine.Pause(false);         // proceed to route
                            mOperationRequested = false;
                            mCheckPause = true;
                        }
                    }
                }
                else
                    mOperationTimer = CM_CARRIER_AWAIT_TIME;   // else awaiting for operations
            }
            else
                mOperationTimer = CM_CARRIER_AWAIT_TIME;  // else awaiting for operations
        }
        else
        {  // proceed route
            if (mCheckPause)
            {   // if need to check pause
                mCheckPause = false;        // checking pause
                mpEngine.Pause(mPauseRequested);  // setting pause
            }
            if (mDestChanged)
            {      // if need to change dest
                mpEngine.MoveTo(mDest, mTime, Storm.Math.KPH2MPS(myRequestedSpeed));     // changing dest
                mDestChanged = false;                // checking destchanger
            }
        }
        if (mpTurrets!=null)
        {
            mCheckFire -= scale;
            if (mCheckFire < 0)
            {
                mCheckFire = 3f;
                iWeaponSystemDedicated turr = null;
                mAiming = 0;
                while ((turr = mpTurrets.GetNextTurret(turr)) !=null)
                {
                    float t = turr.GetAim();
                    if (t > mAiming)
                        mAiming = t;
                }
            }
        }
        return true;
    }

    public StdCarrierAi()
    {
        mpTurrets = null;
        mpEngine = null;
        mLeader = null;
        mCheckPause = false;
        mPauseRequested = false;
        mCurrentFormation = CRC32.CRC_NULL;
        myRequestedSpeed = 0f;
    }

    // type cast
    public IUnitAi GetIUnitAi() { return this; }

    // api
    ~StdCarrierAi() { }

    // IAi
    public override object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case StdCarrierAi.ID: return this;
            case IUnitAi.ID: return GetIUnitAi();
            default: return base.Query(cls_id);
        }
    }

    // IBaseUnitAi 
    public override void SelectTarget(int nTargets, iContact[] Targets, float[] TargetWeights)
    {
        if (isWeaponsEnabled() == false) return;
        if (mpTurrets!=null)
            mpTurrets.SetTargets(nTargets, Targets, TargetWeights);
    }
    public override  float GetAiming()
    {
        return mAiming;
    }
    public override void SetSkill(DWORD skill, bool already_setted)
    {
        base.SetSkill(skill, already_setted);
        if (mpTurrets!=null)
            mpTurrets.SetAimError(mpSkill.valTurretFireAimError);
    }

    // IUnitAi
    public virtual bool setFormation(iContact _leader, Vector3 delta, float dist, DWORD formation_name)
    {
        if (self.Validate() == false) return false;
        if (_leader!=null)
        {
            mpEngine.NearUnit(_leader, delta);
        }
        //mLeader = new TContact(_leader);
        mLeader.setPtr(_leader);
        return true;
    }
    public virtual bool SetDestination(Vector3 org, float time)
    {
        if (self.Validate() == false) return false;
        mDestChanged = true;
        mDest = org;
        mTime = time;
        return true;
    }

    public virtual Vector3 GetDestination()
    {
        return mDest;
    }
    public virtual bool JoinFormation(JoinOption join)
    {
        return true;
    }
    public virtual void Pause(bool pause)
    {
        if (!mOperationRequested)
        {
            mpEngine.Pause(pause);
            mCheckPause = false;
        }
        else
        {
            mCheckPause = true;
            mPauseRequested = pause;
        }
    }
    public virtual void setSpeed(float spd) { myRequestedSpeed = spd; mDestChanged = true; }


    // IHangarAi
    public override iContact RequestToTakeoff(IGroupAi grp_ai, UNIT_DATA dt, out bool appear)
    {
        appear = false;
        self.Validate();
        if (self.Ptr() !=null)
        {
            if (!mOperationRequested)
                Stopping(grp_ai.GetGroupData());
            if (!mpEngine.IsStopped())
                return null;
            return base.RequestToTakeoff(grp_ai, dt, out appear);
        }
        return null;
    }
    public override void RequestToLand(IGroupAi grp_ai, IBaseUnitAi ai, bool repair)
    {
        self.Validate();
        if (self.Ptr() !=null && !mOperationRequested)
            Stopping(grp_ai.GetGroupData());
        base.RequestToLand(grp_ai, ai, repair);
    }

    // IBaseAi
    public override void SetInterface(IGame igame, iContact contact , UNIT_DATA unit_data, IGroupAi grp_ai)
    {
        base.SetInterface(igame, contact, unit_data, grp_ai);
        mCheckFire = -1f;
        mOperationRequested = false;
        mPrevStopped = true;
        mOperationTimer = -1f;
        mDestChanged = false;
        SetState(CarrierIdle);
    }

    const string CarrierMissed = "create: can't create carrier without iMovementSystemCarrier";
    public override void SideChanged(iContact new_cnt)
    {
        base.SideChanged(new_cnt);
        mpTurrets = (iWeaponSystemTurrets) self.Ptr().GetInterface(iWeaponSystemTurrets.ID);
        mpEngine = (iMovementSystemCarrier) self.Ptr().GetInterface(iMovementSystemCarrier.ID);
        if (mpEngine==null)
            throw new System.Exception(CarrierMissed);
    }

    public override void enumTargets(ITargetEnumer en)
    {
        enumTurretsTargets(mpTurrets, en);
    }

    public override void setFireMode(bool enable)
    {
        base.setFireMode(enable);
        if (enable == false)
        {
            mpTurrets.SetTargets(0, null, null);
        }
    }
};
