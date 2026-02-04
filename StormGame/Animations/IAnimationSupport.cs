using UnityEngine;
using crc32 = System.UInt32;

public interface IAnimationSupport
{
    public PARTICLE_SYSTEM createParticle(crc32 name);
    public ILight createLight(VisLightData  data);
    //public ILenzFlare2 createFlare(string name); //Планируются создавать самим  светом.
    public FPO createFpo(string name);

    public void registerObject(IHashObject r);
    public void unregisterObject(IHashObject r);

    public HMember registerHashed(IHashObject r);
    public void unregisterHashed(HMember hm);

    public void updateHashed(HMember hm);
};

public  interface IAnimationServer : IAnimationVariables
{
    public IAnimationSupport getSupport();
	public float getWeight();
	public void addWeight(float weight);
    public void enumerateSlots(string slot_id, IAnimationSlotEnum an, bool recurse);
};

public interface IAnimationVariables
{
    public float getFloat(crc32 name);
    public Vector3 getVector(crc32 name);
    public ILog getLog();
};

public interface IAnimationSlotEnum
{
    public bool processSlot(SLOT_DATA sld, FPO r);
};

public interface IAnimation : IObject
{
    public bool update(float scale, bool visible);
    public void onDestroyFpo(FPO fpo, IKeepParticle keep) ;
};

public interface IKeepParticle
{
    public void keepParticle(PARTICLE_SYSTEM ps);
};

public interface IEffect : IObject, IAnimationSlotEnum
{
    public bool update(float f);
    public void activate();
    public void deactivate();
    public void onDestroyFpo(FPO fpo, IKeepParticle keep);
};
