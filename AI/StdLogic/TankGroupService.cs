public class TankGroupService<TeamImp> : ITankGroupService where TeamImp : StdTankGroupAi
{
    public virtual void setFightMode(int mode)
    {
        myMsn.SetFightMode((uint)mode);
    }

    public virtual void setUseRoads(int mode)
    {
        myMsn.SetUseRoadsMode((uint)mode);
    }

    public TankGroupService(TeamImp imp)
    {
        myMsn = imp;
    }
    TeamImp myMsn;
};