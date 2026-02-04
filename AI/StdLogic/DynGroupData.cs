public class DynamicGroupData
{
    GROUP_DATA data;
    public GROUP_DATA GetGroupData() { return data; }
    public DynamicGroupData()
    {
        data = new GROUP_DATA();
    }
    public void Clear()
    {
        if (data!=null) data=null;
    }
    ~DynamicGroupData()
    {
        Clear();
    }
};
/// <summary>
/// UNIT_DATA
/// </summary>
public class DynamicUnitData
{
    UNIT_DATA data;
    public UNIT_DATA GetUnitData() { return data; }
    DynamicUnitData()
    {
        data = new UNIT_DATA();
    }
    public void Clear()
    {
        if (data != null) data = null;
    }
    ~DynamicUnitData()
    {
        Clear();
    }
};

