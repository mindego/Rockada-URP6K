using geombase;
using System;
using DWORD = System.UInt32;
/// <summary>
/// iUnifiedVariableArray - массив переменных, доступ по индексу
/// </summary>
public interface iUnifiedVariableArray : iUnifiedVariableContainer
{
    new public const DWORD ID = 0x5766773F;
    public DWORD SetSize(DWORD NewSize);
    public iUnifiedVariable CreateVariable(uint ClassID, DWORD index);
    public T CreateVariableTpl<T>(DWORD index);
    public T CreateVariableTpl<T>(int index)
    {
        return CreateVariableTpl<T>((DWORD)index);
    }
    //public T GetVariableTpl<T>(DWORD index);
    public bool SwapVariables(DWORD index1, DWORD index2);
    public DWORD Shrink(DWORD NewSize);
    public string getString(DWORD idx, string def = null);
}
