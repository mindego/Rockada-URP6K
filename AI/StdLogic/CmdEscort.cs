using System.Collections.Generic;
using crc32 = System.UInt32;

public class CmdEscort : BaseCraftGroupCommand
{// BaseCommand
    public override bool exec()
    {
        myCraftGroupService.escort(myDelta, myGroup);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descStrings (myparams,"Group",myGroup);
        myparams = descFloat (myparams,"Delta",myDelta);
        return myparams;
    }

    // BaseCommand
    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Group: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, IParamList.OpType.OP_EQU);
            case prm_Delta: return new IParamList.PInfo(IParamList.VarType.SPT_FLOAT, IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override  bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Group: myGroup.Add(p.myString); break;
            case prm_Delta: myDelta = p.myFloat; break;
            default: return false;
        }
        return true;
    }

    List<string> myGroup = new List<string>(); const crc32 prm_Group = 0x53FE943E;
    float myDelta; const crc32 prm_Delta = 0xA87D2E22;

    public override bool setDefaults(IQuery tm)
    {
        if (base.setDefaults(tm))
        {
            //SetDef(myDelta, 100f); 
            myDelta = 100f;
            return true;
        }
        return false;
    }
};

