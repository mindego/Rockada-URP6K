using Geometry;
using DWORD = System.UInt32;
using WORD = System.UInt16;
using static Asserts;
using static D3DEMULATION;
using UnityEngine;
//typedef std::vector<TerrExpSphere*, NullPassAllocator<TerrExpSphere*>> SphereExpVector;
//typedef std::vector<TerrExpTube*, NullPassAllocator<TerrExpTube*>> TubeExpVector;

//typedef std::vector<int, NullPassAllocator<int>> BoundArray;
//typedef std::vector<WORD, NullPassAllocator<WORD>> IndexArray;
//typedef std::vector<Vector3f, NullPassAllocator<Vector3f>> VertexArray;
using BoundArray = System.Collections.Generic.List<int>;
using IndexArray = System.Collections.Generic.List<System.UInt16>;
using VertexArray = System.Collections.Generic.List<UnityEngine.Vector3>;

public class TerrExpTube : TerrExpBase
{
    public TerrExpTube()
    {
        mRaster = new RasterizeData(0, 0, null, null);

    }
    public bool Initialize(iTerrain terrain, Line line, float r)
    {
        mLine = line;
        mRadius = r;

        AddRef();
        mDataValid = false;
        return true;
    }

    public override bool CopyVertices(int part, DWORD FVF, Strided pvertices)
    {
        AssertBp(0 == part);
        AssertBp(mDescValid);
        AssertBp(mDataValid);

        if ((FVF & D3DFVF_DIFFUSE) != 0)
        {
            AssertBp(0);
        }

        if ((FVF & D3DFVF_XYZ) != 0)
        {
            AssertBp(mDesc.FVF & D3DFVF_XYZ);
            for (int i = 0; i < mDesc.num_vertices; ++i)
                //pvertices.position.Ref<Vector3>(i) = mVertices[i];
                pvertices.position[i] = mVertices[i];
        }

        if ((FVF & D3DFVF_NORMAL) != 0)
        {
            AssertBp(0);
        }

        return false;
    }
    public override bool CopyIndices(int part, ref WORD[] pindices)
    {
        AssertBp(0 == part);
        AssertBp(mDescValid);
        AssertBp(mDataValid);

        for (int i = 0; i < mDesc.num_indices; ++i)
            pindices[i] = mIndices[i];
        return true;
    }
    public override bool CopyLocation(int part, ref Matrix34f tm)
    {
        tm.Identity();
        return true;
    }

    //private:
    //typedef std::vector<int, NullPassAllocator<int>> BoundArray;
    //typedef std::vector<WORD, NullPassAllocator<WORD>> IndexArray;
    //typedef std::vector<Vector3f, NullPassAllocator<Vector3f>> VertexArray;

    private RasterizeData mRaster;

    private BoundArray mLeft;
    private BoundArray mRight;

    private IndexArray mIndices;
    private VertexArray mVertices;
    private bool mDataValid;

    private Line mLine;
    private float mRadius;

    protected override void RetrieveDesc()
    {
        int max_size = mTerrain.GetMaxRasterSize(mRadius);

        //mLeft.resize(max_size);
        //mRight.resize(max_size);
        mLeft.Capacity = max_size;
        mRight.Capacity = max_size;

        MATRIX mm = new MATRIX();
        mm.SetHorizontal(mLine.dir);
        mm.Org = mLine.org;

        Matrix34f m = Storm.MathConvert.FromLocus(mm);

        m.tm.Scale(new Vector3(mRadius, mRadius, mLine.dist));

        mRaster = mTerrain.GetRaster(m, mLeft.ToArray(), mRight.ToArray());

        mDesc.num_vertices = mTerrain.GetMaxRasterVertices(mRaster);
        mDesc.num_indices = mTerrain.GetMaxRasterIndices(mRaster);
        mDesc.FVF = D3DFVF_XYZ;
        mDesc.prim_type = (uint)D3DPRIMITIVETYPE.D3DPT_TRIANGLELIST;
        RetrieveData();

        mDescValid = true;
    }

    void RetrieveData()
    {
        //mIndices.resize(mDesc.num_indices);
        //mVertices.resize(mDesc.num_vertices);
        mIndices.Capacity = mDesc.num_indices;
        mVertices.Capacity = mDesc.num_vertices;

        //mTerrain.ExportRaster(mRaster, new Stride<Vector3>(mVertices.ToArray(), sizeof(Vector3)), mDesc.num_vertices, mIndices.ToArray(), mDesc.num_indices);
        var tmp = new Stride<Vector3>(mVertices.ToArray(), 0);
        mTerrain.ExportRaster(mRaster, ref tmp, mDesc.num_vertices, mIndices.ToArray(), mDesc.num_indices);

        mDataValid = true;
    }
};