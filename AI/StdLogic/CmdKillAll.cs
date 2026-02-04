using crc32 = System.UInt32;

public class CmdKillAll : BaseDynGroupCommand
{
    public override bool exec()
    {
        myDynGroupService.killAll(myFlag!=0);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descInt (myparams,"Flag",myFlag);
        return myparams;
    }

    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Flag: return new IParamList.PInfo(IParamList.VarType.SPT_INT, IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Flag: myFlag = p.myInt; break;
            default: return false;
        }
        return true;
    }

    int myFlag;  const crc32 prm_Flag = 0x8E39BB5B;


    public override bool setDefaults(IQuery tm)
    {
        myFlag =  0;
        return base.setDefaults(tm);
    }


}