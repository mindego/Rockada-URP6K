using DWORD = System.UInt32;

public class BaseDetached : BaseSubobj
{
    public BaseDetached(BaseScene s, DWORD h, SUBOBJ_DATA d) : base(s, h, d)
    {
        mpHash = null;
        mpColliding = null;
    }
    ~BaseDetached()
    {
        Clean();
        //SafeRelease(pFPO);
    }

    public override object GetInterface(DWORD id)
    {
        switch (id)
        {
            case iBaseColliding.ID:
                {
                    BaseDetached d = (BaseDetached)this;
                    if (d.mpColliding == null) d.mpColliding = new BaseCollidingFPO<BaseDetached>(d);
                    return mpColliding;
                }
            default: return base.GetInterface(id);
        }
    }
    public override void HostPrepare(HostScene s, BaseObject o, BaseSubobj so, FPO fpo, SLOT_DATA sld, int slot_id, int lay_id)
    {
        base.HostPrepare(s, o, so, fpo, sld, slot_id, lay_id);
        BasePrepare();
    }

    //virtual void RemotePrepare(RemoteScene*, BaseObject*, BaseSubobj*, FPO*,const SLOT_DATA*,const SubobjCreatePacket*, DWORD);
    public override void Explode(bool CanStaySelf, bool CanKeepChildren)
    {
        Clean();
        base.Explode(false, CanKeepChildren);
    }
    public override void Delete(bool ShouldTryToLeft)
    {
        Clean();
        base.Delete(false);
    }
    private void BasePrepare()
    {
        pFPO.ToWorld();
        pFPO.Detach();

        pFPO.SetFlag(HashFlags.OF_GROUP_COLLIDABLE2);
        mpHash = rScene.CreateHM(pFPO);
    }
    HMember mpHash;
    void Clean()
    {
        if (mpHash != null)
        {
            rScene.DeleteHM(mpHash);
            mpHash = null;
        }
    }
    BaseCollidingFPO<BaseDetached> mpColliding;
};
