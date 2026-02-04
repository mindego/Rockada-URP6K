using UnityEngine;
using UnityEngine.InputSystem.Android;

public class ErrorLog : IErrorLog
{
    public ErrorLog(ITimeService timer)
    {
        myTimer = timer;

        if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
        {
            myLog = openLog("AiScriptExec");
            myLogLtb = openLog("AiScriptExec", "ltb", false);
            if (myLogLtb != null)
                myLogLtb.Message("Time\tOrigin\tType\tDetails");

        }
        myText = "";
    }
    ~ErrorLog()
    {
        closeLog(myLog);
        closeLog(myLogLtb);
    }

    public void setSource(string exec, string source)
    {
        if (exec != null && exec != "")
        {
            myExecutor = exec;
            setExecuteContext(exec);
        }
        myText = source;
    }
    public void showContext()
    {
        if (myLog != null)
        {
            float flt = myTimer.getTime();
            int tmp = (int)flt;
            //myLog.Message("%02d:%02d.%02d:%s", (tmp % 3600) / 60, (tmp % 3600) % 60, getMilliFromSec(flt), myContext);
            //myLog.Message(string.Format("%02d:%02d.%02d:%s", (tmp % 3600) / 60, (tmp % 3600) % 60, 0, myContext));
            myLog.Message(string.Format("{0,02:D}:{1,2:D}.{2,2:D}:{3}", (tmp % 3600) / 60, (tmp % 3600) % 60, 0, myContext));
            myContextPrinted = true;
        }

    }

    public bool Error(string at, int error, params string[] args)
    {
        //Debug.Log(string.Format("Error {0}: [{1}] at [2}", error, AiScriptParser.getAiScriptError(error), at));
        Debug.Log("Error: " + error + " [" + AiScriptParser.getAiScriptError(error) + "] at [" + at + "]");
        //return true;
        return false;
    }
    public virtual void setExecuteContext(string context) {
        if (myLog!=null)
        {
            myContext = context;
            myContextPrinted = false;
        }
    }
    /// <summary>
    /// РАспечатать сообщение виртуальной машины
    /// </summary>
    /// <param name="type"></param>
    /// <param name="fmt"></param>
    /// <param name="args"></param>
    public void printVMessage(int type, string fmt, object[] args)
    {
        //Debug.LogFormat("printVMessage {0} args {1}", fmt, args.Length);
        //foreach (string arg in args)
        //{
        //    Debug.Log(string.Format(fmt, arg));
        //}
        if (myLog != null)
        {
            if (myContextPrinted == false)
                showContext();
            //char buffer[1024];
            //buffer[0] = ' ';
            //buffer[1] = ' ';
            //vsprintf(buffer + 2, fmt, lst);
            myLog.Message(string.Format(fmt, args));
            printTLBMessage(type, string.Format(fmt, args));
        }
    }
    /// <summary>
    /// Распечатать таблицу
    /// </summary>
    /// <param name="type"></param>
    /// <param name="buffer"></param>
    public void printTLBMessage(int type, string buffer)
    {
        if (myLogLtb != null)
        {
            float flt = myTimer.getTime();
            int tmp = (int)flt;
            //myLogLtb.Message("%02d:%02d.%02d\t%s\t%d\t%s", (tmp % 3600) / 60, (tmp % 3600) % 60, getMilliFromSec(flt), cstr(myContext), type, buffer);
            //myLogLtb.Message("{0,2:D}:{1,2:D}.{2,2:D}\t{3}\t{4:D}\t{5}", (tmp % 3600) / 60, (tmp % 3600) % 60, getMilliFromSec(flt), myContext, type, buffer);
            myLogLtb.Message("{0,2:D}:{1,2:D}.{2,2:D}\t{3}\t{4}\t{5}", (tmp % 3600) / 60, (tmp % 3600) % 60, getMilliFromSec(flt), myContext, MessageCodes.IntToString(type), buffer);
        }
    }

    private int getMilliFromSec(float t)
    {
        float ti = (t - Mathf.Floor(t)) * 100;
        if (ti < 0)
            ti = 0;
        return (int)ti;
    }

    ILog openLog(string name, string ext = "", bool open_window = true)
    {
        //Debug.Log("Opening log: " + name + "." + ext);
        ILog log = LogFactory.CreateLOG(name);
        if (log != null)
        {
            if (ext != "" && ext != null)
                log.setExtension(ext);
            log.OpenLogFile();
            if (open_window)
                log.OpenLogWindow();
        }
        return log;

    }
    void closeLog(ILog log)
    {
        if (log != null)
        {
            log.FlushLogFile();
            log.CloseLogWindow();
        }
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }

    //public void printMessage(int type, string fmt, params object[] va_list)
    //{
    //    string res = "Type: " + MessageCodes.IntToString(type);
    //    res += string.Format("\n\t" + fmt, va_list);

    //    //Debug.Log("Type: " + MessageCodes.IntToString(type));
    //    //Debug.Log(string.Format("\t" + fmt, va_list));
    //    //Debug.Log(string.Format("Type: {0}\n"))
    //    //Debug.Log(res);
    //    printVMessage(type, fmt, va_list);
    //}

    ILog myLog;
    ILog myLogLtb;
    string myText;
    string myExecutor;
    string myContext;
    bool myContextPrinted = false;
    ITimeService myTimer;
}