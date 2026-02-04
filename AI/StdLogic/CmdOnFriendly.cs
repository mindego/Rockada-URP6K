using crc32 = System.UInt32;

public class CmdOnFriendly : BaseContactCommand
{
    public override bool exec()
    {
        if (myTimer.getTime() < myCheckTime) return false;
        int n = myInfo.refresh(myScan, myRadius);
        for (int i = 0; i < n; ++i)
        {
            ContactData data = myInfo.getData(i);
            Asserts.Assert(data!=null && data.myName != CRC32.CRC_NULL);
            if (haveGroup(data.myName) && data.myDiff.sqrMagnitude <= (myRadius * myRadius))
                return true;
        }
        refreshCheckTimer(myPeriod);
        return false;
    }

    // BaseControllerCommand
    public override crc32 getID()
    {
        string buffer = string.Format("{0}{1}", myGroup != null ? myGroup : "",myRadius);
        //wsprintf(buffer, "%s%d", myGroup ? cstr(myGroup) : "", int(myRadius));
        //return Crc32.Code(0x79FF33AA, buffer); //OnFriendly
        return Hasher.Code(0x79FF33AA,buffer);
    }

    public override bool setDefaults(IQuery tm)
    {
        if (base.setDefaults(tm))
        {
            myRadius = 3000f;
            myPeriod =  3f;
            myScan = ScanArea.saFriends;
            return true;
        }
        return false;
    }

    protected ScanArea myScan;

};