#define _DEBUG
using System;
using UnityEngine;

public class StdTeamplayAi : StdMissionAi
{
    new public const uint ID = 0x5E2CF0A7;

    public override void AdminBan(MissionClient admin, string arg1, string arg2)
    {
        throw new NotImplementedException();
    }

    public override void AdminKick(MissionClient admin, string arg1, string arg2)
    {
        throw new NotImplementedException();
    }

    public override void AdminLog(MissionClient admin, string arg1, string arg2)
    {
        throw new NotImplementedException();
    }

    public override void AdminMute(MissionClient admin, string arg1, string arg2, bool mute)
    {
        throw new NotImplementedException();
    }

    public override void AdminSaveBanList(MissionClient admin, string arg1, string arg2)
    {
        throw new NotImplementedException();
    }

    public override void AdminUnBan(MissionClient admin, string arg1, string arg2)
    {
        throw new NotImplementedException();
    }

    public override bool canDrawEnemy()
    {
        throw new NotImplementedException();
    }

    public override bool canUseOtherSideData()
    {
        throw new NotImplementedException();
    }

    public override void CheckUniqueCallsigns()
    {
        throw new NotImplementedException();
    }

    public override void ClearUnitDataHandles(iCheckAppear ca)
    {
        throw new NotImplementedException();
    }

    public override void ClientInfoChanged(MissionClient mcl)
    {
        throw new NotImplementedException();
    }

    public override MissionClient Connecting(iClient client)
    {
        throw new NotImplementedException();
    }

    public override void Console(iClient cl, string format, params string[] v)
    {
        throw new NotImplementedException();
    }

    public override uint CountClientsAround(Vector3 org, float radius)
    {
        throw new NotImplementedException();
    }

    public override IBaseUnitAi CreateUnitAi(UNIT_DATA ud, int side, iContact hangar, IGroupAi grp)
    {
        throw new NotImplementedException();
    }

    public override void DeleteContact(iContact cnr, bool explode)
    {
        throw new NotImplementedException();
    }

    public override void DeleteGroup(IGroupAi grp)
    {
        throw new NotImplementedException();
    }


    public override MissionClient Disconnecting(iClient client, bool dropped)
    {
        throw new NotImplementedException();
    }

    public override void ExecuteBatch(string batch_name)
    {
        throw new NotImplementedException();
    }

    public override void ExecuteScript()
    {
        throw new NotImplementedException();
    }

    public override bool FillFragRows(ushort rows_count, ushort[] rows)
    {
        throw new NotImplementedException();
    }

    public override ContactInfo FindContact(uint hndl)
    {
        throw new NotImplementedException();
    }

    public override ContactInfo FindContact(UNIT_DATA data)
    {
        throw new NotImplementedException();
    }

    public override ContactInfo FindContact(IGroupAi gai, uint index)
    {
        throw new NotImplementedException();
    }

    public override ContactInfo FindContact(IBaseUnitAi bua)
    {
        throw new NotImplementedException();
    }

    public override GroupAiCont FindGroupCont(uint id)
    {
        throw new NotImplementedException();
    }

    public override void FinishListName(iClient pClient, uint d)
    {
        throw new NotImplementedException();
    }

    public override uint GetClientsCountForAppear(uint flag)
    {
        throw new NotImplementedException();
    }

    public override bool GetContactInfo(iContact cnt, out uint grp_id, out uint un_index, out uint side)
    {
        throw new NotImplementedException();
    }

    public override GROUP_DATA GetExistedGroupData(uint grp_id)
    {
        throw new NotImplementedException();
    }

    public override ushort GetFragRowsCount()
    {
        throw new NotImplementedException();
    }


    public override MARKER_DATA GetMarkerData(uint mrk_id)
    {
        throw new NotImplementedException();
    }

    public override int GetMenuItems(iClient pClient, uint id, AiMenuItem[] ami, int max_count, int page_index)
    {
        throw new NotImplementedException();
    }



    public override bool GetMessageMode(uint side)
    {
        throw new NotImplementedException();
    }

    public override string GetMissionDescription()
    {
        throw new NotImplementedException();
    }

    public override string GetMissionType()
    {
        throw new NotImplementedException();
    }

    public override uint GetMissionVersion()
    {
        throw new NotImplementedException();
    }

    public override void GetPlayersInfo(out iAboutPlayer info)
    {
        throw new NotImplementedException();
    }


    public override string GetTeamName(uint team_code)
    {
        throw new NotImplementedException();
    }

    public override IVmFactory getTopVmFactory()
    {
        throw new NotImplementedException();
    }

    public override uint GetUniqueGroupID()
    {
        throw new NotImplementedException();
    }

    public override string IsPlayer(UNIT_DATA ud, ref string real_player_name)
    {
        throw new NotImplementedException();
    }

    public override bool IsRepairEnabled()
    {
        throw new NotImplementedException();
    }

    public override bool IsRespawnAfterRepair()
    {
        throw new NotImplementedException();
    }


    public override void NotifyClient(iClient cl, string format, params string[] args)
    {
        throw new NotImplementedException();
    }

    public override void NotifyClients(string format, params string[] args)
    {
        throw new NotImplementedException();
    }

    public override void NotifyHost(string format, params string[] args)
    {
        throw new NotImplementedException();
    }

    public override void OnAddDamage(uint VictimHandle, uint GadHandle, uint WeaponCode, float Damage, bool IsFinal)
    {
        throw new NotImplementedException();
    }

    public override void OnChangeAdminStatus(MissionClient mcl, bool admin)
    {
        throw new NotImplementedException();
    }

    public override void OnCommand(int i, string c, string s)
    {
        throw new NotImplementedException();
    }

    public override bool OnConnect(iClient cl)
    {
        throw new NotImplementedException();
    }

    public override ContactInfo OnCreateContact(IBaseUnitAi ai, iContact cnt, UNIT_DATA ud, IGroupAi ga)
    {
        throw new NotImplementedException();
    }

    public override void OnDeleteContactHandle(uint handle, bool landed_or_repaired)
    {
        throw new NotImplementedException();
    }

    public override void OnDeleteGroup(GroupAiCont gac)
    {
        throw new NotImplementedException();
    }

    public override void OnDisconnect(iClient cl, bool dropped)
    {
        throw new NotImplementedException();
    }

    public override bool OnMutePlayer(MissionClient mcl, bool mute, bool connecting = false)
    {
        throw new NotImplementedException();
    }

    public override void ProcessContactDeath(ContactInfo info, bool landed_or_repaired)
    {
        throw new NotImplementedException();
    }
}
