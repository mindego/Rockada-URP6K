using crc32 = System.UInt32;
using static IParamList;

public class CmdAddObjective : BaseCoopMissionCommand
{
    public override bool exec()
    {
        myData.addObjective(myName, (uint) myPrimary);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descString (myparams,"Name",myName);
        myparams = descInt (myparams,"Primary",myPrimary);
        return myparams;
    }

    public override bool isParsingCorrect()
    {
        return myName!=null;
    }

    public override  PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Name: return new PInfo(VarType.SPT_STRING, OpType.OP_EQU);
            case prm_Primary: return new PInfo(VarType.SPT_INT, OpType.OP_EQU);
        }
        return new PInfo();
    }

    public override  bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Name: myName = p.myString; break;
            case prm_Primary: myPrimary = p.myInt; break;
            default: return false;
        }
        return true;
    }

    string myName; const crc32 prm_Name = 0x01EE2EC7;
    int myPrimary; const crc32 prm_Primary = 0x27072CE5;

    public override  bool setDefaults(IQuery tm)
    {
        myPrimary = 0;
        return base.setDefaults(tm);
    }
};
