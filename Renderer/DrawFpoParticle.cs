using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Pool;
using static HashFlags;
using static RoFlags;
using static renderer_dll;

public class FPOShowInEditor : MonoBehaviour
{
    public string Name;
    public Vector3 Org;
    public Vector3 Dir;
    public Vector3 Up;
    public bool ROFID_FPO;
    public bool ROFID_PARTICLE;
    public bool ROF_TANSP;
    public bool ROF_ST_TANSP;
    public bool OF_GROUP_TMT;
}
public class Vector3ShowInEditor : MonoBehaviour
{
    public string Name;
    public Vector3 Value;
}
public class DrawFpoParticle : HashEnumer
{
    int effects;   // 1 or 0 only
    IHash hasher;
    int layer;

    //LightManager* light;
    public DrawFpoParticle(int e, IHash hs, int lr, object l = null)
    {
        effects = e;
        hasher = hs;
        layer = lr;
        //light = l;
    }

    //public static GameObject DrawFPO(FPO myFPO, GameObject parent = null)
    //{
    //    int id = myFPO.GetHashCode();
    //    if (!Engine.SceneFPOContainersPrev.ContainsKey(id))
    //    {
    //        Engine.SceneFPOContainers.Add(id, new FPO2GameObject(myFPO, parent));
    //    }
    //    else
    //    {
    //        Engine.SceneFPOContainers.Add(id, Engine.SceneFPOContainersPrev[id]);
    //    }

    //    FPO2GameObject container = Engine.SceneFPOContainers[id];

    //    if (myFPO.SubObjects != null)
    //    {
    //        for (RO tmpRO = myFPO.SubObjects; tmpRO != null; tmpRO = tmpRO.Next)
    //        {
    //            try
    //            {
    //                GameObject subGobj;
    //                //GameObject subGobj = DrawFPO((FPO)tmpFPO, container.myGameObject);
    //                uint typeFlag = tmpRO.GetFlag(ROFID_ALLOBJECTS);
    //                switch (typeFlag)
    //                {
    //                    case (ROFID_FPO):
    //                        subGobj = DrawFPO((FPO)tmpRO, container.myGameObject);
    //                        break;
    //                    case (ROFID_PARTICLE):
    //                        Debug.Log("Draw Particle effect: " + tmpRO + " flags " + tmpRO.GetFlags().ToString("X8"));
    //                        break;
    //                    default:
    //                        Debug.Log("Unsupported RO type: " + tmpRO + " flags " + tmpRO.GetFlags().ToString("X8"));
    //                        break;

    //                }
    //            }
    //            catch
    //            {
    //                Debug.Log("Failed to draw RO " + tmpRO + " flags " + tmpRO.GetFlags().ToString("X8"));
    //                throw;
    //            }
    //        }
    //    }
    //    container.Update();
    //    return container.myGameObject;
    //}


    //public static GameObject DrawFPO(FPO myFPO, GameObject parent = null)
    //{
    //    int id = myFPO.GetHashCode();
    //    if (!Engine.SceneFPOs.ContainsKey(id)) Engine.SceneFPOs.Add(id, myFPO);

    //    GameObject Gobj;
    //    if (!Engine.SceneObjects.ContainsKey(id))
    //    {
    //        Gobj = fdata2gameObject(myFPO.fdata, myFPO.fdata.name.ToString("X8"), parent);
    //        if (myFPO.TextName != null) Gobj.name = myFPO.TextName;

    //        if (parent != null)
    //        {
    //            Gobj.transform.parent = parent.transform;
    //            //Gobj.transform.localPosition = myFPO.Org;
    //        }
    //        Gobj.transform.localPosition = myFPO.Org;
    //        Engine.SceneObjects.Add(id, Gobj);
    //    }

    //    Gobj = Engine.SceneObjects[id];

    //    //ShowRoFlags info;
    //    //if (!Gobj.TryGetComponent<ShowRoFlags>(out info))
    //    //{
    //    //    info = Gobj.AddComponent<ShowRoFlags>();
    //    //}
    //    //info.ROFID_PARTICLE = myFPO.MatchFlags(ROObjectId(ROFID_FPO | ROFID_PARTICLE));
    //    //info.ROFID_FPO = myFPO.MatchFlags(ROObjectId(ROF_TANSP | ROF_ST_TANSP));
    //    //info.OF_GROUP_TMT = myFPO.MatchGroup(OF_GROUP_TMT);
    //    //info.RAW = myFPO.GetFlags().ToString("X8");
    //    //info.NAME = Gobj.name;
    //    //info.HashId = myFPO.GetHashCode().ToString("X8");
    //    //info.Dir = myFPO.Dir;
    //    //info.Org = myFPO.Org;

    //    if (myFPO.SubObjects != null)
    //    {
    //        for (RO tmpFPO = myFPO.SubObjects; tmpFPO != null; tmpFPO = tmpFPO.Next)
    //        {
    //            GameObject subGobj = DrawFPO((FPO)tmpFPO, Gobj);
    //        }
    //    }

    //    return Gobj;
    //}



    public static GameObject fdata2gameObject(FpoData fdata, string name = "unknown", GameObject parent = null)
    {
        //if (fdata.images[0].graph == 0xFFFFFFFF) return null;
        //GameObject gobj = new GameObject(name);
        GameObject gobj = DrawGameObjectPool.objectPool.Get();
        if (parent != null)
        {
            gobj.transform.parent = parent.transform;
        }
        gobj.name = name + " NO GRAPH";

        FpoGraphData fpoGraphData;
        if (fdata.images[0].graph == 0xFFFFFFFF)
        {
            fpoGraphData = null;
        }
        else
        {
            fpoGraphData = dll_data.meshes.GetBlock(fdata.images[0].graph).Convert<FpoGraphData>();
        }
        //FpoGraphData fpoGraphData = StormRendererData.GetFpoGraph(fdata.images[0].graph);

        if (fpoGraphData != null)
        {
            if (fpoGraphData.GetLod(0) != 0x00000000)
            {
                //gobj.name = name;
                //StormMesh graph = GameDataHolder.GetResource<StormMesh>(PackType.MeshDB, fpoGraphData.GetLod(0));
                uint id = fpoGraphData.GetLod(0);

                ObjId tmpId = new ObjId(id);
                tmpId = dll_data.meshes.CompleteObjId(tmpId);
                gobj.name = "Mesh " + tmpId.name;



                StormMesh graph = dll_data.GetStormMesh(id);

                if (!DrawGameObjectPool.meshcache.ContainsKey(id))
                {
                    DrawGameObjectPool.meshcache.Add(id, StormMeshImport.ExtractMesh(graph, tmpId.name));
                    //Debug.Log("ID added " + tmpId.obj_id.ToString("X8") + " " + tmpId.name + " size " + DrawGameObjectPool.meshcache.Count);
                }
                Mesh tmpMesh = DrawGameObjectPool.meshcache[id];

                if (tmpMesh != null)
                {
                    MeshRenderer mr;

                    if (!gobj.TryGetComponent<MeshRenderer>(out mr))
                    {
                        //Debug.Log("Adding MR to " + gobj.name);
                        mr = gobj.AddComponent<MeshRenderer>();
                    }
                    mr.materials = StormMeshImport.GetMaterials(graph);
                    MeshFilter mf;

                    if (!gobj.TryGetComponent<MeshFilter>(out mf))
                    {
                        //Debug.Log("Adding MF to " + gobj.name);
                        mf = gobj.AddComponent<MeshFilter>();
                    }
                    mf.mesh = tmpMesh;

                    //Debug.Log("Using mesh " + tmpMesh + " for " + tmpId.name + " MF " + mf.mesh.vertexCount + " " + mf);
                }
                else Debug.Log("Empty mesh for " + gobj.name);
            }
        }

        Vector3 fixedpos;
        fixedpos = fdata.pos.org;
        fixedpos.y *= -1;
        gobj.transform.localPosition = fixedpos;

        Vector3 FPOUp = fdata.pos.e2;
        Vector3 FPOLeft = fdata.pos.e1;

        FPOUp.y *= -1;
        FPOLeft.y *= -1;

        Vector3 FPODir = Vector3.Cross(FPOUp, FPOLeft);

        gobj.transform.localRotation = Quaternion.LookRotation(FPODir, FPOUp * -1);


        foreach (SlotData sld in fdata.slots)
        {
            GameObject slot = new GameObject();
            slot.name = "Slot " + sld.name.ToString("X8") + " " + sld.slot_id;

            Vector3 slotOrg = sld.pos.org;
            slotOrg.y *= -1;
            //Vector3 slotUp = sld.pos.e2;
            //slotUp.y *= -1;
            //Vector3 slotDir = sld.pos.GetE3();
            ////slotDir.y *= -1;

            slot.transform.parent = gobj.transform;
            slot.transform.localPosition = slotOrg;
            //slot.transform.localRotation = Quaternion.LookRotation(slotDir);

            Vector3 SlotUp = fdata.pos.e2;
            Vector3 SlotLeft = fdata.pos.e1;

            SlotUp.y *= -1;
            SlotLeft.y *= -1;

            Vector3 SlotDir = Vector3.Cross(FPOUp, FPOLeft);

            slot.transform.localRotation = Quaternion.LookRotation(SlotDir, SlotUp * -1);
        }
        //for (f = fdata.GetNextData(); f != null; f = f.GetNextData())
        //{
        //    await Task.Yield();
        //    await fdata2gameObject(f, "next", parent);
        //}

        return gobj;

    }


    //public bool ProcessElement(HMember p)
    //{
    //    Debug.Log("GetFlags: " + p.Object().GetFlags().ToString("X8") + " effects " + effects.ToString());
    //    if (p.Object().MatchFlags(ROObjectId(ROFID_FPO | ROFID_PARTICLE)))
    //    {
    //        RO r = (RO)p.Object();
    //        if (effects==0 || (effects!=0 && (r.GetFlag(ROF_TANSP | ROF_ST_TANSP)!=0)))
    //            Debug.Log(string.Format("D3DPipeDraw({0})",r));
    //    }
    //    else if (effects==0 && p.Object().MatchGroup(OF_GROUP_TMT))
    //        Debug.Log(string.Format("Engine::drawTMT((TmTree::node){0}",p.Object()));
    //    return true;
    //}

    public bool ProcessElementNaive(HMember p)
    {
        if (p.Object().MatchFlags(ROObjectId(ROFID_FPO | ROFID_PARTICLE)))
        {
            StormUnityRenderer.DrawFPO((FPO)p.Object());
            //DrawFPO((FPO)p.Object());
            return true;
        }
        if (p.Object().MatchFlags(ROObjectId(ROFID_LIGHT)))
        {
            DrawLight(p.Object());
            return true;
        }
        Debug.LogError("Failed to process" + p.Object());
        return false;
    }

    public bool ProcessElementAdvanced(HMember p)
    {
        if (p.Object().MatchFlags(ROObjectId(ROFID_FPO | ROFID_PARTICLE)))
        {
            RO r;
            try
            {
                r = (RO)p.Object();
            }
            catch
            {
                Debug.LogError("Failed to process as RO:" + p.Object());
                return false;
            }
            if (effects == 0 || (effects != 0 && (r.GetFlag(ROF_TANSP | ROF_ST_TANSP) != 0)))
                D3DPipeDraw(r);
        }
        else if (effects == 0 && p.Object().MatchGroup(OF_GROUP_TMT))
        {
            //Engine.drawTMT((TmTree::node)(p.Object()));
            Debug.LogError("TMT rendering not implemended for " + p.Object());
        }

        return true;
    }
    public bool ProcessElement(HMember p)
    {
        //return ProcessElementNaive(p);
        return ProcessElementAdvanced(p);
    }


    private void D3DPipeDraw(RO r)
    {
        r.SetFlag(ROF_DRAWED);
        ClipStatus clip_status = Engine.SphereVisible(new
          //geombase.Sphere(Engine.GetWorldViewTransform().TransformPoint(r.Org), r.MaxRadius));
          geombase.Sphere(r.Org, r.MaxRadius));

        if (clip_status.ClipUnion == 0)
        {
            LightAdder la = new LightAdder(Engine.light_pipe, Engine.AllLightsEx);

            geombase.Sphere s = new geombase.Sphere(r.Org, r.HashRadius);
            if (Engine.DynamicLighting!=0)
                Engine.scene.objects.hasher.EnumSphere(s,  ROObjectId(ROFID_LIGHT), la);

            Engine.PushLights(Engine.AllLightsEx, la.numlights);

            //Engine.PushClipStatus(clip_status);

            int nl;
            LIGHT[] l = Engine.GetLights(out nl);

            Engine.DrawROEx(r, l, nl, effects);

            //Engine.PopClipStatus();
            Engine.PopLights();
        }
    }

    private void DrawLight(object o)
    {
        //THashed<LightObject> lo = (THashed<LightObject>)o;
        //LightControl<LightObject> lc = (LightControl<LightObject>)lo.Query(ILight.ID);
        //LightEss<LightObject> light = (LightEss<LightObject>)lo.Query(ILightEss.ID);
        ////Debug.Log("Drawing light " + lo + " @ " + lc.GetHashObject());
        ////Debug.Log("Light control: " + lc);
        ////Debug.Log("Light essence: " + light);
        ////Debug.Log("Light @ " + light.GetLIGHT().GetPosition());

        //LIGHT myLight = light.GetLIGHT();
        //int hash = myLight.GetHashCode();
        //if (!Engine.SceneROList.ContainsKey(hash))
        //{
        //    GameObject gobjlight = CreateUnityLight(myLight);
        //    gobjlight.name = "Light " + hash.ToString("X8");
        //    Engine.SceneROList.Add(hash, gobjlight);
        //}
        //GameObject gobj = Engine.SceneROList[hash];
        //gobj.transform.position = Engine.ToCameraReference(myLight.GetPosition());
    }

    private GameObject CreateUnityLight(LIGHT stormLight)
    {
        GameObject gobjlight = new GameObject("Light");
        Light unityLight = gobjlight.AddComponent<Light>();

        switch (stormLight.GetLightType())
        {
            case LIGHTTYPE.LT_POINT:
                unityLight.type = LightType.Point;
                break;
            case LIGHTTYPE.LT_SPOT:
                unityLight.type = LightType.Spot;
                break;
            case LIGHTTYPE.LT_DIRECTIONAL:
                unityLight.type = LightType.Directional;
                break;
            default:
                unityLight.type = LightType.Point;
                break;
        }

        unityLight.useColorTemperature = false;
        unityLight.color = stormLight.GetColor();
        unityLight.intensity = stormLight.GetIntensity();
        unityLight.range = stormLight.GetRadius();
        //unityLight. stormLight.GetRadius();

        return gobjlight;
    }


    //public bool ProcessElementOld(HMember p)
    //{
    //    FPO tmpFPO = (FPO)p.Object();

    //    int id = tmpFPO.GetHashCode();


    //    Vector3 LocalCoordinates = tmpFPO.Org - Engine.EngineCamera.Org;
    //    float distance = LocalCoordinates.magnitude;

    //    GameObject obj;
    //    if (distance > Engine.UnityCamera.farClipPlane)
    //    {
    //        return true;
    //    }

    //    //if (distance > Camera.main.farClipPlane)
    //    //{
    //    //    if (Engine.SceneObjects.ContainsKey(id))
    //    //    {
    //    //        //Debug.Log("Deleting " + tmpFPO.TextName + " " + p.Object().GetHashCode().ToString("X8") + " due dist " + distance);

    //    //        //DrawGameObjectPool.objectPool.Release(Engine.SceneObjects[id]);
    //    //        //Engine.SceneObjects.Remove(id);
    //    //        RemoveTree(tmpFPO);
    //    //        //Debug.Log("Deleted " + tmpFPO.TextName + " " + p.Object().GetHashCode().ToString("X8"));
    //    //    }
    //    //    return true;
    //    //}


    //    //if (!Engine.SceneFPOs.ContainsKey(id)) Engine.SceneFPOs.Add(id, tmpFPO);
    //    //if (!Engine.SceneObjects.ContainsKey(id))
    //    //{
    //    //    obj = DrawFPO(tmpFPO);
    //    //    obj.name = obj.name + " " + p.GetHashCode().ToString("X8");
    //    //    //Engine.SceneObjects.Add(id, obj);
    //    //}
    //    DrawFPO(tmpFPO);
    //    obj = Engine.SceneFPOContainersPrev[id].myGameObject;

    //    //TODO Возможно, это стоить делать в обновлении FPO2GameObject
    //    if (obj != null)
    //    {
    //        //obj.transform.position = Engine.ToCameraReference(LocalCoordinates);
    //        obj.transform.position = Engine.ToCameraReference(tmpFPO.Org);
    //        //obj.transform.rotation = Quaternion.LookRotation(tmpFPO.Dir, tmpFPO.Up);
    //        //obj.transform.localRotation = Quaternion.LookRotation(tmpFPO.Dir, tmpFPO.Up);
    //        UpdateTree(tmpFPO);
    //    }
    //    return true;
    //}

    //void MeoPipeDraw(RO*);
    //void D3DPipeDraw(RO*);


    //private void UpdateTree(RO ro)
    //{
    //    int id = ro.GetHashCode();
    //    if (!Engine.SceneFPOContainers.ContainsKey(id)) return;

    //    //Quaternion tmpRotation = Quaternion.LookRotation(ro.Dir, ro.Up);
    //    //if (Engine.SceneObjects[id].transform.localRotation != tmpRotation) Engine.SceneObjects[id].transform.localRotation = tmpRotation;
    //    if (ro.Dir != Vector3.zero && ro.Up != Vector3.zero) Engine.SceneFPOContainers[id].myGameObject.transform.localRotation = Quaternion.LookRotation(ro.Dir, ro.Up);
    //    Engine.SceneFPOContainers[id].myGameObject.transform.position = Engine.ToCameraReference(ro.Org);
    //    //Debug.Log("Updating tree for " + Engine.SceneObjects[id].name + " of " +  ro.Top().TextName);
    //    if (ro.SubObjects != null)
    //    {
    //        for (RO sub = ro.SubObjects; sub != null; sub = sub.Next)
    //        {
    //            UpdateTree(sub);
    //        }
    //    }
    //}
    //private void UpdateTreeOld(RO ro)
    //{
    //    int id = ro.GetHashCode();
    //    if (!Engine.SceneFPOContainersPrev.ContainsKey(id)) return;

    //    //Quaternion tmpRotation = Quaternion.LookRotation(ro.Dir, ro.Up);
    //    //if (Engine.SceneObjects[id].transform.localRotation != tmpRotation) Engine.SceneObjects[id].transform.localRotation = tmpRotation;
    //    if (ro.Dir != Vector3.zero && ro.Up != Vector3.zero) Engine.SceneFPOContainersPrev[id].myGameObject.transform.localRotation = Quaternion.LookRotation(ro.Dir, ro.Up);
    //    //Debug.Log("Updating tree for " + Engine.SceneObjects[id].name + " of " +  ro.Top().TextName);
    //    if (ro.SubObjects != null)
    //    {
    //        for (RO sub = ro.SubObjects; sub != null; sub = sub.Next)
    //        {
    //            UpdateTree(sub);
    //        }
    //    }
    //}
};



public class ShowRoFlags : MonoBehaviour
{
    public string HashId;
    public string NAME;
    public Vector3 Dir;
    public Vector3 Org;

    public bool ROFID_PARTICLE;
    public bool ROFID_FPO;
    public bool OF_GROUP_TMT;
    public string RAW;
}
