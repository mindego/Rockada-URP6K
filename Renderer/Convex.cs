using UnityEngine;
using VertexCont = System.Collections.Generic.List<UnityEngine.Vector2>;
using Seg = System.ValueTuple<UnityEngine.Vector2, UnityEngine.Vector2>;

public class Convex
{
    public Convex()
    {
        m_Radius = 0;
    }

    delegate float Sfunc(Vector2 p0, Vector2 p1, Vector2 p2);
    delegate bool Spred(float lhs,float rhs);

    private bool StdLess(float lhs,float hrs) {
        return (lhs < hrs);
    }
    /// <summary>
    /// [it.second,last)
    /// evaluates it.second while !=last keeps it.first previous value to it.second
    /// *returns it for which _func( *it.first, *it.second, X ) is minimal in sence of _pred
    /// (first it for which !_pred( _func( *I.first, *I.second, X ),_func( *it.first, *it.second, X ) ) for any I )
    /// </summary>
    /// <param name="it"></param>
    /// <param name="last"></param>
    /// <param name="X"></param>
    /// <param name="_func"></param>
    /// <param name="_pred"></param>
    /// <returns></returns>
    Seg find_min(Seg it, Vector2 last, Vector2 X, Sfunc _func, Spred _pred = null)
    {
        //if (_pred == null) _pred = StdLess; //аналог конструктора Spred &_pred=std::less<Sval> 
        //Seg found = it;
        //float found_val = _func(it.Item1, it.Item2, X);
        //it.Item1 = it.Item2;
        //++it.Item2;
        //for (; last != it.Item2; it.Item1 = it.Item2, ++it.second)
        //{
        //    float val = _func(it.Item1, it.Item2, X);
        //    if (_pred(val, found_val))
        //    {
        //        found = it;
        //        found_val = val;
        //    }
        //}
        //return found;
        return it;
    }
    public void insert(Vector2 X)
    {
        //if (2 < m_VertexList.Count)
        //{

        //    int lastIndex = m_VertexList.Count - 1;
        //    Seg sbegin = (m_VertexList[lastIndex - 1], m_VertexList[0]);


        //    //triangle (smin.first,smin.second,X) is of min square
        //    Seg smin = find_min(
        //        new Seg(m_VertexList[lastIndex - 2], m_VertexList[0]),
        //        m_VertexList[lastIndex],
        //        X,
        //        Value3,
        //        StdLess
        //        );

        //    //square of (smin.first,smin.second,X) triangle
        //    float min_square = Value3(smin.Item1, smin.Item2, X);
        //    //does the X vertex expands polygon's square?
        //    bool expands_square = min_square < 0;

        //    //does the X vertex expands polygon's radius?
        //    bool expands_radius = UpdateRadius(X);

        //    if (!(expands_square || expands_radius)) return;

        //    if (!expands_square && expands_radius)
        //    {
        //        m_VertexList.Insert(smin.second, X);
        //        return;
        //    }

        //    //triangle (smax.first,smax.second,X) is of max square
        //    Seg smax = find_min(
        //      Seg(prev(m_VertexList.end()), m_VertexList.begin()), m_VertexList.end(),
        //      X,
        //      Value3,
        //      std::greater_equal<Sval>());

        //    //look for the first ( after smax ) neg square tri
        //    Seg negf = lower_bound(smax, m_VertexList.end(), X, 0, Value3, std::less<Sval>());
        //    if (m_VertexList.end() == negf.second)
        //        negf = lower_bound(sbegin, smax.second, X, 0, Value3, std::less<Sval>());
        //    float negf_square = Value3(*negf.first, *negf.second, X);

        //    //look for the first ( after negf ) pos square tri
        //    Seg posf = lower_bound(negf, m_VertexList.end(), X, 0, Value3, std::greater_equal<Sval>());
        //    if (m_VertexList.end() == posf.second)
        //        posf = lower_bound(sbegin, negf.second, X, 0, Value3, std::greater_equal<Sval>());
        //    float posf_square = Value3(*posf.first, *posf.second, X);

        //    //removing [negf.second,posf.first)
        //    for (VertexCont::iterator it = negf.second; posf.first != it;)
        //    {
        //        it = m_VertexList.erase(it);
        //        if (m_VertexList.end() == it) it = m_VertexList.begin();
        //    }

        //    m_VertexList.Insert(posf.first, X);

        //    return;
        //}

        //if (2 == m_VertexList.Count)
        //{

        //    UpdateRadius(X);

        //    var it0 = m_VertexList[0];
        //    var it1 = m_VertexList[1];
        //    //m_VertexList.Add(
        //    //  IsCW(it0, it1, X) ? it0 : it1, X);

        //    m_VertexList.Insert(IsCW(it0, it1, X) ? 1 : 0, X);
        //    return;
        //}

        //if (1 == m_VertexList.Count)
        //{
        //    CalcRadius(X);
        //}

        //m_VertexList.Add(X);
    }

    public int size() { return m_VertexList.Count; }


    /// <summary>
    /// updates radius of the poly with respect of X
    /// </summary>
    /// <param name="X"></param>
    /// <returns>true if radius is expanded</returns>
    bool UpdateRadius(Vector2 X)
    {
        float r = CalcRadius(X);
        bool expands_radius = r > m_Radius;
        m_Radius = expands_radius ? r : m_Radius;
        return expands_radius;
    }
    /// <summary>
    /// calculate distance from cur poly to X
    /// </summary>
    /// <param name="X"></param>
    /// <returns></returns>
    float CalcRadius(Vector2 X)
    {
        float maxr = 0;
        foreach (Vector2 it in m_VertexList)
        {
            float r = Value2(it, X);
            if (r > maxr) maxr = r;
        }
        return maxr;
    }

    float Value2(Vector2 p0, Vector2 p1)
    {
        return (p0 - p1).magnitude;
    }

    float Value3(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        Vector2 d10 = (p1 - p0);
        Vector2 d20 = (p2 - p0);

        return
          -(d10.y * d20.x -
          d10.x * d20.y);
    }

    bool IsCW(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        Vector2 d10 = (p1 - p0);
        Vector2 d20 = (p2 - p0);

        float d =
          d10.y * d20.x -
          d10.x * d20.y;

        return d < 0;
    }

    bool IsCCW(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        return !IsCW(p0, p1, p2);
    }
    VertexCont m_VertexList;
    float m_Radius;
}