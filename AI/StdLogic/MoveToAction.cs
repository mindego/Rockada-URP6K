using UnityEngine;
using DWORD = System.UInt32;

class MoveToAction : RouteToAction
{
    public override bool Initialize(IGroupAi grp, Vector3 org, float time, string script)
    {
        bool ret = base.Initialize(grp, org, time, script);
        return ret;
    }

    // API
    public override bool IsDeleteOnPush()
    {
        return true;
    }
    const string sActionName = "MoveTo";
    public override string GetName()
    {
        return sActionName;
    }
    public override DWORD GetCode()
    {
        return Hasher.HshString(sActionName);
    }
};
