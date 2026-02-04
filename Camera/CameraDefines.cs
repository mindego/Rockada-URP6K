public class CameraDefines
{
    public const uint iCmNone = 0x806FFF30; // "none"
    // режим камеры в кокпите
    public const uint iCmCockpit = 0xA47CBB72; // "cockpit"
    public const uint iCmCockpitTracking = 0x9C39FBD7; // "cockpit_tracking"
    public const uint iCmAttached = 0x97589D77;  // "attached"
    public const uint iCmAttachedXZ = 0xC0375182;  // "attached_xz"
    public const uint iCmOrbite = 0x942FB67E;  // "orbite"
    public const uint iCmFree = 0xB2D13E37;  // "free"
    public const uint iCmMap = 0x6C525544; // "map"
    public const uint iCmTactical = 0x29697933;  // "tactical"
    public const uint iCmTacticalInversed = 0x6A95BBB7;  // "tactical_inversed"
    public const uint iCmTracking = 0x57839DE3;  // "tracking"
    public const uint iCmTV = 0x4C1385FB; // "tv"
    public const uint iCmAuto = 0x9945DA05;  // "auto"
    //mindego addon
    public const uint iCmStrategic = 0x6d2bea41;//"strategic";
    public const uint iCmManual = 0xef24413b;//"manual";
    public const uint iCmFPS = 0xaad7c80f;//"fps";

    public const int MAX_RANGES = 2;
    public const int MAX_LD_RADIUSES = 3;

    public const uint CAMERA_DATA = 0x7EBD1993;
    public const uint CAMERA_MAP_DATA = 0xDD3774A5;

}
