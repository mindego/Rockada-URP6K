using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class Textures
{
    struct StormTextureFormat
    {
        //        DWORD Flags;
        int Flags;
        int Width;
        int Height;
        TexturePixelFormat PixelFormat;
        int MipMapCount;
    };

    enum TexturePixelFormat
    {
        TPF_UNKNOWN = 0,
        TPF_R8G8B8 = 1,
        TPF_A8R8G8B8 = 2,
        TPF_X8R8G8B8 = 3,
        TPF_R5G6B5 = 4,
        TPF_R5G5B5 = 5,
        TPF_PALETTE4 = 6,
        TPF_PALETTE8 = 7,
        TPF_A1R5G5B5 = 8,
        TPF_X4R4G4B4 = 9,
        TPF_A4R4G4B4 = 10,
        TPF_L8 = 11,
        TPF_A8L8 = 12,
        TPF_U8V8 = 13,
        TPF_U5V5L6 = 14,
        TPF_U8V8L8 = 15,
        TPF_UYVY = 16,
        TPF_YUY2 = 17,
        TPF_DXT1 = 18,
        TPF_DXT3 = 19,
        TPF_DXT5 = 20,
        TPF_R3G3B2 = 21,
        TPF_A8 = 22,
        TPF_MAXFORMAT = 23,
        //TPF_FORCEMAX = 0xffFFffFF
        TPF_FORCEMAX = 0xffFFff
    };
    /// <summary>
    /// Метод "раскукоживания" текстуры. Возвращает несжатый (ARGB32) вариант текстуры
    /// </summary>
    /// <param name="texture">Текстура для распаковки</param>
    /// <param name="decompressedFormat">Формат получаемой текстуры</param>
    /// <returns>Несжатая текстура</returns>
    public static Texture2D decompressTexture(Texture2D texture, TextureFormat decompressedFormat = TextureFormat.ARGB32)
    {
        Texture2D decompressed = new Texture2D(texture.width, texture.height, decompressedFormat, false);
        Color[] pixels = texture.GetPixels();

        decompressed.SetPixels(pixels);
        decompressed.Apply();

        return decompressed;

    }
    /// <summary>
    /// Метод переворота по горизонтали текстуры. "Отзеркаливает" текстуру таким образом, что верхняя сторона становится нижней, не вращая саму текстуру
    /// </summary>
    /// <param name="texture">Переворачиваемая текстура</param>
    /// <returns>Повёрнутая по горизонтали текстура</returns>
    public static Texture2D flipTexture(Texture2D texture)
    {
        Texture2D flipped = new Texture2D(texture.width, texture.height, texture.format, false);
        Color[] pixels = texture.GetPixels();

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                int index = y * texture.width + x;
                flipped.SetPixel(x, texture.height - y, pixels[index]);
            }
        }
        flipped.Apply();

        return flipped;
    }

    public static void ExportTexture(Texture2D texture, string resname, string outDir = "Data/recode/")
    {
        Texture2D resTexture = Textures.decompressTexture(texture);
        //resTexture = Textures.flipTexture(resTexture);
        byte[] data = resTexture.EncodeToPNG();
        FileStream outfile = File.Create(outDir + resname + ".png");
        outfile.Write(data);
        outfile.Close();
    }

    public interface ITextureFactory : IObject
    {
        //public HRESULT SetTargetDevice(D3DDevice*)=0;
        //public HRESULT CheckTextureFormat(TextureFormat & )=0;
        //public HRESULT CreateTexture(TextureFormat &, DDSurface*& )=0;
        //public HRESULT LoadTexture(DDSurface*, int MipLevel, void* src_memory, TexturePixelFormat src_pixelformat, int src_pitch)=0;
    };

    public static ITextureFactory CreateTextureFactory(object log)
    {
        //return new TextureFactory(log);
        throw new NotImplementedException();
    }
}
