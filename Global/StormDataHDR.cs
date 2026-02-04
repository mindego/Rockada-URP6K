using System;
using UnityEngine;

public static class StormDataHDR 
{
    public static bool RSLV<T>(ref T v) where T:STORM_DATA
    {
        //Debug.Log("RSLV b: " + v);
        //Debug.Log("RSLV b: " + v.Data());

        //Debug.Log("RSLV a:" + v);
        //TODO реализовать корректно функцию RSLV
        return true;
    }

    /*
     * #define RSLV(v,t)   { if (v) v=(t*)t::GetByCode((unsigned int)v); }
#define RSLVLE(v,t) { if (v->Data()) v->SetData((void*)t::GetByCode((unsigned int)v->Data())); }

     * */
}