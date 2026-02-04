public class HUDObjectImpl : IHUDObjectData
{
    bool Hide;
    HUDTree Tree;

    public HUDTree GetTree()
    {
        return Tree;
    }

    public bool IsHidden()
    {
        return Hide;
    }

    public void SetHide(bool off)
    {
        Hide = off;
    }

    public void SetTree(HUDTree tree)
    {
        Tree = tree;
    }
}