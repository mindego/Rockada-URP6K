using geombase;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static AIData;
using static DataHasherDefines;
using static RoadElementState;
using static RoadpointDefines;
using DWORD = System.UInt32;

public struct RoadOrg
{
    public RoadLink owner;
    public RoadNode owner_node;
    public Vector3 org;
    public Vector3 cross;
    public float dist;
    public bool node_cross;
    public void Set(Vector3 _org) { owner = null; org = _org; dist = SEARCH_ROAD_RADIUS; cross = INFINITE_VECTOR; owner_node = null; node_cross = false; }
    public void SetCross(Vector3 _cross, float _dist, RoadLink _lnk, RoadNode _node, bool cr) { cross = _cross; dist = _dist; owner = _lnk; owner_node = _node; node_cross = cr; }
};

public class myRoadStore : IRoadsStore
{
    //TODO - Возможно, стоит убрать нафиг
    public ROADDATA getByCode(uint code, bool mustExists = true)
    {
        return ROADDATA.GetByCode(code, mustExists);
    }
}
public class DataHasher : IDataHasher, HashEnumer
{
    //static string  HelloWorld = string.Format("Data hasher : v.%d.%d  build " __TIME__ " " __DATE__);
    //static string HelloWorld = string.Format("Data hasher : v.{0}.{1} ");
    static string HelloWorld = "Data hasher : v.{0}.{1} ";
    static int MAX_CACHE_SIZE = 1024;
    public const uint DATA_HASHER_VERSION = 0x00010003;
    ILog log;
    // source data

    IRoadsStore mRoadsStore;

    RoadNetData roads;
    int poolsize;

    // hash
    private RoadElementState enum_mode;
    private IHash hash;
    private ProtoHash protohash;

    public bool ProcessElement(HMember m)
    {
        IHashObject obj = m.Object();
        switch (GetMode())
        {
            case POS_OLD:
                AddHashObject(m);
                obj.SetFlag(RS_MODE_MUST_DELETED);
                break;
            case POS_NEW:
                AddHashObject(m);
                obj.SetFlag(RS_MODE_MUST_CREATED);
                break;
            case POS_CALC_SRC:
                {
                    RoadVisualDataPart rv = (RoadVisualDataPart)obj;
                    RoadLink lnk = (RoadLink)(rv.GetParent());
                    CalcDist(ref source_info, lnk);
                }
                break;
            case POS_CALC_DST:
                {
                    RoadVisualDataPart rv = (RoadVisualDataPart)obj;
                    RoadLink lnk = (RoadLink)(rv.GetParent());
                    CalcDist(ref dest_info, lnk);
                }
                break;
        }
        return true;
    }

    private void CreateHash()
    {
        hash = IHashApi.CreateHasher2(IHashApi.HASH_VERSION, null, 400000f, 4000f, poolsize, log, false);
        //protohash = hash.Query<ProtoHash>();
        protohash = (ProtoHash)hash.Query(ProtoHash.ID);
        Asserts.Assert(protohash != null);

    }
    private void DeleteHash()
    {
        throw new System.NotImplementedException();
    }

    public RoadElementState GetMode() { return enum_mode; }
    public int cache_counter;
    public HMember[] hash_cache;
    public void AddHashObject(HMember m)
    {
        //Debug.Log("AddHashObject " + m.Object().GetFlags().ToString("X8") + " " + m.Object());
        //Debug.Log(string.Format("MD {0} MC {1}", RS_MODE_MUST_DELETED.ToString("X8"), RS_MODE_MUST_CREATED.ToString("X8")));
        //Debug.Log(m.Object().GetFlag(RS_MODE_MUST_DELETED | RS_MODE_MUST_CREATED).ToString("X8"));
        if ((m.Object().GetFlag(RS_MODE_MUST_DELETED | RS_MODE_MUST_CREATED)) == 0)
        {
            hash_cache[cache_counter++] = m;
            //Debug.Log("Hash_cache size " + cache_counter);
        }
    }

    // road net storage
    private bool roads_created;
    public void CreateRoads()
    {
        Debug.Log(string.Format("Creating roads roads_created {0} roads == null {1}", roads_created, roads == null));
        if (roads_created || roads == null) return;
        int i;
        // add links
        for (i = 0; i < roads.head.links_count; i++)
        {
            RoadLink link = new RoadLink();
            links.Add(link);
        }

        // add nodes
        for (i = 0; i < roads.head.nodes_count; i++)
        {
            NodeData node_data = roads.GetNodes(i);
            RoadNode node = new RoadNode(node_data.head.org, node_data.head.link_count);
            nodes.Add(node);
        }

        BuildIndexes();
        if (log != null) log.Message("Hash statistics:  links {0}, nodes {1}, visuals {2}", roads.head.links_count.ToString(), roads.head.nodes_count.ToString(), roads.head.visuals_count.ToString());
        Debug.Log(string.Format("Hash statistics:  links {0}, nodes {1}, visuals {2}", roads.head.links_count.ToString(), roads.head.nodes_count.ToString(), roads.head.visuals_count.ToString()));
        // uplink nodes in links
        i = 0;

        for (i = 0; i < links.Count; i++)
        {
            RoadLink lnk = links[i];
            LinkData link_data = roads.GetLinks(i);
            ROADDATA dt = mRoadsStore.getByCode(link_data.roaddata);
            //Debug.Log(string.Format("Creating road {0}\n{1}", i, dt));
            RoadNode node1 = FindNodeByIndex((uint)link_data.node1);
            RoadNode node2 = FindNodeByIndex((uint)link_data.node2);
            Asserts.Assert(dt != null && node1 != null && node2 != null);
            lnk.Set(dt, node1, node2);
            links[i] = lnk;

        }

        // uplink links in nodes
        i = 0;

        for (i = 0; i < nodes.Count; i++)
        {
            RoadNode nd = nodes[i];
            NodeData node_data = roads.GetNodes(i);
            for (int j = 0; j < node_data.head.link_count; j++)
            {
                RoadLink lnk = FindLinkByIndex((uint)node_data.links[j]);
                Asserts.Assert(lnk != null);
                nd.SetLink(j, lnk);
                nodes[i] = nd;
            }
        }

        // add visuals and uplink him
        for (i = 0; i < roads.head.visuals_count; i++)
        {
            VisualData vis_data = roads.GetVisuals(i);
            RoadLink lnk = FindLinkByIndex((uint)vis_data.head.parent);
            Asserts.Assert(lnk != null);
            RoadVisualDataPart part = new RoadVisualDataPart(lnk, vis_data.head.vector_count, vis_data.head.start, vis_data.head.end);
            for (int j = 0; j < vis_data.head.vector_count; j++)
                part.SetNode(j, vis_data.vectors[j]);
            lnk.AddVisualPart(part);
        }
        // hash
        hashed_roads_count = 0;

        foreach (RoadLink lnk in links)
        {
            hashed_roads_count += CountRoadElem(lnk);
        }

        if (hashed_roads_count != 0)
        {
            hashed_roads = new HMember[hashed_roads_count];
            //Инициализируем массив хэшированных дорог
            for (i = 0; i < hashed_roads_count; i++)
            {
                hashed_roads[i] = new HMember();
            }
            int hashed_road_index = 0;

            foreach (RoadLink lnk in links)
            {
                //Debug.Log("Hashing RoadLink " + lnk);
                //m.Add(HashRoadElem(lnk, m));

                RoadVisualDataPart part = lnk.GetVisualHead();
                ROADDATA linkdata = lnk.Data();

                if (linkdata.Show)
                {
                    while (part != null)
                    {
                        HMember m = hashed_roads[hashed_road_index];
                        m.SetObject(part);
                        hash.UpdateMemberStatic(m);
                        hashed_road_index++;
                        part = part.GetNext();
                    }
                }
            }
        }
        roads_created = true;

    }


    public void DeleteRoads()
    {
        if (!roads_created) return;
        if (hashed_roads != null)
        {
            for (int i = 0; i < hashed_roads_count; i++)
            {
                //HMember* m = hashed_roads + i;
                HMember m = hashed_roads[i];
                hash.RemoveMemberStatic(m);
            }
            hashed_roads = null;
        }
        roads_created = false;
        links.Clear();
        nodes.Clear();

    }

    List<RoadLink> links = new List<RoadLink>();
    List<RoadNode> nodes = new List<RoadNode>();
    RoadNode[] mpNodeIndexes;
    RoadLink[] mpLinkIndexes;
    void BuildIndexes()
    {
        int i;

        mpNodeIndexes = new RoadNode[nodes.Count];
        i = 0;
        foreach (RoadNode nd in nodes)
        {
            mpNodeIndexes[i++] = nd;
        }

        mpLinkIndexes = new RoadLink[links.Count];
        i = 0;
        foreach (RoadLink lnk in links) mpLinkIndexes[i++] = lnk;
    }

    RoadNode FindNodeByIndex(DWORD x) { return (x < nodes.Count) ? mpNodeIndexes[x] : null; }
    RoadLink FindLinkByIndex(DWORD x) { return (x < links.Count) ? mpLinkIndexes[x] : null; }

    int CountRoadElem(RoadLink m) { return (m.Data().Show) ? m.PartCount() : 0; }
    int HashRoadElem(RoadLink e, HMember m)
    {
        //RoadVisualDataPart p = e.GetVisualHead();
        //ROADDATA data = e.Data();
        //int n = 0;
        //if (data.Show)
        //{
        //    while (p != null)
        //    {
        //        m.SetObject(p);
        //        hash.UpdateMemberStatic(m);
        //        m++;
        //        n++;
        //        p = p.GetNext();
        //    }
        //}
        //return n;
        return 0;
    }

    int hashed_roads_count;
    HMember[] hashed_roads;

    // calc path
    int calc_path_counter;
    RoadOrg source_info, dest_info;
    RoadNode tentative_head;
    void AddToTentative(RoadNode _nd)
    {
        throw new System.NotImplementedException();
    }
    void SubFromTentative(RoadNode _nd)
    {
        throw new System.NotImplementedException();
    }
    void CalcDist(ref RoadOrg info, RoadLink lnk)
    {
        Vector3 v1 = info.org - lnk.Node1().org;
        Vector3 v2 = lnk.Node2().org - lnk.Node1().org;
        v1.y = 0;
        float len1 = v1.magnitude;
        RoadNode node = null;
        bool node_cross = false;

        float cosf = Vector3.Dot(v1,v2) / (lnk.Length() * len1);
        float min_d;
        Vector3 min_v;

        if (cosf <= 0)
        {
            node = (RoadNode)(lnk.Node1());
            min_v = node.org;
            min_d = len1;
            node_cross = true;
        }
        else
        {
            min_d = len1 * cosf;
            if (min_d > lnk.Length())
            {
                node = (RoadNode)(lnk.Node2());
                min_v = node.org;
                min_d = (info.org - lnk.Node2().org).magnitude;
                node_cross = true;
            }
            else
            {
                v2 /= lnk.Length();
                min_v = lnk.Node1().org + v2 * min_d;
                min_d = Mathf.Sqrt(Mathf.Pow(len1,2) - Mathf.Pow(min_d,2));
                if (min_d > lnk.Length() * 0.5f)
                    node = (RoadNode)(lnk.Node2());
                else
                    node = (RoadNode)(lnk.Node1());
            }
        }
        if (min_d < info.dist)
            info.SetCross(min_v, min_d, lnk, node, node_cross);
    }
    int GetNavFlag(RoadNode cur, RoadNode prv, bool first_point_on_road = false)
    {
        throw new System.NotImplementedException();
    }
    ROADPOINT[] AddPoint(ref ROADPOINT[] cur_buf, Vector3 org, float y, DWORD Flags)
    {
        //cur_buf.Pnt = org;
        //cur_buf.Pnt.y = y;
        //cur_buf.Flags = Flags;
        //return cur_buf + 1;
        return cur_buf;
    }

    ROADPOINT AddPoint(ref ROADPOINT[] cur_buf, ref int index, Vector3 org, float y, DWORD Flags)
    {
        cur_buf[index].Pnt = org;
        cur_buf[index].Pnt.y = y;
        cur_buf[index].Flags = Flags;
        //return cur_buf + 1;
        //return cur_buf;
        index++;
        return cur_buf[index];
    }

    // data
    private bool data_created;
    private int hashed_count;
    private HMember[] hashed_members;
    private GroupDataContainer[] hashed_units;
    private void UpdateGroupData(GroupDataContainer _cont, HMember _memb, GROUP_DATA _grpdata)
    {
        _cont.pGroupData = _grpdata;
        _memb.SetObject(_cont);
        hash.UpdateMember(_memb);
        //Debug.Log("Updating " + _grpdata.Callsign);
    }

    private void CreateData(int grp_count, GROUP_DATA[] dt)
    {
        Debug.Log("Creating Data for " + grp_count + " groups");
        if (data_created) return;

        hashed_count = 0;

        for (int n = 0; n < grp_count; ++n)
        {
            GROUP_DATA grp_data = dt[n];
            if (grp_data.GetFlag(CF_DESTROYED) != 0) continue;
            hashed_count++;
        }

        if (hashed_count != 0)
        {
            // allocate members
            hashed_members = new HMember[hashed_count];
            hashed_units = new GroupDataContainer[hashed_count];
            for (int n = 0; n < grp_count; ++n)
            {
                hashed_members[n] = new HMember();
                hashed_units[n] = new GroupDataContainer();
            }
            int k = 0;

            for (int n = 0; n < grp_count; ++n)
            {
                GROUP_DATA grp_data = dt[n];
                //Debug.Log("Hashing group " + grp_data.Callsign + " " + grp_data.GetFlags());

                if (grp_data.GetFlag(CF_DESTROYED) != 0) continue;
                //UpdateGroupData(hashed_units + k, hashed_members + k, grp_data);
                UpdateGroupData(hashed_units[k], hashed_members[k], grp_data);
                k++;
            }
        }
        data_created = true;
    }
    private void DeleteData()
    {
        if (!data_created) return;
        if (hashed_members != null)
        {
            for (int i = 0; i < hashed_count; i++)
                hash.RemoveMember(hashed_members[i]);
            hashed_members = null;
            hashed_units = null;
        }
        data_created = false;

    }

    private void Clear()
    {
        // hash
        hash = null;
        hash_cache = null;
        // units
        hashed_members = null;
        hashed_count = 0;
        data_created = false;
        // roads
        roads_created = false;
        calc_path_counter = 1;
        hashed_roads = null;
        hashed_roads_count = 0;
        mpNodeIndexes = null;
        mpLinkIndexes = null;

    }

    private void ProcessPosition(ref EnumPosition _pos, uint _flags)
    {
        Sphere oldsp = new Sphere(_pos.old_org, _pos.old_radius);
        Sphere newsp = new Sphere(_pos.new_org, _pos.new_radius);

        geombase.Rect oldrect = protohash.GetRect(oldsp);
        geombase.Rect newrect = protohash.GetRect(newsp);
        if (oldrect == newrect) return;
        Debug.Log(string.Format("Moving from sector {0} to {1} [{2}] {3}", oldrect, newrect, hashed_count, this.GetType()));
        enum_mode = POS_OLD;
        hash.EnumRect(oldrect, _flags, this);
        enum_mode = POS_NEW;
        hash.EnumRect(newrect, _flags, this);
        _pos.old_org = _pos.new_org;
        _pos.old_radius = _pos.new_radius;
    }

    // construct
    public DataHasher(IRoadsStore rs, int _poolsize, ILog _log)
    {
        mRoadsStore = rs;
        Clear();
        hash_cache = new HMember[MAX_CACHE_SIZE];
        if (_log != null)
            _log.Message(HelloWorld, (DATA_HASHER_VERSION >> 16).ToString(), (DATA_HASHER_VERSION & 0x0000FFFF).ToString());
        CreateHash();

    }
    ~DataHasher()
    {
        Dispose();

    }

    protected bool isDisposed = false;
    public void Dispose()
    {
        if (isDisposed) return;
        if (hash_cache != null) hash_cache = null;
        DeleteData();
        DeleteRoads();
        DeleteHash();
        if (roads != null)
            roads = null;
        if (mpNodeIndexes != null)
            mpNodeIndexes = null;
        if (mpLinkIndexes != null)
            mpLinkIndexes = null;
        isDisposed = true;
    }

    // API
    public virtual int HashRoads(IDataBlock rd)
    {
        int len = rd != null ? rd.getLength() : 0;
        if (len > 0)
        {
            byte[] temp = new byte[len];
            rd.getValue(out temp, len);
            LoadRoads(new MemoryStream(temp));
        }
        CreateRoads();

        return 1;
    }

    public virtual int HashRoads(Stream rd)
    {
        int len = rd != null ? (int)rd.Length : 0;
        if (len != 0)
        {
            LoadRoads(rd);
        }
        else
            roads = null;
        CreateRoads();
        //SafeRelease(rd);
        return 1;

    }

    private void LoadRoads(Stream st)
    {
        RoadNetDataHead hd = StormFileUtils.ReadStruct<RoadNetDataHead>(st);
        roads = new RoadNetData(hd);
        roads.head = hd;
        for (int i = 0; i < hd.links_count; i++)
        {
            roads.LinksI[i] = StormFileUtils.ReadStruct<int>(st, st.Position);
        }
        for (int i = 0; i < hd.nodes_count; i++)
        {
            roads.NodesI[i] = StormFileUtils.ReadStruct<int>(st, st.Position);
        }
        for (int i = 0; i < hd.visuals_count; i++)
        {
            roads.VisualsI[i] = StormFileUtils.ReadStruct<int>(st, st.Position);
        }


        for (int i = 0; i < hd.links_count; i++)
        {
            roads.Links[i] = StormFileUtils.ReadStruct<LinkData>(st, st.Position);
        }

        for (int i = 0; i < hd.nodes_count; i++)
        {
            NodeDataHead nhd = StormFileUtils.ReadStruct<NodeDataHead>(st, st.Position);
            NodeData nd = new NodeData();
            nd.head = nhd;
            nd.links = new int[nhd.link_count];
            for (int j = 0; j < nhd.link_count; j++)
            {
                nd.links[j] = StormFileUtils.ReadStruct<int>(st, st.Position);
            }
            roads.Nodes[i] = nd;
        }


        for (int i = 0; i < hd.visuals_count; i++)
        {
            VisualDataHead vdh = StormFileUtils.ReadStruct<VisualDataHead>(st, st.Position);
            VisualData vd = new VisualData();
            vd.head = vdh;
            vd.vectors = new Vector3[vdh.vector_count];
            for (int j = 0; j < vdh.vector_count; j++)
            {
                vd.vectors[j] = StormFileUtils.ReadStruct<Vector3>(st, st.Position);
            }
            roads.Visuals[i] = vd;
        }
        //StormFileUtils.SaveXML<RoadNetData>("Debug.xml", rd);
    }
    public virtual int UnHashRoads()
    {
        DeleteRoads();
        return 1;
    }
    public virtual int HashData(int grp_count, GROUP_DATA[] dt)
    {
        CreateData(grp_count, dt);
        return 1;
    }
    public virtual int UnHashData()
    {
        DeleteData();
        return 1;
    }
    public virtual void CleanData()
    {
        throw new System.NotImplementedException();
    }
    public virtual void EnumCrosses(ref List<EnumPosition> _pos_list, uint _flags, RoadElementEnumer e)
    {
        if (hash != null && protohash != null)
        {
            cache_counter = 0;
            //for (EnumPosition* pos = _pos_list->Head(); pos; pos = pos->Next())
            //    ProcessPosition(pos, _flags);
            EnumPosition pos;
            for (int i = 0; i < _pos_list.Count; i++)
            {
                if (_pos_list[i] == null) continue;
                pos = _pos_list[i];
                ProcessPosition(ref pos, _flags);
                _pos_list[i] = pos;
            }
            //Debug.Log("hashcache size: " + cache_counter);
            for (int i = 0; i < cache_counter; i++)
            {
                //Debug.Log(string.Format("Processing [{0}:{1}] {2}",i,cache_counter,e));
                IHashObject rv = hash_cache[i].Object();
                //Debug.Log(string.Format("Probing flags {0} vs hash flags {1} = {2} {3} [{4}] for {5}",_flags.ToString("X8"),rv.GetFlags().ToString("X8"), rv.GetFlag(_flags).ToString("X8"), rv.GetFlag(_flags) !=0 ? "pass":"skip"  ,rv,e));
                if (rv.GetFlag(_flags) == 0) continue;
                //Debug.Log("Pass");
                e.SetState(POS_NO_ONE);
                if (rv.GetFlag(RS_MODE_MUST_DELETED) != 0)
                    e.SetState(POS_OLD);
                if (rv.GetFlag(RS_MODE_MUST_CREATED) != 0)
                {
                    if (e.GetState() == POS_OLD)
                        e.SetState(POS_BOTH);
                    else
                        e.SetState(POS_NEW);
                }
                rv.ClearFlag(RS_MODE_MUST_DELETED | RS_MODE_MUST_CREATED);
                //hash_cache[i].SetObject(rv); //TODO Вообще, объект в кэше должен передаваться по ссылке, не по значению. Если работает корректно, строку можно просто удалить
                //Debug.Log("grp_data before created" + ((GroupDataContainer)hash_cache[i].Object()).pGroupData.IsCreated().ToString());
                e.ProcessElement(hash_cache[i]);
                //Debug.Log("grp_data after created" + ((GroupDataContainer)hash_cache[i].Object()).pGroupData.IsCreated().ToString());
            }
        }

    }
    //public virtual int CalcPath(Vector3 source, Vector3 dest, ROADPOINT buffer, Vector3 global_started)
    //{
    //    throw new System.NotImplementedException();
    //}

    //public int CalcPath(Vector3 source, Vector3 dest, out ROADPOINT[] buffer, out Vector3 global_started)
    //{
    //    //TODO Корректно реализовать обсчёт пути

    //    List<ROADPOINT> tmpList = new List<ROADPOINT>();
    //    tmpList.Add(new ROADPOINT((source + dest)/2));
    //    tmpList.Add(new ROADPOINT(dest));
    //    global_started = source;
    //    buffer = tmpList.ToArray();

    //    return buffer.Length;
    //}

    public int CalcPath(Vector3 source, Vector3 dest, ref ROADPOINT[] buffer, out Vector3 global_started)
    {
        global_started = Vector3.zero;
        // cut by distance
        Vector3 delta = dest - source;
        delta.y = 0;
        float d = delta.magnitude;
        if (d < SEARCH_PATH_DIST * SEARCH_PATH_DIST) return 0;

        // search nearest nodes
        enum_mode = POS_CALC_SRC;
        source_info.Set(source);
        hash.EnumSphere(new Sphere(source, SEARCH_ROAD_RADIUS), RSObjectId(RS_PATH_RELATED), this);
        enum_mode = POS_CALC_DST;
        dest_info.Set(dest);
        hash.EnumSphere(new Sphere(dest, SEARCH_ROAD_RADIUS), RSObjectId(RS_PATH_RELATED), this);
        if (source_info.owner == null || dest_info.owner == null) return 0;

        // calc path
        RoadNode src = source_info.owner_node;
        RoadNode dst = dest_info.owner_node;
        global_started = source_info.cross;
        if (src == dst) return 0;

        RoadNode cur = dst;
        tentative_head = null;
        calc_path_counter++;

        // set dest node
        dst.SetProp(null, 0, dest_info.owner);
        dst.calc_len_counter = calc_path_counter;
        //cur->SetPermanent(true);
        cur.tent_prev = null;
        cur.tent_next = null;

        // calc path
        do
        {
            RoadNode cur_dst;
            for (int i = 0; i < cur.link_count; i++)
            {
                RoadLink lnk = cur.GetLink(i);
                cur_dst = lnk.Otherwise(cur);
                if (cur_dst.calc_len_counter != calc_path_counter)
                { // if first time
                    cur_dst.SetProp(null, ROAD_INFINITY, null);
                    cur_dst.calc_len_counter = calc_path_counter;
                    AddToTentative(cur_dst);
                }
                if (cur_dst.IsTentative() || tentative_head == cur_dst)
                {
                    float k = cur.path + lnk.Length();
                    if (k < cur_dst.path)
                        cur_dst.SetProp(cur, k, lnk);
                }
            }
            cur = null;
            float lmin = ROAD_INFINITY;
            cur_dst = tentative_head;
            while (cur_dst != null)
            {
                if (cur_dst.path < lmin)
                {
                    lmin = cur_dst.path;
                    cur = cur_dst;
                }
                cur_dst = cur_dst.tent_next;
            }
            if (cur != null)
                SubFromTentative(cur);
            else
                break;
        } while (cur != src);

        // exclude bridge and tunnels cases
        bool cur_is_gate = false;
        if (cur != null)
        {
            cur_is_gate = cur.follow_by_link.IsGate();
            if (!cur_is_gate && cur.predecessor == source_info.owner.Otherwise(cur))
                cur = cur.predecessor;
        }
        // build target path
        int cur_buf_index = 0;
        ROADPOINT cur_buf = buffer[cur_buf_index];
        RoadNode last = null;
        bool first_point_on_road = !source_info.node_cross && !cur_is_gate;
        uint first_point_on_road_uint = (uint)(first_point_on_road ? 1 : 0);
        if (cur != null && first_point_on_road)
            cur_buf = AddPoint(ref buffer, ref cur_buf_index, source_info.cross, 0, (uint)GetNavFlag(cur, null));
        while (cur != null)
        {
            RoadLink lnk = last != null ? cur.GetFollowLink(last) : null;
            ROADDATA rdata = lnk != null ? lnk.Data() : null;
            if (last != null && lnk != null && lnk.IsGate())
            {
                delta = cur.org - last.org;
                delta.y = 0;
                float t = delta.magnitude;
                if (t > 0.01f)
                    delta /= t;
                else
                    delta = Vector3.zero;
                cur_buf = AddPoint(ref buffer, ref cur_buf_index, last.org - delta * rdata.EntranceDeltaLow, 0, lnk.GetNavigationFlag()); // add point 1
                cur_buf = AddPoint(ref buffer, ref cur_buf_index, last.org + delta * rdata.EntranceDeltaHigh, rdata.EntranceHeight, lnk.GetNavigationFlag()); // add point 2
                cur_buf = AddPoint(ref buffer, ref cur_buf_index, cur.org - delta * rdata.EntranceDeltaHigh, rdata.EntranceHeight, lnk.GetNavigationFlag()); // add point 2
                cur_buf = AddPoint(ref buffer, ref cur_buf_index, cur.org + delta * rdata.EntranceDeltaLow, 0, lnk.GetNavigationFlag()); // add point 1
            }
            else
                cur_buf = AddPoint(ref buffer, ref cur_buf_index, cur.org, 0, (uint)GetNavFlag(cur, last, last != null ? false : first_point_on_road)); // add point 
            last = cur;
            cur = cur.predecessor;
        }
        if (System.Array.IndexOf(buffer, cur_buf) != 0)
        {
            // exclude bridge and tunnels cases
            if (last != null && !last.follow_by_link.IsGate() && last.predecessor == dest_info.owner.Node2())
            {
                cur_buf_index--;
                cur_buf = buffer[cur_buf_index];
            }
            if (!dest_info.node_cross && !dest_info.owner.IsGate())
            {
                cur_buf = AddPoint(ref buffer, ref cur_buf_index, dest_info.cross, 0, dest_info.owner.GetNavigationFlag());
            }
        }
        if (source_info.node_cross && cur_buf != buffer[0])
            global_started = buffer[0].Pnt;
        return System.Array.IndexOf(buffer, cur_buf);
    }


    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int CalcPath(Vector3 source, Vector3 dest, ROADPOINT[] buffer, Vector3 global_started)
    {
        throw new System.NotImplementedException();
    }

    //TODO возможно, стоит вынести в отдельный класс
    #region TRefMem emulation
    int ref_count = 1;
    //  public 
    //TRefMem() : ref_count(1) { }
    //  virtual ~TRefMem() { }
    public int Release()
    {
        Asserts.Assert(ref_count > 0);
        if ((--ref_count) == 0)
        {
            Dispose();
            return 0;
        }
        return ref_count;
    }
    public void AddRef()
    {
        ++ref_count;
    }

    #endregion
}

