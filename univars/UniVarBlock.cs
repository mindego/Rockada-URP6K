using System;
using DWORD = System.UInt32;
/// <summary>
/// UniVarBlock - реализация iUnifiedVariableBlock
/// </summary>
public class UniVarBlock : iUnifiedVariableBlock
{

    // от IRefMem
    public void AddRef()
    {
        mCounter++;
    }
    public int Release()
    {
        return 0;
    }

    // от iUnifiedVariable
    public void Query(int value)
    {
        throw new NotImplementedException();
    }
    public uint GetClassId()
    {
        return iUnifiedVariable.ID;
    }

    public bool Delete()
    {
        return false;
    }
    public bool ExportToFile(string filename)
    {
        return false;
    }
    public bool ImportFromFile(string filename)
    {
        return false;
    }
    public int GetNameLength()
    {
        return mpParent.GetNameLength(this);
    }
    public string GetName(ref string buffer)
    {
        return mpParent.GetName(ref buffer, this);
    }

    // от iUnifiedVariableBlock
    public int GetLength()
    {
        return (int)mpParent.GetMemManager().GetSizeByID(mMemID);
    }
    public void GetValue(out byte[] dst, int dst_length)
    {
        dst = mpParent.GetMemManager().GetDataByID(mMemID);
    }
    public bool SetValue(byte[] src, int src_length)
    {
        throw new NotImplementedException("И не нужно!");
    }

    public int RefCount()
    {
        throw new NotImplementedException();
    }

    // own

    private int mCounter;
    private iUniVarParent mpParent;
    private DWORD mMemID;
    public UniVarBlock(iUniVarParent par, DWORD memid) : this(1, par, memid) { }

    public UniVarBlock(int mCounter, iUniVarParent mpParent, uint mMemID)
    {
        this.mCounter = mCounter;
        this.mpParent = mpParent;
        this.mMemID = mMemID;
    }
}
