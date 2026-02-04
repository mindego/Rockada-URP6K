using geombase;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using crc32 = System.UInt32;

public class FpoData : IStormImportable<FpoData>
{
    public Flags flags;
    public crc32 name;
    public uint num_slots;
    public Position pos;
    public ImageData[] images; //STUB!
    public uint tree_next;
    public uint tree_sub;
    public SlotData[] slots; //STUB
    public FpoData next = null;
    public FpoData sub = null;

    //    FpoData* GetNextData() { return tree_next ? (FpoData*)(((char*)(this) + tree_next)) : 0; }
    //  FpoData* GetSubData() { return tree_sub ? (FpoData*)(((char*)(this) + tree_sub)) : 0; }
    public FpoData GetNextData() { return next; }
    public FpoData GetSubData() { return sub; }

    public FpoData Import(Stream st)
    {
        return FpoImport.GetFPOData(st, 0);
    }

    public override string ToString()
    {
        string res = "";
        res += "Name: " + name.ToString("X8") + "\n";
        res += "Num_Slots: " + num_slots.ToString() + "\n";
        res += "Images: \n";

        foreach (ImageData image in images)
        {
            res += image + "\n";
        }
        res += "Tree_next: " + tree_next + "\n";
        res += "Tree_sub: " + tree_sub + "\n";
        res += "Slots: " + slots.Length;
        foreach (SlotData slot in slots)
        {
            res += "Slot: " + slot + "\n";
        }
        return res;
    }
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ImageData
{
    public crc32 graph;
    public crc32 collision;
    public float radius;
    public Vector3 min, max;

    public override string ToString()
    {
        return graph.ToString("X8");
    }

    public Sphere GetBSphere() { return new Sphere((min + max) * .5f, radius); }
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class SlotData
{
    public Flags flags;
    public crc32 name;
    public uint slot_id;
    public Position pos;

    public override string ToString()
    {
        string res = "";
        res += "Name: " + name.ToString("X8");
        res += "Id: " + slot_id.ToString();
        return res;

    }
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class Position
{
    public Vector3 org;
    public Vector3 e1;
    public Vector3 e2;

    public Vector3 GetE3()
    {
        return Vector3.Cross(e1, e2);
    }

    internal void SetMatrix34(out Matrix34f m)
    {
        m = new Matrix34f();
        m.pos = org;
        m.tm[0] = e1;
        m.tm[1] = e2;
        m.tm[2] = GetE3();
    }
}

/// <summary>
/// Класс, содержащий crc32 ресурсов с LOD соответствующего (0-2) уровня.
/// </summary>
public class FpoGraphData : GraphData, IStormImportable<FpoGraphData>
{
    public crc32[] lods; // meshdatas

    public override string ToString()
    {
        string res = "";
        res += "Type:" + type + "\n";
        foreach (crc32 lod in lods)
        {
            res += "Lod: " + lod.ToString("X8") + "\n";
        }
        return res;
    }

    public crc32 GetLod(int index)
    {
        if (index < 0) return default;
        if (index > lods.Length) return default;

        if (lods[index] == 0xFFFFFFFF) return GetLod(index - 1);
        return lods[index];
    }

    public FpoGraphData Import(Stream ms)
    {
        //FpoGraphData fpoGraphData = new FpoGraphData();
        //fpoGraphData.type = FileUtils.ReadStruct<GDType>(ms);
        GraphData graphData = StormFileUtils.ReadStruct<GraphData>(ms);
        FpoGraphData fpoGraphData = new FpoGraphData();
        fpoGraphData.type = graphData.type;

        fpoGraphData.lods = new uint[3];
        for (int i = 0; i < 3; i++)
        {
            fpoGraphData.lods[i] = StormFileUtils.ReadStruct<uint>(ms, (int)ms.Position);
        }
        return fpoGraphData;
    }
}
