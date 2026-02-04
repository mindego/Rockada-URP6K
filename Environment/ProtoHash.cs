using geombase;
using UnityEngine;

public class LayerInc 
{
    int p;
    public LayerInc(ref int _p)
    {
        p = _p;
        p++;
    }
    ~LayerInc() { Dispose(); }
    public void Dispose()
    {
        p--;
    }
};
public class ProtoHash
{
    protected const int MAX_HENUM_VALUE = 3;
    public const uint ID = 0xBA4B5DBE;
    public int mDim;
    protected float square_size, oo_square_size;
    float size, oo_size;
    int mOffset;
    public geombase.Rect rect;
    protected int[] enumc = new int[MAX_HENUM_VALUE];
    protected int enum_layer;

    // const inline methods
    public int GetHashX(float x) { return (int)((x * oo_square_size - 0.5)) + mOffset; }
    public int GetHashY(float y) { return (int)((y * oo_square_size - 0.5)) + mOffset; }
    public int GetRealX(int x) { return x + mOffset; }
    public int GetRealY(int y) { return y + mOffset; }


    bool In(float x, float y) { return rect.In(GetHashX(x), GetHashY(y)); }

    int Min(int x) { return 0 < x ? x : 0; }
    int Max(int x) { return mDim > x ? x : (mDim - 1); }

    public int GetSizeX() { return mDim; }
    public int GetSizeY() { return mDim; }

    //было protected 
    public float GetBaseX() { return square_size * (rect.x0 - mOffset); }
    public float GetBaseY() { return square_size * (rect.y0 - mOffset); }
    public float GetBaseX2() { return square_size * (rect.x1 + mOffset); }
    public float GetBaseY2() { return square_size * (rect.y1 + mOffset); }

    public float GetSquareSize() { return square_size; }
    public float GetOOSquareSize() { return oo_square_size; }

    public geombase.Rect GetRect() { return rect; }

    //Rect GetValidRect(Rect p) { return rect & p; }
    public geombase.Rect GetValidRect(geombase.Rect p)
    {
        return rect & p;
    }
    public geombase.Rect GetRect(Sphere sp)
    {
        return new geombase.Rect(GetHashX(sp.o.x - sp.r), GetHashY(sp.o.z - sp.r), GetHashX(sp.o.x + sp.r), GetHashY(sp.o.z + sp.r));
    }

    public int CountDim(float sz, float sq) { return (int)(sz / sq); }
    public ProtoHash()
    {
        rect = new geombase.Rect(0, 0, -128, -128);
        mDim = 0;
        square_size = 1;
        oo_square_size = 1;
        size = 0;
        oo_size = 1;
        mOffset = 0;
    }
    public void InitProtoHash(int _dim, float sq_size, int offs)
    {
        mDim = _dim;
        square_size = sq_size;
        oo_square_size = 1f / sq_size;
        size = sq_size * mDim;
        oo_size = 1f / size;
        for (int i = 0; i < MAX_HENUM_VALUE; ++i) enumc[i] = 0;
        enum_layer = -1;
        mOffset = offs;
        rect = new geombase.Rect(0, 0, mDim - 1, mDim - 1);
    }


};

