using geombase;
using Geometry;
using System;
using UnityEngine;
using static HashFlags;
using static RoFlags;
using DWORD = System.UInt32;
using WORD = System.UInt16;

public class Decal : IObject, IDecal, IDisposable
{
    protected int mRefCount;
    DecalEss<Decal> mEssence;
    DecalHashed<Decal> mHashed;
    DecalControl<Decal> mControl;

    public Matrix34f mLocation = new Matrix34f();

    int mLayering;
    protected Sphere mBoundingSphereLocal = new Sphere();
    protected Sphere mBoundingSphereWorld = new Sphere();
    protected Line mLinearData;
    protected bool mChangeApplyed;

    protected DWORD mFVF;
    public int mNumVertices;
    public Vertex[] pmVertices;
    public int mNumIndices;
    public WORD[] pmIndices;

    protected Strided mVerticesStrided;

    protected Vector3 mMin;
    protected Vector3 mMax;
    public Decal()
    {
        mEssence = new DecalEss<Decal>(this);
        mHashed = new DecalHashed<Decal>(OF_GROUP_RENDER | ROFID_DECAL, this);
        mControl = new DecalControl<Decal>(this);
        mLayering = 0;
        //pmVertices(0),
        //pmAlpha(0),
        //pmIndices(0),
        mChangeApplyed = true;

        mRefCount = 1;

    }

    public void ApplyChange()
    {
        //TODO Реализовать при необходимости применение изменений в декалях
        //Vertex* v = (Vertex*)pmVertices;

        //int a = Clamp<int>(mColor.a * 256, 0, 255);
        //int r = Clamp<int>(mColor.r * mColor.a * 256, 0, 255);
        //int g = Clamp<int>(mColor.g * mColor.a * 256, 0, 255);
        //int b = Clamp<int>(mColor.b * mColor.a * 256, 0, 255);

        //for (int i = 0; i < mNumVertices; ++i)
        //{
        //    v[i].diffuse = DWORDARGB((pmAlpha[i] * a) >> 8, (pmAlpha[i] * r) >> 8, (pmAlpha[i] * g) >> 8, (pmAlpha[i] * b) >> 8);
        //}

        mChangeApplyed = true;
    }
    public void AddRef()
    {
        mRefCount++;
    }

    public IHashObject GetHashObject()
    {
        return mHashed;
    }

    public int GetLayering()
    {
        return mLayering;
    }

    public int Release()
    {
        Asserts.AssertBp(mRefCount);
        mRefCount--;
        if (mRefCount != 0)
            return mRefCount;
        this.Dispose();
        return 0;
    }

    public void SetColor(Color32 color)
    {
        throw new System.NotImplementedException();
    }

    public void SetLayering(int l)
    {
        mLayering = l;
    }

    public virtual void Dispose()
    {
        //STUB
    }

    public Sphere GetBoundingSphere()
    {
        return mBoundingSphereWorld;
    }
    public Line GetLinearData() { return mLinearData; }

    public object Query(uint iid)
    {
        switch (iid)
        {
            case IDecal.ID:
                {
                    IDecal ret = mControl;
                    ret.AddRef();
                    return ret;
                }
            case IHashObject.ID:
                {
                    IHashObject ret = mHashed;
                    return ret;
                }
            case IDecalEss.ID:
                {
                    IDecalEss ret = mEssence;
                    ret.AddRef();
                    return ret;
                }

        }
        return null;
    }

    public virtual void Draw()
    {
        Debug.Log("Drawing Decal:" + this);
    }
}

public class DecalEss<D> : TWrapObject<D, IDecalEss>, IDecalEss where D : Decal
{
    public DecalEss(D decal) : base(decal) { }

    public int GetLayering()
    {
        return mObject.GetLayering();
    }

    public void Draw()
    {
        mObject.Draw();
    }
};

public interface IDecalEss : IObject
{
    new public const uint ID = 0xFF13DF72;
    public int GetLayering();
    public void Draw();
};

/// <summary>
/// Implementation of Decal object renderer's interface 
/// </summary>
public class DecalControl<D> : IDecal where D : Decal
{

    public DecalControl(D decal)
    {
        mDecal = decal;
    }

    //IDecal interface implementation
    public void SetColor(Color32 color)
    {
        mDecal.SetColor(color);
    }

    public void SetLayering(int l)
    {
        mDecal.SetLayering(l);
    }

    public int GetLayering()
    {
        return mDecal.GetLayering();
    }

    public IHashObject GetHashObject()
    {
        return mDecal.GetHashObject();
    }

    public void AddRef()
    {
        mDecal.AddRef();
    }

    public int Release()
    {
        return mDecal.Release();
    }

    public object Query(uint cls_id)
    {
        return mDecal.Query(cls_id);
    }
    private D mDecal;
};