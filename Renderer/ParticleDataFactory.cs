using UnityEngine;
using crc32 = System.UInt32;
using static renderer_dll;
using System;
public class ParticleDataImpl : IParticleData,IDisposable
{
    public ParticleDataImpl()
    {
        mData = null;
        mTexture = null;
    }
    public void Destroy()
    {
        mTexture = null;

    }
    public bool Initialize(PARTICLE_DATA data)
    {
        mData = data;

        //mTexture = dll_data.LoadTexture(new string(mData.TextureName));
        mTexture = dll_data.LoadTexture(new string(mData.GetTextureName()));
        return true;
    }

    public PARTICLE_DATA GetPARTICLE_DATA()
    {
        return (PARTICLE_DATA)mData;
    }
    public Texture2D GetTexture()
    {
        return mTexture;
    }

    int mRefCount = 0;
    public void AddRef()
    {
        mRefCount++;
    }

    public int RefCount()
    {
        return mRefCount;
    }

    public int Release()
    {
        return mRefCount--;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public crc32 mCode;

    protected PARTICLE_DATA mData;
    Texture2D mTexture;
}
public class ParticleDataFactory : IndexFactory<crc32,ParticleDataImpl>,IDisposable
{
    //В IndexFactory Object объявляется как T, так что в этом случае это ParticleDataImpl
    //typedef T Object;
    public void SetData(crc32 code)
    {
        mCode = code;
    }
    public override bool InitializeObject(ParticleDataImpl myobject)
    {
        PARTICLE_DATA data = dll_data.GetParticleData(mCode);
        if (data == null) return false;

        myobject.mCode = mCode;

        return myobject.Initialize(data);
    }
    void DestroyObject(ParticleDataImpl myobject)
    {
        myobject.Destroy();
        //delete object;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    protected crc32 mCode;
};