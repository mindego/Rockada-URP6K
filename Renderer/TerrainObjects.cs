using System;
using UnityEngine;
using DWORD = System.UInt32;
using WORD = System.UInt16;

public class TerrainObjects
{
    //  void* mappeddata;
    void InitWater(int s)
    {
        int x, z;


        for (z = 0; z < s; ++z)
        {
            for (x = 0; x < s; ++x)
            {
                water[x, z] = new WaterBox();
                water[x, z].index = 0x80008000;
            }
        }

    }
    void DoneWater()
    {

    }

    public bool Initialize(Cd3d _d3d)
    {
        return vbs.Initialize(_d3d);
    }

    //public TMatrix<TerrainBox> data;
    //public TerrainBox[,] data;
    public TMatrix<TerrainBox> data;
    public TerrainVBs vbs;

    //  TMatrix<TerrainFeature> mFeatures;

    //  TMatrix<WaterBox> water;
    public WaterBox[,] water;

    public TerrainLoci m_Loci;

    //  GLVertices water_allvtx, water_fltvtx;
    //  GLStride aNormal[n_water_wrap][n_water_wrap][n_water_frames];
    //GLStride aPosition[n_water_wrap][n_water_wrap][n_water_frames];
    //int curframe, nextframe;
    //  float framebeta;

    private int BD_TriListSize() { return TerrainVBs.nTris * sizeof(short); }
    public TerrainObjects(StormTerrain _terrain, int s)
    {
        //data(s, s) , vbs(_terrain, s, s), water(s, s), mFeatures(s, s), m_Loci(s, s) {

        //data = new TerrainBox[s, s];
        data = new TMatrix<TerrainBox>(s, s);
        vbs = new TerrainVBs(_terrain, s, s);
        water = new WaterBox[s, s];
        //mFeatures = new[s,s] ;
        m_Loci = new TerrainLoci(s, s);

        int x, z;

        InitWater(s);

        int all_elems = s * s;

        //int datasize = all_elems * (BD_TriListSize());
        int datasize = all_elems * BD_TriListSize();

        //setting box constant pointers
        for (z = 0; z < s; ++z)
        {
            for (x = 0; x < s; ++x)
            {
                //TerrainBox Box = data[z,x];
                TerrainBox Box = new TerrainBox();
                data[z, x] = Box;
                Box.Invalidate();

                vbs.GetGeometry(x, z, ref Box.vb, ref Box.start_vtx);
                vbs.setInvalidateHook(x, z, Box);

                Box.idxs = new WORD[all_elems];
                //Box.idxs = (WORD*)vtrl_ptr;
                //vtrl_ptr = MovePtr(vtrl_ptr, BD_TriListSize());
            }
        }
    }

    //  bool Initialize(Cd3d* _d3d);
    ~TerrainObjects()
    {
        DoneWater();
    }
};

public class TerrainVBs
{
    //TODO перенести константы террайна в более правильное место.
    public const int nPoints = 25;
    public const int nTris = 96;
    public const int nLists = 4;

    public int VBHASHEDBOXESX = 16;
    public int VBHASHEDBOXESZ = 16;

    TMatrix<TerrainVB> vbs;
    //TerrainVB[,] vbs;
    StormTerrain terrain;
    public TerrainVBs(StormTerrain _terrain, int sx, int sz)
    {
        vbs = new TMatrix<TerrainVB>(sz / VBHASHEDBOXESZ, sx / VBHASHEDBOXESX);
        terrain = _terrain;
    }
    public bool Initialize(Cd3d d3d)
    {
        Debug.Log("Initializing TerrainVBS");
        for (int z = 0; z < vbs.Height(); ++z)
        {
            for (int x = 0; x < vbs.Width(); ++x)
            {
                VBuffer vb = d3d.vbmanager.CreateVBuffer(nPoints * VBHASHEDBOXESX * VBHASHEDBOXESZ, GroundVertex.FVF, VBPipe.VBF_DRAWABLE);
                if (vb == null)
                {
                    Debug.LogFormat("Failed to create VBuffer for TerrainVBS {0}",this.GetHashCode().ToString("X8"));
                    return false;
                }
                //TerrainVB tvb = vbs[z][x];
                TerrainVB tvb = vbs[z, x];

                tvb.setVBuffer(vb);
                Debug.LogFormat("Checking vbuffer {0}", vbs[z, x].vbuffer == null ? "null" : vbs[z, x].vbuffer);
            }
        }
        return true;
    }
    public void GetGeometry(int x, int z, ref TerrainVB vb, ref int start_v)
    {
        int
            bx = x / VBHASHEDBOXESX,
            bz = z / VBHASHEDBOXESZ;
        //vb = vbs[bz][bx];
        vb = vbs[bz, bx];
        start_v = ((x - bx * VBHASHEDBOXESX) + (z - bz * VBHASHEDBOXESZ) * VBHASHEDBOXESX) * nPoints;
    }
    public void setInvalidateHook(int x, int z, TerrainBox box)
    {
        int
          bx = x / VBHASHEDBOXESX,
          bz = z / VBHASHEDBOXESZ;
        //vbs[bz][bx].setInvalidateHook(terrain);
        vbs[bz, bx].setInvalidateHook(terrain);
    }
    public void UnlockAll()
    {
        for (int z = 0; z < vbs.Height(); ++z)
        {
            for (int x = 0; x < vbs.Width(); ++x)
            {
                vbs[z, x].Unlock();
            }
        }

    }
    ~TerrainVBs() { }
};

public class VBPipe : VBuffer
{
    public const int VBF_DRAWABLE = 0x00000001;
    public const int VBF_READABLE = 0x00000002;
    public const int VBF_CLIPPED = 0x00000004;

    protected   int lock_start;
    protected   int queried_size;
    protected   int max_size;
}

public interface iVBManager : IRefMem
{
    public VBPipe CreateVBPipe(int needed_size, DWORD FVF, DWORD flags);
    public VBuffer CreateVBuffer(int needed_size, DWORD FVF, DWORD flags);
    public void restore();
};
