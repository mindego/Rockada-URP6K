/// <summary>
/// IRenderObject
/// </summary>
public interface IRenderable : IObject
{
    public void Draw();
    public geombase.Sphere GetSphereBound();
};

public interface IRenderGroup : IObject
{
    new public const uint ID = 0x0BBBB803;
    public int GetNumGroups();
  //public IShader      GetShader(int group)=0;
  public IRenderable GetRenderable(int group);
};

public interface IDrawable : IObject
{
    new public const uint ID = 0x09E48D6C;
    public IRenderGroup CreateRenderGroup();
};