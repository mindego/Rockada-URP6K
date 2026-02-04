//using AI_STATE = System.Boolean;
using System;

public static class MessageCodes
{
    public const int AUTO_UNIT = 1;
    public const int AUTO_GROUP = 2;
    public const int CMD_GROUP = 3;
    public const int CMD_MSN = 4;
    public const int THREAD_CREATE_RELEASE = 5;
    public const int THREAD_START_FINISH = 6;
    public const int CMD_NOTIFY = 7;
    public const int MSG_ERROR = 8;
    public const int MSG_UNKNOWN = 0;

    internal static string IntToString(int type)
    {
        switch (type) {
            case AUTO_UNIT: return "AUTO_UNIT";
            case AUTO_GROUP: return "AUTO_GROUP";
            case CMD_GROUP: return "CMD_GROUP";
            case CMD_MSN: return "CMD_MSN";
            case THREAD_CREATE_RELEASE: return "THREAD_CREATE_RELEASE";
            case THREAD_START_FINISH: return "THREAD_START_FINISH";
            case CMD_NOTIFY: return "CMD_NOTIFY";
            case MSG_ERROR: return "MSG_ERROR";
            case MSG_UNKNOWN: return "MSG_UNKNOWN";
        }
        return "INCORRECT CODE";
    }
}
