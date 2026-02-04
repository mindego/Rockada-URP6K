using crc32 = System.UInt32;

public class StdCraftAiFactory : BaseFactory
{
    const string cmdname_AttackCourse = "AttackCourse";
    const crc32 cmd_AttackCourse = 0xA2882710;
    public override IVmCommand createCommand(crc32 name, IVmVariablePool pool, IVmController cont)
    {
        switch (name)
        {
            case cmd_AttackCourse: return CommandsUtils.createScriptCommand<CmdAttackCourse>(cmdname_AttackCourse, myQuery, pool);
            default: return base.createCommand(name, pool, cont);
        }
    }
    public override IVmController createController(crc32 name)
    {
        switch (name)
        {
            default: return base.createController(name);
        }
    }

    public StdCraftAiFactory(IQuery qr, IVmFactory prev_factory) :base(qr, prev_factory) { }

};
