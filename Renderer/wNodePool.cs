
public class wNodePool<NodeType> where NodeType : new()
{
    //typedef N NodeType;
    //typedef N::DataType DataType;
    //typedef N::NodePtr  NodePtr;
    public wNodePool()
    {
        Init();
    }
    public wNodePool(wNodePool<NodeType> w)
    {
        Init();
    }
    void Init()
    {
        mHead = new wListNode<NodeType>();
        mHead.Next(mHead);
        mHead.Prev(mHead);
        mSize = 0;
    }
    ~wNodePool()
    {
        for (wListNode<NodeType> n = mHead.Next(); n != mHead;)
        {
            wListNode<NodeType> nn = n.Next();
            //n.Dispose();
            //delete n;
            n = nn;
        }
        //delete mHead;
    }
    wListNode<NodeType> Allocate(NodeType data)
    {
        ++mSize;
        wListNode<NodeType> node;
        if (mHead == mHead.Next())
            node = new wListNode<NodeType>();
        else
        {
            node = mHead.Next();
            mHead.Next(node.Next());
            mHead.Next().Prev(mHead);
        }
        //node.Data() = data;
        node.SetData(data);
        return node;
    }
    void DeAllocate(wListNode<NodeType> pnode)
    {
        --mSize;
        pnode.Next(mHead.Next());
        mHead.Next(pnode);
        pnode.Prev(mHead);
        pnode.Next().Prev(pnode);
    }
    protected wListNode<NodeType> mHead;
    int mSize;

    //    private:
    //  wNodePool &operator=( wNodePool )
    //{
    //}
};

public class wListNode<DataType>
{
    //public:
    //typedef T            DataType;
    //typedef wListNode   NodePtr;

    public wListNode<DataType> Next() { return mNext; }
    public wListNode<DataType> Prev() { return mPrev; }
    public void Next(wListNode<DataType> next) { mNext = next; }
    public void Prev(wListNode<DataType> prev) { mPrev = prev; }
    public DataType Data() { return mData; }
    public void SetData(DataType data)
    {
        mData = data;
    }

    protected wListNode<DataType> mNext;
    protected wListNode<DataType> mPrev;
    protected DataType mData;
};