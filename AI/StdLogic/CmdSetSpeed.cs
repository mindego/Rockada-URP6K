using crc32 = System.UInt32;

public class CmdSetSpeed : BaseDynGroupCommand
{// BaseCommand
    public override bool exec()
    {
        myDynGroupService.setSpeed(mySpeed);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descFloat (myparams,"Speed",mySpeed);
        return myparams;
    }

    // BaseCommand
    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Speed: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Speed: mySpeed = p.myFloat; break;
            default: return false;
        }
        return true;
    }

    float mySpeed; const crc32 prm_Speed = 0x31182E0D;


    public override bool setDefaults(IQuery tm)
    {
        if (base.setDefaults(tm))
        {
            mySpeed = 0f;
            return true;
        }
        return false;
    }
};