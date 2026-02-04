using DWORD = System.UInt32;
/// <summary>
/// iBaseInterface: умеет только конфертироваться в другие интерфейсы
/// </summary>
public interface iBaseInterface
{
    //public void GetInterface(out DWORD var);
    public const uint ID = 0xFFFFFFFF;
    public object GetInterface(DWORD var)
    {
        return null;
    }
    //public T GetInterface<T>() where T:iBaseInterface
    //{
    //    UnityEngine.Debug.Log("GetInterface using " + ID);
    //    return (T)GetInterface(ID);
    //}
    //public uint GetID();
    //template<class C> C* GetInterface() const { return (C*) GetInterface(C::ID);}
    //public T GetInterface<T>() where T : iBaseInterface;
    //{
        
    //    return (T)GetInterface(T.ID);
    //}
};

public interface IQueryInterface
{
    public object queryObject(DWORD id, int num = 0);
    //template<class C> C* queryObject(int num = 0) const { return (C*) queryObject(C::ID, num);
    //public T queryObject<T>(int num = 0);
};


