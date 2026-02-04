using UnityEngine;
using DWORD = System.UInt32;

public partial class BaseSubSfg: iBaseActor
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

public partial class BaseSubSfg : BaseSubobj, iBaseActor
{
    // IBaseInterface
    public override object GetInterface(DWORD id)
    {
        switch (id)
        {
            default: return base.GetInterface(id);
        }
    }
    public override object queryObject(DWORD id, int num)
    {
        switch (id)
        {
            case iSfg.ID: return mySfg;
            default: return base.queryObject(id, num);
        }
    }


    public BaseSubSfg(BaseScene s, DWORD h, SUBOBJ_DATA d) : base(s, h, d)
    {
        BaseActorInit(s);

        mySfg = new BaseSfg(s, (SUB_SFG_DATA) d, this);
    }

    public override void Dispose()
    {
        base.Dispose();
        BaseActorDispose();
    }

    public override void HostPrepare(HostScene s, BaseObject o, BaseSubobj so, FPO fpo, SLOT_DATA sld, int slot_id, int lay_id)
    {
        base.HostPrepare(s, o, so, fpo, sld, slot_id, lay_id);
        BasePrepare();
    }
    //virtual void RemotePrepare(RemoteScene*, BaseObject*, BaseSubobj*, FPO*,const SLOT_DATA*,const SubobjCreatePacket*, DWORD);// �������������

    // от BaseActor
    public bool Move(float scale) { return true; }
    public void Update(float scale)
    {
        if (pFPO != null)
            mySfg.update(scale, pFPO.ToWorldPoint(Vector3.zero));
    }
    public override bool canChangeImage()
    {
        return mySfg.isImageChanged();
    }

    private BaseSfg mySfg;
    void BasePrepare()
    {
        mySfg.prepare();
    }

};
