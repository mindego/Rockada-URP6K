using static D3DEMULATION;
using static renderer_dll;
using DWORD = System.UInt32;
using WORD = System.UInt16;

public class FpoLod
{
    #region UnityConverted
    private EngineVisualMesh visualMesh;
    #endregion
    public FpoLod()
    {
        //mShaders = null;
        mMeshData = null;
        mNumGroups = 0;
        mGroupData = null;
    }
    ~FpoLod()
    {
        //mShaders = null;
        mMeshData = null;
        mNumGroups = 0;
        mGroupData = null;
    }
    public bool Initialize(MeshData data)
    {
        mMeshData = data;
        mShaders = new IShader[mMeshData.n_maters];

        for (int i = 0; i < mMeshData.n_maters; ++i)
            mShaders[i] = Engine.CreateFpoShader(new FpoShaderId(mMeshData.GetMaters()[i]));

        mGroupData = new GroupData[mMeshData.n_groups];
        FacetGroup[] groups = mMeshData.GetGroups();
        VertGroup[] vgroups = mMeshData.GetVertGroups();
        WORD[] ids = mMeshData.GetIndices();
        int GroupIndex = 0;

        for (int i = 0; i < mMeshData.n_groups; ++i, ++GroupIndex)
        {
            VertGroup vg = vgroups[groups[GroupIndex].vertgroup_number];
            //TODO - Обдумать и реализовать загрузку FpoVert
            //const char* vts = (const char*)dll_data.GetFpoVert(vg.Vertices);
            byte[] vts = dll_data.GetFpoVert(vg.Vertices);

            var startVertex = groups[GroupIndex].start_vert * FVFSize(vg.FVF);
            FpoMesh m = new FpoMesh(groups[GroupIndex].prim_type, vg.FVF, vts[startVertex..], groups[GroupIndex].n_verts, ids, groups[GroupIndex].GetNumIndices());

            //mGroupData[i].mRenderableNoClip = new RefMem<FpoRenderableNoClip>();
            //mGroupData[i].mRenderableNoClip->Initialize( m );
            //mGroupData[i].mRenderableClip   = new RefMem<FpoRenderableClip  >();
            //mGroupData[i].mRenderableClip  ->Initialize( m );

            mGroupData[i] = new GroupData();
            mGroupData[i].mMesh = m;

            mGroupData[i].mRenderableShader = mShaders[groups[GroupIndex].mtl_number];

            //ids += groups[GroupIndex].GetNumIndices();
            ids = ids[groups[GroupIndex].GetNumIndices()..];
        }

        mNumGroups = mMeshData.n_groups;

        return true;
    }

    int FVFSize(DWORD fvf)
    {
        Asserts.AssertBp(fvf == D3DFVF_VERTEX);
        return 32;
    }


    public geombase.Sphere GetSphereBound()
    {
        return mMeshData.b_sphere;
    }

    public int GetNumGroups()
    {
        return mNumGroups;
    }
    public IShader GetShader(int group)
    {
        Asserts.AssertBp(group < mMeshData.n_groups);
        return mGroupData[group].mRenderableShader;

    }
    public FpoMesh GetMesh(int group)
    {
        Asserts.AssertBp(group < mMeshData.n_groups);
        return mGroupData[group].mMesh;
    }

    void TestSphereBound(geombase.Sphere g) { } // судя по всему, что-то делается только при _DEBUG
    protected MeshData mMeshData;

    //interface IShader **mShaders;

    protected int mNumGroups;

    class GroupData
    {
        public IShader mRenderableShader;
        public FpoMesh mMesh;
    };

    GroupData[] mGroupData;
    IShader[] mShaders;
};
