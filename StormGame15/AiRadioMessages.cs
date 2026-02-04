using DWORD = System.UInt32;
public abstract class AiRadioMessages
{
    public const int RM_NONE = 0;

    public const int RM_NOTIFY_LAND_CLEARED = 1;
    public const int RM_NOTIFY_TAKEOFF_CLEARED = 2;

    public const int RM_NOTIFY_MENU_UPDATED = 3;
    public const int RM_NOTIFY_MENU_CLEARED = 4;

    public const int RM_NOTIFY_LAND_FAILED = 5;
}
