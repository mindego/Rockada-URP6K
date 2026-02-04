using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;

//[CustomEditor(typeof(FpoLoaderEngine))]
//public class FpoLoaderEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();
//        FpoLoaderEngine myScript = (FpoLoaderEngine)target;

//        GUILayout.BeginHorizontal();
//        if (GUILayout.Button("Load by name"))
//        {
//            myScript.GetGameObject();
//        }
//        if (GUILayout.Button("Load by Id"))
//        {
//            myScript.GetGameObject();
//        }
//        GUILayout.EndHorizontal();
//        if (GUILayout.Button("Load by Enum"))
//        {
//            //string[] list = GameDataHolder.ListContent(PackType.FPODB);
//            //int pos = (int) myScript.UseFPO;
//            //myScript.FpoName = list[pos];
//            myScript.GetGameObject();
//        }
//        //if (GUILayout.Button("Load by gData object name"))
//        //{
//        //    //string[] list = GameDataHolder.ListContent(PackType.FPODB);
//        //    //int pos = (int) myScript.UseFPO;
//        //    //myScript.FpoName = list[pos];
//        //    myScript.GenerateGameObjectFromgData();
//        //}

//        //GUILayout.BeginHorizontal();
//        //if (GUILayout.Button("Save XML"))
//        //{
//        //    FpoBuilder.SaveXML(myScript.FpoName);
//        //}
//        //if (GUILayout.Button("Load XML"))
//        //{
//        //    FpoBuilder fpoBuilder = new FpoBuilder();
//        //    fpoBuilder.LoadXML(myScript.FpoName);
//        //}
//        //GUILayout.EndHorizontal();
//        //if (GUILayout.Button("Import gameobject from XML"))
//        //{
//        //    FpoBuilder fpoBuilder = new FpoBuilder();
//        //    fpoBuilder.BuildFPO(myScript.FpoName);
//        //}

//        //if (GUILayout.Button("Generate All FPO XMLs"))
//        //{
//        //    GenerateAllFPOXMLS();
//        //}
//    }

//    //public async void GenerateAllFPOXMLS()
//    //{
//    //    string[] list = GameDataHolder.ListContent(PackType.FPODB);
//    //    AssetDatabase.StopAssetEditing();
//    //    foreach (string item in list)
//    //    {
//    //        FpoBuilder.SaveXML(item);
//    //        await Task.Yield();

//    //    }
//    //    Debug.Log("XML generation finished. Importing assets");
//    //    AssetDatabase.StartAssetEditing();
//    //    await Task.Yield();
//    //}

//}
