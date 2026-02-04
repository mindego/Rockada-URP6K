using System.Collections;
using UnityEngine;
using crc32 = System.UInt32;
using DWORD = System.UInt32;

public interface IMissionAi : IAi
{
    new public const uint ID = 0xA6B7E201;

    // mission special
    public IBaseUnitAi CreateUnitAi(UNIT_DATA ud, int side, iContact hangar, IGroupAi grp);
    public IGroupAi GetGroupByID(DWORD id);
    public int UnitExistsByIndex(DWORD grp_id, DWORD un_index);
    public int UnitExistsByHandle(DWORD grp_id, DWORD un_handle);
    public GROUP_DATA GetExistedGroupData(DWORD grp_id);
    public bool GetContactInfo(iContact info, out DWORD grp_id, out DWORD un_index, out DWORD side);
    public IGame GetIGame();
    public MARKER_DATA GetMarkerData(DWORD mrk_id);
    public string IsPlayer(UNIT_DATA ud, ref string real_player_name);

    // messages
    public bool RadioChannelIsFree();
    public void Say(DWORD side_code, string pFormat, RadioMessage pData);
    public AiEventInfo GetMessageCode(string code);
    public bool GetMessageMode(DWORD side);
    public void setFcScale(float scale);

    // network callbacks
    public void ServerShutdown();
    public bool OnConnect(iClient cl);
    public void OnDisconnect(iClient cl, bool dropped);
    public void OnRemoteCommand(iClient pClient, DWORD CommandCode, string pArg1, string pArg2);
    public void OnRemoteTrigger(iClient pClient, DWORD TriggerCode, bool IsOn);
    public int GetMenuItems(iClient pClient, uint id, AiMenuItem[] ami, int max_count, int page_index);
    public int SelectItem(iClient pClient, uint id);

    // damages
    public bool RegisterDamageUser(IBaseUnitAi ai);
    public bool UnRegisterDamageUser(IBaseUnitAi ai);
    public float QueryDamage(DWORD VictimHandle, DWORD GadHandle, DWORD WeaponCode, float Damage);
    public void OnAddDamage(DWORD VictimHandle, DWORD GadHandle, DWORD WeaponCode, float Damage, bool IsFinal);
    public void OnDeleteContactHandle(DWORD handle, bool landed_or_repaired);
    int GetMarkersCount();

    // campaign support
    public DWORD CountClientsAround(Vector3 org, float radius);

    // repair
    public DWORD GetRepairBase(ref RepairBaseInfo[] RepairBase, DWORD len, iContact leader, IBaseUnitAi ai, bool nearest, bool report_about_nothing, DWORD land_flags);

    // players
    public void GetPlayersInfo(out int first, out int second);

    public void GetPlayersInfo(out iAboutPlayer info);

    // message processor
    public IMessageProcessor MessageProcessor();
    public bool presentMessage(IGroupAi ai, crc32 callsign_name, crc32 msg, bool sender, int index = -1);
    public bool isRestartSupported();

    public bool canDrawEnemy();
    public bool canUseOtherSideData();

}
public interface IAi : IObject
{
    new public const DWORD ID = 0xA2F9371A;
    public void ProcessRadioMessage(string msg_code, IAi caller, RadioMessage Info, bool to_all = false, bool say_flag = true);
    public bool Update(float scale);
    //bool canUseOtherSideData();
};

public class AiData
{
    public string name;
    public crc32 code;
    public int lev_id;
    public AiModuleData dll;
    public AiData(string _name, int _lev_id, AiModuleData _dll, crc32 _code)
    {
        name = _name;
        lev_id = _lev_id;
        dll = _dll;
        code = _code;
    }

    public bool Equal(int _lev_id, crc32 new_code) { if (new_code == code && lev_id != 0 & _lev_id != 0) return true; else return false; }
};

public class AiModuleData
{
    public const uint AI_MODULE_INTERFACE_VERSION = 0x499046C1;
    public const int MAX_FILE_NAME = 256;
    //char[] filename;// = new char[MAX_FILE_NAME];
    public string filename;
    //HINSTANCE handle;
    public IAiModule ai_module;
    public AiModuleData(string name)
    {
        // handle = null;
        ai_module = null;
        filename = name;
    }
    //: handle(0),ai_module(0) { StrnCpy(filename, name, MAX_FILE_NAME); }

    public bool Initialize(IServer server, IAiRegister aireg, ILog log)
    {

        ai_module = ModuleApi.CreateAiModule((LOG)log, (int)AI_MODULE_INTERFACE_VERSION); //он в DLL stdlogic
        if (ai_module != null)
        {
            ai_module.Initialize(server, aireg);
            return true;
        }

        return false;
    }

    ~AiModuleData()
    {
        Dispose();
    }
    public void Dispose()
    {
        if (ai_module != null) ai_module.Release();
        //if (handle) FreeLibrary(handle);
    }
};

public interface IGroupAi : IAi
{
    new public const DWORD ID = 0xA53761B7;

#if _DEBUG
    public void showTargets(bool show)=0;
#endif //_DEBUG

    // group special
    public GROUP_DATA GetGroupData();
    public bool HaveHangars(DWORD side_code, DWORD action_flags);
    public iContact GetLeaderContact();
    public IBaseUnitAi GetNextUnit(IBaseUnitAi bua);
    public Vector3 GetLeaderOrg();
    public iSensors GetLeaderSensors();
    public IBaseUnitAi GetUnitAiByData(UNIT_DATA ud);
    public UNIT_DATA GetUnitDataByIndex(DWORD index);
    public bool PlayerIsLeader(DWORD caller_index);
    public iEventDesigner GetEventDesigner();

    // set skill
    public void SetSkill(DWORD skill);

    // menu
    public int GetMenuItems(IBaseUnitAi ai, uint id, AiMenuItem ami, int max_count, int page_index);
    public int SelectItem(IBaseUnitAi ai, uint id);

    // report
    public IGame GetIGame();
    public void OnSwapUnit(IBaseUnitAi old_ai, IBaseUnitAi new_ai);
    public void OnCreateUnit(IBaseUnitAi ai);

    // campaign support
    public bool DeleteWithoutClients();

    public void DeleteUnitAfterDeath(UNIT_DATA ud);
    public void RespawnUnitAfterDeath(UNIT_DATA ud, RespawnType rt, RespawnInfo ri, OverwriteType ot);
};


public class RespawnInfo : UnitSpawnData
{
    //public float mRespawnTime;
    private float mRespawnTimeProxy;
    public float mRespawnTime
    {
        get { return mRespawnTimeProxy; }
        set {
            //Debug.LogFormat("mRespawnTime is changed from {0} to {1}",mRespawnTimeProxy,value);
            mRespawnTimeProxy = value;
        }
    }
    public Vector3 mOrg;
    public float mRespawnAngle;
    public DWORD mBase;
    public void Merge(RespawnInfo od) { mRespawnTime = od.mRespawnTime; mOrg = od.mOrg; mBase = od.mBase; mRespawnAngle = od.mRespawnAngle; }
    public void Merge(UnitSpawnData od)
    {
        ObjectName = od.ObjectName;
        Layout1Name = od.Layout1Name;
        Layout2Name = od.Layout2Name;
        Layout3Name = od.Layout3Name;
        Layout4Name = od.Layout4Name;
        SideCode = od.SideCode;
    }

    public override string ToString()
    {
        string res = "";
        res += "Name = " + ObjectName.ToString("X8") + "\n";
        res += "Time =" + mRespawnTime;
        return res;
    }

    public RespawnInfo()
    {
        mBase = Constants.THANDLE_INVALID;
        mRespawnTime = -1.0f;
        mRespawnAngle = 0f;
    }
    //:mBase(THANDLE_INVALID),mRespawnTime(-1.f),mRespawnAngle(0.f) { }
    public RespawnInfo(RespawnInfo od)
    {
        Merge((UnitSpawnData)od);
        Merge(od);
    }
    public RespawnInfo(UnitSpawnData od) { Merge(od); }
}
// ******************************************************************************************
// IGroupAi
public enum RespawnType
{
    rtDead,
    rtRespawnFromTime,
    rtRespawnNever
};

public enum OverwriteType : uint
{
    otOverwriteFull,
    otOverwriteUnitSpawnData,
    otOverwriteRespawnInfo,
    otOverwriteNone,
    otForceDword = Constants.THANDLE_INVALID
};

public class AiMenuItem
{
    public DWORD MenuID;
    public string pFormat;
    public RadioMessage mMessage;
    public bool myEnabled;
};

public class RadioMessage
{

    public const uint RMF_SAY = 0x00000001;
    public const uint RMF_LOG = 0x00000002;
    public const uint RMF_CONSOLE = 0x00000004;
    public const uint RMF_LOCALIZE = 0x00000008;
    public const uint RMF_TEAM = 0x00000010;
    public const uint RMF_ALL = 0x00000020;
    public DWORD Code;
    public DWORD mFlags;
    public int VoiceCode;
    public string CallerCallsign;
    public int CallerIndex;
    public string RecipientCallsign;
    public int RecipientIndex;
    public Vector3 Org;
    public DWORD myPhraseIndex;
    public iContact TargetContact;
    public IBaseUnitAi TargetAi;
    public string String1;
    public string String2;

    // flags
    void SetFlag(DWORD Flag) { mFlags |= Flag; }
    void ClearFlag(DWORD Flag) { mFlags &= ~Flag; }
    public DWORD GetFlag(DWORD Flag) { return mFlags & Flag; }

    //RadioMessage(): Code(0),CallerCallsign(0),CallerIndex(0),RecipientCallsign(0),RecipientIndex(0),TargetContact(0),TargetAi(0),VoiceCode(0),
    //String1(0),String2(0),myPhraseIndex(0),mFlags(RMF_SAY | RMF_LOG | RMF_LOCALIZE) { }
    public RadioMessage()
    {
        mFlags = RMF_SAY | RMF_LOG | RMF_LOCALIZE;
    }
    public RadioMessage(RadioMessage m) { Copy(m); }
    void Copy(RadioMessage m)
    {
        if (m == null) return;
        Code = m.Code;
        mFlags = m.mFlags;
        VoiceCode = m.VoiceCode;
        CallerCallsign = m.CallerCallsign;
        CallerIndex = m.CallerIndex;
        RecipientCallsign = m.RecipientCallsign;
        RecipientIndex = m.RecipientIndex;
        Org = m.Org;
        myPhraseIndex = m.myPhraseIndex;
        TargetContact = m.TargetContact;
        TargetAi = m.TargetAi;
        String1 = m.String1;
        String2 = m.String2;
    }
    public void SetSay(bool nd) { if (nd) SetFlag(RMF_SAY); else ClearFlag(RMF_SAY); }
    public void SetLog(bool nd) { if (nd) SetFlag(RMF_LOG); else ClearFlag(RMF_LOG); }
    public void SetConsole(bool nd) { if (nd) SetFlag(RMF_CONSOLE); else ClearFlag(RMF_CONSOLE); }
    public void SetLocalize(bool nd) { if (nd) SetFlag(RMF_LOCALIZE); else ClearFlag(RMF_LOCALIZE); }
    public void setTeamFlag(int fl) { if (fl == 0) ClearFlag(RMF_TEAM | RMF_ALL); else SetFlag((uint)fl); }

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendFormat("{0} id {1}\n", GetType().ToString(), GetHashCode().ToString("X8"));
        sb.AppendFormat("mFlags: {0}\n", mFlags.ToString("X8"));
        sb.AppendFormat("Code: {0}\n", AIGroupsEvents.CodeToString(Code));
        if (CallerCallsign != null)
            sb.AppendFormat("Caller: {0} {1}\n", CallerCallsign, CallerIndex);
        else
            sb.AppendFormat("Caller: Anonymous\n");

        sb.AppendFormat("Org: {0}\n", Org);
        if (RecipientCallsign != null)
            sb.AppendFormat("Recipient: {0} {1}", RecipientCallsign, RecipientIndex);
        else
            sb.AppendFormat("Recipient: All");

        if (String1 != null) sb.Append(String1 + "\n");
        if (String2 != null) sb.Append(String2 + "\n");


        return sb.ToString();
    }
}
public interface IBaseUnitAi : IAi
{
    //enum StateFlag
    //{
    //    sfInit,
    //    sfRun,
    //    sfDone
    //};

    new public const DWORD ID = 0xA1E051E7;

    // self
    public IGroupAi GetIGroupAi();
    public iSensors GetSensors();
    public UNIT_DATA GetUnitData();
    // group env
    public iContact GetContact();
    public iContact GetTarget();

    public float GetAiming();
    public void SetSkill(DWORD skill, bool already_set = false);
    public DWORD GetSkill();
    public void SideChanged(iContact new_cnt);
    public void Suicide();
    public void OnDamage(DWORD gad, float damage, bool fatal);
    public void OnKill(DWORD victim);
    public void SetMessagesMode(bool silence);

    // targets
    public void SelectTarget(int nTargets, iContact[] Targets, float[] TargetWeights);

    // automats
    //AI_STATE iState;
    public delegate bool AI_STATE(StateFlag flag, float scale);
    //private bool SafeCallUpdate(StateFlag flag, float scale) { if (iState) return (this->* iState)(flag, scale); return false; }
    //private bool CallUpdate(StateFlag flag, float scale) { return (this->* iState)(flag, scale); }
    //private void SetState(AI_STATE new_state) { SafeCallUpdate(sfDone, 0); iState = new_state; CallUpdate(sfInit, 0); }
    //IBaseUnitAi() :iState(0) { }
    public bool SafeCallUpdate(StateFlag flag, float scale);
    public bool CallUpdate(StateFlag flag, float scale);
    public void SetState(AI_STATE new_state);

    public bool IsHangaringBeforeLastValidate();

    public void enumTargets(ITargetEnumer en);
};

public interface ITargetEnumer
{
    public bool processTarget(iContact cnt, float aim);
};


public interface IAiModule : IMemory
{
    public bool Initialize(IServer iserver, IAiRegister air);
    public IMissionAi CreateMissionAi(crc32 crc, IGame game, IServer server, AiMissionData data);
    public IGroupAi CreateGroupAi(crc32 crc, GROUP_DATA gd, IMissionAi mi);
    public IBaseUnitAi CreateUnitAi(crc32 crc, UNIT_DATA ud, iContact c, IGroupAi ga, IGame g);
    public LOG GetLog();
};

/// <summary>
/// IVehicleAi
/// </summary>
interface IVehicleAi
{
    public const uint ID = 0x4C499A85;
    public bool SetFightMode(DWORD mode);
    public bool SetUseRoadsMode(DWORD mode);
};

public interface IUnitAi
{
    public const uint ID = 0x3AF5BF36;

    // unit special
    public bool setFormation(iContact c, Vector3 delta, float dist, DWORD formation_name);
    public bool SetDestination(Vector3 org, float time);
    public Vector3 GetDestination();
    public bool JoinFormation(JoinOption join);
    public void Pause(bool pause);
    public void setSpeed(float spd);
};

//#define BREAK_LAND_DISTANCE    6000.f
//#define SCAN_LAND_DISTANCE     4000.f

public interface IHangarAi
{
    public const float BREAK_LAND_DISTANCE = 6000f;
    public const float SCAN_LAND_DISTANCE = 4000f;
    public const uint ID = 0xAC718E93;
    /// <summary>
    /// запрос на взлет
    /// </summary>
    /// <param name="grp_ai"></param>
    /// <param name="ud"></param>
    /// <param name="appear"></param>
    /// <returns></returns>
    public iContact RequestToTakeoff(IGroupAi grp_ai, UNIT_DATA ud, out bool appear);

    /// <summary>
    /// запрос на посадку
    /// </summary>
    /// <param name="grp_ai"></param>
    /// <param name="bua"></param>
    /// <param name="repair"></param>
    public void RequestToLand(IGroupAi grp_ai, IBaseUnitAi bua, bool repair);
    /// <summary>
    /// получить кол-во ждущих в очереди
    /// </summary>
    /// <returns></returns>
    public int GetCountToLand();
    /// <summary>
    /// может ли данный юнит взлететь
    /// </summary>
    /// <param name="gd"></param>
    /// <param name="ud"></param>
    /// <param name="immediate"></param>
    /// <returns></returns>
    public bool AbleToTakeOff(GROUP_DATA gd, UNIT_DATA ud, bool immediate = true);
    /// <summary>
    /// может ли данный юнит сесть
    /// </summary>
    /// <param name="cnt"></param>
    /// <param name="immediate"></param>
    /// <returns></returns>
    public bool AbleToLand(iContact cnt, bool immediate = true);
    public bool IsLandWork();
    public bool IsTakeoffWork();
    public void SetTakeoffMember(iContact _ai);
    public void emulateTakeoffRequest(IGroupAi gai, string cs);
    public int wantToTakeoff(crc32 callsign, int idx);
};


public enum JoinOption
{
    joNone,
    joImmediately,
    joTurn,
    joFreeFly
};

public enum StateFlag
{
    sfInit,
    sfRun,
    sfDone
};

public class RepairBaseInfo
{
    public DWORD mID;
    public Vector3 mOrg;
    public void Clear() { mID = Constants.THANDLE_INVALID; }
    public float GetDist(Vector3 org) { Vector3 diff = org - mOrg; diff.y = 0; return diff.magnitude; }
    public Vector3 GetNearOrg(Vector3 org, float h, float max_l)
    {
        Vector3 diff = mOrg - org;
        diff.y = 0;
        float len = diff.magnitude;
        if (len < 0.01f)
            diff = Vector3.forward;
        else
            diff /= len;
        diff = org + diff * (len - max_l);
        diff.y = h;
        return diff;
    }
};

public class AiEventInfo
{
    public DWORD mEventCode;
    public DWORD mCode;
    public float myFreeRadioTime;
    public float mDispersion;
    public AiEventInfo() : this(0, 0, 0f, 0f)
    {
        //mEventCode = 0;
        //mCode = 0;
        //myFreeRadioTime = 0f;
        //mDispersion = 0f;
    }
    public AiEventInfo(DWORD event_code, DWORD code, float free_radio_time, float disp)
    {
        mEventCode = event_code;
        mCode = code;
        myFreeRadioTime = free_radio_time;
        mDispersion = disp;
    }
};


