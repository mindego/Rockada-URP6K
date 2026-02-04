using System.Collections.Generic;
using UnityEngine;
using DWORD = System.UInt32;
using WORD = System.UInt16;

public class MissionClient
{

    public const uint MC_SRV_ADMIN = 0x00000001;
    public const uint MC_SRV_RESPAWN = 0x00000002;
    public const uint MC_SRV_RUNNING = 0x00000004;
    public const uint MC_SRV_CAMERADRAW = 0x00000008;

    WORD mClientID;
    iClient mpClient;
    IGroupAi mpGroup;
    ClientSpawnData mSpawnData = new ClientSpawnData();
    public EnumPosition mpPosition;
    DWORD mFlags;
    float mConnectTime;
    DWORD mTeam;
    DWORD mChangedTeam;
    readonly TContact mContact = new TContact();
    DWORD mHandle;
    bool mInEngbay;
    int myStatus;
    public DWORD Handle() { return mHandle; }
    public iContact Contact() { return mContact.Ptr(); }
    public void SetContact(iContact cnt)
    {
        //mContact = new TContact(cnt);
        mContact.setPtr(cnt);
        mHandle = cnt != null ? cnt.GetHandle() : Constants.THANDLE_INVALID;
    }
    public void Killed()
    {
        //mContact = null;
        mContact.setPtr(null);
    }
    bool IsKilled() { return mContact.Ptr() == null; }
    public int ID() { return mClientID; }
    public DWORD Team() { return mTeam; }
    public string Name() { return mpClient.GetPlayerName(); }
    void ChangeTeam(DWORD team) { mChangedTeam = team; }
    void TeamChanged() { mTeam = mChangedTeam; }
    bool IsCurrentTeamValid()
    {
        return IsTeamValid(mTeam);
    }
    bool IsFutureTeamValid()
    {
        return mChangedTeam != Constants.THANDLE_INVALID;

    }
    DWORD FutureTeam() { return mChangedTeam; }

    public iClient Client() { return mpClient; }
    public IGroupAi Group() { return mpGroup; }
    public void SetGroup(IGroupAi grp) { mpGroup = grp; }
    public EnumPosition Position() { return mpPosition; }

    // работа с флагами
    public void SetFlag(DWORD Flag) { mFlags |= Flag; }
    public void ClearFlag(DWORD Flag) { mFlags &= ~Flag; }
    public DWORD GetFlag(DWORD Flag) { return mFlags & Flag; }
    public void SetFlag(DWORD Flag, bool on) { if (on) SetFlag(Flag); else ClearFlag(Flag); }

    bool IsAdmin() { return GetFlag(MC_SRV_ADMIN) == MC_SRV_ADMIN; }
    void SetAdmin(bool on) { SetFlag(MC_SRV_ADMIN, on); }

    bool IsCameraDraw() { return GetFlag(MC_SRV_CAMERADRAW) == MC_SRV_CAMERADRAW; }
    public void SetCameraDraw(bool on) { SetFlag(MC_SRV_CAMERADRAW, on); mpClient.SetCameraDraw(on); }

    public bool CanRespawn() { return GetFlag(MC_SRV_RESPAWN) == MC_SRV_RESPAWN; }
    public void SetCanRespawn(bool on) { SetFlag(MC_SRV_RESPAWN, on); }

    bool IsClientRunning() { return GetFlag(MC_SRV_RUNNING) == MC_SRV_RUNNING; }
    void ClientRunning(bool on) { SetFlag(MC_SRV_RUNNING, on); }

    List<EnumPosition> mplPositions;

    float GetConnectTime() { return stdlogic_dll.mCurrentTime - mConnectTime; }

    public UNIT_DATA GetUnitData()
    {
        //if (mSpawnData == null) mSpawnData = new ClientSpawnData();//TODO Ленивая инициализации ClientSpawnData здесь быть не должна.
        //Debug.Log("mSpawnData " + mSpawnData);
        return mSpawnData.mpUnitData;
    }
    public GROUP_DATA GetGroupData() { return mSpawnData.mpGroupData; }
    public void ClearSpawnData() { mSpawnData.Clear(); }
    public void SetSpawnData(ClientSpawnData sp)
    {
        Debug.Log("Setting mSpawnData");
        mSpawnData = sp;
    }
    public void SetCamera(CameraInfo info)
    {
        Debug.Log(string.Format("mpClient [{0}] info [{1}]", mpClient, info));
        mpClient.SetCameraPos(info.mOrg, info.mHeading, info.mPitch, info.mRoll);
        mpClient.SetCameraMode(info.mMode, info.mHandle);

    }

    float getConnectTime() { return mConnectTime; }


    bool isEngBayClosed()
    {
        bool ret = false;
        if (mInEngbay)
        {
            if (!mpClient.IsInEngBay())
            {
                setInEngbay(false);
                ret = true;
            }
        }
        return ret;
    }
    void setInEngbay(bool fl) { mInEngbay = fl; }
    bool isInEngBay() { return mInEngbay; }

    public MissionClient(WORD id, iClient _client, ref List<EnumPosition> _positions)
    {
        mpClient = _client;
        mplPositions = _positions;
        mFlags = MC_SRV_CAMERADRAW;
        mTeam = Constants.THANDLE_INVALID;
        mChangedTeam = Constants.THANDLE_INVALID;
        mClientID = id;
        mpPosition = new EnumPosition();
        mplPositions.Add(mpPosition);
        mpGroup = null;
        SetContact(null);
        mInEngbay = false;
        mConnectTime = stdlogic_dll.mCurrentTime;

    }
    ~MissionClient()
    {
        if (mpPosition != null)
            mplPositions.Remove(mpPosition);

    }

    static bool IsTeamValid(DWORD team) { return team != Constants.THANDLE_INVALID; }

    public int setStatus(int status) { return myStatus = status; }
    public int getStatus() { return myStatus; }
};
