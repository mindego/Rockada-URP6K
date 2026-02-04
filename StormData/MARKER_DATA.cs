using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// MARKER_DATA - представление маркера для ИИ
/// </summary>
public class MARKER_DATA : AIData
{
    public DWORD ID;
    public float x, z, Radius;
    public string TextName;

    public uint GetID()
    {
        return ID;
    }

    // создание/удаление
    public void Init(iUnifiedVariableVector pVector,string pName)
    {
        //Storm.CRC32 crc = new Storm.CRC32();
        //ID = crc.HashString(pName);
        ID= Hasher.HshString(pName);
        TextName = pName;
        Vector3 v = pVector.GetValue();
        x = v.x;
        z = v.z;
        Radius = v.y;
    }
}
