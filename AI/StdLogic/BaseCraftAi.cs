using System;
using UnityEngine;
using DWORD = System.UInt32;
using static AICommon;
using static UnitDataDefines;

public abstract class BaseCraftAi : BaseUnitAi, ICraftAi
{
    const int MAX_WEAPON_SLOTS = 3;
    const int WEAPON_SLOT_EMPTY = -1;
    const int WEAPON_SLOT_PRIMARY = 0;
    const int WEAPON_SLOT_SECONDARY = 1;
    const int WEAPON_SLOT_ROCKETS = 2;


    protected const uint CS_MOVE = 0xB0F367B9;
    protected const uint CS_ATTACK = 0xBF93D7F2;
    protected const uint CS_TAKEOFF = 0x4046F4E6;
    protected const uint CS_LANDING = 0xDF780876;

    public static string StateName(uint id)
    {
        switch (id)
        {
            case CS_ATTACK:
                return "CS_ATTACK";
            case CS_MOVE:
                return "CS_MOVE";
            case CS_TAKEOFF:
                return "CS_TAKEOFF";
            case CS_LANDING:
                return "CS_LANDING";
            case Constants.THANDLE_INVALID:
                return "INVALID";
            default:
                return "UNDEFINED";

        }

    }

    // states
    public const uint TARGET_LOST = 0x3E60D008;
    public const uint MISSILE_EVASION = 0xD612A06E;

    public const int ATTACK_TYPE_MEDIUM = 0;
    public const int ATTACK_TYPE_DIR = 1;

    private const string GunsMissed = "create: can't create craft without iWeaponSystemDedicated";
    private const string EngineMissed = "create: can't create craft without iMovementSystemCraft";




    //virtual ~BaseCraftAi();

    // external interfaces
    protected iMovementSystemCraft mpCraft;
    protected iWeaponSystemTurrets mpTurrets;
    public iWeaponSystemDedicated mpGuns;

    public DWORD mAttackType;
    float mAttackAngle;
    public Vector3 mAttackDir;

    public void SetAttackCourse(DWORD mtype, float angle)
    {
        mAttackType = mtype;
        mAttackAngle = angle;
        SetTarget(mTarget.Ptr());
    }

    // state 
    protected CraftAiState mpState;
    protected void SetRecommendedState(DWORD recommended)
    {
        if (mpState != null && mpState.GetStateName() == recommended)
            mpState.ReInitialize();
        else
            UpdateState(recommended);
    }
    public virtual void UpdateState(DWORD recommended) { }


    // type cast
    ICraftAi GetICraftAi() { return this; }

    // states
    bool BaseCraftAi_Move(float scale)
    {
        Asserts.AssertBp(!mpGuns.GetTrigger());
        Vector3 current_dir = self.Ptr().GetDir();
        Vector3 current_speed = new Vector3(0, 0, mLastSpeed);
        if (mpState != null)
        {
            bool clear = false;
            if (MISSILE_EVASION == mpState.GetStateName())
            {
                if (!mpState.Update(scale, ref current_dir, ref current_speed.z, true))
                    clear = true;
                bool battle = false;
                current_dir.Normalize();
                if (GetRealThreat() > mAvoidStart)
                {      // уклонение впри надобности
                    if (ThreatDanger(mLeader.Ptr(), scale, ref current_dir, ref current_speed, false)) current_dir.Normalize();
                    battle = true;
                }
                if (battle)
                    mpCraft.FlyToBattle(current_dir, current_speed);
                else
                    mpCraft.FlyTo(current_dir, current_speed);
            }
            else
                clear = true;
            if (clear)
            {
                ProcessEnvOnNextTick();
                //SafeRelease(mpState);
                mpState = null;
            }
            return true;
        }
        else
        {
            float current_len;
            if (mLeader.Ptr() == null)
            {    // если я лидер
                if (!mPaused)
                {
                    current_dir = mDest - self.Ptr().GetOrg();    // считаем направление
                    if (!CCmp(mRequestedSpeed))       // пересчитываем скорость
                        current_speed.z = Storm.Math.KPH2MPS(mRequestedSpeed);
                    else
                    {
                        mRecalcSpeedTimer -= scale;
                        if (mRecalcSpeedTimer < 0f)
                        {
                            mRecalcSpeedTimer = 3f;
                            Vector3 diff = current_dir;
                            diff.y = 0;
                            current_len = diff.magnitude;
                            float dlt = mTime - stdlogic_dll.mCurrentTime;
                            //float dlt = mTime - BaseScene.SceneTime;//TODO Исправить на нужное значение текущего времени
                            if (dlt > 0)
                                current_speed.z = current_len / dlt;
                            else
                                current_speed.z = Storm.Math.KPH2MPS(1000f);
                        }
                    }
                    current_len = current_dir.magnitude;    // если рядом с точки
                    if (current_len < 75f)
                    {
                        current_dir = self.Ptr().GetDir();  // летим дальше с неизменной скоростью
                        current_dir.y = 0;
                        current_dir.Normalize();
                        current_speed.z = mLastSpeed;
                    }
                    else
                    {            // иначе летим куда надо
                        if (CCmp(current_len))
                            current_dir = Vector3.forward;
                        else
                            current_dir /= current_len;
                    }
                    if (GetRealThreat() > mAvoidStart)       // уклонение впри надобности
                        if (ThreatDanger(mLeader.Ptr(), scale, ref current_dir, ref current_speed, false))
                            current_dir.Normalize();
                    mpCraft.FlyTo(current_dir, current_speed);
                    mLastSpeed = current_speed.z;
                }
            }
            else
            {
                Vector3 delta = mDelta;
                Vector3 delta2 = Vector3.zero;
                if (GetRealThreat() > mAvoidStart)
                    ThreatDanger(mLeader.Ptr(), scale, ref delta, ref delta2, false);
                mpCraft.FollowUnit(mLeader.Ptr(), delta);
            }
            return true;
        }
    }
    bool BaseCraftAi_Landing(float scale)
    {
        if (mLandingOn.Validate() == false)
        {
            SetCurrentState(CS_MOVE);
            return true;
        }
        if (!mLandingNow)
            mLandingNow = mpCraft.Land(mLandingOn.Ptr());
        return true;

    }
    bool BaseCraftAi_Takeoff(float scale)
    {
        if (self.Ptr().GetState() == iSensorsDefines.CS_IN_GAME)
            SetCurrentState(CS_MOVE);
        return true;
    }
    bool BaseCraftAi_Attack(float scale)
    {
        // обновляем цели
        if (UpdateTarget())     // получена новая цель
            TargetUpdated();

        mTarget.Validate();  // померла ли цель ?
        mCheckTimer -= scale;
        if (mCheckTimer <= 0f)
        {     // если пришло время пересчитать параметры
            mCheckTimer = Rnd(mpBaseCraftSkill.valCheckPointMin, mpBaseCraftSkill.valCheckPointBnd); // новое значение таймера

            mFireAim = Rnd(mpBaseCraftSkill.valFireAimMin, mpBaseCraftSkill.valFireAimBnd);
            mFireDistance = Rnd(mpBaseCraftSkill.valFireDistanceMin, mpBaseCraftSkill.valFireDistanceBnd);

            updateAvoids();

            mTargetSearch = Rnd(mpBaseCraftSkill.valTargetSearchMin, mpBaseCraftSkill.valTargetSearchBnd);

            TimerChecked();
        }
        if (mTarget.Ptr()!=null)
        { // цель живая и невредимая
            Vector3 route_diff = self.Ptr().GetDir();
            Vector3 route_speed = new Vector3(0, 0, Storm.Math.KPH2MPS(1000f));
            // считаем текущую позицию цели в зависимости от ее видимости
            bool target_visible_on_radar = true;
            bool target_visible_by_pilot_eyes = true;
            if (mTarget.Ptr().GetAge() > 0f)
            { // если цель не видна на радаре
                target_visible_by_pilot_eyes = mTarget.Ptr().IsOnlyVisual();
                target_visible_on_radar = false;
            }
            else
            /*if (mAngle3D < mpBaseCraftSkill->valAngleVisibilityMin)  // если пилот не видит цель
            target_visible_by_pilot_eyes=false;*/
            if (!target_visible_on_radar && !target_visible_by_pilot_eyes)  // цель на радаре или видна визуально
                SetRecommendedState(TARGET_LOST);
            if (mpState!=null)
            {
                if (!mpState.Update(scale, ref route_diff, ref route_speed.z, target_visible_by_pilot_eyes))
                    SetRecommendedState(mpState.GetNextState());
            }
            float nrm = route_diff.magnitude;
            if (CCmp(nrm))
                route_diff = Vector3.forward;
            else
                route_diff /= nrm;
            if (target_visible_by_pilot_eyes && mDist3D < mFireDistance * mpGuns.GetRange())
            {
                nrm = Mathf.Pow(mDist3D,2) / (Mathf.Pow(mDist3D,2) + Mathf.Pow(mFireAim * mTargetRadius,2));
                if (Mathf.Pow(mAiming,2) > nrm)
                    OpenFire();
                else
                    CeaseFire();
            }
            else
                CeaseFire();
            if (GetRealThreat() > mAvoidStart)
                if (ThreatDanger(null, scale, ref route_diff, ref route_speed, mpGuns.GetTrigger()))
                    route_diff.Normalize();
                    //Norm(route_diff);
            if (mUseBattleMode)
                mpCraft.FlyToBattle(route_diff, route_speed);
            else
                mpCraft.FlyTo(route_diff, route_speed);
        }
        else
            ProcessEnvOnNextTick();
        if (mTarget.Ptr()==null)
            CeaseFire();
        return true;
    }


    // BaseUnitAi  
    public override void OnStateInitialize(DWORD state, bool init)
    {
        if (init)
        {
            switch (state)
            {
                case CS_MOVE:
                    Pause(mPaused);
                    CeaseFire();
                    break;
                case CS_LANDING: mLandingNow = false; break;
            }
        }
    }
    public override bool OnStateRunning(DWORD state, float scale)
    {
        bool ret = true;
        mFireRelaxTimer -= scale;
        mFireBurstTimer -= scale;
        mMissileCheckTimer -= scale;
        if (mMissileCheckTimer < 0f && (mpState==null || mpState!=null && (mpState.GetStateName() != MISSILE_EVASION)) && mJoin != JoinOption.joImmediately)
        {
            iContact thr = self.Ptr().GetThreat();
            if (thr!=null && UnitDataDefines.UT_MISSILE == (uint)thr.GetProtoType())
            {
                if (GetThreat() > mMissileAvoidStart)
                    SetRecommendedState(MISSILE_EVASION);
            }
            mMissileCheckTimer = RandomGenerator.Rnd(mpBaseCraftSkill.valMissileCheckTimeMin, mpBaseCraftSkill.valMissileCheckTimeBnd);
        }

        mpCraft.setMaxEnginePower(mpCraft.GetBatteryLoad() < 0.5f ? 0.8f : 1f);

        switch (state)
        {
            case CS_MOVE: ret = BaseCraftAi_Move(scale); break;
            case CS_TAKEOFF: ret = BaseCraftAi_Takeoff(scale); break;
            case CS_LANDING: ret = BaseCraftAi_Landing(scale); break;
            case CS_ATTACK: ret = BaseCraftAi_Attack(scale); break;
        }
        return ret ? base.OnStateRunning(state, scale) : false;
    }

    // Craft methods
    public virtual void TimerChecked() { }
    public virtual void TargetUpdated() { }
    public virtual bool ThreatDanger(iContact leader, float scale, ref Vector3 route_diff, ref Vector3 route_speed, bool firing)
    {
        route_diff = Vector3.zero;
        route_speed = Vector3.zero;
        return true;
    }
    public virtual void CeaseFire()
    {
        if (true == mpGuns.GetTrigger())
        {  // мы стреляли и цель вышла маневрированем (нашим или ет - неважно)
            mFireRelaxTimer = RandomGenerator.Rnd(mpBaseCraftSkill.valFireRelaxMin, mpBaseCraftSkill.valFireRelaxBnd);
            mpGuns.SetTrigger(false);
        }
    }
    public virtual void OpenFire()
    {
        if (false == mpGuns.GetTrigger())
        {
            if (mFireRelaxTimer > 0f)        // если время выстрела еще не пришло
                return;

            if (IsTalking())
            {
                mpEventDesigner.ProcessExternalEvent(AIGroupsEvents.START_ATTACK, 0, null, new RadioMessageInfo(false, true, true), 0, null, false, EventType.etInternal);
            }

            int i = mpGuns.GetWeapon();
            if (WEAPON_SLOT_ROCKETS == i)
                mFireBurstTimer =RandomGenerator.Rnd(mpBaseCraftSkill.valFireRocketBurstTimeBnd, mpBaseCraftSkill.valFireRocketBurstTimeMin);
            else
                mFireBurstTimer = RandomGenerator.Rnd(mpBaseCraftSkill.valFireCannonBurstTimeBnd, mpBaseCraftSkill.valFireCannonBurstTimeMin);
            mpGuns.SetTrigger(true);
        }
        else
        {    // мы стреляем сейчас
            if (mFireBurstTimer < 0f)
            {        // время залпа кончилось
                CeaseFire();
                return;
            }
        }
    }


    // target 
    protected float mTargetRadius;
    public float mTargetDist;
    protected DWORD mTargetType;
    public readonly TContact mTopTarget = new TContact();
    public Vector3 mSlowTargetsMiddlePos;
    public int mSlowTargetsMiddleCount;
    public int mFastTargetsCount;
    public float mTargetSearch;

    // route properties
    float mRequestedSpeed;
    float mRecalcSpeedTimer;
    float mLastSpeed;

    // takeoffing and landing
    protected readonly TContact mLandingOn = new TContact();
    bool mLandingNow;

    // firing
    float mFireDistance;
    float mFireAim;
    float mFireRelaxTimer;
    float mFireBurstTimer;

    // missile
    float mMissileCheckTimer;

    // avoid
    protected float mAvoidStart;
    float mMissileAvoidStart;
    void updateAvoids()
    {
        mAvoidStart = RandomGenerator.Rnd(mpBaseCraftSkill.valAvoidStartMin, mpBaseCraftSkill.valAvoidStartBnd);
        mMissileAvoidStart = RandomGenerator.Rnd(mpBaseCraftSkill.valMissileAvoidStartMin, mpBaseCraftSkill.valMissileAvoidStartBnd);
    }

    bool mUseBattleMode;
    public void UseBattleMode(bool mode) { mUseBattleMode = mode; }

    // timer
    float mCheckTimer;
    protected void CheckPointOnNextTick() { mCheckTimer = -1f; }

    // target info
    public float mAngle3D;
    public float mEnemyAngle3D;
    float mDist3D;
    public Vector3 mTargetDiff;
    public Vector3 mTargetAim;
    public float GetDist2D()
    {
        Vector3 df = mTargetDiff * mDist3D;
        df.y = 0;
        return df.magnitude;
    }
    float GetAngle2D()
    {
        Vector3 df = mTargetDiff * mDist3D;
        df.y = 0;
        float t = df.magnitude;
        if (CCmp(t))
            return -1f;
        else
            return (Vector3.Dot(df,self.Ptr().GetDir())) / t;
    }

    // targets processing 
    bool UpdateTarget()
    {
        bool ret = false;
        iContact new_target = null;
        new_target = mpGuns.GetTarget();
        if (mTarget.Ptr() != new_target)
        {
            mTarget.Validate();
            if (mTarget.Ptr()!=null && mTarget.Ptr().GetAge() > 0f && mTarget.Ptr().GetAge() < mTargetSearch)
            { // цель вышла маневрированием
                if (new_target!=null)
                {   // если цель есть 
                    Vector3 self_org = self.Ptr().GetOrg();
                    if ((self_org - mTarget.Ptr().GetOrg()).sqrMagnitude < (new_target.GetOrg() - self_org).sqrMagnitude) // если старая цель ближе чем новая, охотимся за старой
                        new_target = mTarget.Ptr();
                }
                else
                    new_target = mTarget.Ptr();  // если новая цель пустая то продолжаем охотиться за старой
            }
            if (new_target != mTarget.Ptr())
            {
                CeaseFire();
                SetTarget(new_target);
                ret = true;
            }
        }
        if (mTarget.Ptr()==null)
        {
            mAiming = 0f;
        }
        else
        {
            mAiming = mpGuns.GetAim();
            mTargetDiff = mTarget.Ptr().GetOrg() - self.Ptr().GetOrg();
            mDist3D = mTargetDiff.magnitude;
            if (CCmp(mDist3D))
            {
                mTargetDiff = Vector3.forward;
                mAngle3D = -1f;
                mEnemyAngle3D = -1f;
            }
            else
            {
                mTargetDiff /= mDist3D;
                mAngle3D = Vector3.Dot(self.Ptr().GetDir(), mTargetDiff);
                mEnemyAngle3D = Vector3.Dot(-mTargetDiff,mTarget.Ptr().GetDir());
            }
            mTargetAim = mpGuns.GetTargetOrg() - self.Ptr().GetOrg();
            mTargetDist = mpGuns.GetTargetDist();
        }
        if (ret || mTarget.Ptr() == null)
        {
            SetRecommendedState(Constants.THANDLE_INVALID);
            ProcessEnvOnNextTick();
        }
        return ret;
    }
    public void ForgetTarget()
    {
        CeaseFire();
        mpGuns.SetTarget(null);
        ProcessEnvOnNextTick();
    }

    void SetTarget(iContact tgt)
    {
        mTarget.setPtr(tgt);
        mTopTarget.setPtr(mTarget.Ptr()!=null ? mTarget.Ptr().GetTopContact() : null);
        if (mTopTarget.Ptr() == mTarget.Ptr())
            mTopTarget.setPtr(null);
        mTargetRadius = mTarget.Ptr()!=null ? mTarget.Ptr().GetRadius() : 0f;
        mTargetType = mTarget.Ptr()!=null ? (uint) mTarget.Ptr().GetProtoType() : 0;
        if (mTarget.Ptr()!=null && mAttackType == ATTACK_TYPE_DIR)
        {
            mAttackDir = mTarget.Ptr().GetDir();
            float cos_angle = Mathf.Cos(Storm.Math.GRD2RD(mAttackAngle));
            float sin_angle = Mathf.Sqrt(1f - Mathf.Pow(cos_angle,2));
            mAttackDir.y = 0f;
            float d = mAttackDir.magnitude;
            mAttackDir = (CCmp(d)) ? Vector3.forward : mAttackDir / d;
            rotatebyangle(ref mAttackDir, cos_angle, sin_angle);
        }
        CheckPointOnNextTick();
    }
    protected void SelectBestWeapon()
    {
        if (mTarget.Ptr() == null) return;
        int i, best = -1;
        float f, bestf = .0f;
        for (i = 0; i <= 2; i++)
        {
            mpGuns.SetWeapon(i);
            f = mpGuns.GetEfficiency(mTarget.Ptr(), mDist3D);
            if (bestf < f)
            {
                best = i;
                bestf = f;
            }
        }
        if (best >= 0)
            mpGuns.SetWeapon(best);
    }

    // skill
    public BaseCraftSkill mpBaseCraftSkill;

    // -------------------------------------------------
    // API
    public BaseCraftAi()
    {
        mpCraft = null;
        mpTurrets = null;
        mpGuns = null;
        mpBaseCraftSkill = null;
        //mTopTarget = new TContact((iContact)null);
        mTopTarget.setPtr(null);
        mpState = null;
        mAttackType = ATTACK_TYPE_MEDIUM;
        mAttackAngle = 0f;
        myCraftUnit = new CraftUnit<BaseCraftAi>(this);
    }
    ~BaseCraftAi()
    {
        //SafeRelease(mpState);
        mpState = null;
    }

    // IAi
    public override void ProcessRadioMessage(string msg_code, IAi caller, RadioMessage Info, bool to_all, bool say_flag)
    {
        base.ProcessRadioMessage(msg_code, caller, Info, to_all, say_flag);
        switch (Info.Code)
        {
            case AiRadioMessages.RM_NOTIFY_LAND_CLEARED:
                {
                    Debug.Log(string.Format("{0} landing on {1}", null, Info.TargetContact));
                    if (Info.TargetAi == GetIBaseUnitAi())
                    {
                        //mLandingOn = new TContact(Info.TargetContact);
                        mLandingOn.setPtr(Info.TargetContact);
                        SetCurrentState(CS_LANDING);
                    }
                }
                break;
        }
    }
    public override object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case ICraftAi.ID: return GetICraftAi();
            case ICraftUnit.ID: return myCraftUnit;
            default: return base.Query(cls_id);
        }
    }

    // IUnitAI
    public override void SelectTarget(int nTargets, iContact[] Targets, float[] TargetWeights)
    {
        if (isWeaponsEnabled() == false) return;
        if (nTargets!=0)
        {
            mFastTargetsCount = 0;
            mSlowTargetsMiddlePos = Vector3.zero;
            mSlowTargetsMiddleCount = 0;
            Vector3 self_org = self.Ptr().GetOrg();
            for (int i = 0; i < nTargets; ++i)
            {
                if (Targets[i] != mTarget)
                {
                    int tp = Targets[i].GetProtoType();
                    if (tp == UT_FLYING_AGILE || tp == UT_FLYING_FAST)
                    {
                        if ((Targets[i].GetOrg() - self_org).sqrMagnitude < 1000f * 1000f)
                            mFastTargetsCount++;
                    }
                    else
                    {
                        mSlowTargetsMiddleCount++;
                        mSlowTargetsMiddlePos += Targets[i].GetOrg();
                    }
                }
            }
            if (mSlowTargetsMiddleCount > 1)
                mSlowTargetsMiddlePos /= (float)(mSlowTargetsMiddleCount);
        }
        if (mpTurrets!=null)
            mpTurrets.SetTargets(nTargets, Targets, TargetWeights);
        mpGuns.SetTarget(mpGuns.SelectTarget(nTargets, Targets, TargetWeights, 10f));
        if (UpdateTarget())
            TargetUpdated();
    }
    public override bool SetDestination(Vector3 org, float time)
    {
        mLeader.setPtr(null);
        Asserts.AssertBp(org.magnitude > 0.5f);
        if (!base.SetDestination(org, time)) return false;
        mDest.y = igame.GetGroundLevel(org) + org.y;
        return true;
    }
    public override void Pause(bool pause)
    {
        base.Pause(pause);
        if (pause && CS_ATTACK != mCurrentState && CS_TAKEOFF != mCurrentState && CS_LANDING != mCurrentState)
            mpCraft.Pause();
    }
    public override void setSpeed(float spd)
    {
        mRequestedSpeed = spd;
    }

    // IBaseUnitAi
    public override void SetSkill(DWORD skill, bool already_setted)
    {
        if (!already_setted)
        {
            if (mpSkill!=null) { mpSkill = null; } //TODO Возожном, потребуется корректная "очистка" переменной
            iUnifiedVariableContainer pskill = Skills.GetSkillContainer(skill);
            mpSkill = new BaseCraftSkill(pskill);
            already_setted = true;
        }
        mpBaseCraftSkill = (BaseCraftSkill) mpSkill.Query(BaseCraftSkill.ID);
        Asserts.AiAssertEx(mpBaseCraftSkill!=null, "can't find BaseCraftSkill");
        base.SetSkill(skill, already_setted);
        mpGuns.SetAimError(mpBaseCraftSkill.valFireAimError);
        if (mpTurrets!=null)
            mpTurrets.SetAimError(mpBaseCraftSkill.valTurretFireAimError);
        if (mpCraft!=null)
            mpCraft.SetControlScale(mpBaseCraftSkill.valControlScale);

        updateAvoids();
    }
    public override void Suicide()
    {
        base.Suicide();
        mpCraft.Manual();
    }

    // ICraftAi
    public virtual void SetRouteProperties(float min_y, float pred_time)
    {
        mpCraft.SetMinAltitude(min_y);
        mpCraft.SetPredictionTime(pred_time);
    }

    // BaseAi
    public override  void SetInterface(IGame igame, iContact contact, UNIT_DATA unit_data, IGroupAi grp_ai)
    {
        base.SetInterface(igame, contact, unit_data, grp_ai);

        // route properties
        mRequestedSpeed = 0f;
        mRecalcSpeedTimer = 0f;
        mLastSpeed = 0f;

        // fire
        mFireRelaxTimer = -1f;
        mFireBurstTimer = -1f;

        // missile
        mMissileCheckTimer = -1f;

        // target
        mSlowTargetsMiddleCount = 0;
        mFastTargetsCount = 0;

        UseBattleMode(true);

        // land/takeoff
        //mLandingOn = null;
        mLandingOn.setPtr(null);
        if (self.Ptr().GetState() == iSensorsDefines.CS_IN_GAME)
            SetCurrentState(CS_MOVE);
        else
            SetCurrentState(CS_TAKEOFF);

        myStdCraftAiFactory = Factories.createStdCraftAiFactory(getIQuery(), myBaseAiFactory);
    }
    public override void SideChanged(iContact new_cnt)
    {
        //Debug.Log("BaseCraftaI SIDECHANGING");
        //base.SideChanged(new_cnt);

        base.SideChanged(new_cnt);
        mpCraft = (iMovementSystemCraft)self.Ptr().GetInterface(iMovementSystemCraft.ID);
        if (mpCraft == null)
            throw new System.Exception(EngineMissed);
        //mpTurrets = (iWeaponSystemTurrets.ID) self.GetInterface(iWeaponSystemTurrets.ID);
        mpGuns = (iWeaponSystemDedicated) self.Ptr().GetInterface(iWeaponSystemDedicated.ID);
        if (mpGuns == null)
            throw new Exception(GunsMissed);
    }


    public override void setFireMode(bool enable)
    {
        base.setFireMode(enable);
        if (enable == false)
        {
            SetTarget(null);
            mpGuns.SetTarget(null);
            mpTurrets.SetTargets(0, null, null);
        }
    }

    CraftUnit<BaseCraftAi> myCraftUnit;

    IVmFactory myStdCraftAiFactory;
    public override IVmFactory getTopFactory() { return myStdCraftAiFactory; }
    public override void enumTargets(ITargetEnumer en)
    {
        enumTurretsTargets(mpTurrets, en);
        base.enumTargets(en);
    }
}


public interface CraftAiState : IObject
{ 
    public void ReInitialize();
    public DWORD GetStateName();
    public DWORD GetNextState();
    public bool Update(float scale, ref Vector3 diff, ref float speed, bool visible_by_eyes);
};

enum TargetPosition
{
    tptAlmostAhead,
    tptAhead,
    tptMiddle,
    tptBack
};

enum PositionType
{
    cpAlmostAheadAttack,
    cpAheadAttack,
    cpMiddleAttack,
    cpBackAttack,
    cpMiddleEngage,
    cpBackEngage,
    cpAheadApproach,
    cpMiddleApproach,
    cpBackApproach
};

public interface ICraftUnit
{
    public const uint ID = 0xD54383A4;
    public void setAttackCourse(float angle, int type);
};

public struct CraftUnit<T> : ICraftUnit where T : BaseCraftAi
{
    public void setAttackCourse(float angle, int type)
    {
        //myMsn.SetAttackCourse(angle, type);
        myMsn.SetAttackCourse((uint)type, angle);
    }

    public CraftUnit(T imp)
    {
        myMsn = imp;
    }

    public T myMsn;
};