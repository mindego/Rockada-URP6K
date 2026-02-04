using UnityEngine;

public abstract class BFAir : CraftAiState
{
    protected SingleBFAi mpCraft;

    protected iContact mTarget;

    protected bool Init()
    {
        mTarget = mpCraft.mTarget.Ptr();
        return mTarget!=null;
    }

    public virtual bool Initialize(SingleBFAi craft) { mpCraft = craft; return true; }

    void Destroy() { }

    public virtual void ReInitialize() { }

    public abstract uint GetStateName();
    public abstract uint GetNextState();
    public abstract bool Update(float scale, ref Vector3 diff, ref float speed, bool visible_by_eyes);
    public void AddRef()
    {

    }
    public int RefCount()
    {
        return 0;
    }
    public int Release()
    {
        return 0;
    }
}