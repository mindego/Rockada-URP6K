using DWORD = System.UInt32;
// *********************************************************************************************************
// iUnifiedVariableInt - хранение int
public interface iUnifiedVariableInt : iUnifiedVariable
{
    new public const DWORD ID = 0x189596B9;
    public int GetValue();
    public int SetValue(int value);
};
