using UnityEngine;
using DWORD = System.UInt32;

public class EventDesigner : iEventDesigner
{
    IMissionAi mpMission;
    IGroupAi mpGroup;
    DWORD mSide;
    StdGroupAi mpStdGroup;
    IRefMem mpRefMem = new RefMem();

    private float myFreeTime;
    // send parameters
    private bool checkType(bool pass_forever, EventType type)
    {
        if (!pass_forever)
        {
            switch (type)
            {
                case EventType.etExternal:
                    return canProcessExternal();
                case EventType.etInternal:
                    return canProcessInternal();
                case EventType.etBase:
                    return canProcessBase();
            }
        }
        return true;

    }
    private bool canProcessExternal() { return mCanProcess && mProcessExternal; }
    private bool canProcessInternal() { return mCanProcess && mProcessInternal; }
    public bool canProcessBase() { return mCanProcess && myProcessBase; }

    bool mCanProcess;
    bool mProcessExternal = false;
    bool mProcessInternal = false;
    bool myProcessBase = true;


    bool canSendEvent(float free_radio_time, float dispersion)
    {
        return myFreeTime >= free_radio_time + RandomGenerator.Rand01() * dispersion;
    }

    DWORD GetUnitVoice(DWORD index) { return 0; }

    // message processor
    IMessageProcessor mpMessageProcessor;


    public EventDesigner()
    {
        mpMission = null;
        mpGroup = null;
        mpStdGroup = null;
        mpMessageProcessor = null;
        mSide = (0);
        mCanProcess = (false);
        myFreeTime = (0);
    }

    //public bool Initialize(IMissionAi mission, IGroupAi group) { return true; }  // API
    public bool Initialize(IMissionAi mission, IGroupAi group)
    {
        Asserts.AssertBp(mission !=null);
        Asserts.AssertBp(group !=null);
        mpMission = mission;
        mpGroup = group;
        mpStdGroup = mpGroup!=null ? (StdGroupAi)mpGroup: null;
        mSide = mpGroup.GetGroupData().Side;
        mCanProcess = mpMission.GetMessageMode(mSide);
        return true;
    }
    public virtual bool CanProcessInternalEvents()
    {
        return canProcessInternal();
    }
    public virtual bool CanProcessExternalEvents()
    {
        return canProcessExternal();
    }
    public virtual bool canProcessBaseEvents()
    {
        return canProcessBase();
    }
    public virtual bool SetInternalProperties(DWORD p_external, DWORD p_internal, DWORD p_base)
    {
        mProcessExternal = p_external != 0;
        mProcessInternal = p_internal != 0;
        myProcessBase = p_base != 0;
        return true;
    }
    public virtual bool ProcessInternalEvent(string event_code, bool is_critical, DWORD recipient_index, DWORD caller_index, string caller_callsign, bool pass_forever = false)
    {
        if (!canProcessInternal() && false == pass_forever) return false;
        if (is_critical || mpMission.RadioChannelIsFree())
        {
            AiEventInfo code = mpMission.GetMessageCode(event_code);
            if (AIGroupsEvents.IsValid(code.mCode) && canSendEvent(code.myFreeRadioTime, code.mDispersion))
            {
                RadioMessage rm = new RadioMessage();
                rm.Code = code.mCode;
                if (is_critical)
                {
                    if (caller_callsign != "")
                        rm.CallerCallsign = caller_callsign;
                    else
                        rm.CallerCallsign = mpStdGroup != null ? mpStdGroup.mpData.Callsign : "";
                    rm.RecipientCallsign = mpStdGroup != null ? mpStdGroup.mpData.Callsign : "";
                }
                rm.CallerIndex = (int)caller_index;
                rm.RecipientIndex = (int)recipient_index;
                rm.VoiceCode = mpStdGroup != null ? (int)mpStdGroup.GetUnitVoice(caller_index) : 0;
                mpMission.ProcessRadioMessage(event_code, mpGroup, rm, false, true);
                return true;
            }
        }
        return false;
    }
    public virtual bool ProcessExternalEvent(string event_code, DWORD recipient_index, string recipient_callsign, RadioMessageInfo rmp, DWORD caller_index, string caller_callsign = null, bool pass_forever = false, EventType lt_message = EventType.etExternal)
    {
        if (!checkType(pass_forever, lt_message)) return false;
        AiEventInfo code = mpMission.GetMessageCode(event_code);
        if (AIGroupsEvents.IsValid(code.mCode) && canSendEvent(code.myFreeRadioTime, code.mDispersion))
        {
            RadioMessage rm = new RadioMessage();
            rm.Code = code.mCode;
            if (lt_message != EventType.etInternal)
            {
                if (caller_callsign != null)
                    rm.CallerCallsign = caller_callsign;
                else
                    rm.CallerCallsign = mpStdGroup != null ? mpStdGroup.mpData.Callsign : null;
            }
            rm.CallerIndex = (int)caller_index;
            rm.RecipientCallsign = recipient_callsign;
            rm.RecipientIndex = (int)recipient_index;
            rm.VoiceCode = mpStdGroup != null ? (int)mpStdGroup.GetUnitVoice(caller_index) : 0;
            mpMission.MessageProcessor().AddMessage(rm, rmp, mpGroup);
            return true;
        }
        return false;
    }
    public virtual bool ProcessExternalEvent(string event_code, RadioMessage rm, RadioMessageInfo rmp, bool fill_caller, DWORD caller_index, string caller_callsign = null, bool pass_forever = false, EventType lt_message = EventType.etExternal)
    {
        AiEventInfo code = mpMission.GetMessageCode(event_code);
        Debug.Log(string.Format("ProcessExternalEvent A [{4} {5}][{3} {2}] IsValid {0} canSendEvent {1}", AIGroupsEvents.IsValid(code.mCode), canSendEvent(code.myFreeRadioTime, code.mDispersion), code.mCode.ToString("X8"),event_code,caller_callsign,caller_index));

        if (AIGroupsEvents.IsValid(code.mCode) && canSendEvent(code.myFreeRadioTime, code.mDispersion))
            return ProcessExternalEvent(code.mCode, rm, rmp, fill_caller, caller_index, caller_callsign, pass_forever, lt_message);
        return false;

    }
    public virtual bool ProcessExternalEvent(DWORD code, RadioMessage rm, RadioMessageInfo rmp, bool fill_caller, DWORD caller_index, string caller_callsign, bool pass_forever = false, EventType lt_message = EventType.etExternal)
    {
        if (!checkType(pass_forever, lt_message)) return false;
        rm.Code = code;
        if (fill_caller)
        {
            if (caller_callsign != null)
                rm.CallerCallsign = caller_callsign;
            else
                rm.CallerCallsign = mpStdGroup != null ? mpStdGroup.mpData.Callsign : null;
            rm.CallerIndex = (int)caller_index;
        }
        if (lt_message != EventType.etNotCheck)
            rm.VoiceCode = mpStdGroup != null ? (int)mpStdGroup.GetUnitVoice(caller_index) : 0;
        Debug.Log(string.Format("ProcessExternalEvent B [{0} {1}] {2}",caller_callsign,caller_index, rm));
        mpMission.MessageProcessor().AddMessage(rm, rmp, mpGroup);
        return true;

    }
    public virtual bool Update(float scale)
    {
        if (mpMission.RadioChannelIsFree())
            myFreeTime += scale;
        else
            myFreeTime = 0;
        return true;
    }

    public void AddRef()
    {
        mpRefMem.AddRef();
    }

    public int RefCount()
    {
        return mpRefMem.RefCount();
    }

    public int Release()
    {
        return mpRefMem.Release();
    }
}
public class EventSendInfo
{
    DWORD mCode;
    float mTime;
    public DWORD GetName() { return mCode; }
    public EventSendInfo(DWORD code, float time)
    {
        mCode = (code);
        mTime = (time);
    }
    bool Update(float scale) { mTime -= scale; return mTime > 0f; }
};

