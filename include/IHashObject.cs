public interface IHashObject : IQuery, System.IDisposable
{
    new const uint ID = 0x7B4D2042;
    public uint GetFlag(uint f);
    public uint SetFlag(uint f);
    public void ClearFlag(uint f);
    bool MatchGroup(uint f);

    bool MatchType(uint f);
    // this nmethod used by hash system
    bool MatchFlags(uint f);


    public Geometry.Line GetLinearData();
    public geombase.Sphere GetBoundingSphere();

    public IHashObject GetIHashObject();
    public uint GetFlags();
}

