using crc32 = System.UInt32;
using static IParamList;

public class CmdSetPlayable : BaseCoopMissionCommand
{// BaseCommand
    public override  bool exec()
    {
        myData.setPlayable(myGroup);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descString (myparams,"Group",myGroup);
        return myparams;
    }

    // BaseCommand
    public override bool isParsingCorrect()
    {
        return myGroup!=null;
    }
    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Group: return new PInfo(VarType.SPT_STRING, OpType.OP_EQU);
        }
        return new PInfo();
    }

    public override  bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Group: myGroup = p.myString; break;
            default: return false;
        }
        return true;
    }

    string myGroup; const crc32 prm_Group = 0x53FE943E;
};