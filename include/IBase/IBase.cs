public class IBase
{
    public const uint UndefinedID = 0xFFFFFFFF;

    public static int SafeRelease<T>(T i) where T : IMemory
    {
        return i != null ? i.Release() : 0;
    }
}