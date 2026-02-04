using crc32 = System.UInt32;

public class  CmdAttackRadius : BaseGroupCommand
{// BaseCommand
    public override  bool exec()
    {
        myGroupService.setAttackRadius(myRadius);
        if (isFloatValid(myCoeff))
        {
            if (myCoeff < 0) myCoeff = 0;
            myGroupService.setAttackCoeff(myCoeff, myClear!=0);
        }
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descFloat(myparams, "Radius", myRadius);
        myparams = descFloat(myparams, "Coeff", myCoeff);
        myparams = descInt(myparams, "Clear", myClear);
        return myparams;
    }

    // BaseCommand
    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Radius: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, (int)IParamList.OpType.OP_EQU);
            case prm_Coeff: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, (int)IParamList.OpType.OP_EQU);
            case prm_Clear: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int)IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Radius: myRadius = p.myFloat; break;
            case prm_Coeff: myCoeff = p.myFloat; break;
            case prm_Clear: myClear = p.myInt; break;
            default: return false;
        }
        return true;
    }

    //ADD_FLOAT(Radius,   0xC32F9493);
    //ADD_FLOAT(Coeff,   0x9CA1CCA9);
    //ADD_INT(Clear,   0xDB8F21FD);
    float myRadius; const crc32 prm_Radius = 0xC32F9493;
    float myCoeff;  const crc32 prm_Coeff = 0x9CA1CCA9;
    int myClear;  const crc32 prm_Clear = 0xDB8F21FD;
    public override bool setDefaults(IQuery tm)
    {
        myRadius =  3000f;
        myCoeff = float.NaN;
        myClear = 0;
        return base.setDefaults(tm);
    }
};