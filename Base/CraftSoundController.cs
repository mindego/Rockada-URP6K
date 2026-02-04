using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// BaseCraftEnumSubobj 
/// в исходниках коммент именно такой
/// </summary>
class CraftSoundController : I3DSoundEventController
{

    // от IObject
    public int Release()
    {
        if (--mCounter > 0) return mCounter;
        //unload data (call destructor)
        //Debug.Log("Killing CraftSoundController");
        Dispose();
        return 0;
    }
    public void AddRef() { mCounter++; }

    // от I3DSoundEventController
    public Vector3 GetPosition()
    {
        return mrCraft.pFPO.Org;
    }
    public Vector3 GetVelocity()
    {
        return mrCraft.Speed;
    }
    public DWORD GetID()
    {
        //if (mrCraft.pFPO == null) return Constants.THANDLE_INVALID; //TODO вернуть, если будут ошибки со звуком
        return (DWORD)mrCraft.pFPO;
    }
    public bool Update(ref UpdateData dt)
    {
        Asserts.AssertBp(mrCraft.pFPO != null);
        dt.frequency = mrCraft.SpeedF * mrCraft.Dt().OOMaxSpeed;
        //dt.frequency = mrCraft.ThrustOut.magnitude;
        dt.volume = (int)Mathf.Clamp(myIdleVolume + myVolumeDelta * Storm.Math.NormaFAbs(mrCraft.ThrustOut), -10000, 0);
        dt.frequency = Mathf.Clamp(dt.frequency, 0, 1);
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
    private BaseCraft mrCraft;
    int mCounter;
    float myIdleVolume, myVolumeDelta;
    public CraftSoundController(BaseCraft pCraft, float idle_volume, float max_volume)
    {
        mrCraft = pCraft;
        mCounter = 1;
        myIdleVolume = idle_volume;

        myVolumeDelta = max_volume - myIdleVolume;
    }
    ~CraftSoundController() {
        Dispose();
    }
    public void Dispose() { }
}
