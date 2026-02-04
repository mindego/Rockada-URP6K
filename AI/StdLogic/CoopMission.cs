using DWORD = System.UInt32;

struct CoopMission<TeamImp> : ICoopMission where TeamImp : StdCooperativeAi
{

    public void addObjective(string name, DWORD primary)
    {
        myMsn.AddObjective(name, primary!=0);
    }

    public void addScore(int score)
    {
        myMsn.IncScore(score);
    }

    public void deleteObjective(string name)
    {
        myMsn.RemoveObjective(name);
    }

    public void setObjective(string name, DWORD success, DWORD primary)
    {
        myMsn.SetObjective(name, success, primary);
    }

    public void setStatistics(bool enable)
    {
        myMsn.SetCountStatisticsMode(enable);
    }

    public void addAward(string name)
    {
        myMsn.AddAward(name);
    }

    public void setPlayerPosition(int pos)
    {
        myMsn.setPlayerPosition(pos - 1);
    }

    public void setPlayable(string group)
    {
        myMsn.SetPlayable(group);
    }

    public void enableEngBay(int enable)
    {
        myMsn.enableEngbay(enable);
    }

    public void forgetPlayer(bool forget)
    {
        myMsn.ForgetPlayers(forget);
    }

    public void setGodMode(DWORD d1, DWORD d2, DWORD d3, DWORD d4)
    {
        myMsn.setGodMode(d1, d2, d3, d4);
    }

    public void setAccess(bool craft, string name, ICoopMission.TechAccess access)
    {
        if (craft)
            myMsn.setCraftAccess(name, access);
        else
            myMsn.setWeaponAccess(name, access);
    }
    public CoopMission(TeamImp imp)
    {
        myMsn = imp;
    }
    TeamImp myMsn;
};