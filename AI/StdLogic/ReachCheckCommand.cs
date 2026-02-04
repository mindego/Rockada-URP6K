using UnityEngine;

public abstract class ReachCheckCommand : BaseControllerCommand
{
    // IVmCommand
    public override bool exec()
    {
        Vector3 leader_org;
        if (myGroupService.getLeaderOrg(out leader_org))
        {
            if (checkVector(leader_org))
            {
                return true;
            }
            else
            {
                if (myGroupService.getPlayerOrg(out leader_org) && checkVector(leader_org))
                    return true;
            }
        }
        return false;
    }

    public abstract bool checkVector(Vector3 org);

    public override bool setDefaults(IQuery tm)
    {
        myContext = (IPointExecutionContext) tm.Query(IPointExecutionContext.ID);
        myGroupService = (IGroupService) tm.Query(IGroupService.ID);
        myDynGroupService = (IDynGroupService) tm.Query(IDynGroupService.ID);
        return myContext!=null && myGroupService!=null && myDynGroupService!=null;
    }

    protected IGroupService myGroupService;
    protected IPointExecutionContext myContext;
    protected IDynGroupService myDynGroupService;
};