using UnityEngine;
using DWORD = System.UInt32;
//using AI_STATE = System.Boolean;
public abstract class BaseUnitAi : BaseAi, IUnitAi
{
    new public const uint ID = 0x84E8AF17;

    // route propeties
    DWORD mCurrentFormation;
    float mFormationDist;
    protected float mTime;
    protected Vector3 mDest;
    protected Vector3 mDelta;
    protected JoinOption mJoin;
    protected bool mPaused;
    protected readonly TContact mLeader = new TContact();

    // fight properties

    public readonly TContact mTarget = new TContact();
    protected float mAiming;
    protected float GetRealThreat()
    {
        float f = self.Ptr().GetThreatF();
        if (f > 1f) f -= 1f;
        return f;
    }
    public float GetThreat() { return self.Ptr() != null ? self.Ptr().GetThreatF() : 0f; }

    // states - should be in BaseUnitAi
    public DWORD mCurrentState;
    public void SetCurrentState(DWORD state)
    {
        Debug.Log(string.Format("{0} {1} Tgt:[{2}] state {3}->{4}",mpGroup.GetGroupData().Callsign, mpData.Number+1,mTarget.Ptr() == null? "NO_TARGET":mTarget.Ptr(), BaseCraftAi.StateName(mCurrentState), BaseCraftAi.StateName(state)));
        OnStateInitialize(mCurrentState, false);
        mCurrentState = state;
        OnStateInitialize(mCurrentState, true);
    }
    public void SetCurrentStateRandomly(DWORD state1, DWORD state2, float prob)
    {
        if (RandomGenerator.Rand01() < prob)
            SetCurrentState(state1);
        else
            SetCurrentState(state2);
    }

    bool DummyState(StateFlag flag, float scale) { return true; }    // dummy state


    // BaseUnitAi  - should be in BaseAi
    float mProcessEnvTimer;
    protected private void ProcessEnvOnNextTick() { mProcessEnvTimer = -1f; }

    public virtual void OnStateInitialize(DWORD state, bool init) { }
    public virtual bool OnStateRunning(DWORD state, float scale) { 
        return true; 
    }
    public virtual void ProcessEnvironment()
    {
        mProcessEnvTimer = 3f;
    }

    // type cast
    public IUnitAi GetIUnitAi() { return this; }

    // -------------------------------------------------
    // API
    public BaseUnitAi()
    {
        //mLeader = new TContact();
        //mTarget = new TContact();
        mLeader.setPtr(null);
        mTarget.setPtr(null);
        mPaused = false;
    }

    // IAi
    public override object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case IUnitAi.ID: return GetIUnitAi();
            case BaseUnitAi.ID: return this;
            default: return base.Query(cls_id);
        }
    }
    public override bool Update(float scale)
    {
        bool ret = base.Update(scale);
        if (!ret) return false;
        mProcessEnvTimer -= scale;
        if (mProcessEnvTimer <= 0f)
            ProcessEnvironment();
        if (!OnStateRunning(mCurrentState, scale))
            return false;
        return true;
    }


    // IUnitAi
    public virtual bool setFormation(iContact _leader, Vector3 _delta, float _dist, DWORD formation_name)
    {
        if (self.Validate() == false) return false;
        mCurrentFormation = formation_name;
        //mLeader = new TContact(_leader);
        mLeader.setPtr(_leader);
        mDelta = _delta;
        mFormationDist = _dist;
        Pause(false);
        return true;
    }
    public virtual bool SetDestination(Vector3 org, float _time)
    {
        // only leaders
        if (self.Validate() == false) return false;
        mTime = _time;
        mDest = org;
        return true;
    }

    public Vector3 GetDestination()
    {
        return mDest;
    }
    public virtual bool JoinFormation(JoinOption _join)
    {
        mJoin = _join;
        return true;
    }
    public virtual void Pause(bool pause)
    {
        mPaused = pause;
    }
    public virtual void setSpeed(float spd) { }


    // IBaseUnit
    public override iContact GetTarget()
    {
        
        return mTarget?.Ptr();
    }
    public override float GetAiming()
    {
        return mAiming;
    }

    // BaseAi
    public override void SetInterface(IGame igame, iContact contact, UNIT_DATA unit_data, IGroupAi grp_ai)
    {
        base.SetInterface(igame, contact, unit_data, grp_ai);
        // route properties
        mFormationDist = 0;
        mTime = 0;
        mDelta = Vector3.zero;
        mDest = Vector3.zero;
        mJoin = JoinOption.joFreeFly;
        mCurrentFormation = Constants.THANDLE_INVALID;

        // fight
        mTarget.setPtr(null);
        mAiming = 0;

        // states
        mProcessEnvTimer = -1f;
        mCurrentState = Constants.THANDLE_INVALID;
        SetState(DummyState);
    }

    // new
    public override IVmFactory getTopFactory() { return myBaseAiFactory; }
    public override void enumTargets(ITargetEnumer en)
    {
        if (mTarget.Ptr()!=null)
            en.processTarget(mTarget.Ptr(), mAiming);
    }
};