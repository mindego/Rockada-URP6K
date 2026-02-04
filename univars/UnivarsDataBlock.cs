using uvb = iUnifiedVariableBlock;
using tuvb = iUnifiedVariableBlock;

public class UnivarsDataBlock : IDataBlock
{
    public int getLength()
    {
        return myBlock!=null ? myBlock.GetLength() : 0;
    }

    public void getValue(out byte[] buf, int len)
    {
        myBlock.GetValue(out buf, len);
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }

    public UnivarsDataBlock(uvb rd)
    {
        myBlock = rd;
    }

    tuvb myBlock;
}