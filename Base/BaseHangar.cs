using UnityEngine;
using UnityEngine.Assertions;
using DWORD = System.UInt32;

public class BaseHangar : iSubHangar
{

    // от iBaseInterface
    public const uint ID = 0x25A6E15D;
    //friend class BaseHangarSlotHelper;

    // от iHangar

    int getAcceptFlag(HangarSlotData i)
    {
        return
            (int)((i.myHaveLand ? iSubHangar.CAN_ACCEPT_LAND : 0) |
            (i.myHaveTakeoff ? iSubHangar.CAN_ACCEPT_TAKEOFF : 0));
    }
    public virtual int canHandleUnit(DWORD CodedName)
    {
        if (pDoor != null)
        {
            HangarSlotData i = getDataFor(CodedName);
            return i != null ? getAcceptFlag(i) : 0;
        }
        else
            return 0;
    }
    public virtual int canHandleUnit(iContact pUnit)
    {
        if (pDoor != null)
        {
            HangarSlotData i = getDataFor(pUnit);
            return i != null ? getAcceptFlag(i) : 0;
        }
        else
            return 0;
    }
    public virtual bool isDoorOpened()
    {
        return ((DoorMaxAngle - DoorAngle) < .01f);
    }
    public virtual bool isDoorClosed()
    {
        return (DoorAngle < .01f);
    }
    public virtual iContact getIContact()
    {
        //Debug.Log("myOwner: " + myOwner);
        //Debug.Log("Contact: " + myOwner.getContact());
        return myOwner.getContact();
    }

    //TODO - возможно, стоит скорость двери сделать константой?
    private float DoorCornerSpeed = Storm.Math.GRD2RD(45f);
    private float DoorMaxAngle = Storm.Math.GRD2RD(90f);

    // свое
    //public	bool onDataPacket(int id);
    /// <summary>
    /// стартовое положение
    /// </summary>
    /// <returns></returns>
    public virtual Vector3 GetPointForStart()
    {
        if (pSelf == null) return myBaseSlotOrg;
        return pSelf.ToWorldPoint(myBaseSlotOrg);
    }

    /// <summary>
    /// конечное положение
    /// </summary>
    /// <param name="cm"></param>
    /// <param name="land"></param>
    /// <returns></returns>
    public virtual Vector3 GetPointFor(iContact cm, bool land)
    {
        if (pSelf == null) return myBaseSlotOrg;
        HangarSlotData i = getDataFor(cm);
        if (i != null)
            return pSelf.ToWorldPoint(land ? i.myLandOrg : i.myTakeoffOrg);
        else
            return myBaseSlotOrg;
    }

    void openDoorImm()
    {
        DoorTargetAngle = DoorMaxAngle;
        if (DoorStart!=null) DoorStart.Start();
    }

    void closeDoorImm()
    {
        setStatus(HangarStatus.hsClosed);
        DoorTargetAngle = 0;
        if (DoorStart!=null) DoorStart.Start();
    }
    /// <summary>
    /// открыть дверь ангара
    /// </summary>
    public virtual void OpenDoor()
    {
        //Debug.Log("Opening door");
        openDoorImm();
        //if (rHScene.IsHost())
        //{
        //    ItemDataPacket Pkt(myOwner->GetHandle(), SDP_HANGAR_OPEN_DOOR,sizeof(ItemDataPacket));
        //    rHScene.SendItemData(&Pkt);
        //    //dprintf("%f sended SDP_HANGAR_OPEN_DOOR\n",rHScene.GetTime());
        //}
    }

    // закрыть дверь ангара
    public virtual void CloseDoor()
    {
        closeDoorImm();
        //if (rHScene.IsHost())
        //{
        //    ItemDataPacket Pkt(myOwner->GetHandle(), SDP_HANGAR_CLOSE_DOOR,sizeof(ItemDataPacket));
        //    rHScene.SendItemData(&Pkt);
        //    //dprintf("%f sended SDP_HANGAR_CLOSE_DOOR\n",rHScene.GetTime());
        //}
    }
    public virtual float GetDoorSpeed()
    {
        return (DoorTargetAngle - DoorAngle);
    }
    /// <summary>
    /// ангару @#^#@^
    /// </summary>
    public virtual void Shutdown()
    {
        pDoor = null;
    }
    public virtual bool ProcessHangar(float scale)
    {
        //Debug.Log(string.Format("pDoor {0} haveBays {1}",pDoor,mHaveBays));
        if (!mHaveBays)
        {
            Debug.Log("No bays for " + pSelf.Top().TextName);
            return true;
        }
        if (pDoor == null) return false;
        // вращаем дверь
        if (pDoor != null)
        {
            float l = DoorCornerSpeed * scale;
            float d = Mathf.Clamp(DoorTargetAngle - DoorAngle, -l, l);
            if (d != .0f)
            {
                DoorAngle += d;
                pDoor.Dir = DoorDir;
                pDoor.Right = DoorRight;
                pDoor.TurnRightPrec(DoorAngle);
                pSelf.RecalcRadius();
                // если звук стоял, запускаем
                if (DoorMoving == false)
                {
                    DoorMoving = true;
                    if (DoorSound!=null) DoorSound.Start();
                }
            }
            else
            {
                // если звук играл, останавливаем
                if (DoorMoving == true)
                {
                    DoorMoving = false;
                    if (DoorSound!=null) DoorSound.Stop();
                    if (DoorStop!=null) DoorStop.Start();
                }
            }
        }
        return true;
    }
    public virtual bool PrepareHangar(string ObjectName, string door_name, FPO mp)
    {
        Debug.Log("Preparing Hangar");
        pSelf = mp;
        if (pSelf == null) return false;
        // ищем координаты
        BaseHangarSlotHelper h = new BaseHangarSlotHelper(this, pHangarData);
        pSelf.EnumerateSlots(h);

        Asserts.AssertEx(myHaveBase);
        checkData(myBFSlot);
        checkData(myIntSlot);
        checkData(myVehicleSlot);
        //Debug.Log(string.Format("mHaveBays {0} set for {1}",mHaveBays.ToString(),mp.Top().TextName));

        // ищем дверь
        pDoor = (FPO)(pSelf.GetSubObject(door_name));
        if (pDoor == null)
        {
            Debug.Log(string.Format("Door [{0}] not found for {1}",door_name,mp.Top().TextName));
            return false;
        }

        Debug.Log(string.Format("Setting door {0} params for {1} success ", pDoor.TextName + " " + pDoor.Name.ToString("X8"), mp.Top().TextName));
        DoorDir = pDoor.Dir;
        DoorRight = pDoor.Right;
        DoorPos = pDoor.Parent.ToWorldPoint(pDoor.Org);
        // создаем звук
        if (rHScene.GetSceneVisualizer() != null)
        {

            /*
            GSParam sound_params (true, false,&DoorPos,0,0,0);
            GSParam sound_params1(false,false,&DoorPos,0,0,0);
            GSParam sound_params2(false,false,&DoorPos,0,0,0);
                */
            //I3DSoundEventController* ctr =
            //    CreateSoundCtrWrapper(&DoorPos, 0, (DWORD)pSelf->Top());

            //DoorSound = rHScene.GetSceneVisualizer()->Get3DSound()->LoadEvent(
            //            "Hangar", ObjectName, "Move", true, false, ctr);

            //DoorStart = rHScene.GetSceneVisualizer()->Get3DSound()->LoadEvent(
            //            "Hangar", ObjectName, "Open", false, false, ctr);

            //DoorStop = rHScene.GetSceneVisualizer()->Get3DSound()->LoadEvent(
            //            "Hangar", ObjectName, "Close", false, false, ctr);

            //ctr->Release();

            Vector3 zero = Vector3.zero;
            I3DSoundEventController ctr = RefSoundCtrWrapper.CreateSoundCtrWrapper(DoorPos, zero, (DWORD)pSelf.Top());
            DoorSound = rHScene.GetSceneVisualizer().Get3DSound().LoadEvent("Hangar", ObjectName, "Move", true, false, ctr);
            DoorStart = rHScene.GetSceneVisualizer().Get3DSound().LoadEvent("Hangar", ObjectName, "Open", false, false, ctr);
            DoorStop = rHScene.GetSceneVisualizer().Get3DSound().LoadEvent("Hangar", ObjectName, "Close", false, false, ctr);
        }
        return true;

    }
    public virtual FPO GetDoor()
    {
        return pDoor;
    }

    public virtual void setStatus(HangarStatus st) { myStatus = st; }
    public HangarStatus getStatus() { return myStatus; }

    // работа с подобъектами
    protected BaseScene rHScene;
    protected readonly HangarData pHangarData;
    protected bool mHaveBays;
    protected FPO pDoor;
    protected float DoorAngle, DoorTargetAngle;
    protected Vector3 DoorDir, DoorRight;
    protected Vector3 DoorPos;

    // own
    private HangarStatus myStatus;
    private BaseSubobj myOwner;
    private FPO pSelf;

    public HangarSlotData myBFSlot;
    public HangarSlotData myIntSlot;
    public HangarSlotData myVehicleSlot;
    public Vector3 myBaseSlotOrg;
    public bool myHaveBase;
    public void setBaseOrg(Vector3 org) { myBaseSlotOrg = org; myHaveBase = true; }
    private void checkData(HangarSlotData data)
    {
        if (data.myInfo.myLandSlot != 0xFFFFFFFF)
        {
            //AssertEx(data.myHaveLand);
            Assert.IsTrue(data.myHaveLand);
        }
        if (data.myInfo.myTakeoffSlot != 0xFFFFFFFF)
        {
            Assert.IsTrue(data.myHaveTakeoff);
        }
        mHaveBays = true;

    }

    I3DSoundEvent DoorSound;
    I3DSoundEvent DoorStart;
    I3DSoundEvent DoorStop;

    //AudioClip DoorSound;
    //AudioClip DoorStart;
    //AudioClip DoorStop;

    bool DoorMoving;

    public HangarSlotData getDataFor(iContact c)
    {
        if (c.GetInterface(BaseVehicle.ID) != null)
            return myVehicleSlot;

        BaseCraft cr = (BaseCraft)c.GetInterface(BaseCraft.ID);
        return cr != null
            ? (cr.Dt<CRAFT_DATA>().IsPlane ? myIntSlot : myBFSlot)
            : null;
    }
    public HangarSlotData getDataFor(DWORD CodedName)
    {
        OBJECT_DATA d = OBJECT_DATA.GetByCode(CodedName, false);
        if (d != null)
        {
            if (d.GetClass() == OBJECT_DATA.OC_VEHICLE)
                return myVehicleSlot;

            if (d.GetClass() != OBJECT_DATA.OC_CRAFT)
                return null;

            return ((CRAFT_DATA)d).IsPlane ?
                myIntSlot : myBFSlot;
        }
        else
            return null;
    }

    public BaseHangar(BaseScene s, HangarData d, BaseSubobj owner)
    {
        pDoor = null;
        DoorAngle = 0;
        DoorTargetAngle = 0;
        pHangarData = d;
        DoorSound = null;
        DoorStart = null;
        DoorStop = null;
        myOwner = owner;
        DoorMoving = false;
        rHScene = s;
        mHaveBays = false;
        myHaveBase = false;
        myStatus = HangarStatus.hsClosed;

        myBFSlot = new HangarSlotData();
        myIntSlot = new HangarSlotData();
        myVehicleSlot = new HangarSlotData();

        myBFSlot.myInfo = pHangarData.myBF;
        myIntSlot.myInfo = pHangarData.myInt;
        myVehicleSlot.myInfo = pHangarData.myVehicle;
    }
    ~BaseHangar()
    {

        //TODO: Выгружать звуки
        //SafeRelease(DoorSound);
        //SafeRelease(DoorStart);
        //SafeRelease(DoorStop);
    }

    public bool onDataPacket(uint id)
    {
        switch (id)
        {
            case BaseStaticPackets.SDP_HANGAR_OPEN_DOOR:
                //dprintf("%f recieved SDP_HANGAR_OPEN_DOOR\n",rHScene.GetTime());
                Debug.Log(string.Format("{0} recieved SDP_HANGAR_OPEN_DOOR",rHScene.GetTime()));
                OpenDoor();
                return true;
            case BaseStaticPackets.SDP_HANGAR_CLOSE_DOOR:
                Debug.Log(string.Format("{0} recieved SDP_HANGAR_CLOSE_DOOR", rHScene.GetTime()));
                //dprintf("%f recieved SDP_HANGAR_CLOSE_DOOR\n",rHScene.GetTime());
                CloseDoor();
                return true;
        }
        return false;
    }
};
