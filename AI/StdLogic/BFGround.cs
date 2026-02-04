//StandartUnitAi было
using UnityEngine;
// GROUND 
public abstract class BFGround : CraftAiState
{
    protected SingleBFAi mpCraft;

    protected iContact mTarget;
    protected iContact mTopTarget;
    protected float mTargetExceed;
    protected float mTargetAway;
    protected float mDist2D;
    protected float mRange;
    public bool Init()
    {
        mTarget = mpCraft.mTarget.Ptr();
        if (mTarget==null) return false;
        mDist2D = mpCraft.GetDist2D() - mTarget.GetRadius();
        if (mDist2D < 0f)
            mDist2D = 0f;
        mRange = mpCraft.mpGuns.GetRange();
        mTargetAway = Mathf.Min(300f, mRange * 0.5f);
        mTopTarget = mpCraft.mTopTarget.Ptr();
        if (mTopTarget==null)
            mTargetExceed = mRange * 0.5f;
        else
        {
            Vector3 delta = mTopTarget.GetOrg() - mTarget.GetOrg();
            mTargetExceed = (mTarget.GetOrg().y - mTopTarget.GetOrg().y) / delta.magnitude;
            if (mTargetExceed > 0.707f) // sin(45)
                mTargetExceed = 0.707f;
            mTargetExceed *= mRange;
        }
        return true;
    }

    public bool Initialize(SingleBFAi craft) { mpCraft = craft; return true; }
    public void Destroy() { }

    public virtual void ReInitialize() { }


    public abstract uint GetStateName();
    public abstract uint GetNextState();
    public abstract bool Update(float scale, ref Vector3 diff,ref float speed, bool visible_by_eyes);

    private int refcount=0;
    public void AddRef()
    {
        refcount++;
    }
    public int RefCount()
    {
        return refcount;
    }
    public int Release()
    {
        if (refcount >0) refcount--;
        return refcount;
    }
}
