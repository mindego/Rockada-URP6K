using crc32 = System.UInt32;
using DWORD = System.UInt32;
using System.Collections.Generic;

public class EngageInfoHolder : IEngageInfo, ITargetEnumer
{
    public virtual int refresh(EngageType type)
    {
        myLastScan = type;
        if (type == EngageType.etWeEngage || type == EngageType.etAny)
            myWeEngage.Clear();
        if (type == EngageType.etTheyEngage || type == EngageType.etAny)
            myTheyEngage.Clear();

        for (int i = 0; i < myAi.mGhostCount; ++i)
        {
            AiUnit ai = myAi.mpUnits[i];
            if (ai==null || ai.GetAI() == null) continue;
            if (type == EngageType.etWeEngage || type == EngageType.etAny)
                checkWeEngage(ai.GetAI());
            if (type == EngageType.etTheyEngage || type == EngageType.etAny)
                checkTheyEngage(ai.GetAI());
        }
        int n = 0;
        if (type == EngageType.etWeEngage || type == EngageType.etAny)
            n += myWeEngage.Count;
        if (type == EngageType.etTheyEngage || type == EngageType.etAny)
            n += myTheyEngage.Count;
        return n;

    }

    bool checkContact(AimingInfo info ) { return false; }
    void checkWeEngage(IBaseUnitAi ai)
    {
        ai.enumTargets(this);
    }
    void checkTheyEngage(IBaseUnitAi ai)
    {
        iContact my = ai.GetContact();
        if (my!=null)
        {
            iContact cnt = my.GetThreat();
            DWORD grp_id;
            DWORD un_id;
            DWORD side;
            if (cnt!=null && myAi.mpMission.GetContactInfo(cnt, out grp_id, out un_id, out side))
                addTheyEngage(grp_id, side == myAi.mpData.Side);
        }

    }

    EngageData getWeData(int i) { return (i < myWeEngage.Count) ? myWeEngage[i] : null; }
    EngageData getTheyData(int i) { return (i < myTheyEngage.Count) ? myTheyEngage[i] : null; }

    public virtual EngageData getData(int i)
    {
        switch (myLastScan)
        {
            case EngageType.etAny:
                if (i < myWeEngage.Count)
                    return getWeData(i);
                else
                {
                    i -= myWeEngage.Count;
                    return getTheyData(i);
                }
            case EngageType.etWeEngage:
                return getWeData(i);
            case EngageType.etTheyEngage:
                return getTheyData(i);
        }
        return null;
    }

    public virtual bool processTarget(iContact cnt, float aim)
    {
        Asserts.Assert(cnt!=null);
        if (cnt!=null && aim > 0.7f)
        {
            DWORD grp_id;
            DWORD un_id;
            DWORD side;
            if (myAi.mpMission.GetContactInfo(cnt, out grp_id, out un_id, out side))
            {
                addWeEngage(grp_id, side == myAi.mpData.Side);
                return true;
            }
        }
        return false;

    }

    public EngageInfoHolder(StdGroupAi grp_ai) { myAi = grp_ai; }

    void addWeEngage(crc32 grp_id, bool fr)
    {
        EngageData data = new EngageData
        {
            myName = grp_id,
            myFriendly = fr
        };
        myWeEngage.Add(data);
    }

    void addTheyEngage(crc32 grp_id, bool fr)
    {
         EngageData data = new EngageData
        {
            myName = grp_id,
            myFriendly = fr
        };
        myTheyEngage.Add(data);
    }

    EngageType myLastScan;
    StdGroupAi myAi;
    List<EngageData> myWeEngage = new List<EngageData>();
    List<EngageData> myTheyEngage = new List<EngageData>();
};

public struct AimingInfo
{
    public iContact myContact;
    public float myAim;
};

