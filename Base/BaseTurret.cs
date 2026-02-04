using DWORD = System.UInt32;
using System;

public partial class BaseTurret:iBaseActor
{
    BaseActor myBaseActor;

    public void BaseActorInit(BaseScene s)
    {
        myBaseActor = new BaseActor(s);
        myBaseActor.SetOwner(this);
    }

    public void BaseActorDispose()
    {
        myBaseActor.Dispose();
    }

    //public iBaseActor Next()
    //{
    //    return myBaseActor.Next();
    //}

    //public iBaseActor Prev()
    //{
    //    return myBaseActor.Prev();
    //}

    //public void SetNext(iBaseActor t)
    //{
    //    myBaseActor.SetNext(t);
    //}

    //public void SetPrev(iBaseActor t)
    //{
    //    myBaseActor.SetPrev(t);
    //}

    // BaseTurret - от BaseActor
    public void Update(float scale) //В оригинале было void
    {
        //UnityEngine.Debug.Log("Updating turret " + this + " " + mpWeaponSystem);
        mpWeaponSystem.Update(scale);
        //return true;
    }

    public bool Move(float scale)
    {
        //UnityEngine.Debug.Log("Moving turret " + this + " " + mpWeaponSystem);
        mpWeaponSystem.UpdateWorldPos();
        return true;
    }
}
public partial class BaseTurret : BaseSubobj, iBaseActor
{
    public override void Dispose()
    {
        base.Dispose();
        BaseActorDispose();
    }

    #region        // от iBaseInterface
    new public const uint ID = 0x49FFFB72;

    //BaseScene GetScene() { return rScene; } // жуть...

    /// <summary>
    /// BaseTurret - от iBaseInterface
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override object GetInterface(DWORD id)
    {
        switch (id)
        {
            case ID:
                return this;
            default:
                if (mpWeaponSystem != null)
                {
                    object r = mpWeaponSystem.GetInterface(id);
                    if (r != null) return r;
                }
                return base.GetInterface(id);
        }
    }

    #endregion
    #region от BaseItem
    public override bool IsRemote() { return false; }
    public override bool IsLocal() { return !IsRemote(); }
    #endregion
    public BaseTurret(BaseScene s, uint h, SUBOBJ_DATA sd) : base(s, h, sd)
    {
        BaseActorInit(s);
    }

    ~BaseTurret()
    {
        Dispose();
    }


    public override void HostPrepare(HostScene s, BaseObject o, BaseSubobj so, FPO fpo, SLOT_DATA sld, int slot_id, int lay_id)
    {// инициализация
        base.HostPrepare(s, o, so, fpo, sld, slot_id, lay_id);
        BasePrepare(sld);
    }
    // BaseTurret - от BaseSubobj
    public void BasePrepare(SLOT_DATA sld)
    {
        switch (((TURRET_DATA)GetData()).WeaponData.Type)
        {
            case WpnDataDefines.WT_MISSILE:
                mpWeaponSystem = new WeaponSystemForTurretWithMissiles(this);
                break;
            case WpnDataDefines.WT_HTGR:
                mpWeaponSystem = new WeaponSystemForTurretWithHTGR(this);
                break;
            case WpnDataDefines.WT_GUN:
            case WpnDataDefines.WT_PLASMA:
                mpWeaponSystem = new WeaponSystemForTurretWithGuns(this);
                break;
            default:
                throw new System.Exception(string.Format("Object \"{0}\", turret \"{1}s\", slot \"{2}\": wrong weapon type!", Owner.GetObjectData().FullName, GetData().FullName, (sld != null && sld.Name != null) ? sld.Name : "<none>"));
        }
        //UnityEngine.Debug.Log("Creating " + new string(sld.Name) + " " + mpWeaponSystem);
        UnityEngine.Debug.Log("Creating mpWeaponSystem " + mpWeaponSystem);
        mpWeaponSystem.Prepare(sld);
    }
    #region взаимодействие с BaseObject
    private BaseTurret NextTurret;
    WeaponSystemForTurret mpWeaponSystem;

    #endregion
    public WeaponSystemForTurret GetWeaponSystem() { return mpWeaponSystem; }
    public BaseTurret GetNextTurret() { return NextTurret; }
    public void SetNextTurret(BaseTurret t) { NextTurret = t; }

    public static float GetCenterC(float a, float center, float diff)
    {
        return (1f - MathF.Abs(a - center) * diff);
    }


}
