using crc32 = System.UInt32;
public class BaseAiFactory : BaseFactory
{
    const string cmdname_Delay = "Delay"; const crc32 cmd_Delay = 0x8CA45060;
    const string cmdname_SendMessage = "SendMessage"; const crc32 cmd_SendMessage = 0x668618E8;
    const string cmdname_NotifyAll = "NotifyAll"; const crc32 cmd_NotifyAll = 0xCDE0A8AD;
    const string cmdname_AddDamage = "AddDamage"; const crc32 cmd_AddDamage = 0x5CE1CA5B;
    const string cmdname_SetSkill = "SetSkill"; const crc32 cmd_SetSkill = 0x5197C448;
    const string cmdname_SetFireMode = "SetFireMode"; const crc32 cmd_SetFireMode = 0x1FFD09F9;

    const string cmdname_OnMessage = "OnMessage"; const crc32 cmd_OnMessage = 0x39D04393;
    const string cmdname_OnDamage = "OnDamage"; const crc32 cmd_OnDamage = 0x164F3796;
    public override IVmCommand createCommand(crc32 name, IVmVariablePool pool, IVmController cont)
    {
        Asserts.Assert(myPrevFactory == null);
        switch (name)
        {
            case cmd_Delay:
                return CommandsUtils.createScriptCommand<CmdDelay>(cmdname_Delay, myQuery, pool);
            case cmd_SendMessage:
                return CommandsUtils.createScriptCommand<CmdSendMessage>(cmdname_SendMessage, myQuery, pool);
            case cmd_NotifyAll:
                return CommandsUtils.createScriptCommand<CmdNotifyAll>(cmdname_NotifyAll, myQuery, pool);
            case cmd_AddDamage:
                return CommandsUtils.createScriptCommand<CmdAddDamage>(cmdname_AddDamage, myQuery, pool);
            case cmd_SetSkill:
                return CommandsUtils.createScriptCommand<CmdSetSkill>(cmdname_SetSkill, myQuery, pool);
            case cmd_SetFireMode:
                return CommandsUtils.createScriptCommand<CmdSetFireMode>(cmdname_SetFireMode, myQuery, pool);

            case cmd_OnMessage:
                return CommandsUtils.createControllerCommand<CmdOnMessage>(cmdname_OnMessage, myQuery, pool, cont);
            case cmd_OnDamage:
                return CommandsUtils.createControllerCommand<CmdOnDamage>(cmdname_OnDamage, myQuery, pool, cont);
            default: return null;
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
            case cmd_OnDamage:
                return Controllers.createOnceController();
            default: return null;
        }
    }


    public BaseAiFactory(IQuery qr, IVmFactory prev_factory) : base(qr, prev_factory) { }

    //public static IVmFactory createBaseAiFactory(IQuery qr)
    //{
    //    IVmFactory prev_factory = null;
    //    //return createObject<BaseAiFactory>(qr, prev_factory);
    //    return new BaseAiFactory(qr, prev_factory);
    //}

};
