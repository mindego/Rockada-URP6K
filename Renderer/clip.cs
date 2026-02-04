using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;
using static D3DEMULATION;
using static GL_STRIDE_FIELD;
using static GL_VB_TYPE;
using static GLStride;
using static Unity.Burst.Intrinsics.X86;
using static Unity.VisualScripting.Dependencies.Sqlite.SQLite3;
using static UnityEngine.CullingGroup;
using CBoxPipe = CTPipe<BoxRObj, BoxCont>;
using DWORD = System.UInt32;
using WORD = System.UInt16;

public class ClipStatus
{
    public DWORD ClipUnion;
    public DWORD ClipIntersection;
    public ClipStatus() { }
    public ClipStatus(DWORD cu, DWORD ci)
    {
        ClipUnion = cu;
        ClipIntersection = ci;
    }
};

public enum ClipStatusFlags
{
    CS_LEFT = 0x0001,
    CS_RIGHT = 0x0002,
    CS_TOP = 0x0004,
    CS_BOTTOM = 0x0008,
    CS_FRONT = 0x0010,
    CS_BACK = 0x0020,
    CS_PLANE0 = 0x0040,
    CS_PLANE1 = 0x0080,
    CS_PLANE2 = 0x0100,
    CS_PLANE3 = 0x0200,
    CS_PLANE4 = 0x0400,
    CS_PLANE5 = 0x0800,

    CS_FRUSTRUM =
      CS_LEFT |
      CS_RIGHT |
      CS_TOP |
      CS_BOTTOM |
      CS_FRONT |
      CS_BACK,
    CS_PLANES =
      CS_PLANE0 |
      CS_PLANE1 |
      CS_PLANE2 |
      CS_PLANE3 |
      CS_PLANE4 |
      CS_PLANE5,
    CS_ALL = CS_FRUSTRUM | CS_PLANES
};

public class GLGeometry
{
    public int num;
    public GLVertices pdata;//=new GLVertices();
};

/// <summary>
/// user's accessing to vertices data through glstride, 
/// api's getting through vb
/// </summary>
public class GLVertices
{
    /// <summary>
    /// format&vtx_size needed to successful locking
    /// </summary>
    int glformat;
    public int d3dformat;
    public GL_VB_TYPE type;
    public int vtx_size;
    /// <summary>
    /// optional use
    /// </summary>
    public IDirect3DVertexBuffer7 vb;
    public object[] lpdata;
    /// <summary>
    /// when it's a d3dvb-it points on last locked vb's data
    /// </summary>
    public GLStride[] data = new GLStride[(int)GL_STRIDE_FIELD.glsfENUM_SIZE];

    static int[] vbfield_size = new int[(int)GL_STRIDE_FIELD.glsfENUM_SIZE];

    public GLVertices()
    {
        for (int i=0;i< (int)GL_STRIDE_FIELD.glsfENUM_SIZE;i++)
        {
            data[i]=new GLStride();
        }
    }

    /// <summary>
    /// D3Dlockmode
    /// </summary>
    /// <param name="lockmode"></param>
    /// <returns></returns>
    public HRESULT Lock(int lockmode)
    {
        //STUB
        return HRESULT.S_OK;
    }
    public HRESULT Unlock()
    {
        //STUB
        return HRESULT.S_OK;
    }

    /// <summary>
    /// sets d3dformat from glformat
    /// </summary>
    /// <param name="glfmt"></param>
    public void format_gl_2_d3d(int fmt) {


        d3dformat = 0;
        glformat = fmt;

        if ((fmt & (1 << (int)glsfPOSITION))!=0)
        {
            if ((fmt & (1 << (int)glsfRHW))!= 0) d3dformat |= D3DFVF_XYZRHW;
            else d3dformat |= D3DFVF_XYZ;
        }

        if ((fmt & (1 << (int)glsfBETA))!= 0) d3dformat |= D3DFVF_XYZB1;

        if ((fmt & (1 << (int)glsfNORMAL))!= 0) d3dformat |= D3DFVF_NORMAL;

        if ((fmt & (1 << (int)glsfDIFFUSE))!= 0) d3dformat |= D3DFVF_DIFFUSE;

        if ((fmt & (1 << (int)glsfSPECULAR))!= 0) d3dformat |= D3DFVF_SPECULAR;

        if ((fmt & (1 << (int)glsfTEXTURE0))!= 0) d3dformat |= D3DFVF_TEX1;

        if ((fmt & (1 << (int)glsfTEXTURE1))!= 0) d3dformat |= D3DFVF_TEX2;

    }
    /// <summary>
    /// sets glformat  from d3dformat
    /// </summary>
    /// <param name="d3dfmt"></param>
    void format_d3d_2_gl(int d3dfmt) {
        throw new NotImplementedException();
    }
    /// <summary>
    /// sets format,vtx_size& data, returns d3dvbformat 
    /// </summary>
    /// <param name="type"></param>
    public void Set(GL_VB_TYPE t)
    {
        type = t;
        int i;
        switch (type)
        {
            case glvtD3D_AOS:
                SetVertexSize();
                for (i = 0; i < (int)glsfENUM_SIZE; ++i)
                {
                    data[i].stride = vtx_size;
                }
                break;
            case glvtBASIC_SOA:
                {
                    SetVertexSize();
                    break;
                }
        }
    }
    public void SetAOSData(ref object[] pdata, int stride) {
        //STUB
        return;
        
        throw new NotImplementedException();
    }

    void Copy(GLVertices v, int n) {
        throw new NotImplementedException();
    }

    static void CreateStrided(ref GLStride dst, int size, int format) { }
    static void DestroyStrided(ref GLStride dst, int fmt) { }

    /// <summary>
    /// format should be set before
    /// </summary>
    private void SetVertexSize() {
        int size = 0;
        for (int i = 0; i < (int) glsfENUM_SIZE; ++i)
        {
            size += ((glformat & GLField(i))!=0) ? GLVertices.vbfield_size[i] : 0;
        }
        vtx_size = size;
    }

    private int GLField(int i) {
        return (1 << i);
    }


};

public enum GL_STRIDE_FIELD
{
    glsfPOSITION,
    glsfNORMAL,
    glsfDIFFUSE,
    glsfSPECULAR,
    glsfTEXTURE0,
    glsfTEXTURE1,
    glsfRHW,
    glsfBETA,
    glsfENUM_SIZE,
    glsfFORSE_DWORD = 0x7fffffff
};

/// <summary>
/// analogous ( exactly:) ) to D3DDP_PTRSTRIDE
/// </summary>
public class GLStride
{
    public object[] ptr;
    public int stride;
    int size;

    public GLStride()
    {
        size = 16;
        ptr=new object[size]; //TODO не уверен, что 16 вершин хватит. Надо обдумать расшириние массива при выходе за его пределы 
    }
    public object this[int i]
    {
        get { return ptr[i]; }
        set {
            if (i >= size) GrowArray();
            ptr[i] = value; 
        }
    }

    private void GrowArray()
    {
        
        int newSize = size * 2;
        Debug.LogFormat("Array resized {0}->{1}",size,newSize);
        object[] newPtr=new object[newSize];

        for (int i = 0; i<size;i++)
        {
            newPtr[i]=ptr[i];
        }
        ptr = newPtr;
        size = newSize;
    }
};


interface ITPipe<T> : IObject
{

    /// <summary>
    /// helper function to evict contained objects ( e.g. layers ) 
    /// </summary>
    public void Reset();

    /// <summary>
    /// brackets beetween wich objects added and drawed
    /// </summary>
    public void Start();
    public void Flush();

    /// <summary>
    /// queue object
    /// </summary>
    public void Add(T t);
};

public class BoxRObj
{
    TerrainBox m_Box;
    int m_Layer;

    public BoxRObj()
    {
        m_Box = null;
        m_Layer = -1;
    }

    public BoxRObj(TerrainBox Box, int Layer)
    {
        m_Box = Box;
        m_Layer = Layer;
    }

    /// <summary>
    ///  Замена bool operator <( const BoxRObj &robj)const;
    /// </summary>
    /// <param name="robj"></param>
    /// <returns></returns>
    public bool LessThan(BoxRObj robj)
    {
        if (m_Layer < robj.m_Layer) return true; if (m_Layer != robj.m_Layer) return false;
        //if (m_Box.vb < robj.m_Box.vb) return true; if (m_Box.vb != robj.m_Box.vb) return false; //TODO здесь сравниваются указатели, а не сами значения

        if (m_Box.msurfacei[m_Layer] < robj.m_Box.msurfacei[robj.m_Layer])
            return true;
        if (m_Box.msurfacei[m_Layer] != robj.m_Box.msurfacei[robj.m_Layer])
            return false;

        if (m_Box.suzerland < robj.m_Box.suzerland) return true; if (m_Box.suzerland != robj.m_Box.suzerland) return false;
        if (m_Box.locus.GetIndex() < robj.m_Box.locus.GetIndex()) return true; if (m_Box.locus.GetIndex() != robj.m_Box.locus.GetIndex()) return false;

        return false;
    }
    //bool operator <( const BoxRObj &robj)const;
    public void Draw(StormTerrain terrain, BoxRObj last_box)
    {
        bool state_changed = false;

        //if ((!last_box.m_Box) || (m_Box->msurfacei[m_Layer] != last_box.m_Box->msurfacei[last_box.m_Layer]))
        //{
        //    d3d.SetTexture(terrain->Surfaces[m_Box->msurfacei[m_Layer]].texture);
        //    d3d.SetMaterial(terrain->Surfaces[m_Box->msurfacei[m_Layer]].material);

        //    state_changed = true;
        //}

        renderer_dll.d3d.DrawIndexedVB(m_Box.vb.vbuffer.GetVB(), m_Box.num_vtx, m_Box.idxs, m_Box.num_idx, m_Box.start_vtx);
    }
};

public class GLTopology
{
    public int num_tris;
    public WORD[] tris;
    public GLTopology() { }
    public GLTopology(int n, WORD[] t)
    {
        num_tris = n;
        tris = t;
    }

};

//typedef std::multiset<BoxRObj,std::less<BoxRObj>, SizeCachedAllocator<BoxRObj> > BoxCont;

public interface IBoxCont<T>
{
    public void insert(T x);
    public bool empty();
    public void clear();
}
public class BoxCont : List<BoxRObj>, IBoxCont<BoxRObj>
{
    public void clear()
    {
        Clear();
    }

    public bool empty()
    {
        return Count == 0;
    }

    public void insert(BoxRObj x)
    {
        Add(x);
    }

}

public class BoxPipe : CBoxPipe
{
    public BoxPipe() : base(new BoxCont()) //TODO Реализовать полностью.
                                           //CBoxPipe
                                           //  (
                                           //  BoxCont(std::less<BoxRObj>(), SizeCachedAllocator<BoxRObj>(s_NodeCache) )
                                           //  )
    { }
    ~BoxPipe() { }

    public bool Initialize(StormTerrain terrain)
    {
        m_Terrain = terrain;
        return true;
    }

    public override void Add(BoxRObj X)
    {
        //prof.AddRef();
        //prof.Start();
        m_ObjCont.insert(X);
        //prof.End();
    }
    public override void Flush()
    {
        //Log->Message( "BoxPipe::Add performance : %d ticks",prof.Avrg() );
        //prof.Reset();
        base.Flush();
    }

    protected override void BeginRender()
    {
        m_LastBox = new BoxRObj();
    }
    protected override void EndRender() { }

    //protected override void Render(BoxCont X)
    //{
    //    X.Draw(m_Terrain, m_LastBox);
    //    m_LastBox = X;
    //}
    protected override void Render(BoxRObj X)
    {
        X.Draw(m_Terrain, m_LastBox);
        m_LastBox = X;
    }
    BoxRObj m_LastBox;

    StormTerrain m_Terrain;

    //Prof prof;
};

public class CTPipe<T, ContType> : ITPipe<T> where ContType : List<T>, IBoxCont<T>
{
    //typedef Cont ContType;

    public CTPipe()
    {
        m_Drawing = false;
    }
    public CTPipe(ContType ObjCont)
    {
        m_Drawing = false;
        m_ObjCont = ObjCont;
    }
    ~CTPipe() { }

    public virtual void Reset() { }

    public virtual void Start()
    {
        Asserts.AssertBp(m_ObjCont.empty());
        Asserts.AssertBp(!m_Drawing);
        m_Drawing = true;
    }
    public virtual void Flush()
    {
        Asserts.AssertBp(m_Drawing);
        BeginRender();
        //for (var it = m_ObjCont.begin(); it != m_ObjCont.end(); ++it)
        //    Render(it);
        foreach (var it in m_ObjCont)
        {
            Render(it);
        }
        EndRender();
        m_ObjCont.clear();
        m_Drawing = false;
    }

    public virtual void Add(T t) { }
    protected virtual void BeginRender() { }
    protected virtual void EndRender() { }
    protected virtual void Render(T v) { }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }

    public ContType m_ObjCont;
    bool m_Drawing;
};
