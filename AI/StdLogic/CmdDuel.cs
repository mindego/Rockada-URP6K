using static IParamList;
using crc32 = System.UInt32;

public class CmdDuel : BaseCraftGroupCommand
{
    public override bool exec()
    {
        myCraftGroupService.duel(myEnable != 0, myFightTime, myFightTimeDisp, myIdleTime, myIdleTimeDisp);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descInt(myparams, "Enable", myEnable);
        myparams = descFloat(myparams, "FightTime", myFightTime);
        myparams = descFloat(myparams, "FightTimeDisp", myFightTimeDisp);
        myparams = descFloat(myparams, "IdleTime", myIdleTime);
        myparams = descFloat(myparams, "IdleTimeDisp", myIdleTimeDisp);
        return myparams;
    }

    // BaseCommand
    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Enable: return new PInfo(VarType.SPT_INT, OpType.OP_EQU);
            case prm_FightTime: return new PInfo(VarType.SPT_FLOAT, OpType.OP_EQU);
            case prm_FightTimeDisp: return new PInfo(VarType.SPT_FLOAT, OpType.OP_EQU);
            case prm_IdleTime: return new PInfo(VarType.SPT_FLOAT, OpType.OP_EQU);
            case prm_IdleTimeDisp: return new PInfo(VarType.SPT_FLOAT, OpType.OP_EQU);
        }
        return new PInfo();
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Enable: myEnable = p.myInt; break;
            case prm_FightTime: myFightTime = p.myFloat; break;
            case prm_FightTimeDisp: myFightTimeDisp = p.myFloat; break;
            case prm_IdleTime: myIdleTime = p.myFloat; break;
            case prm_IdleTimeDisp: myIdleTimeDisp = p.myFloat; break;
            default: return false;
        }
        return true;
    }

    int myEnable; const crc32 prm_Enable = 0x0BF332FB;
    float myFightTime; const crc32 prm_FightTime = 0x33DC7B78;
    float myFightTimeDisp; const crc32 prm_FightTimeDisp = 0x6315800B;
    float myIdleTime; const crc32 prm_IdleTime = 0x13F04EDD;
    float myIdleTimeDisp; const crc32 prm_IdleTimeDisp = 0x395F0521;


    public override bool setDefaults(IQuery tm)
    {
        if (base.setDefaults(tm))
        {
            myEnable = 1;
            myFightTime = 50f;
            myFightTimeDisp = 15f;
            myIdleTime = 50f;
            myIdleTimeDisp = 15f;
            return true;
        }
        return false;
    }


};
