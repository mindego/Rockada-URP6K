
using DWORD = System.UInt32;
/// <summary>
/// BaseWeaponSlotMissile - управляемые ракеты
/// </summary>
class BaseWeaponSlotMissile : BaseWeaponSlotDetaching
{
    /// <summary>
    /// 89.5 degrees
    /// </summary>
    public const float TorpedoAimLim = 0.9999619f;  

    // от iBaseInterface
    new public const uint ID = 0x13CD6EBA;
    public override object GetInterface(DWORD id)
    {
        return (id == ID ? this : base.GetInterface(id));
    }

    // работа с оружием
    private float LockTimer;
    private readonly TContact Target = new TContact();
    private bool myOverrideLockTimer;
    public override void LocalFire(iContact tgt, MATRIX pos)
    {
        Asserts.AssertBp(rScene.IsHost());
        ((HostScene)rScene).CreateMissile(GetWpnData<WPN_DATA_MISSILE>(), (iContact)Owner.GetInterface(iContact.ID), tgt, pos);
        base.LocalFire(tgt, pos);
    }
    public float getLockTimer() { return LockTimer; }
    public void setLockTimer(float lt, bool canoverride = true) { LockTimer = lt; myOverrideLockTimer = canoverride; }
    public void initLockTimer()
    {
        LockTimer = GetWpnData<WPN_DATA_MISSILE>().LockTime;
    }
    //inline const WPN_DATA_MISSILE* GetWpnData  () const { return (const WPN_DATA_MISSILE*)SubobjData; }
    private WPN_DATA_MISSILE GetWpnData<T>() where T : WPN_DATA_MISSILE
    {
        return (WPN_DATA_MISSILE)SubobjData;
    }

    public override float GetEnergyUsage() { return 0; }
    public override float Reload(float scale, float escale)
    {
        if (GetStatus() == WPN_DISABLED) return 0;
        if (Target.Ptr() != null && LockTimer > .0f) LockTimer -= scale;
        /*char buffer[255];
        wsprintf(buffer,"Object=%p,LockTimer=%d\n",this,int(LockTimer));
        OutputDebugString(buffer);*/
        return GetWpnData<WPN_DATA_MISSILE>().GetReload();
    }
    public override int GetStatus()
    {
        if (base.GetStatus() == WPN_DISABLED) return WPN_DISABLED;
        if (GetWpnData<WPN_DATA_MISSILE>().LockTime > .0f)
        { // для ракеты
            return (LockTimer > 0 ? WPN_RELOADING : WPN_READY);
        }
        else
        { // для торпеды
            return WPN_READY;
        }
    }
    public override void GetFullStatus(out int state, out int load, out float progress)
    {
        load = (GetWpnData<WPN_DATA_MISSILE>().nDetachingNames - GetDetachingObjectsIdx());
        progress = (GetWpnData<WPN_DATA_MISSILE>().LockTime > .0f ? (GetWpnData<WPN_DATA_MISSILE>().LockTime - LockTimer) / GetWpnData<WPN_DATA_MISSILE>().LockTime : -1f);
        state = GetStatus();
    }
    public override  bool Fire(iContact tgt, float ood, float dx, float dy)
    {
        if (GetStatus() == WPN_DISABLED) return true;
        if (GetStatus() == WPN_RELOADING) tgt = null;
        base.Fire(tgt, ood, dx, dy);
        LockTimer = GetWpnData<WPN_DATA_MISSILE>().LockTime;
        return true;
    }
    public override float GetEffectivness(iContact tgt, float Dist)
    {
        float f = base.GetEffectivness(tgt, Dist);
        if (tgt == null) return f;
        // поправка для различных типов ракет и целей
        if (GetWpnData<WPN_DATA_MISSILE>().LockAngle == .0f)
        { // торпеда
          // по воздушным целям - ноль
            return ((tgt.GetInterface(BaseCraft.ID) != null) ? 0 : f);
        }
        else
        { // ракета
          // по не-воздушным целям или в подавляющем поле - ноль
            return ((tgt.GetInterface(BaseCraft.ID) == null || Owner.IsInSF() == true || tgt.IsInSF() == true) ? 0 : f);
        }
    }
    public override float GetAimC(float dz, float dist)
    {
        if (GetWpnData<WPN_DATA_MISSILE>().LockAngle > .0f) return (LockTimer > .0f ? dz * .5f : dz);
        else return (dz > TorpedoAimLim ? dz : dz * .5f);
    }
    /// <summary>
    /// один тик обработки цели
    /// </summary>
    /// <param name="Tgt"></param>
    /// <param name="TgtC"></param>
    public void ProcessTarget(iContact Tgt, float TgtC)
    {
        // торпеде цель - пофиг
        if (GetWpnData<WPN_DATA_MISSILE>().LockAngle == .0f) return;
        // в ГПП не работаем
        if (Owner.IsInSF() || (Tgt != null && Tgt.IsInSF())) Tgt = null;
        // не наведена
        if (TgtC < GetWpnData<WPN_DATA_MISSILE>().LockAngleCos) Tgt = null;
        // цель устарела
        if (Tgt != null && (Tgt.GetAge() > .0f || Tgt.IsOnlyVisual())) Tgt =null;
        // цель - не крафт
        if (Tgt != null && Tgt.GetInterface(BaseCraft.ID) == null) Tgt = null;
        // при смене цели сбрасываем таймер
        if (Target.Ptr() == Tgt) return;
        Target.setPtr(Tgt);
        if (Tgt == null || myOverrideLockTimer == false)
            initLockTimer();
    }

    // от BaseSubobj
    public BaseWeaponSlotMissile(BaseScene s, DWORD h,SUBOBJ_DATA d) : base(s,h,d)
    {
        LockTimer = GetWpnData<WPN_DATA_MISSILE>().LockTime;
        myOverrideLockTimer = false;
    }
};
