using UnityEngine;
using DWORD = System.UInt32;

public static class AICommon
{
    public const int DEBUG_NONE = 0;
    public const int DEBUG_MEDIUM = 1;
    public const int DEBUG_HARD = 2;

    public static DWORD mDebugLevel;
    public static void SetDebugLevel(DWORD level) { mDebugLevel = level; }
    public static bool IsLogged(DWORD level) {
        //Debug.Log(string.Format("{0} <= {1}? {2}",level,mDebugLevel, level <= mDebugLevel));
        return (level <= mDebugLevel); 
    }
    

    // messaging
    public static void AiMessage(int type, string title, string frm, params string[] args)
    {
        //Debug.Log("AI Message: "+string.Format(frm, args));
        Parsing.AiMessage(type, title, frm, args);
    }

    const float FEpsilon = 0.01f;
    public static bool CCmp(float x)
    {
         return Mathf.Abs(x) < FEpsilon; 
    }
    public static bool FCmp(float x, float y) { return Mathf.Abs(x - y) < FEpsilon; }

    public static float Rnd(float mn, float bnd) { return mn + RandomGenerator.Rand01() * bnd; }
    public static bool Prb(float mn)
    {
        return RandomGenerator.Rand01() < mn;
    }

    public static void rotatebyangle(ref Vector3 p, float cosb, float sinb)
    {
        float t;
        t = p.x * cosb + p.z * sinb;
        p.z = p.z * cosb - p.x * sinb;
        p.x = t;
    }
}
