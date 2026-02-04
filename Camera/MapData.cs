using UnityEngine;

public struct MapData
{
    public string Name;
    public float SizeX, SizeY;
    public int MapSizeX, MapSizeY;
};

public interface ICompositeMap : IMemory
{
    public void Flush();
    public void Draw(float X, float Z, float Width, float Height, Color32 color);
};