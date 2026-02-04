using DWORD = System.UInt32;
using crc32 = System.UInt32;

public interface iTableAccess : IObject
{
    new public const uint ID = 0x5BCB5EB4;
    public DWORD GetColoumnCount();
    public DWORD GetDataRowsCount();
    public DWORD GetRowsCount();
    public string GetColoumnName(DWORD num);
    public string GetRowName(DWORD num);
    public bool AddColoumn(string name, DWORD id);
    public bool AddRow(string name, DWORD id);
    public bool IsEmpty();
    public bool IsNew();
};

public interface iUnivarTable : iTableAccess
{
    new public const uint ID = 0xF7083980;
    public iUnifiedVariable GetItem(DWORD id, DWORD y, DWORD x);
    public iUnifiedVariable GetItemByNames(DWORD id, crc32 row_name, crc32 coloumn_name);
    public bool MergeTable(iUnivarTable t, bool exact_merge);
    public T GetItemTpl<T>(int y, int x) { return GetItemTpl<T>((DWORD)y, (DWORD)x); }
    public T GetItemTpl<T>(DWORD y, DWORD x)
    {
        uint tid = iUnifiedVariable.GetID<T>();

        iUnifiedVariable t = GetItem(tid, y, x);
        if (t == null) return default;
        T tpl = (T)t.Query(tid);
        t.Release();
        return tpl;
    }
    public T GetItemByNamesTpl<T>(crc32 y, crc32 x)
    {
        uint tid = iUnifiedVariable.GetID<T>();

        iUnifiedVariable t = GetItemByNames(tid, y, x);
        if (t == null) return default;
        T tpl = (T)t.Query(tid);
        t.Release();
        return tpl;
    }
};




public interface iIntTable
{
    public const uint ID = 0x65142A58;
    public void SetValue(DWORD y, DWORD x, int value);
    public int GetValue(DWORD y, DWORD x);
};