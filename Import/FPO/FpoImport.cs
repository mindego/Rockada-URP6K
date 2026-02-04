using System.Collections; 
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using crc32 = System.UInt32;
using System;

public class FpoImport
{
    public static int nodesCount = 0;
    public static bool stop = false;
    public static FpoGraphData GetMeshMeta(Stream ms)
    {
        //FpoGraphData fpoGraphData = new FpoGraphData();
        //fpoGraphData.type = FileUtils.ReadStruct<GDType>(ms);
        GraphData graphData = StormFileUtils.ReadStruct<GraphData>(ms);
        FpoGraphData fpoGraphData = new FpoGraphData();
        fpoGraphData.type = graphData.type;

        fpoGraphData.lods = new uint[3];
        for (int i = 0; i < 3; i++)
        {
            fpoGraphData.lods[i] = StormFileUtils.ReadStruct<uint>(ms, (int)ms.Position);
        }
        return fpoGraphData;

    }
    //public static GameObject LoadFPO(Stream ms, string name)
    //{
    //    GameObject res = new GameObject();
    //    res.name = name;

    //    //GameObject root=GetFPO(ms, "root",0,res);
    //    GameObject root = GetFPO(ms, "root", 0);
    //    //root.transform.localRotation = Quaternion.Euler(0, 0, 180);
    //    root.transform.parent = res.transform;
    //    //return res;
    //    return res;
    //}

    public struct FPONode
    {
        /*public Flags flags;
        public crc32 name;
        public uint num_slots;
        public Position pos;
        public ImageData[] images; //STUB!
        public uint tree_next;
        public uint tree_sub;
        public SlotData[] slots; //STUB*/

        public int flags;
        public string name, parentName;
        public Vector3 pos;
        //public Vector3 up, left;
        public Vector3 up, dir;

        public FPOmesh[] meshes;
        public FPONode[] children;
        public FPOSlot[] slots;

        public FPONode(string name) : this()
        {
            this.name = name;
            this.children = new FPONode[0];
            meshes = new FPOmesh[0];
            this.slots = new FPOSlot[0];
        }

        public override string ToString()
        {
            string res;
            res = $"FPO node {name} children {children.Length} slots {slots.Length} meshes {meshes.Length}";

            foreach (FPONode child in children)
            {
                res += "\n" + child.ToString();
            }
            //return "FPO node " + name + 
            return res;
        }
    }

    public struct FPOSlot
    {
        public int slotId;
        public uint name;
        public Vector3 pos;
        public Vector3 up, dir;
        public int flags;

        public override string ToString()
        {
            return $"Slot {slotId} name {name} flags {flags.ToString("X8")}";
        }
    }
    public struct FPOmesh
    {
        public string meshName;
        public int lod, variant;

        public override string ToString()
        {
            return ("Mesh: [" + meshName + "] lod " + lod + " variant " + variant);
        }
    }

    //public static FPONode GetFPOSchema(Stream ms)
    //{
    //    FPONode root = new FPONode("root");
    //    GetFPOSchema(ms, ref root, 0);
    //    //Debug.Log($"Validating:\n{root}");
    //    return root;
    //}
    //public static void GetFPOSchema(Stream ms, ref FPONode parent, long offset = 0)
    //{
    //    //Debug.Log("Loading Node " + ++nodesCount) ;

    //    FpoData FPO = GetFPOData(ms, (uint)offset);

    //    FPONode node = new FPONode();
    //    node.flags = FPO.flags.ToInt();
    //    node.name = "STUB! id " + FPO.name.ToString("X8");
    //    node.parentName = parent.name;

    //    Vector3 tmpPos = FPO.pos.org;
    //    tmpPos.y *= -1;

    //    node.pos = tmpPos;

    //    node.up = FPO.pos.e2;
    //    Vector3 left = FPO.pos.e1;

    //    node.up.y *= -1;
    //    left.y *= -1;

    //    node.dir = Vector3.Cross(node.up, left);
    //    node.up *= -1;
    //    /*
    //    Vector3 FPOUp = FPO.pos.e2;
    //    Vector3 FPOLeft = FPO.pos.e1;


        
    //    FPOUp.y *= -1;
    //    FPOLeft.y *= -1;

    //    Vector3 FPODir = Vector3.Cross(FPOUp, FPOLeft);
        
    //    res.transform.localRotation = Quaternion.LookRotation(FPODir, FPOUp * -1);
    //    * */
    //    node.children = new FPONode[0];
    //    node.slots = new FPOSlot[0];
    //    node.meshes = new FPOmesh[0];
    //    if (parent.children == null)
    //    {
    //        parent.children = new FPONode[0];
    //    }

    //    List<FPOmesh> tmpMeshes = new List<FPOmesh>();
    //    for (int i = 0; i < FPO.images.Length; i++)
    //    {
    //        if (FPO.images[i].graph == 0xFFFFFFFF) continue;
    //        string meshGraph = GameDataHolder.GetNameById(PackType.MeshDB, FPO.images[i].graph);
    //        FpoGraphData fpoGraphData = GameDataHolder.GetResource<FpoGraphData>(PackType.MeshDB, meshGraph);

    //        for (int j = 0; j < fpoGraphData.lods.Length; j++)
    //        {
    //            FPOmesh meshData = new FPOmesh();
    //            meshData.meshName = GameDataHolder.GetNameById(PackType.MeshDB, fpoGraphData.lods[j]);
    //            meshData.variant = i;
    //            meshData.lod = j;

    //            tmpMeshes.Add(meshData);
    //        }

    //    }

    //    node.meshes = tmpMeshes.ToArray();
    //    if (FPO.tree_next != 0)
    //    {
    //        //List<FPONode> tmpList = new List<FPONode>();

    //        GetFPOSchema(ms, ref parent, offset + FPO.tree_next);
    //        //tmpList.AddRange(parent.children);
    //        //Debug.Log($"Size: {parent.children.Length} Name: {parent.name}");
    //        //tmpNode.name = "next " + tmpNode.name;
    //        //tmpList.Add(tmpNode);

    //        //parent.children = tmpList.ToArray();
    //    }

    //    if (FPO.tree_sub != 0)
    //    {
    //        //List<FPONode> tmpList = new List<FPONode>();

    //        GetFPOSchema(ms, ref node, offset + FPO.tree_sub);
    //        //Debug.Log($"Node name: {node.name} Node children: {node.children.Length}") ;

    //        //tmpList.AddRange(node.children);
    //        //tmpNode.name = "sub " + tmpNode.name;
    //        //tmpList.Add(tmpNode);
    //        //node.children = tmpList.ToArray();
    //    }

    //    node.slots = new FPOSlot[FPO.num_slots];
    //    for (int i = 0; i < FPO.num_slots; i++)
    //    {
    //        FPOSlot slotData = new FPOSlot();
    //        slotData.name = FPO.slots[i].name;
    //        slotData.pos = FPO.slots[i].pos.org;
    //        slotData.dir = FPO.slots[i].pos.GetE3();
    //        slotData.up = FPO.slots[i].pos.e2;

    //        slotData.up.y *= -1;
    //        slotData.dir.y *= -1;
    //        slotData.pos.y *= -1;
    //        slotData.up *= -1;

    //        /*
    //        Vector3 slotPos = slotData.pos.org;
    //        Vector3 slotDir = slotData.pos.GetE3();
    //        Vector3 slotUp = slotData.pos.e2;

    //        slotUp.y *= -1;
    //        slotDir.y *= -1;
    //        slotPos.y *= -1;
            
    //         slot.transform.localRotation = Quaternion.LookRotation(slotDir, slotUp * -1);
    //        */

    //        //slotData.pos.y *= -1;
    //        //slotData.up.y *= -1;
    //        //slotData.dir.y *= -1;
    //        slotData.slotId = (int)FPO.slots[i].slot_id;
    //        slotData.flags = FPO.slots[i].flags.ToInt();

    //        node.slots[i] = slotData;

    //        //Debug.Log($"Adding slot {slotData.slotId} {slotData.name} to {node.name}");
    //    }

    //    //Debug.Log($"FPO generated {node}");
    //    List<FPONode> tmpList = new List<FPONode>();
    //    tmpList.AddRange(parent.children);
    //    tmpList.Add(node);
    //    parent.children = tmpList.ToArray();

    //    return;
    //}

    //public static GameObject GetFPO(Stream ms, string name, long offset = 0, GameObject parent = null)
    //{
    //    FpoData FPO;
    //    string cacheKey = "FPODATA*" + name;
    //    /*if (GameDataCache.isPresentStatic(cacheKey))
    //    {
    //        FPO = GameDataCache.GetCacheStatic<FpoData>(cacheKey);
    //        ms.Seek(GameDataCache.GetCacheStatic<long>("FPODATA*" + name + "*offset"),SeekOrigin.Begin);
    //    } else
    //    {
    //        FPO = GetFPOData(ms, (uint)offset);
    //        GameDataCache.AddCacheStatic("FPODATA*" + name, FPO);
    //        GameDataCache.AddCacheStatic("FPODATA*" + name + "*offset" , ms.Position);
    //    }*/
    //    FPO = GetFPOData(ms, (uint)offset);
    //    //FpoGraphData fpoGraphData = GameDataHolder.GetResource<FpoGraphData>(PackType.MeshDB, FPO.images[0].graph);
    //    GameObject res = new GameObject();
    //    if (parent != null) res.transform.parent = parent.transform;
    //    res.name = name + " " + FPO.name.ToString("X8");
    //    Vector3 tmpPos = FPO.pos.org;
    //    //  tmpPos.x = 0 - tmpPos.x;
    //    //tmpPos.y = 0 - tmpPos.y;
    //    //tmpPos.y = 0 - tmpPos.y;
    //    tmpPos.y *= -1;
    //    //  tmpPos.z = 0 - tmpPos.z;
    //    res.transform.localPosition = tmpPos;

    //    /* Из исходников:
    //     * 
    //     * void Position::SetMatrix34(Matrix34f &m){
    //          m.pos=org;
    //        m.tm[0]=e1;
    //        m.tm[1]=e2;
    //        m.tm[2]=GetE3();
    //    }

    //        Vector3f &Org()   { return tm.pos;   }
    //        Vector3f &Left()  { return tm.tm[0]; }
    //        Vector3f &Up()    { return tm.tm[1]; }
    //        Vector3f &Dir()   { return tm.tm[2]; }

    //        e1 -> Left
    //        e2 -> Up
    //        e3 -> Dir
    //     */
    //    //Quaternion parentRotation = (parent == null) ? Quaternion.Euler(0,0,0):parent.transform.localRotation;
    //    //Quaternion parentRotation = Quaternion.Euler(0, 0, 0);
    //    //Quaternion parentRotation = res.transform.rotation;

    //    /*
    //    //e1 - dir, e2 - up?
    //    Vector3 FPODir = FPO.pos.e1;
    //    FPODir.z = 0-FPODir.z;
    //    Vector3 FPOUp = FPO.pos.e2;
    //    FPOUp.z = 0 - FPOUp.z;
    //    //res.transform.rotation = Quaternion.FromToRotation(parentRotation*new Vector3(1, 0, 0), FPODir);
    //    //res.transform.rotation *= Quaternion.FromToRotation(parentRotation*new Vector3(0, 1, 0), FPOUp);
    //    //res.transform.localRotation *= Quaternion.FromToRotation(parentRotation * new Vector3(0, 0, 1), FPO.pos.GetE3());

    //    //Vector3 localE1 = res.transform.localRotation * new Vector3(1, 0, 0);
    //    //Vector3 localE2 = res.transform.localRotation * new Vector3(0, 1, 0);
    //    */

    //    //Vector3 FPODir = FPO.pos.GetE3();
    //    Vector3 FPOUp = FPO.pos.e2;
    //    Vector3 FPOLeft = FPO.pos.e1;


    //    /*FPODir.y *= -1;
    //    FPOUp.y *= -1;*/
    //    FPOUp.y *= -1;
    //    FPOLeft.y *= -1;

    //    Vector3 FPODir = Vector3.Cross(FPOUp, FPOLeft);

    //    //FPODir *= -1;
    //    //FPOUp *= -1;



    //    Quaternion correctionRotation = (res.transform.parent == null) ? res.transform.root.rotation : res.transform.parent.rotation;


    //    //res.transform.rotation = Quaternion.LookRotation(correctionRotation * FPODir, correctionRotation * FPODir);
    //    //res.transform.rotation = Quaternion.LookRotation(correctionRotation * FPODir, correctionRotation * FPOUp * -1);
    //    res.transform.localRotation = Quaternion.LookRotation(FPODir, FPOUp * -1);
    //    FpoController fpoController = res.AddComponent<FpoController>();
    //    fpoController.SetFpoData(FPO);
    //    fpoController.SetGraph(0);

    //    if (FPO.tree_next != 0)
    //    {
    //        GameObject nextModule = GetFPO(ms, "tmp-next", offset + FPO.tree_next, parent);
    //        //nextModule.transform.parent = parent.transform;
    //    }
    //    if (FPO.tree_sub != 0)
    //    {
    //        GameObject subModule = GetFPO(ms, "tmp-sub", offset + FPO.tree_sub, res);
    //        //subModule.transform.parent = res.transform;

    //    }
    //    foreach (SlotData slotData in FPO.slots)
    //    {
    //        GameObject slot = new GameObject();
    //        slot.name = "Slot " + slotData.slot_id.ToString();
    //        slot.transform.parent = res.transform;
    //        Vector3 slotPos = slotData.pos.org;
    //        Vector3 slotDir = slotData.pos.GetE3();
    //        Vector3 slotUp = slotData.pos.e2;
    //        slotUp.y *= -1;
    //        slotDir.y *= -1;
    //        slotPos.y *= -1;
    //        slot.transform.localPosition = slotPos;
    //        slot.transform.localRotation = Quaternion.LookRotation(slotDir, slotUp * -1);

    //    }
    //    return res;
    //}
    public static FpoData GetFPOData(Stream ms, uint offset)
    {
        FpoData FPO = new FpoData();
        ms.Seek(offset, SeekOrigin.Begin);

        FPO.flags = StormFileUtils.ReadStruct<Flags>(ms, (int)ms.Position);
        FPO.name = StormFileUtils.ReadStruct<crc32>(ms, (int)ms.Position);
        FPO.num_slots = StormFileUtils.ReadStruct<uint>(ms, (int)ms.Position);
        FPO.pos = StormFileUtils.ReadStruct<Position>(ms, (int)ms.Position);

        FPO.images = new ImageData[4];
        for (int i = 0; i < FPO.images.Length; i++)
        {
            FPO.images[i] = StormFileUtils.ReadStruct<ImageData>(ms, (int)ms.Position);
        }
        FPO.tree_next = StormFileUtils.ReadStruct<uint>(ms, (int)ms.Position);
        FPO.tree_sub = StormFileUtils.ReadStruct<uint>(ms, (int)ms.Position);

        FPO.slots = new SlotData[FPO.num_slots];
        for (int i = 0; i < FPO.num_slots; i++)
        {
            FPO.slots[i] = StormFileUtils.ReadStruct<SlotData>(ms, (int)ms.Position);
        }

        if (FPO.tree_next != 0)
        {
            //Debug.Log("Loading next @ " + FPO.tree_next);
            FPO.next = GetFPOData(ms, offset + FPO.tree_next);
        }
        if (FPO.tree_sub != 0)
        {
            //Debug.Log("Loading sub @ " + FPO.tree_sub);
            FPO.sub = GetFPOData(ms, offset + FPO.tree_sub);
        }

        return FPO;

    }
}
