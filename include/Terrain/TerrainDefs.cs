using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


public class TerrainDefs
{
    public const uint VBF_NO_LAYER = 0xff;
    public const int BOXES_CLAMP = 1;

    //public const string MAPDIR = "Data/Scenes/";
    //public const string MAPDIR = "Data/";
    public const int T_MAX_LIGHTMAPS = 32;
    public const int GT_WATER = 0;
    public const int GT_SAND = 1;
    public const int GT_DESERT = 2;
    public const int GT_GROUND = 3;
    public const int GT_GRASS = 4;
    public const int GT_ROCKS = 5;
    public const int GT_SNOW = 6;
    public const int GT_PEAK = 7;

    public const int SQUARES_PAGE_SIZE = 32;
    public const int BOXES_PAGE_SIZE = 32;
    public const int LIGHT_PAGE_SIZE = 32;
    public const int VBOXES_PAGE_SIZE = 16;

    public const int SQUARES_IN_BOX = 4;
    public const int BPAGE_IN_VBPAGES = (BOXES_PAGE_SIZE / VBOXES_PAGE_SIZE);
    public const int BPAGE_IN_LPAGES = (BOXES_PAGE_SIZE / LIGHT_PAGE_SIZE);

    public const float HeightScale = 1f / 5f;

    public const float SQUARE_SIZE = 64f;
    public const float OO_SQUARE_SIZE = 1f / 64f;      // 1./SQUARE_SIZE;
    /// <summary>
    /// Размер бокса террайна
    /// </summary>
    public const float BOX_SIZE = 4f * 64f;      // SQUARES_IN_BOX*SQUARE_SIZE;
    public const float OO_BOX_SIZE = 1f / (4f * 64f); // s1/BOX_SIZE;

    public const int PASS_MASK = 0x1800;
    public const int SQF_SIMMETRY = (1 << 15);
    public const int SQF_GRMASK = (0x07);

    public const uint T_LIGHT = 32;
    public const int DRAW_WATER = (0x10);

    public enum GroundType
    {
        GT_WATER,
        GT_SAND,
        GT_DESERT,
        GT_GROUND,
        GT_GRASS,
        GT_ROCKS,
        GT_SNOW,
        GT_PEAK
    }

    public enum DataSize
    {
        T_SQUARES = 4,
        T_BOXES = 4,
        T_VBOXES = 16
    }
}

public struct T_SQUARE
{
    public short Height;
    public ushort Flag;

    public T_SQUARE(short height, ushort flag)
    {
        Height = height;
        Flag = flag;
    }

    int getRandomIndex()
    {
        return (Flag >> 4) & 63;
    }
    void setRandomIndex(ushort idx)
    {
        /*Flag = (ushort) (Flag & 0x9807);
        Flag += (ushort) (idx * 16); */
        Flag = (ushort)((Flag & 0x9807) + idx * 16);
    }
}
public struct T_BOX
{
    public short Hi;
    public short Lo;
};
/// <summary>
/// Хранилище crc32-идентификаторов поверхностей террайна (32 шт максимум)
/// </summary>
public struct T_MAT
{
    public int[] SurType; //32

    public T_MAT(int maxsize = 32)
    {
        SurType = new int[maxsize];
    }
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct T_VBOX
{
    public short flags;
    public short water_level,
          water_hi,
          water_lo;
    //char[] material; //4
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] material;
    public int blend;
};

public struct T_LightDesc
{
    public Vector3 dir;
    public int is_valid;

    public T_LightDesc(Vector3 dir, int is_valid)
    {
        this.dir = dir;
        this.is_valid = is_valid;
    }
}


public struct T_HEADER
{
    public int SizeXBPages;
    public int SizeZBPages;
    public T_LightDesc[] light_maps; //T_MAX_LIGHTMAPS size;
    public int nMaterials;

    public T_HEADER(int sizeXBPages, int sizeZBPages, T_LightDesc[] light_maps, int nMaterials)
    {
        SizeXBPages = sizeXBPages;
        SizeZBPages = sizeZBPages;
        this.light_maps = light_maps;
        this.nMaterials = nMaterials;
    }

    public T_HEADER(int sizeXBPages = 0, int sizeZBPages = 0, int nMaterials = 0) : this(sizeXBPages, sizeZBPages, new T_LightDesc[TerrainDefs.T_MAX_LIGHTMAPS], nMaterials)
    {
    }
}