using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public static class EngineDebug
{
    private static Dictionary<int, Material> DebugMatCache = new Dictionary<int, Material>();
    internal static void DebugSphere(Vector3 pnt, string name, float radius = 1, float ttl = 0, Material material = null, bool CloneMaterial = true)
    {
        if (material == null) material = new Material(MaterialStorage.DefaultTransparent);
        int hash = material.GetHashCode();

        if (CloneMaterial)
        {
            material = new Material(material);
            //material.SetFloat("_Smoothness", 0.5f); //Блестеть ударная волна не должна!

        }
        if (!DebugMatCache.ContainsKey(hash)) DebugMatCache.Add(hash, material);
        material = DebugMatCache[hash];
        GameObject gobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gobj.name = name;
        gobj.transform.position = Engine.ToCameraReference(pnt);
        gobj.transform.localScale = new Vector3(radius, radius, radius);
        SphereFader ss = gobj.AddComponent<SphereFader>();
        ss.Setup(ttl, radius, material);
    }



    internal static void DebugLine(Vector3 src, Vector3 dst, Color lineColor, float duration = 0)
    {
        Debug.DrawLine(Engine.ToCameraReference(src), Engine.ToCameraReference(dst), lineColor, duration);
    }

    public static void DebugConsole(string text)
    {
        Debug.Log(text);
    }

    internal static void DebugConsoleFormat(string fmt, params object[] args)
    {
        Debug.LogFormat(fmt, args);
    }
}

public class SphereRefractor : MonoBehaviour, IDebugSphereVisializer
{
    private float ttl;
    private float maxttl;
    private float radius;
    private Material material;

    public void Setup(float _ttl, float _radius, Material _material)
    {
        maxttl = _ttl;
        ttl = _ttl;
        radius = _radius;
        material = new Material(_material);

        HDMaterial.SetSurfaceType(material, true);

        HDMaterial.ValidateMaterial(material);
        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        mr.material = material;


    }

    public void UpdateVisual()
    {
        ttl -= Time.deltaTime;
        if (ttl < 0)
        {
            GameObject.Destroy(this.gameObject);
            return;
        }
        float scale = maxttl == 0 ? 1 : ttl / maxttl;

        Color mycolor = material.GetColor("_BaseColor");
        mycolor.a = scale;
        material.SetColor("_BaseColor", mycolor);
        //this.gameObject.transform.localScale=new Vector3(radius*scale, radius*scale, radius* scale);
    }
    public void FixedUpdate()
    {
        UpdateVisual();
    }
}

public interface IDebugSphereVisializer
{
    public void Setup(float _ttl, float _radius, Material _material);
    public void UpdateVisual();
}
public class SphereFader : MonoBehaviour, IDebugSphereVisializer
{
    private float ttl;
    private float maxttl;
    private float radius;
    private Material material;

    public void Setup(float _ttl, float _radius, Material _material)
    {
        maxttl = _ttl;
        ttl = _ttl;
        radius = _radius;
        material = new Material(_material);

        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        mr.material = material;
        mr.shadowCastingMode = ShadowCastingMode.Off;
    }

    public void UpdateVisual()
    {
        ttl -= Time.deltaTime;
        if (ttl < 0)
        {
            GameObject.Destroy(this.gameObject);
            return;
        }
        float scale = maxttl == 0 ? 1 : ttl / maxttl;

        Color mycolor = material.GetColor("_BaseColor");
        mycolor.a = scale;
        material.SetColor("_BaseColor", mycolor);
        //this.gameObject.transform.localScale=new Vector3(radius*scale, radius*scale, radius* scale);
    }
    public void FixedUpdate()
    {
        UpdateVisual();
    }

}

public class SphereShrunker : MonoBehaviour, IDebugSphereVisializer
{
    private float ttl;
    private float maxttl;
    private float radius;
    private Material material;

    public void Setup(float _ttl, float _radius, Material _material)
    {
        maxttl = _ttl;
        ttl = _ttl;
        radius = _radius;
        material = new Material(_material);

        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        mr.material = material;
        mr.shadowCastingMode = ShadowCastingMode.Off;
    }

    public void UpdateVisual()
    {
        ttl -= Time.deltaTime;
        if (ttl < 0)
        {
            GameObject.Destroy(this.gameObject);
            return;
        }
        float scale = maxttl == 0 ? 1 : ttl / maxttl;

        this.gameObject.transform.localScale = new Vector3(radius * scale, radius * scale, radius * scale);
    }
    public void FixedUpdate()
    {
        UpdateVisual();
    }

}