using System.Collections.Generic;
using UnityEngine;
using static BaseClientState;
using DWORD = System.UInt32;
/// <summary>
/// LocalClient - реализация BaseClient для listen server
/// </summary>
public class LocalClient : BaseClient, CommLink
{
    static readonly byte[] spLocalIP = { 127, 0, 0, 1 };
    const string gpEngBayName = "\\Root\\Local\\EngBay";
    const string gpIsInEngBayName = "\\Root\\Local\\EngBay\\OnScreen";
    const string gpCraftsList = "CraftsList";
    const string gpWeaponsList = "WeaponsList";
    const string gpSelectedCraft = "SelectedCraft";
    const string gpSelectedWeapon1 = "SelectedWeapon1";
    const string gpSelectedWeapon2 = "SelectedWeapon2";
    const string gpSelectedWeapon3 = "SelectedWeapon3";

    // от iClient
    // доступ к параметрам клиента
    public override byte[] GetIpAddress()
    {
        return spLocalIP;
    }
    public override IBaseUnitAi GetAI()
    {
        PlayerInterface pi = rScene.GetSceneVisualizer().GetPlayerInterface();
        return (pi != null ? (IBaseUnitAi)pi.GetInterface(IBaseUnitAi.ID) : null);
    }
    public override void SetLog(LOG pLog)
    {
        base.SetLog(pLog);
        // печатаем стартовую погребень
        if (mpLog != null)
        {
            mpLog.Message("ClientType: LocalClient");
        }
        // уведомляем управляемый объект
        PlayerInterface pi = rScene.GetSceneVisualizer().GetPlayerInterface();
        if (pi != null)
        {
            iContact pContact = pi.GetBaseContact();
            if (pContact != null) pContact.SetLog(mpLog);
        }
    }
    // LocalClient - от iClient (доступ к eng bay)
    public override UnitSpawnData GetUnitSpawnData() // получить custom-данные клиента
    {
        return mUnitSpawnData;
    }
    public override UnitSpawnData SetUnitSpawnData(UnitSpawnData rData)
    {
        mUnitSpawnData = rData;
        return mUnitSpawnData;
    }
    public override void showEngBay(uint nCrafts, uint[] pCrafts, uint nWeapons, uint[] pWeapons)
    {
        rScene.showEngbay((int)nCrafts, pCrafts, (int)nWeapons, pWeapons, mUnitSpawnData);
        myIsInEngBay = true;
    }
    public override bool IsInEngBay()
    {
        return myIsInEngBay;
    }
    public override void hideEngBay()
    {
        myIsInEngBay = false;
    }
    // доступ к камере
    public override float GetCameraRange()
    {
        return mrData.GetCameraRange();
    }
    public override DWORD GetCameraMode()
    {
        return rScene.GetSceneVisualizer().GetCameraMode();
    }
    public override DWORD GetCameraTarget()
    {
        return mrData.GetRef();
    }

    public override void SetCameraMode(DWORD Mode, DWORD Target)
    {
        rScene.GetSceneVisualizer().SetCameraLogic(Mode);
        mrData.SetRef(Target);
    }
    public override Vector3 GetCameraOrg()
    {
        return mrData.myCamera.Org;
    }
    public override void GetCameraAngles(out float HeadingAngle, out float PitchAngle, out float RollAngle)
    {
        HeadingAngle = 0;
        PitchAngle = 0;
        RollAngle = 0;
        mrData.myCamera.Vectors2Angles(ref HeadingAngle, ref PitchAngle, ref RollAngle);
    }
    public override void SetCameraPos(Vector3 Org, float HeadingAngle, float PitchAngle, float RollAngle)
    {
        mrData.myCamera.Org = Org;
        mrData.myCamera.Angles2Vectors(HeadingAngle, PitchAngle, RollAngle);
    }
    public override void SetCameraDraw(bool draw)
    {
        rScene.setCameraDraw(draw);
    }
    // remote commands
    public override DWORD RegisterRemoteConsoleCommand(string pName, int nArgs)
    {
        DWORD code = Hasher.HshString(pName);
        mRemoteCommandsList.Add(code);
        rScene.GetCommands().RegisterCommand(pName, this, nArgs);
        return code;
    }
    public override DWORD RegisterRemoteConsoleTrigger(string pName)
    {
        DWORD code = Hasher.HshString(pName);
        mRemoteCommandsList.Add(code);
        rScene.GetCommands().RegisterTrigger(pName, this);
        return code;
    }
    public override void UnRegisterRemoteConsole(DWORD code)
    {
        rScene.GetCommandsApi().UnRegister(this, code);
        mRemoteCommandsList.Remove(code);
    }
    // other
    public override void Message(string pStr)
    {
        rScene.Message(pStr);
    }
    public override void SayMessage(string pFormat, RadioMessage pData)
    {
        rScene.GetRadioEnvironment().AddRadioMessage(pFormat, pData);
    }
    public override void UpdateMenu(bool HasNewItem)
    {
        rScene.GetSceneVisualizer().GetPlayerInterface().OnMenuUpdate(HasNewItem);
    }
    // device access
    public override void CreateDevice(DWORD id, ClientDeviceDataCreate pData)
    {
        if (FindClientDevice(id) != null)
        {
# if _DEBUG
            _asm int 3;
#endif
            return;
        }
        iClientDevice pDev = ClientDeviceFactory.CreateClientDevice(rScene.GetSceneVisualizer(), pData);
        if (pDev == null)
        {
#if _DEBUG
            _asm int 3;
#endif
            return;
        }
        mClientDevicesList.AddToTail(new ClientDeviceContainer(id, pDev));
    }
    public override void UpdateDevice(DWORD id, ClientDeviceData pData)
    {
        {
            ClientDeviceContainer pCtr = FindClientDevice(id);
            if (pCtr != null) pCtr.ProcessData(pData);
        }
    }
    public override void DeleteDevice(DWORD id)
    {
        ClientDeviceContainer pCtr = FindClientDevice(id);
        if (pCtr != null) mClientDevicesList.Sub(pCtr).Dispose();
    }

    // от CommLink
    public virtual void OnTrigger(uint code, bool on)
    {
        //if (mRemoteCommandsList.Find(code) != null)
        if (mRemoteCommandsList.Contains(code))
            rScene.GetMissionAI().OnRemoteTrigger(this, code, on);
    }
    public virtual void OnCommand(uint code, string arg1, string arg2)
    {
        //if (mRemoteCommandsList.Find(code) != null)
        if (mRemoteCommandsList.Contains(code))
            rScene.GetMissionAI().OnRemoteCommand(this, code, arg1, arg2);
    }

    // от BaseClient
    public override IBaseUnitAi Capture(iContact c, UNIT_DATA data, IGroupAi grp)
    {
        Debug.Log("Capturing " + (c!=null? c:"Nothing"));
        if (GetAI() != null)
        {
            rScene.Message("LocalClient::Capture(iContact*): client already controls another unit!");
            return null;
        }
        // создаем клиента по типу объекта
        IBaseUnitAi res = null;
        if (c.GetInterface(BaseCraft.ID) != null)
        { // BaseCraft
          // уведомляем управляемый объект
            
            BaseObject pObj = (BaseObject)c.GetInterface(BaseObject.ID);
            Debug.Log("Assuming direct control over " + pObj);
            if (pObj != null) pObj.SetLog(mpLog);
            rScene.GetSceneVisualizer().SetPlayerInterface(new PlayerInterfaceLocalCraft(rScene, c, data, grp));
            SetCameraDraw(true);
        }
        if (c.GetInterface(BaseVehicle.ID) != null) 
        {  //BaseVehicle
           // уведомляем управляемый объект

            BaseObject pObj = (BaseObject)c.GetInterface(BaseObject.ID);
            Debug.Log("Assuming direct control over " + pObj);
            if (pObj != null) pObj.SetLog(mpLog);
            rScene.GetSceneVisualizer().SetPlayerInterface(new PlayerInterfaceLocalVehicle(rScene, c, data, grp));
            SetCameraDraw(true);
        }
        return (IBaseUnitAi)rScene.GetSceneVisualizer().GetPlayerInterface().GetInterface(IBaseUnitAi.ID);
    }
    public override void applyCraftSelection(UnitSpawnData dt)
    {
        SetUnitSpawnData(dt);
        hideEngBay();
    }
    public override bool Update(float scale)
    {
        for (ClientDeviceContainer pCtr = mClientDevicesList.Head(); pCtr != null; pCtr = pCtr.Next())
            pCtr.updateDevice(scale);
        return true;
    }

    // own
    private CameraData mrData;
    UnitSpawnData mUnitSpawnData;
    List<DWORD> mRemoteCommandsList = new();
    bool myIsInEngBay;
    TLIST<ClientDeviceContainer> mClientDevicesList = new TLIST<ClientDeviceContainer>();
    private ClientDeviceContainer FindClientDevice(DWORD id)
    {
        ClientDeviceContainer pCtr;
        for (pCtr = mClientDevicesList.Head(); pCtr != null; pCtr = pCtr.Next())
        {
            if (pCtr.GetDeviceID() == id) break;
        }
        return pCtr;
    }

    public LocalClient(HostScene s) : base(s)
    {
        mrData = s.GetSceneVisualizer().GetCameraData();
        myIsInEngBay = false;
        pPlayerName = rScene.GetGameData().myPlayerName;//, PLAYER_NAME_LENGTH);
        SetState(csInGame);
    }
    ~LocalClient()
    {
        //TODO возможно, это стоит перенести в метод Dispose
        Disconnect();
        rScene.GetCommandsApi().UnRegister(this);
    }

    public override void Dispose()
    {
        Disconnect();
        rScene.GetCommandsApi().UnRegister(this);
        base.Dispose();
    }
    public override bool isSwapped()
    {
        return mySwapped;
    }
}
