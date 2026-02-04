using static IParamList;
using crc32 = System.UInt32;

public class CmdDeleteMenuItem : BaseCommand
{
    // IVmCommand
    public override  bool exec()
    {
        myMenu.deleteMenuItem(Hasher.HshString(myName));
        return true;
    }
    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        myparams = descString (myparams,"Name",myName);
        return myparams;
    }

    public bool IsParsingCorrect()
    {
        return myName!=null;
    }

    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Name: return new PInfo(VarType.SPT_STRING, OpType.OP_EQU);
        }
        return new  PInfo();
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
        myMenu = (IMenuService) tm.Query(IMenuService.ID);
        return myMenu!=null;
    }
    IMenuService myMenu;
};
