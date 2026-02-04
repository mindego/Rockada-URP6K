using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// main api
/// </summary>
public interface IDataHasher : IRefMem
{
    public const uint DATA_HASHER_VERSION = 0x00010003;
    public int HashRoads(IDataBlock roads);
    public int HashRoads(System.IO.Stream roads);
    public int UnHashRoads();
    public int HashData(int grp_count, GROUP_DATA[] gd);
    public int UnHashData();
    public void CleanData();
    public void EnumCrosses(ref List<EnumPosition> _pos_list, uint _flags, RoadElementEnumer e);
    public int CalcPath(Vector3 source, Vector3 dest, ref ROADPOINT[] buffer, out Vector3 global_started);
};

public class DataHasherDefines
{
    public const uint RS_MODE_MUST_DELETED = 0x00000200;
    public const uint RS_MODE_MUST_CREATED = 0x00000100;
    public const uint RS_MODE_ALREADY_CREATED = 0x00000080;

    public const uint RS_CLASS_ROAD = 0x00000001;
    public const uint RS_CLASS_GROUP = 0x00000002;
    public const uint RS_CLASS_BRIDGE = 0x00000004;
    public const uint RS_CLASS_TUNNEL = 0x00000008;

    public const uint RS_ALL_GROUPS_RELATED = (RS_CLASS_GROUP);
    public const uint RS_ALL_ROADS_RELATED = (RS_CLASS_ROAD | RS_CLASS_BRIDGE | RS_CLASS_TUNNEL);
    public const uint RS_BUILDING_RELATED = (RS_CLASS_BRIDGE | RS_CLASS_TUNNEL);
    public const uint RS_PATH_RELATED = (RS_CLASS_ROAD | RS_CLASS_BRIDGE | RS_CLASS_TUNNEL);

    public static Vector3 INFINITE_VECTOR = new Vector3(-1000000, 0, -1000000);
    public const float SEARCH_ROAD_RADIUS = 1500f;
    public const float SEARCH_PATH_DIST = 500f;

    public static uint RSObjectId(uint RSFID_) { return HashFlags.OF_GROUP_ROADSYSTEM | RSFID_; }
}

public interface IVisualPart : IHashObject
{
    public void SetVisual(RSection _vis);
    public RSection GetVisual();
    public ROADDATA Data();
    public VisPolyDecalData GetVisualData();
};
