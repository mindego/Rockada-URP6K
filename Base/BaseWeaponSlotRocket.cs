using UnityEngine;

using DWORD = System.UInt32;
/// <summary>
/// BaseWeaponSlotRocket - неуправляемые ракеты
/// </summary>
class BaseWeaponSlotRocket : BaseWeaponSlotBarrel
{

    // работа с оружием
    private int nRockets;
    private float ReloadTimer;
    //inline const WPN_DATA_ROCKET* GetWpnData() const { return (const WPN_DATA_ROCKET*)SubobjData; }
    public WPN_DATA_ROCKET GetWpnData<T>() where T : WPN_DATA_ROCKET
    {
        return (WPN_DATA_ROCKET)SubobjData;
    }
    protected override void DoFire(iContact tgt, float ood, float dx, float dy)
    {
        if (Owner.IsLocal() && Owner.IsPlayedByHuman() && !rScene.GetGameData().mUnlimitedAmmo)
        {
            nRockets--;
            Owner.SubWeight(GetWpnData<WPN_DATA_ROCKET>().Mass);
        }
        ReloadTimer = GetWpnData<WPN_DATA_ROCKET>().ReloadTime;
        // стреляем
        Vector3 dir = GetAim(ref dx, ref dy, ood);
        new ProjectileRocket(rScene, GetWpnData(), (iContact)Owner.GetInterface(iContact.ID), InWorld, dir, Owner.IsRemote());
        base.DoFire(tgt, ood, dx, dy);
    }
    public override float GetEnergyUsage()
    {
        return 0;
    }
    public override float Reload(float scale, float escale)
    {
        Update(scale);
        if (GetStatus() == WPN_DISABLED) return 0;
        ReloadTimer -= scale; if (ReloadTimer < 0) ReloadTimer = 0;
        return Owner.IsInSF() && !GetWpnData<WPN_DATA_ROCKET>().myWorkInSFG ? WPN_DISABLED : GetWpnData<WPN_DATA_ROCKET>().ReloadTime;
    }
    public override int GetStatus()
    {
        if (pFPO == null || nRockets == 0) return WPN_DISABLED;
        return (ReloadTimer > 0 ? WPN_RELOADING : WPN_READY);
    }
    public override void GetFullStatus(out int state, out int load, out float progress)
    {
        load = nRockets;
        progress = -1f;
        state = GetStatus();
    }
    public override float GetAmmoLoad()
    {
        return (GetStatus() == WPN_DISABLED ? 0 : ((float)nRockets) / GetWpnData<WPN_DATA_ROCKET>().AmmoLoad);
    }

    // от BaseSubobj
    public BaseWeaponSlotRocket(BaseScene s, DWORD h, SUBOBJ_DATA d) : base(s, h, d)
    {
        nRockets = GetWpnData<WPN_DATA_ROCKET>().AmmoLoad;
        ReloadTimer = 0;
    }
    public override float GetWeight()
    {
        float w = base.GetWeight();
        return (w > 0f ? w - GetWpnData<WPN_DATA_ROCKET>().Mass * (GetWpnData<WPN_DATA_ROCKET>().AmmoLoad - nRockets) : w);
    }
};
