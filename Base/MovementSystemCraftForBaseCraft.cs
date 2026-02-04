using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// MovementSystemCraftForBaseCraft - переходних между iMovementSystemCraft и BaseCraft
/// </summary>
public class MovementSystemCraftForBaseCraft : iMovementSystemCraft
{

    // от iMovementSystemCraft

    // управление автопилотом
    public void Manual()
    {
        if (mpCraft.pAutopilot.GetState() != iSensorsDefines.CS_IN_GAME) return;
        SetAutopilot(BaseCraftAutopilotManual.ID);
    }
    public void FlyTo(Vector3 _TgtDir, Vector3 _TgtSpeed)
    {
        if (mpCraft.pAutopilot.GetState() != iSensorsDefines.CS_IN_GAME) return;
        ((BaseCraftAutopilotFlyTo)SetAutopilot(BaseCraftAutopilotFlyTo.ID)).Set(_TgtDir, _TgtSpeed);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_TgtDir"></param>
    /// <param name="_TgtSpeed">TgtSpeedInLocal</param>
    public void FlyToBattle(Vector3 _TgtDir, Vector3 _TgtSpeed)
    {
        if (mpCraft.pAutopilot.GetState() != iSensorsDefines.CS_IN_GAME) return;
        ((BaseCraftAutopilotBattle)SetAutopilot(BaseCraftAutopilotBattle.ID)).Set(_TgtDir, _TgtSpeed);
    }
    public void FollowUnit(iContact l, Vector3 Delta)
    {
        if (mpCraft.pAutopilot.GetState() != iSensorsDefines.CS_IN_GAME) return;
        ((BaseCraftAutopilotFollow)SetAutopilot(BaseCraftAutopilotFollow.ID)).Set(l, Delta);
    }
    public void Pause()
    {
        if (mpCraft.pAutopilot.GetState() != iSensorsDefines.CS_IN_GAME) return;
#if (_DEBUG) && (_TEST_SMOOTHING)
  SetAutopilot<BaseCraftAutopilotRemote>();
#else
        SetAutopilot(BaseCraftAutopilotPause.ID);
#endif
    }
    public bool Land(iContact c, bool final)
    {
        if (mpCraft.pAutopilot.GetInterface(BaseCraftAutopilotLand.ID) != null) return true;
        if (mpCraft.pAutopilot.GetState() != iSensorsDefines.CS_IN_GAME) return false;
        BaseHangar h = (BaseHangar)c.GetInterface(BaseHangar.ID);
        // проверка на вшивость
        if (h == null || h.canHandleUnit((iContact)mpCraft.GetInterface(iContact.ID)) == 0)
            throw new System.Exception(string.Format("Craft \"{0}\": incompatible hangar!", mpCraft.GetObjectData().FullName));
        // создаем автопилот посадки
        BaseCraftAutopilotLand p = new BaseCraftAutopilotLand(mpCraft, c, h, final);
        mpCraft.pAutopilot.Dispose();
        //delete mpCraft.pAutopilot;
        mpCraft.pAutopilot = p;
        return true;
    }
    // непосредственное управление
    public Vector3 GetThrust()
    {
        return mpCraft.Thrust;
    }
    public void SetThrust(Vector3 t)
    {
        mpCraft.Thrust = t;
    }
    public Vector3 GetControls()
    {
        return mpCraft.Controls;
    }
    public void SetControls(Vector3 t)
    {
        mpCraft.Controls = t;
    }
    // дополнительные параметры
    public float GetDeltaY()
    {
        return mpCraft.DeltaY;
    }
    public float GetSpeedF()
    {
        return mpCraft.SpeedF;
    }
    public void SetMinAltitude(float alt)
    {
        mpCraft.AutopilotDeltaY = Mathf.Clamp(alt, 25f, 500f);
    }
    public float GetMinAltitude()
    {
        return mpCraft.AutopilotDeltaY;
    }
    public void SetPredictionTime(float time)
    {
        mpCraft.AutopilotPredictionTime = Mathf.Clamp(time, .0f, 10f);
    }
    public float GetPredictionTime()
    {
        return mpCraft.AutopilotPredictionTime;
    }
    public float GetBatteryLoad()
    {
        return mpCraft.BatteryCharge / mpCraft.Dt().BatteryCharge;
    }
    public object SetAutopilot(DWORD id)
    {
        // если автопилот уже есть
        if (mpCraft.pAutopilot != null)
        {
            // проверяем, может он подойдет?
            object r = mpCraft.pAutopilot.GetInterface(id);
            if (r != null) return r;
        }
        // пытаемся создать новый
        BaseCraftAutopilot c = null;
        switch (id)
        {
            case BaseCraftAutopilot.ID:
                c = new BaseCraftAutopilot(mpCraft);
                break;
            case BaseCraftAutopilotManual.ID:
                c = new BaseCraftAutopilotManual(mpCraft);
                break;
            case BaseCraftAutopilotTurnTo.ID:
                c = new BaseCraftAutopilotTurnTo(mpCraft);
                break;
            case BaseCraftAutopilotFlyTo.ID:
                c = new BaseCraftAutopilotFlyTo(mpCraft);
                break;
            case BaseCraftAutopilotBattle.ID:
                c = new BaseCraftAutopilotBattle(mpCraft);
                break;
            case BaseCraftAutopilotFollow.ID:
                c = new BaseCraftAutopilotFollow(mpCraft);
                break;
            case BaseCraftAutopilotPause.ID:
                c = new BaseCraftAutopilotPause(mpCraft);
                break;
                //case BaseCraftAutopilotRemote.ID:
                //    c = new BaseCraftAutopilotRemote(mpCraft);
                //    break;
        }
        // если удалось
        if (c != null)
        {
            // удаляем старый

            mpCraft.pAutopilot.Dispose();
            mpCraft.pAutopilot = c;
            // возвращаем новый
            return c.GetInterface(id);
        }
        // нет - возвращаем 0
        return null;
    }
    public object GetAutopilot(DWORD id)
    {
        return (mpCraft.pAutopilot != null ? mpCraft.pAutopilot.GetInterface(id) : null);
    }
    //template<class C> C* SetAutopilot() { return (C*)SetAutopilot(C.ID); }
    //template<class C> C* GetAutopilot() const { return (C*) GetAutopilot(C.ID);}

    /// <summary>
    /// make one step of precise prediction
    /// </summary>
    /// <param name="scale"></param>
    public void MakePrediction(float scale)
    {
        if (mpCraft.pAutopilot != null)
        {
            //Debug.Log(this.ToString()+"Making prediction using " + mpCraft.pAutopilot  + " for " + mpCraft.ObjectData.FullName);
            mpCraft.pAutopilot.Move(scale, true);
        }
        else mpCraft.MakePrediction(scale);
    }
    public void SetControlScale(float s)
    {
        mpCraft.ControlScale = Mathf.Clamp(s, .25f, 1f);
    }

    public bool IsManual()
    {
        return mpCraft.pAutopilot.IsManual();
    }
    public void setMaxEnginePower(float power)
    {
        mpCraft.setMaxEnginePower(power);
    }

    // own
    private BaseCraft mpCraft;
    public MovementSystemCraftForBaseCraft(BaseCraft c)
    {
        mpCraft = c;
    }
    public iMovementSystem getiMovementSystem() { return this; }

    internal void Dispose()
    {
        return;
    }
}
