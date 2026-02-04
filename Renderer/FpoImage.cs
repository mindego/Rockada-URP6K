using System;
using UnityEngine;
using static renderer_dll;

public class TWrapObjectIRData<N> : TWrapObject<N, IRData>, IRData where N : IObject
{
    public TWrapObjectIRData(N myObject) : base(myObject) { }

    public N GetObject()
    {
        return mObject;
    }
}

public class FpoImage : IDrawable, IDisposable
{
    /// <summary>
    /// Конструктор вызывается из FpoImageFactory
    /// </summary>
    public FpoImage()
    {
        //mRData = new TWrapObject<FpoImage, IRData>(this);
        mRData = new TWrapObjectIRData<FpoImage>(this);
    }

    //bool Initialize(NotePool<FpoRenderGroup>*, NotePool<FpoRenderableNoClip>* noclip_pool, NotePool<FpoRenderableClip>* clip_pool, FpoGraphData * data);
    public bool Initialize(object group_pool, object noclip_pool, object clip_pool, FpoGraphData im_data)
    {
        //mGroupPool = group_pool;
        //mNoClipPool = noclip_pool;
        //mClipPool = clip_pool;

        graph_data = im_data;
        for (int i = 0; i < 3; ++i)
        {
            lod[i] = new FpoLod();
            MeshData mesh = dll_data.GetFpoMesh(graph_data.lods[i]);
            Debug.LogFormat("MeshData[{0}] {1}",i,mesh);
            if (mesh != null)
            {
                lod[i].Initialize(mesh);
            }
        }
        geombase.Sphere sph = null;
        bool b_sph_valid = false;
        //#warning message "WARN : must be 3 lods "
        for (int i = 0; i < 3; ++i)
        {
            if (lod[i].GetNumGroups() != 0)
            {
                if (b_sph_valid)
                {
                    sph = geombase.Sphere.SphereUnion(sph, lod[i].GetSphereBound());
                }
                else
                {
                    sph = lod[i].GetSphereBound();
                    b_sph_valid = true;
                }
            }
            Debug.LogFormat("Debug init lod {0}: NumGroups: {1} b_sph_valid {2}",i, lod[i].GetNumGroups(), b_sph_valid);
        }

        if (!b_sph_valid)
            return false;


        b_sphere = sph;

        TestVertexData();

        //Asserts.AssertBp(!IsNan(&b_sphere.r));
        //Asserts.AssertBp(!(IsNan(&b_sphere.Org().x) || IsNan(&b_sphere.Org().y) || IsNan(&b_sphere.Org().z)));
        return true;

    }
    public void Destroy() { }

    public IRenderGroup CreateRenderGroup() { return null; }

    public object Query(uint id)
    {
        switch (id)
        {
            case IDrawable.ID:
                AddRef();
                return this;
            case IRData.ID:
                AddRef();
                return mRData;
        }
        ;
        return 0;

    }

    protected FpoGraphData graph_data;
    public FpoLod[] lod = new FpoLod[3];
    protected geombase.Sphere b_sphere;

    TWrapObject<FpoImage, IRData> mRData;

    //NotePool<FpoRenderGroup>* mGroupPool;
    //NotePool<FpoRenderableNoClip>* mNoClipPool;
    //NotePool<FpoRenderableClip>* mClipPool;

    void TestVertexData() { }//TODO возможно нужно.

    //TODO Странное - в оригинальном коде ссылки  нет
    int mRefCount = 1;
    public void AddRef()
    {
        mRefCount++;
    }

    public int Release()
    {
        return (--mRefCount);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

}
