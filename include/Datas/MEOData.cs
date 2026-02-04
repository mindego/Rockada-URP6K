using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Xml.Linq;
using System.Threading.Tasks;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class SLOT_DATA
{
    public int OfName;
    public Vector3 Org, Dir, Up;
    //[MarshalAs(UnmanagedType.LPStr, SizeConst = 0)]
    //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0)]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    public char[] NameArray;
    //public string Name;

    public string Name
    {
        get
        {
            return new string(NameArray);
        }
    }
    public override string ToString()
    {
        return "MEO Slot " + Name;
    }

}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class IMAGE
{
    //size=36
    public Vector3 Min, Max;
    public float Radius;
    public int nBoxes;
    public int OfPlanes;
    public int OfPlGrps;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    public PLANE[] Planes;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    public short[] PlGrps;

    public void Init(MEO_DATA_HDR Mh)
    {
        //Planes = Mh.Planes + OfPlanes;
        //PlGrps = Mh.PlGrps + OfPlGrps;
        int Planes_num = Mh.Planes.Length - OfPlanes;
        int PlGrps_num = Mh.PlGrps.Length - OfPlGrps;
        Planes = new PLANE[Planes_num];
        PlGrps = new short[PlGrps_num];
        Array.Copy(Mh.Planes, OfPlanes, Planes, 0, Planes_num);
        Array.Copy(Mh.PlGrps, OfPlGrps, PlGrps, 0, PlGrps_num);
    }

    public override string ToString()
    {
        return "Image data: " + Radius + " boxes: " + nBoxes;
    }

    internal bool TraceLine(Vector3 O, Vector3 D, ref float Dist, ref Vector3 norm)
    {
        PLANE[] p = Planes;
        short[] n = PlGrps;
        bool traced = false;
        PLANE resp = null;
        int pIndex = 0;
        int nGrpIndex = 0;
        //p += n + 1; n++
        for (int nb = nBoxes; nb != 0; nb--)
        {          //enumerating boxes...

            bool OutBox = false;
            //TODO здесь, похоже, слишком мало boxes создаётся
            //Debug.Log(string.Format("DEBUGTRACE nBoxes {0} Planes: {1} grpIndex {2}/{3} ({4})", nBoxes, p.Length,nGrpIndex,n.Length, PlGrps[nGrpIndex]));
            //EngineDebug.DebugLine(O, D, Color.yellow, 10);
            PLANE ResPlane = BoxTraceLine(O, D, ref Dist, n[nGrpIndex], p, ref OutBox);
            //Debug.Log(string.Format("DEBUGTRACE O {0} D: {1} Dist: {2}", O, D,Dist));
            if (ResPlane != null)
            {
                resp = ResPlane;
                traced = true;
                if (!OutBox) break;
            }
            int len = p.Length - PlGrps[nGrpIndex];
            var tmp = new PLANE[len];
            //Debug.Log(string.Format("Array.Copy(p, PlGrps[nGrpIndex] + 1 {0}, tmp, 0,len {1})", PlGrps[nGrpIndex],len));
            //Array.Copy(p, PlGrps[nGrpIndex] + 1, tmp, 0,len);
            Array.Copy(p, PlGrps[nGrpIndex], tmp, 0, len);
            p = tmp;
            nGrpIndex++;
        }
        if (traced) norm = resp.n;
        return traced;
    }

    PLANE BoxTraceLine(Vector3 O, Vector3 D, ref float Dist, int nPlanes, PLANE[] Planes, ref bool OutTheBox)
    {
        float touter = 0, tinner = Dist, backinner = -float.MaxValue;
        PLANE ptmp = null, pback = null;
        int pIndex = 1;// skipping sphere approximation
        //Planes++;  // skipping sphere approximation
        bool OutBox = false;
        for (int i = nPlanes; i != 0; pIndex++, i--)
        {//checking a box's planes...
            PLANE myPlane = Planes[pIndex];

            float d = -(Vector3.Dot(O, myPlane.n));//(d - Planes->d) shows how O related to (*Planes)

            float dn = Vector3.Dot(D, myPlane.n);
            float tmp = (d - myPlane.d) / dn;//possible intersection parametrisation  

            if (d < myPlane.d)
            {     //*Planes is a foreplane ( Half-space that contains box not contains O)    
                OutBox = true;          // so O is not in the box
                if (tmp > touter)
                {   //if examined intersection is "father" then "current"-it becomes "current"
                    touter = tmp;
                    ptmp = Planes[0];
                }
                else
                  if (tmp < 0)         //Traced Ray directed out from the in-half-space,therefore out from box
                    return null;
            }
            else
            {
                if (tmp > 0)
                {      //backplane is on the way?
                    if (tmp < tinner) tinner = tmp; //is it closer then closest previous?
                }
                else
                {
                    if (tmp > backinner) { backinner = tmp; pback = Planes[0]; } //in case of !OutTheBox will be useful
                }
            }
            if (touter > tinner) return null;   //box intersected
        }
        if (OutBox)
        {
            Dist = touter;
            OutTheBox = true;
            return ptmp;
        }
        else
        {
            Dist = 0;
            OutTheBox = false;
            return pback;
        }
    }

    internal int WaterCollision(MATRIX World, TERRAIN_DATA td, ref CollideResult[] res)
    {
        int count = 0;
        Vector3 Projected = (World.ProjectPoint(Min));
        count += CheckVectorWaterCollision(Projected, td, ref res[count]);
        Projected += World.Right * (Max.x - Min.x);
        count += CheckVectorWaterCollision(Projected, td, ref res[count]);
        Projected += World.Up * (Max.y - Min.y);
        count += CheckVectorWaterCollision(Projected, td, ref res[count]);
        Projected -= World.Right * (Max.x - Min.x);
        count += CheckVectorWaterCollision(Projected, td, ref res[count]);
        Projected += World.Dir * (Max.z - Min.z);
        count += CheckVectorWaterCollision(Projected, td, ref res[count]);
        Projected += World.Right * (Max.x - Min.x);
        count += CheckVectorWaterCollision(Projected, td, ref res[count]);
        Projected -= World.Up * (Max.y - Min.y);
        count += CheckVectorWaterCollision(Projected, td, ref res[count]);
        Projected -= World.Right * (Max.x - Min.x);
        count += CheckVectorWaterCollision(Projected, td, ref res[count]);
        return count;
    }

    static int CheckVectorWaterCollision(Vector3 Projected, TERRAIN_DATA td, ref CollideResult res)
    {
        TraceResult tr;
        td.WaterLevel(Projected.x, Projected.z, out tr);
        if (tr.dist < Projected.y) return 0;
        res.org = Projected;
        res.normal = tr.normal;
        return 1;
    }

    internal int GroundCollision(MATRIX World, TERRAIN_DATA td, ref CollideResult[] res)
    {
        int count = 0;
        Vector3 Projected = (World.ProjectPoint(Min));
        count += CheckVectorGroundCollision(Projected, td, ref res[count]);
        Projected += World.Right * (Max.x - Min.x);
        count += CheckVectorGroundCollision(Projected, td, ref res[count]);
        Projected += World.Up * (Max.y - Min.y);
        count += CheckVectorGroundCollision(Projected, td, ref res[count]);
        Projected -= World.Right * (Max.x - Min.x);
        count += CheckVectorGroundCollision(Projected, td, ref res[count]);
        Projected += World.Dir * (Max.z - Min.z);
        count += CheckVectorGroundCollision(Projected, td, ref res[count]);
        Projected += World.Right * (Max.x - Min.x);
        count += CheckVectorGroundCollision(Projected, td, ref res[count]);
        Projected -= World.Up * (Max.y - Min.y);
        count += CheckVectorGroundCollision(Projected, td, ref res[count]);
        Projected -= World.Right * (Max.x - Min.x);
        count += CheckVectorGroundCollision(Projected, td, ref res[count]);

        //Debug.LogFormat("Hits {0} for {1}",count,this.GetHashCode().ToString("X8"));
        return count;
    }

    static int CheckVectorGroundCollision(Vector3 Projected, TERRAIN_DATA td, ref CollideResult res)
    {
        TraceResult tr;
        td.GroundLevel(Projected.x, Projected.z, out tr);
        if (tr.dist < Projected.y) return 0;
        //if (res == null) res = new CollideResult();
        res.org = Projected;
        res.normal = tr.normal;
        res.ground_type = tr.ground_type;
        return 1;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class MEO_DATA
{
    public const int Size = 224;//размер структуры в файле

    //size=208
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public IMAGE[] Images;// = new IMAGE[4]; //4
    public Vector3 Org, Dir, Up;
    int reserved;
    public int Name;
    public int Flags;
    public int nSlots;
    public int Of_Slots; //union SLOT_DATA *pSlots
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    public SLOT_DATA[] pSlots;
    public int nSubObjects;
    public int Of_SubObjects; //MEO_DATA* pSubObjects;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    public MEO_DATA[] pSubObjects;


    public override string ToString()
    {

        string res = "MEO_DATA\n";
        byte[] tmpNameInt = BitConverter.GetBytes(Name);
        string tmpName = BitConverter.ToString(tmpNameInt);

        //res += "Name " + Name.ToString("X8") + $" Slots {nSlots}" + $" SubObj {nSubObjects}" + "\n";
        res += "Name " + tmpName + $" Slots {nSlots}" + $" SubObj {nSubObjects}" + "\n";
        res += $"Org {Org}\n";
        res += $"Dir {Dir}\n";
        res += $"Up {Up}\n";

        return res;
    }

    //public void Init(MEO_DATA_HDR Mh, MEO_DATA Top)
    //{

    //    //pSlots = Mh._Slots + Of_Slots;
    //    //pSubObjects = Top + Of_SubObjects;
    //    int i;
    //    for (i = 0; i < 4; i++) Images[i].Init(Mh);

    //    for (i = 0; i < nSubObjects; i++) pSubObjects[i].Init(Mh, Top);
    //}
    public void Init(MEO_DATA_HDR Mh, MEO_DATA Top)
    {
        int i;
        pSlots = new SLOT_DATA[nSlots];
        for (i = 0; i < nSlots; i++)
        {
            pSlots[i] = Mh._Slots[Of_Slots + i];
        }
        pSubObjects = new MEO_DATA[nSubObjects];
        for (i = 0; i < nSubObjects; i++)
        {
            pSubObjects[i] = Mh.MeoData[Of_SubObjects + i];
        }

        //pSlots = Mh._Slots + Of_Slots;
        //pSubObjects = Top + Of_SubObjects;
        for (i = 0; i < 4; i++) Images[i].Init(Mh);

        for (i = 0; i < nSubObjects; i++) pSubObjects[i].Init(Mh, Top);
    }

    public void Load(Stream ms, long position)
    {
        // Debug.Log("Loading MEO_DATA @" + position);
        ms.Seek(position, SeekOrigin.Begin);
        Images = new IMAGE[4];
        for (int i = 0; i < 4; i++)
        {
            Images[i] = StormFileUtils.ReadStruct<IMAGE>(ms, ms.Position);
            //Debug.Log("Image[" + i +"] " +Images[i] + " Offset planes " + Images[i].OfPlanes);
        }
        Org = StormFileUtils.ReadStruct<Vector3>(ms, ms.Position);
        // Debug.Log("Org " + Org);
        Dir = StormFileUtils.ReadStruct<Vector3>(ms, ms.Position);
        // Debug.Log("Dir " + Dir);
        Up = StormFileUtils.ReadStruct<Vector3>(ms, ms.Position);
        // Debug.Log("Up " + Up);
        reserved = StormFileUtils.ReadStruct<int>(ms, ms.Position);
        // Debug.Log("Reserved " + reserved);
        Name = StormFileUtils.ReadStruct<int>(ms, ms.Position);
        // Debug.Log("Name " + Name.ToString("X8"));
        Flags = StormFileUtils.ReadStruct<int>(ms, ms.Position);
        // Debug.Log("Flags " + Flags);
        nSlots = StormFileUtils.ReadStruct<int>(ms, ms.Position);
        // Debug.Log("nSlots " + nSlots);
        Of_Slots = StormFileUtils.ReadStruct<int>(ms, ms.Position);
        // Debug.Log("Of_SLots " + Of_Slots);
        nSubObjects = StormFileUtils.ReadStruct<int>(ms, ms.Position);
        // Debug.Log("nSubObjects " + nSubObjects);
        Of_SubObjects = StormFileUtils.ReadStruct<int>(ms, ms.Position);

        SLOT_DATA[] pSlots = new SLOT_DATA[nSlots];
        //Debug.Log(string.Format("Obj {0} Of_Slots {1}@{2} Of_SubObjects {3}@{4}",Name.ToString("X8"),nSlots,Of_Slots,nSubObjects,Of_SubObjects));
        //for (int i=0;i< nSlots;i++)
        //{
        //    int pos = (int) position + Of_Slots;
        //    ms.Seek(position + Of_Slots, SeekOrigin.Begin);
        //    pSlots[i] = StormFileUtils.ReadStruct<SLOT_DATA>(ms, (int)ms.Position);
        //    Debug.Log("Loaded slot @" + pos + " "+ pSlots[i].OfName);
        //}
        MEO_DATA[] pSubObjects = new MEO_DATA[nSubObjects];
    }

}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class MEO_DATA_HDR
{
    public int OfPlanes; // union { PLANE* Planes;
    public int Of_Slots; //union { SLOT_DATA* _Slots;
    public int OfPlGrps;// union{ short* PlGrps;};
    public int Of_Chars; //union{ char* _Chars;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] MsnBounds;// = new float[4];
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    public MEO_DATA[] MeoData;
    //public MEO_DATA[] MeoData = new MEO_DATA[];
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    public short[] PlGrps;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    public PLANE[] Planes;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    public SLOT_DATA[] _Slots;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    public char[] _Chars;

    public override string ToString()
    {

        return $"OfPlanes {OfPlanes} Of_Slots {Of_Slots} OfPLGrps {OfPlGrps} Of_Chars {Of_Chars} bounds size {MsnBounds.Length} MeoData size {MeoData}";
    }

    //public static MEO_DATA_HDR LoadMEO(string name)
    //{
    //    uint id = Hasher.HshString(name);
    //    return LoadMEO(id);
    //}

    //public static MEO_DATA_HDR LoadMEO(uint id)
    //{
    //    Stream ms = GameDataHolder.GetResource<Stream>(PackType.MEODB, id);
    //    if (ms == null) return null;

    //    return LoadMEO(ms);
    //}

    //int MeoDLdr::MeoDataLoad(void** Data, int Size)
    //{
    //    MEO_DATA_HDR & Mh = *(MEO_DATA_HDR*)(*Data);
    //    if (Size < Mh.Of_Chars)
    //        return DB_LOAD_FAILED;

    //    Mh.Planes = (PLANE*)((char*)(*Data) + Mh.OfPlanes);
    //    Mh._Slots = (SLOT_DATA*)((char*)(*Data) + Mh.Of_Slots);
    //    Mh.PlGrps = (short*)((char*)(*Data) + Mh.OfPlGrps);
    //    Mh._Chars = (char*)((char*)(*Data) + Mh.Of_Chars);

    //    int num_slots = (Mh.OfPlGrps - Mh.Of_Slots) / sizeof(SLOT_DATA);

    //    for (int i = 0; i < num_slots; ++i)
    //        Mh._Slots[i].Name = Mh._Chars + Mh._Slots[i].OfName;

    //    Mh.MeoData->Init(Mh, Mh.MeoData);

    //    return DB_LOAD_OK;
    //}

    public static MEO_DATA_HDR LoadMEO(Stream ms)
    {
        MEO_DATA_HDR Mh = StormFileUtils.ReadStruct<MEO_DATA_HDR>(ms);
        //int pos = Marshal.SizeOf<MEO_DATA_HDR>();
        //ms.Seek(pos, SeekOrigin.Begin);

        List<MEO_DATA> tmpMeoList = new List<MEO_DATA>();
        while (ms.Position < Mh.OfPlanes)
        {
            MEO_DATA tmpData = new MEO_DATA();
            tmpData.Load(ms, ms.Position);
            tmpMeoList.Add(tmpData);
            //Debug.Log("Loading MEO_DATA N " + tmpMeoList.Count + " id " + tmpData.Name.ToString("X8"));
        }
        Mh.MeoData = tmpMeoList.ToArray();

        int planes_num = (Mh.Of_Slots - Mh.OfPlanes) / Marshal.SizeOf<PLANE>();
        Mh.Planes = new PLANE[planes_num];
        ms.Seek(Mh.OfPlanes, SeekOrigin.Begin);
        for (int i = 0; i < planes_num; i++)
        {
            Mh.Planes[i] = StormFileUtils.ReadStruct<PLANE>(ms, ms.Position);
        }
        Debug.Log("LOADED PLANES: " + planes_num);

        int plgrps_num = (Mh.Of_Chars - Mh.OfPlGrps) / sizeof(short);
        Mh.PlGrps = new short[plgrps_num];
        ms.Seek(Mh.OfPlGrps, SeekOrigin.Begin);
        for (int i = 0; i < plgrps_num; i++)
        {
            Mh.PlGrps[i] = StormFileUtils.ReadStruct<short>(ms, ms.Position);
        }
        Debug.Log("LOADED PlGrps: " + plgrps_num);

        ms.Seek(Mh.Of_Slots, SeekOrigin.Begin);
        int num_slots = (Mh.OfPlGrps - Mh.Of_Slots) / Marshal.SizeOf<SLOT_DATA>();
        Mh._Slots = new SLOT_DATA[num_slots];
        for (int i = 0; i < num_slots; i++)
        {
            Mh._Slots[i] = StormFileUtils.ReadStruct<SLOT_DATA>(ms, (int)ms.Position);
        }

        List<char> slotName;
        char currentChar;
        for (int i = 0; i < num_slots; ++i)
        {
            slotName = new List<char>();
            if (Mh._Slots[i] == null)
            {
                Debug.Log(string.Format("Skipping slot {0}", i + 1));
                continue;
            }
            ms.Seek(Mh.Of_Chars + Mh._Slots[i].OfName, SeekOrigin.Begin);
            while (true)
            {
                currentChar = (char)ms.ReadByte();
                if (currentChar == '\0') break;
                slotName.Add(currentChar);
            }
            Mh._Slots[i].NameArray = slotName.ToArray();
        }
        Debug.Log("LOADED Slots: " + num_slots);


        //foreach (MEO_DATA meo in Mh.MeoData)
        //{
        //    meo.pSlots = new SLOT_DATA[meo.nSlots];
        //    for (int i = 0; i < meo.nSlots; i++)
        //    {
        //        int slotOffset = meo.Of_Slots + i;
        //        meo.pSlots[i] = Mh._Slots[slotOffset];
        //        //Debug.Log(string.Format("Object {0} slot {1}",meo.Name.ToString("X8"), new string(Mh._Slots[slotOffset].Name)));
        //    }
        //    meo.pSubObjects = new MEO_DATA[meo.nSubObjects];
        //    for (int i = 0; i < meo.nSubObjects; i++)
        //    {
        //        int subobjId = meo.Of_SubObjects + i;

        //        meo.pSubObjects[i] = Mh.MeoData[subobjId];
        //        //Debug.Log(string.Format("Object {0} subobject {1}", meo.Name.ToString("X8"), Mh.MeoData[subobjId].Name.ToString("X8")));
        //    }
        //}

        //foreach (MEO_DATA meo in Mh.MeoData)
        //{
        //    //Debug.Log(string.Format("Object {0} slots {1} subobj {2}", meo.Name.ToString("X8"), meo.pSlots.Length, meo.pSubObjects.Length));
        //    foreach (SLOT_DATA sld in meo.pSlots)
        //    {
        //        //Debug.Log(string.Format("\tProcessing Slot {0}", new string(sld.Name)));
        //    }

        //    foreach (MEO_DATA subobjMeo in meo.pSubObjects)
        //    {
        //        //Debug.Log(string.Format("\tProcessing Subobj {0}", subobjMeo.Name.ToString("X8")));
        //    }
        //}

        ms.Close();

        Mh.MeoData[0].Init(Mh, Mh.MeoData[0]);
        return Mh;
    }
}




