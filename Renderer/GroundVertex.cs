using UnityEngine;
using static D3DEMULATION;
using DWORD = System.UInt32;

public class GroundVertex
{
    public Vector3 pos; //3*4
    public Vector3 norm;//3*4
    public DWORD color;//4
    public DWORD specular;//4
    public Vector2 tc;//2*4
    //TODO - оставить только константой
    //public static DWORD FVF;
    public const DWORD FVF= D3DFVF_XYZ | D3DFVF_NORMAL | D3DFVF_DIFFUSE | D3DFVF_SPECULAR | D3DFVF_TEX1;

    public static int GetSize()
    {
        return (3 * 4 + 3 * 4 + 4 + 4 + 2 * 4); //40
    }

    public override string ToString()
    {
        return "GroundVertex " + pos;
    }
};
