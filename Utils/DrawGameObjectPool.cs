using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public static class DrawGameObjectPool
{
    public static GameObject GameObjectPoolContainer = new GameObject("GameObject pool");
    public static ObjectPool<GameObject> objectPool = new ObjectPool<GameObject>(onCreateGameObject, onGetGameObject, onReleaseGameObject, onDestroyGameObject, true, 8000);
    public static Dictionary<string,GameObject> GameObjectCache = new Dictionary<string, GameObject>();
    public static GameObject GameObjectCacheHolder = new GameObject("GameObject cache");
    public static Dictionary<uint, Mesh> meshcache = new Dictionary<uint, Mesh>();
    public static Dictionary<uint, Material> materialcache = new Dictionary<uint, Material>();
    public static Dictionary<uint, StormMesh> stormmeshcache = new Dictionary<uint, StormMesh>();

    public static void Init()
    {
        GameObjectPoolContainer = new GameObject("GameObject pool");
        objectPool = new ObjectPool<GameObject>(onCreateGameObject, onGetGameObject, onReleaseGameObject, onDestroyGameObject, true, 8000);
        GameObjectCache = new Dictionary<string, GameObject>();
    }
    public static GameObject onCreateGameObject()
    {
        return new GameObject("New reusable GameObject");
    }

    public static void onGetGameObject(GameObject obj)
    {
        //Debug.Log("Pool state Get" + objectPool.CountActive + "/" + objectPool.CountInactive + " ["+objectPool.CountAll + "]");
        
        obj.transform.parent = null;
        obj.SetActive(true);
    }

    public static void onReleaseGameObject(GameObject obj)
    {
        obj.SetActive(false);
        if (obj.TryGetComponent<MeshFilter>(out MeshFilter mf))
        {
            Object.Destroy(mf);
        }

        if (obj.TryGetComponent<MeshRenderer>(out MeshRenderer mr))
        {
            Object.Destroy(mr);
        }

        if (obj.TryGetComponent<ShowRoFlags>(out ShowRoFlags sf))
        {
            Object.Destroy(sf);
        }
        obj.transform.parent = GameObjectPoolContainer.transform;
        obj.transform.rotation = Quaternion.LookRotation(Vector3.forward);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        obj.name = "Reused GameObject";
        

        while (obj.transform.childCount >0)
        {
            GameObject child = obj.transform.GetChild(0).gameObject;
            objectPool.Release(child);
        }
        //Debug.Log("Pool state Rel" + objectPool.CountActive + "/" + objectPool.CountInactive + " [" + objectPool.CountAll + "]");
    }

    public static void onDestroyGameObject(GameObject obj)
    {
        GameObject.Destroy(obj);
    }
}
