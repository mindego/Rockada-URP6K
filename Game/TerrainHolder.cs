using System.Collections.Generic;
using UnityEngine;

public class TerrainHolder
{
    private TERRAIN_DATA mpTerrain;
    private StormTerrainUtils mpTerrainUtils;
    //public List<StormTerrainChunk> TerrainChunks;
    public StormTerrainChunk[] TerrainChunks;
    public int chunkCount, chunksX, chunksZ;
    public TerrainHolder(TERRAIN_DATA pTerrain, int chunk_size)
    {
        //TerrainChunks = new List<StormTerrainChunk>();
        mpTerrain = pTerrain;
        mpTerrainUtils = new StormTerrainUtils("Debug", pTerrain, chunk_size);
        mpTerrainUtils.Initialize();

        chunkCount = (pTerrain.getXSquares() * pTerrain.getZSquares()) / chunk_size;
        chunksX = pTerrain.getXSquares() / StormTerrainUtils.CHUNK_SIZE;
        chunksZ = pTerrain.getZSquares() / StormTerrainUtils.CHUNK_SIZE;
        TerrainChunks = new StormTerrainChunk[chunkCount];
        Init();
    }
    public void Init()
    {
        //for (int z = 0; z < mpTerrain.Header.SizeZBPages; z++)
        //{
        //    //Debug.Log("Creating sector:" + new Vector2Int(0, z));
        //    for (int x = 0; x < mpTerrain.Header.SizeXBPages; x++)
        //    {
        //        tmpTerrainChunkRemake chunk = new tmpTerrainChunkRemake();
        //        //Vector3 pos = new Vector3(x * TerrainDefs.SQUARE_SIZE * CHUNK_SIZE, 0, z * TerrainDefs.SQUARE_SIZE * CHUNK_SIZE);
        //        Vector3 pos = new Vector3(x * StormTerrainUtils.CHUNK_SIZE * TerrainDefs.SQUARE_SIZE, 0 - (short.MaxValue - short.MinValue) / 2 * TerrainDefs.HeightScale, z * StormTerrainUtils.CHUNK_SIZE * TerrainDefs.SQUARE_SIZE);
        //        chunk.WorldPosition = pos;
        //        //chunk.SetUnit(null);
        //        //chunk.setTerrainGenerator(terrainChunkGenerator);
        //        chunk.loader = mpTerrainUtils;
        //        chunk.GridPosition = new Vector2Int(x, z);
        //        chunks.Add(chunk);
        //        //await Task.Yield();
        //    }
        //}
        //Debug.Log(string.Format("Creating chunks {0} x {1}", mpTerrain.GetXSize(), mpTerrain.GetZSize()) );
        //for (int z = 0; z < mpTerrain.Header.SizeZBPages; z++)
        //{
        //    //Debug.Log("Creating sector:" + new Vector2Int(0, z));
        //    for (int x = 0; x < mpTerrain.Header.SizeXBPages; x++)
        //    {
        //        StormTerrainChunk chunk = new StormTerrainChunk();
        //        //Vector3 pos = new Vector3(x * TerrainDefs.SQUARE_SIZE * CHUNK_SIZE, 0, z * TerrainDefs.SQUARE_SIZE * CHUNK_SIZE);
        //        Vector3 pos = new Vector3(x * StormTerrainUtils.CHUNK_SIZE * TerrainDefs.SQUARE_SIZE, 0 - (short.MaxValue - short.MinValue) / 2 * TerrainDefs.HeightScale, z * StormTerrainUtils.CHUNK_SIZE * TerrainDefs.SQUARE_SIZE);
        //        chunk.WorldPosition = pos;
        //        //chunk.SetUnit(null);
        //        //chunk.setTerrainGenerator(terrainChunkGenerator);
        //        chunk.loader = mpTerrainUtils;
        //        chunk.GridPosition = new Vector2Int(x, z);
        //        //TerrainChunks.Add(chunk);
        //        //await Task.Yield();
        //    }
        //}

        int chunksX = mpTerrain.getXSquares() / StormTerrainUtils.CHUNK_SIZE;
        int chunksZ = mpTerrain.getZSquares() / StormTerrainUtils.CHUNK_SIZE; //TODO - возможно тут было getXSquares не просто так.
        for (int z = 0; z < chunksZ; z++)
        {
            //Debug.Log("Creating sector:" + new Vector2Int(0, z));
            for (int x = 0; x < chunksX; x++)
            {
                StormTerrainChunk chunk = new StormTerrainChunk();
                Vector3 pos = new Vector3(x * StormTerrainUtils.CHUNK_SIZE * TerrainDefs.SQUARE_SIZE, 0 - (short.MaxValue - short.MinValue) / 2 * TerrainDefs.HeightScale, z * StormTerrainUtils.CHUNK_SIZE * TerrainDefs.SQUARE_SIZE);
                chunk.WorldPosition = pos;
                chunk.loader = mpTerrainUtils;
                chunk.GridPosition = new Vector2Int(x, z);
                TerrainChunks[z * chunksX + x] = chunk;
            }
        }
    }
}





