using System;
using System.IO;
using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// UniVarDB - реализация
/// </summary>
public class UniVarDB : iUnifiedVariableDB, iUniVarParent
{
    private iUniVarMemManager mpMemMgr;
    private UniVarDBItem mpRootData;
    private iUnifiedVariable mpRoot;
    private DWORD mCounter;
    private const string spRootStr = "Root";

    public override string ToString()
    {
        string res = GetType().ToString();
        res += "\nMemMGR:" + mpMemMgr;
        res += "\nRoot:" + mpRoot;
        return res;
    }
    public UniVarDB(string filename, iUniVarMemManager pMemMgr)
    {
        //mpMemMgr = pMemMgr;
        ////mpRootData = (UniVarDBItem*)mpMemMgr->GetPtrByID(1);

        //mpRootData = StormFileUtils.ReadStruct<UniVarDBItem>(mpMemMgr.GetDataByID(1));
        //Debug.Log("mpRootData " + mpRootData);
        //Debug.Log("mpRootData mClassId " + mpRootData.mClassId);
        ////mpRootData.Set(0xFFFFFFFF, 0);
        ///

        mpMemMgr = pMemMgr;
        mpRootData = null;
        mpRoot = null;
        mCounter = 1;

      
        if (mpMemMgr.GetDataByID(1)!=null) mpRootData = StormFileUtils.ReadStruct<UniVarDBItem>(mpMemMgr.GetDataByID(1));
        if (mpRootData == null && mpMemMgr.IsReadOnly() == false)
        {
            if (mpMemMgr.Alloc((uint)UniVarDBItem.GetSize()) != 1)
            {
                Debug.LogError("Failed to allocate root db item");
                return;
            }
            mpRootData = StormFileUtils.ReadStruct<UniVarDBItem>(mpMemMgr.GetDataByID(1));
            if (mpRootData != null) mpRootData.Set(Constants.UndefinedID, 0);
        }
    }

    public void AddRef()
    {
        mCounter++;
    }

    public iUnifiedVariable CreateRoot(uint ClassId)
    {
        if (mpRootData == null || mpMemMgr.IsReadOnly()) return null;
        //Debug.Log("mpRootData mClassId " + mpRootData.mClassId.ToString("X8") + " " + univars_dll.GetDescriptionByClassID(mpRootData.mClassId));
        
        // если уже есть
        if (mpRootData.mClassId != Constants.UndefinedID)
        {
            return (mpRootData.mClassId == ClassId ? GetRoot() : null);
        }
        // пытаемся создать
        mpRoot = univars_dll.CreateByClassID(ClassId, this, 0);
        if (mpRoot == null) return null;
        mpRootData.Set(ClassId, mpRootData.mMemId);
        // увеличиваем свой счетчик
        AddRef();
        return mpRoot;
    }

    public iUnifiedVariable CreateVariableByName(uint ClassID, string name)
    {
        throw new NotImplementedException();
    }

    public bool Delete()
    {
        return false;
    }

    public bool ExportToFile(string filename)
    {
        return false;
    }

    public uint GetClassId()
    {
        return iUnifiedVariableContainer.ID;
    }

    public uint GetHandleByName(string Name)
    {
        throw new NotImplementedException();
    }

    public iUniVarMemManager GetMemManager()
    {
        return mpMemMgr;
    }

    public string GetName(ref string value)
    {
        value = "\\.";
        return value;
    }

    public string GetName(ref string pStr, iUnifiedVariable pVar)
    {
        if (pVar != mpRoot) { pStr = null; return null; }
        pStr = '\\' + spRootStr;
        return pStr;
    }

    public string GetNameByHandle(string buffer, uint Handle)
    {
        throw new NotImplementedException();
    }

    public int GetNameLength()
    {
        return 2;
    }

    public int GetNameLength(iUnifiedVariable var)
    {
        throw new NotImplementedException();
    }

    public int GetNameLengthByHandle(uint Handle)
    {
        return Handle == 1 ? spRootStr.Length : 0;
    }

    public uint GetNextHandle(uint Handle)
    {
        return ((Handle == 0) ? (uint)1 : 0);

    }

    public iUnifiedVariable GetRoot()
    {
        // если уже пользуемся
        if (mpRoot != default) { mpRoot.AddRef(); return mpRoot; }
        // пытаемся создать
        //if (mpRootData == default) return null;
        //mpRoot = ExternUtils.CreateByClassID((int)mpRootData.mClassId, this, mpRootData.mMemId);
        mpRoot = univars_dll.CreateByClassID(mpRootData.mClassId, this, mpRootData.mMemId);
        if (mpRoot == default) return null;
        // увеличиваем свой счетчик
        AddRef();
        return mpRoot;

    }

    public uint GetRootDataSize()
    {
        throw new NotImplementedException();
    }

    public uint GetSize()
    {
        return 1;
    }

    public int GetSizeByHandle(uint Handle)
    {
        throw new NotImplementedException();
    }

    public iUnifiedVariable GetVariableByHandle(uint Handle)
    {
        throw new NotImplementedException();
    }

    public iUnifiedVariable GetVariableByName(string name, uint crc = 0xFFFFFFFF)
    {
        int c = name != null ? name.IndexOf('\\') : 0;

        if (c != -1)
        {
            // если строка начинается с '\'
            if (c == 0) return GetVariableByName(name.Substring(1), crc);
            // если это не 'Root'
            //if (name != spRootStr) return null;
            if (!name.StartsWith(spRootStr)) return null;
        }
        else
        {
            // если это не 'Root'
            if (name == null)
            {
                if (crc != 0x4939A99A) return null;
            }
            else
            {
                if (name != spRootStr) return null;
            }
        }

        // получаем root
        iUnifiedVariable rt = GetRoot();
        if (rt == null) return null;
        // если это имя заканчивается
        if (c == 0) return rt;

        // пытаемся сконвертить переменную в контейнер
        iUnifiedVariableContainer ctr = (iUnifiedVariableContainer)rt.Query(iUnifiedVariableContainer.ID);
        rt.Release();
        // если не удалось - возвращаем 0
        if (ctr == null) return null;
        // если удалось - пусть она и разбирает строчку дальше
        rt = ctr.GetVariableByName(name.Substring(c + 1));
        ctr.Release();
        return rt;
    }
    public iUnifiedVariable GetVariableByNameOld(string name, uint crc = 0xFFFFFFFF)
    {
        //if (name != String.Empty) crc = Hasher.HshString(name);
        iUnifiedVariable rt;
        iUnifiedVariableContainer cnt;


        int separatorPosition = name.IndexOf('\\');
        if (separatorPosition == 0) return GetVariableByName(name.Substring(separatorPosition + 1));
        if (separatorPosition > 0)
        {
            string c = name.Substring(0, separatorPosition);
            if (c != "Root") return null;

            name = name.Substring(separatorPosition + 1);

        }
        rt = GetRoot();
        if (rt == null) return null;
        cnt = (iUnifiedVariableContainer)rt.Query(iUnifiedVariableContainer.ID);
        if (cnt == null) return null;
        return cnt.GetVariableTpl<iUnifiedVariableContainer>(name);


        //LOG mpLog = new LOG();
        ////int c = (name != String.Empty ? name.IndexOf('\\') : 0);
        ////string[] names=name.Split('\\');
        //mpLog.LogDebug(name);
        //int separatorPosition = name.IndexOf('\\');
        //string c = name;
        //if (separatorPosition > 0) c = name.Substring(separatorPosition);
        ////string c = (name == String.Empty) ? name : name.Substring(separatorPosition);
        //mpLog.LogDebug(separatorPosition.ToString());
        //mpLog.LogDebug(c);

        //if (separatorPosition >= 0)
        //{
        //    mpLog.LogDebug(" если строка начинается с '\\'");
        //    // если строка начинается с '\'
        //    if (c == name) return GetVariableByName(name.Substring(separatorPosition + 1), crc);
        //    // если это не 'Root'
        //    mpLog.LogDebug("// если это не 'Root'");
        //    if (!name.StartsWith(spRootStr)) return null;
        //}
        //else
        //{
        //    // если это не 'Root'
        //    mpLog.LogDebug("если это не 'Root' (" + name + ")");
        //    if (name == string.Empty)
        //    {
        //        if (crc != 0x4939A99A) return null;
        //    }
        //    else
        //    {
        //        if (name == spRootStr) return null;
        //    }
        //}

        //mpLog.LogDebug("// получаем root");
        //// получаем root
        //iUnifiedVariable rt = GetRoot();
        //if (rt == null) return null;
        //mpLog.LogDebug(String.Format("если это имя заканчивается {0}", c == String.Empty ? "true" : "false"));
        //// если это имя заканчивается
        //if (c == string.Empty) return rt;
        //mpLog.LogDebug("// пытаемся сконвертить переменную в контейнер");
        //// пытаемся сконвертить переменную в контейнер
        ////iUnifiedVariableContainer ctr = rt.Query<iUnifiedVariableContainer>();
        ////iUnifiedVariableContainer ctr = (iUnifiedVariableContainer) rt;
        //iUnifiedVariableContainer ctr = (iUnifiedVariableContainer)rt.Query(iUnifiedVariableContainer.ID);
        //rt.Release();
        //mpLog.LogDebug("// если не удалось - возвращаем 0");
        //// если не удалось - возвращаем 0
        //if (ctr == null) return null;
        //// если удалось - пусть она и разбирает строчку дальше
        //mpLog.LogDebug("// если удалось - пусть она и разбирает строчку дальше");
        ////rt = ctr.GetVariableByName(c + 1);
        //rt = ctr.GetVariableByName(c);
        //ctr.Release();
        //mpLog.LogDebug(ctr.GetType().ToString());
        //mpLog.LogDebug("Return rt");

        //return rt;

    }

    public bool ImportFromFile(string filename)
    {
        return false;
    }

    public bool IsReadOnly()
    {
        return mpMemMgr.IsReadOnly();
    }

    public void OnDelete(iUnifiedVariable var)
    {
        throw new NotImplementedException();
    }

    public void OnRelease(iUnifiedVariable var, uint MemId)
    {
        throw new NotImplementedException();
    }

    public object Query(uint ClassID)
    {
        //switch ((uint) ClassID)
        //{
        //    case iUnifiedVariable.ID: AddRef(); return (iUnifiedVariable)this;
        //    case iUnifiedVariableContainer.ID: AddRef(); return (iUnifiedVariableContainer)this;
        //    case iUnifiedVariableDB.ID: AddRef(); return (iUnifiedVariableDB) this;
        //    default: return null;
        //}
        return null;

    }

    public int Release()
    {
        if (--mCounter > 0) return (int)mCounter;
        return 0;

    }

    public bool Rename(string pSrcName, string pDstName)
    {
        throw new NotImplementedException();
    }

    public bool SaveToFile(string filename)
    {
        return mpMemMgr.SaveToFile(filename);
    }
    //public ~UniVarDB();

    //c# addon
    public T GetRootTpl<T>() where T : iUnifiedVariable
    {
        iUnifiedVariable t = GetRoot();
        if (t == null) return default;
        return (T)t;
    }

    public T GetVariableTpl<T>(string name, uint crc = 0xFFFFFF) where T : iUnifiedVariable
    {
        iUnifiedVariable t = GetVariableByName(name, crc);
        //   if (t == null) return null;
        Debug.LogFormat("Loading VariableTpl {0} {1} {2}",typeof(T).ToString(), name ,t==null? "Failed":"Success");

        return (T)t;
    }

    public T GetVariableTpl<T>(uint Handle) where T : iUnifiedVariable
    {
        throw new NotImplementedException();
    }

    //public iUnifiedVariable CreateByClassID(uint ClassId, iUniVarParent pParent, uint MemId)
    //{
    //    throw new NotImplementedException();
    //}

    public string GetNameByHandle(ref string buffer, uint Handle)
    {
        throw new NotImplementedException();
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
}

