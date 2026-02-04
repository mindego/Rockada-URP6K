using System;
using UnityEngine;
//using EffectFlare = EffectLF<ILenzFlare2, VisDescFlare, EffectFlareData>;
//using EffectLight = EffectLF<ILight, VisDescLight, EffectLightData>;
//using NonHashList = System.Collections.Generic.List<HashObjectCont>;
public class EffectFpo : IEffect
{
    public EffectFpo(EffectFpoData data, IAnimationServer srv)
    {
        myData = data;
        mySupport = srv.getSupport();
    }

    public virtual bool update(float f) { return myFpos.Count() != 0; }

    public virtual void activate()
    {
        for (int i = 0; i < myFpos.Count(); ++i)
            setImage(myFpos[i], myData.myActiveImage);
    }
    public virtual void deactivate()
    {
        for (int i = 0; i < myFpos.Count(); ++i)
            setImage(myFpos[i], myData.myPassiveImage);
    }
    public virtual void onDestroyFpo(FPO fpo, IKeepParticle keep)
    {
        for (int i = 0; i < myFpos.Count();)
        {
            FPO ps = myFpos[i];
            if (ps.Parent == fpo)
            {
                myFpos.erase(i);
                if (myData.myDeadImage < 0)
                {
                    ps.Detach();
                    ps.Release();
                }
                else
                    setImage(ps, myData.myDeadImage);
            }
            else
                i++;
        }
    }

    public virtual bool processSlot(SLOT_DATA sld, FPO r)
    {
        FPO f = mySupport.createFpo(myData.myName);
        if (f != null)
        {
            myFpos.New(IRefMem.addRef(f));
            setImage(f, myData.myPassiveImage);
            if (sld!=null)
                r.AttachObject(f, sld.Org, sld.Dir, sld.Up);
            else
                r.AttachObject(f, Vector3.zero, Vector3.forward, Vector3.up);
        }
        return f!=null;
    }

    private void setImage(FPO fpo, int image)
    {
        fpo.SetMainImage(image, 0, 0);
        fpo.Top().RecalcRadius();
    }

    IAnimationSlotEnum getEnum() { return this; }

    public void AddRef()
    {
        throw new NotImplementedException();
    }

    public int Release()
    {
        return 0; //STUB!
        throw new NotImplementedException();
    }

    EffectFpoData myData;
    IAnimationSupport mySupport;
    AnyRTab<FPO> myFpos = new AnyRTab<FPO>();
};