using crc32 = System.UInt32;

public class CmdPause : BaseDynGroupCommand
{
    public override bool exec()
    {
        myDynGroupService.pauseGroup(myTime);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descFloat (myparams,"Time",myTime);
        return myparams;
    }

    // BaseCommand
    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Time: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Time: myTime = p.myFloat; break;
            default: return false;
        }
        return true;
    }

    float myTime; const crc32 prm_Time = 0x3059C884;

    public override bool setDefaults(IQuery tm)
    {
        myTime = 0f;
        return base.setDefaults(tm);
    }
};