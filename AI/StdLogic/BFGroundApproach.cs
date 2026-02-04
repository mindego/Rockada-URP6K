using UnityEngine;
using DWORD = System.UInt32;
using static SingleBFAi;

public class BFGroundApproach : BFGround
{
    public override DWORD GetNextState()
    {
        return mTarget != null ? GROUND_ATTACK : IVm.THANDLE_INVALID;
    }
    public override DWORD GetStateName()
    {
        return GROUND_APPROACH;
    }
    public override bool Update(float scale, ref Vector3 diff, ref float speed, bool visible_by_eyes)
    {
        if (!Init()) return false;

        if (mDist2D < mRange + 180f)
            return false;

        float t = mDist2D - mRange;
        diff = mpCraft.mTargetDiff * t;
        diff.y = Mathf.Clamp(mTarget.GetOrg().y + mTargetExceed - mpCraft.self.Ptr().GetOrg().y, -t * .5f, t * .5f);
        mpCraft.UseBattleMode(true);

        return true;
    }
}
