using System.Security.Cryptography;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using DWORD = System.UInt32;


public partial class Projectile : iBaseActor
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

    public virtual bool Move(float scale)
    {
        throw new System.NotImplementedException("Abstract projectile can not move!");
    }

    public BaseScene rScene
    {
        get
        {
            return myBaseActor.rScene;
        }
    }
}
/// <summary>
/// Projectile - базовый класс для игровых частей оружия
/// </summary>
public partial class Projectile : iBaseActor
{
    float alpha = .0004f;
    #region от iBaseInterface
    new public const uint ID = 0x8C59BE42;
    public virtual object GetInterface(DWORD id)
    {
        switch (id)
        {
            case Projectile.ID: return this;
            case BaseActor.ID: return (iBaseActor)this;
            default: return null;
        }
    }
    #endregion
    #region от BaseActor
    public virtual void Update(float scale) { }
    #endregion
    #region  свое
    protected MATRIX pos;
    protected Vector3 speed;
    protected float speedf, timer;
    protected WPN_DATA wpndata;
    protected readonly TContact owner = new TContact();
    protected DWORD OwnerHandle;
    protected ProjectileVisual visual;
    protected bool Remote;

    protected EXPLOSION_INFO GetExplInfo(TraceResult tr, out Vector3 org, out Vector3 dir, out Vector3 spd)
    {
        if (tr == null)
        {
            org = pos.Org;
            dir = pos.Dir;
            spd = speed;
            return wpndata.ExplOnEnd;
        }
        org = tr.org;
        dir = tr.Normal(true);
        spd = Vector3.zero;
        spd.Set(0, 0, 0);
        EXPLOSION_INFO info = null;
        if (tr.coll_object != null)
        {
            if (tr.coll_object.Link != null)
            {
                //iContact u = ((iBaseInterface)tr.coll_object.Link).GetInterface<iContact>();
                iBaseInterface intr = (iBaseInterface)tr.coll_object.Link;
                iContact u=null;
                try
                {
                    //u = (iContact)((iBaseInterface)tr.coll_object.Link).GetInterface(iContact.ID);
                    u = (iContact)intr.GetInterface(iContact.ID);
                } catch
                {
                    //throw new System.Exception(string.Format("Failed to convert {0} to iBaseInterface",tr.coll_object.Link));
                    //Debug.LogFormat(string.Format("Failed to convert {0} to iBaseInterface", tr.coll_object.Link.GetType().ToString()));
                    Debug.LogFormat("Failed to convert {0}/{1} into {2}", intr.GetType().ToString(), u == null ? "null" : u.GetType().ToString(), "iContact");
                }
                if (u != null) spd = u.GetSpeed();
            }
            info = wpndata.ExplOnTarget;
        }
        else
        {
            info = wpndata.ExplOnGround[tr.ground_type];
            Debug.Log(string.Format("wpndata.ExplOnGround[{0}] = {1}", tr.ground_type, info));
        }

        //#pragma message "WARKID: по поводу фларов сдвигаем експлозион"
        //TODO Проверить генерацию вспышки
        //Debug.Log(string.Format("info.explosion: {0} Flare {1}", info.explosion, info.explosion.Flare!=null? info.explosion.Flare:"EMPTY"));
        if (info != null && info.explosion != null && info.explosion.Flare != null)
            org = tr.org - pos.Dir;

        return info;
    }
    protected void MakeDamage(TraceResult tr, EXPLOSION_INFO info)
    {
        if (IsRemote()) return;
        // прямое столкновение с объектом
        if (tr != null && tr.coll_object != null && tr.coll_object.Link != null)
        {  // object collide
            Debug.LogFormat("Direct hit! Damaging {0} of {1} projectile owner handle {2}", tr.coll_object, tr.coll_object.Link, OwnerHandle);
            iBaseInterface uf = (iBaseInterface)(tr.coll_object.Link);
            if (uf != null)
            {
                //iBaseVictim vict = uf.GetInterface<iBaseVictim>();
                iBaseVictim vict=null;
                try
                {
                    vict = (iBaseVictim)uf.GetInterface(iBaseVictim.ID);
                }catch
                {
                    Debug.LogFormat("Failed to convert {0} to iBaseVictim",uf.GetType());
                }
                if (vict != null)
                {
                    float damage_speed = speedf;
                    //iContactun = uf->GetInterface<iContact>();
                    iContact un = (iContact)uf.GetInterface(iContact.ID);
                    if (un != null)
                        damage_speed -= Vector3.Dot(un.GetSpeed(), pos.Dir);
                    //#if !_NO_DAMAGE
                    vict.AddDamage(OwnerHandle, wpndata.Name, wpndata.Damage);
                    //#endif

                }
            }
        }
        // взрывная волна
        //# ifndef _NO_DAMAGE
        if (info != null && info.XRadius > EXPLOSION_INFO.EXPLOSION_CUT_RADIUS)
        {
            rScene.MakeAreaDamage(OwnerHandle, wpndata.Name, pos.Org, info.XRadius, info.XDamage);
        }
        //#endif
    }
    protected void MakeExplosion(EXPLOSION_DATA d, Vector3 org, Vector3 dir, Vector3 spd, DWORD id)
    {
        if (rScene.GetSceneVisualizer() == null) return;
        rScene.GetSceneVisualizer().CreateVisualExplosion(
          d,
          org,
          dir,
          spd,
          id);
    }

    public virtual TraceResult ProcessTrace(float scale)
    {
        //Debug.Log("owner is: " + owner.Ptr());
        Vector3 trace_dir = speed / speedf;
        TraceInfo res = rScene.TraceLine(new Geometry.Line(pos.Org, trace_dir, speedf * scale), (owner.Ptr() != null ? owner.Ptr().GetHMember() : null), (int)CollisionDefines.COLLF_ALL);
        //Debug.Log("res.count " + res.count);
        if (res == null || res.count == 0) return null; //Todo проверить - возможно правильнее false
        pos.Org = res.results[0].org;
        Vector3 org, dir, spd;
        EXPLOSION_INFO info = GetExplInfo(res.results[0], out org, out dir, out spd);
        if (info != null)
        {
            MakeDamage(res.results[0], info);
            //TODO вернуть на место взрыв в точке попадания!
            //MakeExplosion(info.explosion, org, dir, spd, (DWORD)(res.results[0].coll_object != null ? (res.results[0].coll_object).Top(): 0));
            MakeExplosion(info.explosion, org, dir, spd, (DWORD)(res.results[0].coll_object != null ? (DWORD)(res.results[0].coll_object).Top() : 0));
        }
        return res.results[0];
    }
    public virtual bool ProcessTimer(float scale)
    {
        timer -= scale;
        if (timer > 0) return true;
        Vector3 org, dir, spd;
        EXPLOSION_INFO info = GetExplInfo(null, out org, out dir, out spd);
        if (info != null)
        {
            MakeDamage(null, info);
            MakeExplosion(info.explosion, org, dir, spd, 0);
        }
        return false;
    }

    public WPN_DATA GetWeaponData() { return wpndata; }
    public virtual bool IsRemote() { return Remote; }
    public virtual bool IsLocal() { return (!Remote); }

    // создание\удаление
    protected void Init(WPN_DATA wd, iContact _owner, Vector3 _org, Vector3 _dir, bool r)
    {
        wpndata = wd;
        owner.setPtr(_owner);
        if (owner.Ptr() != null) OwnerHandle = owner.Ptr().GetHandle();
        Remote = r;
        // setup org
        pos = new MATRIX();
        pos.Org = _org;
        pos.SetHorizontal(_dir);
        // life timer
        timer = wpndata.GetLifeTime();
    }
    public Projectile(BaseScene s, WPN_DATA _wpndata, iContact _owner, Vector3 _org, Vector3 _dir, bool r)
    {
        BaseActorInit(s);
        wpndata = null;
        visual = null;
        //owner = new TContact();
        owner.setPtr(null);
        OwnerHandle = Constants.THANDLE_INVALID;
        Remote = false;
        Init(_wpndata, _owner, _org, _dir, r);
    }
    public Projectile(BaseScene s)
    {
        BaseActorInit(s);
        wpndata = null;
        visual = null;
        owner.setPtr(null);
        Remote = false;
    }
    ~Projectile()
    {
        Dispose();
    }

    public virtual void Dispose()
    {
        //Debug.Log("Killing " + this + " " + this.GetHashCode().ToString("X8"));
        if (visual != null) visual.OnOwnerDelete();
        //base.Dispose();
        BaseActorDispose();
    }

    // для работы c ProjectileVisual
    public MATRIX GetMatrix() { return pos; }
    public virtual Vector3 GetSpeed() { return speed; }
    public float GetSpeedF() { return speedf; }
    #endregion
};
