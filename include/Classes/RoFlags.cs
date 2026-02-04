public class RoFlags
{
    // hashable ro objects, not from ro must support OF_???? hierarchy

    // renderer flags

    public const uint ROF_DRAWED = 0x00200000;
    public const uint ROF_NONHASH_OBJECT = 0x00400000;



    public const uint ROF_SOLID = 0x00010000;
    public const uint ROF_TANSP = 0x00020000;
    public const uint ROF_ST_SOLID = 0x00040000;
    public const uint ROF_ST_TANSP = 0x00080000;

    // object identifiction :
    public const uint ROFID_ALLOBJECTS = 0x00000FFF;

    public const uint ROFID_FPO = 0x00000010;
    public const uint ROFID_PARTICLE = 0x00000040;
    public const uint ROFID_LIGHT = 0x00000080;
    public const uint ROFID_SHADOW = 0x00000100;
    public const uint ROFID_DECAL = 0x00000200;
    public const uint ROFID_STATICSHADOW = 0x00000400;
    public const uint ROFID_FLARE = 0x00000800;


    // Fpo image enumeration
    public const uint FSI_EQUAL_LINKS = 0x00000001;
    public const uint FSI_NONEQUAL_LINKS = 0x00000002;
    public const uint FSI_IGNORE_LINKS = 0x00000003;
    public const uint FSI_FORCE = 0x00000004;
    public const uint FSI_ROUND_UP = 0x00000010;
    public const uint FSI_ROUND_DOWN = 0x00000020;

}
