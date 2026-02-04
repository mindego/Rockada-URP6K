#define _DEBUG
using UnityEngine;

public class InstantMission<TeamImp> : IInstantMission where TeamImp : InstantActionAi
{
    public void setAppearTime(string myGroup, float myTime)
    {
        myMsn.AddAppearTime(myTime, myGroup);
    }

    public InstantMission(TeamImp imp)
    {
        myMsn = imp;
    }
    TeamImp myMsn;
};
