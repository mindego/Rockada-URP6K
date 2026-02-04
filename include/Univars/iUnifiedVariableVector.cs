using UnityEngine;
using DWORD = System.UInt32;
// *********************************************************************************************************
// iUnifiedVariableVector - хранение VECTOR
public interface iUnifiedVariableVector : iUnifiedVariable
{
    new public const DWORD ID = 0x2AF9C65D;
    public Vector3 GetValue();
    public Vector3 SetValue(Vector3 value);
};
