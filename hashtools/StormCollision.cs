using Geometry;
using UnityEngine;
using static CollisionDefines;
using static HashFlags;
using static hashtools_dll;
using static RoFlags;

/// <summary>
/// флаги для всех функций столкновения
/// </summary>
public class StormCollision : ICollision
{
    const int MAX_COLLIDED = 1024;
    private IHash hash;
    private ProtoHash protohash;
    private TERRAIN_DATA terr;
    TraceLineHash h_enum;
    TraceResult[] trace_index = new TraceResult[2]; // тут должны быть указатели (или индексы)
    TraceResult[] tresults = new TraceResult[2];
    public CollideResult[] cresults;// тут должны быть указатели
    int collide_dim;

    int trace_counter;
    public int collide_counter;
    public StormCollision(IHash h, TERRAIN_DATA td)
    {
        h_enum = new TraceLineHash(this);
        hash = h;
        terr = td;
        protohash = (ProtoHash)h;

        tresults=Alloca.ANewN<TraceResult>(tresults.Length);
        cresults=Alloca.ANewN<CollideResult>(MAX_COLLIDED);
        //for (int i = 0; i < tresults.Length; i++)
        //{
        //    tresults[i] = new TraceResult();
        //}

    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public CollideInfo Collide(HMember who_collided, int Flags = 31)
    {
        throw new System.NotImplementedException();
    }

    public bool CollideObjSphere(IHashObject obj, Vector3 o, float r = 0)
    {
        throw new System.NotImplementedException();
    }

    public CollideInfo CollideSphere(Vector3 org, float radius)
    {
        collide_counter = 0;
        CollideFPOSphere c = new CollideFPOSphere(this, org, radius);
        hash.EnumSphere(new geombase.Sphere(org, radius), ROObjectId(ROFID_FPO), c);
        return new CollideInfo(collide_counter, cresults);

    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }
    void StartTrace() { trace_counter = 0; }
    TraceInfo GetTraceInfo() { return new TraceInfo(trace_counter, trace_index); }
    TraceResult NextTrace() { return tresults[trace_counter++]; }
    void PopTrace() { --trace_counter; }

    public TraceInfo TraceLine(Line line, HMember ignored = null, int Flags = (int)COLLF_ALL)
    {
        StartTrace();

        Line l = new Line();
        Line pl = ((Flags & COLLF_FIRST) != 0) ? l : line; //в исходниках pl - указатель.
        TraceResult r = NextTrace(); //И это указатель
        bool needpop = true;


        if ((Flags & COLLF_FIRST) != 0)
        {
            pl = line;
            l = line;
        }


        if ((Flags & (COLLF_WITH_OBJECT | COLLF_WITH_OBJECT2)) != 0)
        {

            uint hFlag =
                (((Flags & COLLF_WITH_OBJECT) != 0) ? OF_GROUP_COLLIDABLE : 0) |
                (((Flags & COLLF_WITH_OBJECT2) != 0) ? OF_GROUP_COLLIDABLE2 : 0);
            h_enum.Init(ignored, r);
            hash.EnumLine(pl, (hFlag | OF_USER_MASK), h_enum);

            if (h_enum.ResultOk())
            {
                //Debug.Log("Hit Object: " + r.coll_object);
                if ((Flags & COLLF_FIRST) != 0)
                {
                    needpop = false;
                    l.dist = h_enum.result.dist;
                }
                else
                {
                    r.org = line.org + line.dir * r.dist;
                    r = NextTrace();
                }
            }
        }

        if ((Flags & COLLF_WITH_GROUND) != 0)
        {
            bool myTrace = terr.TraceLine(pl, ref r);

            //if (terr.TraceLine(pl, r))
            if (myTrace)
            {
                //Debug.Log("Hit terrain? " + myTrace + " results: " + r);
                if ((Flags & COLLF_FIRST) != 0)
                {
                    needpop = false;
                }
                else
                {
                    r.org = line.org + line.dir * r.dist;
                    tresults[trace_counter] = r;
                    r = NextTrace();

                }
            }
        }
        if (needpop)
            PopTrace();
        else
            r.org = line.org + line.dir * r.dist;

        //tresults[trace_counter] = r;
        if (trace_counter != 0)
            SortTraceInfo();

        //EngineDebug.DebugLine(line.org, line.org + line.dir * line.dist, trace_counter == 0 ? Color.green : Color.red, 1);
        return GetTraceInfo();
    }
    void SortTraceInfo()
    {
        //Debug.Log(string.Format("Sorting results: count: {0}\n1:{1}\n2:{2}\n3:{3}\n4:{4}",trace_counter, trace_index[0], trace_index[1], tresults[0], tresults[1]));
        if (trace_counter == 2)
        {
            if (tresults[1].dist <= tresults[0].dist)
            {
                trace_index[0] = tresults[1];
                trace_index[1] = tresults[0];
            }
            else
            {
                trace_index[0] = tresults[0];
                trace_index[1] = tresults[1];
            }
        }
        else
        {
            trace_index[0] = tresults[0];
            //trace_index[0] = tresults[1];
        }
        //Debug.Log(string.Format("Sorted results: count: {0}\n1:{1}\n2:{2}\n3:{3}\n4:{4}", trace_counter, trace_index[0], trace_index[1], tresults[0], tresults[1]));
        //for (int i = 0; i < trace_counter; i++) {
        //    Engine.DebugSphere(tresults[i].org, string.Format("Hit [{0}] {1}",i,tresults[i].coll_object == null? "ground" : tresults[i].coll_object.TextName),10);
        //}

        /*  float mindist=-FLT_MAX;
  int   nsorted=0;
  for ( int i=0; i<nCollided ;++i ) {
    int curi;
    int curd=FLT_MAX;
    for ( int j=0; j<nCollided ;++j ) {
      if ( TraceInfo[j].Dist < curd ) {
        if ( TraceInfo[j].Dist > mindist ) {
          curi=j;
          curd=TraceInfo[j].Dist;
        }
      }
    }
    TraceInfoIndex[i]=TraceInfo+curi;
    mindist=curd;
  }*/
    }

    public CollisionData GetData(ObjId id)
    {
        return cl_dll_data.GetClData(id);
    }
}

public class CollideFPOSphere : HashEnumer
{
    StormCollision coll;
    readonly Vector3 Org;
    readonly float r;
    public CollideFPOSphere(StormCollision c, Vector3 o, float sr)
    {
        coll = c;
        Org = o;
        r = sr;
    }
    public bool ProcessElement(HMember hm)
    {
        FPO o = (FPO)(hm.Object());
        coll.collide_counter += o.CheckSphereCollision(Org, r, coll.cresults,coll.collide_counter);
        return true;
    }
};