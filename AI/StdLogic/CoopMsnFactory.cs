using UnityEngine;
using crc32 = System.UInt32;

public class CoopMsnFactory : BaseFactory
{
    const string cmdname_SetPlayerPosition = "SetPlayerPosition"; const crc32 cmd_SetPlayerPosition = 0x569CDF95;
    const string cmdname_AddObjective = "AddObjective"; const crc32 cmd_AddObjective = 0xCA83E4B3;
    const string cmdname_SetObjective = "SetObjective"; const crc32 cmd_SetObjective = 0xCC80C9F8;
    const string cmdname_AddAward = "AddAward"; const crc32 cmd_AddAward = 0x1ECE31A1;
    const string cmdname_AddScore = "AddScore"; const crc32 cmd_AddScore = 0xA60C2817;
    const string cmdname_DeleteObjective = "DeleteObjective"; const crc32 cmd_DeleteObjective = 0xE036F2B6;
    const string cmdname_SetPlayable = "SetPlayable"; const crc32 cmd_SetPlayable = 0xFDA61D99;
    const string cmdname_SetStatistics = "SetStatistics"; const crc32 cmd_SetStatistics = 0xF4C5382F;
    const string cmdname_ForgetPlayer = "ForgetPlayer"; const crc32 cmd_ForgetPlayer = 0xFCBD74A1;
    const string cmdname_SetGlobalAccessToWeapon = "SetGlobalAccessToWeapon"; const crc32 cmd_SetGlobalAccessToWeapon = 0x0347B523;
    const string cmdname_SetGlobalAccessToCraft = "SetGlobalAccessToCraft"; const crc32 cmd_SetGlobalAccessToCraft = 0x3F040DB3;
    const string cmdname_EnableEngBay = "EnableEngBay"; const crc32 cmd_EnableEngBay = 0xC575030D;
    const string cmdname_SetGodMode = "SetGodMode"; const crc32 cmd_SetGodMode = 0x72B8E13F;
    public override IVmCommand createCommand(crc32 name, IVmVariablePool pool, IVmController cont)
    {
        switch (name)
        {
            case cmd_SetPlayerPosition: return CommandsUtils.createScriptCommand<CmdSetPlayerPosition>(cmdname_SetPlayerPosition, myQuery, pool);
            case cmd_AddObjective: return CommandsUtils.createScriptCommand<CmdAddObjective>(cmdname_AddObjective, myQuery, pool);
            case cmd_SetObjective: return CommandsUtils.createScriptCommand<CmdSetObjective>(cmdname_SetObjective, myQuery, pool);
            case cmd_AddAward: return CommandsUtils.createScriptCommand<CmdAddAward>(cmdname_AddAward, myQuery, pool);
            case cmd_AddScore: return CommandsUtils.createScriptCommand<CmdAddScore>(cmdname_AddScore, myQuery, pool);
            case cmd_DeleteObjective: return CommandsUtils.createScriptCommand<CmdDeleteObjective>(cmdname_DeleteObjective, myQuery, pool);
            case cmd_SetPlayable: return CommandsUtils.createScriptCommand<CmdSetPlayable>(cmdname_SetPlayable, myQuery, pool);
            case cmd_SetStatistics: return CommandsUtils.createScriptCommand<CmdSetStatistics>(cmdname_SetStatistics, myQuery, pool);
            case cmd_ForgetPlayer: return CommandsUtils.createScriptCommand<CmdForgetPlayer>(cmdname_ForgetPlayer, myQuery, pool);
            case cmd_SetGlobalAccessToWeapon: return CommandsUtils.createScriptCommand<CmdSetGlobalAccessToWeapon>(cmdname_SetGlobalAccessToWeapon, myQuery, pool);
            case cmd_SetGlobalAccessToCraft: return CommandsUtils.createScriptCommand<CmdSetGlobalAccessToCraft>(cmdname_SetGlobalAccessToCraft, myQuery, pool);
            case cmd_EnableEngBay: return CommandsUtils.createScriptCommand<CmdEnableEngBay>(cmdname_EnableEngBay, myQuery, pool);
            case cmd_SetGodMode: return CommandsUtils.createScriptCommand<CmdSetGodMode>(cmdname_SetGodMode, myQuery, pool);
            default: {
                    return myPrevFactory.createCommand(name, pool, cont); 
                }
        }
    }
    public override IVmController createController(crc32 name)
    {
        return myPrevFactory.createController(name);
    }

    public CoopMsnFactory(IQuery qr, IVmFactory prev_factory) : base(qr, prev_factory) { }

};
