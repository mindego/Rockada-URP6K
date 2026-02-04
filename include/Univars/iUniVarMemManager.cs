using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DWORD = System.UInt32;

public interface iUniVarMemManager
{
    public DWORD Alloc(DWORD size);
    //public string GetPtrByID(DWORD id);
    //public DWORD GetIDByPtr(string ptr);
    public DWORD GetSizeByID(DWORD id);
    public void Free(DWORD id);
    public bool SaveToFile(string filename);
    public bool IsReady();
    public bool IsReadOnly();

    //Upgrade for c#
    public byte[] GetDataByID(DWORD ID);
    public byte[] GetPtrByID(DWORD id);
    public DWORD GetIDByPtr(byte[] ptr);
    void SetBytesByID(int id,byte[] data);
}
