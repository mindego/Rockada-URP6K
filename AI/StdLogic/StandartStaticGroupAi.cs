using DWORD = System.UInt32;
//define MAX_HANGARS 32
public class StandartStaticGroupAi : StdGroupAi
{
    new public const uint ID = 0xA3A6554F;
    // states 
    bool Idle(StateFlag flag, float scale)
    {
        throw new System.NotImplementedException();
    }

    // api
    // common
    public override object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case StandartStaticGroupAi.ID: return this;
            default: return base.Query(cls_id);
        }
    }

    // StdGroupAi
    public override void LeaderHumanityChanged(bool b) { }
    public override void SetGroupDisappear(bool base_ret, DWORD base_id, float dist, DWORD ultimate, string base_name)
    {
        StdMissionAi miss = (StdMissionAi) mpMission.Query(StdMissionAi.ID);
        if (miss!=null)
            miss.DeleteGroupUnits(GetIGroupAi(), 0, null, ultimate == DisappearCodes.DISAPPEAR_EXPLODE, true);
        mUpdateAlive = true;
    }

    // actions addon
    public override void OnGroupCreate()
    {
        AddAction(ActionFactory.CreateIdleAction(GetIGroupAi()));
    }
    public override void OnActionDeath()
    {
        return ;
    }
    public override void OnActionDeactivate()
    {
        return;
    }
    public override void OnEmptyActions()
    {
        return;
    }

    public override void OnStartAppear(iContact hng)
    {
        base.OnStartAppear(hng);
    }
    public override void OnFinishAppear()
    {
        base.OnFinishAppear();
    }

    // new
    public override IVmFactory getTopFactory() { return myStdGroupFactory; }
};

