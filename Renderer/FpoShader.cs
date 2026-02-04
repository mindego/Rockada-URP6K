using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.LightTransport;
using static renderer_dll;
using static StormMesh;
using static TransparencyType;
//using BumpFpoLayerObject = FpoShaderInner<TPrivateData<BumpFpoLayer>>;
//using StdFpoLayerObject = FpoShaderInner<TPrivateData<StdFpoLayer>>;
//using BumpFpoLayerObject = FpoShaderInner<BumpFpoLayer>;
//using StdFpoLayerObject = FpoShaderInner<StdFpoLayer>;

public class FpoShader : IShader, IDisposable
{
    public FpoShader()
    {
        mLayer = null;
    }

    public bool Initialize(MaterialData data)
    {
        Asserts.AssertBp(mLayer == null);

        FpoLayer layer = CreateLayer(data);

        mDesc.SetNumLayers(1);

        TransparencyType trt;
        if (data.flags.Get(MF2_TRANSPARENT) != 0)
        {
            if (data.flags.Get(MF2_ADDITIVE) != 0)
            {
                trt = TRT_ADD;
            }
            else
            {
                trt = TRT_FILTER;
            }
        }
        else
        {
            mDesc.SetSolid();
            trt = TRT_SOLID;
        }
        layer.SetDoublesided(data.flags.Get(MF2_TWOSIDED) != 0);

        layer.SetTransparencyType(trt);

        layer.SetMaterial(dll_data.LoadMaterial(data.mtl_id));

        mLayer = layer;
        return true;
    }
    public void Destroy()
    {
        //SafeDelete(mLayer);
        mLayer.Dispose();
    }

    public override Desc GetDesc()
    {
        return mDesc;
    }
    public override ILayer GetLayer(int l)
    {
        Asserts.AssertBp(l == 0);
        AddRef();
        return mLayer;
    }

    protected class FpoShaderDesc : Desc
    {
        public void SetNumLayers(int numlayers)
        {
            mNumLayers = numlayers;
        }
        public void SetSolid()
        {
            mFlags.Set(F_SOLID);
        }
    };
    protected FpoShaderDesc mDesc = new FpoShaderDesc();
    protected FpoLayer mLayer;
    FpoLayer CreateLayer(MaterialData data)
    {
        Debug.Log("Creating layer using " + data);
        if (data.flags.Get(MF2_BUMP) != 0 && d3d.caps.use_embm)
        {
            BumpFpoLayer layer = new BumpFpoLayerObject(this);
            layer.mBaseTexture = dll_data.loadFpoTexture(data.bmp_id);
            layer.mBumpTexture = dll_data.LoadTexture(data.bump_id);
            iRS tss = dll_data.CreateRS("std_tss_embm");
            layer.SetTSState(tss); tss.Release();
            return layer;
        }
        else
        {
            Debug.Log("Creating StdFpoLayer using " + data);
            StdFpoLayer layer = new StdFpoLayerObject(this);
            layer.mTexture = dll_data.loadFpoTexture(data.bmp_id);
            string tss_name = layer.mTexture != null ? "std_tss_diffuse" : "std_tss_diffuse_no_texture";
            iRS tss = dll_data.CreateRS(tss_name);
            layer.SetTSState(tss); tss.Release();
            return layer;
        }
    }

    public override void AddRef()
    {
        return;
    }

    public override int Release()
    {
        return 0;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
};

public class FpoShaderId
{
    public FpoShaderId() { }
    public FpoShaderId(MaterialData data)
    {
        mMaterialData = data;
    }

    //bool operator <(FpoShaderId s)
    //{
    //    MaterialData ma = mMaterialData;
    //    MaterialData mb = s.mMaterialData;
    //    return
    //      (ma.flags != mb.flags) ? (ma.flags < mb.flags) :
    //      (ma.bmp_id != mb.bmp_id) ? (ma.bmp_id < mb.bmp_id) :
    //      (ma.mtl_id != mb.mtl_id) ? (ma.mtl_id < mb.mtl_id) :
    //      (ma.bump_id != mb.bump_id) ? (ma.bump_id < mb.bump_id) : false;
    //}

    public static bool operator <(FpoShaderId left, FpoShaderId right)
    {
        MaterialData ma = left.mMaterialData;
        MaterialData mb = right.mMaterialData;
        return
            (ma.flags != mb.flags) ? (ma.flags < mb.flags) :
            (ma.bmp_id != mb.bmp_id) ? (ma.bmp_id < mb.bmp_id) :
            (ma.mtl_id != mb.mtl_id) ? (ma.mtl_id < mb.mtl_id) :
            (ma.bump_id != mb.bump_id) ? (ma.bump_id < mb.bump_id) : false;
    }
    public static bool operator >(FpoShaderId left, FpoShaderId right)
    {
        MaterialData ma = left.mMaterialData;
        MaterialData mb = right.mMaterialData;
        return
            (ma.flags != mb.flags) ? (ma.flags > mb.flags) :
            (ma.bmp_id != mb.bmp_id) ? (ma.bmp_id > mb.bmp_id) :
            (ma.mtl_id != mb.mtl_id) ? (ma.mtl_id > mb.mtl_id) :
            (ma.bump_id != mb.bump_id) ? (ma.bump_id > mb.bump_id) : false;
    }
    public MaterialData mMaterialData;
};

public enum TransparencyType
{
    TRT_SOLID,
    TRT_ADD,
    TRT_FILTER
};

interface TSState { }; //Да, в исходниках это так

public class FpoShaderInner<T> : TWrapObject<FpoShader, T>
{
    public FpoShaderInner(FpoShader shader) : base(shader)  {}

};

//using BumpFpoLayerObject = FpoShaderInner<TPrivateData<BumpFpoLayer>>;
//using StdFpoLayerObject = FpoShaderInner<TPrivateData<StdFpoLayer>>;
//using BumpFpoLayerObject = FpoShaderInner<BumpFpoLayer>;
//using StdFpoLayerObject = FpoShaderInner<StdFpoLayer>;
public class BumpFpoLayerObject : BumpFpoLayer
{
    public BumpFpoLayerObject() : base() { }
    public BumpFpoLayerObject(object stub) : base() { }
}

public class StdFpoLayerObject : StdFpoLayer
{
    public StdFpoLayerObject() : base() { }
    public StdFpoLayerObject(object stub) : base() { }
}