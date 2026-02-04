using DWORD = System.UInt32;
//using AI_STATE = System.Boolean;
public static class AIGroupsEvents
{
    // message events
    public const int EMPTY_CODE = 0;
    public const string EMPTY_EVENT = null;

    // player command events
    public const string ORDER_ATTACK = "ORDER_ATTACK";
    public const string ORDER_ENGAGE = "ORDER_ENGAGE";
    public const string ORDER_COVER = "ORDER_COVER";
    public const string ORDER_DEFEND = "ORDER_DEFEND";
    public const string ORDER_FORMATION = "ORDER_FORMATION";
    public const string ORDER_LAND = "ORDER_LAND";
    public const string ORDER_REPAIR = "ORDER_REPAIR";


    // unit events
    public const string DONE_ATTACK = "DONE_ATTACK";
    public const string DONE_JOINF = "DONE_JOINF";
    public const string START_ATTACK = "START_ATTACK";
    //public const string DONE_ATTACK      = "DONE_ATTACK";
    public const string CONFIRM_ORDER = "CONFIRM_ORDER";
    public const string HIT_LIGHT = "HIT_LIGHT";
    public const string HIT_HEAVY = "HIT_HEAVY";
    public const string HIT_FATAL = "HIT_FATAL";
    public const string REQUEST_HELP = "REQUEST_HELP";
    public const string LOOSE_FORMATION = "LOOSE_FORMATION";
    public const string TIGHT_FORMATION = "TIGHT_FORMATION";

    // special events
    public const string WARN_PLAYER = "WARN_PLAYER";

    public const string INFO_CARRIERSTOP = "INFO_CARRIERSTOP";

    // group events
    public const string ENTER_FIGHT = "ENTER_FIGHT";
    public const string CLEAR_FIGHT = "CLEAR_FIGHT";
    public const string LOST_UNIT = "LOST_UNIT";
    public const string RETURN_BASE = "RETURN_BASE";
    public const string NONEEDED_REPAIR = "NONEEDED_REAPIR";

    public const string REQUEST_TAKEOFF = "REQUEST_TAKEOFF";
    public const string REQUEST_LAND = "REQUEST_LAND";
    public const string CLEAR_LAND = "CLEAR_LAND";
    public const string CLEAR_TAKEOFF = "CLEAR_TAKEOFF";
    public const string FAIL_LAND = "FAIL_LAND";

    public const string REQUEST_TARGETS = "REQUEST_TARGETS";


    // events
    public const uint DC_GROUP_APPEARED = 0x3DEAA898;
    public const uint DC_GROUP_ROUTE = 0x8B39EBE6;
    public const uint DC_GROUP_RETURN_BASE = 0xC9A49480;
    public const uint DC_GROUP_PARKING = 0xA76B40FF;
    public const uint DC_GROUP_LAST_POINT = 0x4673EFE3;
    public const uint DC_GROUP_PAUSED = 0xCFE4F2C0;
    public const uint DC_GROUP_RESUMED = 0x5B373A71;

    // standard message codes
    public const uint MC_GROUP_APPEAR = 0xF7C0C031;
    public const uint MC_GROUP_REPAIR = 0x15528A22;
    public const uint MC_GROUP_KILLED = 0x13FC0920;
    public const uint MC_GROUP_LANDED = 0xBCD58A91;
    public const uint MC_UNIT_KILLED = 0x84DC655E;
    public const uint MC_UNIT_LANDED = 0x2BF5E6EF;
    public const uint MC_UNIT_REPAIRED = 0xE0EA6409;
    public const uint MC_GROUP_ESCORT = 0xEC4661F7;
    public const uint MC_UNIT_APPEAR = 0x02782E1A;
    public const uint MC_UNIT_LOSTDETACHED = 0x989217D9;
    public const uint MC_GROUP_LOSTDETACHED = 0x09D44CDC;

    // HTGR code
    public const uint MC_HTGR_ROCKET_FIRED = 0x02CC10CE;

    public static bool IsValid(DWORD code) { return code != EMPTY_CODE; }

    public static string CodeToString(uint code)
    {
        switch (code)
        {
            case MC_GROUP_APPEAR: return "MC_GROUP_APPEAR";
            case MC_GROUP_REPAIR: return "MC_GROUP_REPAIR";
            case MC_GROUP_KILLED: return "MC_GROUP_KILLED";
            case MC_GROUP_LANDED: return "MC_GROUP_LANDED";
            case MC_UNIT_KILLED: return "MC_UNIT_KILLED";
            case MC_UNIT_LANDED: return "MC_UNIT_LANDED";
            case MC_UNIT_REPAIRED: return "MC_UNIT_REPAIRED";
            case MC_GROUP_ESCORT: return "MC_GROUP_ESCORT";
            case MC_UNIT_APPEAR: return "MC_UNIT_APPEAR";
            case MC_UNIT_LOSTDETACHED: return "MC_UNIT_LOSTDETACHED";
            case MC_GROUP_LOSTDETACHED: return "MC_GROUP_LOSTDETACHED";
            case MC_HTGR_ROCKET_FIRED: return "MC_HTGR_ROCKET_FIRED";
        }
        return "UNKNOWN CODE " + code.ToString("X8") + " h " + Hasher.StringHsh(code) + " c " + Hasher.StringCode(code);
        //return Hasher.StringHsh(code);
    }
}
