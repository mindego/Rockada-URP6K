public static class Constants
{
    public const uint THANDLE_INVALID = 0xFFFFFFFF;
    public const uint THANDLE_POS_MASK = 0x0000FFFF;
    public const uint THANDLE_FRAME_MASK = 0xFFFF0000;
    public const uint THANDLE_FRAME_UNIT = 0x00010000;
    public const uint UndefinedID = 0xFFFFFFFF;
    public const int PLAYER_NAME_LENGTH = 16;
    public const int MAX_CD_NAME_LEN = 4;
    public const float DEFAULT_WAIT_TIME = 0.01f;
    public const float DEFAULT_POST_TIME = 0.1f;
    //public const uint WeaponCodeCollisionGround = 0x95069764; // CollisionGround
    //public const uint WeaponCodeCollisionObject = 0x9A54BFE0; // CollisionObject
    //public const uint WeaponCodeUltimateDeath = 0xBBA8B36D; // UltimateDeath
    
    //Math
    public const float FLT_EPSILON = 1.192092896e-07F;

    //Univar
    //TODO перенести в правильный раздел (univars).
    public const uint UNIVARS_VERSION = 0x9A1FD1C7;  // UniVars 2.3
}

public enum SCENE_VISION
{
    SV_NORMAL,
    SV_INFRA
};

public enum HangarStatus :uint
{
    hsClosed = 0xFFFFFFFF,
    hsTakeoffing = 0,
    hsBFLanding = 1,
    hsIntLanding = 2,
    hsForceDWORD = 0xFFFF0000
};

