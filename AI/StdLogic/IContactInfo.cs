using UnityEngine;
using crc32 = System.UInt32;
public interface IContactInfo
{
    public const uint ID=0x1F116C61;
    public int refresh(ScanArea scan_area, float radius);
    public ContactData getData(int i);
};

public class ContactData
{
    public crc32 myName;
    public Vector3 myDiff;
};

public enum ScanArea :uint
{
    saNone,
    saEnemies,
    saFriends,
    saNeutrals,
    saForceDWORD = 0xFFFFFFFF
};

