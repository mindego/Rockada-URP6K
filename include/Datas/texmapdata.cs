//using PrimitiveType = System.UInt32;
using System.IO;
using System.Runtime.InteropServices;
using crc32value = System.UInt32;

public enum TMType
{
    TMT_STANDARD,
    TMT_UNIFIED
};
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TexMapData : IStormImportable<TexMapData>
{
    public TMType type;

    public override string ToString()
    {
        return base.ToString() + "\n" + "TMType: " + type;
    }
    public TexMapData Import(Stream st)
    {
        return StormFileUtils.ReadStruct<TexMapData>(st);
    }
}
///////////////////////////////////////////////

///////////////////////////////////////////////
//TMT_STANDARD
public enum TMAniMode
{
    TMA_STATIC,
    TMA_SLIDED,
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class StdTexMapData : TexMapData,IStormImportable<StdTexMapData>
{
    public TMAniMode animode;
    public TMDFlags flags;

    public override string ToString()
    {
        return base.ToString() + "\n" + 
            "TMAniMode: " + animode + "\n" +
            "TMDFlags " + flags 
            ;
    }
    new public StdTexMapData Import(Stream st)
    {
        st.Seek(0, SeekOrigin.Begin);
        return StormFileUtils.ReadStruct<StdTexMapData>(st);
    }
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class  StaticStdTexMapData : StdTexMapData, IStormImportable<StaticStdTexMapData>
{
    public crc32value texture;

    new public StaticStdTexMapData Import(Stream st)
    {
        st.Seek(0, SeekOrigin.Begin);
        return StormFileUtils.ReadStruct<StaticStdTexMapData>(st);
    }
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class SlidedStdTexMapData : StdTexMapData, IStormImportable<SlidedStdTexMapData>
{
    public float time_factor;
    public int num;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    public crc32value[] textures;

    SlidedStdTexMapData IStormImportable<SlidedStdTexMapData>.Import(Stream st)
    {
        st.Seek(0, SeekOrigin.Begin);
        SlidedStdTexMapData tmp = StormFileUtils.ReadStruct<SlidedStdTexMapData>(st);
        tmp.textures = new crc32value[tmp.num];
        for (int i=0; i<tmp.num;i++)
        {
            tmp.textures[i] = StormFileUtils.ReadStruct<crc32value>(st);
        }
        return tmp;
    }

    public override string ToString()
    {
        return base.ToString() + "\n" + 
            "time_factor: " + time_factor + "\n" + 
            "num: "+ num + "\n" + 
            "textures: " + textures.Length;
    }
}
