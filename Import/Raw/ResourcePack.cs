using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using UnityEngine;
using System.IO.MemoryMappedFiles;
using UnityEngine.Assertions;
using crc32 = System.UInt32;

public class ResourcePack : IMappedDb
{
    public DbFile RAT; //SQUEAK!
    //private FileStream dbFile;
    //public string[] names;
    public MemoryMappedFile mmf;
    public string FormatId;
    private bool mIsOpened = false;
    //IDataCache cache;

    private string NameString;
    public ResourcePack(string filename)
    {
        Init(filename);
        LoadRAT();
    }

    public ResourcePack()
    {
    }

    public ResourcePack(uint format)
    {

    }

    Dictionary<crc32, object> cache = new Dictionary<crc32, object>();
    object LoadCached(ObjId id)
    {
        Assert.IsNotNull(cache);
        //int i = Opened() ? IndexFind(GetObjId(id)) : -1;
        //return i >= 0 ? cache.CreateData(RAT.GetBlock(i), i) : cache.CreateDefault();
        //int i = Opened() ? GetIndexById(GetObjId(id)) : -1;
        //return i > 0 ? cache.CreateData(GetBlock(id), i) : cache.CreateDefault();
        if (cache.ContainsKey(id.crc32())) return cache[id.crc32()];
        return GetBlock(id);
    }

    crc32 GetObjId(ObjId id)
    {
        return (id.obj_id == CRC32.CRC_NULL && id.name !=null) ? Hasher.HshString(id.name) : id.obj_id;
    }

    bool Opened() { return mIsOpened; }


    public virtual byte[] GetBytesById(uint id)
    {
        int index = GetIndexById(id);
        return GetBytesByIndex(index);
    }
    public byte[] GetBytesByIndex(int index)
    {
        if (index >= RAT.index.Length) return null;
        if (index < 0) return null;

        DbIndex dataIndex = RAT.index[index];
        return StormFileUtils.ReadBytes(mmf, (int)dataIndex.file_offset, (int)dataIndex.data_size);
        //dbFile.Seek(dataIndex.file_offset, SeekOrigin.Begin);
        //MemoryStream ms = new MemoryStream();
        //byte[] buffer = new byte[dataIndex.data_size];
        //dbFile.Read(buffer);
        //ms.Write(buffer);
        //ms.Seek(0, SeekOrigin.Begin);

        //return ms;
    }

    /// <summary>
    /// Функция получения MemoryStream по идентификатору ресурса
    /// </summary>
    /// <param name="id">Идентификатор ресурса</param>
    /// <returns>MemoryStream с данными ресурса или null, если идентификатор не найден</returns>
    public virtual Stream GetStreamById(uint id)
    {
        int index = GetIndexById(id);
        return GetStreamByIndex(index);
    }
    /// <summary>
    /// Функция получения MemoryStream по имени ресурса
    /// </summary>
    /// <param name="name">Имя ресурса</param>
    /// <returns>MemoryStream с данными ресурса или null, если имя не найдено</returns>
    public virtual Stream GetStreamByName(string name)
    {
        int index = GetIndexByName(name);
        return GetStreamByIndex(index);

    }
    /// <summary>
    /// Функция получения индекса в паке ресурсов по идентификатору ресурса.
    /// </summary>
    /// <param name="id">Имя ресурса</param>
    /// <returns>int Индекс ресурса или -1, если ресурс не найден</returns>

    public int GetIndexById(uint id)
    {
        for (int i = 0; i < RAT.num; i++)
        {
            if (RAT.index[i].object_id == id)
            {
                return i;
            }
        }
        return -1;
    }
    /// <summary>
    /// Функция получения индекса в паке ресурсов по имени ресурса.
    /// </summary>
    /// <param name="name">Имя ресурса</param>
    /// <returns>int Индекс ресурса или -1, если ресурс не найден</returns>
    public int GetIndexByName(string name)
    {
        //for (int i = 0; i < RAT.num; i++)
        //{
        //    if (names[i] == name)
        //    {
        //        return i;
        //    }
        //}
        //return -1;
        uint id = Hasher.HshString(name);
        return GetIndexById(id);
    }
    /// <summary>
    /// Функция получения MemoryStream по индексу в паке ресурсов
    /// </summary>
    /// <param name="index">Индекс записи в паке ресурсов</param>
    /// <returns>MemoryStream или null, если такого индекса нет</returns>
    public Stream GetStreamByIndex(int index)
    {
        if (index >= RAT.index.Length) return null;
        if (index < 0) return null;

        DbIndex dataIndex = RAT.index[index];
        return StormFileUtils.GetStream(mmf, (int)dataIndex.file_offset, (int)dataIndex.data_size);
        //dbFile.Seek(dataIndex.file_offset, SeekOrigin.Begin);
        //MemoryStream ms = new MemoryStream();
        //byte[] buffer = new byte[dataIndex.data_size];
        //dbFile.Read(buffer);
        //ms.Write(buffer);
        //ms.Seek(0, SeekOrigin.Begin);

        //return ms;
    }
    /// <summary>
    /// Получение идентификатора ресурса из имени ресурса
    /// </summary>
    /// <param name="name">Имя ресурса</param>
    /// <returns>Идентификатор ресурса</returns>
    public virtual uint GetIdByName(string name)
    {
        int i = GetIndexByName(name);
        uint id = 0xFFFFFFFF;
        try
        {
            id = RAT.index[i].object_id;
        }
        catch
        {
            // Debug.Log($"Object [{name}] id {id:X8}  not found");
        }
        return id;
    }
    public string GetNameByIndex(int index)
    {
        if (index >= RAT.index.Length) return "ID GREATER THAN MAX " + myFileName;
        if (index < 0) return "RESOURCE NAME NOT FOUND IN " + myFileName;
        //return names[index];
        string res = "";
        int nameIndex = (int)RAT.index[index].name_index;
        for (int i = nameIndex; i < NameString.Length; i++)
        {
            if (NameString[i] == '\0') return res;
            res += NameString[i];
        }
        return "RESOURCE NAME ERROR";
    }

    public string GetNameById(uint id)
    {

        //for (int i = 0; i < RAT.index.Length; i++)
        //{
        //    if (RAT.index[i].object_id == id)
        //    {
        //        uint nameIndex = RAT.index[i].name_index;
        //        Debug.Log((nameIndex, names.Length));
        //        return names[nameIndex];
        //    }


        //}
        //Debug.Log("Using index " + GetIndexById(id) + " searching " + id.ToString("X8"));
        return GetNameByIndex(GetIndexById(id));
        //return  "Unnamed Resource " + id.ToString("X8");
    }


    string myFileName;
    public bool Init(string filename)
    {
        myFileName = filename;
        //if (!File.Exists(filename)) throw new Exception("File not found "+filename);
        if (!File.Exists(filename)) return false;
        string hash = Hash128.Compute(filename.ToLower()).ToString();
        //Debug.Log("Loading " + filename);

        try
        {
            //Debug.Log("Loading old " + filename);
            mmf = MemoryMappedFile.OpenExisting(hash);
            //Debug.Log("[SUCCESS]");
            //Debug.Log("Loaded old " + filename);
        }
        catch (IOException)
        {

            //Debug.Log("Loading new " + filename + " exists " + File.Exists(filename) + mmf);
            mmf = MemoryMappedFile.CreateFromFile(filename, FileMode.Open, hash);
        }

        return true;
    }
    public struct PackHeader
    {
        public int db_id;
        public int num;
        public int data_size;
        public int name_size;
    }

    public bool LoadRAT()
    {
        PackHeader packHeader = (PackHeader)StormFileUtils.ReadStruct<PackHeader>(mmf);

        RAT.data_size = packHeader.data_size;
        RAT.db_id = packHeader.db_id;
        RAT.num = packHeader.num;
        RAT.name_size = packHeader.name_size;
        RAT.index = new DbIndex[RAT.num];

        int dataOffset = 0 + StormFileUtils.GetSize<PackHeader>();


        RAT.index = StormFileUtils.ReadStructs<DbIndex>(mmf, dataOffset, packHeader.num);

        //int namesOffset = dataOffset + StormFileUtils.GetSize<DbIndex>() * packHeader.num + packHeader.data_size;
        int namesOffset = packHeader.data_size + dataOffset + StormFileUtils.GetSize<DbIndex>() * packHeader.num;

        byte[] buffer = StormFileUtils.ReadBytes(mmf, namesOffset, packHeader.name_size);
        Encoding encoding = Encoding.GetEncoding("windows-1251");
        NameString = encoding.GetString(buffer);

        //names = namesString.Trim('\0').Split('\0');

        //List<string> tmpList = new List<string>();
        //string tmpName = "";
        //foreach (char c in namesString)
        //{
        //    if (c=='\0')
        //    {
        //        tmpList.Add(tmpName);
        //        tmpName = "";
        //        continue;
        //    }

        //    tmpName += c;
        //}
        //names = tmpList.ToArray();
        //Debug.Log("Result array size: " + names.Length);
        //Debug.Log("Splitted array size " + names.Length);
        //names = new string[RAT.num+100];
        //int nameIndex = 0;
        //Debug.Log("Array size  " + names.Length);
        //int cnt = 0;
        //Debug.Log("First name index " + RAT.index[0].name_index);
        //foreach (char c in namesString)
        //{
        //    cnt++;
        //    if (c == '\0') nameIndex++;
        //    try
        //    {
        //        names[nameIndex] += c;
        //    }catch
        //    {
        //        Debug.Log("Index: " + nameIndex + " cnt " + cnt);
        //        Debug.Log("Prev name: " + names[nameIndex-5]);
        //        StormFileUtils.SaveXML<string>("debug.xml" ,namesString);
        //        throw;
        //    }
        //}

        return true;
    }
    //public bool LoadRATFile()
    //{

    //    byte[] buffer = new byte[4];

    //    dbFile.Read(buffer);
    //    RAT.db_id=BitConverter.ToInt32(buffer);
    //    dbFile.Read(buffer);
    //    RAT.num = BitConverter.ToInt32(buffer);
    //    dbFile.Read(buffer);
    //    RAT.data_size = BitConverter.ToInt32(buffer);
    //    dbFile.Read(buffer);
    //    RAT.name_size = BitConverter.ToInt32(buffer);

    //    RAT.index = new DbIndex[RAT.num];

    //    for (int i=0;i<RAT.num; i++)
    //    {
    //        DbIndex index = new DbIndex();
    //        dbFile.Read(buffer);
    //        index.object_id = BitConverter.ToUInt32(buffer);
    //        dbFile.Read(buffer);
    //        index.name_index= BitConverter.ToUInt32(buffer);
    //        dbFile.Read(buffer);
    //        index.data_size = BitConverter.ToUInt32(buffer);
    //        dbFile.Read(buffer);
    //        index.file_offset = BitConverter.ToUInt32(buffer);

    //        RAT.index[i] = index;
    //    }

    //    int names_offset = 4 * 4 + RAT.num * DbIndex.SIZE + RAT.data_size; // 4*4 - header size in bytes;

    //    buffer = new byte[RAT.name_size];
    //    dbFile.Seek(names_offset, SeekOrigin.Begin);
    //    dbFile.Read(buffer);

    //    Encoding encoding = Encoding.GetEncoding("windows-1251");
    //    string namesString = encoding.GetString(buffer);

    //    string[] namesArray = namesString.Trim('\0').Split('\0');
    //    //Debug.Log((namesArray.Length,RAT.num));
    //    names = new string[RAT.num];
    //    for (int i = 0; i < RAT.num; i++)
    //    {
    //        //DbIndex dataIndex = RAT.index[i];
    //        //string smallString = namesString.Substring((int)dataIndex.name_index);
    //        //string[] namesArray = smallString.Trim('\0').Split('\0');
    //        //names[i] = GetNameFromFile(i);
    //        names[i] = namesArray[i];
    //    } 
    //        return true;
    //}

    public int Open(string name, bool write = false)
    {
        Debug.Log("Loading " + name);
        mIsOpened = false;
        //if (!File.Exists(name)) return DBDef.DB_CANT_OPEN;
        if (!Init(name)) return DBDef.DB_CANT_OPEN;
        if (!LoadRAT()) return DBDef.DB_BAD_FORMAT;

        mIsOpened = true;
        return DBDef.DB_OK;
    }

    public void Close()
    {
        if (mmf != null) mmf.Dispose();
        mIsOpened = false;
        //throw new NotImplementedException();
    }

    public int GetFormatID()
    {
        throw new NotImplementedException();
    }

    public bool IsOpened()
    {
        return mIsOpened;
    }

    public int GetNumRecords()
    {
        return RAT.num;
    }

    public ObjId GetRecord(int index)
    {
        throw new NotImplementedException();
    }

    public int GetRecordNumber(ObjId id)
    {
        for (int i = 0; i < RAT.num; i++)
        {
            if (id.obj_id == RAT.index[i].GetCode()) return i;
        }
        return -1;
    }

    public uint GetRecordCrc(int index)
    {
        throw new NotImplementedException();
    }

    public MemBlock GetBlock(ObjId id)
    {
        if (id.name != null) id.obj_id = Hasher.HshString(id.name);
        //MemBlock res = new MemBlock(GetStreamById(id.obj_id));
        MemBlock res = new MemBlock();
        //res.myStream = GetStreamById(id.obj_id);
        res.buffer = GetBytesById(id.obj_id);
        return res;
    }

    public ObjId CompleteObjId(ObjId id)
    {

        //id.name = GetNameById(id);
        if (id.name != null) return new ObjId(id.name, Hasher.HshString(id.name));
        return new ObjId(GetNameById(id), id.obj_id);
    }

    public void CompleteObjId(ref ObjId id)
    {

        //id.name = GetNameById(id);
        if (id.name != null) {
            id.obj_id = Hasher.HshString(id.name);
            return;
        }
        id.name = GetNameById(id);
        return;
    }

    public void AddRef()
    {
        throw new NotImplementedException();
    }

    public int Release()
    {
        return 0;
    }

    public int RefCount()
    {
        throw new NotImplementedException();
    }

    public void ListRecords()
    {
        foreach (DbIndex val in RAT.index)
        {
            ObjId tmp = new ObjId(val.object_id);
            CompleteObjId(tmp);
            Debug.Log(string.Format("{0} {1}", tmp.name, tmp.obj_id.ToString("X8")));
        }
    }

}

public struct DbIndex
{
    /*
    short, short int, signed short, signed short int

    unsigned short, unsigned short int

    int, signed, signed int

    unsigned, unsigned int

    long, long int, signed long, signed long int

    unsigned long, unsigned long int

    long long, long long int, signed long long, signed long long int

    unsigned long long, unsigned long long int

     */
    public uint object_id;
    public uint name_index;
    public uint data_size;
    public uint file_offset;
    public const int SIZE = 4 * 4;
    public uint GetCode()
    {
        return object_id;
    }

    public override string ToString()
    {
        return "Oid: " + object_id + " NI " + name_index + " DS " + data_size + " FO " + file_offset;
    }
}
public struct DbFile
{
    public int db_id;              // data id
    public int num;                // number of data entries
    public int data_size;          //
    public int name_size;
    public DbIndex[] index;

    public DbIndex GetRecord(int i)
    {
        return index[i];
    }

    public byte[] GetNameTable()
    {
        return new byte[0];
    }

    //public MemBlock GetBlock(int i) { return new MemBlock(GetData(i), (int)index[i].data_size); }

    /*  const DbIndex & GetRecord(int i) const {  return index[i];  }

  char* GetNameTable() { return (char*)this + sizeof(DbFile) + sizeof(DbIndex) * num + data_size; }

  char* GetData(int i) { return (char*)this + index[i].file_offset; }
  MemBlock GetBlock(int i) { return MemBlock(GetData(i), index[i].data_size); }
  ObjId GetObjId(int i) { return ObjId(GetNameTable() + index[i].name_index, index[i].object_id); }

  int FileSize() const     { return sizeof(DbFile) +sizeof(DbIndex) * num + data_size + name_size; }
    */

};

//public class ResourceStorage<T> : ResourcePack
//{
//    Dictionary<string, T> storage = new Dictionary<string, T>();
//    Storm.CRC32 crc = new Storm.CRC32();

//    public ResourceStorage(string filename) : base(filename)
//    {
//    }


//    public override uint GetIdByName(string name)
//    {
//        //Debug.Log("Finding [" + name + "]");
//        try
//        {
//            if (!name.Contains('#')) return base.GetIdByName(name);
//        }
//        catch
//        {
//            Debug.Log("Error finding [" + name + "]");
//            return 0xFFFFFFFF;
//        }
//        crc = new Storm.CRC32();
//        return crc.HashString(name);
//    }

//    public override Stream GetStreamById(uint id)
//    {
//        return base.GetStreamById(id);
//    }

//    public override Stream GetStreamByName(string name)
//    {
//        if (!name.Contains('#')) return base.GetStreamByName(name);

//        return GetStreamById(GetIdByName(name));
//    }
//}

interface IDataCache : IRefMem
{
    public const uint id = 0x69BD7B35;
    public void InitCache(int num_indexes);
    public void DoneCache();
    public IObject CreateData(MemBlock bl, int index);
    public IObject CreateDefault() { return CreateData(new MemBlock(), -1); }
};
