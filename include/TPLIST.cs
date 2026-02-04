/// <summary>
/// внутренняя структура для TPLIST
/// </summary>
/// <typeparam name="T"></typeparam>
class TPLIST_ELEM<T> where T : class
{
    //friend class TPLIST<T>;
    public TPLIST_ELEM<T> prev;
    public TPLIST_ELEM<T> next;
    public T data;
    public TPLIST_ELEM(T ptr = null)
    {
        prev = null;
        next = null;
        data = ptr;
    }
    public TPLIST_ELEM<T> Next() { return next; }
    public TPLIST_ELEM<T> Prev() { return prev; }
    public T Data() { return data; }
};



/// <summary>
/// TPLIST
/// Связанный список с хранением объекта в отдельном поле "data" элемента списка
/// Полный аналог LinkedList
/// </summary>
/// <typeparam name="T"></typeparam>
class TPLIST<T> where T : class
{
    TPLIST_ELEM<T> head;
    TPLIST_ELEM<T> tail;
    int count;
    public TPLIST()
    {
        head = null;
        tail = null;
        count = 0;
    }
    ~TPLIST() { Free(); }
    void Zero() { head = null; tail = null; count = 0; }
    void Free()
    {
        while (tail != null)
        {
            TPLIST_ELEM<T> t = tail;
            tail = tail.prev;
            //delete t; 
        }
        head = null;
        count = 0;
    }
    public int Counter() { return count; }
    public TPLIST_ELEM<T> Head() { return head; }
    public TPLIST_ELEM<T> Tail() { return tail; }
    TPLIST_ELEM<T> Next(TPLIST_ELEM<T> t) { return (t != null ? t.next : head); }
    TPLIST_ELEM<T> Prev(TPLIST_ELEM<T> t) { return (t!=null ? t.prev : tail); }
    public TPLIST_ELEM<T> AddToTail(T d) { if (tail!=null) return InsertAfter(tail, d); 
        count++; 
        head = tail = new TPLIST_ELEM<T>(d);
        return tail;
    }
    TPLIST_ELEM<T> AddToHead(T d) { if (head!=null) return InsertBefore(head, d); 
        count++; 
        head = tail = new TPLIST_ELEM<T>(d);
        return head;
    }
    TPLIST_ELEM<T> Sub(TPLIST_ELEM<T> n) { if (n.prev!=null) n.prev.next = n.next; else head = n.next; if (n.next!=null) n.next.prev = n.prev; else tail = n.prev; n.prev = null; n.next = null; count--; return n; }
    TPLIST_ELEM<T> InsertAfter(TPLIST_ELEM<T> a, T d) { TPLIST_ELEM<T> n = new TPLIST_ELEM<T>(d); n.next = a.next; n.prev = a; if (a.next!=null) a.next.prev = n; else tail = n; a.next = n; count++; return n; }
    TPLIST_ELEM<T> InsertBefore(TPLIST_ELEM<T> b, T d) { TPLIST_ELEM<T> n = new TPLIST_ELEM<T>(d); n.prev = b.prev; n.next = b; if (b.prev!=null) b.prev.next = n; else head = n; b.prev = n; count++; return n; }
    public TPLIST_ELEM<T> Find(T d) {
        TPLIST_ELEM<T> c = null;
        for (c = head; c!=null; c = c.next) if (c.data == d) break; return c; }
};
