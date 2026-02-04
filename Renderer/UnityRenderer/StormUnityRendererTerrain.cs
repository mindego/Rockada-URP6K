using System.Collections.Generic;
using UnityEngine;
using static TerrainDefs;

public class StormUnityRendererTerrain
{
    float TERRAINCENTEROFFSET = StormTerrainUtils.CHUNK_SIZE * TerrainDefs.SQUARE_SIZE * 0.5f;
    float WATERCENTEROFFSET = StormTerrainUtils.CHUNK_SIZE * TerrainDefs.SQUARE_SIZE * 0.5f;
    float DRAW_DISTANCE = Engine.UnityCamera.farClipPlane * 2f;
    public bool inited;

    private TerrainHolder th;
    private TERRAIN_DATA Data;
    private int bxclamp_min, bxclamp_max;
    private int bzclamp_min, bzclamp_max;

    public void Init(TerrainHolder _th, int _bxclamp_min, int _bxclamp_max, int _bzclamp_min, int _bzclamp_max)
    {
        th = _th;
        inited = true;
        bxclamp_min = _bxclamp_min;
        bxclamp_max = _bxclamp_max;
        bzclamp_min = _bzclamp_min;
        bzclamp_max = _bzclamp_max;
        //Debug.Log(("Clamps",_bxclamp_min, _bxclamp_max, _bzclamp_min, _bzclamp_max));
    }
    public void Init(TERRAIN_DATA td, int _bxclamp_min, int _bxclamp_max, int _bzclamp_min, int _bzclamp_max)
    {
        Data = td;
        Init(new TerrainHolder(td, 64), _bxclamp_min, _bxclamp_max, _bzclamp_min, _bzclamp_max);
    }

    public void DrawBox(int ix, int iz)
    {
        int clamped_x = Storm.Math.ClampIntWrapped(ix, bxclamp_min, bxclamp_max, BOXES_CLAMP);
        int clamped_z = Storm.Math.ClampIntWrapped(iz, bzclamp_min, bzclamp_max, BOXES_CLAMP);
        T_BOX box = Data.Boxes.pager.Get(ix, iz);

        float lo = box.Lo * HeightScale;
        float hi = box.Hi * HeightScale;

        float ptx = ix * BOX_SIZE;
        float ptz = iz * BOX_SIZE;

        Vector3 center = new Vector3(ptx + BOX_SIZE * .5f, (lo + hi) * .5f, ptz + BOX_SIZE * .5f);
        float r = Mathf.Sqrt(Mathf.Pow(BOX_SIZE, 2) * 2f + Mathf.Pow((hi - lo), 2)) * .5f;

        int sx = clamped_x * SQUARES_IN_BOX;
        int sz = clamped_z * SQUARES_IN_BOX;

        GameObject stub = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stub.transform.position = Engine.ToCameraReference(new Vector3(sx, 0, sz) + center);

        GameObject.Destroy(stub, .5f);
    }

    private List<StormTerrainChunk> CurrentDrawnChunks = new List<StormTerrainChunk>();
    private List<StormTerrainChunk> PrevDrawnChunks = new List<StormTerrainChunk>();
    private int clipLeft, clipRight, clipUp, clipDown;

    public float SurfaceLevel(float x, float z)
    {
        //TraceResult tr;
        //Data.GroundLevel(x, z, out tr);
        //float r = tr.dist;
        //Data.WaterLevel(x, z, out tr);
        //return (r > tr.dist ? r : tr.dist);
        return Mathf.Max(GroundLevel(x, z), WaterLevel(x, z));
    }
    public float GroundLevel(float x, float z)
    {
        TraceResult tr;
        Data.GroundLevel(x, z, out tr);
        return tr.dist;
    }

    public float WaterLevel(float x, float z)
    {
        TraceResult tr;
        Data.WaterLevel(x, z, out tr);
        return tr.dist;
    }

    public void DrawGroundChunks()
    {
        //float alt = SurfaceLevel(Engine.EngineCamera.Org.x, Engine.EngineCamera.Org.z);

        //if (alt < 100f) //TODO! вместо константы - подсчитывать!
        //{
        //    DrawGroundNearChunks();
        //}
        DrawGroundNearChunks();
       // DrawGroundDistantLand(alt);

        DrawGroundChunksCleanup();
    }

    public void DrawGroundNearChunks()
    {
        ClipArea();
        StormTerrainChunk chunk;
        int size = th.TerrainChunks.Length;
        for (int z = clipDown; z <= clipUp; z++)
        {
            for (int x = clipLeft; x <= clipRight; x++)
            {
                int chunkindex = z * th.chunksX + x;
                if (chunkindex < 0 || chunkindex >= size) continue;
                chunk = th.TerrainChunks[z * th.chunksX + x];
                if (chunk != null)
                {
                    chunk.Draw(); //TODO странное! Возможно, стоит проверять генерировать "вылет за край карты" как-то иначе
                    CurrentDrawnChunks.Add(chunk);
                }
            }
        }
    }

    GameObject DistantLand;
    public void DrawGroundDistantLand(float alt=0)
    {
        if (DistantLand == null)
        {
            DistantLand = GameObject.CreatePrimitive(PrimitiveType.Quad);
            DistantLand.transform.localScale = Vector3.one * 100;
            DistantLand.name = "Distant land";
        }
        Vector3 pos = Engine.UnityCamera.transform.position;
        pos.y -= alt + 50;
        DistantLand.transform.position = pos;
    }



    private Vector2Int GetCameraGridPosition()
    {
        return new Vector2Int((int)Engine.EngineCamera.Org.x, (int)Engine.EngineCamera.Org.z) / (int)(StormTerrainUtils.CHUNK_SIZE * TerrainDefs.SQUARE_SIZE);
    }

    int DrawDistanceCells = -1;
    private void ClipArea()
    {
        Vector2Int CameraGridPosition = GetCameraGridPosition();
        if (DrawDistanceCells == -1) DrawDistanceCells = Mathf.CeilToInt(DRAW_DISTANCE / (StormTerrainUtils.CHUNK_SIZE * TerrainDefs.SQUARE_SIZE));

        clipLeft = Mathf.Clamp(CameraGridPosition.x - DrawDistanceCells, 0, th.chunksX);
        clipRight = Mathf.Clamp(CameraGridPosition.x + DrawDistanceCells, 0, th.chunksX);
        clipUp = Mathf.Clamp(CameraGridPosition.y + DrawDistanceCells, 0, th.chunksZ);
        clipDown = Mathf.Clamp(CameraGridPosition.y - DrawDistanceCells, 0, th.chunksZ);
        //Debug.LogFormat("Clipped Area: x{0}-{1} y{2}-{3}",clipLeft,clipRight,clipDown,clipUp);
    }
    public void DrawGroundChunksCleanup()
    {
        foreach (StormTerrainChunk chunk in PrevDrawnChunks)
        {
            if (CurrentDrawnChunks.Contains(chunk)) continue;
            chunk.Undraw();
        }

        PrevDrawnChunks = CurrentDrawnChunks;
        CurrentDrawnChunks = new List<StormTerrainChunk>();
        //CurrentDrawnChunks.Clear();
    }
}