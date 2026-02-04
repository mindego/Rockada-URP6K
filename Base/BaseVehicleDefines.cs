using System;

public static class BaseVehicleDefines
{
    public const uint AVOID_VEHICLE_ALL_GOOD = 0x00000000;
    public const uint AVOID_VEHICLE_ANGLE_BAD = 0x00000001;
    public const uint AVOID_VEHICLE_OBJECT_FUTURE_COLLIDE = 0x00000002;
    public const uint AVOID_VEHICLE_OBJECT_COLLIDE = 0x00000004;
    public const uint AVOID_VEHICLE_REACHED = 0x00000008;
}
public class NavOrderContainer : TLIST_ELEM<NavOrderContainer>,IDisposable
{
    INavigationOrder order;
    ROADPOINT last, lprev;
    ROADPOINT[] buffer;
    public void SetLast(ROADPOINT _last) { lprev = last; last = _last; }
    public ROADPOINT GetLast() { return last; }
    public ROADPOINT GetPrev() { return lprev; }
    public ROADPOINT[] GetBuffer() { return buffer; }
    public void Normalize()
    {
        ROADPOINT[] pnt = order.GetRouteBuffer();
        int index = GetIndex(pnt, last);
        if (index == -1) return;

        if (index == 0)
            lprev = null;
        else
            lprev = pnt[index - 1];


        //if (last == pnt[0])     // если в начале
        //    lprev = null;
        //else
        //    lprev = last - 1;
    }
    //void CalcTime(float cur_time, float dst_time); Предположительно не используется

    public int GetIndex(ROADPOINT pnt)
    {
        return GetIndex(buffer, pnt);
    }
    public int GetIndex(ROADPOINT[] buffer, ROADPOINT pnt)
    {
        for (int i=0; i<buffer.Length;i++)
        {
            if (pnt == buffer[i]) return i;
        }
        return -1;
    }
    public INavigationOrder Order() { return order; }

    private NavOrderContainer prev, next;
    public NavOrderContainer Next()
    {
        return next;
    }

    public NavOrderContainer Prev()
    {
        return prev;
    }

    public void SetNext(NavOrderContainer t)
    {
        next = t;
    }

    public void SetPrev(NavOrderContainer t)
    {
        prev = t;
    }

    public NavOrderContainer(INavigationOrder ord)
    {
        order = ord;
        order.AddRef();
        //GetLog().Message("%p order addreffed",order);
        last = order.GetRouteBuffer()[0];
        lprev = null;
        //buffer = last;
        buffer = order.GetRouteBuffer();
    }
    ~NavOrderContainer()
    {
        Dispose();
    }

    public void Dispose()
    {
        //GetLog().Message("%p order subreffer",order);
        if (order!=null) order.Release();

    }
};