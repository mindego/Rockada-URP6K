using UnityEngine;
using static AiRadioMessages;
using static BaseCraftController;
using DWORD = System.UInt32;
/// <summary>
/// PlayerInterfaceLocalCraft - класс для интерфейса между игрой и игроком на крафте на хосте
/// </summary>
//class PlayerInterfaceLocalCraft : PlayerInterfaceLocalAi, BaseCraftController, IUnitAi
class PlayerInterfaceLocalCraft : PlayerInterfaceLocalAi, IBaseCraftController, IUnitAi
{

    // от iBaseInterface
    new public const uint ID = 0x128A419C;
    public override object GetInterface(DWORD id)
    {
        switch (id)
        {
            case BaseCraftController.ID: return (IBaseCraftController)this;
            case IUnitAi.ID: return (IUnitAi)this;
            case ID: return this;
            default: return base.GetInterface(id); //base в этом случае - PlayerInterfaceLocalAi
        }
    }

    // от IAi
    public override void ProcessRadioMessage(string msg_code, IAi caller, RadioMessage Info, bool to_all, bool say_flag)
    {
        if (Info.TargetAi != this) return;
        // обработка специфичных сообщений
        switch (Info.Code)
        {
            case RM_NOTIFY_LAND_CLEARED: MakeLanding(Info.TargetContact); break;
            case RM_NOTIFY_LAND_FAILED: MakeLanding(null); break;
        }
        base.ProcessRadioMessage(msg_code, caller, Info, to_all, say_flag);
    }

    // от IBaseUnitAi
    public override float GetAiming()
    {
        mPlayer.Validate();
        return ((mPlayer.Ptr() != null && myBaseCraftController.pWeapons != null) ? myBaseCraftController.pWeapons.GetAim() : .0f);
    }
    public override iContact GetTarget()
    {
        mPlayer.Validate();
        return ((mPlayer.Ptr() != null && myBaseCraftController.pWeapons != null) ? myBaseCraftController.pWeapons.GetTarget() : null);
    }
    public override void enumTargets(ITargetEnumer en)
    {
        mPlayer.Validate();
        if ((mPlayer.Ptr() != null && myBaseCraftController.pWeapons != null))
        {
            iContact cnt = myBaseCraftController.pWeapons.GetTarget();
            if (cnt!=null)
                en.processTarget(cnt, myBaseCraftController.pWeapons.GetAim());
        }
    }
    public override void SelectTarget(int nTargets, iContact[] Targets, float[] TargetWeights)
    {
        myBaseCraftController.SelectSuggestedTarget(nTargets, Targets, TargetWeights);
    }

    // от IUnitAi
    public virtual bool setFormation(iContact Leader, Vector3 delta, float dist, DWORD formation_name)
    {
        myBaseCraftController.SetAutopilotData(Leader, delta, -.1f);
        return true;
    }
    public virtual bool SetDestination(Vector3 org, float time)
    {
        myBaseCraftController.SetAutopilotData(null, org, time);
        return true;
    }
    public virtual bool JoinFormation(JoinOption join)
    {
        return true;
    }
    public virtual void Pause(bool pause)
    {
        myBaseCraftController.SetPause(pause);
    }
    // own

    private BaseCraftController myBaseCraftController;
    public PlayerInterfaceLocalCraft(BaseScene s, iContact p, UNIT_DATA data, IGroupAi grp) : base(s, p, data, grp)
    {

        myBaseCraftController = new BaseCraftController(s);
        myBaseCraftController.SetOwnContact(p);
    }
    public override void Process(float scale)
    {
        base.Process(scale);
        myBaseCraftController.Process(scale);
    }

    public iContact GetAutopiloLeader()
    {
        return ((IBaseCraftController)myBaseCraftController).GetAutopiloLeader();
    }

    public Vector3 GetAutopilotOrg()
    {
        return ((IBaseCraftController)myBaseCraftController).GetAutopilotOrg();
    }

    public int GetAutopilotState()
    {
        return ((IBaseCraftController)myBaseCraftController).GetAutopilotState();
    }

    public int GetSituationState()
    {
        return ((IBaseCraftController)myBaseCraftController).GetSituationState();
    }

    public iContact GetSuggestedTarget()
    {
        return ((IBaseCraftController)myBaseCraftController).GetSuggestedTarget();
    }

    public bool IsAutopilotOn()
    {
        return ((IBaseCraftController)myBaseCraftController).IsAutopilotOn();
    }

    public bool IsPaused()
    {
        return ((IBaseCraftController)myBaseCraftController).IsPaused();
    }

    public bool isShowedWaypoint()
    {
        return ((IBaseCraftController)myBaseCraftController).isShowedWaypoint();
    }

    public void MakeLanding(iContact hg)
    {
        ((IBaseCraftController)myBaseCraftController).MakeLanding(hg);
    }

    public void SetAutopilotData(iContact leader, Vector3 org, float time)
    {
        ((IBaseCraftController)myBaseCraftController).SetAutopilotData(leader, org, time);
    }

    public void SetAutopilotMode(bool on)
    {
        ((IBaseCraftController)myBaseCraftController).SetAutopilotMode(on);
    }

    public void SetDevice(int v)
    {
        ((IBaseCraftController)myBaseCraftController).SetDevice(v);
    }

    public void SetDeviceInvY(int v)
    {
        ((IBaseCraftController)myBaseCraftController).SetDeviceInvY(v);
    }

    public void SetDeviceSens(float v)
    {
        ((IBaseCraftController)myBaseCraftController).SetDeviceSens(v);
    }

    public void SetDeviceXasYaw(int v)
    {
        ((IBaseCraftController)myBaseCraftController).SetDeviceXasYaw(v);
    }

    public void SetDeviceZero(float v)
    {
        ((IBaseCraftController)myBaseCraftController).SetDeviceZero(v);
    }

    public void SetPause(bool p)
    {
        ((IBaseCraftController)myBaseCraftController).SetPause(p);
    }

    public void SetRudder(int v)
    {
       myBaseCraftController.SetRudder(v);
    }

    public void SetRudderSens(float v)
    {
        myBaseCraftController.SetRudderSens(v);
    }

    public void SetRudderZero(float v)
    {
       myBaseCraftController.SetRudderZero(v);
    }

    public void setSpeed(float spd)
    {
        myBaseCraftController.setSpeed(spd);
    }

    public Vector3 GetDestination()
    {
        throw new System.NotImplementedException();
    }
}
