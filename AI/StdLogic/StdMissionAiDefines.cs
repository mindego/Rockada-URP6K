using UnityEngine;
using DWORD = System.UInt32;


public class ClientSpawnData
{
    public UNIT_DATA mpUnitData;
    public GROUP_DATA mpGroupData;
    public ClientSpawnData(UNIT_DATA dt, GROUP_DATA gr) { mpUnitData = dt; mpGroupData = gr; }
    public ClientSpawnData() { Clear(); }
    public bool IsValid() { return mpUnitData != null && mpGroupData != null; }
    public void Clear() { mpUnitData = null; mpGroupData = null; }
};

public class CameraInfo
{
    public DWORD mMode;
    public Vector3 mOrg;
    public float mHeading, mPitch, mRoll;
    public DWORD mHandle;
    public CameraInfo()
    {
        //mMode = 0x806FFF30; //#define iCmNone 0x806FFF30 // "none"
        mMode = CameraDefines.iCmNone;
        mOrg = new Vector3(5000f, 100f, 5000f);
        mHeading = 0;
        mPitch = 0;
        mRoll = 0;
        mHandle = Constants.THANDLE_INVALID;
    }
};

public class ContactInfo
{
    public const uint CI_KILLED = 0x00000001;
    public const uint CI_NOTIFY = 0x00000002;
    public const uint CI_GODNESS = 0x00000004;


    public DWORD mHandle;
    public GroupAiCont mpGroupCont;
    public DWORD mFlags;
    public DamageInfo mpDamages;
    public DWORD mLastAttacker;
    public UNIT_DATA mpData;
    public IBaseUnitAi mpAi;

    // работа с флагами
    public void SetFlag(DWORD Flag) { mFlags |= Flag; }
    public void ClearFlag(DWORD Flag) { mFlags &= ~Flag; }
    public DWORD GetFlag(DWORD Flag) { return mFlags & Flag; }
    public void SetFlag(DWORD Flag, bool on) { if (on) SetFlag(Flag); else ClearFlag(Flag); }

    public void SetGodness(bool god) { SetFlag(CI_GODNESS, god); }
    public void Kill() { SetFlag(CI_KILLED); if (mpData != null) mpData.NotLanded(); }
    public void NotifyAboutDamages(bool notify) { SetFlag(CI_NOTIFY, notify); }

    public bool IsGod() { return GetFlag(CI_GODNESS) == CI_GODNESS; }
    public bool IsKilled() { return GetFlag(CI_KILLED) == CI_KILLED; }
    public bool IsNotify() { return GetFlag(CI_NOTIFY) == CI_NOTIFY; }

    void AddDamage(DWORD damager_id, DWORD damager_handle, float damage, DWORD weapon)
    {
        DamageInfo cur;
        for (cur = mpDamages; cur != null; cur = cur.mpNext)
        {
            if (Constants.THANDLE_INVALID == cur.mDamagerID)
            {// common contact
                if (cur.mDamagerHandle == damager_handle)
                    break;
            }
            else if (cur.mDamagerID == damager_id)
                break;
        }
        if (cur == null)
        {
            cur = new DamageInfo(damager_id, damager_handle, mpDamages);
            mpDamages = cur;
        }
        cur.AddDamage(damage, weapon);

    }
    public void ClearDamages()
    {
        //DamageInfo nxt;
        for (DamageInfo cur = mpDamages, nxt; cur != null;)
        {
            nxt = cur.mpNext;
            //delete cur;
            cur = nxt;
        }
        mpDamages = null;

    }
    public void ClientDisconnecting(MissionClient attacker)
    {
        for (DamageInfo cur = mpDamages; cur != null; cur = cur.mpNext)
        {
            if (cur.mDamagerID == attacker.ID())
            {
                cur.ClearDamage();
                return;
            }
        }

    }

    public void SetLastAttacker(DWORD attacker) { mLastAttacker = attacker; }
    public DWORD GetLastAttacker() { return mLastAttacker; }

    ~ContactInfo()
    {
        Dispose();
    }

    private bool isDisposed=false;
    public void Dispose()
    {
        if (isDisposed) return;
        ClearDamages();
        mpAi = null;
        mpGroupCont = null;
        isDisposed = true;
    }
    public ContactInfo(IBaseUnitAi ai, DWORD hndl, UNIT_DATA dt, GroupAiCont grp)
    {
        mpAi = (ai);
        mHandle = (hndl);
        mpGroupCont = (grp);
        mLastAttacker = (Constants.THANDLE_INVALID);
        mpDamages = null;
        mpData = (dt);
        mFlags = (0);
    }
};

public class DamageInfo
{
    public DamageInfo mpNext;
    public float mDamage;
    public DWORD mDamagerID;
    public DWORD mDamagerHandle;
    public DWORD mWeapon;
    public DamageInfo(DWORD damager_id, DWORD damager_handle, DamageInfo nxt)
    {
        mDamagerID = (damager_id);
        mDamagerHandle = (damager_handle);
        mpNext = (nxt);
        mDamage = (0f);
        mWeapon = (Constants.THANDLE_INVALID);
    }


    public void AddDamage(float dam, DWORD weapon) { mDamage += dam; mWeapon = weapon; }
    public void ClearDamage() { mDamage = -1f; mDamagerHandle = Constants.THANDLE_INVALID; mDamagerID = Constants.THANDLE_INVALID; mWeapon = Constants.THANDLE_INVALID; }
};

class MissionTerminator
{
    DWORD mName;
    bool mReleased;
    bool mMissionTerminated;
    bool mTerminate;
    bool mCallQuit;
    bool mMissionCalled;
    float mQuitTime;
    float mCallTime;
    DWORD mCode;
    StdMissionAi mpMission;

    public MissionTerminator(DWORD name,
                    float call_time,
                    float quit_time,
                    StdMissionAi mp,
                    bool terminate,
                    bool call_quit,
                    DWORD code)
    {
        mName = name;
        mCallTime = call_time;
        mQuitTime = quit_time;
        mpMission = mp;
        mMissionTerminated = false;
        mTerminate = terminate;
        mMissionCalled = false;
        mCallQuit = call_quit;
        mCode = code; mReleased = false;

    }
    public DWORD Code() { return mCode; }
    public DWORD Name() { return mName; }
    public bool Update(float scale)
    {
        if (false == mMissionTerminated)
        {    // если еще не закончена миссия
            mQuitTime -= scale;
            mCallTime -= scale;
            if (false == mMissionCalled && mCallTime < 0f)
            {
                mMissionCalled = true;
                if (mCallQuit)
                    mpMission.OnQuit(mName, mCode);
            }
            if (mQuitTime < 0f)
                mMissionTerminated = true;
            return true;
        }
        else
            return !mTerminate;

    }
    public void Release() { mReleased = true; }
    public bool IsReleased() { return mReleased; }
};

public class LogHolder
{
    LOG mpLog;
    public DWORD mID;
    public LogHolder(DWORD id, LOG lg)
    {
        mID = id;
        mpLog = lg;
    }
    void Clear()
    {
        if (mpLog != null)
        {
            //mpLog.Release();
            mpLog = null;
        }
    }
    ~LogHolder()
    {
        Clear();
    }
};

public class IPHolder
{
    //SUB!
    //IP mMask;
    //IP mAddr;

    //IPHolder() { }
    //IPHolder(IP& ip, IP& mask) :mAddr(ip),mMask(mask) { }

    //bool IsEqual(IP ip, IP mask) { return (mAddr == ip && mMask == mask); }

    //bool MatchByMask(IP ip)
    //{
    //    return (mAddr.dwIP | mMask.dwIP) == (ip.dwIP | mMask.dwIP);
    //}
};



public class LogLimitsInfo
{
    bool myTimeLimited;
    float myMaxTime;
    float myCurrentTime;

    public LogLimitsInfo()
    {
        myTimeLimited = false;
        myMaxTime = 0;
    }
    public void setMaxTime(float sz) { myCurrentTime = sz; myMaxTime = sz; myTimeLimited = myMaxTime > 0; }
    public bool processTime(float scale)
    {
        bool ret = false;
        if (myTimeLimited)
        {
            myCurrentTime -= scale;
            if (myCurrentTime < 0)
            {
                myCurrentTime = myMaxTime;
                ret = true;
            }
        }
        return ret;
    }
};


public static class ModuleApi
{
    public static bool checkEvent(ILog log)
    {
        return true;
    }

    public static IAiModule CreateAiModule(LOG log, int Version)
    {
        //if (Version == IAiModuleCreate.AI_MODULE_INTERFACE_VERSION && checkEvent(GetLog()))
        if (Version == IAiModuleCreate.AI_MODULE_INTERFACE_VERSION && checkEvent(log))
            return new StdAiModule(log);
        if (log != null)
            log.Message("Error : incompatible version of module interface");
        return null;
    }

}

// ---------------------------------------------------------------------
// Section : menu adder
// ---------------------------------------------------------------------
class AiMenuAdder
{

    public static void addItemEqual(ref int item_count, ref AiMenuItem item, DWORD menu_code, bool enabled)
    {
        AiMenuAdder.addItem(ref item_count, ref item, menu_code, menu_code, enabled);
    }
    public static void AddItem(ref int item_count, ref AiMenuItem item, DWORD menu_code, DWORD message_code, bool enabled)
    {
        AiMenuAdder.addItem(ref item_count, ref item, menu_code, message_code, enabled);
    }
    public static void AddItem(ref int item_count, ref AiMenuItem item, DWORD code, RadioMessage msg, bool enabled)
    {
        AiMenuAdder.addItem(ref item_count, ref item, code, code, enabled, null, msg);
    }
    public static void AddItem(ref int item_count, ref AiMenuItem item, DWORD menu_code, string frm, bool enabled)
    {
        AiMenuAdder.addItem(ref item_count, ref item, menu_code, menu_code, enabled, frm);
    }
    public static AiMenuItem GetCurrentItem(int item_count, AiMenuItem item)
    {
        return (item_count > 0) ? item : null;
    }
    private static void addItem(ref int item_count, ref AiMenuItem item, DWORD menu_code, DWORD msg_code, bool enabled, string frm = null, RadioMessage msg = null)
    {
        if (item_count > 0)
        {
            item.pFormat = frm;
            item.myEnabled = enabled;
            if (msg != null)
                if (msg != null)
                    item.mMessage = msg;
                else
                    item.mMessage.Code = msg_code;
            //item++.MenuID = menu_code; //TODO это указатель, так что сюда должен передаваться _массим_ AiMenuItemов. Нужно исправить!
            item_count--;
        }
    }
};
