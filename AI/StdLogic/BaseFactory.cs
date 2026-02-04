public abstract class BaseFactory : IVmFactory
{
    public BaseFactory(IQuery q, IVmFactory prev_factory)
    {
        myQuery = q;
        myPrevFactory = IRefMem.SafeAddRef(prev_factory);
    }
    //: myQuery(qr), myPrevFactory(SafeAddRef(prev_factory)) { }

    public IQuery myQuery;
    public IVmFactory myPrevFactory;

    public virtual IVmCommand createCommand(uint name, IVmVariablePool pool = null, IVmController cont = null)
    {
        throw new System.NotImplementedException();
    }

    public virtual IVmController createController(uint name)
    {
        
        throw new System.NotImplementedException("Create controller " + name.ToString("X8"));
    }

    public void AddRef()
    {
        //throw new System.NotImplementedException();
    }

    public int Release()
    {
        return 0;
        //throw new System.NotImplementedException();
    }

    public int RefCount()
    {
        return 0;
        //throw new System.NotImplementedException();
    }
}