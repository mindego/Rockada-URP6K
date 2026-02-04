using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DWORD = System.UInt32;

public interface iUniVarParent
{
    public iUnifiedVariable GetVariableByName(string name, DWORD crc);
    public iUniVarMemManager GetMemManager();
    public bool IsReadOnly();
    public void OnRelease(iUnifiedVariable var, DWORD MemId);
    public void OnDelete(iUnifiedVariable var);
    public int GetNameLength(iUnifiedVariable var);
    public string GetName(ref string lVar, iUnifiedVariable rVar);
};
