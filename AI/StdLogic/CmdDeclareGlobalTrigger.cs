using crc32 = System.UInt32;
using static IParamList;

public class CmdDeclareGlobalTrigger : BaseCommand
{
    // IVmCommand
    public override bool exec()
    {
        int val;
        myTriggers.getTrigger(myName, out val);
        myTriggers.setTrigger(myName, val, true);
        return true;
    }

    public override bool isParsingCorrect()
    {
        return myName!=null;
    }

    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        myparams = descString (myparams,"Name",myName);
        return myparams;
    }

    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Name: return new PInfo(VarType.SPT_STRING, OpType.OP_EQU);
            default: return new PInfo();
        }
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Name: myName = p.myString; break;
            default: return false;
        }
        return true;
    }

    string myName;  const crc32 prm_Name = 0x01EE2EC7;

    public override bool setDefaults(IQuery tm)
    {
        myTriggers = (ITriggerService)tm.Query(ITriggerService.ID);
        myName = null;
        return myTriggers!=null;
    }

    ITriggerService myTriggers;
};