using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Geometry;
using static CollideClass;
using static TerrainDefs;

public class TERRAIN_DATA
{
    public T_HEADER Header;
    public T_MAT Materials;

    /*   TerrainFile<T_SQUARE, SQUARES_PAGE_SIZE> Squares;
       TerrainFile<T_BOX, BOXES_PAGE_SIZE> Boxes;
       TerrainFile<T_VBOX, VBOXES_PAGE_SIZE> VBoxes;*/
    //TerrainFile<T_LIGHT, LIGHT_PAGE_SIZE> Lights[2];
    //TerrainFile<T_LIGHT, LIGHT_PAGE_SIZE>[] Lights; //2
    //public TerrainFileSQ Squares;
    public TerrainFile<T_SQUARE> Squares;
    //public TerrainFileBX Boxes;
    public TerrainFile<T_BOX> Boxes;
    //public TerrainFileVB VBoxes;
    public TerrainFile<T_VBOX> VBoxes;

    private string ext;
    private string name;

    public TERRAIN_DATA() : this("undefined")
    {

    }
    public TERRAIN_DATA(string name)
    {
        this.name = name;
        //Squares = new TerrainFileSQ();
        Squares = new TerrainFile<T_SQUARE>();
        VBoxes = new TerrainFile<T_VBOX>();
        Boxes = new TerrainFile<T_BOX>();
    }

    public bool Open(string name)
    {
        this.name = name;
        return OpenHdr();
    }

    public void Close()
    {
        //Boxes.Close();
        //Squares.Close();
        //VBoxes.Close();
        //Lights[0].Close();
        //Lights[1].Close();
        //SafeRelease(RandomObjects); RandomObjects = 0;
        //if (save_hdr)
        //    SaveHdr();
    }

    public bool OpenVb(bool OpenOld, bool CanWrite)
    {
        int Bx = Header.SizeXBPages, Bz = Header.SizeZBPages;
        ext = ".vb";

        //if (!VBoxes.Open(TerrainDefs.MAPDIR + name + ext, OpenOld, CanWrite, Bx * TerrainDefs.BPAGE_IN_VBPAGES, Bz * TerrainDefs.BPAGE_IN_VBPAGES, TerrainDefs.VBOXES_PAGE_SIZE))
        //if (!VBoxes.Open(ProductDefs.GetPI().getHddFile(name + ext), OpenOld, CanWrite, Bx * TerrainDefs.BPAGE_IN_VBPAGES, Bz * TerrainDefs.BPAGE_IN_VBPAGES, TerrainDefs.VBOXES_PAGE_SIZE))
        if (!VBoxes.Open(name + ext, OpenOld, CanWrite, Bx * TerrainDefs.BPAGE_IN_VBPAGES, Bz * TerrainDefs.BPAGE_IN_VBPAGES, TerrainDefs.VBOXES_PAGE_SIZE))
        {
            return false;
        }
        return true;
    }

    public bool OpenBx(bool OpenOld, bool CanWrite)
    {
        int Bx = Header.SizeXBPages, Bz = Header.SizeZBPages;
        ext = ".bx";

        //if (!Boxes.Open(ProductDefs.GetPI().getHddFile(name + ext), OpenOld, CanWrite, Bx, Bz))
        if (!Boxes.Open(name + ext, OpenOld, CanWrite, Bx, Bz,TerrainDefs.BOXES_PAGE_SIZE))
        {
            //            Boxes.Close();
            return false;
        }
        return true;
    }

    public bool OpenSq(bool OpenOld, bool CanWrite)
    {
        int Bx = Header.SizeXBPages, Bz = Header.SizeZBPages;
        ext = ".sq";
        //if (!Squares.Open(ProductDefs.GetPI().getHddFile(name + ext), OpenOld, CanWrite, Bx * TerrainDefs.SQUARES_IN_BOX, Bz * TerrainDefs.SQUARES_IN_BOX, TerrainDefs.SQUARES_PAGE_SIZE))
        if (!Squares.Open(name + ext, OpenOld, CanWrite, Bx * TerrainDefs.SQUARES_IN_BOX, Bz * TerrainDefs.SQUARES_IN_BOX, TerrainDefs.SQUARES_PAGE_SIZE))
        {
            //Squares.Close();
            return false;
        }
        return true;
    }
    public bool OpenHdr()
    {
        ext = ".hd";

        Header = new T_HEADER(0, 0, 0);
        //string filename = ProductDefs.GetPI().getHddFile(name + ext);
        string filename = name + ext;
        if (!File.Exists(filename))
        {
            Debug.Log("No file found " + filename);
            return false;
        }

        FileStream stream = File.OpenRead(filename);
        byte[] buffer = new byte[4];
        stream.Read(buffer);
        Header.SizeXBPages = BitConverter.ToInt32(buffer);

        stream.Read(buffer);
        Header.SizeZBPages = BitConverter.ToInt32(buffer);

        float[] tmpDir = new float[3];
        for (int i = 0; i < TerrainDefs.T_MAX_LIGHTMAPS; i++)
        {

            for (int j = 0; j < 3; j++)
            {
                stream.Read(buffer);
                tmpDir[j] = BitConverter.ToInt32(buffer);

            }
            Vector3 dir = new Vector3(tmpDir[0], tmpDir[1], tmpDir[2]);
            //Debug.Log(dir);

            stream.Read(buffer);
            int isValid = BitConverter.ToInt32(buffer);
            //Debug.Log(isValid);

            Header.light_maps[i] = new T_LightDesc(dir, isValid);
        }

        stream.Read(buffer);
        Header.nMaterials = BitConverter.ToInt32(buffer);

        Materials = new T_MAT(32);
        for (int i = 0; i < Header.nMaterials; i++)
        {
            stream.Read(buffer);
            Materials.SurType[i] = BitConverter.ToInt32(buffer);
        }
        return true;
    }

    internal bool TraceLine(Line line, ref TraceResult r)
    {
        //Debug.Log("Traceline " + line);
        float[] Clipped = { 0, line.dist };
        
        //Debug.Log("ClipLine");
        if (!ClipLine(line, ref Clipped))
            return TraceLineOut(line, ref r, 0, line.dist);
        
        //Debug.Log("Clipped[0] " + Clipped[0]);
        if (0 != Clipped[0])
            if (TraceLineOut(line, ref r, 0, Clipped[0]))
                return true;

        //Debug.Log(string.Format("TraceLineIn {0} {1}",Clipped[0], Clipped[1]));
        if (TraceLineIn(line, ref r, Clipped[0], Clipped[1]))
            return true;

        //Debug.Log("line.dist > Clipped[1]");
        if (line.dist > Clipped[1])
            return TraceLineOut(line, ref r, Clipped[1], line.dist);

        //Debug.Log(string.Format("Clipped[0] {0} Clipped[1] {1} line.dist {2} from {3}", Clipped[0], Clipped[1], line.dist, line.org));
        return false;
    }

    bool TraceBoxLine(Line l, ref TraceResult r, float startdist, float enddist, T_BOX Box, T_VBOX VBox)
    {
        /*  hlog->Message("Tracing Box");
          hlog->Message(" from (%f %f %f) to (%f %f %f)\n",
                                          l.org.x+l.dir.x*startdist,l.org.y+l.dir.y*startdist,l.org.z+l.dir.z*startdist,l.org.x+l.dir.x*enddist,l.org.y+l.dir.y*enddist,l.org.z+l.dir.z*enddist);


          hlog->Message(" heights : %f to %f.",lo,hi);
        */
        //Debug.Log(string.Format("Probing Tracing Box sd{0} ed{1} HS {2} Hi {3} Lo{4} wl {5} wh {6}",startdist,enddist,HeightScale,Box.Hi, Box.Lo, VBox.water_lo,VBox.water_hi));
        //Debug.Log(string.Format("l.org.y + l.dir.y * startdist {0} > HeightScale * Box.Hi {1} ", l.org.y + l.dir.y * startdist, HeightScale * Box.Hi));
        //Debug.Log(string.Format("l.org.y + l.dir.y * enddist {0} > HeightScale * Box.Hi {1} ", l.org.y + l.dir.y * enddist, HeightScale * Box.Hi));
        if ((l.org.y + l.dir.y * startdist > HeightScale * Box.Hi)
          && (l.org.y + l.dir.y * enddist > HeightScale * Box.Hi))
        {
            if ((l.org.y + l.dir.y * startdist > HeightScale * VBox.water_lo)
              && (l.org.y + l.dir.y * enddist < HeightScale * VBox.water_hi))
            {
                //  .
                //   \
                //  ~~\~~~~~~~~~~~ <- water
                //     \.
                //     ___
                //    /    \       <- ground
                //  /        \


                //float Clipped[2]={startdist,enddist};
                //if( PLANE(VECTOR(0,-1,0),VBox.water_level).ClipVector(O,D,Clipped) ) {
                r.cls = COLC_WATER;

                //  l.org.y+l.dir.y*dist = (HeightScale*( (int)VBox.water_lo+
                r.dist =
                  (HeightScale * ((int)VBox.water_lo + (int)VBox.water_hi) * .5f - l.org.y) / l.dir.y;

                r.normal = Vector3.up;
                r.ground_type = GT_WATER;
                return true;
                //}
            }
            return false;
        }

        /*  if ( ( l.org.y+l.dir.y*startdist<HeightScale*Box.Lo ) 
            && ( l.org.y+l.dir.y*enddist  <HeightScale*Box.Lo ) ) { 
            hlog.Message("Bad box!");
            //AssertBp(0); 
          }*/

        ITERATION2D Iterator = new ITERATION2D(
          l.org.x + l.dir.x * startdist,
          l.org.z + l.dir.z * startdist,
          l.dir.x, l.dir.z, SQUARE_SIZE, OO_SQUARE_SIZE);

        //Debug.Log("Probing ground pre-final");
        float curdist = startdist, nextdist;
        while ((nextdist = Iterator.GetPosition() + startdist) < enddist)
        {
            if (TraceSquareLine(l, ref r, curdist - APRECISION, nextdist + APRECISION,
                                 Iterator.xIter.Box, Iterator.yIter.Box))
                return true;

            curdist = nextdist;
            Iterator.Next();
        }

        //Debug.Log("Probing ground final");
        return TraceSquareLine(l, ref r, curdist - APRECISION, enddist + APRECISION, Iterator.xIter.Box, Iterator.yIter.Box);
    }

    /// <summary>
    /// checks single terrain square
    /// </summary>
    /// <param name="l"></param>
    /// <param name="r"></param>
    /// <param name="startdist"></param>
    /// <param name="enddist"></param>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    bool TraceSquareLine(Line l, ref TraceResult r, float startdist, float enddist, int x, int z)
    {
        /*  
          hlog.Message("Tracing Facet from (%f %f %f) to (%f %f %f) \n",
                                 l.org.x+l.dir.x*startdist,l.org.y+l.dir.y*startdist,l.org.z+l.dir.z*startdist,
                                 l.org.x+l.dir.x*enddist,l.org.y+l.dir.y*enddist,l.org.z+l.dir.z*enddist);
          hlog.Message("Bound by X:%f  Z:%f \n",x*float(SQUARE_SIZE),z*float(SQUARE_SIZE));
          */
        float miny;

        float sx, sy, sz, ey;
        {
            sy = l.org.y + l.dir.y * startdist;
            ey = l.org.y + l.dir.y * enddist;
            miny = sy < ey ? sy : ey;
        }

        float
          H = HeightScale * Squares.pager.Get(x, z).Height,
          Hx = HeightScale * Squares.pager.Get(x + 1, z).Height,
          Hz = HeightScale * Squares.pager.Get(x, z + 1).Height,
          Hxz = HeightScale * Squares.pager.Get(x + 1, z + 1).Height;

        if ((miny > H) && (miny > Hx) && (miny > Hz) && (miny > Hxz)) return false;

        int
          Flag = Squares.pager.Get(x, z).Flag;

        float
          X = x * SQUARE_SIZE,
          Z = z * SQUARE_SIZE;

        sx = l.org.x;
        sy = l.org.y;
        sz = l.org.z;


        if ((Flag & SQF_SIMMETRY) != 0)
        {
            {
                Vector3 norm = new Vector3(Hz - Hxz, SQUARE_SIZE, H - Hz);
                float dp = Vector3.Dot(l.dir, norm);
                if (dp < 0)
                {
                    Vector3 T = new Vector3(X - sx, H - sy, Z - sz);
                    float dst = Vector3.Dot(T, norm) / dp;
                    if ((dst > startdist) && (dst < enddist) && ((T.x - T.z + (-l.dir.x + l.dir.z) * dst) > 0))
                    {
                        r.dist = dst;
                        r.normal = norm;
                        goto TRACED;
                    }
                }
            }
            {
                Vector3 norm = new Vector3(H-Hx,SQUARE_SIZE,Hx - Hxz);
                float dp = Vector3.Dot(l.dir,norm);
                if (dp < 0)
                {
                    Vector3 T = new Vector3(X-sx,H - sy,Z - sz);
                    float dst = Vector3.Dot(T,norm) / dp;
                    if ((dst > startdist) && (dst < enddist) && ((T.x - T.z + (-l.dir.x + l.dir.z) * dst) < 0))
                    {
                        r.dist = dst;
                        r.normal = norm;
                        goto TRACED;
                    }
                }
            }
        }
        else
        {
            {
                Vector3 norm = new Vector3(Hz-Hxz,SQUARE_SIZE,Hx - Hxz);
                float dp = Vector3.Dot(l.dir, norm);
                if (dp < 0)
                {
                    Vector3 T = new Vector3(X+SQUARE_SIZE - sx,Hxz - sy,Z + SQUARE_SIZE - sz);
                    float dst = Vector3.Dot(T , norm) / dp;
                    if ((dst > startdist) && (dst < enddist) && ((l.dir.x + l.dir.z) * dst > T.x + T.z - SQUARE_SIZE))
                    {
                        r.dist = dst;
                        r.normal = norm;
                        goto TRACED;
                    }
                }
            }
            {
                Vector3 norm = new Vector3(H-Hx,SQUARE_SIZE,H - Hz);
                float dp = Vector3.Dot(l.dir , norm);
                if (dp < 0)
                {
                    Vector3 T = new Vector3(X-sx,H - sy,Z - sz);
                    float dst = Vector3.Dot(T, norm) / dp;
                    if ((dst > startdist) && (dst < enddist) && ((l.dir.x + l.dir.z) * dst < T.x + T.z + SQUARE_SIZE))
                    {
                        r.dist = dst;
                        r.normal = norm;
                        goto TRACED;
                    }
                }
            }
        }
        return false;
    TRACED:
        r.cls = COLC_GROUND;
        r.ground_type = Flag & SQF_GRMASK;
        //Debug.Log(string.Format("Checking ground type: {0} mask {1} [2]",Flag.ToString("X8"),SQF_GRMASK.ToString("X8"), r.ground_type));
        r.coll_object = null;
        return true;
    }

    const int APRECISION = 0;
    bool TraceLineIn(Line l, ref TraceResult r, float startdist, float enddist)
    {

        ITERATION2D Iterator = new ITERATION2D(l.org.x + l.dir.x * startdist,
                             l.org.z + l.dir.z * startdist,
                             l.dir.x, l.dir.z, BOX_SIZE, OO_BOX_SIZE);

        //Debug.Log(string.Format("TraceLineIn loop Probing VBOX/TBOX [{0}:{1}]", Iterator.xIter.Box, Iterator.yIter.Box));
        //Debug.Log(string.Format("Iterator pos {0} + startdist {1} < enddist {2}", Iterator.GetPosition(),startdist , enddist));
        float curdist = startdist, nextdist;
        while ((nextdist = Iterator.GetPosition() + startdist) < enddist)
        {
            //Debug.Log(string.Format("Probing VBOX/TBOX [{0}:{1}]",Iterator.xIter.Box, Iterator.yIter.Box));
            if (TraceBoxLine(l, ref r, curdist - APRECISION, nextdist + APRECISION,
               Boxes.pager.Get(Iterator.xIter.Box, Iterator.yIter.Box),
              VBoxes.pager.Get(Iterator.xIter.Box, Iterator.yIter.Box)))
                return true;
            curdist = nextdist;
            Iterator.Next();
        }
        //Debug.Log(string.Format("TraceLineIn final Probing VBOX/TBOX [{0}:{1}]", Iterator.xIter.Box, Iterator.yIter.Box));
        //Debug.Log(string.Format("Pager size: {0} pages {1}", Boxes.pager.SizeX(), Boxes.pager.SizeXPages()));
        return TraceBoxLine(l, ref r, curdist - APRECISION, enddist + APRECISION,
          Boxes.pager.Get(Iterator.xIter.Box, Iterator.yIter.Box),
          VBoxes.pager.Get(Iterator.xIter.Box, Iterator.yIter.Box));
    }

    bool TraceLineOut(Line line, ref TraceResult r, float startdist, float enddist)
    {
        //VECTOR n = VECTOR(0, -1, 0);
        Vector3 n = Vector3.down;

        float d = 0;

        float dd = Vector3.Dot(line.org, n) + d;
        float dn = Vector3.Dot(line.dir, n);
        float param = -dd / dn; // cross parameter

        //Debug.Log(string.Format("start {0} end {1} param {2}",startdist,enddist,param));
        //Debug.Log(string.Format("line.org {0} line.dir {1}", line.org, line.dir));
        if (startdist <= param && param <= enddist)
        {
            r.cls = COLC_GROUND;
            r.dist = param;
            r.normal = Vector3.up;
            r.ground_type = GT_WATER;
            r.coll_object = null;
            return true;
        }
        return false;
    }

    public bool ClipLine(Line l, ref float[] Clipped)
    { // TODO возможно, нужно ref Line
        float Indent = 16;

        //Engine.DebugLine(l.org, l.org + l.dir * l.dist, Color.yellow);
        if (!new PLANE(Vector3.left, Indent).ClipVector(l.org, l.dir, ref Clipped)) return false;
        if (!new PLANE(Vector3.back, Indent).ClipVector(l.org, l.dir, ref Clipped)) return false;
        if (!new PLANE(Vector3.right, -(GetXSize() - Indent)).ClipVector(l.org, l.dir, ref Clipped)) return false;
        if (!new PLANE(Vector3.forward, -(GetZSize() - Indent)).ClipVector(l.org, l.dir, ref Clipped)) return false;
        return true;
    }


    public override string ToString()
    {
        return Header.SizeXBPages + "x" + Header.SizeZBPages + " nMaterials " + Header.nMaterials;
    }

    public int getXSquares() { return Header.SizeXBPages * TerrainDefs.BOXES_PAGE_SIZE * TerrainDefs.SQUARES_IN_BOX; }
    public int getZSquares() { return Header.SizeZBPages * TerrainDefs.BOXES_PAGE_SIZE * TerrainDefs.SQUARES_IN_BOX; }

    public float GetXSize() { return getXSquares() * TerrainDefs.SQUARE_SIZE; }
    public float GetZSize() { return getZSquares() * TerrainDefs.SQUARE_SIZE; }

    public int GroundPass(int x, int z)
    {
        return Squares.pager.GetCl(x, z).Flag & (int)terrpdef.PASS_ANGLE_MASK;
    }

    public float GroundLevel(float x, float z)
    {
        int X = (int)Mathf.Floor(x * TerrainDefs.OO_SQUARE_SIZE); x -= (X * TerrainDefs.SQUARE_SIZE);
        int Z = (int)Mathf.Floor(z * TerrainDefs.OO_SQUARE_SIZE); z -= (Z * TerrainDefs.SQUARE_SIZE);
        X = Mathf.Clamp(X, 0, Squares.pager.SizeXPages() * TerrainDefs.SQUARES_PAGE_SIZE - 2);
        Z = Mathf.Clamp(Z, 0, Squares.pager.SizeZPages() * TerrainDefs.SQUARES_PAGE_SIZE - 2);
        float H, Hx, Hz, Hxz;
        H = TerrainDefs.HeightScale * Squares.pager.Get(X, Z).Height;
        Hx = TerrainDefs.HeightScale * Squares.pager.Get(X + 1, Z).Height;
        Hz = TerrainDefs.HeightScale * Squares.pager.Get(X, Z + 1).Height;
        Hxz = TerrainDefs.HeightScale * Squares.pager.Get(X + 1, Z + 1).Height;
        ushort Flag = Squares.pager.Get(X, Z).Flag;
        if ((Flag & TerrainDefs.SQF_SIMMETRY) != 0)
            return (x < z ? (H + (x * (Hxz - Hz) + z * (Hz - H)) * TerrainDefs.OO_SQUARE_SIZE) : (H + (x * (Hx - H) + z * (Hxz - Hx)) * TerrainDefs.OO_SQUARE_SIZE));
        else
            return (x > (TerrainDefs.SQUARE_SIZE - z) ? (Hxz + ((TerrainDefs.SQUARE_SIZE - x) * (Hz - Hxz) + (TerrainDefs.SQUARE_SIZE - z) * (Hx - Hxz)) * TerrainDefs.OO_SQUARE_SIZE) : (H + (x * (Hx - H) + z * (Hz - H)) * TerrainDefs.OO_SQUARE_SIZE));
    }
    /// <summary>
    /// Высота поверхности земли
    /// </summary>
    /// <param name="x">координата Х</param>
    /// <param name="z">координата Y</param>
    /// <param name="r">Радиус???</param>
    /// <returns>Высота точки внутри блока</returns>
    public float GroundLevelMedian(float x, float z, float r)
    {

        int X = (int)Mathf.Clamp(((x - r) * TerrainDefs.OO_SQUARE_SIZE - .5f), 0, Squares.pager.SizeXPages() * TerrainDefs.SQUARES_PAGE_SIZE - 1);
        int Z = (int)Mathf.Clamp(((z - r) * TerrainDefs.OO_SQUARE_SIZE - .5f), 0, Squares.pager.SizeZPages() * TerrainDefs.SQUARES_PAGE_SIZE - 1);
        int eX = (int)Mathf.Clamp(((x + r) * TerrainDefs.OO_SQUARE_SIZE + .5f), 0, Squares.pager.SizeXPages() * TerrainDefs.SQUARES_PAGE_SIZE - 1);
        int eZ = (int)Mathf.Clamp(((z + r) * TerrainDefs.OO_SQUARE_SIZE + .5f), 0, Squares.pager.SizeXPages() * TerrainDefs.SQUARES_PAGE_SIZE - 1);
        float GL = .0f;
        for (int j = Z; j < eZ; j++)
            for (int i = X; i < eX; i++)
                GL += Squares.pager.Get(i, j).Height;
        int num = (eX - X) * (eZ - Z);
        return (num > 0 ? TerrainDefs.HeightScale * GL / num : .0f);
    }
    public void GroundLevel(float x, float z, out TraceResult cur)
    {
        cur = new TraceResult();
        int X = (int)Mathf.Floor(x * TerrainDefs.OO_SQUARE_SIZE); x -= (X * TerrainDefs.SQUARE_SIZE);
        int Z = (int)Mathf.Floor(z * TerrainDefs.OO_SQUARE_SIZE); z -= (Z * TerrainDefs.SQUARE_SIZE);
        X = Math.Clamp(X, 0, Squares.pager.SizeXPages() * TerrainDefs.SQUARES_PAGE_SIZE - 2);
        Z = Math.Clamp(Z, 0, Squares.pager.SizeZPages() * TerrainDefs.SQUARES_PAGE_SIZE - 2);
        float H, Hx, Hz, Hxz, l;
        H = TerrainDefs.HeightScale * Squares.pager.Get(X, Z).Height;
        Hx = TerrainDefs.HeightScale * Squares.pager.Get(X + 1, Z).Height;
        Hz = TerrainDefs.HeightScale * Squares.pager.Get(X, Z + 1).Height;
        Hxz = TerrainDefs.HeightScale * Squares.pager.Get(X + 1, Z + 1).Height;
        ushort Flag = Squares.pager.Get(X, Z).Flag;
        if ((Flag & TerrainDefs.SQF_SIMMETRY) != 0)
        {
            if (x < z)
            {
                cur.normal.Set(Hz - Hxz, TerrainDefs.SQUARE_SIZE, H - Hz);
                l = (H + (x * (Hxz - Hz) + z * (Hz - H)) * TerrainDefs.OO_SQUARE_SIZE);
            }
            else
            {
                cur.normal.Set(H - Hx, TerrainDefs.SQUARE_SIZE, Hx - Hxz);
                l = (H + (x * (Hx - H) + z * (Hxz - Hx)) * TerrainDefs.OO_SQUARE_SIZE);
            }
        }
        else
        {
            if (x > (TerrainDefs.SQUARE_SIZE - z))
            {
                cur.normal.Set(Hz - Hxz, TerrainDefs.SQUARE_SIZE, Hx - Hxz);
                l = (Hxz + ((TerrainDefs.SQUARE_SIZE - x) * (Hz - Hxz) + (TerrainDefs.SQUARE_SIZE - z) * (Hx - Hxz)) * TerrainDefs.OO_SQUARE_SIZE);
            }
            else
            {
                cur.normal.Set(H - Hx, TerrainDefs.SQUARE_SIZE, H - Hz);
                l = (H + (x * (Hx - H) + z * (Hz - H)) * TerrainDefs.OO_SQUARE_SIZE);
            }
        }
        cur.cls = CollideClass.COLC_GROUND;
        cur.dist = l;
        cur.ground_type = Flag & TerrainDefs.SQF_GRMASK;
        cur.org = new Vector3(x, l, z);
        cur.coll_object = null;
    }

    public float WaterLevel(float x, float z)
    {
        int X = (int)Mathf.Floor(x * TerrainDefs.OO_BOX_SIZE); x = x * TerrainDefs.OO_BOX_SIZE - X;
        int Z = (int)Mathf.Floor(z * TerrainDefs.OO_BOX_SIZE); z = z * TerrainDefs.OO_BOX_SIZE - Z;
        X = (int)Mathf.Clamp(X, 0, Boxes.pager.SizeXPages() * TerrainDefs.BOXES_PAGE_SIZE - 2);
        Z = (int)Mathf.Clamp(Z, 0, Boxes.pager.SizeZPages() * TerrainDefs.BOXES_PAGE_SIZE - 2);
        float H, Hx, Hz, Hxz;
        H = VBoxes.pager.Get(X, Z).water_level;
        Hx = VBoxes.pager.Get(X + 1, Z).water_level;
        Hz = VBoxes.pager.Get(X, Z + 1).water_level;
        Hxz = VBoxes.pager.Get(X + 1, Z + 1).water_level;
        return TerrainDefs.HeightScale * ((H * z + Hx * (1 - z)) * x + (Hz * z + Hxz * (1 - z)) * (1 - x));
    }

    public void WaterLevel(float x, float z, out TraceResult cur)
    {
        cur = new TraceResult();
        int X = (int)Mathf.Floor(x * TerrainDefs.OO_BOX_SIZE); x = x * TerrainDefs.OO_BOX_SIZE - X;
        int Z = (int)Mathf.Floor(z * TerrainDefs.OO_BOX_SIZE); z = z * TerrainDefs.OO_BOX_SIZE - Z;
        //Debug.Log("BOxes " + Boxes.pager.SizeXPages());
        //Debug.Log("VBoxes " + VBoxes.pager.SizeXPages());
        X = Math.Clamp(X, 0, Boxes.pager.SizeXPages() * TerrainDefs.BOXES_PAGE_SIZE - 2);
        Z = Math.Clamp(Z, 0, Boxes.pager.SizeZPages() * TerrainDefs.BOXES_PAGE_SIZE - 2);
        float H, Hx, Hz, Hxz;
        H = VBoxes.pager.Get(X, Z).water_level;
        Hx = VBoxes.pager.Get(X + 1, Z).water_level;
        Hz = VBoxes.pager.Get(X, Z + 1).water_level;
        Hxz = VBoxes.pager.Get(X + 1, Z + 1).water_level;
        //#pragma message ("Water normal could be fine calculated ( gradient )")
        cur.cls = CollideClass.COLC_WATER;
        cur.dist = TerrainDefs.HeightScale * ((H * z + Hx * (1 - z)) * x + (Hz * z + Hxz * (1 - z)) * (1 - x));
        cur.ground_type = TerrainDefs.GT_WATER;
        cur.org = new Vector3(x, cur.dist, z);
        cur.normal = Vector3.up;
        cur.coll_object = null;
    }

}

public class terrpdef
{
    //#define PASS_ANGLE_0     0.707107     // cos(45)
    //#define PASS_ANGLE_1     0.642788     // cos(50)
    //#define PASS_ANGLE_2     0.573576     // cos(55)

    public const float PASS_ANGLE_IN_DEG = 38;           // 38
    public const float PASS_ANGLE = 0.788011f;     // cos(38)
                                                   //#define PASS_ANGLE_0     0.866025     // cos(30)
                                                   //#define PASS_ANGLE_1     0.788011     // cos(38)
                                                   //#define PASS_ANGLE_2     0.707107     // cos(45)
                                                   //#define PASS_ANGLE_2     0.642788     // cos(50)
    public const float PASS_WATER_H = 3.2f;

    public const uint PASS_ANGLE_MASK = (1 << 11);
    public const uint PASS_SECOND_ANGLE_MASK = (1 << 12);
    public const float PASS_ANGLE_NONE_MASK = 0x00000000;
}
/*public class TerrainFile<T,Z>
{
    private FileStream file;
    private Pager<T,Z> pager;
    private string tfAccessMode;
    public bool Open(string name,bool OpenOld,bool Write,int SizeX,int SizeZ)
    {
        file=File.OpenRead(name);
        byte[] data = File.ReadAllBytes(name);
        
        pager = new Pager<T, Z>(null, SizeX, SizeZ);
        return true;
    }
}*/

public class TerrainFile<T>
{
    //private FileStream file;
    //private Stream file;
    public Pager<T, T> pager;


    public bool Open(string name, bool OpenOld, bool Write, int SizeX, int SizeZ, int dim)
    {
        if (!File.Exists(name))
        {
            Debug.Log("Terrainfile not found: " + name);
            return false;
        }
        MemoryStream ms = new MemoryStream();
        ms.Write(File.ReadAllBytes(name));

        //file = File.OpenRead(name);
        //pager = new Pager<T, T>(file, SizeX, SizeZ, dim);
        pager = new Pager<T, T>(ms, SizeX, SizeZ, dim);

        return true;
    }
    public bool Open(string name, bool OpenOld, bool Write, int SizeX, int SizeZ)
    {
        return Open(name, OpenOld, Write, SizeX, SizeZ, 0);
    }
}

public class TerrainFileVB
{
    private FileStream file;
    public Pager<T_VBOX, T_VBOX> pager;

    public bool Open(string name, bool OpenOld, bool Write, int SizeX, int SizeZ)
    {
        file = File.OpenRead(name);
        pager = new Pager<T_VBOX, T_VBOX>(file, SizeX, SizeZ, TerrainDefs.VBOXES_PAGE_SIZE);
        return true;
    }
}
public class TerrainFileBX
{
    private FileStream file;
    public Pager<T_BOX, T_BOX> pager;


}
public class TerrainFileSQ
{
    private FileStream file;
    public Pager<T_SQUARE, T_SQUARE> pager;

    public bool Open(string name, bool OpenOld, bool Write, int SizeX, int SizeZ)
    {
        //byte[] data = File.ReadAllBytes(name);
        file = File.OpenRead(name);

        //T_SQUARE[] data = Convert(file,SizeX,SizeZ);
        pager = new Pager<T_SQUARE, T_SQUARE>(file, SizeX, SizeZ, TerrainDefs.SQUARES_PAGE_SIZE);
        return true;
    }

    public void Close()
    {
        //pager.SetData(0, 0, 0);
        file.Close();

    }
    private T_SQUARE[] Convert(FileStream data, int SizeX, int SizeZ)
    {
        /*        int Bx = Header.SizeXBPages, Bz = Header.SizeZBPages;

                _strcpy(Ext, ".sq");
                if (!Squares.Open(Name, OpenOld, CanWrite, Bx * SQUARES_IN_BOX, Bz * SQUARES_IN_BOX))
                {
                    Squares.Close();
                    return false;
                }
                return true;*/
        T_SQUARE[] squares = new T_SQUARE[SizeX * SizeZ];
        byte[] HeightBytes = new byte[2];
        byte[] FlagBytes = new byte[2];
        for (int Bz = 0; Bz < SizeZ; Bz++)
        {
            for (int Bx = 0; Bx < SizeX; Bx++)
            {
                for (int Pz = 0; Pz < TerrainDefs.SQUARES_PAGE_SIZE; Pz++)
                {
                    for (int Px = 0; Px < TerrainDefs.SQUARES_PAGE_SIZE; Px++)
                    {
                        data.Read(HeightBytes);
                        short Height = BitConverter.ToInt16(HeightBytes);
                        ushort Flag = BitConverter.ToUInt16(FlagBytes);
                        int squareIndex = (Bx + Bz * SizeZ) + (Px + Pz * TerrainDefs.SQUARES_PAGE_SIZE);
                        Debug.Log((squareIndex, squares));
                        squares[squareIndex] = new T_SQUARE(Height, Flag);
                        //if (squareIndex > 100) return default;
                    }
                }
            }
        }
        return squares;
    }
}