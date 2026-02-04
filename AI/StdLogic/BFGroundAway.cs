using UnityEngine;
using DWORD = System.UInt32;
using static BaseCraftAi;
using static AICommon;
using static SingleBFAi;
// states
public class BFGroundAway : BFGround
{
    public override DWORD GetNextState()
    {
        return mTarget!=null ? GROUND_APPROACH : IVm.THANDLE_INVALID;
    }
    public override DWORD GetStateName()
    {
        return GROUND_AWAY;
    }
    public override bool Update(float scale, ref Vector3  diff, ref float speed, bool visible_by_eyes)
    {
        if (!Init()) return false;

        if (mDist2D > mRange + 200f)
            return false;

        Vector3 df=Vector3.zero;
        if (mTopTarget!=null)
        {
            df = mTarget.GetOrg() - mTopTarget.GetOrg();
        }
        else
        {
            if (mpCraft.mSlowTargetsMiddleCount == 0)   // если нет середины 
                df = -mpCraft.mTargetDiff;      // берем вектор от цели
            else
            {
                Vector3 v2=Vector3.zero;
                switch (mpCraft.mAttackType)
                {
                    case ATTACK_TYPE_MEDIUM:
                        {
                            df = mTarget.GetOrg() - mpCraft.mSlowTargetsMiddlePos;     // иначе берем вектор от середины
                            v2 = mpCraft.self.Ptr().GetOrg() - mpCraft.mSlowTargetsMiddlePos;
                        }
                        break;
                    case ATTACK_TYPE_DIR:
                        {
                            df = mpCraft.mAttackDir;
                            v2 = Vector3.forward;
                        }
                        break;
                }
                df.y = 0f;
                float d1 = df.magnitude;
                df = CCmp(d1) ? Vector3.forward : df / d1;
                v2.y = 0f;
                float d2 = v2.magnitude;
                if (CCmp(d2) == false)
                {
                    v2 /= d2;
                    df += v2 * 0.5f;
                }
            }
        }
        df.y = 0f;
        float dflen;
        dflen = df.magnitude;
        df = CCmp(dflen) ? Vector3.forward: df / dflen;
        diff = df;
        diff *= (mRange + 200f);
        diff.y = mpCraft.mTargetDiff.y * mpCraft.mTargetDist + mTargetExceed * (mpCraft.mFastTargetsCount!=0 ? 0.5f : 1f);
        mpCraft.UseBattleMode(true);
        return true;
    }
};
