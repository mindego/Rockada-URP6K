using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class StormUnityPolyDecalDraw
{
    Dictionary<int, GameObject> currentFrameGameObjects;
    Dictionary<int, GameObject> previousFrameGameObjects;

    public StormUnityPolyDecalDraw()
    {
        currentFrameGameObjects = new Dictionary<int, GameObject>();
        previousFrameGameObjects = new Dictionary<int, GameObject>();
    }
    public void Draw(PolyDecal polyDecal)
    {
        return;
        if (!StormUnityRenderer.FrustrumCulling(polyDecal.mLocation.pos)) return;
        int hash = polyDecal.GetHashCode();
        //if (!currentFrameGameObjects.ContainsKey(hash)) currentFrameGameObjects.Add(hash, GenerateGameObjects(polyDecal));
        if (!currentFrameGameObjects.ContainsKey(hash)) currentFrameGameObjects.Add(hash, GenerateGameObject(polyDecal));

        GameObject gobj = currentFrameGameObjects[hash];
        gobj.transform.position = Engine.ToCameraReference(polyDecal.mLocation.pos);
    }

    private Mesh GenerateMesh(PolyDecal polyDecal)
    {
        Mesh mesh = new Mesh();

        Vector3[] mVertices = new Vector3[polyDecal.mNumVertices];
        Vector3[] mNormals= new Vector3[polyDecal.mNumVertices];
        Vector2[] mUVs = new Vector2[mVertices.Length];
        ushort[] mIndices = polyDecal.pmIndices;

        for (int i=0;i<polyDecal.mNumVertices; i++)
        {
            mVertices[i] = polyDecal.pmVertices[i].pos;
            mUVs[i] = polyDecal.pmVertices[i].tc;
            mNormals[i] = polyDecal.pmVertices[i].norm;
        }

        mesh.SetVertices(mVertices);
        mesh.SetUVs(0,mUVs);
        mesh.SetNormals(mNormals);
        mesh.SetIndices(mIndices,MeshTopology.Triangles,0);
        mesh.RecalculateBounds();
        return mesh;
    }
    //private Mesh GenerateMesh(PolyDecal polyDecal)
    //{
    //    Mesh mesh = new Mesh();

    //    Vector3[] mVertices = new Vector3[polyDecal.myData.numnodes + 1];
    //    Vector2[] mUVs = new Vector2[mVertices.Length];
    //    int[] mIndices = new int[mVertices.Length * 3];
    //    //for (int i = 0; i < polyDecal.myData.numnodes; ++i)
    //    //{
    //    //    mVertices[i] = polyDecal.myData.nodes[i];
    //    //}

    //    int mIndicesCount = mIndices.Length;

    //    Vector3 tg_offs = polyDecal.mLocation.pos - polyDecal.myData.tc_base;

    //    int centerIndex = polyDecal.myData.numnodes;
    //    mVertices[centerIndex] = Vector3.zero;
    //    mUVs[centerIndex] = new Vector2(.5f, .5f);

    //    int i, j;
    //    int trisIndex = 0;
    //    for (i = 0; i < polyDecal.myData.numnodes; i++)
    //    {
    //        j = i + 1; j = j < polyDecal.myData.numnodes ? j : 0;
    //        mVertices[i] = polyDecal.myData.nodes[i] - polyDecal.mLocation.pos;
    //        Vector3 gv = Matrix3f.Vector3Matrix3fMultiply(mVertices[i] + tg_offs, polyDecal.myData.tc_gen);
    //        mUVs[i] = new Vector2(gv.x, gv.z);
    //        mIndices[trisIndex++] = centerIndex;
    //        mIndices[trisIndex++] = j;
    //        mIndices[trisIndex++] = i;
    //    }

    //    mesh.vertices = mVertices;
    //    mesh.triangles = mIndices;
    //    //mesh.normals = 
    //    mesh.uv = mUVs;

    //    mesh.RecalculateBounds();
    //    return mesh;
    //}


    private GameObject GenerateGameObjects(PolyDecal polyDecal)
    {
        GameObject res = new GameObject(renderer_dll.d3d.GetTexture().name);
        for (int i = 0; i < polyDecal.myData.numnodes; ++i)
        {
            int j = i + 1; j = j >= polyDecal.myData.numnodes ? 0 : j;
            GameObject node = GenerateGameObject((polyDecal.myData.nodes[j] + polyDecal.myData.nodes[i]) / 2, string.Format("{0}/{1}:{2}", res.name, i, j));
            //node.transform.position = Engine.ToCameraReference(polyDecal.myData.nodes[i]);

            node.transform.rotation = Quaternion.LookRotation(polyDecal.myData.nodes[j] - polyDecal.myData.nodes[i], Vector3.up);
            node.transform.parent = res.transform;

        }

        GameObject lr = new GameObject();
        lr.transform.parent = res.transform;
        lr.transform.position = Vector3.zero;
        lr.transform.rotation = Quaternion.identity;
        LineRenderer myLineRenderer = lr.AddComponent<LineRenderer>();
        //myLineRenderer.material = new Material(MaterialStorage.DefaultTransparentUnlit);
        myLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        myLineRenderer.startColor = Color.red;
        myLineRenderer.endColor = Color.green;
        myLineRenderer.loop = true;

        myLineRenderer.positionCount = polyDecal.myData.numnodes;
        for (int i = 0; i < polyDecal.myData.numnodes; ++i)
        {
            //int j = i + 1; j = j >= polyDecal.myData.numnodes ? 0 : j;
            myLineRenderer.SetPosition(i, Engine.ToCameraReference(polyDecal.myData.nodes[i]));
            //Color tmpColor = new Color(1, 1, i / polyDecal.myData.numnodes, 1);
            //EngineDebug.DebugLine(polyDecal.myData.nodes[j], polyDecal.myData.nodes[i], tmpColor);
        }

        //for (int i = 0; i < polyDecal.myData.numnodes; ++i)
        //{
        //    int j = i + 1; j = j >= polyDecal.myData.numnodes ? 0 : j;
        //    Vector3 n = Vector3.Cross(Vector3.up, (polyDecal.myData.nodes[j] - polyDecal.myData.nodes[i]));
        //    n.Normalize();
        //    //cplanes[i] = new geombase.Plane(-new Vector3(n.x, n.y, n.z), Vector3.Dot(n, (data.nodes[i] - mLocation.pos)));
        //    GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    //tmp.transform.position = -n;
        //    tmp.transform.rotation = Quaternion.LookRotation(-n);
        //    tmp.transform.localPosition += Vector3.forward * Vector3.Dot(n, (polyDecal.myData.nodes[i] - polyDecal.mLocation.pos));
        //    tmp.transform.parent = res.transform;

        //}
        return res;
    }

    private GameObject GenerateOutline(PolyDecal polyDecal)
    {
        GameObject lr = new GameObject();
        lr.transform.position = Vector3.zero;
        lr.transform.rotation = Quaternion.identity;
        LineRenderer myLineRenderer = lr.AddComponent<LineRenderer>();
        //myLineRenderer.material = new Material(MaterialStorage.DefaultTransparentUnlit);
        myLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        myLineRenderer.startColor = Color.red;
        myLineRenderer.endColor = Color.green;
        myLineRenderer.loop = true;

        myLineRenderer.positionCount = polyDecal.myData.numnodes;
        for (int i = 0; i < polyDecal.myData.numnodes; ++i)
        {
            myLineRenderer.SetPosition(i, Engine.ToCameraReference(polyDecal.myData.nodes[i]));
        }

        return lr;
    }
    private GameObject GenerateGameObject(PolyDecal polyDecal)
    {
        GameObject res = new GameObject(renderer_dll.d3d.GetTexture().name);
        MeshRenderer mr = res.AddComponent<MeshRenderer>();
        MeshFilter mf = res.AddComponent<MeshFilter>();

        //Vector3 dir = new Vector3(polyDecal.myData.tc_gen[0].z, polyDecal.myData.tc_gen[1].z, polyDecal.myData.tc_gen[2].z);
        //Vector3 up = new Vector3(polyDecal.myData.tc_gen[0].y, polyDecal.myData.tc_gen[1].y, polyDecal.myData.tc_gen[2].y);
        //Vector3 right = new Vector3(polyDecal.myData.tc_gen[0].x, polyDecal.myData.tc_gen[1].x, polyDecal.myData.tc_gen[2].x);
        //res.transform.rotation = Quaternion.LookRotation(dir, up);

        mf.mesh = GenerateMesh(polyDecal);
//        mr.material = new Material(MaterialStorage.DefaultDecal);
        mr.material = new Material(MaterialStorage.DefaultSolid);
        mr.material.mainTexture = renderer_dll.d3d.GetTexture();

        var debug = res.AddComponent<DetailDebug>();
        debug.val1 = polyDecal.mLocation.pos;
        debug.val2 = polyDecal.myData.tc_gen.raw[0];

        return res;
    }
    //private GameObject GenerateGameObject(PolyDecal polyDecal)
    //{
    //    GameObject res = new GameObject(renderer_dll.d3d.GetTexture().name);
    //    MeshRenderer mr = res.AddComponent<MeshRenderer>();
    //    MeshFilter mf = res.AddComponent<MeshFilter>();

    //    //Vector3 dir = new Vector3(polyDecal.myData.tc_gen[0].z, polyDecal.myData.tc_gen[1].z, polyDecal.myData.tc_gen[2].z);
    //    //Vector3 up = new Vector3(polyDecal.myData.tc_gen[0].y, polyDecal.myData.tc_gen[1].y, polyDecal.myData.tc_gen[2].y);
    //    //Vector3 right = new Vector3(polyDecal.myData.tc_gen[0].x, polyDecal.myData.tc_gen[1].x, polyDecal.myData.tc_gen[2].x);
    //    //res.transform.rotation = Quaternion.LookRotation(dir, up);

    //    mf.mesh = GenerateMesh(polyDecal);
    //    //mr.material = new Material(MaterialStorage.DefaultDecal);
    //    mr.material = new Material(MaterialStorage.DefaultSolid);
    //    mr.material.mainTexture = renderer_dll.d3d.GetTexture();

    //    var debug = res.AddComponent<DetailDebug>();
    //    debug.val1 = polyDecal.mLocation.pos;
    //    debug.val2 = polyDecal.myData.tc_gen.raw[0];

    //    return res;
    //}
    //private GameObject GenerateGameObject(PolyDecal polyDecal)
    //{
    //    GameObject res = new GameObject("Decal");
    //    DecalProjector projector = res.AddComponent<DecalProjector>();
    //    projector.material = new Material(MaterialStorage.DefaultDecal);
    //    projector.material.mainTexture = renderer_dll.d3d.GetTexture();
    //    HDMaterial.ValidateMaterial(projector.material);
    //    return res;
    //}
    private GameObject GenerateGameObject(Vector3 node, string name = "Decal")
    {
        GameObject res = new GameObject(name + " " + node);
        res.transform.position = Engine.ToCameraReference(node + Vector3.up * 100);

        GameObject projectorGObj = new GameObject(name + " projector");
        projectorGObj.transform.parent = res.transform;
        projectorGObj.transform.localPosition = Vector3.zero;
        projectorGObj.transform.rotation = Quaternion.LookRotation(Vector3.down);

        DecalProjector projector = projectorGObj.AddComponent<DecalProjector>();
        projector.material = new Material(MaterialStorage.DefaultDecal);
        projector.material.mainTexture = renderer_dll.d3d.GetTexture();
        projector.size = new Vector3(40, 40, 150); //TODO скейлить проектор в размеры ноды

        HDMaterial.ValidateMaterial(projector.material);
        return res;
    }



    public void Flush()
    {
        foreach (KeyValuePair<int, GameObject> v in previousFrameGameObjects)
        {
            if (currentFrameGameObjects.ContainsKey(v.Key)) continue;
            GameObject.Destroy(v.Value);
        }

        previousFrameGameObjects = currentFrameGameObjects;
        currentFrameGameObjects.Clear();
    }
}



