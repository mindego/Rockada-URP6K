using System;
using System.IO;
using System.Text;
using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// UniVarReference - реализация iUnifiedVariableReference
/// </summary>
public class UniVarReference : iUnifiedVariableReference
{
    private int mCounter;
    private iUniVarParent mpParent;
    private DWORD mMemID;

    // UniVarReference - от iUnifiedVariable
    public object Query(uint ClassID)
    {
        switch (ClassID)
        {
            case iUnifiedVariable.ID: return (iUnifiedVariable)this;
            case iUnifiedVariableReference.ID: return (iUnifiedVariableReference)this;
            default:
                {
                    iUnifiedVariable pVar = GetReference();
                    if (pVar == null) return null;
                    var r = pVar.Query(ClassID);
                    pVar.Release();
                    return r;
                }
        }
    }

    public UniVarReference(iUniVarParent par, DWORD memid) : this(1, par, memid) { }

    public UniVarReference(int mCounter, iUniVarParent mpParent, uint mMemID)
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
        byte[] data = mpParent.GetMemManager().GetDataByID(mMemID);
        Out.Write(data);
        Out.Close();
        return true;
    }

    public uint GetClassId()
    {
        return iUnifiedVariableReference.ID;
    }

    public string GetName(ref string value)
    {
        return mpParent.GetName(ref value, this);
    }

    public int GetNameLength()
    {
        return mpParent.GetNameLength(this);
    }

    public iUnifiedVariable GetReference()
    {
        Debug.Log("Converting reference to actual value");
        int l = (int) mpParent.GetMemManager().GetSizeByID(mMemID);
        if (l == 0) return null;

        //string name = mpParent.GetMemManager().GetPtrByID(mMemID);
        //string name = "no"; //TODO - исправить некорректное получение имени ссылки.
        //Debug.Log(l);
        byte[] data = mpParent.GetMemManager().GetPtrByID(mMemID);
        Encoding enc = Encoding.GetEncoding("windows-1251");
        string name = enc.GetString(data);
        //Debug.Log(name);
        var val = mpParent.GetVariableByName(name, 0xFFFFFFFF);
        //Debug.Log(val!=null? val.getNameShort():"failed");

        return val;
        //return mpParent.GetVariableByName(name, 0xFFFFFFFF);
    }

    public void GetReferenceName(out string dst)
    {
        dst = null;
        int l = (int) mpParent.GetMemManager().GetSizeByID(mMemID);
        if (l > 0)
        {
            Encoding enc = Encoding.GetEncoding("windows-1251");
            dst = enc.GetString(mpParent.GetMemManager().GetPtrByID(mMemID));
        }
    }

    public int GetReferenceNameLength()
    {
        return (int) mpParent.GetMemManager().GetSizeByID(mMemID);
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

    public bool SetReferenceName(string src)
    {
        return false;
    }


}