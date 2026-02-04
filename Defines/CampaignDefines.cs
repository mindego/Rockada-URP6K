using System.Collections;
using System.Collections.Generic;

public class CampaignDefines
{
    public enum SideTable
    {
        HUMANS,
        VELIANS,
        NEUTRAL,
        ALIENS
    }

    public const uint CF_LEVEL_MASK     =            0x0000000F;
    public const uint CF_LEVEL_MISSION   =           0x00000001;
    public const uint CF_LEVEL_GROUP      =          0x00000002;
    public const uint CF_LEVEL_UNIT        =         0x00000003;

    // item classes
    public const uint CF_CLASS_MASK         =       (0x000000F0 | CF_LEVEL_MASK);
    public const uint CF_CLASS_MISSION       =      (0x00000000 | CF_LEVEL_MISSION);
    public const uint CF_CLASS_STATIC_GROUP   =     (0x00000010 | CF_LEVEL_GROUP);
    public const uint CF_CLASS_DYNAMIC_GROUP   =    (0x00000020 | CF_LEVEL_GROUP);

    public const uint CF_CLASS_STATIC        =      (0x00000010 | CF_LEVEL_UNIT);
    public const uint CF_CLASS_VEHICLE        =     (0x00000020 | CF_LEVEL_UNIT);
    public const uint CF_CLASS_CRAFT           =    (0x00000040 | CF_LEVEL_UNIT);

    public const uint CF_CLASS_AIR_CARRIER   =      (0x00000080 | CF_LEVEL_UNIT);
    public const uint CF_CLASS_SEA_CARRIER    =     (0x00000000 | CF_LEVEL_UNIT); // (0x00000100|CF_LEVEL_UNIT â include/StormGame/CampaignDefines.hpp

    public const uint CF_HIDDEN  =                   0x00001000;

    public const int CS_SIDE_HUMANS   =             1;
    public const int CS_SIDE_VELIANS   =            2;
    public const int CS_SIDE_NEUTRAL    =           0;
    public const int CS_SIDE_ALIENS      =          3;


    public const uint CUF_PLAYABLE = 0x00800000;

    // appear flags
    public const uint CF_APPEAR_MASK = 0x00000F00;
    public const uint CF_APPEAR_ONE_CLIENT = 0x00000100;
    public const uint CF_APPEAR_TWO_CLIENTS = 0x00000200;
    public const uint CF_APPEAR_THREE_CLIENTS = 0x00000400;
    public const uint CF_APPEAR_FOUR_CLIENTS = 0x00000800;

    public const uint CF_APPEAR_ARCADE = 0x01000000;
    public const uint CF_APPEAR_NORMAL = 0x02000000;
    public const uint CF_APPEAR_HARDCORE = 0x04000000;
    public const uint CF_APPEAR_NEW_FORMAT = 0x08000000;

}
