public class CollideInfo
{
    public int count;
    public CollideResult[] results;
    public CollideInfo() : this(0, new CollideResult[0]) { }
    public CollideInfo(int _count, CollideResult[] _res)
    {
        count = _count;
        results = _res;
    }
    //operator const CollideResult* () const { return results; }
}
