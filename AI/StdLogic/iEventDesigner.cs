using DWORD = System.UInt32;

public enum EventType { etNotCheck, etExternal, etInternal, etBase };
public interface iEventDesigner : IObject
{
    public bool CanProcessInternalEvents();
    public bool CanProcessExternalEvents();
    public bool canProcessBaseEvents();
    public bool SetInternalProperties(DWORD external, DWORD myInternal, DWORD myBase);
    public bool ProcessInternalEvent(string code, bool is_critical, DWORD recipient_index, DWORD caller_index, string caller_callsign = "", bool pass_forever = false);
    public bool ProcessExternalEvent(string code, DWORD recipient_index, string recipient_callsign, RadioMessageInfo rmp, DWORD caller_index, string caller_callsign = "", bool pass_forever = false, EventType lt_message = EventType.etExternal);
    public bool ProcessExternalEvent(string code, RadioMessage rm, RadioMessageInfo rmp, bool fill_caller, DWORD caller_index, string caller_callsign = "", bool pass_forever = false, EventType lt_message = EventType.etExternal);
    public bool ProcessExternalEvent(DWORD code, RadioMessage rm, RadioMessageInfo rmp, bool fill_caller, DWORD caller_index, string caller_callsign, bool pass_forever = false, EventType lt_message = EventType.etExternal);
    public bool Update(float scale);
};




