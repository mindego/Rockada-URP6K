using crc32 = System.UInt32;

public class StdHangarAiFactory : BaseFactory
{
    const string cmdname_HangarStatus = "HangarStatus";
    const crc32 cmd_HangarStatus = 0x7EEBC966;
    public override IVmCommand createCommand(crc32 name, IVmVariablePool pool, IVmController cont)
    {
        switch (name)
        {
            case cmd_HangarStatus: return CommandsUtils.createScriptCommand<CmdHangarStatus>(cmdname_HangarStatus, myQuery, pool);
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

    public StdHangarAiFactory(IQuery qr, IVmFactory prev_factory) : base(qr, prev_factory) { }

    //public static IVmFactory createStdHangarAiFactory(IQuery qr, IVmFactory prev_factory)
    //{
    //    return new StdHangarAiFactory(qr, prev_factory);
    //}
};
