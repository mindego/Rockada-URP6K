/*******************************/
//BaseTexMap
/*******************************/
using UnityEngine;

public class StdTexMap : TexMap
{
    Texture2D texture;
    //protected Cd3d* d3d;
    protected int ref_count;
    protected void SetCurTexture(Texture2D _texture)
    {
        texture = _texture;
    }
    ~StdTexMap()
    {
        //SafeRelease(texture);
        Dispose();
    }

    protected virtual void Dispose()
    {
        texture = null;
    }
    public StdTexMap(object o)
    {

        ref_count = 1;
        texture = null;
    }

    public virtual Texture2D GetTexture()
    {
        return texture;
    }

    public virtual void AddRef()
    {
        ref_count++;
    }
    public virtual int Release()
    {
        if ((--ref_count) == 0)
        {
            Dispose();
            return 0;
        }
        return ref_count;
    }
    public virtual object Query(uint iid)
    {
        switch (iid)
        {
            case TexMap.ID:
                {
                    AddRef();
                    return (TexMap)this;
                };
        }
        return null;
    }
};


/*******************************/
//StaticStdTexMap
/*******************************/
class StaticStdTexMap : StdTexMap
{
    Texture2D texture;
    ~StaticStdTexMap()
    {
        Dispose();
    }
    public StaticStdTexMap(object o) : base(o)
    {
        texture = null;
    }
    public bool Initialize(StaticStdTexMapData data)
    {
        if (texture = renderer_dll.dll_data.LoadTexture(data.texture, data.flags.GetBUMPDUDV()))
        {
            SetCurTexture(texture);
            return true;
        }
        return false;
    }
    public override void AddRef()
    {
        ref_count++;
    }
    public override int Release()
    {
        if ((--ref_count) == 0)
        {
            Dispose();
            return 0;
        }
        return ref_count;
    }
};

class Discretizator
{
    float factor;
    int frame;
    float reminder;
    public void Advance(float delta)
    {
        reminder += delta * factor;
        int d = (int)reminder;
        frame += d;
        reminder -= d;
    }
    public void Init(float start_time, float _factor)
    {
        factor = _factor;
        reminder = 0;
        frame = 0;
        Advance(start_time);
    }
    public int GetFrame()
    {
        return frame;
    }
};

/*******************************/
//StdFrameAnimation
/*******************************/
class StdFrameAnimation : iUpdatable
{
    Discretizator discr;
    int ref_count;
    public StdFrameAnimation()
    {
        ref_count = 1;
    }
    public bool Initialize(float start, float factor)
    {
        if (discr == null) discr = new Discretizator();
        discr.Init(start, factor);
        return true;
    }
    public void Update(float scale)
    {
        discr.Advance(scale);
    }
    public int GetFrame()
    {
        return discr.GetFrame();
    }
    public void AddRef()
    {
        ref_count++;
    }
    public int Release()
    {
        if ((--ref_count) == 0)
        {
            Dispose();
            return 0;
        }
        return ref_count;
    }

    public void Dispose() { }
};


/*******************************/
//StaticStdTexMap
/*******************************/
class SlidedStdTexMap : StdTexMap
{
    int num_textures;
    Texture2D[] textures;
    StdFrameAnimation animator;
    ~SlidedStdTexMap()
    {
        Dispose();
    }

    protected override void Dispose()
    {
        if (textures != null)
        {
            for (int i = 0; i < num_textures; ++i)
                textures[i] = null;
            //SafeRelease(textures[i]);
            //delete[] textures;
            textures = null;
        }
        //SafeRelease(animator);
        animator = null;
    }
    public SlidedStdTexMap(object o) : base(o)
    {
        textures = null;
        animator = null;
    }
    public bool Initialize(SlidedStdTexMapData data)
    {
        Debug.Log("Initialize(SlidedStdTexMapData data):" + data);
        num_textures = data.num;
        textures = new Texture2D[num_textures];
        int i;
        for (i = 0; i < num_textures; ++i) textures[i] = null;
        for (i = 0; i < num_textures; ++i)
        {
            textures[i] = renderer_dll.dll_data.LoadTexture(data.textures[i], data.flags.GetBUMPDUDV());
            if (textures[i] == null)
                return false;
        }

        animator = new StdFrameAnimation();
        return animator.Initialize(0, data.time_factor);
    }
    public override Texture2D GetTexture()
    {
        return textures[animator.GetFrame() % num_textures];
    }
    public override void AddRef()
    {
        ref_count++;
    }

    public override int Release()
    {
        if ((--ref_count) == 0)
        {
            Dispose();
            return 0;
        }
        return ref_count;
    }
    public override object Query(uint iid)
    {
        switch (iid)
        {
            case StormTexture.ID:
                {
                    AddRef();
                    //return (Texture*)this;
                    return this;
                }
            case iUpdatable.ID:
                {
                    return animator;
                }
        };
        return null;

    }
};

/*******************************/
//BaseTexMap
/*******************************/
/*******************************/

//TexMap* CreateTexMap(ObjId id, int)
//{
//    TexMapData* data = dll_data.LoadFile<TexMapData>(id);
//    if (!data)
//    {
//        Log->Message("Error : Can't load tex map : %p\n", id);
//        return 0;
//    }
//    switch (data->type)
//    {
//        case TMT_STANDARD:
//            {
//                return CreateStdTexMap((StdTexMapData*)data);

//            }
//    };
//    return 0;
//}

//TexMap* CreateStdTexMap(StdTexMapData* data)
//{
//    switch (data->animode)
//    {
//        case TMA_STATIC:
//            {
//                StaticStdTexMap* tex_map = new StaticStdTexMap(&d3d);
//                if (!tex_map->Initialize((StaticStdTexMapData*)data))
//                {
//                    tex_map->Release();
//                    return 0;
//                }
//                return tex_map;
//            }
//        case TMA_SLIDED:
//            {
//                SlidedStdTexMap* tex_map = new SlidedStdTexMap(&d3d);
//                if (!tex_map->Initialize((SlidedStdTexMapData*)data))
//                {
//                    tex_map->Release();
//                    return 0;
//                }
//                return tex_map;
//            }
//    };
//    return 0;
//}


/*
template<class C>
class Inner : public C{
  IObject *outer;
public:
  Inner(IObject *_outer) :
  outer(_outer)
  {}

  void AddRefSelf(){
    C::AddRef();
  }

  int  ReleaseSelf(){
    return C::Release();
  }

  void AddRef();
  int  Release();
  void *Query(int iid);
};

void TextureInner::AddRef(){
  outer->AddRef();
}

int TextureInner::Release(){
  return outer->Release();
}

void *TextureInner::Query(int iid){
  return outer->Query(iid);
}
  */