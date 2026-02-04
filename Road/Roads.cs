using Geometry;
using Storm;
using System;
using System.Collections.Generic;
using UnityEngine;
using static DataHasherDefines;
using crc32 = System.UInt32;
using DWORD = System.UInt32;

public class RSection : IDisposable
{
    // взаимодействие с BaseScene
    protected List<RSection> list;
    protected IVisualPart obj;
    public RSection(IVisualPart obj, List<RSection> _list)
    {
        this.obj = obj;
        list = _list;
    }
    ~RSection() { }

    public virtual void Dispose()
    {

    }
}
public class RDecal : RSection
{
    SceneVisualizer rVis;

    IDecal decal;
    HMember decalhm;

    //# ifdef _LASER_TEST
    //interface ILaser   *mpPath;
    //#endif //_LASER_TEST

    /*FPO            **fpos;
    HMember        **fposhm;*/

    IDecal[] node_decal = new IDecal[2];
    HMember[] node_decalhm = new HMember[2];

    public RDecal(SceneVisualizer v, IVisualPart obj, List<RSection> _list) : base(obj, _list)
    {
        rVis = v;
        decal = null;
        decalhm = null;

        Geometry.Line line = obj.GetLinearData();
        Asserts.AssertBp(line.dist < 700);
        // prepare mapping
        ROADDATA data = obj.Data();
#if _LASER_TEST
        mpPath = 0;
        if (data)
        {
            VECTOR v1 = line.org;
            VECTOR v2 = line.org + line.dir * line.dist;
            v1.y = rVis.GetScene().GroundLevel(v1.x, v1.z) + 100.f;
            v2.y = rVis.GetScene().GroundLevel(v2.x, v2.z) + 100.f;
            MATRIX m;
            m.Dir = v2 - v1;
            float d = m.Dir.Norma();
            AssertBp(d > 0.01f);
            m.Dir /= d;
            m.Org = v1;
            m.Up = VUp;
            m.Right = m.Up ^ m.Dir;
            m.Up = m.Dir ^ m.Right;
            mpPath = rVis.GetSceneApi()->CreateLaser();
            mpPath->SetParams(m, line.dist, 0.3, 0.3, FVec4(1.f, 0.f, 1.f, 1.f));
        }
#else
        // create decals
        PolyDecalData pd;
        PolyDecalData p = (PolyDecalData)obj.GetVisualData();
        Asserts.AssertBp(p != null);
        pd = p;

        if (data != null)
        {
            MATRIX m = new MATRIX();
            m.Dir = line.dir;
            m.Dir.y = 0.0f;
            m.Dir.Normalize();
            m.Right.Set(m.Dir.z, 0, -m.Dir.x);
            pd.tc_base = line.org - m.Right * (0.5f * data.Width);
            m.Up.Set(0, 1.0f, 0);
            m.Dir /= data.Width;
            m.Right /= data.Width;

            pd.tc_gen[0] = new Vector3(m.Right.x, m.Up.x, m.Dir.x);
            pd.tc_gen[1] = new Vector3(m.Right.y, m.Up.y, m.Dir.y);
            pd.tc_gen[2] = new Vector3(m.Right.z, m.Up.z, m.Dir.z);
            pd.linear = line;
        }
# if _DEBUG 
        else
            rVis.GetScene().Message("Error: can't find road data!");
#endif //_DEBUG 

        decal = rVis.GetSceneApi().CreatePolyDecal(pd);
        Debug.Log("Created rdecal: " + decal);
        decalhm = decal != null ? rVis.rScene.CreateHMByLine(decal.GetHashObject()) : null;
        Debug.Log("Hashed rdecal: " + (decalhm == null ? "no" : decalhm));
#endif // _LASER_TEST
    }
    //public RDecal(SceneVisualizer v, IVisualPart obj, List<RSection> _list) : base(obj, _list)
    //{
    //    rVis = v;

    //    decal = null;
    //    decalhm = null;

    //    geombase.Line line = obj.GetLinearData();
    //    ROADDATA data = obj.Data();

    //    if (data != null) Debug.Log("Drawing " + data.FullName + obj.GetVisual() + " @ " + line.org + " " + line.dist) ;


    //}
    //public RDecal(SceneVisualizer v, IVisualPart obj, List<RSection> _list) : base(obj, _list)
    //{
    //    rVis = v;
    //    decal = null;
    //    decalhm = null;

    //    geombase.Line line = obj.GetLinearData();
    //    ROADDATA data = obj.Data();

    //    PolyDecalData p = (PolyDecalData)obj.GetVisualData();
    //    Debug.LogFormat("Creating visual {0} for {1}",p,data.FullName );
    //}

    //public RDecal(SceneVisualizer v, IVisualPart obj, List<RSection> _list) : base(obj, _list)
    //{
    //    rVis = v;
    //    decal = null;
    //    decalhm = null;

    //    //Geometry.Line line = obj.GetLinearData();
    //    Assert.IsTrue(line.dist < 700);
    //    // prepare mapping
    //    ROADDATA data = obj.Data();


    //    PolyDecalData pd;
    //    {
    //        PolyDecalData p = (PolyDecalData)obj.GetVisualData();
    //        Assert.IsNotNull(p);
    //        pd = p;
    //    }

    //    if (data!=null)
    //    {
    //        Matrix m = new Matrix();
    //        m.Dir = line.dir;
    //        m.Dir.y = 0f;
    //        m.Dir.Normalize();
    //        m.Right.Set(m.Dir.z, 0, -m.Dir.x);
    //        pd.tc_base = line.org - m.Right * (0.5f * data.Width);
    //        m.Up.Set(0, 1f, 0);
    //        m.Dir /= data.Width;
    //        m.Right /= data.Width;

    //        pd.tc_gen.raw[0] = new Vector3(m.Right.x, m.Up.x, m.Dir.x);
    //        pd.tc_gen.raw[1] = new Vector3(m.Right.y, m.Up.y, m.Dir.y);
    //        pd.tc_gen.raw[2] = new Vector3(m.Right.z, m.Up.z, m.Dir.z);
    //        pd.linear = line;
    //    }
    //    else
    //        rVis.GetScene().Message("Error: can't find road data!");

    //    decal = rVis.GetSceneApi().CreatePolyDecal(pd);
    //    decalhm = decal!=null ? rVis.rScene.CreateHMByLine(decal.GetHashObject()) : null;
    //}
    ~RDecal()
    {
        Dispose();
    }

    public override void Dispose()
    {
        if (decalhm != null) rVis.rScene.DeleteHMByLine(decalhm);
        IMemory.SafeRelease(decal);
        base.Dispose();
    }
};

public class RBuilding : RSection
{
    BaseScene rScene;

    FPO start;
    FPO end;
    HMember starthm;
    HMember endhm;

    int middles_count;
    FPO[] middles;
    HMember middleshm;

    //std::vector<BaseCollidingForRBuilding*, NullPassAllocator<BaseCollidingForRBuilding*>> m_Colliding;

    void AddColliding(FPO f) { }
    void CheckY(FPO _check) { }
    public RBuilding(BaseScene h, IVisualPart obj, List<RSection> _list) : base(obj, _list)
    {
        rScene = h;
    }
    ~RBuilding() { }
};


public interface IDecal : IObject
{
    new public const uint ID = (0xB592FEAC);

    /// <summary>
    /// set's color of the object ( color.a is intensity(opacity) )
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(Color32 color);

    /// <summary>
    /// decals drawn in layering order ( the less layer number - the sooner it's drawn )
    /// </summary>
    /// <param name="i"></param>
    public void SetLayering(int i);
    /// <summary>
    /// recommended : the less layers - the better
    /// </summary>
    /// <returns></returns>
    public int GetLayering();

    public IHashObject GetHashObject();

    new public DWORD GetMyID()
    {
        return ID;
    }
};

public class RoadNode
{
    // env
    public Vector3 org;
    public int link_count;
    RoadLink[] links;

    // paths
    public int calc_len_counter;
    public RoadNode predecessor;
    public RoadLink follow_by_link;
    public RoadNode tent_prev, tent_next;
    public float path;
    bool deleted;

    // virtual ~RoadNode();

    // local
    public RoadLink GetLink(int n) { return links[n]; }
    public RoadLink GetFollowLink(RoadNode nd)
    {
        for (int i = 0; i < link_count; i++)
        {
            RoadLink lnk = links[i];
            if (lnk.Node1() == nd || lnk.Node2() == nd)
                return lnk;
        }
        return null;

    }

    void BuildVisual()
    {
        // sorting links
        //qsort(links, link_count, sizeof(RoadLink*), LinksCompare);

        RoadDirData cur = new RoadDirData();

        cur.Fill(this, links[0]);

        for (int i = 0; i < link_count; i++)
        {
            Vector3 cross;
            if (link_count > 1)
            {
                RoadDirData nxt = new RoadDirData();
                nxt.Fill(this, links[(i + 1) % link_count]);

                Vector3 coeff1, coeff2;
                Vector3 org1, org2;
                bool p1, p2;

                p1 = cur.Calc(out coeff1, SideOp.SIDE_LEFT);
                p2 = nxt.Calc(out coeff2, SideOp.SIDE_RIGHT);
                cur.SideOrg(out org1, SideOp.SIDE_LEFT);
                nxt.SideOrg(out org2, SideOp.SIDE_RIGHT);
                Roadgeom.CalcOrg(org1, org2, coeff1, coeff2, out cross, p1, p2);

                cur.AddCross(cross, SideOp.SIDE_LEFT);
                cur.AddCross(org, SideOp.SIDE_CENTER);

                nxt.AddCross(org, SideOp.SIDE_CENTER);
                nxt.AddCross(cross, SideOp.SIDE_RIGHT);
                cur = nxt;
            }
            else
            {

                cur.SideOrg(out cross, SideOp.SIDE_RIGHT);
                cur.AddCross(cross, SideOp.SIDE_RIGHT);

                cur.AddCross(org, SideOp.SIDE_CENTER);

                cur.SideOrg(out cross, SideOp.SIDE_LEFT);
                cur.AddCross(cross, SideOp.SIDE_LEFT);
            }
        }

    }
    public void LinkDeleted(RoadLink link)
    {
        //Assert(!deleted);
        if (deleted) throw new System.Exception("Trying to delete already deleted Road node");
        for (int i = 0; i < link_count; ++i)
        {
            if (links[i] == link)
            {
                links[i] = null;
                break;
            }
        }
        RoadLink[] new_links = new RoadLink[0];
        if (link_count > 1)
        {
            int j = 0;
            new_links = new RoadLink[link_count - 1];
            for (int i = 0; i < link_count; ++i)
                if (links[i] != null)
                    new_links[j++] = links[i];
        }

        if (link_count > 0)
        {
            link_count--;
            //delete links;
        }
        links = new_links;

    }

    bool IsDeleted() { return deleted; }
    void DeleteNode(RoadNode new_node)
    {
        if (deleted) throw new System.Exception("Trying to delete already deleted roadnode");
        deleted = true;
        for (int i = 0; i < link_count; ++i)
        {
            links[i].NodeDeleted(this, new_node);
            if (new_node != null)
                new_node.AddLink(links[i]);
        }
        if (links != null)
        {
            //delete links;
            links = new RoadLink[0]; link_count = 0;
        }

    }
    bool IsBadFood()
    {
        if (deleted) return false;
        return link_count == 0;

    }

    // paths
    public void SetProp(RoadNode _prev, float _len, RoadLink _follow) { predecessor = _prev; path = _len; follow_by_link = _follow; }
    public bool IsTentative() { return tent_next != null || tent_prev != null; }

    // methods
    void AddLink(RoadLink _lnk)
    {
        //Assert(!deleted);
        if (deleted) throw new System.Exception("Trying to add road link to deleted node");
        RoadLink[] new_links = new RoadLink[link_count + 1];
        for (int i = 0; i < link_count; ++i)
            new_links[i] = links[i];
        new_links[link_count++] = _lnk;
        //if (links!=null) delete links;
        links = new_links;
    }
    public void SetLink(int num, RoadLink _lnk)
    {
        //        Assert(num < link_count);
        if (num >= link_count) throw new System.Exception("num greater than link_count");
        links[num] = _lnk;
    }
    //int SizeForNodeData() { return sizeof(NodeDataHead) + sizeof(int) * link_count; }

    public RoadNode(Vector3 _org, int l_count = 0)
    {
        if (l_count == 0)
        {
            links = new RoadLink[0];
            link_count = 0;
        }
        else
        {
            link_count = l_count;
            links = new RoadLink[link_count];
            for (int i = 0; i < link_count; ++i)
                links[i] = null;
        }
        org.y = 0;

    }
};

public class RoadLink
{
    const float CRITICAL_LEN = 0.01f;

    ROADDATA data;                     // данные по секции дороги
    RoadNode node1;                    // точка дорог
    RoadNode node2;                    // точка дорог
                                       // sys
    int part_count;
    RoadVisualDataPart part;
    Vector3 right;
    float length;
    bool deleted;

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(data.ToString());
        sb.AppendLine("Lenght: " + length);
        sb.AppendLine("Part count: " + part_count);
        return sb.ToString();
    }
    void CalcLen()
    {
        if (node1 != null && node2 != null)
            length = (node1.org - node2.org).magnitude;
        else
            length = 0f;

    }
    void CalcRight()
    {
        if (length < CRITICAL_LEN) return;
        Vector3 dir;
        dir = node2.org - node1.org;
        right.Set(dir.z, 0, -dir.x);
        right.Normalize();

    }
    public RoadLink()
    {
        deleted = false;
        Set(null, null, null);
    }
    public RoadLink(ROADDATA _data, RoadNode _node1, RoadNode _node2)
    {
        deleted = false;
        Set(_data, _node1, _node2);
    }
    public void Set(ROADDATA _data, RoadNode _node1, RoadNode _node2)
    {
        data = _data;
        node1 = _node1;
        node2 = _node2;
        part_count = 0;
        part = null;
        CalcLen();
        CalcRight();
    }
    ~RoadLink()
    {
        if (GetVisualHead() != null)
            FreeVisualParts();

    }
    // api
    public RoadNode Node1() { return node1; }
    public RoadNode Node2() { return node2; }
    public RoadNode Otherwise(RoadNode r) { if (r == node1) return node2; else if (r == node2) return node1; else return null; }
    public ROADDATA Data() { return data; }
    public Vector3 GetRight() { return right; }

    bool IsDeleted() { return deleted; }
    void DeleteLink()
    {
        //        Assert(!deleted);
        if (deleted) throw new System.Exception("Trying delete already deleted road link");
        deleted = true;
        node1.LinkDeleted(this);
        node2.LinkDeleted(this);
        node1 = node2 = null;

    }
    bool IsBadFood()
    {
        if (deleted) return false;
        return node1 == node2;

    }

    // visual
    public RoadVisualDataPart GetVisualHead() { return part; }
    public int PartCount() { return part_count; }
    public void AddVisualPart(RoadVisualDataPart _part) { _part.SetNext(part); part = _part; part_count++; }
    void FreeVisualParts()
    {
        RoadVisualDataPart p = part, n;

        while (p != null)
        {
            n = p.GetNext();
            //delete p;
            p = n;
        }
        part = null;

    }

    // paths
    public bool IsGate()
    {
        return (data.Type != RoadType.Road);
    }
    public DWORD GetNavigationFlag()
    {
        switch (data.Type)
        {
            case RoadType.Road: return RoadpointDefines.NAV_ROAD | RoadpointDefines.NAV_GOOD;
            case RoadType.Tunnel: return RoadpointDefines.NAV_TUNNEL | RoadpointDefines.NAV_GOOD;
            case RoadType.Bridge: return RoadpointDefines.NAV_BRIDGE | RoadpointDefines.NAV_GOOD;
            default: return RoadpointDefines.NAV_PLAIN | RoadpointDefines.NAV_DANGER;
        }

    }

    // const inline methods
    public void NodeDeleted(RoadNode node, RoadNode new_node)
    {

        //        Assert(!deleted);
        if (deleted) throw new System.Exception("Trying to delete already deleted road node");
        if (node1 == node)
            node1 = new_node;
        else if (node2 == node)
            node2 = new_node;
        else
        {
            throw new System.Exception("Can not delete roadnode because of... reasons.");
            //Assert(0);
        }
        CalcLen();
        CalcRight();

    }
    public float Length() { return length; }
    //int SizeForLinkData() { return sizeof(LinkData); }
}
public enum SideOp
{
    SIDE_NONE,
    SIDE_CENTER,
    SIDE_LEFT,
    SIDE_RIGHT
};

public enum PartOp
{
    poNode1Left,
    poNode1Center,
    poNode1Right,
    poNode2Left,
    poNode2Center,
    poNode2Right,
    poNodeNone
};

public class RoadVisualDataPart : IVisualPart
{
    const int ROAD_PART_MAX_LEN = 600;
    //IID(0x91E0555A);
    RSection visual;
    Vector3 start, end, diff;
    RoadLink parent;
    VisPolyDecalData visual_data; // визуальное данные
    RoadVisualDataPart next;
    PartOp[] dir_flags;
    int intersections_filled;
    float length;
    int nodes;

    #region от GetIHashObject 
    public IHashObject GetIHashObject()
    {
        return this;
    }
    #endregion
    public RoadVisualDataPart(RoadLink _parent, int _nodes, Vector3 _start, Vector3 _end)
    {
        nodes = _nodes;
        parent = _parent;
        ROADDATA data = _parent.Data();
        visual_data = (data.Type == RoadType.Road) ?
            new VisPolyDecalData(nodes, data.SectionScript, data.SectionTexture, data.SectionMaterial) : null;
        switch (data.Type)
        {
            case RoadType.Road: SetFlag((uint)DataHasherDefines.RSObjectId((int)DataHasherDefines.RS_CLASS_ROAD)); break;
            case RoadType.Bridge: SetFlag((uint)DataHasherDefines.RSObjectId((int)DataHasherDefines.RS_CLASS_BRIDGE)); break;
            case RoadType.Tunnel: SetFlag((uint)DataHasherDefines.RSObjectId((int)DataHasherDefines.RS_CLASS_TUNNEL)); break;
        }

        dir_flags = new PartOp[nodes];
        for (int i = 0; i < nodes; i++) dir_flags[i] = PartOp.poNodeNone;
        start = _start;
        end = _end;
        UpdateParams();
    }
    //~RoadVisualDataPart();

    // parent and visual data queue
    public void SetNext(RoadVisualDataPart _next) { next = _next; }
    public RoadVisualDataPart GetNext() { return next; }
    public RoadLink GetParent() { return parent; }

    // visual data
    void SetVisualData(VisPolyDecalData _vis) { visual_data = _vis; }
    void FreeVisualData() { if (visual_data != null) visual_data = null; }
    public void SetNode(int num, Vector3 org) { if (visual_data != null) visual_data.nodes[num] = org; }

    // build visual
    Vector3 GetVector(PartOp _op)
    {
        //Assert(visual_data);
        int index = GetIndexFromOp(_op);
        //Assert(index != -1);
        return visual_data.nodes[index];

    }
    int GetIndexFromOp(PartOp _op)
    {
        switch (_op)
        {
            case PartOp.poNode1Left: return 5;
            case PartOp.poNode1Center: return 4;
            case PartOp.poNode1Right: return 3;
            case PartOp.poNode2Left: return 2;
            case PartOp.poNode2Center: return 1;
            case PartOp.poNode2Right: return 0;
            default: return -1;
        }
        ;

    }
    void CheckOrger()
    {
        if (visual_data == null) return;
        if (length > ROAD_PART_MAX_LEN)
            Duplicate(length);
    }
    void CheckFigure()
    {
        if (visual_data == null) return;

        bool ret = true;
        ret &= CheckNorma(0, 1);
        ret &= CheckNorma(0, 2);
        ret &= CheckNorma(0, 3);
        ret &= CheckNorma(0, 4);
        ret &= CheckNorma(0, 5);
        if (!ret)
        {
            BuildDefault();
        }

    }
    void BuildDefault()
    {
        float wdh = parent.Data().Width * 0.5f;

        SetIntersection(end - parent.GetRight() * wdh, PartOp.poNode1Left);
        SetIntersection(end, PartOp.poNode1Center);
        SetIntersection(end + parent.GetRight() * wdh, PartOp.poNode1Right);
        SetIntersection(start - parent.GetRight() * wdh, PartOp.poNode2Right);
        SetIntersection(start, PartOp.poNode2Center);
        SetIntersection(start + parent.GetRight() * wdh, PartOp.poNode2Left);

    }
    bool CheckNorma(int n1, int n2)
    {
        float nrm = (visual_data.nodes[n1] - visual_data.nodes[n2]).magnitude;
        return !(nrm > ROAD_PART_MAX_LEN + 150f || nrm < 0f);

    }
    public void SetIntersection(Vector3 _intersection, PartOp _op)
    {
        if (visual_data == null) return;
        int i = GetIndexFromOp(_op);
        dir_flags[i] = _op;
        visual_data.nodes[i] = _intersection;

    }
    void Duplicate(float length)
    {
        int n = (int)(length / ROAD_PART_MAX_LEN);
        if ((ROAD_PART_MAX_LEN * n) + 2f < length)
            n++;

        float cur_len = 0;
        Vector3 cur_org = start;
        Vector3 dir = end - start;
        dir /= length;
        Vector3 next_end;

        for (int i = 0; i < n; i++)
        {
            float ost = length - cur_len;
            Vector3 next;
            if (ost > ROAD_PART_MAX_LEN)
            {
                next = cur_org + dir * ROAD_PART_MAX_LEN;
                cur_len += ROAD_PART_MAX_LEN;
            }
            else
            {
                next = end;
                cur_len += ost;
            }
            RoadVisualDataPart part = null;
            if (i == 0)
            {
                next_end = next;
            }
            else
            {
                part = new RoadVisualDataPart(parent, 6, cur_org, next);
                part.SetIntersection(cur_org - parent.GetRight() * parent.Data().Width * 0.5f, PartOp.poNode1Right);
                part.SetIntersection(cur_org, PartOp.poNode1Center);
                part.SetIntersection(cur_org + parent.GetRight() * parent.Data().Width * 0.5f, PartOp.poNode1Left);
            }
            if (i == n - 1)
            {
                if (part != null)
                {
                    part.SetIntersection(GetVector(PartOp.poNode2Left), PartOp.poNode2Left);
                    part.SetIntersection(GetVector(PartOp.poNode2Center), PartOp.poNode2Center);
                    part.SetIntersection(GetVector(PartOp.poNode2Right), PartOp.poNode2Right);
                }
            }
            else
            {
                if (part != null)
                {
                    part.SetIntersection(next - parent.GetRight() * parent.Data().Width * 0.5f, PartOp.poNode2Left);
                    part.SetIntersection(next, PartOp.poNode2Center);
                    part.SetIntersection(next + parent.GetRight() * parent.Data().Width * 0.5f, PartOp.poNode2Right);
                }
            }
            cur_org = next;
            if (part != null)
                parent.AddVisualPart(part);
        }


    }
    void UpdateParams()
    {
        diff = end - start;
        length = diff.magnitude;
        //Assert(length > 0.1f);
        diff /= length;

    }
    //int SizeForVisualData() { return visual_data ? sizeof(VisualDataHead) + sizeof(VECTOR) * visual_data.numnodes : sizeof(VisualDataHead); }

    // api
    // api
    public virtual void SetVisual(RSection _vis)
    {
        visual = _vis;
    }
    public virtual RSection GetVisual()
    {
        return visual;
    }
    public virtual ROADDATA Data()
    {
        return parent.Data();
    }
    public virtual VisPolyDecalData GetVisualData()
    {
        return visual_data;
    }
    public virtual geombase.Sphere GetBoundingSphere()
    {
        return new geombase.Sphere(start + (end - start) * 0.5f, 0);
    }
    public virtual Line GetLinearData()
    {
        return new Line(start, diff, length);
    }


    //От IHashObject
    uint flags;
    public uint GetFlag(uint f) { return flags & f; }
    public uint SetFlag(uint f) { flags |= f; return f; }
    public void ClearFlag(uint f) { flags &= ~f; }

    public object Query(uint cls_id)
    {
        throw new System.NotImplementedException();
    }

    public bool MatchGroup(uint f)
    {
        return (GetFlag(HashFlags.OF_GROUP_MASK & f) != 0);
    }

    public bool MatchType(uint f)
    {
        return (GetFlag(HashFlags.OF_USER_MASK & f) != 0);
    }

    public bool MatchFlags(uint f)
    {
        return MatchGroup(f) && MatchType(f);
    }

    public uint GetFlags()
    {
        return flags;
    }

    public void Dispose()
    {
        if (visual_data != null)
            visual_data.Dispose();
        dir_flags = null;
    }
}
public class RoadpointDefines
{
    public const uint NAV_TUNNEL = 0x00000001;
    public const uint NAV_ROAD = 0x00000002;
    public const uint NAV_BRIDGE = 0x00000004;
    public const uint NAV_PLAIN = 0x00000008;

    public const uint NAV_IMPOSSIBLE = 0x00000010;
    public const uint NAV_DANGER = 0x00000020;
    public const uint NAV_GOOD = 0x00000040;

    public const uint NAV_GROUND_LEVEL = 0x10000000;

    // flags
    public const uint NAV_MOD_MASK = 0x00000070;
    public const uint NAV_STATE_MASK = 0x0000000F;

    public const uint NAV_VISITED = 0x00000800;
    public const uint NAV_FAILED = 0x00001000;

    public const float ROAD_INFINITY = float.MaxValue;//10000000000f;

    // Road ELEM flags
    public const uint LINK_HIDDEN = 0x00000008;
    public const uint LINK_CREATED = 0x00000010;

    public const uint NODE_CREATED = 0x00000010;
    public const uint NODE_PERMANENT = 0x00000008;

}

public class RoadDirData
{
    RoadNode otherwise_node, node;
    RoadLink link;
    Vector3 dir;
    Vector3 right;

    public void AddCross(Vector3 _org, SideOp _op)
    {
        RoadVisualDataPart part = link.GetVisualHead();
        if (part == null)
        {
            Vector3 org1 = link.Node1().org;
            Vector3 org2 = link.Node2().org;
            part = new RoadVisualDataPart(link, 6, org1, org2);
            link.AddVisualPart(part);
        }
        bool second = false;
        PartOp op = PartOp.poNodeNone;
        if (link.Node2() == node) second = true;
        if (second)
        {
            switch (_op)
            {
                case SideOp.SIDE_RIGHT: op = PartOp.poNode2Right; break;
                case SideOp.SIDE_LEFT: op = PartOp.poNode2Left; break;
                case SideOp.SIDE_CENTER: op = PartOp.poNode2Center; break;
            }
        }
        else
        {
            switch (_op)
            {
                case SideOp.SIDE_RIGHT: op = PartOp.poNode1Right; break;
                case SideOp.SIDE_LEFT: op = PartOp.poNode1Left; break;
                case SideOp.SIDE_CENTER: op = PartOp.poNode1Center; break;
            }
        }
        part.SetIntersection(_org, op);

    }
    public void Fill(RoadNode _node, RoadLink _link)
    {
        link = _link;
        node = _node;
        otherwise_node = link.Otherwise(_node);
        dir = _node.org - otherwise_node.org;
        right.Set(dir.z, 0, -dir.x);
        right.Normalize();

    }
    public bool Calc(out Vector3 _coeff, SideOp _op)
    {
        Vector3 lorg;
        SideOrg(out lorg, _op);
        return Roadgeom.CalcCoeffs(lorg, dir, out _coeff);
    }
    public void SideOrg(out Vector3 _org, SideOp _op)
    {
        _org = Vector3.zero;
        if (_op == SideOp.SIDE_RIGHT)
            _org = node.org + right * link.Data().Width * 0.5f;
        else if (_op == SideOp.SIDE_LEFT)
            _org = node.org - right * link.Data().Width * 0.5f;

    }
    public RoadDirData() : this(null, null, Vector3.zero)
    {
        //otherwise_node = null;
        //link = null;
        //dir = Vector3.zero;
    }
    public RoadDirData(RoadNode _otherwise_node, RoadLink _link, Vector3 _dir)
    {
        otherwise_node = _otherwise_node;
        link = _link;
        dir = _dir;
    }
};

public static class Roadgeom
{
    const float COORD_CUT_LEN = 0.01f;

    // cross math
    public static bool CalcCoeffs(Vector3 begin, Vector3 dir, out Vector3 coeff)
    {
        coeff = new Vector3();
        bool ret = false;
        if (dir.x < COORD_CUT_LEN && dir.x > -COORD_CUT_LEN) { dir.x = 0; ret = true; }
        if (dir.z < COORD_CUT_LEN && dir.z > -COORD_CUT_LEN) dir.z = 0;

        if (ret) return ret;
        float x = dir.z / dir.x;
        float z = -x * begin.x + begin.z;
        coeff.Set(x, 0, z);
        return ret;

    }
    public static void CalcOrg(Vector3 begin1, Vector3 begin2, Vector3 coeff1, Vector3 coeff2, out Vector3 CrossOrg, bool p1, bool p2)
    {
        float a = coeff1.x - coeff2.x, b = coeff2.z - coeff1.z;

        if (p1)
        {
            if (p2)
            {
                CrossOrg = begin1;
            }
            else
            {
                CrossOrg.x = begin1.x;
                CrossOrg.z = coeff2.x * CrossOrg.x + coeff2.z;
            }
        }
        else
        {
            if (p2)
            {
                CrossOrg.x = begin2.x;
                CrossOrg.z = coeff1.x * CrossOrg.x + coeff1.z;
            }
            else
            {
                CrossOrg.x = b / a;
                CrossOrg.z = coeff1.x * CrossOrg.x + coeff1.z;
            }
        }
        CrossOrg.y = 0;

    }
    public static bool ProbCross(Vector3 begin1, Vector3 end1, Vector3 begin2, Vector3 end2, Vector3 dir2)
    {
        dir2 = end2 - begin2;

        Vector3 v1 = begin1 - begin2;
        Vector3 v2 = end1 - begin2;

        float n1 = v1.magnitude;
        float n2 = v2.magnitude;
        float n3 = dir2.magnitude;

        float d1 = (v1.x * dir2.x + v1.z * dir2.z) / (n1 * n3);
        float d2 = (v1.x * v2.x + v1.z * v2.z) / (n1 * n2);
        if (d1 < d2) return false;

        if ((v1.x * v2.z - v1.z * v2.x) * (v1.x * dir2.z - v1.z * dir2.x) <= 0) return false;

        return true;

    }
}

public class RoadBuilder : IRoadBuilder
{
    const int MAX_QUEUE_SIZE = 1024;
    const int MAX_CROSSES = 1024;

    const float LINK_LEN_CUT = 3.0f;
    const float NODE_LEN_CUT = 40.0f;
    const float CUT_BOUND = 60.0f;

    public class CrossData
    {
        RoadLink link1;
        RoadLink link2;
        RoadNode node;
        Vector3 Org;

        CrossData() { link1 = link2 = null; d[0] = d[1] = d[2] = d[3] = 0; Org = Vector3.zero; node = null; }
        CrossData(RoadLink _link1, RoadLink _link2, Vector3 _org)
        {
            link1 = _link1; link2 = _link2; Org = _org; d[0] = d[1] = d[2] = d[3] = 0; node = null;
        }
        float GetLast(RoadLink link) { if (link == link1) return d[1]; else if (link == link2) return d[3]; return 0; }
        float[] d = new float[4];
    }

    List<CrossData> crosses = new List<CrossData>();
    CrossData[] cross_data;

    List<RoadLink> links;
    List<RoadNode> nodes;

    RoadNetData build;
    char[] builded_buffer;
    IRoadsStore mRoadsStore;

    public RoadBuilder(IRoadsStore rs)
    {
        mRoadsStore = rs;
        cross_data = new CrossData[MAX_CROSSES];
        build = null;
        builded_buffer = null;
    }

    public bool MergeData(iUnifiedVariableArray uva, IPercentNotifier pn)
    {
        throw new System.NotImplementedException();
    }

    public RoadNetData GetData()
    {
        throw new System.NotImplementedException();
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

public interface IRoadsStore
{
    ROADDATA getByCode(crc32 code, bool mustExists = true);
};

public class GroupDataContainer : IHashObject
{
    private IHashObject myHashObject;

    public GROUP_DATA pGroupData;

    void Clear() { SetFlag((uint)RSObjectId((int)RS_CLASS_GROUP)); ClearFlag(RS_MODE_MUST_DELETED | RS_MODE_MUST_CREATED); }

    public GroupDataContainer() : this(null) { }

    public GroupDataContainer(GROUP_DATA _group_data)
    {
        myHashObject = new HashObject();

        pGroupData = _group_data;
        Clear();
    }

    public void Dispose()
    {
        throw new System.NotImplementedException();
    }

    //#region от IHashObject:
    //uint flags;

    //public IHashObject GetIHashObject()
    //{
    //    return this;
    //}
    //public uint GetFlag(uint f)
    //{
    //    return flags & f;
    //}

    //public uint SetFlag(uint f)
    //{
    //    flags |= f; return f;
    //}

    //public void ClearFlag(uint f)
    //{
    //    flags &= ~f;
    //}



    //public object Query(int cls_id)
    //{
    //    throw new System.NotImplementedException();
    //}

    public geombase.Sphere GetBoundingSphere()
    {
        return new geombase.Sphere(new Vector3(pGroupData.CenterX, 0, pGroupData.CenterZ), pGroupData.Radius);
    }

    public uint GetFlag(uint f)
    {
        return myHashObject.GetFlag(f);
    }

    public uint SetFlag(uint f)
    {
        return myHashObject.SetFlag(f);
    }

    public void ClearFlag(uint f)
    {
        myHashObject.ClearFlag(f);
    }

    public bool MatchGroup(uint f)
    {
        return myHashObject.MatchGroup(f);
    }

    public bool MatchType(uint f)
    {
        return myHashObject.MatchType(f);
    }

    public bool MatchFlags(uint f)
    {
        return myHashObject.MatchFlags(f);
    }

    public Line GetLinearData()
    {
        return myHashObject.GetLinearData();
    }

    public IHashObject GetIHashObject()
    {
        return this;
    }

    public uint GetFlags()
    {
        return myHashObject.GetFlags();
    }

    public object Query(uint cls_id)
    {
        return myHashObject.Query(cls_id);
    }

    //public bool MatchGroup(uint f)
    //{
    //    return (GetFlag(HashFlags.OF_GROUP_MASK & f)!=0);
    //}

    //public bool MatchType(uint f)
    //{
    //    return (GetFlag(HashFlags.OF_USER_MASK & f) !=0);
    //}

    //public bool MatchFlags(uint f)
    //{
    //    return MatchGroup(f) && MatchType(f);
    //}

    //public object Query(uint cls_id)
    //{
    //    throw new System.NotImplementedException();
    //}

    //public uint GetFlags()
    //{
    //    return flags;
    //}
    //#endregion
}

