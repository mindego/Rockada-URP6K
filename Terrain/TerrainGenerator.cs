using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using static renderer_dll;


//public class ChunkedTerrainGenerator : BaseObject
//{
//    public string mapName;
//    public const int CHUNK_SIZE = 128;

//    public BaseScene scene;
//    private tmpCamera myCamera;
//    private TERRAIN_DATA mpTerrain;
//    private StormTerrainUtils terrainChunkGenerator;
//    public ChunkedTerrainGenerator(BaseScene scene,string mapName)
//    {
//        this.mapName = mapName;
//        this.scene = scene;
//    }

//    public ChunkedTerrainGenerator(BaseScene scene, string mapName, TERRAIN_DATA terrainData) : this(scene, mapName)
//    {
//        mpTerrain = terrainData;
//        Initialize();
//    }

//    public void Initialize() //TODO: Перенести в загрузчик миссии?
//    {
//        terrainChunkGenerator = new StormTerrainUtils(mapName,mpTerrain);
//        terrainChunkGenerator.Initialize();

//        for (int z = 0; z < mpTerrain.Header.SizeZBPages; z++)
//        {
//            Debug.Log("Creating sector:" + new Vector2Int(0, z));
//            for (int x = 0; x < mpTerrain.Header.SizeXBPages; x++)
//            {
//                tmpTerrainChunkRemake chunk = new tmpTerrainChunkRemake();
//                //Vector3 pos = new Vector3(x * TerrainDefs.SQUARE_SIZE * CHUNK_SIZE, 0, z * TerrainDefs.SQUARE_SIZE * CHUNK_SIZE);
//                Vector3 pos = new Vector3(x * CHUNK_SIZE * TerrainDefs.SQUARE_SIZE, 0 - (short.MaxValue - short.MinValue) / 2 * TerrainDefs.HeightScale, z * CHUNK_SIZE * TerrainDefs.SQUARE_SIZE);
//                chunk.SetOrg(pos);
//                chunk.SetUnit(null);
//                chunk.setTerrainGenerator(terrainChunkGenerator);
//                chunk.pos = new Vector2Int(x, z);
//                scene.AddItem(chunk);
//                scene.AddActor(chunk);
//                //await Task.Yield();
//            }
//        }
//    }

//    public void SetCamera(tmpCamera camera)
//    {
//        myCamera = camera;
//    }

//    public override bool Move(float f)
//    {
//        //This is terrain chunk. It should not move anywhere
//        return true;
//    }

//    public override void Update(float f)
//    {

//        base.Update(f);
//    }
//}
public class TerrainGenerator 
{
    // Start is called before the first frame update
    [Tooltip("Terrains scale (how many units in per pixel")]
    //public const int TerrainDefs.SQUARE_SIZE = 64;
    //[Tooltip("Terrain file name")]
    public string mapName;
    [Tooltip("Map (chunks) filename")]
    public string GameMapName;
    [Tooltip("Map (chunks) size")]
    public Vector2Int GameMapSize;
    private TERRAIN_DATA std;
    public const int CHUNK_SIZE = 128;
    private List<SpoolChunkData> terrainPool;
    private List<SpoolChunkData> loadedChunks;
    public string[] TerrainTextures;
    public SurfData[] SurfDatas;
    private Texture2D[] TerrainTexturesArray;

    private GameObject World;
    private GameObject TerrainPoolParent;

    public Vector2Int CurrentChunkPos = new Vector2Int(-1,-1);

    public BaseScene scene;

    public TerrainGenerator() : this(null,"NetArena")
    {
    }

    public TerrainGenerator(BaseScene scene,string mapName)
    {
        this.mapName = mapName;
        this.scene = scene;
        Initialize();
    }

    public TerrainGenerator(BaseScene scene, string mapName,TERRAIN_DATA terrainData)
    {
        this.mapName = mapName;
        this.scene = scene;
        this.std = terrainData;
        Initialize();
    }


    private void InitTextures()
    {
        if (TerrainTextures.Length == 0) return;
        TerrainTexturesArray = new Texture2D[TerrainTextures.Length];
        Texture2D tmpTexture;
        for (int i = 0; i < TerrainTextures.Length; i++)
        {
            string textureName = TerrainTextures[i];
            //tmpTexture = GameDataHolder.GetResource<Texture2D>(PackType.TexturesDB, textureName);
            tmpTexture = dll_data.LoadTexture(textureName);
            if (tmpTexture == null) tmpTexture = default;

            TerrainTexturesArray[i] = tmpTexture;
        }
    }
    private void InitTerrainPool()
    {
        terrainPool = new List<SpoolChunkData>();
        loadedChunks = new List<SpoolChunkData>();
        TerrainPoolParent = new GameObject("World Pool");

        //FillTerrainSpool(10);
    }

    private void FillTerrainSpool(int ChunkCount)
    {
        terrainPool = new List<SpoolChunkData>();
        for (int i = 0; i < ChunkCount; i++)
        {
            Vector2Int pos = new Vector2Int(i, i);
            //GameObject spoolChunk = GetNewChunk(pos.x, pos.y, CHUNK_SIZE);
            //spoolChunk.SetActive(false);
            //terrainPool.Add(new SpoolChunkData(pos, spoolChunk));
            terrainPool.Add(GetChunkFromSpool(pos, CHUNK_SIZE));
        }
    }

    private struct SpoolChunkData
    {
        public Vector2Int pos;
        public GameObject chunk;

        public SpoolChunkData(Vector2Int pos, GameObject chunk)
        {
            this.pos = pos;
            this.chunk = chunk;
            //this.chunkData = chunkData;
            //if (this.chunkData == null)
            //{
            //    this.chunkData = new();
            //    this.chunkData.SetUnit(chunk);
            //}
        }
    }

    private void CleanupLoadedChunks()
    {
        List<SpoolChunkData> cleanedLoadedChunks = new List<SpoolChunkData>();
        List<SpoolChunkData> returnList = new List<SpoolChunkData>();
        foreach (SpoolChunkData spoolChunkData in loadedChunks)
        {
            if (Vector2Int.Distance(CurrentChunkPos, spoolChunkData.pos) <= 3)
            {
                cleanedLoadedChunks.Add(spoolChunkData);
            }
            else
            {
                //spoolChunkData.chunk.transform.parent = TerrainPoolParent.transform;
                //spoolChunkData.chunk.SetActive(false);
                //terrainPool.Add(spoolChunkData);
                //ReturnToTerrainPool(spoolChunkData);
                returnList.Add(spoolChunkData);
            }
        }

        foreach(SpoolChunkData spoolChunkData in returnList)
        {
            ReturnToTerrainPool(spoolChunkData);
        }
        loadedChunks = cleanedLoadedChunks;
    }

    private void LoadChunksArea(Vector2Int pos, int side = 5)
    {
        LoadChunksAreaAsync(pos, side);
    }
    /// <summary>
    /// Загрузить асинхронно группу террайнов размером sideXside блоков в центре pos
    /// </summary>
    /// <param name="pos">Номер блока по x и y</param>
    /// <param name="side">количество блоков по каждой стороне</param>
    private async void LoadChunksAreaAsync(Vector2Int pos, int side = 1)
    {
        Debug.Log("Loading Area @" + pos);
        for (int y = -side; y <= side; y++)
        {
            for (int x = -side; x <= side; x++)
            {
                //Vector2Int newPos = pos + new Vector2Int(x - side / 2 - 1, y - side / 2 - 1);
                Vector2Int newPos = pos + new Vector2Int(x, y);

                if (newPos.x < 0) continue;
                if (newPos.y < 0) continue;
                bool res=LoadChunk(newPos);
                Debug.Log("Loading " + newPos +" "+ (res ? "success":"failure"));
                await Task.Yield();
            }
        }
        Debug.Log("Area " + pos + "loaded");
        CleanupLoadedChunks();
    }


    private SpoolChunkData GetChunkFromSpool(int startX, int startY, int size)
    {
        Vector2Int pos = new Vector2Int(startX, startY);
        return GetChunkFromSpool(pos, size);
    }

    private SpoolChunkData GetChunkFromSpool(Vector2Int pos, int size)
    {
        SpoolChunkData chunkData;
        GameObject chunk;
        if (terrainPool.Count <= 0) {
            chunk = GetNewChunk(pos.x, pos.y, size);
            //loadedChunks.Add(new SpoolChunkData(pos, chunk));
            //return chunk;
            chunkData = new SpoolChunkData(pos, chunk);
            terrainPool.Add(chunkData);
        }

        //int index = terrainPool.Count - 1;
        //foreach (SpoolChunkData chunkData in terrainPool)
        //for (int i=0;i<terrainPool.Count;i++)
        //{
        //    SpoolChunkData chunkData = terrainPool[i];
        //    if (chunkData.pos == pos)
        //    {
        //        //chunkData.chunk.SetActive(true);
        //        //terrainPool.Remove(chunkData);
        //        //return chunkData.chunk;
        //        index = i;

        //    }
        //}

        chunkData = terrainPool.Last();

        //chunkData = terrainPool[index];
        //terrainPool.RemoveAt(index);
        //loadedChunks.Add(chunkData);
        //Debug.Log("Terrain spool size: " + terrainPool.Count);
        GetFromTerrainPool(chunkData);
        UpdateChunk(chunkData.chunk, pos.x, pos.y, size);
        chunkData.chunk.SetActive(true);
        return chunkData;
    }
    
    private void ReturnToTerrainPool(SpoolChunkData spoolChunkData)
    {
        loadedChunks.Remove(spoolChunkData);
        terrainPool.Add(spoolChunkData);
        spoolChunkData.chunk.transform.parent = TerrainPoolParent.transform;
    }
    private void GetFromTerrainPool(SpoolChunkData spoolChunkData)
    {
        terrainPool.Remove(spoolChunkData);
        loadedChunks.Add(spoolChunkData);
        spoolChunkData.chunk.transform.parent = World.transform;
    }

    private bool IsLoaded(Vector2Int pos)
    {
        foreach (SpoolChunkData chunkData in loadedChunks)
        {
            if (pos == chunkData.pos) return true;
        }
        return false;
    }
    private bool LoadChunk(Vector2Int pos,int scale = 1)
    {
        if (IsLoaded(pos)) return true;

        SpoolChunkData chunkData = GetChunkFromSpool(pos.x, pos.y, CHUNK_SIZE);
        //Debug.Log(pos + " is " + chunk.activeSelf);

        //chunkData.chunkData.SetOrg(new Vector3(pos.x * CHUNK_SIZE * TerrainDefs.SQUARE_SIZE, 0 - (CHUNK_SIZE * TerrainDefs.SQUARE_SIZE / 2), pos.y * CHUNK_SIZE * TerrainDefs.SQUARE_SIZE));
        //chunkData.chunkData.SetOrg(new Vector3(pos.x * CHUNK_SIZE * TerrainDefs.SQUARE_SIZE, 0 - (short.MaxValue - short.MinValue)/2*TerrainDefs.HeightScale, pos.y * CHUNK_SIZE * TerrainDefs.SQUARE_SIZE));
        //scene.AddActor(chunkData.chunkData);
        //scene.AddItem(chunkData.chunkData);
        //loadedChunks.Add(new SpoolChunkData(pos, chunk));
        return true;
    }

    private Vector2Int GetObjectChunkXY(Vector3 pos)
    {
        Vector2Int res = new Vector2Int
        {
            x = (int)System.Math.Floor(pos.x / (TerrainDefs.SQUARE_SIZE * CHUNK_SIZE)),
            y = (int)System.Math.Floor(pos.z / (TerrainDefs.SQUARE_SIZE * CHUNK_SIZE))
        };

        return res;

    }

    public void Initialize()
    {
        if (std == null)
        {
            //LoadHeader();
            std = new TERRAIN_DATA(mapName);
            std.OpenHdr();
            std.OpenSq(true, false);
            std.OpenVb(true, false);
        }
        InitMaterials();

        World = new GameObject
        {
            name = "World"
        };
        InitTerrainPool();
    }
    public void LoadHeader()
    {
        std = new TERRAIN_DATA(mapName);
        std.OpenHdr();

        //string message = mapName + " Terrain size: " + std.Header.SizeXBPages * TerrainDefs.BOXES_PAGE_SIZE * TerrainDefs.SQUARES_IN_BOX + "x" + std.Header.SizeZBPages * TerrainDefs.BOXES_PAGE_SIZE * TerrainDefs.SQUARES_IN_BOX;
        //Debug.Log(message);

        //Debug.Log(std);
        //std.OpenSQ(false,false);
        //Debug.Log(std.Squares.pager.Get(100, 100));
    }

    private void InitMaterials()
    {
        //Stream stream = GameDataHolder.GetResource<Stream>(PackType.rData, "default#terrmtl");
        //TODO брать из настроек террайна.
        Stream stream = dll_data.files.GetBlock("default#terrmtl").myStream;
        TerrainMtlCfg terrainMtlCfg = StormFileUtils.ReadStruct<TerrainMtlCfg>(stream);

        TerrainTextures = new string[std.Header.nMaterials];
        SurfDatas = new SurfData[std.Header.nMaterials];
        //foreach (int MaterialId in std.Materials.SurType)
        for (int i = 0; i < std.Header.nMaterials; i++)
        {
            int SurfaceType = std.Materials.SurType[i];
            SurfaceDesc surfaceDesc = StormFileUtils.ReadStruct<SurfaceDesc>(stream, stream.Position);
            //Debug.Log("Material ID:" + SurfaceType.ToString("X8"));
            //Debug.Log(surfaceDesc);
            //TerrainTextures[i] = GameDataHolder.GetNameById(PackType.TexturesDB, surfaceDesc.texture);
            TerrainTextures[i] = dll_data.files.CompleteObjId(surfaceDesc.texture);
            SurfDatas[i] = new SurfData
            {
                name = TerrainTextures[i],
                terrainType = (TerrainDefs.GroundType)SurfaceType
            };
        }

        InitTextures();
    }
    [System.Serializable]
    public struct SurfData
    {
        public string name;
        public TerrainDefs.GroundType terrainType;
    }

    public struct SectorData
    {
        public float[,] sectorHeightMap;
        public int[,] terrainTypes;
        public int[] terrainTypesEnum;
    }

    private T_SQUARE[] GetDataArray(int startX, int startY, int size = CHUNK_SIZE)
    {
        //Debug.Log($"Loading from {startX}:{startY}");
        T_SQUARE[] dataArray = new T_SQUARE[size * size];
        T_VBOX[] Vboxes = new T_VBOX[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                dataArray[y * size + x] = std.Squares.pager.Get(startX + x, startY + y);
            }
        }


        //if (!File.Exists(cacheFileName)) StormFileUtils.SaveXML<T_SQUARE[]>(cacheFileName, dataArray);
        //Debug.Log((dataArray.Length,cnt));
        return dataArray;
    }

    private SectorData GetSectorData(int startX, int startY, int size)
    {
        //Debug.Log($"Loading {startX}:{startY} {size}");
        T_SQUARE data;
        int groundType;

        float[,] sectorHeightMap = new float[size + 1, size + 1];
        List<int> TTE = new();
        int[,] splatmap = new int[size + 1, size + 1];

        T_SQUARE[] dataArray = GetDataArray(startX*CHUNK_SIZE, startY*CHUNK_SIZE, size + 1);

        for (int y = 0; y < size + 1; y++)
        {
            for (int x = 0; x < size + 1; x++)
            {
                //data = tmpstd.Squares.pager.Get(startX + x, startY + y);
                data = dataArray[y * (size + 1) + x];
                sectorHeightMap[y, x] = NormalizeHeight(data.Height);
                //sectorHeightMap[y, x] = 0;
                //Color heatColor = new Color(sectorHeightMap[y, x], 0, 1 - sectorHeightMap[y, x], 0);
                //heatTexture.SetPixel(x, y, heatColor);
                groundType = data.Flag & TerrainDefs.SQF_GRMASK;

                splatmap[x, y] = groundType;
                if (!TTE.Contains(groundType)) TTE.Add(groundType);
            }
        }


        SectorData res = new SectorData();
        res.sectorHeightMap = sectorHeightMap;
        res.terrainTypes = splatmap;
        res.terrainTypesEnum = TTE.ToArray();


        return res;
    }


    private void UpdateChunk(GameObject chunk, Vector2Int coords, int size)
    {
        UpdateChunk(chunk, coords.x, coords.y, size);
    }
    private void UpdateChunk(GameObject chunk, int startX, int startY, int size)
    {
        //SectorData data = await Task.Run(() => GetSectorData(chunk, startX, startY, size));
        SectorData data = GetSectorData(startX, startY, size);
        //SectorData data = await Task.Run(() => GetSectorData(startX, startY, size));


        //Debug.Log($"Loading sector [{startX}:{startY}] " + data.terrainTypesEnum.Length);
        int LayersCount = data.terrainTypesEnum.Length;
        float[,,] splatmap2 = new float[size + 1, size + 1, LayersCount];
        TerrainLayer[] terrainLayers = new TerrainLayer[LayersCount];

        for (int i = 0; i < LayersCount; i++)
        {
            TerrainLayer tmpLayer = new TerrainLayer();
            //tmpLayer.name = "Layer " + i + " " + TerrainTextures[i];
            //Debug.Log("Using terrain type: " + i);
            tmpLayer.name = TerrainTextures[data.terrainTypesEnum[i]];
            //tmpLayer.tileSize = new Vector2(CHUNK_SIZE * TerrainDefs.SQUARE_SIZE / 4, CHUNK_SIZE * TerrainDefs.SQUARE_SIZE / 4);
            tmpLayer.tileSize = new Vector2(TerrainDefs.BOX_SIZE, TerrainDefs.BOX_SIZE);
            tmpLayer.diffuseTexture = TerrainTexturesArray[data.terrainTypesEnum[i]];
            terrainLayers[i] = tmpLayer;

            int groundType;
            for (int y = 0; y < size + 1; y++)
            {
                for (int x = 0; x < size + 1; x++)
                {
                    //groundType = splatmap[y, x];
                    groundType = data.terrainTypes[y, x];
                    splatmap2[x, y, i] = 0;
                    if (groundType == data.terrainTypesEnum[i]) splatmap2[x, y, i] = 1;
                }
            }
        }
        TerrainData terrainData = chunk.GetComponent<Terrain>().terrainData;
        terrainData.name = "TDATA " + startX + " " + startY;
        terrainData.SetHeights(0, 0, data.sectorHeightMap);


        //TerrainLayer[] terrainLayers = terrainData.terrainLayers;
        //terrainLayers[0].diffuseTexture = heatTexture;

        terrainData.terrainLayers = terrainLayers;
        terrainData.SetAlphamaps(0, 0, splatmap2);
        chunk.name = "Terrain " + startX + " " + startY;
    }

    private GameObject GetNewChunk(int startX, int startY, int size)
    {
        TerrainData terrainData = new TerrainData();
        terrainData.baseMapResolution = CHUNK_SIZE;
        terrainData.heightmapResolution = size + 1;
        terrainData.alphamapResolution = size + 1;
        terrainData.SetDetailResolution(1024, CHUNK_SIZE);
        terrainData.name = "TDATA " + startX + " " + startY;
        //terrainData.size = new Vector3(size * TerrainDefs.SQUARE_SIZE, size * TerrainDefs.SQUARE_SIZE, size * TerrainDefs.SQUARE_SIZE);
        terrainData.size = new Vector3(size * TerrainDefs.SQUARE_SIZE, (short.MaxValue-short.MinValue) * TerrainDefs.HeightScale, size * TerrainDefs.SQUARE_SIZE);

        GameObject terrain = Terrain.CreateTerrainGameObject(terrainData);
        terrain.SetActive(false);
        terrain.name = "Terrain [" + startX + " " + startY + "]";

        //terrain.transform.position = new Vector3(0, (0 - terrainData.size.y / 2), 0); //set terrain on default water level
        //terrain.transform.parent = transform;
        terrain.transform.parent = World.transform;

        //UpdateChunk(terrain, startX, startY, size);
        //terrain.SetActive(true);
        return terrain;
    }

    private float NormalizeHeight(short height)
    {
        //        short minHeight = -32768; //min short value
        //      short maxHeight = 32767;//max short value
        short minHeight = short.MinValue;
        short maxHeight = short.MaxValue;

        float NormalizedHeight = (float)((height - minHeight) / (float)(maxHeight - minHeight));

        return NormalizedHeight;
    }
}
