using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Text;

//[CustomEditor(typeof(ImportMesh))]
//public class ImportMeshEditor : Editor
//{
//    public struct materialData
//    {
//        public string meshMaterial;
//        public string meshTexture;
//        public int flags;

//        public override string ToString()
//        {
//            StringBuilder sb = new StringBuilder();
//            sb.AppendFormat("Material: {0}\n", meshMaterial);
//            sb.AppendFormat("Texture: {0}\n", meshTexture);
//            foreach (int flag in Enum.GetValues(typeof(MaterialFlags)))
//            {
//                string flagName = Enum.GetName(typeof(MaterialFlags), flag);
//                sb.AppendFormat("Flag {0} : {1}\n",flagName,(flags & flag) != 0 ? "Set":"Unset");
//            }

//            return sb.ToString();
//        }
//    }

//    public enum MaterialFlags
//    {
//        MF2_TEXTURED = 0x0001,
//        MF2_TRANSPARENT = 0x0002,
//        MF2_ADDITIVE = 0x0004,
//        MF2_BUMP = 0x0010,
//        MF2_ENVMAP = 0x0020,
//        MF2_TWOSIDED = 0x0040
//    }
//    public struct meshData
//    {
//        public int flags;
//        public string meshName;
//        //public string[] meshMaterials;
//        //public string[] meshTextures;
//        public materialData[] meshMaterials;

//        public override string ToString()
//        {
//            string res = $"Meshname: {meshName}\n";
//            foreach (materialData materialData in meshMaterials)
//            {
//                res+=$"Material {materialData.meshMaterial} Texture {materialData.meshTexture}";
//            }
//            return res;
//        }
//    }
//    public struct meshDataHolder
//    {
//        public meshData[] meshDatas;
//    }
//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();
//        ImportMesh myScript = (ImportMesh)target;

//        if (GUILayout.Button("Import by Name"))
//        {
//            ImportMeshByName(myScript.MeshName);
//        }
//        if (GUILayout.Button("Import All"))
//        {
//            ImportAllMeshes();
//        }
//        //if (GUILayout.Button("Export mesh by name"))
//        if (GUILayout.Button("Export All meshes  to xml"))
//        {
//            bool failsafe = false;
//            if (failsafe) return;
//            ExportAllMeshXML();
//        }

//        if (GUILayout.Button("Test load from xml"))
//        {
//            LoadXML();
//        }
//    }

//    private meshData[] LoadXML()
//    {
//        XmlSerializer ser = new XmlSerializer(typeof(meshData[]));
//        FileStream ms = File.Open("Assets/Database/MeshData.xml", FileMode.Open);
//        meshData[] data = (meshData[])ser.Deserialize(ms);
//        ms.Close();

//        Debug.Log(data.Length);
//        return data;
//    }
//    private meshData GetMeshData(string meshName)
//    {
//        StormMesh stormMesh;
//        meshData tmpMeshData = new meshData();
//        try
//        {
//            stormMesh = GameDataHolder.GetResource<StormMesh>(PackType.MeshDB, meshName);
//            tmpMeshData.meshName = meshName;


//            int mtlCount = stormMesh.materialData.Length;
//            tmpMeshData.meshMaterials = new materialData[mtlCount];
//            //tmpMeshData.meshTextures = new string[mtlCount];
//            for (int i = 0; i < mtlCount; i++)
//            {
//                materialData materialData = new materialData();
//                materialData.meshMaterial = GameDataHolder.GetNameById(PackType.MaterialsDB, stormMesh.materialData[i].mtl_id);
//                materialData.meshTexture = GameDataHolder.GetNameById(PackType.TexturesDB, stormMesh.materialData[i].bmp_id);
//                materialData.flags = stormMesh.materialData[i].flags.ToInt();
//                //tmpMeshData.meshMaterials[i] = GameDataHolder.GetNameById(PackType.MaterialsDB, stormMesh.materialData[i].mtl_id);
//                //tmpMeshData.meshTextures[i] = GameDataHolder.GetNameById(PackType.TexturesDB, stormMesh.materialData[i].bmp_id);
//                tmpMeshData.meshMaterials[i] = materialData;
//            }
//        }
//        catch
//        {
//            return default;
//        }
//        return tmpMeshData;
//    }
//    private void ExportAllMeshXML()
//    {
//        List<meshData> meshDatas = new List<meshData>();
//        UnityEngine.Object[] data = AssetDatabase.LoadAllAssetsAtPath("Assets/Meshes/uber.asset");

//        foreach (Mesh mesh in data)
//        {
//            meshData tmpMeshData = GetMeshData(mesh.name);

//            meshDatas.Add(tmpMeshData);
//        }

//        meshData[] tmpArray = meshDatas.ToArray();
//        XmlSerializer ser = new XmlSerializer(typeof(meshData[]));
//        FileStream ms = File.Open("Assets/Database/MeshData.xml", FileMode.OpenOrCreate);
//        ser.Serialize(ms, tmpArray);
//        ms.Close();
//        Debug.Log("Exported items: " + tmpArray.Length);
//    }

//    public void ImportMeshById(string MeshId)
//    {
//        uint intMeshId = uint.Parse(MeshId, System.Globalization.NumberStyles.HexNumber);
//        Debug.Log("Importing " + intMeshId);
//        ImportMeshById(intMeshId);
//    }
//    public void ImportMeshById(uint MeshId)
//    {
//        Mesh mesh = GameDataHolder.GetResource<Mesh>(PackType.MeshDB, MeshId.ToString());

//    }

//    public void ImportMeshByName(string MeshName)
//    {
//        Mesh mesh = GameDataHolder.GetResource<Mesh>(PackType.MeshDB, MeshName);
//        AssetDatabase.CreateAsset(mesh, "Assets/Meshes/" + MeshName + ".asset");

//    }

//    public void CreateMeshAsset(string MeshName)
//    {
//        Mesh mesh = GameDataHolder.GetResource<Mesh>(PackType.MeshDB, MeshName);
//        AssetDatabase.CreateAsset(mesh, "Assets/Meshes/uber.asset");
//    }
//    public void AddMeshToAsset(string MeshName)
//    {
//        Mesh mesh = GameDataHolder.GetResource<Mesh>(PackType.MeshDB, MeshName);
//        AssetDatabase.AddObjectToAsset(mesh, "Assets/Meshes/uber.asset");
//    }
//    public void ImportAllMeshes()
//    {
//        bool failsafe = true;
//        string[] list = GameDataHolder.ListContent(PackType.MeshDB);

//        Debug.Log("Importing " + list.Length + " items. It will take some time");

//        if (failsafe)
//        {
//            Debug.Log("Failsafe!"); return;
//        }

//        AssetDatabase.StartAssetEditing();
//        bool NewOne = true;
//        foreach (string itemName in list)
//        {
//            try
//            {
//                if (NewOne)
//                {
//                    CreateMeshAsset(itemName);
//                    NewOne = false;
//                }
//                else
//                {
//                    AddMeshToAsset(itemName);
//                }
//                //ImportMeshByName(itemName);
//            }
//            catch
//            {
//                Debug.Log("Skipped " + itemName);
//            }

//        }
//        AssetDatabase.StopAssetEditing();
//        AssetDatabase.ImportAsset("Assets/Meshes/uber.asset");
//    }

//}
