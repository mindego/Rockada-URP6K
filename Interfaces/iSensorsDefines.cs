public class iSensorsDefines
{
    public const float MIN_THREAT_F = .01f;
    public const int CONTACT_NAME_LENGTH = 16;

    public const uint UT_AIR = 0x00000001;
    public const uint UT_GROUND = 0x00000002;
    public const uint UT_SHIP = 0x00000004;
    public const uint UT_VEHICLE = 0x00000008;

    // ****************************************************************************
    // return codes for iContact::GetState()
    public const int CS_DEAD = 0;
    public const int CS_HIDDEN = 1;
    public const int CS_IN_GAME = 100;
    public const int CS_ENTERING_HANGAR = 101;
    public const int CS_LEAVING_HANGAR = 102;
    public const int CS_IN_TUNNEL = 103;

}
