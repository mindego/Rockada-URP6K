using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine.Rendering.Universal;
using static D3DEMULATION;
using static StormMesh;
using crc32 = System.UInt32;
using DWORD = System.UInt32;
using WORD = System.UInt16;

//namespace Storm
//{
//    [StructLayout(LayoutKind.Sequential, Pack = 1)]
//    public class MeshData : IStormImportable<MeshData>, ISizeEvaluator
//    {
//        public MeshDataType type;
//        public int n_maters;
//        public int n_groups;
//        public int n_vertgroups;

//        public geombase.Sphere b_sphere;   // bounding sphere
//        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
//        char[] data;
//        public int GetMatersSize()
//        {
//            return n_maters * MaterialData.GetSize();
//        }
//        //public MaterialData[] GetMaters();
//        public int GetGroupsSize()
//        {
//            return n_groups * FacetGroup.GetSize();
//        }
//        //public FacetGroup[] GetGroups();
//        public int GetVertGroupsSize()
//        {
//            return n_vertgroups * VertGroup.GetSize();
//        }
//        //public VertGroup[] GetVertGroups();
//        public int GetIndicesSize()
//        {
//            return -1;
//            //FacetGroup[] fg = GetGroups();
//            //int sum = 0;
//            //for (int i = 0; i < n_groups; ++i) sum += fg[i].GetNumIndices() * sizeof(WORD);
//            //return sum;
//        }
//        //public WORD GetIndices();
//        //public WORD GetIndices(int group);

//        public static int GetSize()
//        {
//            //return sizeof(MeshData) + GetMatersSize() + GetGroupsSize() + GetVertGroupsSize() + GetIndicesSize();
//            return -1;
//        }

//        public MeshData Import(Stream st)
//        {
//            return StormFileUtils.ReadStruct<MeshData>(st);

//        }
//    }
//    public enum MeshDataType
//    {
//        MDT_STD,
//    };


//    public class MaterialData : ISizeEvaluator
//    {         // datafile entry
//        Flags flags;
//        crc32 bmp_id;
//        crc32 mtl_id;
//        //    union {
//        //float parameter;
//        //    crc32 bump_id;
//        //    crc32 envmap_id;
//        //};
//        #region эмуляция union
//        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
//        byte[] varId = new byte[4];

//        public float parameter
//        {
//            get
//            {
//                return BitConverter.ToSingle(varId, 0);
//            }
//        }

//        public crc32 bump_id
//        {
//            get
//            {
//                return BitConverter.ToUInt32(varId, 0);
//            }
//        }

//        public crc32 envmap_id
//        {
//            get
//            {
//                return BitConverter.ToUInt32(varId, 0);
//            }
//        }
//        #endregion
//        public static int GetSize()
//        {
//            //sizeof(flags) + sizeof(bmp_id) + sizeof(mtl_id) + sizeof(varId);
//            //return 4 + 4 + 4 + 4;
//            //return Marshal.SizeOf(typeof(MaterialData));
//            return Marshal.SizeOf<MaterialData>();
//        }

//    };

//    public struct FacetGroup : ISizeEvaluator
//    {
//        D3DPRIMITIVETYPE prim_type;

//        WORD mtl_number;

//        WORD vertgroup_number;
//        WORD start_vert;
//        WORD n_verts;

//        public int GetNumIndices()
//        {
//            return n_indices;//+(unused==1?0x10000:0);
//        }
//        public void SetNumIndices(int NumIndices)
//        {
//            n_indices = (ushort)(NumIndices & 0xffff);

//            unused = ((NumIndices & 0x10000) != 0) ? (ushort)1 : (ushort)0;
//        }
//        private WORD n_indices;
//        private WORD unused;

//        public static int GetSize()
//        {
//            //return 4 + 2 + 2 + 2 + 2;
//            return Marshal.SizeOf<FacetGroup>();
//        }
//    };

//}


