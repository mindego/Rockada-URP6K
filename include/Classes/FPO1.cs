using System;
using System.Collections.Generic;
using UnityEngine;
using static CollideClass;
using static HashFlags;
using static RoFlags;
using static MeoDefines;
using DWORD = System.UInt32;

public partial class FPO : RO
{
    public MEO_DATA ObjectData;
    public FpoData fdata;
    public IRData[] r_images = new IRData[4];

    public void CreateHiddenObjects(ICreateHidden ich)
    {
        FpoData fd = fdata.GetSubData();
        int num = ObjectData.nSubObjects;
        MEO_DATA[] md = ObjectData.pSubObjects;

        for (int i = 0; i < num; fd = fd.GetNextData(), i++)
            if ((md[i].Flags & ODF_HIDDEN) != 0)
                ich.ProcessHidden(this, new FPO(md[i], fd));
    }
    public bool EnumerateSlots(ISlotEnum e, bool EnumSubObjs = true, MEO_DATA_HDR hdr = null)
    {
        //RO top = Top();

        //if (hdr==null) hdr = ((FPO)top).GetMeoData();

        int i = 0;

        foreach (var z in ObjectData.pSlots)
        {
            e.ProcessSlot(z, i++, this);
        }

        if (SubObjects != null)
        {
            for (var o = SubObjects; o != null; o = o.Next)
            {
                if (o.GetFlag(ROFID_FPO) == 0) continue;
                ((FPO)o).EnumerateSlots(e, true, hdr);
                //if (o.GetFlag(ROFID_FPO)!=0) ((FPO)o).EnumerateSlots(e, true, hdr);
                //((FPO)o).EnumerateSlots(e, true, hdr);
            }
        }
        return true;
    }

    //public bool EnumerateSlots(ISlotEnum e, bool EnumSubObjs = true, MEO_DATA_HDR hdr = null)
    //{
    //    RO top = Top();
    //    Asserts.Assert(top.GetFlag(ROFID_FPO) != 0);
    //    if (hdr == null) hdr = ((FPO)top).GetMeoData();
    //    Asserts.Assert(hdr!=null);

    //    //int start = ObjectData.pSlots - hdr._Slots;
    //    int start = Array.IndexOf(hdr._Slots,ObjectData.pSlots[0]); // Есть подозрения, что не сработает, т.к. это могут быть разнные объекты.
    //    for (int i = 0; i < ObjectData.nSlots; i++)
    //        if (!e.ProcessSlot(ObjectData.pSlots[i], start + i, this))
    //            return false;

    //    if (EnumSubObjs && SubObjects != null)
    //    {
    //        RO p;

    //        for (start = 0, p = SubObjects; p != null; p = p.Next)
    //            if (p.GetFlag(ROFID_FPO) != 0) start++;

    //        if (start != 0)
    //        {
    //            FPO[] sub = Alloca.ANewN<FPO>(start);
    //            for (start = 0, p = SubObjects; p != 0; p = p.Next)
    //                if (p.GetFlag(ROFID_FPO)) sub[start++] = (FPO)p;

    //            for (int i = 0; i < start; ++i)
    //                if (!sub[i].EnumerateSlots(e, true, hdr))
    //                    return false;
    //        }
    //    }
    //    return true;
    //}
    public override void Dispose()
    {
        //Destroy();
    }

    public override void Destroy()
    {
        for (int i = 0; i < 4; ++i)
            IBase.SafeRelease<IRData>(r_images[i]);
    }
    public bool EnumerateSlotsID(ISlotEnum e, int slot_id, MEO_DATA_HDR hdr)
    {
        return true;
    }


    //private MEOData.MEO_DATA_HDR GetMeoData() {
    //    return Parent!=null ? null : ObjectData.;
    //}

    public FPO(MEO_DATA Data, FpoData fd, FPO Par = null) : base(HashFlags.OF_GROUP_COLLIDABLE | RoFlags.ROFID_FPO)
    {
        ObjectData = Data;
        fdata = fd;
        CurrentImage = 0;
        Name = (uint)ObjectData.Name;
        Parent = Par;
        Set(ObjectData.Org, ObjectData.Dir, ObjectData.Up);

        //Debug.Log("FPO Name " + Name.ToString("X8"));

        MaxRadius = SelfRadius = HashRadius = ObjectData.Images[0].Radius;
        SubObjects = null;

        int i;
        for (i = 0; i < 4; ++i) r_images[i] = null;

        if (ObjectData.nSubObjects == GetNumSubObjs(fd))
        {

            fd = fd.GetSubData();

            for (i = 0; i < ObjectData.nSubObjects; i++)
            {

                if ((ObjectData.pSubObjects[i].Flags & MeoDefines.ODF_HIDDEN) == 0)
                {
                    FPO f = new FPO(ObjectData.pSubObjects[i], fd, this);
                    f.Next = SubObjects;
                    if (SubObjects != null) SubObjects.Prev = f;
                    SubObjects = f;
                }
                //Assert(fd);
                if (fd == null) throw new Exception("Out of fdata!");
                fd = fd.GetNextData();
            }
            if (SubObjects != null) SubObjects.Prev = null;
        }
        else
        {
            // Error situation
            Debug.Log("// Error situation");
            Debug.Log(string.Format("ObjectData.nSubObjects {0} GetNumSubObjs(fd) {1} ", ObjectData.nSubObjects, GetNumSubObjs(fd)));
            Debug.Log(string.Format("FD sub {0} next {1}", fd.tree_sub, fd.tree_next));
            Top().SetFlag(HashFlags.OF_GROUP_MASK);
        }

        if (Par == null)
        {
            RecalcRadius();
            Next = Prev = null;
        } 

            //Debug.LogFormat("FPO created Name: {0} MR:{1} SR:{2} HR:{3}", TextName + "#" + Name.ToString("X8"), MaxRadius, SelfRadius, HashRadius);
            Debug.LogFormat("FPO created Name: {0} MR:{1} SR:{2} HR:{3}", GetFullName(), MaxRadius, SelfRadius, HashRadius);
    }

    public string GetFullName()
    {
        string resname= TextName + "#" + Name.ToString("X8");
        if (Parent != null) resname = Parent.TextName + "/" + resname;
        return resname;
    }
    internal int CheckWaterCollision(MATRIX World, TERRAIN_DATA td, ref CollideResult[] res, int startpos = 0)
    {
#warning ("hash optimization here!!")
        MATRIX LocalWorld = new MATRIX(World, this);
        int count = ObjectData.Images[CurrentImage].WaterCollision(LocalWorld, td, ref res);
        for (int i = 0; i < count; i++)
        {
            if ((i + startpos) >= res.Length) return 0; //TODO Не должно этого быть тут. Слишком много столкновений с землёй
            res[i + startpos].coll_cls = COLC_GROUND;
            res[i + startpos].caller_fpo = this;

        }
        for (RO o = SubObjects; o != null; o = o.Next)
        {
            if (o.GetFlag(ROFID_FPO) == 0) continue;
            count += ((FPO)o).CheckWaterCollision(LocalWorld, td, ref res, count + startpos);
        }
        return count;
    }

    internal int CheckGroundCollision(MATRIX World, TERRAIN_DATA td, ref CollideResult[] res, int startPos = 0)
    {
#warning ("hash optimization here!!")
        MATRIX LocalWorld = new MATRIX(World, this);
        int count = ObjectData.Images[CurrentImage].GroundCollision(LocalWorld, td, ref res);
        for (int i = 0; i < count; i++)
        {
            res[i + startPos].coll_cls = COLC_GROUND;
            res[i + startPos].caller_fpo = this;
        }
        for (RO o = SubObjects; o != null; o = o.Next)
        {
            if (o.GetFlag(ROFID_FPO) == 0) continue;
            count += ((FPO)o).CheckGroundCollision(LocalWorld, td, ref res, count + startPos);
        }
        return count;
    }

    public int GetNumSubObjs(FpoData f)
    {
        int i;
        f = f.GetSubData();
        for (i = 0; f != null; f = f.GetNextData()) ++i;
        return i;
    }

    public float MaxX() { return ObjectData.Images[CurrentImage].Max.x; }
    public float MinX() { return ObjectData.Images[CurrentImage].Min.x; }
    public float MaxY() { return ObjectData.Images[CurrentImage].Max.y; }
    public float MinY() { return ObjectData.Images[CurrentImage].Min.y; }
    public float MaxZ() { return ObjectData.Images[CurrentImage].Max.z; }
    public float MinZ() { return ObjectData.Images[CurrentImage].Min.z; }

    public int SetImage(int nImage, int Flags, object Sign, bool RecalcRadius = true)
    {
        Debug.LogFormat("Setting image for {0} IF N {1} != CI {2} OR FLAGS {3} vs {4} !=0 ({5}) RES: {6}",
            this,
            nImage,
            CurrentImage,
            Flags.ToString("X8"), 
            RoFlags.FSI_FORCE.ToString("X8"),
            (Flags & RoFlags.FSI_FORCE).ToString("X8"),
            (!(nImage != CurrentImage || (Flags & RoFlags.FSI_FORCE) != 0))
            );
        if (!(nImage != CurrentImage || (Flags & RoFlags.FSI_FORCE) != 0)) return CurrentImage;
        Debug.LogFormat("Really setting image for {0}, top {1}", this, Top());
        for (RO o = SubObjects; o != null; o = o.Next)
        {
            if ((Flags & RoFlags.FSI_EQUAL_LINKS) != 0) { if (o.Link != Sign) continue; }
            if ((Flags & RoFlags.FSI_NONEQUAL_LINKS) != 0) { if (o.Link == Sign) continue; }
            if (o.GetFlag(RoFlags.ROFID_FPO) != 0) ((FPO)o).SetImage(nImage, Flags, Sign, false);
        }

        SetMainImage(nImage, Flags, Sign);
        if (RecalcRadius) Top().RecalcRadius();
        return CurrentImage;
    }

    //public int SetMainImage(int nImage, int Flags, object Sign) //TODO Вот не нравится мне это. Надо бы убрать Goto
    //{
    //    int i;

    //    if (r_images[nImage] != null) goto SetImage;

    //    if ((Flags & RoFlags.FSI_ROUND_UP) != 0)
    //    {
    //        for (i = nImage - 1; i >= 0; i--)
    //            if (r_images[i] != null) { nImage = i; goto SetImage; }
    //    }
    //    if ((Flags & RoFlags.FSI_ROUND_DOWN) != 0)
    //    {
    //        for (i = nImage + 1; i < 4; i++)
    //            if (r_images[i] != null) { nImage = i; goto SetImage; }
    //    }
    ///*if (!(Flags&FSI_FORCE))
    //  return ObjectData.Images[CurrentImage].Lods[0].nPts;*/
    //SetImage:
    //    if (nImage != CurrentImage || (Flags & RoFlags.FSI_FORCE) != 0)
    //        SelfRadius = ObjectData.Images[CurrentImage = nImage].Radius;
    //    return CurrentImage;
    //}

    public int SetMainImage(int nImage, int Flags, object Sign)
    {
        int i;
        int nImage_bak = nImage;


        if (r_images[nImage] == null)
        {

            if ((Flags & RoFlags.FSI_ROUND_UP) != 0)
            {
                for (i = nImage - 1; i >= 0; i--)
                    if (r_images[i] != null) { nImage = i;break; }
            }
            if ((Flags & RoFlags.FSI_ROUND_DOWN) != 0)
            {
                for (i = nImage + 1; i < 4; i++)
                    if (r_images[i] != null) { nImage = i;break; }
            }
        }
    /*if (!(Flags&FSI_FORCE))
      return ObjectData.Images[CurrentImage].Lods[0].nPts;*/
        if (nImage != CurrentImage || (Flags & RoFlags.FSI_FORCE) != 0)
            SelfRadius = ObjectData.Images[CurrentImage = nImage].Radius;

        if (SelfRadius == 0)
        {
            //TODO Выяснить, почему пустые имаджи
            //Debug.LogErrorFormat("Incorrect SelfRadius for image {0} of {2}. Check: {1}", nImage, ObjectData.Images[nImage], this);
            //Debug.LogErrorFormat("Image update: {0}->{1}, r_images before: {2} r_images after: {3}", nImage_bak, nImage, r_images[nImage_bak] ==null ?"EMPTY": r_images[nImage_bak], r_images[nImage] == null ? "EMPTY" : r_images[nImage]);

        }
        return CurrentImage;
    }

    internal void TraceLine(Geometry.Line line, TraceResult r)
    {
        //Debug.LogFormat("DEBUGTRACE FPO TRACING {0}->{1}",line.org,line.org + line.dir*line.dist);
        Geometry.Line l = new Geometry.Line(line);
        l.org -= Org;
        //checking sphere intersected?
        float r2 = Mathf.Pow(MaxRadius, 2);
        //float t = -(l.dir * l.org);
        float t = -Vector3.Dot(l.dir, l.org);
        if (l.org.sqrMagnitude - Mathf.Pow(t, 2) > r2) return;
        if (t < 0)
        {
            if (l.org.sqrMagnitude > r2) return;
        }
        else
        {
            if (t > l.dist)
                if ((l.org + l.dir * l.dist).sqrMagnitude > r2) return;
        }
        //could be...---------------------------
        //l.org.Set(l.org * Right, l.org * Up, l.org * Dir);
        //l.dir.Set(l.dir * Right, l.dir * Up, l.dir * Dir);
        l.org.Set(Vector3.Dot(l.org, Right), Vector3.Dot(l.org, Up), Vector3.Dot(l.org, Dir));
        l.dir.Set(Vector3.Dot(l.dir, Right), Vector3.Dot(l.dir, Up), Vector3.Dot(l.dir, Dir));

        if (TraceExactLine(l.org, l.dir, ref l.dist, ref r.normal))
        {
            r.cls = CollideClass.COLC_FPO;
            r.coll_object = this;
            r.dist = l.dist;
        }

        for (RO o = SubObjects; o != null; o = o.Next)
        {
            if (o.MatchGroup(OF_GROUP_COLLIDABLE))
            {
                //Debug.LogFormat("DEBUGTRACE SUBFPO TRACING {0}->{1}", l.org, l.org + l.dir * l.dist);
                FPO f = (FPO)o;
                f.TraceLine(l, r);
            }
        }

    }

    private bool TraceExactLine(Vector3 O, Vector3 D, ref float Dist, ref Vector3 n)
    {
        return
          ObjectData.Images[CurrentImage].nBoxes != 0 ?
            ObjectData.Images[CurrentImage].TraceLine(O, D, ref Dist, ref n) : false;
    }

    public Vector3 ImageCenter()
    {
        IMAGE Im = ObjectData.Images[CurrentImage];
        return (Im.Min + Im.Max) * 0.5f;
    }

}

public partial class FPO //FpoWithPFO
{
    public int Collide(FPO Obj, MATRIX ObjToParent, ref CollideResult[] res, int startpos = 0)
    {
        float d2 = (Org - ObjToParent.Org).sqrMagnitude;
        // пересечение двух деревьев
        if (d2 > Mathf.Pow(MaxRadius + Obj.MaxRadius, 2)) return 0;
        // пересечение меня с его деревом
        int count = 0;
        if (d2 < Mathf.Pow(SelfRadius + Obj.MaxRadius, 2)) count = CollideSelf(d2, Obj, ObjToParent, ref res, startpos);
        // пересечение моего поддерева с ним
        MATRIX ObjToLocal = new MATRIX();
        ObjToLocal.Inherit(ObjToParent, this);
        for (RO o = SubObjects; o != null; o = o.Next)
        {
            if (o.GetFlag(ROFID_FPO) == 0) continue;
            count += ((FPO)o).Collide(Obj, ObjToLocal, ref res, count + startpos);
        }
        // возвращаем кол-во столкновений
        return count;
    }

    int CollideSelf(float d2, FPO Obj, MATRIX ObjToParent, ref CollideResult[] res, int startpos = 0)
    {
        // проверяем на столкновение его объекта со своим объектом
        int count = 0;
        if (d2 < Mathf.Pow(SelfRadius + Obj.SelfRadius, 2))
        {
            if (CollideImages(this.ObjectData.Images[CurrentImage], this, Obj.ObjectData.Images[Obj.CurrentImage], ObjToParent, ref res[startpos].org, ref res[startpos].normal) == true)
            {
                // заполняем структуру
                res[startpos].coll_cls = COLC_FPO;
                res[startpos].caller_fpo = this;
                res[startpos].collided_fpo = Obj;
                if (Parent != null) res[startpos].org = Parent.ToWorldPoint(res[startpos].org);
                if (Parent != null) res[startpos].normal = Parent.ToWorldVector(res[startpos].normal);
                DrawDebugSphere(res[startpos].org, true);
                count++;
            }
        }
        // проверяем на столкновение его подобъектов со своим объектом
        for (RO o = Obj.SubObjects; o != null; o = o.Next)
        {
            if (o.GetFlag(ROFID_FPO) == 0) continue;
            MATRIX SubobjToParent = new MATRIX();
            SubobjToParent.Expand(ObjToParent, o);
            d2 = (Org - SubobjToParent.Org).sqrMagnitude;
            if (d2 < Mathf.Pow(SelfRadius + o.MaxRadius, 2))
                count += CollideSelf(d2, (FPO)o, SubobjToParent, ref res, count + startpos);
        }
        // возвращаем кол-во столкновений
        return count;
    }

    static Material Red, Green;

    public static void DrawDebugSphere(Vector3 org, bool red)
    {
        if (Red == null)
        {
            Red = new Material(MaterialStorage.DefaultSolid);
            Red.color = Color.red;
        }
        if (Green == null)
        {
            Green = new Material(MaterialStorage.DefaultSolid);
            Green.color = Color.green;
        }
        EngineDebug.DebugSphere(org, "Collision point " + org, 1, 5f, red ? Red : Green);
    }
    static bool CollideImages(IMAGE First, MATRIX FirstLocal,
                          IMAGE Second, MATRIX SecondLocal,
                          ref Vector3 org, ref Vector3 norm)
    {
        if (First.nBoxes == 0 || Second.nBoxes == 0) return false;
        //PLANE[] pf = First.Planes;
        List<PLANE> pfPlanesList = new List<PLANE>(First.Planes);
        short[] nf = First.PlGrps;

        int pfIndex = 0, nfIndex = 0;

        for (int nbf = First.nBoxes; nbf != 0; nfIndex++, nbf--)
        {
            //PLANE[] ps = Second.Planes;
            List<PLANE> psPlanesList = new List<PLANE>(Second.Planes);
            short[] ns = Second.PlGrps;
            int psIndex = 0, nsIndex = 0;
            for (int nbs = Second.nBoxes; nbs != 0; nsIndex++, nbs--)
            {

                if (pfPlanesList[pfIndex].d > psPlanesList[psIndex].d)
                {
                    if (BoxSphereCollision(FirstLocal.ExpressPoint(SecondLocal.ProjectPoint(psPlanesList[psIndex].n)), psPlanesList[psIndex].d, nf[nfIndex], pfPlanesList.ToArray(), ref org, ref norm))
                    {
                        org = FirstLocal.ProjectPoint(org);
                        Debug.Log("Collision @F " + org);
                        norm = FirstLocal.ProjectVector(norm);
                        return true;
                    }
                }
                else
                {
                    if (BoxSphereCollision(SecondLocal.ExpressPoint(FirstLocal.ProjectPoint(pfPlanesList[pfIndex].n)), pfPlanesList[pfIndex].d, ns[nsIndex], psPlanesList.ToArray(), ref org, ref norm))
                    {
                        org = SecondLocal.ProjectPoint(org);
                        Debug.Log("Collision @S " + org);
                        norm = -SecondLocal.ProjectVector(norm);
                        return true;
                    }
                }
                psPlanesList.RemoveRange(0, ns[nsIndex]);
                //tmpPlanesList = new System.Collections.Generic.List<PLANE>(ps);
                //tmpPlanesList.RemoveRange(0, ns[nfIndex]);
                //ps = tmpPlanesList.ToArray();
            }
            pfPlanesList.RemoveRange(0, nf[nfIndex]);
            //tmpPlanesList = new System.Collections.Generic.List<PLANE>(pf);
            //tmpPlanesList.RemoveRange(0, nf[nfIndex]);
            //pf = tmpPlanesList.ToArray();

            //int nfIndexValue = nf[nfIndex];
            //Debug.LogFormat("pf.Length {0} nf {1}", pf.Length, nf[nfIndex]);
            //newLen = pf.Length - nf[nfIndex];
            //tmp = new PLANE[newLen];
            //Array.Copy(pf, nf[nfIndex], tmp, 0, newLen);
            //pf = tmp;
        }
        return false;
    }

    public int CheckSphereCollision(Vector3 O, float r, CollideResult[] res, int resIndex = 0)
    {
        int count = 0;
        O -= Org;
        float d = O.magnitude;
        if (d >= MaxRadius + r) return count;
        O.Set(Vector3.Dot(O, Right), Vector3.Dot(O, Up), Vector3.Dot(O, Dir));
#warning more precise method commented!"
        /*
          if (CheckExactSphereCollision(O,r)) {
        */
        if (d < SelfRadius + r)
        {
            res[resIndex].coll_cls = COLC_FPO;
            res[resIndex].caller_fpo = this;
            res[resIndex].collided_fpo = null;
            count++;
        }
        for (RO o = SubObjects; o != null; o = o.Next)
        {
            if (o.GetFlag(ROFID_FPO) == 0) continue;
            count += ((FPO)o).CheckSphereCollision(O, r, res, resIndex + count);
        }
        return count;
    }

    static bool BoxSphereCollision(Vector3 O, float R, int nPlanes, PLANE[] Planes, ref Vector3 org, ref Vector3 norm)
    {
        int curPlaneIndex = 0;
        if ((O - Planes[curPlaneIndex].n).sqrMagnitude > Mathf.Pow(R + Planes[curPlaneIndex].d, 2)) return false;
        PLANE bp = null;
        float bd = float.MaxValue;
        for (curPlaneIndex++; nPlanes != 0; curPlaneIndex++, --nPlanes)
        {
            float d = Vector3.Dot(Planes[curPlaneIndex].n, O) + Planes[curPlaneIndex].d;
            if (d > R) return false;
            // ищем плоскость с наибольшим d
            if (bp == null || bd < d)
            {
                bp = Planes[curPlaneIndex];
                bd = d;
            }
        }
        // считаем "точку столкновения"
        org = O - bp.n * bd;
        norm = bp.n;

        Debug.Log("Collision in " + org);
        return true;
    }
}
