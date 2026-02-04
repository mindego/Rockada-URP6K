//using PYMENU_API = public;
using System.Globalization;
using System.IO;
using static ProductDefs;

public static class PythonIntegrationW
{
    public delegate void func(string s);

    //public static bool appendFillPath<func>(string dir)
    //{
    //    //func(std::string(getRootFile()) + dir);
    //    return true;
    //}
    //    template<void func(std::string)> bool appendFillPath(cstr dir)
    //{
    //    func(std::string(getRootFile()) + dir);
    //    return true;
    //}
    public static bool appendFillPath(func myFunc,string dir)
    {
        myFunc(getRootFile() + dir);
        return true;
    }

    public static bool init(string home)
    {
        //return appendFillPath<PythonIntegration.init>(home);
        return appendFillPath(PythonIntegration.init,home);
    }

    public static bool addPath(string path)
    {
        return appendFillPath(PythonIntegration.addPath,path);
    }
    public static bool done()
    {
        PythonIntegration.done();
        return true;
    }
};

public static class PythonIntegration
{
    //struct Error;
    //struct FatalError;
    //struct PythonAlreadyInitializedError;

    static bool initialized = false;
    public static void init(string home)
    {
    //    bool inited = false;
    //    if (!inited)
    //    {
    //        if (Py_IsInitialized())
    //            throw PythonIntegration::PythonAlreadyInitializedError();
    //        inited = true;
    //        Py_SetPythonHome(home);
    //        Py_Initialize();
    //        if (PyErr_Occurred())
    //        {
    //            PyErr_Print();
    //            throw PythonIntegration.FatalError("PythonIntegration::init:Py_Initialize failed...");
    //        }
    //        std::string str = "import sys;import os;"
    //"sys.path = [p for p in sys.path if p.startswith(os.path.abspath(r'" + home + "'))]\n";
    //        PyRun_SimpleString(const_cast<char*>(str.c_str()));
    //        if (PyErr_Occurred())
    //        {
    //            PyErr_Print();
    //        }
    //        PythonIntegration.initialized = true;
    //    }
    }
    public static void addPath(string path)
    {
        //std::string str = "import sys\nsys.path.insert(0,r\'" + menu_modules_path + "\')\n";
        //PyRun_SimpleString(const_cast<char*>(str.c_str()));
        //if (PyErr_Occurred())
        //{
        //    PyErr_Print();
        //    throw PythonIntegration::FatalError("PythonIntegration::addPath:PyRun_SimpleString failed...");
        //}
    }
    public static void done() { }
};