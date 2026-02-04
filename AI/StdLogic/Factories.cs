public static class Factories
{
    public static IVmFactory createStdMsnFactory(IQuery qr)
    {
        return new StdMsnFactory(qr, null);
    }
    public static IVmFactory createCoopMsnFactory(IQuery qr, IVmFactory prev_factory)
    {
        return new CoopMsnFactory(qr, prev_factory);
    }
    public static IVmFactory createTeamMsnFactory(IQuery qr, IVmFactory prev_factory)
    {
        return null; //STUB!
    }
    public static IVmFactory createInstantMsnFactory(IQuery qr, IVmFactory prev_factory)
    {
        return null; //STUB!
    }

    public static IVmFactory createStdGroupFactory(IQuery qr)
    {
        return new StdGroupFactory(qr, null);
    }
    public static IVmFactory createStdDynGroupFactory(IQuery qr, IVmFactory prev_factory)
    {
        return new StdDynGroupFactory(qr, prev_factory);
    }
    public static IVmFactory createStdCraftGroupFactory(IQuery qr, IVmFactory prev_factory)
    {
        //return createObject<StdCraftGroupFactory>(qr, prev_factory);
        return new StdCraftGroupFactory(qr, prev_factory);
    }
    public static IVmFactory createStdTankGroupFactory(IQuery qr, IVmFactory prev_factory)
    {
        return new StdTankGroupFactory(qr, prev_factory);
    }

    public static IVmFactory createCaptureGroupFactory(IQuery qr, IVmFactory prev_factory)
    {
        return null; //STUB
        //return new CaptureGroupFactory(qr, prev_factory);
    }


    public static IVmFactory createBaseAiFactory(IQuery qr)
    {
        return new BaseAiFactory(qr, null);
    }
    public static IVmFactory createStdSFGAiFactory(IQuery qr, IVmFactory prev_factory)
    {
        return new StdSFGAiFactory(qr, prev_factory);
    }
    public static IVmFactory createStdHangarAiFactory(IQuery qr, IVmFactory prev_factory)
    {
        return new StdHangarAiFactory(qr, prev_factory);
    }
    public static IVmFactory createStdCraftAiFactory(IQuery qr, IVmFactory prev_factory)
    {
        return new StdCraftAiFactory(qr, prev_factory);
    }
    public static IVmFactory createStdCargoAiFactory(IQuery qr, IVmFactory prev_factory)
    {
        return null; //STUB
        //return new StdCargoAiFactory(qr, prev_factory);
    }

}
