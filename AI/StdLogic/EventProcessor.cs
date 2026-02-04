using DWORD = System.UInt32;
using System.Collections.Generic;
using UnityEngine;
using static AICommon;
using static MessageCodes;
using static Parsing;

public class EventProcessor : IEventProcessor
{
    const int MAX_CODES = 64;
    const char EOL_SYMBOL1 = '\r'; //0xA
    const char EOL_SYMBOL2 = '\n'; //0xD
    List<EventInfo> mlEvents = new List<EventInfo>();
    EventInfo[] mpIndex;

    public EventProcessor()
    {
        mpIndex = null;
    }

    void Alltrim(string str, out string buf)
    {
        string cur = str.Trim();
        buf = cur;
    }
    int AddEvent(string name, string short_name, iUnifiedVariableString str)
    {

        DWORD hashed_name = Hasher.HshString(short_name);

        foreach (var info in mlEvents)
        {
            if (hashed_name == info.GetName())
            {
                if (IsLogged(DEBUG_MEDIUM))
                {
                    AiMessage(MSG_ERROR, sAiWarning, "string \"{0}\" define already existing event \"{1}\"", name, short_name);
                }
                return 0;
            }
        }

        int dim = str.StrLen();
        if (dim == 0) return 0;

        int num = 0;
        DWORD[] codes = new DWORD[MAX_CODES];
        string buffer;
        str.StrCpy(out buffer);

        int n = 0;
        float free_radio_time = 0f;
        float dispersion = 0f;

        string[] multipart = buffer.Split(EOL_SYMBOL1);

        if (multipart.Length == 2)
        {
            string eventparams = multipart[0].Trim(new char[] { '[', ']' });
            buffer = multipart[1];

            string[] evp = eventparams.Split(',');
            if (evp.Length ==2)
            {
                dispersion = int.Parse(evp[1]);
            }
            free_radio_time = int.Parse(evp[0]);
            
        }

        codes[num++] = Hasher.HshString(buffer.Trim());


        if (num>0)
        {
            if (IsLogged(DEBUG_MEDIUM))
            {
                AiMessage(MSG_UNKNOWN, "AddEvent", "Name={0} Count={1} FRT={2} Disp={3}", short_name, num, free_radio_time, dispersion);
            }
            mlEvents.Add(new EventInfo(hashed_name, codes[0], free_radio_time, dispersion));
        }
        if (num != 0) num = 1;
        return num;
    }
    void BuildIndex()
    {
        Asserts.AssertBp(mlEvents.Count != 0);
        mpIndex = new EventInfo[mlEvents.Count];
        int n = 0;
        foreach (EventInfo info in mlEvents)
        {
            mpIndex[n++] = info;
        }
        //for (EventInfo* info = mlEvents.Head(); info; info = info->Next())
        //    mpIndex[n++] = info;
    }
    int LoadEvents(iUnifiedVariableContainer db)
    {
        Debug.Log("Loading events container: " + db);
        int upload = 0;
        if (db != null)
        {
            for (DWORD hndl = 0; (hndl = db.GetNextHandle(hndl)) != 0;)
            {
                iUnifiedVariable cur = db.GetVariableByHandle(hndl);
                //Debug.Log(string.Format("Loading handle {0} : {1}", hndl, cur == null ? "FAILED":cur));
                Asserts.AssertBp(cur != null);
                string name = cur.getNameLong();
                iUnifiedVariableString arr = (iUnifiedVariableString)(cur.Query(iUnifiedVariableString.ID));
                iUnifiedVariableContainer cont = (iUnifiedVariableContainer)(cur.Query(iUnifiedVariableContainer.ID));

                if (arr != null)
                {
                    //Debug.Log(string.Format("Loading handle {0} as string {1}", hndl, arr));
                    upload += AddEvent(name, cur.getNameShort(), arr);
                }
                else if (cont != null)
                {

                    //Debug.Log(string.Format("Loading handle {0} as container {1}", hndl, cont));
                    upload += LoadEvents(cont);
                }
                else if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
                {
                    AICommon.AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, "incompatible type of variable \"{0}\"", name);
                }
                //SafeRelease(cur);
                cur = null;
                arr = null;
            }
        }
        //Debug.Log("Loaded events container: " + upload);
        return upload;
    }

    public bool Initialize()
    {
        iUnifiedVariableContainer events = stdlogic_dll.mpAiData != null ? stdlogic_dll.mpAiData.GetVariableTpl<iUnifiedVariableContainer>("Events") : null;
        int upload = LoadEvents(events);
        if (upload != 0)
            BuildIndex();
        Debug.Log("Events loaded: " + upload);
        return upload != 0;

    }
    void Destroy()
    {
        mlEvents.Clear();
        if (mpIndex != null)
            mpIndex = null;

    }
    // API
    public virtual DWORD GetEventsCount()
    {
        return (uint)mlEvents.Count;
    }
    public virtual AiEventInfo GetEventInfo(DWORD name)
    {
        //Debug.Log("Events count: " + mlEvents.Count);
        for (int i = 0; i < mlEvents.Count; ++i)
        {
            if (name == mpIndex[i].GetName())
            {
                //Debug.Log(string.Format("{0} vs {1}", Hasher.StringHsh(name), Hasher.StringHsh(mpIndex[i].GetName())));
                return new AiEventInfo(name, mpIndex[i].GetMessageCode(), mpIndex[i].getFreeRadioTime(), mpIndex[i].GetDispersion());
            }
        }
        return new AiEventInfo();
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        //TODO!! STub
        return 0;
        //throw new System.NotImplementedException();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }
}

class EventInfo
{
    DWORD mName;
    float myFreeRadioTime;
    float mDispersion;
    DWORD myCode;
    public EventInfo()
    {
        myFreeRadioTime = 0f;
    }
    public EventInfo(DWORD name, DWORD code, float free_radio_time, float disp)
    {
        myCode = code;
        mName = name;
        myFreeRadioTime = free_radio_time;
        mDispersion = disp;
    }

    public DWORD GetName() { return mName; }
    public float getFreeRadioTime() { return myFreeRadioTime; }
    public float GetDispersion() { return mDispersion; }
    public DWORD GetMessageCode() { return myCode; }
};

