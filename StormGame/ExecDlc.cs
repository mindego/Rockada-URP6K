using System.Collections;
using UnityEngine;

//template < cstr rname(cstr), class Cmd>
//void execDlc(Cmd * cmd, cstr name1, cstr name2, bool createNew) {
//    char buffer[256];
//    wsprintf(buffer, "%s%s%s.dlc", name1, name2 ? "_" : "", name2 ? name2 : "");
//    if (!cmd->ExecFile(buffer) && createNew)
//        File(rname(buffer), true, CREATE_NEW);
//}

public static class ExecDlc
{
    public static void execDlc(CommandsApi Cmd,string name1, string name2, bool createNew )
    {
        string filename = name1 + (name2 != null ? "_" : "") + (name2 != null ? name2 : "") + ".dlc";
        Debug.Log(string.Format("Processing DLC file: {0} using CommandsApi {1} (new file? {2})",filename,Cmd,createNew.ToString()));
        Cmd.ExecFile(filename);
    }
}

public static class WaypointSuggester {
    public static int getWaypoint(IBaseCraftController cc, ref Vector3 org)
    {
        if (cc.isShowedWaypoint())
        {
            org = cc.GetAutopilotOrg();
            return 2;
        }
        iContact ldr = cc.GetAutopiloLeader();
        if (ldr != null)
        {
            org = ldr.GetOrg();
            return 1;
        }

        else if (!cc.IsPaused())
        {

            org = cc.GetAutopilotOrg();
            return 2;
        }
        return 0;
    }
}
