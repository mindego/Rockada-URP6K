using crc32 = System.UInt32;

public class CmdOnContact : BaseContactCommand
{

    // BaseCommand
    public override bool exec()
    {
        if (myTimer.getTime() < myStartTime) return false;
        if (myTimer.getTime() < myCheckTime) return false;
        int n = myInfo.refresh(ScanArea.saEnemies, myRadius);
        for (int i = 0; i < n; ++i)
        {
            ContactData data = myInfo.getData(i);
            Asserts.Assert(data!=null && data.myName != CRC32.CRC_NULL);
            if (checkContact(data))
            {
                refreshTimer();
                    return true;
            }
        }
        refreshCheckTimer(3f);
        return false;
    }

    public override bool restart()
    {
        bool ret = base.restart();
        if (ret && Storm.Math.cmpFloat(myPeriod, 0, 1e-6f))
            myController.shutdown();
        return ret;
    }

    // BaseControllerCommand
    public override crc32 getID()
    {
        string buffer = string.Format("{0}{0}{0}",myGroup,myPeriod*10,myRadius);
        //wsprintf(buffer, "%s%d%d", myGroup ? cstr(myGroup) : "", int(myPeriod * 10), int(myRadius));
        //return Crc32.Code(0xC30F95D4, buffer); //OnContact
        return Hasher.Code(0xC30F95D4, buffer);
    }


    public override bool setDefaults(IQuery tm)
    {
        myRadius = 60000f;
        myStartTime = 0f;
        return base.setDefaults(tm);
    }

    float myStartTime;
    void refreshTimer() { myStartTime = myTimer.getTime() + myPeriod; }
};
