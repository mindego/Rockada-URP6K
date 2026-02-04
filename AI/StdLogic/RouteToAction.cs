using UnityEngine;
using DWORD = System.UInt32;

class RouteToAction : RouteAction
{
    public virtual bool Initialize(IGroupAi grp, Vector3 org, float time, string script)
    {
        //Debug.Log("Init RouteTo data for " + grp);
        bool ret = base.Initialize(grp, 0, null);
        if (ret)
        {
            mDynPoints.Add(new POINT_DATA(org, time, null));
            if (script != null)
            {
                POINT_DATA point = mDynPoints[0];
                point.SetScript(script);
                mDynPoints[0] = point;
            }
            SetPoints();
        }
        return ret;
    }

    // API
    public override  bool IsDeleteOnPush()
    {
        return false;
    }

    const string sActionName = "RouteTo";
    public override string  GetName()
    {
        return sActionName;
    }
    public override DWORD GetCode()
    {
        return Hasher.HshString(sActionName);
    }
};
