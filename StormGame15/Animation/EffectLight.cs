//using static AnimationData;
using UnityEngine;

//using EffectLight = EffectLF<ILight, VisDescLight, EffectLightData>;
public delegate void EffectLF_call(IAnimationSupport sup);
//template <class VisObject, class VisDesc, class Data> struct EffectLF : IEffect {
public class EffectLight : IEffect
{
    //public static VisObject CreateVisObject<VisObject,Data> (Data data,IAnimationSupport srv)
    //{
    //}
    //public static ILight createVisObject<ILight, EffectLightData>(EffectLightData data, IAnimationSupport srv) {
    //public static ILight createVisObject(EffectLightData data, IAnimationSupport srv)
    public static ILight createVisObject(EffectBaseData data, IAnimationSupport srv)
    {
        EffectLightData tmpData=(EffectLightData)data;
        VisLightData dt = new VisLightData
        {
            mRadius = tmpData.myRadius,
            mColor = tmpData.myColor,
            mIntensity = 1.0f
        };
        return srv.createLight(dt);
    }

    public EffectLight(EffectLightData data, IAnimationServer srv)
    {
        myData = data;
        mySupport = srv.getSupport();

    }

    void call(EffectLF_call func)
    {
        throw new System.NotImplementedException("WTF??");
        //for (int i = 0; i < myObjects.Count(); i++)
        //    myObjects[i].func(mySupport);
    }
    AnyDTab<VisDescLight> myObjects;
    EffectLightData myData;
    IAnimationSupport mySupport;

    public virtual bool processSlot(SLOT_DATA sld, FPO r)
    {
        //VisObject vo = createVisObject<VisObject>(myData, mySupport);
        ILight vo = createVisObject(myData, mySupport);
        Debug.Log("Light created:" + vo);
        if (vo != null)
        {
            myObjects.New(new VisDescLight(vo, r, (sld!=null ? sld.Org : Vector3.zero), mySupport, myData.myHashed));

        }
        return vo != null;
    }

    public virtual void activate()
    {
        //call(VisDesc.activate);
        for (int i = 0; i < myObjects.Count(); i++)
            myObjects[i].activate(mySupport);
    }

    public virtual void deactivate()
    {
        //call(VisDesc.deactivate);
        for (int i = 0; i < myObjects.Count(); i++)
            myObjects[i].deactivate(mySupport);
    }

    public virtual bool update(float f)
    {
        //call(VisDesc.update);
        for (int i = 0; i < myObjects.Count(); i++)
            myObjects[i].update(mySupport);
        return myObjects.Count()!=0;
    }

    public virtual void onDestroyFpo(FPO fpo, IKeepParticle keepParticle)
    {
        for (int i = 0; i < myObjects.Count();)
        {
            if (myObjects[i].onDestroyFpo(fpo))
            {
                myObjects[i].destroy(mySupport);
                myObjects.erase(i);
            }
            else
                i++;
        }
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }
}

/// <summary>
/// light
/// </summary>
public class VisDescLight : IVisDesc
{
    Vector3 myPos;
    HMember myInHash;
    FPO myFpo;
    ILight myVisObject;


    public VisDescLight(ILight l, FPO r, Vector3 pos, IAnimationSupport v, bool hashed = true)
    {
        myInHash = null;
        myVisObject = l;
        myPos = pos;
        myFpo = r;
    }

    public void activate(IAnimationSupport sup)
    {
        myInHash = sup.registerHashed(myVisObject.GetHashObject());
    }

    public void deactivate(IAnimationSupport sup)
    {
        sup.unregisterHashed(myInHash);
        myInHash = null;
    }

    public void destroy(IAnimationSupport sup)
    {
        if (myInHash != null)
            deactivate(sup);
    }

    public void update(IAnimationSupport sup)
    {
        if (myInHash != null)
        {
            myVisObject.SetPosition(myFpo.ToWorldPoint(myPos));
            sup.updateHashed(myInHash);
        }
    }

    public bool onDestroyFpo(FPO fpo)
    {
        return myFpo == fpo;
    }

    public void Dispose()
    {
        throw new System.NotImplementedException();
    }
};

/// <summary>
/// flare
/// </summary>
public class VisDescFlare : IVisDesc
{
    Vector3 myPos;
    HMember myInHash;
    FPO myFpo;
    ILenzFlare2 myVisObject;
    bool myHashed;


    public VisDescFlare(ILenzFlare2 l, FPO r, Vector3 pos, IAnimationSupport sup, bool hashed = true)
    {
        myVisObject = l;
        myPos = pos;
        myFpo = r;
        myHashed = hashed;
        myInHash = null;
        if (hashed)
            myInHash = sup.registerHashed(l.GetHashObject());
        else
            sup.registerObject(l.GetHashObject());
    }

    public void activate(IAnimationSupport sup)
    {
        myVisObject.activate(true);
    }

    public void deactivate(IAnimationSupport sup)
    {
        myVisObject.activate(false);
    }

    public void destroy(IAnimationSupport sup)
    {
        if (myHashed)
        {
            sup.unregisterHashed(myInHash);
            myInHash = null;
        }
        else
            sup.unregisterObject(myVisObject.GetHashObject());
    }

    public void update(IAnimationSupport sup)
    {
        myVisObject.SetPosition(myFpo.ToWorldPoint(myPos));
        if (myHashed)
            sup.updateHashed(myInHash);
    }

    public bool onDestroyFpo(FPO fpo)
    {
        return myFpo == fpo;
    }

    public void Dispose()
    {
        throw new System.NotImplementedException();
    }
};
