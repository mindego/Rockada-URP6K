using UnityEngine;
using crc32 = System.UInt32;
using DWORD = System.UInt32;

public interface IGame
{
    // system
    public float Time();
    public ILog Log();
    // data load
    public string GetObjectDataName(DWORD name);
    public DWORD ObjectDataExists(DWORD name);
    public FormationInfo GetFormationInfo(ObjId id);
    public PatrolInfo GetPatrolInfo(ObjId id);
    public string AddLocalizedText(ref string pDst, int DstLength, DWORD Code,string  pString = null);
    // environment
    public float GetGroundLevel(Vector3 org);
    public float GetGroundLevelMedian(Vector3 org, float r);
    public bool RadioChannelIsFree();
    public int getPhrasesCount(crc32 code);
    public GameOptions getGameOptions();
};

public interface IAiRegister
{
    public bool RegisterAi(IAiModule module, string name, int lev_id); // true if new
};

public interface IServer
{
    // ai
    public IGroupAi CreateGroupAi(string name, GROUP_DATA data);
    public IBaseUnitAi CreateUnitAi(string name, UNIT_DATA data, iContact contact, IGroupAi grp);
    public IBaseUnitAi CreateUnitAi(iClient client, UNIT_DATA data, iContact contact, IGroupAi grp);
    public iContact CreateUnit(UnitSpawnData data, Vector3 org, float angle, iContact hangar = null);
    public void DeleteUnit(iContact i, bool explode);
    public int GetTimeScale();
    public int SetTimeScale(int sc);
    public int GetDifficulty();
    // environment
    public IDataHasher GetDataHash();
    public CommandsApi GetCommands();
    public iUnifiedVariableContainer GetCurrentEventData();
    public iUnifiedVariableContainer GetOptions(string pCtrName = "");
    // contacts
    public iContact GetContactByHandle(DWORD hndl);
};