
using geombase;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public interface IVisualDebris
{
    public void OnOwnerDelete();
    public void OnCollide(TraceResult tr, float y);
    public void Appear();
};

public class VisualDebris : VisualBaseActor, IVisualDebris
{
    // start parameters
    readonly DEBRIS_DATA DebrisData;
    // visual
    BaseDebris debris;
    FPO body;
    bool in_hash;
    bool m_IsHidden;
    Vector3 last_speed;
    bool mVisualSetuped;
    float mRecalcRadiusTimer;

    int particles_count, living_count;
    PARTICLE_SYSTEM[] particles;

    void SetupParticles()
    {
        PARTICLE_SYSTEM fly_particle = null;
        if (ToCreateEffect())
            if (DebrisData.ParticleOnFly != 0)
            {
                fly_particle = pVis.CreateParticle(DebrisData.ParticleOnFly, 50);
                if (fly_particle != null)
                {
                    body.AttachObject(fly_particle, DebrisData.SmokeOffset, Vector3.forward, Vector3.up);
#warning ("TODO : AddNonHashObject here if ROF_NONHASH_OBJECT and AddNonHashObject on detach if !ROF_NONHASH_OBJECT ")
                    //if (fly_particle.GetFlag(ROF_NONHASH_OBJECT)) 
                    pVis.AddNonHashObject(fly_particle);
                    fly_particle.Org += body.ImageCenter();
                }
            }

        // count existing particles
        CountParticles pcounter = new CountParticles(this);
        body.EnumerateSubobjects(pcounter, RoFlags.ROFID_PARTICLE);
        if (particles_count != 0)
        {
            AddParticles padder = new AddParticles(this);
            particles = new PARTICLE_SYSTEM[particles_count];
            body.EnumerateSubobjects(padder, RoFlags.ROFID_PARTICLE);
        }
    }
    void DeleteParticles()
    {
        if (living_count == 0) return;
        for (int i = 0; i < particles_count; i++)
            DeleteParticle(i);
    }
    void DeleteParticle(int i)
    {
        PARTICLE_SYSTEM s = particles[i];
        if (s == null) return;
#warning ("WARN : SubNonHashObject particle")
        //if (s.GetFlag(ROF_NONHASH_OBJECT)) 
        pVis.SubNonHashObject(s);

        s.Release();
        particles[i] = null;
        living_count--;
    }
    void DieParticles()
    {

        if (particles_dying || body == null) return;
        for (int i = 0; i < particles_count; i++)
        {
            PARTICLE_SYSTEM s = particles[i];
            if (s == null) continue;
            s.ToWorld();
            s.Detach();
            s.Die();
        }
        particles_dying = true;

    }
    void ProcessParticles(float scale)
    {
        if (living_count != 0)
        {
            if (debris != null)
                last_speed = debris.GetBodySpeed();
            UpdateParticles(scale, last_speed); // speed
            if (smoke_timer <= 0)
                DieParticles();
        }
    }
    void UpdateParticles(float scale, Vector3 Speed)
    {
        for (int i = 0; i < particles_count; i++)
        {
            PARTICLE_SYSTEM s = particles[i];
            if (s == null) continue;
            if (s.Living())
            {
                s.Update(scale, Speed);
            }
            else
                DeleteParticle(i);
        }
        if (body != null && particles_count != 0)
        {
            mRecalcRadiusTimer -= scale;
            if (mRecalcRadiusTimer <= 0.0f)
            {
                body.RecalcRadius();
                mRecalcRadiusTimer = 1.0f;
            }
        }
    }

    // state flags
    bool particles_dying;

    // timers
    float smoke_timer;

    public virtual void OnOwnerDelete()
    {
        last_speed = debris.GetBodySpeed();

        if (IsHidden()) return;

        if (mVisualSetuped)
        {
            DieParticles();
            if (!in_hash)
                pVis.SubNonHashObject(body);
        }

        if (!debris.GetGroundLie() && DebrisData.ExplOnEnd != null)
            if (ToCreateEffect())
                pVis.CreateVisualExplosion(DebrisData.ExplOnEnd.explosion, body.Org, Vector3.up, debris.GetBodySpeed());

        debris = null;
        body = null;
    }
    public virtual void OnCollide(TraceResult tr, float y)
    {
        if (IsHidden()) return;

        EXPLOSION_DATA expl = null;
        if (tr.coll_object != null)
        {
            if (DebrisData.ExplOnTarget != null)
                expl = DebrisData.ExplOnTarget.explosion;
        }
        else
        {
            if (tr.ground_type == TerrainDefs.GT_WATER && mVisualSetuped)
                DieParticles();
            if (DebrisData.ExplOnGround[tr.ground_type] != null)
                expl = DebrisData.ExplOnGround[tr.ground_type].explosion;
        }
        if (ToCreateEffect())
            if (expl != null && debris.GetNorma2() > 15.0f)
            {
                Vector3 coll = body.Org;
                coll.y += y;
                pVis.CreateVisualExplosion(expl, coll, Vector3.up, debris.GetBodySpeed());
            }
    }
    void ProcessTimers(float scale)
    {
        smoke_timer -= scale;
        if (smoke_timer < 0 && mVisualSetuped)
            DieParticles();
    }

    void SetupVariables()
    {
        // state flags
        particles_dying = false;
        mRecalcRadiusTimer = -1.0f;
    }

    bool ToCreateEffect()
    {
        return DebrisData.EffectsProbability > RandomGenerator.Rand01();
    }

    bool IsHidden() { return m_IsHidden; }

    // взаимодействие с окружающей средой
    public override bool Update(float scale)
    {
        if (IsHidden()) return true;

        if (debris == null && living_count == 0) return false;
        if (!mVisualSetuped)
        {
            if (!in_hash)
                pVis.AddNonHashObject(body);
            smoke_timer = Mathf.Abs(Distr.Gauss() * (DebrisData.MaxSmokeTimer - DebrisData.MinSmokeTimer)) + DebrisData.MinSmokeTimer;
            // create particles management
            SetupParticles();
            mVisualSetuped = true;
        }
        ProcessTimers(scale);
        if (mVisualSetuped)
            ProcessParticles(scale);
        return true;
    }

    public virtual void Appear()
    {
        Asserts.AssertBp(IsHidden());

        // создаем случайные взрывы
        if (ToCreateEffect())
        {
            EXPLOSION_DATA le = EffectsCommon.GetRandomListElemFromList<EXPLOSION_DATA>(DebrisData.ExplsOnStart);
            if (le != null) pVis.CreateVisualExplosion(le, body.Org, body.Dir, Vector3.zero);
        }

        m_IsHidden = false;
    }

    public VisualDebris(SceneVisualizer _scene, DEBRIS_DATA _data, BaseDebris _body, bool _in_hash) : base(_scene)
    {
        DebrisData = _data;
        in_hash = _in_hash;
        debris = _body;
        mVisualSetuped = false;
        particles_count = 0;
        living_count = 0;
        particles = null;
        m_IsHidden = true;


        body = debris.GetBody();
        SetupVariables();
    }
    ~VisualDebris()
    {
        Dispose();
    }

    public override void Dispose()
    {
        DeleteParticles();
        if (particles != null) particles = null;//TODO - возможно, здесь нужно диспозить каждый элемент
        base.Dispose();
    }

    public void RegisterParticle() { particles_count++; }
    public void AddParticle(PARTICLE_SYSTEM part) { if (living_count < particles_count) particles[living_count++] = part; }
};

public static class EffectsCommon
{
    public static T GetRandomListElem<T>(TLIST<T> l) where T : class, TLIST_ELEM<T>
    {
        int i = l.Counter();
        if (i == 0) return null;
        i = RandomGenerator.Rand() % i;
        T le;
        for (le = l.Head(); le != null && i != 0; le = l.Next(le), i--) ;
        return le;
    }


    public static T GetRandomListElemFromList<T>(List<object> l) where T : class
    {
        //int i = l.Counter();
        //if (!i) return 0;
        //i = Rand() % i;
        //for (LIST_ELEM* le = l.Head(); le && i; le = le->Next(), i--) ;
        //return (T*)(le->Data());
        int i = l.Count();
        if (i == 0) return null;
        i = RandomGenerator.Rand() % i;
        if (i >= l.Count()) return null;
        return (T) l[i];
    }
}
