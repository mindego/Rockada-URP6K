using UnityEngine;
using UnityEditor;
using System.IO;
using System.Threading.Tasks;

//[CustomEditor(typeof(MaterialImportScript))]
//public class MaterialImportScriptEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();
//        MaterialImportScript myScript = (MaterialImportScript)target;

//        if (GUILayout.Button("Load by name"))
//        {
//            //myScript.ImportMaterialAsset();
//            ImportMaterial(myScript.MaterialName);
//        }

//        if (GUILayout.Button("Import All"))
//        {
//            Debug.Log("STUB! Importing _only_ material names");
//            ImportAllMaterials();
//        }
//    }

//    private async void ImportAllMaterials()
//    {
//        string[] list = GameDataHolder.ListContent(PackType.MaterialsDB);

//        Debug.Log("Importing " + list.Length + " items. It will take some time");
//        AssetDatabase.StartAssetEditing();

//        foreach (string MaterialName in list)
//        {
//            ImportMaterial(MaterialName);
            
//        }
//        await Task.Yield();
//        AssetDatabase.StopAssetEditing();

//    }
//    private void ImportMaterial(string MaterialName)
//    {
//        Material material = GameDataHolder.GetResource<Material>(PackType.MaterialsDB, MaterialName);
        
//        AssetDatabase.CreateAsset(material, "Assets/Materials/" + MaterialName + ".mat");
//    }
//    private void ImportMaterialOld(string MaterialName)
//    {
//        Debug.Log("Importing " + MaterialName);

//        Stream ms = GameDataHolder.GetResource<Stream>(PackType.MaterialsDB, MaterialName);
//        MaterialImport.D3DMATERIAL7 mat = MaterialImport.GetD3DMATERIAL7(ms);

//        ms.Close();
//        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
//        Material material = new Material(shader);
//        material.name = MaterialName;
//        //material.color = mat.diffuse.ToColor();
//        material.SetColor("_BaseColor", mat.diffuse.ToColor());
//        //material.SetColor("_SpecColor",mat.specular.ToColor());
        
//        //material.EnableKeyword("_EMISSION");
//        //material.SetColor("_EmissionColor", mat.emissive.ToColor());

//        //material.SetFloat("_WorkflowMode", 0f);
//        AssetDatabase.CreateAsset(material, "Assets/Materials/"+MaterialName+".mat");
        
//    }

//}
