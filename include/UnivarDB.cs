using System;
using UnityEngine;
using DWORD = System.UInt32;

public class UnivarDB
{

    private iUnifiedVariableDB myDB;
    private iUnifiedVariableContainer myRoot;


    public T GetVariableTpl<T>(string name, DWORD crc) where T:class
    {
        iUnifiedVariable t = myRoot.GetVariableByName(name, crc);
        //if (t==null) return null;
        //T tpl = (T*)t->Query(T::ID);
        //  t->Release();
        //return tpl;
        return (T)t;
    }

    private iUnifiedVariableContainer createRoot()
    {
        iUnifiedVariableContainer root = myDB.GetRootTpl<iUnifiedVariableContainer>();
        return root != null ? root : (iUnifiedVariableContainer)myDB.CreateRootTpl(iUnifiedVariableContainer.ID);
    }

    public  bool openRoot()
    {
        return (myRoot = createRoot()) != null;
    }

    private void closeRoot()
    {
        myRoot = null;
    }

    public iUnifiedVariableDB getDB() { return myDB; }

    //    uvc* operator -> () const { return myRoot; }
    //uvc* operator () () const { return myRoot; }

    public void close()
    {
        closeRoot();
        myDB = null;
    }

    public bool open(string name, bool read_only = true)
    {
        close();
        myDB = UniVarUtils.CreateUnifiedVariableDB(Constants.UNIVARS_VERSION, name, read_only);
        if (myDB==null) Debug.LogFormat("{0} load [{1}]",name, "FAIL");
        Debug.Log("Opening root for " + name);
        return myDB != null ? openRoot() : false;
    }

    public bool create()
    {
        return open(null, false);
    }

    public bool clear()
    {
        bool ok = myDB != null;
        if (ok)
        {
            closeRoot();
            iUnifiedVariableContainer fld = createRoot();
            ok = fld.Delete();
            if (!ok)
                fld.Release();
            openRoot();
        }
        return ok;
    }

    public bool save(string name)
    {
        bool ok = myDB != null;
        if (ok)
        {
            closeRoot();
            ok = myDB.SaveToFile(name);
            openRoot();
        }
        return ok;
    }

    public iUnifiedVariableContainer GetRoot()
    {
        return myRoot;
    }

    public override string ToString()
    {
        if (myRoot != null) return base.ToString() + " root:\n" + myRoot;
        return base.ToString();
    }
    //public UniVarContainer GetRoot()
    //{
    //    return (UniVarContainer) myRoot;
    //}

}