using DWORD = System.UInt32;
/// <summary>
/// iUnifiedVariableReference - хранение ссылки на другую переменную
/// </summary>
public interface iUnifiedVariableReference : iUnifiedVariable
{
    new public const DWORD ID = 0x9E045FA0;
    public iUnifiedVariable GetReference();
    //template<class T> T* GetReferenceTpl()const;

    // obsolete functions
    public int GetReferenceNameLength();
    public void GetReferenceName(out string dst);
    public bool SetReferenceName(string src);

    // wrappers
    public int StrLen() { return GetReferenceNameLength(); }
    public void StrCpy(out string dst) { GetReferenceName(out dst); }
    public bool SetValue(string src) { return SetReferenceName(src); }
};
