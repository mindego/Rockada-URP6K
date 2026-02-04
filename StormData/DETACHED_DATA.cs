public class DETACHED_DATA : SUBOBJ_DATA
{
    public DETACHED_DATA(string name) : base(name)
    {
        SetFlag(SC_DETACHED);
    }
};

