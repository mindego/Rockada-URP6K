#define _DEBUG
using UnityEngine;
using DWORD = System.UInt32;

public interface iMovementSystemCraft : iMovementSystem
{
    new public const uint ID = 0x3DE562C6;
    // управление автопилотом
    public void Manual();
    public void FlyTo(Vector3 TgtDir, Vector3 TgtSpeedInLocal);
    public void FlyToBattle(Vector3 TgtDir, Vector3 TgtSpeedInLocal);
    public void FollowUnit(iContact i, Vector3 Delta);
    public void Pause();
    public bool Land(iContact i, bool final = false);
    // непосредственное управление
    public Vector3 GetThrust();
    public void SetThrust(Vector3 v);
    public Vector3 GetControls();
    public void SetControls(Vector3 v);
    // дополнительные параметры
    public float GetDeltaY();
    public float GetSpeedF();
    public void SetMinAltitude(float alt);
    public float GetMinAltitude();
    public void SetPredictionTime(float time);
    public float GetPredictionTime();
    public float GetBatteryLoad();
    public object SetAutopilot(DWORD id);
    public object GetAutopilot(DWORD id);
    //template<class C> C* SetAutopilot() { return (C*)SetAutopilot(C::ID); }
    //template<class C> C* GetAutopilot() const { return (C*) GetAutopilot(C::ID);}
    public void MakePrediction(float scale); // make one step of precise prediction
    public void SetControlScale(float s);
    public void setMaxEnginePower(float thrust);
};