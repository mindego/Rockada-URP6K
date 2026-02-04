using DWORD = System.UInt32;
using WORD = System.UInt16;
public class ClientDeviceDataCreate : ClientDeviceData
{
    public const int MAX_CD_NAME_LEN = 4;

 
    //char mName[MAX_CD_NAME_LEN];
    string mName;
    DWORD mDeviceType;
    public ClientDeviceDataCreate(string name, DWORD dtype, WORD type, WORD length):base(type,length)
    {
        mName = name;
        mDeviceType = dtype;
    }
};

public class ClientDeviceData

{
    // list of client devices
    public const uint CLIENT_DEVICE_TABLE = 0xEB38BF1C;
    public const uint CLIENT_DEVICE_LABEL = 0x13B660B2;
    public const uint CLIENT_DEVICE_TIMELEFT = 0x50B6B31A;
    public const uint CLIENT_DEVICE_CONFIG = 0xB64549E3;
    public const uint CLIENT_DEVICE_PRINTER = 0x04F2F71B;
    public const uint CLIENT_DEVICE_MENU = 0xD44F7485;

    public const uint CLIENT_DEVICE_CLINFO = 0xEB62CECF;

    public const ushort DEVICE_HIDE_DATA_V1 = 0xB4AD;

    public const ushort LABEL_CREATE_DATA_V1 = 0x9FA3;
    public const ushort LABEL_UPDATE_TITLE_DATA_V1 = 0xFC5A;
    public const ushort LABEL_UPDATE_CONTENT_DATA_V1 = 0xB912;

    public const ushort TIMELEFT_UPDATE_CONTENT_DATA_V1 = 0x3AD7;

    public const ushort CONFIG_CREATE_DATA_V1 = 0x4159;
    public const ushort CONFIG_UPDATE_CONFIG_V1 = 0x718E;

    public const int HEADER_TEAM_ID = 999;// table header
    public const ushort TABLE_CREATE_DATA_V1 = 0x6837;
    public const ushort TABLE_ADD_ROW_DATA_V1 = 0x61A0;
    public const ushort TABLE_SET_VALUE_DATA_V1 = 0xD7DB;
    public const ushort TABLE_SET_ROW_DATA_V1 = 0x08D2;
    public const ushort TABLE_DEL_ROW_DATA_V1 = 0xED30;
    public const ushort TABLE_SET_REINDEX_DATA_V1 = 0x9BC8;
    public const ushort TABLE_SET_TEAM_COUNT_V1 = 0x0586;

    public const ushort PRINTER_CREATE_DATA_V1 = 0xEC51;
    public const ushort PRINTER_PRINT_DATA_V1 = 0x8EB6;

    public const ushort MENU_CREATE_DATA_V1 = 0x6EE5;

    public const ushort CLINFO_CREATE_DATA_V1 = 0x6486;
    public const ushort CLINFO_UPDATE_DATA_V1 = 0x010F;

    public WORD mType;            // тип пакета, >=DPT_USER
    public WORD mLength;          // длина пакета
    public ClientDeviceData(WORD type, WORD length)
    {
        mType = type;
        mLength = length;
    } 
    public ClientDeviceData() { }

    public int AddString(string myString)
    {
        //int size = CopySize((char*)this + mLength, string);
        //mLength += size;
        //return mLength - size;
        ushort size= (ushort)myString.Length;
        mLength += size;

        return mLength - size;
    }

    //template<class T> inline int AddData(DWORD count, const T* pStr);
    //template<class T> inline T* GetData(DWORD Offs) const;

    public int AddData<T>(DWORD count, T pData)
    {
        Asserts.Assert(pData!=null);
        //TODO! Реализовать по необходимости
        //T mypData = GetData<T>(mLength);
        //for (int i = 0; i < count; ++i)
        //    mypData[i] = pData[i];
        //i = count * sizeof(T);
        //mLength += i;
        //return mLength - i;
        return 1;

    }
    public T GetData<T>(DWORD Offs)
    {
        Asserts.Assert(Offs!=0);
        //TODO! Реализовать по необходимости
        //return (T)((char)this + Offs);
        return default(T);
    }
};
