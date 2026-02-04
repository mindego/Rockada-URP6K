using UnityEngine;
using crc32 = System.UInt32;

public static class enumGlobalTriggers
{
    public static void addEnabled(TriggersSystem s, ref Tab<crc32> trg)
    {
        int i = 0;
        TriggerValue val;
        while ((val = s.getTrigger(i++)) != null)
        {
            crc32 name = Hasher.HshString(val.myName);
            if (val.myValue == (int)ICoopMission.TechAccess.taEnabled && trg.find(name) < 0)
                trg.New(name);
        }
    }

    public static void addNotDisabled(int count, crc32[] names, TriggersSystem s, ref Tab<crc32> trg)
    {
        for (int i = 0; i < count; ++i)
        {
            crc32 name = names[i];
            int value;
            bool add = s.getTrigger(name, out value) ? value != (int)ICoopMission.TechAccess.taDisabled : true;
            if (add)
                trg.New(name);
        }

    }
}
