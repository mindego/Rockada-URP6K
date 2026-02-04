using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer.Explorer;
using System;
using System.Runtime.InteropServices;
using System.Security.Policy;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static StormMesh;
using crc32 = System.UInt32;

public class StormTestCalculate : Editor
{
    private static void DebugObjId(ObjId id)
    {
        Debug.LogFormat("Converted ObjId: {0} type {1}",id,id.GetType());
        Debug.LogFormat("Converted ObjId: " + id.ToString());
    }
    [MenuItem("Tests/Calculate/crc32 to ObjId")]
    public static void TestObjId()
    {
        crc32 tempCRC = 0x77F354ED;
        DebugObjId(tempCRC);
        string tempName = "zazizga";
        DebugObjId(tempName);
    }
    [MenuItem("Tests/Calculate/Phong Power")]
    public static void CalcPhong()
    {
        //float mat_power = 20; //Glass
        //float mat_power = 1; //Beton
        Debug.LogFormat("Calculated smoothness for power {0} is {1}",20, GetSmoothness(20));
        Debug.LogFormat("Calculated smoothness for power {0} is {1}", 1, GetSmoothness(1));
        Debug.Log(Mathf.Pow(2f / 22f, 0.25f));
        Debug.Log(Mathf.Pow(81, 0.25f));
    }

    public static float GetSmoothness(float power)
    {
        return Mathf.Pow(2 / (power + 2),1/4f);
    }
}

public class StormTestUP: Editor
{
    [MenuItem("Tests/Calculate/Marshal.SizeOf")] 
    public static void ShowMarshalSize()
    {
        //        Debug.Log(Marshal.SizeOf(typeof(MaterialData)));
        //Debug.Log(Marshal.SizeOf<FacetGroup>());
        Debug.Log(Marshal.SizeOf<MaterialData>());
    }
    [MenuItem("Tests/RoundUp")]
    public static void TestClamps()
    {
        //uint Flags = RoFlags.FSI_ROUND_UP;
        uint Flags = RoFlags.FSI_ROUND_DOWN;
        int nImage = 2;
        int i;
        string[] r_images = new string[4];
        r_images[0] = "0";
        r_images[1] = null;
        r_images[2] = null;
        r_images[3] = "3";


        if (r_images[nImage] == null)
        {

            if ((Flags & RoFlags.FSI_ROUND_UP) != 0)
            {
                for (i = nImage - 1; i >= 0; i--)
                    if (r_images[i] != null) { nImage = i; break; }
            }
            if ((Flags & RoFlags.FSI_ROUND_DOWN) != 0)
            {
                for (i = nImage + 1; i < 4; i++)
                    if (r_images[i] != null) { nImage = i; break; }
            }
        }
        Debug.LogFormat("Final value: {0}", nImage);
    }
}