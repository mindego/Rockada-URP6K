using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using DWORD = System.UInt32;
/// <summary>
/// interface for sensors
/// </summary>
public interface iSensors : iBaseInterface
{
    public const uint ID = 0xD77B8774;
    public int GetSideCode();
    public iContact GetContact(DWORD handle);
    public iContact GetContactEx(DWORD handle);
    public iContact GetFriend(iContact prev = null);
    public iContact GetEnemy(iContact prev = null);
    public iContact GetEnemyInZone(Vector3 org, float radius, iContact prev = null, float LookBackTime = 0, bool IncludeNeutrals = false);

    public static bool IsHangaring(int state, bool prev_state) //TODO перенести в более подходящее место (iSensors?)
    {
        if (state != iSensorsDefines.CS_DEAD)
            return (state == iSensorsDefines.CS_ENTERING_HANGAR || state == iSensorsDefines.CS_LEAVING_HANGAR);
        return prev_state;

    }
};
/// <summary>
/// Контейнер-хранилище объектов iContact
/// ВНИМАНИЕ! Сравнивать его с чем-то (включая null) НЕ НУЖНО. Если требуется сравнение - смотреть поле ptr или метод Ptr()
/// В оригинале это структура, поэтому её стоит присваивать значение переменной в конструкторе или непосредственно в объявлении переменной
/// Во всех остальных случаях (где в штормокоде присваивается значение) использовать метод setPtr(iContact p)
/// </summary>
public class TContact
{
    iContact ptr;
    public TContact(iContact p = null)
    {
        ptr = p;
        { if (ptr != null) ptr.AddRef(); }
    }
    public TContact(TContact p)
    {
        ptr = p.ptr;
        { if (ptr != null) ptr.AddRef(); }
    }
    ~TContact()
    {
        if (ptr != null) ptr.Release();
    }
    public iContact Ptr() { return ptr; }
    //public void setPtr(iContact p) { ptr = p; }
    public void setPtr(iContact p)
    {
        if (ptr != p)
        {
            if (ptr != null) ptr.Release();
            ptr = p;
            if (ptr != null) ptr.AddRef();
        }
    }
    //iContact* operator -> () const {
    //    Assert(ptr!=0);
    //return ptr;
    //  }

    public bool Validate()
    {
        if (ptr != null && ptr.GetState() != iSensorsDefines.CS_DEAD) return true;
        setPtr(null);
        return false;
    }

    public override string ToString()
    {
        return this.GetType().ToString() + " " + (ptr != null ? ptr : "EMPTY");
    }

    public override int GetHashCode()
    {
        return ptr.GetHashCode();
    }

    //public override bool Equals(object obj)
    //{
    //    //TODO Возможно, это переопределение стрёмное.
    //    return obj == ptr;
    //// или, для отслеживания некорректных вызовов
    // throw new System.Exception(string.Format("Should not compare TCONTACT {0} with {1}!", this, obj));
    //}

    // По хорошему здесь бы реализовать как:
    //            operator iContact* () const { return ptr; }
    // Но исторически сложилось, что такие обращения все переделаны в .Ptr()


    //public static bool operator ==(TContact obj1, object obj2)
    //{
    //    throw new System.Exception(string.Format("Should not compare TCONTACT {0} with {1}!", obj1, obj2));
    //}
    //public static bool operator !=(TContact obj1, object obj2)
    //{
    //    throw new System.Exception(string.Format("Should not compare TCONTACT {0} with {1}!", obj1, obj2));
    //}
}

