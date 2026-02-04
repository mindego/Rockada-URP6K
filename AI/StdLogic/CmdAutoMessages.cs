using crc32 = System.UInt32;

public class CmdAutoMessages : BaseGroupCommand
{// BaseCommand
    public override bool exec()
    {
        myGroupService.setAutoMessages(myUnit, myGroup, myBase);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descInt (myparams,"Unit",myUnit);
        myparams = descInt (myparams,"Group",myGroup);
        myparams = descInt (myparams,"Base",myBase);
        return myparams;
    }

    // BaseCommand
    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Unit: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int)IParamList.OpType.OP_EQU);
            case prm_Group: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int)IParamList.OpType.OP_EQU);
            case prm_Base: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int)IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Unit: myUnit = p.myInt; break;
            case prm_Group: myGroup = p.myInt; break;
            case prm_Base: myBase = p.myInt; break;
            default: return false;
        }
        return true;
    }

    //TODO проверить, возможно, правильнее uint
    int myUnit; const crc32 prm_Unit = 0x83765C92;
    int myGroup; const crc32 prm_Group = 0x53FE943E;
    int myBase; const crc32 prm_Base = 0x9F79AEA0;

    public override bool setDefaults(IQuery tm)
    {
        myUnit =  0;
        myGroup = 0;
        myBase = 1;
        return base.setDefaults(tm);
    }


};