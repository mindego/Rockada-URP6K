public class TraceInfo
{
    public int count;
    public TraceResult[] results;
    public TraceInfo() :this (0,new TraceResult[0]){ 
    }
    public TraceInfo(int _count, TraceResult[] _res)
    {
        count = _count;
        results = _res;
    }
  //operator TraceResult const * const * () const { return results; }
}
