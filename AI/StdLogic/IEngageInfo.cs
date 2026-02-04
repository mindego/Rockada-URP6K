using crc32 = System.UInt32;
public interface IEngageInfo
{
    public const uint ID = 0x2C7C97F2;
    public int refresh(EngageType type);
    public EngageData getData(int i);
};

public enum EngageType:uint
{
    etNone,
    etAny,
    etWeEngage,
    etTheyEngage,
    etForceDWORD = 0xFFFFFFFF
};

public class EngageData
{
    public crc32 myName;
    public bool myFriendly;
};

