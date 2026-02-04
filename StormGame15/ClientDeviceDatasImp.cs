using DWORD = System.UInt32;
using WORD = System.UInt16;
class ClientDeviceLabelCreateData : ClientDeviceDataCreate
{
    // list of client devices

    float mX;
    float mY;
    char mColorIndex;
    char mAlign;
    float mFontSize;

    public ClientDeviceLabelCreateData(int type, string name, float x, float y, char index, float sz, char aligned) : base(name, (uint)type, LABEL_CREATE_DATA_V1, 16)
    {
        mX = x;
        mY = y;
        mColorIndex = index;
        mFontSize = sz;
        mAlign = aligned;
    }
};

public class ClientDeviceHideData : ClientDeviceData
{
    bool mOff;
    public ClientDeviceHideData(bool off) : base(ClientDeviceDataCreate.DEVICE_HIDE_DATA_V1, 1)
    { mOff = off; }
};

public class ClientDeviceTableCreateData : ClientDeviceDataCreate
{
    float mX;
    float mY;
    float mH;
    float mW;
    WORD mColoumns;
    public ClientDeviceTableCreateData(string name, float x, float y, float h, float w, WORD cols) : base(name, CLIENT_DEVICE_TABLE, TABLE_CREATE_DATA_V1, 16)
    {
        mX = (x);
        mY = (y);
        mH = (h);
        mW = (w);
        mColoumns = (cols);
    }

};

public class ClientDeviceTableAddRowData : ClientDeviceData
{
    WORD mRowName;
    int mColorIndex;
    int mTextColorIndex;
    float mFontSize;
    float mFontSpacing;

    public ClientDeviceTableAddRowData(WORD name,
                                int cindex,
                                int ctindex,
                                float font_size,
                                float font_spacing) : base(TABLE_ADD_ROW_DATA_V1, 16)
    {
        mRowName = name;
        mColorIndex = cindex;
        mTextColorIndex = ctindex;
        mFontSize = font_size;
        mFontSpacing = font_spacing;
    }
};

public class ClientDeviceTableSetRowData : ClientDeviceData
{
    WORD mRowName;
    int mColorIndex;
    int mTextColorIndex;
    public ClientDeviceTableSetRowData(WORD row_name,
                                 int cindex,
                                 int ctindex) : base(TABLE_SET_ROW_DATA_V1, 16)
    {
        mRowName = row_name;
        mColorIndex = cindex;
        mTextColorIndex = ctindex;
    }
};

public class ClientDeviceTableDelRowData : ClientDeviceData
{
    WORD mRowName;
    public ClientDeviceTableDelRowData(WORD row_name) : base(TABLE_DEL_ROW_DATA_V1, 16)
    {
        mRowName = row_name;
    }
};

public class ClientDeviceTableReindexData : ClientDeviceData
{
    WORD mRowCount;
    WORD mRowsIndexesOffset;

    public ClientDeviceTableReindexData(WORD row_count, WORD rows) : base(TABLE_SET_REINDEX_DATA_V1, 32)
    {
        mRowCount = row_count;
        mRowsIndexesOffset = (ushort) AddData(row_count, rows);
    }
};

public class ClientDeviceClInfoCreateData : ClientDeviceDataCreate
{
    public ClientDeviceClInfoCreateData(string name) : base(name, CLIENT_DEVICE_CLINFO, CLINFO_CREATE_DATA_V1, 32) { }
};

public class ClientDeviceTimeLeftUpdateContentData : ClientDeviceData
{
    public DWORD myTime;
};

public class ClientDeviceTableSetValueData : ClientDeviceData
{
    WORD mRowName;
    WORD mColoumn;
    WORD mContentOffset;

    public ClientDeviceTableSetValueData(WORD row_name, WORD col, string str) : base(ClientDeviceData.TABLE_SET_VALUE_DATA_V1, 32)
    {
        mRowName = row_name;
        mColoumn = col;
        mContentOffset = (ushort) AddString(str);
    }

};

public class ClientDeviceConfigCreateData : ClientDeviceDataCreate
{
    public ClientDeviceConfigCreateData(string name): base(name, CLIENT_DEVICE_CONFIG, CONFIG_CREATE_DATA_V1,32) {}
};









