using System.Collections.Generic;
using static SceneVisualizer;

public delegate IEffect CreateEffect(EffectBaseData dt, IAnimationServer srv); //Так не работает
//public delegate IEffect CreateEffect(EffectParticleData dt, IAnimationServer srv); //А так - работает
public abstract class BaseEmitter : IAnimation
{
    public BaseEmitter(IAnimationServer gm, AnimationData data)
    {
        myGame = gm; myData = data;

        myEffects.SetCount(data.myParticles.Count + data.myLights.Count + data.myFlares.Count);
        myEffects.SetCount(0);
        int i;
        for (i = 0; i < data.myParticles.Count; ++i)
            createEffect<EffectParticleData>(data.myParticles[i], gm, createParticleEffect);
        for (i = 0; i < data.myLights.Count; ++i)
            createEffect<EffectLightData>(data.myLights[i], gm, createLightEffect);
        for (i = 0; i < data.myFlares.Count; ++i)
            createEffect<EffectFlareData>(data.myFlares[i], gm, createFlareEffect);
        for (i = 0; i < data.myFpos.Count; ++i)
            createEffect<EffectFpoData>(data.myFpos[i], gm, createFpoEffect);
    }

    //public IEffect createParticleEffect(EffectBaseData dt, IAnimationServer srv)
    //{
    //    return SceneVisualizer.createParticleEffect((EffectParticleData)dt, srv);
    //    //return createObject < EffectParticle,const EffectParticleData&> (dt, srv);
    //}

    void createEffect<DataClass>(EffectBaseData data,IAnimationServer srv, CreateEffect create)
    {
        IEffect eff = create(data, srv);
        if (eff!=null)
        {
            myGame.enumerateSlots(data.mySlot, eff,data.myRecurse);
            myEffects.New(eff);
        }
    }
    //void createEffect<DataClass>(DataClass data, IAnimationServer srv, IEffect (create) (DataClass& , IAnimationServer srv)) 
    //{
    //IEffect eff = (*create)(data, srv);
    //    if (eff!=null) {
    //myGame.enumerateSlots(data.mySlot, eff, data.myRecurse);
    //myEffects.Add(eff);
    //    }
    //}

    public virtual bool update(float scale, bool visible = true)
    {
        for (int i = 0; i < myEffects.Count();)
        {
            if (!myEffects[i].update(scale))
                myEffects.erase(i);
            else
                i++;
        }
        return myEffects.Count() != 0;
    }

    public virtual void onDestroyFpo(FPO fpo, IKeepParticle keep)
    {
        for (int i = 0; i < myEffects.Count(); ++i)
            myEffects[i].onDestroyFpo(fpo, keep);
    }

    protected void activate()
    {
        myIsActivated = true;
        for (int i = 0; i < myEffects.Count(); ++i)
            myEffects[i].activate();
        myGame.addWeight(myData.myWeight);
    }

    protected void deactivate()
    {
        myIsActivated = false;
        for (int i = 0; i < myEffects.Count(); ++i)
            myEffects[i].deactivate();
        myGame.addWeight(-myData.myWeight);
    }

    protected bool isActivated()
    {
        return myIsActivated;
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }

    protected AnyRTab<IEffect> myEffects = new AnyRTab<IEffect>();
    //protected List<IEffect> myEffects = new();

    protected IAnimationServer myGame;
    protected AnimationData myData;
    protected bool myIsActivated = false;
}