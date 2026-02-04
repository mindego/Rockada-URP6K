public abstract class BaseGroupCommand : BaseCommand
{
    public override bool setDefaults(IQuery tm)
    {
        myGroupService = (IGroupService) tm.Query(IGroupService.ID);
        return myGroupService!=null;
    }

    protected IGroupService myGroupService;
};

