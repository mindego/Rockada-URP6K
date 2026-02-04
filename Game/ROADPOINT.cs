using System;
using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// точка маршрута
/// </summary>
public class ROADPOINT
{

    public override string ToString()
    {
        return string.Format("ROADPOINT {0} {1}", Pnt, Flags.ToString("X8"));
    }
    public DWORD Flags;
    public Vector3 Pnt;
    // работа с флагами
    public void SetFlag(DWORD Flag) { Flags |= Flag; }
    public void ClearFlag(DWORD Flag) { Flags &= ~Flag; }
    public DWORD GetFlag(DWORD Flag) { return Flags & Flag; }

    public ROADPOINT() { Pnt = Vector3.zero; Flags = 0; }
    public ROADPOINT(Vector3 pnt) { Pnt = pnt; Flags = 0; }
};
