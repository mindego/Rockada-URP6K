using UnityEngine;

public interface TexMap : IObject
{
    public const uint id = 0x72DA51BA;
    public Texture2D GetTexture();
};