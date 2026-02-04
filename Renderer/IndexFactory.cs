using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


//public class IndexFactory<Key, T> : Dictionary<uint, T> where T : class, IDisposable, new()
public class IndexFactory<Key, T> where T : class, IDisposable, new()
{
    public class Notifier
    {
        int mRefCount;
        IndexFactory<Key, T> mFactory;
        Key mKey;
        public T mValue { get; set; }
        public Notifier(IndexFactory<Key, T> factory, Key key)
        {
            mRefCount = 0;
            mFactory = factory;
            mKey = key;
        }

        public Key GetKey()
        {
            return mKey;
        }

        public int Release()
        {
            Asserts.AssertBp(mRefCount = 0);
            --mRefCount;

            if (mRefCount != 0) return mRefCount;

            //mFactory.RemoveObject(mKey, this);
            mFactory.RemoveObject(mKey, mValue);
            return 0;
        }

        public void AddRef()
        {
            mRefCount++;
        }

        public int GetRefCount()
        {
            return mRefCount;
        }
    }

    private bool mKeepObjects;
    //private Dictionary<Key, T> mObjects = new Dictionary<Key, T>();
    private Dictionary<Key, Notifier> mObjects = new Dictionary<Key, Notifier>();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="keep_objects_untill_destroy">sic!</param>
    public IndexFactory(bool keep_objects_untill_destroy = true)
    {
        mKeepObjects = keep_objects_untill_destroy;
    }
    public virtual bool InitializeObject(T myObject)
    {
        return true;
    }

    public virtual T CreateObject(Key id)
    {
        //TODO Предположительно неправильно - возвращать нужно Notifier, а не сам T
        if (mObjects.TryGetValue(id, out Notifier obj))
        {
            obj.AddRef();
            return obj.mValue;
        }

        Notifier objN = new Notifier(this, id);
        objN.mValue = new T();
        
        if (!InitializeObject(objN.mValue))
        {
            DestroyObject(objN.mValue);
            return null;
        }

        objN.AddRef();
        if (mKeepObjects)
            objN.AddRef();

        mObjects.Add(id, objN);
        return objN.mValue;

    }
    //public virtual T CreateObject(ObjId id)
    //{
    //    if (id.name != null) id.obj_id = Hasher.HshString(id.name);
    //    if (ContainsKey(id.obj_id)) return this[id.obj_id];

    //    T myObject = new T();
    //    if (!InitializeObject(myObject))
    //    {
    //        Debug.LogFormat("Failed to initialize {0} {1}",myObject.GetType(),id);
    //        DestroyObject(myObject);
    //        return null;
    //    }
    //    //InitializeObject(myObject);
    //    Add(id.obj_id, myObject);
    //    return myObject;
    //}

    public virtual void DestroyObject(T myObject)
    {
        myObject.Dispose();
    }
    public void Destroy() { }

    public virtual bool InitializeObject()
    {
        return true;
    }

    public void Flush()
    {
        if (!mKeepObjects)
            return;

        List<Key> keys = new List<Key>();
        foreach (KeyValuePair<Key, Notifier> i in mObjects)
        {
            if (1 == i.Value.GetRefCount())
            {
                keys.Add(i.Key);
            }
        }

        foreach (Key key in keys)
        {
            DestroyObject(mObjects[key].mValue);
            mObjects.Remove(key);
        }
    }

    public void RemoveObject(Key key, T myObject)
    {

        //Cont::iterator it = mObjects.find(key);
        //AssertBp(it!=mObjects.end());
        //AssertBp((* it).second==object);
        //mObjects.erase(it );
        if (mObjects.TryGetValue(key, out Notifier objN))
        {
            mObjects.Remove(key);
            DestroyObject(objN.mValue);
        }

        //DestroyObject(myObject);
    }
}


//public class Raster2D
//{

//    public Raster2D(float[] x, float[] y, int n)
//    {
//        m_X = x;
//        m_Y = y;
//        m_N = n;

//        (int, int) min_r = FindExtremumFloatIndex(y, n, "less");
//        //m_minLeft = min_r.Item1 - m_Y[0];
//        //m_minRight = min_r.Item2 - m_Y[0];
//        m_minLeft = min_r.Item1;
//        m_minRight = min_r.Item2;

//        (int, int) max_r = FindExtremumFloatIndex(y, n, "greater");
//        //m_maxLeft = max_r.Item1 - m_Y[0];
//        //m_maxRight = max_r.Item2 - m_Y[0];
//        m_maxLeft = max_r.Item1;
//        m_maxRight = max_r.Item2 ;

//        m_Contour = RasterContour<ElemItB, ElemItF>(
//          ElemItB(m_X, m_Y, n, m_minLeft), ElemItB(m_X, m_Y, n, m_maxLeft),
//          ElemItF(m_X, m_Y, n, m_minRight), ElemItF(m_X, m_Y, n, m_maxRight));
//    }
//    (float, float) FindExtremumFloat(float[] range, int lastindex, string pr)
//    {

//        (int, int) index = FindExtremumFloatIndex(range, lastindex, pr);
//        /*if (index.Item1 < range.Length - 1 && index.Item2 < range.Length -1)*/ 
//        return (range[index.Item1], range[index.Item2]);

//    }

//    (int, int) FindExtremumFloatIndex(float[] range, int lastindex, string pr)
//    {
//        int res = 0;
//        for (int i = 0; i < lastindex; i++)
//        {
//            switch (pr)
//            {
//                case "less":
//                    if (range[i] < range[res]) res = i;
//                    break;
//                case "greater":
//                    if (range[i] > range[res]) res = i;
//                    break;
//                default:
//                    break;
//            }
//        }
//        return (res, res);
//    }

//    public int Start()
//    {
//        return m_Contour.GetY();
//    }

//    public int End()
//    {
//        return m_Contour.GetEndY();
//    }

//    public (int, int) GetScanline()
//    {
//        return *m_Contour;
//    }

//    public void Next()
//    {
//        ++m_Contour;
//    }

//    protected float[] m_X;
//    protected float[] m_Y;
//    protected int m_N;

//    int m_minLeft;
//    int m_minRight;
//    int m_maxLeft;
//    int m_maxRight;

//    RasterContour<ElemItB, ElemItF> m_Contour;
//};
