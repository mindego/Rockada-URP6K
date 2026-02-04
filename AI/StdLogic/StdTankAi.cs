using UnityEngine;
using DWORD = System.UInt32;
using static AICommon;

enum VehicleFightMode
{
    tfmForceAssault,
    tfmSiege,
    tfmAssault,
    tfmCamp
};

public class StdTankAi : StandartUnitAi, IVehicleAi
{
    new public const uint ID = 0xA0CB8AEA;
    public const float SIEGE_FACTOR = 1.2f;
    const string MSystemMissed = "create: can't create vehicle without iMovementSystemVehicle";
    iMovementSystemVehicle vehicle;
    iWeaponSystemDedicated main_turret;
    iWeaponSystemTurrets mpTurrets;

    float main_turret_condition;
    bool dest_changed, leader_changed;
    VehicleFightMode fmode;

    bool mUseRoads;
    int mRoadMode;
    bool mPaused;

    VehicleFightMode GetFightMode() { return fmode; }
    void UpdateUseRoads() { mUseRoads = mRoadMode != 0; }
    void ProcessTarget(float scale)
    {
        if (main_turret != null)
        {
            //mTarget = new TContact(main_turret.GetTarget());
            mTarget.setPtr(main_turret.GetTarget());
            aiming = main_turret.GetAim();
            main_turret_condition = main_turret.GetCondition();
        }
        else
        {
            //mTarget = null;
            mTarget.setPtr(null);
            aiming = 0f;
            main_turret_condition = 0f;
        }

    }

    void ReturnToRoute() { leader_changed = true; }

    bool Move(StateFlag flag, float scale)
    {
        switch (flag)
        {
            case StateFlag.sfInit:
                vehicle.ClearRoute();
                vehicle.Pause(mPaused);
                UpdateUseRoads();
                dest_changed = true;
                return true;
            case StateFlag.sfDone:
                return true;
        }
        ProcessTarget(scale);
        tactics_timer -= scale;
        if (tactics_timer < 0)
            ProcessTactics(scale);
        ProcessMoveDest(scale);
        return true;
    }

    void ProcessMoveDest(float scale)
    {
        // we are leader
        if (leader.Ptr() == null)
        {
            if (dest_changed)
            {      // set new dest
                vehicle.MoveTo(dest, mUseRoads, time);
                dest_changed = false;
            }
        }
        else
        {
            if (leader_changed)
            {
                if (mCurrentFormation == FormationDefs.iFormationColoumn)
                    vehicle.FollowUnit(leader.Ptr(), formation_dist);
                else
                    vehicle.NearUnit(leader.Ptr(), formation_dist, delta);
                leader_changed = false;
            }
        }

    }

    bool TakeOffing(StateFlag flag, float scale)
    {
        switch (flag)
        {
            case StateFlag.sfInit:
                return true;
            case StateFlag.sfDone:
                return true;
        }
        if (self.Ptr().GetState() == iSensorsDefines.CS_IN_GAME)
            SetState(Move);
        return true;

    }
    bool BattleRoam(StateFlag flag, float scale)
    {
        switch (flag)
        {
            case StateFlag.sfInit:
                resume();
                vehicle.ClearRoute();
                mUseRoads = mRoadMode == 2;
                return true;
            case StateFlag.sfDone:
                return true;
        }
        if (vehicle.IsPaused())
            resume();
        ProcessTarget(scale);
        ProcessBattleRoamDest(scale);
        tactics_timer -= scale;
        if (tactics_timer < 0)
            ProcessTactics(scale);
        return true;

    }
    bool Landing(StateFlag flag, float scale)
    {
        switch (flag)
        {
            case StateFlag.sfInit:
                resume();
                landing = false;
                return true;
            case StateFlag.sfDone:
                return true;
        }
        if (mLandingOn.Validate() == false)
        {
            SetState(Move);
            return true;
        }
        if (!landing)
            landing = vehicle.Land(mLandingOn.Ptr());
        return true;

    }
    void ProcessBattleRoamDest(float scale)
    {
        if (vehicle.GetReachedFlag() && mTarget.Ptr() != null && main_turret != null)
        {  // arrived
            Vector3 tgt_org = main_turret.GetTargetOrg();
            Vector3 self_org = self.Ptr().GetOrg();
            Vector3 diff;
            diff = tgt_org - self_org;
            diff.y = 0;
            float d = diff.magnitude;
            if (CCmp(d))
                diff = Vector3.forward;
            else
                diff /= d;
            float len = 0;
            float radius;
            d -= mTarget.Ptr().GetRadius();
            float range = main_turret.GetRange();
            if (d > range)
            {
                float mul = RandomGenerator.Rand01() * 0.3f;
                mul += (GetFightMode() == VehicleFightMode.tfmAssault) ? 0.6f : 0.2f;
                len = d - range + range * mul;
                radius = 0;
            }
            else
            {
                len = RandomGenerator.Rand01() * d * 0.5f;
                radius = range * 0.1f;
            }
            Vector3 right = Vector3.zero;
            right.Set(diff.z, 0, -diff.x);
            //diff = self_org + diff * len + Distr.Sphere() * radius;
            diff = self_org + diff * len;//TODO! Исправить на правильное положение
            vehicle.MoveTo(diff, mUseRoads, 0);
        }

    }

    bool BattleSiege(StateFlag flag, float scale)
    {
        switch (flag)
        {
            case StateFlag.sfInit:
                vehicle.ClearRoute();
                pause();
                return true;
            case StateFlag.sfDone:
                vehicle.Pause(mPaused);
                return true;
        }
        ProcessTarget(scale);
        tactics_timer -= scale;
        if (tactics_timer < 0)
            ProcessTactics(scale);
        return true;

    }

    void ProcessTactics(float scale)
    {
        tactics_timer = RandomGenerator.Rand01() * 2f; // skill->TacticsTimer
        if (iState == Move)
        {
            if (GetFightMode() == VehicleFightMode.tfmSiege)
                SetState(BattleSiege);
            else
            if (mTarget.Ptr() != null && join == JoinOption.joFreeFly && main_turret != null)
                SetState(BattleRoam);
        }
        else
        {
            if (iState == BattleSiege || iState == BattleRoam)
            {
                if ((mTarget.Ptr() == null || join == JoinOption.joImmediately || main_turret_condition == 0f) && GetFightMode() != VehicleFightMode.tfmSiege)
                {
                    SetState(Move);
                    ReturnToRoute();
                    return;
                }
                if (mTarget.Ptr() != null && main_turret != null)
                {
                    float threat_factor = GetRealThreat();
                    if (iState == BattleRoam)
                    {
                        bool can_siege = false;
                        if (threat_factor < SIEGE_FACTOR && main_turret.GetAim() > 0.99f)
                            can_siege = true;
                        if (can_siege)
                            SetState(BattleSiege);
                    }
                    else if ((iState = BattleSiege) != null)
                    {
                        if ((CCmp(main_turret.GetAim()) || threat_factor > SIEGE_FACTOR) && GetFightMode() != VehicleFightMode.tfmSiege)      // target blocked
                            SetState(BattleRoam);
                    }
                }
            }
        }

    }

    // land
    readonly TContact mLandingOn = new TContact();
    bool landing;
    void Land(iContact p)
    {
        mLandingOn.setPtr(p);
        SetState(Landing);
    }


    IVehicleAi GetIVehicleAi() { return this; }

    public StdTankAi()
    {
        main_turret = null;
        mPaused = false;
        mpTurrets = null;
    }

    public virtual void pause()
    {
        vehicle.Pause(true);
    }
    public virtual void resume()
    {
        dest_changed = true;
        vehicle.Pause(false);
    }

    // api
    // IAi
    public override object Query(uint cls_id)
    {
        switch ((uint)cls_id)
        {
            case StdTankAi.ID: return this;
            case IVehicleAi.ID: return GetIVehicleAi();
            default: return base.Query(cls_id);
        }
    }

    public override void ProcessRadioMessage(string msg_code, IAi caller, RadioMessage Info, bool to_all, bool say_flag)
    {
        base.ProcessRadioMessage(msg_code, caller, Info, to_all, say_flag);
        switch (Info.Code)
        {
            case AiRadioMessages.RM_NOTIFY_LAND_CLEARED: if (Info.TargetAi == GetIBaseUnitAi()) Land(Info.TargetContact); break;
        }

    }
    // IBaseUnitAi
    public override void SetSkill(DWORD skill, bool already_setted)
    {
        base.SetSkill(skill, already_setted);
        if (main_turret != null)
            main_turret.SetAimError(mpSkill.valTurretFireAimError);

    }
    public override void Suicide()
    {
        base.Suicide();
        vehicle.ClearRoute();
        pause();
    }

    //IUnitAi
    public override bool setFormation(iContact _leader, Vector3 _delta, float dist, DWORD formation_name)
    {
        iContact old_leader = leader.Ptr() != null ? leader.Ptr() : null;
        DWORD old_form = mCurrentFormation;
        base.setFormation(_leader, _delta, dist, formation_name);
        if (old_leader != leader || old_form != formation_name)
        {     // leader changed
            if (leader.Ptr() == null)
                dest_changed = true;
            else
                leader_changed = true;
        }
        return true;

    }
    public override bool SetDestination(Vector3 org, float _time)
    {
        dest_changed = true;
        leader.setPtr(null);
        return base.SetDestination(org, _time);

    }
    public override void SelectTarget(int nTargets, iContact[] Targets, float[] TargetWeights)
    {
        if (isWeaponsEnabled() == false) return;
        if (main_turret != null)
            main_turret.SetTarget(main_turret.SelectTarget(nTargets, Targets, TargetWeights));
        if (mpTurrets != null)
            mpTurrets.SetTargets(nTargets, Targets, TargetWeights);

    }
    public override void Pause(bool pause)
    {
        if (mPaused != pause)
        {
            mPaused = pause;
            if (iState == Move)
                vehicle.Pause(pause);
        }

    }

    public override void setSpeed(float spd)
    {
        vehicle.setMaxSpeed(Storm.Math.KPH2MPS(spd));
    }

    // IBaseAi
    public override void SetInterface(IGame igame, iContact contact, UNIT_DATA unit_data, IGroupAi grp_ai)
    {
        //StandartUnitAi::SetInterface(igame, contact, unit_data, grp_ai);
        base.SetInterface(igame, contact, unit_data, grp_ai);
        mLandingOn.setPtr(null);
        dest_changed = false;
        leader_changed = false;
        dest = self.Ptr().GetOrg();
        SetFightMode((uint)VehicleFightMode.tfmAssault);
        mRoadMode = 1;
        UpdateUseRoads();
        tactics_timer = RandomGenerator.Rand01() * 4f; // skill->TacticsTimer
        main_turret_condition = 1f;

        if (self.Ptr().GetState() == iSensorsDefines.CS_IN_GAME)
            SetState(Move);
        else
            SetState(TakeOffing);
    }

    public override void SideChanged(iContact new_cnt)
    {
        base.SideChanged(new_cnt);
        main_turret = (iWeaponSystemDedicated)self.Ptr().GetInterface(iWeaponSystemDedicated.ID);
        vehicle = (iMovementSystemVehicle)self.Ptr().GetInterface(iMovementSystemVehicle.ID);
        if (vehicle == null)
            throw new System.Exception(MSystemMissed);
        mpTurrets = (iWeaponSystemTurrets)self.Ptr().GetInterface(iWeaponSystemTurrets.ID);
    }


    // IVehicleAi
    public virtual bool SetFightMode(DWORD mode)
    {
        switch (mode)
        {
            case 3: fmode = VehicleFightMode.tfmCamp; break;
            case 2: fmode = VehicleFightMode.tfmForceAssault; break;
            case 0: fmode = VehicleFightMode.tfmSiege; break;
            case 1: fmode = VehicleFightMode.tfmAssault; break;
        }
        return true;

    }
    public virtual bool SetUseRoadsMode(DWORD mode)
    {
        mRoadMode = (int)mode;
        UpdateUseRoads();
        if (leader.Ptr() == null)
            dest_changed = true;
        return true;

    }

    public override void setFireMode(bool enable)
    {
        base.setFireMode(enable);
        if (enable == false)
        {
            setTarget(null);
            main_turret.SetTarget(null);
            mpTurrets.SetTargets(0, null, null);
        }

    }

};


