using geombase;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
/// <summary>
/// Collision
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class CollisionData : IStormImportable<CollisionData>
{
    public const uint CDF_DEFAULT = 0x00000001;

    Flags flags;
    public int n_planes;   // total with spheres
    public int n_groups;
    int reserved;

    public Sphere b_sphere;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    byte[] data;   // spheres, planes ( first plane = box sphere )

    int GetPlanesSize() { return n_planes * Plane.SIZE; }
    int GetGroupsSize() { return n_groups * sizeof(short); }

    public Plane[] GetPlanes()
    {
        //return (Plane)(data); 
        return StormFileUtils.ReadStructs<Plane>(data, 0, n_planes);
    }
    public short[] GetGroups()
    {
        //return (short*)(data + GetPlanesSize());
        return StormFileUtils.ReadStructs<short>(data, GetPlanesSize(), n_groups);
    }

    int GetSize()
    {
        throw new System.NotImplementedException();
        //return sizeof(CollisionData) + GetPlanesSize() + GetGroupsSize();
    }

    public CollisionData Import(Stream st)
    {
        CollisionData cd = StormFileUtils.ReadStruct<CollisionData>(st);
        List<byte> myData = new List<byte>();
        while (st.Position < st.Length)
        {
            myData.Add(StormFileUtils.ReadStruct<byte>(st, st.Position));
        }
        cd.data = myData.ToArray();
        return cd;
    }
}