using geombase;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WORD = System.UInt16;


public partial class PolyDecal
{
    public static void ClipIndexed(geombase.Plane[] planes, int num_planes,
                     Vertex[] in_v, int num_in_v, WORD[] in_i, int num_in_i,
                     ref Vertex[] out_v, ref int num_out_v, ref WORD[] out_i, ref int num_out_i)
    {
        //Vertex* v0 = (Vertex*)alloca(sizeof(Vertex) * (num_planes + 3));
        //Vertex* v1 = (Vertex*)alloca(sizeof(Vertex) * (num_planes + 3));
        Vertex[] v0 = Alloca.ANewN<Vertex>(num_planes + 3);
        Vertex[] v1 = Alloca.ANewN<Vertex>(num_planes + 3);

        num_out_v = 0;
        num_out_i = 0;

        int num;
        int tmp_in_i = 0;
        int tmp_out_i = 0;
        int tmp_out_v = 0;

        //перебираем _треугольники_ и отбрасываем отрезаемые плоскостью _вершины_
        for (; num_in_i != 0; num_in_i -= 3, tmp_in_i += 3)
        {
            Debug.LogFormat("Probing offset {0}: {1} {2} {3} index", tmp_in_i, in_i[tmp_in_i + 0], in_i[tmp_in_i + 2], in_i[tmp_in_i + 1]);
            v0[0] = in_v[in_i[tmp_in_i + 0]];
            v0[1] = in_v[in_i[tmp_in_i + 1]];
            v0[2] = in_v[in_i[tmp_in_i + 2]];
            Debug.LogFormat("Probing offset {0}: {1} {2} {3} vertex", tmp_in_i, v0[0].pos, v0[1].pos, v0[2].pos);
            num = 3;
            for (int np = 0; np < num_planes; np++)
            {
                Varr in_val = new Varr(v0, num);
                Vout out_val = new Vout(v1);
                clPlane p = new clPlane(planes[np]);
                //Clip<clPlane, Varr, Vout>(p, in_val, out_val);
                Clip(p, in_val, ref out_val);
                v1 = out_val.v;
                std.swap(ref v0, ref v1);
                Debug.LogFormat("out_val.i {0} v0:{1} v1:{2} plane[{3}/{4}] {5}", out_val.i, v0.GetHashCode().ToString("X8"), v1.GetHashCode().ToString("X8"), np + 1, num_planes, planes[np]);
                if ((num = out_val.i) == 0) break;
            }

            if (num == 0) continue;

            //copiing indices
            Debug.LogFormat(" copiing indices {0} {1}", num - 1, tmp_out_i);
            int i;
            for (i = 1; i < num - 1; ++i)
            {
                out_i[tmp_out_i + 0] = (WORD)(num_out_v);
                out_i[tmp_out_i + 1] = (WORD)(num_out_v + i);
                out_i[tmp_out_i + 2] = (WORD)(num_out_v + i + 1);
                tmp_out_i += 3;
            }
            num_out_i += (num - 2) * 3;

            //copiing vertices
            Debug.LogFormat(" copiing vertices {0}", num);
            for (i = 0; i < num; ++i) out_v[tmp_out_v + i] = v0[i];
            tmp_out_v += num;
            num_out_v += num;
        }
    }

    //TODO - Перенести в tclip.cpp
    public static void Clip(clPlane plane, Varr in_val, ref Vout out_val)
    {
        bool clipped;

        bool suz_or = false, suz_and = true;

        LinkedListNode<Vertex> v = in_val.Begin();
        LinkedListNode<Vertex> nv = null;
        LinkedListNode<Vertex> yv = null;

        Debug.LogFormat("In_val: [{0}] Out_val: [{1}]", in_val, out_val);
        int cnt = in_val.GetSize();
        for (; v != null && cnt >0; v = v.Next,cnt--)
        {
            nv = plane.suz(v.Value) ? v : nv;
            yv = !plane.suz(v.Value) ? v : yv;
        }

        if (yv == null) return;
        if (nv != null)
        {
            //string res = "";
            //foreach(var z in in_val)
            //{
            //    res += " [" + z.pos + "]";
            //}
            //Debug.Log(res);
            IteratorCicler in_cicler = new IteratorCicler(in_val.Begin(), in_val.End());
            LinkedListNode<Vertex> yv0 = nv;//yv0 - "first" ( in order of ++ ) visible 
            while (plane.suz(yv0.Value))
                in_cicler.Inc(ref yv0);

            LinkedListNode<Vertex> nv0 = yv0;    //nv0 - pre-ajacent to yv0 ("last")not-visible
            in_cicler.Dec(ref nv0);

            LinkedListNode<Vertex> nv1 = yv0;//yv0 - "first" ( in order of ++ ) not-visible 
            while (!plane.suz(nv1.Value))
                in_cicler.Inc(ref nv1);

            LinkedListNode<Vertex> yv1 = nv1;//while( plane.suz(yv1--) ); results in nv0
            in_cicler.Dec(ref yv1);


            //Debug.LogFormat("nv {0} yv0 {1} nv0 {2} nv1 {3} yv1 {4}",nv.GetHashCode().ToString("X8"),yv0.GetHashCode().ToString("X8"), nv0.GetHashCode().ToString("X8"), nv1.GetHashCode().ToString("X8"), yv1.GetHashCode().ToString("X8"));
            //Debug.LogFormat("nv {0} yv0 {1} nv0 {2} nv1 {3} yv1 {4}", nv.Value.pos, yv0.Value.pos, nv0.Value.pos, nv1.Value.pos, yv1.Value.pos);
            v = yv0;
            cnt = 0;
            //for (; v != nv1; in_cicler.Inc(ref v)) out_val.Add(v.Value);
            for (; v != nv1; in_cicler.Inc(ref v))
            {
                //Debug.Log("cnt: " + (cnt++));
                out_val.Add(v.Value);
            }

            out_val.Add(plane.Clip(yv1.Value, nv1.Value));
            out_val.Add(plane.Clip(nv0.Value, yv0.Value));
            return;
        }

        var vr = in_val.Begin();
        cnt = in_val.GetSize();
        for (; vr != null && cnt>0; vr = vr.Next,cnt--) out_val.Add(vr.Value);
    }

    class IteratorCicler
    {
        LinkedListNode<Vertex> begin;
        LinkedListNode<Vertex> end;
        public IteratorCicler(LinkedListNode<Vertex> _begin, LinkedListNode<Vertex> _end)
        {
            begin = _begin;
            end = _end;
        }
        public LinkedListNode<Vertex> Inc(ref LinkedListNode<Vertex> it)
        {
            //it = it.Next;
            //if (it == end) it = begin;
            //return it;

            it = (it == end) ? begin : it.Next;
            
            return it;
        }

        public LinkedListNode<Vertex> Dec(ref LinkedListNode<Vertex> it)
        {
            //if (it == begin) it = end;
            //it = it.Previous;
            //return it;

            it = (it == begin) ? end : it.Previous;
            
            return it;
        }
    }
    //class IteratorCicler<I>
    //{
    //    I begin,end;
    //    public IteratorCicler(I _begin, I _end)
    //    {
    //        begin = _begin;
    //        end = _end;
    //    }
    //    I Inc(I it)
    //    {
    //        ++it;
    //        if (it == end) it = begin;
    //        return it;
    //    }
    //    I Dec(I it)
    //    {
    //        if (it == begin) it = end;
    //        it--;
    //        return it;
    //    }
    //};
}

public struct Varr : IEnumerable<Vertex>
{
    //Vertex[] v;
    Vertex[] v1;
    LinkedList<Vertex> v;
    int size;

    //typedef Vertex*  iterator;


    public Varr(Vertex[] _v, int _size)
    {
        v = new LinkedList<Vertex>(_v);
        v1 = _v;
        //v = _v;
        size = _size;
    }

    //iterator Begin() { return v; }
    //iterator End() { return v + size; }
    public LinkedListNode<Vertex> Begin() { return v.First; }
    public LinkedListNode<Vertex> End() { return v.Find(v1[size - 1]); }

    public IEnumerator<Vertex> GetEnumerator()
    {
        return ((IEnumerable<Vertex>)v).GetEnumerator();
    }

    internal int GetSize()
    {
        return size;
    }

    public override string ToString()
    {
        return string.Format("Type: {0} arr_size: {1} int_size:{2}", GetType().ToString(), v.Count, size);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return v.GetEnumerator();
    }



    //class clPlane;
    //friend class clPlane;
};

public struct Vout
{
    public Vertex[] v;
    public int i;

    public Vout(Vertex[] _v)
    {
        v = _v;
        i = 0;
    }
    public void Add(Vertex V)
    {
        if (i >= v.Length) Debug.LogErrorFormat("This should not be ! i:{0} v.length:{1}", i, v.Length);

        v[i++] = V;
    }

    public override string ToString()
    {
        return string.Format("Type: {0} arr_size: {1} current i:{2}", GetType().ToString(), v.Length, i);
    }

};



public struct clPlane
{
    geombase.Plane p;
    public clPlane(geombase.Plane _p)
    {
        p = _p;
    }

    public Vertex Clip(Vertex v0, Vertex v1)
    {
        float dd0 = p.Distance(v0.pos);
        float dd1 = p.Distance(v1.pos);
        float ood = 1 / (dd0 - dd1);
        Vertex v = new Vertex();
        v.pos = (-(v0.pos * dd1) + v1.pos * dd0) * ood;
        v.norm = (-(v0.norm * dd1) + v1.norm * dd0) * ood;
        v.tc = (-(v0.tc * dd1) + v1.tc * dd0) * ood;
        v.color = new DWORDARGB(ColorOps.MedianARGB(new DWORDARGB(v0.color), -dd1 * ood, new DWORDARGB(v1.color), dd0 * ood));
        return v;
    }

    public bool suz(Vertex v)
    {
        return p.Distance(v.pos) < 0;
    }

};

public class ColorOps
{
    public static Vector4 MedianARGB(Vector4 c1, float w1, Vector4 c2, float w2)
    {
        Vector4 resVec4 = c1 * (int)(w2 * 256) + c2 * (int)(w2 * 256);
        resVec4.x = (int)resVec4.x >> 8;
        resVec4.y = (int)resVec4.y >> 8;
        resVec4.z = (int)resVec4.z >> 8;
        resVec4.w = (int)resVec4.w >> 8;

        return resVec4;
    }

    public static Vector4 MedianARGB(DWORDARGB c1, float w1, DWORDARGB c2, float w2)
    {
        return MedianARGB(new Vector4(c1.b, c1.g, c1.r, c1.a), w1, new Vector4(c1.b, c1.g, c1.r, c1.a), w2);
    }
}

