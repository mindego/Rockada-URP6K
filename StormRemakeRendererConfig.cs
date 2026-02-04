using UnityEngine;

[CreateAssetMenu(fileName = "StormRemakeRendererConfig", menuName = "ScriptableObjects/StormRemakeRendererConfig", order = 1)]
[System.Serializable]
public class StormRemakeRendererConfig : ScriptableObject
{
    [Header("Environment materials")]
    public Material WaterMaterial;
    public Material SkyMaterial;

    public Shader LitShader; // E:\Unity\Wedge\Rockada URP6K\Library\PackageCache\com.unity.render-pipelines.high-definition\Runtime\Material\Lit\Lit.shader

    //[Tooltip("Materials for particle emmiters")]
    [Header("Materials for particle emitters")]
    public Material PD_ADDA;
    public Material PD_ATEXTURE;
    public Material PD_ATEXTUREINV;
    public Material PD_VECTOR;

    //[Tooltip("Material templates for meshes")]
    [Header("Material templates for meshes")]
    public Material Solid;
    public Material Transparent;
    public Material TransparentUnlit;
    public Material TransparentAdd;
    public Material TransparentBlend;
    public Material UI;
    public Material Refractive;
    public Material DecalMaterial;
    public Material TerrainMaterial;

    [Header("Prefabs")]
    public GameObject SunPrefab;
    public GameObject MoonPrefab;
    public GameObject StrategicGUI;
}