using UnityEngine;

namespace Geometry
{
    /*===========================================================================*\
    |  Ray, Line                                                                  |
    \*===========================================================================*/
    public class Ray
    {
        public Vector3 org;
        public Vector3 dir;

        public Ray() { }
        public Ray(Vector3 o, Vector3 d)
        {
            org = o;
            dir = d;
        }

        public Ray(Ray r)
        {
            org = r.org;
            dir = r.dir;
        }

        Vector3 Point(float x) { return org + dir * x; }
        public float IntersectPlane(Storm.Plane p) { return -p.Distance(org) / (Vector3.Dot(p.n, dir)); }
    }


    public class Line : Ray
    {
        public float dist;

        public override string ToString()
        {
            //string res = string.Format("Line from {0} dir {1} length {2}",org,dir,dist);
            string res = string.Format("Line from {0} to {1} length {2}", org, org+dir*dist, dist);
            return res;
        }
        public Line() { }
        public Line(Line orig) : base(orig)
        {
            dist = orig.dist;
        }
        public Line(Vector3 o, Vector3 d, float dst) : base(o, d)
        {
            dist = dst;
        }
        public Line(Ray r, float dst) : base(r)
        {
            dist = dst;
        }

        public void Set(Vector3 o, Vector3 d, float dst) { org = o; dir = d; dist = dst; }
    }

    /*===========================================================================*\
    |  class SphereRayIntersect compute intersection between a ray and a sphere   |
    \*===========================================================================*/

    public class SphereRayIntersect
    {
        float a, b, c, d;
        public SphereRayIntersect(geombase.Sphere s, Ray r)
        {
            Vector3 dif = r.org - s.o;
            a = r.dir.sqrMagnitude;
            b = Vector3.Dot(r.dir, dif) * 2;
            c = dif.sqrMagnitude - s.r * s.r;
            d = b * b - 4 * a * c;
        }

        bool IsIntersected() { return d >= 0; }

        Vector2 GetSolution()
        {
            double div = .5f / a;
            double d_s = Mathf.Sqrt(d);
            return new Vector2((float) ((-b - d_s) * div), (float)((-b + d_s) * div));
        }
    }
};



namespace Storm
{
    public class Plane
    {
        /// <summary>
        /// нормаль плоскости
        /// </summary>
        public Vector3 n;
        /// <summary>
        /// Расстояние?
        /// </summary>
        public float d;

        public float Distance(Vector3 x) { return Vector3.Dot(x, n) + d; }

        // normal look outside body
        public bool Positive(Vector3 x) { return Distance(x) < 0; }

        public Plane() { }
        public Plane(Vector3 norm, float dist)
        {
            n = norm;
            d = dist;
        }
        public Plane(Vector3 norm, Vector3 point) : this(norm, -(Vector3.Dot(norm, point)))
        {

        }

        //  ClipVector result positive points
        public bool ClipVector(Vector3 org, Vector3 dir, float[] clip)
        {
            float dd = Vector3.Dot(org, n) + d;
            float dn = Vector3.Dot(dir, n);

            bool test0 = dd + dn * clip[0] > 0; // point clipped bt plane
            bool test1 = dd + dn * clip[1] > 0; // point clipped bt plane

            if (test0 || test1)
            {
                float param = -dd / dn; // cross parameter

                if (test0)
                {
                    if (param > clip[0])
                        clip[0] = param;
                    else
                        return false;
                }

                if (test1)
                {
                    if (param < clip[1])
                        clip[1] = param;
                    else
                        return false;
                }
                return clip[0] < clip[1]; // is line empty
            }
            return true;

        }
        //operator Vector4f&() { return *(Vector4f*)this;}
    }

}

