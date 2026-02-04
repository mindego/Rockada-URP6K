using crc32 = System.UInt32;
using static IParamList;

public class StdMsnFactory : BaseFactory
{
    const string cmdname_SetGodness = "SetGodness"; const crc32 cmd_SetGodness = 0x3CFF55FE;
    const string cmdname_Delay = "Delay"; const crc32 cmd_Delay = 0x8CA45060;
    const string cmdname_Quit = "Quit"; const crc32 cmd_Quit = 0x1C678754;
    const string cmdname_SendMessage = "SendMessage"; const crc32 cmd_SendMessage = 0x668618E8;
    const string cmdname_SetCamera = "SetCamera"; const crc32 cmd_SetCamera = 0x5F778695;
    const string cmdname_SetSkill = "SetSkill"; const crc32 cmd_SetSkill = 0x5197C448;
    const string cmdname_NotifyAll = "NotifyAll"; const crc32 cmd_NotifyAll = 0xCDE0A8AD;
    const string cmdname_RandomCode = "RandomCode"; const crc32 cmd_RandomCode = 0x3E77C0AD;
    const string cmdname_SetTrigger = "SetTrigger"; const crc32 cmd_SetTrigger = 0x380218FC;
    const string cmdname_IncTrigger = "IncTrigger"; const crc32 cmd_IncTrigger = 0xFCA5AF32;
    const string cmdname_AddMenuItem = "AddMenuItem"; const crc32 cmd_AddMenuItem = 0xAC8A1C49;
    const string cmdname_DeleteMenuItem = "DeleteMenuItem"; const crc32 cmd_DeleteMenuItem = 0x76B78DB2;
    const string cmdname_DeclareGlobalTrigger = "DeclareGlobalTrigger"; const crc32 cmd_DeclareGlobalTrigger = 0x7B7A4DDC;

    const string cmdname_OnTimeExceed = "OnTimeExceed"; const crc32 cmd_OnTimeExceed = 0xB5CD309E;
    const string cmdname_OnMessage = "OnMessage"; const crc32 cmd_OnMessage = 0x39D04393;
    const string cmdname_OnGroupDeath = "OnGroupDeath"; const crc32 cmd_OnGroupDeath = 0x3A5116C8;
    const string cmdname_OnMenuItemSelect = "OnMenuItemSelect"; const crc32 cmd_OnMenuItemSelect = 0xF702A4A0;
    const string cmdname_OnTriggers = "OnTriggers"; const crc32 cmd_OnTriggers = 0x75FFA5E0;

    //const crc32 cmd_OnMessage = 0x39D04393;
    //const crc32 cmd_OnMenuItemSelect = 0xF702A4A0;
    //const crc32 cmd_OnGroupDeath = 0x3A5116C8;
    //const crc32 cmd_OnTriggers = 0x75FFA5E0;
    //const crc32 cmd_OnTimeExceed = 0xB5CD309E;

    public override IVmCommand createCommand(crc32 name, IVmVariablePool pool, IVmController cont)
    {
        Asserts.Assert(myPrevFactory == null);
        switch (name)
        {
            case cmd_SetGodness:
                return CommandsUtils.createScriptCommand<CmdSetGodness>(cmdname_SetGodness, myQuery, pool);
            case cmd_Delay:
                return CommandsUtils.createScriptCommand<CmdDelay>(cmdname_Delay, myQuery, pool);
            case cmd_Quit:
                return CommandsUtils.createScriptCommand<CmdQuit>(cmdname_Quit, myQuery, pool);
            case cmd_SendMessage:
                return CommandsUtils.createScriptCommand<CmdSendMessage>(cmdname_SendMessage, myQuery, pool);
            case cmd_SetCamera:
                return CommandsUtils.createScriptCommand<CmdSetCamera>(cmdname_SetCamera, myQuery, pool);
            case cmd_SetSkill:
                return CommandsUtils.createScriptCommand<CmdSetSkill>(cmdname_SetSkill, myQuery, pool);
            case cmd_NotifyAll:
                return CommandsUtils.createScriptCommand<CmdNotifyAll>(cmdname_NotifyAll, myQuery, pool);
            case cmd_RandomCode:
                return CommandsUtils.createScriptCommand<CmdRandomCode>(cmdname_RandomCode, myQuery, pool);
            case cmd_SetTrigger:
                return CommandsUtils.createScriptCommand<CmdSetTrigger>(cmdname_SetTrigger, myQuery, pool);
            case cmd_IncTrigger:
                return CommandsUtils.createScriptCommand<CmdIncTrigger>(cmdname_IncTrigger, myQuery, pool);
            case cmd_AddMenuItem:
                return CommandsUtils.createScriptCommand<CmdAddMenuItem>(cmdname_AddMenuItem, myQuery, pool);
            case cmd_DeleteMenuItem:
                return CommandsUtils.createScriptCommand<CmdDeleteMenuItem>(cmdname_DeleteMenuItem, myQuery, pool);
            case cmd_DeclareGlobalTrigger:
                return CommandsUtils.createScriptCommand<CmdDeclareGlobalTrigger>(cmdname_DeclareGlobalTrigger, myQuery, pool);

            case cmd_OnTimeExceed:
                return CommandsUtils.createControllerCommand<CmdOnTimeExceed>(cmdname_OnTimeExceed, myQuery, pool, cont);
            case cmd_OnMessage:
                return CommandsUtils.createControllerCommand<CmdOnMessage>(cmdname_OnMessage, myQuery, pool, cont);
            case cmd_OnGroupDeath:
                return CommandsUtils.createControllerCommand<CmdOnGroupDeath>(cmdname_OnGroupDeath, myQuery, pool, cont);
            case cmd_OnMenuItemSelect:
                return CommandsUtils.createControllerCommand<CmdOnMenuItemSelect>(cmdname_OnMenuItemSelect, myQuery, pool, cont);
            case cmd_OnTriggers:
                return CommandsUtils.createControllerCommand<CmdOnTriggers>(cmdname_OnTriggers, myQuery, pool, cont);
        }
        return null;
    }

    public override IVmController createController(crc32 name)
    {
        Asserts.Assert(myPrevFactory == null);
        switch (name)
        {
            case cmd_OnMessage:
                return Controllers.createCountController();
            case cmd_OnMenuItemSelect:
                return Controllers.createCountController();
            case cmd_OnGroupDeath:
                return Controllers.createOnceController();
            case cmd_OnTriggers:
                return Controllers.createCountController();
            case cmd_OnTimeExceed:
                return Controllers.createCountController();
            default: return null;
        }
    }

    public StdMsnFactory(IQuery qr, IVmFactory prev_factory) : base(qr, prev_factory) { }
};

public class CmdSetGodness : BaseCommand
{
    public override bool exec()
    {
        myMission.setGodness(myGroup, myUnit, myCanDie != 0);
        return true;
    }

    public override string describeParams(ref string myparams)
    {
        myparams = descString(myparams, "Group", myGroup);
        myparams = descInt(myparams, "Unit", myUnit);
        myparams = descInt(myparams, "CanDie", myCanDie);
        return myparams;
    }

    // BaseCommand
    public override bool isParsingCorrect()
    {
        return myGroup != null;
    }
    public override PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_Group: return new PInfo(VarType.SPT_STRING, OpType.OP_EQU);
            case prm_Unit: return new PInfo(VarType.SPT_INT, OpType.OP_EQU);
            case prm_CanDie: return new PInfo(VarType.SPT_INT, OpType.OP_EQU);
        }
        return new PInfo();
    }

    public override bool addParameter(crc32 param_name, OpType op, Param p, string real_name)
    {
        switch (param_name)
        {
            case prm_Group: myGroup = p.myString; break;
            case prm_Unit: myUnit = p.myInt; break;
            case prm_CanDie: myCanDie = p.myInt; break;
            default: return false;
        }
        return true;
    }

    string myGroup; const crc32 prm_Group = 0x53FE943E;
    int myUnit; const crc32 prm_Unit = 0x83765C92;
    int myCanDie; const crc32 prm_CanDie = 0x462E9876;


    public override bool setDefaults(IQuery tm)
    {
        myUnit = 0;
        myCanDie = 1;
        myMission = (IMissionService)tm.Query(IMissionService.ID);
        return myMission != null;
    }
    IMissionService myMission;

};
