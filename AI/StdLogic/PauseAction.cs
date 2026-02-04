using UnityEngine;
using DWORD = System.UInt32;

class PauseAction : DynamicAction
{
    const float PAUSE_RD = 1000f;

    Vector3 mPauseOrg;
    float mPauseTime;
    bool mBreakOnTime;
    DWORD mPrevNear;

    public bool Initialize(IGroupAi grp, float tm, Vector3 org)
    {
        bool ret = base.Initialize(grp);
        if (ret)
        {
            mPauseTime = tm;
            mPauseOrg = org;
            mBreakOnTime = !AICommon.FCmp(mPauseTime, 0);
            mpDynGroup.CheckRouteDelta(mPauseOrg, out mPrevNear);
        }
        return ret;
    }

    // API
    public override ActionStatus Update(float scale)
    {
        ActionStatus status = base.Update(scale);
        if (status.IsActionAlive() && mpStdGroup.IsGroupAlive())
        {   // already alive
            mPauseTime -= scale;
            if (mBreakOnTime && mPauseTime <= 0f)
                status.ActionDead();
            else
            {
                if (mpDynGroup.SectorIsClear())
                    NeedToChangePoint();
                mDistTimer -= scale;
                if (mDistTimer <= 0f)
                {
                    DWORD leader_near;
                    float saved_rd = mpDynGroup.GetRouteDelta();
                    if (saved_rd < PAUSE_RD)
                        saved_rd = PAUSE_RD;
                    mDistTimer = mpDynGroup.CheckRouteDelta(mPauseOrg, out leader_near, saved_rd);
                    if (mPrevNear != leader_near)
                        NeedToChangePoint();
                    mPrevNear = leader_near;
                }
                if (mNeedToChangePoint)
                {
                    PauseGroup(mPrevNear != 0, false);     // not pausing
                    SetDestination(mPauseOrg, 0);    // setting dest
                    PointChanged();
                }
                if (isNeedToChangeFormation())
                {
                    SetFormation();
                    FormationChanged();
                }
            }
        }
        //Debug.Log(string.Format("Pause action status mDistTimer {0} mPauseTime {3} isAlive? {1} isDeactivated? {2} mBreakOnTime {4}", mDistTimer,status.IsActionAlive(),status.IsActionDeactivated(), mPauseTime, mBreakOnTime));
        return status;
    }
    public override bool IsDeleteOnPush()
    {
        return true;
    }

    const string sActionName = "Pause";
    public override string GetName()
    {
        return sActionName;
    }
    public override DWORD GetCode()
    {
        return Hasher.HshString(sActionName);
    }
};
