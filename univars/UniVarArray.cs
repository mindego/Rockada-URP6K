using geombase;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// UniVarArray - реализация iUnifiedVariableArray
/// </summary>
public class UniVarArray : iUniVarParent, iUnifiedVariableArray
{
    public void Dump()
    {
        Debug.Log("mArraySize " + mArraySize);
        //foreach (var v in mpArray)
        //{
        //    Debug.Log(v);
        //}
        for (int i = 0; i < mpArray.Length; i++)
        {
            Debug.Log("Element " + i + " : " + mpArray[i]);
        }
    }
    private void Recopy(DWORD NewSize)
    {
        if (NewSize != mArraySize)
        {
            UniVarArrayItem[] pNewArray = new UniVarArrayItem[0];
            // если новый размер не 0
            if (NewSize > 0)
            {
                // выделяем новый блок
                pNewArray = new UniVarArrayItem[NewSize];
                // копируем туда первую часть
                int l = (NewSize < mArraySize ? (int)NewSize : (int)mArraySize);
                //if (l > 0) MemCpy(pNewArray, mpArray, sizeof(UniVarArrayItem) * l);
                if (l > 0) Array.Copy(mpArray, pNewArray, l);
                // обнуляем вторую часть
                for (; l < NewSize; l++)
                {
                    pNewArray[l] = new UniVarArrayItem();
                    pNewArray[l].Set(null);

                }
                ;
            }

            //for (DWORD i = NewSize; i < mArraySize; i++)
            //{
            //    SafeRelease(mpArray[i].mpVar);
            //}

            // удаляем старые данные
            //if (mpArray != 0) delete[] mpArray;
            // переназначаем данные
            mArraySize = NewSize;
            mpArray = pNewArray;

        }
    }

    public iUnifiedVariable GetVariableByName(string name, uint crc)
    {
        //Debug.Log("GetVariableByName [" + name + "]");
        iUnifiedVariable rt = GetVariableByHandle(GetHandleByName(name));

        if (name[0] != '\\') return rt;


        //        Debug.Log(rt.GetType());

        return rt;
    }

    public iUnifiedVariableString openString(DWORD index)
    {
        return GetVariableTpl<iUnifiedVariableString>(index);
    }

    public iUniVarMemManager GetMemManager()
    {
        return mpMemMgr;
    }

    public bool IsReadOnly()
    {
        return mpMemMgr.IsReadOnly();
    }

    public void OnRelease(iUnifiedVariable var, uint MemId)
    {
        throw new NotImplementedException("И не надо!");
    }

    public void OnDelete(iUnifiedVariable var)
    {
        throw new NotImplementedException("И не надо!");
    }

    public int GetNameLength(iUnifiedVariable pVar)
    {
        // ищем запись для этой переменной
        DWORD i;
        for (i = 0; i < mArraySize; i++)
        {
            if (mpArray[i].mpVar == pVar)
            {
                // возвращаем длину своего имени + '\' + длину имени этой переменной
                return (GetNameLength() + 1 + GetNameLengthByHandle(i + 1));
            }
        }
        // не наша переменная - возвращаем 0
        return 0;
    }

    public string GetName(ref string pStr, iUnifiedVariable pVar)
    {
        //Debug.Log("array mpParent" + mpParent);
        //string discard = String.Empty;
        //Debug.Log(mpParent.GetName(ref pStr, this));
        //Debug.Log("pStr:" + pStr);
        //Debug.Log(GetNameByHandle(ref pStr, 1));
        //throw new Exception("Arrr");
        // ищем запись для этой переменной
        DWORD i;
        for (i = 0; i < mArraySize; i++)
        {
            if (mpArray[i].mpVar == pVar)
            {
                // копируем свое имя
                mpParent.GetName(ref pStr, this);
                //char* c;
                //for (c = pStr; *c != 0; c++) ;
                //*c++ = '\\';
                pStr += '\\';
                return GetNameByHandle(ref pStr, i + 1);
            }
        }
        // не наша переменная
        pStr = null;
        return null;

    }

    public uint SetSize(uint NewSize)
    {
        if (mpMemMgr.IsReadOnly()) return mArraySize;
        if (NewSize < mArraySize)
        {
            for (DWORD i = mArraySize; i > NewSize; i--)
                if (mpArray[i - 1].mClassId != UndefinedID)
                    NewSize = i;
        }
        Recopy(NewSize);
        // возвращаем новый размер
        return mArraySize;
    }

    public iUnifiedVariable CreateVariable(uint ClassID, uint index)
    {
        if (mpMemMgr.IsReadOnly()) return null;
        // проверяем границы индекса
        if (index >= mArraySize) return null;
        // проверяем существование описания
        if (mpArray[index].mClassId != UndefinedID)
        {
            return (mpArray[index].mClassId == ClassID ? GetVariableByHandle(index + 1) : null);
        }
        // создаен новую переменную
        iUnifiedVariable var = univars_dll.CreateByClassID(ClassID, this, 0);
        if (var == null) return null;
        // заполняем описание
        mpArray[index].mClassId = ClassID;
        mpArray[index].mpVar = var;
        AddRef();
        return var;
    }

    public bool SwapVariables(uint index1, uint index2)
    {
        throw new NotImplementedException();
    }

    public uint Shrink(uint NewSize)
    {
        throw new NotImplementedException();
    }

    public void Query(int ClassID)
    {
        throw new NotImplementedException();
    }

    public uint GetSize()
    {
        return mArraySize;
    }

    public uint GetNextHandle(uint Handle)
    {
        while (true)
        {
            if (Handle >= mArraySize) return 0;
            if (mpArray[Handle].mClassId != UndefinedID) return Handle + 1;
            Handle++;
        }
    }

    public int GetNameLengthByHandle(uint Handle)
    {
        return 8; //Ну вот в исходниках так.
    }

    public T CreateVariableTpl<T>(DWORD index) {
        Type vartype = typeof(T);
        iUnifiedVariable res = null;
        if (vartype == typeof(iUnifiedVariableContainer)) res = CreateVariable(iUnifiedVariableContainer.ID,index);
        if (vartype == typeof(iUnifiedVariableArray)) res = CreateVariable(iUnifiedVariableArray.ID, index);
        if (vartype == typeof(iUnifiedVariableString)) { res = CreateVariable(iUnifiedVariableString.ID, index); }
        if (vartype == typeof(iUnifiedVariableInt)) res = CreateVariable(iUnifiedVariableInt.ID, index);
        if (vartype == typeof(iUnifiedVariableVector)) res = CreateVariable(iUnifiedVariableVector.ID, index);
        if (vartype == typeof(iUnifiedVariableFloat)) res = CreateVariable(iUnifiedVariableFloat.ID, index);
        if (vartype == typeof(iUnifiedVariableBlock)) res = CreateVariable(iUnifiedVariableBlock.ID, index);

        if (res == null) throw new System.Exception("Could not create variable " + vartype);
        if (res == null) return default;
        return (T)res;
}

public string GetNameByHandle(ref string buffer, uint Handle)
    {
        //c++
        //char* __cdecl _addint(char* s, int a, int count = 0, int pow = 10, int c = 32, int inv = 0)

        //_addint(buffer, Handle - 1, 8, 10, '0', false);
        //return buffer;

        //MAD_TOOLS_API char* __cdecl _addint(char *s,int a,int count,int pow,int c,int inv) {
        //return a < 0 ? _addintc(s, -1, -a, count, pow, c, inv) : _addintc(s, 1, a, count, pow, c, inv);
        //}
        //-----------

        //// проверяем границы индекса
        //int index = (int)Handle - 1;
        //if (index < 0 || index >= mArraySize) return null;
        //// если переменной не создано
        //if (mpArray[index].mpVar == null) return null;
        //mpArray[index].mpVar.GetName(ref buffer);
        //return buffer;
        string res;
        res = (Handle - 1).ToString();
        buffer += res;
        return res;
    }

    public object Query(uint ClassID)
    {
        switch (ClassID)
        {
            case iUnifiedVariable.ID: AddRef(); return (iUnifiedVariable)this;
            case iUnifiedVariableContainer.ID: AddRef(); return (iUnifiedVariableContainer)this;
            case iUnifiedVariableArray.ID: AddRef(); return (iUnifiedVariableArray)this;
            default: return null;
        }
    }


    public uint GetHandleByName(string Name)
    {
        //Debug.Log("COnverting 2 handle " + Name + " = " + (uint.Parse(Name) + 1));
        //throw new NotImplementedException();
        return uint.Parse(Name) + 1;
    }

    public int GetSizeByHandle(uint Handle)
    {
        throw new NotImplementedException();
    }

    public iUnifiedVariable GetVariableByHandle(uint Handle)
    {
        // проверяем границы индекса
        int index = (int)Handle - 1;
        if (index < 0 || index >= mArraySize) return null;
        // если переменной не создано
        if (mpArray[index].mpVar == null)
        {
            mpArray[index].mpVar = univars_dll.CreateByClassID(mpArray[index].mClassId, this, mpArray[index].mMemId);
            if (mpArray[index].mpVar != null) AddRef();
        }
        else
        {
            mpArray[index].mpVar.AddRef();
        }
        return mpArray[index].mpVar;

    }

    public iUnifiedVariable CreateVariableByName(uint ClassID, string name)
    {
        DWORD index = uint.Parse(name);
        if (index >= mArraySize) SetSize(index + 1);
        return CreateVariable(ClassID, index);
    }

    public bool Rename(string pSrcName, string pDstName)
    {
        throw new NotImplementedException();
    }

    public uint GetClassId()
    {
        return iUnifiedVariableArray.ID;
    }

    public bool Delete()
    {
        return false;
    }

    public bool ExportToFile(string filename)
    {
        Debug.Log(filename + " size " + mArraySize + " " + mMemID);

        return true;
    }

    public bool ImportFromFile(string filename)
    {
        throw new NotImplementedException();
    }

    public int GetNameLength()
    {
        return mpParent.GetNameLength(this);
    }

    public string GetName(ref string pStr)
    {
        //Debug.Log("pStr" + pStr);
        //throw new Exception("Me dead!");
        return mpParent.GetName(ref pStr, this);
    }

    public void AddRef()
    {
        mCounter++;
    }

    public int Release()
    {
        throw new NotImplementedException();
    }

    private int mCounter;
    private iUniVarParent mpParent;
    private iUniVarMemManager mpMemMgr;
    private DWORD mMemID;
    private DWORD mArraySize;
    private UniVarArrayItem[] mpArray;

    //c#
    private const uint UndefinedID = 0xFFFFFFFF;
    public UniVarArray(iUniVarParent par, DWORD memid) : this(1, par, par.GetMemManager(), memid, 0, new UniVarArrayItem[0])
    {
        if (memid == 0) return;

        byte[] data = mpMemMgr.GetDataByID(memid);
        //Stream fs = File.Open(memid + ".memid",FileMode.Create);
        //fs.Write(data);
        //fs.Close();
        mArraySize = BitConverter.ToUInt32(data);
        if (mArraySize <= 0) return;
        mpArray = new UniVarArrayItem[mArraySize];
        for (int i = 0; i < mArraySize; i++)
        {
            UniVarArrayItemSave tmp = StormFileUtils.ReadStruct<UniVarArrayItemSave>(data, 4 + i * 2 * 4); //4 - 2*4 - размер структуры UniVarArrayItemSave
            mpArray[i] = new UniVarArrayItem();
            mpArray[i].Set(tmp);
            //Debug.Log($"item {i} " + tmp + " " + mpArray[i].mpName);
        }
    }

    public UniVarArray(int mCounter, iUniVarParent mpParent, iUniVarMemManager mpMemMgr, uint mMemID, uint mArraySize, UniVarArrayItem[] mpArray)
    {
        this.mCounter = mCounter;
        this.mpParent = mpParent;
        this.mpMemMgr = mpMemMgr;
        this.mMemID = mMemID;
        this.mArraySize = mArraySize;
        this.mpArray = mpArray;
    }

    ~UniVarArray()
    {
        this.mpArray = null;
    }

    public T GetVariableTpl<T>(string name, uint crc = 0xFFFFFFFF) where T : iUnifiedVariable
    {
        //Debug.Log(name);
        iUnifiedVariable t = GetVariableByName(name, crc);
        //if (t == null) return null;
        if (t == null) return default;
        return (T)t;
    }

    public T GetVariableTpl<T>(DWORD Handle) where T : iUnifiedVariable
    {
        iUnifiedVariable t = GetVariableByHandle(Handle + 1);
        //if (t == null) return null;
        if (t == null) return default;
        return (T)t;
    }

    public override string ToString()
    {
        string res = string.Format("{0} {1} {2}", GetType().ToString(), mArraySize, mMemID);
        for (int i = 0; i < mArraySize; i++)
        {
            res += string.Format("\n{0} {1}", i, mpArray[i].ToString());
        }
        return res;
    }

    public iUnifiedVariableContainer createContainer(string v)
    {
        throw new NotImplementedException();
    }

    public Stream openStream(string name)
    {
        throw new NotImplementedException();
    }

    public int RefCount()
    {
        throw new NotImplementedException();
    }

    public string getString(uint idx, string def = null)
    {
        iUnifiedVariableString n = openString(idx);
        if (n != null) return n.GetValue();

        return null;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class UniVarArrayItemSave
{
    public DWORD mClassId;
    public DWORD mMemId;
    public void Set(UniVarArrayItem src)
    {
        mClassId = src.mClassId;
        mMemId = src.mMemId;
    }
};

public struct UniVarArrayItem
{
    public DWORD mClassId;
    public DWORD mMemId;
    public iUnifiedVariable mpVar;

    private const DWORD UndefinedID = 0xFFFFFFFF;
    public void Set(UniVarArrayItemSave src)
    {
        //mClassId = (src != default ? src.mClassId : UndefinedID);
        //mMemId = (src != 0 ? src->mMemId : 0);
        mClassId = src != null ? src.mClassId : UndefinedID;
        mMemId = src != null ? src.mMemId : 0;
        mpVar = null;
    }

    public override string ToString()
    {
        string res = string.Format("\t{0} {1}", univars_dll.GetDescriptionByClassID(mClassId), mMemId);
        if (mpVar != null) res += "\n" + mpVar;
        return res;
    }
}