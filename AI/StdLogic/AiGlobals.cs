using DWORD = System.UInt32;

public static class stdlogic_dll
{
    public static ILog gamelog;
    public static float mCurrentTime=0f;
    public static DWORD mCurrentTick=0;
    public static DWORD mDebugLevel = AICommon.DEBUG_NONE;
    public static iUnifiedVariableContainer mpAiData;

    public const string sAiDataName = "AiData.dat";

    public static bool DllMain()
    {
        //iUnifiedVariableDB mpAiDataDB = UniVarUtils.CreateUnifiedVariableDB(Constants.UNIVARS_VERSION, sAiDataName,true);
        iUnifiedVariableDB mpAiDataDB = UniVarUtils.CreateUnifiedVariableDB(Constants.UNIVARS_VERSION, ProductDefs.GetPI().getHddFile(sAiDataName));
        if (mpAiDataDB == null) return false;
        mpAiData = mpAiDataDB.GetRootTpl<iUnifiedVariableContainer>();
        if (mpAiData == null) return false;

        return true;
    }
}