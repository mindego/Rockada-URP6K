using UnityEngine;
using DWORD = System.UInt32;
/// interface for vehicle (one dedicated turret)
public interface iWeaponSystemDedicated
{
    public const DWORD ID = 0x5945A516;
    public float GetCondition();
    public float GetEfficiency(iContact pTarget = null, float Dist = 0);
    public bool presentWeapon(int idx);
    public void SetAimError(float err);
    public float GetAimError();
    public void SetTarget(iContact pTarget);
    public iContact GetTarget();
    public iContact SelectTarget(int nTargets, iContact[] Targets, float[] TargetWeights, float MaxAge = .0f);
    public Vector3 GetTargetOrg();
    public float GetTargetDist();
    public float GetAim();
    public float GetRange();
    public void SetTrigger(bool on);
    public bool GetTrigger();
    public void SetWeapon(int wpn);
    public int GetWeapon();
};

/// <summary>
/// interface for turrets
/// </summary>
public interface iWeaponSystemTurrets
{
    public const DWORD ID = 0x80CB605C;
    public float GetCondition();
    public void SetAimError(float err);
    public void SetTargets(int nTargets, iContact[] Targets, float[] TargetWeights);
    public iWeaponSystemDedicated GetNextTurret(iWeaponSystemDedicated prev);
};

/// <summary>
/// interface for turrets
/// </summary>
public interface iWeaponSystemHTGR
{
    public const DWORD ID = 0x9110524B;
    public bool isRocketFired() ;
    public float getRelaxTime() ;
};

public class IAiModuleCreate
{
    public const uint AI_MODULE_INTERFACE_VERSION = 0x499046C1;//AiModileInterface .99a

}