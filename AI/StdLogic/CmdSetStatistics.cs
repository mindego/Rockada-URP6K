using crc32 = System.UInt32;
using static IParamList;

public class CmdSetStatistics : BaseCoopMissionCommand
{// BaseCommand
    public override  bool exec()
    {
        myData.setStatistics(myEnable!=0);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
       myparams = descInt (myparams,"Enable",myEnable);
        return myparams;
    }

    // BaseCommand
    public override bool isParsingCorrect()
    {
        return isIntValid(myEnable);
    }
    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Enable: return new PInfo(VarType.SPT_INT, OpType.OP_EQU);
        }
        return new PInfo();
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string  real_name)
    {
        switch (param_name)
        {
            case prm_Enable: myEnable = p.myInt; break;
            default: return false;
        }
        return true;
    }

    int myEnable; const crc32 prm_Enable = 0x0BF332FB;

    public override  bool setDefaults(IQuery tm)
    {
        myEnable = 0;
        return base.setDefaults(tm);
    }


};