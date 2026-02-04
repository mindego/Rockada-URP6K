using UnityEngine;
using DWORD = System.UInt32;
using static SingleBFAi;

public class  BFGroundAttack : BFGround
{
    public override DWORD GetNextState()
    {
        return mTarget!=null ? GROUND_AWAY : IVm.THANDLE_INVALID;
    }
    public override DWORD GetStateName()
    {
        return GROUND_ATTACK;
    }
    public override bool Update(float scale, ref Vector3 diff, ref float speed, bool visible_by_eyes)
    {
        if (!Init()) return false;

        float t = mTargetAway;
        if (mpCraft.GetThreat() > mpCraft.mpBFSkill.valAwayThreatFactor)
            t = Mathf.Min(1200f, mRange * mpCraft.mpBFSkill.valAwayThreatMul);

        if (mDist2D < t)
            return false;

        diff = mpCraft.mTargetAim;
        mpCraft.UseBattleMode(true);

        return true;
    }
};
