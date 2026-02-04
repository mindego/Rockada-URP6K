public class BaseList
{
    public int Head, Tail, Counter;
    public int[] Next, Prev;
    private int offset;
    public BaseList(int[] next, int[] prev, int offset=0)
    {
        Counter = 0;
        Next = next;
        Prev = prev;
        this.offset = offset;
    }
    private int GetOffsetted(int i) { return i + offset; }
    public int NextElem(int i) { return Next[GetOffsetted(i)]; }
    public int NextElemCicled(int i) { if (GetOffsetted(i) == Tail) return Head; else return Next[GetOffsetted(i)]; }
    public int PrevElem(int i) { return Prev[GetOffsetted(i)]; }
    public int PrevElemCicled(int i) { if (GetOffsetted(i) == Head) return Tail; else return Prev[GetOffsetted(i)]; }

    public int ExtractTail()
    {
        Asserts.AssertBp(Counter);
        Counter--;
        int i = Tail;
        if (Counter != 0) Tail = Prev[i];
        return GetOffsetted(i);
    }

    public int ExtractHead()
    {
        Asserts.AssertBp(Counter);
        Counter--;
        int i = Head;
        if (Counter != 0) Head = Next[i];
        return GetOffsetted(i);
    }

    public int Extract(int i)
    {
        Asserts.AssertBp(Counter);
        if (GetOffsetted(i) == Tail) return ExtractTail();
        if (GetOffsetted(i) == Head) return ExtractHead();
        Prev[Next[GetOffsetted(i)]] = Prev[GetOffsetted(i)];
        Next[Prev[GetOffsetted(i)]] = Next[GetOffsetted(i)];
        Counter--;
        return GetOffsetted(i);
    }

    public int AddHead(int i)
    {
        if (Counter != 0)
        {
            Next[GetOffsetted(i)] = Head;
            Prev[Head] = GetOffsetted(i);
            Head = GetOffsetted(i);
        }
        else
        {
            Head = Tail = GetOffsetted(i);
        }
        Counter++;
        return GetOffsetted(i);
    }

    public int AddTail(int i)
    {
        if (Counter != 0)
        {
            Prev[GetOffsetted(i)] = Tail;
            Next[Tail] = GetOffsetted(i);
            Tail = GetOffsetted(i);
        }
        else
        {
            Head = Tail = GetOffsetted(i);
        }
        Counter++;
        return GetOffsetted(i);
    }

    public int AddAfter(int Inner, int Outer)
    {
        if (GetOffsetted(Inner) == Tail) return AddTail(GetOffsetted(Outer));
        Prev[GetOffsetted(Outer)] = GetOffsetted(Inner);
        Next[GetOffsetted(Outer)] = Next[GetOffsetted(Inner)];
        Next[GetOffsetted(Inner)] = GetOffsetted(Outer);
        Prev[Next[GetOffsetted(Outer)]] = GetOffsetted(Outer);
        Counter++;
        return GetOffsetted(Outer);
    }

    public int AddBefore(int Inner, int Outer)
    {
        if (GetOffsetted(Inner) == Head) return AddHead(GetOffsetted(Outer));
        Next[GetOffsetted(Outer)] = GetOffsetted(Inner);
        Prev[GetOffsetted(Outer)] = Prev[GetOffsetted(Inner)];
        Next[Prev[GetOffsetted(Inner)]] = GetOffsetted(Outer);
        Prev[GetOffsetted(Inner)] = GetOffsetted(Outer);
        Counter++;
        return GetOffsetted(Outer);
    }


};

public class AList<T>
{
    BaseList FullList, FreeList;
    public T[] Data;
    private int offset;

    public AList(T[] D, int[] next, int[] prev, int _offset = 0)
    {
        Data = D;
        FreeList = new BaseList(next, prev, _offset);
        FullList = new BaseList(next, prev, _offset);
        offset = _offset;
    }

    public void Empty()
    { FullList.Counter = 0; FreeList.Counter = 0; }
    public int Counter()
    { return FullList.Counter; }

    public int Head() { return FullList.Head; }
    public int Tail() { return FullList.Tail; }

    public int NextElem(int i) { return FullList.NextElem(i); }
    public int NextElemCicled(int i) { return FullList.NextElemCicled(i); }
    public int PrevElem(int i) { return FullList.PrevElem(i); }
    public int PrevElemCicled(int i) { return FullList.PrevElemCicled(i); }

    public int Extract(int i)
    {
        FreeList.AddHead(FullList.Extract(i));
        return i;
    }


    public int AddHead(T ToAdd)
    {
        int i;
        if (FreeList.Counter > 0) i = FreeList.ExtractHead();
        else i = FreeList.Counter + FullList.Counter;
        Data[FullList.AddHead(i)] = ToAdd;
        return i;
    }

    public int AddTail(T ToAdd)
    {
        int i;
        if (FreeList.Counter > 0) i = FreeList.ExtractHead();
        else i = FullList.Counter;
        Data[FullList.AddTail(i)] = ToAdd;
        return i;
    }

    public int AddAfter(int Inner, T ToAdd)
    {
        int i;
        if (FreeList.Counter > 0) i = FreeList.ExtractHead();
        else i = FullList.Counter;
        Data[FullList.AddAfter(Inner, i)] = ToAdd;
        return i;
    }

    public int AddBefore(int Inner, T ToAdd)
    {
        int i;
        if (FreeList.Counter > 0) i = FreeList.ExtractHead();
        else i = FullList.Counter;
        Data[FullList.AddBefore(Inner, i)] = ToAdd;
        return i;
    }
};
