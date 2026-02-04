

public class ProductDll
{
    //  ProductInfo operator()() { return mProc? mProc():0; }

    //bool IsBadVersion() { return m_hDll != null && (!mProc || !mProc()); }

    public ProductDll(string Path, bool NeedSlash = false)
    {
        string DllName;
        DllName = string.Format(NeedSlash ? "{0}\\{1}" : "{0}{1}",Path,ProductInfo.PInfoDll);

        //m_hDll = LoadLibrary(DllName);
        //mProc = m_hDll ? GetProcAddress(m_hDll, "GetPI") : 0;
    }

    ~ProductDll()
    {
        //if (m_hDll) FreeLibrary(m_hDll);
    }

    //HMODULE m_hDll;
    //GetPIProc mProc;
};