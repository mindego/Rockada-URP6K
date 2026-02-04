using DWORD = System.UInt32;
using static CampaignDefines;
using static ObjectiveState;
using UnityEngine;
using System;

public sealed class StdCooperativeAiDefines
{
    public const int OBJECTIVE_IN_PROGRESS = 0;
    public const int OBJECTIVE_SUCCESS = 1;
    public const int OBJECTIVE_FAILED = -1;
}
/// <summary>
/// states
/// </summary>
public enum StdCoopState : DWORD
{
    scs_CreatEnv,               // creating environment                  
    scs_WaitForAdmin,           // waiting for start from admin          
    scs_WaitForAll,             // waiting for all start or timer exceed 
    scs_Running,                // rinning
    scs_ForceDWORD = 0xFFFFFFFF
};

/// <summary>
/// objectives 
/// </summary>
public enum ObjectiveState : DWORD
{
    osInProcess,
    osFailed,
    osCompleted,
    osDWORD = 0xFFFFFFFF
};


public class ObjectiveHolder : TLIST_ELEM<ObjectiveHolder>,IDisposable
{
    DWORD mName;
    bool mPrimary;
    iUnifiedVariableInt mpObjective;
    public DWORD GetName() { return mName; }
    public bool IsPrimary() { return mPrimary; }
    public iUnifiedVariableInt Objective() { return mpObjective; }
    public void Destroy()
    {
        if (mpObjective!=null) mpObjective.Delete();
        mpObjective = null;
    }

    public void setState(ObjectiveState state)
    {
        switch (state)
        {
            case osInProcess: mpObjective.SetValue(0); break;
            case osFailed: mpObjective.SetValue(-1); break;
            case osCompleted: mpObjective.SetValue(1); break;
        }

    }

    public ObjectiveState getState()
    {
        int value = mpObjective.GetValue();
        switch (value)
        {
            case 0: return osInProcess;
            case -1: return osFailed;
            case 1: return osCompleted;
        }
        return osInProcess;
    }

    public bool isInProcess()
    {
        return getState() == osInProcess;
    }

    public bool isFailed()
    {
        return getState() == osFailed;
    }

    public bool isCompleted()
    {
        return getState() == osCompleted;
    }

    public ObjectiveHolder Next()
    {
        return myElem.Next();
    }

    public ObjectiveHolder Prev()
    {
        return myElem.Prev();
    }

    public void SetNext(ObjectiveHolder t)
    {
        myElem.SetNext(t);
    }

    public void SetPrev(ObjectiveHolder t)
    {
        myElem.SetPrev(t);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    TLIST_ELEM<ObjectiveHolder> myElem;
    public ObjectiveHolder(DWORD name, bool primary, iUnifiedVariableInt m)
    {
        mName = name;
        mpObjective = m;
        mPrimary = primary;
        myElem = new TLIST_ELEM_IMP<ObjectiveHolder>();
    }
    ~ObjectiveHolder()
    {
        mpObjective.Release();
    }
}
/// <summary>
/// checking appear
/// </summary>
public class CoopCheckAppear : iCheckAppear
{
    public CoopCheckAppear(DWORD num) :base(num) { }

    public override bool IsGroupAppear(GROUP_DATA grp)
    {
        return isAppear(grp.GetFlag(CF_APPEAR_MASK), mClientCount);
    }
    public override  bool IsUnitAppear(UNIT_DATA un, int difficulty)
    {
        bool appear = isAppear(un.GetFlag(CF_APPEAR_MASK), mClientCount);
        if (appear && un.GetFlag(CF_APPEAR_NEW_FORMAT)!=0)
        {
            switch (difficulty)
            {
                case 0: return un.GetFlag(CF_APPEAR_ARCADE)!=0;
                case 1: return un.GetFlag(CF_APPEAR_NORMAL)!=0;
                case 2: return un.GetFlag(CF_APPEAR_HARDCORE)!=0;
            }
        }
        return appear;
    }
};