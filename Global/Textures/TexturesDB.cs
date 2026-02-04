using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DWORD = System.UInt32;
using static renderer_dll;
using System;

public class TexturesDB : ITexturesDB, IDisposable
{
    //private ResourcePack resourcePack;
    public Dictionary<uint, Texture2D> cache = new Dictionary<DWORD, Texture2D>();
    //private Dictionary<uint, Texture2D> cache = new Dictionary<DWORD, Texture2D>();
    protected IMappedDb mDB;
    TextureFactory mFactory;

    public TexturesDB()
    {
        mFactory = null;
        mDB = null;
    }
    ~TexturesDB()
    {
        // Destroy();
    }

    public void Dispose()
    {
        Asserts.AssertBp(mFactory = null);
        Asserts.AssertBp(mDB == null);
    }
    public bool Initialize(string path)
    {
        if (d3d == null) d3d = new Cd3d(); //TODO Что.то здесь не так. Создавать d3d ленивым образом - неправильно!
        mFactory = d3d.CreateTexturesFactory();
        //packs.Add(PackType.TexturesDB, new ResourcePack(ProductDefs.GetPI().getHddFile("/Graphics/textures.dat")));
        //packs.Add(PackType.TexturesDB, new ResourcePack("E:/Unity/Wedge/Echelon Island Defence/DataWW/Graphics/atextures.dat"));
        //Debug.Log("Loading textures: " + ProductDefs.GetPI().getHddFile(path));
        //resourcePack = new ResourcePack(ProductDefs.GetPI().getHddFile(path));
        mDB = IMappedDb.CreateMappedDb(IMappedDb.DBFORMAT_NAKED);//CreateMappedDb("TEXS");

        if (mDB.Open(ProductDefs.GetPI().getHddFile(path)) != DBDef.DB_OK)
        {
            Log.Message("Can't open textures DB {0}", path);
            return false;
        }

        //if (resourcePack == null) return false;

        return true;
    }
    public void Destroy()
    {
        if (mDB != null && mDB.IsOpened()) mDB.Close();
        if (mFactory != null) mFactory.Destroy();
        mFactory = null;
        mDB = null;
    }
    public Texture2D CreateTexture(string name)
    {
        uint crc32 = Hasher.HshString(name);
        Texture2D data = CreateTexture(crc32);
        data.name = name;
        return data;
    }

    //public Texture2D CreateTexture(DWORD crc32)
    //{
    //    if (crc32 == 0xffffffff) return default; //TODO Возвращать текстуру с ошибкой
    //    if (cache.ContainsKey(crc32)) return cache[crc32];

    //    Stream ms = resourcePack.GetStreamById(crc32);
    //    if (ms == null) throw new System.Exception("Failed to create stream for CRC " + crc32.ToString("X8") + "/" + resourcePack.GetNameById(crc32));
    //    Texture2D data = TextureImport.GetTexture(ms, (int)crc32);
    //    data.name = resourcePack.GetNameById(crc32);
    //    cache.Add(crc32, data);
    //    return data;
    //}

    public Texture2D CreateTexture(DWORD crc32)
    {
        if (crc32 == 0xffffffff) return default; //TODO Возвращать текстуру с ошибкой
        if (cache.ContainsKey(crc32)) return cache[crc32];

        ObjId id = new ObjId(crc32);
        id = mDB.CompleteObjId(id);
        MemBlock bl = mDB.GetBlock(id);
        if (bl.myStream == null)
        {
            Debug.Log("Failed to load texture " + id.ToString());
            return null;
        }

        //Stream ms = resourcePack.GetStreamById(crc32);
        //if (ms == null) throw new System.Exception("Failed to create stream for CRC " + crc32.ToString("X8") + "/" + resourcePack.GetNameById(crc32));
        //Debug.Log("Loading texture: " + id.cstr() + " " + id.crc32().ToString("X8"));
        Texture2D data = TextureImport.GetTexture(bl.myStream, (int)crc32);
        data.name = id.cstr();
        cache.Add(crc32, data);
        return data;
    }
    //public Texture2D CreateTexture(DWORD id)
    //{
    //    return (Texture2D) mDB.LoadCached(id);
    //}
    public Flags Flags() { return mFactory.flags; }  //TODO Реализовать корректную работу с флагами БД текстур
    public IMappedDb GetMappedDb()
    {
        return mDB;
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }


}

public class TextureFactory : IDataFactory
{
    public Cd3d d3d;

    public Flags flags = new Flags();

    public void AddRef()
    {
        return;
    }

    public void CreateNotifyObject(MemBlock bl, IObject iob)
    {

    }
    public void NotifyUserLoX(IObject lpt, int idx)
    {
        Log.Message("TextureFactory::NotifyUserLoX({0}), Ref = {1}", lpt, lpt.RefCount());
    }

    public int Release()
    {
        return 0;
    }


    internal void Destroy()
    {
        Release();

    }
}
public interface IDataFactory : IRefMem
{
    public void CreateNotifyObject(MemBlock bl, IObject iob);
    public IObject CreateDefaultObject() { return null; }
    public void NotifyUserLoX(IObject iob, int i);
};