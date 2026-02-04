using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.LightTransport;

public static class Alloca
{
    //#define ANew(type)    new (_alloca(sizeof(type))) type
    //#define ANewN(type,n) new (_alloca(sizeof(type)*(n))) type
    public static T ANew<T>() where T:new()
{
        return new T();
    }
    public static T[] ANewN<T>(int n) where T : new()
    {
        T[] tmpArray = new T[n];
        for (int i = 0; i < n; i++)
        {
            tmpArray[i] = new T();
        }
        return tmpArray;
    }
}

public static class stdmem
{
    //public static void b_zero(object[] d, int s) { int* i = (int*)d; s >>= 2; while (s--) *i++ = 0; }

    public static void bzero(ref object[] d, int s)
    {
        //MemSet(d, 0, s); 
    }

    public static void bzero<T>(ref T[] d, int s, T def_value)
    {
        for (int i = 0; i < s; i++)
        {
            d[i] = def_value;
        }
    }

    //public static object[] MemAlloc(uint s) { return new char[s]; }
    public static T[] MemAlloc<T>(uint s) { return new T[s]; }

    public static void MemFree(object[] p)
    {
        //TODO надо бы обдумать, как это правильно очишать.
        //delete p;
    }
}
