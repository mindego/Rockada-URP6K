using crc32 = System.UInt32;

public class CmdSwitch : BaseDynGroupCommand
{
    public override  bool exec()
    {
        myDynGroupService.switchPoint(myNum);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descInt (myparams,"Num",myNum);
        return myparams;
    }

    // BaseCommand
    public override  bool isParsingCorrect()
    {
        return isIntValid(myNum);
    }

    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Num: return new IParamList.PInfo(IParamList.VarType.SPT_INT, IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Num: myNum = p.myInt; break;
            default: return false;
        }
        return true;
    }

    int myNum; const crc32 prm_Num = 0x1BF11671;

    public override bool setDefaults(IQuery tm)
    {
        myNum= INT_NULL();
        return base.setDefaults(tm);
    }
};