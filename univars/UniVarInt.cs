using System;
using System.IO;
using UnityEngine;
using DWORD = System.UInt32;

/// <summary>
/// UniVarInt - реализация iUnifiedVariableInt
/// </summary>
public class UniVarInt : iUnifiedVariableInt
{

    private int mCounter;
    private iUniVarParent mpParent;
    private DWORD mMemID;

    public UniVarInt(iUniVarParent par, DWORD memid) : this(1, par, memid) { }

    public UniVarInt(int mCounter, iUniVarParent mpParent, DWORD mMemID)
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
        return iUnifiedVariableInt.ID;
    }

    public string GetName(ref string value)
    {
        return mpParent.GetName(ref value, this);
    }

    public int GetNameLength()
    {
        return mpParent.GetNameLength(this);
    }

    public int GetValue()
    {
        if (mMemID == 0) return 0;
        byte[] data = mpParent.GetMemManager().GetDataByID(mMemID);
        if (data.Length < 4) return 0;
        return BitConverter.ToInt32(data);
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

    public int SetValue(int value)
    {
        if (mpParent.IsReadOnly()) return GetValue();

        mMemID = mpParent.GetMemManager().Alloc(sizeof(int));
        //mpParent.GetMemManager().GetPtrByID(mMemID)=value;
        //Debug.Log(string.Format("{0} alloc mMemID {1}", mpParent.GetMemManager(),(int)mMemID));
        mpParent.GetMemManager().SetBytesByID((int)mMemID, BitConverter.GetBytes(value));
        return value;
    }
}



