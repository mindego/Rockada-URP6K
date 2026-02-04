using UnityEngine;
using static IParamList;
using crc32 = System.UInt32;

public class CmdAppearTime : BaseCommand
{
    public override bool exec()
    {
        myGame.setAppearTime(myGroup, myTime);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descString(myparams, "Group", myGroup);
        myparams = descFloat(myparams, "Time", myTime);
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
            case prm_Time: return new PInfo(VarType.SPT_FLOAT, OpType.OP_EQU);
        }
        return new PInfo();
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Group: myGroup = p.myString.Trim('\"').Trim('\''); break;
            case prm_Time: myTime = p.myFloat; break;
            default: return false;
        }
        return true;
    }

    string myGroup; const crc32 prm_Group = 0x53FE943E;
    float myTime; const crc32 prm_Time = 0x3059C884;


    public override bool setDefaults(IQuery tm)
    {
        myGame = (IInstantMission) tm.Query(IInstantMission.ID);
        //SetDef(myTime, 10.0f);
        myTime = 10.0f;
        return myGame!=null;
    }

    IInstantMission myGame;
};
