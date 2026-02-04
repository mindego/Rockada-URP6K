using UnityEngine;
using static renderer_dll;

public class StdFpoLayer : FpoLayer
{
    public StdFpoLayer()
    {
        mTexture = null;
    }
    ~StdFpoLayer()
    {
        Dispose();
    }

    public override void Dispose()
    {
        //IRefMem.SafeRelease(mTexture);
        mTexture = null; ;
        base.Dispose();
    }
    public override HRESULT Apply()
    {
        base.Apply();
        d3d.SetTexture(mTexture);
        return HRESULT.S_OK;
    }

    public Texture2D mTexture;
};
