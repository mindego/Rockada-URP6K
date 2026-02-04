using DWORD = System.UInt32;
using UnityEngine;


public partial  class BaseRadar : iBaseActor //от BaseActor
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

    // от BaseActor
    public virtual bool Move(float scale)
    {
        if (pFPO != null)
        {
            pFPO.TurnRightPrec(Dt().CornerSpeed * scale);
        }
        return true;
    }
    public virtual void Update(float scale) { }
}

// базовый класс
public partial class BaseRadar : BaseSubobj, iBaseActor
{
    // от iBaseInterface
    new public const uint ID = 0x3C10116D;
    public override object GetInterface(DWORD id)
    {
        return (id == ID ? this : base.GetInterface(id));
    }

    //// от BaseItem
    //public virtual DWORD GetCreatePktLength(DWORD Code, DWORD Offset)const;
    //virtual DWORD GetCreatePkt(ItemDataPacket*, DWORD Offset)const;

    // от BaseSubobj
    public BaseRadar(BaseScene s, DWORD h, SUBOBJ_DATA d) : base(s, h, d)
    {
        //myBaseActor = new BaseActor(s);
        BaseActorInit(s);

    }
    ~BaseRadar()
    {
        Dispose();
    }

    public override void Dispose()
    {
        base.Dispose();
        BaseActorDispose();
    }
    public override void HostPrepare(HostScene s, BaseObject o, BaseSubobj so, FPO fpo, SLOT_DATA sld, int slot_id, int lay_id)
    {
        base.HostPrepare(s, o, so, fpo, sld, slot_id, lay_id);
    }
    //virtual void RemotePrepare(RemoteScene*, BaseObject*, BaseSubobj*, FPO*,const SLOT_DATA*,const SubobjCreatePacket*, DWORD);// инициализация

    // own
    public RADAR_DATA Dt<T>() where T : RADAR_DATA { return (RADAR_DATA)SubobjData; }
    public RADAR_DATA Dt() { return (RADAR_DATA)SubobjData; }
};
