//using DWORD = System.UInt32;
//public class Hlist<T>
//{
//    typedef Pool<T>::pelem HPelem;

//    bool IsClear() { return head == null; }
//    void Clear() { head = null; }

//    int Remove(T hm, Pool<T>* );
//    HPelem Add(T* hm, Pool<T>* p)
//    {
//        HPelem* q = p->New(hm, head);
//        if (q) head = q;
//        return q;
//    }
//    int LayerEnumerate(int Flag, HashEnumer e, int layer, int cmp_sign);
//    DWORD Upload();
//    private HPelem head;
//};

//public class Pool<T>
//{

//    private class Pelem<T>
//    {
//        public Pelem<T> next;
//        T data;
//    };

//    Pelem<T>[] BasePelem;
//    Pelem<T> _all;
//    readonly int size;


//    //public typedef Pelem<T> pelem;

//    public Pool(int s)
//    {
//        size = s;
//        BasePelem = new Pelem<T>[s];
//        _all = null;
//    }

//    ~Pool() { BasePelem = null; }

//    void Initialize()
//    {
//        Pelem<T> e = base + size - 1;
//        Pelem<T> p;
//        for (Pelem<T> p = base; p < e; ++p)
//            p->next = p + 1;
//        p.next = null;
//        _all = base;
//        _all = BasePelem[0];
//    }

//    int Size() { return size; }

//    public Pelem<T> New(Pelem<T> nx = null)
//    {
//        Pelem<T> r = _all;
//        if (r!=null) { _all = r.next; r.next = nx; }
//        return r;
//    }

//    Pelem<T>* New(T* dt, Pelem<T>* nx = 0)
//    {
//        Pelem<T>* r = _all;
//        if (r) { _all = r->next; r->next = nx; r->data = dt; }
//        return r;
//    }


//    void Delete(Pelem<T> p)
//    {
//        /*#ifdef _DEBUG
//        int n = p-base;
//        if (n<0 || n>=size) 
//        __asm int 3;
//        for (Pelem<T> *q=_all; q; q=q->next)
//        if (p==q) __asm int 3;
//        p->data=0;
//    #endif*/
//        p.next = _all; _all = p;
//    }

//    int FreeElems() {
//        int i = 0; for (Pelem<T> s = _all; s!=null; s = s.next) i++; return i; 
//    }
//};

using DWORD = System.UInt32;
using System.Collections.Generic;
using UnityEngine;

public class Hlist<T> { 
    //typedef Pool<T>::pelem HPelem;

    public bool IsClear() { return head.Count == 0; }
    public void Clear() { head = new List<T>(); }

    public int Remove(T hm)
    {
        return head.Remove(hm) ? 1 : 0;
    }
    public T Add(T hm)
    {
        //HPelem* q = p->New(hm, head);
        //if (q) head = q;
        head.Add(hm);
        return hm;
    }
    public int LayerEnumerate(uint Flag, HashEnumer e, int layer, int cmp_sign)
    {
        int count = 0;
        foreach (T t in head)
        {

            HMember dt = t as HMember;
            if (dt.Object() != null && dt.MatchOnLayer(Flag, layer, cmp_sign))
            {
                dt.SetLayer(layer, cmp_sign); count++;
                if (!e.ProcessElement(dt)) return count;
            }
        }
        //Debug.LogFormat("Enumerated in layer {0} {1} {2}",layer,cmp_sign,count);
        return count;
    }

    public DWORD Upload() { return (uint)head.Count; }
    private List<T> head;
};