using crc32 = System.UInt32;
using static IParamList;

public class CmdForgetPlayer : BaseCoopMissionCommand
{
    public override bool exec()
    {
        myData.forgetPlayer(myForget!=0);
        return true;
    }

    public override string  describeParams(ref string myparams)
    {
        myparams = descInt (myparams,"Forget",myForget);
        return myparams;
    }

    // BaseCommand
    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Forget: return new PInfo(VarType.SPT_INT, OpType.OP_EQU);
        }
        return new PInfo();
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Forget: myForget = p.myInt; break;
            default: return false;
        }
        return true;
    }

    int myForget; const crc32 prm_Forget = 0x4F12C8C4;

    public override bool setDefaults(IQuery tm)
    {
        myForget = 0;
        return base.setDefaults(tm);
    }


};