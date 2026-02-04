using crc32 = System.UInt32;

public class CmdOnNeutral : CmdOnFriendly
{

    // BaseControllerCommand
    public override crc32 getID()
    {
        string buffer = string.Format("{0}{1}", myGroup != null ? myGroup : "", (int)myRadius);
        //wsprintf(buffer, "%s%d", myGroup ? cstr(myGroup) : "", int(myRadius));
        //return Crc32.Code(0x2B49961C, buffer); //OnNeutral
        return Hasher.Code(0x2B49961C ,buffer);
    }

    public override bool setDefaults(IQuery tm)
    {
        if (base.setDefaults(tm))
        {
            myScan = ScanArea.saNeutrals;
            return true;
        }
        return false;
    }
};
