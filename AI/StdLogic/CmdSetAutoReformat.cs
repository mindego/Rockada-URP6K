using crc32 = System.UInt32;

public class CmdSetAutoReformat : BaseDynGroupCommand
{
    public override bool exec()
    {
        myDynGroupService.setAutoReformat(myEnable!=0);
        return true;
    }

    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        myparams = descInt (myparams,"Enable",myEnable);
        return myparams;
    }

    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Enable: return new IParamList.PInfo(IParamList.VarType.SPT_INT, IParamList.OpType.OP_EQU);
            default: return new IParamList.PInfo();
        }
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Enable: myEnable = p.myInt; break;
        }
        return false;
    }

    int myEnable;  const crc32 prm_Enable = 0x0BF332FB;

    public override bool setDefaults(IQuery tm)
    {
        myEnable = 0;
        return base.setDefaults(tm);
    }

};

