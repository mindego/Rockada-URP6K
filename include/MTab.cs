using System;

public class Tab<t>
{
    protected Th<t> th;
    protected int myCnt;

    public Tab()
    {
        myCnt = 0;
        th = new();
    }
    public Tab(Tab<t> tb)
    {
        myCnt = tb.myCnt;
        th = new Th<t>(tb.th, tb.myCnt);
    }
    //Tab<t> & operator = (const Tab<t> &);

    public int Count() { return myCnt; }
    public void SetCount(int c) { if (c != 0) GetValue(c - 1); myCnt = c; }
    public void Zero() { myCnt = 0; }

    //t Begin() { return th.ptr[0]; }
    public t[] Begin() { return th.ptr; }
    public t End() { return th.ptr[myCnt]; }

    //    t operator [](int i) ;
    //TODO Проверить необходимость возвращать как ref
    public t this[int i]
    {
        get
        {
            Asserts.Assert(i < myCnt && i >= 0);
            return th.ptr[i];
        }
        set
        {
            Asserts.Assert(i < myCnt && i >= 0);
            th.ptr[i] = value;
        }
    }

    public t this[uint i]
    {
        get
        {
            return this[(int)i];
        }
        set
        {
            th.ptr[(int)i] = value;
        }
    }
    //t operator () (int i);
    t GetValue(int i) //TODO переименовать во что-то более приличное
    {
        if (i >= myCnt)
        {
            if (i >= th.alc)
                th.ReAlloc(i + 1, myCnt);
            myCnt = i + 1;
        }
        return th.ptr[i];
    }

    //t New() { return operator()(myCnt); }   // append one
    /// <summary>
    /// append one
    /// </summary>
    /// <returns></returns>
    public t New() { return GetValue(myCnt); }

    public t New(t value)
    {
        GetValue(myCnt);
        th.ptr[myCnt - 1] = value;
        return th.ptr[myCnt - 1];
    }
    t Insert(int at = 0)
    {
        New();
        for (int e = myCnt - 1; e > at; --e)
            th.ptr[e] = th.ptr[e - 1];
        return th.ptr[at];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="s">start</param>
    /// <param name="e">end</param>
    public void Remove(int s, int e)
    {
        Asserts.Assert(s < myCnt && s >= 0 && e < myCnt && e >= 0 && s <= e);
        for (e -= s - 1, myCnt -= e; s < myCnt; ++s)
            th.ptr[s] = th.ptr[s + e];
    }
    public void Remove(int pos) { Remove(pos, pos); }

    public int find(t obj)
    {
        //th.ptr[0].Equals(obj);
        int i;
        //for (i = 0; i < myCnt && th.ptr[i] != obj; i++) ;
        for (i = 0; i < myCnt && !th.ptr[i].Equals(obj); i++) ;
        return i == myCnt ? -1 : i;
    }

    void copy(t[] data, int cnt, int pos = 0)
    {
        GetValue(pos + cnt - 1);
        for (int i = 0; i != cnt; ++i)
            th.ptr[pos + i] = data[i];
    }

};

public class AnyDTab<t> : Tab<t> where t : class, IDisposable
{
    bool check;
    public AnyDTab(bool check = false) : base()
    {
        check = false;
    }
    ~AnyDTab() { Clear(); }
    void Clear() { DeleteTabCont<t>(this); SetCount(0); }
    public void erase(int num)
    {
        if (!check || th.ptr[num] != null)
        {
            th.ptr[num].Dispose();
            //delete th.ptr[num];
        }
        Remove(num, num);
    }

    public static void DeleteTabCont<t>(Tab<t> tb, int start = 0, int end = -1, bool check = false) where t : class, IDisposable
    {
        if (tb.Count() != 0)
        {
            int begin = start;
            int pend = end < 0 ? tb.Count() - 1 : end;

            for (int i = begin; i < pend; i++)
            {
                if (!check || tb[start] == null)
                {
                    tb[i].Dispose(); //TODO тут должен быть вызов деструктора t
                    tb[i] = null;
                }

            }

        }
    }
};

/// <summary>
/// Класс-обертка над AnyRTab(t,r)
/// </summary>
/// <typeparam name="t"></typeparam>
public class AnyRTab<t> : AnyRTab<t,t> where t : class, IMemory
{

}
/// <summary>
/// По умолчанию r=t
/// </summary>
/// <typeparam name="t"></typeparam>
/// <typeparam name="r"></typeparam>
public class AnyRTab<t,r> : Tab<t> where t : class, IMemory where r : IMemory
{
    ~AnyRTab() { Clear(); }
    public void Clear() { ReleaseTabCont<t,r>(this); SetCount(0); }
    public void erase(int num)
    {
        IRefMem.SafeRelease(th.ptr[num]);
        Remove(num, num);
    }

    public static void ReleaseTabCont<t,r>(Tab<t> tb, int start = 0, int end = -1) where t: class,IMemory where r:IMemory
    {
        if (tb.Count() != 0)
        {
            int begin = start;
            int pend = end < 0 ? tb.Count() - 1 : end;

            for (int i = begin; i < pend; i++)
            {
                IMemory.SafeRelease(tb[i]); tb[i] = null;
            }
        }
    }
};

public class Th<t>
{
    public int alc;
    public t[] ptr;
    public Th()
    {
        alc = 0;
        ptr = null;
    }
    public Th(Th<t> th, int cnt)
    {
        ptr = new t[alc = cnt];
        for (int i = 0; i < cnt; ++i) ptr[i] = th.ptr[i];
    }
    ~Th()
    {
        if (ptr != null)
        {
            //delete[] ptr;
            ptr = null;
        }
    }
    public void ReAlloc(int n, int ncpy)
    {
        t[] pnew = new t[alc = TabReallocCnt(alc, n)];
        for (n = 0; n < ncpy; ++n) pnew[n] = ptr[n];
        if (ptr != null)
        {
            //delete[] ptr
            ptr = null;
        }
        ; ptr = pnew;
    }

    int TabReallocCnt(int old, int need)
    {
        int newcnt = (3 * old) >> 1;
        return need > newcnt ? need : newcnt;
    }
};