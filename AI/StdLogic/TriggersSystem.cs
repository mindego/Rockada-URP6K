using System.Collections.Generic;
using crc32 = System.UInt32;

public class TriggersSystem : ITriggerService
{
    public void setTrigger(string name, int value, bool global = false)
    {
        TriggerValue val = get(Hasher.HshString(name));
        if (val==null)
        {
            val = new TriggerValue(name, value, global);
            //myTriggers.New() = val;
            myTriggers.Add(val);
        }
        else
        {
            val.myValue = value;
            if (global == true)
                val.myGlobal = global;
        }
    }
    public bool getTrigger(string name, out int value)
    {
        return getTrigger(Hasher.HshString(name), out value);
    }

    public bool isTriggerGlobal(string name)
    {
        TriggerValue val = get(Hasher.HshString(name));
        return val!=null ? val.myGlobal : false;
    }

    public TriggerValue getTrigger(int i)
    {
        return i < myTriggers.Count ? myTriggers[i] : null;
    }

    public bool getTrigger(crc32 name, out int value)
    {
        TriggerValue val = get(name);
        value = val!=null ? val.myValue : 0;
        return val!=null;
    }

    private TriggerValue get(crc32 hn)
    {
        for (int i = 0; i < myTriggers.Count; ++i)
        {
            if (Hasher.HshString(myTriggers[i].myName) == hn)
                return myTriggers[i];
        }
        return null;
    }

    //AnyDTab<TriggerValue*> myTriggers;
    List<TriggerValue> myTriggers = new List<TriggerValue>();
};
