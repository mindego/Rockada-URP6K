//StandartUnitAi было
using UnityEngine;
using UnityEngine.Assertions;
using DWORD = System.UInt32;
using static Storm.Math;
using static UnitDataDefines;
using static AICommon;
using static JoinOption;

public class SingleBFAi : BaseCraftAi
{
    public const float BF_SEARCH_RADIUS = 150f;

    public const int TCT_AWAY = 0;     // отходим от цели на заданную высоту
    public const int TCT_APPROACH = 1;    // подлетаем к цели на заданной высоте
    public const int TCT_ATTACK = 2;     // атакуем цель

    public const uint GROUND_AWAY = 0x4137AB43;
    public const uint GROUND_ATTACK = 0x92588CFA;
    public const uint GROUND_APPROACH = 0xA6D13039;
    public const uint AIR_TURN = 0x628D0446;
    public const uint AIR_ATTACK = 0xDB5868B0;
    public const uint AIR_AWAY = 0xE4F2B3D0;
    public const uint AIR_RELAX = 0xB41480ED;


    new public const uint ID = 0x3989FF5A;
    // targets
    float mStrafeTimer;
    Vector3 mStrafeVector;
    Vector3 mLeaderStrafeVector;
    float mThreatDelta;

    public bool TargetCanBeAttacked(float dist, float cos_angle, int pred_time)
    {
        Assert.IsNotNull(mTarget.Ptr());
        mTarget.Ptr().StartPrediction();
        self.Ptr().StartPrediction();

        Vector3 target_diff;
        float dist3d;
        float angle3d;
        bool ret = false;
        Vector3 target_speed = new Vector3(0, 0, Storm.Math.KPH2MPS(1000f));

        for (int i = 0; i < pred_time * 4; ++i)
        {
            target_diff = mTarget.Ptr().GetOrg() - self.Ptr().GetOrg();
            dist3d = target_diff.magnitude;
            if (CCmp(dist3d))
            {
                target_diff = Vector3.forward;
                angle3d = -1f;
            }
            else
            {
                target_diff /= dist3d;
                angle3d = Vector3.Dot(self.Ptr().GetDir() , target_diff);
            }
            if (angle3d > cos_angle && dist3d > dist)
            {
                ret = true;
                break;
            }
            mpCraft.FlyToBattle(target_diff, target_speed);
            mpCraft.MakePrediction(0.25f);
            mTarget.Ptr().MakePrediction(0.25f);
        }
        mTarget.Ptr().EndPrediction();
        self.Ptr().EndPrediction();
        return ret;
    }
    public bool TargetWillBeCrashed(int pred_time)
    {
        Assert.IsNotNull(mTarget.Ptr());
        mTarget.Ptr().StartPrediction();
        self.Ptr().StartPrediction();

        Vector3 target_diff;
        float dist3d;
        float angle3d;
        bool ret = false;
        float radius = self.Ptr().GetRadius() + mTarget.Ptr().GetRadius();
        Vector3 target_speed = new Vector3(0, 0, Storm.Math.KPH2MPS(1000f));

        for (int i = 0; i < pred_time * 4; ++i)
        {
            target_diff = mTarget.Ptr().GetOrg() - self.Ptr().GetOrg();
            dist3d = target_diff.magnitude;
            if (CCmp(dist3d))
            {
                target_diff = Vector3.forward;
                angle3d = -1f;
            }
            else
            {
                target_diff /= dist3d;
                angle3d = Vector3.Dot(self.Ptr().GetDir() , target_diff);
            }
            if (dist3d < radius)
            {
                ret = true;
                break;
            }
            mpCraft.FlyToBattle(target_diff, target_speed);
            mpCraft.MakePrediction(0.25f);
            mTarget.Ptr().MakePrediction(0.25f);
        }
        mTarget.Ptr().EndPrediction();
        self.Ptr().EndPrediction();
        return ret;
    }
    public override void TimerChecked()
    {
        if (mTarget.Ptr()!=null)   // если есть цель то пересчитываем параметры
            SelectBestWeapon();
    }
    public override void TargetUpdated()
    {
        // выбираем лучшее оружие
        SelectBestWeapon();
    }

    const float MAX_WPN_RANGE = 5000f;
    public override bool ThreatDanger(iContact leader, float scale, ref Vector3 route_diff, ref Vector3 route_speed, bool firing)
    {
        if (UT_FLYING_FAST == self.Ptr().GetProtoType())
        {
            if (firing) return false;
            if (leader!=null)
            { // счмтаем место где мы по идее находимся 
                float t1 = mpBFSkill.valThreatLeaderAmplitudeMin * Mathf.Cos(GRD2RD(stdlogic_dll.mCurrentTime * mpBFSkill.valThreatLeaderSpeed));
                float t2 = mpBFSkill.valThreatLeaderAmplitudeMin * Mathf.Cos(GRD2RD((stdlogic_dll.mCurrentTime + mThreatDelta) * mpBFSkill.valThreatLeaderSpeed));
                route_diff.x += t1;
                route_diff.y += t2;
                return true;
            }
            else
            {
                Vector3 org = self.Ptr().GetOrg() + route_diff * mpBFSkill.valThreatPeriodMin;
                float t1 = mpBFSkill.valThreatAmplitudeMin * Mathf.Cos(GRD2RD(mpBFSkill.valThreatPeriodMin * stdlogic_dll.mCurrentTime * mpBFSkill.valThreatSpeed));
                float t2 = mpBFSkill.valThreatAmplitudeMin * Mathf.Cos(GRD2RD(mpBFSkill.valThreatPeriodMin * (stdlogic_dll.mCurrentTime + mThreatDelta) * mpBFSkill.valThreatSpeed));
                org += self.Ptr().GetRight() * t1 + self.Ptr().GetUp() * t2;
                route_diff = org - self.Ptr().GetOrg();
                return true;
            }
        }
        else
        {
            if (leader!=null && mCurrentState == CS_MOVE)
            {
                route_diff += mLeaderStrafeVector;
            }
            else
            {
                route_speed.x += mStrafeVector.x;
                route_speed.y += mStrafeVector.y;
            }
            return false;
        }
    }
    public override void UpdateState(DWORD recommended)
    {
        IBase.SafeRelease(mpState);
        mpState = null;
        if (recommended == IVm.THANDLE_INVALID)
        {
            if (mTarget.Ptr() !=null)
            {
                if (UT_FLYING == mTargetType || UT_FLYING_AGILE == mTargetType || UT_FLYING_FAST == mTargetType || UT_MISSILE == mTargetType)
                {   // выбираем из воздушных целей
                    if (TargetCanBeAttacked((mType == UT_FLYING_FAST) ? 900f : 600f, 0.87f, 15))  // cos(30.f)
                        recommended = AIR_TURN;
                    else
                        recommended = AIR_AWAY;
                }
                else
                {
                    float mDist2D = GetDist2D() - mTargetRadius;
                    float range = mpGuns.GetRange();
                    if (range > MAX_WPN_RANGE)
                        range = MAX_WPN_RANGE;
                    if (mDist2D < 300f)
                    {
                        recommended = GROUND_AWAY;                // то отходим
                    }
                    else if (mDist2D < range + 200f)
                    {    // если пора стрелять
                        if (mAngle3D > 0)
                        {
                            if (GetRealThreat() < mAvoidStart)   // и мы смотрим куда надо
                                recommended = GROUND_ATTACK;        // то атакуем 
                            else
                                recommended = GROUND_AWAY;          // иначе отходим 
                        }
                        else
                        {
                            recommended = GROUND_AWAY;          // иначе отходим 
                        }
                    }
                    else
                    {
                        recommended = GROUND_APPROACH;        // то атакуем 
                    }
                }
            }
        }
        switch (recommended)
        {
            case GROUND_AWAY:
                {
                    BFGroundAway state = new BFGroundAway();
                    state.Initialize(this);
                    mpState = state;
                }
                break;
            case GROUND_ATTACK:
                {
                    BFGroundAttack state = new BFGroundAttack();
                    state.Initialize(this);
                    mpState = state;
                }
                break;
            case GROUND_APPROACH:
                {
                    BFGroundApproach state = new BFGroundApproach();
                    state.Initialize(this);
                    mpState = state;
                }
                break;
            case TARGET_LOST:
                {
                    if (UT_ON_SURFACE == mTargetType)
                        ForgetTarget();
                    else
                    {
                        BFAirLost state = new BFAirLost();
                        state.Initialize(this);
                        mpState = state;
                    }
                }
                break;
            case AIR_TURN:
                {
                    BFAirTurn state = new BFAirTurn();
                    state.Initialize(this);
                    mpState = state;
                }
                break;
            case AIR_AWAY:
                {
                    BFAirAway state = new BFAirAway();
                    state.Initialize(this);
                    mpState = state;
                }
                break;
            case AIR_ATTACK:
                {
                    BFAirAttack state = new BFAirAttack();
                    state.Initialize(this);
                    mpState = state;
                }
                break;
            case MISSILE_EVASION:
                {
                    BFAirMissileEvasion state = new BFAirMissileEvasion();
                    state.Initialize(this);
                    mpState = state;
                }
                break;
        }
    }
    // skill
    public BFSkill mpBFSkill;

    // default constructor
    public SingleBFAi() { mpBFSkill = null; }
    ~SingleBFAi() { }


    // BaseUnitAi  
    public override void OnStateInitialize(DWORD state, bool init)
    {
        base.OnStateInitialize(state, init);
        if (init)
        {
            switch (state)
            {
                case CS_ATTACK:
                    SetRecommendedState(IVm.THANDLE_INVALID);
                    CheckPointOnNextTick();
                    break;
            }
        }
    }
    public override bool OnStateRunning(DWORD state, float scale)
    {
        mStrafeTimer -= scale;
        if (mStrafeTimer < 0f)
        {
            mStrafeTimer = Rnd(mpBFSkill.valTargetStrafeTimeMin, mpBFSkill.valTargetStrafeTimeBnd);
            float intensity = Rnd(mpBFSkill.valTargetStrafeIntensityMin, mpBFSkill.valTargetStrafeIntensityBnd);
            float ret1, ret2;
            do
            {
                ret1 = Prb(0.5f) ? -intensity : intensity;
                ret2 = Prb(0.5f) ? -intensity : intensity;
            } while (!CCmp(intensity) && FCmp(mStrafeVector.x, ret1) && FCmp(mStrafeVector.y, ret2));
            mStrafeVector.x = ret1;
            mStrafeVector.y = ret2;
            mLeaderStrafeVector = Distr.Sphere() * mpBFSkill.valTargetLeaderDelta;
        }
        return base.OnStateRunning(state, scale);
    }
    public override void ProcessEnvironment()
    {
        base.ProcessEnvironment();
        //Debug.Log(string.Format("{0} {1} {2} state {3}",mpGroup.GetGroupData().Callsign, mpData.Number+1,mTarget, mCurrentState.ToString("X8")));
        switch (mCurrentState)
        {
            case CS_MOVE:
                {
                    bool set_attack = false;
                    if (mTarget.Ptr()!=null)
                    {
                        if (mJoin == joFreeFly)
                            set_attack = true;
                        else if (mJoin == joTurn && mAngle3D > 0.5f)
                            set_attack = true;
                    }
                    if (set_attack)
                        SetCurrentState(CS_ATTACK);
                }
                break;
            case CS_ATTACK:
                {
                    bool set_move = false;
                    if (mTarget.Ptr() == null || mJoin == joImmediately)
                        set_move = true;
                    else if (mJoin == joTurn && mAngle3D < -.5f)
                        set_move = true;
                    if (set_move)
                        SetCurrentState(CS_MOVE);
                }
                break;
        }
    }

    // IBaseUnitAi
    public override void SetSkill(DWORD skill, bool already_setted)
    {
        if (!already_setted)
        {
            if (mpSkill!=null)
            {
                mpSkill.Dispose();
                mpSkill = null;
            }
            iUnifiedVariableContainer pskill =Skills.GetSkillContainer(skill);
            mpSkill = new BFSkill(pskill);
            already_setted = true;
        }
        mpBFSkill = (BFSkill)mpSkill.Query(BFSkill.ID);
        //Asserts.AiAssertEx(mpBFSkill!=null, "can't find BFSkill");
        Assert.IsNotNull(mpBFSkill, "can't find BFSkill");
        base.SetSkill(skill, already_setted);
    }

    // BaseAi
    public override void SetInterface(IGame igame, iContact contact, UNIT_DATA unit_data, IGroupAi grp_ai)
    {
        base.SetInterface(igame, contact, unit_data, grp_ai);
        mStrafeTimer = -1f;
        mStrafeVector = Vector3.zero;
        mLeaderStrafeVector = Vector3.zero;
        mThreatDelta = RandomGenerator.Rand01() * 200f + 25f;
    }
}
