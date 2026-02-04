using UnityEngine;

/// <summary>
/// Заготовка для управления техникой
/// </summary>
class PlayerInterfaceLocalVehicle : PlayerInterfaceLocalAi, IBaseCraftController, IUnitAi
{
    public PlayerInterfaceLocalVehicle(BaseScene s, iContact p, UNIT_DATA data, IGroupAi grp) : base(s, p, data, grp)
    {
    }

    public iContact GetAutopiloLeader()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetAutopilotOrg()
    {
        throw new System.NotImplementedException();
    }

    public int GetAutopilotState()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetDestination()
    {
        throw new System.NotImplementedException();
    }

    public int GetSituationState()
    {
        throw new System.NotImplementedException();
    }

    public iContact GetSuggestedTarget()
    {
        throw new System.NotImplementedException();
    }

    public bool IsAutopilotOn()
    {
        throw new System.NotImplementedException();
    }

    public bool IsPaused()
    {
        throw new System.NotImplementedException();
    }

    public bool isShowedWaypoint()
    {
        throw new System.NotImplementedException();
    }

    public bool JoinFormation(JoinOption join)
    {
        throw new System.NotImplementedException();
    }

    public void MakeLanding(iContact hg)
    {
        throw new System.NotImplementedException();
    }

    public void Pause(bool pause)
    {
        throw new System.NotImplementedException();
    }

    public void SetAutopilotData(iContact leader, Vector3 org, float time)
    {
        throw new System.NotImplementedException();
    }

    public void SetAutopilotMode(bool on)
    {
        throw new System.NotImplementedException();
    }

    public bool SetDestination(Vector3 org, float time)
    {
        throw new System.NotImplementedException();
    }

    public void SetDevice(int v)
    {
        throw new System.NotImplementedException();
    }

    public void SetDeviceInvY(int v)
    {
        throw new System.NotImplementedException();
    }

    public void SetDeviceSens(float v)
    {
        throw new System.NotImplementedException();
    }

    public void SetDeviceXasYaw(int v)
    {
        throw new System.NotImplementedException();
    }

    public void SetDeviceZero(float v)
    {
        throw new System.NotImplementedException();
    }

    public bool setFormation(iContact c, Vector3 delta, float dist, uint formation_name)
    {
        throw new System.NotImplementedException();
    }

    public void SetPause(bool p)
    {
        throw new System.NotImplementedException();
    }

    public void SetRudder(int v)
    {
        throw new System.NotImplementedException();
    }

    public void SetRudderSens(float v)
    {
        throw new System.NotImplementedException();
    }

    public void SetRudderZero(float v)
    {
        throw new System.NotImplementedException();
    }

    public void setSpeed(float spd)
    {
        throw new System.NotImplementedException();
    }
}