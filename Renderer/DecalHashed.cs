using geombase;
using Geometry;
//using NonHashList = System.Collections.Generic.List<IHashObject>;
//using NonHashList = System.Collections.Generic.List<HashObjectCont>;
/// <summary>
/// Implementation of Decal object interface with HashTools
/// </summary>
/// <typeparam name="D"></typeparam>
public class DecalHashed<D> : IHashObject where D: Decal
{
    private IHashObject ho;
    public DecalHashed(uint flags, D decal) {
        ho = new HashObject(flags);
        mDecal = decal;
    }
    ~DecalHashed() { Dispose(); }

    public void Dispose() { }

    //IHashObject interface implementation
    public Sphere GetBoundingSphere()
    {
        return mDecal.GetBoundingSphere();
    }
    public Line GetLinearData() { return mDecal.GetLinearData(); }


    public object Query(uint cls_id)
    {
        return mDecal.Query(cls_id);
    }

    public uint GetFlag(uint f)
    {
        return ho.GetFlag(f);
    }

    public uint SetFlag(uint f)
    {
        return ho.SetFlag(f);
    }

    public void ClearFlag(uint f)
    {
        ho.ClearFlag(f);
    }

    public bool MatchGroup(uint f)
    {
        return ho.MatchGroup(f);
    }

    public bool MatchType(uint f)
    {
        return ho.MatchType(f);
    }

    public bool MatchFlags(uint f)
    {
        return ho.MatchFlags(f);
    }

    public IHashObject GetIHashObject()
    {
        return ho.GetIHashObject();
    }

    public uint GetFlags()
    {
        return ho.GetFlags();
    }

    private D mDecal;
};
