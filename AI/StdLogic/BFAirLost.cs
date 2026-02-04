using UnityEngine;
using DWORD = System.UInt32;
using static SingleBFAi;
using static BaseCraftAi;
using static UnitDataDefines;
using static AICommon;

public class BFAirLost : CraftAiState
{
    SingleBFAi mpCraft;

    float mSearchTimer;
    float mTargetSearch;

    public bool Initialize(SingleBFAi craft)
    {
        mpCraft = craft;
        mTargetSearch = mpCraft.mTargetSearch;
        mSearchTimer = mTargetSearch; // начинаем процесс поиска
        return true;
    }
    void Destroy() { }

    public virtual void ReInitialize() { }
    public virtual DWORD GetNextState()
    {
        return IVm.THANDLE_INVALID;
    }
    public virtual DWORD GetStateName()
    {
        return TARGET_LOST;
    }
    public bool Update(float scale, ref Vector3 diff, ref float speed, bool visible_by_eyes)
    {
        if (mpCraft.mTarget.Ptr() == null)
            return false;
        if (visible_by_eyes)
            return false;
        mSearchTimer -= scale; // уменьшаем счетчик
        if (mSearchTimer < 0f)
        { // если ищем уще долго
            mpCraft.ForgetTarget();             // бросаем цель
            return false;
        }
        speed = Storm.Math.KPH2MPS(1000f);
        diff = mpCraft.mTarget.Ptr().GetOrg() - mpCraft.self.Ptr().GetOrg();  // место последнего контакта
        diff.y = mpCraft.mpBFSkill.valTargetSearchAltitude; // летим в точку которая чуть выше ем последнее местоположение
        if (Mathf.Pow(diff.x, 2) + Mathf.Pow(diff.z, 2) < Mathf.Pow(BF_SEARCH_RADIUS, 2))
        { // находимся рядом с последним местопребыванием
            mpCraft.ForgetTarget();             // бросаем цель
            return false;
        }
        return true;
    }


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

public class BFAirMissileEvasion : BFAir
{
    Vector3 mDest;
    iContact mpThreat;
    float myFinishTime;

    public override bool Initialize(SingleBFAi craft)
    {
        if (base.Initialize(craft))
        {
            mDest = mpCraft.self.Ptr().GetRight();
            mDest.y = 0;
            if (Prb(0.5f)) mDest *= -1f;
            mpThreat = mpCraft.self.Ptr().GetThreat();
            myFinishTime = Rnd(craft.mpBaseCraftSkill.valMissileAvoidTimeMin, craft.mpBaseCraftSkill.valMissileAvoidTimeBnd);
            return true;
        }
        return false;
    }
    public BFAirMissileEvasion() { mpThreat = null; }

    public override DWORD GetNextState()
    {
        return IVm.THANDLE_INVALID;
    }
    public override DWORD GetStateName()
    {
        return MISSILE_EVASION;
    }
    public override bool Update(float scale, ref Vector3 diff, ref float speed, bool visible_by_eyes)
    {
        myFinishTime -= scale;
        if (myFinishTime < 0)
            return false;

        iContact cnt = mpCraft.self.Ptr().GetThreat();
        if (cnt != mpThreat)
            return false;

        mpCraft.UseBattleMode(false);
        diff = mDest;
        speed = 1000f;

        return true;
    }
};


public class BFAirAttack : BFAir
{
    float mCheckTimer;
    DWORD mRecommendedState;

    public BFAirAttack() { mRecommendedState = IVm.THANDLE_INVALID; mCheckTimer = -1f; }

    public override DWORD GetNextState()
    {
        return mTarget != null ? mRecommendedState : IVm.THANDLE_INVALID;
    }
    public override DWORD GetStateName()
    {
        return AIR_ATTACK;
    }
    public override bool Update(float scale, ref Vector3 diff, ref float speed, bool visible_by_eyes)
    {
        if (!Init()) return false;

        //if (mpCraft.mpGroup.GetGroupData().Side==1)
        mCheckTimer -= scale;
        if (mCheckTimer < 0f)
        {
            if (mpCraft.TargetWillBeCrashed(2))
            {
                mRecommendedState = AIR_AWAY;
                return false;
            }
            mCheckTimer = 0.5f;
        }

        if (mpCraft.mAngle3D < 0.34f)
        {      // cos(70)
            mRecommendedState = AIR_TURN;
            return false;
        }

        // висим на хвосте
        if (mpCraft.mTargetDist < mpCraft.mpGuns.GetRange() * .8f && mpCraft.mEnemyAngle3D < 0 && mpCraft.GetThreat() < mpCraft.mpBFSkill.valStayOnSixThreatFactor)
        {
            speed = mTarget.GetSpeed().magnitude;
        }

        diff = mpCraft.mTargetAim;
        return true;
    }
};

public class BFAirTurn : BFAir
{
    float mCheckTimer;
    DWORD mRecommendedState;

    public BFAirTurn() { mRecommendedState = IVm.THANDLE_INVALID; mCheckTimer = -1f; }

    public override DWORD GetNextState()
    {
        return mTarget != null ? mRecommendedState : IVm.THANDLE_INVALID;
    }
    public override DWORD GetStateName()
    {
        return AIR_TURN;
    }
    public override bool Update(float scale, ref Vector3 diff, ref float speed, bool visible_by_eyes)
    {
        if (!Init()) return false;

        //if (mpCraft.mpGroup.GetGroupData().Side==1)
        mCheckTimer -= scale;
        if (mCheckTimer < 0f)
        {
            if (!mpCraft.TargetCanBeAttacked((mpCraft.mType == UT_FLYING_FAST) ? 900f : 600f, 0.87f, 10))
            {
                mRecommendedState = AIR_AWAY;
                return false;
            }
            mCheckTimer = 2f;
        }

        if (mpCraft.mAngle3D > 0.87f)
        { // cos(20) угол маленький 
            mRecommendedState = AIR_ATTACK;
            return false;
        }

        // висим на хвосте
        if (mpCraft.mTargetDist < mpCraft.mpGuns.GetRange() * .8f && mpCraft.mEnemyAngle3D < 0 && mpCraft.GetThreat() < mpCraft.mpBFSkill.valStayOnSixThreatFactor)
        {
            speed = mTarget.GetSpeed().magnitude;
        }

        diff = mpCraft.mTargetAim;
        return true;
    }
};

public class BFAirAway : BFAir
{
    float mCheckTimer;

    public BFAirAway() { mCheckTimer = -1f; }

    public override DWORD GetNextState()
    {
        return mTarget != null ? AIR_TURN : IVm.THANDLE_INVALID;
    }
    public override DWORD GetStateName()
    {
        return AIR_AWAY;
    }
    public override bool Update(float scale, ref Vector3 diff, ref float speed, bool visible_by_eyes)
    {
        if (!Init()) return false;

        //if (mpCraft.mpGroup.GetGroupData().Side==1)
        mCheckTimer -= scale;
        if (mCheckTimer < 0f)
        {
            if (mpCraft.TargetCanBeAttacked((mpCraft.mType == UT_FLYING_FAST) ? 900f : 600f, 0.87f, 10))
            {
                if (UT_FLYING_FAST == mpCraft.mType && mpCraft.mFastTargetsCount != 0)
                { // не поворачиваем если есть рядом враги
                }
                else
                    return false;
            }
            mCheckTimer = 2f;
        }

        diff = -mpCraft.mTargetDiff;
        diff.y = 0f;
        return true;
    }
};
