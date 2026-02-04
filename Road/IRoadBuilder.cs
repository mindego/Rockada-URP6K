
/// <summary>
/// строит ноды по линиям 
/// </summary>
public interface IRoadBuilder : IRefMem
{
    public const int ROAD_BUILDER_VERSION = 0x00010001;
    // initialize 
    public bool MergeData(iUnifiedVariableArray uva, IPercentNotifier pn);
    public RoadNetData GetData();
};

public interface IPercentNotifier
{
    public void NotifyPercentage(float percent, string action);
};