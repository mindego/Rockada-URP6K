using crc32 = System.UInt32;

public class CmdOnEngage : GroupAndPeriodReact
{
    // IVmCommand
    public override  bool exec()
    {
        if (myTimer.getTime() < myStartTime) return false;
        if (myTimer.getTime() < myCheckTime) return false;
        int n = myInfo.refresh(myScan);
        for (int i = 0; i < n; ++i)
        {
            EngageData data = myInfo.getData(i);
            Asserts.Assert(data !=null && data.myName != CRC32.CRC_NULL);
            if (haveGroup(data.myName) &&
                data.myFriendly == (myFriendly!=0))
            {
                refreshTimer();
                return true;
            }
        }
        refreshCheckTimer(3f);
        return false;
    }
    EngageType getEngageType(int num)
    {
        switch (num)
        {
            case 0: return EngageType.etAny;
            case 1: return EngageType.etWeEngage;
            case 2: return EngageType.etTheyEngage;
            default: return EngageType.etNone;
        }
    }

    public override bool isParsingCorrect()
    {
        if (myController!=null && Storm.Math.cmpFloat(myPeriod, 0, 1e-6f))
            myController.shutdown();
        myScan = getEngageType(mySelfFire);
        return myScan != EngageType.etNone && base.isParsingCorrect();
    }


    // BaseCommand
    public override string  describeParams(ref string myparams)
    {
        myparams = base.describeParams (ref myparams);
        myparams = descInt (myparams,"SelfFire",mySelfFire);
        myparams = descInt (myparams,"Friendly",myFriendly);
        return myparams;
    }

    // BaseControllerCommand
    public override crc32 getID()
    {
        string buffer = string.Format("{0}{1}{2}{3}", myGroup != null ? myGroup : "", myPeriod, myFriendly, mySelfFire);
        //wsprintf(buffer, "%s%d%d%d", myGroup ? cstr(myGroup) : "", int(myPeriod), myFriendly, mySelfFire);
        //return Crc32.Code(0x30FE9386, buffer);
        return Hasher.Code(0x30FE9386, buffer);
    }


    public override IParamList.PInfo getParamInfo(crc32 param_name)
    {
        switch (param_name)
        {
            case prm_SelfFire: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int)IParamList.OpType.OP_EQU);
            case prm_Friendly: return new IParamList.PInfo(IParamList.VarType.SPT_INT, (int)IParamList.OpType.OP_EQU);
            default: return base.getParamInfo(param_name);
        }
    }

    public override bool addParameter(crc32 param_name, IParamList.OpType op, IParamList.Param p, string  real_name)
    {
        switch (param_name)
        {
            case prm_SelfFire: mySelfFire = p.myInt; break;
            case prm_Friendly: myFriendly = p.myInt; break;
            default: return base.addParameter(param_name, op, p, real_name);
        }
        return true;
    }

    int mySelfFire; const crc32 prm_SelfFire = 0x281683D0;
    int myFriendly; const crc32 prm_Friendly = 0x044782AE;



    public override bool setDefaults(IQuery tm)
    {
        base.setDefaults(tm);
        myInfo = (IEngageInfo) tm.Query(IEngageInfo.ID);
        myStartTime = 0;
        mySelfFire = 0;
        myFriendly = 0;
        return myInfo!=null;
    }
    IEngageInfo myInfo;

    float myStartTime;
    void refreshTimer() { myStartTime = myTimer.getTime() + myPeriod; }
    EngageType myScan;
};