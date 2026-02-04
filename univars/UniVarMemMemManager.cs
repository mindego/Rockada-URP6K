using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using DWORD = System.UInt32;

public class UniVarMemMemManager : iUniVarMemManager
{
    private readonly uint UVX_SIGN = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("EVG1"));
    private readonly uint UVD_SIGN = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("MIKH"));

    // от iUniVarMemManager

    public virtual DWORD Alloc(DWORD size)
    {
        //Debug.Log("This allocating size: " + size);
        if (size == 0) return 0;
        DWORD id = 1;
        for (; id < mMaxMemBlock; id++)
        {
            if (mpMemBlocks[id].Ptr==null)
            {
                return Alloc(id, size);
            }
        }
        //Asserts.Assert(id == mMaxMemBlock);
        Assert.IsTrue(id == mMaxMemBlock);
        if (ReallocTable())
        {
            return Alloc(id, size);
        }
        Debug.Log(string.Format("Failed allocating size: {0} {1}/{2} ",size, id,mMaxMemBlock));
        return 0;
    }

    public virtual byte[] GetPtrByID(DWORD id)
    {
        return (id < mMaxMemBlock ? mpMemBlocks[id].Ptr : null);
    }
    public virtual DWORD GetIDByPtr(byte[] ptr)
    {
        for (int id = 0; id < mMaxMemBlock; id++)
        {
            if (ptr == mpMemBlocks[id].Ptr) return (uint)id;
        }
        return 0;
    }
    public virtual DWORD GetSizeByID(DWORD id)
    {
        return (id < mMaxMemBlock ? mpMemBlocks[id].Size : 0);
    }
    public virtual void Free(DWORD id) {
        if (mpMemBlocks[id].Ptr!=null)
        {
            //delete[] mpMemBlocks[id].Ptr;
            mpMemBlocks[id].Ptr = null;
            mpMemBlocks[id].Size = 0;
        }
    }
    public virtual bool SaveToFile(string filename) { return false; }
    public virtual bool IsReady()
    {
        return mReady;
    }
    public virtual bool IsReadOnly()
    {
        return false;
    }
    ~UniVarMemMemManager()
    {
        if (mReady)
        {
            for (int id = 0; id < mMaxMemBlock; id++)
            {
                if (mpMemBlocks[id] != null) mpMemBlocks[id] = null;
            }
            mpMemBlocks = null;
        }
    }

    // own
    private UnivarMemBlockData[] mpMemBlocks;
    private int mMaxMemBlock;
    private bool mReady;
    private DWORD Alloc(DWORD id, DWORD Size)
    {
        mpMemBlocks[id].Ptr = new byte[Size];
        mpMemBlocks[id].Size = Size;
        return id;
    }
    private bool ReallocTable() {
        int newSize = mMaxMemBlock * 2;
        UnivarMemBlockData[] newMemBlocks = new UnivarMemBlockData[newSize];
        if (newSize==0) return false;
        //MemSet(newMemBlocks, 0, newSize * sizeof(UnivarMemBlock));
        //MemCpy(newMemBlocks, mpMemBlocks, mMaxMemBlock * sizeof(UnivarMemBlock));
        //delete[] mpMemBlocks;
        for (int i=0;i<newSize;i++)
        {
            newMemBlocks[i] = new UnivarMemBlockData();
            newMemBlocks[i].Ptr = null; ;
            newMemBlocks[i].Size = 0;
            newMemBlocks[i].Offset = 0;
        }
        Array.Copy(mpMemBlocks, newMemBlocks, mpMemBlocks.Length);
        mMaxMemBlock = newSize;
        mpMemBlocks = newMemBlocks;
        return true;

    }
    private void Init(int minsize)
    {
        mMaxMemBlock = minsize > 2 ? minsize : 2;
        mpMemBlocks = new UnivarMemBlockData[mMaxMemBlock];
        for (int i=0;i<minsize;i++)
        {
            mpMemBlocks[i] = new UnivarMemBlockData();
            //mpMemBlocks[i].Ptr = new byte[0];
            mpMemBlocks[i].Ptr = null;;
            mpMemBlocks[i].Size = 0;
            mpMemBlocks[i].Offset = 0;
        }
        //MemSet(mpMemBlocks, 0, mMaxMemBlock * sizeof(UnivarMemBlock));
    }
    private void Purge() { }

    /// <summary>
    /// Получение массива байтов ресурса
    /// </summary>
    /// <param name="ID">Идентификатор ресурса</param>
    /// <returns>Массив "сырых" байтов ресурса</returns>
    public byte[] GetDataByID(uint ID)
    {
        return GetPtrByID(ID);
    }

    public void SetBytesByID(int id,byte[] data)
    {
        mpMemBlocks[id].Ptr = data;
    }

    public UniVarMemMemManager(int minsize = 4)
    {
        mReady = true;
        Init(minsize);
    }
    public UniVarMemMemManager(string Name)
    {
        //mReady = false;

        //TRef<IFileMapper> file = OpenMapFile(Name);
        //if (file)
        //{
        //    MemBlock d = file->GetData();
        //    const UVX* uvx = d.Convert<UVX>();
        //    Assert(uvx && uvx->Sign == UVX_SIGN);
        //    const UVD* uvd = (UVD*)(d.ptr + uvx->Size + sizeof(UVX));
        //    Assert(uvd && uvd->Sign == UVD_SIGN);

        //    Init(uvx->Size / sizeof(UnivarMemBlock)); //validate mMaxMemBlock 
        //    for (int id = 0; id < mMaxMemBlock; id++)
        //    {
        //        if (uvx->indx[id].Size)
        //        {
        //            Alloc(id, uvx->indx[id].Size);
        //            MemCpy(mpMemBlocks[id].Ptr, uvd->data + uvx->indx[id].Offset, uvx->indx[id].Size);
        //        }
        //    }
        //    mReady = true;
        //}

        mReady = false;

        if (!File.Exists(Name)) return;
        FileStream fs = File.Open(Name, FileMode.Open); 
        UVX uvx = StormFileUtils.ReadStruct<UVX>(fs);
        if (uvx.Sign != UVX_SIGN)
        {
            Debug.Log("Incorrect UVX header");
            fs.Close();
            return;
        }
        Init(uvx.Size / Marshal.SizeOf<UnivarMemBlock>());


        //uvx.indx = new UnivarMemBlock[mMaxMemBlock];
        for (int id = 0; id < mMaxMemBlock; id++)
        {
            //if (uvx.indx[id].Size != 0)
            //{
            //    Alloc((uint)id, uvx.indx[id].Size);
                UnivarMemBlock tmp = StormFileUtils.ReadStruct<UnivarMemBlock>(fs, fs.Position);
                mpMemBlocks[id] = new UnivarMemBlockData();
                mpMemBlocks[id].Offset = tmp.Offset;
                mpMemBlocks[id].Size = tmp.Size;
                mpMemBlocks[id].Ptr=new byte[tmp.Size];
            //}
        }
        UVD uvd = StormFileUtils.ReadStruct<UVD>(fs, fs.Position);
        if (uvd.Sign != UVD_SIGN)
        {
            Debug.Log("Incorrect UVD header");
            fs.Close();
            return;
        }
        uvd.data = new byte[uvd.Size];
        fs.Read(uvd.data, 0, uvd.Size);

        for (int id=0;id<mMaxMemBlock;id++)
        {
            //mpMemBlocks[i].Ptr = new byte[mpMemBlocks[i].Size];
            Array.Copy(uvd.data, mpMemBlocks[id].Offset, mpMemBlocks[id].Ptr, 0, mpMemBlocks[id].Size);

        }

        //Debug.Log("Loaded bytes: " + uvd.Size); 
        //Debug.Log($"Loaded [{Name}] mMaxMemBlock {mMaxMemBlock} UVD size {uvd.Size}");
        fs.Close();
        mReady = true;
        return;
    }
}