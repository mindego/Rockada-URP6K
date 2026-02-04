using UnityEngine;
using DWORD = System.UInt32;

class PatrolAction : RouteAction
{

    public bool Initialize(IGroupAi grp, Vector3  center,float dist)
    {
        bool ret = base.Initialize(grp, 0, null);
        if (ret)
        {
            Vector3 cent = center;
            if (cent.y < 3f) cent.y = 3f;
            if (dist < 200f) dist = 200f;

            PatrolInfo info = mpDynGroup.GetIGame().GetPatrolInfo(PatrolInfo.iPatrolRoute);
            if (info!=null)
            {
                float cos_angle = Mathf.Cos(Storm.Math.GRD2RD((RandomGenerator.Rand01() * 359f)));
                float sin_angle = Mathf.Sqrt(1f - Mathf.Pow(cos_angle,2));
                for (int i = 0; i < PatrolInfo.PATROL_DIM; i++)
                {
                    Vector3 org = info.Delta[i] * dist;
                    AICommon.rotatebyangle(ref org, cos_angle, sin_angle);
                    org += cent;
                    org.y = cent.y;
                    mDynPoints.Add(new POINT_DATA(org, 0f, null));
                }
                POINT_DATA point = mDynPoints[mDynPoints.Count - 1];
                point.SetScript("Switch(Num=0);");
                mDynPoints[mDynPoints.Count - 1] = point;
                SetPoints();
            }
            else
                ret = false;
        }
        return ret;
    }

    // API
    public override bool IsDeleteOnPush()
    {
        return false;
    }
    
    const string sActionName = "Patrol";
    public override string GetName()
    {
        return sActionName;
    }
    public override DWORD GetCode()
    {
        return Hasher.HshString(sActionName);
    }
    public override bool IsSwitching()
    {
        return true;
    }
};