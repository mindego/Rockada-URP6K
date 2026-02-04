using System;
using static D3DCULL;
using static D3DEMULATION;
using static D3DRENDERSTATETYPE;
using static renderer_dll;
using static TransparencyType;
using DWORD = System.UInt32;

public class FpoLayer : ILayer
{
    protected const DWORD F_FOGCUSTOM = 0x00000001;
    protected const DWORD F_DOUBLESIDED = 0x00000002;

    Flags mFlags = new Flags();
    DWORD mFogColor;
    /// <summary>
    /// RenderState
    /// см. Direct3DDevice7.SetRenderState
    /// </summary>
    iRS mRState;
    /// <summary>
    /// TextureStageState
    /// см. Direct3DDevice7.GetTextureStageState
    /// </summary>
    iRS mTSState;
    D3DMATERIAL7 mMaterial;

    TSState GetTSS() { return null; }

    public FpoLayer()
    {
        mFogColor = 0;
        mRState = null;
        mTSState = null;
        mMaterial = null;
    }
    ~FpoLayer()
    {
        Dispose();
    }

    private bool isDisposed = false;
    public virtual void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;

        IRefMem.SafeRelease(mRState);
        IRefMem.SafeRelease(mTSState);
    }

    public virtual HRESULT Apply()
    {
        mRState.Apply();
        mTSState.Apply();
        d3d.SetMaterial(mMaterial);
        d3d.Device().SetRenderState(D3DRENDERSTATE_CULLMODE, mFlags.Get(F_DOUBLESIDED) != 0 ? (DWORD)D3DCULL_NONE : (DWORD)D3DCULL_CW);
        //TODO реализовать установку параметров тумана
        //Engine.SetFogState(
        //  Engine.GetFogStartDist(),
        //  Engine.GetFogEndDist(),
        //  mFlags.Get(F_FOGCUSTOM) != 0 ? mFogColor : (DWORD)Engine.GetFogColor().PackARGB()
        //  );

        return HRESULT.S_OK;
    }

    public void SetTransparencyType(TransparencyType tt)
    {
        switch (tt)
        {
            case TRT_SOLID:
                mRState = dll_data.CreateRS("std_transp_none");
                break;
            case TRT_ADD:
                mRState = dll_data.CreateRS("std_transp_add");
                SetCustomFogColor(0);
                break;
            case TRT_FILTER:
                mRState = dll_data.CreateRS("std_transp_blend");
                break;
        }
    }

    public void SetDoublesided(bool twosided)
    {
        if (twosided)
            mFlags.Set(F_DOUBLESIDED);
    }

    public void SetCustomFogColor(DWORD fog_color)
    {
        mFlags.Set(F_FOGCUSTOM);
        mFogColor = fog_color;
    }

    public void SetTSState(iRS tss)
    {
        mTSState = tss;
        mTSState.AddRef();
    }

    public void SetMaterial(D3DMATERIAL7 m)
    {
        mMaterial = m;
    }

    public void FreePrivateData()
    {
        throw new NotImplementedException();
    }

    public void AddRef()
    {
        throw new NotImplementedException();
    }

    public int Release()
    {
        throw new NotImplementedException();
    }
}
