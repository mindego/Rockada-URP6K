using UnityEngine;
using Geometry;
using static HashFlags;

public class HashObject : IHashObject
{
    // Each hash object has unique flags set, it selected from hash
    // by logical and operation its` flags and flags_enumerate parameter
    uint flags;

    public HashObject(uint _flags = 0)
    {
        flags = _flags;
    }

    public bool MatchGroup(uint f)
    {
        return GetFlag(OF_GROUP_MASK & f) != 0;
    }
    public bool MatchType(uint f)
    {
        return GetFlag(OF_USER_MASK & f) != 0;
    }
    // this nmethod used by hash system
    public bool MatchFlags(uint f) { return MatchGroup(f) && MatchType(f); }

    public uint GetFlag(uint f) { return flags & f; }
    public uint SetFlag(uint f) { flags |= f; return f; }
    public void ClearFlag(uint f) { flags &= ~f; }
    public uint GetFlags() { return flags; }

    // virtual api section 
    public virtual Line GetLinearData()
    {
        return new Line();
    }

    public virtual geombase.Sphere GetBoundingSphere()
    {
        return new geombase.Sphere(Vector3.zero, 0);
    }

    //public virtual IHashObject GetIHashObject() { return this; }
    public virtual IHashObject GetIHashObject() { throw new System.NotImplementedException("GetIHashObject() should be overriden"); }

    public virtual object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case IHashObject.ID:
                return GetIHashObject();
            default:
                return null;
        }
    }

    public void Dispose() { }
}

