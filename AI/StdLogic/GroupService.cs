using UnityEngine;
using DWORD = System.UInt32;
using crc32 = System.UInt32;

public class GroupService<T> : IGroupService where T : StdGroupAi
{
    public virtual bool getLeaderOrg(out Vector3 org)
    {
        //org = Vector3.zero;
        return myMsn.getLeaderOrg(out org);
    }

    public virtual bool getPlayerOrg(out Vector3 org)
    {
        return myMsn.getPlayerOrg(out org);
    }

    public virtual bool isAppeared()
    {
        return myMsn.isAppeared();
    }
    public virtual void setPriority(string group, float coeff)
    {
        myMsn.AddPriority((int)Hasher.HshString(group), coeff);
    }

    public virtual void appear(int scramble, string baseName)
    {
        myMsn.SetGroupAppear(baseName, scramble != 0, true);
    }

    public virtual void setAttackRadius(float radius)
    {
        myMsn.setAttackRadius(radius);
    }
    public virtual void setAttackCoeff(float base_coeff, bool clear)
    {
        myMsn.setAttackCoeff(base_coeff, clear);
    }

    public virtual void setAutoMessages(int myUnit, int myGroup, int baseId)
    {
        myMsn.SetAutoMessages((uint)myGroup, (uint)myUnit, (uint)baseId);
    }

    public virtual void changeSide(int side)
    {
        myMsn.OnChangeSide((uint)side);
    }

    public virtual void disappear(string baseName, float dist, DWORD ultimate)
    {
        Debug.Log(string.Format("Trying to disappear @ base {0}", baseName == null ? "NO_BASE_SET":baseName));
        myMsn.SetGroupDisappear(baseName != null, Hasher.HshString(baseName), dist, ultimate, baseName);
    }

    public virtual void setIncludeSubobjects(int include)
    {
        myMsn.IncludeSubobjects((uint)include);
    }
    public virtual void setRandomBounds(float percent)
    {
        myMsn.setRandomBounds(percent);
    }

    public virtual void setShowCallsign(int enable)
    {
        myMsn.SetShowCallsign(enable != 0);
    }

    public virtual void suicide(int units_count, DWORD[] units, int phys)
    {
        myMsn.GroupSuicide(units_count, units, phys != 0);
    }

    public virtual void setSkill(DWORD name)
    {
        myMsn.SetSkill(name);
    }

    public GroupService(T imp) { myMsn = imp; }

    T myMsn;
};
