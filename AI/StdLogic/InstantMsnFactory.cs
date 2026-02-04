#define _DEBUG
using crc32 = System.UInt32;
using static CommandsUtils;

public class InstantMsnFactory : BaseFactory
{
    const string cmdname_AppearTime = "AppearTime"; const crc32 cmd_AppearTime = 0x46284D27;
    public override IVmCommand createCommand(crc32 name, IVmVariablePool pool, IVmController cont)
    {
        switch (name)
        {
            case cmd_AppearTime:
                return createScriptCommand<CmdAppearTime>(cmdname_AppearTime, myQuery, pool);
            default: return myPrevFactory.createCommand(name, pool, cont);
        }
    }
    public override IVmController createController(crc32 name)
    {
        return myPrevFactory.createController(name);
    }


    InstantMsnFactory(IQuery qr, IVmFactory prev_factory) : base(qr, prev_factory) { }

    public static IVmFactory createInstantMsnFactory(IQuery qr, IVmFactory prev_factory)
    {
        return new InstantMsnFactory(qr, prev_factory);
    }
};
