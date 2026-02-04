using DWORD = System.UInt32;
using crc32 = System.UInt32;
using static ICoopMission.TechAccess;

public interface ICoopMission
{
    public const uint ID=0x6F1144A1;
    public void addObjective(string name, DWORD primary);
    public void addScore(int score);
    public void deleteObjective(string name);
    public void setObjective(string name, DWORD success, DWORD primary);
    public void setStatistics(bool enable);
    public void addAward(string name);
    public void setPlayerPosition(int pos);
    public void setPlayable(string group);
    public void forgetPlayer(bool forget);
    public void setGodMode(DWORD unlimited_ammo, DWORD unlimited_armor, DWORD no_ground, DWORD no_object);

    enum TechAccess
    {
        taEnabled = 0,
        taDisabled = 1,
        taDefault = 2,
        taUnknown = 3
    };

    static TechAccess getAccessFromCrc(crc32 name)
    {
        switch (name)
        {
            case 0xD31CC6BC: return taDefault;
            case 0x60BB7DE7: return taEnabled;
            case 0x8277F34A: return taDisabled;
        }
        return taUnknown;
    }

    public void setAccess(bool craft, string name, TechAccess access);
    public void enableEngBay(int enable);
};