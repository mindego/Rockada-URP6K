using NUnit.Framework;
using static IParamList;
using DWORD = System.UInt32;

public class IntTable : UnivarTable, iIntTable
{
    public const uint ID = 0xFD906F15;
    public override bool Initialize(iUnifiedVariableContainer root, string name)
    {
        return base.Initialize(root, name);
    }
    public override void Destroy()
    {
        base.Destroy();
    }

    iIntTable GetIIntTable() { return this; }

    public void SetValue(DWORD y, DWORD x, int value)
    {
        iUnifiedVariableInt ret_int = (this as iUnivarTable).GetItemTpl<iUnifiedVariableInt>(y, x);
        if (ret_int!=null)
            ret_int.SetValue(value);
    }
    public int GetValue(DWORD y, DWORD x)
    {
        iUnifiedVariableInt ret_int = (this as iUnivarTable).GetItemTpl<iUnifiedVariableInt>(y, x);
        return ret_int!=null ? ret_int.GetValue() : 0;
    }

    // API
    public object Query(uint id)
    {
        switch (id)
        {
            case iIntTable.ID: return GetIIntTable();
        }
        return 0;
    }
    public override bool MergeTable(iUnivarTable table, bool exact_merge)
    {
        DWORD len_y = table.GetDataRowsCount();
        DWORD len_x = table.GetColoumnCount();
        if (len_y==0 || len_x==0) return false;
        if (mInfo.GetColoumnsCount() != len_x)
            return false;   // проверяем количество  
        if (exact_merge)
        {   // если точное совпадение должно быть
            if (mInfo.GetRowsCount() != len_y)
                return false;
        }

        int sz = (int) (len_x * len_y);
        bool[] saved = sz!=0 ? new bool[sz] : null;
        stdmem.bzero(ref saved, sz, false);
        //bzero(saved, sizeof(bool) * sz);
        int i;
        for (i = 0; i < len_y; ++i)
        {
            DWORD row_name = Hasher.HashString(table.GetRowName((DWORD)i));
            for (int j = 0; j < len_x; ++j)
            {
                DWORD col_name = Hasher.HashString(table.GetColoumnName((DWORD)j));
                iUnifiedVariableInt loc = (this as iUnivarTable).GetItemByNamesTpl<iUnifiedVariableInt>(row_name, col_name);
                if (loc!=null)
                {
                    iUnifiedVariableInt var = table.GetItemTpl<iUnifiedVariableInt>(i, j);
                    if (var!=null)
                        loc.SetValue(loc.GetValue() + var.GetValue());
                    saved[i * len_x + j] = true;
                }
            }
        }
        for (i = 0; i < len_y; ++i)
        {
            if (!saved[i * len_x])
            {
                AddRow(table.GetRowName((DWORD)i), iUnifiedVariableInt.ID);
                for (int j = 0; j < len_x; ++j)
                {
                    iUnifiedVariableInt var = table.GetItemTpl<iUnifiedVariableInt>(i, j);
                    iUnifiedVariableInt loc = (this as iUnivarTable).GetItemTpl<iUnifiedVariableInt>(mpRows.Count() - 1, j);
                    if (loc!=null && var!=null)
                        loc.SetValue(var.GetValue());
                }
            }
        }
        if (saved!=null) saved=null;
        return true;
    }
};
