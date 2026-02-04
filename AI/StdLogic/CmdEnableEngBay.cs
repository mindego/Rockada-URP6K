using crc32 = System.UInt32;
using static IParamList;

public class CmdEnableEngBay : BaseCoopMissionCommand
{
    public override bool exec()
    {
        myData.enableEngBay(myFlag);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descInt (myparams,"Flag",myFlag);
        return myparams;
    }

    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Flag: return new PInfo(VarType.SPT_INT, OpType.OP_EQU);
        }
        return new PInfo();
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Flag: myFlag = p.myInt; break;
            default: return false;
        }
        return true;
    }

    public override bool setDefaults(IQuery tm)
    {
        myFlag = 1;
        return base.setDefaults(tm);
    }

    int myFlag; const crc32 prm_Flag = 0x8E39BB5B;
};