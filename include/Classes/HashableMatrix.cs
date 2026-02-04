using Geometry;
public abstract class HashableMatrix : MATRIX, IHashObject
{
    float HashRadius;
    private IHashObject ho;

    public abstract void Dispose();
    public HashableMatrix(uint fl = 0)
    {
        ho = new HashObject(fl);
        HashRadius = 0;
    }

    public void ClearFlag(uint f)
    {
        ho.ClearFlag(f);
    }

    public geombase.Sphere GetBoundingSphere() { return new geombase.Sphere(Org, HashRadius); }
    public IHashObject GetHashObject() { return this; }

    public uint GetFlag(uint f)
    {
        return ho.GetFlag(f);
    }

    public uint GetFlags()
    {
        return ho.GetFlags();
    }

    public IHashObject GetIHashObject()
    {
        return this;
    }

    public Line GetLinearData()
    {
        return ho.GetLinearData();
    }

    public bool MatchFlags(uint f)
    {
        return ho.MatchFlags(f);
    }

    public bool MatchGroup(uint f)
    {
        return ho.MatchGroup(f);
    }

    public bool MatchType(uint f)
    {
        return ho.MatchType(f);
    }

    public object Query(uint cls_id)
    {
        return ho.Query(cls_id);
    }

    public uint SetFlag(uint f)
    {
        return ho.SetFlag(f);
    }

}


