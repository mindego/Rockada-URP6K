using UnityEngine;

using DWORD = System.UInt32;
/// <summary>
/// BaseWeaponSlotPlasma - плазменное оружие
/// </summary>
class BaseWeaponSlotPlasma : BaseWeaponSlotBarrel
{

    // работа с оружием
    private float Charge;
    private float fireTime;
    //inline const WPN_DATA_PLASMA* GetWpnData() const { return (const WPN_DATA_PLASMA*)SubobjData; }
    private WPN_DATA_PLASMA GetWpnData<T>() where T : WPN_DATA_PLASMA
    {
        return (WPN_DATA_PLASMA)SubobjData;
    }
    bool canShot()
    {
        return Charge >= GetWpnData<WPN_DATA_PLASMA>().EnergyPerShot && fireTime == 0;
    }
    protected override void DoFire(iContact tgt, float ood, float dx, float dy)
    {
        if (canShot())
        {
            fireTime = GetWpnData<WPN_DATA_PLASMA>().ReloadTime;
            Charge -= GetWpnData<WPN_DATA_PLASMA>().EnergyPerShot;
            Vector3 dir = GetAim(ref dx, ref dy, ood);

            new ProjectileLine(
                rScene, GetWpnData(),
                (iContact)Owner.GetInterface(iContact.ID),
                InWorld, dir, getSpeedBonus(),
                Owner.IsRemote());

            base.DoFire(tgt, ood, dx, dy);
        }
    }
    public override float GetEnergyUsage()
    {
        return
    (GetStatus() == WPN_DISABLED ||
     Charge == GetWpnData<WPN_DATA_PLASMA>().Charge ||
     Owner.IsPlayedByHuman() == false ||
     rScene.GetGameData().mUnlimitedAmmo == true) ? 0 : GetWpnData<WPN_DATA_PLASMA>().RechargeSpeed;
    }
    public override float Reload(float scale, float escale)
    {
        fireTime -= scale;
        if (fireTime < 0) fireTime = 0;

        Update(scale);
        if (GetStatus() == WPN_DISABLED) return 0;

        float energyDrain = GetWpnData<WPN_DATA_PLASMA>().RechargeSpeed * escale;
        if (energyDrain == 0) return 1000;

        Charge += energyDrain * scale;
        if (Charge > GetWpnData<WPN_DATA_PLASMA>().Charge)
            Charge = GetWpnData<WPN_DATA_PLASMA>().Charge;

        if (Charge >= GetWpnData<WPN_DATA_PLASMA>().EnergyPerShot)
            return GetWpnData<WPN_DATA_PLASMA>().ReloadTime;
        else
            return GetWpnData<WPN_DATA_PLASMA>().EnergyPerShot / energyDrain;
    }
    public override int GetStatus()
    {
        if (pFPO == null || Owner.IsInSF()) return WPN_DISABLED;
        return canShot() ? WPN_READY : WPN_RELOADING;
    }
    public override void GetFullStatus(out int state, out int load, out float progress)
    {
        //load = GetWpnData()->Charge > GetWpnData()->EnergyPerShot*2 ?
        //    Floor(GetWpnData()->Charge/GetWpnData()->EnergyPerShot) : -1;
        load = -1;

        progress = Charge / GetWpnData<WPN_DATA_PLASMA>().Charge;
        state = GetStatus();
    }

    // от BaseSubobj
    public BaseWeaponSlotPlasma(BaseScene s, DWORD h, SUBOBJ_DATA d) : base(s, h, d)
    {
        Charge = (GetWpnData<WPN_DATA_PLASMA>().Charge);
        fireTime = GetWpnData<WPN_DATA_PLASMA>().ReloadTime;
        Asserts.Assert(GetWpnData<WPN_DATA_PLASMA>().Charge >= GetWpnData<WPN_DATA_PLASMA>().EnergyPerShot);
    }
}
