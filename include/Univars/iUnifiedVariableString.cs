using DWORD = System.UInt32;
/// <summary>
/// iUnifiedVariableString - хранение строки
/// </summary>
public interface iUnifiedVariableString : iUnifiedVariable
{
    new public const DWORD ID = 0xAF293CAF;
    public int StrLen();
    public void StrCpy(out string dst);
    public void StrnCpy(out string dst, int n);
    public bool SetValue(string src);

    public string GetValue();
};
