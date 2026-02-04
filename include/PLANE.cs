using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PLANE
{   // plane equation : x*n+d=0;
    public Vector3 n;
    public float d;

    float Distance(Vector3 x) { return Vector3.Dot(x, n) + d; }


    public PLANE() { }
    public PLANE(Vector3 norm, float dir)
    {
        n = norm;
        d = dir;
    }

    // x*n+d<0 : ClipVector result where x = Line.Point( t )
    public bool ClipVector(Vector3 org, Vector3 dir, ref float[] clip)
    {
        //float dd = Vector3.Dot(org, n) + d;
        float dd = Distance(org);
        float dn = Vector3.Dot(dir, n);

        bool test0 = dd + dn * clip[0] > 0; // point clipped bt plane
        bool test1 = dd + dn * clip[1] > 0; // point clipped bt plane

        //Debug.Log(string.Format("Distance: {0} test0 {1} test1 {2} ",dd,test0,test1));
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
            Debug.Log(string.Format("Clip[0] {0} Clip[1] {1} res: {2}", clip[0], clip[1], clip[0] < clip[1]));
            return clip[0] < clip[1]; // is line empty
        }
        return true;
    }
};




