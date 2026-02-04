using UnityEngine;
using DWORD = System.UInt32;

public class DummyHasher : IHash
{
    ProtoHash protohash;
    public DummyHasher() { }

    public virtual IHashInfo GetHashInfo()
    {
        return new IHashInfo();
    }
    public virtual void SetSecondCache(DWORD mask) { }
    public virtual HMember RemoveMember(HMember Info) { return Info; }
    public virtual HMember UpdateMember(HMember Info) { return Info; }
    public virtual HMember RemoveMemberByLine(HMember Info) { return Info; }
    public virtual HMember UpdateMemberByLine(HMember Info) { return Info; }
    public virtual HMember UpdateMemberStatic(HMember Info) { return Info; }
    public virtual HMember RemoveMemberStatic(HMember Info) { return Info; }


    public virtual object Query(uint cls_id)
    {
        switch ((uint)cls_id)
        {
            case IHash.ID:
                return GetThis();
            case ProtoHash.ID:
                return protohash;
            default:
                return null;
        }

    }

    public virtual int EnumPoly(RasterizeData r, uint _flags, HashEnumer e) { return 0; }
    public virtual int EnumRect(geombase.Rect    _rt      , uint _flags, HashEnumer e) { return 0; }
    public virtual int EnumSphere(geombase.Sphere  _sp      , uint _flags, HashEnumer e) { return 0; }
    public virtual int EnumLine(Geometry.Line    _ln      , uint _flags, HashLineEnumer _e) { return 0; }
    public virtual int EnumSphereRough(geombase.Sphere  _sp , int _flags, HashEnumer e) { return 0; }
    public IHash GetThis() { return this; }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }
}

