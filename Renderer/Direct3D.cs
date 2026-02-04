using System;
using UnityEditor.SceneManagement;
using UnityEngine;
using static D3DEMULATION;
using static MissionPatch;
using static renderer_dll;
using DWORD = System.UInt32;
using static D3DRENDERSTATETYPE;
using UnityEngine.Rendering;

public class Direct3D7 : IDirect3D7
{
    private int RefCount;
    public void AddRef()
    {
        RefCount++;
    }

    /*
     

HRESULT CreateVertexBuffer(
  [in]          UINT                   Length,
  [in]          DWORD                  Usage,
  [in]          DWORD                  FVF,
  [in]          D3DPOOL                Pool,
  [out, retval] IDirect3DVertexBuffer9 **ppVertexBuffer,
  [in]          HANDLE                 *pSharedHandle
);
    */
    public HRESULT CreateVertexBuffer(D3DVERTEXBUFFERDESC vbDesc, out IDirect3DVertexBuffer7 vb, int v)
    {
        vb = new IDirect3DVertexBuffer7(vbDesc);
        return HRESULT.S_OK;
    }

    public int Release()
    {
        return --RefCount;
    }

    public HRESULT CreateDevice(out IDirect3DDevice7 lpDevice)
    {
        lpDevice = new Direct3DDevice7();
        return HRESULT.S_OK;
    }

    public Direct3D7()
    {
        RefCount = 1;
    }
}

public class Direct3DDevice7 : IDirect3DDevice7
{
    private int RefCount;
    public void AddRef()
    {
        RefCount++;
    }

    public HRESULT GetCaps(out D3DDEVICEDESC7 caps)
    {
        caps = new D3DDEVICEDESC7();

        return HRESULT.S_OK;
    }

    public HRESULT GetDirect3D(out IDirect3D7 lpD3D)
    {
        lpD3D = new Direct3D7();
        return HRESULT.S_OK;
    }

    public HRESULT LightEnable(int d3dindex, bool v)
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        return --RefCount;
    }

    public HRESULT SetLight(int d3dindex, ref D3DLIGHT7 l)
    {
        throw new System.NotImplementedException();
    }

    public HRESULT SetRenderState(D3DRENDERSTATETYPE State, DWORD Value)
    {
        //TODO реализовать внятно установку рендера.
        return HRESULT.S_OK;
    }

    public HRESULT SetTransform(D3DTRANSFORMSTATETYPE State, D3DMATRIX value)
    {
        throw new NotImplementedException();
    }

    public Direct3DDevice7()
    {
        RefCount = 1;
    }

}

public struct D3DMATRIX
{
    //union {
    //    struct {
    //float _11, _12, _13, _14;
    //float _21, _22, _23, _24;
    //float _31, _32, _33, _34;
    //float _41, _42, _43, _44;

    //};
    //float m[4][4];

    float[][] m;

    public D3DMATRIX(float[][] _m=null)
    {
        if (_m!=null)
        {
            m = _m;
            return;
        }

        m = new float[4][];
        for (int i = 0;i<4;i++)
        {
            for (int j = 0;j<4;j++)
            {
                m[i][j] = 0;
            }
        }
    }
};

public struct RCaps
{
    public bool use_32bpp_textures;
    public bool use_pal_textures;
    public bool use_dxt_textures;
    public bool use_mipmapped_textures;

    public bool use_embm;

    public bool use_wbuffer;

    public bool use_hw_raster;
    public bool use_hw_tnl;

    public int TextureDetail;

    public int MaxTextureWidth;
    public int MaxTextureHeight;

    public bool use_anitialiasing;
    //D3DANTIALIASMODE aatype;
};


/// <summary>
/// Класс-эмулятор базового класса D3D "Шторма"
/// </summary>
public class Cd3d
{
    #region constants
    public const uint CD3D_ZBUFFER = 0x00000001; // Create and use a zbuffer
    public const uint CD3D_STENCILBUFFER = 0x00000002; // Use a z-buffer w/stenciling
    public const uint CD3D_NO_FPUSETUP = 0x00000004; // Don't use default DDSCL_FPUSETUP flag
    #endregion

    Textures.ITextureFactory mTextureFactory;

    #region emulated d3d
    //TODO - нужно загружать через CreateDirect3D, а не создавать здесь
    //D3DDEVICEDESC7 device_desc = new();        // The DirectD3Device description
    D3DDEVICEDESC7 device_desc;        // The DirectD3Device description
    IDirect3D7 lpD3D;             // The Direct3D object
    IDirect3DDevice7 lpDevice;          // The D3D device

    public RCaps caps;//alowed/able to use
    #endregion
    public void preInit()
    {
        vbmanager = null;
    }

    iVBManager CreateVBManager(IDirect3DDevice7 lpD3D)
    {
        //VBManager manager = new VBManagerObject();
        VBManager manager = new VBManager();
        if (manager.Initialize(lpD3D))
            return manager;
        Log.Message("VBManager : Can't create VBManager.");
        manager.Release();
        return null;
    }

    HRESULT Create3DDevice()
    {
        //STUB
        return lpD3D.CreateDevice(out lpDevice);
    }

    public string Init(object value1, IRendererConfig rc, DWORD Flags)
    {
        preInit();

        //STUB!
        //vbmanager = new VBManager();
        CreateEnvironment(rc, Flags);
        vbmanager = CreateVBManager(lpDevice);

        //        string hr = CreateEnvironment(lp_driverguid, &DeviceGUID, rc, Flags);
        //if (hr!=null || (hr = SetUp()) || !(vbmanager = CreateVBManager(lpDevice)))
        //{
        //    if (!hr) hr = "Can't create vbmanager!";
        //    Done();
        //}


        return null;
        //mTextureFactory = Textures.CreateTextureFactory(Log);
        //caps.use_32bpp_textures = rc->GetUse32BppTex();
        //caps.use_pal_textures = 0;
        //caps.use_dxt_textures = rc->GetUseDXTn();
        //caps.use_embm = rc->GetUseEMBM();
        //caps.use_mipmapped_textures = rc->GetUseMipMap();
        //caps.use_wbuffer = 0;
        //caps.use_hw_tnl = false;
        //caps.use_hw_raster = false;
        //caps.TextureDetail = rc->GetTextureDetail();
        //caps.use_anitialiasing = rc->GetUseAA();
        //caps.aatype = D3DANTIALIAS_NONE;
    }

    /// <summary>
    /// Create the Direct3D interface
    /// </summary>
    /// <param name="dwFlags"></param>
    /// <returns></returns>
    public string CreateDirect3D(DWORD dwFlags)
    {
        //Упрощённый вариант для обеспечения достаточной функциональности
        // Query DirectDraw for access to Direct3D
        lpD3D = new Direct3D7();
        device_desc = new D3DDEVICEDESC7();
        return null;
    }

    /// <summary>
    /// Desc: Creates the internal objects for the framework
    /// </summary>
    /// <param name="pDriverGUID"></param>
    /// <param name="pDeviceGUID"></param>
    /// <param name="rc"></param>
    /// <param name="dwFlags"></param>
    /// <returns></returns>
    string CreateEnvironment(IRendererConfig rc, DWORD dwFlags)
    {
        //STUB!
        CreateDirect3D(dwFlags);
        Create3DDevice();

        //string hr;

        // // Create the DDraw object
        // if (hr = CreateDirectDraw(pDriverGUID, dwFlags)) return hr;

        // // Create the Direct3D object
        // if (pDeviceGUID)
        //     if (hr = CreateDirect3D(pDeviceGUID, dwFlags)) return hr;

        // // Create the front and back buffers, and attach a clipper
        // int width = rc->GetWidth();
        // int height = rc->GetHeight();
        // int bpp = rc->GetBPP();
        // if (hr = Fullscreen ? CreateFullscreenBuffers(width, height, bpp, dwFlags, rc->GetRefreshRate())
        //                      : CreateWindowedBuffers(width, height, dwFlags)) return hr;

        // // If there is no device GUID, then the app only wants 2D, so we're done
        // if (0 == pDeviceGUID) return "No Device GUID";

        // // Query the render buffer for the 3ddevice
        // if (hr = Create3DDevice(pDeviceGUID)) return hr;

        // // Create and set the viewport
        // D3DVIEWPORT7 vp = { 0, 0, RenderWidth, RenderHeight, 0.0f, 1.0f };

        // HRESULT hr2 = SetViewport(vp);

        // if (FAILED(hr2))
        // {
        //     DEBUG_MSG(DDErr(hr2));
        //     return "Error: Couldn't set current viewport to device";
        // }

        // return 0;
        return null;
    }

    /// <summary>
    /// Desc: Cleans everything up upon deletion. This code returns an error
    /// if any of the objects have remaining reference counts.
    /// </summary>
    /// <returns></returns>
    string Done()
    {
        return null;
    }

    public TextureFactory CreateTexturesFactory()
    {
        TextureFactory f = new TextureFactory();
        f.d3d = this;
        return f;
    }

    internal float Dx()
    {
        return Screen.width;
    }
    internal float Dy()
    {
        return Screen.height;
    }

    internal IDirect3DDevice7 Device()
    {
        return lpDevice;
    }

    internal void Validate(string v)
    {
        return; //Да, в оригинале этот так, остальное закомментировано.

        //DWORD NumPass;
        //HRESULT hr = lpDevice->ValidateDevice(&NumPass);
        //if (FAILED(hr) || (NumPass != 1))
        //{
        //    Log->Message("%s - validate failed: %s ( num_passes %d).\n", s, DDErr(hr), NumPass);
        //    if (FAILED(hr))
        //    {
        //        DEBUG_MSG(s);
        //        DEBUG_MSG("Validate :");
        //        DEBUG_MSG(DDErr(hr));
        //    }
        //    AssertBp(0);
        //}
    }

    internal int MaxLights()
    {
        return device_desc.dwMaxActiveLights;
    }

    private Texture2D currentTexture;
    internal void SetTexture(Texture2D texture, int stage = 0)
    {
        currentTexture = texture;
        //TODO обрабатывать Stage для ээээ. мультитекстур? Детальных текстур? Bump?
    }

    /*
    HRESULT Cd3d::SetTexture( DDSurface *_surface, int stage)  {
  DDSurface *surface=_surface?_surface:GetNullTexture( stage );
  HRESULT hr=(surface!=current_tex[stage]) ? 
    lpDevice->SetTexture(stage, current_tex[stage]=surface) : S_OK;
#if defined _DEBUG
  Validate("SetTexture");
#endif
  return hr;
}
    */
    internal Texture2D GetTexture()
    {
        return currentTexture;
    }

    private D3DMATERIAL7 currentMaterial;
    public void SetMaterial(D3DMATERIAL7 material)
    {
        currentMaterial = material;
    }
    public D3DMATERIAL7 GetMaterial()
    {
        return currentMaterial;
    }

    internal void DrawIndexedVB(IDirect3DVertexBuffer7 direct3DVertexBuffer7, int num_vtx, ushort[] idxs, int num_idx, int start_vtx)
    {
        StormUnityRenderer.DrawIndexedMesh(direct3DVertexBuffer7, num_vtx, idxs, num_idx, start_vtx);
    }

    public HRESULT FogColor(DWORD color) { return lpDevice.SetRenderState(D3DRENDERSTATE_FOGCOLOR, color); }

    public iVBManager vbmanager;
}

public enum D3DCULL {
    D3DCULL_NONE = 1,
    D3DCULL_CW = 2,
    D3DCULL_CCW = 3
}
public enum D3DTRANSFORMSTATETYPE
{
    D3DTRANSFORMSTATE_WORLD         = 1,
    D3DTRANSFORMSTATE_VIEW = 2,
    D3DTRANSFORMSTATE_PROJECTION = 3,
    D3DTRANSFORMSTATE_WORLD1 = 4,
    D3DTRANSFORMSTATE_WORLD2 = 5,
    D3DTRANSFORMSTATE_WORLD3 = 6,
    D3DTRANSFORMSTATE_TEXTURE0 = 16,
    D3DTRANSFORMSTATE_TEXTURE1 = 17,
    D3DTRANSFORMSTATE_TEXTURE2 = 18,
    D3DTRANSFORMSTATE_TEXTURE3 = 19,
    D3DTRANSFORMSTATE_TEXTURE4 = 20,
    D3DTRANSFORMSTATE_TEXTURE5 = 21,
    D3DTRANSFORMSTATE_TEXTURE6 = 22,
    D3DTRANSFORMSTATE_TEXTURE7 = 23,
    D3DTRANSFORMSTATE_FORCE_DWORD = 0x7fffffff
}


public enum D3DRENDERSTATETYPE
{
    D3DRENDERSTATE_ANTIALIAS = 2,    //Antialiasing mode 
    D3DRENDERSTATE_TEXTUREPERSPECTIVE = 4,    //Perspective correction
    D3DRENDERSTATE_ZENABLE = 7,    //Enable z test 
    D3DRENDERSTATE_FILLMODE = 8,    //Fill mode 
    D3DRENDERSTATE_SHADEMODE = 9,    //Shade mode 
    D3DRENDERSTATE_LINEPATTERN = 10,   //Line pattern 
    D3DRENDERSTATE_ZWRITEENABLE = 14,   //Enable z writes
    D3DRENDERSTATE_ALPHATESTENABLE = 15,   //Enable alpha tests 
    D3DRENDERSTATE_LASTPIXEL = 16,   //Draw last pixel in a line 
    D3DRENDERSTATE_SRCBLEND = 19,   //Blend factor for source 
    D3DRENDERSTATE_DESTBLEND = 20,   //Blend factor for destination 
    D3DRENDERSTATE_CULLMODE = 22,   //Back-face culling mode 
    D3DRENDERSTATE_ZFUNC = 23,   //Z-comparison function 
    D3DRENDERSTATE_ALPHAREF = 24,   //Reference alpha value 
    D3DRENDERSTATE_ALPHAFUNC = 25,   //Alpha-comparison function 
    D3DRENDERSTATE_DITHERENABLE = 26,   //Enable dithering 
    D3DRENDERSTATE_ALPHABLENDENABLE = 27,   //Enable alpha blending 
    D3DRENDERSTATE_FOGENABLE = 28,   //Enable fog 
    D3DRENDERSTATE_SPECULARENABLE = 29,   //Enable specular highlights 
    D3DRENDERSTATE_ZVISIBLE = 30,   //Enable z-checking 
    D3DRENDERSTATE_STIPPLEDALPHA = 33,   //Enable stippled alpha 
    D3DRENDERSTATE_FOGCOLOR = 34,   //Fog color 
    D3DRENDERSTATE_FOGTABLEMODE = 35,   //Fog mode 
    D3DRENDERSTATE_FOGTABLESTART = 36,   //Fog table start (same as D3DRENDERSTATE_FOGSTART)
    D3DRENDERSTATE_FOGTABLEEND = 37,   //Fog table end (same as D3DRENDERSTATE_FOGEND)
    D3DRENDERSTATE_FOGTABLEDENSITY = 38,   //Fog table density (same as D3DRENDERSTATE_FOGDENSITY)
    D3DRENDERSTATE_FOGSTART = 36,   //Fog start (for both vertex and pixel fog) 
    D3DRENDERSTATE_FOGEND = 37,   //Fog end (for both vertex and pixel fog)   
    D3DRENDERSTATE_FOGDENSITY = 38,   //Fog density (for both vertex and pixel fog)
    D3DRENDERSTATE_EDGEANTIALIAS = 40,   //Antialias edges 
    D3DRENDERSTATE_COLORKEYENABLE = 41,   //Enable color-key transparency 
    D3DRENDERSTATE_ZBIAS = 47,   //Z-bias 
    D3DRENDERSTATE_RANGEFOGENABLE = 48,   //Enables range-based fog 
    D3DRENDERSTATE_STENCILENABLE = 52,   //Enable or disable stenciling 
    D3DRENDERSTATE_STENCILFAIL = 53,   //Stencil operation 
    D3DRENDERSTATE_STENCILZFAIL = 54,   //Stencil operation 
    D3DRENDERSTATE_STENCILPASS = 55,   //Stencil operation 
    D3DRENDERSTATE_STENCILFUNC = 56,   //Stencil comparison function 
    D3DRENDERSTATE_STENCILREF = 57,   //Reference value for stencil test 
    D3DRENDERSTATE_STENCILMASK = 58,   //Mask value used in stencil test 
    D3DRENDERSTATE_STENCILWRITEMASK = 59,   //Stencil buffer write mask 
    D3DRENDERSTATE_TEXTUREFACTOR = 60,   //Texture factor 
    D3DRENDERSTATE_WRAP0 = 128,  //Wrap flags for 1st texture coord set 
                                 // Wrap render states 1 through 6 omitted here.
    D3DRENDERSTATE_WRAP7 = 135,  //Wrap flags for last texture coord set 
    D3DRENDERSTATE_CLIPPING = 136, //Enable or disable primitive clipping 
    D3DRENDERSTATE_LIGHTING = 137, //Enable or disable lighting 
    D3DRENDERSTATE_EXTENTS = 138, //Enable or disable updating screen extents 
    D3DRENDERSTATE_AMBIENT = 139, //Ambient color for scene 
    D3DRENDERSTATE_FOGVERTEXMODE = 140, //Fog mode for vertex fog 
    D3DRENDERSTATE_COLORVERTEX = 141, //Enable or disable per-vertex color 
    D3DRENDERSTATE_LOCALVIEWER = 142, //Enable or disable perspective specular highlights  
    D3DRENDERSTATE_NORMALIZENORMALS = 143, //Enable automatic normalization of vertex normals 
    D3DRENDERSTATE_COLORKEYBLENDENABLE = 144, //Enable or disable alpha-blended color keying
    D3DRENDERSTATE_DIFFUSEMATERIALSOURCE = 145, //Location for per-vertex diffuse color 
    D3DRENDERSTATE_SPECULARMATERIALSOURCE = 146, //Location for per-vertex specular color 
    D3DRENDERSTATE_AMBIENTMATERIALSOURCE = 147, //Location for per-vertex ambient color 
    D3DRENDERSTATE_EMISSIVEMATERIALSOURCE = 148, //Location for per-vertex emissive color 
    D3DRENDERSTATE_VERTEXBLEND = 151, //Multi-matrix vertex blending mode
    D3DRENDERSTATE_CLIPPLANEENABLE = 152, //Enable one or more user-defined clipping planes
    D3DRENDERSTATE_FORCE_DWORD = 0x7fffffff,
}
