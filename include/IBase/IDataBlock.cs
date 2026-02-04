public interface IDataBlock : IObject
{
    public int getLength();
    public void getValue(out byte[] buf, int len);
};

