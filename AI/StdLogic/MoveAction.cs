using DWORD = System.UInt32;

class MoveAction : RouteAction
{
    public override bool Initialize(IGroupAi grp, DWORD pts_count, POINT_DATA[] pts)
    {
        return base.Initialize(grp, pts_count, pts);
    }

    public override bool IsDeleteOnFinish()
    {
        return mPoints.Count() == 0;
    }

    // API
    public override bool IsDeleteOnPush()
    {
        return false;
    }

    const string sActionName = "Move";
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
    public override  void OnGroupReborn()
    {
        base.OnGroupReborn();
        mCurrentPoint = Constants.THANDLE_INVALID;
        SetNextPoint(Constants.THANDLE_INVALID);
        NextPoint();
    }
};
