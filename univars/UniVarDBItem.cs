using System.Runtime.InteropServices;
using DWORD = System.UInt32;
/// <summary>
/// UniVarDBItem - служебная структура
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class UniVarDBItem
{
    public DWORD mClassId;
    public DWORD mMemId;
    public void Set(DWORD c, DWORD m) { mClassId = c; mMemId = m; }

    public static int GetSize()
    {
        return sizeof(DWORD) + sizeof(DWORD);
    }
};



