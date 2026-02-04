using static direct3lights;
public static class direct3lights
{
    public static uint LP_SHIFT = 16;
    public static uint LP_MASK = 0xffff0000;

    public static uint lsPASSIVE = 0x00000001;
    public static uint lsACTIVE = 0x00000002;
    public static uint lsTO_ACTIVATE = 0x00000004;
    public static uint lsUPDATED = 0x00000008;
    public static uint LS_SHIFT = 0;
    public static uint LS_MASK = 0xffff;


    public static uint LS_ENABLED = 0x80000000;

    public static uint LR_OK = 0x00000000;
    public static uint LR_CANTADD = 0x00000001;
    public static uint LR_CANTENABLE = 0x00000002;
}
public class LightIndex
{


    private int flag;
    public int d3dindex;

    public LIGHT light;        // coresponding remote object's id
    LightIndex next_added;
    LightIndex prev_added;
    public LightIndex next_active;
    LightIndex prev_active;
    public LightIndex next_toactivate;
    public LightIndex next_todeactivate;

    public void Add(int priority, int status, LIGHT l, LightIndex n_added, LightIndex p_added = null)
    {
        light = l;
        next_added = n_added;
        prev_added = p_added;
        SetPriority(priority);
        ClearStatus(LS_MASK);
        SetStatus((uint)status);

    }


    public void SetStatus(uint s) { flag |= ((int)s << (int)LS_SHIFT); }
    public void ClearStatus(uint s) { flag &= ~((int)s << (int)LS_SHIFT); }
    void SetPriority(int p) { flag = (flag & ~(int)LP_MASK) | (p << (int)LP_SHIFT); }
    public uint GetStatus() { return (uint)(flag & (int)LS_MASK) >> (int)LS_SHIFT; }
    uint GetPriority() { return (uint)(flag & (int)LP_MASK) >> (int)LP_SHIFT; }
};