using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using static MaterialStorage;
using static renderer_dll;
using static TerrainDefs;
public class StormTerrainUtils
{
    public string mapName;
    /// <summary>
    /// Размер (сторона квадата) чанка террайна в блоках T_SQUARE
    /// </summary>
    public static int CHUNK_SIZE;// = 128;
    /// <summary>
    /// Размер (сторона квадрата) одного "бокса"
    /// </summary>
    public static int BOX_SIZE;// = CHUNK_SIZE / TerrainDefs.SQUARES_IN_BOX;
    private TERRAIN_DATA mpTerrain;
    private Texture2D[] TerrainTextures;
    private SurfData[] SurfDatas;

    static private GameObject WorldPool = new GameObject("World Terrain Pool");
    static private GameObject World = new GameObject("Terrain");
    private ObjectPool<GameObject> TerrainGameObjectPool = new ObjectPool<GameObject>(onCreatePoolObject, onTakePoolObject, onReleasePoolObject, onDestroyPoolObject);
    private ObjectPool<GameObject> UnityWaterPool = new ObjectPool<GameObject>(onCreateWaterPoolObject, onTakeWaterPoolObject, onReleaseWaterPoolObject, onDestroyWaterPoolObject);

    private static void onReleaseWaterPoolObject(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.parent = WorldPool.transform;
    }

    private static void onTakeWaterPoolObject(GameObject obj)
    {
        obj.transform.parent = World.transform;
        obj.SetActive(true);
        return;
    }

    private static void onDestroyWaterPoolObject(GameObject obj)
    {
        GameObject.Destroy(obj);
    }

    private static GameObject onCreateWaterPoolObject()
    {
        GameObject waterchunk = GameObject.CreatePrimitive(PrimitiveType.Quad);
        waterchunk.name = "Water template";

        return waterchunk;
    }

    private static void onDestroyPoolObject(GameObject obj)
    {
        GameObject.Destroy(obj);
    }

    private static void onTakePoolObject(GameObject obj)
    {
        obj.transform.parent = World.transform;
        obj.SetActive(true);
        return;
    }

    private static void onReleasePoolObject(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.parent = WorldPool.transform;
    }

    //static Material terrainMaterial;

    private static GameObject onCreatePoolObject()
    {
        TerrainData terrainData = new TerrainData();
        terrainData.baseMapResolution = CHUNK_SIZE;
        terrainData.heightmapResolution = CHUNK_SIZE + 1;
        terrainData.SetDetailResolution(512, 32);
        terrainData.name = "TDATA template";
        //terrainData.size = new Vector3(size * TerrainDefs.SQUARE_SIZE, size * TerrainDefs.SQUARE_SIZE, size * TerrainDefs.SQUARE_SIZE);
        terrainData.size = new Vector3(CHUNK_SIZE * TerrainDefs.SQUARE_SIZE, (short.MaxValue - short.MinValue) * TerrainDefs.HeightScale, CHUNK_SIZE * TerrainDefs.SQUARE_SIZE);
        //terrainData.alphamapResolution = CHUNK_SIZE + 1;//+1
        terrainData.alphamapResolution = BOX_SIZE + 1;//+1
        //terrainData.alphamapResolution = (int)terrainData.size.x;


        GameObject terrain = Terrain.CreateTerrainGameObject(terrainData);
        Terrain td = terrain.GetComponent<Terrain>();
        td.basemapDistance = 5000;
        //td.materialTemplate = MaterialStorage.myRendererConfig.TerrainMaterial;
        //Terrain.activeTerrain.basemapDistance = 2000;
        terrain.SetActive(false);
        td.drawInstanced = true;
        terrain.name = "Terrain template";
        //if (terrainMaterial == null)
        //{
        ////Shader terrainShader = Shader.Find("Universal Render Pipeline/Terrain/Lit"); //URP
        //    Shader terrainShader = Shader.Find("HDRP/TerrainLit"); //HDRP
        //    terrainMaterial = new Material(terrainShader);
        //}
        //td.materialTemplate = terrainMaterial; 
        td.materialTemplate = MaterialStorage.myRendererConfig.TerrainMaterial;
        return terrain;
    }

    public StormTerrainUtils(string mapName, TERRAIN_DATA mpTerrain, int chunk_size = 128)
    {
        this.mapName = mapName;
        CHUNK_SIZE = chunk_size;
        BOX_SIZE = CHUNK_SIZE / TerrainDefs.SQUARES_IN_BOX;
        this.mpTerrain = mpTerrain;
    }

    public void Initialize()
    {
        InitMaterials();
        //InitTextures();
    }
    //private void InitTextures()
    //{
    //    if (TerrainTextures.Length == 0) return;
    //    TerrainTexturesArray = new Texture2D[TerrainTextures.Length];
    //    Texture2D tmpTexture;
    //    for (int i = 0; i < TerrainTextures.Length; i++)
    //    {
    //        string textureName = TerrainTextures[i];
    //        //tmpTexture = GameDataHolder.GetResource<Texture2D>(PackType.TexturesDB, textureName);
    //        tmpTexture = Renderer.sdr.LoadTexture(textureName);
    //        if (tmpTexture == null) tmpTexture = default;

    //        TerrainTexturesArray[i] = tmpTexture;
    //    }
    //}

    private void InitMaterials()
    {
        //Stream stream = GameDataHolder.GetResource<Stream>(PackType.rData, "default#terrmtl");
        //Stream stream = Renderer.sdr.files.GetBlock("default#terrmtl").myStream;
        Stream stream = dll_data.files.GetBlock("continent#terrmtl").myStream;
        TerrainMtlCfg terrainMtlCfg = StormFileUtils.ReadStruct<TerrainMtlCfg>(stream);

        //TerrainTextures = new string[mpTerrain.Header.nMaterials];
        TerrainTextures = new Texture2D[mpTerrain.Header.nMaterials];
        SurfDatas = new SurfData[mpTerrain.Header.nMaterials];
        //foreach (int MaterialId in std.Materials.SurType)
        for (int i = 0; i < mpTerrain.Header.nMaterials; i++)
        {
            int SurfaceType = mpTerrain.Materials.SurType[i];
            SurfaceDesc surfaceDesc = StormFileUtils.ReadStruct<SurfaceDesc>(stream, stream.Position);
            //Debug.Log("Surface [" +i +"]\n"+ surfaceDesc);
            //Debug.Log(surfaceDesc);
            TerrainTextures[i] = dll_data.LoadTexture(surfaceDesc.texture);
            //TerrainTextures[i] = dll_data.loadFpoTexture(surfaceDesc.texture);
            if (TerrainTextures[i] == null) Debug.Log("Failed to load texture " + surfaceDesc);
            SurfDatas[i] = new SurfData
            {
                name = TerrainTextures[i] != null ? TerrainTextures[i].name : "EMPTY",
                terrainType = (TerrainDefs.GroundType)SurfaceType
            };
        }
    }

    public Vector2Int GetObjectChunkXY(Vector3 pos)
    {
        Vector2Int res = new Vector2Int
        {
            x = (int)System.Math.Floor(pos.x / (TerrainDefs.SQUARE_SIZE * CHUNK_SIZE)),
            y = (int)System.Math.Floor(pos.z / (TerrainDefs.SQUARE_SIZE * CHUNK_SIZE))
        };

        return res;
    }

    public GameObject GetTerrainChunk(Vector2Int pos)
    {
        //GameObject chunk = GetNewTerrainChunk(pos);
        //GameObject chunk = GetFromSpool(pos);
        GameObject chunk = TerrainGameObjectPool.Get();
        UpdateChunk(chunk, pos);
        return chunk;
    }


    public void PutToSpool(GameObject chunk)
    {
        //chunk.transform.parent = WorldPool.transform;
        //chunk.SetActive(false);
        //Spool.Add(chunk);
        TerrainGameObjectPool.Release(chunk);
    }

    private float GetHeight(int x, int y)
    {
        return HeightScale * mpTerrain.Squares.pager.Get(x, y).Height;
    }

    private SectorData GetSectorData(Vector2Int pos)
    {
        T_SQUARE sq;
        T_VBOX vbx;
        int SQoffsetX = pos.x * CHUNK_SIZE;
        int SQoffsetY = pos.y * CHUNK_SIZE;
        int VBOXoffsetX = pos.x * BOX_SIZE;
        int VBOXoffsetY = pos.y * BOX_SIZE;
        SectorData res = new SectorData();
        res.terrainTypes = new int[CHUNK_SIZE + 1, CHUNK_SIZE + 1];
        res.groundMaterial = new int[BOX_SIZE + 1, BOX_SIZE + 1];
        res.sectorHeightMap = new float[CHUNK_SIZE + 1, CHUNK_SIZE  + 1];
        //res.sectorHeightMap = new float[CHUNK_SIZE*2 + 2, CHUNK_SIZE*2  + 2];
        res.sectorWaterHeightMap = new float[BOX_SIZE + 1, BOX_SIZE + 1];
        List<int> TTE = new();
        int groundType;
        for (int z = 0; z < CHUNK_SIZE + 1; z++)
        {
            for (int x = 0; x < CHUNK_SIZE + 1; x++)
            {
                //sq = mpTerrain.Squares.pager.Get(SQoffsetX + x, SQoffsetY + z);
                sq = mpTerrain.Squares.pager.GetCl(SQoffsetX + x, SQoffsetY + z);
                //vbx= mpTerrain.VBoxes.pager.Get((SQoffsetX + x)/TerrainDefs.SQUARES_IN_BOX, (SQoffsetY + y)/ TerrainDefs.SQUARES_IN_BOX);
                //vbx = mpTerrain.VBoxes.pager.Get((SQoffsetX + x) / 2, (SQoffsetY + y) / 2);
                //throw new Exception(vbx.material[0].ToString());
                //res.sectorHeightMap[z , x ] = NormalizeHeight(sq.Height);

                float H= NormalizeHeight(sq.Height);
                //float Hx = NormalizeHeight(mpTerrain.Squares.pager.GetCl(SQoffsetX + x + 1, SQoffsetY + z).Height);
                //float Hz = NormalizeHeight(mpTerrain.Squares.pager.GetCl(SQoffsetX + x, SQoffsetY + z + 1).Height);
                //float Hxz = NormalizeHeight(mpTerrain.Squares.pager.GetCl(SQoffsetX + x + 1, SQoffsetY + z + 1).Height);

                res.sectorHeightMap[z, x] = H;

                //Debug.LogFormat("Max: {0} z*2 {1} x*2 {2}",CHUNK_SIZE*2 +1,z*2,x*2);
                //res.sectorHeightMap[z * 2, x * 2] = H;
                //res.sectorHeightMap[z * 2, x * 2 + 1] = (H + Hx) * 0.5f;
                //res.sectorHeightMap[z * 2 + 1, x * 2] = (H + Hz) * 0.5f;
                //res.sectorHeightMap[z * 2 + 1, x * 2 + 1] = (H + Hxz) * 0.5f;

                ushort Flag = sq.Flag;
                


                groundType = sq.Flag & TerrainDefs.SQF_GRMASK;
                //groundType = vbx.material[0];
                res.terrainTypes[x, z] = groundType;
                //if (!TTE.Contains(groundType)) TTE.Add(groundType);
            }
        }

        for (int z = 0; z < BOX_SIZE + 1; z++)
        {
            for (int x = 0; x < BOX_SIZE + 1; x++)
            {
                vbx = mpTerrain.VBoxes.pager.Get(VBOXoffsetX + x, VBOXoffsetY + z);
                res.groundMaterial[x, z] = vbx.material[0];
                if (!TTE.Contains(vbx.material[0])) TTE.Add(vbx.material[0]);
                //res.sectorWaterHeightMap[x, y] = NormalizeHeight(vbx.water_level) * TerrainDefs.HeightScale;
                res.sectorWaterHeightMap[x, z] = vbx.water_level * TerrainDefs.HeightScale;
            }
        }

        //BuildTexture("Chunk-" + pos.x + "-" + pos.y, res.groundMaterial);
        res.terrainTypesEnum = TTE.ToArray();
        return res;
    }

    private Color[] CreateTexturesPalette()
    {
        Color[] palette = {
            new Color(0, 0, 50), //0
            new Color(0, 50, 0),         // 1
            new Color(0, 70, 0) ,          // 2
            new Color(0, 90, 0),           // 3
            new Color(0, 110, 0),          // 4
            new Color(50, 50, 0),         // 5
            new Color(70, 70, 0),         // 6
            new Color(90, 90, 0),         // 7
            new Color(110, 110, 0),         // 8
            new Color(130, 130, 0),         // 9
            new Color(150, 150, 0),         //10
            new Color(170, 170, 0),         // 1
            new Color(190, 190, 0),         // 2
            new Color(110, 0, 0),          // 3
            new Color(130, 0, 0),           // 4
            new Color(150, 0, 0),           // 5
            new Color(50, 50, 50),         // 6
            new Color(70, 70, 70),         // 7
            new Color(90, 90, 90),         // 8
            new Color(110, 110, 110),      // 9
            new Color(130, 130, 130),      //20
            new Color(150, 150, 150),       // 1
            new Color(170, 170, 170),       // 2
            new Color(30, 0, 30),         // 3
            new Color(50, 0, 50),         // 4
            new Color(70, 0, 70),         // 5
            new Color(0, 250, 0),          // 6 Forest G250
            new Color(110, 0, 110),       // 7
            new Color(0, 200, 0),          // 8 Forest G200
            new Color(150, 0, 150)        // 9
            };
        return palette;
    }
    //private void BuildTexture(string name, int[,] materials)
    //{
    //    Color[] palette = CreateTexturesPalette();
    //    int width = materials.GetLength(0);
    //    int height = materials.GetLength(1);
    //    Texture2D texture = new Texture2D(width, height);
    //    for (int y = 0; y < width; y++)
    //    {
    //        for (int x = 0; x < height; x++)
    //        {
    //            texture.SetPixel(x, y, palette[materials[x, y]]);
    //        }
    //    }
    //    AssetDatabase.CreateAsset(texture, "Assets/" + name + ".texture2d");
    //}
    public struct SurfData
    {
        public string name;
        public TerrainDefs.GroundType terrainType;
    }
    public struct SectorData
    {
        public float[,] sectorHeightMap;
        public float[,] sectorWaterHeightMap;
        public int[,] terrainTypes;
        public int[,] groundMaterial;
        public int[] terrainTypesEnum;
    }

    private T_SQUARE[] GetDataArray(int startX, int startY, int size = -1)
    {
        if (size == -1) size = CHUNK_SIZE;
        //Debug.Log($"Loading from {startX}:{startY}");
        T_SQUARE[] dataArray = new T_SQUARE[size * size];
        T_VBOX[] Vboxes = new T_VBOX[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                dataArray[y * size + x] = mpTerrain.Squares.pager.Get(startX + x, startY + y);
            }
        }


        //if (!File.Exists(cacheFileName)) StormFileUtils.SaveXML<T_SQUARE[]>(cacheFileName, dataArray);
        //Debug.Log((dataArray.Length,cnt));
        return dataArray;
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
    private Texture2D[] serviceTextures = new Texture2D[4];
    private Texture2D GetColorTexture(int size, int colorIndex)
    {
        if (serviceTextures[colorIndex] != null) return serviceTextures[colorIndex];
        Color[] colorValues = { Color.red, Color.green, Color.blue, Color.black };


        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = colorValues[colorIndex];
        }
        texture.SetPixels(pixels);
        texture.Apply();
        serviceTextures[colorIndex] = texture;
        return texture;
    }

    //const int LayersCount = 3; //Возможно 4, если RGBA
    //private float[,,] GenerateSplatmap(SectorData data)
    //{

    //    int size = BOX_SIZE + 1;
    //    int groundType;
    //    Texture2D currentTexture;
    //    float[,,] splatmap = new float[BOX_SIZE + 1, BOX_SIZE + 1, LayersCount];
    //    for (int y = 0; y < size; y++)
    //    {
    //        for (int x = 0; x < size; x++)
    //        {
    //            //groundType = splatmap[y, x];
    //            groundType = data.groundMaterial[y, x];
    //            currentTexture = TerrainTextures[groundType];
    //            Color pixel = currentTexture.GetPixel(x % GetTextureScale() / GetTextureScale(), y % GetTextureScale());
    //            splatmap[y, x, 0] = pixel.r;
    //            splatmap[y, x, 1] = pixel.g;
    //            splatmap[y, x, 2] = pixel.b;
    //            //splatmap[x, y, 3] = pixel.a;
    //        }
    //    }
    //    return splatmap;
    //}

    private int GetTextureScale() // TODO Возвращать из TerrainStateCfg
    {
        return 4;
    }

    private float[,,] GenerateSplatmap(SectorData data, int LayersCount)
    {
        int groundType;
        Dictionary<int, int> inverted = new Dictionary<int, int>();
        for (int i = 0; i < LayersCount; i++)
        {
            inverted.Add(data.terrainTypesEnum[i], i);
        }

        int size = BOX_SIZE + 1;
        float[,,] splatmap = new float[size, size, LayersCount];

        //TerrainDefs.GT_ *
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                //groundType = splatmap[y, x];
                groundType = data.groundMaterial[y, x];

                splatmap[x, y, inverted[groundType]] = 1;
            }
        }
        return splatmap;
    }
    private void UpdateChunk(GameObject chunk, Vector2Int pos)
    {
        SectorData data = GetSectorData(pos);
        //int LayersCount = TerrainTextures.Length;
        int LayersCount = data.terrainTypesEnum.Length;

        //float[,,] splatmap;
        float[,,] splatmap;//= new float[BOX_SIZE + 1, BOX_SIZE + 1, LayersCount];
        TerrainLayer[] terrainLayers = new TerrainLayer[LayersCount];

        for (int i = 0; i < LayersCount; i++)
        {
            TerrainLayer tmpLayer = new TerrainLayer();
            Texture2D tmpTexture = TerrainTextures[data.terrainTypesEnum[i]];
            tmpLayer.name = tmpTexture.name;
            tmpLayer.diffuseTexture = tmpTexture;
            //tmpLayer.name = TerrainTextures[i] != null ? TerrainTextures[i].name : "EMPTY";
            //tmpLayer.diffuseTexture = TerrainTextures[i];

            //tmpLayer.name = colornames[i];
            //tmpLayer.diffuseTexture = GetColorTexture(TerrainTextures[0].width, i);
            tmpLayer.tileSize = new Vector2(TerrainDefs.BOX_SIZE, TerrainDefs.BOX_SIZE);
            terrainLayers[i] = tmpLayer;

            //int groundType;
            //for (int y = 0; y < CHUNK_SIZE / TerrainDefs.SQUARES_IN_BOX; y++)
            //{
            //    for (int x = 0; x < CHUNK_SIZE / TerrainDefs.SQUARES_IN_BOX; x++)
            //    {
            //        //groundType = splatmap[y, x];
            //        groundType = data.groundMaterial[y, x];
            //        splatmap[x, y, i] = 0;
            //        if (groundType == data.terrainTypesEnum[i])
            //        //if (groundType == i)
            //        {
            //            //Debug.Log("Painting " + groundType);
            //            splatmap[x, y, i] = 1;
            //        }
            //    }
            //}
        }
        //int groundType;
        //int size = BOX_SIZE + 1;


        ////TerrainDefs.GT_*
        //for (int y = 0; y < size; y++)
        //{
        //    for (int x = 0; x < size; x++)
        //    {
        //        //groundType = splatmap[y, x];
        //        groundType = data.groundMaterial[y, x];
        //        splatmap[x, y, groundType] = 1;
        //    }
        //}

        splatmap = GenerateSplatmap(data, data.terrainTypesEnum.Length);
        Terrain terrain = chunk.GetComponent<Terrain>();

        TerrainData terrainData = chunk.GetComponent<Terrain>().terrainData;
        terrainData.name = "TDATA " + pos.x + " " + pos.y;
        terrainData.SetHeights(0, 0, data.sectorHeightMap);

        //DetailPrototype grass = new DetailPrototype();
        //grass.minHeight = 10;
        //grass.maxHeight = 15;
        //grass.minWidth = 10;
        //grass.maxWidth = 15;

        //grass.prototypeTexture = Renderer.sdr.LoadTexture("trees"); //TODO взять траву поприличнее (ПЫХ!)

        //int[,] grassmap = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, 0);

        //for (int y = 0; y < terrainData.detailHeight; y++)
        //{
        //    int modY = (int)((float) y / (float) terrainData.detailHeight * (CHUNK_SIZE + 1));
        //    Debug.Log(string.Format("Grassing! {0} {1} {2}", y, terrainData.detailHeight, modY));
        //    for (int x = 0; x < terrainData.detailWidth; x++)
        //    {
        //        int modX = x / terrainData.detailHeight * (CHUNK_SIZE + 1);
        //        if (data.terrainTypes[modX, modY] == TerrainDefs.GT_GRASS)
        //        {
        //            grassmap[x, y] = 10;
        //        }
        //    }
        //}

        //terrainData.detailPrototypes = new DetailPrototype[] { grass };
        //terrainData.SetDetailLayer(0, 0, 0, grassmap);


        //TerrainLayer[] terrainLayers = terrainData.terrainLayers;
        //terrainLayers[0].diffuseTexture = heatTexture;

        terrainData.terrainLayers = terrainLayers;
        terrainData.SetAlphamaps(0, 0, splatmap);

        chunk.name = "Terrain " + pos.x + " " + pos.y;


        //TODO восстановить отрисовку воды
        //GameObject gobj = new GameObject("Water layer");
        GameObject gobj;

        if (chunk.transform.Find("Water layer") == null)
        {
            gobj = new GameObject("Water layer");
        }
        else
        {
            gobj = chunk.transform.Find("Water layer").gameObject;
        }
        //Mesh waterMesh = GenerateWaterMesh(BOX_SIZE + 1, BOX_SIZE + 1, (int)TerrainDefs.BOX_SIZE);
        Mesh waterMesh = GenerateWaterMesh(BOX_SIZE + 1, BOX_SIZE + 1, (int)TerrainDefs.BOX_SIZE, data.sectorWaterHeightMap);
        //MeshFilter mf = gobj.AddComponent<MeshFilter>();
        //MeshRenderer mr = gobj.AddComponent<MeshRenderer>();
        MeshFilter mf;
        MeshRenderer mr;

        if (!gobj.TryGetComponent<MeshFilter>(out mf)) mf = gobj.AddComponent<MeshFilter>();
        if (!gobj.TryGetComponent<MeshRenderer>(out mr)) mr = gobj.AddComponent<MeshRenderer>();

        mf.mesh = waterMesh;
        mr.material = StaticWaterMaterial;

        gobj.transform.parent = chunk.transform;
        gobj.transform.localPosition = new Vector3(0, ((short.MaxValue - short.MinValue) / 2) * TerrainDefs.HeightScale, 0);

    }

    private Mesh GenerateWaterMesh(int sizeX, int sizeZ, int step, float[,] sectorWaterHeightMap)
    {
        Mesh waterMesh = new Mesh();
        waterMesh.name = "Water plane";
        int verticesCount = sizeX * sizeZ;
        int trianglesCount = (sizeX - 1) * (sizeZ - 1) * 2;

        Vector3[] vertices = new Vector3[verticesCount];
        int[] triangles = new int[trianglesCount * 3];
        Vector2[] uvs = new Vector2[verticesCount];

        int vertIndex;
        int trisIndex = 0;
        for (int z = 0; z < sizeZ; z++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                vertIndex = z * sizeX + x;

                vertices[vertIndex] = new Vector3(x * step, sectorWaterHeightMap[x, z], z * step);
                uvs[vertIndex] = new Vector2(x / (sizeX - 1f), z / (sizeZ - 1f));
                if (z == sizeZ - 1 || x == sizeX - 1) continue;

                triangles[trisIndex++] = vertIndex;
                triangles[trisIndex++] = vertIndex + sizeX;
                triangles[trisIndex++] = vertIndex + sizeX + 1;
                triangles[trisIndex++] = vertIndex;
                triangles[trisIndex++] = vertIndex + sizeX + 1;
                triangles[trisIndex++] = vertIndex + 1;
            }
        }

        waterMesh.vertices = vertices;
        waterMesh.triangles = triangles;
        waterMesh.uv = uvs;

        waterMesh.RecalculateBounds();
        waterMesh.RecalculateNormals();
        waterMesh.RecalculateTangents();

        return waterMesh;
    }
    private Mesh GenerateWaterMesh(int sizeX, int sizeZ, int step = 1)
    {
        Mesh waterMesh = new Mesh();
        waterMesh.name = "Water plane";
        int verticesCount = sizeX * sizeZ;
        int trianglesCount = (sizeX - 1) * (sizeZ - 1) * 2;

        Vector3[] vertices = new Vector3[verticesCount];
        int[] triangles = new int[trianglesCount * 3];
        Vector2[] uvs = new Vector2[verticesCount];

        int vertIndex;
        int trisIndex = 0;
        for (int z = 0; z < sizeZ; z++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                vertIndex = z * sizeX + x;
                vertices[vertIndex] = new Vector3(x * step, 0, z * step);
                uvs[vertIndex] = new Vector2(x / (sizeX - 1f), z / (sizeZ - 1f));
                if (z == sizeZ - 1 || x == sizeX - 1) continue;

                triangles[trisIndex++] = vertIndex;
                triangles[trisIndex++] = vertIndex + sizeX;
                triangles[trisIndex++] = vertIndex + sizeX + 1;
                triangles[trisIndex++] = vertIndex;
                triangles[trisIndex++] = vertIndex + sizeX + 1;
                triangles[trisIndex++] = vertIndex + 1;
            }
        }

        waterMesh.vertices = vertices;
        waterMesh.triangles = triangles;
        waterMesh.uv = uvs;
        waterMesh.RecalculateBounds();
        waterMesh.RecalculateNormals();
        waterMesh.RecalculateTangents();

        return waterMesh;
    }
}
