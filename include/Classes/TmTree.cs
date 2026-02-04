using geombase;
using Geometry;
using System;
using UnityEngine.Assertions;
using static HashFlags;
/// <summary>
/// Transformation tree                                                        |
/// </summary>
public abstract class TmTree : HashedTm, IHashObject //: LTree<HashedTm>
{
    // renderer flags  ( 2 pass  )
    public const uint TMTF_PASSES = 0x00F00000;

    public const uint TMTF_PASS1 = 0x00100000;
    public const uint TMTF_PASS2 = 0x00200000;
    public const uint TMTF_ST_PASS1 = 0x00400000;
    public const uint TMTF_ST_PASS2 = 0x00800000;

    // object identifiction :
    public const uint TMTID_PRIVATE = 0x00000001;
    public const uint TMTID_FPO = 0x00000002;


    public const uint ID = 0x8EC889BF;

    //uint flags;

    //public override bool MatchGroup(uint f)
    //{
    //    return GetFlag(OF_GROUP_MASK & f) != 0;
    //}
    //public bool MatchType(uint f) { return GetFlag(OF_USER_MASK & f) != 0; }
    //// this nmethod used by hash system
    //public bool MatchFlags(uint f) { return MatchGroup(f) && MatchType(f); }

    //public uint GetFlag(uint f) { return flags & f; }
    //public uint SetFlag(uint f) { flags |= f; return f; }
    //public void ClearFlag(uint f) { flags &= ~f; }

    public TmTree(uint TmtId)
    {
        RefCount = 1;
        SetFlag(HashFlags.TMTObjectId(TmtId));
    }

    int RefCount;  // reference count
    public override void AddRef()
    {
        ++RefCount;
    }
    public override int Release()
    {
        Assert.IsTrue(RefCount > 0);
        if ((--RefCount) == 0)
        {
            //for (TmTree::iterator i = Begin(); i;)
            //{
            //    TmTree::node node = i.Node(); ++i;
            //    node->Release();
            //}
            Destroy();
            //delete this;
            Dispose();
            return 0;
        }
        return RefCount;
    }
    public abstract void Destroy();     // pseudo destructor


    public override object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case ID: return (TmTree)this;
            default: return base.Query(cls_id);
        }
    }
    public TmTree parent, sub_tree;
    public TmTree prev, next;
    public TmTree Begin()
    {
        return sub_tree;
    }
    
    public TmTree Next() { return next; }
    public TmTree Prev() { return prev; }
    public void Detach()
    {
        Asserts.Assert(parent != null);
        if (this == parent.sub_tree) parent.sub_tree = next;
        if (next != null) next.prev = prev;
        if (prev != null) prev.next = next;
        parent = next = prev = null;
    }

}

/// <summary>
/// Hashed matrix with own hash radius
/// </summary>

public abstract class HashedTm : IHashObject, IObject
{
    public Matrix34f tm;
    public float h_radius;

    /// <summary>
    /// эмуляция множественного наследования
    /// </summary>
    private IHashObject myHashObject;

    public HashedTm()
    {

        myHashObject = new HashObject();
        h_radius = 0;
    }

    float HashRadius() { return h_radius; }

    public virtual Sphere GetBoundingSphere()
    {
        throw new NotImplementedException();
        //return new Sphere(tm.pos, h_radius);
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

    public virtual object Query(uint cls_id)
    {
        return myHashObject.Query(cls_id);
    }

    public virtual void Dispose()
    {
        myHashObject.Dispose();
    }

    public abstract void AddRef();
    public abstract int Release();
}

//public class LTChildIterator
//{

//    TmTree _node;
//    public LTChildIterator(TmTree first)
//    {
//        _node = first;
//    }
//    public LTChildIterator(LTChildIterator i)
//    {
//        _node = i._node;
//    }
//    public static LTChildIterator operator ++(LTChildIterator operand)
//    {
//        operand._node = operand._node.next;
//        return operand;
//    }
//    //LTree<I>* operator *() { return _node; }
//    //I*    operator ->   () const { return _node; }


//    TmTree Node() { return _node; }

//    LTChildIterator Next() { return _node.next; }
//    LTChildIterator Prev() { return _node.prev; }
//};