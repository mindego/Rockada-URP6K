using UnityEngine;
using System.IO;

public class MaterialImportScript : MonoBehaviour
{
    public string MaterialName;

    public void ImportMaterialAsset()
    {
        ImportMaterialAsset(MaterialName);
    }
    public void ImportMaterialAsset(string matName)
    {
        ResourcePack rp = new ResourcePack();
        rp.Init("Data/Graphics/textures.dat");
        //rp.Init("Data/Graphics/ctrltex.dat");

        rp.LoadRAT();
        Stream ms = rp.GetStreamByName(matName);
    }
}
