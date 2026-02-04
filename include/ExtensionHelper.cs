using crc32 = System.UInt32;
using DWORD = System.UInt32;
using tuvc = iUnifiedVariableContainer;
using tuva = iUnifiedVariableArray;
using UnityEngine;

public class ExtensionHelper
{
    public ExtensionHelper(string name)
    {
        string buffer = "";
        string dot = name != null ? wintools.GetFnName(name, ref buffer) : null;
        if (dot != null)
        {
            myExt = dot + 1;
            resetName(buffer);
        }
        else
        {
            myExt = "";
            resetName(name);
        }
        refreshFullName();
    }

    public void setName(string name) { resetName(name); }
    public string getName() { return myName; }
    public void setExtension(string ext) { myExt = ext; refreshFullName(); }
    public string getExtension() { return myExt; }
    public string getFullName() { return myFullName; }
    public string getShortName() { return myShortName; }
    private void refreshFullName()
    {
        //char buffer[MAX_PATH];
        //wsprintf(buffer, "%s%s%s", cstr(myName), *myExt.getPtr() ? "." : "", cstr(myExt));
        //myFullName = buffer;
        myFullName = string.Format("{0}{1}{2}",myName, myExt != null ? "." : "", myExt);
    }

    void resetName(string name)
    {
        myName = name;
        myShortName = wintools.GetFnDir(myName);
        refreshFullName();
    }

    string myName;
    string myExt;
    string myFullName;

    string myShortName;

    public override string ToString()
    {
        return string.Format("{0}.{1}\nFullName: {2}\nShortName: {3}", myName, myExt, myFullName, myShortName);
    }
}
