using DWORD = System.UInt32;

public struct IHashInfo
{
    public DWORD mCapacity;
    public DWORD mUploadInFirst;
    public DWORD mUploadInSecond;
    public DWORD mMaxUploadInFirst;
    public DWORD mMaxUploadInSecond;
};
