using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.LightTransport;
using static MaterialStorage;
using static StormUnityRenderer;
using static UnityEngine.ParticleSystem;
using DWORD = System.UInt32;

//public class StormUnityRendererParticles
//{
//    private Dictionary<int,GameObject> SceneParticleContainersCurrent= new Dictionary<int, GameObject>();
//    private Dictionary<int, GameObject> SceneParticleContainersPrev = new Dictionary<int, GameObject>();

//    public void Draw(RO r)
//    {
//        GameObject Gobj;
//        bool inCurrent, inPrev;
//        int id = r.GetHashCode();


//        inCurrent = SceneParticleContainersCurrent.ContainsKey(id);
//        inPrev = SceneParticleContainersPrev.ContainsKey(id);

//        if (!inCurrent && inPrev)
//        {
//            SceneParticleContainersCurrent.Add(id, SceneParticleContainersPrev[id]);
//            inCurrent = true;
//        }

//        if (!inCurrent)
//        {
//            Gobj = CreateGameObject(r);
//            SceneParticleContainersCurrent.Add(id, Gobj);
//        }
//    }

//    //TODO Возможно, это лучше вынести в отдельную фабрику
//    private GameObject CreateGameObject(RO r)
//    {
//        GameObject GObj = new GameObject();
//        StormUnityParticleEmitter emitter = GObj.AddComponent<StormUnityParticleEmitter>();
//        emitter.Init((PARTICLE_SYSTEM)r);
//        return GObj;
//    }

//    public void Cleanup()
//    {
//        GameObject pgoc;
//        List<int> removeId = new List<int>();
//        foreach (KeyValuePair<int, GameObject> kvp in SceneParticleContainersPrev)
//        {
//            if (!SceneParticleContainersCurrent.ContainsKey(kvp.Key)) removeId.Add(kvp.Key);
//        }

//        foreach (int key in removeId)
//        {
//            pgoc = SceneParticleContainersPrev[key];
//            GameObject.Destroy(pgoc);
//        }
//        SceneParticleContainersPrev = SceneParticleContainersCurrent;
//        SceneParticleContainersCurrent = new Dictionary<int, GameObject>();
//    }
//}
public class StormUnityRendererParticles
{
    private Dictionary<int, ParticleGameObjectContainer> SceneParticleContainersCurrent = new Dictionary<int, ParticleGameObjectContainer>();
    private Dictionary<int, ParticleGameObjectContainer> SceneParticleContainersPrev = new Dictionary<int, ParticleGameObjectContainer>();

    private bool FrustrumCulling(RO r)
    {
        return StormUnityRenderer.FrustrumCulling(r.Top().Org, r.Top().MaxRadius);
    }
    public void Draw(RO r)
    {
        if (!FrustrumCulling(r)) return;

        ParticleGameObjectContainer pgoc;
        bool inCurrent, inPrev;
        int id = r.GetHashCode();


        inCurrent = SceneParticleContainersCurrent.ContainsKey(id);
        inPrev = SceneParticleContainersPrev.ContainsKey(id);

        if (!inCurrent && inPrev)
        {
            SceneParticleContainersCurrent.Add(id, SceneParticleContainersPrev[id]);
            inCurrent = true;
        }

        if (!inCurrent)
        {
            pgoc = new ParticleGameObjectContainer();
            pgoc.Init(r);
            SceneParticleContainersCurrent.Add(id, pgoc);
        }

        pgoc = SceneParticleContainersCurrent[id];
        pgoc.Update();
    }

    public void Cleanup()
    {
        ParticleGameObjectContainer pgoc;
        List<int> removeId = new List<int>();
        foreach (KeyValuePair<int, ParticleGameObjectContainer> kvp in SceneParticleContainersPrev)
        {
            if (!SceneParticleContainersCurrent.ContainsKey(kvp.Key)) removeId.Add(kvp.Key);
        }

        foreach (int key in removeId)
        {
            pgoc = SceneParticleContainersPrev[key];
            if (pgoc.SomeAlive())
            {
                pgoc.SetLoop(false);
                continue;
            }
            GameObjectFactory.ReleaseParticle(pgoc.myGameObject);
        }
        SceneParticleContainersPrev = SceneParticleContainersCurrent;
        SceneParticleContainersCurrent = new Dictionary<int, ParticleGameObjectContainer>();
    }
}
