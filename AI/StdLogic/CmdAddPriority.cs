using crc32 = System.UInt32;

public class CmdAddPriority : BaseGroupCommand
{// BaseCommand
    public override bool exec()
    {
        myGroupService.setPriority(myGroup, myCoeff);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descString (myparams,"Group",myGroup);
        myparams = descFloat (myparams,"Coeff",myCoeff);
        return myparams;
    }

    // BaseCommand
    public override bool isParsingCorrect()
    {
        return myGroup!=null;
    }

    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Group: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, (int)IParamList.OpType.OP_EQU);
            case prm_Coeff: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, (int)IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Group: myGroup = p.myString; break;
            case prm_Coeff: myCoeff = p.myFloat; break;
            default: return false;
        }
        return true;
    }

    string myGroup; const crc32 prm_Group = 0x53FE943E;
    float myCoeff; const crc32 prm_Coeff = 0x9CA1CCA9;
    public override bool setDefaults(IQuery tm)
    {
        myCoeff = 0f;
        return base.setDefaults(tm);
    }
};
