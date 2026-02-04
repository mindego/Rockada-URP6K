using System;
using UnityEngine;

public class StormUnityRendererIndexedVB
{
    public void Draw(IDirect3DVertexBuffer7 vb, int num_vtx, ushort[] idxs, int num_idx, int start_vtx)
    {
        Mesh mesh = new Mesh();
        mesh.Clear();

        Debug.LogFormat("Creating mesh data VB {0} {1}",vb.GetHashCode().ToString("X8"),start_vtx);
        Vector3[] vertices = new Vector3[num_vtx];
        Vector3[] normals = new Vector3[num_vtx];
        GroundVertex gv;
        for (int i=start_vtx,j=0;j<num_vtx; i++,j++)
        {
            gv = (GroundVertex)vb.data[i];
            vertices[j] = gv.pos;
            normals[j] = gv.norm;
            //Debug.LogFormat("Vertex: {0} Normal {1} [{2}+{3}]", vertices[j], normals[j], start_vtx,j);
        }
        mesh.vertices = vertices;
        mesh.normals = normals;
        gv=(GroundVertex)vb.data[start_vtx];
        //Debug.LogFormat("Drawing num_vtx {0} num_idx {1} start_vtx {2} GroundVertices.Length {3}", num_vtx, num_idx, start_vtx, vb.data.Length);
        var tMatrix = Matrix4x4.Translate(Engine.ToCameraReference(gv.pos));
        GameObject proxy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        proxy.transform.position =Engine.ToCameraReference(gv.pos);

        var rp = new RenderParams(MaterialStorage.DefaultSolid);
        Graphics.RenderMesh(rp, mesh,0,tMatrix);
        
    }
}
