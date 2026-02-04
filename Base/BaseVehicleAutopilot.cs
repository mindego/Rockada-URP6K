using System;
using UnityEngine;
using DWORD = System.UInt32;

public class BaseVehicleAutopilot : IDisposable
{
    public void APDebug(string message )
    {
        return;
        Debug.Log(string.Format("AP {0} {1} {2}", this.GetType().ToString(), this.GetHashCode().ToString("X8"), message));
    }
    public const uint ID = 0x7E685D1E;
    public virtual object GetInterface(DWORD id)
    {
        return (id == ID ? this : null);
    }
    //    template<class C> C* GetInterface() const { return (C*) GetInterface(C::ID);
    //}

    // интерфейс с BaseVehicle
    protected BaseScene rScene;

    // свое
    public BaseVehicle rOwner;
    public Vector3 mDest;             // собственно точка
    public Vector3 mNowDest;          // текущая точка
    public float mTime;             // время прибытия
    public bool mUseRoads;         // использовать ли дороги
    public float mTargetSpeed;      // нужная скорость

    protected void setTargetSpeed(float speed)
    {
        mTargetSpeed = speed;
        rOwner.mTargetThrust = mTargetSpeed * rOwner.GetVehicleData().OO_MaxSpeed;
    }
    public BaseVehicleAutopilot(BaseScene sc, BaseVehicle c)
    {
        rOwner = c;
        rScene = sc;
        mTime = 555555555555f;
        mUseRoads = false;
        mTargetSpeed = 0;
    }
    ~BaseVehicleAutopilot() { Dispose(); }

    protected bool isDisposed = false;
    public virtual void Dispose() { isDisposed = true; }
    public virtual void OnSetRoute(Vector3 v, bool use_roads, float time)
    {
        mDest = v;
        mUseRoads = use_roads;
        mTime = time;
        //GetLog().Message("%p setting dest %f %f , time %f , use roads %d",this,mDest.x,mDest.y,mTime,mUseRoads);
    }
    public virtual bool Move(float scale)
    {
        return true;
    }
    public virtual int GetState()
    {
        return (rOwner.pFPO != null ? iSensorsDefines.CS_IN_GAME : iSensorsDefines.CS_DEAD);
    }
    public virtual bool IsOnBridge()
    {
        return false;
    }
    public virtual bool IsOnEarth()
    {
        return true;
    }
    //virtual bool OnUpdate(float PacketDelay,const VehicleUpdatePacket* pPkt);
};

