using UnityEngine;
using DWORD = System.UInt32;
using static HashFlags;
using static hashtools_dll;

class CollisionModuleAPI
{
    public static ICollision CreateCl(DWORD Version, IHash h, TERRAIN_DATA td)
    {
        cl_dll_data.Open();
        //return Version == CollisionDefines.COLL_VERSION ? createObject<Collision>(h, td) : null;
        return Version == CollisionDefines.COLL_VERSION ? new StormCollision(h, td) : null;
    }
}




public class TraceLineHash : HashLineEnumer
{

    StormCollision coll;
    HMember ignored;
    public TraceResult result;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="i">ignored</param>
    /// <param name="r"></param>
    public void Init(HMember i, TraceResult r)
    {
        ignored = i;
        result = r;
        result.coll_object = null;

    }
    public TraceLineHash(StormCollision c)
    {
        coll = c;
    }

    public bool ResultOk() {
        return result.coll_object != null; }

    public override bool ProcessElement(HMember h)
    {
        if (h != ignored)
        {
            if (h.Object().MatchGroup(OF_GROUP_RENDER))
            {  // collidable && render => FPO
                try
                {
                    FPO f = (FPO)h.Object();
                    f.TraceLine(line, result);
                } catch
                {
                    Debug.Log("Skip " + h.Object());
                }
            }
            else
            if (h.Object().MatchGroup(OF_GROUP_TMT))
            { // collidable && tmt => Fpo
              // Fpo *f = (Fpo*)h->Object();

            }
        }

        return true;

    }
};
/// <summary>
/// флаги для всех функций столкновения
/// </summary>
//public class StormCollision : ICollision
//{
//    public const int MAX_COLLIDED = 1024;
//    //friend class CollideFPOSphere;
//    //friend class CollideFPOFPO;
//    TraceLineHash h_enum;

//    private TraceResult[] trace_index = new TraceResult[2];
//    private TraceResult[] tresults = new TraceResult[2];
//    CollideResult[] cresults;
//    int collide_dim;

//    int trace_counter;
//    int collide_counter;


//    void StartTrace() { trace_counter = 0; }
//    TraceInfo GetTraceInfo() { return new TraceInfo(trace_counter, trace_index); }
//    TraceResult NextTrace() { return tresults + trace_counter++; }
//    void PopTrace() { --trace_counter; }

//    private IHash hash;
//    private ProtoHash protohash;
//    private TERRAIN_DATA terr;

//    public StormCollision(IHash h, TERRAIN_DATA td)
//    {
//        h_enum = new TraceLineHash(this);
//        hash = h;
//        terr = td;
//        protohash = (ProtoHash)h;
//        cresults = new CollideResult[MAX_COLLIDED];
//        }
//    ~StormCollision() { }

//    // trace functions
//    bool TraceLineIn(Vector3 O, Vector3 D, float Dist, FPO ResObj, FPO Ignored);
//    bool TraceLineOut(Vector3 O, Vector3 D, float Dist, FPO ResObj, FPO Ignored);
//    void SortTraceInfo()
//    {
//        if (trace_counter == 2)
//        {
//            if (tresults[1].dist <= tresults[0].dist)
//            {
//                trace_index[0] = tresults + 1;
//                trace_index[1] = tresults;
//            }
//            else
//            {
//                trace_index[0] = tresults;
//                trace_index[1] = tresults + 1;
//            }
//        }
//        else
//            trace_index[0] = tresults;
//    }
//    void SortTraceInfoRevert()
//    {
//        if (trace_counter == 2)
//        {
//            if (tresults[1].dist > tresults[0].dist)
//            {
//                trace_index[0] = tresults + 1;
//                trace_index[1] = tresults;
//            }
//            else
//            {
//                trace_index[0] = tresults;
//                trace_index[1] = tresults + 1;
//            }
//        }
//        else
//            trace_index[0] = tresults;
//    }

//    // api
//    // data access
//    virtual CollisionData GetData(ObjId id);
//    // collisions and his results
//    public CollideInfo Collide(HMember who_collided, int Flags);
//    public TraceInfo TraceLine(Storm.Line line, HMember i, int Flags);
//    public CollideInfo CollideSphere(Vector3 org, float radius);
//    public bool CollideObjSphere(IHashObject obj, Vector3 o, float r);

//    public void AddRef()
//    {
//        throw new System.NotImplementedException();
//    }

//    public int RefCount()
//    {
//        throw new System.NotImplementedException();
//    }

//    public int Release()
//    {
//        throw new System.NotImplementedException();
//    }
//}