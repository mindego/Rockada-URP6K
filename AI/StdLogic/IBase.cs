public class RefMem<T>  
{
    private int ref_count;
  RefMem()
    {
        ref_count = 1;
    }
    public int Release()
    {
        --ref_count;
        //if (!(--ref_count))
        //{
        //    Destroy();
        //    delete this;
        //    return 0;
        //}

        return ref_count;
    }
    public void AddRef()
    {
        ++ref_count;
    }
};


