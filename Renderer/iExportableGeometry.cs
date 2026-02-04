using DWORD = System.UInt32;
using WORD = System.UInt16;
//using PrimType = D3DPRIMITIVETYPE;
using PrimType = System.UInt32;
using UnityEngine;

using geombase;
using Geometry;
public class Strided
{
    const int max_texcoords = 8;

    public Stride<Vector3> position;
    public Stride<Vector3> normal;
    public Stride<DWORD> diffuse;
    public Stride<Vector3> specular;
    public Stride<Vector2>[] texcoords = new Stride<Vector2>[max_texcoords];

    private int num_vertices;
    public Strided(int _num_vertices=0)
    {
        num_vertices = _num_vertices;
    }
    public Vertex[] ExportVertices()
    {
        Vertex tmpVertex;
        Vertex[] result = new Vertex[num_vertices];
        for (int i=0;i<num_vertices;i++)
        {
            tmpVertex = new Vertex
            {
                norm = normal[i],
                pos = position[i],
                color = diffuse[i]
            };
            result[i] = tmpVertex;
        }
        return result;
    }
};

public struct MeshDesc
{
    public DWORD FVF;
    public PrimType prim_type;
    public int num_vertices;
    public int num_indices;
};

public interface IMesh : IObject
{
    public int GetNumParts();
    public MeshDesc GetPartDesc(int part);
    public bool CopyVertices(int part, DWORD FVF, Strided pvertices);//size of pvertices array should be >=num_vertices
    public bool CopyIndices(int part, ref WORD[] pindices); //size of pindices  array should be >=num_indices
    public bool CopyLocation(int part, ref Matrix34f m);//false if identity
};

public interface IMeshExporter : IObject
{
    public IMesh Export(Sphere s);//exports sphered 
    public IMesh Export(Line axis, float radius);//exports inf. tubed
};

public struct MeshPart
{
    public int n_v;
    public int n_i;
    public Vertex[] v;
    public WORD[] i;
}