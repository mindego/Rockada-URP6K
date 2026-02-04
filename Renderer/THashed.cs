//using NonHashList = System.Collections.Generic.List<IHashObject>;
using Geometry;

public class THashed<T> : IHashObject where T : LightObject
{
    private IHashObject myHashObject;
    public THashed(uint flags, T myobject)
    {
        myHashObject = new HashObject(flags);
        //IHashObject(flags);
        mObject = myobject;
    }
    ~THashed() { }

    //IHashObject interface implementation
    public geombase.Sphere GetBoundingSphere()
    {

        return mObject.GetBoundingSphere();
    }

    public object Query(uint cls_id)
    {
        return mObject.Query(cls_id);
    }

    public uint GetFlag(uint f)
    {
        return myHashObject.GetFlag(f);
    }

    public uint SetFlag(uint f)
    {
        return myHashObject.SetFlag(f);
    }

    public void ClearFlag(uint f)
    {
        myHashObject.ClearFlag(f);
    }

    public bool MatchGroup(uint f)
    {
        return myHashObject.MatchGroup(f);
    }

    public bool MatchType(uint f)
    {
        return myHashObject.MatchType(f);
    }

    public bool MatchFlags(uint f)
    {
        return myHashObject.MatchFlags(f);
    }

    public Line GetLinearData()
    {
        return myHashObject.GetLinearData();
    }

    public IHashObject GetIHashObject()
    {
        //return myHashObject.GetIHashObject();
        return this;
    }

    public uint GetFlags()
    {
        return myHashObject.GetFlags();
    }

    public void Dispose()
    {
        myHashObject.Dispose();
    }

    private T mObject;
}
