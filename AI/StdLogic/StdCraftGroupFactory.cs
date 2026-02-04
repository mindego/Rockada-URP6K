using crc32 = System.UInt32;

public class StdCraftGroupFactory : BaseFactory
{
    const string cmdname_AvoidTerrain = "AvoidTerrain"; const crc32 cmd_AvoidTerrain = 0x275129AF;
    const string cmdname_Escort = "Escort"; const crc32 cmd_Escort = 0x8FA3253D;
    const string cmdname_Patrol = "Patrol"; const crc32 cmd_Patrol = 0xF3A8D9B8;
    const string cmdname_Duel = "Duel"; const crc32 cmd_Duel = 0xC479F7A3;
    const string cmdname_AddMenuItem = "AddMenuItem"; const crc32 cmd_AddMenuItem = 0xAC8A1C49;
    const string cmdname_DeleteMenuItem = "DeleteMenuItem"; const crc32 cmd_DeleteMenuItem = 0x76B78DB2;
    const string cmdname_SendPlayerMessage = "SendPlayerMessage"; const crc32 cmd_SendPlayerMessage = 0x668618E8;

    const string cmdname_OnMenuItemSelect = "OnMenuItemSelect"; const crc32 cmd_OnMenuItemSelect = 0xF702A4A0;

    public override IVmCommand createCommand(crc32 name, IVmVariablePool pool, IVmController cont)
    {
        switch (name)
        {
            case cmd_AvoidTerrain:
                return CommandsUtils.createScriptCommand<CmdAvoidTerrain>(cmdname_AvoidTerrain, myQuery, pool);
            case cmd_Escort:
                return CommandsUtils.createScriptCommand<CmdEscort>(cmdname_Escort, myQuery, pool);
            case cmd_Patrol:
                return CommandsUtils.createScriptCommand<CmdPatrol>(cmdname_Patrol, myQuery, pool);
            case cmd_Duel:
                return CommandsUtils.createScriptCommand<CmdDuel>(cmdname_Duel, myQuery, pool);
            case cmd_AddMenuItem:
                return CommandsUtils.createScriptCommand<CmdAddMenuItem>(cmdname_AddMenuItem, myQuery, pool);
            case cmd_DeleteMenuItem:
                return CommandsUtils.createScriptCommand<CmdDeleteMenuItem>(cmdname_DeleteMenuItem, myQuery, pool);
            case cmd_SendPlayerMessage:
                return CommandsUtils.createScriptCommand<CmdSendPlayerMessage>(cmdname_SendPlayerMessage, myQuery, pool);

            case cmd_OnMenuItemSelect: return CommandsUtils.createControllerCommand<CmdOnMenuItemSelect>(cmdname_OnMenuItemSelect, myQuery, pool, cont);
            default: return myPrevFactory.createCommand(name, pool, cont);
        }

    }

    public override IVmController createController(crc32 name)
    {
        switch (name)
        {
            case cmd_OnMenuItemSelect: return Controllers.createCountController();
            default: return myPrevFactory.createController(name);
        }
    }

    public StdCraftGroupFactory(IQuery qr, IVmFactory prev_factory) : base(qr, prev_factory) { }
};
