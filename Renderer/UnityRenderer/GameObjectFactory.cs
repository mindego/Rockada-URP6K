using System;
using UnityEngine;
using UnityEngine.Pool;

public class GameObjectFactory
{
    private static GameObject PoolHolder = new GameObject("GameObject main pool");
    private static ObjectPool<GameObject> BaseGameObjectPool = new ObjectPool<GameObject>(onCreateGameObject, onGetGameObject, onReleaseGameObject, null, true, 2000);
    private static ObjectPool<GameObject> MeshGameObjectPool = new ObjectPool<GameObject>(onCreateGameObjectMesh, onGetGameObject, onReleaseGameObjectMesh, null, true, 2000);
    private static ObjectPool<GameObject> ParticleGameObjectPool = new ObjectPool<GameObject>(onCreateParticleGameObject, onGetGameObject, onReleaseParticleGameObject, null,true,2000);

    private static void onReleaseParticleGameObject(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(PoolHolder.transform);
        obj.GetComponent<ParticleSystem>().Stop();

    }

    private static GameObject onCreateParticleGameObject()
    {
        GameObject gobj = new GameObject("particle mesh gameobject");
        gobj.transform.SetParent(PoolHolder.transform);
        gobj.SetActive(false);
        gobj.AddComponent<ParticleSystem>();

        return gobj;
    }

    public enum GameObjectType
    {
        EMPTY,
        MESH,
        PARTICLE
    }

    private static GameObject onCreateGameObjectMesh()
    {
        GameObject gobj = new GameObject("pool mesh gameobject");
        gobj.transform.SetParent(PoolHolder.transform);
        gobj.SetActive(false);
        gobj.AddComponent<MeshRenderer>();
        gobj.AddComponent<MeshFilter>();
        gobj.AddComponent<MeshCollider>();

        return gobj;
    }
    private static void onReleaseGameObjectMesh(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(PoolHolder.transform);
        obj.GetComponent<MeshFilter>().mesh = null;
        obj.GetComponent<MeshRenderer>().materials = new Material[0];
        obj.GetComponent<MeshCollider>().sharedMesh = null;
    }

    public static GameObject GetGameObject<T>(RO obj)
    {
        if (obj == null) return null;

        if (obj.GetType() == typeof(PARTICLE_SYSTEM)) return GetGameObject(GameObjectType.PARTICLE);
        if (obj.GetType() == typeof(FPO)) return GetGameObject(GameObjectType.MESH);

        return null;
    }
    public static GameObject GetGameObject(GameObjectType gobjType=GameObjectType.EMPTY)
    {
        switch (gobjType)
        {
            case GameObjectType.EMPTY:
                return BaseGameObjectPool.Get();
            case GameObjectType.MESH:
                return MeshGameObjectPool.Get();
            case GameObjectType.PARTICLE:
                return ParticleGameObjectPool.Get();
            default:
                return null;
        }
    }

    public static void ReleaseParticle(GameObject obj)
    {
        ParticleGameObjectPool.Release(obj);
    }

    public static void ReleaseMesh(GameObject obj)
    {
        MeshGameObjectPool.Release(obj);
    }
    public static void Release(GameObject obj)
    {
        //while (obj.GetComponentCount()>0)
        //{
        //    Debug.Log("Removing first of " + obj.GetComponentCount());
        //    UnityEngine.Object.Destroy(obj.GetComponentAtIndex(0));
        //}
        if (obj.TryGetComponent<MeshFilter>(out MeshFilter mf))
        {
            ReleaseMesh(obj);
            return;
        }
        BaseGameObjectPool.Release(obj);
    }


    private static void onReleaseGameObject(GameObject obj)
    {
        obj.transform.SetParent(PoolHolder.transform);
        //while (obj.transform.childCount > 0)
        //{
        //    BaseGameObjectPool.Release(obj.transform.GetChild(0).gameObject);
        //}
    }

    private static void onGetGameObject(GameObject obj)
    {
        obj.transform.SetParent(null);
        obj.SetActive(true);
    }


    private static GameObject onCreateGameObject()
    {
        GameObject gobj = new GameObject("pool gameobject");
        gobj.transform.SetParent(PoolHolder.transform);
        gobj.SetActive(false);

        return gobj;
    }

}
