using crc32 = System.UInt32;
using static IParamList;

public abstract class BaseGlobalAccessCommand : BaseCoopMissionCommand
{
    public override bool isParsingCorrect()
    {
        return myName!=null && myAccess!=null && ICoopMission.getAccessFromCrc(Hasher.HshString(myAccess)) != ICoopMission.TechAccess.taUnknown;
    }

    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        myparams = descString (myparams,"Name",myName);
        myparams = descString (myparams,"Access",myAccess);
        return myparams;
    }

    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Name: return new  PInfo(VarType.SPT_STRING, OpType.OP_EQU);
            case prm_Access: return new PInfo(VarType.SPT_STRING, OpType.OP_EQU);
            default: return new PInfo();
        }
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Name: myName = p.myString; break;
            case prm_Access: myAccess = p.myString; break;
            default: return false;
        }
        return true;
    }

    protected string myName;  const crc32 prm_Name = 0x01EE2EC7;
    protected string myAccess;  const crc32 prm_Access = 0xFE3AD19D;

    public override bool setDefaults(IQuery tm)
    {
        myName = null;
        myAccess = null;
        return base.setDefaults(tm);
    }
};