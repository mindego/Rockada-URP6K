using DWORD = System.UInt32;

public static class GameStatuses
{
    public enum ClientStatuses { CS_Connected, CS_Escaped, CS_Landed, CS_Killed, CS_Leaved };
    public enum MissionStatuses { MS_Normal, MS_Complete, MS_Failed };

    private static string[] client_statuses = new string[] { "connected", "escaped", "landed", "killed", "leaved" };
    private static string[] mission_statuses = new string[] {"normal", "completed", "failed"};
        
    public static string convertClientStatus(int status)
    {
        return client_statuses[status];
    }

    public static string convertMissionStatus(int status)
    {
        return mission_statuses[status];
    }

    public static void setLocalClientStatus(ref UniVarContainer local, DWORD status)
    {
        //dprintf("setLocalClientStatus=%s\n",convertClientStatus(status));
        //local.setString("ClientStatus", convertClientStatus((int)status));
    }

    public static  void setLocalMissionStatus(ref UniVarContainer local, DWORD status)
    {
        //dprintf("setLocalMissionStatus=%s\n",convertMissionStatus(status));
        //local.setString("MissionStatus", convertMissionStatus((int)status));
    }

}