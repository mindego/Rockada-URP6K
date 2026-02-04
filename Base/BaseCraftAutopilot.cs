using UnityEngine;
using DWORD = System.UInt32;

/// <summary>
/// BaseCraftAutopilot - базовый класс для различных стилей поведения Craft'a
/// </summary>
public class BaseCraftAutopilot
{
    // для кверенья
    public const uint ID = 0xC04D60CC;
    public virtual object GetInterface(DWORD id)
    {
        return (id == ID ? this : null);
    }
    //    template<class C> C* GetInterface() const { return (C*) GetInterface(C::ID);
    //}

    // интерфейс с BaseCraft
    protected BaseCraft Owner;

    public float myFlyDontUp;
    public BaseCraftAutopilot(BaseCraft c)
    {
        Owner = c;
        myFlyDontUp = 0;
    }
    ~BaseCraftAutopilot() { Dispose(); }

    public virtual void Dispose() { }
    public virtual bool Move(float scale, bool pred)
    {
        Vector3 spd = Owner.GetSpeedInLocal();
        Vector3 Faero = Owner.GetFaero(spd, scale);
        Vector3 Treq = Owner.GetTreqForThrust(Owner.Thrust, spd);
        Vector3 ExtRotate = Owner.ApplyForces(Faero, Treq, spd, scale);
        Owner.ApplyControls(Owner.Controls, ExtRotate, scale);
        Owner.MakeRotation(spd, scale);
        Owner.MakeMove(scale, pred);
        if (pred == true)
            Owner.CheckTerrainRough(myFlyDontUp, true);
        else
            Owner.CheckTerrainPrecise();
        return true;
    }
    public virtual int GetState()
    {
        return (Owner.pFPO != null ? iSensorsDefines.CS_IN_GAME : iSensorsDefines.CS_DEAD);
    }
    public virtual bool OnUpdate(float PacketDelay, string pPkt)
    {
        return false;
    }
    public virtual int GetUpdatePktLength()
    {
        //return sizeof(CraftUpdatePacketStd);
        return 0;
    }
    public virtual int GetUpdatePkt(string pPtr, int stamp)
    {
        //CraftUpdatePacketStd pPkt = (CraftUpdatePacketStd)pPtr;
        CraftUpdatePacketStd pPkt = new CraftUpdatePacketStd();
        pPkt.Stamp = stamp;
        pPkt.Org = Owner.pFPO.Org;
        pPkt.SpeedX = BaseCraftPackets.PackSpeed(Owner.Speed.x);
        pPkt.SpeedY = BaseCraftPackets.PackSpeed(Owner.Speed.y);
        pPkt.SpeedZ = BaseCraftPackets.PackSpeed(Owner.Speed.z);
        pPkt.Angles = BaseCraftPackets.PackAngles(Owner.HeadingAngle, Owner.PitchAngle, Owner.RollAngle);
        pPkt.Flags = 0;
        if (Mathf.Abs(Owner.ThrustOut.x) > .05) { pPkt.Flags |= 0x01; if (Owner.ThrustOut.x > 0) pPkt.Flags |= 0x02; }
        if (Mathf.Abs(Owner.ThrustOut.y) > .05) { pPkt.Flags |= 0x04; if (Owner.ThrustOut.y > 0) pPkt.Flags |= 0x08; }
        if (Mathf.Abs(Owner.ThrustOut.z) > .05) { pPkt.Flags |= 0x10; if (Owner.ThrustOut.z > 0) pPkt.Flags |= 0x20; }

        switch (GetState())
        {
            case iSensorsDefines.CS_ENTERING_HANGAR: pPkt.Flags |= 0x40; break;
            case iSensorsDefines.CS_LEAVING_HANGAR: pPkt.Flags |= 0x80; break;
            case iSensorsDefines.CS_DEAD: pPkt.Flags |= 0xC0; break;
        }
        //dprintf("sended CDP_UPDATE_STD\n");
        return (int)BaseCraftPackets.CDP_UPDATE_STD;
    }
    public virtual bool IsManual() { return true; }
    public virtual string Describe() { return "Doing nothing"; }
};
enum TakeOffStages
{
    tsWaitForDoor = 0, tsRun = 1, tsClimb = 2, tsFlight = 3
}

enum LandingStages
{
    lsCrosswind = 0, lsDownwind = 1, lsFinal = 2, lsRun = 3, lsWait = 4
}

// BaseCraftAutopilotRemote - интерполяция
//class BaseCraftAutopilotRemote : public BaseCraftAutopilot
//{

//  // для кверенья
//public:
//  IID(0xEB116405);
//virtual void* GetInterface(DWORD)const;

//// интерфейс с BaseCraft
//public:
//  BaseCraftAutopilotRemote(BaseCraft* cr);
//virtual bool Move(float scale, bool pred);
//virtual int GetState()const;
//virtual bool OnUpdate(float PacketDelay,const char* pPkt);

//// свое
//protected:
//  SmootherOrientation<SmootherOrg2> mSmoother;
//Vector3 mThrust;
//int mState;
//int myTimeStamp;
//bool mManual;
//public:
//  void SetManual(bool manual) { mManual = manual; }
//void SetState(int NewState);
//virtual bool IsManual() const { return mManual; }
//};
