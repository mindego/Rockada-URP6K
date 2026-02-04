using UnityEngine;
using DWORD = System.UInt32;
using WORD = System.UInt16;
using static D3DEMULATION;
using Unity.VisualScripting;



public class FpoMesh
{
    public D3DPRIMITIVETYPE mPrimitiveType;
    public DWORD mFVF;
    public byte[] mVertices;
    public int mNumVertices;
    public WORD[] mIndices;
    public int mNumIndices;


    FpoMesh() { }
    public FpoMesh(D3DPRIMITIVETYPE pt, DWORD fvf, byte[] v, int nv, WORD[] ids, int ni)
    {
        mPrimitiveType = pt;
        mFVF = fvf;
        mVertices = v;
        mNumVertices = nv;
        mIndices = ids;
        mNumIndices = ni;

        for (int i = 0; i != mNumIndices; ++i)
        {
            Asserts.AssertBp(mIndices[i] < mNumVertices);
            if (!(mIndices[i] < mNumVertices)) Debug.LogFormat("Mesh index error {0}/{2} Index {1} >= mNumVertices {3}", i,mIndices[i], mNumIndices, mNumVertices);
        }
    }

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLineFormat("mPrimitiveType: {0}", mPrimitiveType);
        sb.AppendLineFormat("mFVF: {0}", mFVF);
        sb.AppendLineFormat("mVertices: {0} bytes of {1} vertices", mVertices.Length, mNumVertices);
        sb.AppendLineFormat("mIndices: {0} bytes of {1} indices", mIndices.Length, mNumIndices);

        return sb.ToString();
    }
};

