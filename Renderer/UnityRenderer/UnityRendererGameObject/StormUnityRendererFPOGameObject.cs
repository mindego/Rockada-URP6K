using System.Collections.Generic;
using UnityEngine;
using static RoFlags;
public class StormUnityRendererFPOGameObject : IStormUnityRendererFPO
{
    private Dictionary<int, FPO2GameObject> SceneFPOContainers = new Dictionary<int, FPO2GameObject>();
    private Dictionary<int, FPO2GameObject> SceneFPOContainersPrev = new Dictionary<int, FPO2GameObject>();
    private static Stack<Transform> transforms = new Stack<Transform>();
    
    public void Cleanup()
    {
        //Debug.Log(string.Format("SO before {0}/{1}", SceneFPOContainers.Count, SceneFPOContainersPrev.Count));
        List<int> removeId = new List<int>();
        foreach (KeyValuePair<int, FPO2GameObject> kvp in SceneFPOContainersPrev)
        {
            //if (kvp.Value == null) continue;
            if (!SceneFPOContainers.ContainsKey(kvp.Key)) removeId.Add(kvp.Key);
        }

        foreach (int key in removeId)
        {
            //DrawGameObjectPool.objectPool.Release(SceneObjects[key]);
            //if (SceneFPOContainersPrev[key].myGameObject != null) GameObject.Destroy(SceneFPOContainersPrev[key].myGameObject);
            if (SceneFPOContainersPrev[key].myGameObject != null) SceneFPOContainersPrev[key].Release();
            //SceneFPOContainers.Remove(key);
        }
        SceneFPOContainersPrev = SceneFPOContainers;
        //SceneFPOContainers.Clear();
        SceneFPOContainers = new Dictionary<int, FPO2GameObject>();
        //Debug.Log(string.Format("SO after {0}/{1}", SceneFPOContainers.Count, SceneFPOContainersPrev.Count));
    }

    public static Transform GetCurrentTransform()
    {
        if (transforms.Count == 0) return null;
        return transforms.Peek();
    }
    public void Draw(FPO myFPO)
    {
        if(!StormUnityRenderer.FrustrumCulling(myFPO.Top().Org,myFPO.Top().MaxRadius)) return;
        int id = myFPO.GetHashCode();
        FPO2GameObject parent = null;

        bool inCurrent, inPrev;
        inCurrent = SceneFPOContainers.ContainsKey(id);
        inPrev = SceneFPOContainersPrev.ContainsKey(id);

        SceneFPOContainers.TryGetValue(id, out parent);
        if (!inCurrent && inPrev)
        {
            SceneFPOContainers.Add(id, SceneFPOContainersPrev[id]);
            inCurrent = true;
        }

        if (!inCurrent)
        {
            SceneFPOContainers.Add(id, new FPO2GameObject(myFPO, parent));
            inCurrent = true;
        }


        FPO2GameObject container = SceneFPOContainers[id];

        container.Draw();
        transforms.Push(container.myGameObject.transform);
        if (myFPO.SubObjects != null)
        {
            for (RO tmpRO = myFPO.SubObjects; tmpRO != null; tmpRO = tmpRO.Next)
            {
                try
                {
                    uint typeFlag = tmpRO.GetFlag(ROFID_ALLOBJECTS);
                    switch (typeFlag)
                    {
                        case (ROFID_FPO):
                            Draw((FPO)tmpRO);
                            break;
                        case (ROFID_PARTICLE):
                            StormUnityRenderer.DrawParticle(tmpRO);
                            break;
                        default:
                            Debug.Log("Unsupported RO type: " + tmpRO + " flags " + tmpRO.GetFlags().ToString("X8"));
                            break;

                    }
                }
                catch
                {
                    Debug.Log("Failed to draw RO " + tmpRO + " flags " + tmpRO.GetFlags().ToString("X8"));
                    throw;
                }
            }
        }
        transforms.Pop();
    }
}

