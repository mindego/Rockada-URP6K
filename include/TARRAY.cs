using DWORD = System.UInt32;
using static TArrayDefines;
using UnityEngine.Assertions;

public class TArrayDefines
{
    public const uint THANDLE_INVALID = 0xFFFFFFFF;
    public const uint THANDLE_POS_MASK = 0x0000FFFF;
    public const uint THANDLE_FRAME_MASK = 0xFFFF0000;
    public const uint THANDLE_FRAME_UNIT = 0x00010000;
}

// *********************************************************************************
// TARRAY_ITEM
public class TItem<T> where T : class
{
    public T Item;
    public DWORD Frame;
    internal TItem()
    {
        Item = null;
        Frame = 0;
    }

    public void Clear() { Item = null; }
    public DWORD Set(T i) { Item = i; Frame += THANDLE_FRAME_UNIT; return Frame; }
    public void Set(T i, DWORD f) { Item = i; Frame = f; }
    public T Get(DWORD f)
    {
        return (f == Frame ? Item : null);
    }
    public T Get() { return Item; }
};


// *********************************************************************************
// TArray
public class TArray<T> where T : class
{
    private TItem<T>[] myArray;
    DWORD Size;
    DWORD Pos;
    public TArray(int size)
    {
        myArray = new TItem<T>[size];
        for (int i=0;i<size;i++)
        {
            myArray[i] = new TItem<T>();
        }

        Size = (uint)size;
        Pos = Size;
    }
    ~TArray() { myArray = null; }
    public TItem<T>[] GetArray() { return myArray; }
    public DWORD GetSize() { return Size; }
    public T Get(DWORD h) { DWORD idx = (h & THANDLE_POS_MASK); return (idx < Size ? myArray[idx].Get(h & THANDLE_FRAME_MASK) : null); }
    public void Debug(DWORD h)
    {
        DWORD idx = (h & THANDLE_POS_MASK);
        string res = "Failed to find " + h.ToString("X8") + "\n";
        res += "idx: " + idx + "\n";
        res += "idx < Size? " + (idx < Size).ToString() + "\n";
        res += "TItem: " + (idx < Size ? myArray[idx] : "null") + "\n";
        res += "Mask: " + (h & THANDLE_FRAME_MASK).ToString("X8") + "\n";
        res += "TItem value: " + (idx < Size ? myArray[idx].Get(h & THANDLE_FRAME_MASK) : "null") + "\n";
        UnityEngine.Debug.Log(res);
    }
    //public T              operator [] (DWORD idx) const { return (idx<Size? Array[idx].Get() : 0); }

    public T this[DWORD idx]
    {
        get
        {
            return (idx < Size ? myArray[idx].Get() : null);
        }
    }

    public T this[int idx]
    {
        get
        {
            return this[(uint)idx];
        }
    }
    public DWORD GetHandle(DWORD idx) { return (idx < Size ? (idx | myArray[idx].Frame) : THANDLE_INVALID); }
    public T Set(DWORD h, T i)
    {
        DWORD idx = h & THANDLE_POS_MASK;
        Assert.IsTrue(idx < Size);
        myArray[idx].Set(i, h & THANDLE_FRAME_MASK);
        return i;
    }
    public T Sub(DWORD h)
    {
        DWORD idx = h & THANDLE_POS_MASK;
        if (idx >= Size) return null;
        T i = myArray[idx].Get(h & THANDLE_FRAME_MASK);
        if (i != null) myArray[idx].Clear();
        return i;
    }
    public DWORD Add(T i)
    {
        DWORD old = Pos;
        bool over = false;
        while (true)
        {
            if (Pos >= Size) { Pos = 0; over = true; }
            Assert.IsTrue((over && Pos == old) == false);
            if (myArray[Pos].Get() == null)
            {
                return (myArray[Pos].Set(i) | Pos);
            }
            Pos++;
        }
    }
};