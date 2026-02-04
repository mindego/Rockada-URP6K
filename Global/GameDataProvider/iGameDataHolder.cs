using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameDataProvider
{

    public string[] ListContent(PackType packId);
    public Dictionary<string, uint> GetContentDictionary(PackType packId);
    public T GetResource<T>(PackType packId, string name);
    public string GetNameById(PackType packId, uint id);
    public T GetResource<T>(PackType packId, uint id, string name = null);
    public ResourcePack GetPack(PackType packId);

}

public enum PackType
{
    MeshDB,
    TexturesDB,
    MaterialsDB,
    RenderStatesDB,
    MEODB,
    FPODB,
    gData,
    rData,
    ParticlesDB,
    Voice0DB,
    Voice1DB,
    Voice2DB
}

