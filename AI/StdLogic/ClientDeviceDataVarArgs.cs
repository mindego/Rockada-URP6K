using DWORD = System.UInt32;
using WORD = System.UInt16;
public class ClientDeviceDataVarArgs : ClientDeviceData
{
    //struct unknown{
    int mDecl; //: 28; // 2 bites for type for 14 parameters
    int mCount; //: 4;
                //};

    string mData;

    void addType(int type, int size)
    {
        mDecl |= type << 2 * mCount++;
        mLength += (ushort)size;
    }

    public ClientDeviceDataVarArgs(DWORD packet_type = 0) : base((ushort) packet_type, 32)
    {
        mCount = 0;
        mDecl = 0;
    }


    int getParamCount() { return mCount; }
    int getParamDecl() { return mDecl; }
    string getParamData() { return mData; }

    public void addWord(int val)
    {
        //base.GetData<WORD>(mLength) = val;
        //addType(PT_WORD, sizeof(WORD));
    }

    public void addDword(int val)
    {
        //base.GetData<DWORD>(mLength) = val;
        //addType(PT_DWORD, sizeof(DWORD));
    }

    public void addFloat(float val)
    {
        //TODO! Реализовать по необходимости
        //ClientDeviceData::GetData<float>(mLength) = val;
        //addType(PT_FLOAT, sizeof(float));
    }

    void addData(MemBlock val)
    {
        //TODO! Реализовать по необходимости
        //ClientDeviceData::GetData<WORD>(mLength) = val.size;
        //string blk = base.GetData<string>((uint)mLength + sizeof(WORD));

        //for (int i = 0; i < val.size; ++i) blk[i] = val.ptr[i];
        //addType(PT_BLOCK, val.size + sizeof(WORD));
    }

    public void addString(string str)
    {
        //TODO! Реализовать по необходимости
        //addData(MemBlock(str, StrLen(str) + 1));
    }
};
