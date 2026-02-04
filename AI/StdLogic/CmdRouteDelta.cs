using crc32 = System.UInt32;

public class CmdRouteDelta : BaseDynGroupCommand
{
    public override bool exec()
    {
        myDynGroupService.setRouteDelta(myDelta);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descFloat (myparams,"Delta",myDelta);
        return myparams;
    }

    // BaseCommand
    public override bool isParsingCorrect()
    {
        return myDelta >= -1;
    }

    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Delta: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Delta: myDelta = p.myFloat; break;
            default: return false;
        }
        return false;
    }

    float myDelta; const crc32 prm_Delta = 0xA87D2E22;


    public override bool setDefaults(IQuery tm)
    {
        myDelta = -1000f;
        return base.setDefaults(tm);
    }


};