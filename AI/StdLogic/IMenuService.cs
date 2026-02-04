using crc32 = System.UInt32;

public interface IMenuHandler
{
    public bool notifySelect(crc32 code);
};
public interface IMenuService
{
    public const uint ID = 0xC0AF96DF;
    public void addMenuItem(string name, string caption, string parentitem);
    public void deleteMenuItem(crc32 nm);
    public void registerHandler(IMenuHandler imh);
    public void unregisterHandler(IMenuHandler imh);
    public void notifySelected(crc32 select);
};
