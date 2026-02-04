using UnityEngine;
using crc32 = System.UInt32;
using static IParamList;

class CmdAppear : BaseGroupCommand
{// BaseCommand
    public override bool exec()
    {
        myGroupService.appear(myScramble, myBase);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descInt (myparams,"Scramble",myScramble);
        myparams = descString (myparams,"Base",myBase);
        
        return myparams;
    }

    // BaseCommand
    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Scramble:
                return new PInfo(VarType.SPT_INT, (int)OpType.OP_EQU);
            case prm_Base:
                return new PInfo(VarType.SPT_STRING, (int)OpType.OP_EQU);
        }
        return new PInfo();
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Scramble: myScramble = p.myInt; break;
            //case prm_Base: myBase = p.myString; break;
            //case prm_Base: myBase = p.myString.Replace("\"",""); break; // удаляем кавычки
            case prm_Base: myBase = p.myString.Replace("\"", null); break; // удаляем кавычки
        }
        return true;
    }

    int myScramble; const crc32 prm_Scramble = 0x6F1DF110;
    string myBase; const crc32 prm_Base = 0x9F79AEA0;
    
    public override bool setDefaults(IQuery tm)
    {
        myScramble = 1;
        return base.setDefaults(tm);
    }
};

