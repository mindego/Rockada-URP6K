using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//TODO удалить файл нафик.

//public class GameDataProvider
//{
//    public static GameDataProvider instance;

//    private Dictionary<PackId, string> defaultPack = new Dictionary<PackId, string>
//    {
//        { PackId.Texture2D, "Data/Graphics/textures.dat" },
//        { PackId.Mesh, "Data/Graphics/mesh.dat"},
//        { PackId.Material,"Data/Graphics/materials.dat" },
//        { PackId.Sound,"Data/sounds.dat" }
//    };



//    private Dictionary<PackId, ResourcePack> packs;
//    private Dictionary<PackId, GameDataProviderCache> cache;

//    public GameDataProvider()
//    {
//        foreach (KeyValuePair<PackId,string> kvp in defaultPack)
//        {
//            packs.Add(kvp.Key, new ResourcePack(kvp.Value));
//        }
//    }

//    public static GameDataProvider GetInstance()
//    {
//        if (instance == null) instance = new GameDataProvider();
//        return instance;
//    }

//    private Stream GetStream(string resourceName,PackId resourceType)
//    {
//        if (!defaultPack.ContainsKey(resourceType)) return null;
//        if (!packs.ContainsKey(resourceType)) return null;

//        return packs[resourceType].GetStreamByName(resourceName);

//    }

//    private Stream GetStream(uint id, PackId resourceType)
//    {
//        if (!defaultPack.ContainsKey(resourceType)) return null;
//        if (!packs.ContainsKey(resourceType)) return null;

//        return packs[resourceType].GetStreamById(id);

//    }
//    public static T GetResource<T>(string ResourceName)
//    {
//        Type type = typeof(T);
//        switch (type.ToString())
//        {
//            case "UnityEngine.Texture2D":
//                return GetResource<T>(ResourceName, PackId.Texture2D);
//            case "UnityEngine.Mesh":
//                return GetResource<T>(ResourceName, PackId.Mesh);
//            case "UnityEngine.Sound":
//                return GetResource<T>(ResourceName, PackId.Sound);
//            case "UnityEngine.Material":
//                return GetResource<T>(ResourceName, PackId.Material);
//        }
//        return default;
//    }

//    public static T GetResource<T>(string resourceName, PackId packId)
//    {
//        instance = GetInstance();
//        if (!instance.cache.ContainsKey(packId)) instance.cache.Add(packId, new GameDataProviderCache<T>());
//        Stream st = instance.GetStream(resourceName,packId);
//        if (st == null) return default;

//        Type type = typeof(T);
//        switch (type.ToString())
//        {
//            case "UnityEngine.Texture2D":
//                return (T)(object)TextureImport.GetTexture(st, resourceName);

//        }
//        //if (typeof(T) == typeof(Texture2D)) return (T) (object) TextureImport.GetTexture(st, resourceName);


//        return default;
//    }

        
//    public enum PackId
//    {
//        Texture2D,
//        Mesh,
//        Sound,
//        Material
//    }
//}

//public class GameDataProviderCache
//{

//}
//public class GameDataProviderCache<T> :GameDataProviderCache
//{
//    private Dictionary<string, T> cache = new Dictionary<string, T>();

//    public void AddItem(string resourceName,T cachedObject)
//    {
//        if (cache.ContainsKey(resourceName)) return;

//        cache.Add(resourceName, cachedObject);
//    }

//    public T GetItem(string resourceName)
//    {
//        if (cache.ContainsKey(resourceName)) return cache[resourceName];

//        return default(T);
//    }
//}
