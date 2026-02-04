using DWORD = System.UInt32;
using UnityEngine;
using static AICommon;

class AlertAction : RouteAction
{
    public const uint ID = 0x5961E2FB;

    public bool Initialize(IGroupAi grp, Vector3 center)
    {
        bool ret = base.Initialize(grp, 0, null);
        if (ret)
        {
            iContact leader = mpGroup.GetLeaderContact();
            if (leader!=null)
            {
                Vector3 Dest;
                float dist = CalcNearAlertVector(center, leader.GetOrg(), out Dest);
                Dest.y = 15f;
                if (!CCmp(dist))
                    mDynPoints.Add(new POINT_DATA(Dest, dist / leader.GetMaxSpeed(), "AttackRadius(Radius=1000);RouteDelta(Delta=500);KillAll(Flag=0);"));
                Dest = center;
                Dest.y = 150f;
                mDynPoints.Add(new POINT_DATA(Dest, 0f, "AttackRadius(Radius=10000);RouteDelta(Delta=10000);KillAll(Flag=1);"));
                SetPoints();
            }
            else
                ret = false;
        }
        return ret;
    }


    // API
    public override object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case AlertAction.ID: return this;
            default: return base.Query(cls_id);
        }
    }

    public override  bool IsDeleteOnPush()
    {
        return true;
    }


    const string sActionName = "Alert";
    public override string GetName()
    {
        return sActionName;
    }
    public override DWORD GetCode()
    {
        return Hasher.HshString(sActionName);
    }

    public void UpdateAlert(Vector3 org)
    {
        iContact leader = mpGroup.GetLeaderContact();
        POINT_DATA point;

        if (mPoints.Count() > 1 && leader!=null)
        {
            Vector3 Dest;
            float dist = CalcNearAlertVector(org, leader.GetOrg(), out Dest);
            if (!CCmp(dist))
            {
                point = mPoints[0];
                point.Org = Dest;
                mPoints[0] = point;
            }
        }
        point = mPoints[mPoints.Count() -1];
        point.Org = org;
        mPoints[mPoints.Count() - 1] = point;

        NeedToChangePoint();
    }

    //TODO возможно, стоит перенесли константу и метод в static метод AlertAction
    public const float ALERT_DIST = 5000f;
    public static float CalcNearAlertVector(Vector3 alert_org, Vector3 leader_org, out Vector3 move_org)
    {
        move_org = Vector3.zero;
        Vector3 Dest = alert_org - leader_org;
        Dest.y = 0;
        float d = Dest.magnitude;
        if (d > ALERT_DIST)
        {
            Dest /= d;
            move_org = leader_org + Dest * (d - ALERT_DIST);
            return d;
        }
        return 0f; 
    }
};