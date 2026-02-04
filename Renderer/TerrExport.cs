using geombase;
using Geometry;
using System.Collections.Generic;
using static Asserts;
//typedef std::vector<TerrExpSphere*, NullPassAllocator<TerrExpSphere*>> SphereExpVector;
//typedef std::vector<TerrExpTube*, NullPassAllocator<TerrExpTube*>> TubeExpVector;
using SphereExpVector = System.Collections.Generic.List<TerrExpSphere>;
using TubeExpVector = System.Collections.Generic.List<TerrExpTube>;

//typedef std::vector<int, NullPassAllocator<int>> BoundArray;
//typedef std::vector<WORD, NullPassAllocator<WORD>> IndexArray;
//typedef std::vector<Vector3f, NullPassAllocator<Vector3f>> VertexArray;


class TerrExport : IMeshExporter
{
    public TerrExport()
    {
        mInitialized = false;
        mTerrain = null;
        mRefCount = 1;
    }

    public bool Initialize(iTerrain terrain)
    {
        AssertBp(mInitialized == false);
        mTerrain = terrain;
        mInitialized = true;
        return true;
    }

    public IMesh Export(Sphere sphere)
    {
        TerrExpSphere mesh = CreateSphereExporter();
        if (!mesh.Initialize(mTerrain, sphere))
        {
            mesh.Release();
            return null;
        }
        return mesh;
    }

    public IMesh Export(Line line, float radius)
    {
        if ((line.dir.x == 0) && (line.dir.z == 0))
        {
            TerrExpSphere mesh = CreateSphereExporter();
            if (!mesh.Initialize(mTerrain, line.org.x, line.org.z, radius))
            {
                return null;
            }
            return mesh;
        }
        else
        {
            TerrExpTube mesh = CreateTubeExporter();
            if (!mesh.Initialize(mTerrain, line, radius))
            {
                return null;
            }
            return mesh;
        }
    }

    public void AddRef()
    {
        mRefCount++;
    }
    public int Release()
    {
        AssertBp(mRefCount);
        mRefCount--;
        if (mRefCount != 0)
            return mRefCount;
        this.Dispose();
        return 0;
    }

    private iTerrain mTerrain;
    private bool mInitialized;
    private int mRefCount;

    //typedef std::vector<TerrExpSphere*, NullPassAllocator<TerrExpSphere*>> SphereExpVector;
    //typedef std::vector<TerrExpTube*, NullPassAllocator<TerrExpTube*>> TubeExpVector;
    SphereExpVector mSphereExporters = new SphereExpVector();
    TubeExpVector mTubeExporters= new TubeExpVector();



    TerrExpSphere CreateSphereExporter()
    {
        return CreateObjectInArray<TerrExpSphere>(ref mSphereExporters);
    }
    TerrExpTube CreateTubeExporter()
    {
        return CreateObjectInArray<TerrExpTube>(ref mTubeExporters);
    }

    ~TerrExport()
    {
        Dispose();
    }

    private bool isDisposed = false;
    public void Dispose()
    {
        if (isDisposed) return;
        DeleteObjectsArray(mSphereExporters);
        DeleteObjectsArray(mTubeExporters);
        isDisposed = true;
    }

    public static T CreateObjectInArray<T>(ref List<T> cont) where T : TerrExpBase, new()
    {
        T obj = null;
        for (int i = 0; i < cont.Count; ++i)
        {
            if (!cont[i].IsInUse())
            {
                obj = cont[i];
                break;
            }
        }
        if (obj == null)
        {
            obj = new T();
            cont.Add(obj);
        }
        return obj;
    }

    public static void DeleteObjectsArray<T>(List<T> cont) where T : TerrExpBase
    {
        for (int i = 0; i < cont.Count; ++i)
        {
            AssertBp(!cont[i].IsInUse());
            cont[i].Dispose();
            cont[i] = null;
        }
    }
};
