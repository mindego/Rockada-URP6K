using crc32 = System.UInt32;

public class CmdRandomBounds : BaseGroupCommand
{
    public override bool exec()
    {
        myGroupService.setRandomBounds(myValue);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descFloat(myparams, "Value", myValue);
        return myparams;
    }

    // BaseCommand
    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Value: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, (int)IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Value:
                myValue = p.myFloat;
                if (myValue < 0) myValue = 0;
                else if (myValue > 100f) myValue = 100f;
                myValue *= 0.01f;
                break;
            default: return false;
        }
        return true;
    }

    float myValue;  const crc32 prm_Value = 0x234988CF;


    public override bool setDefaults(IQuery tm)
    {
        myValue = 10f;
        return base.setDefaults(tm);
    }


};