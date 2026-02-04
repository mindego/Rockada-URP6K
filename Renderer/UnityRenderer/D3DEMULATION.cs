using System;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.LightTransport;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.Rendering.HDROutputUtils;
using D3DVALUE = System.Single; //The D3DVALUE type is the fundamental Direct3D fractional data type.
using DWORD = System.UInt32;

public partial class D3DEMULATION //IDirectSound3DBuffer
{
    public const uint DS3D_DEFERRED = 0;
    public const uint DS3D_IMMEDIATE = 1;

    public const uint DS3DMODE_NORMAL = 0;
    public const uint DS3DMODE_HEADRELATIVE = 1;
    public const uint DS3DMODE_DISABLE = 2;
}
public partial class D3DEMULATION //Базовые данные
{
    public class D3DVECTOR
    {
        public D3DVALUE x, y, z;
        //Эмуляция union

        public D3DVALUE dvX
        {
            get { return x; }
        }
        public D3DVALUE dvY
        {
            get { return y; }
        }
        public D3DVALUE dvZ
        {
            get { return z; }
        }

        public D3DVECTOR(D3DVALUE _x, D3DVALUE _y, D3DVALUE _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
    };
    /// <summary>
    /// D3DLIGHT_POINT
    /// Light is a point source. The light has a position in space and radiates light in all directions.
    /// D3DLIGHT_SPOT
    /// Light is a spotlight source. This light is like a point light, except that the illumination is limited to a cone. This light type has a direction and several other parameters that determine the shape of the cone it produces. For information about these parameters, see the D3DLIGHT7 structure.
    /// D3DLIGHT_DIRECTIONAL
    /// Light is a directional source. This is equivalent to using a point light source at an infinite distance.
    /// D3DLIGHT_FORCE_DWORD
    /// Forces this enumeration to compile to 32 bits in size. This value is not used.
    /// Remarks
    /// Directional lights are slightly faster than point light sources, but point lights look a little better. Spotlights offer interesting visual effects but are computationally expensive.
    /// </summary>
    public enum D3DLIGHTTYPE
    {
        D3DLIGHT_POINT = 1,
        D3DLIGHT_SPOT = 2,
        D3DLIGHT_DIRECTIONAL = 3,
        D3DLIGHT_FORCE_DWORD = 0x7fffffff,
    }

    public struct D3DLIGHT7
    {
        //struct D3DLIGHT7
        //{
        //    D3DLIGHTTYPE dltType;
        //    D3DCOLORVALUE dcvDiffuse;
        //    D3DCOLORVALUE dcvSpecular;
        //    D3DCOLORVALUE dcvAmbient;
        //    D3DVECTOR dvPosition;
        //    D3DVECTOR dvDirection;
        //    D3DVALUE dvRange;
        //    D3DVALUE dvFalloff;
        //    D3DVALUE dvAttenuation0;
        //    D3DVALUE dvAttenuation1;
        //    D3DVALUE dvAttenuation2;
        //    D3DVALUE dvTheta;
        //    D3DVALUE dvPhi;
        //};
        public D3DLIGHTTYPE dltType;
        public D3DCOLORVALUE dcvDiffuse;
        public D3DCOLORVALUE dcvSpecular;
        public D3DCOLORVALUE dcvAmbient;
        public D3DVECTOR dvPosition;
        public D3DVECTOR dvDirection;
        public D3DVALUE dvRange;
        public D3DVALUE dvFalloff;
        public D3DVALUE dvAttenuation0;
        public D3DVALUE dvAttenuation1;
        public D3DVALUE dvAttenuation2;
        public D3DVALUE dvTheta;
        public D3DVALUE dvPhi;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class D3DMATERIAL7 : IStormImportable<D3DMATERIAL7>
    {
        /* 
         * Diffuse reflection: Defines how the polygon reflects diffuse lighting (any light that does not come from ambient light). This is described in terms of a color, which represents the color best reflected by the polygon. Other colors are reflected less in proportion to how different they are from the diffuse color.
         * Ambient reflection: Defines how the polygon reflects ambient lighting. This is described in terms of a color, which, as with diffuse reflection, represents the color best reflected by the polygon.
         * Light emission: Makes the polygon appear to emit a certain color of light (this does not actually light up the world; it only changes the appearance of the polygon).
         * Specular highlighting: Describes how shiny the polygon is. A material whose color components are R: 1.0, G: 1.0, B: 1.0, A: 1.0 will reflect all the light that comes its way. Likewise, a material with R: 0.0, G: 1.0, B: 0.0, A: 1.0 will reflect all of the green light that is directed at it. [font="Courier New"][color="#000080"]SetMaterial()[/color][/font] sets the material properties for the device. 
        */
        /// <summary>
        /// Diffuse color RGBA
        /// </summary>
        public D3DCOLORVALUE diffuse; /* Diffuse color RGBA */
        /// <summary>
        /// Ambient color RGB
        /// </summary>
        public D3DCOLORVALUE ambient; /* Ambient color RGB */
        /// <summary>
        /// Specular 'shininess'
        /// </summary>
        public D3DCOLORVALUE specular; /* Specular 'shininess' */
        /// <summary>
        /// Emissive color RGB
        /// </summary>
        public D3DCOLORVALUE emissive;  /* Emissive color RGB */
        /// <summary>
        /// Sharpness if specular highlight
        /// из D3DMATERIAL9:
        /// Floating-point value specifying the sharpness of specular highlights. The higher the value, the sharper the highlight.
        /// </summary>
        public float power;  /* Sharpness if specular highlight */


        public D3DMATERIAL7 Import(Stream st)
        {
            D3DMATERIAL7 StormMaterial = StormFileUtils.ReadStruct<D3DMATERIAL7>(st);
            return StormMaterial;
        }

        public override string ToString()
        {
            return $"Diffuse {diffuse}\nAmbient {ambient}\nSpecular {specular}\nEmissive {emissive}\nPower {power}";
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class D3DCOLORVALUE
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public D3DCOLORVALUE(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
        public D3DCOLORVALUE() : this(0, 0, 0, 0) { }

        public override string ToString()
        {
            return $"Red: {r} Green: {g} Blue: {b} Alpha {a}";
        }

        /// <summary>
        /// Convert into Unity Color with alpha 
        /// </summary>
        /// <returns></returns>
        public Color ToColor()
        {
            Color color = new Color
            {
                r = r,
                g = g,
                b = b,
                a = a
            };
            return color;
        }
        /// <summary>
        /// Convert into Unity Color
        /// </summary>
        /// <returns></returns>
        internal Color ToColorRGB()
        {
            Color color = new Color
            {
                r = r,
                g = g,
                b = b,
            };
            return color;
        }
    }

    /// <summary>
    /// Определяет примитивы, поддерживаемые Direct3D.
    /// </summary>
    public enum D3DPRIMITIVETYPE
    {
        D3DPT_POINTLIST = 1,
        D3DPT_LINELIST = 2,
        D3DPT_LINESTRIP = 3,
        D3DPT_TRIANGLELIST = 4,
        D3DPT_TRIANGLESTRIP = 5,
        D3DPT_TRIANGLEFAN = 6,
        D3DPT_FORCE_DWORD = 0x7fffffff
    }

}

public partial class D3DEMULATION //IDirect3DDevice7
{
    /// <summary>
    /// The IDirect3DDevice7 interface provides methods enabling applications to perform DrawPrimitive-based rendering, in contrast to the IDirect3DDevice interface, which applications use to work with execute buffers. You can create a Direct3DDevice object and retrieve a pointer to this interface by calling the IDirect3D7::CreateDevice method. 
    /// </summary>
    public interface IDirect3DDevice7 : IRefMem
    {
        /// <summary>
        /// The IDirect3DDevice7::GetCaps method retrieves the capabilities of the Direct3D device.
        /// 
        /// If the method succeeds, the return value is D3D_OK.
        /// 
        /// If the method fails, the return value can be one of the following:
        /// DDERR_INVALIDOBJECT
        /// DDERR_INVALIDPARAMS
        /// </summary>
        /// <param name="caps">Address of the D3DDEVICEDESC7 structure to contain the hardware features of the device. </param>
        /// <returns></returns>
        HRESULT GetCaps(out D3DDEVICEDESC7 lpD3DDevDesc);
        /// <summary>
        /// The IDirect3DDevice7::GetDirect3D method retrieves the Direct3D object for this device.
        /// </summary>
        /// <param name="lplpD3D">Address that receives a pointer to the Direct3D object's IDirect3D7 interface when the method returns. </param>
        /// <returns></returns>
        public HRESULT GetDirect3D(out IDirect3D7 lplpD3D);
        public HRESULT LightEnable(int d3dindex, bool v);
        public HRESULT SetLight(int d3dindex, ref D3DLIGHT7 l);
        public HRESULT SetRenderState(D3DRENDERSTATETYPE State, DWORD Value);
        public HRESULT SetTransform(D3DTRANSFORMSTATETYPE State, D3DMATRIX value);
        //internal HRESULT LightEnable(int d3dindex, bool v)
        //{
        //    throw new System.NotImplementedException();
        //}

        //internal HRESULT SetLight(int d3dindex, ref D3DLIGHT7 l)
        //{
        //    throw new System.NotImplementedException();
        //}
    }
}

public partial class D3DEMULATION //D3DDEVICEDESC7
{
    /// <summary>
    /// The D3DDEVICEDESC7 structure contains a description of the current device. This structure is used to query the current device by such methods as IDirect3DDevice7::GetCaps.
    /// https://microsoft.github.io/windows-docs-rs/doc/windows/Win32/Graphics/Direct3D9/struct.D3DDEVICEDESC7.html
    /// </summary>
    public class D3DDEVICEDESC7
    {
        //TODO - Загружать откуда-нибудь данные о эмулируемом д3д устройстве
        public int dwMaxActiveLights = 0x1024;
    }
}

public partial class D3DEMULATION // IDirect3D7
{
    public interface IDirect3D7 : IRefMem
    {
        public HRESULT CreateDevice(out IDirect3DDevice7 lpDevice);
        /// <summary>
        /// The IDirect3D7::CreateVertexBuffer method creates a vertex buffer object.
        /// </summary>
        /// <param name="lpVBDesc">Address of a D3DVERTEXBUFFERDESC structure that describes the format and number of vertices that the vertex buffer receives.</param>
        /// <param name="lplpD3DVertexBuffer">Address of a variable to receive a pointer to a IDirect3DVertexBuffer7 interface for the new vertex buffer.</param>
        /// <param name="dwFlags">No flags are currently defined for this method.This parameter must be 0. </param>
        /// <returns></returns>
        public HRESULT CreateVertexBuffer(D3DVERTEXBUFFERDESC lpVBDesc, out IDirect3DVertexBuffer7 lplpD3DVertexBuffer, int dwFlags);

    }

    /*
     	STDMETHOD(EnumDevices)(THIS_ LPD3DENUMDEVICESCALLBACK7, LPVOID);
	STDMETHOD(CreateDevice)(THIS_ REFCLSID, LPDIRECTDRAWSURFACE7, LPDIRECT3DDEVICE7*);
	STDMETHOD(CreateVertexBuffer)(THIS_ LPD3DVERTEXBUFFERDESC, LPDIRECT3DVERTEXBUFFER7*, DWORD);
	STDMETHOD(EnumZBufferFormats)(THIS_ REFCLSID, LPD3DENUMPIXELFORMATSCALLBACK, LPVOID);
	STDMETHOD(EvictManagedTextures)(THIS);
     */
}

public partial class D3DEMULATION //d3dtypes.h
{
    public const int D3DFVF_RESERVED0 = 0x001;
    public const int D3DFVF_POSITION_MASK = 0x00E;
    public const int D3DFVF_XYZ = 0x002;
    public const int D3DFVF_XYZRHW = 0x004;
    public const int D3DFVF_XYZB1 = 0x006;
    public const int D3DFVF_XYZB2 = 0x008;
    public const int D3DFVF_XYZB3 = 0x00a;
    public const int D3DFVF_XYZB4 = 0x00c;
    public const int D3DFVF_XYZB5 = 0x00e;

    public const int D3DFVF_NORMAL = 0x010;
    public const int D3DFVF_RESERVED1 = 0x020;
    public const int D3DFVF_DIFFUSE = 0x040;
    public const int D3DFVF_SPECULAR = 0x080;
    public const int D3DFVF_TEXCOUNT_MASK = 0xf00;
    public const int D3DFVF_TEXCOUNT_SHIFT = 8;
    public const int D3DFVF_TEX0 = 0x000;
    public const int D3DFVF_TEX1 = 0x100;
    public const int D3DFVF_TEX2 = 0x200;
    public const int D3DFVF_TEX3 = 0x300;
    public const int D3DFVF_TEX4 = 0x400;
    public const int D3DFVF_TEX5 = 0x500;
    public const int D3DFVF_TEX6 = 0x600;
    public const int D3DFVF_TEX7 = 0x700;
    public const int D3DFVF_TEX8 = 0x800;

    public const int D3DFVF_RESERVED2 = 0xf000;

    public const int D3DFVF_VERTEX = (D3DFVF_XYZ | D3DFVF_NORMAL | D3DFVF_TEX1);
    public const int D3DFVF_LVERTEX = (D3DFVF_XYZ | D3DFVF_RESERVED1 | D3DFVF_DIFFUSE | D3DFVF_SPECULAR | D3DFVF_TEX1);
    public const int D3DFVF_TLVERTEX = (D3DFVF_XYZRHW | D3DFVF_DIFFUSE | D3DFVF_SPECULAR | D3DFVF_TEX1);

    public const uint D3DVBCAPS_SYSTEMMEMORY = 0x00000800;
    public const uint D3DVBCAPS_WRITEONLY = 0x00010000;
    public const uint D3DVBCAPS_OPTIMIZED = 0x80000000;
    public const uint D3DVBCAPS_DONOTCLIP = 0x00000001;

    /// <summary>
    /// The D3DVERTEXBUFFERDESC structure describes the properties of a vertex buffer object. This structure is used with the IDirect3D7::CreateVertexBuffer and IDirect3DVertexBuffer7::GetVertexBufferDesc methods.
    /// </summary>
    public struct D3DVERTEXBUFFERDESC
    {
        /// <summary>
        /// Size of this structure, in bytes. This member must be initialized before the structure is used.
        /// </summary>
        public DWORD dwSize;
        /// <summary>
        /// Capability flags that describe the vertex buffer and identify whether the vertex buffer can contain optimized vertex data. This parameter can be any combination of the following flags:
        ///
        ///(none)
        ///     The vertex buffer should be created in whatever memory the driver chooses, to allow efficient read operations.
        ///D3DVBCAPS_DONOTCLIP
        ///     The vertex buffer cannot contain clipping information.
        ///D3DVBCAPS_OPTIMIZED
        ///     The vertex buffer can contain optimized vertex data. (This flag is not used when creating a new vertex buffer.)
        ///D3DVBCAPS_SYSTEMMEMORY
        ///     The vertex buffer should be created in system memory.Use this capability for vertex buffers to be rendered by using software devices (MMX and RGB devices).
        ///D3DVBCAPS_WRITEONLY
        ///     Informs the system that the application only writes to the vertex buffer. Using this flag enables the driver to choose the best memory location for efficient write operations and rendering. Attempts to read from a vertex buffer that is created with this capability can result in degraded performance. 
        /// </summary>
        public DWORD dwCaps;
        /// <summary>
        /// A combination of flexible vertex format flags that describes the vertex format of the vertices in this buffer.
        /// </summary>
        public DWORD dwFVF;
        /// <summary>
        /// The maximum number of vertices that this vertex buffer can contain.The maximum number of vertices allowed is D3DMAXNUMVERTICES (0xFFFF). 
        /// </summary>
        public DWORD dwNumVertices;

        /// <summary>
        /// Замена sizeof
        /// </summary>
        /// <returns></returns>
        public static int GetSize()
        {
            return 4 + 4 + 4 + 4; //16 байт 
        }
    }
}

