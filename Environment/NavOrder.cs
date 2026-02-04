using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class NavOrder : INavigationOrder,TLIST_ELEM<NavOrder>,IDisposable
{
    NavigationOrderState state;
    Vector3 src, dst;
    int route_dim;
    bool use_global;
    ROADPOINT[] route_buffer;

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine(this.GetType().ToString());
        sb.AppendFormat("{0}->{1}\n", src, dst);
        sb.AppendLine("route_dim: " + route_dim + " route_buffer size: " + (route_buffer!=null? route_buffer.Length:"null"));
        if (route_buffer!=null &&  route_buffer.Length > 0 )
        {
            foreach ( ROADPOINT p in route_buffer ) { sb.AppendLine("\t" + (p!=null ? p.ToString():"null" ));}
        }
        sb.AppendLine("State: " + state);

        return sb.ToString();
    }
    // fetch
    public void RecieveRoute(int local_dim, ROADPOINT[] local_buf, int global_dim, ROADPOINT[] global_buf)
    {
        if (state != NavigationOrderState.Ready)
        {
            int dim = local_dim + global_dim + 1;  // count summary
            route_dim = dim - 1;
            // copy data to internal buffer
            route_buffer = new ROADPOINT[dim];
            if (local_dim != 0) //MemCpy(route_buffer, local_buf, sizeof(ROADPOINT) * local_dim);
                Array.Copy(local_buf, route_buffer, local_dim);
            if (global_dim != 0) //MemCpy(route_buffer + local_dim, global_buf, sizeof(ROADPOINT) * global_dim);
                Array.Copy(global_buf, 0, route_buffer, local_dim, global_dim);
            state = NavigationOrderState.Ready;
        }
        //VisualizeRoute();
    }

    private void VisualizeRoute()
    {
        int cnt = route_buffer.Length;
        //Debug.Log("Drawing route: " + cnt + " ROADPOINTs");
        for (int i = 0; i < route_buffer.Length - 1; i++)
        {
            if (route_buffer[i] == null )
            {
                Debug.LogError("NULL roadpoint " + i);
                continue;
            }
                if (route_buffer[i + 1] == null)
            {
                Debug.LogError("NULL roadpoint " + (i+1));
                continue;
            }
            if (route_buffer[i].Pnt == null || route_buffer[i+1].Pnt == null) continue;
            Debug.LogFormat("Draw from {0} to {1}", route_buffer[i].Pnt, route_buffer[i+1].Pnt);
            EngineDebug.DebugLine(route_buffer[i].Pnt, route_buffer[i+1].Pnt, Color.white,5);
        }
    }
    public Vector3 Src() { return src; }
    public Vector3 Dst() { return dst; }
    public bool UseGlobal() { return use_global; }

    // construct
    public NavOrder(Vector3 _src, Vector3 _dst, bool _use_global)
    {
        src = _src;
        dst = _dst;
        state = NavigationOrderState.Waiting;
        route_dim = 0;
        route_buffer = null;
        use_global = _use_global;
    }
    ~NavOrder()
    {
        if (route_buffer != null)
            route_buffer = null;

    }

    // api
    public virtual NavigationOrderState GetState()
    {
        return state;
    }
    public virtual int GetRouteDimension()
    {
        if (state != NavigationOrderState.Ready) return 0;
        return route_dim;

    }
    public virtual ROADPOINT[] GetRouteBuffer()
    {
        return route_buffer;
    }

    public void AddRef()
    {
        return;
    }

    public int RefCount()
    {
        return 0;
    }

    public int Release()
    {
        return 0;
    }

    private NavOrder next, prev;
    public NavOrder Next()
    {
        return next;
    }

    public NavOrder Prev()
    {
        return prev;
    }

    public void SetNext(NavOrder t)
    {
        next = t;
    }

    public void SetPrev(NavOrder t)
    {
        prev = t;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
