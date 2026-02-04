public class LoadTriggers : IEnumer<TriggerValue>
{
    ITriggerService myTriggers;

    public LoadTriggers(ITriggerService trigs) {
        myTriggers = trigs;
    }

    public void process(TriggerValue val)
    {
        myTriggers.setTrigger(val.myName, val.myValue, true);
    }

    public static void enumGlobalTriggers(UniVarContainer cont, IEnumer<TriggerValue> enumer)
    {
        uint handle = 0;
        while ((handle = cont.GetNextHandle(handle))!=0)
        {
            iUnifiedVariableInt trig = cont.openInt(handle);
            if (trig!=null)
            {
                string name = trig.getNameShort();
                TriggerValue val = new TriggerValue(name, trig.GetValue(),true);
                enumer.process(val);
            }
        }
    }

    public static int saveTriggers(ITriggerService trigs, UniVarContainer root)
    {
        int i = 0;
        int n = 0;
        TriggerValue val;
        while ((val = trigs.getTrigger(i++))!=null)
        {
            if (val.myGlobal)
            {
                root.setInt(val.myName, val.myValue);
                n++;
            }
        }
        return n;
    }

};


