using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static RoFlags;

public class ParticleHolder
{
    public List<PARTICLE_SYSTEM> myParticles;
    Vector3 mySpeed;
    IAnimationSupport mySupport;

    public ParticleHolder(IAnimationSupport mySupport)
    {
        this.myParticles = new List<PARTICLE_SYSTEM>();
        this.mySpeed = Vector3.zero;
        this.mySupport = mySupport;
    }

    public bool update(float scale)
    {
        for (int i = 0; i < myParticles.Count;)
        {
            PARTICLE_SYSTEM m = myParticles[i];
            m.Update(scale, mySpeed);
            if (!m.Living())
                eraseParticle(i);
            else
                i++;
        }
        return myParticles.Count != 0;
    }

    public PARTICLE_SYSTEM getParticle(int i) { return myParticles[i]; }

    public void eraseParticle(int i)
    {
        if (myParticles[i].isLocal() == false)
            mySupport.unregisterObject(myParticles[i].GetHashObject());
        //myParticles.erase(i);
        IRefMem.SafeRelease(myParticles[i]);
        myParticles.RemoveAt(i);
    }

    public void addParticle(PARTICLE_SYSTEM p)
    {
        if (p.isLocal() == false)
        {
            mySupport.registerObject(p.GetHashObject());
            p.SetFlag(ROF_NONHASH_OBJECT);
        }
        myParticles.Add(p);
    }

    ~ParticleHolder()
    {
        //for (int i = 0; i < myParticles.Count; ++i)
        //    if (myParticles[i].isLocal() == false)
        //        mySupport.unregisterObject(myParticles[i].GetHashObject());
        Dispose();
    }

    private bool isDisposed = false;
    public void Dispose()
    {
        if (isDisposed) return;
        for (int i = 0; i < myParticles.Count; ++i)
            if (myParticles[i].isLocal() == false)
                mySupport.unregisterObject(myParticles[i].GetHashObject());
        isDisposed = true;
    }

    public void setSpeed(Vector3 spd) { mySpeed = spd; }

    public void call(PS_Call CallFunc)
    {
        for (int i = 0; i < myParticles.Count; i++)
        {
            Debug.Log("particle call what??");
        }
        //(myParticles[i].CallFunc())();
    }

    public IAnimationSupport getSupport()
    {
        return mySupport;
    }

    public int getCount() { return myParticles.Count(); }
}

public delegate void PS_Call();
