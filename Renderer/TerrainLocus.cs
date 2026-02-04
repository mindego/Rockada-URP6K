using System;
using UnityEngine;
using DWORD = System.UInt32;

public class TerrainLocus : ITerrainLocus
{
    DWORD GetIndex() { return m_Index; }
    public Matrix34f GetTransform() { return m_Locus; }
    public Matrix34f GetTransformInv() { return m_LocusInv; }
    public void Initialize(int bx, int bz, float x, float z)
    {
        m_Index = (uint) Storm.Math.Index2D(bx, bz);
        m_Locus=new Matrix34f();
        m_Locus.tm.Identity();
        m_Locus.pos = new Vector3(x, 0, z);
        m_LocusInv = new Matrix34f();
        m_LocusInv.tm.Identity();
        m_LocusInv.pos = new Vector3(-x, 0, -z);

    }

    uint ITerrainLocus.GetIndex()
    {
        return GetIndex();
    }

    public void AddRef()
    {
        throw new NotImplementedException();
    }

    public int Release()
    {
        throw new NotImplementedException();
    }

    private Matrix34f m_Locus;
    private Matrix34f m_LocusInv;
    private DWORD m_Index;
};