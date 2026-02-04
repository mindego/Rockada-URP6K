using UnityEngine;

public class StormTerrainChunk
{
    public Vector2Int GridPosition;
    public Vector3 WorldPosition;
    public GameObject TerrainGameObject;
    public StormTerrainUtils loader;

    public void Draw()
    {
        if (TerrainGameObject == null)
        {
            TerrainGameObject = loader.GetTerrainChunk(GridPosition);
        }
        TerrainGameObject.transform.position = Engine.ToCameraReference(WorldPosition);
    }

    public void Undraw() {
        if (TerrainGameObject == null) return;

        TerrainGameObject.transform.position = Engine.ToCameraReference(Engine.FarFarAway);
        loader.PutToSpool(TerrainGameObject);
        TerrainGameObject = null;
    }
}

public class StormWaterChunk
{
    public Vector2Int GridPosition;
    public Vector3 WorldPosition;
    public GameObject WaterGameObject;
    public StormTerrainUtils loader;

    public void Draw()
    {
        if (WaterGameObject != null) return;
        WaterGameObject = loader.GetTerrainChunk(GridPosition);
        //TerrainGameObject.name = WorldPosition.ToString();
        //TerrainGameObject.SetActive(true);
    }

    public void Undraw()
    {
        if (WaterGameObject == null) return;

        //Debug.Log("Discarding " + GridPosition);
        //TerrainGameObject.SetActive(false);
        loader.PutToSpool(WaterGameObject);
        WaterGameObject = null;
    }
}





