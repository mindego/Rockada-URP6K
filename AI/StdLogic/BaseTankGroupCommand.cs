public abstract class BaseTankGroupCommand : BaseCommand
{
    public override bool setDefaults(IQuery tm)
    {
        myTankGroupService = (ITankGroupService) tm.Query(ITankGroupService.ID);
        return myTankGroupService !=null;
    }
    protected ITankGroupService myTankGroupService;
};
