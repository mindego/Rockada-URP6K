using uvc = iUnifiedVariableContainer;
using uva = iUnifiedVariableArray;
using DWORD = System.UInt32;
using System;
using System.Collections.Generic;
using UnityEngine;
//using BaseRecord = ScriptableEvent<IRecord, IGamesetChange, IGamesetChange::onRenameRecord, IGamesetChange::onDeleteRecord, IGamesetChange::onChange> BaseRecord;
public static class Savings
{
    public static bool loadStringVar(string name, ref string str, uvc gsd, ILoadErrorLog log)
    {
        if (!gsd.getString(name, ref str))
            return addVarMissed(gsd, log, name);
        return true;
    }

    public static bool loadRefVar(string name, ref string str, uvc gsd, ILoadErrorLog log)
    {
        //Debug.Log("loadRefVar " + name);
        if (!gsd.getRef(name, ref str))
            return addVarMissed(gsd, log, name);
        return true;
    }

    public static bool loadIntVar(string name, ref int str, uvc gsd, ILoadErrorLog log) //TODO - возможно, правилние везде out вместо ref
    {
        if (!gsd.getInt(name, out str))
            return addVarMissed(gsd, log, name);
        return true;
    }

    public static bool loadIntVar(string name, ref uint str, uvc gsd, ILoadErrorLog log) //TODO Проверить конвертацию из uint в int
    {
        int tmp = 0;
        if (!loadIntVar(name, ref tmp, gsd, log)) return false;

        //str = Convert.ToUInt32(tmp);
        str = BitConverter.ToUInt32(BitConverter.GetBytes(tmp));
        return true;
    }
    //public static bool loadUIntVar(string name, ref uint str, uvc gsd, ILoadErrorLog log) //TODO Проверить конвертацию из uint в int
    //{
    //    int tmp = 0;
    //    if (!loadIntVar(name, ref tmp, gsd, log)) return false;

    //    //str = Convert.ToUInt32(tmp);
    //    str = BitConverter.ToUInt32(BitConverter.GetBytes(tmp));
    //    return true;
    //}

    public static bool addResolveError(uvc cont, ILoadErrorLog log, string name = null)
    {
        if (log != null)
        {
            name = cont.getNameLong();
            log.addWarning(DataCore.LE_RESOLVE_ERROR, name, "skipped");
        }
        return false;
    }

    public static void addResolveWarning(uvc cont, ILoadErrorLog log, string name = null)
    {
        Debug.Log(cont);
        Debug.Log(cont.getNameShort());
        if (log != null)
        {
            name = cont.getNameLong();
            log.addWarning(DataCore.LE_RESOLVE_ERROR, name, "set to empty");
        }
    }

    public static bool addVarMissed(uvc cont, ILoadErrorLog log, string var_name = null)
    {
        if (log != null)
        {
            string name = cont.getNameLong();
            //char buffer[256];
            //wsprintf(buffer, "%s\\%s", name, var_name);
            string buffer = string.Format("{0}\\{1}", name, var_name);
            log.addWarning(DataCore.LE_VAR_MISSED, buffer, "skipped");
        }
        return false;
    }

    /// <summary>
    /// Создание объекта без параметра
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public delegate T createNoPar<T>();
    public static void enumSimpleArray<CreatureImpl, UniType, Creator>(uva root, Creator i, createNoPar<CreatureImpl> create, ILoadErrorLog log) where CreatureImpl : IDeletableMember,ILoadableMember where UniType:class,iUnifiedVariable
    {
        uint n = root.GetSize();
        UniType gsd_ev;
        for (uint handle = 0; handle < n; handle++)
            if ((gsd_ev = root.GetVariableTpl<UniType>(handle)) != null)
                loadSimpleArray<CreatureImpl, UniType, Creator>(gsd_ev, i, create, log);
    }


    public static void loadSimpleArray<CreatureImpl, UniType, Creator>(UniType gsl, Creator i, createNoPar<CreatureImpl> create, ILoadErrorLog log) where CreatureImpl : IDeletableMember, ILoadableMember where UniType : class,iUnifiedVariable
    {
        //CreatureImpl rec = i.create();
        CreatureImpl rec = create();
        if (!rec.load(gsl, log))
            rec.deleteMe();
    }


    public static void loadTwin<CreatureImpl, UniType, Creator>(string name, iUnifiedVariableContainer gsd, iUnifiedVariableContainer gsl, Creator i, createParString<CreatureImpl> create, ILoadErrorLog log) where CreatureImpl : IDeletableMember, ILoadableTransMember
    {
        CreatureImpl rec = create(name);
        //Debug.Log("My type: " + typeof(CreatureImpl) + " rec " + (rec != null? rec.GetType():"[Failed]"));
        if (!rec.load(gsd, gsl, log))
            rec.deleteMe();
    }
    public delegate T createParString<T>(string s);
    //template<class CreatureImpl, class UniType, class Creator> 
    //public static void enumSimple<CreatureImpl,UniType,Creator>(uvc root1, Creator* i, CreatureImpl* (Creator::* create)(cstr), ILoadErrorLog* log)
    public static void enumSimple<CreatureImpl, UniType, Creator>(uvc root1, Creator i, createParString<CreatureImpl> create, ILoadErrorLog log) where UniType : class,iUnifiedVariable where CreatureImpl : IGamesetMember, ILoadableMember
    {
        UniType gsd_ev;
        for (DWORD handle = 0; (handle = root1.GetNextHandle(handle)) != 0;)
        {
            //Debug.Log("Processing handle " + handle + " of container " + root1.getNameShort() + "\n"+root1);
            if ((gsd_ev = root1.GetVariableTpl<UniType>(handle)) != null)
            {
                //Debug.Log(gsd_ev);
                loadSimple<CreatureImpl, UniType, Creator>(gsd_ev.getNameShort(), gsd_ev, i, create, log);
            }
        }
    }

    //template<class CreatureImpl, class UniType, class Creator> 
    public static void loadSimple<CreatureImpl, UniType, Creator>(string name, UniType gsl, Creator i, createParString<CreatureImpl> create, ILoadErrorLog log) where CreatureImpl : IGamesetMember,ILoadableMember
    {
        CreatureImpl rec = create(name);
        if (rec != null && !rec.load(gsl, log))
            rec.deleteMe();
    }



    internal static void saveSimpleArray<T1, T2, T3>(uva ar, List<T1> myRoads)
    {
        throw new NotImplementedException();
    }

    internal static void saveTwin<T1, T2>(T2 cont1, T2 cont2, Dictionary<string, T1> mySelectionEvents, string name)
    {
        throw new NotImplementedException();
    }

    internal static void saveSimple<T1, T2>(iUnifiedVariable cont1, Dictionary<string, T1> myLocations, string name = null)
    {
        throw new NotImplementedException();
    }
}
