using UnityEngine;

public class  DynamicAction : IdleAction
{
    protected bool mNeedToChangePoint;
    protected bool mNeedToChangeFormation;
    protected float mDistTimer;

    protected StdDynamicGroupAi mpDynGroup;

    protected void NeedToChangePoint() { mNeedToChangePoint = true; }
    protected void PointChanged() { mNeedToChangePoint = false; }
    protected void NeedToChangeFormation() { mNeedToChangeFormation = true; }
    protected void FormationChanged() { mNeedToChangeFormation = false; }
    protected bool isNeedToChangeFormation() { return mNeedToChangeFormation; }

    void CheckForUnitsChanges()
    {
        if (mpDynGroup.isLeaderChanged())
        {
            NeedToChangePoint();
            NeedToChangeFormation();
        }
    }
    protected bool executeScript(string script, string namesp)
    {
        bool ret = mpStdGroup.executeScript(null, script, namesp);
        CheckForUnitsChanges();
        return ret;
    }

    public DynamicAction() { mpDynGroup = null; }

    protected bool PauseGroup(bool pause, bool process_not_wing)
    {
        int n = 0;
        for (int j = 0; j < mpStdGroup.GetGhostCount(); ++j)
        {
            AiUnit ai = mpStdGroup.GetAiUnit((uint)j);
            if (ai.GetAI()!=null)
            {
                if (n == 0)
                    mpDynGroup.SetPauseToUnit(ai, pause);
                else if (process_not_wing)
                    mpDynGroup.SetPauseToUnit(ai, false);
                n++;
            }
        }
        return n!=0;
    }
    protected bool SetDestination(Vector3 org, float time)
    {
        for (int j = 0; j < mpStdGroup.GetGhostCount(); ++j)
        {
            AiUnit ai = mpStdGroup.GetAiUnit((uint)j);
            if (ai!=null && mpStdGroup.isLeader(ai.GetAI()))
            {
                mpDynGroup.SetDestinationToUnit(ai, org, time);
                return true;
            }
        }
        return false;
    }
    protected void SetFormation()
    {
        uint n = 0;
        bool not_reformat = (mpDynGroup.isAutoReformated() == false && mpDynGroup.isNeedFormationChange() == false);
        for (int j = 0; j < mpStdGroup.GetGhostCount(); ++j)
        {
            AiUnit ai = mpStdGroup.GetAiUnit((uint)j);
            bool is_leader = mpStdGroup.isLeader(ai.GetAI());
            if (ai.GetAI() !=null && is_leader == false) //TODO - Возможно, тут нужен сам aiunit, а не его GetAI
            {
                mpDynGroup.SetFormationToUnit(ai, not_reformat ? ai.UnitData().Number : n + 1, null);
                ++n;
            }
        }
        mpDynGroup.checkFormationChanged();
    }

    public override bool Initialize(IGroupAi grp)
    {
        bool ret = base.Initialize(grp);
        if (ret)
            mpDynGroup = (StdDynamicGroupAi)(mpGroup.Query(StdDynamicGroupAi.ID));
        NeedToChangePoint();
        NeedToChangeFormation();
        mDistTimer = -1f;
        return ret && mpDynGroup!=null;
    }

    // API
    public override void Activate()
    {
        base.Activate();
        NeedToChangePoint();
        NeedToChangeFormation();
    }
    public override ActionStatus Update(float scale)
    {
        ActionStatus status = base.Update(scale);
        if (status.IsActionAlive() && mpStdGroup.IsGroupAlive())
        {
            CheckForUnitsChanges();
            mpDynGroup.ProcessReaches();
        }
        return status;
    }
    public override  void updatePoint() { NeedToChangePoint(); }
};