using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// UniVarMemROManager - реализация iUniVarMemManager
/// </summary>
public class UniVarMemROManager : iUniVarMemManager
{
    private UVX uvx;
    private UVD uvd;
    private int mMaxMemBlock;
    private bool mReady;
    private readonly uint UVX_SIGN = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("EVG1"));
    private readonly uint UVD_SIGN = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("MIKH"));
    //define UVX_SIGN (*((int*)"EVG1"))
    //define UVD_SIGN (*((int*)"MIKH"))

    public UniVarMemROManager(string Name)
    {
        uvx = default;
        uvd = default;
        mMaxMemBlock = 0;
        mReady = false;

        if (!File.Exists(Name)) return;
        FileStream fs = File.Open(Name, FileMode.Open); uvx = StormFileUtils.ReadStruct<UVX>(fs);

        if (uvx.Sign != UVX_SIGN)
        {
            Debug.Log("Incorrect UVX header");
            fs.Close();
            return;
        }
        mMaxMemBlock = uvx.Size / Marshal.SizeOf<UnivarMemBlock>();
        uvx.indx = new UnivarMemBlock[mMaxMemBlock];
        for (int i = 0; i < mMaxMemBlock; i++)
        {
            uvx.indx[i] = StormFileUtils.ReadStruct<UnivarMemBlock>(fs, fs.Position);
        }

        uvd = StormFileUtils.ReadStruct<UVD>(fs, fs.Position);
        if (uvd.Sign != UVD_SIGN)
        {
            Debug.Log("Incorrect UVD header");
            fs.Close();
            return;
        }
        uvd.data = new byte[uvd.Size];
        fs.Read(uvd.data, 0, uvd.Size);
        //Debug.Log("Loaded bytes: " + uvd.Size); 
        //Debug.Log($"Loaded [{Name}] mMaxMemBlock {mMaxMemBlock} UVD size {uvd.Size}");
        fs.Close();
        mReady = true;
        return;
    }

    public virtual uint Alloc(uint size)
    {
        return 0;
    }

    public string GetPtrByID(uint id)
    {
        throw new NotImplementedException();
    }

    public uint GetIDByPtr(string ptr)
    {
        throw new NotImplementedException();
    }

    public uint GetSizeByID(uint id)
    {
        if (id >= mMaxMemBlock) return 0;
        return uvx.indx[id].Size;
    }

    public void Free(uint id)
    {
        return;
    }

    public bool SaveToFile(string filename)
    {
        throw new NotImplementedException("И не надо!");
    }

    public bool IsReady()
    {
        return mReady;
    }

    public bool IsReadOnly()
    {
        return true;
    }
    /// <summary>
    /// Получение массива байтов ресурса
    /// </summary>
    /// <param name="ID">Идентификатор ресурса</param>
    /// <returns>Массив "сырых" байтов ресурса</returns>
    public byte[] GetDataByID(uint ID)
    {
        UnivarMemBlock univarMemBlock = uvx.indx[ID];
        byte[] res = new byte[univarMemBlock.Size];
        Array.Copy(uvd.data, univarMemBlock.Offset, res, 0, univarMemBlock.Size);
        return res;
    }

    byte[] iUniVarMemManager.GetPtrByID(uint id)
    {
        throw new NotImplementedException();
    }

    public uint GetIDByPtr(byte[] ptr)
    {
        throw new NotImplementedException();
    }

    public void SetBytesByID(int id, byte[] data)
    {
        throw new NotImplementedException();
    }
}


public struct UnivarMemBlock
{
    public DWORD Offset;
    public DWORD Size;
};

public class UnivarMemBlockData
{
    public DWORD Offset;
    public DWORD Size;
    public byte[] Ptr;
}

public struct UVX
{
    public int Sign;
    public int Size;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    public UnivarMemBlock[] indx;
};

public struct UVD
{
    public int Sign;
    public int Size;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    public byte[] data; //в оригинале - char
};
