public class SVec4
{
    public int a, r, g, b;
    public SVec4() { }
    public SVec4(int _r, int _g, int _b)
    {

        a = (0); r = (_r); g = (_g); b = (_b);
    }
    public SVec4(int _a, int _r, int _g, int _b) { a = (_a); r = (_r); g = (_g); b = (_b); }
    public SVec4(int c) { a = ((c >> 24) & 0xff); r = ((c >> 16) & 0xff); g = ((c >> 8) & 0xff); b = ((c >> 0) & 0xff); }
    public SVec4(SVec4 c0, float d0, SVec4 c1, float d1)
    {
        Median(c0, d0, c1, d1);
    }
    //    SVec4& operator += (SVec4  &s)       { a+=s.a; r += s.r; g += s.g; b += s.b; return *this; }
    //SVec4 & operator -= (SVec4  &s) { a -= s.a; r -= s.r; g -= s.g; b -= s.b; return *this; }
    //SVec4 operator *(SVec4  &s) { return SVec4(a * s.a, r * s.r, g * s.g, b * s.g); }
    //SVec4 operator *(int m) { return SVec4(a * m, r * m, g * m, b * m); }
    //SVec4 operator /(int m) { return SVec4(a / m, r / m, g / m, b / m); }
    //SVec4 operator +(SVec4  &s) { return SVec4(a + s.a, r + s.r, g + s.g, b + s.b); }
    //SVec4 operator -(SVec4  &s) { return SVec4(a - s.a, r - s.r, g - s.g, b - s.b); }
    //SVec4 operator >>(int m) { return SVec4(a >> m, r >> m, g >> m, b >> m); }

    public void Set(int _r, int _g, int _b, int _a) { a = _a; r = _r; g = _g; b = _b; }
    public int PackRGB() { return (r << 16) + (g << 8) + b; }
    public void Max() { if (r > 255) r = 255; if (g > 255) g = 255; if (b > 255) b = 255; }
    public int PackRGBMax() { return ((r < 255) ? r << 16 : 255 << 16) + ((g < 255) ? g << 8 : 255 << 8) + ((b < 255) ? b : 255); }
    public int PackARGB() { return (a << 24) + (r << 16) + (g << 8) + b; }
    public int PackARGBMax() { return ((a < 255) ? a << 24 : 255 << 24) + ((r < 255) ? r << 16 : 255 << 16) + ((g < 255) ? g << 8 : 255 << 8) + ((b < 255) ? b : 255); }
    void Median(SVec4 V1, float f1, SVec4 V2, float f2)
    {
        int if1 = (int)(f1 * 128.0f), if2 = (int)(f2 * 128.0f);
        a = ((int)(V1.a) * if1 + (int)(V2.a) * if2) >> 7;
        r = ((int)(V1.r) * if1 + (int)(V2.r) * if2) >> 7;
        g = ((int)(V1.g) * if1 + (int)(V2.g) * if2) >> 7;
        b = ((int)(V1.b) * if1 + (int)(V2.b) * if2) >> 7;
    }
};
