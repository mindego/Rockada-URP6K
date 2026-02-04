using crc32 = System.UInt32;
using static IParamList;

public class CmdQuit : BaseCommand
{
    // IVmCommand
    public override bool exec()
    {
        myMission.quitMission();
        return true;
    }
    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        return myparams;
    }

    public override PInfo getParamInfo(crc32 param_name)
    {
        return new PInfo();
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        return false;
    }

    public override  bool setDefaults(IQuery tm)
    {
        myMission = (IMissionService)tm.Query(IMissionService.ID);
        return myMission!=null;

    }
    IMissionService myMission;
};