using UnityEngine;

using DWORD = System.UInt32;
/// <summary>
/// базовый класс
/// </summary>
public class BaseWeaponSlot : BaseSubobj
{
    public override string ToString()
    {
        return GetType().ToString() + " " + GetHashCode().ToString("X8");
    }

    public const int WPN_RELOADING = 5;
    public const int WPN_READY = 3;
    public const int WPN_DISABLED = 6;

    // от iBaseInterface
    new public const uint ID = 0x606CD2F3;
    public override object GetInterface(DWORD id) { return (id == ID ? this : base.GetInterface(id)); }

    // от iBaseVictim
    public override void AddRadiusDamage(DWORD GadHandle, DWORD WeaponCode, Vector3 Org, float Xr, float Xd) { }

    // от BaseItem
    //public  virtual bool OnDataPacket(float PacketDelay,const ItemDataPacket*);

    private int Group;
    BaseWeaponSlot NextWS;
    public BaseWeaponSlot GetNext() { return NextWS; }
    public void SetNext(BaseWeaponSlot s) { NextWS = s; }
    public int GetGroup() { return Group; }
    public void SetGroup(int g) { Group = g; }

    // работа с оружием
    /// <summary>
    /// стрельнуть
    /// </summary>
    /// <param name="tgt"></param>
    /// <param name="ood"></param>
    /// <param name="dx"></param>
    /// <param name="dy"></param>
    protected virtual void DoFire(iContact tgt, float ood, float dx, float dy) { }
    /// <summary>
    /// требуемая энергия
    /// </summary>
    /// <returns></returns>
    public virtual float GetEnergyUsage() { return 0; }
    /// <summary>
    /// перезарядка
    /// </summary>
    /// <param name=""></param>
    /// <param name=""></param>
    /// <returns></returns>
    public virtual float Reload(float f1, float f2) { return 0; }
    /// <summary>
    /// готово ли?
    /// </summary>
    /// <returns></returns>
    public virtual int GetStatus() { return 0; }
    /// <summary>
    /// развернутое состояние
    /// </summary>
    /// <param name=""></param>
    /// <param name=""></param>
    /// <param name=""></param>
    public virtual void GetFullStatus(out int state, out int load, out float progress)
    {
        state = 0; load = 0; progress = 0;
    }
    /// <summary>
    /// стрельнуть
    /// </summary>
    /// <param name="tgt"></param>
    /// <param name="ood"></param>
    /// <param name="dx"></param>
    /// <param name="dy"></param>
    /// <returns></returns>
    public virtual bool Fire(iContact tgt, float ood, float dx, float dy)
    {
        if (GetStatus() == WPN_DISABLED) return true;
        if (GetStatus() == WPN_RELOADING) return false;
        // если посылаем отдельным пакетом, отключено
        //if (GetWpnData()->SeparatedPacket == true)
        //{
        //    if (Owner->IsRemote()) return false;
        //    ItemDataPacket Pkt(GetHandle(), WSP_FIRE,sizeof(ItemDataPacket));
        //    rScene.SendItemData(&Pkt);
        //}
        // стреляем
        DoFire(tgt, ood, dx, dy);
        return true;
    }
    /// <summary>
    /// эффективность по цели
    /// </summary>
    /// <param name="tgt"></param>
    /// <param name="Dist"></param>
    /// <returns></returns>
    public virtual float GetEffectivness(iContact tgt, float Dist)
    {
        if (GetStatus() == WPN_DISABLED) return 0;
        if (Dist > GetWpnData().Range) return (GetWpnData().Range * .00001f);
        return ((tgt != null && tgt.GetInterface(BaseCraft.ID) != null) ? GetWpnData().GetCraftEff(Dist) : GetWpnData().GetNonCraftEff());
    }
    /// <summary>
    /// сколько патронов осталось
    /// </summary>
    /// <returns></returns>
    public virtual float GetAmmoLoad()
    {
        return (GetStatus() == WPN_DISABLED ? 0 : 1);
    }
    /// <summary>
    /// поправка степени наведенности для AI
    /// </summary>
    /// <param name="dz"></param>
    /// <param name="dist"></param>
    /// <returns></returns>
    public virtual float GetAimC(float dz, float dist)
    {
        return dz;
    }
    public virtual WPN_DATA GetWpnData() { return (WPN_DATA)SubobjData; }

    // от BaseSubobj
    public BaseWeaponSlot(BaseScene s, DWORD h, SUBOBJ_DATA d) : base(s, h, d)
    {
        NextWS = null;
        Group = 0;
    }
    /// <summary>
    /// текущий вес подвески
    /// </summary>
    /// <returns></returns>
    public override float GetWeight()
    {
        if (pFPO == null) return 0f;
        switch (GetWpnData().Weight)
        {
            case WpnDataDefines.WW_SMALL: return 500f;
            case WpnDataDefines.WW_MEDIUM: return 1000f;
            default: return 2000f;
        }

    }


    // internal helpers

    public float getSpeedBonus()
    { // described ImproveFiringConditions_Issue
      // used when  "new ProjectileLine"
        return Vector3.Dot(Owner.GetFpo().Dir, Owner.GetSpeed());
    }
}
