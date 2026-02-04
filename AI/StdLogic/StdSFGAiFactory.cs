using crc32 = System.UInt32;



public class StdSFGAiFactory : BaseFactory
{
    public override IVmCommand createCommand(crc32 name, IVmVariablePool pool, IVmController cont)
    {
        //switch (name)
        //{
        //    CREATE_CMD(SfgStatus, 0xC7D28C3C);
        //default : return myPrevFactory->createCommand(name, pool, cont);
        return null;
    }

    public override IVmController createController(crc32 name)
    {
        //switch (name)
        //{
        //    default: return myPrevFactorycreateController(name);
        //}
        return null;
    }

    public StdSFGAiFactory(IQuery qr, IVmFactory prev_factory) :base(qr, prev_factory) { }

    //public static IVmFactory createStdSFGAiFactory(IQuery qr, IVmFactory prev_factory)
    //{
    //    return new StdSFGAiFactory(qr, prev_factory);
    //}

}





