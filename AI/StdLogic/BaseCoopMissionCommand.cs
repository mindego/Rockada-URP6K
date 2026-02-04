public abstract class BaseCoopMissionCommand : BaseCommand
{

    public override bool setDefaults(IQuery tm)
    {
        myData = (ICoopMission) tm.Query(ICoopMission.ID);
        return myData!=null;
    }

    protected ICoopMission myData;
};