// may be used in hash enumeration selection
//inline int ROObjectId(int ROFID_) { return OF_GROUP_RENDER | ROFID_; }
//inline int TMTObjectId(int TMTID_) { return OF_GROUP_TMT | TMTID_; }
public class HashFlags
{
    public const uint OF_GROUP_MASK = 0xFF000000;
    public const uint OF_USER_MASK = 0x00FFFFFF;

    public const uint OF_GROUP_COLLIDABLE = 0x10000000;
    public const uint OF_GROUP_COLLIDABLE2 = 0x01000000;
    public const uint OF_GROUP_RENDER = 0x20000000;
    public const uint OF_GROUP_ROADSYSTEM = 0x40000000;
    public const uint OF_GROUP_TMT = 0x80000000;

    public static uint ROObjectId(uint ROFID_) { return (uint) (OF_GROUP_RENDER | ROFID_); }
    public static uint TMTObjectId(uint TMTID_) { return (uint) (OF_GROUP_TMT | TMTID_); }
}

//public enum HashFlags : uint
//{
//    OF_GROUP_MASK = 0xFF000000,
//    OF_USER_MASK = 0x00FFFFFF,

//    OF_GROUP_COLLIDABLE = 0x10000000,
//    OF_GROUP_COLLIDABLE2 = 0x01000000,
//    OF_GROUP_RENDER = 0x20000000,
//    OF_GROUP_ROADSYSTEM = 0x40000000,
//    OF_GROUP_TMT = 0x80000000,
//}

// may be used in hash enumeration selection
//inline int ROObjectId(int ROFID_) { return OF_GROUP_RENDER | ROFID_; }
//inline int TMTObjectId(int TMTID_) { return OF_GROUP_TMT | TMTID_; }



