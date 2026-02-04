using System.Collections.Generic;
using crc32 = System.UInt32;
using DWORD = System.UInt32;

public class CmdSuicide : BaseGroupCommand
{// BaseCommand
    public override bool exec()
    {
        DWORD[] units =new DWORD[0];
        if (myUnit.Count!=0)
        {
            units = new DWORD[myUnit.Count];
            for (int i = 0; i < myUnit.Count; ++i)
                units[i] = myUnit[i];
        }
        myGroupService.suicide(myUnit.Count, units, myPhysically);
        //SafeDelete(units);
        units = null;
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descInts (myparams,"Unit",myUnit);
        myparams = descInt (myparams,"Physically",myPhysically);
        return myparams;
    }

    // BaseCommand
    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Unit: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int) IParamList.OpType.OP_EQU);
            case prm_Physically: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int)IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Unit: myUnit.Add((uint)p.myInt); break;
            case prm_Physically: myPhysically = p.myInt; break;
            default: return false;
        }
        return true;
    }

    List<uint> myUnit = new List<uint>(); const crc32 prm_Unit = 0x83765C92;
    int myPhysically; const crc32 prm_Physically = 0xFFEA6E6E;

    public override bool setDefaults(IQuery tm)
    {
        myPhysically = 1;
        return base.setDefaults(tm);
    }


};