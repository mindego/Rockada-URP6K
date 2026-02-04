using DWORD = System.UInt32;

public interface IEventProcessor : IObject
{
    public AiEventInfo GetEventInfo(DWORD name);
    public DWORD GetEventsCount();
};

