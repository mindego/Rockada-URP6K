using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// BaseVehicleAutopilotManual - ручное управление
/// </summary>
class BaseVehicleAutopilotManual : BaseVehicleAutopilot
{
    // для кверенья
    new public const uint ID = 0xFF17E876;
    public override object GetInterface(DWORD id)
    {
        return (id == ID ? this : null);
    }

    // интерфейс с BaseVehicle
    public BaseVehicleAutopilotManual(BaseScene sc, BaseVehicle c) : base(sc,c) { }
    public override bool Move(float scale)
    {
        //Debug.Log("AP pre-process " + this.GetType().ToString() + " " + rOwner.pFPO.Dir);
        rOwner.mTargetThrust = mTargetSpeed * rOwner.GetVehicleData().OO_MaxSpeed;
        return rOwner.ProcessPhysic(scale);
    }
};