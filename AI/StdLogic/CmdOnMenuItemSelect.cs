using static IParamList;
using crc32 = System.UInt32;

public class CmdOnMenuItemSelect : BaseControllerCommand, IMenuHandler
{
    // IVmCommand
    public override bool exec()
    {
        return getSelected();
    }

    bool getSelected()
    {
        bool ret = myIsSelected;
        myIsSelected = false;
        return ret;
    }

    public virtual bool notifySelect(crc32 code)
    {
        return myIsSelected = (myHashCode == code);
    }

    ~CmdOnMenuItemSelect()
    {
        registerMe(false);
    }

    public override void onOverride()
    {
        registerMe(false);
    }

    // BaseCommand
    public override string describeParams(ref string myparams)
    {
        myparams = descString(myparams, "Name", myName);
        return myparams;
    }

    public override bool isParsingCorrect()
    {
        return isIntValid(myHashCode) && base.isParsingCorrect();
    }

    public override bool restart()
    {
        myHashCode = Hasher.HshString(myName);
        registerMe(true);
        return base.restart();
    }

    // BaseControllerCommand
    public override crc32 getID()
    {
        //return Crc32.Code(0xF702A4A0, myName);
        //return Hasher.HshString(myName); //Это неправильно. Должно быть как-то иначе
        return Hasher.HshString("OnMenuItemSelect" + myName);
    }


    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Name: return new PInfo(VarType.SPT_STRING, OpType.OP_EQU);
            default: return base.getParamInfo(param_name);
        }
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Name: myName = p.myString; break;
            default: return base.addParameter(param_name, op, p, real_name);
        }
        return true;
    }

    string myName; const crc32 prm_Name = 0x01EE2EC7;

    public override bool setDefaults(IQuery tm)
    {
        if ((myMenu = (IMenuService)tm.Query(IMenuService.ID)) != null)
            myMenu.registerHandler(this);
        return myMenu != null;
    }
    bool myIsSelected = false;
    IMenuService myMenu;
    crc32 myHashCode;

    void registerMe(bool reg)
    {
        if (myMenu != null)
        {
            if (reg)
            {
                myMenu.registerHandler(this);

            }
            else
            {
                myMenu.unregisterHandler(this);
            };
        }
    }
};
