public interface IBaseItem : iBaseInterface
{
    new public const uint ID = 0x9EAD7039;
    public uint GetHandle();
    public BaseScene getScene();
    public bool IsLocal();
    public bool IsRemote();

    public int SetRemote(char[] pData);
    public bool SetLocal(int DataLength,char[] pData);
    public void Release();

}