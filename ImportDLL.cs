using DWORD = System.UInt32;
using static DLLEmulation;
/// <summary>
/// Ёмул€ци€ загрузки DLL
/// </summary>
public class ImportDLL
{
    public static void LoadDLLS()
    {
        //MainMenuStorm_dll.DllMain(null, DLL_PROCESS_ATTACH, null); ;
        renderer_dll.DllMain();
        stormdata_dll.DllMain();
        stdlogic_dll.DllMain();
        //mctrls_dll.DllMain(null, DLL_PROCESS_ATTACH,null);
        hashtools_dll.DllMain();
    }


}

