using System;
using System.IO;
using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// iUnifiedVariableContainer - базовый класс для переменных-контейнеров
/// </summary>
public interface iUnifiedVariableContainer : iUnifiedVariable
{
    new public const DWORD ID = 0xF705FAA8;
    //public object Query(uint ClassID);
    //template<class C> C* Query() { return (C*)Query(C::ID); }
    public DWORD GetSize();
    public DWORD GetNextHandle(DWORD Handle);
    public int GetNameLengthByHandle(DWORD Handle);
    public string GetNameByHandle(ref string buffer, DWORD Handle);
    public DWORD GetHandleByName(string Name);
    public int GetSizeByHandle(DWORD Handle);
    bool getRef(string name, ref string data, string def = null)
    {
        iUnifiedVariableReference n = openReference(name);
        if (n != null)
            n.StrCpy(out data);
        //n->StrCpy(data.lock (n->StrLen() + 1));
        else if (def != null)
            data = def;
        return n != null;
    }

    iUnifiedVariableReference openReference(string name)
    {
        return GetVariableTpl<iUnifiedVariableReference>(name);
    }

    public iUnifiedVariable GetVariableByHandle(DWORD Handle);
    public iUnifiedVariable GetVariableByName(string name, DWORD crc = 0xFFFFFFFF);
    //TODO В оригинале было int ClassUD, возоможно надо вернуть

    public iUnifiedVariable CreateVariableByName(uint ClassID, string name);
    bool setVector(string name, Vector3 data)
    {
        iUnifiedVariableVector n = createVector(name);
        if (n != null) n.SetValue(data);
        return n != null;
    }

    void setFloat(string name, float data)
    {
        iUnifiedVariableFloat n = createFloat(name);
        if (n != null) n.SetValue(data);
    }

    iUnifiedVariableFloat createFloat(string name)
    {
        return CreateVariableTpl<iUnifiedVariableFloat>(name);
    }

    iUnifiedVariableVector createVector(string name)
    {
        return CreateVariableTpl<iUnifiedVariableVector>(name);
    }
    public bool Rename(string pSrcName, string pDstName);

    //template<class T> T* CreateVariableTpl(const char* name);
    //template<class T> T* GetVariableTpl(DWORD Handle);
    //template<class T> T* GetVariableTpl(const char* name, DWORD crc=0xFFFFFFFF);

    public bool getInt(string name, out int data, int def = 0)
    {
        iUnifiedVariableInt n = openInt(name);
        if (n != null)
            data = n.GetValue();
        else
            data = def;
        return n != null;
    }

    void setInt(string name, int data)
    {
        iUnifiedVariableInt tmp = createInt(name);
        tmp.SetValue(data);
    }
    void setInt(string name, uint data)
    {
        int tmp = BitConverter.ToInt32(BitConverter.GetBytes(data));
        setInt(name, tmp);
    }
    iUnifiedVariableInt createInt(string name)
    {
        return CreateVariableTpl<iUnifiedVariableInt>(name);
    }


    public bool setString(string name, string data)
    {
        iUnifiedVariableString n = createString(name);
        if (n != null) n.SetValue(data);
        return n != null;
    }



    iUnifiedVariableString createString(string name)
    {
        return CreateVariableTpl<iUnifiedVariableString>(name);
    }
    iUnifiedVariableString createString(string name,string value)
    {

        iUnifiedVariableString res = CreateVariableTpl<iUnifiedVariableString>(name);
        if (res != null) res.SetValue(value);
        return res;
    }

    public bool getInt(string name, out uint data, uint def = 0)
    {
        data = 0;
        int defInt = BitConverter.ToInt32(BitConverter.GetBytes(def));
        int tmp;
        if (!getInt(name, out tmp, defInt)) return false;

        data = BitConverter.ToUInt32(BitConverter.GetBytes(tmp));
        return true;
    }

    public int getInt(string name)
    {
        var myInt = openInt(name);
        //Debug.Log(string.Format("{0} : {1}", name, myInt != null? myInt:"NULL"));
        return myInt == null ? 0 : myInt.GetValue();
    }
    iUnifiedVariableInt openInt(string name)
    {
        return GetVariableTpl<iUnifiedVariableInt>(name);
    }
    public bool getFloat(string name, ref float data, float def = 0)
    {
        iUnifiedVariableFloat n = openFloat(name);
        if (n != null)
            data = n.GetValue();
        else
            data = def;
        return n != null;
    }
    iUnifiedVariableFloat openFloat(string name)
    {
        return GetVariableTpl<iUnifiedVariableFloat>(name);
    }

    public iUnifiedVariableArray createArray(string name)
    {
        return CreateVariableTpl<iUnifiedVariableArray>(name);
    }
    //CreateVariableTpl<T>(string name) where T : iUnifiedVariable
    //{
    //    iUnifiedVariable t = CreateVariableByName(T.GetID(), name);
    //    //if (t==null) return 0;
    //    //T* tpl = (T*)t->Query(T::ID);
    //    //  t->Release();
    //    return (T)t;
    //}

    public T CreateVariableTpl<T>(string name) where T : iUnifiedVariable
    {
        Type vartype = typeof(T);
        iUnifiedVariable res = null;
        if (vartype == typeof(iUnifiedVariableContainer)) res = CreateVariableByName(iUnifiedVariableContainer.ID, name);
        if (vartype == typeof(iUnifiedVariableArray)) res = CreateVariableByName(iUnifiedVariableArray.ID, name);
        if (vartype == typeof(iUnifiedVariableString)) res = CreateVariableByName(iUnifiedVariableString.ID, name);
        if (vartype == typeof(iUnifiedVariableInt)) res = CreateVariableByName(iUnifiedVariableInt.ID, name);
        if (vartype == typeof(iUnifiedVariableVector)) res = CreateVariableByName(iUnifiedVariableVector.ID, name);
        if (vartype == typeof(iUnifiedVariableFloat)) res = CreateVariableByName(iUnifiedVariableFloat.ID, name);
        if (vartype == typeof(iUnifiedVariableBlock)) res = CreateVariableByName(iUnifiedVariableBlock.ID, name);

        if (res == null) throw new System.Exception("Could not create variable " + vartype);
        if (res == null) return default;
        return (T)res;
    }

    public float getFloat(string name)
    {
        iUnifiedVariableFloat myFloat = openFloat(name);
        return myFloat == null ? 0 : myFloat.GetValue();
    }
    public T GetVariableTpl<T>(string name, DWORD crc = 0xFFFFFFFF) where T : iUnifiedVariable;
    public T GetVariableTpl<T>(DWORD Handle) where T : iUnifiedVariable;
    /// <summary>
    /// Обёртка метода для использования int вместо DWORD
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Handle"></param>
    /// <returns></returns>
    public T GetVariableTpl<T>(int Handle) where T : iUnifiedVariable { return  GetVariableTpl<T>((DWORD)Handle); } 

    iUnifiedVariableVector openVector(string name)
    {
        return GetVariableTpl<iUnifiedVariableVector>(name);
    }
    public bool getVector(string name, out Vector3 data, Vector3 def = default)
    {
        iUnifiedVariableVector n = openVector(name);
        if (n != null)
            data = n.GetValue();
        else
            data = def;
        return n != null;
    }

    //public T GetVariableTpl<T>(string name, uint crc = 0xFFFFFFFF)
    //{
    //    iUnifiedVariable t = GetVariableByName(name, crc);
    //    //Debug.Log(name + " tpl " + t);
    //    if (t == null) return default;
    //    return (T)t;
    //}

    //public T GetVariableTpl<T>(DWORD Handle)
    //{
    //    iUnifiedVariable t = GetVariableByHandle(Handle);
    //    if (t == null) return default;
    //    return (T)t;
    //}

    //public virtual iUnifiedVariable CreateByClassID(int ClassId, iUniVarParent pParent, DWORD MemId)
    //{
    //    switch ((uint)ClassId)
    //    {
    //        case iUnifiedVariableInt.ID: return new UniVarInt(pParent, MemId);
    //        case iUnifiedVariableFloat.ID: return new UniVarFloat(pParent, MemId);
    //        case iUnifiedVariableVector.ID: return new UniVarVector(pParent, MemId);
    //        case iUnifiedVariableString.ID: return new UniVarString(pParent, MemId);
    //        case iUnifiedVariableBlock.ID: return new UniVarBlock(pParent, MemId);
    //        case iUnifiedVariableReference.ID: return new UniVarReference(pParent, MemId);
    //        case iUnifiedVariableContainer.ID: return new UniVarContainer(pParent, MemId);
    //        case iUnifiedVariableArray.ID: return new UniVarArray(pParent, MemId);
    //        default: return null;
    //    }
    //}

    public Stream openStream(string name);
    public iUnifiedVariableContainer openContainer(string name)
    {
        //Debug.Log("Opening Container " + name);
        return GetVariableTpl<iUnifiedVariableContainer>(name);
    }

    public iUnifiedVariableContainer openContainer(DWORD handle)
    {
        return GetVariableTpl<iUnifiedVariableContainer>(handle);
    }

    iUnifiedVariableContainer createContainer(string v);
    public bool getString(string name, ref string data, string def = null)
    {
        //TODO - Возможно, то правильнее изменить ref на out
        iUnifiedVariableString n = openString(name);
        Debug.Log("Loading " + n + " for " + name);
        if (n != null)
            data = n.GetValue();
        else if (def != null)
            data = def;
        return n != null;
    }

    iUnifiedVariableBlock createBlock(string name)
    {
        return CreateVariableTpl<iUnifiedVariableBlock>(name);
    }

    public string getString(string name)
    {
        iUnifiedVariableString n = openString(name);
        if (n != null)
        {
            return n.GetValue();
        }
        else
            return null;
    }

    public string debugString(string name)
    {
        Debug.Log("Opening name " + name);
        return "STUB_" + name;
    }

    public string die()
    {
        throw new Exception("Me should not be called!");
    }

    iUnifiedVariableString openString(string name)
    {
        Debug.Log("Opening name " + name);
        return GetVariableTpl<iUnifiedVariableString>(name);
    }

    iUnifiedVariableString openString(DWORD index)
    {
        Debug.Log("Opening index " + index.ToString("X8"));
        return GetVariableTpl<iUnifiedVariableString>(index);
    }

    iUnifiedVariableBlock openBlock(string name)
    {
        Debug.Log("Loading block: " + name);
        return GetVariableTpl<iUnifiedVariableBlock>(name);
    }

    bool setRef(string name, string data)
    {
        iUnifiedVariableReference n = createReference(name);
        if (n != null) n.SetValue(data);
        return n != null;
    }
    iUnifiedVariableReference createReference(string name)
    {
        return CreateVariableTpl<iUnifiedVariableReference>(name);
    }

    //bool setBlock(string v, Tab<uint> myEnabledWeapons);
    public bool setBlock<T>(string name, Tab<T> vct)
    {
        iUnifiedVariableBlock n = createBlock(name);
        //      if (n) {
        //          if (vct.Count())
        //              n->SetValue(vct.Begin(), vct.Count()*sizeof(T));
        //          else
        //              n->SetValue(0,0);
        //}
        //return n;
        return default;
    }

    public bool GetBlockDWORD(string name,out Tab<DWORD> vct)
    {
        vct = new Tab<DWORD>();
        iUnifiedVariableBlock n = openBlock(name);
        if (n == null) return false;

        int size = sizeof(DWORD);
        int len = n.GetLength() / size; ;
        byte[] buffer = new byte[size];
        n.GetValue(out byte[]val,n.GetLength());
        for (int i=0;i < len;i++)
        {
            Array.Copy(val, i * size, buffer, 0,size);
            vct.SetCount(len);
            //vct.Add(BitConverter.ToUInt32(buffer));
        }
        return true;
    }
    public bool getBlock<T>(string name, out Tab<T> vct, int size)
    {
        vct = new();
        //iUnifiedVariableBlock n = openBlock(name);
        //if (n != null)
        //{
        //    int len = n->GetLength() / sizeof(T);
        //    vct.SetCount(len);
        //    n->GetValue(vct.Begin(), len * sizeof(T));
        //}
        //return n;
        return default;//TODO корректно реализовать получение блока байтов из univar бд.
    }

    iUnifiedVariableArray openArray(string name)
    {
        return GetVariableTpl<iUnifiedVariableArray>(name);
    }
    iUnifiedVariableArray openArray(DWORD handle)
    {
        return GetVariableTpl<iUnifiedVariableArray>(handle);
    }

}


