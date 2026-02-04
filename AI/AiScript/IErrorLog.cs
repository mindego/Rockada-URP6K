using System;
using UnityEngine;

public interface IErrorLog : IObject
{
    new public const uint ID = 0xA5C080F0;
    public bool Error(string at, int error, params string[] args);
    public  void setExecuteContext(string context);
    public  void setSource(string name, string text);
    
    public void printVMessage(int type, string fmt, object[] args);

    void printMessage(int type, string fmt, params object[] va_list)
    {
        //string res = "Type: " + MessageCodes.IntToString(type);
        //res += string.Format("\n\t" + fmt, va_list);

        //Debug.Log("Type: " + MessageCodes.IntToString(type));
        //Debug.Log(string.Format("\t" + fmt, va_list));
        //Debug.Log(string.Format("Type: {0}\n"))
        //Debug.Log(res);
        printVMessage(type, fmt, va_list);
    }
}