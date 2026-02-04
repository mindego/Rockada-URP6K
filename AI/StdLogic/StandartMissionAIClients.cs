using System.Collections.Generic;
using UnityEngine;
using DWORD = System.UInt32;

public abstract partial class StdMissionAi
{

    protected void ScoreMessage(string title, string frm, params object[] args)
    {
        if (myErrorLog == null) return;
        //char[] buffer = new char[256];
        string buffer = string.Format("{0}{1}{2}", title, title != null ? " : " : Parsing.sAiEmpty, frm);
        myScoreLog.VMessage(buffer, args);
        //TODO! Реализовать вывод 
        //wsprintf(buffer, "%s%s%s", title, title[0] ? " : " : Parsing.sAiEmpty, frm);

        //va_list v;
        //va_start(v, frm);
        //myScoreLog.VMessage(buffer, v);
    }

    DWORD GetCurrentPlayers()
    {
        int second = mlClients.Count;
        if (second != 0 && mlClients.First.Value.Client().IsHidden() == true) second--;

        //if (second && mlClients.Head().Client().IsHidden() == true)
        //    second--;
        return (uint)second;
    }

    void GetPlayersInfo(out int first, out int second)
    {
        first = (int)mMaxPlayers;
        second = (int)GetCurrentPlayers();
    }

    void GetPlayersInfo(iAboutPlayer ab)
    {
        //for (MissionClient mcl = mlClients.Head(); mcl!=null; mcl = mcl.Next())
        //    if (mcl.Client().IsHidden() == false)
        //        ab.addPlayer(mcl.Name(), -1, -1);
        foreach (MissionClient mcl in mlClients)
        {
            if (mcl.Client().IsHidden() == false) ab.addPlayer(mcl.Name(), -1, -1);
        }
    }

    bool isCanRespawn()
    {
        return GetCurrentPlayers() < GetMaxPlayers();
    }

    public void setCamera(DWORD mode, Vector3 org, float angle1, float angle2, float angle3, string group, DWORD index)
    {
        DWORD hs = Hasher.HshString(group);

        GroupAiCont cont = (Constants.THANDLE_INVALID == hs) ? null : FindGroupCont(hs);

        ContactInfo info = cont != null ? FindContact(cont.ai, index) : null;
        DWORD handle = info != null ? info.mHandle : Constants.THANDLE_INVALID;

        mLastCamera.mMode = mode;
        mLastCamera.mHandle = handle;
        mLastCamera.mOrg = org;
        mLastCamera.mHeading = Storm.Math.GRD2RD(angle1);
        mLastCamera.mPitch = Storm.Math.GRD2RD(angle2);
        mLastCamera.mRoll = Storm.Math.GRD2RD(angle3);
        //for (MissionClient mcl=mlClients.Head();mcl;mcl=mcl->Next())
        foreach (MissionClient mcl in mlClients)
            mcl.SetCamera(mLastCamera);
    }

    protected void updateMissionStatus()
    {
        LinkedListNode<MissionClient> mcl = mlClients.First;
        for (; mcl != null; mcl = mcl.Next)
            myClientInfo.setMissionStatus(mcl.Value.Client(), (uint)getMissionStatus());
    }

    public void ServerShutdown()
    {
        myVM.reset();
        server_shutdown = true;
    }

    public void StartListName(iClient pClient, string name)
    {
        NotifyClient(pClient, "{0}:", name);
        NotifyClient(pClient, ServerMessages.sSrvSeparator);
    }

    protected virtual void setMissionStatus(int status)
    {
        myMissionStatus = status;
        updateMissionStatus();
    }

    public bool SetClientName(MissionClient cl, string name, bool eliminate_colors)
    {
        //char[] local_buffer = new char[256];
        string local_buffer;
        if (eliminate_colors)
            EliminateColors(out local_buffer, name);
        else
            //StrCpy(local_buffer, name);
            local_buffer = name;
        ProcessUniqueName(cl, ref local_buffer);
        if (name != cl.Name())
        {
            cl.Client().SetPlayerName(name);
            IBaseUnitAi ai = cl.Client().GetAI();
            if (ai != null)
            {
                iContact cnt = ai.GetContact();
                if (cnt != null)
                    cnt.SetName(name, false);
            }
        }
        return true;
    }

    void EliminateColors(out string buffer, string name)
    {
        //int n = 0, i = 0;
        //while (name[i] != 0)
        //{
        //    if (name[i] == '^')
        //        i++;
        //    else
        //        buffer[n++] = name[i];
        //    i++;
        //}
        //buffer[n] = 0;
        //удалять цвета не будем
        buffer = name;

    }

    protected void setClientStatus(MissionClient mcl, int status)
    {
        myClientInfo.setClientStatus(mcl.Client(), (uint)mcl.setStatus(status));
    }

    public void sendPlayerMessage(DWORD team, iClient client, string arg1)
    {
        RadioMessage Info = new RadioMessage();
        Info.CallerCallsign = client.GetPlayerName();
        Info.SetConsole(true);
        Info.SetLocalize(false);
        Info.setTeamFlag((int)(team != Constants.THANDLE_INVALID ? RadioMessage.RMF_TEAM : RadioMessage.RMF_ALL));
        Radio(null, Info, team, arg1, "");
    }

    public bool ProcessUniqueName(MissionClient mcl, ref string buffer)
    {
        bool unique_name = true;
        //buffer[PLAYER_NAME_LENGTH] = 0;
        for (LinkedListNode<MissionClient> cl = mlClients.First; cl != null; cl = cl.Next)
            if (cl.Value != mcl && (cl.Value.Name() == buffer))
            {
                //wsprintf(buffer, "Player #%d", mcl->ID());
                buffer = string.Format("Player #{0}", mcl.ID());
                //Debug.Log(string.Format("Player #{0} {1}", buffer, mcl.ID()));
                unique_name = false;
                break;
            }
        return unique_name;
    }

    public void OnRemoteTrigger(iClient pClient, DWORD TriggerCode, bool IsOn)
    {
        MissionClient mcl = FindClient(pClient);
        if (mcl == null)
            return;
    }

    public void OnRemoteCommand(iClient pClient, DWORD CommandCode, string pArg1, string pArg2)
    {
        //Сетевая часть не планируется
    }
}
/// <summary>
/// Реализация интерфейса учёта количества ссылок на объект
/// </summary>
public class RefMem : IRefMem
{
    public void AddRef()
    {
        //throw new System.NotImplementedException();
        return;
    }

    public int RefCount()
    {
        return 1;
        //throw new System.NotImplementedException();
    }

    public int Release()
    {
        return 1;
        throw new System.NotImplementedException();
    }
}

