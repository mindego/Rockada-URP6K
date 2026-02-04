using System;
using D3DVBuffer = IDirect3DVertexBuffer7;
using DWORD = System.UInt32;
using static D3DEMULATION;
using UnityEngine;

/// <summary>
/// Vertex buffer
/// </summary>
public class VBuffer : IRefMem
{
    protected DWORD FVF;
    protected DWORD Caps;
    protected D3DVBuffer vb;
    GroundVertex[] locked_data;
    Invalidator invalidator;

    public VBuffer()
    {
        vb = null;
        locked_data = null;
    }
    ~VBuffer()
    {
        if (vb != null)
        {
            vb.Release();
            vb = null;
        }
    }
    public D3DVBuffer GetVB()
    { return vb; }

    public object GetDataPtr()
    {//returns locked data pointer 
        return locked_data;
    }

    public V GetData<V>()
    {// same as GetDataPtr but returns typized data pointer
        //AssertBp(V::FVF);
        return (V)GetDataPtr();
    }

    public V[] GetDataFromIndex<V>(int startIndex) where V:class
    {
        Asserts.AssertBp(startIndex < locked_data.Length);

        int newArraySize = locked_data.Length - startIndex;
        V[] tmp = new V[newArraySize];

        //Array.Copy((V[])locked_data, startIndex, tmp, 0, newArraySize);
        int i, j;

        for (i = startIndex, j = 0; i < locked_data.Length; i++, j++)
        {
            UnityEngine.Debug.LogFormat("Casting {0} -> {1}", tmp.GetType().ToString(), locked_data[i].GetType().ToString());
            tmp[j] = locked_data[i] as V;
        }
        return tmp;
    }

    public void SaveDataFromIndex<V>(V[] data,int startIndex) where V:GroundVertex
    {
        for (int i=0;i<data.Length;i++)
        {
            Debug.LogFormat("Saving to {0} array from {1} array {2} {3}", locked_data==null?"Error": locked_data[startIndex + i],data==null? "Error" : data[i],startIndex,i);
            locked_data[startIndex+i] = data[i];
        }
        vb.data = locked_data;
    }

    public void Lock(DWORD flags)
    {
        int ignore = 0;
        HRESULT hr = vb.Lock<GroundVertex>(flags, out locked_data, ref ignore);
        Debug.LogFormat("locked_data is: {0}", locked_data==null?"null": locked_data);
#if _DEBUG
    if(FAILED(hr)){
      Log->Message("Error : Can't lock vertex buffer : %s!",DDErr(hr));
      AssertBp(0);
    }
#endif
    }
    public void Unlock()
    {
        HRESULT hr = vb.Unlock();
#if _DEBUG
    if(FAILED(hr)){
      Log->Message("Error : Can't unlock vertex buffer : %s!",DDErr(hr));
      AssertBp(0);
    }
#endif
        locked_data = null;
    }

    public void setInvalidateHook(Invalidator hook)
    {
        invalidator = hook;
    }

    void invalidate()
    {
        invalidator.invalidate();
    }

    public void AddRef()
    {
        throw new NotImplementedException();
    }

    public int Release()
    {
        throw new NotImplementedException();
    }
};

/// <summary>
/// Эмуляция IDirect3DVertexBuffer7
/// </summary>
public class IDirect3DVertexBuffer7 : IDirect3DResource7
{
    private D3DVERTEXBUFFERDESC myDesc;
    public object[] data;
    //public Span<object> data;
    private int Size;
    public D3DVERTEXBUFFERDESC GetDesc()
    {
        return myDesc;
    }
    public HRESULT Lock() { return HRESULT.S_OK; }
    public HRESULT Unlock() { return HRESULT.S_OK; }

    internal HRESULT Lock<T>(uint flags, out T[] locked_data, ref int v) where T : class,new()
    {
        //UnityEngine.Debug.LogFormat("Lock is set with flags {0} size {1}", flags.ToString("X8"), myDesc.dwSize);
        Size = (int)myDesc.dwNumVertices;
        if (data == null)
        {
            CreateData<T>();
        }
        
        locked_data = (T[])data;
        v = Size;
        return Lock();
    }

    private void CreateData<T>() where T:new()
    {
        UnityEngine.Debug.LogFormat("Creating data for Vertex Buffer {0} items", Size);
        data = new object[Size];
        for (int i = 0; i < Size; i++)
        {
            data[i] = new T();
        }
    }

    internal HRESULT Lock<T>(uint flags, out Span<T> locked_data, ref int v) where T : class, new()
    {
        //UnityEngine.Debug.LogFormat("Lock is set with flags {0} size {1}", flags.ToString("X8"), myDesc.dwSize);
        Size = (int)myDesc.dwNumVertices;
        if (data == null)
        {
            CreateData<T>();
        }
        locked_data = (T[])data;
        v = Size;
        return Lock();
    }
    public IDirect3DVertexBuffer7(D3DVERTEXBUFFERDESC desc)
    {
        myDesc = desc;
    }

}

public class IDirect3DResource7 : IUnknown
{

}

public class IUnknown
{
    private int Refcount;
    public int AddRef()
    {
        return ++Refcount;
    }
    public int Release()
    {
        return --Refcount;
    }
    //IUnknown::QueryInterface

    //A helper function template that infers an interface identifier, and calls QueryInterface(REFIID, void).
    //IUnknown::QueryInterface

    //Retrieves pointers to the supported interfaces on an object.

}