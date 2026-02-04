using UnityEngine;

using DWORD = System.UInt32;
/// <summary>
/// BaseWeaponSlotGun - пулевое оружие
/// </summary>
class BaseWeaponSlotGun : BaseWeaponSlotBarrel
{

    // работа с оружием
    private int nShells;
    private float ReloadTimer;
    private WPN_DATA_GUN GetWpnData<T>() where T : WPN_DATA_GUN
    {
        return (WPN_DATA_GUN)SubobjData;
    }
    protected override void DoFire(iContact tgt, float ood, float dx, float dy)
    {
        if (Owner.IsLocal() && Owner.IsPlayedByHuman() && !rScene.GetGameData().mUnlimitedAmmo) nShells--;
        ReloadTimer = GetWpnData<WPN_DATA_GUN>().ReloadTime;
        // стреляем
        Vector3 dir = GetAim(ref dx, ref dy, ood);

        //Debug.Log("Owner " + Owner);
        //Debug.Log("Owner GI " + Owner.GetInterface(iContact.ID));
        var pr=new ProjectileLine(
            rScene, GetWpnData(),
            (iContact)Owner.GetInterface(iContact.ID),
            InWorld, dir, getSpeedBonus(),
            Owner.IsRemote());

        base.DoFire(tgt, ood, dx, dy);
        // выбрасываем гильзы
        //if (Owner->GetDetailStage() <= DETAIL_STAGE_FULL && rScene.GetSceneVisualizer() != 0 && rScene.GetSceneVisualizer()->GetSceneConfig()->v_brass != 0 && GetWpnData()->nShells > 0)
        //{
        MATRIX Pos = new MATRIX(Owner.GetPosition());
        Pos.Org = InWorld - Pos.Dir * Max.z;
        //TODO Возможно, стоит изменить на создание нового 
        Vector3 v = Owner.GetSpeed();
        for (int i = 0; i < GetWpnData<WPN_DATA_GUN>().nShells; i++)
        {
            v.x += Storm.Math.norm_rand() * .5f; v.y -= 2f; v.z += Storm.Math.norm_rand() * .5f;
            //TODO Реализовать отрисовку гильз.
            //rScene.CreateBaseDebris(Pos, GetWpnData<WPN_DATA_GUN>().Shell).SetSpeed(v);
        }
        //}
    }
    public override float GetEnergyUsage()
    {
        return 0;
    }

    public override float Reload(float scale, float escale)
    {
        Update(scale);
        if (GetStatus() == WPN_DISABLED) return 0;
        ReloadTimer -= scale;
        return GetWpnData<WPN_DATA_GUN>().ReloadTime;
    }
    public override int GetStatus()
    {
        if (pFPO == null || nShells == 0) return WPN_DISABLED;
        return (ReloadTimer > 0 ? WPN_RELOADING : WPN_READY);
    }
    public override void GetFullStatus(out int state, out int load, out float progress)
    {
        load = nShells;
        progress = -1f;
        state = GetStatus();
    }
    public override float GetAmmoLoad()
    {
        return (GetStatus() == WPN_DISABLED ? 0 : ((float)nShells) / GetWpnData<WPN_DATA_GUN>().AmmoLoad);
    }
    // от BaseSubobj
    public BaseWeaponSlotGun(BaseScene s, DWORD h, SUBOBJ_DATA d) : base(s, h, d)
    {
        nShells = GetWpnData<WPN_DATA_GUN>().AmmoLoad;
        ReloadTimer = 0;
    }
};
