using crc32 = System.UInt32;

public class CmdReachRadius : BaseDynGroupCommand
{
    public override  bool exec()
    {
        myDynGroupService.setReachRadius(myRadius);
        return true;
    }

    public override string  describeParams(ref string myparams)
    {
        myparams = descFloat (myparams,"Radius",myRadius);
        return myparams;
    }

    // BaseCommand
    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Radius: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Radius: myRadius = p.myFloat; break;
            default: return false;
        }
        return true;
    }

    float myRadius; const crc32 prm_Radius = 0xC32F9493;

    public override bool setDefaults(IQuery tm)
    {
        myRadius =  100f;
        return base.setDefaults(tm);
    }


};

