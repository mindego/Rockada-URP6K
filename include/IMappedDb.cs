using crc32 = System.UInt32;

public interface IMappedDb : IRefMem
{ // only support DBA_READONLY
    new public const uint ID = (0x72E1CDD6);

    public const uint DBFORMAT_NAKED = 0x41544144;
    // Open/Close
    public int Open(string name, bool write = false);
    public void Close();

    // Info
    public int GetFormatID();
    public bool IsOpened();

    public int GetNumRecords();         // Get number of items
    public ObjId GetRecord(int index);          // Get id by number
    public int GetRecordNumber(ObjId id);  // Get number by id
    public crc32 GetRecordCrc(int index);       // Get checksum by number

    // L0 api
    public MemBlock GetBlock(ObjId id);    // Retrieve the data memory block
    public ObjId CompleteObjId(ObjId id);    // Resolve name by id
    public void CompleteObjId(ref ObjId id);    // Resolve name by id

    //template<class T> T* LoadData(ObjId id)
    //  {
    //      return GetBlock(id).Convert<T>();
    //  }

    //  public interface ICatalogEnum
    //  {
    //      public bool ProcessCatRecord(MemBlock, ObjId);
    //};

    //  virtual void EnumCatalog(ICatalogEnum ce, bool IncludeUnnamed = false); // Enum catalog

    // L1 api, using Cache (readonly mode)
    //virtual IDataCache InstallCache(IDataCache dataCache);        // Set DataCache object

    //virtual IObject LoadCached(ObjId);                 // Load cached object
    //virtual IObject CreateDefault();                   // call IDataCache::CreateDefault method

    public static IMappedDb CreateMappedDb(uint format)
    {
        return new ResourcePack(format);
    }

    //DEBUG
    public void ListRecords();

};

