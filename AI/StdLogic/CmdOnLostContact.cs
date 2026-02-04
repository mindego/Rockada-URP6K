using crc32 = System.UInt32;

public class CmdOnLostContact : BaseContactCommand
{
    // IVmCommand
    public override bool exec()
    {
        if (myTimer.getTime() < myCheckTime) return false;
        int n = myInfo.refresh(ScanArea.saEnemies, myRadius);
        int count = 0;
        for (int i = 0; i < n; ++i)
        {
            ContactData data = myInfo.getData(i);
            Asserts.Assert(data != null && data.myName != CRC32.CRC_NULL);
            if (checkContact(data))
                count++;
        }
        refreshCheckTimer(3f);
        if (count == 0)
            return myActive ? true : false;
        else
            myActive = true;
        return false;
    }

    public override bool restart()
    {
        myActive = false;
        return base.restart();
    }

    // BaseControllerCommand
    public override crc32 getID()
    {
        //char buffer[64];
        //wsprintf(buffer, "%s%d", myGroup ? cstr(myGroup) : "", int(myRadius));
        //return Crc32.Code(0xA82CF680, buffer); //OnLostContact
        return Hasher.Code(0xA82CF680,myGroup != null ? myGroup : "" + myRadius);
    }

    public override bool setDefaults(IQuery tm)
    {
        myRadius = 60000f;
        return base.setDefaults(tm);
    }

    bool myActive;
};

