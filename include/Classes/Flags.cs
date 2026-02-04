using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class Flags : ISizeEvaluator
{
     int flags;

    public static bool operator < (Flags left, Flags right)
    {
        return left.flags < right.flags;
    }

    public static bool operator >(Flags left, Flags right)
    {
        return left.flags > right.flags;
    }
    public Flags(int flags)
    {
        this.flags = flags;
    }

    public Flags() : this(0)
    {
    }

    public int Get(int f)  
    {
        return flags & f; 
    }

    public uint Get(uint f)
    {
        return (uint)Get((int)f);
    }
    public void Set(int f, bool st = true) 
    {
        flags = st ? flags | f : flags & ~f;
    }

    public void Set(uint f, bool st = true)
    {
        Set((int)f, st);
    }

    public void set(int f)
    {
        Set(f, true);
    }
    public void clear(int f)
    {
        Set(f, false);
    }

    public int get(int f)
    {
        return Get(f);
    }
  
    public int GetFlags()
    {
        return flags;
    }

    public int ToInt()
    {
        return flags;
    }
    public override string ToString()
    {
        return "Flags: " + flags.ToString("X8");
    }

    public static int GetSize()
    {
        return sizeof(int);
    }
}
