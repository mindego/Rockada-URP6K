using UnityEngine;

public interface IBaseCraftController
{
    public const uint ID = 0x0AB7E70C;
    iContact GetAutopiloLeader();
    Vector3 GetAutopilotOrg();
    int GetAutopilotState();
    int GetSituationState();
    iContact GetSuggestedTarget();
    bool IsAutopilotOn();
    bool IsPaused();
    bool isShowedWaypoint();
    void MakeLanding(iContact hg);
    void OnCommand(uint code, string arg1, string arg2);
    void OnTrigger(uint code, bool on);
    object OnVariable(uint code, object data);
    void SetAutopilotData(iContact leader, Vector3 org, float time);
    void SetAutopilotMode(bool on);
    void SetDevice(int v);
    void SetDeviceInvY(int v);
    void SetDeviceSens(float v);
    void SetDeviceXasYaw(int v);
    void SetDeviceZero(float v);
    void SetPause(bool p);
    void SetRudder(int v);
    void SetRudderSens(float v);
    void SetRudderZero(float v);
    void setSpeed(float spd);
}