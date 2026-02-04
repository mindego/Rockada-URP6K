using DWORD = System.UInt32;
using crc32 = System.UInt32;
using UnityEngine;

public class StdDynamicGroupAi : StdGroupAi
{
    new public const crc32 ID = 0x5A4009B9;

    const string sColoumnName = "Coloumn";
    public StdDynamicGroupAi()
    {
        myDynGroupService = new DynGroupService<StdDynamicGroupAi>(this);
        myExecutionContext = new PointExecutionContext<StdDynamicGroupAi>(this);
    }

    // takeoff
    float mMaxUnitRadius;

    // route
    float mTimeToPoint;
    protected float mRouteDelta;
    Vector3 mLastLeaderOrg;
    bool myAutoReformat = false;

    // speed
    float myLastSpeed;
    public void SetSpeed(float spd, IBaseUnitAi ai = null)
    {
        myLastSpeed = spd;
        if (mpLeaderAi != null)
            ai = mpLeaderAi;
        if (ai != null)
        {
            IUnitAi cr_ai = (IUnitAi)ai.Query(IUnitAi.ID);
            if (cr_ai != null)
                cr_ai.setSpeed(spd);
        }
        else
            LeaderMissed("SetSpeed");
    }



    // messages
    float mCheckRadioEnvTimer;

    // alerted
    // formation
    FormationInfo mpFormation;
    protected float mFormationDist;
    protected DWORD mCurrentFormation;
    DWORD mSavedFormation;
    float mSavedFormDist;
    float mSavedRouteDelta;

    float mDistTimer;


    Vector3 GetFormationPos(int n)
    {
        if (mpFormation != null)
            return mpFormation.Delta[n] * mMaxUnitRadius * 2f * mFormationDist;
        else
        {
            MATRIX leader = new MATRIX();
            leader.Up = Vector3.up;
            leader.Dir = Vector3.forward;
            leader.Right = Vector3.right;
            leader.Org = mpData.Units[0].Org;
            leader.TurnRightPrec(mpData.Units[0].Angle);
            return leader.ExpressPoint(mpData.Units[n].Org);
        }
    }

    internal int GetMarkersCount()
    {
        return mpMission.GetMarkersCount();
    }

    void ResetFormation()
    {
        float d = 0;
        bool set_angle = false;
        Vector3 lDir = Vector3.forward;
        Vector3 lRight = Vector3.right;

        if (mpData.nPoints > 0)
        {
            lDir = mpData.Points[0].Org - mpData.Units[0].Org;
            lDir.y = 0;
            d = lDir.magnitude;
            if (d == 0)
            {
                lDir = Vector3.forward;
                d = 1f;
            }
            else
                lDir /= d;

            d = Mathf.Acos(lDir.z);
            if (lDir.x < 0) d = 2.0f * Storm.Math.PI - d;
            set_angle = true;

            lRight.Set(lDir.z, 0, -lDir.x);
        }
        int k = 0;
        if (mpFormation != null)
        {
            set_angle = true;
            d = mpData.Units[0].Angle;
        }
        bool set_orgs = true;
        if (mpData.nPoints == 0 && mpFormation == null)
            set_orgs = false;
        else
        {
            lDir.Set(Mathf.Sin(mpData.Units[0].Angle), 0, Mathf.Cos(mpData.Units[0].Angle)); 
            lRight.Set(lDir.z, 0, -lDir.x);
        }

        for (int i = 0; i < mGhostCount; ++i)
        {
            AiUnit ai = mpUnits[i];
            if (!ai.UnitData().IsDestroyed())
            {
                if (set_orgs)
                {
                    Vector3 dst = GetFormationPos(k);
                    ai.SetOrg(mpData.Units[0].Org +
                      lDir * dst.z +
                      lRight * dst.x +
                      Vector3.up * dst.y);
                }
                if (set_angle)
                    ai.UnitData().Angle = d;
                k++;
            }
        }
    }
    JoinOption GetJoinOption(Vector3 org1, Vector3 org2, float radius)
    {
        if (radius < 0.1f) return JoinOption.joImmediately;
        Vector3 md = org1 - org2;
        float d = md.x * md.x + md.z * md.z;
        JoinOption jo;
        if (d > Mathf.Pow(1.1f * radius, 2))
            jo = JoinOption.joImmediately;
        else
        {
            if (d > Mathf.Pow(0.9f * radius, 2))
                jo = JoinOption.joTurn;
            else
                jo = JoinOption.joFreeFly;
        }
        return jo;
    }
    public bool SetFormation(uint code, float dist, string name, bool from_menu, bool notify)
    {
        if (code == 0x85007DB7)
        { // Column
            name = sColoumnName;
            code = Hasher.HshString(name);
        }
        FormationInfo info = null;
        bool ret = false;
        if (code != Constants.THANDLE_INVALID)
            info = mpGame.GetFormationInfo(code);
        if (info != null)
        {
            if (IsPlayerLeader() && !from_menu)
            {
                mSavedFormation = code;
                mSavedFormDist = dist;
            }
            else
            {
                mpFormation = info;
                needToChangeFormation();
                if (from_menu || dist != -1)
                    mFormationDist = dist;
                mCurrentFormation = code;
            }
            if (notify && ((IsPlayerLeader() && from_menu) || IsPlayerInGroup()))
            {
                UpdateLeader();
                if (mpLeaderAi != null)
                {
                    RadioMessage rm = new RadioMessage();
                    rm.String1 = name;
                    rm.RecipientCallsign = mpData.Callsign;
                    mpEventDesigner.ProcessExternalEvent(AIGroupsEvents.ORDER_FORMATION, rm, new RadioMessageInfo(IsRadioEnvForClients(), false, true), true, mpLeaderAi.GetUnitData().Number + 1, null, true);
                }
            }
            ret = true;
        }
        else
        {
            if (Constants.THANDLE_INVALID == code)
            {
                mpFormation = null;
                needToChangeFormation();
                mCurrentFormation = code;
                ret = true;
            }
            else
            {
                if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
                {
                    Parsing.AiMessage(MessageCodes.MSG_ERROR, "Error", "requested formation \"{0}\" not found", name);
                }
                ret = false;
            }
        }
        if (!IsGroupAlive() && Constants.THANDLE_INVALID == mTakeOffGroup)
            ResetFormation();
        return ret;
    }
    bool CheckMemberDist(iContact leader_un, bool defined, Vector3 org) { return false; } //TODO Возможно, как-то всё-таки используется.

    // points
    void CheckPositionForReach(Vector3 v) { }
    float mReachRadius;
    void ExecutePoint(string script)
    {
        ClearAllActions();
        if (!executeScript(mpData.Callsign, script, null)) return;
    }
    void CheckRouteReach(float scale) { }

    // return base
    bool mKillAll;
    public bool Script_SetPark(string base_name, DWORD ultimate)
    {
        return OnPark(base_name, ultimate);
    }
    public bool Script_SetReturnToBase(DWORD baseId, float dist, DWORD ultimate, string base_name)
    {
        Debug.Log("RTB for " + GetIGroupAi().GetGroupData().Callsign);
        if (mpLeader == null)
        {
            LeaderMissed("SetReturnToBase");
            return false;
        }
        StdGroupAi grp = null;
        GetHangarInGroup(baseId, null, mpLeader, ref grp, false);
        if (grp == null)
        {
            Debug.Log(string.Format("Can not find hangar for {0} in {1} ({2})", GetIGroupAi().GetGroupData().Callsign, base_name,baseId.ToString("X8")));
            return false;
        }

        if (dist < 500f) dist = 500f;
        if (dist > 3000f) dist = 3000f;

        Vector3 org;
        iContact leader = grp.GetLeaderContact();
        if (leader != null)
            org = leader.GetOrg();
        else
            org = grp.GetGroupData().Units[0].Org;

        Vector3 selforg = mpLeader.GetOrg();
        Vector3 delta = org - selforg;
        delta.y = 0f;
        float d = delta.magnitude;
        delta /= d;

        Vector3 land_pos;
        float land_time;
        if (d > dist)
        {
            land_pos = selforg + delta * (d - dist);
            float spd = mpLeader.GetMaxSpeed() * 0.75f;
            if (AICommon.CCmp(spd))
            {
                spd = 1f;
                d = 0f;
            }
            land_time = stdlogic_dll.mCurrentTime + d / spd;
        }
        else
        {
            land_pos = selforg;
            land_time = 0f;
        }
        //char buffer[256];
        //AssertBp(StrLen(base_name) < 256);
        //wsprintf(buffer, "Park(Base=\"%s\",Ultimate=%d);", base_name, ultimate);
        string buffer = string.Format("Park(Base=\"{0}\",Ultimate={1});", base_name, ultimate);
        //string buffer = string.Format("Park(Base={0},Ultimate={1});", base_name, ultimate);
        AddAction(ActionFactory.CreateMoveToAction(GetIGroupAi(), land_pos, land_time, buffer));
        return true;
    }
    public void Script_SetResume(bool break_only_pause)
    {
        bool error = false;
        if (mpCurrentAction != null)
        {
            if (break_only_pause && mpCurrentAction.GetCode() != 0xE9A44216) // "Pause"
                error = true;
            if (error == false)
                mpCurrentAction.Dead();
        }
        else
            error = true;
        if (error && AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
        {
            Parsing.AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, "unexpected \"%s\" command", break_only_pause ? "Resume" : "Break");
        }
    }
    public void Script_SetRouteTo(Vector3 org, float time, bool clear_all)
    {
        if (clear_all)
            ClearAllActions();
        AddAction(ActionFactory.CreateRouteToAction(GetIGroupAi(), org, time, null));
    }

    // route
    public void Script_SetPause(float time)
    {
        if (mpLeader == null)
        {
            LeaderMissed("Pause");
            return;
        }
        AddAction(ActionFactory.CreatePauseAction(GetIGroupAi(), time, mpLeader.GetOrg()));
    }
    public void Script_SetSwitch(int num)
    {
        if (mpLeader == null)
        {
            LeaderMissed("Switch");
            return;
        }
        iAction first_switched = null;
        for (int i = mActions.Count - 1; i >= 0; --i)
        {
            iAction cur = mActions[i];
            if (cur.IsSwitching() == false)
                cur.Dead();
            else if (first_switched == null)
                first_switched = cur;
        }
        if (first_switched != null)
            first_switched.Switch((uint)num, mpLeader.GetMaxSpeed());
    }
    public void SetKillAll(bool flag)
    {
        mKillAll = flag;
    }

    public StdGroupAi RepairUnit(DWORD base_id, int num, bool repair_only, bool respawn_from_time, iContact hangar_body, bool call_from_menu = false)
    {
        if (hangar_body != null)
            hangar_body = null;

        StdGroupAi grp_ai = null;

        DWORD leader_num = Constants.THANDLE_INVALID;
        int i;
        for (i = 0; i < mGhostCount; ++i)
        {
            AiUnit un = mpUnits[i];
            if (un != null)
            {
                leader_num = (uint)i;
                break;
            }
        }

        for (i = mGhostCount - 1; i >= 0; i--)
        {
            AiUnit un = mpUnits[i];
            if (un == null) continue;
            iContact cur_cnt = un.GetAI().GetContact();
            if (cur_cnt == null) continue;
            bool process = false;
            if (num == AiCommands.ENTIRE_WING_CODE) process = true;
            else if (un.UnitData().Number == num) process = true;
            else if (num == AiCommands.ENTIRE_WINGMENS_CODE && i != leader_num) process = true;
            if (process)
            {
                if (call_from_menu && leader_num != i && repair_only && !checkReapiriness(cur_cnt))
                {
                    RadioMessageInfo rmi = new RadioMessageInfo(true, false, true);
                    rmi.myWaitTime = 10;
                    mpEventDesigner.ProcessExternalEvent(AIGroupsEvents.NONEEDED_REPAIR, 0, null, rmi, un.UnitData().Number + 1, mpData.Callsign, false);
                    continue;
                }

                //Ересь подозреваю %( Скорее всего поиск АИ ангара должен деаться не так.
                IBaseUnitAi hangar_ai = GetHangarInGroup(base_id, null, cur_cnt, ref grp_ai, false);
                if (hangar_ai == null) return null;

                if (hangar_body != null)
                    hangar_body = hangar_ai.GetContact();
                IHangarAi hng_ai = (IHangarAi)hangar_ai.Query(IHangarAi.ID);

                hng_ai.RequestToLand(GetIGroupAi(), un.GetAI(), repair_only);
                if (repair_only)
                {
                    RespawnInfo info = new RespawnInfo();
                    info.SideCode = mpData.Side;
                    info.mBase = base_id;
                    info.ObjectName = un.UnitData().CodedName;
                    info.mOrg = un.UnitData().Org;
                    info.Layout1Name = un.UnitData().Layout1;
                    info.Layout2Name = un.UnitData().Layout2;
                    info.Layout3Name = un.UnitData().Layout3;
                    info.Layout4Name = un.UnitData().Layout4;
                    if (respawn_from_time)
                    {
                        info.mRespawnTime = 1f;
                        base.RespawnUnitAfterDeath(un.UnitData(), RespawnType.rtRespawnFromTime, info, OverwriteType.otOverwriteRespawnInfo);
                    }
                    else
                        base.RespawnUnitAfterDeath(un.UnitData(), RespawnType.rtRespawnNever, null, OverwriteType.otOverwriteNone);
                }
                else
                    base.DeleteUnitAfterDeath(un.UnitData());
            }
        }
        return grp_ai;
    }

    // actions support
    public void SetPauseToUnit(AiUnit ai, bool pause)
    {
        IUnitAi un = (IUnitAi)ai.GetAI().Query(IUnitAi.ID);
        if (un != null) un.Pause(pause);
    }

    public void SetDestinationToUnit(AiUnit ai, Vector3 org, float time)
    {
        IUnitAi un = (IUnitAi)ai.GetAI().Query(IUnitAi.ID);
        if (un != null)
        {
            un.SetDestination(org, time);
        }
    }
    public void SetFormationToUnit(AiUnit ai, DWORD n, iContact leader = null, float dlt = 0)
    {
        Vector3 Delta = GetFormationPos((int)n);
        Delta.y += dlt;
        IUnitAi un = (IUnitAi)ai.GetAI().Query(IUnitAi.ID);
        if (leader == null)
            leader = mpLeader;
        if (leader != null && un != null)
        {
            un.setFormation(leader, Delta, n * mFormationDist * mMaxUnitRadius, mCurrentFormation);
        }
    }

    const float CHECK_ROUTE_DELTA_TIME = 3f;
    public float CheckRouteDelta(Vector3 def, out DWORD leader_near, float delta = -1f)
    {
        if (mpLeader == null)
        {
            leader_near = 1;
            return CHECK_ROUTE_DELTA_TIME;
        }
        float route_delta = (delta < 0f) ? mRouteDelta : delta;
        bool free_fly = mKillAll && !(route_delta <= 0f);
        JoinOption lead_jo;
        if (free_fly)
            lead_jo = JoinOption.joFreeFly;
        else
            lead_jo = GetJoinOption(mpLeader.GetOrg(), def, route_delta);

        leader_near = (lead_jo == JoinOption.joFreeFly) ? (uint)1 : 0;
        ((IUnitAi)mpLeaderAi.Query(IUnitAi.ID)).JoinFormation(lead_jo);
        for (int i = 0; i < GetGhostCount(); i++)
        {
            AiUnit ai = GetAiUnit((uint)i);
            if (ai.GetAI() != null && ai.GetAI() != mpLeaderAi)
            {
                iContact un = ai.GetAI().GetContact();
                if (un != null)
                {
                    float tst;
                    if (AICommon.CCmp(route_delta) && mAttackedContact.Ptr() == null && mDefendedContact.Ptr() == null)
                        tst = -1f;
                    else
                        tst = route_delta;
                    ((IUnitAi)ai.GetAI().Query(IUnitAi.ID)).JoinFormation(GetJoinOption(mpLeader.GetOrg(), un.GetOrg(), tst));
                }
            }
        }
        return CHECK_ROUTE_DELTA_TIME;
    }
    public void SetRouteDelta(float dlt)
    {
        if (mPlayerIsLeader)
            mSavedRouteDelta = dlt;
        else
            mRouteDelta = dlt;
    }
    public float GetRouteDelta() { return mRouteDelta; }

    // ===================== api ============================
    // IAi
    public override object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case StdDynamicGroupAi.ID: return this;
            case IDynGroupService.ID: return myDynGroupService;
            case IPointExecutionContext.ID: return myExecutionContext;
            default: return base.Query(cls_id);
        }
    }
    public override bool Update(float scale)
    {
        mCheckRadioEnvTimer -= scale;
        bool ret = base.Update(scale);
        if (mpLeader != null)
            mLastLeaderOrg = mpLeader.GetOrg();
        return ret;
    }

    const string NotDynamicUnit = "create : not dynamic unit in dynamic group \"{0}\"";
    // IGroupAi
    public override void OnCreateUnit(IBaseUnitAi ai)
    {
        base.OnCreateUnit(ai);
        if (ai.Query(IUnitAi.ID) == null)
            throw new System.Exception(string.Format(NotDynamicUnit, mpData.Callsign + " " + ai));
        iContact cnt = ai.GetContact();
        float loc = cnt.GetRadius();
        if (loc > mMaxUnitRadius)
            mMaxUnitRadius = loc;
        if (myLastSpeed >= 0)
            SetSpeed(myLastSpeed, ai);
    }

    // StdGroupAi
    public override void LeaderHumanityChanged(bool player_changed)
    {
        if (player_changed)
        {
            if (IsPlayerLeader())
            {
                mSavedRouteDelta = mRouteDelta;            // запоминаем старые значения 
                mSavedFormation = mCurrentFormation;
                mSavedFormDist = mFormationDist;
                mRouteDelta = 100000f;
            }
            else
            {
                SetFormation(mSavedFormation, mSavedFormDist, "saved formation", false, false);  // восстанавливаем старые или заданные
                mRouteDelta = mSavedRouteDelta;
            }
        }
        base.LeaderHumanityChanged(player_changed);
    }

    // StdDynamicGroupAi
    public override void SetInterface(GROUP_DATA data, IMissionAi miss_ai)
    {
        mMaxUnitRadius = 15f;

        mCurrentFormation = Constants.THANDLE_INVALID;

        mReachRadius = 100f;
        mRouteDelta = 20000f;

        // return
        mKillAll = false;

        mpFormation = null;
        mFormationDist = 3f;
        mDistTimer = -1f;
        mCheckRadioEnvTimer = -1f;

        myLastSpeed = -1;

        mSavedFormation = mCurrentFormation;
        mSavedFormDist = mFormationDist;
        mSavedRouteDelta = mRouteDelta;

        Asserts.AssertBp(data != null);
        base.SetInterface(data, miss_ai);
        myStdDynGroupFactory = Factories.createStdDynGroupFactory(getIQuery(), myStdGroupFactory);
    }
    public override void OnUnitLost(AiUnit ai, bool killed)
    {
        if (IsGroupAlive() && IsRadioEnvForClients() == false && killed)
            mpEventDesigner.ProcessExternalEvent(AIGroupsEvents.LOST_UNIT, 0, null, new RadioMessageInfo(false, true, true), 0);
        base.OnUnitLost(ai, killed);
    }
    public override void OnChangeSide(DWORD new_side)
    {
        UnitWasDeleted();
        base.OnChangeSide(new_side);
    }

    // actions 
    public virtual bool IsPointReached(Vector3 org, Vector3 prev_org, bool prev_exists)
    {
        bool local_reached = false;
        bool executed = false;
        float rad = Mathf.Pow(mReachRadius, 2);
        Vector3 leader_org = mpLeader.GetOrg();
        Vector3 Pr2 = org - leader_org;
        Pr2.y = 0;

        if (prev_exists)
        {
            Vector3 RouteDelta = org - prev_org;
            RouteDelta.y = 0f;
            float d = Vector3.Dot(RouteDelta, Pr2);
            if (d <= 0)
                local_reached = true;
        }

        float t = Pr2.sqrMagnitude;
        if (!local_reached)
        {
            if (t < rad)
                local_reached = true;
        }

        if (local_reached)
        {
            if (mKillAll)
            {
                if (mCurrentEnemyCount.mRealEnemyCount == 0)
                    executed = true;
            }
            else
                executed = true;
        }
        return executed;
    }
    public virtual bool ProcessReaches()
    {
        Vector3 leader_org = mpLeader.GetOrg();
        if (IsPlayerInGroup())
        {
            for (int i = 0; i < mGhostCount; ++i)
            {
                AiUnit ai = mpUnits[i];
                if (ai != null)
                {
                    iContact cnt = ai.GetAI().GetContact();
                    if (cnt != null && (cnt.IsPlayedByHuman() || cnt == mpLeader))
                        CheckPositionForReach(cnt.GetOrg());
                }
            }
        }
        else
            CheckPositionForReach(leader_org);
        return true;
    }

    public override float ScanEnemies(Vector3 center, float radius)
    {
        float ret = base.ScanEnemies(center, radius);
        if (IsRadioEnvForClients() == false && mAttackRadius > 0.1f)
        {
            if (SectorIsClear())
                mpEventDesigner.ProcessExternalEvent(AIGroupsEvents.CLEAR_FIGHT, 0, null, new RadioMessageInfo(false, true, true), 0);
            else if (SectorIsDirty())
                mpEventDesigner.ProcessExternalEvent(AIGroupsEvents.ENTER_FIGHT, 0, null, new RadioMessageInfo(false, true, true), 0);
        }
        if (mCheckRadioEnvTimer < 0f)
        {
            if (IsAiInGroup() && IsRadioEnvForClients() && mpEventDesigner.CanProcessInternalEvents())
            {
                string event_name = null;
                DWORD notify_index = Constants.THANDLE_INVALID;
                for (int i = 0; i < mGhostCount; ++i)
                {
                    AiUnit ai = mpUnits[i];
                    //if (ai != null)
                    if (ai.GetAI() != null)
                    {
                        iContact cnt = ai.GetAI().GetContact();
                        if (cnt != null)
                        {
                            iContact thr = cnt.GetThreat();
                            if (thr != null && thr.GetSideCode() != mpData.Side && cnt.GetThreatF() > 1f)
                            { // shooting
                                Vector3 delta = cnt.GetOrg() - thr.GetOrg();
                                float len = delta.magnitude;
                                if (len <= 800f && len >= 1f)
                                {   // near
                                    delta /= len;
                                    float angle1 = Vector3.Dot(delta, thr.GetDir());
                                    float angle2 = Vector3.Dot(-delta, cnt.GetDir());
                                    if (angle1 > 0.86 && angle2 < 0f)
                                    { // on six
                                        if (event_name == null || (event_name == AIGroupsEvents.WARN_PLAYER && AICommon.Prb(0.5f)))
                                        {  // already notified
                                            event_name = cnt.IsPlayedByHuman() ? AIGroupsEvents.WARN_PLAYER : AIGroupsEvents.REQUEST_HELP;
                                            notify_index = ai.UnitData().Number;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (event_name != null)
                {
                    DWORD caller_index = Constants.THANDLE_INVALID;
                    for (int i = 0; i < mGhostCount; ++i)
                    {
                        AiUnit ai = mpUnits[i];
                        if (ai != null)
                        {
                            if (ai.UnitData().Number != notify_index)
                            {
                                iContact cnt = ai.GetAI().GetContact();
                                if (cnt != null && cnt.IsPlayedByHuman() == false)
                                {
                                    caller_index = ai.UnitData().Number;
                                    break;
                                }
                            }
                        }
                    }
                    if (caller_index != Constants.THANDLE_INVALID)
                    {
                        if (event_name != AIGroupsEvents.WARN_PLAYER)
                            mpEventDesigner.ProcessExternalEvent(event_name, 0, null, new RadioMessageInfo(false, true, true), 0, null, false, EventType.etInternal);
                        else
                            mpEventDesigner.ProcessExternalEvent(event_name, 0, null, new RadioMessageInfo(false, true, true), 0, null, false, EventType.etInternal);
                    }
                }
            }
            mCheckRadioEnvTimer = RandomGenerator.Rnd(3f, 2f);
        }
        return ret;
    }

    public override void OnGroupCreate()
    {
        AddAction(ActionFactory.CreateMoveAction(GetIGroupAi(), mpData.nPoints, mpData.Points));
    }
    public override void OnActionDeath() { }
    public override void OnActionDeactivate()
    {
        Asserts.AssertBp(mpLeader != null);
        if (mActionsCandidates.Count == 0)
            AddAction(ActionFactory.CreatePauseAction(GetIGroupAi(), 0f, mpLeader.GetOrg()));
    }
    public override void OnEmptyActions()
    {
        if (mActionsCandidates.Count == 0)
        {
            Vector3 org = mpLeader != null ? mpLeader.GetOrg() : mLastLeaderOrg + Distr.Sphere() * 700f;
            AddAction(ActionFactory.CreatePauseAction(GetIGroupAi(), 0f, org));
        }
    }
    public override void OnStartAppear(iContact hng)
    {
        base.OnStartAppear(hng);
    }
    public override void OnFinishAppear()
    {
        base.OnFinishAppear();
    }

    public override void SetGroupDisappear(bool base_ret, DWORD baseId, float dist, DWORD ultimate, string base_name)
    {
        Debug.Log("Moving to disappear @ base " + base_name);
        if (base_ret)
        {
            if (!Script_SetReturnToBase(baseId, dist, ultimate, base_name))
            {
                if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
                {
                    AICommon.AiMessage(MessageCodes.MSG_ERROR, "Error", "requested park base \"{0}\" not found", base_name);
                }
            }
            else if (IsRadioEnvForClients() == false && IsGroupAlive())
                mpEventDesigner.ProcessExternalEvent(AIGroupsEvents.RETURN_BASE, 0, null, new RadioMessageInfo(false, true, true), 0);
        }
        else
        {
            //mpEventDesigner.SetInternalProperties(false, false, true);  // ?????????????
            mpEventDesigner.SetInternalProperties(0, 0, 0);  // ?????????????
            StdMissionAi miss = (StdMissionAi)mpMission.Query(StdMissionAi.ID);
            if (miss != null)
                miss.DeleteGroupUnits(GetIGroupAi(), 0, null, ultimate == DisappearCodes.DISAPPEAR_EXPLODE, true);
            if (ultimate != DisappearCodes.DISAPPEAR_DEATH)
            {
                for (int i = 0; i < mGhostCount; ++i)
                    RespawnUnitAfterDeath(mpUnits[i].UnitData(), RespawnType.rtRespawnNever, null, OverwriteType.otOverwriteNone);
            }
            mUpdateAlive = true;
        }
    }

    public virtual void OnAlert(Vector3 org)
    {
        if (mpCurrentAction != null && mpCurrentAction.GetCode() == 0x29C3963A)
        { // Alert
            AlertAction al = (AlertAction)mpCurrentAction.Query(AlertAction.ID);
            if (al != null)
                al.UpdateAlert(org);
        }
        else
            AddAction(ActionFactory.CreateAlertAction(GetIGroupAi(), org));
    }
    // park 
    public virtual bool OnPark(string base_name, DWORD ultimate) { return false; }
    public virtual string ParkGroup(DWORD baseId, bool repair, bool respawn)
    {
        if (mpLeader == null)
        {
            LeaderMissed("SetParking");
            return null;
        }

        Asserts.AssertBp(IsGroupAlive());
        StdGroupAi grp_ai = null;
        iContact hng = null;
        grp_ai = RepairUnit(baseId, AiCommands.ENTIRE_WING_CODE, repair, respawn, hng);
        return grp_ai != null ? grp_ai.mpData.Callsign : null;
    }
    public virtual string ParkUnit(DWORD baseId, DWORD number, bool repair, bool respawn)
    {
        StdGroupAi grp_ai = null;
        iContact hng = null;
        grp_ai = RepairUnit(baseId, (int)number, repair, respawn, hng);
        return grp_ai != null ? grp_ai.mpData.Callsign : null;
    }

    // new
    DynGroupService<StdDynamicGroupAi> myDynGroupService;
    protected IVmFactory myStdDynGroupFactory;
    PointExecutionContext<StdDynamicGroupAi> myExecutionContext;

    public override IVmFactory getTopFactory() { return myStdDynGroupFactory; }
    public float getReachRadius() { return mReachRadius; }
    public MARKER_DATA getMarkerData(crc32 id)
    {
        return mpMission.GetMarkerData(id);
    }
    public void Script_SetReachRadius(float radius) { mReachRadius = radius; }
    public void setAutoReformat(bool enable) { myAutoReformat = enable; }
    public bool isAutoReformated() { return myAutoReformat; }

    bool myChangeFormation = false;
    void needToChangeFormation()
    {
        UnitWasDeleted();
        myChangeFormation = true;
    }

    public bool isNeedFormationChange()
    {
        return myChangeFormation;
    }

    public void checkFormationChanged()
    {
        myChangeFormation = false;
    }

    public static bool checkReapiriness(iContact cur_cnt)
    {
        //1: check subobjects
        iContact subcnt = null;
        while ((subcnt = cur_cnt.GetNextSubContact(subcnt, 0xFFFFFFFF, false)) != null)
        {
            if (subcnt == cur_cnt) break;
            if (subcnt.getLifeState() >= State2Life.sDamaged)
                return true;
        }
        //2: check weapon
        iWeaponSystemDedicated turret = (iWeaponSystemDedicated)cur_cnt.GetInterface(iWeaponSystemDedicated.ID);
        if (turret == null) return true;
        int old_weapon = turret.GetWeapon();
        for (int i = 0; i <= 2; i++)
        {
            if (!turret.presentWeapon(i)) continue;
            turret.SetWeapon(i);
            if (turret.GetCondition() == 0)
            {
                turret.SetWeapon(old_weapon);
                return true;
            }
        }
        turret.SetWeapon(old_weapon);
        return false;
    }
}
public class PointExecutionContext<T> : IPointExecutionContext where T : StdGroupAi
{
    public Vector3 getPosition()
    {
        return myMsn.mpCurrentAction != null ? myMsn.mpCurrentAction.getExecutionPos() : Vector3.zero;
    }

    public PointExecutionContext(T msn)
    {
        myMsn = msn;
    }
    T myMsn;
}

public static class AiCommands
{
    public const uint Cmd_iCoverMe = 0x1FAE8895; // "mb_CoverMe"
    public const uint Cmd_iAttackMyTarget = 0xFCD9EF54; // "mb_AttackMyTarget"
    public const uint Cmd_iDefendMyTarget = 0x268C8B0B; // "mb_CoverMyTarget"
    public const uint Cmd_iEngageAtWish = 0x50720E4D; // "mb_EngageAtWill"
    public const uint Cmd_iRepair = 0x4F492D74; // "mf_Repair"
    public const uint Cmd_iRepairSelf = 0xB70073D3; // "mf_RepairFast" 
    public const uint Cmd_iJoinFormation = 0xE611627A; // "mf_Formation"
    public const uint Cmd_iReturnFormation = 0xFC31E817; // "mb_ReturnToFormation"
    public const uint Cmd_iLand = 0xC56A1C03; // "mf_Land"
    public const uint Cmd_iLandSelf = 0xD785790E; // "mf_LandFast" 
    public const uint Cmd_iTightFormation = 0x0E382609; // "mf_Formation_Tighten"
    public const uint Cmd_iLooseFormation = 0x755E4DA2; // "mf_Formation_Loose"
    public const uint Cmd_iSelf = 0x1621F0B8; // "mf_Self"
    public const uint Cmd_iEntireWing = 0xD568CC9F; // "mf_Wing"
    public const uint Cmd_iWingMen = 0x448AC0FA; // "mf_Wingman"
    public const uint Cmd_iRepairBase = 0x7C53809A; // "mf_ReturnToBase_Item"
                                                    // former all wingmens
    public const uint Cmd_iAllWingmens = 0x20E605FE; // "mf_Wingmen"
    public const uint Cmd_iProceedBase = 0x80EF35D6; // "mf_ReturnToBase"

    public const uint Cmd_iAddBot = 0xF0485265; // "mm_AddBot"
    public const uint Cmd_iRestartMatch = 0x488EB786; // "mm_RestartMatch"

    public const uint Cmd_iChangeSide = 0x57C5E393; // "mm_ChangeSide"

    public const uint Cmd_mHumanBF1 = 0x5C9888FF; // "mm_HBF1"
    public const uint Cmd_mHumanBF2 = 0xC591D945; // "mm_HBF2"
    public const uint Cmd_mHumanBF3 = 0xB296E9D3; // "mm_HBF3"
    public const uint Cmd_mHumanBF4 = 0x2CF27C70; // "mm_HBF4"
    public const uint Cmd_mVelianBF1 = 0xEC5EA853; // "mm_VBF1"
    public const uint Cmd_mVelianBF2 = 0x7557F9E9; // "mm_VBF2"
    public const uint Cmd_mVelianBF3 = 0x0250C97F; // "mm_VBF3"

    public const uint Cmd_tHumanBF1 = 0x804A43AF; // "Human_BF1"
    public const uint Cmd_tHumanBF2 = 0x19431215; // "Human_BF2"
    public const uint Cmd_tHumanBF3 = 0x6E442283; // "Human_BF3"
    public const uint Cmd_tHumanBF4 = 0xF020B720; // "Human_BF4"
    public const uint Cmd_tVelianBF1 = 0xD5C6A29F; // "Velian_BF1"
    public const uint Cmd_tVelianBF2 = 0x4CCFF325; // "Velian_BF2"
    public const uint Cmd_tVelianBF3 = 0x3BC8C3B3; // "Velian_BF3"

    // "Alpha %d"
    public const uint iAlpha_2 = 0xECDF7A7B;
    public const uint iAlpha_3 = 0x9BD84AED;
    public const uint iAlpha_4 = 0x05BCDF4E;
    public const uint iAlpha_5 = 0x72BBEFD8;
    public const uint iAlpha_6 = 0xEBB2BE62;
    public const uint iAlpha_7 = 0x9CB58EF4;
    public const uint iAlpha_8 = 0x0C0A9365;
    public const uint iAlpha_9 = 0x7B0DA3F3;

    public const int ENTIRE_WING_CODE = 555;
    public const int ENTIRE_WINGMENS_CODE = 556;
}
