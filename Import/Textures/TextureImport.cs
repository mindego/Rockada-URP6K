using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Класс импорта текстур
/// </summary>
public class TextureImport
{
    public static Texture2D GetTexture(Stream ms, int id, TextureFormat defaultFormat = TextureFormat.Alpha8)
    {
        return GetTexture(ms, "TextureId " + id.ToString("X8"));
    }
    public static Texture2D GetTexture(Stream ms, string name, TextureFormat defaultFormat = TextureFormat.Alpha8)
    {
        byte[] buffer = new byte[4];
        StormTexture2D texture = new StormTexture2D();
        StormTexture2D.TextureDataHdr header = new StormTexture2D.TextureDataHdr();
        ms.Seek(0, SeekOrigin.Begin);

        ms.Read(buffer);
        header.DataSize = BitConverter.ToInt32(buffer);
        ms.Read(buffer);
        header.Reserved0 = BitConverter.ToInt32(buffer);
        ms.Read(buffer);
        header.Flags = BitConverter.ToInt32(buffer);
        ms.Read(buffer);
        header.PixelFormat = (StormTexture2D.TDPixelFmt)BitConverter.ToInt32(buffer);
        ms.Read(buffer);
        header.Width = BitConverter.ToInt32(buffer);
        ms.Read(buffer);
        header.Height = BitConverter.ToInt32(buffer);

        header.Requirements = new StormTexture2D.TDRequrements();
        ms.Read(buffer);
        header.Requirements.Flags = BitConverter.ToInt32(buffer);
        ms.Read(buffer);
        header.Requirements.AlphaDepth = BitConverter.ToInt32(buffer);
        ms.Read(buffer);
        header.Requirements.RGRChannelDepth = BitConverter.ToInt32(buffer);
        ms.Read(buffer);
        header.Requirements.LuminanceDepth = BitConverter.ToInt32(buffer);
        ms.Read(buffer);
        header.Requirements.DuDvChannelDepth = BitConverter.ToInt32(buffer);
        ms.Read(buffer);
        header.Requirements.BumpLuminanceDepth = BitConverter.ToInt32(buffer);

        TextureFormat textureFormat;
        Texture2D resTexture;
        switch (header.PixelFormat)
        {
            case StormTexture2D.TDPixelFmt.TDPF_PAL:
                textureFormat = TextureFormat.RGBA32;
                StormTexture2D.TData_PAL headerPal = StormTexture2D.TData_PAL.Convert(header);
                for (int i = 0; i < headerPal.Palette.Length; i++)
                {
                    ms.Read(buffer);
                    headerPal.Palette[i] = BitConverter.ToUInt32(buffer);
                }
                resTexture = new Texture2D(header.Width, header.Height, textureFormat, false)
                {
                    //name = "Generated_PAL_" + name
                };
                header = headerPal;
                break;
            case StormTexture2D.TDPixelFmt.TDPF_ALPHALUM_REPLICATED:
                textureFormat = TextureFormat.Alpha8;
                resTexture = new Texture2D(header.Width, header.Height, textureFormat, false)
                {
                    //name= "Generated_Alpha8_" + name
                };
                break;
            case StormTexture2D.TDPixelFmt.TDPF_DUDV:
                textureFormat = TextureFormat.Alpha8;
                resTexture = new Texture2D(header.Width, header.Height, textureFormat, false)
                {
                    //name = "Generated_Alpha8_" + name
                };
                break;
            case StormTexture2D.TDPixelFmt.TDPF_DXTN:
                StormTexture2D.TData_DXTN DXTheader = StormTexture2D.TData_DXTN.Convert(header);

                ms.Read(buffer);
                DXTheader.Code = BitConverter.ToInt32(buffer);
                ms.Read(buffer);
                DXTheader.MipMapCount = BitConverter.ToInt32(buffer);
                if (DXTheader.MipMapCount == 0) DXTheader.MipMapCount = 1;

                switch (DXTheader.Code)
                {
                    case StormTexture2D.FOURCC_DXT1:
                        textureFormat = TextureFormat.DXT1;
                        //name = "Generated_DXT1_" + name;
                        break;
                    case StormTexture2D.FOURCC_DXT3:
                        textureFormat = TextureFormat.DXT5;
                        //name = "Generated_DXT3_" + name;
                        break;
                    case StormTexture2D.FOURCC_DXT5:
                        textureFormat = TextureFormat.DXT5;
                        //name = "Generated_DXT5_" + name;
                        break;
                    default:
                        Debug.Log("Incorrect texture Code, fallback to DXT1");
                        textureFormat = TextureFormat.DXT1;
                        break;
                }
                resTexture = new Texture2D(header.Width, header.Height, textureFormat, DXTheader.MipMapCount, true)
                //resTexture = new Texture2D(header.Width, header.Height, textureFormat, false)
                {
                    name = name
                };
                header = DXTheader;
                break;
            default:
                Debug.Log("Format Unsupported " + header.PixelFormat);
                //STUB! Возвращать служебную текстуру с ошибкой
                textureFormat = TextureFormat.DXT1;
                resTexture = new Texture2D(header.Width, header.Height, textureFormat, false);
                break;
        }

        byte[] bytes = new byte[header.DataSize - ms.Position];



        int dataCount = resTexture.GetRawTextureData().Length;
        //Debug.Log("Expecting " + dataCount + " bytes, " + bytes.Length + " provided");
        ms.Read(bytes);

        switch (header.PixelFormat)
        {
            case StormTexture2D.TDPixelFmt.TDPF_PAL:
                byte[] rgbBytes = new byte[bytes.Length * 4];
                StormTexture2D.TData_PAL headerPal = (StormTexture2D.TData_PAL)header;
                int i = 0;
                foreach (byte colorIndex in bytes)
                {
                    uint color = headerPal.Palette[colorIndex];
                    byte[] tmpBytes = BitConverter.GetBytes(color);
                    foreach (byte colorByte in tmpBytes)
                    {
                        rgbBytes[i++] = colorByte;
                    }
                }
                bytes = rgbBytes;
                break;
        }

        //resTexture.name = "Generated_" + textureFormat + "_" + name;
        resTexture.name = name;
        resTexture.LoadRawTextureData(bytes);

        resTexture.Apply();

        //Debug.LogFormat("Generated texture {0} format {1}/{2} TextureFlags {3}", name, textureFormat,resTexture.format, header.Flags.ToString("X8"));
        resTexture = flipTexture(resTexture);
        //flipTexture(ref resTexture);
        //Debug.LogFormat("Generated texture {0} format {1}/{2} after flip", name, textureFormat, resTexture.format);
        return resTexture;
    }


    private static void flipTexture(ref Texture2D resTexture)
    {
        int width = resTexture.width;
        int height = resTexture.height;

        TextureFormat textureFormat = resTexture.format;
        bool NeedCompress = false;
        switch (textureFormat)
        {
            case TextureFormat.DXT1:
                textureFormat = TextureFormat.RGB24;
                NeedCompress = true;
                break;
            case TextureFormat.DXT5:
                textureFormat = TextureFormat.RGBA32;
                NeedCompress = true;
                break;
            default:
                break;
        }
        Texture2D snap = new Texture2D(width, height, textureFormat, true);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++) {
                //snap.SetPixel(x, y,resTexture.GetPixel(x,height-1-y));
                snap.SetPixel(x, y, resTexture.GetPixel(x, height - 1 - y));
            }
        }
        if (NeedCompress) snap.Compress(true);
        snap.Apply();
        resTexture = snap;
    }
    private static Texture2D flipTexture(Texture2D texture)
    {
        int width = texture.width;
        int height = texture.height;

        TextureFormat textureFormat = TextureFormat.RGBA32;
        switch (texture.format)
        {
            case TextureFormat.DXT1:
                textureFormat = TextureFormat.RGB24;
                break;
            case TextureFormat.Alpha8:
                textureFormat = TextureFormat.Alpha8;
                break;
            default:
                break;
        }
        //Texture2D snap = new Texture2D(width, height,texture.format,true);

        //Texture2D snap = new Texture2D(width, height);
        Texture2D snap = new Texture2D(width, height, textureFormat,true);
        Color[] pixels = texture.GetPixels();
        Color[] pixelsFlipped = new Color[pixels.Length];

        for (int i = 0; i < height; i++)
        {
            Array.Copy(pixels, i * width, pixelsFlipped, (height - i - 1) * width, width);
        }

        snap.name = texture.name;
        snap.SetPixels(pixelsFlipped);
        snap.Apply();
        switch (texture.format)
        {
            case TextureFormat.DXT1:
            case TextureFormat.DXT5:
                snap.Compress(true);
                break;
        }

        return snap;
    }
}


public class StormTexture2D
{
    const int TDRF_ALPHA_PRESENT = 0x01;
    const int TDRF_RGB_PRESENT = 0x02;
    const int TDRF_LUMINANCE_PRESENT =0x04;
    const int TDRF_DUDV_PRESENT =0x08;
    const int TDRF_BUMPLUMINANCE_PRESENT = 0x10;

    const int TDF_NOMIPMAP = 0x01;

    public const int FOURCC_DXT1 = 0x31545844;
    public const int FOURCC_DXT3 = 0x33545844;
    public const int FOURCC_DXT5 = 0x35545844;
    public enum TDPixelFmt
    {
        TDPF_ARGB,
        TDPF_ALPHALUM_REPLICATED,
        TDPF_PAL,
        TDPF_DUDV,
        TDPF_DXTN,
        TDPF_DUDVLUM,
        TDPF_RGB,
        TDPF_MAX
    };

    public struct TDRequrements
    {
        //DWORD Flags;
        public int Flags;

        public int AlphaDepth;
        public int RGRChannelDepth;
        public int LuminanceDepth;
        public int DuDvChannelDepth;
        public int BumpLuminanceDepth;

        public override string ToString()
        {
            string res = "Flags: " + Convert.ToString(Flags, 2) + "\n";
            res+="Alpha: " + AlphaDepth + "\n";
            res+= "RGRChannelDepth: " + RGRChannelDepth + "\n";
            res+= "LuminanceDepth: " + LuminanceDepth + "\n";
            res+= "DuDvChannelDepth: " + DuDvChannelDepth + "\n";
            res+= "BumpLuminanceDepth: " + BumpLuminanceDepth;

            return res;
        }
    }
    public class TextureDataHdr
    {
        public int DataSize;
        //DWORD Reserved0;
        public int Reserved0;

        //DWORD Flags;
        public int Flags;
        public TDPixelFmt PixelFormat;
        public int Width;
        public int Height;

        public TDRequrements Requirements;

        struct TData_PAL
        {
            int[] Palette; //256
            byte[] PixelData;
        };

        struct TData_ALPHALUM_REPLICATED
        {
            byte[] PixelData;
        };

        struct TData_DUDV
        {
            byte[] PixelData;//height info
        };

        struct TData_DUDVLUM
        {
            //Vec3<BYTE> PixelData[];//du8dv8lum8
            Vector3[] PixelData;//du8dv8lum8
        };

        struct TData_DXTN 
        {
            //DWORD Code;//"DXT1","DXT3","DXT5"
            int Code;//"DXT1","DXT3","DXT5"
            int MipMapCount;
            byte[] LinearData;//continiously from mip0 data thorough the rest
        };

        public override string ToString()
        {
            string res = "Datasize: " + DataSize + "\n";
            res+= "Reserved: " + Reserved0 + "\n";
            res+= "Flags: " + Flags + "\n";
            res += "Height: " + Height + "\n";
            res += "Width: " + Width;

            //res += "Reqs:\n" + Requirements;

            return res;
        }
    }
    public class TData_PAL : TextureDataHdr
    {
        public uint[] Palette; //256
        public byte[] PixelData;

        internal static TData_PAL Convert(TextureDataHdr header)
        {
            TData_PAL extendedHeader = new TData_PAL();
            extendedHeader.DataSize = header.DataSize;
            extendedHeader.Reserved0 = header.Reserved0;
            extendedHeader.Flags = header.Flags;
            extendedHeader.Height = header.Height;
            extendedHeader.Width = header.Width;
            extendedHeader.PixelFormat = header.PixelFormat;
            extendedHeader.Requirements = header.Requirements;

            extendedHeader.Palette = new uint[256];
            extendedHeader.PixelData = new byte[0];
            return extendedHeader;
        }
    };

    public class TData_DUDV : TextureDataHdr
    {
        public byte[] PixelData;//height info

        internal static TData_DUDV Convert(TextureDataHdr header)
        {
            TData_DUDV extendedHeader = new TData_DUDV();
            extendedHeader.DataSize = header.DataSize;
            extendedHeader.Reserved0 = header.Reserved0;
            extendedHeader.Flags = header.Flags;
            extendedHeader.Height = header.Height;
            extendedHeader.Width = header.Width;
            extendedHeader.PixelFormat = header.PixelFormat;
            extendedHeader.Requirements = header.Requirements;

            extendedHeader.PixelData = new byte[0];
            return extendedHeader;
        }
    };
    public class TData_DXTN: TextureDataHdr
    {
        //DWORD Code;//"DXT1","DXT3","DXT5"
        public int Code;//"DXT1","DXT3","DXT5"
        public int MipMapCount;
        public byte[] LinearData;//continiously from mip0 data thorough the rest

        internal static TData_DXTN Convert(TextureDataHdr header)
        {
            TData_DXTN extendedHeader = new TData_DXTN();
            extendedHeader.DataSize = header.DataSize;
            extendedHeader.Reserved0 = header.Reserved0;
            extendedHeader.Flags = header.Flags;
            extendedHeader.Height = header.Height;
            extendedHeader.Width = header.Width;
            extendedHeader.PixelFormat = header.PixelFormat;
            extendedHeader.Requirements = header.Requirements;
            
            extendedHeader.Code = 0;
            extendedHeader.MipMapCount = 0;
            extendedHeader.LinearData = new byte[0];
            return extendedHeader;
        }

        public override string ToString()
        {
            string res= base.ToString()+"\n";
            res += "Code: " + Code + "\n";
            res += "MipMapCount: " + MipMapCount + "\n";
            res += "LinearData size:" + LinearData.Length;


            return res;
        }
    }
    public class TData_ALPHALUM_REPLICATED: TextureDataHdr
    {
        public byte[] PixelData;

        internal static TData_ALPHALUM_REPLICATED Convert(TextureDataHdr header)
        {
            TData_ALPHALUM_REPLICATED extendedHeader = new TData_ALPHALUM_REPLICATED();
            extendedHeader.DataSize = header.DataSize;
            extendedHeader.Reserved0 = header.Reserved0;
            extendedHeader.Flags = header.Flags;
            extendedHeader.Height = header.Height;
            extendedHeader.Width = header.Width;
            extendedHeader.PixelFormat = header.PixelFormat;
            extendedHeader.Requirements = header.Requirements;

            extendedHeader.PixelData = new byte[0];

            return extendedHeader;
        }
    };

    int GetBlockSize(int code)
    {
        switch (code)
        {
            case FOURCC_DXT1:
                return 8;
            case FOURCC_DXT3:
                return 16;
            case FOURCC_DXT5:
                return 16;
        }
        return 0;
    }

    int GetMipLinearSize(int code, int height, int width)
    {
        int hb = height >> 2 + (((height & 3) != 0) ? 1 : 0); if (hb == 0) hb = 1;
        int wb = width >> 2 + (((width & 3) !=0 ) ? 1 : 0); if (wb == 0) wb = 1;

        return hb * wb * GetBlockSize(code);
    }

    void GetSubMipSize(int height, int width )
    {
        height >>= 1;
        width >>= 1;
        if (height == 0) height = 1;
        if (width == 0) width = 1;
    }


}




