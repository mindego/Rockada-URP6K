using crc32 = System.UInt32;

public class CmdResume : BaseDynGroupCommand
{
    public override bool exec()
    {
        myDynGroupService.resumeGroup();
        return true;
    }

    public override string  describeParams(ref string myparams)
    {
        return myparams;
    }

    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        return false;
    }
};