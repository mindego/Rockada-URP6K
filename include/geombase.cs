using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace geombase
{
    public static class Inline
    {
        public static bool IsIntersected(Sphere s1, Sphere s2)
        {
            return (s1.o - s2.o).sqrMagnitude <= Mathf.Pow(s1.r + s2.r, 2);
        }

        // p - half_space
        public static bool IsIntersected(Sphere s, Plane p)
        {
            return p.Distance(s.o) <= s.r;
        }
    }

    /// <summary>
    /// Sphere
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Sphere
    {
        public Vector3 o;
        public float r;

        public Sphere()
        {
            o = Vector3.zero;
            r = 0;
        }
        public Sphere(Vector3 o, float r)
        {
            this.o = o;
            this.r = r;
        }

        public Vector3 Org()
        {
            return o;
        }
        public float Radius()
        {
            return r;
        }

        public Sphere Operator(Vector3 p)
        {
            return new Sphere(o + p, r);
        }

        public override string ToString()
        {
            return "Sphere O:" + o + " Radius: " + r;
        }

        public static Sphere SphereUnion(Sphere s1, Sphere s2)
        {
            Vector3 dif = s1.o - s2.o;
            float dist = dif.magnitude;

            if (dist + s1.r <= s2.r) return s2;
            if (dist + s2.r <= s1.r) return s1;

            float radius = (dist + s1.r + s2.r) * 0.5f;
            Vector3 center = s2.o + dif * ((radius - s2.r) / dist);

            return new Sphere(center, radius);
        }
    }

    /// <summary>
    /// Plane with equation n* x+d=0
    /// </summary>
    public struct Plane
    {
        public const int SIZE = 3 * 4 + 4;
        public Vector3 n;
        public float d;

        public float Distance(Vector3 x)
        {
            //t       operator *(Rv v) const { return x * v.x + y * v.y + z * v.z; }

            return Vector3.Dot(n, x) + d;
            //    return x * n + d; 
        }

        // normal look outside body
        public bool Positive(Vector3 x) { return Distance(x) < 0; }

        public Plane(bool z = false)
        {
            n = Vector3.up;
            d = 0;
        }
        public Plane(Vector3 norm, float dist)
        {
            n = norm;
            d = dist;
        }
        public Plane(Vector3 norm, Vector3 point)
        {
            n = norm;
            d = -(Vector3.Dot(norm, point));
            //d = -(norm * point);
        }

        //  ClipVector result positive points
        //bool ClipVector(Vector3 org, Vector3 dir, float clipped);
        //operator Vector4f&() { return *(Vector4f*) this; }
    };
    /// <summary>
    /// Ray
    /// </summary>
    public struct Ray
    {
        public Vector3 org;
        public Vector3 dir;

        public Ray(bool z = false) : this(Vector3.zero, Vector3.forward) { }
        public Ray(Vector3 o, Vector3 d)
        {
            org = o;
            dir = d;
        }

        Vector3 Point(float x) { return org + dir * x; }

        float IntersectPlane(Plane p) { return -p.Distance(org) / (Vector3.Dot(p.n, dir)); }
    };

    ///// <summary>
    ///// Line
    ///// </summary>
    //public struct Line
    //{
    //    public Vector3 org;
    //    public Vector3 dir;
    //    public float dist;

    //    public Line(bool z = false)
    //    {
    //        org = Vector3.zero;
    //        dir = Vector3.forward;
    //        dist = 0;
    //    }
    //    public Line(Vector3 o, Vector3 d, float dst)
    //    {
    //        org = o;
    //        dir = d;
    //        dist = dst;
    //    }
    //    public Line(Ray r, float dst)
    //    {
    //        org = r.org;
    //        dir = r.dir;
    //        dist = dst;
    //    }

    //    void Set(Vector3 o, Vector3 d, float dst) { org = o; dir = d; dist = dst; }

    //    public override string ToString()
    //    {
    //        return GetType().ToString() + string.Format("Org {0} Dir {1} dst {2}", org, dir, dist);
    //    }
    //}
    public struct Rect
    {
        public int x0, y0;
        public int x1, y1;

        public override string ToString()
        {
            return base.ToString() + String.Format("[0 ({0}:{1})] [1 ({2}:{3})]", x0, y0, x1, y1);
        }
        public Rect(bool z = false) : this(0, 0, 0, 0)
        {

        }
        //Rect(int _x0, int _y0, int _x1, int _y1) : x0(_x0), y0(_y0), x1(_x1), y1(_y1) { }

        public Rect(int _x0, int _y0, int _x1, int _y1)
        {
            x0 = _x0;
            y0 = _y0;
            x1 = _x1;
            y1 = _y1;
        }

        int Max(int x, int y) { return x > y ? x : y; }
        int Min(int x, int y) { return x < y ? x : y; }

        public bool In(int x, int y) { return (x0 <= x) && (x <= x1) && (y0 <= y) && (y <= y1); }
        public void Invalidate() { x0 = 32768; x1 = -32768; }
        public bool Valid() { return x0 <= x1 && y0 <= y1; }

        public override bool Equals(object obj)
        {
            return obj is Rect rect &&
                   x0 == rect.x0 &&
                   y0 == rect.y0 &&
                   x1 == rect.x1 &&
                   y1 == rect.y1;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x0, y0, x1, y1);
        }

        public static Rect operator &(Rect l, Rect r) { return new Rect(l.Max(l.x0, r.x0), l.Max(l.y0, r.y0), l.Min(l.x1, r.x1), l.Min(l.y1, r.y1)); }

        public static Rect operator |(Rect l, Rect r)
        {
            return new Rect(l.Min(l.x0, r.x0), l.Min(l.y0, r.y0), l.Max(l.x1, r.x1), l.Max(l.y1, r.y1));
        }

        public static bool operator ==(Rect l, Rect r)
        {
            return r.x0 == l.x0 && r.y0 == l.y0 && r.x1 == l.x1 && r.y1 == l.y1;
        }

        public static bool operator !=(Rect l, Rect r)
        {
            //return r.x0 != x0 || r.y0 != y0 || r.x1 != x1 || r.y1 != y1;
            return !(l == r);
        }

        public static bool operator >=(Rect l, Rect r)
        {
            return r.x0 >= l.x0 && l.x1 >= r.x1 && r.y0 >= l.y0 && l.y1 >= r.y1;
        }

        public static bool operator <=(Rect l, Rect r)
        {
            return l.x0 >= r.x0 && r.x1 >= l.x1 && l.y0 >= r.y0 && r.y1 >= l.y1;
        }

    }
}
