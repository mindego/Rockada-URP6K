using System;
using System.IO;
using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// UniVarVector - реализация iUnifiedVariableVector
/// </summary>
public class UniVarVector :  iUnifiedVariableVector
{
    private int mCounter;
    private iUniVarParent mpParent;
    private DWORD mMemID;
    public UniVarVector(iUniVarParent par, DWORD memid) : this(1, par, memid) { }

    public UniVarVector(int mCounter, iUniVarParent mpParent, uint mMemID)
    {
        this.mCounter = mCounter;
        this.mpParent = mpParent;
        this.mMemID = mMemID;
    }

    public void AddRef()
    {
        mCounter++;
    }

    public bool Delete()
    {
        if (mCounter > 1 || mpParent.IsReadOnly()) return false;
        mpParent.GetMemManager().Free(mMemID);
        mpParent.OnDelete(this);
        //delete this;
        return true;
    }

    public bool ExportToFile(string filename)
    {
        TextWriter Out = File.CreateText(filename);
        //if (!Out.) return false;

        Out.Write(GetValue());
        return true;
    }

    public uint GetClassId()
    {
        return iUnifiedVariableVector.ID;
    }

    public string GetName(ref string value)
    {
        return mpParent.GetName(ref value, this);
    }

    public int GetNameLength()
    {
        return mpParent.GetNameLength(this);
    }

    public Vector3 GetValue()
    {
        if (mMemID == 0) return Vector3.zero;
        byte[] data = mpParent.GetMemManager().GetDataByID(mMemID);
        Vector3 res = Vector3.zero;
        res.x = BitConverter.ToSingle(data, 0 * 4 );
        res.y = BitConverter.ToSingle(data, 1 * 4);
        res.z = BitConverter.ToSingle(data, 2 * 4);

        return res;
    }

    public bool ImportFromFile(string filename)
    {
        return false;
    }

    public int RefCount()
    {
        throw new NotImplementedException();
    }

    public int Release()
    {
        if (--mCounter > 0) return mCounter;
        mpParent.OnRelease(this, mMemID);
        //delete this;
        return 0;
    }

    public Vector3 SetValue(Vector3 value)
    {
        return value;
    }
}