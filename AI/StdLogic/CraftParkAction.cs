using UnityEngine;
using DWORD = System.UInt32;

class CraftParkAction : ParkAction
{
    const float PARK_DIST = 1800f;
    public override bool Initialize(IGroupAi grp, string base_name, DWORD ultimate)
    {
        bool ret = base.Initialize(grp, base_name, ultimate);
        if (ret)
        {
            iContact leader = mpGroup.GetLeaderContact();
            if (leader!=null)
            {
                IMissionAi miss = mpStdGroup.GetIMissionAi();
                Vector3 org=Vector3.zero;
                Vector3 dir;
                Vector3 right;
                iContact hangar_leader = null;
                IGroupAi grp_ai = miss.GetGroupByID(mBaseId);
                if (grp_ai!=null)
                    hangar_leader = grp_ai.GetLeaderContact();
                if (hangar_leader!=null)
                    org = hangar_leader.GetOrg();
                else if (leader!=null)
                    org = leader.GetOrg();
                dir = leader.GetDir();
                right = leader.GetRight();
                org.y = 100f;
                dir.y = 0f;
                dir.Normalize();
                right.y = 0f;
                right.Normalize();
                mDynPoints.Add(new POINT_DATA(org + dir * PARK_DIST, 0f, null));
                mDynPoints.Add(new POINT_DATA(org + right * PARK_DIST, 0f, null));
                mDynPoints.Add(new POINT_DATA(org - dir * PARK_DIST, 0f, null));
                mDynPoints.Add(new POINT_DATA(org - right * PARK_DIST, 0f, null));
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
    public override ActionStatus Update(float scale)
    {
        ActionStatus status = base.Update(scale);
        if (status.IsActionAlive() && mpStdGroup.IsGroupAlive())
        {
            for (int i = 0; i < mpStdGroup.GetGhostCount(); ++i)
            {
                AiUnit ai = mpStdGroup.GetAiUnit(i);
                if (ai==null) continue;
                DWORD n = FindMember(ai.UnitData().Number);
                if (Constants.THANDLE_INVALID == n || (Constants.THANDLE_INVALID != n && mMembers[(int)n].Status() == MemberStatus.STATUS_REPAIRED)) continue;
                if (ai.UnitData().IsLanded(false)) continue;  // уже запрашивали
                if (mBaseId == Constants.THANDLE_INVALID) continue;              // нет баз
                mpDynGroup.RepairUnit(mBaseId, (int)ai.UnitData().Number, mUltimate != DisappearCodes.DISAPPEAR_DEATH, mUltimate == DisappearCodes.DISAPPEAR_REPAIR, null);  // садимся на последнюю
            }
        }
        return status;
    }

    const string sActionName = "CraftPark";
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
