using UnityEngine;
using static D3DTRANSFORMSTATETYPE;
using static renderer_dll;

public class BumpFpoLayer : FpoLayer
{
    public BumpFpoLayer()
    {
        mBaseTexture = null;
        mBumpTexture = null;
    }
    ~BumpFpoLayer()
    {
        Dispose();
    }

    private bool isDisposed = false;
    public override void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        //IRefMem.SafeRelease(mBaseTexture);
        //IRefMem.SafeRelease(mBumpTexture);
        base.Dispose();
    }

    public override HRESULT Apply()
    {
        base.Apply();
        d3d.SetTexture(mBaseTexture);
        d3d.SetTexture(mBumpTexture, 1);

        Engine.SetEnvMap(2);

        Matrix4f m_id = new Matrix4f(); m_id.Identity();
        d3d.Device().SetTransform(D3DTRANSFORMSTATE_TEXTURE0, m_id.GetD3dmatrix());
        d3d.Device().SetTransform(D3DTRANSFORMSTATE_TEXTURE1, m_id.GetD3dmatrix());
        d3d.Device().SetTransform(D3DTRANSFORMSTATE_TEXTURE2, Engine.env_transform.GetD3dmatrix());

        return HRESULT.S_OK;
    }

    public Texture2D mBaseTexture;
    public Texture2D mBumpTexture;
    iRS mTSState;
};
