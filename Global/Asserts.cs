//#define DEBUG_ASSET
using System;
using UnityEngine;

internal static class Asserts
{

    public static void Assert(bool expr)
    {
        if (!expr)
        {
            Debug.LogError("Expression failed");
            Debug.Break();
        }
#if DEBUG_ASSERT
        if (!expr) throw new Exception("Expression failed");
#endif

    }

    public static void AssertBp(bool expr)
    {
        if (!expr)
        {
            Debug.LogError("Expression failed");
            Debug.Break();
        }
        //Debugger.Break();
#if DEBUG_ASSERT
        if (!expr) throw new Exception("Expression failed");
        //Debugger.Break();
#endif
    }

    public static void AssertEx(bool expr)
    {
#if DEBUG_ASSET
        if (!expr) throw new Exception("Expression failed");
        //Debugger.Break();
#endif
    }

    public static void AiAssertEx(bool expr, string reason)
    {
        if (!expr) throw new Exception(String.Format("Expression failed due a {0}", reason));
    }

    internal static void AssertExfpo(bool v)
    {
        throw new NotImplementedException();
    }

    internal static void AssertBp(object value)
    {
        AssertBp(value != null);
    }
    internal static void AssertBp(int value)
    {
        AssertBp(value != 0);
    }
}