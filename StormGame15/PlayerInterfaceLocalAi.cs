using DWORD = System.UInt32;
/// <summary>
/// PlayerInterfaceLocalAi - класс для интерфейса между игрой и игроком на хосте
/// </summary>
class PlayerInterfaceLocalAi : PlayerInterfaceLocal, IBaseUnitAi
{

    // от iBaseInterface
    new public const uint ID = 0x8B5D270A;
    public override object GetInterface(DWORD id)
    {
        switch (id)
        {
            case IBaseUnitAi.ID: return (IBaseUnitAi)this;
            case ID: return this;
            default: return base.GetInterface(id);
        }
    }


    // от IObject
    public virtual int Release()
    {
        mrScene.GetSceneVisualizer().SetPlayerInterface();
        return 0;
    }
    public virtual void AddRef() { }
    public virtual object Query(uint cls_id)
    {
        return GetInterface(cls_id);
    }

    // от IAi
    public virtual void ProcessRadioMessage(string msg_code, IAi caller, RadioMessage Info, bool to_all, bool say_flag)
    {
        return;
    }
    public virtual bool Update(float scale)
    {
        if (mPlayer.Ptr() != null)
            myHangaring = myHangaring = iSensors.IsHangaring(mPlayer.Ptr().GetState(), myHangaring); ;
        mPlayer.Validate();
        return (mPlayer.Ptr() != null);
    }

    // от IBaseUnitAi
    public virtual UNIT_DATA GetUnitData()
    {
        return mpData;
    }
    public virtual float GetAiming()
    {
        return 0;
    }
    public virtual IGroupAi GetIGroupAi()
    {
        return mpGroup;
    }
    public virtual iContact GetContact()
    {
        return mPlayer.Ptr();
    }
    public virtual iContact GetTarget()
    {
        return null;
    }
    public virtual iSensors GetSensors()
    {
        return (mPlayer.Ptr() != null ? (iSensors)mPlayer.Ptr().GetInterface(iSensors.ID) : null);
    }

    public virtual void SetSkill(DWORD skill, bool already_set)
    {

    }
    public virtual DWORD GetSkill() { return 0; }
    public virtual void SideChanged(iContact new_cnt)
    {
        throw new System.Exception("Player cannot change side!");
    }
    public virtual void Suicide()
    {
        mPlayer.setPtr(null);
        //mPlayer == null;
    }
    public virtual void OnDamage(DWORD d, float f, bool b) { }
    public virtual void OnKill(DWORD victim) { }
    public virtual void SetMessagesMode(bool silence) { }
    public virtual void SelectTarget(int nTargets, iContact[] Targets, float[] TargetWeights) { }
    public virtual bool IsHangaringBeforeLastValidate()
    {
        return myHangaring;
    }

    public bool SafeCallUpdate(StateFlag flag, float scale)
    {
        throw new System.NotImplementedException();
    }

    public bool CallUpdate(StateFlag flag, float scale)
    {
        throw new System.NotImplementedException();
    }

    public void SetState(IBaseUnitAi.AI_STATE new_state)
    {
        throw new System.NotImplementedException();
    }

    public virtual void enumTargets(ITargetEnumer en)
    {
        throw new System.NotImplementedException();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    // от PlayerInterface
    public PlayerInterfaceLocalAi(BaseScene s, iContact p, UNIT_DATA data, IGroupAi grp) : base(s, p)
    {
        mpData = data;
        mpGroup = grp;
        mRefCount = 1;
        myHangaring = false;
    }

    // own
    private int mRefCount;
    UNIT_DATA mpData;
    IGroupAi mpGroup;
    bool myHangaring;
}
