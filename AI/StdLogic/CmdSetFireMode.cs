using crc32 = System.UInt32;

public class CmdSetFireMode : BaseCommand
{
    public override bool exec()
    {
        myUnitService.setFireMode(myEnable!=0);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descInt (myparams,"Enable",myEnable);
        return myparams;
    }

    // BaseCommand
    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Enable: return new IParamList.PInfo(IParamList.VarType.SPT_INT, IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string  real_name)
    {
        switch (param_name)
        {
            case prm_Enable: myEnable = p.myInt; break;
            default: return false;
        }
        return true;
    }

    int myEnable; const crc32 prm_Enable = 0x0BF332FB;

    public override  bool setDefaults(IQuery tm)
    {
        myUnitService = (IUnitService) tm.Query(IUnitService.ID);
        myEnable =  1;
        return myUnitService!=null;
    }

    IUnitService myUnitService;
};
