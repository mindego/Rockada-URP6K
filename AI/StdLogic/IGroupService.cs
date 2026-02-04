using UnityEngine;
using DWORD = System.UInt32;

public interface IGroupService
{
    public const uint ID = 0xADE2A3B4;
    public bool getLeaderOrg(out Vector3 v);
    public bool getPlayerOrg(out Vector3 v);
    public bool isAppeared();
    public void setPriority(string group, float coeff);
    public void appear(int scramble, string baseName   );
    public void setAttackRadius(float radius);
    public void setAttackCoeff(float base_coeff, bool clear);
    public void setAutoMessages(int myUnit, int myGroup, int baseId);
    public void changeSide(int side);
    public void disappear(string baseName, float dist, DWORD ultimate);
    public void setIncludeSubobjects(int include);
    public void setRandomBounds(float percent);
    public void setShowCallsign(int enable);
    public void suicide(int units_count, DWORD[] units, int phys);
};

