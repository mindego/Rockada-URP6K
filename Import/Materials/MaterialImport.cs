using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using static D3DEMULATION;
using static MaterialStorage;
using static renderer_dll;

public class MaterialImport
{
    //private const string DEFAULT_MATERIAL = "Universal Render Pipeline/Lit";
    //private const string DEFAULT_PHONG_MATERIAL = "Universal Render Pipeline/Simple Lit";
    //private static Dictionary<string, Material> materialcache = new Dictionary<string, Material>();

    private static MaterialImport instance;

    public MaterialImport GetInstance()
    {
        if (instance == null) instance = new MaterialImport();
        return instance;
    }

    public static Mtl GetPhongMaterial(Stream ms, string name = "")
    {
        /* из editmtl.cpp
         * Diffuse - цвет источника, на который влияет расстояния.
Ambient - цвет источника, на который не влияет расстояние/угол.
Specular - цвет источника, зависящий от угла нормали (блики).
Attenuation(0, 1, 2), Range - параметр затухания.
        */
        int multiplier = 1;
        byte[] buffer = new byte[4];

        ms.Seek(0, SeekOrigin.Begin);

        Mtl PhongMaterialData = new Mtl();
        PhongMaterialData.diffuse = GetVector3(ms) * multiplier;
        PhongMaterialData.diffuse_a = GetFloat(ms) * multiplier;
        PhongMaterialData.ambient = GetVector3(ms) * multiplier;
        PhongMaterialData.ambient_a = GetFloat(ms) * multiplier;
        PhongMaterialData.specular = GetVector3(ms) * multiplier;
        PhongMaterialData.specular_a = GetFloat(ms) * multiplier;
        PhongMaterialData.emissive = GetVector3(ms) * multiplier;
        PhongMaterialData.emissive_a = GetFloat(ms) * multiplier;
        PhongMaterialData.power = GetFloat(ms);

        return PhongMaterialData;
    }

    public static D3DMATERIAL7 GetD3DMATERIAL7(Stream ms)
    {
        D3DMATERIAL7 StormMaterial = StormFileUtils.ReadStruct<D3DMATERIAL7>(ms);
        //Debug.Log(StormMaterial);
        return StormMaterial;
    }

    //public static Material GetMaterial(Mtl PhongMaterial,Texture2D texture)
    //{
    //    //Shader shader = Shader.Find(DEFAULT_MATERIAL);
    //    Material material = new Material(MainMenu.StaticDefaultShader);

    //    material.mainTexture = texture;

    //    return material;
    //}

    public static Material GetMaterial(Stream ms, uint materialId, uint textureId, bool isTransparent = false)
    {
        string cachekey = materialId.ToString("X8") + textureId.ToString("X8");
        if (materialcache.ContainsKey(cachekey)) return materialcache[cachekey];

        if (ms == null)
        {
            //            return default; 
            ms = dll_data.materials.GetBlock(Hasher.HshString("Beton")).myStream;
            if (ms == null) return default;
        }

        Material material;

        D3DMATERIAL7 mat = GetD3DMATERIAL7(ms);
        //Shader shader = Shader.Find(DEFAULT_MATERIAL);
        //material = StaticDefaultShader != null? new Material(StaticDefaultShader): new Material(Shader.Find("HDRP/Lit"));
        //material.SetColor("_BaseColor", mat.diffuse.ToColor());

        //if (mat.diffuse.a == 0) mat.diffuse.a = 1;

        //if ((mat.diffuse.a < 1) | isTransparent)
        if (isTransparent)
        {
            //material.SetColor("_BaseColor", mat.ambient.ToColor());
            //material.EnableKeyword("_SPECULAR_SETUP");
            //material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            //material.EnableKeyword("_ALPHABLEND_ON");

            //material.SetFloat("_WorkflowMode", 0f);
            //material.SetFloat("_Surface", 1f);
            //material.SetFloat("_DstBlend", 10f); // This makes trasnparent happen
            //material.SetFloat("_Mode", 2f); // This says make it transparent
            //material.SetFloat("_SrcBlend", 5f);
            //material.SetFloat("_ZWrite", 0f);
            ////material.SetFloat("_Smoothness", 1f);
            //material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            ////material.SetColor("_SpecColor", mat.specular.ToColor());
            //material.SetColor("_SpecColor", mat.ambient.ToColor());
            material = new Material(DefaultTransparent);
            D3DCOLORVALUE tmp = mat.ambient;
            tmp.a = 1 - mat.diffuse.a;
            material.SetColor("_BaseColor", tmp.ToColor());
            //material.SetColor("_BaseColor", mat.diffuse.ToColor());
            //material.SetColor("_BaseColor", mat.ambient.ToColorRGB());

            //material.SetColor("_SpecColor", mat.specular.ToColor());
            //material.SetColor("_EmissionColor", mat.emissive.ToColor());
            //material.SetMaterialType(MaterialId.LitStandard);
            //HDMaterial.SetSurfaceType(material, true);
            //material.SetColor("_SpecularColor", mat.specular.ToColorRGB());
        }
        else
        {
            //zi = (xi – min(x)) / (max(x) – min(x))
            //material.SetFloat("_Metallic", metallization);
            //material.EnableKeyword("_EMISSION");
            //material.SetColor("_EmissionColor", mat.emissive.ToColor());
            material = new Material(DefaultSolid);
            //material.SetMaterialType(MaterialId.LitSpecular);
            //material.SetColor("_SpecularColor", mat.specular.ToColorRGB());

        }
        //float metallization = mat.power * Mathf.Sqrt(mat.specular.r * mat.specular.r + mat.specular.g * mat.specular.g + mat.specular.b * mat.specular.b);
        //material.SetFloat("_Smoothness",metallization/33); //TODO Исправить на 

        //material.SetColor("_EmissionColor", mat.emissive.ToColor());
        //float smoothness = (mat.specular.r + mat.specular.g + mat.specular.b) / 3;
        //float smoothness = mat.power / 20; //20 - power материала Glass. А что может быть ровнее стекла? Велианский металл (22)
        //float smoothness = 1 - Mathf.Exp(-mat.power);
        //Beckmann distribution (inverted)
        //float smoothness = 1-Mathf.Sqrt(2.0f / (mat.specular.a + 2.0f));
        //Blinn - Phong power is defined as 2 / roughness ^ 4 - 2.
        float smoothness = 1 - Mathf.Pow(2 / (mat.power + 2), 1 / 4f);
        material.SetFloat("_Smoothness", smoothness);

        //float metallization = Mathf.Clamp(0,1,mat.power/33);
        //float metallization = (mat.specular.r + mat.specular.g + mat.specular.b) / 3;
        //float metallization = new Vector3(mat.specular.r, mat.specular.g, mat.specular.b).normalized.magnitude;
        float metallization = 1 - Mathf.Sqrt(2.0f / (mat.specular.a + 2.0f));
        material.SetFloat("_Metallic", metallization);
        //material.SetColor("_SpecularColor", mat.specular.ToColorRGB()); //HDRP

        material.name = dll_data.materials.CompleteObjId(materialId).name;

        if (textureId != 0xFFFFFFFF)
        {
            //material.mainTexture = dll_data.LoadTexture(textureId);
            material.mainTexture = dll_data.loadFpoTexture(textureId);
            //material.SetTexture("_SpecularColorMap", material.mainTexture);
            material.name += "#" + material.mainTexture.name;
            //material.SetTexture("_MaskMap", GenMaskTexture((Texture2D)material.mainTexture, mat));
        }

        Debug.Log("Material created " + material.name + " " + mat + "spec " + mat.specular.ToColorRGB() + " metallization " + metallization);
        HDMaterial.ValidateMaterial(material);

        materialcache.Add(cachekey, material);
        return material;
    }

    private static float GetColorDistance(Color A, Color B)
    {
        float myDistance = Mathf.Abs(A.r - B.r) + Mathf.Abs(A.g - B.g) + Mathf.Abs(A.b - B.b);
        myDistance = myDistance / 3;
        return myDistance;
    }

    private static Dictionary<string, Texture2D> maskCache = new Dictionary<string, Texture2D>();
    private static Texture2D GenMaskTexture(Texture2D source, D3DMATERIAL7 mat)
    {
        string maskName = source.name + "#mask";
        if (maskCache.ContainsKey(maskName)) return maskCache[maskName];
        Texture2D mask = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        mask.name = maskName;
        float metallic, ambientOcclusion, detailMask, smoothness;
        Color inPixelColor;
        Color outPixelColor;
        detailMask = 0;
        ambientOcclusion = 1;
        smoothness = (mat.specular.r + mat.specular.g + mat.specular.b) / 3;
        metallic = smoothness * mat.power * mat.specular.a;
        for (int y = 0; y < source.height; y++)
        {
            for (int x = 0; x < source.width; x++)
            {
                inPixelColor = source.GetPixel(x, y);

                //ambientOcclusion = 1-GetColorDistance(inPixelColor, mat.ambient.ToColorRGB());
                //metallic = 1-GetColorDistance(inPixelColor, mat.specular.ToColorRGB());
                //smoothness = 1 - GetColorDistance(inPixelColor, mat.specular.ToColorRGB());
                outPixelColor = GetMaskPixel(metallic, ambientOcclusion, detailMask, smoothness);
                mask.SetPixel(x, y, outPixelColor);
            }
        }

        mask.Compress(true);
        mask.Apply();
        maskCache.Add(maskName, mask);
        return mask;
    }

    private static Color GetMaskPixel(float metallic, float ambientOcclusion, float detailMask, float smoothness)
    {
        return new Color(metallic, ambientOcclusion, detailMask, smoothness);
    }
    //public static Material GetMaterial(Stream ms, string name)
    //{
    //    Material material;
    //    if (!name.Contains('#'))
    //    {
    //        MaterialImport.D3DMATERIAL7 mat = MaterialImport.GetD3DMATERIAL7(ms);
    //        Shader shader = Shader.Find(DEFAULT_MATERIAL);
    //        //Shader shader = Shader.Find("HDRP/Lit");
    //        material = new Material(shader);
    //        material.name = name;
    //        material.SetColor("_BaseColor", mat.diffuse.ToColor());
    //        //material.SetColor("_SpecColor",mat.specular.ToColor());

    //        //material.EnableKeyword("_EMISSION");
    //        //material.SetColor("_EmissionColor", mat.emissive.ToColor());

    //        //material.SetFloat("_WorkflowMode", 0f);
    //        if (mat.diffuse.a == 0) mat.diffuse.a = 1;

    //        if (mat.diffuse.a < 1)
    //        {
    //            material.EnableKeyword("_SPECULAR_SETUP");
    //            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
    //            material.EnableKeyword("_ALPHABLEND_ON");

    //            material.SetFloat("_WorkflowMode", 0f);
    //            material.SetFloat("_Surface", 1f);
    //            material.SetFloat("_DstBlend", 10f); // This makes trasnparent happen
    //            material.SetFloat("_Mode", 2f); // This says make it transparent
    //            material.SetFloat("_SrcBlend", 5f);
    //            material.SetFloat("_ZWrite", 0f);
    //            material.SetFloat("_Smoothness", 1f);
    //            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

    //            material.SetColor("_SpecColor", mat.specular.ToColor());
    //        }
    //        return material;
    //    }
    //    string[] parts = name.Split('#');

    //    //Material templateMaterial = GameDataHolder.GetResource<Material>(PackType.MaterialsDB, parts[0]);
    //    //material = GameDataHolder.GetResource<Material>(PackType.MaterialsDB, parts[0]);

    //    Stream st = Renderer.sdr.materials.GetBlock(parts[0]).myStream;
    //    material = GetMaterial(st, parts[0]);
    //    //material = GetMaterial(ms, parts[0]);
    //    //Texture2D texture = GameDataHolder.GetResource<Texture2D>(PackType.TexturesDB, parts[1]);
    //    Texture2D texture = Renderer.sdr.LoadTexture(parts[1]);

    //    //Debug.Log(texture.name);
    //    material.name = name;
    //    material.mainTexture = texture;

    //    return material;
    //}


    public static Texture2D GetMetallicMap(Mtl PhongMaterial, Texture2D texture)
    {
        Texture2D resTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false)
        {
            name = texture.name + "_metallic"
        };
        Color[] pixels = texture.GetPixels();
        Color[] outPixels = new Color[pixels.Length];
        Color inColor = pixels[300];
        Color outColor;
        Color PhongColorAmbient = new Color(PhongMaterial.ambient.x, PhongMaterial.ambient.y, PhongMaterial.ambient.z);
        Color PhongColorSpecular = new Color(PhongMaterial.specular.x, PhongMaterial.specular.y, PhongMaterial.specular.z, PhongMaterial.specular_a / 2);
        Debug.Log(("Example Pixel data: ", pixels[300]));
        Debug.Log(("Phong Spec: ", PhongColorSpecular));
        Debug.Log(("DiffA: ", PhongColorSpecular - pixels[300]));
        Debug.Log(("DiffB: ", pixels[300] - PhongColorSpecular));
        Debug.Log(("Constructed: " + (new Color(inColor.r - PhongColorSpecular.r, inColor.g - PhongColorSpecular.g, inColor.b - PhongColorSpecular.b))));
        Debug.Log(("Distance: ", GetColorDistance(inColor, PhongColorSpecular)));

        for (int i = 0; i < pixels.Length; i++)
        {
            inColor = pixels[i];

            //outColor = inColor - PhongColorAmbient;
            //outColor = inColor;
            //outColor = PhongColorSpecular - inColor;
            //outColor = new Color(inColor.r - PhongColorSpecular.r, inColor.g - PhongColorSpecular.g, inColor.b - PhongColorSpecular.b);
            float distanceSpec = GetColorDistance(inColor, PhongColorSpecular);
            float distanceAmbient = GetColorDistance(inColor, PhongColorAmbient);
            outColor = new Color(distanceSpec, 0, 0, distanceAmbient);


            outPixels[i] = outColor;
        }
        resTexture.SetPixels(outPixels);
        resTexture.Apply();
        return resTexture;
    }

    private static Vector3 GetVector3(Stream ms)
    {
        byte[] buffer = new byte[4];
        Vector3 vector3 = new Vector3();

        vector3.x = GetFloat(ms);
        vector3.y = GetFloat(ms);
        vector3.z = GetFloat(ms);

        return vector3;
    }

    private static float GetFloat(Stream ms)
    {
        byte[] buffer = new byte[4];

        ms.Read(buffer);
        return BitConverter.ToSingle(buffer);
    }


    public struct Mtl
    {
        public Vector3 diffuse; public float diffuse_a;
        public Vector3 ambient; public float ambient_a;
        public Vector3 specular; public float specular_a;
        public Vector3 emissive; public float emissive_a;
        public float power;

        public override string ToString()
        {
            string res = "Diffuse: " + diffuse.ToString() + " : " + diffuse_a + "\n";
            res += "Ambient: " + ambient.ToString() + " : " + ambient_a + "\n";
            res += "Specular: " + specular.ToString() + " : " + specular_a + "\n";
            res += "Emissive: " + emissive.ToString() + " : " + emissive_a + "\n";
            res += "Power: " + power + "\n";

            return res;
        }
    }
}
