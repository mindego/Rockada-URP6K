using System.Text;
using DWORD = System.UInt32;

public partial class StdMissionAi
{
    DWORD FindClientLog(DWORD id)
    {
        for (int i = 0; i < mClientLogs.Count; ++i)
            if (mClientLogs[i].mID == id)
                return (DWORD)i;
        return Constants.THANDLE_INVALID;
    }

    public virtual void say(uint what, iClient cl=null)
    {
        RadioMessage Info = new RadioMessage();
        Info.Code = what;
        Radio(cl, Info, Constants.THANDLE_INVALID, "", "");
    }

    void Radio(iClient cl, RadioMessage info, DWORD team, string format, params string[] v)
    {
        if (true == IsServerShutdowned()) return;
        if (false == IsNotifyEnable()) return;
        string to_print;
        if (v != null)
        {
            //Vsprintf(mNotifyBuffer, format, v);
            //to_print = mNotifyBuffer;
            Helpers.Vsprintf(out to_print, format, v);
        }
        else
            to_print = format;
        fillRandomPhrase(ref info);
        if (cl != null)
            cl.SayMessage(to_print, info);
        else
        {
            //for (MissionClient* mcl = mlClients.Head(); mcl; mcl = mcl->Next())
            foreach (MissionClient mcl in mlClients)
                if (team == Constants.THANDLE_INVALID || mcl.Team() == team)
                    mcl.Client().SayMessage(to_print, info);
        }
    }

    bool RemoveClientLog(MissionClient mcl, MissionClient admin)
    {
        DWORD log_num = FindClientLog((uint)mcl.ID());
        if (log_num != Constants.THANDLE_INVALID)
        {
            mcl.Client().SetLog(null);
            //mClientLogs[(int)log_num] = null;
            //mClientLogs.Remove(log_num, log_num);
            mClientLogs.RemoveAt((int)log_num);
            return true;
        }
        return false;
    }

    bool PauseServer()
    {
        DWORD cur_p = (uint)iserver.GetTimeScale();
        if (cur_p > 0)
            iserver.SetTimeScale(0);
        else
            iserver.SetTimeScale(1);
        return (cur_p > 0);
    }

    bool OpenClientLog(MissionClient mcl, MissionClient admin)
    {
        DWORD log_num = FindClientLog((uint)mcl.ID());
        if (log_num == Constants.THANDLE_INVALID)
        {
            string buffer;
            buffer=string.Format("Client {0}",mcl.ID());
            LOG log = (LOG) LogFactory.CreateLOG(buffer);
            mClientLogs.Add(new LogHolder((uint)mcl.ID(), log));
            log.OpenLogFile(true);
            log.OpenLogWindow();
            log.Message("Client: {0} \"{1}\"", mcl.ID().ToString(), mcl.Client().GetPlayerName());
            mcl.Client().SetLog(log);
        }
        NotifyClient(admin.Client(), ServerMessages.sAdminLogged, mcl.ID().ToString());
        return true;
    }

    public iContact GetContactByIndex(DWORD grp_id, DWORD un_id)
    {
        foreach (ContactInfo info in mlContacts)
        {
            if (info.mpData.Number + 1 == un_id && info.mpGroupCont.mpGroupData.ID == grp_id) return info.mpAi.GetContact();
        }
        return null;
    }
}

public static class Helpers
{
    public static void Vsprintf(out string output, string format, string[] data)
    {
        StringBuilder SB = new StringBuilder();
        foreach (string myString in data)
        {
            SB.Append(string.Format(format, myString));
        }
        output = SB.ToString();
    }
}

