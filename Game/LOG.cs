using System;
using System.IO;
using UnityEngine;

public class LOG : ILog
{
    //public static System.IO.StreamWriter logfile;
    public System.IO.StreamWriter logfile;
    public string myFileName = "Wedge";

    public string myExtension = "log";
    //STUB!
    public void FlushLogFile()
    {
        logfile.Flush();
    }

    public LOG()
    {
        if (logfile == null) OpenLogFile();
    }

    public LOG(string name)
    {
        Debug.Log("Creating new LOG: " + name);
        myFileName = name;
        //if (logfile == null) OpenLogFile();
    }
    public void Message(string message)
    {
        if (logfile==null) OpenLogFile();
        if (logfile == null) return;
        //Debug.Log(message);
        logfile.WriteLine(message);
    }

    public void LogMessage(string message)
    {
        //logfile.WriteLine(message);
        Message(message);
    }

    internal void setAsStdOut()
    {
        //if (myTextFile.get())
        //   myTextFile.get()->setAsStdout();
    }

    private void OpenLogFile()
    {
        string fname = myFileName + "." + myExtension;
        uint hash = Hasher.HashString(fname);
        if (logfile != null) logfile.Close();
        logfile = new System.IO.StreamWriter(hash.ToString("X8") + "-" + myFileName + "." + myExtension);
        logfile.AutoFlush = true;
        logfile.WriteLine(System.DateTime.Now);
    }

    public void Append(string message)
    {
        LogMessage(message);
    }
    public void AppendFailed()
    {
        Append("FAILED");
    }
    public void AppendSucceeded()
    {
        Append("OK");
    }

    public void AddException(Exception e)
    {
        Append(e.StackTrace);
        Append(e.Message);
        throw e;
    }

    public void VMessage(string format, params string[] marker)
    {
        throw new NotImplementedException();
    }

    ~LOG()
    {
        logfile.Close();
    }

    public static ILog createLOG(string name, int version = ILog.LOG_VERSION)
    {
        return (version == ILog.LOG_VERSION) ? new LOG(name) : null;
    }

    public virtual void CloseLogWindow()
    {
        if (logfile!=null) logfile.Close();
        //консоль пока не реализована
        //    if (myConsole!=null)
        //    myLog.subRenderer(myConsole);
        ////SafeDelete(myConsole);
        //myConsole = null;


    }

    public void setExtension(string ext)
    {
        myExtension = ext;
    }

    public void OpenLogFile(bool new_file = true, int max_size = 0)
    {
        string fname = myFileName + "." + myExtension;
        uint hash = Hasher.HashString(fname);
        if (logfile != null) logfile.Close();
        logfile = new System.IO.StreamWriter(hash.ToString("X8") + "-" + myFileName + "." + myExtension, !new_file);
        //if (logfile == null) logfile = new System.IO.StreamWriter(hash.ToString("X8") + "-" + myFileName + "." + myExtension,FileMode.Truncate,max_size);
        logfile.AutoFlush = true;
        logfile.WriteLine(System.DateTime.Now);

        //if (myTextFile.get() == 0)
        //{
        //    myIterator = std::auto_ptr<FileNameIterator>(new FileNameIterator(myFileName, myExtension));
        //    LogFile* text_file = new LogFile(myIterator.get(), new_file);
        //    text_file->setMaxSize(max_size);
        //    myTextFile = std::auto_ptr<LogFile>(text_file);
        //    myLogClientFile.setFile(myTextFile.get());
        //    myLog.addRenderer(myLogClientFile);
        //}

    }

    public void OpenLogWindow(string title = "")
    {
        //if (myConsole == 0)
        //{
        //    myConsole = new LogClientConsole(myMaxLineCount, title ? title : myFileName);
        //    myLog.addRenderer(*myConsole);
        //}
    }

    public void Message(params string[] messages)
    {
        //foreach (string message in messages)
        //{
        //    Message(message);
        //}
        if (messages.Length == 0) return;
        string format = messages[0];
        string[] args = new string[messages.Length - 1];
        Array.Copy(messages, 1, args, 0, messages.Length - 1);
        Message(string.Format(format, args));
    }

    //public void Message(string format, params string[] strings)
    //{
    //    Message(string.Format(format, strings));
    //}

    //public void Message(string format, params float[] digits)
    //{
    //    Debug.Log(format);
    //    Debug.Log(digits.Length);
    //    Message(string.Format(format, digits));
    //    string.Format(format,
    //}

    //public void CriticalMessage(string format, params string[] args)
    //{
    //    Message("CRITICAL: " + format, args);
    //}

    public void Message(string format, params object[] myparams)
    {
        Message(string.Format(format, myparams));
    }

    public void Append(string format, params object[] args)
    {
        Debug.Log(string.Format(format, args));
    }

    public void CriticalMessage(string format, params object[] agrs)
    {
        throw new NotImplementedException();
    }

    public void SetIdent(int ident)
    {
        throw new NotImplementedException();
    }

    public int GetIdent()
    {
        throw new NotImplementedException();
    }

    public void IncIdent()
    {
        //TODO Логи что-то должны делать с идентификатором
        //throw new NotImplementedException();
    }

    public void DecIdent()
    {
        //throw new NotImplementedException();
    }

    public void VMessage(string a, params object[] va_list)
    {
        Debug.LogFormat(a, va_list);
    }

    public void VCriticalMessage(string a, params object[] va_list)
    {
        throw new NotImplementedException();
    }

    public void VAppend(string a, params object[] va_list)
    {
        throw new NotImplementedException();
    }

    public void ReOpenLogFile()
    {
        throw new NotImplementedException();
    }

    public void SetLogFileMaxSize(int max_size)
    {
        throw new NotImplementedException();
    }

    public bool IsWindowOpen()
    {
        throw new NotImplementedException();
    }

    public void setIdentSize(int size)
    {
        throw new NotImplementedException();
    }

    public void setConsoleLineCount(int size)
    {
        throw new NotImplementedException();
    }

    void ILog.setAsStdOut()
    {
        throw new NotImplementedException();
    }

    public void AddRef()
    {
        throw new NotImplementedException();
    }

    public int RefCount()
    {
        throw new NotImplementedException();
    }

    public int Release()
    {
        return 0;
    }
}


