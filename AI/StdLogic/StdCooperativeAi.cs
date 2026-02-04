using System.Collections.Generic;
using UnityEngine;
using static DataHasherDefines;
using static RoadElementState;
using DWORD = System.UInt32;
using crc32 = System.UInt32;
using WORD = System.UInt16;
using static CampaignDefines;
using static MissionSideDefines;
using static ServerMessages;
using static StdCoopState;
using static enumGlobalTriggers;
using static GameStatuses.MissionStatuses;
using static GameStatuses.ClientStatuses;
using static StdCooperativeAiMessages;
using static ObjectiveState;
using static StdCooperativeAiDefines;
using static AICommon;

public sealed class StdCooperativeAiMessages
{
    public const uint ENG_BAY_CLOSED = 0x01613849;   // "mc_MissionStart"

    public const uint COUNTDOWN_1 = 0xA0EA8F14; //"mc_Countdown1"
    public const uint COUNTDOWN_2 = 0x39E3DEAE; //"mc_Countdown2"
    public const uint COUNTDOWN_3 = 0x4EE4EE38; //"mc_Countdown3"
    public const uint COUNTDOWN_4 = 0xD0807B9B; //"mc_Countdown4"
    public const uint COUNTDOWN_5 = 0xA7874B0D; //"mc_Countdown5"
    public const uint COUNTDOWN_10 = 0x3CA30E5E;//"mc_Countdown10"
    public const uint COUNTDOWN_20 = 0x178E5D9D; //"mc_Countdown20"

    public const uint NOTIFY_MISSION_SUCCESS = 0x807A3E78; // "mc_MissionSuccess"
    public const uint NOTIFY_MISSION_FAILED = 0x3E103312; // "mc_MissionFailed"

    //public const int AWARDS_COUNT = 18;
    //public const int AWARDSNIPER3 = 0;
    //public const int AWARDSNIPER2 = 1;
    //public const int AWARDSNIPER1 = 2;
    //public const int AWARDFALCON3 = 3;
    //public const int AWARDFALCON2 = 4;
    //public const int AWARDFALCON1 = 5;
    //public const int AWARDBOOT3 = 6;
    //public const int AWARDBOOT2 = 7;
    //public const int AWARDBOOT1 = 8;
    //public const int DIAMONDBOOT3 = 9;
    //public const int DIAMONDBOOT2 = 10;
    //public const int DIAMONDBOOT1 = 11;
    //public const int BESTFIGHTER3 = 12;
    //public const int BESTFIGHTER2 = 13;
    //public const int BESTFIGHTER1 = 14;
    //public const int HANDICAPFIGHTER = 15;
    //public const int STARMONSTER = 16;
    //public const int CHIPANDDAIL = 17;
}
public partial class StdCooperativeAi : StdMissionAi
{
    internal void setWeaponAccess(string name, ICoopMission.TechAccess access)
    {
        myWeaponsAccess.setTrigger(name, (int)access, true);
    }

    bool IsStatusCompleted(DWORD status)
    {
        return (status == (uint)MS_Complete);
    }
    bool IsPrimaryCompleted()
    {
        bool primary_completed = IsStatusCompleted((uint)getMissionStatus());
        for (ObjectiveHolder hld = mlObjectives.Head(); hld != null; hld = hld.Next())
        {
            if (hld.isInProcess())
                hld.setState(osFailed);
            if (hld.IsPrimary())
                primary_completed &= (hld.isCompleted());
        }
        return primary_completed;
    }


    void sayWithForget(uint what) { if (!mForgetFlag) say(what); }
    protected override void setMissionStatus(int succ)
    {
        base.setMissionStatus(succ);
        switch (succ)
        {
            case (int)MS_Failed:
                AddMessage("mc_MissionFailed");
                sayWithForget(NOTIFY_MISSION_FAILED);
                break;
            case (int)MS_Complete:
                AddMessage("mc_MissionSuccess");
                sayWithForget(IsPrimaryCompleted() ? NOTIFY_MISSION_SUCCESS : NOTIFY_MISSION_FAILED);
                break;
        }
    }
    void AddMessage(string title)
    {
        iUnifiedVariableInt var = mpOthers.createInt(title);
        if (var != null)
            var.SetValue(2);
    }

    const string sNoObjective1 = "can`t create or get objective \"{0}\"";
    const string sNoObjective2 = "objective \"{0}\" does`t exists";
    public void AddObjective(string name, bool primary)
    {
        DWORD hs = Hasher.HshString(name);
        ObjectiveHolder obj = FindObjective(hs);     // find objective
        iUnifiedVariableInt var = obj != null ? obj.Objective() : null;
        if (var == null)
        {           // this is new objective
            if (SpawnObjective(hs, name, primary) == null)
            {
                if (IsLogged(DEBUG_MEDIUM))
                {
                    AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, sNoObjective1, name);
                }
            }
        }
        else if (AICommon.IsLogged(DEBUG_MEDIUM))
        {
            AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, "objective \"{0}\" already exists", name);
        }
    }

    ObjectiveHolder SpawnObjective(DWORD code, string name, bool primary)
    {

        iUnifiedVariableContainer cont = primary ? mpPrimaries : mpSecondaries;
        if (cont == null) return null;
        iUnifiedVariableInt var = cont.createInt(name);
        return var != null ? mlObjectives.AddToTail(new ObjectiveHolder(code, primary, var)) : null;
    }

    public void SetObjective(string name, DWORD success, DWORD primary)
    {
        DWORD hs = Hasher.HshString(name);
        ObjectiveHolder var = FindObjective(hs);     // find objective
        if (var != null)
        {
            if (primary != Constants.THANDLE_INVALID && (var.IsPrimary() ? 1 : 0) != primary)
            {    // надо менять primary
                ObjectiveHolder var2 = SpawnObjective(var.GetName(), name, (primary == 1));
                DeleteObjective(var);
                var = var2;
            }
            if (Constants.THANDLE_INVALID != success)
            { // надо менять success
                ObjectiveState prev_value = var.getState();
                ObjectiveState now_value = success != 0 ? osCompleted : osFailed;
                if (now_value != prev_value)
                {
                    var.setState(now_value);
                    int score = (var.IsPrimary()) ? 1000 : 500;
                    switch ((int)prev_value)
                    {
                        case OBJECTIVE_SUCCESS: score *= -1; break;
                        case OBJECTIVE_IN_PROGRESS: if ((int)now_value != OBJECTIVE_SUCCESS) score = 0; break;
                    }
                    if (IsLogged(DEBUG_MEDIUM))
                    {
                        ScoreMessage("AddScore", "\\objective\\{0}\\score\\{1}", var.IsPrimary() ? "primary" : "secondary", score);
                    }
                    AddScore(score, ref mObjectiveCount, false);
                }
            }
        }
        else if (IsLogged(DEBUG_MEDIUM))
        {
            AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, sNoObjective2, name);
        }
    }
    public void RemoveObjective(string name)
    {
        DWORD hs = Hasher.HshString(name);
        ObjectiveHolder obj = FindObjective(hs);     // find objective
        if (obj != null)
            DeleteObjective(obj);
        else if (IsLogged(DEBUG_MEDIUM))
        {
            AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, sNoObjective2, name);
        }

    }

    ObjectiveHolder FindObjective(DWORD hs)
    {
        for (ObjectiveHolder hld = mlObjectives.Head(); hld != null; hld = hld.Next())
            if (hld.GetName() == hs) return hld;
        return null;
    }
    void AddScore(int score, ref int cont, bool from_script)
    {
        if (IsCountStatistics() || true == from_script)
        {
            //if (cont!=0) cont += (float)score;
            //Здесь с нулём сравнивается указатель,  
            //if (cont != 0) cont += score;
            cont += score;
            mTotalScore += score;
        }
    }

    void DeleteObjective(ObjectiveHolder hld)
    {
        hld.Destroy();
        mlObjectives.Sub(hld).Destroy();
    }

    internal void IncScore(int score)
    {
        AddScore(score, ref mOtherCount, true);
    }

    public bool IsAddingStatistics() { return mAddStatistics == 1; }
    public bool IsCountStatistics() { return mCountStatistics; }

    public void SetAddStatisticsMode(DWORD mode) { mAddStatistics = mode; }
    public void SetCountStatisticsMode(bool mode) { mCountStatistics = mode; }

    internal void setPlayerPosition(int pos)
    {
        myPlayerPosition = pos;
    }

    const string sBadGroup = "can't find group \"{0}\" in {1}";
    internal void SetPlayable(string name)
    {
        DWORD hs = Hasher.HshString(name);
        GroupAiCont cont = (Constants.THANDLE_INVALID == hs) ? null : FindGroupCont(hs);
        if (cont != null)
            myPlayableGroup = hs;
        else if (IsLogged(DEBUG_MEDIUM))
        {
            AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, sBadGroup, name, "SetPlayable");
        }
    }

    internal void enableEngbay(int enable)
    {
        myEngBayEnabled = enable != 0;
    }

    internal void ForgetPlayers(bool forget)
    {
        mForgetFlag = forget;
    }

    bool myUnlimitedAmmo, myUnlimitedArmor, myNoGroundCollision, myNoObjectCollision;
    internal void setGodMode(uint d1, uint d2, uint d3, uint d4)
    {
        Debug.Log("Хер тебе, а не бессмертие!");
        //MissionClient* mcl = mlClients.Head();
        //for (; mcl; mcl = mcl->Next())
        //foreach (MissionClient mcl in mlClients)
        //    myClientInfo.setGodMode(mcl.Client(), (d1 == 2) ? myUnlimitedAmmo : d1, (d2 == 2) ? myUnlimitedArmor : d2, (d3 == 2) ? myNoGroundCollision : d3, (d4 == 2) ? myNoObjectCollision : d4);
    }

    internal void setCraftAccess(string name, ICoopMission.TechAccess access)
    {
        myCraftsAccess.setTrigger(name, (int)access, true);
    }

    //struct AwardInfo
    //{
    //    public string mpName;
    //    public bool mTwice;

    //    public AwardInfo(string mpName, bool mTwice)
    //    {
    //        this.mpName = mpName;
    //        this.mTwice = mTwice;
    //    }
    //}
}
public partial class StdCooperativeAi : StdMissionAi
{
    new public const uint ID = 0xB82BCB5B;
    int mCooperativeGroupsCreated = 0;
    float update_env_timer = 0;
    public IDataHasher hasher;

    public override object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case StdCooperativeAi.ID: return this;
            case ICoopMission.ID: return myCoopMission;
            default: return base.Query(cls_id);
        }
    }

    public bool havePlayableGroup() { return myPlayableGroup != Constants.THANDLE_INVALID; }

    void updatePlayableGroup()
    {
        if (havePlayableGroup())
        {
            for (int i = 0; i < mpData.GetGroupsCount(); ++i)
            {
                GROUP_DATA Grp = mpData.GetGroups()[i];
                if (Grp.GetFlag(CUF_PLAYABLE)!=0)
                    Grp.ClearFlag(CUF_PLAYABLE);
                if (Grp.ID == myPlayableGroup)
                    Grp.SetFlag(CUF_PLAYABLE);
            }
            //for (MissionClient mcl = mlClients.Head(); mcl; mcl = mcl.Next())
            foreach(MissionClient mcl in mlClients)
            {
                if (mcl.GetUnitData() == null)
                {
                    ClientSpawnData cl_data = FindUnitDataForClient(mcl, isHost(mcl));
                    if (cl_data.mpUnitData!=null)
                    {
                        UnitSpawnData sp_data=new UnitSpawnData();
                        Parsing.FillSpawnDataFromUnitData(cl_data.mpUnitData, ref sp_data); // заполняем данные
                        mcl.Client().SetUnitSpawnData(sp_data);      // проставляем
                        mcl.SetSpawnData(cl_data);
                    }
                }
            }

        }
    }
    public void InitEnvironment()
    {
        // инициализируем удаленную систему хеширования данных
        hasher = iserver.GetDataHash();
        update_env_timer = -1.0f;
        if (hasher != null)
        {
            hasher.HashData((int)mpData.GetDefGroupsCount(), mpData.GetDefGroups());
            hasher.AddRef();
        }
    }

    bool mDeleteUnitsAfterDeath;
    protected void SetDeleteUnitFlag(bool fl) { mDeleteUnitsAfterDeath = fl; }

    public void DoneEnvironment()
    {
        hasher.UnHashData();
        hasher.Release();
    }


    // Section : common and initialize
    // ---------------------------------------------------------------------
    const string sCheat1 = "GroundCollision";
    const string sCheat2 = "ObjectCollision";
    const string sCheat3 = "UnlimitedAmmo";
    const string sCheat4 = "UnlimitedArmor";
    const string sServerSuccees = "srv_success";

    public override void SetInterface(IGame _igame, IServer _iserver, AiMissionData msn_data)
    {
        base.SetInterface(_igame, _iserver, msn_data);
        InitEnvironment();
        iUnifiedVariableContainer ns = GetVars(sLocal);
        InitVariables(ns);
        bool cheats = GetIntValue(mGameOptions, sCheat1, 0) == 0 &&
            GetIntValue(mGameOptions, sCheat2, 0) == 0 &&
            GetIntValue(mGameOptions, sCheat3, 0) == 0 &&
            GetIntValue(mGameOptions, sCheat4, 0) == 0;
        SetAddStatisticsMode((uint)(cheats ? 1 : 0));

        mpScoresInfo = stdlogic_dll.mpAiData.GetVariableTpl<iUnifiedVariableContainer>("Scores");
        iserver.SetTimeScale(0);
        mState = scs_WaitForAdmin;
        myUnlimitedAmmo = igame.getGameOptions().mUnlimitedAmmo;
        myUnlimitedArmor = igame.getGameOptions().mUnlimitedArmor;
        myNoGroundCollision = igame.getGameOptions().mNoDamGround;
        myNoObjectCollision = igame.getGameOptions().mNoDamObjects;
        mCheckEngage = false;
        SetNotifyEnable(false);

        CoopCheckAppear check_appear = new CoopCheckAppear(1);
        ClearUnitDataHandles(check_appear);
# if _DEBUG
        GetCommandsApi()->RegisterCommand(sGroupTarget, this, 2);
#endif //_DEBUG
        GetCommandsApi().RegisterCommand(sServerSuccees, this, 1);
# if MISSION_CREATION_FLAG
        //GetCommandsApi()->ExecFile("*server.dlc");
#endif
        setMissionType('C');
        myCoopFactory = Factories.createCoopMsnFactory(getIQuery(), myStdMsnFactory);

        loadMissionTriggers(myCraftsAccess, "AccessRules", "Crafts"); //ВОзможно тут нужны ref
        loadMissionTriggers(myWeaponsAccess, "AccessRules", "Weapons");
    }

    protected IVmFactory myCoopFactory;
    // Section : common initializers
    // ---------------------------------------------------------------------
    void InitVariables(iUnifiedVariableContainer ns)
    {
        iUnifiedVariableContainer global = GetVars(sLocal);

        mpKillboard = new CDWrapperTable("KB", mlClients, KB_X, KB_Y, KB_HEIGHT, KB_WIDTH, KB_COL_COUNT);
        mpKillboard.AddRow(null, KB_ROW_HUMANS_ID, mPlainColor.mBackColorIndex, mPlainColor.mTextColorIndex, mPlainColor.mFontSize, mPlainColor.mFontSpacing);
        mpKillboard.AddRow(null, KB_ROW_VELIANS_ID, mPlainColor.mBackColorIndex, mPlainColor.mTextColorIndex, mPlainColor.mFontSize, mPlainColor.mFontSpacing);
        mpKillboard.AddRow(null, KB_ROW_WING_ID, mPlainColor.mBackColorIndex, mPlainColor.mTextColorIndex, mPlainColor.mFontSize, mPlainColor.mFontSpacing);
        mpKillboard.AddRow(null, KB_ROW_PLAYER_ID, mPlainColor.mBackColorIndex, mPlainColor.mTextColorIndex, mPlainColor.mFontSize, mPlainColor.mFontSpacing);

        //myCoopScores.New() = new CoopScoreInfo(KB_ROW_HUMANS_ID, "row_Humans");
        //myCoopScores.New() = new CoopScoreInfo(KB_ROW_VELIANS_ID, "row_Velian");
        //myCoopScores.New() = new CoopScoreInfo(KB_ROW_WING_ID, "row_PlayerWing");
        //myCoopScores.New() = new CoopScoreInfo(KB_ROW_PLAYER_ID, "row_Player");
        myCoopScores.Add(new CoopScoreInfo(KB_ROW_HUMANS_ID, "row_Humans"));
        myCoopScores.Add(new CoopScoreInfo(KB_ROW_VELIANS_ID, "row_Velian"));
        myCoopScores.Add(new CoopScoreInfo(KB_ROW_WING_ID, "row_PlayerWing"));
        myCoopScores.Add(new CoopScoreInfo(KB_ROW_PLAYER_ID, "row_Player"));

        mColNames[KB_COL_TEAM] = Parsing.sAiEmpty;
        mColNames[KB_COL_NAME] = "col_Name";
        mColNames[KB_COL_NAME2] = Parsing.sAiEmpty;
        mColNames[KB_COL_AIR] = "col_CoopAir";
        mColNames[KB_COL_GROUND] = "col_CoopGround";
        mColNames[KB_COL_VEHICLE] = "col_CoopVehicle";
        mColNames[KB_COL_SHIPS] = "col_CoopShips";

        mpAwards = UniTools.CreateIntTable(global, "AwardsList");
        if (mpAwards!=null)
            CreateAwardsList(mpAwards);

        if (ns != null)
        {
            // init objectives
            mpPrimaries = GetVars("PrimaryObjectives");
            mpSecondaries = GetVars("SecondaryObjectives");
            mpOthers = GetVars("OtherObjectives");
        }
    }
    public override void AdminBan(MissionClient admin, string arg1, string arg2)
    {
        throw new System.NotImplementedException();
    }

    public override void AdminKick(MissionClient admin, string arg1, string arg2)
    {
        throw new System.NotImplementedException();
    }

    public override void AdminLog(MissionClient admin, string arg1, string arg2)
    {
        throw new System.NotImplementedException();
    }

    public override void AdminMute(MissionClient admin, string arg1, string arg2, bool mute)
    {
        throw new System.NotImplementedException();
    }

    public override void AdminSaveBanList(MissionClient admin, string arg1, string arg2)
    {
        throw new System.NotImplementedException();
    }

    public override void AdminUnBan(MissionClient admin, string arg1, string arg2)
    {
        throw new System.NotImplementedException();
    }

    public override bool canDrawEnemy()
    {
        return false;
    }

    public override bool canUseOtherSideData()
    {
        return true;
    }

    public override void CheckUniqueCallsigns()
    {
        throw new System.NotImplementedException();
    }

    public override void ClearUnitDataHandles(iCheckAppear check)
    {
        for (int i = 0; i < mpData.GetGroupsCount(); ++i)
        {
            GROUP_DATA Grp = mpData.GetGroups()[i];
            if (check != null && check.IsGroupAppear(Grp) == false)
            {
                //Debug.Log(string.Format("Destroyed group: {0} due check {1} {2}", Grp.Callsign, check, check != null ? check.IsGroupAppear(Grp): "check is null"));
                Grp.Destroyed();
                continue;
            }
            else
                Grp.Cleared();
            for (int j = 0; j < Grp.nUnits; ++j)
            {
                UNIT_DATA un = Grp.Units[j];
                if (check != null && check.IsUnitAppear(un, myDifficulty) == false)
                {
                    un.Destroyed();
                    //Debug.Log(string.Format("Destroyed unit  [{0} {1}] due check {2} {3}", Grp.Callsign, (int)Grp.Units[j].Number, check, check != null ? check.IsUnitAppear(un, myDifficulty) : "check is null"));
                }
                else
                {
                    un.Cleared();
                    //Debug.Log(string.Format("Enabled unit  [{0} {1}] due check {2} {3}", Grp.Callsign, (int)Grp.Units[j].Number, check, check != null ? check.IsUnitAppear(un, myDifficulty) : "check is null"));
                }
            }
        }
    }

    public override void ClientInfoChanged(MissionClient mcl)
    {
        throw new System.NotImplementedException();
    }

    public override void Console(iClient cl, string format, params string[] v)
    {
        throw new System.NotImplementedException();
    }

    public override uint CountClientsAround(Vector3 org, float radius)
    {
        throw new System.NotImplementedException();
    }

    public override void DeleteContact(iContact cnt, bool explode)
    {
        Asserts.AssertBp(cnt != null);
        if (cnt.GetState() != iSensorsDefines.CS_DEAD)
            iserver.DeleteUnit(cnt, explode);

    }

    public override void DeleteGroup(IGroupAi grp)
    {
        throw new System.NotImplementedException();
    }

    public override MissionClient Disconnecting(iClient client, bool dropped)
    {
        throw new System.NotImplementedException();
    }

    public override void ExecuteBatch(string batch_name)
    {
        throw new System.NotImplementedException();
    }

    public override bool FillFragRows(ushort rows_count, ushort[] rows)
    {
        throw new System.NotImplementedException();
    }

    //public override ContactInfo FindContact(uint hndl)
    //{
    //    throw new System.NotImplementedException();

    //}

    //public override ContactInfo FindContact(UNIT_DATA data)
    //{
    //    throw new System.NotImplementedException();
    //}

    //public override ContactInfo FindContact(IGroupAi gai, uint index)
    //{
    //    throw new System.NotImplementedException();
    //}

    //public override ContactInfo FindContact(IBaseUnitAi bua)
    //{
    //    base.FindContact
    //    throw new System.NotImplementedException();
    //}

    public override GroupAiCont FindGroupCont(uint id)
    {
        for (int i = 0; i < myGroups.Count; ++i)
            if (myGroups[i].mpGroupData.ID == id) return myGroups[i];
        return null;
    }

    public override void FinishListName(iClient pClient, uint d)
    {
        throw new System.NotImplementedException();
    }

    public override uint GetClientsCountForAppear(uint flag)
    {
        throw new System.NotImplementedException();
    }

    //public override iContact GetContactByIndex(uint grp_id, uint un_id)
    //{
    //    throw new System.NotImplementedException();
    //}

    public override bool GetContactInfo(iContact cnt, out uint grp_id, out uint un_index, out uint side)
    {
        grp_id = 0;
        un_index = 0;
        side = 0;
        iContact root = cnt.GetTopContact();
        ContactInfo info = FindContact(root.GetHandle());
        if (info == null)
            return false;
        grp_id = info.mpGroupCont.mpGroupData.ID;
        un_index = info.mpData.Number + 1;
        side = info.mpGroupCont.mpGroupData.Side;
        return true;

    }

    public override GROUP_DATA GetExistedGroupData(uint grp_id)
    {
        return mpData != null ? mpData.GetGroupByID(grp_id) : null;
    }

    public override ushort GetFragRowsCount()
    {
        throw new System.NotImplementedException();
    }

    //public override MARKER_DATA GetMarkerData(uint mrk_id)
    //{
    //    throw new System.NotImplementedException();
    //}

    public override int GetMenuItems(iClient pClient, uint id, AiMenuItem[] ami, int max_count, int page_index)
    {
        Debug.Log("Total: " + ami.Length);
        foreach (var v in ami)
        {
            Debug.Log(v.mMessage);
        }
        throw new System.NotImplementedException();
    }


    public override bool GetMessageMode(uint side)
    {
        return (CampaignDefines.CS_SIDE_HUMANS == side);
    }

    public const string MyMissionMsg = "Standard Cooperative";
    public override string GetMissionDescription()
    {
        return MyMissionMsg;
    }

    public override string GetMissionType()
    {
        throw new System.NotImplementedException();
    }

    public override uint GetMissionVersion()
    {
        throw new System.NotImplementedException();
    }

    public override void GetPlayersInfo(out iAboutPlayer info)
    {
        throw new System.NotImplementedException();
    }


    public override string GetTeamName(uint team_code)
    {
        throw new System.NotImplementedException();
    }

    public override IVmFactory getTopVmFactory()
    {
        return myCoopFactory;
    }

    public override uint GetUniqueGroupID()
    {
        throw new System.NotImplementedException();
    }


    public override void Say(uint side_code, string pFormat, RadioMessage pData)
    {
        //for (MissionClient mcl = mlClients.Head(); mcl!=null; mcl = mcl.Next())
        //{
        //    DWORD side_to_check = SIDE_MISSION;
        //    if (side_code != SIDE_MISSION)
        //        side_to_check = mcl->GetGroupData() ? mcl->GetGroupData()->Side : CS_SIDE_HUMANS;
        //    if (side_to_check == side_code || side_code == CS_SIDE_NEUTRAL)
        //        mcl->Client()->SayMessage(pFormat, pData);
        //}
        foreach (MissionClient mcl in mlClients)
        {
            DWORD side_to_check = SIDE_MISSION;
            if (side_code != SIDE_MISSION)
                side_to_check = mcl.GetGroupData() != null ? mcl.GetGroupData().Side : CS_SIDE_HUMANS;
            if (side_to_check == side_code || side_code == CS_SIDE_NEUTRAL)
                mcl.Client().SayMessage(pFormat, pData);
        }
    }

    public override string IsPlayer(UNIT_DATA dt, ref string real_player_name)
    {
        string ret = null;
        MissionClient mcl = FindClient(dt);
        if (mcl != null && mcl.GetGroupData() != null)
        {
            ret = mcl.GetGroupData().Callsign;
            if (real_player_name != null)
                real_player_name = mcl.Name();
        }
        return ret;
    }

    public override bool IsRepairEnabled()
    {
        throw new System.NotImplementedException();
    }

    public override bool IsRespawnAfterRepair()
    {
        throw new System.NotImplementedException();
    }


    public override void NotifyClient(iClient cl, string format, params string[] args)
    {
        throw new System.NotImplementedException();
    }

    public override void NotifyClients(string format, params string[] args)
    {
        throw new System.NotImplementedException();
    }

    public override void NotifyHost(string format, params string[] args)
    {
        throw new System.NotImplementedException();
    }

    // ---------------------------------------------------------------------
    // Section : killboard
    // ---------------------------------------------------------------------
    public override void OnAddDamage(DWORD VictimHandle, DWORD GadHandle, DWORD WeaponCode, float Damage, bool IsFinal)
    {
        if (null == mpJustDamaged) return;

        Asserts.AssertBp(mpJustDamaged.mHandle == VictimHandle);

        if (mpJustDamaged.IsKilled()) return;

        if (GadHandle != Constants.THANDLE_INVALID && VictimHandle != GadHandle)
        {      // если атакующий не земля то он последний
            mpJustDamaged.SetLastAttacker(GadHandle);
        }

        if (IsFinal && !IsServerShutdowned())
        {     // произошло убийство
            mpJustDamaged.Kill();
            if (mDeleteUnitsAfterDeath)
                mpJustDamaged.mpGroupCont.ai.DeleteUnitAfterDeath(mpJustDamaged.mpData);
            else
                mpJustDamaged.mpGroupCont.ai.RespawnUnitAfterDeath(mpJustDamaged.mpData, RespawnType.rtRespawnNever, null, OverwriteType.otOverwriteNone);
            ContactInfo killer = (mpJustDamaged.GetLastAttacker() == Constants.THANDLE_INVALID ? null : FindContact(mpJustDamaged.GetLastAttacker()));
            if (killer != null)
            {
                if (IsCountStatistics())
                    RegisterKill(mpJustDamaged, killer);
                if (killer.IsNotify())
                    killer.mpAi.OnKill(VictimHandle);
            }
        }
        if (mpJustDamaged.IsNotify())
            mpJustDamaged.mpAi.OnDamage(GadHandle, Damage, IsFinal);
    }

    public void RegisterKill(ContactInfo victim, ContactInfo killer)
    {
        //TODO Корректно регистрировать убийства
    }
    public override void OnChangeAdminStatus(MissionClient mcl, bool admin)
    {
        throw new System.NotImplementedException();
    }

    public override void OnCommand(int i, string c, string s)
    {
        throw new System.NotImplementedException();
    }

    void UploadHostClientName()
    {
        iUnifiedVariableContainer local = GetVars(sLocal);
        if (local != null)
        {
            MissionClient mcl = getHost();
            if (mcl != null)
            {
                iUnifiedVariableString player_name = local.CreateVariableTpl<iUnifiedVariableString>("PlayerName");
                if (player_name != null)
                    player_name.SetValue(mcl.Client().GetPlayerName());
            }
        }
    }

    GROUP_DATA GetPlayableGroup(MissionClient mcl)
    {
        for (int i = 0; i < mpData.GetGroupsCount(); ++i)
        {
            GROUP_DATA Grp = mpData.GetGroups()[i];
            if (Grp.GetFlag(CUF_PLAYABLE) != 0)
                return Grp;
        }
        return null;
    }

    const int CRC_NULL = -1;
    ClientSpawnData FindUnitDataForClient(MissionClient mcl, bool is_leader)
    {
        ClientSpawnData sp_data = new ClientSpawnData();
        GROUP_DATA grp_data = GetPlayableGroup(mcl); // находим группу для клиента
        Debug.Log(string.Format("нашли группу для клиента: {0}", grp_data != null ? grp_data.Callsign : "NOT FOUND"));
        sp_data.mpGroupData = grp_data;
        UNIT_DATA ret = null;
        if (grp_data != null)
        {             // если нашли
            if (is_leader)
            {            // если это лидер
                Debug.Log("это лидер");
                ret = getPosition(grp_data, myPlayerPosition);
                Debug.Log("позиция установлена: " + ret.Number);
                sp_data.mpUnitData = ret;
                Debug.Log(string.Format("заполняем UNIT_DATA для клиента"));
                mcl.SetSpawnData(sp_data); // заполняем UNIT_DATA для клиента
            }
            else
            {
                // двигаем если только в одной группе
                if (GetNotBusyCount(grp_data) > 0)
                {     // все летим в одной группе !!
                    getHost().ClearSpawnData(); // забываем место лидера
                    ret = getPosition(grp_data, CRC_NULL);
                    sp_data.mpUnitData = ret;
                    mcl.SetSpawnData(sp_data); // заполняем UNIT_DATA для клиента
                    sp_data.mpUnitData = getPosition(grp_data, myPlayerPosition);
                    Asserts.Assert(sp_data.mpUnitData != null);
                    getHost().SetSpawnData(sp_data);
                }
            }
        }
        sp_data.mpGroupData = grp_data;
        sp_data.mpUnitData = ret;
        return sp_data;
    }

    DWORD GetNotBusyCount(GROUP_DATA grp_data)
    {
        DWORD n = 0;
        for (int i = (int)grp_data.nUnits - 1; i >= 0; --i)
        {
            if (!PlaceIsBusy(grp_data.Units[i]))
                n++;
        }
        return n;
    }

    bool PlaceIsBusy(UNIT_DATA dt)
    {
        if (dt.IsDestroyed())  // если юнит убит - выходим
            return true;
        //for (MissionClient* mcl = mlClients.Head(); mcl; mcl = mcl->Next())
        //    if (mcl->GetUnitData() == dt) return true;     // если есть у какого-нибудь клиента
        foreach (MissionClient mcl in mlClients)
        {
            if (mcl.GetUnitData() == dt) return true;
        }
        return false;
    }

    UNIT_DATA getPosition(GROUP_DATA grp_data, int offset)
    {
        UNIT_DATA last_free = null;
        for (int i = 0; i < grp_data.nUnits; ++i)
        {
            if (!PlaceIsBusy(grp_data.Units[i]))
            {
                if (offset == 0)
                    return grp_data.Units[i];
                else
                {
                    last_free = grp_data.Units[i];
                    offset--;
                }
            }
        }
        return last_free;
    }

    // environment
    StdCoopState mState;
    bool CanShowEngbay() { return (mState != scs_Running && mState != scs_CreatEnv); }

    bool myEngBayEnabled = true;
    public override bool OnConnect(iClient client)
    {
        Debug.Log("Creating client: " + client);
        MissionClient mcl = Connecting(client);
        if (mcl != null)
        {
            if (mcl == getHost())
            { // регистрируем команду только первому клиенту
                client.RegisterRemoteConsoleCommand(sServerPause, 0);
                client.RegisterRemoteConsoleCommand(sServerAccel, 0);
                UploadHostClientName();
            }
            if (mcl.Client().IsHidden() == false)
            {
                client.RegisterRemoteConsoleTrigger(sClientScores);
                iserver.SetTimeScale(1);
                if (mlClients.Count > 1)
                {
                    if (CanShowEngbay())
                    {
                        CoopCheckAppear check_appear = new CoopCheckAppear((uint)computeRealPlayers());
                        ClearUnitDataHandles(check_appear);
                    }
                }
                bool not_observer = mcl.CanRespawn() && CanShowEngbay();
                bool is_leader = mcl == getHost();
                Debug.Log(string.Format("Not_observer [{3}] CR {0} CSE {1} mState {2}", mcl.CanRespawn(), CanShowEngbay(), mState, not_observer));
                if (not_observer)
                {
                    Debug.Log("находим стартовое место для клиента");
                    ClientSpawnData cl_data = FindUnitDataForClient(mcl, mcl == getHost());  // находим стартовое место для клиента
                    if (cl_data.IsValid())
                    {       // если таковое нашлось
                        UNIT_DATA dt = cl_data.mpUnitData;
                        UnitSpawnData sp_data = new UnitSpawnData();
                        Parsing.FillSpawnDataFromUnitData(cl_data.mpUnitData, ref sp_data); // заполняем данные
                        client.SetUnitSpawnData(sp_data);      // проставляем
                        fillTechData(dt, is_leader);
                        myEngBayEnabled = false;//TODO удалить отключение отображения ангара!
                        if (myEngBayEnabled)
                        {
                            crc32[] weapons = myWeapons.Begin();
                            //var weapons = myWeapons.ToArray();
                            DWORD weapons_count = (uint)myWeapons.Count();
                            crc32[] crafts = myCrafts.Begin();
                            //var crafts = myCrafts.ToArray();
                            DWORD crafts_count = (uint)myCrafts.Count();
                            client.showEngBay(crafts_count, crafts, weapons_count, weapons);            // показывем engbay
                        }
                    }
                }
                else
                    mcl.SetCameraDraw(true);

                mpKillboard.Create(mcl);

                //SetKillBoardScreen(mcl, true);
                //CooperativeAiInformer informer = new CooperativeAiInformer(ref mPlainColor, ref mHeadColor, mColNames, ref myCoopScores);
                //Synchronize(mcl, mpKillboard, informer.GetiGetTableInfoClient());
            }
        }
        return mcl != null;
    }

    CDWrapperTable mpKillboard;
    string[] mColNames = new string[KB_COL_COUNT];
    //CoopScoreInfo[] myCoopScores; //TODO - тут возможно правильнее List
    List<CoopScoreInfo> myCoopScores = new List<CoopScoreInfo>();

    void SetKillBoardScreen(MissionClient mcl, bool off)
    {
        if (mcl.Client().IsHidden() == false)
            mpKillboard.Hide(mcl, off);
    }

    //public override ContactInfo OnCreateContact(IBaseUnitAi ai, iContact cnt, UNIT_DATA ud, IGroupAi ga)
    //{
    //    throw new System.NotImplementedException();
    //}


    //public override void OnDeleteGroup(GroupAiCont gac)
    //{
    //    throw new System.NotImplementedException();
    //}

    public override void OnDisconnect(iClient cl, bool dropped)
    {
        throw new System.NotImplementedException();
    }

    public override bool OnMutePlayer(MissionClient mcl, bool mute, bool connecting = false)
    {
        if (mcl.Client().IsHidden() == true && connecting == false) return false;
        if (mute)
        {
            mcl.Client().UnRegisterRemoteConsole(Hasher.HshString(sCommonSay));
        }
        else
        {
            mcl.Client().RegisterRemoteConsoleCommand(sCommonSay, 1);
        }
        return true;
    }

    public override void ProcessContactDeath(ContactInfo info, bool landed_or_repaired) { }

    public override void ProcessClientDeath(ContactInfo info, MissionClient mcl, bool landed_or_repaired)
    {
        base.ProcessClientDeath(info, mcl, landed_or_repaired);

        if (!landed_or_repaired)
            mcl.ClearSpawnData();

        bool respawning = false;
        if (landed_or_repaired)
        {
            if (mcl.GetUnitData().IsLanded(true))
                respawning = true;
            else if (havePlayableGroup())
            {
                mcl.ClearSpawnData();
                updatePlayableGroup();
                respawning = true;
            }
        }
        // выход клиента 
        if (!respawning)
        {
            SetKillBoardScreen(mcl, false);
            setClientStatus(mcl, landed_or_repaired ? (int)CS_Landed : (int)CS_Killed);
            checkMissionStatus();
        }

    }
    void checkMissionStatus()
    {
        if (getMissionStatus() == (int) MS_Normal)
        {
            int landed = 0, flying = 0;
            //for (MissionClient* mcl = mlClients.Head(); mcl; mcl = mcl->Next())
            foreach(var mcl in mlClients)
            {
                if (mcl.getStatus() == (int) CS_Landed) landed++;
                if (mcl.getStatus() == (int) CS_Escaped) flying++;
            }

            if (flying==0)
                setMissionStatus(landed!=0 ? (int)MS_Complete : (int)MS_Failed);
        }
    }

    public iContact CreateContact(UNIT_DATA un, int side, iContact hangar)
    {
        UnitSpawnData data = new UnitSpawnData();             // create unit spawn data
        Parsing.FillSpawnDataFromUnitData(un, ref data);
        data.SideCode = (uint)side;
        return iserver.CreateUnit(data, un.Org, un.Angle, hangar);
    }

    // ---------------------------------------------------------------------
    // Section : units creation
    // ---------------------------------------------------------------------
    public override IBaseUnitAi CreateUnitAi(UNIT_DATA un, int side, iContact hangar, IGroupAi grp)
    {
        //Debug.Log("Create UnitAi " + grp.GetGroupData().Callsign);
        iContact cnt = CreateContact(un, side, hangar);
        IBaseUnitAi ai = null;
        bool unit_ai = true;
        MissionClient mcl = null;
        mcl = FindClient(un);
        if (mcl != null)
        {
            Asserts.AssertBp(mcl.Client().GetAI() == null);
            unit_ai = false;
        }
        else
            unit_ai = true;
        if (!unit_ai)
        {
            ai = iserver.CreateUnitAi(mcl.Client(), un, cnt, grp);             // create client ai
                                                                               //mcl->mpUnitData=un;
        }
        else
            ai = iserver.CreateUnitAi(un.AI, un, cnt, grp);          // else create unit ai
        if (ai == null)
        {                                                 // if ai creation failed
            DeleteContact(cnt, false);                             // delete body
            un.Destroyed();
        }
        else
        {
            if (!unit_ai) update_env_timer = -1f;
            un.Created();                                         // else mark as created
            ai.SetSkill(mSkill);
            OnCreateContact(ai, cnt, un, grp);
            if (mcl != null)
            {
                mcl.SetGroup(grp);                                   // setup group
                mcl.SetContact(cnt);
                setClientStatus(mcl, (int)GameStatuses.ClientStatuses.CS_Escaped);
            }
        }
        return ai;
    }

    //public override bool Update(float scale)
    //{
    //    bool update_env = true;
    //    UpdateEnvironment(scale);
    //    return update_env ? base.Update(scale) : true;
    //}

    float myDelayStart = 0;
    float myFcScale = 0;
    bool myDelayStartCompleted = true;

    public override void setFcScale(float scale)
    {
        myFcScale = scale;
    }
    // variables
    iUnifiedVariableContainer mpScoresInfo;
    DWORD mLACount;
    DWORD mTurretsCount;

    // objectives
    iUnifiedVariableContainer mpPrimaries;
    iUnifiedVariableContainer mpSecondaries;
    iUnifiedVariableContainer mpOthers;
    TLIST<ObjectiveHolder> mlObjectives = new TLIST<ObjectiveHolder>();
    int mBodyCount;
    int mObjectiveCount;
    int mOtherCount;
    int mTotalScore;
    int mAwardsAwarded;
    bool mCountStatistics;
    DWORD mAddStatistics;
    int myPlayerPosition = CRC_NULL;

    iUnivarTable mpAwards;

    DWORD myPlayableGroup;

    // forgeting
    bool mForgetFlag;

    CoopMission<StdCooperativeAi> myCoopMission;

    public StdCooperativeAi()
    {
        mDeleteUnitsAfterDeath = true;
        mpScoresInfo = null;
        mBodyCount = 0;
        mObjectiveCount = 0;
        mOtherCount = 0;
        mpKillboard = null;
        mpAwards = null;
        mLACount = 0;
        mTurretsCount = 0;
        mTotalScore = 0;
        myPlayableGroup = Constants.THANDLE_INVALID;
        mCountStatistics = true;
        mAddStatistics = 1;
        mCooperativeGroupsCreated = 0;
        mAwardsAwarded = 0;
        mForgetFlag = false;
        myCoopMission = new CoopMission<StdCooperativeAi>(this);
        myDelayStart = 60;
        { }
    }


    // Section : scene update

    const int ENG_BAY_WAIT_TIME = 21;


    bool areClientsSwapped()
    {
        int overall_count = 0, swapped_count = 0;
        //for (MissionClient* mcl = mlClients.Head(); mcl; mcl = mcl->Next())
        //{
        //    overall_count++;
        //    if (mcl->Client()->isSwapped())
        //        swapped_count++;
        //}
        foreach (MissionClient mcl in mlClients)
        {
            overall_count++;
            if (mcl.Client().isSwapped())
                swapped_count++;
        }
        return overall_count == swapped_count;
    }
    public override bool Update(float scale)
    {

        if (!myDelayStartCompleted)
        {
            myDelayStart -= myFcScale;
            if (myDelayStart < 0 || areClientsSwapped())
            {
                myDelayStartCompleted = true;
                iserver.SetTimeScale(1);
            }
        }

        bool update_env = true;
        UpdateEnvironment(scale);
        if (mState != scs_Running)
        {
            switch (mState)
            {
                // ------------------------------------------------------ 
                case scs_WaitForAdmin:
                    { // ждем лидеров
                        update_env = false;
                        MissionClient mcl = getHost();
                        if (mcl != null)
                        {
                            SetNotifyEnable(true);
                            if (!mcl.Client().IsInEngBay())
                            {  // if leader not in eng bay
                                if (InEngBayCount() == 0)
                                {    // if all not in eng bay 
                                    ClientsFinishEngBay();
                                    mState = scs_CreatEnv;      // just start
                                    mLastStartTimeCounter = stdlogic_dll.mCurrentTick;
                                }
                                else
                                {                        // notify all about leader select
                                    say(ENG_BAY_CLOSED);// "notify_mission_start"
                                    mStartTimer = ENG_BAY_WAIT_TIME;
                                    mLastStartTimeCounter = (uint)mStartTimer;
                                    mState = scs_WaitForAll;
                                    mLastTime = CppFunctionEmulator.GetTickCount();
                                }
                            }
                        }
                    }
                    break;
                // ------------------------------------------------------
                case scs_WaitForAll:
                    { // ждем всех клиентов
                        update_env = false;
                        DWORD temp = CppFunctionEmulator.GetTickCount();
                        mStartTimer -= (temp - mLastTime) * .001f;
                        mLastTime = temp;
                        if (mStartTimer > 0 && InEngBayCount() != 0)
                        {
                            DWORD now = (uint)mStartTimer;
                            if (now != mLastStartTimeCounter)
                            {
                                mLastStartTimeCounter = now;
                                if (now == 20 || now == 10 || (now <= 5 && now > 0))
                                    say(GetCountdownCodeFromDWORD(mLastStartTimeCounter));
                            }
                        }
                        else
                        {
                            ClientsFinishEngBay();
                            mState = scs_CreatEnv;      // just start
                            mLastStartTimeCounter = stdlogic_dll.mCurrentTick;

                        }
                    }
                    break;
                // ------------------------------------------------------
                case scs_CreatEnv:
                    {    // ждем создания юнитов
                        if (stdlogic_dll.mCurrentTick - mLastStartTimeCounter >= 4)
                        {
                            mState = scs_Running;
                            setMissionStatus((int)MS_Normal);
                            ClientsStartDraw();
                            myDelayStartCompleted = false;
                            if (mlClients.Count == 1)
                                myDelayStart = 0;
                            iserver.SetTimeScale(0);
                        }
                    }
                    break;
                    // ------------------------------------------------------
            }
        }
        if (mCheckEngage)
        {
# if MISSION_CREATION_FLAG
            if (iserver.GetTimeScale() == 1 || IsClientsEngaged())
            {
                iserver.SetTimeScale(1);
                mCheckEngage = false;
            }
#endif
        }
        if (!update_env) Debug.Log("update_env " + update_env + " mState " + mState);
        if (!myDelayStartCompleted) Debug.Log(string.Format("myDelayStart < 0 " + myDelayStart + " || areClientsSwapped() " + areClientsSwapped()));
        return update_env ? base.Update(scale) : true;
    }

    bool mCheckEngage;

    // eng bay
    DWORD mLastStartTimeCounter;
    float mStartTimer;
    DWORD mLastTime;

    DWORD GetCountdownCodeFromDWORD(DWORD code)
    {
        switch (code)
        {
            case 1: return COUNTDOWN_1;
            case 2: return COUNTDOWN_2;
            case 3: return COUNTDOWN_3;
            case 4: return COUNTDOWN_4;
            case 5: return COUNTDOWN_5;
            case 10: return COUNTDOWN_10;
            case 20: return COUNTDOWN_20;
        }
        return 0;
    }
    void ClientsStartDraw()
    {
        //    for (MissionClient* mcl = mlClients.Head(); mcl; mcl = mcl->Next()) // бежим по всем клиентам
        //        mcl->SetCameraDraw(true);
        foreach (MissionClient mcl in mlClients)
        {
            mcl.SetCameraDraw(true);
        }
    }
    void ClientsFinishEngBay()
    {
        //for (MissionClient* mcl = mlClients.Head(); mcl; mcl = mcl->Next())
        //{ // бежим по всем клиентам
        //    UNIT_DATA* dt = mcl->GetUnitData();
        //    if (dt)
        //        FillUnitDataFromSpawnData(mcl->Client()->GetUnitSpawnData(), dt);
        //    mcl->Client()->hideEngBay();
        //}
        foreach (MissionClient mcl in mlClients)
        {
            UNIT_DATA dt = mcl.GetUnitData();
            if (dt != null) Parsing.FillUnitDataFromSpawnData(mcl.Client().GetUnitSpawnData(), ref dt);
        }
    }
}



public partial class StdCooperativeAi : RoadElementEnumer
{
    RoadElementState state;
    public void SetState(RoadElementState new_state) { state = new_state; }
    public RoadElementState GetState() { return state; }

    const float UpdateTime = 2f;
    public void UpdateEnvironment(float scale)
    {
        update_env_timer -= scale;
        //TODO исправить время на корректное
        //update_env_timer = 0;

        //if (update_env_timer > 0 || mlPositions.Count == 0 || hasher == null) return;
        if (update_env_timer > 0 || hasher == null) return;
        update_env_timer = UpdateTime;

        //TODO создавать здесь клиента - плохая идея. Нужно перенести в другое место

        //for (LinkedListNode<MissionClient> mcl = mlClients.First; mcl != null; mcl = mcl.Next)
        //{
        //    EnumPosition pos = mcl.Value.Position();
        //    pos.new_org = mcl.Value.Client().GetCameraOrg();
        //    pos.new_radius = mcl.Value.Client().GetCameraRange() + 1000f;
        //    mcl.Value.mpPosition = pos;
        //}
        //hasher.EnumCrosses(mlPositions, RSObjectId(RS_ALL_GROUPS_RELATED), this);

        //if (mlPositions.Count == 0) mlPositions.Add(new EnumPosition());
        //mlPositions[0].new_org = SceneVisualizer.pCameraData.myCamera.Org;
        //mlPositions[0].new_radius = SceneVisualizer.pCameraData.GetCameraRange() + 1000f;


        for (LinkedListNode<MissionClient> mcl = mlClients.First; mcl != null; mcl = mcl.Next)
        {
            EnumPosition pos = mcl.Value.Position();
            pos.new_org = mcl.Value.Client().GetCameraOrg();
            pos.new_radius = mcl.Value.Client().GetCameraRange() + 1000f;
            mcl.Value.mpPosition = pos;
        }

        //Здесь рисуются дороги и здания из миссии по умолчанию
        hasher.EnumCrosses(ref mlPositions, RSObjectId(RS_ALL_GROUPS_RELATED), this);
    }

    public bool ProcessElement(HMember m)
    {
        IHashObject p = m.Object();

        //Debug.Log("Enumerator state is " + state + " for " + ((GroupDataContainer)m.Object()).pGroupData.Callsign);
        //Debug.Log("grp_data before created " + ((GroupDataContainer)m.Object()).pGroupData.IsCreated().ToString());
        switch (state)
        {
            case POS_OLD:
                if (p.GetFlag(RS_MODE_ALREADY_CREATED) != 0)
                {    // deleting
                    if (p.GetFlag(RS_CLASS_GROUP) != 0)
                    {
                        //if (DeleteGroupData((GroupDataContainer)p)) //TODO возможно, надо вернуть
                        if (DeleteGroupData(ref p))
                        {
                            p.ClearFlag(RS_MODE_ALREADY_CREATED);
                            mCooperativeGroupsCreated--;
                        }
                    }
                }
                break;
            case POS_NEW:
                if (p.GetFlag(RS_MODE_ALREADY_CREATED) == 0)
                {  // creating
                    if (p.GetFlag(RS_CLASS_GROUP) != 0)
                    {
                        //if (CreateGroupData((GroupDataContainer)p))
                        if (CreateGroupData(ref p))
                        {
                            p.SetFlag(RS_MODE_ALREADY_CREATED);
                            mCooperativeGroupsCreated++;
                        }
                    }
                }
                break;
        }
        return true;
    }

    //public bool CreateGroupData(ref GroupDataContainer uc)
    public bool CreateGroupData(ref IHashObject uc)
    {
        GROUP_DATA grp_data = ((GroupDataContainer)uc).pGroupData;
        Debug.Log("CreateGroupData " + grp_data.Callsign);
        if (grp_data.IsDestroyed())
            return false; // if group killed

        if (grp_data.IsCreated())       // if unit is created by group
        {
            return true;
        }
        //return CreateGroupAi(grp_data.AI, ref grp_data) != null;     // если нету ее то создаем
        bool res = CreateGroupAi(grp_data.AI, ref grp_data) != null;
        //Debug.Log(string.Format("{2} Created in var {0}, created in container {1}", grp_data.IsCreated().ToString(), ((GroupDataContainer)uc).pGroupData.IsCreated().ToString(), ((GroupDataContainer)uc).pGroupData.Callsign));
        return res;
    }

    //bool DeleteGroupData(GroupDataContainer uc)
    bool DeleteGroupData(ref IHashObject uc)
    {
        GROUP_DATA grp_data = ((GroupDataContainer)uc).pGroupData;
        if (grp_data.IsDestroyed())
            return false;

        GroupAiCont grp = FindGroupCont(grp_data.ID);
        if (grp != null)
            OnDeleteGroup(grp);

        grp_data.Deleted();
        return true;
    }

}

//StdCooperativeAiClients
public partial class StdCooperativeAi
{
    Tab<crc32> myCrafts = new Tab<DWORD>();
    Tab<crc32> myWeapons = new Tab<DWORD>();
    TriggersSystem myCraftsAccess = new(), myWeaponsAccess = new();
    int computeRealPlayers()
    {
        int count = 0;
        foreach (var mcl in mlClients)
        {
            if (mcl.CanRespawn() && CanShowEngbay() && mcl.Client().IsHidden() == false)
                count++;

        }
        //for (MissionClient mcl = mlClients.Head(); mcl; mcl = mcl->Next())
        //{
        //    if (mcl->CanRespawn() && CanShowEngbay() && mcl->Client()->IsHidden() == false)
        //        count++;
        //}
        return count;
    }

    void fillTechData(UNIT_DATA dt, bool is_leader)
    {
        myWeapons.Zero();
        myCrafts.Zero();


        if (is_leader && myPlayerPosition == 0)
            addNotDisabled((int)mpData.GetCraftCount(), mpData.GetCrafts(), myCraftsAccess, ref myCrafts);
        else
            myCrafts.New(dt.CodedName);

        addEnabled(myCraftsAccess, ref myCrafts);
        addNotDisabled((int)mpData.GetWeaponsCount(), mpData.GetWeapons(), myWeaponsAccess, ref myWeapons);
        addEnabled(myWeaponsAccess, ref myWeapons);
    }


}

//StdCooperativeAiDevices
public partial class StdCooperativeAi
{
    // killboard
    const int KB_COL_COUNT = 7;
    const int KB_COL_TEAM = 0;  // needed by StormGame
    const int KB_COL_NAME = 1;
    const int KB_COL_NAME2 = 2;
    const int KB_COL_AIR = 3;
    const int KB_COL_GROUND = 4;
    const int KB_COL_VEHICLE = 5;
    const int KB_COL_SHIPS = 6;


    const int KB_ROW_VELIANS_ID = 5000;
    const int KB_ROW_HUMANS_ID = 5001;
    const int KB_ROW_WING_ID = 5002;
    const int KB_ROW_PLAYER_ID = 5003;


    const float KB_X = 0.05f;
    const float KB_Y = 0.1f;
    const float KB_HEIGHT = 0.55f;
    const float KB_WIDTH = 0.9f;

    struct CooperativeAiInformer : iGetTableInfoClient
    {
        RowInfo mrPlainColor;
        RowInfo mrHeadColor;
        string[] mColNames;
        CoopScoreInfo[] myScores;
        // colors and cell data to constructor
        public CooperativeAiInformer(ref RowInfo plain, ref RowInfo head, string[] cols, ref CoopScoreInfo[] scores)
        {
            mrPlainColor = plain;
            mrHeadColor = head;
            mColNames = cols;
            myScores = scores;

        }

        public CoopScoreInfo getScore(DWORD id)
        {
            for (int i = 0; i < myScores.Length; ++i)
                if (myScores[i].getID() == id)
                    return myScores[i];
            return null;
        }


        public iGetTableInfoClient GetiGetTableInfoClient() { return this; }
        public bool GetRow(WORD y_name, ref RowInfo r) //TODO Реализовать корректню генерацию таблицы киллов
        {
            r = null;
            return false;
        }
        public bool GetCell(WORD y_name, WORD x, ref string buf, DWORD len)
        {
            buf = null;
            return false;
        }


    }
}
