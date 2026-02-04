using System;
using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// iUnifiedVariableDB - root
/// </summary>
public interface iUnifiedVariableDB : iUnifiedVariableContainer
{
    new public const DWORD ID = 0xBAADF00D;
    public DWORD GetRootDataSize();
    public iUnifiedVariable GetRoot();
    public iUnifiedVariable CreateRoot(uint ClassID);
    public bool SaveToFile(string filename);

    //template<class T> T* GetRootTpl();
    //template<class T> T* CreateRootTpl();
    public T GetRootTpl<T>() where T : iUnifiedVariable
    {
        iUnifiedVariable t = GetRoot();
        if (t == null) return default(T);
        Type tplType = typeof(T);

        if (tplType == typeof(iUnifiedVariableContainer)) return (T)this;
        
        return default(T);

    }

    iUnifiedVariableContainer CreateRootTpl(uint id)
    {
        iUnifiedVariable t = CreateRoot(id);
        //Debug.Log("crt " + (t != null? "Created":"Failed"));
        if (t == null) return null;
        object tpl = t.Query(id);
        t.Release();
        return (iUnifiedVariableContainer) tpl;

    }
}