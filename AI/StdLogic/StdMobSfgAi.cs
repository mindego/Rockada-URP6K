public class StdMobSfgAi : StdTankAi
{
    new public const uint ID = 0xF37A5FEF;
    const string WSFGMissed = "create: can't create MobSfg without iSfg";
    bool myWaitingForStop;

    public void SetStatus(bool on_off)
    {
        if (on_off)
        {
            pause();
            myWaitingForStop = true;
        }
        else
        {
            resume();
            mySfg.TurnOff();
            myWaitingForStop = false;
        }
    }

    public StdMobSfgAi()
    {
        myWaitingForStop = false;
        mySfgUnit = new SfgUnit<StdMobSfgAi>(this);
    } 

    iSfg mySfg;

    // IAi
    public override object Query(uint cls_id)
    {
        switch ((uint)cls_id)
        {
            case ISfgUnit.ID: return mySfgUnit;
            default: return base.Query(cls_id);
        }
    }


    public override bool Update(float scale)
    {
        self.Validate();
        if (myWaitingForStop)
        {
            //if (self->GetSpeed().Norma2() < 0.01f)
            if (self.Ptr().GetSpeed().sqrMagnitude < 0.01f)
            {
                mySfg.TurnOn();
                myWaitingForStop = false;
            }
        }
        return base.Update(scale);
    }


    // IBaseAi
    public override void SetInterface(IGame igame, iContact contact, UNIT_DATA unit_data, IGroupAi grp_ai)
    {
        base.SetInterface(igame, contact, unit_data, grp_ai);
        myStdMobSFGAiFactory = Factories.createStdSFGAiFactory(getIQuery(), myBaseAiFactory);
    }

    public override void SideChanged(iContact new_cnt)
    {
        base.SideChanged(new_cnt);
        //mySfg = (iSfg) self.Ptr().queryObject<iSfg>();
        mySfg = (iSfg)self.Ptr().queryObject(iSfg.ID);
        if (mySfg==null)
            throw new System.Exception(WSFGMissed);
    }
    SfgUnit<StdMobSfgAi> mySfgUnit;
    IVmFactory myStdMobSFGAiFactory;
    public override IVmFactory getTopFactory() { return myStdMobSFGAiFactory; }

};


struct SfgUnit <TeamImp>: ISfgUnit where TeamImp: StdMobSfgAi
{
    public  void setStatus(bool on)
    {
        myMsn.SetStatus(on);
    }

    public SfgUnit(TeamImp imp) { myMsn = imp; }

    TeamImp myMsn;
};

public interface ISfgUnit : IIDable
{
    new public const uint ID=0x16403111;
    public void setStatus(bool on);
};

public interface IIDable
{
    public const uint ID = 0xBAADBEEF;
    public static uint GetIID() { return ID; }
}






