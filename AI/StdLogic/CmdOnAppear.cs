using UnityEngine;
using crc32 = System.UInt32;

public class CmdOnAppear : BaseControllerCommand
{
    // IVmCommand
    public override bool exec()
    {
        bool app = myGroup.isAppeared() && !myAppeared;
        if (app)
            myAppeared = true;
        return app;
    }

    public override string describeParams(ref string myparams)
    {
        return myparams;
    }

    // BaseControllerCommand
    public override crc32 getID()
    {
        return 0x6BF11DC8; // OnAppear
    }

    public override bool setDefaults(IQuery tm)
    {
        myGroup = (IGroupService) tm.Query(IGroupService.ID);
        myAppeared = false;
        Debug.Log(string.Format("MyGroup {0} of {1}",myGroup!=null? myGroup:"Failed",tm));
        return myGroup!=null;
    }

    bool myAppeared;
    IGroupService myGroup;
};