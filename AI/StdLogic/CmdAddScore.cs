using crc32 = System.UInt32;
using static IParamList;

public class CmdAddScore : BaseCoopMissionCommand
{
    public override bool exec()
    {
        myData.addScore(myScore);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descInt (myparams,"Score",myScore);
        return myparams;
    }

    // BaseCommand
    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Score: return new PInfo(VarType.SPT_INT, OpType.OP_EQU);
        }
        return new PInfo();
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Score: myScore = p.myInt; break;
            default: return false;
        }
        return true;
    }

    int myScore; const crc32 prm_Score = 0x0CA7E7AA;

    public override bool setDefaults(IQuery tm)
    {
        myScore =0;
        return base.setDefaults(tm);
    }
};