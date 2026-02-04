using UnityEngine;
using DWORD = System.UInt32;

class ParkAction : RouteAction
{
    protected DWORD mUltimate;
    protected string mpBaseName;
    protected DWORD mBaseId;
    protected Tab<MemberStatus> mMembers = new Tab<MemberStatus>();
    protected DWORD mLanded;
    protected DWORD mRepaired;
    protected DWORD mNotLanded;
    protected DWORD mDeleted;

    public override string ToString()
    {
        string res = GetType().ToString();
        res += "\nmpBaseName: " + mpBaseName;
        res += "\nmUltimate: " + mUltimate;
        res += "\nmBaseId: " + mBaseId.ToString("X8");
        res += "\nmMembers: " + mMembers.Count();
        res += "\nmLanded: " + mLanded;
        res += "\nmRepaired: " + mRepaired;
        res += "\nmNotLanded: " + mNotLanded;
        res += "\nmDeleted: " + mDeleted;
        return res;
    }
    public DWORD FindMember(DWORD numb)
    {
        for (int i = 0; i < mMembers.Count(); ++i)
            if (mMembers[i].Number() == numb)
                return (uint)i;
        return Constants.THANDLE_INVALID;
    }
    public void UpdateMembers()
    {
        mLanded = 0;
        mRepaired = 0;
        mNotLanded = 0;
        mDeleted = 0;
        for (int i = 0; i < mMembers.Count(); ++i)
        {
            AiUnit ptr_ai = null;
            for (int j = 0; j < mpStdGroup.GetGhostCount(); ++j)
            {
                AiUnit ai = mpStdGroup.GetAiUnit(j);
                if (ai.UnitData().Number == mMembers[i].Number())
                {
                    ptr_ai = ai;
                    break;
                }
            }
            if (ptr_ai != null)
            {
                AiUnit ref_ai = ptr_ai;
                if (ref_ai != null)
                {
                    switch (mMembers[i].Status())
                    {
                        case MemberStatus.STATUS_LANDED: mMembers[i].SetStatus(MemberStatus.STATUS_REPAIRED); break; // before not landed
                        case MemberStatus.STATUS_DELETED: case MemberStatus.STATUS_REPAIRED: break;
                    }
                }
                else
                {
                    switch (mMembers[i].Status())
                    {
                        case MemberStatus.STATUS_NOT_LANDED: case MemberStatus.STATUS_REPAIRED: mMembers[i].SetStatus(MemberStatus.STATUS_LANDED); break; // before not landed
                        case MemberStatus.STATUS_DELETED: break;
                        default: mMembers[i].SetStatus(MemberStatus.STATUS_LANDED); break;
                    }
                }
            }
            else
                mMembers[i].SetStatus(MemberStatus.STATUS_DELETED);
            switch (mMembers[i].Status())
            {
                case MemberStatus.STATUS_NOT_LANDED: mNotLanded++; break;
                case MemberStatus.STATUS_LANDED: mLanded++; break;
                case MemberStatus.STATUS_REPAIRED: mRepaired++; break;
                case MemberStatus.STATUS_DELETED: mDeleted++; break;
            }
        }
        Debug.Log(this);
    }
    public void InitMembers()
    {
        for (int i = 0; i < mpStdGroup.GetGhostCount(); ++i)
        {
            AiUnit ai = mpStdGroup.GetAiUnit((uint)i);

            MemberStatus mb = new MemberStatus(ai.UnitData().Number);
            mb.SetStatus(ai.GetAI() != null ? MemberStatus.STATUS_NOT_LANDED : MemberStatus.STATUS_LANDED);
            mMembers.New(new MemberStatus(ai.UnitData().Number));
        }
    }

    public ParkAction() { mpBaseName = null; }
    ~ParkAction()
    {
        if (mpBaseName != null)
            mpBaseName = null;
    }
    public virtual bool Initialize(IGroupAi grp, string base_name, DWORD ultimate)
    {
        bool ret = base.Initialize(grp, 0, null);
        if (ret)
        {
            mUltimate = ultimate;
            //mpBaseName = StrDup(base_name);
            mpBaseName = base_name;
            mBaseId = Hasher.HshString(mpBaseName);
            InitMembers();
            mpDynGroup.ParkGroup(mBaseId, mUltimate != DisappearCodes.DISAPPEAR_DEATH, mUltimate == DisappearCodes.DISAPPEAR_REPAIR);
        }
        return ret;
    }

    // API
    public override bool IsDeleteOnPush()
    {
        return true;
    }

    public override ActionStatus Update(float scale)
    {
        //Debug.Log(string.Format("Parking {0} point left: {1} ghosts: {2}",mpGroup.GetGroupData().Callsign, mPoints.Count, mpStdGroup.GetGhostCount()));
        ActionStatus status = GetAliveStatus();
        if (mpStdGroup.GetGhostCount() == 0)
        {
            status.GroupDead();
            return status;
        }
        else if (mPoints.Count() != 0)
            status = base.Update(scale);
        if (mpStdGroup.isLeaderChanged())
        {
            UpdateMembers();
            switch (mUltimate)
            {
                case DisappearCodes.DISAPPEAR_LAND: if (mNotLanded == 0) status.ActionDead(); break;
                case DisappearCodes.DISAPPEAR_REPAIR: if (mRepaired == mpStdGroup.GetReqGhostCount()) status.ActionDead(); break;
            }
        }
        return status;
    }
    public override bool IsCanBeBreaked()
    {
        return false;
    }
}


class MemberStatus
{
    public const uint STATUS_UNKNOWN = 0;
    public const uint STATUS_NOT_LANDED = 1;
    public const uint STATUS_LANDED = 2;
    public const uint STATUS_REPAIRED = 3;
    public const uint STATUS_DELETED = 4;

    DWORD mNumber;
    DWORD mStatus;
    public MemberStatus()
    {
        mNumber = Constants.THANDLE_INVALID;
        mStatus = STATUS_UNKNOWN;
    }
    public MemberStatus(DWORD numb)
    {
        mNumber = numb;
        mStatus = STATUS_UNKNOWN;
    }
    public DWORD Number() { return mNumber; }
    public DWORD Status() { return mStatus; }
    public void SetStatus(DWORD stat) { mStatus = stat; }
};