using geombase;
using UnityEngine;
using static Asserts;
using static D3DEMULATION;
using DWORD = System.UInt32;
using WORD = System.UInt16;

public class TerrExpSphere : TerrExpBase
{
    public override bool CopyVertices(int part, DWORD FVF, Strided pvertices)
    {
        AssertBp(0 == part);
        AssertBp(mDescValid);

        if ((FVF & D3DFVF_DIFFUSE) != 0)
        {
            AssertBp(mDesc.FVF & D3DFVF_DIFFUSE);

            //int* src_l = (int*)alloca(sizeof(int) * mDesc.num_vertices);
            //int[] src_l = Alloca.ANewN<int>(mDesc.num_vertices);
            DWORD[] src_l = Alloca.ANewN<DWORD>(mDesc.num_vertices);

            //AssertBp(sizeof(DWORDARGB) == 4);

            //mTerrain.ExportColors(mRect, new Stride(src_l, sizeof(int)));
            Stride<DWORD> stride = new Stride<DWORD>(src_l, 4);
            mTerrain.ExportColors(mRect, ref stride);
            for (int i = 0; i < mDesc.num_vertices; ++i)
            {
                //int c = Mathf.Clamp(src_l[i], 0, 255);
                int c = (int) Mathf.Clamp(stride[i], 0, 255);
                //pvertices.diffuse.Ref<DWORD>(i) = new DWORDARGB(0, c, c, c);
                pvertices.diffuse[i] = new DWORDARGB(0, c, c, c);
            }
        }

        if ((FVF & D3DFVF_XYZ)!=0)
        {
            AssertBp(mDesc.FVF & D3DFVF_XYZ);
            mTerrain.ExportVertices(mRect, ref pvertices.position);
        }

        if ((FVF & D3DFVF_NORMAL)!= 0)
        {
            AssertBp(mDesc.FVF & D3DFVF_NORMAL);
            mTerrain.ExportNormals(mRect, ref pvertices.normal);
        }

        return true;
    }
    public override bool CopyIndices(int part, ref WORD[] pindices)
    {
        AssertBp(0 == part);
        AssertBp(mDescValid);

        mTerrain.ExportTriangles(mRect, ref pindices);

        return true;
    }
    public override bool CopyLocation(int part, ref Matrix34f tm )
    {
        tm.Identity();
        return false;
    }
    private float[] mArea = new float[4];

    geombase.Rect mRect;

    protected override void RetrieveDesc()
    {
        mRect = mTerrain.ClipRect(mTerrain.GetRect(mArea));

        int n_vtx = mTerrain.RectVtxCount(mRect);
        int n_tri = mTerrain.RectTriCount(mRect);

        mDesc.FVF = D3DFVF_XYZ | D3DFVF_NORMAL | D3DFVF_DIFFUSE | D3DFVF_TEX1;
        mDesc.num_indices = n_tri;
        mDesc.num_vertices = n_vtx;
        mDesc.prim_type = (uint)D3DPRIMITIVETYPE.D3DPT_TRIANGLELIST;

        mDescValid = true;
    }

    public bool Initialize(iTerrain terrain, Sphere sphere)
    {
        return Initialize(terrain, sphere.Org().x, sphere.Org().z, sphere.Radius());
    }
    public bool Initialize(iTerrain terrain, float x, float z, float r)
    {
        mTerrain = terrain;
        mArea[0] = x - r;
        mArea[1] = z - r;
        mArea[2] = x + r;
        mArea[3] = z + r;

        AddRef();
        return true;
    }
};
