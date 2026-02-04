using System.Collections.Generic;
using UnityEngine;
using DWORD = System.UInt32;

class  EscortAction : DynamicAction
{
    bool mEscortReported;
    readonly TContact mEscortLeader = new TContact();
    float mEscortDelta;
    Tab<DWORD> mGroupCodes = new Tab<DWORD>();
    //List<string> mGroupNames = new List<string>();
    Tab<string> mGroupNames = new Tab<string>();

    void ProcessEscortDest(float scale) { } 
    void UpdateEscorted()
    {
        mEscortLeader.setPtr( null);
        IMissionAi msn = mpDynGroup.GetIMissionAi();
        for (int i = 0; i < mGroupCodes.Count(); ++i)
        {
            IGroupAi grp = msn.GetGroupByID(mGroupCodes[i]);
            if (grp==null)
                mGroupCodes[i] = Constants.THANDLE_INVALID;
            else
            {
                //mEscortLeader = new TContact(grp.GetLeaderContact());
                mEscortLeader.setPtr(grp.GetLeaderContact());
                if (mEscortLeader.Ptr()==null)
                {
                    mGroupCodes[i] = Constants.THANDLE_INVALID;
                    if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
                    {
                        AICommon.AiMessage(MessageCodes.AUTO_GROUP, "GrpInfo", "escorting group \"{0}\" stopped, leader dead", mGroupNames[i]);
                    }
                }
            }
            if (mEscortLeader.Ptr()!=null)
                break;
        }
        int n = 0;
        while (n < mGroupCodes.Count())
        {
            if (mGroupCodes[n] == Constants.THANDLE_INVALID)
            {
                mGroupCodes.Remove(n, n);
                //mGroupNames[n]=null;
                mGroupNames.Remove(n, n);
            }
            else
                n++;
        }
    }

    void SetEscortFormation(iContact leader)
    {
        int n = 0;
        bool not_reformat = (mpDynGroup.isAutoReformated() == false && mpDynGroup.isNeedFormationChange() == false);
        for (int j = 0; j < mpStdGroup.GetGhostCount(); ++j)
        {
            AiUnit ai = mpStdGroup.GetAiUnit(j);
            if (ai.GetAI()!=null)
            {
                mpDynGroup.SetFormationToUnit(ai, not_reformat ? ai.UnitData().Number : (uint) n, leader, mEscortDelta);
                ++n;
            }
        }
        mpDynGroup.checkFormationChanged();
    }
    public bool Initialize(IGroupAi grp, List<string> grps, float delta)
    {
        bool ret = base.Initialize(grp);
        if (ret)
        {
            IMissionAi msn = mpDynGroup.GetIMissionAi();
            for (int i = 0; i < grps.Count; ++i)
            {
                DWORD id = Hasher.HshString(grps[i]);
                if (msn.GetGroupByID(id)==null)
                {
                    if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
                    {
                        AICommon.AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, "group \"{0}\" not found , can't start escorting", grps[i]);
                    }
                }
                else
                {
                    mGroupCodes.New(id);
                    mGroupNames.New(grps[i]);
                }
            }
            PauseGroup(false, false);
            mEscortDelta = delta;
        }
        return ret;
    }

    // API
    public override  ActionStatus Update(float scale)
    {
        ActionStatus status = base.Update(scale);
        if (status.IsActionAlive() && mpStdGroup.IsGroupAlive())
        {   // already alive
            if (mEscortLeader.Validate() == false)
            {
                UpdateEscorted();
                if (mEscortLeader.Ptr()==null)
                {
                    status.ActionDead();
                    return status;
                }
                else
                    mEscortReported = false;
                NeedToChangeFormation();
                NeedToChangePoint();
            }
            Asserts.AssertBp(mEscortLeader.Ptr()!=null);
            if (mNeedToChangePoint)
            {
                PauseGroup(false, true);     // not pausing
                PointChanged();
            }
            if (isNeedToChangeFormation())
            {
                PauseGroup(false, true);     // not pausing
                SetEscortFormation(mEscortLeader.Ptr());
                FormationChanged();
            }
            mDistTimer -= scale;
            if (mDistTimer <= 0f)
            {
                iContact leader = mpDynGroup.GetLeaderContact();
                if (leader!=null)
                {
                    DWORD leader_near;
                    mDistTimer = mpDynGroup.CheckRouteDelta(mEscortLeader.Ptr().GetOrg(), out leader_near);
                    Vector3 diff = leader.GetOrg() - mEscortLeader.Ptr().GetOrg();
                    diff.y = 0;
                    if (!mEscortReported && diff.sqrMagnitude < Mathf.Pow(800f,2))
                    {
                        mpStdGroup.SendNotify(MessageCodes.AUTO_GROUP, AIGroupsEvents.MC_GROUP_ESCORT, 0, "OnGroupEscort");
                        mEscortReported = true;
                    }
                }
            }
        }
        return status;
    }
    public override bool IsDeleteOnPush()
    {
        return false;
    }
    const string sActionName = "Escort";
    public override string GetName()
    {
        return sActionName;
    }
    public override DWORD GetCode()
    {
        return Hasher.HshString(sActionName);
    }
};
