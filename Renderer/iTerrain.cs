using geombase;
using WORD = System.UInt16;
using DWORD = System.UInt32;

public interface iTerrain
{
    //time
    public void Update(float scale);

    //visualization
    public void Prepare();
    public void DrawGround();
    public void DrawWater();
    public IMeshExporter CreateMeshExporter();
    int GetMaxRasterSize(float R);
    RasterizeData GetRaster(Matrix34f Cam, int[] aleft, int[] aright);
    Rect GetRect(float[] mArea);
    Rect ClipRect(Rect v);
    int RectVtxCount(Rect mRect);
    int RectTriCount(Rect mRect);
    void ExportColors(Rect mRect, ref Stride<DWORD> stride);
    void ExportVertices(Rect mRect, ref Stride<UnityEngine.Vector3> position);
    void ExportNormals(Rect mRect, ref Stride<UnityEngine.Vector3> normal);
    int GetMaxRasterVertices(RasterizeData mRaster);
    int GetMaxRasterIndices(RasterizeData mRaster);
    void ExportRaster(RasterizeData mRaster, ref Stride<UnityEngine.Vector3> pos, int num_vertices, WORD[] tris, int num_indices);
    void ExportTriangles(Rect mRect, ref ushort[] pindices);
};
