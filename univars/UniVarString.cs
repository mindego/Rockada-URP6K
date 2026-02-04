using System;
using System.IO;
using System.Text;
using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// UniVarString - реализация iUnifiedVariableString
/// </summary>
public class UniVarString : iUnifiedVariableString
{
    private int mCounter;
    private iUniVarParent mpParent;
    private DWORD mMemID;
    public UniVarString(iUniVarParent par, DWORD memid) : this(1, par, memid) { }
    public UniVarString(int mCounter, iUniVarParent mpParent, uint mMemID)
    {
        this.mCounter = mCounter;
        this.mpParent = mpParent;
        this.mMemID = mMemID;
    }

    // UniVarString - от iUnifiedVariable
    public object Query(uint ClassID)
    {
        switch (ClassID)
        {
            case iUnifiedVariable.ID: AddRef(); return (iUnifiedVariable)this;
            case iUnifiedVariableString.ID: AddRef(); return (iUnifiedVariableString)this;
            default: return null;
        }
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
        return iUnifiedVariableString.ID;
    }

    public string GetName(ref string value)
    {
        //string st= mpParent.GetName(ref value, this); 
        //Debug.Log(string.Format("Trying to obtain my name ({0}) {1}" ,st,this));
        //return st;
        
        return mpParent.GetName(ref value, this);
    }

    public int GetNameLength()
    {
        return mpParent.GetNameLength(this);
    }

    public bool ImportFromFile(string filename)
    {
        return false;
    }

    public int Release()
    {
        if (--mCounter > 0) return mCounter;
        mpParent.OnRelease(this, mMemID);
        //delete this;
        return 0;
    }

    public bool SetValue(string src)
    {
        //Debug.Log("Setting value for  " + this + " " + src);
        if (mpParent.IsReadOnly()) return false;
        int src_length = (src != null ? src.Length : 0);
        if (mpParent.GetMemManager().GetSizeByID(mMemID) != src_length)
        {
            mpParent.GetMemManager().Free(mMemID);
            mMemID = mpParent.GetMemManager().Alloc((uint)src_length);
        }
        //if (mMemID != 0) MemCpy(mpParent->GetMemManager()->GetPtrByID(mMemID), src, src_length);
        byte[] data = Encoding.GetEncoding("windows-1251").GetBytes(src);
        if (mMemID != 0) mpParent.GetMemManager().SetBytesByID((int)mMemID,data);
        return true;
    }

    public void StrCpy(out string dst)
    {
        if (this == default)
        {
            dst = string.Empty;
            return ;
        }
        int l = (int) mpParent.GetMemManager().GetSizeByID(mMemID);
        if (l == 0) { dst = string.Empty; return; }

        byte[] data = mpParent.GetMemManager().GetDataByID(mMemID);

        Encoding enc = Encoding.GetEncoding("windows-1251");
        dst = enc.GetString(data);
    }

    public string GetValue()
    {
        StrCpy(out string dst);
        return dst;
    }

    public int StrLen()
    {
        return (int) mpParent.GetMemManager().GetSizeByID(mMemID);
    }


    public void StrnCpy(out string dst, int n)
    {
       StrCpy(out dst);
    }

    public int RefCount()
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        string res = GetType().ToString();
        res += string.Format("\nmMemId {0} {1}",mMemID,GetValue());
        return res;
    }
}