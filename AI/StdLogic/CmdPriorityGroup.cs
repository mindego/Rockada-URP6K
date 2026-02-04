using System.Collections.Generic;
using crc32 = System.UInt32;

public class CmdPriorityGroup : BaseGroupCommand
{// BaseCommand
    public override bool exec()
    {
        for (int i = 0; i < myGroup.Count; ++i)
            myGroupService.setPriority(myGroup[i] /*->getPtr()*/, 2f);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descStrings (myparams,"Group",myGroup);
        return myparams;
    }

    // BaseCommand
    public override bool isParsingCorrect()
    {
        return myGroup.Count!=0;
    }

    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Group: return new IParamList.PInfo(IParamList.VarType.SPT_STRING, (int)IParamList.OpType.OP_EQU);
        }
        return new IParamList.PInfo();
    }

    public override  bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Group: myGroup.Add(p.myString); break;
            default: return false;
        }
        return true;
    }

    List<string> myGroup = new List<string>() ; const crc32 prm_Group = 0x53FE943E;
};