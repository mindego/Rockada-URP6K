using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using crc32value = System.UInt32;
using DWORD = System.UInt32;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TerrainStateCfg : IStormImportable<TerrainStateCfg>
{
    public DWORD StaticLightingMode;

    public int texture_scale;

    public int detail_enabled;
    public int detail_scale;
    public float detail_start;
    public float detail_end;
    public int water_lod;

    public TerrainStateCfg Import(Stream st)
    {
        return StormFileUtils.ReadStruct<TerrainStateCfg>(st);
    }

    public override string ToString()
    {
        string res = this.GetType().ToString();
        res += "\ntexture_scale: " + texture_scale;
        return res;
    }
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TerrainCfg : IStormImportable<TerrainCfg>
{
    public const int TSLM_NORMAL = 0;
    public const int TSLM_EEI = 1;

    public crc32value material;
    public crc32value state;
    public crc32value features;
    public crc32value unused;

    public TerrainCfg Import(Stream st)
    {
        return StormFileUtils.ReadStruct<TerrainCfg>(st);
    }
}
public struct SurfaceDesc
{
    public crc32value texture;
    public crc32value material;
    public crc32value rs;
    float unused;

    public override string ToString()
    {
        string res = "Surface texture " + texture.ToString("X8") + "\n";
        res += "Material texture " + material.ToString("X8") + "\n";
        res += "rs " + rs.ToString("X8") + "\n";
        return res;
    }
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TerrainMtlCfg : IStormImportable<TerrainMtlCfg>
{
    public crc32value main_rs;
    public crc32value blend_rs;

    public SurfaceDesc DetailSurface;
    public crc32value detail_rs;

    public SurfaceDesc WaterSurface;
    public crc32value water_rs;

    public float WavePeriod;

    const int MaxSurfaces = 32; //static
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    public SurfaceDesc[] GroundSurfaces; //32

    public TerrainMtlCfg Import(Stream st)
    {
        TerrainMtlCfg terrainMtlCfg = StormFileUtils.ReadStruct<TerrainMtlCfg>(st);

        terrainMtlCfg.GroundSurfaces = new SurfaceDesc[32];
        for (int i = 0; i < 32; i++)
        {
            terrainMtlCfg.GroundSurfaces[i] = StormFileUtils.ReadStruct<SurfaceDesc>(st);
        }
        return terrainMtlCfg;
    }
}
