using UnityEngine;
using DWORD = System.UInt32;

public class CraftTakeoffAction : RouteAction
{
    const float TAKEOFF_DIST = 1000f;
    const string sActionName = "CraftTakeoff";
    bool mTakeoffed;

    void SetTakeoffRoute(Vector3 center, Vector3 dir, Vector3 right)
    {
        mDynPoints.Clear();

        Vector3 org = center;
        org.y = 100f;
        Vector3 ldir = dir;
        ldir.y = 0f;
        ldir.Normalize();
        Vector3 lright = right;
        lright.y = 0f;
        lright.Normalize();

        mDynPoints.Add(new POINT_DATA(org + ldir * TAKEOFF_DIST, 0f, null));
        mDynPoints.Add(new POINT_DATA(org + lright * TAKEOFF_DIST, 0f, null));
        mDynPoints.Add(new POINT_DATA(org - ldir * TAKEOFF_DIST, 0f, null));
        mDynPoints.Add(new POINT_DATA(org - right * TAKEOFF_DIST, 0f, null));
        mDynPoints[mDynPoints.Count - 1].SetScript("Switch(Num=0);");
        SetPoints();
    }

    public override bool Initialize(IGroupAi grp)
    {
        bool ret = base.Initialize(grp, 0, new POINT_DATA[0]);
        if (ret)
        {
            mTakeoffed = false;
        }
        return ret;
    }

    // API
    public override bool IsDeleteOnPush()
    {
        return true;
    }
    public override string  GetName()
    {
        return sActionName;
    }
    public override DWORD GetCode()
    {
        return Hasher.HshString(sActionName);
    }
    public override ActionStatus Update(float scale)
    {
        ActionStatus status = GetAliveStatus();
        if (mTakeoffed == false)
        {    // wait for first unit
            if (mpStdGroup.GetGhostCount() == 0)
                status.GroupDead();
            else
            {
                iContact cnt = mpStdGroup.GetLeaderContact();
                if (cnt!=null)
                {
                    SetTakeoffRoute(cnt.GetOrg(), cnt.GetDir(), cnt.GetRight());
                    mTakeoffed = true;
                }
            }
        }
        else
        {
            status = base.Update(scale);
            if (status.IsActionAlive())
            {
                if (mpDynGroup.GetAliveCount() == mpDynGroup.GetReqGhostCount())
                    status.ActionDead();
            }
        }
        return status;
    }
    public override bool IsSwitching()
    {
        return true;
    }
    public override  bool IsCanBeBreaked()
    {
        return false;
    }
};
