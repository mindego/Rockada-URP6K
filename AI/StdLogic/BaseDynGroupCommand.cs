public abstract class BaseDynGroupCommand : BaseCommand
    {// BaseCommand
        public override bool setDefaults(IQuery tm)
        {
            myDynGroupService = (IDynGroupService)tm.Query(IDynGroupService.ID);
            return myDynGroupService != null;
        }
        protected IDynGroupService myDynGroupService;
    }