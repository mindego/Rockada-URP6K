using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

public struct Pager<T,Z> 
{
    /*const int PG_SIZE = dim;
    const int OFF_MASK = dim - 1;
    const int ADR_MASK = ~OFF_MASK;*/
    int PG_SIZE;
    int OFF_MASK;
    int ADR_MASK;

    private Stream Data;

    int SizeXP;
    int SizeZP;

    public Pager(Stream data, int sizeXP, int sizeZP, int dim = 0)
    {
        Data = data;
        SizeXP = sizeXP;
        SizeZP = sizeZP;

        PG_SIZE = dim;
        OFF_MASK = dim - 1;
        ADR_MASK = ~OFF_MASK;
    }

    //public void SetData(T[] d, int sx, int sz)
    public void SetData(Stream d, int sx, int sz)
    {
        Data = d; 
        SizeXP = sx; 
        SizeZP = sz;
    }

    int ClampX(int x) 
    { 
        return ClampInt(x,0, SizeXP* PG_SIZE-1);
    }
    int ClampZ(int z) 
    { 
        return ClampInt(z,0, SizeZP*PG_SIZE-1);
    }

    public int Offset(int x, int z) {

        return ((x&ADR_MASK)+(z&ADR_MASK)*SizeXP)*PG_SIZE+(x&OFF_MASK)+(z&OFF_MASK)*PG_SIZE;
     }

    int OffsetCl(int x, int z) {
        return  Offset(ClampX(x), ClampZ(z));
    }

    public int SizeXPages() 
    { 
        return SizeXP; 
    }
    public int SizeZPages() 
    { 
        return SizeZP; 
    }

    public int SizeX() 
    { 
        return SizeXP*PG_SIZE; 
    }
    public int SizeZ() 
    { 
        return SizeZP*PG_SIZE; 
    }

    int Size() 
    { 
        return SizeXP*SizeZP;  
    } // Memory size in intel pages 4K

    //public T Get(int x,int y)
    //{
    //    //return Data + Offset(x, y);
    //    //Debug.Log(x+"x" + y + " Offset " + Offset(x, y));
    //    int blocksize = 4;
    //    if (typeof(T) == typeof(T_VBOX)) blocksize = 16;
    //    T tmpData = default;
    //    try
    //    {
    //        tmpData = StormFileUtils.ReadStruct<T>(Data, Offset(x, y) * blocksize); //4 - размер в байтах данных T_SQUARE, 16 - T_VBOX
    //    } catch
    //    {
    //        Debug.Log(string.Format("Failed to load {0} data @ [{1}:{2}]",typeof(T).ToString(),x,y));
    //    }

    //    //return default(T);
    //    return tmpData; 
    //}

    public T Get(int x, int y)
    {
        return Get(Offset(x, y));
    }
    public T GetCl(int x, int y)
    {

        return Get(OffsetCl(x, y));
        //throw new System.NotImplementedException();
        ////return Data + OffsetCl(x, y);
        //return default(T);
    }

    public T Get(int Offset)
    {
        int blocksize = 4;
        if (typeof(T) == typeof(T_VBOX)) blocksize = 16;
        T tmpData = default;
        try
        {
            tmpData = StormFileUtils.ReadStruct<T>(Data, Offset * blocksize); //4 - размер в байтах данных T_SQUARE, 16 - T_VBOX
        }
        catch
        {
            //Debug.Log(string.Format("Failed to load {0} data @ [{1}:{2}]", typeof(T).ToString(), x, y));
            Debug.Log(string.Format("Failed to load {0} data @ [{1}]", typeof(T).ToString(), Offset));
        }

        //return default(T);
        return tmpData;
    }
    int ClampInt(int x, int min, int max)
    {
        return (x > max ? max : (x < min ? min : x));
    }
}
