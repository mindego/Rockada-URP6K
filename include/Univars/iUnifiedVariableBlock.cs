using DWORD = System.UInt32;
/// <summary>
/// iUnifiedVariableBlock - хранение блока двоичных данных
/// </summary>
public interface iUnifiedVariableBlock : iUnifiedVariable
{
    new public const DWORD ID = 0x75710EAA;
    public int GetLength();
    public void GetValue(out byte[] dst, int dst_length);
    public bool SetValue(byte[] src,int src_length);
};
