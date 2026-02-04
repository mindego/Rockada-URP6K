using System.Collections.Generic;
using DWORD = System.UInt32;
using crc32 = System.UInt32;
using WORD = System.UInt16;

public class CDWrapper
{
    public const int WHOLE_ROW_UPDATED = 999;
    //char[] mHUDName = new char[ClientDeviceDataCreate.MAX_CD_NAME_LEN];
    string mHUDName;
    DWORD mName;
    LinkedList<MissionClient> mpClientsList;

    public string GetHUDName() { return mHUDName; }
    DWORD GetName() { return mName; }

    public void Hide(MissionClient client, bool off)
    {
        ClientDeviceHideData data = new ClientDeviceHideData(off);
        UpdateDevice(client, data);
    }

    public void CreateDevice(MissionClient mcl, ClientDeviceDataCreate pData)
    {
        if (mcl != null)
            mcl.Client().CreateDevice(GetName(), pData);
        else
        {
            //for (LinkedListNode<MissionClient> ml = mpClientsList.Fir; ml; ml = ml->Next())
            foreach (MissionClient ml in mpClientsList)
                ml.Client().CreateDevice(GetName(), pData);
        }
    }

    public void UpdateDevice(MissionClient mcl, ClientDeviceData pData)
    {
        if (mcl != null)
            mcl.Client().UpdateDevice(GetName(), pData);
        else
        {
            //for (MissionClient ml = mpClientsList->Head(); ml; ml = ml->Next())
            foreach (MissionClient ml in mpClientsList)
                ml.Client().UpdateDevice(GetName(), pData);
        }
    }

    public void DeleteDevice(MissionClient mcl)
    {
        if (mcl != null)
            mcl.Client().DeleteDevice(GetName());
        else
        {
            //for (MissionClient ml = mpClientsList->Head(); ml; ml = ml->Next())
            foreach (MissionClient ml in mpClientsList)
                ml.Client().DeleteDevice(GetName());
        }
    }



    public CDWrapper(string hud_name, LinkedList<MissionClient> cl_list)
    {
        mpClientsList = cl_list;
    if (hud_name != "")
            //StrnCpy(mHUDName, hud_name, MAX_CD_NAME_LEN);
            mHUDName = hud_name;
        else
            mHUDName = "";
        mName = Hasher.HshString(mHUDName);
    }
};

public class RowInfo
{
    public WORD mRowName;
    public int mBackColorIndex;
    public int mTextColorIndex;
    public float mFontSize;
    public float mFontSpacing;

    public void Set(int tc, int bc, float fz, float fs)
    {
        mBackColorIndex = bc;
        mTextColorIndex = tc;
        mFontSize = fz;
        mFontSpacing = fs;
    }
};

//#define WHOLE_ROW_UPDATED    999

interface iSortRowClient
{
    public int Compare(WORD num1, WORD num2);
};

public interface iGetTableInfoClient
{
    public bool GetRow(WORD y_name, ref RowInfo ri);
    public bool GetCell(WORD y_name, WORD x, ref string buf, DWORD len);
};

public class CDWrapperTable : CDWrapper
{
    static iSortRowClient mpCurrentSorter;
    crc32 mLastSort;

    float mX, mY, mHeight, mWidth;
    WORD mColsCount;

    // wrapped api
    public void Create(MissionClient client)
    {
        ClientDeviceTableCreateData data = new ClientDeviceTableCreateData(GetHUDName(), mX, mY, mHeight, mWidth, mColsCount);
        CreateDevice(client, data);
    }
    public void AddRow(MissionClient client, WORD row_name, int back_color_index, int text_color_index, float font_size, float font_spacing)
    {
        ClientDeviceTableAddRowData data = new ClientDeviceTableAddRowData(row_name, back_color_index, text_color_index, font_size, font_spacing);
        UpdateDevice(client, data);
    }
    void SetRowColor(MissionClient client, WORD row_name, int back_color_index, int text_color_index)
    {
        ClientDeviceTableSetRowData data = new ClientDeviceTableSetRowData(row_name, back_color_index, text_color_index);
        UpdateDevice(client, data);
    }

    void SetValue(MissionClient client, WORD row_name, WORD col_index, string value)
    {
        //TODO! Реализовать по необходимости.
        //char buf[1024];
        //ClientDeviceTableSetValueData val = (ClientDeviceTableSetValueData)buf;
        //new(val) ClientDeviceTableSetValueData(row_name, col_index, value);
        //UpdateDevice(client, val);
    }

    void SetValue(MissionClient client, WORD row_name, WORD col_index, int val)
    {
        //TODO! Реализовать по необходимости.
        //char buf[36];
        //wsprintf(buf, "%2d", val);
        //SetValue(client, row_name, col_index, buf);
    }

    void DeleteRow(MissionClient client, WORD row_name)
    {
        ClientDeviceTableDelRowData data = new ClientDeviceTableDelRowData(row_name);
        UpdateDevice(client, data);
    }

    void Reindex(WORD row_count, WORD[] rows, iSortRowClient sl)
    {
        //STUB!
        //TODO! Реализовать по необходимости
        //dllmain.AssertBp(sl!=null);
        //mpCurrentSorter = sl;
        //qsort(rows, row_count, sizeof(WORD), RowsCompare);
        //mpCurrentSorter = null;
        //crc32 cur_sort = Crc32.Code(cstr(rows), row_count * sizeof(WORD));
        //if (cur_sort != mLastSort)
        //{
        //    mLastSort = cur_sort;
        //    // sending data to clients
        //    char buf[1024];
        //    ClientDeviceTableReindexData val = (ClientDeviceTableReindexData*)buf;
        //    new(val) ClientDeviceTableReindexData(row_count, rows);
        //    UpdateDevice(null, val);
        //}
    }

    public void Synchronize(MissionClient client, WORD row_count, WORD[] rows, iGetTableInfoClient gcl)
    {
        //STUB!
        //TODO! Реализовать по необходимости
        //dllmain.AssertBp(gcl!=null);
        //char buf[256];
        //RowInfo info;
        //for (int i = 0; i < row_count; ++i)
        //{
        //    gcl.GetRow(rows[i], info);
        //    AddRow(client, info.mRowName, info.mBackColorIndex, info.mTextColorIndex, info.mFontSize, info.mFontSpacing);
        //    for (int j = 0; j < mColsCount; ++j)
        //    {
        //        if (gcl->GetCell(rows[i], j, buf, 256))
        //            SetValue(client, rows[i], j, buf);
        //    }
        //}
    }

    void SetTeamCount(MissionClient client, WORD team_count)
    {
        //STUB!
        //TODO! Реализовать по необходимости
        //AllocCDDVA(dt, 16, TABLE_SET_TEAM_COUNT_V1);
        //dt.addWord(team_count);
        //UpdateDevice(client, &dt);
    }

    //int RowsCompare( const void* arg1, const void* arg2)
    //{
    //    WORD d1 = *((WORD*)arg1);
    //    WORD d2 = *((WORD*)arg2);
    //    return CDWrapperTable::mpCurrentSorter->Compare(d1, d2);
    //}


    public CDWrapperTable(string hud_name, LinkedList<MissionClient> cl_list, float x, float y, float height, float width, WORD cols_count) : base(hud_name, cl_list)
    {
        mLastSort = CRC32.CRC_NULL;
        mColsCount = cols_count;
        mX = x;
        mY = y;
        mHeight = height;
        mWidth = width;
    }
};
public class CDWrapperLabel : CDWrapper
{

    float mX, mY;
    int mColorIndex;
    float mFontSize;
    int mAlign;
    int myType;

    // wrapped api
    void Create(MissionClient client) { }
    void SetTitle(MissionClient client, string val) { }
    void SetValue(MissionClient client, string val) { }
    void SetValue(MissionClient client, int val) { }
    void SetTimeValue(MissionClient client, DWORD time) { }

    public CDWrapperLabel(string hud_name, LinkedList<MissionClient> cl_list,float x, float y, int color_index, float font_size, int aligned,int type = (int) ClientDeviceData.CLIENT_DEVICE_LABEL) : base(hud_name, cl_list)
    { 
        myType = type;
      mX=(x);
      mY=(y);
      mColorIndex=(color_index);
      mFontSize=(font_size);
        mAlign=(aligned);
     }
};

public class CDWrapperTimeLeft : CDWrapperLabel
{
    CDWrapperTimeLeft(string hud_name, LinkedList<MissionClient> cl_list,float x, float y, int color_index, float font_size, int aligned): base (hud_name, cl_list, x, y, color_index, font_size, aligned, (int)ClientDeviceData.CLIENT_DEVICE_TIMELEFT) { }

    void SendTimeValue(MissionClient client, DWORD time)
    {
        ClientDeviceTimeLeftUpdateContentData mdata = new ClientDeviceTimeLeftUpdateContentData();
        mdata.mType = ClientDeviceData.TIMELEFT_UPDATE_CONTENT_DATA_V1;
        //mdata.mLength = sizeof(ClientDeviceData) + sizeof(DWORD);
        mdata.mLength = 32;//TODO! Реализовать как нужно!
        mdata.myTime = time;
        base.UpdateDevice(client, mdata);
    }

};
public class CDWrapperConfigController : CDWrapper
{

    string ConfigName;

    // wrapped api
    void Create(MissionClient client)
    {
        ClientDeviceConfigCreateData data = new ClientDeviceConfigCreateData(GetHUDName());
        CreateDevice(client, data);
        if (ConfigName!="")
            ApplyConfig(client);

    }
    void SetConfig(MissionClient client, string val)
    {
        Clear();
        //ConfigName = StrDup(val);
        ConfigName = val;
        ApplyConfig(client);
    }

    CDWrapperConfigController(string hud_name, List<MissionClient> cl_list):base(hud_name,null)
         { ConfigName = ""; }
  
    void Clear()
    {
        if (ConfigName!="")
        {
                      ConfigName = "";
        }

    }

    void ApplyConfig(MissionClient client)
    {
        //TODO! Реализовать по необходимости
        //char buf[1024];
        //ClientDeviceConfigUpdateContentData* mdata =
        //(ClientDeviceConfigUpdateContentData*)buf;
        //mdata->mType = CONFIG_UPDATE_CONFIG_V1;
        //mdata->mLength =
        //sizeof(ClientDeviceData) + CopySize(mdata->mConfigName, ConfigName);
        //UpdateDevice(client, mdata);
    }

    ~CDWrapperConfigController()
    {
        Clear();
    }
};


public class CDWrapperPrinter : CDWrapper
{
    // wrapped api
    public void Create(MissionClient client) { }

    public CDWrapperPrinter(string hud_name, LinkedList<MissionClient> cl_list): base(hud_name,cl_list)
    { }

    public void TalkToClient(MissionClient mcl, DWORD str_id, params string[] args) { }
};
public class CDWrapperMenu : CDWrapper
{
    // wrapped api
    public void Create(MissionClient client) { }

    public CDWrapperMenu(string hud_name, LinkedList<MissionClient> cl_list): base(hud_name, cl_list)
    { }
};
public class CDWrapperClientInfo
{
    crc32 myDeviceId;

    public CDWrapperClientInfo() {
        myDeviceId = Hasher.HshString("ClientInfo"); }

    public void create(iClient client)
    {
        ClientDeviceClInfoCreateData buffer=new ClientDeviceClInfoCreateData("CLI");
        client.CreateDevice(myDeviceId, buffer);
    }

    public void setPlayerName(iClient client)
    {
        AllocCDDVA(out ClientDeviceDataVarArgs buffer, 128, ClientDeviceData.CLINFO_UPDATE_DATA_V1);
        buffer.addString(client.GetPlayerName());
        client.UpdateDevice(myDeviceId, buffer);
    }

    public void setClientStatus(iClient client, DWORD status)
    {
        AllocCDDVA(out ClientDeviceDataVarArgs buffer, 32, ClientDeviceData.CLINFO_UPDATE_DATA_V1);
        buffer.addWord((int)status);
        client.UpdateDevice(myDeviceId, buffer);
    }

    public void setMissionStatus(iClient client, DWORD status)
    {
        AllocCDDVA(out ClientDeviceDataVarArgs buffer, 32, ClientDeviceData.CLINFO_UPDATE_DATA_V1);
        buffer.addDword((int)status);
        client.UpdateDevice(myDeviceId, buffer);
    }

    public void setGodMode(iClient client, DWORD ammo, DWORD armor, DWORD ground, DWORD myobject)
    {
        AllocCDDVA(out ClientDeviceDataVarArgs buffer, 32, ClientDeviceData.CLINFO_UPDATE_DATA_V1);

        DWORD d = ammo | (armor << 8) | (ground << 16) | (myobject << 24);
        buffer.addFloat(d);
        client.UpdateDevice(myDeviceId, buffer);
    }

    void deleteDevice(iClient client)
    {
        client.DeleteDevice(myDeviceId);
    }

    public static void AllocCDDVA(out ClientDeviceDataVarArgs name, int size, DWORD packet_type) {
        name = new ClientDeviceDataVarArgs(packet_type);
    }

};



/*

FragTable

  При подключении нового игрока:
    Создание новой таблицы на клиенте
    Синхронизация всех данных на клиенте
    Создание новой строчки на всех клиентах
    Установка цветного выделения для клиента
    Reindex и установка Reindex для всех клиента (обязательно)

  При отключении игрока:
    Удаление его строчки на всех клиентах
    Reindex и установка Reindex для всех клиента (обязательно)

  При изменении какой-либо ячейки
    Изменение содержимого ячейки на всех клиентах
    Reindex и установка Reindex для всех клиента ( если нужно )

  На смерть клиента
    Включается на клиенте

TimeLimit

  При изменении времени на секунду:
    Посылка всем клиентам

  На смерть клиента
    Включается на клиенте



FragCounter

  При изменении ячейки посылается соответствующему клиенту


ElapsedTime

  На смерть клиента
    Включается на клиенте

*/
