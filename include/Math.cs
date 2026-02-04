using geombase;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using DWORD = System.UInt32;


namespace Storm
{
    public static class MathConvert
    {
        public static Matrix34f FromLocus(MATRIX locus)
        {
            return new Matrix34f(new Matrix3f(locus.Right, locus.Up, locus.Dir), locus.Org);
        }
        public static MATRIX ToLocus(Matrix34f tr)
        {
            return new MATRIX(tr.pos, tr.tm[2], tr.tm[1], tr.tm[0]);
        }
    }
    public static class Math
    {
        //const float DEGTORAD = 0.01745329251994329547f;
        //const float RADTODEG = 57.29577951308232286465f;
        //public const float OO2 = 1 / 2;
        //public const float OO256 = 1 / 256;
        //public const float OO32K = 1 / 0x8000;
        //public const float OO64K = 1 / 0x10000;

        public const float PI = 3.14159265358979323846f;
        public const float PI_2 = 6.28318530717958623200f;
        public const float PI_DIV_2 = 1.57079632679489655800f;
        public const float PI_DIV_4 = 0.78539816339744827900f;
        public const float INV_PI = 0.31830988618379069122f;
        public const float DEGTORAD = 0.01745329251994329547f;
        public const float RADTODEG = 57.29577951308232286465f;

        public const float PI2 = 6.28318530717958623200f;
        public const float PI5 = 1.57079632679489655800f;
        public const float OO2 = 1f / 2f;
        public const float OO256 = 1f / 256f;
        public const float OO32K = 1f / 0x8000f;
        public const float OO64K = 1f / 0x10000f;
        public const float FLT_EPSILON = 1.192092896e-07F; //float.Epsilon?
        public const float PRECISION = FLT_EPSILON;


        public static float fov(float a, float r = 1) { return 2 * Mathf.Atan2(r, 2 * a); }
        public static float aspect(float f, float r = 1) { return r / (Mathf.Atan(f * 0.5f) * 2); }

        public static float SphereAspect(float d, float r = 1) { return Mathf.Sqrt(d * d - r * r) / (2 * r); }
        public static float SphereAspect2(float d2, float r = 1) { return d2 > r * r ? Mathf.Sqrt(d2 - r * r) / (2 * r) : 0; }

        public static float SphereFov(float d, float r = 1) { return 2 * Mathf.Atan2(r, Mathf.Sqrt(d * d - r * r)); }

        public static float GRD2RD(float k) { return k * DEGTORAD; }
        public static float RD2GRD(float k) { return k * RADTODEG; }

        public static Vector2 getSinCos(float a)
        {
            Vector2 res = new Vector2();
            SinCos(a, out res.x, out res.y);

            return res;
        }
        public static void SinCos(float a, out float psin, out float pcos)
        {
            //psin = Mathf.Sin(a);
            //pcos = Mathf.Cos(a);
            Unity.Mathematics.math.sincos(a, out double tmpsin, out double tmpcos);
            psin = (float)tmpsin;
            pcos = (float)tmpcos;
        }
        public static float NormaFAbs(Vector3 vector)
        {
            return Mathf.Abs(vector.x) + Mathf.Abs(vector.y) + Mathf.Abs(vector.z);
        }


        public static float KPH2MPS(float kph) { return (kph * 0.2778f); }
        public static float MPS2KPH(float mps) { return (mps * 3.6f); }

        public static bool solveSquare(float a, float b, float c, out Vector2 solve)
        {
            solve = new Vector2();
            if (a == 0)
            {
                if (b == 0) return false;
                solve.x = solve.y = -c / b;
                return true;
            }

            float d = b * b - 4f * a * c;
            if (d < 0) return false;
            double s1 = 0.5f / a;
            double s2 = Mathf.Sqrt(d);
            solve.x = (float)((-b + s2) * s1);
            solve.y = (float)((-b - s2) * s1);
            return true;
        }

        internal static int ClampIntWrapped(int src, int min, int max, int coeff)
        {
            if (src < min) return min + ((min - src) & (coeff - 1));
            else if (src >= max) return max - coeff + ((src - max) & (coeff - 1));

            return src;
        }

        internal static Color Median(Color32 V1, float f1, Color32 V2, float f2)
        {
            int a, r, g, b;
            int if1 = (int)(f1 * 128.0f), if2 = (int)(f2 * 128.0f);

            a = ((int)V1.a * if1 + (int)V2.a * if2) >> 7;
            r = ((int)V1.r * if1 + (int)V2.r * if2) >> 7;
            g = ((int)V1.g * if1 + (int)V2.g * if2) >> 7;
            b = ((int)V1.b * if1 + (int)V2.b * if2) >> 7;
            //return new Color(r/255, g/255, b/255, a/255);
            return new Color32((byte)r, (byte)g, (byte)b, (byte)a);
        }

        public static bool isInRange(int x, int min, int max)
        {
            return min <= x && x <= max;
        }

        public static bool isInRange(float x, float min, float max)
        {
            return min <= x && x <= max;
        }

        public static double xGauss(double rand0_1)
        {
            return System.Math.Sqrt(-2.0f * System.Math.Log(rand0_1));
        }

        public static Vector2 gauss(double rnd0_1a, double rnd0_1b)
        {
            //static const float max_norm = 4.15625;
            const float max_norm = 4.15625f;
            double r = xGauss(rnd0_1a);
            if (r > max_norm)
                r = xGauss(1 - rnd0_1a);
            //return getSinCos<double>(Mathf.Pi * 2.0f * rnd0_1b) * r;
            return getSinCos(Mathf.PI * 2.0f * (float)rnd0_1b) * (float)r;
        }

        //const float FEpsilon = 0.001f;
        //public static bool CCmp(float x) { return Mathf.Abs(x) < FEpsilon; }

        public static bool IsNan(float f)
        {
            return (((int)f) & 0x7F800000) == 0x7F800000;
        }

        public static bool isNan(float v)
        {
            return IsNan(v);
        }

        public static float getNan()
        {
            //union {
            //    float f;
            //    uint d;
            //};
            //d = 0x7F800000;
            //return f;
            return 0;
        }

        public static bool isVectorOk(Vector3 norm)
        {
            return !isNan(norm.x) && !isNan(norm.y) && !isNan(norm.z);
        }

        internal static int Index2D(int x, int z)
        {
            return (x & 0xffff) + (z << 16);
        }

        public static float GetSimpleC(float d, float Diff, float baseval = 1f)
        {
            if (d < .0f) return baseval;
            baseval -= d * Diff;
            return (baseval < .0f ? .0f : baseval);

        }

        public static float GetOneOverC(float V, float Vc, float Vmin = .0f, float Vmax = 1f)
        {
            return (V <= .0f ? Vmax : Mathf.Clamp(Vc / V, Vmin, Vmax));
        }

        public static Vector2 Rotate2d(Vector2 v, Vector2 sincos)
        {
            //u=x,v=y
            return new Vector2(v.x * sincos.y + v.y * sincos.x, v.y * sincos.y - v.x * sincos.x);
        }

        internal static bool cmpFloat(float x, float y, float precision)
        {
            return Mathf.Abs(x - y) < precision;
        }

        internal static float Pi()
        {
            return Mathf.PI;
        }

        internal static float frndint(float x)
        {
            return (float)((int)(x));
        }

        static int fliper = 0;
        static float next;
        public static float norm_rand()
        {
            fliper = 1 - fliper;
            if (fliper != 0)
            {
                Vector2 pair = gauss(RandomGenerator.Rand01(), RandomGenerator.Rand01());
                next = pair.x;
                return pair.y;
            }
            else
                return next;
            //return Rand01();
        }
    }
}

public static class Distr
{
    static DISTRIBUTION instance;

    private static DISTRIBUTION getInstance()
    {
        if (instance == null) instance = new DISTRIBUTION();
        return instance;
    }

    public static Vector3 Sphere() { return getInstance().Sphere(); }
    public static float Gauss() { return getInstance().Gauss(); }
    public static Vector2 Circle() { return getInstance().Circle(); }
}
public class DISTRIBUTION
{
    const int RANDOM_TABLE_SIZE = 1024;
    float[] norm_tbl = new float[RANDOM_TABLE_SIZE];
    float[] sphr_tbl = new float[RANDOM_TABLE_SIZE * 3];
    float[] crcl_tbl = new float[RANDOM_TABLE_SIZE * 2];

    int nd, cd, sd;

    int NextNd() { return ++nd & (RANDOM_TABLE_SIZE - 1); }

    int NextSd() { return (++sd & (RANDOM_TABLE_SIZE - 1)) * 3; }

    int NextCd() { return (++cd & (RANDOM_TABLE_SIZE - 1)) * 2; }


    public DISTRIBUTION()
    {
        //Randomize();
        Reset();
        //FnInit();
        //float nt = norm_tbl[0];
        //float ct = crcl_tbl[0];
        //float st = sphr_tbl[0];
        int nt = 0;
        int ct = 0;
        int st = 0;
        for (int i = 0; i < RANDOM_TABLE_SIZE; nt++, ct += 2, st += 3, i++)
        {
            norm_tbl[nt] = Storm.Math.norm_rand();

            Vector2 sc = Storm.Math.getSinCos(Storm.Math.PI_2 * RandomGenerator.Rand01());

            //ct[0] = sc.u;
            //ct[1] = sc.v;
            crcl_tbl[ct] = sc.x;
            crcl_tbl[ct + 1] = sc.y;

            float
              s1 = Storm.Math.norm_rand(),
              s2 = Storm.Math.norm_rand(),
              s3 = Storm.Math.norm_rand(),
              sr = 1 / new Vector3(s1, s2, s3).magnitude;

            //st[0] = s1 * sr;
            //st[1] = s2 * sr;
            //st[2] = s3 * sr;
            sphr_tbl[st] = s1 * sr;
            sphr_tbl[st + 1] = s2 * sr;
            sphr_tbl[st + 2] = s3 * sr;
        }
    }

    //void Reset() { nd = rand_u32(sd = rand_u32(cd = lclock())); }
    void Reset() { }
    public float Gauss() { return norm_tbl[NextNd()]; }
    public Vector3 Gauss3D() { return new Vector3(Gauss(), Gauss(), Gauss()); }
    public Vector3 Sphere() { int p = NextSd(); return new Vector3(sphr_tbl[p], sphr_tbl[p + 1], sphr_tbl[p + 2]); }
    public Vector2 Circle() { int p = NextCd(); return new Vector2(crcl_tbl[p], crcl_tbl[p + 1]); }

};

public class Prof
{
    int time;
    int count;
    public Prof()
    {
        count = 0;
        time = 0;
    }
    public Prof(int cnt)
    {
        count = cnt;
        time = 0;
    }
    public void Reset() { count = 0; time = 0; }
    public void Start() { time -= (int)lclock(); }
    public void End() { time += (int)lclock() - 36; }
    public void AddRef(int num = 1) { count += num; }
    public int Avrg() { return count != 0 ? time / count : time; }
    public int Time() { return time; }
    public int Count() { return count; }

    public static long lclock()
    {
        return Stopwatch.GetTimestamp();
    }
};