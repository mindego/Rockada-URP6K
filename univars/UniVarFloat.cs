using System;
using System.IO;
using DWORD = System.UInt32;
/// <summary>
///  UniVarFloat - реализация iUnifiedVariableFloat
/// </summary>
public class UniVarFloat : iUnifiedVariableFloat
{

    private int mCounter;
    private iUniVarParent mpParent;
    private DWORD mMemID;

    public override string ToString()
    {
        return this.GetType().Name + " value: " + GetValue();
    }
    public UniVarFloat(iUniVarParent par, DWORD memid) : this(1, par, memid) { }

    public UniVarFloat(int mCounter, iUniVarParent mpParent, uint mMemID)
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
        return iUnifiedVariableFloat.ID;
    }

    public string GetName(ref string value)
    {
        return mpParent.GetName(ref value, this);
    }

    public int GetNameLength()
    {
        return mpParent.GetNameLength(this);
    }

    public float GetValue()
    {
        if (mMemID == 0) return .0f;
        byte[] data = mpParent.GetMemManager().GetDataByID(mMemID);
        //UnityEngine.Debug.Log("Data size (should be 4):" + data.Length);
        //if (data.Length < 4 ) return 0;
        
        return BitConverter.ToSingle(data);
        //return *((float*)mpParent->GetMemManager()->GetPtrByID(mMemID));
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

    public float SetValue(float value)
    {
        if (mpParent.IsReadOnly()) return GetValue();

        mMemID = mpParent.GetMemManager().Alloc(sizeof(float));
        mpParent.GetMemManager().SetBytesByID((int)mMemID, BitConverter.GetBytes(value));
        return value;
    }
}
