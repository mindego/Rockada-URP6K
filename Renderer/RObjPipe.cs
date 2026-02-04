using UnityEngine.Assertions;
using LayersCont = System.Collections.Generic.List<LayerDesc>;
using LayerId = System.Int32;
using QueuedUnitsCont = System.Collections.Generic.List<int>;
using QueuedLayersCont = System.Collections.Generic.List<int>;
using UnitsContAlloc = wNodePool<RenderUnit>;
using UnitsCont = System.Collections.Generic.List<RenderUnit>; //TODO - может так проще?
using UnitsContPool = System.Collections.Generic.List<System.Collections.Generic.List<RenderUnit>>;

public class RObjPipe : IRObjPipe
{
    LayersCont mLayersCont;
    UnitsContAlloc mUnitsContAlloc;
    QueuedUnitsCont[] mQueuedUnits = new QueuedUnitsCont[RObj.MaxPriority];
    QueuedLayersCont mQueuedLayers;
    UnitsContPool mUnitsContPool;
    public RObjPipe() { }

    ~RObjPipe()
    {
        Dispose();
    }
    public void Destroy()
    {
        Dispose();
    }

    public void Reset()
    {
        Dispose();
    }

    public void Dispose()
    {
        ReleaseLayers();
        ReleaseUnitsConts();
    }

    public void Start()
    {
        Assert.IsTrue(mQueuedLayers.Count == 0);
    }

    public void Flush()
    {

        for (int p = 0; p < RObj.MaxPriority; ++p)
        {//for each priority
            foreach (int q_it in mQueuedUnits[p])
            {//for each layer
                LayerDesc ld = mLayersCont[q_it];
                if (Engine.sApplyRObjLayer) ld.layer.Apply();
                Engine.ShadersApplied++;
                foreach (RenderUnit u_it in ld.units[p])
                {
                    RenderUnit ru = u_it;
                    if (Engine.UseD3DPipe != 0)
                    {//draw object
                        ru.myobject.Draw();
                    }
                }
                ld.units[p].Clear();
                FreeObjectsCont(ld.units[p]);
            }
            mQueuedUnits[p].Clear();
        }
        foreach (int q_it in mQueuedLayers)
        {//clear queued layers desc
            mLayersCont[q_it].Clear();
        }
        mQueuedLayers.Clear();
    }

    void ReleaseLayers()
    {
        foreach (LayerDesc it in mLayersCont)
        {
            ILayer layer = it.layer;
            layer.FreePrivateData();
            layer.Release();
        }
        mLayersCont.Clear();
    }

    void FreeObjectsCont(UnitsCont cont)
    {
        //mUnitsContPool.push_back(cont);
        mUnitsContPool.Add(cont); //TODO Вот что0то не уверен я  , что это правильно.
    }

    void ReleaseUnitsConts()
    {
        {
            foreach (UnitsCont it in mUnitsContPool)
            {
                it.Clear();
            }
        }
        mUnitsContPool.Clear();
    }

    public void Add(RObj r)
    {
        throw new System.NotImplementedException();
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }
}

public class LayerDesc
{
    public ILayer layer;
    public UnitsCont[] units = new UnitsCont[RObj.MaxPriority];
    bool queued;
    LayerDesc()
    {
    }
    LayerDesc(ILayer _layer)
    {
        layer = _layer;

        Clear();
    }
    public void Clear()
    {
        queued = false;
        for (int i = 0; i < RObj.MaxPriority; ++i) units[i] = null;
    }
};

public interface IRObjPipe : IObject
{

    //helper function to evict contained objects ( e.g. layers ) 
    public void Reset();

    //brackets beetween wich objects added and drawed
    public void Start();
    public void Flush();

    //queue object
    public void Add(RObj r);
};

public struct RObj
{
    public const int MaxPriority = 8;
    int priority;//0..8
    public ILayer layer;     //pipe suggests layer to be long-term property ( outer start/flush scope )
    public IRenderable renderable;//pipe needs renderable only between start/flush
                                  //public RObj() { }
    public RObj(int _priority, ILayer _layer, IRenderable _renderable)
    {
        priority = _priority;
        layer = _layer;
        renderable = _renderable;
    }

};

public interface ILayer : iRS  //TODO временно пустой интерфейс подлежит реализации
{
    //  virtual TSState* GetTSS()=0;
    //virtual void SetPrivateData( const void* pdata, int data_size )=0;
    public void FreePrivateData();
    //virtual void GetPrivateData(void* pdata, int data_size)=0;
    //virtual bool HasPrivateData()=0;
    //template<class T>
    //  void SetPrivateData( const T &data);
    //template<class T>
    //  T GetPrivateData();
};

public class RenderUnit
{
    public IRenderable myobject;
    public RenderUnit() { }
    public RenderUnit(IRenderable r)
    {
        myobject = r;

    }
};


public interface iRS : IObject
{
    new public const uint ID = (0x3D57E9DA);
    public HRESULT Apply();
};