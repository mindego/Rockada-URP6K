using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.PackageManager;
using UnityEngine;
using DWORD = System.UInt32;

public struct DWORDARGB
{
    public byte b;
    public byte g;
    public byte r;
    public byte a;

    DWORD color
    {
        get
        {
            return BitConverter.ToUInt32(new byte[] { a, r, g, b }, 0);
        }
        set
        {
            byte[] bitarray = BitConverter.GetBytes(value);
            a = bitarray[0];
            r = bitarray[1];
            g = bitarray[2];
            b = bitarray[3];
        }
    }

    public DWORDARGB(int _a, int _r, int _g, int _b)
    {
        b = (byte)_b;
        g = (byte)_g;
        r = (byte)_r;
        a = (byte)_a;
    }
    public DWORDARGB(DWORD _color)
    {
        byte[] bitarray = BitConverter.GetBytes(_color);
        a = bitarray[0];
        r = bitarray[1];
        g = bitarray[2];
        b = bitarray[3];
    }

    public DWORDARGB(Vector4 c)
    {
        b = (byte)(int)c[0];
        g = (byte)(int)c[1];
        r = (byte)(int)c[2];
        a = (byte)(int)c[3];
    }

    public Color ToColor()
    {
        return new Color32(r, g, b, a);
    }

    public static implicit operator DWORD(DWORDARGB b) => b.color;
    //    public DWORDARGB(Vec4<T> c)   b(c.b),g(c.g),r(c.r),a(c.a)
    //    { }
    //    //operator DWORD&()     { return color; }
    //    template<class T>
    //  operator Vec4<T>()
    //{ return Vec4<T>(r, g, b, a); }
    //};
}