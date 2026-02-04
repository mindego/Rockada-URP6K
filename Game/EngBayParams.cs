using crc32 = System.UInt32;

public struct EngBayParams
{
    public int nCrafts;
    public crc32[] pCrafts;
    public int nWeapons;
    public crc32[] pWeapons;
    public crc32[] mySelection; //

    bool isWeaponAllowed(crc32 name)
    {
        for (int i = 0; i < nWeapons; ++i)
            if (pWeapons[i] == name)
                return true;
        for (int i = 0; i < 3; ++i)
            if (mySelection[i + 1] == name)
                return true;
        return false;
    }

    bool isCraftAllowed(crc32 name)
    {
        for (int i = 0; i < nCrafts; ++i)
        {
            if (pCrafts[i] == name)
                return true;
        }
        return mySelection[0] == name;
    }
};
