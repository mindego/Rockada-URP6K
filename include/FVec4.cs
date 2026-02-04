public class FVec4
{
    //float[] v = new float[4];

    //public float a
    //{
    //    get
    //    {
    //        return v[0];
    //    }
    //    set
    //    {
    //        v[0] = value;
    //    }
    //}
    //public float r
    //{
    //    get
    //    {
    //        return v[1];
    //    }
    //    set
    //    {
    //        v[1] = value;
    //    }
    //}
    //public float g
    //{
    //    get
    //    {
    //        return v[2];
    //    }
    //    set
    //    {
    //        v[2] = value;
    //    }
    //}
    //public float b
    //{
    //    get
    //    {
    //        return v[3];
    //    }
    //    set
    //    {
    //        v[3] = value;
    //    }
    //}

    public float a, r, g, b;
    //    union {
    //    struct { float a, r, g, b; };
    //    float v[4];
    //};

    public FVec4() { }
    public FVec4(float _a, float _r, float _g, float _b)
    {
        a = (_a);
        r = (_r);
        g = (_g);
        b = (_b);
    }
    public void Set(float _a, float _r, float _g, float _b) { a = _a; r = _r; g = _g; b = _b; }
    public static FVec4 operator *(FVec4 lhs, float k)
    {
        FVec4 result = new FVec4();
        result.a = lhs.a * k;
        result.r = lhs.r * k;
        result.g = lhs.g * k;
        result.b = lhs.b * k;

        return result;
    }
    //const FVec4& operator *= (float k)              { a*=k, r*=k, g*=k, b*=k; return *this; }
    //    FVec4 operator *(float k)        const { return FVec4(a*k, r*k, g*k, b*k); }
    public int PackARGB() { return ((int)(a * 255) << 24) + ((int)(r * 255) << 16) + ((int)(g * 255) << 8) + (int)(b * 255); }
};