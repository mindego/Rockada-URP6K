using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using static D3DEMULATION;
using static RoFlags;
using static TerrainDefs;
using static MaterialStorage;

/// <summary>
/// Класс-обёртка для совместимости. Надо будет удалить её и использовать менеджер напрямую.
/// </summary>
public class StormUnityManagerFPO : IStormUnityRendererFPO
{
    private StormUnityDrawableManager<FPO, FPO2GameObjectContainer> manager;

    public StormUnityManagerFPO()
    {
        manager = new StormUnityDrawableManager<FPO, FPO2GameObjectContainer>();
    }
    public void Cleanup()
    {
        manager.Cleanup();
    }

    public void Draw(FPO myFPO)
    {
        manager.Draw(myFPO);
    }
}
public class StormUnityDrawableManager<SRCCLASS, CONTAINERCLASS> : IStormUnityDrawableManager<SRCCLASS> where CONTAINERCLASS : IStormUnityContainer<SRCCLASS>, new()
{
    private Dictionary<int, CONTAINERCLASS> drawnthisframe = new Dictionary<int, CONTAINERCLASS>();
    private Dictionary<int, CONTAINERCLASS> drawnprevframe = new Dictionary<int, CONTAINERCLASS>();

    public void Cleanup()
    {
        List<int> removeId = new List<int>();
        foreach (KeyValuePair<int, CONTAINERCLASS> kvp in drawnprevframe)
        {
            //if (kvp.Value == null) continue;
            if (!drawnthisframe.ContainsKey(kvp.Key)) kvp.Value.Destroy();
        }

        drawnprevframe = drawnthisframe;
        drawnthisframe = new Dictionary<int, CONTAINERCLASS>();
    }

    public void Draw(SRCCLASS myObj)
    {
        int hash = myObj.GetHashCode();
        CONTAINERCLASS c;

        bool inCurrent = drawnthisframe.ContainsKey(hash);
        bool inPrev = drawnprevframe.ContainsKey(hash);

        if (!inCurrent && inPrev)
        {
            drawnthisframe.Add(hash, drawnprevframe[hash]);
            inCurrent = true;
        }

        if (!inCurrent)
        {
            c = new CONTAINERCLASS();
            c.Init(myObj);
            drawnthisframe.Add(hash, c);
            inCurrent = true;
        }

        c = drawnthisframe[hash];
        c.Draw();
    }
}


public class FPO2GameObjectContainer : IStormUnityContainer<FPO>
{
    const int NORMAL = 0, DAMAGED = 1, DESTROYED = 2, INTERNAL = 3;
    const int VERTEXDATASIZE = 4 * (3 + 3 + 2);
    private FPO myFPO;
    private int CurrentImage;
    private GameObject myGameObject;
    private MeshRenderer mr;
    private MeshFilter mf;
    private MeshCollider mc;

    public void Destroy()
    {
        if (myGameObject != null) GameObject.Destroy(myGameObject); //TODO - возвращать в пул!
    }

    public void Draw()
    {
        UpdateGameObjectVisual();
        UpdateGameObjectTransform();

        if (myFPO.SubObjects == null) return;

        StormUnityRenderer.PushTransform(myGameObject.transform);
        for (RO tmpRO = myFPO.SubObjects; tmpRO != null; tmpRO = tmpRO.Next)
        {
            try
            {
                uint typeFlag = tmpRO.GetFlag(ROFID_ALLOBJECTS);
                switch (typeFlag)
                {
                    case (ROFID_FPO):
                        //Draw((FPO)tmpRO);
                        StormUnityRenderer.DrawFPO((FPO)tmpRO);
                        break;
                    case (ROFID_PARTICLE):
                        StormUnityRenderer.DrawParticle(tmpRO);
                        break;
                    default:
                        Debug.Log("Unsupported RO type: " + tmpRO + " flags " + tmpRO.GetFlags().ToString("X8"));
                        break;

                }
            }
            catch
            {
                Debug.Log("Failed to draw RO " + tmpRO + " flags " + tmpRO.GetFlags().ToString("X8"));
                throw;
            }
        }
        StormUnityRenderer.PopTransform();
    }

    private void UpdateGameObjectVisual()
    {
        if (myFPO.CurrentImage == CurrentImage) return;
        if (myFPO.r_images[myFPO.CurrentImage] == null) return;

        //FpoImage tmpFpoImage = ((TWrapObjectIRData<FpoImage>)myFPO.r_images[CurrentImage]).GetObject();
        FpoImage tmpFpoImage = (FpoImage)myFPO.r_images[myFPO.CurrentImage].Query(IDrawable.ID);


        //Debug.LogFormat("Updating image {0}->{1} details:{2}", CurrentImage, myFPO.CurrentImage, tmpFpoImage);
        CurrentImage = myFPO.CurrentImage;
        UpdateGameObject(tmpFpoImage);
    }

    //TODO - вынести кэши в отдельный файл.
    private static Dictionary<int, Mesh> MeshCache = new Dictionary<int, Mesh>();
    private void UpdateGameObject(FpoImage myFPOImage)
    {
        int LOD = BaseScene.DETAIL_STAGE_FULL; // Правильнее - создавать LodGroup;

        FpoLod myFpoLod = myFPOImage.lod[LOD];

        if (myFpoLod.GetNumGroups() == 0)
        {
            mf.sharedMesh = null;
            mr.materials = new Material[0];
            mc.sharedMesh = null;
            return;
        }

        mf.sharedMesh = GetMeshWithSubmeshes(myFpoLod);
        mc.sharedMesh = mf.sharedMesh;

        mr.materials = GetMaterials(myFpoLod);
        //Временно!
        //mr.materials = new Material[myFpoLod.GetNumGroups()];
        //for (int i = 0; i < myFpoLod.GetNumGroups(); i++)
        //{
        //    mr.materials[i]= MaterialStorage.DefaultSolid;
        //}

    }

    private Material[] GetMaterials(FpoLod myFpoLod)
    {
        Material[] materials = new Material[myFpoLod.GetNumGroups()];

        for (int i = 0; i < myFpoLod.GetNumGroups(); i++)
        {
            IShader myStormShader = myFpoLod.GetShader(i);
            ILayer layer = myStormShader.GetLayer(0);
            materials[i] = CreateMaterial(layer);
        }

        return materials;
    }

    private Material CreateMaterial(ILayer layer)
    {
        string cachekey = layer.GetHashCode().ToString("X8");
        if (materialcache.ContainsKey(cachekey)) return materialcache[cachekey];
        layer.Apply();
        //Debug.LogFormat("Creating material from layer {0} current texture: {1} material: {2}", layer, renderer_dll.d3d.GetTexture(), renderer_dll.d3d.GetMaterial());

        D3DMATERIAL7 stormMaterial = renderer_dll.d3d.GetMaterial();
        Texture2D texture = renderer_dll.d3d.GetTexture();

        Material material;

        /*
                    case TRT_SOLID:
                        mRState = dll_data.CreateRS("std_transp_none");
                        break;
                    case TRT_ADD:
                        mRState = dll_data.CreateRS("std_transp_add");
                        SetCustomFogColor(0);
                        break;
                    case TRT_FILTER:
                        mRState = dll_data.CreateRS("std_transp_blend");
                        break;
        */
        string matname;
        switch (Engine.CurrentRenderingStageTransparancy)
        {
            case "std_transp_none":
                material = new Material(DefaultSolid);
                if (RendererDllData.MaterialNames.TryGetValue(stormMaterial.GetHashCode(), out matname)) {
                    material.name = matname;
                }
                else
                {
                    material.name = "ERROR MATERIAL";
                }

                break;
            case "std_transp_add":
                material = new Material(DefaultTransparentAdd);
                if (RendererDllData.MaterialNames.TryGetValue(stormMaterial.GetHashCode(), out matname)) {
                    material.name = "UNIMPLEMENTED " + matname;
                }
                else
                {
                    material.name = "ERROR MATERIAL";
                }

                break;
            case "std_transp_blend":
                material = new Material(DefaultTransparentBlend);
                D3DCOLORVALUE tmp = stormMaterial.ambient;
                tmp.a = 1 - stormMaterial.diffuse.a;
                material.SetColor("_BaseColor", tmp.ToColor());

                if (RendererDllData.MaterialNames.TryGetValue(stormMaterial.GetHashCode(), out matname)) {
                    material.name = matname;
                }
                else
                {
                    material.name = "ERROR MATERIAL";
                }
                break;
            default:
                material = new Material(DefaultSolid);
                material.name = "Default transp material due stage:" + Engine.CurrentRenderingStageTransparancy;
                break;
        }
        float smoothness = 1 - Mathf.Pow(2 / (stormMaterial.power + 2), 1 / 4f);
        material.SetFloat("_Smoothness", smoothness);
        float metallization = 1 - Mathf.Sqrt(2.0f / (stormMaterial.specular.a + 2.0f));
        material.SetFloat("_Metallic", metallization);

        //const char *tss_name = layer->mTexture?"std_tss_diffuse":"std_tss_diffuse_no_texture";
        //iRS *tss=dll_data.CreateRS("std_tss_embm");
        if (texture != null) material.mainTexture = texture;
        switch (Engine.CurrentRenderingStageTexture)
        {
            case "std_tss_diffuse":
                material.mainTexture = texture;
                material.name += "#" + texture.name;
                break;
            case "std_tss_diffuse_no_texture":
                break;
            default:
                material.name = "ERROR " + material.name;
                material.name += "#" + texture.name;
                break;
        }


        HDMaterial.ValidateMaterial(material);
        materialcache.Add(cachekey, material);

        return material;
    }

    private Mesh GetMeshWithSubmeshes(FpoLod myLod)
    {
        int meshId = myLod.GetHashCode();

        if (MeshCache.ContainsKey(meshId)) return MeshCache[meshId];

        Mesh gobjMesh = new Mesh();
        //Из-за особенностей хранения данных меш 0 содержкит информацию о вершинах _всех_ субмешей
        FpoMesh mesh = myLod.GetMesh(0);
        Vector3[] vertices = new Vector3[mesh.mVertices.Length];
        Vector3[] normals = new Vector3[mesh.mVertices.Length];
        Vector2[] uvs = new Vector2[mesh.mVertices.Length];

        Vector3 tmpVector;
        int start = 0;
        byte[] buffer;

        //Debug.LogFormat("Trying to create {0} vertices from {1} bytes buffer {2}", mesh.mNumVertices, mesh.mVertices.Length, buffer.Length);
        int VertexCount = mesh.mVertices.Length / VERTEXDATASIZE;
        for (int i = 0; i < VertexCount; i++)
        {
            start = i * VERTEXDATASIZE;
            //Debug.LogFormat("Creating vertex {0}/{1} start byte{2}/{3}", i, mesh.mNumVertices - 1, start, mesh.mVertices.Length);
            tmpVector = new Vector3();
            buffer = mesh.mVertices[start..(start + 4)];
            tmpVector.x = BitConverter.ToSingle(buffer);
            start += 4;
            buffer = mesh.mVertices[start..(start + 4)];
            tmpVector.y = BitConverter.ToSingle(buffer);
            start += 4;
            buffer = mesh.mVertices[start..(start + 4)];
            tmpVector.z = BitConverter.ToSingle(buffer);

            //Отзеркалить по вертикали
            tmpVector.y = -tmpVector.y;
            vertices[i] = tmpVector;

            tmpVector = new Vector3();
            start += 4;
            buffer = mesh.mVertices[start..(start + 4)];
            tmpVector.x = BitConverter.ToSingle(buffer);
            start += 4;
            buffer = mesh.mVertices[start..(start + 4)];
            tmpVector.y = BitConverter.ToSingle(buffer);
            start += 4;
            buffer = mesh.mVertices[start..(start + 4)];
            tmpVector.z = BitConverter.ToSingle(buffer);

            //Отзеркалить по вертикали
            tmpVector.y = -tmpVector.y;
            normals[i] = tmpVector;


            tmpVector = new Vector2();
            start += 4;
            buffer = mesh.mVertices[start..(start + 4)];
            tmpVector.x = BitConverter.ToSingle(buffer);
            start += 4;
            buffer = mesh.mVertices[start..(start + 4)];
            tmpVector.y = BitConverter.ToSingle(buffer);

            tmpVector.y = 1 - tmpVector.y; //Перевернуть UV координату в соответствии с ориентацией текстуры
            uvs[i] = tmpVector;
        }

        gobjMesh.SetVertices(vertices);
        gobjMesh.SetNormals(normals);
        gobjMesh.SetUVs(0, uvs);

        gobjMesh.subMeshCount = myLod.GetNumGroups();
        int baseIndex = 0;
        int baseVertex = 0;

        int[] indices;
        for (int i = 0; i < myLod.GetNumGroups(); i++)
        {
            mesh = myLod.GetMesh(i);
            indices = new int[mesh.mNumIndices];
            Array.Copy(mesh.mIndices, 0, indices, 0, mesh.mNumIndices);
            FlipTrianglesFaces(ref indices);
            gobjMesh.SetIndices(indices, MeshTopology.Triangles, i, true, baseVertex);

            baseIndex += mesh.mNumIndices;
            baseVertex += mesh.mNumVertices;
        }

        MeshCache.Add(meshId, gobjMesh);

        return gobjMesh;
    }
    private Mesh GetMeshSingle(FpoMesh mesh)
    {
        int meshId = mesh.GetHashCode();

        if (MeshCache.ContainsKey(meshId)) return MeshCache[meshId];

        Mesh gobjMesh = new Mesh();
        Vector3[] vertices = new Vector3[mesh.mNumVertices];
        Vector3[] normals = new Vector3[mesh.mNumVertices];
        Vector2[] uvs = new Vector2[mesh.mNumVertices];

        Vector3 tmpVector;
        int start = 0;
        byte[] buffer = mesh.mVertices[start..(start + 4)];

        //Debug.LogFormat("Trying to create {0} vertices from {1} bytes buffer {2}", mesh.mNumVertices, mesh.mVertices.Length, buffer.Length);
        for (int i = 0; i < mesh.mNumVertices; i++)
        {
            start = i * 4 * (3 + 3 + 2);
            //Debug.LogFormat("Creating vertex {0}/{1} start byte{2}/{3}", i, mesh.mNumVertices - 1, start, mesh.mVertices.Length);
            tmpVector = new Vector3();
            buffer = mesh.mVertices[start..(start + 4)];
            tmpVector.x = BitConverter.ToSingle(buffer);
            start += 4;
            buffer = mesh.mVertices[start..(start + 4)];
            tmpVector.y = BitConverter.ToSingle(buffer);
            start += 4;
            buffer = mesh.mVertices[start..(start + 4)];
            tmpVector.z = BitConverter.ToSingle(buffer);

            //Отзеркалить по вертикали
            tmpVector.y = -tmpVector.y;
            vertices[i] = tmpVector;

            tmpVector = new Vector3();
            start += 4;
            buffer = mesh.mVertices[start..(start + 4)];
            tmpVector.x = BitConverter.ToSingle(buffer);
            start += 4;
            buffer = mesh.mVertices[start..(start + 4)];
            tmpVector.y = BitConverter.ToSingle(buffer);
            start += 4;
            buffer = mesh.mVertices[start..(start + 4)];
            tmpVector.z = BitConverter.ToSingle(buffer);

            //Отзеркалить по вертикали
            tmpVector.y = -tmpVector.y;
            normals[i] = tmpVector;


            tmpVector = new Vector2();
            start += 4;
            buffer = mesh.mVertices[start..(start + 4)];
            tmpVector.x = BitConverter.ToSingle(buffer);
            start += 4;
            buffer = mesh.mVertices[start..(start + 4)];
            tmpVector.y = BitConverter.ToSingle(buffer);

            tmpVector.y = 1 - tmpVector.y; //Перевернуть UV координату в соответствии с ориентацией текстуры
            uvs[i] = tmpVector;
        }

        gobjMesh.SetVertices(vertices);
        gobjMesh.SetNormals(normals);
        gobjMesh.SetUVs(0, uvs);

        int[] indices = new int[mesh.mNumIndices];
        //Debug.LogFormat("Generating indices {0} from buffer {1}", mesh.mNumIndices,mesh.mIndices.Length);
        for (int i = 0; i < mesh.mNumIndices; i++)
        {
            indices[i] = mesh.mIndices[i];
        }
        FlipTrianglesFaces(ref indices);

        gobjMesh.SetIndices(indices, MeshTopology.Triangles, 0);

        MeshCache.Add(meshId, gobjMesh);


        return gobjMesh;
    }

    private void FlipTrianglesFaces(ref int[] indices)
    {
        for (int i = 0; i < indices.Length; i += 3)
        {
            int[] buffer = new int[3];
            Array.Copy(indices, i, buffer, 0, 3);
            int tmp = buffer[0];
            buffer[0] = buffer[2];
            buffer[2] = tmp;
            Array.Copy(buffer, 0, indices, i, 3);
        }
    }


    private Vector3 Dir, Up, Org;
    public void UpdateGameObjectTransform()
    {
        UpdateParent();
        UpdatePosition();
        UpdateRotation();
    }

    private void UpdateRotation()
    {
        if (myFPO.Dir == Vector3.zero || myFPO.Up == Vector3.zero) return;
        if (Dir == myFPO.Dir && Up == myFPO.Up) return;
        myGameObject.transform.localRotation = Quaternion.LookRotation(myFPO.Dir, myFPO.Up);
        Dir = myFPO.Dir;
        Up = myFPO.Up;
    }
    private void UpdateParent()
    {
        Transform currentTransform = StormUnityRenderer.GetCurrentTransform();
        if (myGameObject.transform.parent == currentTransform) return;

        myGameObject.transform.parent = currentTransform;
    }
    private void UpdatePosition()
    {
        if (myGameObject.transform.parent != null)
        {
            myGameObject.transform.localPosition = myFPO.Org;
        }
        else
        {
            if (!IsValid(myFPO.Org))
            {
                Debug.LogErrorFormat("Invalid Vector org {0}, prev value {1} for FPO {2} {3}", myFPO.Org, Org, myFPO.TextName, myFPO.GetHashCode().ToString("X8"));
            }
            myGameObject.transform.position = Engine.ToCameraReference(myFPO.Org);
        }
        Org = myFPO.Org;
    }

    private bool IsValid(Vector3 v)
    {
        if (float.IsNaN(v.x)) return false;
        if (float.IsNaN(v.y)) return false;
        if (float.IsNaN(v.z)) return false;
        return true;
    }

    public void Init(FPO _myFPO)
    {
        CurrentImage = -1;
        myFPO = _myFPO;
        GenerateGameObject();
    }

    private LODGroup myLODgroup;
    private void GenerateGameObject()
    {
        string name = myFPO.fdata.name.ToString("X8");

        if (myFPO.TextName != null) name = myFPO.TextName + " " + name;

        //myGameObject = GameObjectFactory.GetGameObject(GameObjectFactory.GameObjectType.MESH);
        //myGameObject.name = name;

        //mr = myGameObject.GetComponent<MeshRenderer>();
        //mf = myGameObject.GetComponent<MeshFilter>();
        //mc = myGameObject.GetComponent<MeshCollider>();

        myGameObject = new GameObject(name);
        mr = myGameObject.AddComponent<MeshRenderer>();
        mf = myGameObject.AddComponent<MeshFilter>();
        mc = myGameObject.AddComponent<MeshCollider>();

        //myLODgroup = myGameObject.AddComponent<LODGroup>();

        //LOD[] lods = new LOD[BaseScene.DETAIL_STAGE_NONE];

        //for (int i=BaseScene.DETAIL_STAGE_FULL;i< BaseScene.DETAIL_STAGE_NONE;i++)
        //{
        //    lods[i] = new LOD();
        //}

        ///*
        //Из  SceneVisualizer::GetDetailStage(const VECTOR& Org) (при r_range: 4000) :
        //if (a < 500.f) return DETAIL_STAGE_FULL;
        //if (a < 3000.f) return DETAIL_STAGE_HALF;
        //return (a<cd.GetCameraRange()? DETAIL_STAGE_QUARTER:DETAIL_STAGE_NONE);

        //*/

        //myLODgroup.SetLODs(lods);

    }
}
public interface IStormUnityDrawableManager<SRCCLASS>
{
    public void Cleanup();
    public void Draw(SRCCLASS myObj);
}
public interface IStormUnityContainer<SRCCLASS>
{
    public void Init(SRCCLASS myObject);
    public void Destroy();
    public void Draw();
}

public class StormUnityFPOImageGameObject : MonoBehaviour
{
    private FPO myFPO;

    public StormUnityFPOImageGameObject(FPO _myFPO)
    {
        myFPO = _myFPO;
    }

    private void Update()
    {
        if (myFPO == null) return;
    }

}

