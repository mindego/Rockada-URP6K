public class StdStaticHangarAi : StdHangarAi
{
    new public const uint ID=0x7A1A86A8;
    bool HangarIdle(StateFlag flag, float scale)
    {
        ProcessQueue(scale);
        return true;
    }
    // api
    // IAi
    public override object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case StdStaticHangarAi.ID: return this;
            default: return base.Query(cls_id);
        }
    }

    //IBaseUnitAi
    public override void SelectTarget(int nTargets, iContact[] Targets, float[] TargetWeights) { }
    public override float GetAiming()
    {
        return 0;
    }

    // IBaseAi
    public override void SetInterface(IGame igame, iContact contact, UNIT_DATA unit_data, IGroupAi grp_ai)
    {
        base.SetInterface(igame, contact, unit_data, grp_ai);
        SetState(HangarIdle);
    }
};