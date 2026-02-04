using UnityEngine;
using DWORD = System.UInt32;

public class StandartUnitAi : BaseAi, IUnitAi
{
    new public const uint ID = 0x52851F10;

    public DWORD mCurrentFormation;
    public float time;
    public float tactics_timer;
    public Vector3 delta, dest;
    public float formation_dist;
    public JoinOption join;

    public readonly TContact mTarget = new TContact();
    public void setTarget(iContact tar) { mTarget.setPtr(null); aiming = 0; }
    public readonly TContact leader = new TContact();
    public float aiming;

    // unit special
    public float GetRealThreat()
    {
        return self.Ptr().GetThreatF();
    }

    IUnitAi GetIUnitAi() { return this; }

    // api
    public StandartUnitAi()
    {
        //leader = new TContact();
        //mTarget = new TContact();
        leader.setPtr(null);
        mTarget.setPtr(null);
    }

    // IAi
    public override object Query(uint cls_id)
    {
        switch ((uint)cls_id)
        {
            case IUnitAi.ID: return GetIUnitAi();
            case StandartUnitAi.ID: return this;
            default: return base.Query(cls_id);
        }

    }

    // IUnitAi
    public virtual bool setFormation(iContact _leader, Vector3 _delta, float _dist, DWORD formation_name)
    {
        if (self.Validate() == false) return false;
        //#pragma message ("EEI:dummy")
        mCurrentFormation = formation_name;

        //leader = new TContact(_leader);
        leader.setPtr(_leader);
        delta = _delta;
        formation_dist = _dist;
        return true;
    }

    /// <summary>
    /// only leaders
    /// </summary>
    /// <returns></returns>
    public virtual bool SetDestination(Vector3 org, float _time)
    {
        // only leaders
        if (self.Validate() == false) return false;
        time = _time;
        dest = org;
        return true;
    }
    public virtual Vector3 GetDestination()
    {
        return dest;
    }

    public virtual bool JoinFormation(JoinOption _join)
    {
        join = _join;
        return true;
    }

    public virtual void Pause(bool pause) { }
    public virtual void setSpeed(float spd) { }


    // IBaseUnit
    public override iContact GetTarget() { return mTarget.Ptr(); }
    public override float GetAiming() { return aiming; }

    // IBaseAi
    public override void SetInterface(IGame igame, iContact contact, UNIT_DATA unit_data, IGroupAi grp_ai)
    {
        base.SetInterface(igame, contact, unit_data, grp_ai);
        formation_dist = 0;
        time = 0;
        delta = Vector3.zero;
        dest = Vector3.zero;
        join = JoinOption.joFreeFly;
        mTarget.setPtr(null);
        aiming = 0;
        mCurrentFormation = Constants.THANDLE_INVALID;
    }

    // new
    public override IVmFactory getTopFactory() { return myBaseAiFactory; }
    public override void enumTargets(ITargetEnumer en)
    {
        if (mTarget.Ptr() != null)
            en.processTarget(mTarget.Ptr(), aiming);
    }

};

