using geombase;
using Geometry;
using System.Collections.Generic;
using UnityEngine;
using DWORD = System.UInt32;

public class GlobalHasher : ProtoHash, IHash
{
    static string HelloWorld = "Hasher : v.{0}.{1}  build ";
    static string HashInfo = "Info   : dim {0}x{1}x{2}";
    static int mHashCount = 1;

    //Pool<HMember>* pool;
    //HashTreeHead<HMember>** data;
    HashTreeHead<HMember>[] data;
    int upload;
    Geometry.Line mLine;
    DWORD mHashID;
    DWORD mSecondMask;

    //TODO DEBUG!
    public LinkedList<HMember> myHash = new LinkedList<HMember>();


    public GlobalHasher(int _dim, float _sq_size, int poolsize, ILog _log) { Debug.LogError("Incorrect call. Me new " + GetType().ToString() + " " + this.GetHashCode().ToString("X8")); }
    public GlobalHasher(TERRAIN_DATA terrain, float hash_size, float _sq_size, int poolsize, ILog _log)
    {
        //Debug.Log(string.Format("Me new {0} hash_size {1} sq_size {2} pool {3} hashcode {4}", GetType().ToString(), hash_size, _sq_size, poolsize, GetHashCode().ToString("X8")));

        float terrain_size = hash_size;
        if (terrain != null)
        {
            //terrain_size = terrain.GetXSize();
            //if (terrain.GetZSize() > terrain_size)
            //    terrain_size = terrain.GetZSize();
            terrain_size = Mathf.Max(terrain.GetXSize(), terrain.GetZSize());
        }
        DWORD dim1 = (uint)CountDim(terrain_size, _sq_size);
        DWORD dim2 = (uint)CountDim(hash_size, _sq_size);
        //Debug.Log(string.Format("dim1 {0} dim2 {1} terrain_size {2} hash_size {3}",dim1,dim2,terrain_size,hash_size));
        if (Mathf.Abs(dim1 - dim2) % 2 == 1)
            dim2++;
        InitProtoHash((int)dim2, _sq_size, (int)(dim2 - dim1) / 2);

        //pool = new Pool<HMember>(poolsize);

        data = new HashTreeHead<HMember>[mDim * mDim]; //As HashTreeMember
        //data = new Cell[mDim * mDim]; // as List
        upload = 0;

        mHashID = (uint)mHashCount++;

        mSecondMask = 0;

        Clear();
        if (_log != null)
        {
            _log.Message(HelloWorld, (IHashApi.HASH_VERSION >> 16).ToString(), (IHashApi.HASH_VERSION & 0x0000FFFF).ToString());
            _log.Message(HashInfo, mDim.ToString(), mDim.ToString(), mHashID.ToString());
        }

    }

    void Clear()
    {
        //pool.Initialize();
        int sz = mDim * mDim;
        for (int i = 0; i < sz; ++i) data[i] = null;
        upload = 0;
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int EnumLine(Geometry.Line _line, uint _flags, HashLineEnumer e)
    {
        //Debug.LogFormat("DEBUGTRACE EnumLine {0}",_line);
        if (enum_layer >= MAX_HENUM_VALUE - 1) return 0;
        //LayerInc linc = new LayerInc(ref enum_layer);
        enum_layer++;
        int cmp_sign = ++enumc[enum_layer];
        mLine = _line;
        e.SetLineData(mLine);
        var res = TraceLine(_flags, enum_layer, cmp_sign, ref mLine, e);
        //linc.Dispose();
        enum_layer--;
        return res;
    }

    int TraceLineIn(uint _flags, int layer, int cmp_sign, ref Geometry.Line _line, HashEnumer e)
    {
        ITERATION2D Iterator = new ITERATION2D(
          _line.org.x, _line.org.z,
          _line.dir.x, _line.dir.z, square_size, oo_square_size);

        float BoxDist;
        int x = GetRealX(Iterator.xIter.Box);
        int y = GetRealY(Iterator.yIter.Box);

        //List<HMember> plist = new List<HMember>();
        while ((BoxDist = Iterator.GetPosition()) < _line.dist)
        {
            //#pragma message ("     EEI: SLOW CODE (relative)!!!!!!!! Repair ASAP !!!!")
            if (rect.In(x, y))
            {
                //List<HMember> plist = GetHMemberInCell(x, y);
                HashTreeHead<HMember> plist = data[GetAddress(x, y)];
                //HashTreeHead<HMember>** plist = GetAddress(x, y);
                LayerEnumerate(true, plist, _flags, e, layer, cmp_sign);
                data[GetAddress(x, y)] = plist;
            }
            if (_line.dist < BoxDist) return 1;
            Iterator.Next();
            x = GetRealX(Iterator.xIter.Box);
            y = GetRealY(Iterator.yIter.Box);
        }
        if (rect.In(x, y))
        {
            //HashTreeHead<HMember>** plist = GetAddress(x, y);
            HashTreeHead<HMember> plist = data[GetAddress(x, y)];
            //List<HMember> plist = GetHMemberInCell(x, y);
            LayerEnumerate(true, plist, _flags, e, layer, cmp_sign);
            data[GetAddress(x, y)] = plist;
        }
        return 1;
    }

    const float PRECISION = float.Epsilon; //FLT_EPSILON
    bool ClipLine(Vector3 O, Vector3 D, ref float[] Clipped)
    {
        if (!new PLANE(Vector3.left, GetBaseX() + PRECISION).ClipVector(O, D, ref Clipped)) return false;
        if (!new PLANE(Vector3.back, GetBaseY() + PRECISION).ClipVector(O, D, ref Clipped)) return false;

        if (!new PLANE(Vector3.right, -(GetBaseX2() - PRECISION)).ClipVector(O, D, ref Clipped)) return false;
        if (!new PLANE(Vector3.forward, -(GetBaseY2() - PRECISION)).ClipVector(O, D, ref Clipped)) return false;
        return true;
    }
    void SetClip(ref Geometry.Line l, float s, float e)
    {
        l.org += l.dir * s; l.dist = e - s;
    }
    int TraceLine(uint _flags, int layer, int cmp_sign, ref Geometry.Line _line, HashEnumer e)
    {
        int ret;
        float[] Clipped = { 0, _line.dist }; //размер [2]

        if (ClipLine(_line.org, _line.dir, ref Clipped))
        {
            SetClip(ref _line, Clipped[0], Clipped[1]);
            ret = TraceLineIn(_flags, layer, cmp_sign, ref _line, e);
            _line.dist += Clipped[0];
        }
        else ret = 0;
        return ret;
    }

    //private int LayerEnumerate(bool first, HMember hm, int Flag, HashEnumer e, int layer, int cmp_sign) {
    //    return (hm !=null) ? hm.LayerEnumerate(first, Flag, e, layer, cmp_sign) :null;
    //}

    public int EnumPoly(RasterizeData r, uint _flags, HashEnumer e)
    {
        //Debug.Log("Enumerating at " + r.starty + " from " + myHash.Count + " " + this.GetHashCode().ToString("X8"));
        //Debug.Log("Raster r " + r);
        //Debug.Log(r);
        //if (enum_layer >= MAX_HENUM_VALUE - 1) return 0;
        //LayerInc linc(enum_layer);

        //int count = 0;
        ////int cmp_sign = ++enumc[enum_layer];
        //int cmp_sign = 0;
        //int lb = 0;
        //int rb = 0;

        //int endy = r.starty + r.nlines;
        //bool first_cache = IsInFirstCache(_flags);
        //List<HMember> plist = new List<HMember>();
        //for (int y = r.starty; y < endy; ++y)
        //{
        //    plist.Clear();
        //    int x = r.left[lb], x2 = r.right[rb++];
        //    int cellIndex = GetAddress(x, y);

        //    for (; x <= x2; ++x)
        //    {
        //        if (data[cellIndex] == null) continue;
        //        //Assert(rect.In(x, y));
        //        foreach (HMember h in data[cellIndex].GetHMembers())
        //        {
        //            if (h == null) continue;
        //            if (!h.Object().MatchFlags(_flags))
        //            {
        //                //Debug.Log(string.Format("Probing in rect fl {0} ex {1} {2} {3}", h.Object().GetFlags().ToString("X8"), _flags.ToString("X8"), h.Object().MatchFlags(_flags),e));
        //                continue;
        //            }
        //            //e.ProcessElement(h);
        //            plist.Add(h);
        //            //count++;
        //        }
        //        //count += LayerEnumerate(first_cache, plist, _flags, e, enum_layer, cmp_sign);
        //    }
        //    count += LayerEnumerate(first_cache, plist, _flags, e, enum_layer, cmp_sign);
        //}
        //return count;
        return EnumRect(new geombase.Rect(0, 0, mDim - 1, mDim - 1), _flags, e);
    }

    public geombase.Rect GetRect(geombase.Rect myRect)
    {
        return myRect & rect;
    }
    //public int EnumRect(geombase.Rect _rt, uint _flags, HashEnumer e)
    //{

    //    if (enum_layer >= MAX_HENUM_VALUE - 1) return 0;
    //    //LayerInc linc(enum_layer);
    //    //int cmp_sign = ++enumc[enum_layer];
    //    int cmp_sign = 0;
    //    int count = 0;
    //    geombase.Rect r = _rt & rect;
    //    bool first_cache = IsInFirstCache(_flags);

    //    //Debug.Log(string.Format("Probing rect: {0}, main rect: {1} my rect: {2}",r,rect,_rt));
    //    for (int y = r.y0; y <= r.y1; y++)
    //    {
    //        //HashTreeHead<HMember>** plist = GetAddress(r.x0, y);
    //        List<HMember> plist = new List<HMember>();
    //        for (int x = r.x0; x <= r.x1; x++)
    //        {
    //            int cellIndex = GetAddress(x, y);
    //            if (data[cellIndex] == null) continue;
    //            foreach (HMember h in data[cellIndex].GetHMembers())
    //            {
    //                if (h == null) continue;
    //                if (!h.Object().MatchFlags(_flags))
    //                {
    //                    continue;
    //                }
    //                plist.Add(h);
    //            }
    //            //count += LayerEnumerate(first_cache, plist, _flags, e, enum_layer, cmp_sign);
    //            count += LayerEnumerate(first_cache, plist, _flags, e, enum_layer, cmp_sign);
    //        }

    //    }
    //    //Debug.Log("Hashed items: " + count);

    //    return count;
    //}

    public int EnumRect(geombase.Rect p, uint _flags, HashEnumer e)
    {
        //if ((_flags & RoFlags.ROFID_DECAL) != 0) Debug.Log("Enum for decals " + _flags.ToString("X8") + " rect " + p );
        if (enum_layer >= MAX_HENUM_VALUE - 1) return 0;
        //LayerInc linc(enum_layer);
        enum_layer++;
        int cmp_sign = ++enumc[enum_layer];
        int count = 0;
        geombase.Rect r = p & rect;
        bool first_cache = IsInFirstCache(_flags);

        for (int y = r.y0; y <= r.y1; y++)
        {
            for (int x = r.x0; x <= r.x1; x++)
            {
                HashTreeHead<HMember> plist = data[GetAddress(x, y)];
                count += LayerEnumerate(first_cache, plist, _flags, e, enum_layer, cmp_sign);
            }
        }
        enum_layer--;
        return count;
    }

    private int LayerEnumerate(bool first, HashTreeHead<HMember> hm, uint Flag, HashEnumer e, int layer, int cmp_sign)
    {
        //if ((Flag & RoFlags.ROFID_DECAL) != 0) Debug.Log("Enum for decals " + Flag.ToString("X8") + " hm " + (hm!=null? hm:"null"));
        return (hm != null) ? hm.LayerEnumerate(first, Flag, e, layer, cmp_sign) : 0;
    }

    //int LayerEnumerate(bool first, List<HMember> hm, uint Flag, HashEnumer e, int layer, int cmp_Sign)
    //{
    //    //Debug.Log("Enumerating HMembers: " +  hm.Count);
    //    if (hm == null) return 0;
    //    int count = 0;
    //    for (int i = 0; i < hm.Count; i++)
    //    {
    //        HMember m = hm[i];
    //        e.ProcessElement(m);
    //        count++;
    //    }
    //    return count;
    //}

    //int LayerEnumerate(bool first, List<HMember> hm, uint Flag, HashEnumer e, int layer, int cmp_sign)
    //{
    //    return (hm != null) ? hm.LayerEnumerate(first, Flag, e, layer, cmp_sign) : 0;
    //}
    //int LayerEnumerate(bool first, HashTreeHead<HMember>** hm, int Flag, HashEnumer* e, int layer, int cmp_sign)  {
    //    return (hm !=null)? hm.LayerEnumerate(first, Flag, e, layer, cmp_sign) :0;
    //}
    //    int LayerEnumerate(bool first, int Flag, HashEnumer e, int layer, int cmp_sign)  {
    //    return first? mFirstCache.LayerEnumerate(Flag, e, layer, cmp_sign):mSecondCache.LayerEnumerate(Flag, e, layer, cmp_sign);
    //}

    //int LayerEnumerate(int Flag, HashEnumer e, int layer, int cmp_sign)  {
    //    int count = 0;
    //    HPelem* h = head;
    //    for (;h;) {
    //        HMember dt = h.data;
    //        if (dt->Object() && dt->MatchOnLayer(Flag, layer, cmp_sign)) {
    //            dt->SetLayer(layer, cmp_sign); count++;
    //            if (!e->ProcessElement(dt)) return count;
    //        }
    //h = h->next;
    //    }
    //    return count;
    //}

    public int EnumSphere(Sphere sp, uint flags, HashEnumer e)
    {
        geombase.Rect p = GetRect(sp);
        return EnumRect(p, flags, e);
    }

    public IHashInfo GetHashInfo()
    {
        IHashInfo info;
        int sz = mDim * mDim;
        info.mCapacity = (uint)sz;
        info.mMaxUploadInFirst = 0;
        info.mMaxUploadInSecond = 0;
        info.mUploadInFirst = 0;
        info.mUploadInSecond = 0;
        for (int i = 0; i < sz; ++i)
        {
            if (data[i] != null)
            {
                int n1 = (int)data[i].Upload(true);
                int n2 = (int)data[i].Upload(false);
                if (n1 != 0)
                {
                    info.mUploadInFirst += (uint)n1;
                    if (n1 > info.mMaxUploadInFirst)
                        info.mMaxUploadInFirst = (uint)n1;
                }
                if (n2 != 0)
                {
                    info.mUploadInSecond += (uint)n2;
                    if (n2 > info.mMaxUploadInSecond)
                        info.mMaxUploadInSecond = (uint)n2;
                }
            }
        }
        return info;

    }

    public object Query(uint id)
    {
        switch (id)
        {
            case ProtoHash.ID:
                return GetProtoHash();
            default:
                return null;
        }

    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }

    public HMember RemoveMember(HMember m)
    {
        geombase.Rect newpos = m.GetRect() & rect;
        if (newpos.Valid())
        {
            ProcessRect(true, m, newpos, newpos, SubHashOp.HOP_REM);
        }
        m.Invalidate();
        return m;

    }

    public HMember RemoveMemberByLine(HMember Info)
    {
        throw new System.NotImplementedException();
    }

    public HMember RemoveMemberStatic(HMember Info)
    {
        throw new System.NotImplementedException();
    }

    public void SetSecondCache(uint mask)
    {
        if (0 == mSecondMask)
            mSecondMask = mask;
    }

    public HMember UpdateMember(HMember m)
    {

        geombase.Rect newrect = GetRect(m.Object().GetBoundingSphere());
        geombase.Rect oldrect = m.GetRect() & rect;
        geombase.Rect newpos = newrect & rect;

        if (!oldrect.Valid())
        {   // if old position not in hash
            //Debug.Log("old position not in hash");
            if (newpos.Valid())
            {   // and new position in hash 
                //Debug.Log("new position in hash ");
                bool first_cache = IsInFirstCache(m.Object().GetFlags());
                ProcessRect(first_cache, m, newpos, oldrect, SubHashOp.HOP_ADD);
            }
        }
        else
        {                     // if old position in hash
            if (!newpos.Valid())
            {     // and new postion not in hash
                RemoveMember(m);      // just remove
            }
            else
            {
                if (newpos != oldrect)
                {    // if position moved
                    bool first_cache = IsInFirstCache(m.Object().GetFlags());
                    geombase.Rect cross = oldrect & newpos;
                    ProcessRect(first_cache, m, oldrect, cross, SubHashOp.HOP_REM);  // remove from old not in cross
                    ProcessRect(first_cache, m, newpos, cross, SubHashOp.HOP_ADD);  // add to new and not in cross
                }
            }
        }
        m.SetRect(newrect);

        return m;

    }
    //public HMember UpdateMemberEchelon(HMember m)
    //{

    //    geombase.Rect newrect = GetRect(m.Object().GetBoundingSphere());
    //    geombase.Rect oldrect = m.GetRect() & rect;
    //    geombase.Rect newpos = newrect & rect;

    //    if (!oldrect.Valid())
    //    {   // if old position not in hash
    //        //Debug.Log("old position not in hash");
    //        if (newpos.Valid())
    //        {   // and new position in hash 
    //            //Debug.Log("new position in hash ");
    //            bool first_cache = IsInFirstCache(m.Object().GetFlags());
    //            ProcessRect(first_cache, m, newpos, oldrect, SubHashOp.HOP_ADD);
    //        }
    //    }
    //    else
    //    {                     // if old position in hash
    //        //Debug.Log("old position in hash");
    //        if (!newpos.Valid())
    //        {     // and new postion not in hash
    //            //Debug.Log("new postion not in hash");
    //            RemoveMember(m);      // just remove
    //        }
    //        else
    //        {
    //            if (newpos != oldrect)
    //            {    // if position moved
    //                //Debug.Log("position moved");
    //                bool first_cache = IsInFirstCache(m.Object().GetFlags());
    //                geombase.Rect cross = oldrect & newpos;
    //                ProcessRect(first_cache, m, oldrect, cross, SubHashOp.HOP_REM);  // remove from old not in cross
    //                ProcessRect(first_cache, m, newpos, cross, SubHashOp.HOP_ADD);  // add to new and not in cross
    //            }
    //        }
    //    }
    //    m.SetRect(newrect);
    //    //Debug.Log(newrect);



    //    return m;
    //    //myHash.Update(Info);
    //    //return Info;
    //}

    //void ProcessRect(bool first_cache, HMember m, geombase.Rect include, geombase.Rect exclude, SubHashOp op)
    //{
    //    int cellIndex;
    //    for (int y = include.y0; y <= include.y1; ++y)
    //    {
    //        for (int x = include.x0; x <= include.x1; ++x)
    //        {
    //            cellIndex = y * mDim + x;
    //            //Debug.Log("Cell index: " + cellIndex);
    //            if (data[cellIndex] == null) data[cellIndex] = new Cell(cellIndex);
    //            switch (op)
    //            {
    //                case SubHashOp.HOP_ADD:
    //                    data[cellIndex].Add(m);
    //                    break;
    //                case SubHashOp.HOP_REM:
    //                    data[cellIndex].Remove(m);
    //                    break;
    //                default:
    //                    break;
    //            }
    //            ;
    //        }
    //    }

    //    ////TODO Реализовать как нужно! отрисовку в секторе 
    //    //for (int y = include.y0; y <= include.y1; ++y)
    //    //{
    //    //    HashTreeHead<HMember>** plist = GetAddress(include.x0, y);
    //    //    for (int x = include.x0; x <= include.x1; ++plist, ++x)
    //    //        if (!exclude.In(x, y))
    //    //        {
    //    //            switch (op)
    //    //            {
    //    //                case SubHashOp.HOP_ADD:
    //    //                    if (!Add(first_cache, plist, m, pool))
    //    //                    {
    //    //                        hlog.Message("Error : no more place in pool !");
    //    //                    }
    //    //                    else
    //    //                        upload++;
    //    //                    break;
    //    //                case SubHashOp.HOP_REM:
    //    //                    upload -= Remove(first_cache, plist, m, pool);
    //    //                    break;
    //    //            }
    //    //        }
    //    //}
    //}

    void ProcessRect(bool first_cache, HMember m, geombase.Rect include, geombase.Rect exclude, SubHashOp op)
    {
        int cellIndex;
        for (int y = include.y0; y <= include.y1; ++y)
        {
            for (int x = include.x0; x <= include.x1; ++x)
            {
                if (!exclude.In(x, y))
                {
                    cellIndex = y * mDim + x;
                    //Debug.Log("Cell index: " + cellIndex);
                    //if (data[cellIndex] == null) data[cellIndex] = new();
                    HashTreeHead<HMember> plist = data[cellIndex];
                    switch (op)
                    {
                        case SubHashOp.HOP_ADD:
                            if (!Add(first_cache, ref plist, m))
                            {
                                hashtools_dll.hlog.Message("Error : no more place in pool !");
                            }
                            else
                                upload++;
                            break;
                        case SubHashOp.HOP_REM:
                            upload -= Remove(first_cache, ref plist, m);
                            break;
                        default:
                            break;
                    }
                    data[cellIndex] = plist;
                }
            }
        }
    }

    private bool Add(bool first, ref HashTreeHead<HMember> hm, HMember m) //TODO Возможно, стоит получать из пула?
    {
        if (null == hm)
            hm = new HashTreeHead<HMember>();
        return hm.Add(first, m);
    }

    private int Remove(bool first, ref HashTreeHead<HMember> hm, HMember m)
    {
        int ret = 0;
        if (null != hm)
        {
            ret = hm.Remove(first, m);
            if (hm.IsClear())
            {
                hm = null;
            }
        }
        return ret;
    }
    private bool IsInFirstCache(DWORD flags)
    {
        bool ret = true;
        if (0 != mSecondMask)
        {
            if (MatchGroup(flags) == MatchGroup(mSecondMask))
                if (MatchUser(flags) == MatchUser(mSecondMask))
                    ret = false;
        }
        return ret;
    }

    private DWORD MatchGroup(DWORD flags) { return flags & HashFlags.OF_GROUP_MASK; }
    private DWORD MatchUser(DWORD flags) { return flags & HashFlags.OF_USER_MASK; }


    private int GetIndex(float x, float y)
    {
        return Mathf.FloorToInt((y / mDim) * mDim + x / mDim);
    }

    public HMember UpdateMemberByLine(HMember m)
    {
        Line line = m.Object().GetLinearData();
        Debug.Log("Line: " + (line  == null? "null":line)+ " for " + m.Object());
        ITERATION2D Iterator = new ITERATION2D(
          line.org.x,
          line.org.z,
          line.dir.x,
          line.dir.z,
          GetSquareSize(),
          GetOOSquareSize()

          );
        int i = 0;
        int x = GetRealX(Iterator.xIter.Box);
        int y = GetRealY(Iterator.yIter.Box);
        AddToHash(x, y, m);
        while (Iterator.GetPosition() < line.dist)
        {
            AddToHash(x, y, m);
            i++;
            Iterator.Next();
            x = GetRealX(Iterator.xIter.Box);
            y = GetRealY(Iterator.yIter.Box);
        }
        if (i!=0)
            AddToHash(x, y, m);
        return m;

    }

    public HMember UpdateMemberStatic(HMember m)
    {
        return processMemberStatic(m, true);
    }

    void processMember(float x, float z, HMember m, bool add)
    {
        int ix = GetHashX(x);
        int iz = GetHashY(z);
        if (add)
        {
            AddToHash(ix, iz, m);
        }
        else
        {
            RemoveFromHash(ix, iz, m);
        }

    }

    private void AddToHash(int x, int y, HMember m)
    {
        if (rect.In(x, y))
        {
            //HashTreeHead<HMember>** plist = GetAddress(x, y);
            //if (Add(true, plist, m, pool))
            //    upload++;
            //Debug.Log(string.Format("Adding to Hash {0} [{1}:{2}]", m.Object(),x,y));

            //As List
            //int cellIndex = GetAddress(x, y);
            //if (data[cellIndex] == null) data[cellIndex] = new Cell(cellIndex);

            //data[cellIndex].Update(m);
            HashTreeHead<HMember> plist = data[GetAddress(x, y)];
            if (Add(true, ref plist, m))
            {
                upload++;
                data[GetAddress(x, y)] = plist;
            }
        }
    }

    private void RemoveFromHash(int x, int y, HMember m)
    {
        if (rect.In(x, y))
        {
            //as list
            //int cellIndex = GetAddress(x, y);
            //if (data[cellIndex] == null) data[cellIndex] = new Cell(cellIndex);

            //Debug.Log(string.Format("Removing from Hash {0} [{1}:{2}]", m.Object(), x, y));
            //data[cellIndex].Remove(m);

            //as HTM
            HashTreeHead<HMember> plist = data[GetAddress(x, y)];
            upload -= Remove(true, ref plist, m);
            data[GetAddress(x, y)] = plist;

        }
    }

    //private List<HMember> GetHMemberInCell(int x, int y)
    //{
    //    int cellIndex = GetAddress(x, y);
    //    if (data[cellIndex] == null) return new List<HMember>();

    //    return data[cellIndex].GetHMembers();
    //}

    private int GetAddress(int x, int y)
    {
        return y * mDim + x;
    }

    HMember processMemberStatic(HMember m, bool add)
    {
        Line sp = m.Object().GetLinearData();
        Vector3 end = sp.org + sp.dir * sp.dist;
        processMember(sp.org.x, sp.org.z, m, add);
        processMember(end.x, end.z, m, add);
        return m;
    }


    ProtoHash GetProtoHash() { return this; }
}

public class Cell
{
    private List<HMember> members = new List<HMember>();
    private int index;

    public Cell(int index)
    {
        this.index = index;
    }
    public bool Add(HMember h)
    {
        //Debug.Log(string.Format("Cell {0} added member {1} {2} total {3}", index, h.Object(),h.Object().GetHashCode().ToString("X8"), members.Count));
        members.Add(h);
        return true;
    }

    public bool Remove(HMember h)
    {
        //Debug.Log(string.Format("Cell {0} removed member {1} {2} total {3}", index, h.Object(),h.Object().GetHashCode().ToString("X8"), members.Count));
        return members.Remove(h);

    }

    public bool Update(HMember h)
    {
        Remove(h);
        Add(h);
        return true;
    }

    public List<HMember> GetHMembers()
    {
        return members;
    }

    public int Upload(bool firstCache)
    {
        return members.Count;
    }

}

enum SubHashOp
{
    HOP_ADD,
    HOP_REM
};

//class HashTreeHead<T>
//{
//    List<T> mFirstCache;
//    List<T> mSecondCache;
//    void Clear() { mFirstCache.Clear(); mSecondCache.Clear(); }
//    //typedef Hlist<T>::HPelem HPelem;
//    public HashTreeHead() { Clear(); }
//    int Remove(bool first, T hm, List<T> pool)
//    {
//        return first ? mFirstCache.Remove(hm, pool) : mSecondCache.Remove(hm, pool);
//    }
//    HPelem* Add(bool first, T hm, Pool<T>* p)
//    {
//        return first ? mFirstCache.Add(hm, p) : mSecondCache.Add(hm, p);
//    }

//    int LayerEnumerate(bool first, int Flag, HashEnumer e, int layer, int cmp_sign)
//    {
//        return first ? mFirstCache.LayerEnumerate(Flag, e, layer, cmp_sign) : mSecondCache.LayerEnumerate(Flag, e, layer, cmp_sign);
//    }
//    bool IsClear() { return mFirstCache.IsClear() && mSecondCache.IsClear(); }
//    bool IsFirstClear() { return mFirstCache.IsClear(); }
//    bool IsSecondClear() { return mSecondCache.IsClear(); }
//    DWORD Upload(bool first) { return first ? mFirstCache.Upload() : mSecondCache.Upload(); }
//    ~HashTreeHead() { Clear(); }
//};


//class HashTreeHead<T>
//{
//    Hlist<T> mFirstCache;
//    Hlist<T> mSecondCache;
//    void Clear() { mFirstCache.Clear(); mSecondCache.Clear(); }
//    public:
//    typedef Hlist<T>::HPelem HPelem;
//    HashTreeHead() { Clear(); }
//    int Remove(bool first, T hm, Pool<T>* p);
//    HPelem* Add(bool first, T hm, Pool<T>* p)
//    {
//        return first ? mFirstCache.Add(hm, p) : mSecondCache.Add(hm, p);
//    }

//    int LayerEnumerate(bool first, int Flag, HashEnumer* e, int layer, int cmp_sign) const;
//    bool IsClear() { return mFirstCache.IsClear() && mSecondCache.IsClear(); }
//    bool IsFirstClear() { return mFirstCache.IsClear(); }
//    bool IsSecondClear() { return mSecondCache.IsClear(); }
//    DWORD Upload(bool first) { return first ? mFirstCache.Upload() : mSecondCache.Upload(); }
//    ~HashTreeHead() { Clear(); }
//};

class HashTreeHead<T>
{
    Hlist<T> mFirstCache = new Hlist<T>();
    Hlist<T> mSecondCache = new Hlist<T>();
    void Clear() { mFirstCache.Clear(); mSecondCache.Clear(); }

    public HashTreeHead() { Clear(); }
    public int Remove(bool first, T hm)
    {
        return first ? mFirstCache.Remove(hm) : mSecondCache.Remove(hm);
    }
    public bool Add(bool first, T hm)
    {
        if (first) mFirstCache.Add(hm); else mSecondCache.Add(hm);
        return true; //При использовании pool возвращать необходимо результат получения из пула
    }

    public int LayerEnumerate(bool first, uint Flag, HashEnumer e, int layer, int cmp_sign)
    {
        return first ? mFirstCache.LayerEnumerate(Flag, e, layer, cmp_sign) : mSecondCache.LayerEnumerate(Flag, e, layer, cmp_sign);
    }
    public bool IsClear() { return IsFirstClear() && IsSecondClear(); }
    public bool IsFirstClear() { return mFirstCache.IsClear(); }
    bool IsSecondClear() { return mSecondCache.IsClear(); }
    //public DWORD Upload(bool first) { return first ? mFirstCache.Upload() : mSecondCache.Upload(); }
    public DWORD Upload(bool first) { return first ? mFirstCache.Upload() : mSecondCache.Upload(); }
    ~HashTreeHead() { Clear(); }
};

