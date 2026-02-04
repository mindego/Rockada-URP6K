public interface HashEnumer
{
    public bool ProcessElement(HMember h);
};

public interface RasterizeEnumer
{
    public bool ProcessElement(int x, int y);
};

public class HashLineEnumer : HashEnumer
{
    public Geometry.Line line;
    public void SetLineData(Geometry.Line _line) { line = _line; }
    public virtual bool ProcessElement(HMember h) { return true; }
};


