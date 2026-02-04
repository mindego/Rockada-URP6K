using crc32 = System.UInt32;
using static IParamList;
using System.Collections.Generic;

public class CmdOnGroupDeath : RadioHandlerCommand
{
    // IVmCommand
    public override bool checkMessage(RadioMessage msg)
    {
        if (msg.Code == 0x13FC0920)
        { // OnGroupKill
            crc32 code = Hasher.HshString(msg.CallerCallsign);
            for (int j = 0; j < myHashCodes.Count(); ++j)
                if (myHashCodes[j] == code)
                    return setStarted(true);
        }
        return false;
    }
    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        myparams = descStrings (myparams,"Group",myGroup);
        return myparams;
    }

    public override bool isParsingCorrect()
    {
        return myGroup.Count!=0 && base.isParsingCorrect();
    }

    // BaseControllerCommand
    public override crc32 getID()
    {
        crc32 code = 0x3A5116C8;
        for (int i = 0; i < myGroup.Count; ++i)
            //code = Crc32.Code(code, myGroup[i]);
            code = Hasher.HshString("OnGroupDeath" + myGroup[i]);
        return code;
    }


    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Group: return new PInfo(VarType.SPT_STRING, OpType.OP_EQU);
            default: return base.getParamInfo(param_name);
        }
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Group:
                myGroup.Add(p.myString);
                myHashCodes.New(Hasher.HshString(p.myString));
                break;
            default: return base.addParameter(param_name, op, p, real_name);
        }
        return true;
    }

    List<string> myGroup = new List<string>(); const crc32 prm_Group = 0x53FE943E;

    Tab<crc32> myHashCodes;
};