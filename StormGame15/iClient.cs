using UnityEngine;
using crc32 = System.UInt32;
using DWORD = System.UInt32;
/// <summary>
/// iClient - interface for AI
/// </summary>
public interface iClient
{
    // доступ к параметрам клиента
    /// <summary>
    /// IP-адрес игрока [4]
    /// </summary>
    /// <returns></returns>
    public byte[] GetIpAddress  ();  
    /// <summary>
    /// получить имя игрока
    /// </summary>
    /// <returns></returns>
    public string GetPlayerName();
    /// <summary>
    /// установить имя игрока
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public string SetPlayerName(string s);
    public IBaseUnitAi GetAI();
    public string GetAiName();
    // доступ к eng bay
    /// <summary>
    /// получить custom-данные клиента
    /// </summary>
    /// <returns></returns>
    public UnitSpawnData GetUnitSpawnData();
    /// <summary>
    /// установить custom-данные клиента
    /// </summary>
    /// <param name="usd"></param>
    /// <returns></returns>
    public UnitSpawnData SetUnitSpawnData(UnitSpawnData usd);
    /// <summary>
    /// клиент попадает в меню EngBay
    /// </summary>
    /// <param name="nCrafts"></param>
    /// <param name="pCrafts"></param>
    /// <param name="nWeapons"></param>
    /// <param name="pWeapons"></param>
    public void showEngBay(DWORD nCrafts, crc32[] pCrafts, DWORD nWeapons, crc32[] pWeapons);
    /// <summary>
    /// клиент находиться в меню EngBay?
    /// </summary>
    /// <returns></returns>
    public bool IsInEngBay();  
    public void hideEngBay();

    // доступ к камере
    /// <summary>
    /// получить дальность отображения для клиента
    /// </summary>
    /// <returns></returns>
    public float GetCameraRange();
    /// <summary>
    /// получить режим камеры клиента
    /// </summary>
    /// <returns></returns>
    public DWORD GetCameraMode();
    /// <summary>
    /// получить хендл цели камеры клиента
    /// </summary>
    /// <returns></returns>
    public DWORD GetCameraTarget();
    /// <summary>
    /// установить режим и цель камеры клиента
    /// </summary>
    /// <param name="Mode"></param>
    /// <param name="Target"></param>
    public void SetCameraMode(DWORD Mode, DWORD Target);
    /// <summary>
    /// получить положение камеры клиента
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCameraOrg();  
    /// <summary>
    /// получить углы поворта камеры клиента
    /// </summary>
    /// <param name="HeadingAngle"></param>
    /// <param name="PitchAngle"></param>
    /// <param name="RollAngle"></param>
    public void GetCameraAngles(out float HeadingAngle, out float PitchAngle, out float RollAngle);
    /// <summary>
    /// установить положение и углы поворта камеры клиента
    /// </summary>
    /// <param name="Org"></param>
    /// <param name="HeadingAngle"></param>
    /// <param name="PitchAngle"></param>
    /// <param name="RollAngle"></param>
    public void SetCameraPos(Vector3 Org, float HeadingAngle, float PitchAngle, float RollAngle);
    public void SetCameraDraw(bool draw);
    // remote commands
    public DWORD RegisterRemoteConsoleCommand(string pName, int nArgs);
    public DWORD RegisterRemoteConsoleTrigger(string pName);
    public void UnRegisterRemoteConsole(DWORD d);
    // other
    public bool Disconnect();
    public void Message(string s);
    public void SayMessage(string pFormat, RadioMessage pData);
    public void UpdateMenu(bool HasNewItem);
    // device access
    public void CreateDevice(DWORD id, ClientDeviceDataCreate pData);
    public void UpdateDevice(DWORD id, ClientDeviceData pData);
    public void DeleteDevice(DWORD id);
    // logging
    public void SetLog(LOG log);
    // hidden properties
    public bool IsHidden();
    // hidden properties
    public bool isSwapped();
}


