using crc32 = System.UInt32;

public class CmdAddDamage : BaseCommand
{
    public override  bool exec()
    {
        myUnitService.addDamage(myName, myCoeff);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descString (myparams,"Name",myName);
        myparams = descFloat (myparams,"Coeff",myCoeff);
        return myparams;
    }

    // BaseCommand
    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Name: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, (int) IParamList.OpType.OP_EQU);
            case prm_Coeff: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, (int) IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Name: myName = p.myString; break;
            case prm_Coeff: myCoeff = p.myFloat; break;
            default: return false;
        }
        return true;
    }

    string myName; const crc32 prm_Name = 0x01EE2EC7;
    float myCoeff; const crc32 prm_Coeff = 0x9CA1CCA9;

    public override  bool setDefaults(IQuery tm)
    {
        myUnitService = (IUnitService) tm.Query(IUnitService.ID);
        myCoeff = 1f;
        return myUnitService!=null;
    }
    IUnitService myUnitService;
};
