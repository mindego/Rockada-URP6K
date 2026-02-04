using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using DWORD = System.UInt32;

/// <summary>
/// Служебная структура для UniVarContainer
/// </summary>
public class UniVarContainerItem
{
    public DWORD mClassId;
    public DWORD mMemId;
    public DWORD mCode;
    public iUnifiedVariable mpVar;
    public string mpName;

    public void Set(UniVarContainerItemSave pSrc, string pBase)
    {
        mClassId = pSrc.mClassId;
        mMemId = pSrc.mMemId;
        mpVar = null;
        //int l = StrLen(pBase + pSrc->mOffset) + 1;
        //mpName = new char[l];
        //MemCpy(mpName, pBase + pSrc->mOffset, l);
        //mpName = String.Empty; //STUB!
        mpName = pBase; //STUB!
        //Storm.CRC32 crc = new Storm.CRC32();
        //mCode = crc.HashString(mpName);
        mCode = Hasher.HshString(mpName);
    }
    public void Set(DWORD ClassId, DWORD Code, iUnifiedVariable pVar, string pName, DWORD MemId = 0)
    {
        mClassId = ClassId;
        mMemId = MemId;
        mCode = Code;
        mpVar = pVar;
        //int l = StrLen(pName) + 1;
        //mpName = new char[l];
        //MemCpy(mpName, pName, l);
        mpName = pName;

    }

    public override string ToString()
    {
        //return $"[{mpName}] + mMemId";
        //return string.Format("{0} {1} {2}", mpName, mMemId, univars_dll.GetDescriptionByClassID(mClassId));
        if (mpVar!=null) return mpName + " : " + mpVar.ToString();
        return string.Format("Unset {0} {1} {2}", mpName, mMemId, univars_dll.GetDescriptionByClassID(mClassId));
    }

    //public void Free() { if (mpName != 0) delete[] mpName; mpName = 0; }
}

/// <summary>
/// Служебная структура хранения данных в файле ресурса
/// </summary>
public struct UniVarContainerItemSave
{
    public DWORD mClassId;
    public DWORD mMemId;
    public DWORD mOffset;
    public void Set(UniVarContainerItem pSrc, DWORD Off)
    {
        mClassId = pSrc.mClassId;
        mMemId = pSrc.mMemId;
        mOffset = Off;
    }

    public override string ToString()
    {
        return $"ClassID {mClassId:X8} MemId {mMemId} Offset {mOffset}";
    }
}/// <summary>
/// UniVarContainer - реализация iUnifiedVariableContainer
/// </summary>
public class UniVarContainer : iUniVarParent, iUnifiedVariableContainer
{
    private int mCounter;
    private iUniVarParent mpParent;
    private iUniVarMemManager mpMemMgr;
    private DWORD mMemID;
    private int mArraySize;
    private UniVarContainerItem[] mpArray;

    private bool mIsDeleting;
    private void ReAlloc(int srcl1, int dst2, int src2)
    {
        //int NewL = dst2 + mArraySize - src2;
        //UniVarContainerItem[] pNewContainer = null;
        //// если новый размер не 0
        //if (NewL > 0)
        //{
        //    // выделяем новый блок
        //    pNewContainer = new UniVarContainerItem[NewL];
        //    // копируем туда первую часть
        //    if (srcl1 > 0) MemCpy(pNewContainer, mpArray, sizeof(UniVarContainerItem) * srcl1);
        //    // копируем туда вторую часть
        //    srcl1 = mArraySize - src2;
        //    if (srcl1 > 0) MemCpy(pNewContainer + dst2, mpArray + src2, sizeof(UniVarContainerItem) * srcl1);
        //}
        //// удаляем старые данные
        //if (mpArray != 0) delete[] mpArray;
        //// переназначаем данные
        //mArraySize = NewL;
        //mpArray = pNewContainer;
    }

    public iUnifiedVariableInt openInt(string name)
    {
        return GetVariableTpl<iUnifiedVariableInt>(name);
    }

    //internal float getFloat(string v)
    //{
    //    throw new NotImplementedException();
    //}

    public iUnifiedVariableFloat openFloat(string name )
    {
        return GetVariableTpl<iUnifiedVariableFloat>(name);
    }

    public iUnifiedVariableInt openInt(DWORD handle)
    {
        return GetVariableTpl<iUnifiedVariableInt>(handle);
    }

    //public iUnifiedVariableContainer openContainer(string name)
    //{
    //    return GetVariableTpl<iUnifiedVariableContainer>(name);
    //}

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(GetType().ToString() + " mMemId: " + mMemID + " mArraySize: " + mArraySize);
        //sb.AppendLine("Parent:\n" + mpParent);
        for (int i = 0; i < mArraySize; i++) // 0 индекс - служебный
        {
            UniVarContainerItem item = mpArray[i];
            sb.AppendFormat("\n\t{0}",item);
        }
        sb.AppendLine("\n<=============> end [" + GetType().ToString() + " " + mMemID + "]");
        return sb.ToString();

    }

    private DWORD GetHandle(DWORD Code)
    {
        //if (mpArray[0].mCode == Code) return 1;
        //if (mpArray[mArraySize - 1].mCode == Code) return (DWORD) mArraySize;

        //int _1 = 0, _2 = mArraySize - 1;
        //while ((_2 - _1) > 1)
        //{
        //    int _3 = (_1 + _2) >> 1;
        //    if (mpArray[_3].mCode == Code) return (DWORD) _3 + 1;
        //    if (mpArray[_3].mCode < Code) _1 = _3; else _2 = _3;
        //}
        //return 0;
        for (int i = 0; i < mpArray.Length; i++)
        //foreach(UniVarContainerItem var in mpArray)
        {
            UniVarContainerItem var = mpArray[i];
            //Debug.Log("Testing [" + var.mpName + "] Code " + var.mCode);
            if (var.mCode == Code)
            {
                //Debug.Log("Match!");
                return (uint)i + 1;
            }
        }
        return 0;
    }

    public UniVarContainer(iUniVarParent par, DWORD memid) : this(1, par, par.GetMemManager(), memid, 0, new UniVarContainerItem[0], false)
    {
        if (memid == 0) return;

        byte[] data = mpMemMgr.GetDataByID(memid);
        mArraySize = BitConverter.ToInt32(data);
        if (mArraySize <= 0) return;
        mpArray = new UniVarContainerItem[mArraySize];
        for (int i = 0; i < mArraySize; i++)
        {
            UniVarContainerItemSave tmp = StormFileUtils.ReadStruct<UniVarContainerItemSave>(data, 4 + i * 3 * 4); //4 - заголовок,  3*4 - размер структуры
            mpArray[i] = new UniVarContainerItem();
            mpArray[i].Set(tmp, ReadContainterItemName(data, (int)tmp.mOffset));
            //if (mpArray[i].mpName == "ViewBackward") Debug.Log($"item {i} " + tmp + " " + mpArray[i].mpName);
        }
    }

    public iUnifiedVariableString openString(string name)
    {
        return GetVariableTpl<iUnifiedVariableString>(name);
    }

    public bool getString(string name, ref string data, string def = null)
    {
        //TODO - Возможно, то правильнее изменить ref на out
        iUnifiedVariableString n = openString(name);
        if (n != null)
            data = n.GetValue();
        else if (def != null)
            data = def;
        return n != null;
    }

    private string ReadContainterItemName(byte[] data, int offset)
    {
        byte[] resData = new byte[255];
        if (offset >= data.Length) return string.Empty;
        int cnt = 0;
        for (int i = offset; i < data.Length; i++)
        {
            if (data[i] == '\0') break;
            resData[cnt++] = data[i];
        }
        Encoding enc = Encoding.GetEncoding("windows-1251");
        //Debug.Log("Name data array: " + BitConverter.ToString(resData));
        return enc.GetString(resData).Trim('\0');
    }

    ~UniVarContainer()
    {
        if (mArraySize <= 0) return;
        mpArray = null;
        mArraySize = 0;
    }

    public UniVarContainer(int mCounter, iUniVarParent mpParent, iUniVarMemManager mpMemMgr, uint mMemID, int mArraySize, UniVarContainerItem[] mpArray, bool mIsDeleting)
    {
        this.mCounter = mCounter;
        this.mpParent = mpParent;
        this.mpMemMgr = mpMemMgr;
        this.mMemID = mMemID;
        this.mArraySize = mArraySize;
        this.mpArray = mpArray;
        this.mIsDeleting = mIsDeleting;
    }

    //~UniVarContainer();

    public void AddRef()
    {
        mCounter++;
    }

    public iUnifiedVariable CreateVariableByName(uint ClassID, string name)
    {
        if (mpMemMgr.IsReadOnly()) return null;
        uint Code = Hasher.HshString(name);
        // проверяем существование такой
        DWORD index;
        for (index = 0; index < mArraySize; index++)
        {
            if (mpArray[index].mCode < Code) continue; //TODO Проверить! имхо правильнее !=
            if (mpArray[index].mCode == Code)
            {
                return (mpArray[index].mClassId == ClassID ? GetVariableByHandle(index + 1) : null);
            }
            break;
        }
        // создаен новую переменную
        iUnifiedVariable var = univars_dll.CreateByClassID(ClassID, this, 0);
        if (var == null) return null;
        // перевыделяем массив и записываем туда новую переменную
        //ReAlloc(index, index + 1, index);
        //mpArray[index].Set(ClassID, Code, var, name);
        UniVarContainerItem[] tmpArray = new UniVarContainerItem[mArraySize + 1];
        Array.Copy(mpArray, tmpArray, mArraySize);

        UniVarContainerItem tmpItem = new UniVarContainerItem();
        tmpItem.mClassId = ClassID;
        tmpItem.mCode = Code;
        tmpItem.mpName = name;
        tmpItem.mpVar = var;
        tmpItem.mMemId = (uint) mArraySize;

        tmpArray[mArraySize] = tmpItem;
        mpArray = tmpArray;
        mArraySize = mpArray.Length;
        AddRef();
        return var;
    }

    public bool Delete()
    {
        throw new NotImplementedException("И не надо!");
    }

    public bool ExportToFile(string filename)
    {
        //Stream st = File.Open(filename, FileMode.CreateNew);
        //if (!st.CanWrite) return false;
        //StreamWriter sw = new StreamWriter(st);

        //sw.WriteLine($"mArraySize {mArraySize}");

        //sw.Close();
        //st.Close();
        //return true;

        Debug.Log($"mArraySize {mArraySize}");
        foreach (UniVarContainerItem item in mpArray)
        {
            Debug.Log(item.mpName);
        }
        return true;
    }

    public uint GetClassId()
    {
        return iUnifiedVariableContainer.ID;
    }

    public uint GetHandleByName(string Name)
    {
        //Storm.CRC32 crc = new Storm.CRC32();
        //return crc.HashString(Name);
        return GetHandle(Hasher.HshString(Name));
    }

    public iUniVarMemManager GetMemManager()
    {
        return mpMemMgr;
    }

    //public string GetName(string pStr, iUnifiedVariable pVar)
    //{
    //    // ищем запись для этой переменной
    //    DWORD i;
    //    for (i = 0; i < mArraySize; i++)
    //    {
    //        if (mpArray[i].mpVar == pVar)
    //        {
    //            // копируем свое имя
    //            pStr = mpParent.GetName(pStr, this);
    //            //char* c;
    //            //for (c = pStr; *c != 0; c++) ;
    //            //*c++ = '\\';
    //            //return GetNameByHandle(c, i + 1);
    //            return pStr;
    //        }
    //    }
    //    // не наша переменная
    //    //pStr[0] = 0;
    //    return string.Empty;
    //}
    public string GetName(ref string pStr, iUnifiedVariable pVar)
    {
        DWORD i;
        for (i = 0; i < mArraySize; i++)
        {
            if (mpArray[i].mpVar == pVar)
            {
                mpParent.GetName(ref pStr, this);
                pStr += '\\';
                //string c = "";

                //return GetNameByHandle(ref c, i + 1);
                return GetNameByHandle(ref pStr, i + 1);
            }
        }
        pStr = null;

        return null;

        // c++
        //          if (mpArray[i].mpVar==pVar) {
        //  // копируем свое имя
        //  mpParent->GetName(pStr,this);
        //  char* c;
        //  for (c=pStr; *c!=0; c++) ;
        //  *c++='\\';
        //  return GetNameByHandle(c,i+1);
        //}

    }

    public string GetName(ref string pStr)
    {
        //return "Badum-tssss!";
        //throw new NotImplementedException();
        //Debug.Log("Badum-tssss!" + mpParent.GetName(pStr, this));
        //Debug.Log("Current pStr: [" + pStr + "] from " + mpParent);
        return mpParent.GetName(ref pStr, this);
    }

    public string GetNameByHandle(ref string buffer, uint Handle)
    {
        //if (Handle > mArraySize || Handle == 0) return String.Empty;
        if (Handle > mArraySize || Handle == 0) return null;
        //Debug.Log((Handle,mpArray[Handle - 1]));
        //Debug.Log("Returning name: " + mpArray[Handle - 1].mpName);
        //if (buffer == null) buffer = "";
        buffer = "";
        buffer += mpArray[Handle - 1].mpName;
        return mpArray[Handle - 1].mpName;

        //buffer = mpArray[Handle - 1].mpName;
        //return mpArray[Handle - 1].mpName;
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

    public int GetNameLength()
    {
        return mpParent.GetNameLength(this);
    }

    public int GetNameLengthByHandle(uint Handle)
    {
        return (Handle <= mArraySize ? mpArray[Handle - 1].mpName.Length : 0);
    }

    public uint GetNextHandle(uint Handle)
    {
        //Debug.Log((mArraySize,Handle));
        return Handle < mArraySize ? Handle + 1 : 0;
    }

    public uint GetSize()
    {
        return (uint)mArraySize;
    }

    public int GetSizeByHandle(uint Handle)
    {
        return (Handle <= mArraySize ? (int)mpMemMgr.GetSizeByID(mpArray[Handle - 1].mMemId) : 0);
    }

    public iUnifiedVariable GetVariableByHandle(uint Handle)
    {
        // проверяем границы индекса
        if (Handle == 0 || (Handle > mArraySize)) return null;
        //if (Handle > mArraySize) return null;
        // если переменной не создано
        //UniVarContainerItem dst = mpArray + (Handle - 1);
        UniVarContainerItem dst = mpArray[Handle - 1];
        //UniVarContainerItem dst = mpArray[Handle];
        //Debug.Log("Class ID: " + dst.mClassId.ToString("X8") + " memid: " + dst.mMemId + " handle " + Handle);
        if (dst.mpVar == null)
        {
            dst.mpVar = univars_dll.CreateByClassID(dst.mClassId, this, dst.mMemId);
            //Debug.Log("mpVar not found,creating" + dst.mpName) ;
            if (dst.mpVar != null) AddRef();
        }
        else
        {
            dst.mpVar.AddRef();
        }
        return dst.mpVar;

    }

    iUnifiedVariableInt createInt(string name)
    {
        iUnifiedVariable t = CreateVariableByName((int)iUnifiedVariableInt.ID, name);
        if (t == null) return null;
        return (iUnifiedVariableInt)t;
        //return CreateVariableTpl<iUnifiedVariableInt>(name);
    }




    public void setInt(string name, int data)
    {
        iUnifiedVariableInt t = createInt(name);
        t.SetValue(data);
        //tuvi(createInt(name))->SetValue(data);
    }

    public iUnifiedVariable GetVariableByName(string name, uint crc = 0xFFFFFFFF)
    {
        int c = name != null ? name.IndexOf('\\') : 0;
        //Debug.Log("pos " + c);
        if (c != -1)
        {
            //Debug.Log("Long path! " + name +" " + mpParent);
            // если строка начинается с '\'
            if (c == 0) return mpParent.GetVariableByName(name, crc);
            // получаем код
            crc = Hasher.HshString(name.Substring(0, c));
            // если это '..'
            if (crc == 0x69F7E9E3) return mpParent.GetVariableByName(name.Substring(c + 1), crc);
        }
        else
        {
            //Debug.Log("Short path! " + name);
            // получаем код
            if (name != null) crc = Hasher.HshString(name);
            // если это '.'
            if (crc == 0xF12B1DBD) return (iUnifiedVariable)this;
        }
        // получаем переменную
        iUnifiedVariable rt = GetVariableByHandle(GetHandle(crc));
        if (rt == null) return null;
        // если это имя заканчивается
        if (c == -1) return rt;
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
        //iUnifiedVariable rt;
        iUnifiedVariableContainer cnt;

        int separatorPosition = name.IndexOf('\\');
        if (separatorPosition == -1)
        {
            if (name != string.Empty) crc = Hasher.HshString(name);
            return GetVariableByHandle(GetHandle(crc));
        }

        if (separatorPosition > 0)
        {
            string c = name.Substring(0, separatorPosition);

            cnt = (iUnifiedVariableContainer)GetVariableByHandle(GetHandle(Hasher.HshString(c)));
            if (cnt == null) return null;
            return cnt.GetVariableByName(name.Substring(separatorPosition + 1));
            //name = name.Substring(separatorPosition + 1);
        }

        Debug.Log("Me dead!");
        return null;

        //Storm.CRC32 StormCrc = new Storm.CRC32();

        //if (name != string.Empty) crc = StormCrc.HashString(name);
        //iUnifiedVariable rt = GetVariableByHandle(GetHandle(crc));

        //if (name[0] != '\\') return rt;


        ////        mpLog.LogDebug(rt.GetType());

        //return rt;

        //Storm.CRC32 StormCrc = new Storm.CRC32();
        //uint DebugCRC = StormCrc.HashString(name);
        //int separatorPosition = name.IndexOf('\\');
        //mpLog.LogDebug(this.GetType().ToString() + " Container " + name + " separator " + separatorPosition);
        //string c = name;
        ////throw new Exception(c);
        //if (separatorPosition > 0) c = name.Substring(separatorPosition);
        ////string c = (name == String.Empty) ? name : name.Substring(separatorPosition);
        //if (separatorPosition >= 0)
        //{
        //    mpLog.LogDebug("// если строка начинается с '\\' " + ((c == name) ? "true":"false"));
        //    // если строка начинается с '\'
        //    if (c == name) return mpParent.GetVariableByName(name, crc);
        //    mpLog.LogDebug("// получаем код");
        //    // получаем код
        //    //crc = Crc32.Code(name, c - name);
        //    //// если это '..'
        //    //if (crc == 0x69F7E9E3) return mpParent->GetVariableByName(c + 1, crc);
        //}
        //else
        //{
        //    // получаем код
        //    if (name != String.Empty) crc = Hasher.HshString(name);
        //    // если это '.'
        //    if (crc == 0xF12B1DBD) { AddRef(); return (iUnifiedVariable)this; }
        //}
        //mpLog.LogDebug(this.GetType().ToString() + " получаем переменную " + name +" " + crc.ToString("X8") + " ref " + DebugCRC.ToString("X8"));
        //// получаем переменную
        //iUnifiedVariable rt = GetVariableByHandle(GetHandle(crc));
        //if (rt == null)
        //{
        //    throw new Exception("Null value");
        //    return null;
        //}
        //mpLog.LogDebug(this.GetType().ToString() + " если это имя заканчивается [" + c + "]" );
        //// если это имя заканчивается
        //if (c == string.Empty) return rt;
        //mpLog.LogDebug(this.GetType().ToString() + " пытаемся сконвертить переменную " + rt.GetType() + " в контейнер");
        //// пытаемся сконвертить переменную в контейнер
        ////iUnifiedVariableContainer ctr = rt->Query<iUnifiedVariableContainer>();
        //iUnifiedVariableContainer ctr = (iUnifiedVariableContainer)rt;
        //rt.Release();
        //mpLog.LogDebug(this.GetType().ToString() + "если не удалось - возвращаем 0");
        //// если не удалось - возвращаем 0
        //if (ctr == null) return null;
        //mpLog.LogDebug(this.GetType().ToString() + "если удалось - пусть она и разбирает строчку ["+ c +"] дальше");
        //// если удалось - пусть она и разбирает строчку дальше
        ////rt = ctr.GetVariableByName(c + 1);
        //rt = ctr.GetVariableByName(c);
        //ctr.Release();
        //mpLog.LogDebug(this.GetType() + " Done! ");
        //return rt;
    }

    public bool ImportFromFile(string filename)
    {
        throw new NotImplementedException();
    }

    public bool IsReadOnly()
    {
        return mpMemMgr.IsReadOnly();
    }

    public void OnDelete(iUnifiedVariable var)
    {
        Assert.IsFalse(mpMemMgr.IsReadOnly());
        // ищем эту перемнную
        int i;
        for (i = 0; i < mArraySize; i++)
            if (mpArray[i].mpVar == var) break;
        Assert.IsTrue(i < mArraySize);
        mpArray[i].mpVar = null; 
        // переписываем массив (если не удаляемся сами)
        if (mIsDeleting == false) ReAlloc(i, i, i + 1);
        // уменьшаем свой счетчик
        this.Release();
    }

    public void OnRelease(iUnifiedVariable var, uint MemId)
    {
        // ищем эту перемнную
        int i;
        for (i = 0; i < mArraySize; i++)
            if (mpArray[i].mpVar == var) break;
        Assert.IsTrue(i < mArraySize);
        // запоминае ее MemId
        mpArray[i].mMemId = MemId;
        mpArray[i].mpVar = null;
        // уменьшаем свой счетчик
        this.Release();
    }

    public object Query(uint ClassID)
    {
        switch (ClassID)
        {
            case iUnifiedVariable.ID: AddRef(); return (iUnifiedVariable)this;
            case iUnifiedVariableContainer.ID: AddRef(); return (iUnifiedVariableContainer)this;
            default: return null;
        }

    }

    public int Release()
    {
        return 0;
    }

    public bool Rename(string pSrcName, string pDstName)
    {
        throw new NotImplementedException();
    }

    public T GetVariableTpl<T>(string name, uint crc = 0xFFFFFFFF) where T : iUnifiedVariable
    {
        iUnifiedVariable t = GetVariableByName(name, crc);
        if (t == null) Debug.LogFormat("{0} tpl {1} req <{2}>\n{3}",name,"[Failed]",typeof(T),this);
        T dT = default;
        try
        {
            dT = (T)t;
            return dT;
        }
        catch
        {
            if (typeof(UniVarReference) == t.GetType())
            {
                dT = (T)((UniVarReference)t).GetReference();
            }
        }


        return dT;
    }

    public T GetVariableTpl<T>(DWORD Handle) where T : iUnifiedVariable
    {
        iUnifiedVariable t = GetVariableByHandle(Handle);
        if (t == null) return default;
        return (T)t;
    }

    public iUnifiedVariableContainer CreateVariableContainer(string name)
    {
        iUnifiedVariable t = CreateVariableByName(iUnifiedVariableContainer.ID, name);
        if (t == null) return null;
        iUnifiedVariableContainer tpl = (iUnifiedVariableContainer)t.Query(iUnifiedVariableContainer.ID);
        t = null;
        return tpl;
    }


    public iUnifiedVariableContainer createContainer(string name)
    {
        return CreateVariableContainer(name);
    }

    public Stream openStream(string name)
    {
        UniVarBlock block = (UniVarBlock)GetVariableByName(name);
        byte[] data;
        block.GetValue(out data, block.GetLength());
        MemoryStream st = new MemoryStream(data);
        return st;
    }

    public int RefCount()
    {
        throw new NotImplementedException();
    }
}




