using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Asserts;
using static D3DEMULATION;
using static renderer_dll;
using DWORD = System.UInt32;
using WORD = System.UInt16;


public class DebugMeshMover: MonoBehaviour
{
    private Vector3 myPos;

    public void SetPos(Vector3 pos)
    {
        myPos = pos;
    }

    public void FixedUpdate()
    {
        this.transform.position = Engine.ToCameraReference(myPos);
    }
}
public partial class PolyDecal : Decal
{
    const float FLT_MAX = float.MaxValue;
    const float FLT_MIN = float.MinValue;
    public PolyDecal() : base()
    {
        mTexture = null;
        mMaterial = null;
        //mRState = null;
    }


    public override void Dispose()
    {
        //IRefMem.SafeRelease(mTexture);
        //IRefMem.SafeRelease(mRState);
        base.Dispose();
    }

    public PolyDecalData myData;


    //private void DebugExportMesh(IMesh mesh, geombase.Plane[] cplanes, int nplanes)
    //{
    //    int i, part;

    //    int num_parts = mesh.GetNumParts();
    //    Matrix3f tm_fromworld = new Matrix3f();
    //    mLocation.tm.GetTranspose(ref tm_fromworld);

    //    MeshPart[] parts = Alloca.ANewN<MeshPart>(num_parts);

    //    Vector3 v_min, v_max;
    //    EngineDebug.DebugConsoleFormat("PolyDecal parts {0}",num_parts);
    //    for (part = 0; part < num_parts; ++part)
    //    {
    //        MeshDesc desc = mesh.GetPartDesc(part);
    //        AssertBp(desc.FVF & D3DFVF_XYZ);

    //        Vertex[] src_v = Alloca.ANewN<Vertex>(desc.num_vertices);
    //        WORD[] src_i = Alloca.ANewN<WORD>(desc.num_indices);
    //        Matrix34f tm = new Matrix34f();

    //        Strided strided = new Strided(desc.num_vertices);
    //        strided.position = new Stride<Vector3>(new Vector3[desc.num_vertices], desc.num_vertices);
    //        strided.normal = new Stride<Vector3>(new Vector3[desc.num_vertices], desc.num_vertices);
    //        strided.diffuse = new Stride<DWORD>(new DWORD[desc.num_vertices], desc.num_vertices);

    //        mesh.CopyVertices(part, D3DFVF_XYZ | D3DFVF_NORMAL | D3DFVF_DIFFUSE, strided);
    //        mesh.CopyIndices(part, ref src_i);
    //        mesh.CopyLocation(part, ref tm);

    //        src_v = strided.ExportVertices();
    //        EngineDebug.DebugConsoleFormat("src_v: {0},example: {1}",src_v.Length, src_v.Length > 0? src_v[0]:"no vertices");

    //        Vector3 ofs_to_local = tm.pos - mLocation.pos;
    //        Matrix3f tm_to_local = new Matrix3f();
    //        tm_to_local.Multiply(tm.tm, tm_fromworld);

    //        Matrix34f tm_res = new Matrix34f(tm_to_local, Matrix3f.Vector3Matrix3fMultiply(ofs_to_local, tm_fromworld));
    //        {
    //            v_min = new Vector3(FLT_MAX, FLT_MAX, FLT_MAX);
    //            v_max = new Vector3(-FLT_MAX, -FLT_MAX, -FLT_MAX);

    //            for (i = 0; i < desc.num_vertices; ++i)
    //            {
    //                src_v[i].pos = tm_res.TransformPoint(src_v[i].pos);
    //                //v_min = Select(v_min, src_v[i].pos, std::less<float>());
    //                //v_max = Select(v_max, src_v[i].pos, std::greater<float>());
    //                v_min = Select(v_min, src_v[i].pos, std_less);
    //                v_max = Select(v_max, src_v[i].pos, std_greater);

    //            }
    //        }

    //        //now vertices in decal's local system ( data.location )
    //        //clip them here

    //        //parts[part].v=(Vertex*) alloca(sizeof(Vertex)*(((desc.num_indices+2)/3)*(3+6)));
    //        //parts[part].i=(WORD*) alloca(sizeof(WORD  )*(((desc.num_indices+2)/3)*64));
    //        parts[part].v = Alloca.ANewN<Vertex>(((desc.num_indices + 2) / 3) * (3 + 6));
    //        parts[part].i = Alloca.ANewN<WORD>(((desc.num_indices + 2) / 3) * 64);

    //        ClipIndexed(cplanes, nplanes,
    //                    src_v, desc.num_vertices, src_i, desc.num_indices,
    //                    ref parts[part].v, ref parts[part].n_v, ref parts[part].i, ref parts[part].n_i);
    //    }
    //}

    public bool Initialize(IMeshExporter mesh_exp, PolyDecalData data)
    {
        if (mesh_exp == null) return false;
        myData = data;
        geombase.Plane[] cplanes = new geombase.Plane[6];
        int nplanes;

        float minx = data.nodes[0].x, minz = data.nodes[0].z, maxx = data.nodes[0].x, maxz = data.nodes[0].z;
        int i, part;
        for (i = 0; i < data.numnodes; ++i)
        {
            minx = Mathf.Min(minx, data.nodes[i].x);
            minz = Mathf.Min(minz, data.nodes[i].z);
            maxx = Mathf.Max(maxx, data.nodes[i].x);
            maxz = Mathf.Max(maxz, data.nodes[i].z);
        }

        mLocation.pos = new Vector3((minx + maxx) * .5f, 0, (minz + maxz) * .5f);
        mLocation.tm.Identity();

        for (i = 0; i < data.numnodes; ++i)
        {
            int j = i + 1; j = j >= data.numnodes ? 0 : j;
            Vector3 n = Vector3.Cross(Vector3.up, (data.nodes[j] - data.nodes[i]));
            n.Normalize();
            cplanes[i] = new geombase.Plane(-new Vector3(n.x, n.y, n.z), Vector3.Dot(n, (data.nodes[i] - mLocation.pos)));
        }

        nplanes = data.numnodes;

        float r = new Vector2((maxx - minx) * .5f, (maxz - minz) * .5f).magnitude;
        Vector3 dir = new Vector3(0, -1, 0);
        IMesh mesh = mesh_exp.Export(new Geometry.Line(mLocation.pos - dir * 10000, dir, 20000), r);
        if (mesh == null) return false;
        
        int num_parts = mesh.GetNumParts();
        Matrix3f tm_fromworld = new Matrix3f();
        mLocation.tm.GetTranspose(ref tm_fromworld);

        MeshPart[] parts = Alloca.ANewN<MeshPart>(num_parts);

        Vector3 v_min, v_max;

        for (part = 0; part < num_parts; ++part)
        {
            MeshDesc desc = mesh.GetPartDesc(part);
            AssertBp(desc.FVF & D3DFVF_XYZ);

            //Vertex* src_v = (Vertex*)alloca(sizeof(Vertex) * desc.num_vertices);
            //WORD* src_i = (WORD*)alloca(sizeof(WORD) * desc.num_indices);
            Vertex[] src_v = Alloca.ANewN<Vertex>(desc.num_vertices);
            WORD[] src_i = Alloca.ANewN<WORD>(desc.num_indices);
            Matrix34f tm = new Matrix34f();

            Strided strided = new Strided(desc.num_vertices);
            //strided.position = new Stride<Vector3>(src_v, sizeof(Vertex), offsetof(Vertex, pos));
            //strided.normal = new Stride<Vector3>(src_v, sizeof(Vertex), offsetof(Vertex, norm));
            //strided.diffuse = new Stride<DWORD>(src_v, sizeof(Vertex), offsetof(Vertex, color));
            strided.position = new Stride<Vector3>(new Vector3[desc.num_vertices], desc.num_vertices);
            strided.normal = new Stride<Vector3>(new Vector3[desc.num_vertices], desc.num_vertices);
            strided.diffuse = new Stride<DWORD>(new DWORD[desc.num_vertices], desc.num_vertices);

            mesh.CopyVertices(part, D3DFVF_XYZ | D3DFVF_NORMAL | D3DFVF_DIFFUSE, strided);
            mesh.CopyIndices(part, ref src_i);
            mesh.CopyLocation(part, ref tm);

            src_v = strided.ExportVertices();

            Vector3 ofs_to_local = tm.pos - mLocation.pos;
            Matrix3f tm_to_local = new Matrix3f();
            tm_to_local.Multiply(tm.tm, tm_fromworld);

            Matrix34f tm_res = new Matrix34f(tm_to_local, Matrix3f.Vector3Matrix3fMultiply(ofs_to_local, tm_fromworld));
            {
                v_min = new Vector3(FLT_MAX, FLT_MAX, FLT_MAX);
                v_max = new Vector3(-FLT_MAX, -FLT_MAX, -FLT_MAX);

                for (i = 0; i < desc.num_vertices; ++i)
                {
                    src_v[i].pos = tm_res.TransformPoint(src_v[i].pos);
                    //v_min = Select(v_min, src_v[i].pos, std::less<float>());
                    //v_max = Select(v_max, src_v[i].pos, std::greater<float>());
                    v_min = Select(v_min, src_v[i].pos, std_less);
                    v_max = Select(v_max, src_v[i].pos, std_greater);

                }
            }

            //now vertices in decal's local system ( data.location )
            //clip them here

            //parts[part].v=(Vertex*) alloca(sizeof(Vertex)*(((desc.num_indices+2)/3)*(3+6)));
            //parts[part].i=(WORD*) alloca(sizeof(WORD  )*(((desc.num_indices+2)/3)*64));
            parts[part].v = Alloca.ANewN<Vertex>(((desc.num_indices + 2) / 3) * (3 + 6));
            parts[part].i = Alloca.ANewN<WORD>(((desc.num_indices + 2) / 3) * 64);

            //DebugClipIndexed(cplanes, nplanes, src_v, src_i);
            ClipIndexed(cplanes, nplanes,
                        src_v, desc.num_vertices, src_i, desc.num_indices,
                        ref parts[part].v, ref parts[part].n_v, ref parts[part].i, ref parts[part].n_i);
        }
        
        int res_numv = 0, res_numi = 0;
        for (i = 0; i < num_parts; ++i)
        {
            res_numv += parts[i].n_v;
            res_numi += parts[i].n_i;
        }

        mesh.Release(); mesh = null;
        if (res_numv == 0)
            return false;


        InitVisualData(data);


        AssertBp(res_numi);

        Vertex[] pvertices = new Vertex[res_numv];
        WORD[] pindices = new WORD[res_numi];

        Vector3 tg_offs = mLocation.pos - data.tc_base;

        v_min = new Vector3(FLT_MAX, FLT_MAX, FLT_MAX);
        v_max = new Vector3(FLT_MIN, FLT_MIN, FLT_MIN);
        int cur_sv = 0, cur_si = 0;
        for (part = 0; part < num_parts; ++part)
        {
            for (int vi = 0; vi < parts[part].n_v; ++vi)
            {
                Vertex v = parts[part].v[vi];

                Vector3 gv = Matrix3f.Vector3Matrix3fMultiply((v.pos + tg_offs), data.tc_gen);
                v.tc = new Vector2(gv.x, gv.z);

                DWORDARGB s_clr = new DWORDARGB(v.color);
                v.color = new DWORDARGB(
                255,
                (int)Mathf.Clamp(s_clr.r * mMaterial.diffuse.r, 0, 255),
                (int)Mathf.Clamp(s_clr.g * mMaterial.diffuse.g, 0, 255),
                (int)Mathf.Clamp(s_clr.b * mMaterial.diffuse.b, 0, 255));

                pvertices[cur_sv + vi] = v;


                //v_min = Select(v_min, v.pos, std::less<float>());
                //v_max = Select(v_max, v.pos, std::greater<float>());
                v_min = Select(v_min, v.pos, std_less);
                v_max = Select(v_max, v.pos, std_greater);
            }

            for (int ii = 0; ii < parts[part].n_i; ++ii)
            {
                pindices[cur_si + ii] = (WORD)(cur_sv + parts[part].i[ii]);
            }

            cur_sv += parts[part].n_v;
            cur_si += parts[part].n_i;
        }

        mFVF = Vertex.FVF;

        mNumVertices = res_numv;
        mNumIndices = res_numi;
        pmVertices = pvertices;
        pmIndices = pindices;

        mMin = v_min;
        mMax = v_max;

        mBoundingSphereLocal.o = (mMin + mMax) * .5f;
        mBoundingSphereLocal.r = ((mMax - mMin) * .5f).magnitude;

        mBoundingSphereWorld.o = mLocation.TransformPoint(mBoundingSphereLocal.o);
        mBoundingSphereWorld.r = mBoundingSphereLocal.r;//mLocation.TransformV((mMax-mMin)*.5).Norma();
        mLinearData = data.linear;

        return true;
    }


    private bool debugged = false;
    private void DebugClipIndexed(geombase.Plane[] cplanes, int nplanes, Vertex[] src_v, WORD[] src_i)
    {
        if (debugged) return;

        GameObject pdecal = new GameObject();
        pdecal.name = "Decal " + pdecal.GetHashCode().ToString("X8") + "@" + mLocation.pos;
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Color32> colors = new List<Color32>();
        foreach (Vertex v in src_v)
        {
            Vector3 node = v.pos;
            vertices.Add(node);
            byte a = (byte)((v.color >> 24) & 0xFF);
            byte r = (byte)((v.color >> 16) & 0xFF);
            byte g = (byte)((v.color >> 8) & 0xFF);
            byte b = (byte)((v.color) & 0xFF);

            colors.Add(new Color32(r,g,b,a));

            GameObject nodeVis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nodeVis.name = "Node " + node;
            nodeVis.transform.parent = pdecal.transform;
            nodeVis.transform.localPosition = node;
        }
        mesh.SetVertices(vertices);
        mesh.SetIndices(src_i,MeshTopology.Triangles,0);
        mesh.SetColors(colors);
        

        MeshRenderer mr= pdecal.AddComponent<MeshRenderer>();
        MeshFilter mf = pdecal.AddComponent<MeshFilter>();
        DebugMeshMover mv = pdecal.AddComponent<DebugMeshMover>();

        mv.SetPos(mLocation.pos);
        mf.mesh = mesh;
        mr.material = MaterialStorage.DefaultSolid;

        Shader shader = Shader.Find("HDRP/Unlit");
        Material red = new Material(shader);
        Material green = new Material(shader);
        Material blue = new Material(shader);
        red.color = Color.red;
        green.color = Color.green;
        blue.color = Color.blue;

        GameObject PlanesStorage = new GameObject("Planes");
        PlanesStorage.transform.parent = pdecal.transform;
        PlanesStorage.transform.localPosition = Vector3.zero;
        GameObject planeGobj;
        int cnt = 0;
        foreach (geombase.Plane plane in cplanes)
        {
            planeGobj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            planeGobj.name = string.Format("Plane {0} {1} {2}",cnt++, plane.n, plane.d);

            planeGobj.transform.parent = PlanesStorage.transform;
            planeGobj.transform.rotation = Quaternion.LookRotation(Vector3.up, plane.n);
            planeGobj.transform.position = plane.n * (-plane.d);

            mr = planeGobj.GetComponent<MeshRenderer>();
            mr.material = blue;
        }
        debugged = true;
    }
    /// <summary>Упрощённый вариант для дебуга
    /// </summary>
    /// <param name="data"></param>
    //public bool Initialize(IMeshExporter mesh_exp, PolyDecalData data)
    //{
    //    myData = data;
    //    geombase.Plane[] cplanes = new geombase.Plane[6];
    //    int nplanes;

    //    float minx = data.nodes[0].x, minz = data.nodes[0].z, maxx = data.nodes[0].x, maxz = data.nodes[0].z;
    //    int i;
    //    for (i = 0; i < data.numnodes; ++i)
    //    {
    //        minx = Mathf.Min(minx, data.nodes[i].x);
    //        minz = Mathf.Min(minz, data.nodes[i].z);
    //        maxx = Mathf.Max(maxx, data.nodes[i].x);
    //        maxz = Mathf.Max(maxz, data.nodes[i].z);
    //    }

    //    mLocation.pos = new Vector3((minx + maxx) * .5f, 0, (minz + maxz) * .5f);
    //    mLocation.tm.Identity();

    //    for (i = 0; i < data.numnodes; ++i)
    //    {
    //        int j = i + 1; j = j >= data.numnodes ? 0 : j;
    //        Vector3 n = Vector3.Cross(Vector3.up, (data.nodes[j] - data.nodes[i]));
    //        n.Normalize();
    //        cplanes[i] = new geombase.Plane(-new Vector3(n.x, n.y, n.z), Vector3.Dot(n, (data.nodes[i] - mLocation.pos)));
    //    }

    //    nplanes = data.numnodes;

    //    float r = new Vector2((maxx - minx) * .5f, (maxz - minz) * .5f).magnitude;
    //    Vector3 dir = new Vector3(0, -1, 0);
    //    IMesh mesh = mesh_exp.Export(new Geometry.Line(mLocation.pos - dir * 10000, dir, 20000), r);

    //    if (mesh == null) return false;
    //    DebugExportMesh(mesh, cplanes, nplanes);

    //    //int num_parts = mesh.GetNumParts();
    //    //Matrix3f tm_fromworld = new Matrix3f();
    //    //mLocation.tm.GetTranspose(ref tm_fromworld);

    //    // Объявление перенесенов в iExportableGeometry.cs
    //    //public struct MeshPart
    //    //{
    //    //    int n_v;
    //    //    int n_i;
    //    //    Vertex v;
    //    //    WORD i;
    //    //} 

    //    //MeshPart[] parts = Alloca.ANewN<MeshPart>(num_parts);


    //    InitVisualData(data);
    //    return true;
    //}

    //public bool Initialize(IMeshExporter mesh_exp, PolyDecalData data)
    //{
    //    myData = data;
    //    geombase.Plane[] cplanes = new geombase.Plane[6];
    //    int nplanes;

    //    float minx = data.nodes[0].x, minz = data.nodes[0].z, maxx = data.nodes[0].x, maxz = data.nodes[0].z;
    //    int i, part;
    //    for (i = 0; i < data.numnodes; ++i)
    //    {
    //        minx = Mathf.Min(minx, data.nodes[i].x);
    //        minz = Mathf.Min(minz, data.nodes[i].z);
    //        maxx = Mathf.Max(maxx, data.nodes[i].x);
    //        maxz = Mathf.Max(maxz, data.nodes[i].z);
    //    }

    //    mLocation.pos = new Vector3((minx + maxx) * .5f, 0, (minz + maxz) * .5f);
    //    mLocation.tm.Identity();

    //    for (i = 0; i < data.numnodes; ++i)
    //    {
    //        int j = i + 1; j = j >= data.numnodes ? 0 : j;
    //        Vector3 n = Vector3.Cross(Vector3.up, (data.nodes[j] - data.nodes[i]));
    //        n.Normalize();
    //        cplanes[i] = new geombase.Plane(-new Vector3(n.x, n.y, n.z), Vector3.Dot(n, (data.nodes[i] - mLocation.pos)));
    //    }

    //    nplanes = data.numnodes;

    //    float r = new Vector2((maxx - minx) * .5f, (maxz - minz) * .5f).magnitude;
    //    Vector3 dir = new Vector3(0, -1, 0);
    //    IMesh mesh = mesh_exp.Export(new Geometry.Line(mLocation.pos - dir * 10000, dir, 20000), r);
    //    if (mesh == null) return false;

    //    int num_parts = mesh.GetNumParts();
    //    Matrix3f tm_fromworld = new Matrix3f();
    //    mLocation.tm.GetTranspose(ref tm_fromworld);

    //    MeshPart[] parts = Alloca.ANewN<MeshPart>(num_parts);

    //    Vector3 v_min, v_max;

    //    for (part = 0; part < num_parts; ++part)
    //    {
    //        MeshDesc desc = mesh.GetPartDesc(part);
    //        AssertBp(desc.FVF & D3DFVF_XYZ);

    //        //Vertex* src_v = (Vertex*)alloca(sizeof(Vertex) * desc.num_vertices);
    //        //WORD* src_i = (WORD*)alloca(sizeof(WORD) * desc.num_indices);
    //        Vertex[] src_v = Alloca.ANewN<Vertex>(desc.num_vertices);
    //        WORD[] src_i = Alloca.ANewN<WORD>(desc.num_indices);
    //        Matrix34f tm = new Matrix34f();

    //        Strided strided = new Strided(desc.num_vertices);
    //        //strided.position = new Stride<Vector3>(src_v, sizeof(Vertex), offsetof(Vertex, pos));
    //        //strided.normal = new Stride<Vector3>(src_v, sizeof(Vertex), offsetof(Vertex, norm));
    //        //strided.diffuse = new Stride<DWORD>(src_v, sizeof(Vertex), offsetof(Vertex, color));
    //        strided.position = new Stride<Vector3>(new Vector3[desc.num_vertices], desc.num_vertices);
    //        strided.normal = new Stride<Vector3>(new Vector3[desc.num_vertices], desc.num_vertices);
    //        strided.diffuse = new Stride<DWORD>(new DWORD[desc.num_vertices], desc.num_vertices);

    //        mesh.CopyVertices(part, D3DFVF_XYZ | D3DFVF_NORMAL | D3DFVF_DIFFUSE, strided);
    //        mesh.CopyIndices(part, ref src_i);
    //        mesh.CopyLocation(part, ref tm);

    //        src_v = strided.ExportVertices();

    //        Vector3 ofs_to_local = tm.pos - mLocation.pos;
    //        Matrix3f tm_to_local = new Matrix3f();
    //        tm_to_local.Multiply(tm.tm, tm_fromworld);

    //        Matrix34f tm_res = new Matrix34f(tm_to_local, Matrix3f.Vector3Matrix3fMultiply(ofs_to_local, tm_fromworld));
    //        {
    //            v_min = new Vector3(FLT_MAX, FLT_MAX, FLT_MAX);
    //            v_max = new Vector3(-FLT_MAX, -FLT_MAX, -FLT_MAX);

    //            for (i = 0; i < desc.num_vertices; ++i)
    //            {
    //                src_v[i].pos = tm_res.TransformPoint(src_v[i].pos);
    //                //v_min = Select(v_min, src_v[i].pos, std::less<float>());
    //                //v_max = Select(v_max, src_v[i].pos, std::greater<float>());
    //                v_min = Select(v_min, src_v[i].pos, std_less);
    //                v_max = Select(v_max, src_v[i].pos, std_greater);

    //            }
    //        }

    //        //now vertices in decal's local system ( data.location )
    //        //clip them here

    //        //parts[part].v=(Vertex*) alloca(sizeof(Vertex)*(((desc.num_indices+2)/3)*(3+6)));
    //        //parts[part].i=(WORD*) alloca(sizeof(WORD  )*(((desc.num_indices+2)/3)*64));
    //        parts[part].v = Alloca.ANewN<Vertex>(((desc.num_indices + 2) / 3) * (3 + 6));
    //        parts[part].i = Alloca.ANewN<WORD>(((desc.num_indices + 2) / 3) * 64);

    //        ClipIndexed(cplanes, nplanes,
    //                    src_v, desc.num_vertices, src_i, desc.num_indices,
    //                    ref parts[part].v, ref parts[part].n_v, ref parts[part].i, ref parts[part].n_i);
    //    }

    //    int res_numv = 0, res_numi = 0;
    //    for (i = 0; i < num_parts; ++i)
    //    {
    //        res_numv += parts[i].n_v;
    //        res_numi += parts[i].n_i;
    //    }

    //    mesh.Release(); mesh = null;

    //    if (res_numv == 0)
    //        return false;


    //    InitVisualData(data);


    //    AssertBp(res_numi);

    //    Vertex[] pvertices = new Vertex[res_numv];
    //    WORD[] pindices = new WORD[res_numi];

    //    Vector3 tg_offs = mLocation.pos - data.tc_base;

    //    v_min = new Vector3(FLT_MAX, FLT_MAX, FLT_MAX);
    //    v_max = new Vector3(FLT_MIN, FLT_MIN, FLT_MIN);
    //    int cur_sv = 0, cur_si = 0;
    //    for (part = 0; part < num_parts; ++part)
    //    {
    //        for (int vi = 0; vi < parts[part].n_v; ++vi)
    //        {
    //            Vertex v = parts[part].v[vi];

    //            Vector3 gv = Matrix3f.Vector3Matrix3fMultiply((v.pos + tg_offs), data.tc_gen);
    //            v.tc = new Vector2(gv.x, gv.z);

    //            DWORDARGB s_clr = new DWORDARGB(v.color);
    //            v.color = new DWORDARGB(
    //            255,
    //            (int)Mathf.Clamp(s_clr.r * mMaterial.diffuse.r, 0, 255),
    //            (int)Mathf.Clamp(s_clr.g * mMaterial.diffuse.g, 0, 255),
    //            (int)Mathf.Clamp(s_clr.b * mMaterial.diffuse.b, 0, 255));

    //            pvertices[cur_sv + vi] = v;


    //            //v_min = Select(v_min, v.pos, std::less<float>());
    //            //v_max = Select(v_max, v.pos, std::greater<float>());
    //            v_min = Select(v_min, v.pos, std_less);
    //            v_max = Select(v_max, v.pos, std_greater);
    //        }

    //        for (int ii = 0; ii < parts[part].n_i; ++ii)
    //        {
    //            pindices[cur_si + ii] = (WORD)(cur_sv + parts[part].i[ii]);
    //        }

    //        cur_sv += parts[part].n_v;
    //        cur_si += parts[part].n_i;
    //    }

    //    mFVF = Vertex.FVF;

    //    mNumVertices = res_numv;
    //    mNumIndices = res_numi;
    //    pmVertices = pvertices;
    //    pmIndices = pindices;

    //    mMin = v_min;
    //    mMax = v_max;

    //    mBoundingSphereLocal.o = (mMin + mMax) * .5f;
    //    mBoundingSphereLocal.r = ((mMax - mMin) * .5f).magnitude;

    //    mBoundingSphereWorld.o = mLocation.TransformPoint(mBoundingSphereLocal.o);
    //    mBoundingSphereWorld.r = mBoundingSphereLocal.r;//mLocation.TransformV((mMax-mMin)*.5).Norma();
    //    mLinearData = data.linear;

    //    return true;
    //}

    private void InitVisualData(PolyDecalData data)
    {
        mTexture = dll_data.LoadTexture(data.texture);
        mMaterial = dll_data.LoadMaterial(data.material);
        mLinearData = data.linear;
    }

    public override void Draw()
    {
        if (!mChangeApplyed)
            ApplyChange();

        //Engine::SetFogState(
        //  Engine::GetFogStartDist(),
        //  Engine::GetFogEndDist(),
        //  Engine::GetFogColor().PackARGB());
        d3d.SetTexture(mTexture);
        d3d.SetMaterial(mMaterial);
        //mRState.Apply();

        Engine.PushWorldTransform(mLocation);
        Engine.ApplyWorldTransform();
        //d3d.DrawIndexed(mFVF, pmVertices, mNumVertices, pmIndices, mNumIndices);
        d3dunity.DrawDecal(this);
        Engine.PopWorldTransform();
    }

    Texture2D mTexture;
    D3DMATERIAL7 mMaterial;



    //TODO - перенести в отдельный (статический?) класс
    private delegate bool Pred(float a, float b);
    private Vector3 Select(Vector3 a, Vector3 b, Pred pred)
    {
        return new Vector3(Choose(a[0], b[0], pred), Choose(a[1], b[1], pred), Choose(a[2], b[2], pred));
    }

    private float Choose(float a, float b, Pred pred)
    {
        return pred(a, b) ? a : b;
    }

    private bool std_less(float a, float b)
    {
        return a < b;
    }
    private bool std_greater(float a, float b)
    {
        return a > b;
    }

}
