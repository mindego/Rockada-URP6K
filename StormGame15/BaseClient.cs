using System;
using System.IO;
using UnityEngine;
using static BaseClientState;
/// <summary>
/// BaseClient - реализация iBaseClient для listen server
/// </summary>
public abstract class BaseClient : iClient, TLIST_ELEM<BaseClient>,IDisposable
{

    // от iClient
    public virtual object Query(uint cls_id)
    {
        return null;
    }
    public virtual string GetPlayerName() { return pPlayerName; }
    public virtual string SetPlayerName(string s)
    {
        pPlayerName = s;
        return pPlayerName;
    }
    public virtual void SetLog(LOG log)
    {
        mpLog = log;
    }
    public virtual bool Disconnect()
    {
        SetState(csDead);
        return true;
    }
    public virtual bool IsHidden()
    {
        return false;
    }

    // для HostScene
    public virtual IBaseUnitAi Capture(iContact c, UNIT_DATA data, IGroupAi grp)
    {
        return null;
    }
    public virtual bool Update(float scale)
    {
        return (GetState() != csDead);
    }
    //virtual void ItemUpdate(DWORD d1, DWORD d1, BaseItem*);
    //virtual void SendItemCreate(const BaseItem*);
    public virtual void SendItemData(ItemDataPacket idp) { }
    //public virtual void SendData();
    //public virtual void Send(GamePacket* p);
    public bool IsInGame() { return (State == csInGame); }

    public abstract void applyCraftSelection(UnitSpawnData usd);
    // own
    private BaseClientState State;
    protected bool mySwapped = false;
    protected HostScene rScene;
    protected string pPlayerName;
    protected ILog mpLog;
    float myPing;

    void setPing(float ping) { myPing = ping; }

    protected virtual void SetState(BaseClientState s)
    {
        if (State == s) return;
        switch (s)
        {
            case csInGame:
                State = csInGame;
                if (rScene.GetMissionAI().OnConnect(this) == false)
                    State = csDead;
                break;
            case csDead:
                {
                    BaseClientState old_state = State;
                    State = csDead;
                    if (old_state == csInGame)
                        rScene.GetMissionAI().OnDisconnect(this, false);
                }
                break;
            default:
                State = s;
                break;
        }
    }
    BaseClientState GetState() { return State; }
    public BaseClient(HostScene s)
    {
        rScene = s;
        State = csConnected;
        rScene.AddClient(this);
    }
    ~BaseClient()
    {
        Asserts.AssertBp(State == csDead);
        rScene.SubClient(this);

    }
    void setSwapComplete()
    {
        mySwapped = true;
    }

    //TLIST_ELEM
    protected BaseClient next, prev;
    public void SetPrev(BaseClient c)
    {
        prev = c;
    }

    public void SetNext(BaseClient c)
    {
        next = c;
    }

    public BaseClient Next()
    {
        return next;
    }

    public BaseClient Prev()
    {
        return prev;
    }

    public abstract byte[] GetIpAddress();
    public abstract IBaseUnitAi GetAI();
    public virtual string GetAiName()
    {
        return null;
    }
    public abstract UnitSpawnData GetUnitSpawnData();
    public abstract UnitSpawnData SetUnitSpawnData(UnitSpawnData usd);
    public abstract void showEngBay(uint nCrafts, uint[] pCrafts, uint nWeapons, uint[] pWeapons);
    public abstract bool IsInEngBay();
    public abstract void hideEngBay();
    public abstract float GetCameraRange();
    public abstract uint GetCameraMode();
    public abstract uint GetCameraTarget();
    public abstract void SetCameraMode(uint Mode, uint Target);
    public abstract Vector3 GetCameraOrg();
    public abstract void GetCameraAngles(out float HeadingAngle, out float PitchAngle, out float RollAngle);
    public abstract void SetCameraPos(Vector3 Org, float HeadingAngle, float PitchAngle, float RollAngle);
    public abstract void SetCameraDraw(bool draw);
    public abstract uint RegisterRemoteConsoleCommand(string pName, int nArgs);
    public abstract uint RegisterRemoteConsoleTrigger(string pName);
    public abstract void UnRegisterRemoteConsole(uint d);
    public abstract void Message(string s);
    public abstract void SayMessage(string pFormat, RadioMessage pData);
    public abstract void UpdateMenu(bool HasNewItem);
    public abstract void CreateDevice(uint id, ClientDeviceDataCreate pData);
    public abstract void UpdateDevice(uint id, ClientDeviceData pData);
    public abstract void DeleteDevice(uint id);
    public abstract bool isSwapped();

    public virtual void Dispose()
    {
        throw new NotImplementedException();
    }
}
