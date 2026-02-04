using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using static renderer_dll;
using static RoFlags;

public class StormUnityRendererFPOMesh : IStormUnityRendererFPO
{
    private Stack<Transform> transformStack;
    private Dictionary<int, FPORendererContainer> FPOCache;

    public StormUnityRendererFPOMesh()
    {
        transformStack = new Stack<Transform>();
        FPOCache = new Dictionary<int, FPORendererContainer>();
    }

    public static Stack<FPORendererContainer> containers = new Stack<FPORendererContainer>();
    public static FPORendererContainer myParent
    {
        get
        {
            if (containers.Count == 0) return null;
            return containers.Peek();
        }
    }
    public void Draw(FPO myFPO)
    {
        if (!StormUnityRenderer.FrustrumCulling(myFPO.Top().Org)) return;
        int hash = myFPO.GetHashCode();
        if (!FPOCache.ContainsKey(hash)) FPOCache.Add(hash, new FPORendererContainer(myFPO));
        
        FPOCache[hash].Draw();
        containers.Push(FPOCache[hash]);
        if (myFPO.SubObjects != null)
        {
            for (RO tmpRO = myFPO.SubObjects; tmpRO != null; tmpRO = tmpRO.Next)
            {
                try
                {
                    uint typeFlag = tmpRO.GetFlag(ROFID_ALLOBJECTS);
                    switch (typeFlag)
                    {
                        case (ROFID_FPO):
                            Draw((FPO)tmpRO);
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
        }
        containers.Pop();
    }

    public void Cleanup() { }
}

public class FPOImage
{
    public Mesh myMesh { get; set; }
    public Material[] myMaterials { get; set; }
}
public class FPORendererContainer
{
    private FPO myFPO;
    private FpoGraphData[] myFpoGraphDatas;

    private FPOImage[] Images;
    int LOD;
    int CurrentImage;

    public FPORendererContainer(FPO drawFPO)
    {
        LOD = 0;
        CurrentImage = 0;
        myFPO = drawFPO;
        myFpoGraphDatas = new FpoGraphData[4];
        Images= new FPOImage[4];

        LoadFpoGraphDatas();
        LoadFPOImages();
    }

    private void LoadFpoGraphDatas()
    {
        for (int i = 0; i < 4; i++)
        {
            myFpoGraphDatas[i] = (myFPO.fdata.images[i].graph == 0xFFFFFFFF) ? null : dll_data.meshes.GetBlock(myFPO.fdata.images[i].graph).Convert<FpoGraphData>();
        }
    }

    private void LoadFPOImages()
    {
        uint id;
        StormMesh graph;
        for (int i = 0; i < 4; i++)
        {
            if (myFpoGraphDatas[i] == null)
            {
                Images[i] = null;
                continue;
            }
            id = myFpoGraphDatas[i].GetLod(LOD);
            graph = dll_data.GetStormMesh(id);
            if (graph == null) continue;

            ObjId tmpId = new ObjId(id);
            tmpId = dll_data.meshes.CompleteObjId(tmpId);

            Images[i] = new FPOImage();
            Images[i].myMesh = StormMeshImport.ExtractMesh(graph, tmpId.name);
            Images[i].myMaterials = StormMeshImport.GetMaterials(graph);
        }
    }

    public void Draw()
    {
        DrawMesh(myFPO.CurrentImage);
    }

    private Vector3 Dir, Up,Org;
    private Quaternion rotation;

    Matrix4x4 tMatrix;
    private void DrawMesh(int imageId)
    {
        if (Images[imageId] == null || Images[imageId].myMesh == null || Images[imageId].myMaterials == null) return;

        RenderParams rp;
        for (int i = 0; i < Images[imageId].myMaterials.Length; i++)
        {
            rp = new RenderParams(Images[imageId].myMaterials[i]);

            if (myFPO.Parent==null)
            {
                tMatrix = Matrix4x4.Translate(Engine.ToCameraReference(myFPO.Org));
            } else
            {
                tMatrix = StormUnityRendererFPOMesh.myParent.tMatrix * Matrix4x4.Translate(myFPO.Org);
            }
                
            if (myFPO.Dir != Vector3.zero && myFPO.Up != Vector3.zero)
            {
                if (myFPO.Dir != Dir || myFPO.Up != Up)
                {
                    Dir = myFPO.Dir;
                    Up = myFPO.Up;
                    rotation = Quaternion.LookRotation(myFPO.Dir, myFPO.Up);
                }
            }
            tMatrix *= Matrix4x4.Rotate(rotation);
            Graphics.RenderMesh(rp, Images[imageId].myMesh, i, tMatrix);
        }
    }
}
