using System;

public class HashObjectCont : TLIST_ELEM_IMP<HashObjectCont>,IDisposable
{
     

    public IHashObject myobject;
    public HashObjectCont(IHashObject _object)
    {
        myobject = _object;
    }

    public void Dispose()
    {
        myobject.Dispose();
    }
};
