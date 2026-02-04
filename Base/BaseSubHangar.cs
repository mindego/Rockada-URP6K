using UnityEngine;
using DWORD = System.UInt32;

public partial class BaseSubHangar : iBaseActor
{
    //От iBaseActor

    BaseActor myBaseActor;
    public void BaseActorInit(BaseScene s)
    {
        myBaseActor = new BaseActor(s);
        myBaseActor.SetOwner(this);
    }

    public void BaseActorDispose()
    {
        myBaseActor.Dispose();
    }

    //public void SetNext(iBaseActor t)
    //{
    //    myBaseActor.SetNext(t);
    //}

    //public void SetPrev(iBaseActor t)
    //{
    //    myBaseActor.SetPrev(t);
    //}

    //iBaseActor TLIST_ELEM<iBaseActor>.Next()
    //{
    //    return myBaseActor.Next();
    //}

    //iBaseActor TLIST_ELEM<iBaseActor>.Prev()
    //{
    //    return myBaseActor.Next();
    //}
}
public partial class BaseSubHangar : BaseSubobj
{
    // IBaseInterface
    public override object GetInterface(DWORD id)
    {
        switch (id)
        {
            case BaseHangar.ID: return mHangar;
            default: return base.GetInterface(id);
        }
    }
    public override object queryObject(DWORD id, int num)
    {
        switch (id)
        {
            case iSubHangar.ID: return mHangar;
            default: return base.queryObject(id, num);
        }
    }

    public bool OnDataPacket(float PacketDelay, ItemDataPacket pPtr)
    {
        //TODO! Реализовать обработку пакетов данных
        return true;
        //if (mHangar.onDataPacket(pPtr.Type))
        //    return true;
        //return base.OnDataPacket(PacketDelay, pPtr);
    }

    public BaseSubHangar(BaseScene s, DWORD h, SUBOBJ_DATA d) : base(s, h, d)
    {
        mHangar = new BaseHangar(s, ((SUB_HANGAR_DATA)d).myHangarData, this);
        //rScene.ActorsList.AddLast(this);
        BaseActorInit(s);
    }

    public override void Dispose()
    {
        base.Dispose();
        BaseActorDispose();
    }

    public override void HostPrepare(HostScene s, BaseObject o, BaseSubobj so, FPO fpo, SLOT_DATA sld, int slot_id, int lay_id) {
        base.HostPrepare(s, o, so, fpo, sld, slot_id, lay_id);
        Debug.Log("Subhangar preparing");
        BasePrepare();
    }

    ////virtual void RemotePrepare(RemoteScene*, BaseObject*, BaseSubobj*, FPO,SLOT_DATA,SubobjCreatePacket*, DWORD);// инициализация
    //virtual bool OnDataPacket(float PacketDelay,const ItemDataPacket* pPtr);
    // от BaseActor
    public bool Move(float scale) {
        return true; 
    }
    public void Update(float scale)
    {
        //Debug.Log("Processing hangaring duties");
       mHangar.ProcessHangar(scale); 
    }
    public override float getFloat(DWORD name)
    {
        if (mHangar.GetDoor() == null) return 0f;
        switch (name)
        {
            case AnimationParams.iIsOperating:
                return (mHangar.getStatus() != HangarStatus.hsClosed || mHangar.GetDoorSpeed() != 0) ? 1:0;
            case AnimationParams.iCycledCounter:
                return getTimer(mHangar.getStatus(), 4);
            case AnimationParams.iBlinkCounter:
                return getTimer(mHangar.getStatus(), 1);
            case AnimationParams.iIsDoorOperating: return (mHangar.GetDoorSpeed() != 0) ? 1:0;
            case AnimationParams.iIsBfLanding: return (mHangar.getStatus() == HangarStatus.hsBFLanding) ? 1:0;
            case AnimationParams.iIsIntLanding: return (mHangar.getStatus() == HangarStatus.hsIntLanding) ? 1:0;
            default: return base.getFloat(name);
        }
    }
    private BaseHangar mHangar;

    float getTimer(HangarStatus hs, float max_value)
    {
        switch (hs)
        {
            case HangarStatus.hsClosed: return -1;
            //case HangarStatus.hsTakeoffing: return getPeriodic(getScene().GetTime(), max_value);
            //case HangarStatus.hsBFLanding: case HangarStatus.hsIntLanding: return getPeriodicInv(getScene().GetTime(), max_value);
            case HangarStatus.hsTakeoffing: return PeriodicCounter.getPeriodic(rScene.GetTime(), (int) max_value);
            case HangarStatus.hsBFLanding: case HangarStatus.hsIntLanding: return PeriodicCounter.getPeriodicInv(rScene.GetTime(), (int) max_value);
        }
        return 0;
    }
    void BasePrepare()
    {
        SUB_HANGAR_DATA sb = (SUB_HANGAR_DATA)this.SubobjData;
        //mHangar.PrepareHangar(sb.mySoundName, sb.myDoorName, pFPO);
        mHangar.PrepareHangar(Owner.ObjectData.FullName, sb.myDoorName, pFPO); //TODO! Возможно, для СН потребуется передавать имя именно звукового объекта
    }
};
