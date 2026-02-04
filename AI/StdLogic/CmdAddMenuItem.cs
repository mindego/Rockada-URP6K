using static IParamList;
using crc32 = System.UInt32;

public class CmdAddMenuItem : BaseCommand
{
    // IVmCommand
    public override bool exec()
    {
        myMenu.addMenuItem(myName, myCaption, myParentItem);
        return true;
    }
    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        myparams = descString(myparams, "Name", myName);
        myparams = descString(myparams, "Caption", myCaption);
        myparams = descString(myparams, "ParentItem", myParentItem);
        return myparams;
    }

    public override bool isParsingCorrect()
    {
        return myName != null;
    }

    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Name: return new PInfo(VarType.SPT_STRING, OpType.OP_EQU);
            case prm_Caption: return new PInfo(VarType.SPT_STRING, OpType.OP_EQU);
            case prm_ParentItem: return new PInfo(VarType.SPT_STRING, OpType.OP_EQU);
        }
        return new PInfo();
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Name: myName = p.myString; break;
            case prm_Caption: myCaption = p.myString; break;
            case prm_ParentItem: myParentItem = p.myString; break;
            default: return false;
        }
        return true;
    }

    string myName; const crc32 prm_Name = 0x01EE2EC7;
    string myCaption; const crc32 prm_Caption = 0xC63E8F06;
    string myParentItem; const crc32 prm_ParentItem = 0x79430C90;

    public override bool setDefaults(IQuery tm)
    {
        myMenu = (IMenuService)tm.Query(IMenuService.ID);
        myName = null;
        //SET_DEF(Name, STR_NULL());
        return myMenu != null;
    }
    IMenuService myMenu;
};
