public abstract class BaseCraftGroupCommand : BaseCommand
{
    public override  bool setDefaults(IQuery tm)
    {
        myCraftGroupService = (ICraftGroupService) tm.Query(ICraftGroupService.ID);
        return myCraftGroupService!=null;
    }
    protected ICraftGroupService myCraftGroupService;
};

