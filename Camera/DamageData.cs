using System;

public class DamageData : TLIST_ELEM<DamageData>, IDisposable
{
    public int colour;
    public float[] xy;
    public float[] txy;
    public IBObject BillObj;
    //public IObject BillObj;
    public string name;
    public DamageData(float[] _xy, float[] _txy, string _name = null)
    {
        colour = 0;
        BillObj = null;
        xy = _xy;
        txy = _txy;
        name = _name;
    }
    ~DamageData()
    {
        if (BillObj != null) BillObj.Release();
    }

    DamageData prev, next;


    public DamageData Next()
    {
        return next;
    }

    public DamageData Prev()
    {
        return prev;
    }

    public void SetNext(DamageData t)
    {
        next = t;
    }

    public void SetPrev(DamageData t)
    {
        prev = t;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}