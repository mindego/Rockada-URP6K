using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// ProjectileVisual - базовый класс для визуальных частей оружия
/// </summary>
public class ProjectileVisual : VisualBaseActor
{
    #region от VisualBaseActor
    public override bool Update(float scale)
    {
        if (owner == null) return false;
        if (sound!=null) sound.UpdateController(owner.GetMatrix().Org);
        //Debug.Log("Moving sound source for ProjectileVisual " + this + " of " + owner + " to " + owner.GetMatrix().Org) ;
        return true;
    }
    #endregion
    // для Projectile
    public virtual void OnOwnerDelete()
    {
        if (light != null)
        {
            pVis.DeleteHM(hlight);
            //SafeRelease(light);
            //light_count--;
        }
        IMemory.SafeRelease(sound);
        sound = null;
        owner = null;
    }

    // создание/удаление
    protected ILight light;
    protected HMember hlight;
    I3DSoundEvent sound;
    //AudioClip sound;
    protected Projectile owner;
    public ProjectileVisual(SceneVisualizer _vis, Projectile _owner, WPN_DATA wd) : base(_vis)
    {
        Debug.Log("New ProjectileVisual " + this+ "for " + _owner + " : " + wd.FullName);
        light = null;
        hlight = null;
        sound = null;
        owner = _owner;

        // light
        Color cl = wd.GetLightColor();
        float lr = wd.GetLightRadius();
        if (cl != null && lr > WPN_DATA.WPN_LIGHT_CUT_RADIUS)
        {
            VisLightData ld;
            ld.mColor = new Vector3(cl.r, cl.g, cl.b);
            ld.mRadius = lr;
            ld.mIntensity = 1;
            light = pVis.CreateLight(ld);
            //light_count++;
            if (light != null)
            {
                light.SetPosition(owner.GetMatrix().Org);
                hlight = pVis.CreateHM(light.GetHashObject());
            }
        }
        //sound
        // GSParam sound_params(true,false,&(owner->GetMatrix().Org),&(owner->GetSpeed()),0,0);
        I3DSoundEventController ctr =
          RefSoundCtrWrapper.CreateSoundCtrWrapper(owner.GetMatrix().Org, owner.GetSpeed(), (DWORD)this.GetHashCode());

        sound = pVis.Get3DSound().LoadEvent(
          "Weapon", wd.FullName, "Fly", true, false, ctr);

        ctr.Release();

        if (sound!=null) sound.Start();
        
        //Debug.Log("Звук летящего снаряда")
    }
    ~ProjectileVisual()
    {
        Asserts.AssertBp(owner == null);
    }

    public static ProjectileVisual Create(SceneVisualizer scene, Projectile owner, WPN_DATA wpndata)
    {
        if (scene == null)
            return null;
        if (wpndata.MeshName != 0xFFFFFFFF)
        {
            if (wpndata.ParticleName != 0xFFFFFFFF)
                return new ProjectileVisualMeshAndParticle(scene, owner, wpndata);
            else
                return new ProjectileVisualOnlyMesh(scene, owner, wpndata);
        }
        else
        {
            if (wpndata.ParticleName != 0xFFFFFFFF)
                return new ProjectileVisualOnlyParticle(scene, owner, wpndata);
            else
                return null;
        }
    }
};
