public static class HudData
{
    public const uint HUD_TEXEL_DATA = 0x5CD3CFE1;

    // device data name defines
    public const string sDesktop = "desktop";
    public const uint iDesktop = 0xF69EFA5B;
    public const string sCompas = "compas";
    public const uint iCompas = 0xF368FEDA;
    public const string sAltimeter = "altimeter";
    public const uint iAltimeter = 0x7A559A5B;
    public const string sSpeedmeter = "speedmeter";
    public const uint iSpeedmeter = 0xBB6B11A5;
    public const string sAviaHor = "aviahor";
    public const uint iAviaHor = 0x9F3BF1B6;
    public const string sRadar = "radar";
    public const uint iRadar = 0xB4660221;
    public const string sRecticle = "recticle";
    public const uint iRecticle = 0xD73B924E;
    public const string sAimCue = "aim_cue";
    public const uint iAimCue = 0xB764F857;
    public const string sLocker = "locker";
    public const uint iLocker = 0xE1F9823F;
    public const string sColor = "color";
    public const uint iColor = 0x99A9B716;
    public const string sMap = "map";
    public const uint iMap = 0x6C525544;
    public const string sThreat = "threat";
    public const uint iThreat = 0xD368A318;
    public const string sWeapon = "weapon";
    public const uint iWeapon = 0x96CC5819;
    public const string sHorizon = "horizon";
    public const uint iHorizon = 0xF75D421B;
    public const string sTarget = "target";
    public const uint iTarget = 0xB990D003;
    public const string sDamage = "damage";
    public const uint iDamage = 0xEE37AB93;
    public const string sEnergy = "energy";
    public const uint iEnergy = 0x68EE866E;
    public const string sEnergyScale = "energyscale";
    public const uint iEnergyScale = 0xCA0CEE57;
    public const string sEnergyBar = "energybar";
    public const uint iEnergyBar = 0xE19C4348;
    public const string sTest = "test";
    public const uint iTest = 0x278081F3;
    public const string sRadio = "radio";
    public const uint iRadio = 0x1FB9E4F0;
    public const string sAiMenu = "menu";
    public const uint iAiMenu = 0x82FAC56C;
    public const string sRing = "ring";
    public const uint iRing = 0x70230A89;
    public const string sRecticles = "recticles";
    public const uint iRecticles = 0x58BF7783;
    public const string sFPS = "fps";
    public const uint iFPS = 0xAAD7C80F;
    public const string sLabel = "label";
    public const uint iLabel = 0x30998013;

    public const string sViewPort = "viewport";
    public const uint iViewPort = 0x9FD29151;

    // ****************************************************************************
    // HZ
    public const uint HUDSTATE_MASK = 0x000000FF;
    public const uint HUDSTATE_OFF = 0x00000001;
    public const uint HUDSTATE_DIGITAL = 0x00000002;
    public const uint HUDSTATE_TEXT = 0x00000002;//same as HUDSTATE_DIGITAL     
    public const uint HUDSTATE_VIEW_SCALE = 0x00000004;
    public const uint HUDSTATE_SCALE = 0x00000008;
    public const uint HUDSTATE_POINTER = 0x00000010;
    public const uint HUDSTATE_BRACKETS = 0x00000020;
    public const uint HUDSTATE_MAP = 0x00000040;
    public const uint HUDSTATE_BAR = 0x00000080;
    public const uint HUDSTATE_SCALE_MASK = 0xF0000000;
    public const uint HUDSTATE_SCALE_DEF = 0x00000000;
    public const uint HUDSTATE_SCALE_R = 0x10000000;
    public const uint HUDSTATE_SCALE_L = 0x20000000;
    public const uint HUDSTATE_SCALE_H = 0x30000000;

    public const uint HUDSTATE_TEST = 0x00080000;
}
