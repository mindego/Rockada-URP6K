using DWORD = System.UInt32;
using UnityEngine;

public interface IMissionService
{
    public const uint ID = 0x07910F96;
    public void quitMission();
    public void setCamera(DWORD mode, Vector3 org, float angle1, float angle2, float angle3, string group, DWORD index);
    public void setGodness(string group, int unit, bool can_die);
};

public class MissionService<TeamImp> : IMissionService where TeamImp : StdMissionAi
{
    public void quitMission()
    {
        myMsn.quitMission();
    }
    public void setCamera(DWORD mode, Vector3 org, float angle1, float angle2, float angle3, string group, DWORD index)
    {
        myMsn.setCamera(mode, org, angle1, angle2, angle3, group, index);
    }

    public void setGodness(string group, int unit, bool can_die)
    {
        myMsn.SetGodness(group, (uint)unit, !can_die);
    }

    public MissionService(TeamImp imp) { myMsn = imp; }

    TeamImp myMsn;
};
