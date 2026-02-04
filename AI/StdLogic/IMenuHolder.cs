using DWORD = System.UInt32;
public interface IMenuChanged : IObject
{
    public void MenuChanged(bool HasNewItem);
};

public interface IMenuItemHolder : IObject
{
    public IQuery GetIQueryData();
    public void SetIQueryData(IQuery q);
    public DWORD GetName();
    public string GetCaption();
    public int GetChildCount();
    public IMenuItemHolder GetChild(int index);
};

public interface IMenuHolder : IObject
{
    public IMenuItemHolder AddMenuItem(string name, DWORD hashed_name, string caption, string parentitem);
    public bool DeleteMenuItem(DWORD nm);
    public IMenuItemHolder FindItem(DWORD nm);
    public void SetMenuChangedCallback(IMenuChanged mc);
};
