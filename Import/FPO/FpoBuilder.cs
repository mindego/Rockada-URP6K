//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Xml.Serialization;
//using UnityEditor;
//using UnityEngine;

//public class FpoBuilder
//{
//    private bool isLoaded = false;
//    //public UnityEngine.Object[] meshDB;
//    //public UnityEngine.Object[] texturesDB;
//    //public Hashtable meshDB;
//    //public Hashtable texturesDB;
//    //public Dictionary<string, Mesh> meshDB;
//    //public Dictionary<string, Texture2D> texturesDB;
//    public Dictionary<string, ImportMeshEditor.meshData> meshInfo;
//    //public ImportMeshEditor.meshData[] meshInfo;
//    public Material[] materialDB;

//    public FpoBuilder()
//    {
        
//    }

//    public static FpoImport.FPONode LoadXML(Stream stream)
//    {
//        XmlSerializer serializer = new XmlSerializer(typeof(FpoImport.FPONode));

//        FpoImport.FPONode schema = (FpoImport.FPONode)serializer.Deserialize(stream);

//        return schema;
//    }

//    public FpoImport.FPONode LoadXML(TextAsset xml)
//    {
//        MemoryStream ms = new MemoryStream(xml.bytes);

//          FpoImport.FPONode schema = LoadXML(ms);
//        ms.Close();
//        return schema;

//    }

//    public FpoImport.FPONode LoadXML(string FPOName)
//    {
//        string filename = "Assets/Database/FPOData/FPO-" + FPOName + ".xml";
        
//        if (!File.Exists(filename))
//        {
//            Debug.Log("XML not found for " + FPOName);
//            throw new IOException();
//            //return default;
//        }
//        LoadData();
//        FileStream ms = File.Open(filename, FileMode.OpenOrCreate);

//        FpoImport.FPONode schema = LoadXML(ms);
//        ms.Close();
//        return schema;
//    }

//    //    private void LoadDataOld()
//     //    {
//     //        if (isLoaded) return;

//    //        Object[] tmpmeshDB =  AssetDatabase.LoadAllAssetsAtPath("Assets/Meshes/uber.asset");
//    //        //meshDB = new Hashtable();
//    //        meshDB = new Dictionary<string, Mesh>();
//    //        foreach (Object tmpmesh in tmpmeshDB)
//    //        {
//    //            meshDB.Add(tmpmesh.name,(Mesh) tmpmesh);
//    //        }

//    //        Object[] tmptexturesDB = AssetDatabase.LoadAllAssetsAtPath("Assets/Textures/uber.asset");
//    //        // texturesDB = new Hashtable();
//    //        texturesDB = new Dictionary<string, Texture2D>();

//    //        foreach (Texture2D texture2D in tmptexturesDB)
//    //        {
//    //            texturesDB.Add(texture2D.name, texture2D);
//    //        }
//    //        XmlSerializer serializer = new XmlSerializer(typeof(ImportMeshEditor.meshData[]));
//    //        FileStream ms = File.Open("Assets/Database/MeshData.xml", FileMode.Open);

//    //        meshInfo = new Dictionary<string, ImportMeshEditor.meshData>();
//    //        ImportMeshEditor.meshData[]  tmpmeshInfo = (ImportMeshEditor.meshData[]) serializer.Deserialize(ms);
//    //        for (int i=0;i<tmpmeshInfo.Length;i++)
//    //        {
//    //            ImportMeshEditor.meshData meshData = tmpmeshInfo[i];

//    ////        foreach (ImportMeshEditor.meshData meshData in tmpmeshInfo) {
//    //            //Debug.Log(meshData.meshName);
//    //            if (meshData.meshName == null) //TODO! Выяснить, почему пустой meshData
//    //            {
//    //                //Debug.LogError("Empty name " + i + " of " + tmpmeshInfo.Length) ;
//    //                //Debug.Log("Prev: "+ tmpmeshInfo[i - 1]);
//    //                continue;

//    //            } else
//    //            {
//    //                meshInfo.Add(meshData.meshName, meshData);
//    //            }
//    //        }
//    //        ms.Close();
//    //        isLoaded = true;
//    //    }

//    private void LoadData(FpoImport.FPONode schema)
//    {
//        if (meshInfo == null) meshInfo = new Dictionary<string, ImportMeshEditor.meshData>();
//        StormMesh stormMesh;
//        ImportMeshEditor.meshData meshData;
//        foreach (FpoImport.FPOmesh mesh in schema.meshes)
//        {
//            if (meshInfo.ContainsKey(mesh.meshName)) continue;

//            stormMesh = GameDataHolder.GetResource<StormMesh>(PackType.MeshDB, mesh.meshName);
//            int mtlCount = stormMesh.materialData.Length; 
//            meshData.meshMaterials = new ImportMeshEditor.materialData[mtlCount];
//            //tmpMeshData.meshTextures = new string[mtlCount];
//            for (int i = 0; i < mtlCount; i++)  
//            {
//                ImportMeshEditor.materialData materialData = new ImportMeshEditor.materialData();
//                materialData.meshMaterial = GameDataHolder.GetNameById(PackType.MaterialsDB, stormMesh.materialData[i].mtl_id);
//                materialData.meshTexture = GameDataHolder.GetNameById(PackType.TexturesDB, stormMesh.materialData[i].bmp_id);
//                materialData.flags = stormMesh.materialData[i].flags.ToInt();
//                //tmpMeshData.meshMaterials[i] = GameDataHolder.GetNameById(PackType.MaterialsDB, stormMesh.materialData[i].mtl_id);
//                //tmpMeshData.meshTextures[i] = GameDataHolder.GetNameById(PackType.TexturesDB, stormMesh.materialData[i].bmp_id);
//                meshData.meshMaterials[i] = materialData;
//            }                                                      
//        }
//    }
//    private void LoadData()
//    {
//        if (isLoaded) return;
//        XmlSerializer serializer = new XmlSerializer(typeof(ImportMeshEditor.meshData[]));
//        FileStream ms = File.Open("Assets/Database/MeshData.xml", FileMode.Open);

//        meshInfo = new Dictionary<string, ImportMeshEditor.meshData>();
//        ImportMeshEditor.meshData[] tmpmeshInfo = (ImportMeshEditor.meshData[])serializer.Deserialize(ms);
//        for (int i = 0; i < tmpmeshInfo.Length; i++)
//        {
//            ImportMeshEditor.meshData meshData = tmpmeshInfo[i];

//            //        foreach (ImportMeshEditor.meshData meshData in tmpmeshInfo) {
//            //Debug.Log(meshData.meshName);
//            if (meshData.meshName == null) //TODO! Выяснить, почему пустой meshData
//            {
//                //Debug.LogError("Empty name " + i + " of " + tmpmeshInfo.Length) ;
//                //Debug.Log("Prev: "+ tmpmeshInfo[i - 1]);
//                continue;

//            }
//            else
//            {
//                meshInfo.Add(meshData.meshName, meshData);
//            }
//        }
//        ms.Close();
//        isLoaded = true;
//        return;
//    }
    

//    public GameObject BuildFPO(string FPOName, TextAsset xml)
//    {
//        //LoadData();
//        FpoImport.FPONode schema = LoadXML(xml);
//        return BuildFPO(FPOName, schema);
//    }

//    public GameObject BuildFPO(string FPOName, FpoImport.FPONode schema)
//    {
//        //LoadData(schema);

//        //GameObject StormObject = new GameObject
//        //{
//        //    name = FPOName
//        //};
//        GameObject StormObject = GameObjectSpool.GetFromSpoolFactory();
//        StormObject.SetActive(true);

//        StormObject.name = FPOName;
//        //GameObject node = BuildGameobject(schema,StormObject);
//        BuildGameobject(schema, StormObject);
//        //node.transform.parent = StormObject.transform;

//        return StormObject;
//    }

//    public GameObject BuildFPO(string FPOName)
//    {
//        FpoImport.FPONode schema = GameDataHolder.GetResource<FpoImport.FPONode>(PackType.FPODB, FPOName);
//        //FpoImport.FPONode schema = LoadXML(FPOName);
//        return BuildFPO(FPOName, schema);
//    }

//    //private GameObject BuildGameobject(FpoImport.FPONode schema, GameObject parent = null)
//    private void BuildGameobject(FpoImport.FPONode schema, GameObject parent = null)
//    {
//        //GameObject node = new GameObject();
//        GameObject node = GameObjectSpool.GetFromSpoolFactory();
//        if (node == null) throw new Exception("No objects in spool?");
//        node.SetActive(true);
//        node.name = schema.name;
//        if (parent != null)
//        {
//            node.transform.parent = parent.transform;
//        }

//        node.transform.localPosition = schema.pos;
//        if (schema.dir != Vector3.zero)
//        {
//            node.transform.localRotation = Quaternion.LookRotation(schema.dir, schema.up);
//        }

//        int lod = 0;
//        int variant = 0;
//        //string meshName = "Error mesh";
//        //string meshNameEnum = "Error mesh enum";

//        UnitImageData unitImageData = node.AddComponent<UnitImageData>();
//        unitImageData.lod = lod;
//        unitImageData.variant = variant;
//        unitImageData.setMeshes(schema.meshes);
//        unitImageData.updateImage(variant,lod,true);
//        //await Task.Yield();
//        //foreach (FpoImport.FPOmesh meshData in schema.meshes)
//        //{
//        //    if (meshData.meshName != null) meshNameEnum = meshData.meshName;
//        //    if (meshData.lod == lod & meshData.variant == variant) meshName = meshData.meshName;
//        //}

//        //Mesh mesh= GetMesh(meshName);
//        //if (mesh != null)
//        //{
//        //    MeshFilter mf = node.AddComponent<MeshFilter>();
//        //    MeshRenderer mr = node.AddComponent<MeshRenderer>();
//        //    mf.mesh = mesh;
//        //    mr.materials = GetMaterials(meshName);
//        //}

//        //string[] nodetree = meshNameEnum.Split('_');
//        //node.name = nodetree.Length > 2 ? nodetree[nodetree.Length - 3] : meshNameEnum;

//        foreach (FpoImport.FPONode child in schema.children)
//        {
//            BuildGameobject(child, node);
            
//        }

//        foreach (FpoImport.FPOSlot slotData in schema.slots)
//        {
//            //GameObject slot = new GameObject();
//            GameObject slot = GameObjectSpool.GetFromSpoolFactory();
//            slot.SetActive(true);
//            slot.name = "Slot " + slotData.name.ToString("X8");
//            slot.transform.parent = node.transform;
//            slot.transform.localPosition = slotData.pos;
//            if (slotData.dir != Vector3.zero)
//            {
//                slot.transform.localRotation = Quaternion.LookRotation(slotData.dir, slotData.up);
//            }
//            UnitSlotData tmpData = slot.AddComponent<UnitSlotData>();
//            tmpData.SetSlotData(slotData);
//        }
//        //return node;
//    }


//    /// <summary>
//    /// Сохраняет данные FPO, с указанными именем <see cref="FPOName"/> в XML файл
//    /// </summary>
//    /// <param name="FPOName">Имя FPO</param>
//    public static void SaveXML(string FPOName)
//    {
//        string dirname = "Assets/Database/FPOData";
//        string filename = dirname + "/FPO-" + FPOName + ".xml";
//        FpoImport.FPONode FPO = GameDataHolder.GetResource<FpoImport.FPONode>(PackType.FPODB, FPOName);

//        if (!Directory.Exists(dirname)) Directory.CreateDirectory(dirname);
//        if (File.Exists(filename)) File.Delete(filename);
//        Debug.Log(FPO);
//        FileStream ms = File.Open(filename, FileMode.OpenOrCreate);
//        XmlSerializer serializer = new XmlSerializer(FPO.GetType());

//        //serializer.Serialize(ms, GameDataHolder.GetResource<FpoImport.FPONode>(PackType.FPODB, FPOName));
//        serializer.Serialize(ms, FPO);
//        ms.Close();

//        Debug.Log("File saved: " + filename);
//    }

// }

//public class GameObjectSpool
//{
//    private List<GameObject> spoolData;
//    private static GameObjectSpool instance;
//    private GameObject GameObjectSpoolHolder;

//    private static GameObjectSpool GetInstance()
//    {
//        if (GameObjectSpool.instance == null) GameObjectSpool.instance = new GameObjectSpool(10000);
//        return GameObjectSpool.instance;
//    }

//    public GameObjectSpool(int count)
//    {
//        InitGameObjectSpool(count);
//    }
//    private void InitGameObjectSpool(int count)
//    {
//        GameObjectSpoolHolder = new GameObject("GameObject spool holder");
//        spoolData = new List<GameObject>();
//        AddToSpool(count);
//    }

//    private void AddToSpool(int count)
//    {
//        for (int i = 0; i < count; i++)
//        {
//            GameObject tmpObject = new GameObject("Spool " + i);
//            tmpObject.transform.parent = GameObjectSpoolHolder.transform;
//            tmpObject.SetActive(false);
//            spoolData.Add(tmpObject);
//        }
//    }
//    private GameObject GetFromSpool()
//    {
//        if (GameObjectSpoolHolder == null)
//        {
//            InitGameObjectSpool(5);
//        }
//        if (spoolData.Count <= 0)
//        {
//            AddToSpool(5);
//        }

//        GameObject output= spoolData.Last();
//        output.transform.parent = null;
//        spoolData.Remove(output);
//        return output;
//    }

//    public static GameObject GetFromSpoolFactory()
//    {
//        GameObjectSpool spool = GameObjectSpool.GetInstance();
//        return spool.GetFromSpool();
//    }

//}