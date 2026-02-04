using System.IO.Hashing;
using DWORD = System.UInt32;
using UVTab = Tab<iUnifiedVariable>;
using crc32 = System.UInt32;

//#define TITEM_INVALID 0xFFFFFFFF
public class CellInfo
{
    public string mpName;
    public DWORD mNumber;
    public CellInfo(string name, DWORD numb)
    {
        mpName = name;
        mNumber = numb;
    }
    ~CellInfo()
    {
        if (mpName != null) mpName = null;
    }
};

public class TableInfo
{
    public const DWORD TITEM_INVALID = 0xFFFFFFFF;
    Tab<CellInfo> mpRows = new Tab<CellInfo>();
    Tab<CellInfo> mpColoumns = new Tab<CellInfo>();

    public void SetRowCount(DWORD row) { mpRows.SetCount((int)row); }
    public void SetColoumnCount(DWORD col) { mpColoumns.SetCount((int)col); }
    public DWORD GetRowsCount() { return (DWORD)mpRows.Count(); }
    public DWORD GetColoumnsCount() { return (DWORD)mpColoumns.Count(); }

    public void SetRowName(DWORD num, string name) { throw new System.NotImplementedException(); }
    public void SetColoumnName(DWORD num, string name) { throw new System.NotImplementedException(); }
    public string GetRowName(DWORD num) { return mpRows[(int)num].mpName; }
    public string GetColoumnName(DWORD num) { return mpColoumns[(int)num].mpName; }
    public bool AddColoumn(string name, iUnifiedVariableContainer cols)
    {
        bool ret = false;
        DWORD hs = Hasher.HashString(name);
        for (int i = 0; i < mpColoumns.Count(); ++i)
        {
            if (Hasher.HashString(mpColoumns[i].mpName) == hs)
                return false;
        }
        if (cols!=null)
        {
            iUnifiedVariableInt val = cols.CreateVariableTpl<iUnifiedVariableInt>(name);
            if (val!=null)
            {
                mpColoumns.New(new CellInfo(name, (DWORD) mpColoumns.Count()));
                val.SetValue(mpColoumns.Count() - 1);
                ret = true;
            }
        }
        return ret;
    }
    public bool AddRow(string name, iUnifiedVariableContainer rows)
    {
        bool ret = false;
        DWORD hs = Hasher.HashString(name);
        for (int i = 0; i < mpRows.Count(); ++i)
        {
            if (Hasher.HashString(mpRows[i].mpName) == hs)
                return false;
        }
        if (rows!= null)
        {
            iUnifiedVariableInt val = rows.CreateVariableTpl<iUnifiedVariableInt>(name);
            if (val!= null)
            {
                mpRows.New(new CellInfo(name, (DWORD) mpRows.Count()));
                val.SetValue(mpRows.Count() - 1);
                ret = true;
            }
        }
        return ret;
    }

    public DWORD FindRowByName(crc32 name)
    {
        for (int i = 0; i < mpRows.Count(); ++i)
            if (Hasher.HashString(mpRows[i].mpName) == name)
            {
                return mpRows[i].mNumber;
            }
        return TITEM_INVALID;
    }
    public DWORD FindColoumnByName(crc32 name)
    {
        for (int i = 0; i < mpColoumns.Count(); ++i)
            if (Hasher.HashString(mpColoumns[i].mpName) == name)
                return mpColoumns[i].mNumber;
        return TITEM_INVALID;
    }

    public bool InitRowData(iUnifiedVariableContainer root, Tab<CellInfo> tab, int num) {
        DWORD hndl = 0;
        //char buffer[256];
        string buffer="";
        int n = 0;
        tab.SetCount(num);
        for (int i = 0; i < num; ++i)
            tab[i] = null;
        while ((hndl = root.GetNextHandle(hndl))!=0)
        {
            iUnifiedVariableInt cur = root.GetVariableTpl<iUnifiedVariableInt>(hndl);
            if (cur!=null)
            {
                int vn = cur.GetValue();
                root.GetNameByHandle(ref buffer, hndl);
                if (vn < tab.Count())
                    tab[cur.GetValue()] = new CellInfo(buffer, (DWORD) vn);
            }
            n++;
        }
        return true;
    }
    public bool InitStructure(iUnifiedVariableContainer rows, iUnifiedVariableContainer cols)
    {
        bool ret = false;
        if (rows!=null && cols!=null)
        {
            ret = InitRowData(cols, mpColoumns, (int)cols.GetSize());
            if (ret)
                ret = InitRowData(rows, mpRows, (int)rows.GetSize());
        }
        return ret;
    }
    ~TableInfo()
    {
        //for (int i = 0; i < mpRows.Count(); ++i)
        //    if (mpRows[i]) delete mpRows[i];
        //for (i = 0; i < mpColoumns.Count(); ++i)
        //    if (mpColoumns[i]) delete mpColoumns[i];

    }
};

public class UnivarTable : iUnivarTable
{
    protected TableInfo mInfo = new TableInfo();

    iUnifiedVariableArray mpTableData;
    iUnifiedVariableContainer mpRowsData;
    iUnifiedVariableContainer mpColsData;

    protected Tab<UVTab> mpRows = new Tab<UVTab>();

    bool InitData(iUnifiedVariableArray arr)
    {
        int n = (int)arr.GetSize();     // берем количество строчек
        mpRows.SetCount(n);   // устанавливаем количество строчек
        for (int i = 0; i < n; ++i)
        {    // бежим по строчкам
            UVTab local_tab = new UVTab(); // создаем tab
            mpRows[i] = local_tab;
            local_tab.SetCount((int)mInfo.GetColoumnsCount());   // размерность равна установленному
            iUnifiedVariableArray row = arr.GetVariableTpl<iUnifiedVariableArray>(i);
            if (row != null && row.GetSize() != mInfo.GetColoumnsCount())
                return false; // integrity fail
            for (int j = 0; j < mInfo.GetColoumnsCount(); ++j)
            {   // заполняем
                iUnifiedVariable var = null;
                if (row != null)
                    var = row.GetVariableTpl<iUnifiedVariable>(j);
                local_tab[j] = var;
                //TODO Возможно, правильнее использовать mpRows[i]
            }
        }
        return true;
    }
    public bool InitTable(iUnifiedVariableContainer root)
    {
        iUnifiedVariableContainer structure = root.CreateVariableTpl<iUnifiedVariableContainer>("Structure");
        mpTableData = root.CreateVariableTpl<iUnifiedVariableArray>("Table");
        bool ret = false;
        if (structure != null && mpTableData != null)
        {
            mpRowsData = structure.CreateVariableTpl<iUnifiedVariableContainer>("Rows");
            mpColsData = structure.CreateVariableTpl<iUnifiedVariableContainer>("Coloumns");
            ret = mInfo.InitStructure(mpRowsData, mpColsData);
        }
        if (ret && root != null)
            ret = InitData(mpTableData);
        return ret;
    }


    bool AppendRow(string name, DWORD id)
    {
        bool ret = false;
        if (mpTableData != null && mInfo.GetColoumnsCount() > 0)
        {
            uint n = mpTableData.GetSize();             // теперь вставляем строчку в UDB
            mpTableData.SetSize(n + 1);
            iUnifiedVariableArray row = mpTableData.CreateVariableTpl<iUnifiedVariableArray>(n);
            if (row != null)
            {       // теперь вставляем колонки
                row.SetSize(mInfo.GetColoumnsCount());
                UVTab local_tab = new UVTab();       // создаем новую строчку
                //mpRows.New() = local_tab;           // вставляем ее
                mpRows.New(local_tab);
                local_tab.SetCount((int)mInfo.GetColoumnsCount());
                for (int i = 0; i < mInfo.GetColoumnsCount(); ++i)
                {
                    iUnifiedVariable var = row.CreateVariable(id, (uint)i);
                    (local_tab)[i] = var;
                    if (var == null)
                        ret = true;
                }
                ret = !ret;
            }
        }
        return ret;
    }

    bool AppendColoumn(string name, DWORD id)
    {
        bool ret = false;
        if (mpTableData != null)
        {
            if (mpTableData.GetSize() != mpRows.Count())
                return false;
            int i;
            for (i = 0; i < mpTableData.GetSize(); ++i)
            {
                iUnifiedVariableArray row = mpTableData.CreateVariableTpl<iUnifiedVariableArray>(i);
                if (row != null)
                {
                    UVTab local_tab = mpRows[i];
                    uint n = row.GetSize();
                    row.SetSize(n + 1);
                    iUnifiedVariable var = row.CreateVariable(id, n);
                    if (var != null)
                    {
                        n = (uint)local_tab.Count();
                        local_tab.SetCount((int)(n + 1));
                        local_tab[n] = var;
                    }
                }
            }
            ret = (i == mpTableData.GetSize());
        }
        return ret;
    }

    bool CheckY(DWORD num) { return num < mInfo.GetRowsCount(); }
    bool CheckX(DWORD num) { return num < mInfo.GetColoumnsCount(); }

    iUnifiedVariable GetCell(DWORD y, DWORD x) { throw new System.NotImplementedException("Should be implemented in nested classess"); }

    public virtual bool Initialize(iUnifiedVariableContainer root, string name)
    {
        iUnifiedVariableContainer local = root.CreateVariableTpl<iUnifiedVariableContainer>(name);
        bool ret = false;
        if (local != null)
            ret = InitTable(local);
        return ret;
    }
    public virtual void Destroy()
    {
        for (int i = 0; i < mpRows.Count(); ++i)
        {
            UVTab local_tab = mpRows[i];
            for (int j = 0; j < local_tab.Count(); ++j)
            {
                if (local_tab[j] != null)
                    local_tab[j].Release();
            }
            //TODO Проверить корректность освобождения строки
            //mpRows[i].Dispose(); 
        }
        IRefMem.SafeRelease(mpTableData);
        IRefMem.SafeRelease(mpRowsData);
        IRefMem.SafeRelease(mpColsData);
    }

    public UnivarTable()
    {
        mpTableData = null;
        mpRowsData = null;
        mpColsData = null;

    }

    // API
    // iTable Access
    public DWORD GetColoumnCount()
    {
        return mInfo.GetColoumnsCount();
    }
    public DWORD GetDataRowsCount()
    {
        return (DWORD)mpRows.Count();
    }
    public DWORD GetRowsCount()
    {
        return (DWORD)mInfo.GetRowsCount();
    }
    public string GetColoumnName(DWORD num)
    {
        return mInfo.GetColoumnName(num);
    }
    public string GetRowName(DWORD num)
    {
        return mInfo.GetRowName(num);
    }
    public bool AddColoumn(string name, DWORD id)
    {
        if (name == null) return false;
        bool ret = mInfo.AddColoumn(name, mpColsData);
        if (ret)
            ret = AppendColoumn(name, id);
        return ret;
    }
    public bool AddRow(string name, DWORD id)
    {
        bool ret = name != null ? mInfo.AddRow(name, mpRowsData) : true;
        if (ret)
            ret = AppendRow(name, id);
        return ret;
    }
    public bool IsEmpty()
    {
        return mpRows.Count() == 0;
    }
    public bool IsNew()
    {
        return !(mInfo.GetColoumnsCount() != 0 && mInfo.GetRowsCount() != 0);
    }


    // iUnivarTable
    public iUnifiedVariable GetItem(DWORD id, DWORD y, DWORD x)
    {
        iUnifiedVariable ret = null;
        if (y < mpRows.Count())
        {
            UVTab tab = mpRows[y];
            if (x < tab.Count())
                ret = tab[x];
        }
        if (ret != null)
            ret.AddRef();
        return ret;
    }
    public virtual bool MergeTable(iUnivarTable t, bool exact_merge)
    {
        return false;
    }
    public iUnifiedVariable GetItemByNames(DWORD id, crc32 row_name, crc32 coloumn_name)
    {
        DWORD x = mInfo.FindColoumnByName(coloumn_name);
        DWORD y = mInfo.FindRowByName(row_name);
        iUnifiedVariable ret = null;
        if (y < mpRows.Count())
        {
            UVTab tab = mpRows[y];
            if (x < tab.Count())
                ret = tab[x];
        }
        if (ret != null)
            ret.AddRef();
        return ret;
    }

    #region IRefMem
    public void AddRef() { throw new System.NotImplementedException(); }
    #endregion

    #region IMemory
    public int Release() { throw new System.NotImplementedException(); }
    #endregion
};
