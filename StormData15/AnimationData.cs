using System.Collections.Generic;
using UnityEngine;
using crc32 = System.UInt32;

public class AnimationData
{
    public AnimationData()
    {
        myWeight = 0;
        myCondition = null;
    }
    public float myWeight;

    //AnyDTab<EffectParticleData*> myParticles;
    //AnyDTab<EffectLightData*> myLights;
    //AnyDTab<EffectFlareData*> myFlares;
    //AnyDTab<EffectFpoData*> myFpos;
    //AnyDTab<Condition*> myConds;
    public List<EffectParticleData> myParticles = new();
    public List<EffectLightData> myLights = new();
    public List<EffectFlareData> myFlares = new();
    public List<EffectFpoData> myFpos = new();
    public List<Condition> myConds = new();
    string myCondition;
    public bool load(READ_TEXT_STREAM f)
    {
        if (f.GetNextItem()[0] != '{') return false;
        while (true)
        {
            string c = f.GetNextItem();
            if (c[0] == '}')
            {
                break;
            }
            if (f.LoadFloat(ref myWeight, "Weight")) continue;

            // for compatibiluty
            if (f.Recognize("VarName")) { f.GetNextItem(); continue; }
            if (f.Recognize("VarLimitMin")) { f.GetNextItem(); continue; }
            if (f.Recognize("VarLimitMax")) { f.GetNextItem(); continue; }

            if (f.Recognize("Condition"))
            {
                string cond = f.GetNextItem();
                myCondition = cond;
                if (cond != null && Condition.parseCondition(ref myConds, cond))
                    continue;
                return false;
            }
            if (f.Recognize("EffectParticle"))
            {
                EffectParticleData data = new EffectParticleData();
                if (data.load(f))
                {
                    myParticles.Add(data);
                    continue;
                }
                data = null;
                return false;
            }
            if (f.Recognize("EffectLight"))
            {
                EffectLightData data = new EffectLightData();
                if (data.load(f))
                {
                    myLights.Add(data);
                    continue;
                }
                data = null;
                return false;
            }
            if (f.Recognize("EffectFlare"))
            {
                EffectFlareData data = new EffectFlareData();
                if (data.load(f))
                {
                    myFlares.Add(data);
                    continue;
                }
                data = null;
                return false;
            }
            if (f.Recognize("EffectFpo"))
            {
                EffectFpoData data = new EffectFpoData();
                if (data.load(f))
                {
                    myFpos.Add(data);
                    continue;
                }
                data = null;
                return false;
            }

            return false;
        }
        //Debug.Log("AnimationData loading  success");
        return true;
    }
};
public class EffectBaseData
{
    public string mySlot;
    public bool myRecurse;
    public EffectBaseData()
    {
        myRecurse = false;
        mySlot = null;
    }
    public bool load(READ_TEXT_STREAM f)
    {
        if (f.GetNextItem()[0] != '{') return false;
        while (true)
        {
            string c = f.GetNextItem();
            if (c[0] == '}') break;
            if (!processToken(f, c))
                return false;
        }
        return true;
    }
    public virtual bool processToken(READ_TEXT_STREAM f, string c)
    {
        do
        {
            if (f.LdAS(ref mySlot, "SlotName")) continue;
            if (f.LoadBool(ref myRecurse, "ProcessFpoTree")) continue;
            return false;
        } while (false);
        return true;
    }
};

public class EffectLightData : EffectBaseData
{
    public EffectLightData()
    {
        myRadius = 0;
    }
    //public Color myColor;
    public Vector3 myColor;
    public float myRadius;
    public bool myHashed = true;
    public override bool processToken(READ_TEXT_STREAM f, string c)
    {
        do
        {
            if (f.LoadFloat(ref myRadius, "LightRadius")) continue;
            if (f.Recognize("LightColor"))
            {
                myColor = f.recognizeVector3f(f) * Storm.Math.OO256;
                continue;
            }
            return base.processToken(f, c);
        } while (false);
        return true;
    }
};

public class EffectFpoData : EffectBaseData
{
    public EffectFpoData()
    {
        myName = null;
        myDeadImage = -1;
        myActiveImage = 1;
        myPassiveImage = 0;
    }
    public string myName;
    public int myActiveImage, myDeadImage, myPassiveImage;
    public override bool processToken(READ_TEXT_STREAM f, string c)
    {
        do
        {
            if (f.LdAS(ref myName, "FpoName")) continue;
            if (f.LoadInt(ref myActiveImage, "ActiveImage")) continue;
            if (f.LoadInt(ref myPassiveImage, "PassiveImage")) continue;
            if (f.LoadInt(ref myDeadImage, "DeadImage")) continue;
            return base.processToken(f, c);
        } while (false);
        return true;
    }
};

public class EffectParticleData : EffectBaseData
{
    public EffectParticleData()
    {
        myOwnerHandle = false;
        myParticle = null;
    }
    public string myParticle;
    public bool myOwnerHandle;
    public override bool processToken(READ_TEXT_STREAM f, string c)
    {
        do
        {
            if (f.LdAS(ref myParticle, "ParticleName")) continue;
            if (f.LoadBool(ref myOwnerHandle, "DelayedDeath")) continue;
            return base.processToken(f, c);
        } while (false);
        return true;
    }
};

public class EffectFlareData : EffectBaseData
{
    public EffectFlareData()
    {
        myFlare = null;
    }
    string myFlare;
    bool myHashed = true;
    public override bool processToken(READ_TEXT_STREAM f, string c)
    {
        do
        {
            if (f.LdAS(ref myFlare, "Flare")) continue;
            if (f.LoadBool(ref myHashed, "Hashed")) continue;
            return base.processToken(f, c);
        } while (false);
        return true;

    }
};

public class AnimationPackage
{
    public AnimationPackage(string name)
    {
        myName = name;
    }
    public List<AnimationData> myAnimations = new List<AnimationData>();
    string myName;
    public bool load(READ_TEXT_STREAM f)
    {
        if (f.GetNextItem()[0] != '{') return false;
        while (true)
        {
            string c = f.GetNextItem();
            if (c[0] == '}')
            {
                break;
            }
            if (f.Recognize("Animation"))
            {
                AnimationData data = new AnimationData();
                if (data.load(f))
                {
                    myAnimations.Add(data);
                    continue;
                }
                data = null;
                return false;
            }

            return false;
        }
        return true;
    }

    public static void loadAnimationPackages(IMappedDb db)
    {
        //parseData(db, "animation packages", "Animations", "Animations.txt", "[STORM ANIMATION DATA FILE V1.0]", "AnimationPackage", insertAnimationPackage);
        LoadUtils.parseData(db, "animation packages", "Animations", "Animations.txt", "[STORM ANIMATION DATA FILE V1.0]", "AnimationPackage", insertAnimationPackage);
    }

    public static void insertAnimationPackage(READ_TEXT_STREAM f)
    {
        AnimationPackage data = new AnimationPackage(f.GetNextItem());
        StormLog.LogMessage("Loading AnimationPackage: " + data.myName, StormLog.logPriority.NORMAL);
        if (!data.load(f))
        {
            data = null;
            //PARSE_ERROR;
            throw new System.Exception(string.Format("{0} {1} {2}", f.LineNumber(), "Animations", f.DebugStream()));
        }
        else
            gAnimationPackages.Add(data);
    }
    public static void clearAnimationPackages()
    {
        gAnimationPackages.Clear();
    }

    public static AnimationPackage getAnimationPackage(crc32 name)
    {
        for (int i = 0; i < gAnimationPackages.Count; ++i)
            if (Hasher.HashString(gAnimationPackages[i].myName) == name)
                return gAnimationPackages[i];
        return null;
    }

    public static List<AnimationPackage> gAnimationPackages = new List<AnimationPackage>();
};