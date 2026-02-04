using System.IO;
using crc32value = System.UInt32;
//using NonHashList = System.Collections.Generic.List<IHashObject>;
//using NonHashList = System.Collections.Generic.List<HashObjectCont>;
class EnvCfg : IStormImportable<EnvCfg>
{
    public crc32value g_light;//GLightConfig
    public crc32value fog;    //GFogConfig
    public crc32value sky;    //SkyConfig
    public crc32value terrain;//TerrainConfig

    public crc32value[] sg_maps = new crc32value[8];

    public SceneGameInfo game_info;

    public EnvCfg Import(Stream st)
    {
        st.Seek(0, SeekOrigin.Begin);
        g_light = StormFileUtils.ReadStruct<crc32value>(st, st.Position);
        fog = StormFileUtils.ReadStruct<crc32value>(st, st.Position);
        sky = fog = StormFileUtils.ReadStruct<crc32value>(st, st.Position);
        terrain = StormFileUtils.ReadStruct<crc32value>(st, st.Position);
        for (int i = 0; i < 8; i++)
        {
            sg_maps[i] = StormFileUtils.ReadStruct<crc32value>(st, st.Position);
        }
        game_info = StormFileUtils.ReadStruct<SceneGameInfo>(st, st.Position);
        return this;
    }
}
