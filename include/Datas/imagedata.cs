using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor.PackageManager;
using UnityEngine;
using static D3DEMULATION;
using crc32 = System.UInt32;
using DWORD = System.UInt32;
using WORD = System.UInt16;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class MaterialData : ISizeEvaluator
{         // datafile entry
    public Flags flags;
    public crc32 bmp_id;
    public crc32 mtl_id;
    //    union {
    //float parameter;
    //    crc32 bump_id;
    //    crc32 envmap_id;
    //};
    #region эмуляция union
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    byte[] varId = new byte[4];

    public float parameter
    {
        get
        {
            return BitConverter.ToSingle(varId, 0);
        }
    }

    public crc32 bump_id
    {
        get
        {
            return BitConverter.ToUInt32(varId, 0);
        }
    }

    public crc32 envmap_id
    {
        get
        {
            return BitConverter.ToUInt32(varId, 0);
        }
    }
    #endregion
    public static int GetSize()
    {
        //sizeof(flags) + sizeof(bmp_id) + sizeof(mtl_id) + sizeof(varId);
        //return 4 + 4 + 4 + 4;
        //return Marshal.SizeOf(typeof(MaterialData));
        return Marshal.SizeOf<MaterialData>();
    }

    public override string ToString()
    {
        string res = "";

        res += "Flags: " + flags + "\n";
        res += "bmp_id: " + bmp_id.ToString("X8") + "\n";
        res += "mtl_id: " + mtl_id.ToString("X8") + "\n";
        res += "Union parameter: " + parameter + "\n";
        res += "Union bump_id: " + bump_id.ToString("X8") + "\n";
        res += "Union envmap_id: " + envmap_id.ToString("X8") + "\n";
        return res;
    }
};

/// <summary>
/// Группа вершин
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertGroup : ISizeEvaluator
{
    public DWORD FVF;
    public int num;
    public crc32 Vertices;
    int GetVertDataSize()
    {
        //Assert(FVF == D3DFVF_VERTEX);
        return 32 * num;
    }

    public override string ToString()
    {
        string res = "";
        res += "FVF: " + FVF + "\n";
        res += "Num: " + num + "\n";
        res += "Vertices: " + Vertices;

        return res;
    }
    //void* NextPoints(void* p) { return ((char*)p) + GetVertDataSize(); };

    public static int GetSize()
    {
        return Marshal.SizeOf<VertGroup>();
    }
}

/// <summary>
/// группа сторон?
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FacetGroup
{
    public D3DPRIMITIVETYPE prim_type;

    public WORD mtl_number;

    public WORD vertgroup_number;
    public WORD start_vert;
    public WORD n_verts;

    private WORD n_indices;
    private WORD unused;

    public int GetNumIndices()
    {
        return n_indices;//+(unused==1?0x10000:0);
    }

    void SetNumIndices(int NumIndices)
    {
        n_indices = (ushort)(NumIndices & 0xffff);
        unused = (ushort)((NumIndices & 0x10000) != 0 ? 1 : 0);
    }

    public override string ToString()
    {
        string res = "";
        res += "Prim type " + prim_type + "\n";
        res += "mtl_number " + mtl_number + "\n";
        res += "vertgroup_number " + vertgroup_number + "\n";
        res += "start_vert " + start_vert + "\n";
        res += "n_verts " + n_verts + "\n";
        res += "n_indices " + n_indices + "\n";
        res += "unused " + unused;
        return res;
    }

    public static int GetSize()
    {
        return Marshal.SizeOf<FacetGroup>();
    }
}

public enum MeshDataType
{
    MDT_STD,
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class MeshData : IStormImportable<MeshData>, ISizeEvaluator
//public class MeshData : ISizeEvaluator
{
    public MeshDataType type;
    public int n_maters;
    public int n_groups;
    public int n_vertgroups;

    public geombase.Sphere b_sphere;   // bounding sphere
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    public byte[] data;
    public int GetMatersSize()
    {
        return n_maters * MaterialData.GetSize();
    }
    public MaterialData[] GetMaters()
    {
        return StormFileUtils.ReadStructs<MaterialData>(data, 0, n_maters, MaterialData.GetSize());
    }
    public int GetGroupsSize()
    {
        return n_groups * FacetGroup.GetSize();
    }
    public FacetGroup[] GetGroups()
    {
        return StormFileUtils.ReadStructs<FacetGroup>(data, GetMatersSize(), n_groups, FacetGroup.GetSize());
    }
    public int GetVertGroupsSize()
    {
        return n_vertgroups * VertGroup.GetSize();
    }
    public VertGroup[] GetVertGroups()
    {
        return StormFileUtils.ReadStructs<VertGroup>(data, GetMatersSize() + GetGroupsSize(), n_vertgroups, VertGroup.GetSize());
    }
    public int GetIndicesSize()
    {
        FacetGroup[] fg = GetGroups();
        int sum = 0;
        for (int i = 0; i < n_groups; ++i) sum += fg[i].GetNumIndices() * sizeof(WORD);
        return sum;
    }

    private int GetIndicesCount()
    {
        FacetGroup[] fg = GetGroups();
        int sum = 0;
        for (int i = 0; i < n_groups; ++i) sum += fg[i].GetNumIndices();
        return sum;
    }
    public WORD[] GetIndices()
    {
        return StormFileUtils.ReadStructs<WORD>(data, GetMatersSize() + GetGroupsSize() + GetVertGroupsSize(), GetIndicesCount());
    }
    public WORD[] GetIndices(int group)
    {
        FacetGroup[] fg = GetGroups();
        int sum = 0;
        for (int i = 0; i < group; ++i) sum += fg[i].GetNumIndices() * sizeof(WORD);

        WORD[] res = new WORD[fg.Length - sum - 1];
        Array.Copy(GetIndices(), sum, res, 0, res.Length);
        return res;
    }

    public static int GetSize()
    {
        return -1;
    }

    public MeshData Import(Stream st)
    {
        MeshData res = StormFileUtils.ReadStruct<MeshData>(st);
        res.data = new byte[st.Length - st.Position];
        //res.data = new byte[st.Length - (st.Position + 1)];
        st.Read(res.data);
        //string debugfname = "Debug#" + res.GetHashCode().ToString("X8");
        //FileStream fs = new FileStream(debugfname, FileMode.Create);
        //fs.Write(res.data);
        //fs.Close();

        return res;
    }

    public override string ToString()
    {
        //        public MeshDataType type;
        //public int n_maters;
        //public int n_groups;
        //public int n_vertgroups;
        string res = "";
        res += string.Format("MeshDataType {0}\n",type);
        res += string.Format("n_maters {0}\n", n_maters);
        res += string.Format("n_groups {0}\n", n_groups);
        res += string.Format("n_vertgroups {0}\n", n_vertgroups);
        res += string.Format("data size {0}\n", data!=null ? data.Length:"Empty");

        return res;
    }
}

namespace ExtensionMethods
{
    public static class ImportFromStream
    {
        public static MeshData Import(Stream st)
        {
            MeshData res = StormFileUtils.ReadStruct<MeshData>(st);
            res.data = new byte[st.Length - (st.Position + 1)];
            st.Read(res.data);
            return res;
        }
    }
}