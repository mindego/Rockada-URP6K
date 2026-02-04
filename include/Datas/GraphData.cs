//using PrimitiveType = System.UInt32;
using System.Runtime.InteropServices;
//public struct Sphere
//{
//    public Vector3 o;
//    public float r;

//    public Sphere(Vector3 o, float r)
//    {
//        this.o = o;
//        this.r = r;
//    }

//    public Vector3 Org()
//    {
//        return o;
//    }
//    public float Radius()
//    {
//        return r;
//    }

//    public Sphere Operator(Vector3 p) {
//      return new Sphere(o+p, r);
//    }

//    public override string ToString()
//    {
//        return "Sphere O:" + o + " Radius: " + r;
//    }
//}

public enum GDType
{
    FGDT_STD,
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class GraphData
{
    public GDType type;
};
