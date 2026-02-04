using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using static HashFlags;
using static renderer_dll;
using static RoFlags;
public class FPO2GameObject
{
    const int NORMAL = 0, DAMAGED = 1, DESTROYED = 2, INTERNAL = 3;
    public GameObject myGameObject { get; private set; }
    private MeshRenderer mr;
    private MeshFilter mf;
    private MeshCollider mc;

    private GameObject parent;
    private int CurrentImage;
    private FPO myFPO;
    FpoGraphData[] myFpoGraphDatas;

    public FPO2GameObject(FPO drawFPO, FPO2GameObject FPOparent) : this(drawFPO, FPOparent == null ? null : FPOparent.parent) { }
    public FPO2GameObject(FPO drawFPO, GameObject parent = null)
    {
        myFPO = drawFPO;
        myFpoGraphDatas = new FpoGraphData[4];
        string name = myFPO.fdata.name.ToString("X8");
        if (myFPO.TextName != null) name = myFPO.TextName + " " + name;

        SetMyGameObject(name);

        //if (myFPO.TextName != null) myGameObject.name = myFPO.TextName + " " + myFPO.GetHashCode().ToString("X8") + " " + myFPO.fdata.name.ToString("X8");
        if (myFPO.TextName != null) myGameObject.name = myFPO.TextName + " " + myGameObject.name;

        CurrentImage = -1;
        this.parent = parent;

        // Debug.Log("Created GameObject visual for [" + myFPO.Top().TextName + " " + myFPO.Top().GetHashCode().ToString("X8") + "] " + myFPO.fdata.name.ToString("X8") + " image " + myFPO.CurrentImage + "/" + CurrentImage + " wrapper " + this.GetHashCode().ToString("X8"));
        Init();
    }

    public void Release()
    {
        if (myGameObject == null) return;
        try
        {
            GameObjectFactory.Release(myGameObject); //TODO вообще надо просто смотреть, вернут он или нет.
        }
        catch
        {

        }

        myGameObject = null;
    }
    private void SetMyGameObject(string name)
    {
        myGameObject = GameObjectFactory.GetGameObject(GameObjectFactory.GameObjectType.MESH);
        myGameObject.name = name;

        mr = myGameObject.GetComponent<MeshRenderer>();
        mf = myGameObject.GetComponent<MeshFilter>();
        mc = myGameObject.GetComponent<MeshCollider>();

        FPOEditor viewer;
        if (!myGameObject.TryGetComponent<FPOEditor>(out viewer))
        {
            viewer = myGameObject.AddComponent<FPOEditor>();
        }
        viewer.SetFPO(myFPO);
    }

    public class FPOEditor : MonoBehaviour
    {
        private FPO myFPO;
        public string FpoName;
        public string crc32;
        public string parent;
        public Vector3 Org;
        public Vector3 Dir;
        [Range(0,3)]
        public int CurrentImage;

        public void SetFPO(FPO FPO)
        {
            myFPO = FPO;
            FpoName = myFPO.TextName;
            crc32 = myFPO.Name.ToString("X8");
            parent = "none";
        }

        private void FixedUpdate()
        {
            if (myFPO.Parent != null) parent = myFPO.Parent.Name.ToString("X8");
            Org = myFPO.Org;
            Dir = myFPO.Dir;
            CurrentImage = myFPO.CurrentImage;
        }

    }

    public void Init()
    {
        myGameObject.transform.position = Engine.FarFarAway;
        for (int i = 0; i < 4; i++)
        {
            myFpoGraphDatas[i] = null;
            if (myFPO.fdata.images[i].graph == 0xFFFFFFFF) continue;

            myFpoGraphDatas[i] = dll_data.meshes.GetBlock(myFPO.fdata.images[i].graph).Convert<FpoGraphData>();
        }

        //if (parent != null)
        //{
        //    myGameObject.transform.parent = parent.transform;
        //}
        myGameObject.transform.parent = StormUnityRendererFPOGameObject.GetCurrentTransform();
        // CreateSlotsGameObjects();

        // AddFPOComponent();

        //UpdateGameObjectVisual();
        //UpdateGameObjectTransform();
    }

    private void AddFPOComponent()
    {
        FPOShowInEditor FPOComponent;
        if (!myGameObject.TryGetComponent<FPOShowInEditor>(out FPOComponent)) FPOComponent = myGameObject.AddComponent<FPOShowInEditor>();

        if (myFPO.MatchFlags(ROObjectId(ROFID_FPO | ROFID_PARTICLE)))
        {
            FPOComponent.ROFID_FPO = myFPO.MatchFlags(ROObjectId(ROFID_FPO));
            FPOComponent.ROFID_PARTICLE = myFPO.MatchFlags(ROObjectId(ROFID_PARTICLE));
            FPOComponent.ROF_TANSP = myFPO.GetFlag(ROF_TANSP) != 0;
            FPOComponent.ROF_ST_TANSP = myFPO.GetFlag(ROF_ST_TANSP) != 0;
        }
        else if (myFPO.MatchGroup(OF_GROUP_TMT))
            FPOComponent.OF_GROUP_TMT = myFPO.MatchGroup(OF_GROUP_TMT);
        FPOComponent.Name = myFPO.GetFlags().ToString("X8");
        FPOComponent.Dir = myFPO.Dir;
        FPOComponent.Org = myFPO.Org;
        FPOComponent.Up = myFPO.Up;
    }

    public void CreateSlotsGameObjects()
    {
        foreach (SlotData sld in myFPO.fdata.slots)
        {
            //GameObject slot = new GameObject();
            GameObject slot = GameObjectFactory.GetGameObject(GameObjectFactory.GameObjectType.EMPTY);
            slot.name = "Slot " + Hasher.StringHsh(sld.name) + " " + sld.name.ToString("X8") + " " + sld.slot_id;

            Vector3 slotOrg = sld.pos.org;
            slotOrg.y *= -1;
            //Vector3 slotUp = sld.pos.e2;
            //slotUp.y *= -1;
            //Vector3 slotDir = sld.pos.GetE3();
            ////slotDir.y *= -1;

            slot.transform.parent = myGameObject.transform;
            slot.transform.localPosition = slotOrg;
            //slot.transform.localRotation = Quaternion.LookRotation(slotDir);

            //Vector3 SlotUp = myFPO.fdata.pos.e2;
            //Vector3 SlotLeft = myFPO.fdata.pos.e1;
            //TODO вообще-то неправильно, должно быть:
            Vector3 SlotUp = sld.pos.e2;
            Vector3 SlotLeft = sld.pos.e1;

            SlotUp.y *= -1;
            SlotLeft.y *= -1;

            Vector3 SlotDir = Vector3.Cross(SlotUp, SlotLeft);

            //slot.transform.localRotation = Quaternion.LookRotation(SlotDir, SlotUp * -1);
            slot.transform.parent = myGameObject.transform.root;
            slot.transform.localRotation = Quaternion.LookRotation(SlotDir, SlotUp * -1);
            slot.transform.parent = myGameObject.transform;
            Vector3ShowInEditor Up = slot.AddComponent<Vector3ShowInEditor>();
            Up.Name = "Slot Up " + Hasher.StringHsh(sld.name) + " " + sld.slot_id;
            Up.Value = sld.pos.e2;

            Vector3ShowInEditor Dir = slot.AddComponent<Vector3ShowInEditor>();
            Dir.Name = "Slot Dir " + Hasher.StringHsh(sld.name) + " " + sld.slot_id;
            Dir.Value = sld.pos.GetE3();

            Vector3ShowInEditor Pos = slot.AddComponent<Vector3ShowInEditor>();
            Dir.Name = "Slot pos " + Hasher.StringHsh(sld.name) + " " + sld.slot_id;
            Dir.Value = sld.pos.org;

        }
    }


    public void Draw()
    {
        UpdateGameObjectVisual();
        UpdateGameObjectTransform();
    }

    private Vector3 Dir, Up, Org;
    public void UpdateGameObjectTransform()
    {
        UpdateParent();
        UpdatePosition();
        UpdateRotation();
    }

    private void UpdateParent()
    {
        Transform currentTransform = StormUnityRendererFPOGameObject.GetCurrentTransform();
        if (myGameObject.transform.parent == currentTransform) return;

        myGameObject.transform.parent = currentTransform;
    }

    private void UpdateRotation()
    {
        //if (myFPO.Dir != Vector3.zero && myFPO.Up != Vector3.zero) myGameObject.transform.localRotation = Quaternion.LookRotation(myFPO.Dir, myFPO.Up);
        if (myFPO.Dir == Vector3.zero || myFPO.Up == Vector3.zero) return;
        if (Dir == myFPO.Dir && Up == myFPO.Up) return;
        myGameObject.transform.localRotation = Quaternion.LookRotation(myFPO.Dir, myFPO.Up);
        //Debug.Log((myFPO.Dir, myFPO.Up));
        Dir = myFPO.Dir;
        Up = myFPO.Up;
    }
    private void UpdatePosition()
    {
        if (StormUnityRendererFPOGameObject.GetCurrentTransform() !=null )
        {
            myGameObject.transform.localPosition = myFPO.Org;
        }
        else
        {
            if (!IsValid(myFPO.Org))
            {
                Debug.LogErrorFormat("Invalid Vector org {0}, prev value {1} for FPO {2} {3}",myFPO.Org,Org,myFPO.TextName,myFPO.GetHashCode().ToString("X8"));
            }
            myGameObject.transform.position = Engine.ToCameraReference(myFPO.Org);
        }
        Org = myFPO.Org;
    }

    public static bool IsValid(Vector3 v)
    {
        if (float.IsNaN(v.x)) return false;
        if (float.IsNaN(v.y)) return false;
        if (float.IsNaN(v.z)) return false;
        return true;
    }
    public void UpdateGameObjectVisual()
    {
        int LOD = 0;

        if (CurrentImage == myFPO.CurrentImage) return;

        CurrentImage = myFPO.CurrentImage;
        //TODO Создавать mesh с LODами.
        if (myFpoGraphDatas[CurrentImage] == null)
        {
            //Debug.Log("Failed to update GameObject visual for [" + MyFPODescrtiption() + "]");

            //No visual for this FPO in current Image
            mr.materials = new Material[0];
            mf.sharedMesh = null;
            mc.sharedMesh = null;

            return;
        }
        uint id = myFpoGraphDatas[CurrentImage].GetLod(LOD);

        ObjId tmpId = new ObjId(id);
        tmpId = dll_data.meshes.CompleteObjId(tmpId);

        StormMesh graph = dll_data.GetStormMesh(id);

        if (!DrawGameObjectPool.meshcache.ContainsKey(id))
        {
            DrawGameObjectPool.meshcache.Add(id, StormMeshImport.ExtractMesh(graph, tmpId.name));
        }
        Mesh tmpMesh = DrawGameObjectPool.meshcache[id];

        if (tmpMesh != null)
        {
            //MeshRenderer mr;

            //if (!myGameObject.TryGetComponent<MeshRenderer>(out mr))
            //{
            //    //Debug.Log("Adding MR to " + gobj.name);
            //    mr = myGameObject.AddComponent<MeshRenderer>();
            //}
            try
            {
                mr.materials = StormMeshImport.GetMaterials(graph);
            }
            catch
            {
                Debug.LogError(myGameObject != null ? myGameObject.name : "EMPTY GOBJ");
                Debug.Break();
            }

            //if (myFPO.TextName == null) myFPO.TextName = tmpId.name;
            myGameObject.name = myFPO.TextName == null ? tmpId.name : myFPO.TextName;
            myGameObject.name += "#" + myFPO.GetHashCode().ToString("X8");
            //MeshFilter mf;

            //if (!myGameObject.TryGetComponent<MeshFilter>(out mf))
            //{
            //    //Debug.Log("Adding MF to " + gobj.name);
            //    mf = myGameObject.AddComponent<MeshFilter>();
            //}
            //mf.mesh = tmpMesh;
            mf.sharedMesh = tmpMesh;

            //MeshCollider mc;
            //if (!myGameObject.TryGetComponent<MeshCollider>(out mc))
            //{
            //    mc = myGameObject.AddComponent<MeshCollider>();
            //}
            mc.sharedMesh = tmpMesh;
            //Debug.Log("Using mesh " + tmpMesh + " for " + tmpId.name + " MF " + mf.mesh.vertexCount + " " + mf);
        }
        else Debug.LogErrorFormat("Empty mesh for {0} Image:{1} LOD:{2}", myFPO.TextName, CurrentImage, LOD);
        //else Debug.LogErrorFormat("Empty mesh for {}" + myGameObject.name);

        //Debug.Log("Done Updating GameObject visual for [" + MyFPODescrtiption() + " image " + "] " + myFPO.CurrentImage + "/" + CurrentImage);
    }
}