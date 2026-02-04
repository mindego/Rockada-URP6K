using System;
using UnityEngine;
using DWORD = System.UInt32;
using crc32 = System.UInt32;
/// <summary>
/// BaseCarrier - большие игровые объекты
/// </summary>
public class BaseCarrier : BaseObject, iMovementSystemCarrier
{
    // от iBaseInterface
    new public const uint ID = 0x60B4C742;
    public override object GetInterface(DWORD id)
    {
        switch (id)
        {
            case iBaseColliding.ID:
                {
                    BaseCarrier s = (BaseCarrier)this;
                    if (s.m_pColliding == null)
                        s.m_pColliding = new BaseCollidingForBaseCarrier(s);
                }
                return m_pColliding;
            case BaseCarrier.ID: return (pFPO != null ? this : null);
            case iMovementSystemCarrier.ID: return (pFPO != null ? (iMovementSystemCarrier)this : 0);
            default: return base.GetInterface(id);
        }
    }

    // от iMovementSystemCarrier
    public virtual void Pause(bool pause)
    {
        myPaused = pause;
    }
    public virtual void MoveTo(Vector3 v, float time, float max_speed)
    {
        myTargetLeader.setPtr(null);
        myTargetOrg = v;
        myTargetTime = time;
        myDestSet = true;
        myMaxSpeed = max_speed;

    }

    public virtual void NearUnit(iContact cnt, Vector3 Delta)
    {
        myTargetLeader.setPtr(cnt);
        myTargetDelta = Delta;
    }
    public virtual bool IsStopped()
    {
        return myPaused && MathF.Abs(myLSpeed.x) < 0.001f && MathF.Abs(myLSpeed.y) < 0.001f && MathF.Abs(myASpeed.x) < 0.001f && MathF.Abs(myASpeed.y) < 0.001f;
    }

    //// от BaseItem
    //public  virtual DWORD GetCreatePktLength(DWORD Code, DWORD Offset)const;
    //virtual DWORD GetCreatePkt(ItemDataPacket*, DWORD Offset)const;
    //virtual bool OnDataPacket(float PacketDelay,const ItemDataPacket*);
    //virtual DWORD GetUpdatePktData(Vector3 Org)const;
    //virtual void GetUpdatePkt(ItemDataPacket*);

    // от BaseActor
    public override bool Move(float scale)
    {
        if (base.Move(scale) == false) return false;

        if (pFPO != null)
        {
            myAutopilot.move(scale);
            rScene.UpdateHM(pHash);
        }

        return true;

    }
    public override int getUnitType() { return (int)iSensorsDefines.UT_SHIP; }


    // от iContact
    public override Vector3 GetSpeed()
    {
        return mySpeed;
    }
    public override int GetState()
    {
        return pFPO != null ? iSensorsDefines.CS_IN_GAME : iSensorsDefines.CS_DEAD;
    }

    // perfomance data
    public override bool IsSurfaced()
    {
        return (GetCarrierData().IsSeaUnit == true);
    }
    public override float GetMaxSpeed()
    {
        return GetCarrierData().MaxSpeedZ;
    }
    public override float GetMaxCornerSpeed()
    {
        return GetCarrierData().ASpeedX;
    }

    // от BaseObject
    protected virtual void BasePrepare()
    {
        // physics
        mySpeed = Vector3.zero;
        myASpeed = new Vector2(0, 0);
        myLSpeed = new Vector2(0, 0);
        myPrevSpeed = 0;
    }
    private BaseCarrierAutopilot myAutopilot;
    public BaseCarrier(BaseScene s, DWORD h, OBJECT_DATA od) : base(s, h, od)
    {
        m_pColliding = null;
        //TODO Ну вот не знаю я, как FPO правильно туда загнать в физику авианосца, nак как myPos передаётся как ссылка

        myPhys = new CarrierPhysics(myPos, myLSpeed, myASpeed, GetCarrierData());
        myMaxSpeed = 0;

    }
    public override void HostPrepare(HostScene s, UnitSpawnData sd, Vector3 v, float a, iContact hangar)
    { // инициализация на хосте
      // координаты появления
        base.HostPrepare(s, sd, v, a, hangar);
        // готовим слоты
        if ((uint)sd.Layout1Name != 0xFFFFFFFF)
        {
            LAYOUT_DATA l = ObjectData.GetLayout(LAYOUT_DATA.SLOTS_LAYOUT, (uint)sd.Layout1Name);
            if (l == null)
                throw new System.Exception(string.Format("Carrier \"{0}\": cannot find slots layout {1}", ObjectData.FullName, sd.Layout1Name));
            BaseObjectEnumSubobj Helper = new BaseObjectEnumSubobj(rScene, this, l);
            pFPO.EnumerateSlots(Helper);
        }
        myAutopilot = new LocalCarrierAutopilot(getScene(), this);
        BasePrepare();
    }
    //virtual void RemotePrepare(RemoteScene*,const ObjectCreatePacket*, DWORD); // инициализация на клиенте
    ~BaseCarrier()
    {
        myAutopilot = null;
        if (m_pColliding != null)
            m_pColliding = null;
    }

    const float zeroSpeed = .001f;
    public override float getFloat(crc32 name)
    {
        switch (name)
        {
            case AnimationParams.iSpeedName:
                return myLSpeed.x;
            case AnimationParams.iAngleSpeedName:
                return Storm.Math.RD2GRD(myASpeed.x);
            case AnimationParams.iIsBraking:
                return ((myPrevSpeed > zeroSpeed) && (myPrevSpeed > mySpeed.sqrMagnitude)) ? 1 : 0;
            case AnimationParams.iIsEngineWorking:
                return ((myPrevSpeed > zeroSpeed) && (myPrevSpeed <= mySpeed.sqrMagnitude)) ? 1 : 0;
            default:
                return base.getFloat(name);
        }
    }

    // own

    //#ifdef VIS_TARGET
    //    TRef<ILaser> myDir, myDir2;
    //#endif
    protected virtual void updateTargetDest(ref Vector3 org, ref Vector3 dir, float scale) { }

    readonly TContact myTargetLeader = new TContact();
    Vector3 myTargetDelta;

    Vector3 myTargetOrg;
    float myTargetTime;
    float myMaxSpeed;
    bool myDestSet = false;


    public void updatePhysic(float scale)
    {
        myPos = pFPO;

        Vector3 org = myPos.Org;
        Vector3 dir = myPos.Dir; dir.y = 0;
        Vector3 diff;
        float time = 0;

        if (myTargetLeader.Ptr() != null)
        {
            myTargetLeader.Validate();
            if (myTargetLeader.Ptr() != null)
            {
                org = myTargetLeader.Ptr().GetOrg() + myTargetLeader.Ptr().GetRight() * myTargetDelta.x + myTargetLeader.Ptr().GetDir() * myTargetDelta.z + myTargetLeader.Ptr().GetUp() * myTargetDelta.y;
                dir = myTargetLeader.Ptr().GetDir();
                diff = org - myPos.Org;
            }
        }
        else if (myDestSet && !myPaused)
        {
            org = myTargetOrg;
            time = myTargetTime - rScene.GetTime();
        }

        updateTargetDest(ref org, ref dir, scale);

        //# ifdef VIS_TARGET
        //        MATRIX s = *pFPO;
        //        Vector3 d = org - myPos.Org;
        //        if (d.Norma() > 0)
        //        {
        //            s.SetHorizontal(d / d.Norma());
        //            myDir->SetParams(s, d.Norma(), 5, 5, FVec4(255, 0, 0, 255));
        //        }
        //#endif
        myPhys.updateOrg(myPos);
        myPhys.update(org, time, scale, pFPO.HashRadius, dir,ref myLSpeed,ref myASpeed);

        pFPO.TurnRightPrec(myASpeed.x * scale);
        pFPO.TurnUpPrec(myASpeed.y * scale);

        pFPO.Dir.Normalize();
        pFPO.Up.Normalize();
        pFPO.Right.Normalize();

        if (myMaxSpeed > 0f && myLSpeed.x > myMaxSpeed)
            myLSpeed.x = myMaxSpeed;

        myPrevSpeed = mySpeed.sqrMagnitude;
        mySpeed = pFPO.Dir * myLSpeed.x;
        mySpeed.y += myLSpeed.y;

        pFPO.Org += mySpeed * scale;

        updateAngles();
    }
    CarrierPhysics myPhys;

    bool myPaused = false;
    protected MATRIX myPos;
    protected Vector3 mySpeed;

    protected Vector2 myLSpeed;
    float myPrevSpeed;
    protected Vector2 myASpeed;
    BaseCollidingForBaseCarrier m_pColliding;
    public CARRIER_DATA GetCarrierData() { return (CARRIER_DATA)ObjectData; }
};
