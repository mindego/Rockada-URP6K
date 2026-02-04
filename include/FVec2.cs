using Unity.VisualScripting;

public class FVec2
{
    float u, v;
    public FVec2() { }
    public FVec2(float _u, float _v)
    {
        u = _u;
        v = _v;
    }
    //FVec2 operator *= (float a) { u *= a; v *= a; return this; }
    //FVec2 operator /= (float a) { u /= a; v /= a; return this; }
    //FVec2 operator += (FVec2 t) { u += t.u; v += t.v; return this; }
    //FVec2 operator -= (FVec2 t) { u -= t.u; v -= t.v; return this; }
    //FVec2 operator *(FVec2 t) { return new FVec2(u * t.u, v * t.v); }
    //FVec2 operator *(float a) { return new FVec2(u * a, v * a); }
    //FVec2 operator /(float a) { return new FVec2(u / a, v / a); }
    //FVec2 operator +(FVec2 t) { return new FVec2(u + t.u, v + t.v); }
    //FVec2 operator -(FVec2 t) { return new FVec2(u - t.u, v - t.v); }

    public static FVec2 operator +(FVec2 lhs,FVec2 rhs)
    {
        FVec2 result = new FVec2();
        result.u = lhs.u + rhs.u;
        result.v = lhs.v + rhs.v;
        return result;
    }
    public static FVec2 operator *(FVec2 lhs, FVec2 rhs)
    {
        FVec2 result = new FVec2();
        result.u = lhs.u * rhs.u;
        result.v = lhs.v * rhs.v;
        return result;
    }
    public static FVec2 operator *(FVec2 fVec, float a)
    {
        FVec2 result = new FVec2();
        result.u = fVec.u * a;
        result.v = fVec.v * a;
        return result;
    }
    public static FVec2 operator / (FVec2 fVec2, float a) {
        FVec2 result = new FVec2(); 
        result.u = fVec2.u/a;
        result.v = fVec2.v/a;
        return result;
        }

    FVec2 RoundInt() { return new FVec2(Storm.Math.frndint(u), Storm.Math.frndint(v)); }
};