using UnityEngine;

public class StormLog
{
    public enum logPriority
    {
        CRITICAL,
        ERROR,
        NORMAL,
        WARNING,
        DEBUG
    }

    public static logPriority defaultPriority = logPriority.NORMAL;
    public static void LogMessage(string message,logPriority priority = logPriority.DEBUG)
    {
        if (priority > defaultPriority) return;
        Debug.Log(message);
    }
}

