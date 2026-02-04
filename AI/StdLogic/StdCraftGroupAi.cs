using System;
using System.Collections.Generic;
using UnityEngine;
using crc32 = System.UInt32;
using DWORD = System.UInt32;
using static AiCommands;
using static AIGroupsEvents;
using static HangarDefs;
using static FormationDefs;

//public class StdCraftGroupAi : StdDynamicGroupAi { new public const uint ID = 0x26E5E0F2; }

public class StdCraftGroupAi : StdDynamicGroupAi
{
    #region global variables
    public const int MAX_MENU_ITEMS = 10;

    public const int MAX_REPAIR_BASES_COUNT = 8;
    public const float PROCEED_BASE_DIST = 301f;
    #endregion

    new public const crc32 ID = 0x26E5E0F2;

    // route parameters
    public void SetRouteProperties(float min_y, float pred_time)
    {
        for (int i = 0; i < mGhostCount; i++)
        {
            AiUnit ai = mpUnits[i];
            if (ai==null||ai.GetAI() == null) continue;
            ICraftAi cr_ai = (ICraftAi)ai.GetAI().Query(ICraftAi.ID);
            if (cr_ai != null)
                cr_ai.SetRouteProperties(min_y, pred_time);
        }
    }

    // duel
    bool mEngaging;
    float mCurrentTimer;
    float mFightTime, mFightTimeBnd, mIdleTime, mIdleTimeBnd, mSavedRouteDelta;
    public void Script_SetDuel(bool mEnable, float FightTime, float FightTimeBnd, float IdleTime, float IdleTimeBnd)
    {
        if (mEnable)
            AddAction(ActionFactory.CreateDuelAction(GetIGroupAi(), mpData.nPoints, mpData.Points, FightTime, FightTimeBnd, IdleTime, IdleTimeBnd));
        else
        {
            if (mpCurrentAction != null && mpCurrentAction.GetCode() == 0xC479F7A3) // Duel
                mpCurrentAction.Dead();
            else
            {
                if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
                {
                    AICommon.AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, "unexpected \"Duel\" command");
                }
            }
        }
    }

    //menu
    int mSelected;
    uint[] mPrevSelected = new uint[16];
    void PushSelected(DWORD id) { mPrevSelected[mSelected++] = id; }
    DWORD PopSelected() { if (mSelected == 0) return IVm.THANDLE_INVALID; return mPrevSelected[--mSelected]; }
    void ClearSelected() { mSelected = 0; }
    DWORD CurSelected() { if (mSelected == 0) return IVm.THANDLE_INVALID; return mPrevSelected[mSelected - 1]; }
    DWORD PrevSelected() { if (mSelected < 2) return IVm.THANDLE_INVALID; return mPrevSelected[mSelected - 2]; }

    // ADDING
    AiMenuItem AddRepairBasesItems(ref AiMenuItem item, ref int item_left, IBaseUnitAi ai) //TODO Возможно здесь и в остальных Add* методах правильнее передавать массив Itemов и не как ref
    {
        UpdateLeader();
        Asserts.AssertBp(mpLeader != null);
        mRepairBasesCount = mpMission.GetRepairBase(ref mRepairBases, MAX_REPAIR_BASES_COUNT, mpLeader, ai, false, true, HangarDefs.HANGARS_LAND_ONLY);
        for (int i = 0; i < mRepairBasesCount; ++i)
        {
            if (mRepairBases[i].GetDist(mpLeader.GetOrg()) > PROCEED_BASE_DIST)
            {
                //                RepairBaseInfo[] tmpBases = new RepairBaseInfo[mRepairBasesCount - i];
                //              Array.Copy(mRepairBases, i, tmpBases, 0, mRepairBasesCount - i); //TODO Возможно, можно получать список необработаннх баз проще.
                AddBaseInfo(ref item_left, ref item, mRepairBases[i], AiCommands.Cmd_iRepairBase);
            }
        }
        return item;
    }
    AiMenuItem addDefaultBattleItems(ref AiMenuItem item, ref int item_left, IBaseUnitAi ai)
    {
        bool enabled = (mAliveCount > 1) && isLeader(ai);
        AiMenuAdder.addItemEqual(ref item_left, ref item, AiCommands.Cmd_iEngageAtWish, enabled);
        AiMenuAdder.addItemEqual(ref item_left, ref item, AiCommands.Cmd_iReturnFormation, enabled);
        AiMenuAdder.addItemEqual(ref item_left, ref item, AiCommands.Cmd_iCoverMe, enabled);
        // add required item depend on target side
        iContact cnt;
        if ((cnt = ai.GetTarget()) != null)
            AiMenuAdder.addItemEqual(ref item_left, ref item, cnt.GetSideCode() != mpData.Side ? AiCommands.Cmd_iAttackMyTarget : AiCommands.Cmd_iDefendMyTarget, enabled);
        else
            AiMenuAdder.addItemEqual(ref item_left, ref item, AiCommands.Cmd_iAttackMyTarget, false);
        return item;
    }
    AiMenuItem addDefaultFlightItems(ref AiMenuItem item, ref int item_left, IBaseUnitAi ai)
    {
        RepairBaseInfo[] discard = null;
        int repair_bases_count = (int)mpMission.GetRepairBase(ref discard, 0, mpLeader, ai, false, true, HangarDefs.HANGARS_LAND_ONLY);

        bool can_proceed = mpCurrentAction != null ? mpCurrentAction.IsCanBeBreaked() : true;

        AiMenuAdder.addItemEqual(ref item_left, ref item, AiCommands.Cmd_iProceedBase, can_proceed && repair_bases_count != 0);

        AiMenuAdder.addItemEqual(ref item_left, ref item, AiCommands.Cmd_iRepair, isLandEnabled(ai, true));
        AiMenuAdder.addItemEqual(ref item_left, ref item, AiCommands.Cmd_iLand, isLandEnabled(ai));
        AiMenuAdder.addItemEqual(ref item_left, ref item, AiCommands.Cmd_iJoinFormation, mAliveCount > 1);
        return item;
    }
    AiMenuItem AddPlayerItems(ref AiMenuItem item, ref int item_left, IBaseUnitAi ai)
    {
        AiMenuAdder.addItemEqual(ref item_left, ref item, AiCommands.Cmd_iRepairSelf, isLandEnabled(ai, true));
        AiMenuAdder.addItemEqual(ref item_left, ref item, AiCommands.Cmd_iLandSelf, isLandEnabled(ai));
        return item;
    }

    AiMenuItem AddFormationItems(ref AiMenuItem item, ref int item_left)
    {
        for (int i = 0; i < FormationDefs.FORMATION_COUNT; i++)
            AiMenuAdder.addItemEqual(ref item_left, ref item, FormationDefs.items_formations[i].obj_id, true);
        AiMenuAdder.addItemEqual(ref item_left, ref item, AiCommands.Cmd_iTightFormation, mFormationDist > 1);
        AiMenuAdder.addItemEqual(ref item_left, ref item, AiCommands.Cmd_iLooseFormation, mFormationDist < 5);
        return item;
    }
    const string membersFormat = "%RecipientCallsign% %RecipientIndex%";
    AiMenuItem AddLandRepairMemberItems(ref AiMenuItem item, bool include_self, bool is_leader, ref int item_left)
    {
        if (include_self)
            AiMenuAdder.addItemEqual(ref item_left, ref item, AiCommands.Cmd_iSelf, true);
        if (mAliveCount == 1 || !is_leader) return item;
        for (int j = 0; j < mGhostCount; j++)
        {
            AiUnit ai = mpUnits[j];

            AiMenuItem cur_item = AiMenuAdder.GetCurrentItem(item_left, item);
            if (cur_item != null)
            {
                if (j == 0)
                {
                    AiMenuAdder.addItemEqual(ref item_left, ref item, AiCommands.Cmd_iEntireWing, true);
                    AiMenuAdder.addItemEqual(ref item_left, ref item, AiCommands.Cmd_iAllWingmens, true);
                }
                else
                {
                    cur_item.mMessage.RecipientCallsign = mpData.Callsign;
                    cur_item.mMessage.RecipientIndex = (int)(ai.UnitData().Number + 1);
                    AiMenuAdder.AddItem(ref item_left, ref item, GetMemberId((int)ai.UnitData().Number), membersFormat, ai != null);
                }
            }
        }
        return item;
    }

    #region Глобальные метода из StdCraftGroupAICommands.cpp
    uint GetMemberId(int i)
    {
        switch (i)
        {
            case ENTIRE_WING_CODE: return Cmd_iEntireWing;
            case ENTIRE_WINGMENS_CODE: return Cmd_iAllWingmens;
            case 0: return Cmd_iSelf;
            case 1: return iAlpha_2;
            case 2: return iAlpha_3;
            case 3: return iAlpha_4;
            case 4: return iAlpha_5;
            case 5: return iAlpha_6;
            case 6: return iAlpha_7;
            case 7: return iAlpha_8;
            case 8: return iAlpha_9;
        }
        return IVm.THANDLE_INVALID;
    }

    int GetMemberNum(uint id)
    {
        switch (id)
        {
            case Cmd_iEntireWing: return ENTIRE_WING_CODE;
            case Cmd_iAllWingmens: return ENTIRE_WINGMENS_CODE;
            case Cmd_iSelf: return 0;
            case iAlpha_2: return 1;
            case iAlpha_3: return 2;
            case iAlpha_4: return 3;
            case iAlpha_5: return 4;
            case iAlpha_6: return 5;
            case iAlpha_7: return 6;
            case iAlpha_8: return 7;
            case iAlpha_9: return 8;
        }
        return -1;
    }
    #endregion

    int getFlightMenu(IBaseUnitAi ai, uint id, AiMenuItem start, int max_count)
    {
        AiMenuItem item = start;
        int items_left = max_count;
        switch (id)
        {
            case IVm.THANDLE_INVALID:
                {    // root
                    ClearSelected();
                    item = addDefaultFlightItems(ref item, ref items_left, ai);
                    item = AddPlayerItems(ref item, ref items_left, ai);
                }
                break;
            case Cmd_iProceedBase:
                item = AddRepairBasesItems(ref item, ref items_left, ai);
                break;
            case Cmd_iJoinFormation:
                item = AddFormationItems(ref item, ref items_left);
                break;
            case Cmd_iRepair:
            case Cmd_iLand:
                item = AddLandRepairMemberItems(ref item, (id == Cmd_iRepair) || (id == Cmd_iLand), isLeader(ai), ref items_left);
                break;
            case Cmd_iSelf:
            case Cmd_iEntireWing:
            case Cmd_iAllWingmens:
            case iAlpha_2:
            case iAlpha_3:
            case iAlpha_4:
            case iAlpha_5:
            case iAlpha_6:
            case iAlpha_7:
            case iAlpha_8:
            case iAlpha_9:
                //item=addMemberDependItems(item,PrevSelected(),items_left);
                break;
        }
        //return item - start;
        return 0; //TODO НЕПРАВИЛЬНО! нужно возвратить индекс в массиве AiMenuItemов, так как start и item - указатели
    }
    int getBattleMenu(IBaseUnitAi ai, uint id, AiMenuItem start, int max_count)
    {
        if (id == IVm.THANDLE_INVALID)
        {
            ClearSelected();
            addDefaultBattleItems(ref start, ref max_count, ai); // - start;
            return 0;//TODO Неправильно! Должен возвращаться индекс в массиве.
        }
        else return 0;
    }

    bool isLandEnabled(IBaseUnitAi ai, bool repair = false)
    {
        RepairBaseInfo[] discard = null;
        DWORD bases_count = mpMission.GetRepairBase(ref discard, 0, mpLeader, ai, true, false, HangarDefs.HANGARS_LAND_ONLY);
        if (bases_count > 0)
        {
            StdMissionAi std_miss = (StdMissionAi)mpMission.Query(StdMissionAi.ID);
            return !repair || std_miss != null || std_miss.IsRepairEnabled();
        }
        return false;
    }

    DWORD mRepairBasesCount;
    RepairBaseInfo[] mRepairBases = new RepairBaseInfo[MAX_REPAIR_BASES_COUNT];
    void AddBaseInfo(ref int item_left, ref AiMenuItem item, RepairBaseInfo bs, DWORD cd = IVm.THANDLE_INVALID)
    {
        GROUP_DATA dt = mpMission.GetExistedGroupData(bs.mID);
        AiMenuItem cur_item = AiMenuAdder.GetCurrentItem(item_left, item);
        if (dt != null && cur_item != null)
        {
            cur_item.mMessage.RecipientCallsign = dt.Callsign;
            cur_item.mMessage.Org = bs.mOrg;
            if (cd != IVm.THANDLE_INVALID)
                AiMenuAdder.AddItem(ref item_left, ref item, bs.mID, cd, true);
            else
                AiMenuAdder.addItemEqual(ref item_left, ref item, bs.mID, true);
        }
    }

    float mEscortDelta;
    public void Script_SetEscort(float dlt, List<string> grps)
    {
        mEscortDelta = dlt;
        AddAction(ActionFactory.CreateEscortAction(GetIGroupAi(), grps, mEscortDelta));
    }

    float mPatrolHeight;
    float mPatrolDist;
    public void Script_SetPatrol(Vector3 center, float dist)
    {
        mPatrolHeight = center.y;
        mPatrolDist = dist;
        AddAction(ActionFactory.CreatePatrolAction(GetIGroupAi(), center, dist));
    }

    void ProcessBaseSelect(IBaseUnitAi ai, DWORD prev2_selected_item, DWORD prev_selected_item, DWORD id)
    {
        switch (prev2_selected_item)
        {
            case Cmd_iProceedBase:
                {
                    for (int i = 0; i < mRepairBasesCount; ++i)
                        if (mRepairBases[i].mID == id)
                        {
                            float len = mRepairBases[i].GetDist(mpLeader.GetOrg());
                            if (len > PROCEED_BASE_DIST)
                            {
                                Vector3 org = mRepairBases[i].GetNearOrg(mpLeader.GetOrg(), 100f, PROCEED_BASE_DIST - 300f);
                                float spd = mpLeader.GetMaxSpeed() * 0.8f;
                                if (AICommon.CCmp(spd))
                                {
                                    len = 0f;
                                    spd = 1f;
                                }
                                AddAction(ActionFactory.CreateMoveToAction(GetIGroupAi(), org, len / spd, null));
                            }
                            mRepairBasesCount = 0;
                            break;
                        }
                }
                break;
            default:
                {
                    int num = GetMemberNum(prev_selected_item);
                    if (num == 0)
                        num = (int)ai.GetUnitData().Number;
                    Asserts.AssertBp(num >= 0);
                    StdMissionAi std_miss = (StdMissionAi)mpMission.Query(StdMissionAi.ID);
                    bool respawn = std_miss != null ? std_miss.IsRespawnAfterRepair() : true;
                    bool repair = (prev2_selected_item == Cmd_iRepair);
                    StdGroupAi grp_ai = RepairUnit(id, num, repair, respawn, null, prev_selected_item != Cmd_iSelf);

                    string discard = null;
                    string callsign = mpMission.IsPlayer(ai.GetUnitData(), ref discard);
                    if (num != ai.GetUnitData().Number)
                    {
                        if (num != ENTIRE_WING_CODE && num != ENTIRE_WINGMENS_CODE && grp_ai != null)
                        {
                            ConfirmOrder(CONFIRM_ORDER, 0.1f, true, prev_selected_item);
                            checkHangar(grp_ai.mpData.Callsign, callsign, num + 1);
                        }
                        else if (grp_ai != null)
                        {
                            ConfirmOrder(CONFIRM_ORDER, 0.1f, true);
                            requestLand(grp_ai.mpData.Callsign, callsign, 0);
                        }
                    }
                    else if (grp_ai != null)
                        requestLand(grp_ai.mpData.Callsign, callsign, num + 1);
                    break;
                }
        }
    }
    bool ProcessItem(IBaseUnitAi ai, DWORD prev_selected_item, DWORD id)
    {
        Asserts.AssertBp(IsGroupAlive());
        switch (prev_selected_item)
        {
            case Cmd_iRepair:
            case Cmd_iLand:
                {
                    UpdateLeader();
                    Asserts.AssertBp(mpLeader != null);
                    //RepairBaseInfo rbi;
                    RepairBaseInfo[] rbi =
                    {
                        new RepairBaseInfo()
                    };
                    if (mpMission.GetRepairBase(ref rbi, 1, mpLeader, ai, true, true, (prev_selected_item == Cmd_iRepair) ? HANGARS_LAND_TAKEOFF : HANGARS_LAND_ONLY) != 0)
                    {
                        ProcessBaseSelect(ai, prev_selected_item, id, rbi[0].mID); //TODO Проверить корректность использования указателя на первый элемент массица
                        return true;
                    }
                    break;
                }
        }
        return false;
    }


    void requestLand(string base_callsign, string callsign, int num)
    {
        RadioMessageInfo rmi = new RadioMessageInfo(true, false, true);
        rmi.myWaitTime = 20;
        rmi.mySayIfExceed = false;
        // zzzzzzzzz
        //dprintf("sended REQUEST_IN\n");
        mpEventDesigner.ProcessExternalEvent(GetRequestEventCode(REQUEST_IN), 0, null, rmi, (uint)num, callsign, false, EventType.etBase);
        checkHangar(base_callsign, callsign, num);
    }
    void checkHangar(string base_callsign, string callsign, int num)
    {
        StdGroupAi discard = null;
        if (GetHangarInGroup(Hasher.HshString(base_callsign), null, mpLeader, ref discard, true) == null)
        {
            RadioMessageInfo rmi = new RadioMessageInfo(true, false, true);
            rmi.myWaitTime = 10;
            RadioMessage msg = new RadioMessage();
            msg.RecipientCallsign = callsign;
            msg.RecipientIndex = num;
            mpEventDesigner.ProcessExternalEvent(AIGroupsEvents.INFO_CARRIERSTOP, msg, rmi, false, 0, null, false, EventType.etBase);
        }
    }

    // sets 
    void Cover(DWORD id, IBaseUnitAi ai) { }
    void Attack(DWORD id, IBaseUnitAi ai) { }
    void EngageAtWish(DWORD id, IBaseUnitAi ai) { } 
    void ConfirmOrder(string event_name, float post_time, bool is_critical, uint unit_id = IVm.THANDLE_INVALID)
    {
        if (mpLeaderAi == null && mGhostCount == 1 && IsAiInGroup() == false) return;
        int n = 0;
        int i;
        for (i = 0; i < mGhostCount; ++i)
        {
            AiUnit ai = mpUnits[i];
            if (ai.GetAI() != null) n++;
            if (n == 2)
                break;
        }

        iContact cnt = mpLeaderAi.GetContact();
        for (i = 0; i < mGhostCount; ++i)
        {
            AiUnit ai = mpUnits[i];
            if (ai.GetAI() != null && cnt != null && cnt.GetHandle() != ai.GetHandle())
            {
                RadioMessage rm = new RadioMessage();
                if (unit_id == IVm.THANDLE_INVALID)
                    mpEventDesigner.ProcessExternalEvent(event_name, rm, new RadioMessageInfo(10, post_time, is_critical, false, true), true, mpUnits[i].UnitData().Number + 1, null, true);
                else if (GetMemberNum(unit_id) == mpUnits[i].UnitData().Number)
                {
                    mpEventDesigner.ProcessExternalEvent(event_name, rm, new RadioMessageInfo(10, post_time, is_critical, false, true), true, mpUnits[i].UnitData().Number + 1, null, true);
                    break;
                }
            }

        }
    }
    void processFormationOrder(string id)
    {
        UpdateLeader();
        if (mpLeaderAi != null)
        {
            RadioMessage rm = new RadioMessage();
            rm.RecipientCallsign = mpData.Callsign;
            mpEventDesigner.ProcessExternalEvent(id, rm, new RadioMessageInfo(IsRadioEnvForClients(), false, true), true, mpLeaderAi.GetUnitData().Number + 1, null, true);
        }
    }

    bool CraftDuel(StateFlag flag, float scale) { return false; } 
    // ========================================================================

    public  StdCraftGroupAi()
    {
        myCraftGroupService = new CraftGroupService<StdCraftGroupAi>(this);
        myLastFormation = IVm.THANDLE_INVALID;
    }
    // api
    // common
    public override object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case StdCraftGroupAi.ID: return this;
            case ICraftGroupService.ID: return myCraftGroupService;
            case IMenuService.ID:
                {
                    Debug.Log("myStdMsn:" + myStdMsn);
                    Debug.Log("Menu:" + myStdMsn.getMenu());
                    return myStdMsn.getMenu();
                }
            default: return base.Query(cls_id);
        }
    }
    public override bool Update(float scale)
    {
        bool ret = base.Update(scale);
        if (IsPlayerLeader())
        {
            myTargetsRequestTimer -= scale;
            if (ret && myTargetsRequestTimer < 0)
            {
                checkZeroTargets();
                myTargetsRequestTimer = myRequestTimerO;
            }
        }
        return ret;
    }

    // menu
    public override int GetMenuItems(IBaseUnitAi ai, uint id, AiMenuItem start, int max_count, int page_index)
    {
        switch (page_index)
        {
            case 0: return getFlightMenu(ai, id, start, max_count);
            case 1: return getBattleMenu(ai, id, start, max_count);
        }
        return 0;
    }
    public override int SelectItem(IBaseUnitAi ai, uint id)
    {
        // check for start
        switch (id)
        {
            case Cmd_iReturnFormation:
                gotoFormation(ai, myLastFormation, true);
                ClearSelected();
                ConfirmOrder(CONFIRM_ORDER, 0.1f, true);
                break;
            case Cmd_iCoverMe:
            case Cmd_iDefendMyTarget:
                {
                    iContact cnt = null;
                    string myevent = ORDER_COVER;
                    if (id == Cmd_iDefendMyTarget)
                    { // если это наша цель
                        cnt = ai.GetTarget();
                        if (cnt != null)
                        {
                            if (cnt.GetSideCode() != mpData.Side) // проверяем на сторону
                                cnt = null;
                        }
                        myevent = ORDER_DEFEND;
                    }
                    else
                        cnt = ai.GetContact();
                    //mDefendedContact = new TContact(cnt);
                    mDefendedContact.setPtr(cnt);
                    mpEventDesigner.ProcessInternalEvent(myevent, true, 0, ai.GetUnitData().Number + 1, null, true);
                    if (mDefendedContact.Ptr() != null)
                    { // выставлям контакт который нужно охранять
                        ConfirmOrder(CONFIRM_ORDER, 0.1f, true);
                        //mAttackedContact = null;
                        mAttackedContact.setPtr(null);
                        leaveFormation();
                    }
                    ClearSelected();
                }
                break;
            case Cmd_iAttackMyTarget:
                {
                    // send message        
                    ClearSelected();
                    // attack target
                    //mAttackedContact = new TContact(ai.GetTarget());
                    mAttackedContact.setPtr(ai.GetTarget());
                    mpEventDesigner.ProcessInternalEvent(ORDER_ATTACK, true, 0, ai.GetUnitData().Number + 1, null, true);
                    if (mAttackedContact.Ptr() != null)
                    {
                        if (mAttackedContact.Ptr().GetSideCode() != mpData.Side)
                        {
                            ConfirmOrder(CONFIRM_ORDER, 0.1f, true);
                            mDefendedContact.setPtr(null);
                            // attack target
                            leaveFormation();
                        }
                        else
                            mAttackedContact.setPtr( null);
                    }
                }
                break;
            case Cmd_iEngageAtWish:
                {
                    // send message
                    mpEventDesigner.ProcessInternalEvent(ORDER_ENGAGE, true, 0, ai.GetUnitData().Number + 1, null, true);
                    ClearSelected();
                    ConfirmOrder(CONFIRM_ORDER, 0.1f, true);
                    mDefendedContact.setPtr(null);
                    mAttackedContact.setPtr(null);
                    // attack target
                    leaveFormation();
                }
                break;
            case Cmd_iRepairSelf:
                ProcessItem(ai, Cmd_iRepair, Cmd_iSelf);
                break;
            case Cmd_iLandSelf:
                ProcessItem(ai, Cmd_iLand, isLeader(ai) ? Cmd_iEntireWing : Cmd_iSelf);
                break;
            case Cmd_iJoinFormation:
            case Cmd_iProceedBase:
            case Cmd_iRepair:
            case Cmd_iLand:
                PushSelected(id);
                break;
            case Cmd_iTightFormation:
                UpdateAlive();
                if (SetFormation(mCurrentFormation, mFormationDist - 1f, "menu", true, false))
                    processFormationOrder(TIGHT_FORMATION);

                ClearSelected();
                ConfirmOrder(CONFIRM_ORDER, 0.1f, true);
                break;
            case Cmd_iLooseFormation:
                UpdateAlive();
                if (SetFormation(mCurrentFormation, mFormationDist + 1f, "menu", true, false))
                    processFormationOrder(LOOSE_FORMATION);
                ClearSelected();
                ConfirmOrder(CONFIRM_ORDER, 0.1f, true);
                break;
            case iFormationLine:
            case iFormationColoumn:
            case iFormationWedge:
            case iFormationVee:
            case iFormationEchelonRight:
            case iFormationEchelonLeft:
                gotoFormation(ai, id, false);
                ClearSelected();
                ConfirmOrder(CONFIRM_ORDER, 0.1f, true);
                break;
            case Cmd_iSelf:
            case Cmd_iEntireWing:
            case Cmd_iAllWingmens:
            case iAlpha_2:
            case iAlpha_3:
            case iAlpha_4:
            case iAlpha_5:
            case iAlpha_6:
            case iAlpha_7:
            case iAlpha_8:
            case iAlpha_9:
                if (!ProcessItem(ai, CurSelected(), id))
                    ConfirmOrder(CONFIRM_ORDER, 0.1f, true, id);
                ClearSelected();
                break;
            default:
                {
                    bool process_base = false;
                    for (int i = 0; i < mRepairBasesCount; ++i)
                    {
                        if (mRepairBases[i].mID == id)
                        {
                            process_base = true;
                            break;
                        }
                    }
                    if (process_base)
                    {
                        ProcessBaseSelect(ai, CurSelected(), 0xFFFFFFFF, id);
                        ClearSelected();
                    }
                    break;
                }
        }
        return (int)CurSelected();
    }

    static string NotAirUnit = "create : not air unit in air group \"{0}\"";
    // StdGroupAi
    public override void OnCreateUnit(IBaseUnitAi ai)
    {
        base.OnCreateUnit(ai);
        if (ai.Query(IVehicleAi.ID) != null)
            throw new SystemException(string.Format(NotAirUnit, mpData.Callsign));
    }

    public override void LeaderHumanityChanged(bool player_changed)
    {
        base.LeaderHumanityChanged(player_changed);
    }

    // on StdDynamicGroupAi
    public override void SetInterface(GROUP_DATA data, IMissionAi miss_ai)
    {
        // menu
        mSelected = 0;
        mRepairBasesCount = 0;

        mEscortDelta = 100f;
        mPatrolDist = 2000f;
        mPatrolHeight = 500f;
        iUnifiedVariableContainer cnt = stdlogic_dll.mpAiData.openContainer("GroupProperties");
        myRequestTimerO = cnt != null ? cnt.getFloat("RequestTargetTime") : 20f;
        myTargetsRequestTimer = myRequestTimerO;

        base.SetInterface(data, miss_ai);

        mIncludeSubobjects = INCLUDE_OBJECTS_AND_SUBOBJECTS;

        myStdCraftGroupFactory = Factories.createStdCraftGroupFactory(getIQuery(), myStdDynGroupFactory);
    }
    public override string GetRequestEventCode(DWORD index)
    {
        {
            switch (index)
            {
                case REQUEST_IN: return REQUEST_LAND;
                case REQUEST_OUT: return REQUEST_TAKEOFF;
                case REQUEST_FAIL: return FAIL_LAND;
                case CLEAR_IN: return CLEAR_LAND;
                case CLEAR_OUT: return CLEAR_TAKEOFF;
                default: return EMPTY_EVENT;
            }
        }
    }
    public override void WorkContactIsLost()
    {
        if (IsPlayerLeader())
            keepInFormation();
    }

    public override void OnStartAppear(iContact hng)
    {
        base.OnStartAppear(hng);
    }

    public override bool SetGroupAppear(string base_name, bool scramble, bool process_appear)
    {
        bool ret = base.SetGroupAppear(base_name, scramble, process_appear);
        if (ret)
        {
            if (mTakeOffGroup != IVm.THANDLE_INVALID && mTakeOffScramble == false && process_appear)
                AddAction(ActionFactory.CreateCraftTakeoffAction(GetIGroupAi()));
        }
        return ret;
    }

    // park
    public override bool OnPark(string base_name, DWORD ultimate)
    {
        return AddAction(ActionFactory.CreateCraftParkAction(GetIGroupAi(), base_name, ultimate));
    }
    public override string ParkGroup(DWORD base_id, bool repair, bool respawn)
    {
        string call = base.ParkGroup(base_id, repair, respawn);
        if (call != null)
            requestLand(call, mpData.Callsign, 0);
        return call;
    }


    // new
    CraftGroupService<StdCraftGroupAi> myCraftGroupService;
    IVmFactory myStdCraftGroupFactory;

    public override IVmFactory getTopFactory() { return myStdCraftGroupFactory; }
    private void gotoFormation(IBaseUnitAi ai, crc32 id, bool return_to_formation)
    {
        myLastFormation = id;
        UpdateAlive();
        string nm = GetFormationNameFromId(id);
        if (mpLeaderAi != null && SetFormation(id, mFormationDist, nm, true, !return_to_formation))
        {
            if (return_to_formation)
            {
                keepInFormation();
                mDefendedContact.setPtr(null);
                mAttackedContact.setPtr(null);
            }
            if (return_to_formation)
                processFormationOrder(DONE_JOINF);
        }
    }
    private crc32 myLastFormation;

    private void checkZeroTargets()
    {
        if (mpLeaderAi != null)
        {
            iContact tgt = mpLeaderAi.GetTarget();
            iContact ldr = mpLeaderAi.GetContact();
            if (tgt != null && ldr != null && myInFormation && tgt.GetSideCode() != ldr.GetSideCode())
                ConfirmOrder(REQUEST_TARGETS, 0.1f, false);
        }
    }
    private float myTargetsRequestTimer;
    private float myRequestTimerO;

    private bool myInFormation = false;
    private void keepInFormation()
    {
        mRouteDelta = -1;
        myInFormation = true;
    }

    private void leaveFormation()
    {
        mRouteDelta = 100000f;
        myInFormation = false;
    }
};


public class CraftGroupService<T> : ICraftGroupService where T:StdCraftGroupAi
{
    public void avoidTerrain(float alt, float time)
    {
        myMsn.SetRouteProperties(alt, time);
    }

    public virtual void escort(float delta, List<string> groups)
    {
        myMsn.Script_SetEscort(delta, groups);
    }

    public virtual void patrol(Vector3 center, float dist)
    {
        myMsn.Script_SetPatrol(center, dist);
    }

    public virtual void duel(bool mEnable, float FightTime, float FightTimeBnd, float IdleTime, float IdleTimeBnd)
    {
        myMsn.Script_SetDuel(mEnable, FightTime, FightTimeBnd, IdleTime, IdleTimeBnd);
    }

    public CraftGroupService(T imp) {
        myMsn = imp;
    }
    T myMsn;
};