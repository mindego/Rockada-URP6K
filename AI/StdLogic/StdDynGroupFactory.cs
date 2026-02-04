using UnityEngine;
using crc32 = System.UInt32;

public class StdDynGroupFactory : BaseFactory
{
    const string cmdname_SetFormation = "SetFormation"; const crc32 cmd_SetFormation = 0x35561946;
    const string cmdname_Switch = "Switch"; const crc32 cmd_Switch = 0x97BAB1D1;
    const string cmdname_ReachRadius = "ReachRadius"; const crc32 cmd_ReachRadius = 0xA3531F30;
    const string cmdname_Pause = "Pause"; const crc32 cmd_Pause = 0xE9A44216;
    const string cmdname_Resume = "Resume"; const crc32 cmd_Resume = 0x98922A69;
    const string cmdname_Break = "Break"; const crc32 cmd_Break = 0x32428021;
    const string cmdname_KillAll = "KillAll"; const crc32 cmd_KillAll = 0xDA426348;
    const string cmdname_RouteDelta = "RouteDelta"; const crc32 cmd_RouteDelta = 0x17405C6C;
    const string cmdname_RouteTo = "RouteTo"; const crc32 cmd_RouteTo = 0xB7D2D0B6;
    const string cmdname_SetSpeed = "SetSpeed"; const crc32 cmd_SetSpeed = 0x008CDEC9;
    const string cmdname_Alert = "Alert"; const crc32 cmd_Alert = 0x29C3963A;
    const string cmdname_Park = "Park"; const crc32 cmd_Park = 0x9BCA2DF2;
    const string cmdname_SetAutoReformat = "SetAutoReformat"; const crc32 cmd_SetAutoReformat = 0x3CC5AF39;

    const string cmdname_OnReach = "OnReach"; const crc32 cmd_OnReach = 0xE8A9A0B8;
    const string cmdname_OnLeave = "OnLeave"; const crc32 cmd_OnLeave = 0x7E7D13F2;
    const string cmdname_OnRectReach = "OnRectReach"; const crc32 cmd_OnRectReach = 0xFA23A2FA;
    const string cmdname_OnRectLeave = "OnRectLeave"; const crc32 cmd_OnRectLeave = 0x6CF711B0;

    public override IVmCommand createCommand(crc32 name, IVmVariablePool pool, IVmController cont)
    {
        switch (name)
        {
            case cmd_SetFormation:
                return CommandsUtils.createScriptCommand<CmdSetFormation>(cmdname_SetFormation, myQuery, pool);
            case cmd_Switch:
                return CommandsUtils.createScriptCommand<CmdSwitch>(cmdname_Switch, myQuery, pool);
            case cmd_ReachRadius:
                return CommandsUtils.createScriptCommand<CmdReachRadius>(cmdname_ReachRadius, myQuery, pool);
            case cmd_Pause:
                return CommandsUtils.createScriptCommand<CmdPause>(cmdname_Pause, myQuery, pool);
            case cmd_Resume:
                return CommandsUtils.createScriptCommand<CmdResume>(cmdname_Resume, myQuery, pool);
            case cmd_Break:
                return CommandsUtils.createScriptCommand<CmdBreak>(cmdname_Break, myQuery, pool);
            case cmd_KillAll:
                return CommandsUtils.createScriptCommand<CmdKillAll>(cmdname_KillAll, myQuery, pool);
            case cmd_RouteDelta:
                return CommandsUtils.createScriptCommand<CmdRouteDelta>(cmdname_RouteDelta, myQuery, pool);
            case cmd_RouteTo:
                return CommandsUtils.createScriptCommand<CmdRouteTo>(cmdname_RouteTo, myQuery, pool);
            case cmd_SetSpeed:
                return CommandsUtils.createScriptCommand<CmdSetSpeed>(cmdname_SetSpeed, myQuery, pool);
            case cmd_Alert:
                return CommandsUtils.createScriptCommand<CmdAlert>(cmdname_Alert, myQuery, pool);
            case cmd_Park:
                return CommandsUtils.createScriptCommand<CmdPark>(cmdname_Park, myQuery, pool);
            case cmd_SetAutoReformat:
                return CommandsUtils.createScriptCommand<CmdSetAutoReformat>(cmdname_SetAutoReformat, myQuery, pool);

            case cmd_OnReach:
                return CommandsUtils.createControllerCommand<CmdOnReach>(cmdname_OnReach, myQuery, pool, cont);
            case cmd_OnLeave:
                return CommandsUtils.createControllerCommand<CmdOnLeave>(cmdname_OnLeave, myQuery, pool, cont);
            case cmd_OnRectReach:
                return CommandsUtils.createControllerCommand<CmdOnRectReach>(cmdname_OnRectReach, myQuery, pool, cont);
            case cmd_OnRectLeave:
                return CommandsUtils.createControllerCommand<CmdOnRectLeave>(cmdname_OnRectLeave, myQuery, pool, cont);
            default: return myPrevFactory.createCommand(name, pool, cont);
        }
    }

    public override IVmController createController(crc32 name)
    {
        //Debug.Log("myPrevFactory " + myPrevFactory);
        switch (name)
        {
            case cmd_OnReach:
                return Controllers.createOnceController();
            case cmd_OnLeave:
                return Controllers.createOnceController();
            case cmd_OnRectReach:
                return Controllers.createOnceController();
            case cmd_OnRectLeave:
                return Controllers.createOnceController();
            default: return myPrevFactory.createController(name);
        }
    }

    public StdDynGroupFactory(IQuery qr, IVmFactory prev_factory) : base(qr, prev_factory) { }

    //public static IVmFactory createStdDynGroupFactory(IQuery qr, IVmFactory prev_factory)
    //{
    //    return new StdDynGroupFactory(qr, prev_factory);
    //}
};
