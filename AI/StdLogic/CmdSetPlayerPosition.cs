using crc32 = System.UInt32;
using static IParamList;

public class  CmdSetPlayerPosition : BaseCoopMissionCommand
{// BaseCommand
    public override  bool exec()
    {
        myData.setPlayerPosition(myPosition);
        return true;
    }

    public override bool isParsingCorrect()
    {
        return isIntValid(myPosition);
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descInt (myparams,"Position",myPosition);
        return myparams;
    }

    // BaseCommand
    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Position: return new PInfo( VarType.SPT_INT, OpType.OP_EQU);
        }
        return new PInfo();
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Position: myPosition = p.myInt; break;
            default: return false;
        }
        return true;
    }

    public override bool setDefaults(IQuery tm)
    {
        myPosition = -1;
        return base.setDefaults(tm);
    }

    int myPosition; const crc32 prm_Position = 0x40A5795C;
};
