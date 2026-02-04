using UnityEngine;
using DWORD = System.UInt32;

public interface ILenzFlare : IObject
{
    new public const uint ID = 0x60C036D6;

    public void SetIntensity(float f);
    public void SetColor(Vector3 v);
    public void SetPosition(Vector3 v);

    public IHashObject GetHashObject();
};


public interface ILenzFlare2 : ILenzFlare
{
    public void activate(bool b);
};

public abstract class IShader : IObject
{
    public class Desc
    {
        int NumLayers() { return mNumLayers; }
        bool IsSolid() { return mFlags.Get( (int) F_SOLID)!=0; }

        protected const DWORD F_SOLID = 0x00000001;
        protected Flags mFlags = new Flags();
        protected int mNumLayers;
    };

    public abstract Desc GetDesc();
    public abstract ILayer GetLayer(int i);
    public abstract void AddRef();
    public abstract int Release();
};