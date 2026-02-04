#define RELEASE
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static RoadpointDefines;
using static TerrainDefs;
using DWORD = System.UInt32;

class TraceNode
{
    bool passable;
    bool danger;
    int calc_len_counter;
    TraceNode predecessor;
    TraceNode tent_prev, tent_next;
    float path;
    // paths
    void SetProp(TraceNode _prev, float _len) { predecessor = _prev; path = _len; }
    bool IsTentative() { return tent_next != null || tent_prev != null; }
    void ClearTentative() { tent_prev = tent_next = null; }
};

class TracePos : IComparable<TracePos>
{
    int step;
    public DWORD flags;
    // работа с флагами
    public void SetStep(int _step) { step = _step; }
    public int GetStep() { return step; }
    public void SetFlag(DWORD Flag) { flags |= Flag; }
    public void ClearFlag(DWORD Flag) { flags &= ~Flag; }
    public DWORD GetFlag(DWORD Flag) { return flags & Flag; }

    public int CompareTo(TracePos other)
    {
        return (this == other) ? 0 : 1;
    }
};

public class Navigation : INavigation
{
    const int MAX_GLOBAL_DIM = 1024;


    bool initialized;
    IDataHasher srv;
    TERRAIN_DATA trn;

    //List<NavOrder> orders = new List<NavOrder>();
    TLIST<NavOrder> orders = new TLIST<NavOrder>();

    /// <summary>
    /// A*? алгоритм построения пути по середине квадратов (64x64 юнита) карты
    /// </summary>
    /// <param name="src">Координаты начальной точки</param>
    /// <param name="dst">Координаты конечной точки </param>
    /// <param name="buffer">массив точек маршрута от начальной до конечной точки</param>
    /// <returns>количество точек в найденном маршруте. Может быть 0, если конечная точка недостижима</returns>
#if RELEASE
    int CalcLocalPath(Vector3 src, Vector3 dst, ref ROADPOINT[] buffer)
    {
        Debug.LogFormat("me started finding roadpoints from {0} to {1}", src, dst);
        Vector3 diff = dst - src;
        diff.y = 0;

        // calc norma
        float d = diff.magnitude;

        if (d > local_calc_radius)
        { //clip for distance
            diff /= d;
            d = local_calc_radius;
            diff *= d;
        }
        diff += src;

        int start_x, start_z, final_x, final_z, current_x, current_z, current_step = 1;
        TracePos current_pos;

        start_x = GetX(src.x) - trace_dim_2;
        start_z = GetY(src.z) - trace_dim_2;
        final_x = GetX(diff.x);
        final_z = GetY(diff.z);

        if (trn.GroundPass(final_x, final_z) == 0)
            return 0;
        Debug.LogFormat("me found final roadpoints passable from {0} to {1}", src, dst);
        final_x -= start_x;
        final_z -= start_z;
        current_x = trace_dim_2;
        current_z = trace_dim_2;
        if (current_x == final_x && current_z == final_z)
        {

            buffer[0].Pnt = dst;
            buffer[0].SetFlag(NAV_PLAIN);
            return 1;
        }
        Prof p = new Prof();
        p.Start();

        stack_counter = 0;
        //b_zero(trace_buffer, trace_dim * trace_dim * sizeof(TracePos)); ?? очистнка trace_buffer?
        //trace_buffer = new TracePos[trace_dim * trace_dim];
        trace_buffer = Alloca.ANewN<TracePos>(trace_dim * trace_dim);
        current_pos = Square(current_x, current_z);
        current_pos.SetStep(current_step);
        current_pos.SetFlag(NAV_VISITED);
        int i;
        do
        {
            int delta_x = 0, delta_z = 0;
            bool finded = false;
            delta_x = final_x - current_x; delta_z = final_z - current_z;
            int delta = 1;

            if (delta_x > 0) delta_x = 1;
            else if (delta_x < 0) delta_x = -1;

            if (delta_z > 0) delta_z = 1;
            else if (delta_z < 0) delta_z = -1;

            int index = CoordPair.FindPair(delta_x, delta_z);

            for (i = 0; i < 8; i++)
            {
                int cdx = CoordPair.coord_pairs[index].dx;
                int cdz = CoordPair.coord_pairs[index].dz;
                int cur_x = current_x + cdx;
                int cur_z = current_z + cdz;
                TracePos cur = Square(cur_x, cur_z);
                if (cur.GetFlag(NAV_VISITED) != 0)
                {   // уже были
                    // оптимизируем
                    if (cur.GetStep() > current_step + 1)    // если в соседний квадрат можно попасть из текущего сократив дистанцию
                        cur.SetStep(current_step + 1);
                    else if (cur.GetStep() + 1 < current_step)
                        current_pos.SetStep(cur.GetStep() + 1);  // если в нас можно попасть из соседнего сокращая дистанцию
                }
                else
                {
                    uint fl = cur.GetFlag(NAV_MOD_MASK);
                    if (fl == 0)
                    {           // никогда не были
                        if (cur_x == 0 || cur_z == 0 || cur_x == trace_dim - 1 || cur_z == trace_dim - 1)
                            cur.SetFlag(NAV_IMPOSSIBLE);
                        else
                        if (trn.GroundPass(start_x + current_x + cdx, start_z + current_z + cdz) == 0)
                            cur.SetFlag(NAV_IMPOSSIBLE);
                        else
                            cur.SetFlag(NAV_GOOD);
                    }
                    fl = cur.GetFlag(NAV_MOD_MASK);
                    if (fl == NAV_GOOD && !finded)
                    {
                        delta_x = cdx;
                        delta_z = cdz;
                        finded = true;
                    }
                }
                index = (index + delta) % 8;
                if (index < 0) index = 8 + index;
                if (delta < 0) delta = 1 - delta; else delta = -delta - 1;
            }
            if (finded)
            {
                Push(current_pos);
                current_x += delta_x;
                current_z += delta_z;
                current_step = current_pos.GetStep() + 1;
                current_pos = Square(current_x, current_z);
                current_pos.SetFlag(NAV_VISITED);
                current_pos.SetStep(current_step);
            }
            else
            {
                current_pos.SetFlag(NAV_FAILED);
                current_pos = Pop();
                if (current_pos != null)
                {
                    current_step = current_pos.GetStep();
                    //int num = current_pos - trace_buffer;
                    //int num = Array.BinarySearch(trace_buffer, current_pos);
                    int num = Array.IndexOf(trace_buffer, current_pos);
                    current_z = num / trace_dim;
                    current_x = num % trace_dim;
                }
            }
            //PrintTrace();
            if (current_x == final_x && current_z == final_z) break;
        } while (current_pos != null);

        if (current_pos == null || current_x != final_x || current_z != final_z)
        {
            p.End();
            return 0;
        }

        ROADPOINT fin_st = null;
        //fin_st = local_route;
        current_x = trace_dim_2;
        current_z = trace_dim_2;
        current_pos = Square(current_x, current_z);


        for (i = 0; i < local_route.Length; i++)
        {
            fin_st = local_route[i];
            fin_st.Pnt.x = (start_x + current_x) * SQUARE_SIZE + SQUARE_SIZE * 0.5f;
            fin_st.Pnt.z = (start_z + current_z) * SQUARE_SIZE + SQUARE_SIZE * 0.5f;

            TracePos nxt = FindMax(current_pos);
            current_pos.SetFlag(NAV_FAILED | NAV_PLAIN);
            fin_st.Flags = current_pos.flags;
            Asserts.AssertBp(fin_st.Flags != 0);
            current_pos = nxt;
            //int num = Array.BinarySearch(trace_buffer, current_pos);
            int num = Array.IndexOf(trace_buffer, current_pos);
            current_z = num / trace_dim;
            current_x = num % trace_dim;
            if (current_pos == null) break;
        }
        p.End();
        Debug.LogFormat("me finding roadpoint [{0}] from {3} to {4} in {1} {2}", (fin_st == null ? "null" : fin_st), local_route.Length, i, src, dst);
        //return Array.BinarySearch(local_route, fin_st);
        return Array.IndexOf(local_route, fin_st);
    }

#else
    int CalcLocalPath(Vector3 src, Vector3 dst, ref ROADPOINT[] buffer)
    {

        Vector3 diff = dst - src;
        Debug.Log(string.Format("CalcLocalPath src {0} dst {1} diff {2}", src, dst, diff));
        Engine.DebugLine(src, dst, Color.red, 5);
        diff.y = 0;
        Vector3 movedir = diff.normalized;
        // calc norma
        float d = diff.magnitude;

        if (d > local_calc_radius)
        { //clip for distance
            diff /= d;
            d = local_calc_radius;
            diff *= d;
        }
        diff += src;

        //if (buffer[0] == null) buffer[0] = new ROADPOINT();
        //buffer[0].Pnt = dst;
        //buffer[0].SetFlag(NAV_PLAIN);
        //return 1;

        int steps = Mathf.CeilToInt(d / SQUARE_SIZE);
        List<ROADPOINT> tmpList = new List<ROADPOINT>();
        Vector3 prev = src;
        for (int i = 0; i < steps; i++)
        {
            ROADPOINT tmp = new ROADPOINT(src + movedir * SQUARE_SIZE * i);
            tmpList.Add(tmp);
            Engine.DebugLine(prev, tmp.Pnt, Color.red, 5);
            //Engine.DebugSpere(tmp.Pnt, "sphere " + i + " " + buffer.GetHashCode().ToString("X8"), 5);
            prev = tmp.Pnt;
        }
        Debug.Log("CalcLocalPath with steps " + steps);
        buffer = tmpList.ToArray();
        return buffer.Length;


        //TODO Корректно обсчитывать локальные пути.

        //List <ROADPOINT> tmpList = new List<ROADPOINT>();
        ////tmpList.Add(new ROADPOINT(src));
        ////tmpList.Add(new ROADPOINT((src+dst)/2));
        //tmpList.Add(new ROADPOINT(dst));
        //buffer = tmpList.ToArray();
        //return buffer.Length;
        //buffer[0] = new ROADPOINT(dst);
        //buffer[0].SetFlag(NAV_PLAIN);
        //return 1;

        //Vector3 diff = dst - src;
        //diff.y = 0;

        //// calc norma
        //float d = diff.magnitude;

        //if (d > local_calc_radius)
        //{ //clip for distance
        //    diff /= d;
        //    d = local_calc_radius;
        //    diff *= d;
        //}

        //diff += src;
        //int start_x, start_z, final_x, final_z, current_x, current_z, current_step = 1;
        //TracePos current_pos;

        //start_x = GetX(src.x) - trace_dim_2;
        //start_z = GetY(src.z) - trace_dim_2;
        //final_x = GetX(diff.x);
        //final_z = GetY(diff.z);

        //if (trn.GroundPass(final_x, final_z) == 0)
        //    return 0;

        //final_x -= start_x;
        //final_z -= start_z;
        //current_x = trace_dim_2;
        //current_z = trace_dim_2;
        //if (current_x == final_x && current_z == final_z)
        //{
        //    if (buffer[0] == null) buffer[0] = new ROADPOINT();
        //    buffer[0].Pnt = dst;
        //    buffer[0].SetFlag(NAV_PLAIN);
        //    return 1;
        //}

        ////TODO восстановить профилировщик, если это необходимо.
        ////Prof p;
        ////p.Start();
        //stack_counter = 0;
        ////b_zero(trace_buffer, trace_dim * trace_dim * sizeof(TracePos));
        //trace_buffer = new TracePos[trace_dim * trace_dim];

        //current_pos = Square(current_x, current_z);
        //current_pos.SetStep(current_step);
        //current_pos.SetFlag(NAV_VISITED);
        //int i;
        //do
        //{
        //    int delta_x = 0, delta_z = 0;
        //    bool finded = false;
        //    delta_x = final_x - current_x; delta_z = final_z - current_z;
        //    int delta = 1;

        //    if (delta_x > 0) delta_x = 1;
        //    else if (delta_x < 0) delta_x = -1;

        //    if (delta_z > 0) delta_z = 1;
        //    else if (delta_z < 0) delta_z = -1;

        //    int index = CoordPair.FindPair(delta_x, delta_z);
        //                for (i = 0; i < 8; i++)
        //    {
        //        int cdx = CoordPair.coord_pairs[index].dx;
        //        int cdz = CoordPair.coord_pairs[index].dz;
        //        int cur_x = current_x + cdx;
        //        int cur_z = current_z + cdz;
        //        TracePos cur = Square(cur_x, cur_z);
        //        if (cur.GetFlag(NAV_VISITED) != 0)
        //        {   // уже были
        //            // оптимизируем
        //            if (cur.GetStep() > current_step + 1)    // если в соседний квадрат можно попасть из текущего сократив дистанцию
        //                cur.SetStep(current_step + 1);
        //            else if (cur.GetStep() + 1 < current_step)
        //                current_pos.SetStep(cur.GetStep() + 1);  // если в нас можно попасть из соседнего сокращая дистанцию
        //        }
        //        else
        //        {
        //            uint fl = cur.GetFlag(NAV_MOD_MASK);
        //            if (fl == 0)
        //            {           // никогда не были
        //                if (cur_x == 0 || cur_z == 0 || cur_x == trace_dim - 1 || cur_z == trace_dim - 1)
        //                    cur.SetFlag(NAV_IMPOSSIBLE);
        //                else
        //                if (trn.GroundPass(start_x + current_x + cdx, start_z + current_z + cdz) == 0)
        //                    cur.SetFlag(NAV_IMPOSSIBLE);
        //                else
        //                    cur.SetFlag(NAV_GOOD);
        //            }
        //            fl = cur.GetFlag(NAV_MOD_MASK);
        //            if (fl == NAV_GOOD && !finded)
        //            {
        //                delta_x = cdx;
        //                delta_z = cdz;
        //                finded = true;
        //            }
        //        }
        //        index = (index + delta) % 8;
        //        if (index < 0) index = 8 + index;
        //        if (delta < 0) delta = 1 - delta; else delta = -delta - 1;
        //    }
        //    if (finded)
        //    {
        //        Push(current_pos);
        //        current_x += delta_x;
        //        current_z += delta_z;
        //        current_step = current_pos.GetStep() + 1;
        //        current_pos = Square(current_x, current_z);
        //        current_pos.SetFlag(NAV_VISITED);
        //        current_pos.SetStep(current_step);
        //    }
        //    else
        //    {
        //        current_pos.SetFlag(NAV_FAILED);
        //        current_pos = Pop();
        //        if (current_pos != null)
        //        {
        //            current_step = current_pos.GetStep();
        //            int num = current_pos - trace_buffer[0];
        //            current_z = num / trace_dim;
        //            current_x = num % trace_dim;
        //        }
        //    }
        //    //PrintTrace();
        //    if (current_x == final_x && current_z == final_z) break;
        //} while (current_pos != null);

        //if (current_pos == null || current_x != final_x || current_z != final_z)
        //{
        //    //p.End();
        //    return 0;
        //}

        //ROADPOINT fin_st;
        ////fin_st = local_route[0];
        //current_x = trace_dim_2;
        //current_z = trace_dim_2;
        //current_pos = Square(current_x, current_z);

        //i = 0;
        //do
        //{
        //    if (local_route[i] == null) local_route[i] = new ROADPOINT();
        //    fin_st = local_route[i];

        //    fin_st.Pnt.x = (start_x + current_x) * SQUARE_SIZE + SQUARE_SIZE * 0.5f;
        //    fin_st.Pnt.z = (start_z + current_z) * SQUARE_SIZE + SQUARE_SIZE * 0.5f;
        //    TracePos nxt = FindMax(current_pos);
        //    current_pos.SetFlag(NAV_FAILED | NAV_PLAIN);
        //    fin_st.Flags = current_pos.flags;
        //    //Assert(fin_st.Flags);
        //    //fin_st++;
        //    i++;
        //    current_pos = nxt;
        //    int num = current_pos - trace_buffer;
        //    current_z = num / trace_dim;
        //    current_x = num % trace_dim;
        //} while (current_pos != null);
        ////p.End();
        //return fin_st - local_route;

    }
#endif 
    void CalcRoute(NavOrder order)
    {
        Debug.LogFormat("Started processing NavOrder " + order);
        CalcRoutes(order, order.UseGlobal());
        // if global or local path does not exists so try to calc path directly to destination
        if (global_count == 0 || local_count == 0)
        {
            global_count = 0;
            local_count = CalcLocalPath(order.Src(), order.Dst(), ref local_route);
            if (local_count == 0 && !order.UseGlobal())
                CalcRoutes(order, true);
        }
        order.RecieveRoute(local_count, local_route, global_count, global_route);
        Debug.LogFormat("Finished processing NavOrder " + order);
        //orders.Remove(order);
        orders.Sub(order);
        order.Release();
    }
    void CalcRoutes(NavOrder order, bool use_global)
    {
        Vector3 global_started = Vector3.zero; //TODO Это неправильно. Оно должно передаваться как out в CalcPath
        local_count = 0;
        // calc the global path
        global_count = use_global ? srv.CalcPath(order.Src(), order.Dst(), ref global_route, out global_started) : 0;

        Asserts.Assert(global_count < MAX_GLOBAL_DIM);

        if (global_count != 0)
        {// if global path exists 
         // try to calc local path to point where global path begin
            local_count = CalcLocalPath(order.Src(), global_started, ref local_route);

            // if local path does not exists so try to calc the path directly to destination
            if (local_count == 0)
            {
                global_count = 0;
                local_count = CalcLocalPath(order.Src(), order.Dst(), ref local_route);
            }
        }
        else
            local_count = CalcLocalPath(order.Src(), order.Dst(), ref local_route);
    }

    void Clear()
    {
        local_route = null;
        global_route = null;
        initialized = false;
        global_count = 0;
        local_count = 0;
        trace_buffer = null;
        trace_stack = null;

        // calc 2
        node_buffer = null;
        tentative_head = null;
        calc_path_counter = 1;
    }

    // calc 2 parameter
    TraceNode[] node_buffer;
    TraceNode tentative_head;
    int calc_path_counter;
    //int NodeX(TraceNode p) { return (p - node_buffer) % trace_dim; }
    //int NodeZ(TraceNode p) { return (p - node_buffer) / trace_dim; }
    int NodeX(TraceNode p) { return FindTraceNodeIndex(p) % trace_dim; }
    int NodeZ(TraceNode p) { return FindTraceNodeIndex(p) / trace_dim; }

    int FindTraceNodeIndex(TraceNode p)
    {
        for (int i = 0; i < node_buffer.Length; i++)
        {
            if (p == node_buffer[i]) return i;
        }
        return -1;
    }
    //TraceNode Node(int TX, int TZ) { return node_buffer + TZ * trace_dim + TX; }
    TraceNode Node(int TX, int TZ) { return node_buffer[TZ * trace_dim + TX]; }
    TraceNode Delta(TraceNode p, int DX, int DZ) { return node_buffer[FindTraceNodeIndex(p) + DZ * trace_dim + DX]; }
    void AddToTentative(TraceNode _nd)
    {
        throw new System.NotFiniteNumberException();
    }
    void SubFromTentative(TraceNode _nd)
    {
        throw new System.NotFiniteNumberException();
    }


    // calc parameters
    float local_calc_radius;
    float calc_time;
    int trace_dim, trace_dim_2;
    TracePos[] trace_buffer;
    TracePos[] trace_stack;
    int stack_counter;
    int GetX(float x) { return (int)((x * TerrainDefs.OO_SQUARE_SIZE - 0.5)); }
    int GetY(float y) { return (int)((y * TerrainDefs.OO_SQUARE_SIZE - 0.5)); }
    TracePos Square(int TX, int TZ) { return trace_buffer[TZ * trace_dim + TX]; }
    TracePos Delta(TracePos p, int DX, int DZ) { return trace_buffer[FindTracePosIndex(p) + DZ * trace_dim + DX]; }
    void Push(TracePos pos) { trace_stack[stack_counter++] = pos; }
    TracePos Pop() { if (stack_counter > 0) return trace_stack[--stack_counter]; return null; }
    int FindTracePosIndex(TracePos p)
    {
        for (int i = 0; i < trace_buffer.Length; i++)
        {
            if (trace_buffer[i] == p) return i;
        }
        return -1;
    }

    TracePos FindMax(TracePos p)
    {
        TracePos ret = null;
        int MaxS = -1;

        for (int i = 0; i < 8; i++)
        {
            TracePos nxt = Delta(p, CoordPair.coord_pairs[i].dx, CoordPair.coord_pairs[i].dz); // получаем следующий
            int fl = (int)nxt.GetFlag(RoadpointDefines.NAV_MOD_MASK);
            if (fl != 0)
            {                     // если были
                if (fl == RoadpointDefines.NAV_IMPOSSIBLE)     // если никак
                    p.SetFlag(RoadpointDefines.NAV_DANGER);
                else if (nxt.GetFlag(RoadpointDefines.NAV_VISITED) != 0 && nxt.GetFlag(RoadpointDefines.NAV_FAILED) == 0)
                { // если хороший квадрат
                    if (nxt.GetStep() > MaxS)
                    {
                        MaxS = nxt.GetStep();
                        ret = nxt;
                    }
                }
            }
        }
        return ret;
    }
    void PrintTrace()
    {
        //TODO Реализовать печать трассировки
    }
    // calc period  
    float calc_timer;

    // route counts and buffers
    int global_count;
    int local_count;

    ROADPOINT[] local_route;
    ROADPOINT[] global_route;

    //static string HelloWorld = "Navigation system : v.{0}.{1}  build " __TIME__ " " __DATE__;
    static string HelloWorld = "Navigation system : v.{0}.{1}  build by mindego";
    const uint NAVIGATION_VERSION = 0x00010001;
    // construct/destruct
    public Navigation(IDataHasher _srv, TERRAIN_DATA _trn, ILog _log)
    {
        srv = _srv;
        trn = _trn;
        if (_log != null)
        {
            _log.Message(HelloWorld, NAVIGATION_VERSION >> 16, (NAVIGATION_VERSION & 0x0000FFFF));
        }
        Clear();
    }
    ~Navigation()
    {
        //Пусть очищает Garbage Collector
        //orders = null;
        //NavOrder* head;
        //while (head = orders.Head())
        //{
        //    orders.Sub(head);
        //    head->Release();
        //}
        //if (local_route)
        //    delete local_route;
        //if (global_route)
        //    delete global_route;
        //if (trace_buffer)
        //    delete trace_buffer;
        //if (trace_stack)
        //    delete trace_stack;
        //// calc 2
        //if (node_buffer)
        //    delete node_buffer;
    }


    // api
    public float GetSquareSize()
    {
        return TerrainDefs.SQUARE_SIZE;
    }
    public bool SquareIsFree(Vector3 pos)
    {
        int temp_x = GetX(pos.x);
        int temp_z = GetX(pos.z);
        return trn.GroundPass(temp_x, temp_z) != 0;
    }
    /// <summary>
    /// calc navigation order 
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dst"></param>
    /// <param name="use_global"></param>
    /// <returns></returns>
    public INavigationOrder CalcOrder(Vector3 src, Vector3 dst, bool use_global)
    {
        NavOrder order = new NavOrder(src, dst, use_global);
        //orders.Add(order);
        orders.AddToTail(order);
        return order;
    }

    /// <summary>
    /// cancel navigation order 
    /// </summary>
    /// <param name=""></param>
    public void CancelOrder(INavigationOrder ord)
    {
        NavOrder order = (NavOrder)ord; // Query ???
        //orders.Remove(order);
        orders.Sub(order);
        order.Release();
    }
    public void Initialize(float _calc_time, float calc_radius)
    {
        // setup
        calc_time = _calc_time;
        local_calc_radius = calc_radius;
        trace_dim = (int)(2 * (local_calc_radius * TerrainDefs.OO_SQUARE_SIZE - 0.5));
        trace_dim_2 = trace_dim / 2;
        local_calc_radius = (trace_dim_2 - 2) * TerrainDefs.SQUARE_SIZE;
        //trace_buffer = new TracePos[trace_dim * trace_dim];
        trace_buffer = Alloca.ANewN<TracePos>(trace_dim * trace_dim);
        //trace_stack = new TracePos[trace_dim * trace_dim];
        trace_stack = Alloca.ANewN<TracePos>(trace_dim * trace_dim);
       //local_route = new ROADPOINT[trace_dim * trace_dim];
       //global_route = new ROADPOINT[MAX_GLOBAL_DIM];
        local_route = Alloca.ANewN<ROADPOINT>(trace_dim * trace_dim);
        global_route = Alloca.ANewN<ROADPOINT>(MAX_GLOBAL_DIM);
        calc_timer = calc_time;
        initialized = true;

        // calc 2
        node_buffer = new TraceNode[trace_dim * trace_dim];
    }
    public void Update(float scale)
    {
        calc_timer -= scale;
        if (calc_timer <= 0)
        {
            //Debug.LogFormat("Updating navigation of {0} orders",orders.Counter());
            //if (orders.Count == 0)
            //{
            //    calc_timer = calc_time;
            //    return;
            //}

            //NavOrder order = orders[0];
            NavOrder order = orders.Head();
            if (order != null)
                CalcRoute(order);
            calc_timer = calc_time;
        }
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

//public class NavigationFake : INavigation
//{
//    private IDataHasher srv;
//    private TERRAIN_DATA trn;
//    private ILog log;

//    public NavigationFake(IDataHasher srv, TERRAIN_DATA trn, ILog log)
//    {
//        this.srv = srv;
//        this.trn = trn;
//        this.log = log;
//    }

//    public void AddRef()
//    {
//        throw new System.NotImplementedException();
//    }

//    public INavigationOrder CalcOrder(Vector3 src, Vector3 dst, bool use_global)
//    {
//        NavOrder order = new NavOrder(src, dst, use_global);
//        return order;

//    }

//    public void CancelOrder(INavigationOrder ino)
//    {
//        throw new System.NotImplementedException();
//    }

//    public float GetSquareSize()
//    {

//        return TerrainDefs.SQUARE_SIZE;
//    }

//    public void Initialize(float calc_time, float local_calc_radius)
//    {
//        return;
//    }

//    public int RefCount()
//    {
//        throw new System.NotImplementedException();
//    }

//    public int Release()
//    {
//        throw new System.NotImplementedException();
//    }

//    public bool SquareIsFree(Vector3 pos)
//    {
//        int temp_x = GetX(pos.x);
//        int temp_z = GetX(pos.z);
//        return trn.GroundPass(temp_x, temp_z) != 0;
//    }

//    public void Update(float scale)
//    {
//        throw System.NotImplementedException();
//    }

//    int GetX(float x) { return (int)((x * TerrainDefs.OO_SQUARE_SIZE - 0.5)); }
//    int GetY(float y) { return (int)((y * TerrainDefs.OO_SQUARE_SIZE - 0.5)); }
//}

// координатная пара
struct CoordPair
{
    public static CoordPair[] coord_pairs = new CoordPair[8]
    {
          new CoordPair(0  ,1  ,1f),
          new CoordPair(1  ,1  ,1f),
          new CoordPair(1  ,0  ,1f),
          new CoordPair(1  ,-1 ,1f),
          new CoordPair(0  ,-1 ,1f),
          new CoordPair(-1 ,-1 ,1f),
          new CoordPair(-1 ,0  ,1f),
          new CoordPair(-1 ,1  ,1f)
    };

    public int dx, dz;
    float len;
    public CoordPair(int _dx, int _dz, float _len)
    {
        dx = _dx;
        dz = _dz;
        len = _len;
    }
    public static int FindPair(int dx, int dz)
    {
        for (int i = 0; i < 8; i++)
        {
            if (coord_pairs[i].dx == dx && coord_pairs[i].dz == dz)
                return i;
        }
        return -1;
    }
};