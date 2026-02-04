using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// VehicleSoundController
/// </summary>
public class VehicleSoundController : I3DSoundEventController
{

    // от IObject
    public virtual int Release()
    {
        if (--mCounter > 0) return mCounter;
        this.Dispose();
        return 0;
    }
    public virtual void AddRef()
    {
        mCounter++;
    }

    // от I3DSoundEventController
    public virtual Vector3 GetPosition()
    {
        return mrVehicle.pFPO.Org;
    }
    public virtual Vector3 GetVelocity()
    {
        return mrVehicle.Speed;
    }
    public virtual DWORD GetID()
    {
        //return (DWORD)mrVehicle.pFPO.GetHashCode();
        return (DWORD)mrVehicle.pFPO;
    }
    public virtual bool Update(ref UpdateData dt )
    {
        Asserts.AssertBp(mrVehicle.pFPO != null);
        dt.frequency = mrVehicle.GetThrustCoefficient();
        return true;
    }

    public int RefCount()
    {
        return mCounter;
    }

    public void SetPosition(Vector3 inWorld)
    {
        throw new System.NotImplementedException();
    }

    // own
    private BaseVehicle mrVehicle;
    private int mCounter;
    public VehicleSoundController(BaseVehicle pVehicle)
    {
        mrVehicle = pVehicle;
        mCounter = 1;
    }

    ~VehicleSoundController() {
        Dispose();
    }

    public void Dispose()  {
        Debug.Log("Disposed of " + this);
    }
}