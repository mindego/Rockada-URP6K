
/// <summary>
/// Query to some group of pointers by cls_id parameter
/// Rem: offen query each vftbl, that may be usefull 
/// </summary>
public interface IQuery
{
    const uint ID = 0xBAADF00D;
    public object Query(uint cls_id);
    //public T Query<T>() where T:IQuery
    //{
    //    return (T)Query(T.GetQueryID());
    //}
    //public static uint GetQueryID()
    //{
    //    return ID;
    //}
}

/*===========================================================================*\
|  IQMemory able to release and query                                         |
\*===========================================================================*/
public interface IQMemory : IMemory
{
    public object Query(int i ) { return null; }
    //template<class C> C* QueryI() { return (C*)Query(C::ID); }
};
