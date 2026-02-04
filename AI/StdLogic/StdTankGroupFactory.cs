using crc32 = System.UInt32;

public class StdTankGroupFactory : BaseFactory
{
    const string cmdname_FightMode = "FightMode";
    const crc32 cmd_FightMode = 0xCB82A496;
    const string cmdname_UseRoads = "UseRoads";
    const crc32 cmd_UseRoads = 0xA2BCDDA7;
    public override IVmCommand createCommand(crc32 name, IVmVariablePool pool, IVmController cont)
    {
        switch (name)
        {
            case cmd_FightMode: return CommandsUtils.createScriptCommand<CmdFightMode>(cmdname_FightMode, myQuery, pool);
            case cmd_UseRoads: return CommandsUtils.createScriptCommand<CmdUseRoads>(cmdname_UseRoads, myQuery, pool);
            default: return myPrevFactory.createCommand(name, pool, cont);
        }

    }

    public override IVmController createController(crc32 name)
    {
        switch (name)
        {
            default: return myPrevFactory.createController(name);
        }
    }

    public StdTankGroupFactory(IQuery qr, IVmFactory prev_factory) : base(qr, prev_factory) { }
    //public static IVmFactory createStdTankGroupFactory(IQuery qr, IVmFactory prev_factory)
    //{
    //    return new StdTankGroupFactory(qr, prev_factory);
    //}
};

