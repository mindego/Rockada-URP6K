using DWORD = System.UInt32;
/*===========================================================================*\
|  Deleteble object                                                           |
\*===========================================================================*/
public interface IMemory
{
    public const DWORD ID = 0xEF03808B;
    public int Release();

    public static int SafeRelease(IMemory i)
    {
        return i!=null ?  i.Release() : 0;
    }
};


