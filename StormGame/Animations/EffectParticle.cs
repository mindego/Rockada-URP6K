using UnityEngine;
using static AnimationParams;
using crc32 = System.UInt32;

public class EffectParticle : IEffect
{
    public EffectParticle(EffectParticleData data, IAnimationServer srv)
    {
        myGame = srv;
        myData = data;
        myHolder = new ParticleHolder(srv.getSupport());
    }

    public bool update(float scale)
    {
        myHolder.setSpeed(myGame.getVector(iOSpeedName));
        return myHolder.update(scale);
    }

    public virtual bool processSlot(SLOT_DATA sld, FPO r)
    {
        return myData.myParticle != null && createParticle(Hasher.HashString(myData.myParticle), r, sld);
    }

    public void activate()
    {
        for (int i = 0; i < myHolder.myParticles.Count; i++)
        {
            //Debug.Log("particle call what??");
            myHolder.myParticles[i].Activate();
        }
        //myHolder.call(PARTICLE_SYSTEM.Activate);
    }

    public void deactivate()
    {
        for (int i = 0; i < myHolder.myParticles.Count; i++)
        {
            //Debug.Log("particle call what??");
            myHolder.myParticles[i].DeActivate();
        }
        //myHolder.call(PARTICLE_SYSTEM.DeActivate);
    }

    public void onDestroyFpo(FPO fpo, IKeepParticle keep)
    {
        for (int i = 0; i < myHolder.getCount();)
        {
            PARTICLE_SYSTEM ps = myHolder.getParticle(i);
            if (ps.Parent == fpo)
            {
                if (myData.myOwnerHandle)
                {
                    ps.ToWorld();
                    keep.keepParticle(ps);
                }
                else
                    ps.Release();
                ps.Detach();
                myHolder.eraseParticle(i);
            }
            else
                i++;
        }
    }

    protected bool createParticle(crc32 name, FPO r, SLOT_DATA sld)
    {
        //Debug.LogFormat("Creating PARTICLE_SYSTEM for {0} in {1}",r.TextName,sld !=null ? sld.Name:"no slot");
        PARTICLE_SYSTEM part = myHolder.getSupport().createParticle(name);
        if (part != null)
        {
            myHolder.addParticle(IRefMem.addRef(part));
            if (sld != null)
                r.AttachObject(part, sld.Org, sld.Dir, sld.Up, false);
            else
                r.AttachObject(part, Vector3.zero, Vector3.forward, Vector3.up, false);
            part.DeActivate();
        }
        return part!=null;
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        return 0; //STUB!

        //throw new System.NotImplementedException();
    }

    private IAnimationServer myGame;
    ParticleHolder myHolder;
    EffectParticleData myData;
};