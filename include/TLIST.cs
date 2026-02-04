using System;
using UnityEngine;

public interface TLIST_ELEM<T> : IDisposable where T : class
{
    public T Next();
    public T Prev();

    public void SetNext(T t);
    public void SetPrev(T t);
}

public class TLIST_ELEM_IMP<T> : TLIST_ELEM<T> where T : class
{
    T prev, next;

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public T Next()
    {
        return next;
    }

    public T Prev()
    {
        return prev;
    }

    public void SetNext(T t)
    {
        next = t;
    }

    public void SetPrev(T t)
    {
        prev = t;
    }
}

public class TLIST<T> where T : class, TLIST_ELEM<T>, IDisposable
{
    T head;
    T tail;
    int count;

    bool auto_sub;

    public TLIST()
    {
        head = null;
        tail = null;
        count = 0;
    }

    public TLIST(bool _auto_sub) : this()
    {
        auto_sub = _auto_sub;
    }

    ~TLIST() { Free(); }
    public void Zero() { head = null; tail = null; count = 0; }
    public void Free()
    {
        if (auto_sub) tail.Dispose();
        while (tail != null)
        {
            T t = tail;
            tail = tail.Prev();
        }
        head = null;
        count = 0;
    }
    public int Counter() { return count; }
    public T Head() { return head; }
    public T Tail() { return tail; }
    public T Next(T t) { return (t != null ? t.Next() : head); }
    public T Prev(T t) { return (t != null ? t.Prev() : tail); }
    public T AddToTail(T n)
    {
        if (tail != null) return InsertAfter(tail, n);
        count++;
        return head = tail = n;
    }
    public T AddToHead(T n)
    {
        if (head != null) return InsertBefore(head, n);
        count++;
        return head = tail = n;
    }
    public T Sub(T n)
    {
        if (n.Prev() != null)
            n.Prev().SetNext(n.Next());
        else
            head = n.Next();
        if (n.Next() != null)
            n.Next().SetPrev(n.Prev());
        else tail = n.Prev();
        n.SetPrev(null);
        n.SetNext(null);
        count--;
        return n;
    }
    T InsertAfter(T a, T n)
    {
        n.SetNext(a.Next());
        n.SetPrev(a);
        if (a.Next() != null)
            a.Next().SetPrev(n);
        else
            tail = n;
        a.SetNext(n);
        count++;
        return n;
    }
    T InsertBefore(T b, T n)
    {
        n.SetPrev(b.Prev());
        n.SetNext(b);
        if (b.Prev() != null)
            b.Prev().SetNext(n);
        else head = n;
        b.SetPrev(n);
        count++;
        return n;
    }
    public T Find(T f) { T c = head; while (c != null && c != f) c = c.Next(); return c; }
}
