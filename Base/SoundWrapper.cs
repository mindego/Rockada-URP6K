using UnityEngine;
using DWORD = System.UInt32;

public class RefSoundCtrWrapper : I3DSoundEventController
{

    public override string ToString()
    {
        string res = GetType().ToString();
        res += string.Format("\nposition " + GetPosition());
        res += string.Format("\nvelocity " + GetVelocity());
        res += string.Format("\nid " + GetID().ToString("X8"));
        return res;

    }
    public RefSoundCtrWrapper()
    {
        zero = Vector3.zero;
        pos = null;
    }

    Vector3 zero;
    public Vector3 position;
    public Vector3 velocity;
    public MATRIX pos;
    public DWORD id;

    //public Vector3 GetPosition() { return position != null ? position : zero; }
    public Vector3 GetPosition() { 
        if (pos!=null) //TODO звук должен идти из места звука, а не от места владелцьа
        {
            return pos.Org; 
        }
        return position != null ? position : zero; 
    }

    public void SetPosition(Vector3 inWorld)
    {
        position = inWorld;
    }
    public Vector3 GetVelocity() { return velocity != null ? velocity : zero; }
    public DWORD GetID() { return id; }

    public void AddRef()
    {
        //noting
    }

    public int RefCount()
    {
        return 0;
    }

    public int Release()
    {
        return 0;
    }

    public bool Update(ref UpdateData u)
    {
        //return true;
        return false;
        //throw new System.NotImplementedException();
    }
    public static I3DSoundEventController CreateSoundCtrWrapper(Vector3 pos, Vector3 vel, DWORD id)
    {

        RefSoundCtrWrapper wr = new RefSoundCtrWrapper();
        wr.position = pos;
        wr.velocity = vel;
        wr.id = id;

        Debug.LogFormat("Created wr {0} #{1}" ,wr,wr.id.ToString("X8"));
        return wr;
    }

    public static I3DSoundEventController CreateSoundCtrWrapper(MATRIX pos, Vector3 vel, DWORD id)
    {

        RefSoundCtrWrapper wr = new RefSoundCtrWrapper();
        wr.pos = pos;
        //wr.position = pos;
        wr.velocity = vel;
        wr.id = id;

        Debug.Log("Created wr " + wr);
        return wr;
    }

    public void Dispose()  { }
}


//class WeaponSoundController : I3DSoundEventController
//{
//    private BaseWeaponSlot Slot;
//    private DWORD id;

//    public WeaponSoundController(BaseWeaponSlot _Slot)
//    {
//        Slot = _Slot;
//    }
//    public void AddRef()
//    {
//        return;
//    }

//    public uint GetID()
//    {
//        return id;
//    }

//    public Vector3 GetPosition()
//    {
//        //TODO стрёмный вариант. Правильнее использовать отдельный метод для BaseWeaponSlot*.
//        if (Slot.GetType() == typeof(BaseWeaponSlotBarrel)) return ((BaseWeaponSlotBarrel)Slot).InWorld;
//        if (Slot.GetType() == typeof(BaseWeaponSlotGun)) return ((BaseWeaponSlotGun)Slot).InWorld;
//        if (Slot.GetType() == typeof(BaseWeaponSlotPlasma)) return ((BaseWeaponSlotPlasma)Slot).InWorld;
//        //Debug.Log("return " + Slot);
//        return Vector3.zero;
//    }

//    public Vector3 GetVelocity()
//    {
//        throw new System.NotImplementedException();
//    }

//    public int RefCount()
//    {
//        return 1;
//    }

//    public int Release()
//    {
//        return 0;
//    }

//    public bool Update(ref UpdateData u)
//    {
//        return true;
//    }

//    public static I3DSoundEventController CreateWeaponCtrWrapper(BaseWeaponSlot slot,Vector3 velocity,DWORD id)
//    {
//        WeaponSoundController ctr = new WeaponSoundController(slot);
//        ctr.id = id;
//        return ctr;
//    }
//}