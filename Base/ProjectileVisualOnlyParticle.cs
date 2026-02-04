using UnityEngine;
/// <summary>
/// ProjectileVisualMeshAndParticle - только ParticleSystem
/// </summary>
class ProjectileVisualOnlyParticle : ProjectileVisual
{
    #region от VisualBaseActor
    public override bool Update(float scale)
    {
        base.Update(scale);
        if (particle == null) return false;
        if (owner != null)
        {
            //TODO прописать кординаты сюда.
            //*((MATRIX*)particle) = owner->GetMatrix();
            particle.Set(owner.GetMatrix());
            if (DeltaZ != .0f) particle.Org += particle.Dir * DeltaZ;
            particle.Update(scale, owner.GetSpeed());
            if (light != null)
            {
                light.SetPosition(owner.GetMatrix().Org);
                pVis.UpdateHM(hlight);
            }
            return true;
        }
        else
        {
            particle.Org += last_speed * scale;
            //Debug.Log("particle.Org " + particle.GetHashCode().ToString("X8") + " wait " + WaitForParticle +" living " + particle.Living());
            particle.Update(scale, last_speed);
            return (WaitForParticle && particle.Living());
        }
    }
    #endregion
    // для Projectile
    public override void OnOwnerDelete()
    {
        //*((MATRIX*)particle) = owner->GetMatrix();
        particle.Set(owner.GetMatrix());
        last_speed = owner.GetSpeed();
        if (particle!=null)
            particle.Die();
        base.OnOwnerDelete();
    }

    // создание/удаление
    private Vector3 last_speed;
    bool WaitForParticle;
    float DeltaZ;
    PARTICLE_SYSTEM particle;
    public ProjectileVisualOnlyParticle(SceneVisualizer scene, Projectile owner, WPN_DATA wd, float deltaz = .0f, bool wait = false) : base(scene, owner, wd)
    {
        //Debug.Log("PE new ! " + owner);
        particle = null;
        DeltaZ = deltaz;
        WaitForParticle = wait;

        particle = pVis.CreateParticle(wd.ParticleName, wd.GetSpeed());

        if (particle != null)
        {
            pVis.AddNonHashObject(particle);
            particle.Set(owner.GetMatrix());
        }
        else
        {
            Debug.Log("Failed to create particle for " + wd.FileName);
        }

    }
    ~ProjectileVisualOnlyParticle()
    {
        Dispose();
    }
    public override void Dispose()
    {
        if (particle != null)
        {
            pVis.SubNonHashObject(particle);
            particle.Release();
        }

        base.Dispose();
    }
};