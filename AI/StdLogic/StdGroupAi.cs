using UnityEngine;
using DWORD = System.UInt32;
using crc32 = System.UInt32;
using System.Collections.Generic;
using static AICommon;
using Unity.VisualScripting;

public abstract class StdGroupAi : IGroupAi, IQuery
{
    public const uint ID = 0xBE800EBD;
    const string DesignerMissed = "create: can't create group without iEventDesigner";
    const string sLeaderMissed = "accessing to leader properties without units in \"{0}\", skipped";
    const float gTakeoffRequstPeriod = 30f;
    const float DEFAULT_PRIORITY_COEFF = 2f;

    public const int REQUEST_IN = 0;
    public const int REQUEST_OUT = 1;
    public const int REQUEST_FAIL = 2;
    public const int CLEAR_IN = 3;
    public const int CLEAR_OUT = 4;

    const float CHECK_ENEMY_TIME = 4f;
    public const int ALL_ENGAGE = 0;
    public const int SELF_ENGAGE = 1;
    public const int ENEMY_ENGAGE = 2;

    public const int SEND_TO_ALL = 0;
    public const int SEND_TO_LEADER = 1;
    public const int SEND_TO_MEMBERS = 2;

    public const int INCLUDE_ONLY_OBJECTS = 0;
    public const int INCLUDE_ONLY_SUBOBJECTS = 1;
    public const int INCLUDE_OBJECTS_AND_SUBOBJECTS = 2;

    // external data and interfaces
    public IGame mpGame;
    public IMissionAi mpMission;
    public GROUP_DATA mpData;
    bool mFirstTick;
    int mRefCount;

    crc32 myName;

    // parsing
    public bool executeScript(string name, string text, string namesp)
    {
        if (myStdMsn != null && text != "" && text !=null)
        {
            myStdMsn.setSource(name, text);
            if (myVm != null)
            {
                //Debug.Log(string.Format("Loading script for group {0}", mpData.Callsign));
                bool res = myVm.parseScript(text+'\0', namesp);
                Debug.Log(string.Format("Loading script for group {0} : [{1}]", mpData.Callsign, res ? "SUCCESS" : "FAILURE"));
                if (!res) Debug.LogError(text);
                return res;
            }
        }
        return true;

    }

    // ai units and management
    public AiUnit[] mpUnits;
    protected iContact mpLeader;
    iContact mpFirstPlayer;
    protected IBaseUnitAi mpLeaderAi;

# if _DEBUG
    void showScanInfo(string comment, iContact* cnt, float weight = 0);
    public virtual void showTargets(bool show)
    {
        myShowTargets = show;
    }
    Bool<false> myShowTargets;
#endif //_DEBUG

    public int mAliveCount, mGhostCount, mRequestedGhostCount;
    bool mHidden;
    void SetGhostCount(int cnt)
    {
        mGhostCount = cnt;
        if (mGhostCount < mRequestedGhostCount)
            mRequestedGhostCount = mGhostCount;
    }
    void SetReqGhostCount(int cnt)
    {
        mRequestedGhostCount = cnt;
        if (mGhostCount < mRequestedGhostCount)
            mRequestedGhostCount = mGhostCount;
    }
    public bool IsGroupAlive() { return !(mAliveCount <= 0) && mpLeader != null; }
    int GetNumByHandle(DWORD hndl)
    {
        for (int i = 0; i < mGhostCount; i++)
            if (mpUnits[i] != null)
            {
                iContact cnt = mpUnits[i].GetAI().GetContact();
                if (cnt != null && cnt.GetHandle() == hndl) return i;
            }
        return -1;

    }
    int GetNumByAI(IBaseUnitAi ai) { for (int i = 0; i < mGhostCount; i++) if (mpUnits[i].GetAI() == ai) return i; return -1; }
    int GetNumByUnitData(UNIT_DATA dt) { for (int i = 0; i < mGhostCount; i++) if (mpUnits[i].UnitData() == dt) return i; return -1; }
    void UpdateUnits()
    {
        int new_count = 0;
        //Debug.Log(mGhostCount);
        //Debug.Log(mpUnits.Length);
        for (int i = 0; i < mGhostCount; i++)
        {
            //Debug.Log(string.Format("[{0} {1}] {2}",GetGroupData().Callsign, mpUnits[i].!=null ? (int)mpUnits[i].mpUnitData.Number:"ERR",i));
            AiUnit ai = mpUnits[i];
            if (!ai.IsDead())
                new_count++;
        }

        //Debug.Log("unhidden " + new_count + ":" + mGhostCount);
        if (new_count == mGhostCount) return;
        AiUnit[] new_units = null;
        if (new_count != 0)
        {
            new_units = new AiUnit[new_count];
            int cur_count = 0;
            for (int i = 0; i < mGhostCount; i++)
            {
                AiUnit ai = mpUnits[i];
                if (!ai.IsDead())
                    new_units[cur_count++] = mpUnits[i];
            }
        }
        if (mpUnits != null) mpUnits = null;
        SetGhostCount(new_count);
        mpUnits = new_units;
        Debug.Log("Unit list reconstructed for " + mpData.Callsign);
        CheckPlayerLeadership();
    }

    bool ProcessRespawn(ref AiUnit ai, float scale)
    {
        bool update = false;
        bool has_been_respawned = false;
        RespawnInfo info = ai.GetRespawnInfo();
        //Debug.LogFormat("ProcessRespawn {0} can? {1} flags {2} RI {3}", 
        //    mpData.Callsign + " " + ai.mpUnitData.Number,
        //    ai.CanRespawn(), 
        //    ai.UnitData().Flags.ToString("X8"),
        //    ai.GetRespawnInfo()
        //    );
        if (false == ai.CanRespawn())
        {
            OnSwapUnit(ai.GetAI(), null);
            ai.UnitData().Destroyed();
            update = true;
        }
        else if ((ai.GetRespawnType() == RespawnType.rtRespawnFromTime) && (info != null))
        {
            info.mRespawnTime -= scale;
            if (info.mRespawnTime < 0)
            {
                Parsing.FillUnitDataFromRespawnInfo(ref ai.mpUnitData, info);
                iContact hangar = null;
                IBaseUnitAi created_ai = OnCreateUnitData(ai.UnitData(), info, ref hangar);
                if (created_ai != null)
                {
                    has_been_respawned = true;
                    OnSwapUnit(null, created_ai);
                    ResetNames();
                    created_ai.SetMessagesMode(false == mpEventDesigner.CanProcessInternalEvents());
                    CheckPlayerLeadership();
                    if (!mpData.IsInCampaign())
                        SendNotify(MessageCodes.AUTO_UNIT, AIGroupsEvents.MC_UNIT_APPEAR, ai.UnitData().Number + 1, "OnUnitAppear");
                    if (mAliveCount == 1)
                    {
                        OnStartAppear(hangar);
                        mNotifiedAboutDisappear = false;
                    }
                    if (mAliveCount == mRequestedGhostCount)
                        OnFinishAppear();
                }
            }
        }
        if (has_been_respawned && !mpData.IsInCampaign())
            ai.HasBeenRespawned();
        return update;

    }

    iSensors GetFirstSensors()
    {
        for (int i = 0; i < mGhostCount; i++) if (mpUnits[i].GetAI() != null) return mpUnits[i].GetAI().GetSensors(); return null;
    }
    protected bool UpdateLeader()
    {
        iContact prev_leader = mpLeader;
        mpLeaderAi = null;
        mpLeader = null;
        for (int i = 0; i < mGhostCount; i++)
        {
            if (mpUnits[i].GetAI() != null)
            {
                iContact cnt = mpUnits[i].GetAI().GetContact();
                if (isContactLeader(cnt))
                {
                    mpLeaderAi = mpUnits[i].GetAI();
                    mpLeader = cnt;
                    if (mpLeader != prev_leader)
                        LeaderWasChanged();
                    return false;
                }
            }
        }
        return true;

    }

    public static bool isContactLeader(iContact cnt)
    {
        return (cnt != null) && (cnt.GetCondition() > 0f);
    }

    protected void UpdateAlive()
    {
        mAliveCount = 0;
        for (int i = 0; i < mGhostCount; i++)
            if (mpUnits[i].GetAI() != null)
            {
                iContact cnt = mpUnits[i].GetAI().GetContact();
                if (cnt != null)
                    mAliveCount++;
            }
        mUpdateAlive = false;
        UpdateLeader();
    }

    //TODO Удалить нижеследующий метод после проверки корректности работы.
    //protected void UpdateAlive()
    //{
    //    mAliveCount = 0;
    //    for (int i = 0; i < mGhostCount; i++)
    //        if (mpUnits[i] != null)
    //        {
    //            if (mpUnits[i].GetAI() == null) continue
    //            iContact cnt = mpUnits[i].GetAI().GetContact();
    //            if (cnt != null)
    //                mAliveCount++;
    //        }
    //    mUpdateAlive = false;
    //    UpdateLeader();
    //}

    // targets management
    bool mHasSubContactsOnLastTick;
    protected DWORD mIncludeSubobjects;
    public void IncludeSubobjects(DWORD inc) { mIncludeSubobjects = inc; }

    float myLastTakeoffRequestTime;
    protected readonly TContact mAttackedContact = new TContact();
    protected readonly TContact mDefendedContact = new TContact();
    DWORD mExcludeSide;
    float mRandomBounds;
    public void setRandomBounds(float bounds) { mRandomBounds = bounds; }

    protected float mAttackRadius;
    float mAttackCoeff;
    float mEnemyTimer;
    Vector3 mRadarCenter;
    protected EnemyCount mCurrentEnemyCount = new EnemyCount();
    protected EnemyCount mPreviousEnemyCount = new EnemyCount();
    List<iContact> myScannedTargets = new List<iContact>();
    List<float> myScannedWeights = new List<float>();

    void EqualizeEnemies() { mPreviousEnemyCount = mCurrentEnemyCount; }
    public bool SectorIsClear() { return 0 == mCurrentEnemyCount.mRealEnemyCount && 0 != mPreviousEnemyCount.mRealEnemyCount; }
    protected bool SectorIsDirty() { return 0 != mCurrentEnemyCount.mRealEnemyCount && 0 == mPreviousEnemyCount.mRealEnemyCount; }

    void SetExcludeSide(DWORD side) { mExcludeSide = side; }
    void AttackUnit(DWORD grp_id, DWORD un_id)
    {
        StdMissionAi std_miss = (StdMissionAi)mpMission.Query(StdMissionAi.ID);
        if (std_miss != null)
            //mAttackedContact = new TContact(std_miss.GetContactByIndex(grp_id, un_id));
            mAttackedContact.setPtr(std_miss.GetContactByIndex(grp_id, un_id));
    }

    LinkedList<PriorityGroup> mlPriorityGroups = new LinkedList<PriorityGroup>();
    public void AddPriority(int grp_id, float coeff)
    {
        if (mpMission.UnitExistsByIndex((uint)grp_id, Constants.THANDLE_INVALID) == 0) return;

        for (LinkedListNode<PriorityGroup> grp = mlPriorityGroups.First; grp != null; grp = grp.Next)
        {
            if (grp.Value.grp == grp_id)
            {
                grp.Value.coeff = coeff;
                return;
            }
        }

        mlPriorityGroups.AddFirst(new PriorityGroup((uint)grp_id, coeff));
    }
    PriorityGroup InPriorityGroup(DWORD handle)
    {
        LinkedListNode<PriorityGroup> grp_next = null;

        //for (PriorityGroup* grp = mlPriorityGroups.Head(); grp; grp = grp_next)
        //{
        //    grp_next = grp->Next();
        //    if (mpMission->UnitExistsByIndex(grp->grp, THANDLE_INVALID))
        //    {
        //        if (mpMission->UnitExistsByHandle(grp->grp, handle))
        //            return grp;
        //    }
        //    else
        //        delete mlPriorityGroups.Sub(grp);
        //}
        //return 0;

        for (LinkedListNode<PriorityGroup> grp = mlPriorityGroups.First; grp != null; grp = grp_next)
        {
            grp_next = grp.Next;
            if (mpMission.UnitExistsByIndex(grp.Value.grp, Constants.THANDLE_INVALID) != 0)
            {
                return grp.Value;
            }
            else mlPriorityGroups.Remove(grp);
        }
        return null;
    }
    void GetEnemies() { } //TODO проверить на использование и удалить нафиг
    public void SendTargets(DWORD mode, int count, iContact[] targets, float[] weights)
    {
        int n = 0;

        myContacts.clearEnemies();
        int i;
        for (i = 0; i < count; ++i)
        {
            DWORD grp_id;
            DWORD un_id;
            DWORD side;
            if (mpLeader != null && mpMission.GetContactInfo(targets[i], out grp_id, out un_id, out side))
                myContacts.addEnemy(grp_id, targets[i].GetOrg() - mpLeader.GetOrg());
        }
        for (i = 0; i < mGhostCount; i++)
        {
            AiUnit ai = mpUnits[i];
            if (ai.GetAI() != null)
            {
                if (SEND_TO_ALL == mode || (0 == n && SEND_TO_LEADER == mode) || (n != 0 && SEND_TO_MEMBERS == mode))
                    ai.GetAI().SelectTarget(count, targets, weights);
                n++;
            }
        }

    }
    public EnemyCount ScanTargets(Vector3 center, float radius, float start_coeff, out List<iContact> tgts, out List<float> wghts)
    {
        
        tgts = new List<iContact>();
        tgts.Clear();
        wghts = new List<float>();
        wghts.Clear();

        EnemyCount enemy_count = new EnemyCount();
        iContact cur = null;

        iSensors sensors = GetFirstSensors();
        //Debug.Log(string.Format("{2} Scanning @ {0} R {1} : {3} using sensors: {4}", center, radius, mpData.Callsign, "started",sensors==null? "NO SENSORS!":sensors));
        while ((cur = sensors.GetEnemyInZone(center, radius, cur, 10f)) != null)
        {
# if _DEBUG
            showScanInfo("Present: ", cur);
#endif //_DEBUG
            if (mExcludeSide != Constants.THANDLE_INVALID && cur.GetSideCode() == mExcludeSide)
                continue;
            DWORD target_handle = cur.GetHandle();
            float scanned_weight = GetTargetWeight(cur, center);  // назначаем пороговое значение
            PriorityGroup pr_grp = InPriorityGroup(target_handle);
            if (pr_grp != null)
                scanned_weight *= pr_grp.coeff;
            else
                scanned_weight *= start_coeff;
            if (scanned_weight > 0f)
                enemy_count.IncReal();
            if (INCLUDE_ONLY_OBJECTS == mIncludeSubobjects || (INCLUDE_OBJECTS_AND_SUBOBJECTS == mIncludeSubobjects && !cur.HasSubContacts()))
            {
#if _DEBUG
                showScanInfo("Added object: ", cur, scanned_weight);
#endif //_DEBUG
                tgts.Add(cur);
                wghts.Add(scanned_weight);
                enemy_count.Inc();
            }
            else
            {
                iContact sub = null;
                while (true)
                {
                    sub = cur.GetNextSubContact(sub);
                    if (sub == cur) break;
# if _DEBUG
                    showScanInfo("Added subobject: ", cur, scanned_weight);
#endif //_DEBUG
                    tgts.Add(sub);
                    wghts.Add(scanned_weight);
                    enemy_count.Inc();
                }
            }
        }
        //Debug.Log(string.Format("{2} Scanning @ {0} R {1} : {3}", center, radius, mpData.Callsign,enemy_count.mEnemyCount));
        return enemy_count;

    }
    float GetTargetWeight(iContact un, Vector3 lead)
    {
        return (1f - (un.GetOrg() - lead).magnitude * .5f / mAttackRadius);
    }
    void CheckForContact(iContact cur) { }
    void CheckForLostContact(iContact cur) { }

    // alerts
    void AddEngageSequence(string start, string end, DWORD group_id, float period, int selffire, bool friendly) { }
    void AddContactSequence(string start, string end, float radius, DWORD group_id, float period) { }
    void AddDeathSequence(string start, string end, int num, int count) { }
    void AddMessageSequence(string start, string end, int count, int codes_count, DWORD[] codes, bool reply, bool copy) { }

    // hangars
    IBaseUnitAi GetHangarForTakeOff(GROUP_DATA gr, UNIT_DATA dt, bool immediate)
    {
        bool have_hangars = false;
        IBaseUnitAi ai_finded = null;
        for (int i = 0; i < mGhostCount; i++)
        {
            AiUnit ai = mpUnits[i];
            if (ai == null || ai.GetAI()==null) continue;
            IHangarAi hng = (IHangarAi)ai.GetAI().Query(IHangarAi.ID);
            have_hangars |= (hng != null);
            if (hng != null && hng.AbleToTakeOff(gr, dt, immediate))
            {       // can take off from hangar
                int res = hng.wantToTakeoff(Hasher.HshString(gr.Callsign), (int)dt.Number + 1);
                if (res == 1)
                    return ai.GetAI();
                else if (res == -1 && ai_finded == null)
                    ai_finded = ai.GetAI();
            }
        }
        if (ai_finded != null) // no waiting hangars
            return ai_finded;
        if (!have_hangars)
            if (AICommon.IsLogged(AICommon.DEBUG_HARD))
            {
                AICommon.AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, "couldn`t proceed takeoff request : no hangars");
            }
        return null;

    }
    IBaseUnitAi GetHangarForLand(iContact cnt, bool immediate)
    {
        float min_coeff = float.MaxValue;
        IBaseUnitAi ret = null;
        bool have_hangars = false;

        for (int i = 0; i < mGhostCount; i++)
        {
            AiUnit ai = mpUnits[i];
            if (ai == null) continue;
            IHangarAi hng = (IHangarAi)ai.GetAI().Query(IHangarAi.ID);
            have_hangars |= (hng != null);
            if (hng != null && hng.AbleToLand(cnt, immediate))
            {       // can take off from hangar
                iContact hng_cnt = ai.GetAI().GetContact();
                if (hng_cnt != null)
                {
                    float coeff = hng.GetCountToLand();
                    if (coeff < min_coeff)
                    {                // find less busy hangar
                        ret = ai.GetAI();
                        min_coeff = coeff;
                    }
                }
            }
        }
        if (ret == null)
        {
            if (AICommon.IsLogged(AICommon.DEBUG_HARD))
            {
                AICommon.AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, "couldn`t proceed land request : %s", have_hangars ? "no suitable hangars" : "no hangars");
            }
        }
        return ret;

    }

    // appear / suicide
    protected DWORD mTakeOffGroup;
    protected bool mTakeOffScramble;
    protected bool mUpdateAlive;
    bool mNotifiedAboutDisappear;
    bool mShowCallsign;
    public void SetShowCallsign(bool use)
    {
        mShowCallsign = use;
        ResetNames();
    }
    void ResetNames()
    {
        if (mpMission.GetMessageMode(mpData.Side))
        {
            int n = 0;
            for (int i = 0; i < mGhostCount; ++i)
            {
                AiUnit ai = mpUnits[i];
                if (ai.GetAI() != null)
                {
                    iContact cnt = ai.GetAI().GetContact();
                    bool set_name = true;
                    if (cnt != null)
                    {
                        string discard = null;
                        string human_name = mpMission.IsPlayer(ai.UnitData(), ref discard);
                        string name = Parsing.sAiEmpty;
                        bool localize = false;
                        if (n == 0 && cnt.GetCondition() > 0f)
                        {
                            if (human_name != null)
                            {
                                //Debug.Log("human_name " + (human_name==null ? "null":human_name) + " "+  mpData.Callsign);
                                name = human_name;
                            }
                            else
                            {
                                //Debug.Log("mpData.Callsign " + mpData.Callsign);
                                name = mpData.Callsign;
                                localize = true;
                            }
                            n++;
                        }
                        else
                        {
                            set_name = (cnt.GetName() != "");
                        }
                        if (mShowCallsign == false)
                        {   // override setups
                            set_name = true;
                            name = Parsing.sAiEmpty;
                        }
                        if (set_name)
                        {
                            //Debug.Log(string.Format("setting name [{0}] for \"{1}\" {2}",name,mpData.Callsign,i));
                            cnt.SetName(name, localize);
                        }
                    }
                }
            }
        }

    }
    public void GroupSuicide(int count, DWORD[] un_list, bool physically)
    {
        //StdMissionAi miss = mpMission.Query<StdMissionAi>();
        StdMissionAi miss = (StdMissionAi)mpMission.Query(StdMissionAi.ID);
        if (miss != null)
            miss.DeleteGroupUnits(GetIGroupAi(), count, un_list, true, physically);
        mUpdateAlive = true;
    }
    IBaseUnitAi OnCreateUnitData(UNIT_DATA dt, RespawnInfo info, ref iContact hangar_ptr)
    {
        //Debug.Log("OnCreateUnitData " + dt.CodedName.ToString("X8"));
        Asserts.AssertBp(info != null);
        Asserts.AssertBp(!dt.IsCreated());
        iContact hangar = null;
        IBaseUnitAi hng_ai = null;
        IHangarAi hangar_ai = null;
        if (hangar_ptr != null) hangar_ptr = null;
        if (info.mBase != Constants.THANDLE_INVALID)
        {
            StdGroupAi st_ai = null;
            hng_ai = GetHangarInGroup(info.mBase, dt, null, ref st_ai, true);
            if (hng_ai == null)
                return null;
            hangar_ai = (IHangarAi)hng_ai.Query(IHangarAi.ID);
            if (hangar_ai != null)
            {
                if (!IsGroupAlive() && myLastTakeoffRequestTime >= 0f && myTakeoffed)
                {
                    string discard=null;
                    string player_name = mpMission.IsPlayer(dt, ref discard);
                    hangar_ai.emulateTakeoffRequest(GetIGroupAi(), player_name != "" ? player_name : mpData.Callsign);
                    myTakeoffed = false;
                    myLastTakeoffRequestTime -= gTakeoffRequstPeriod;
                }
                bool appear;
                if (mpMission.presentMessage(st_ai.GetIGroupAi(), Hasher.HshString(mpData.Callsign), mpMission.GetMessageCode(GetRequestEventCode(REQUEST_OUT)).mCode, true))
                    return null;
                hangar = hangar_ai.RequestToTakeoff(GetIGroupAi(), dt, out appear);
                if (appear == false)
                    return null;
            }
            //Debug.Log(string.Format("Taking off [{0} {1}] @ {2}", mpData.Callsign, dt.Number + 1, hangar));
        }
        if (!IsGroupAlive())
            if (AICommon.IsLogged(AICommon.DEBUG_HARD))
            {
                AICommon.AiMessage(MessageCodes.AUTO_GROUP, "Appear", "start appear" + " [" + mpData.Callsign + " " + (dt.Number + 1) + "]");
            }
        IBaseUnitAi ai;

        ai = mpMission.CreateUnitAi(dt, (int)info.SideCode, hangar, GetIGroupAi());
        if (ai != null)
        {
            //try
            //{
            OnCreateUnit(ai);
            //}
            //catch (System.Exception e)
            //{
            //    if (ai != null)
            //        ai.Release();
            //    throw e;
            //}
            iContact cnt = ai.GetContact();
            if (cnt != null)
            {
                mHasSubContactsOnLastTick |= cnt.HasSubContacts();
                if (hangar_ai != null)
                    hangar_ai.SetTakeoffMember(cnt);
            }
        }
        if (hangar_ptr != null) hangar_ptr = hangar;

        myTakeoffed = true;
        return ai;

    }
    protected IBaseUnitAi GetHangarInGroup(DWORD id, UNIT_DATA dt, iContact leader, ref StdGroupAi std_grp_ai, bool immediate)
    {
        if (std_grp_ai != null)
            std_grp_ai = null;
        if (id == Constants.THANDLE_INVALID) return null;
        IGroupAi grp = mpMission.GetGroupByID(id);
        if (grp == null) return null;
        StdGroupAi st_ai = (StdGroupAi)grp.Query(StdGroupAi.ID);
        if (st_ai == null) return null;
        IBaseUnitAi hng = null;
        if (std_grp_ai == null) std_grp_ai = st_ai;
        if (dt != null)
            hng = st_ai.GetHangarForTakeOff(mpData, dt, immediate);
        else if (leader != null)
            hng = st_ai.GetHangarForLand(leader, immediate);
        return hng;
    }

    public void StartAppearing(float time, DWORD max_units)
    {
        mHidden = false;
        mNotifiedAboutDisappear = false;
        SetReqGhostCount((int)max_units);
        int n = 0;
        int i;

        for (i = 0; i < mGhostCount; i++)
        {
            AiUnit ai = mpUnits[i];
            RespawnInfo info = ai.GetRespawnInfo();
            //Debug.Log(mpData.Callsign + " " + (i+1) + " info @ appear " + info);
            if (info != null)
            {
                bool respawn = false;
                if (n < max_units)
                {
                    if (ai.UnitData().IsDestroyed() == false)
                    {
                        n++;
                        respawn = true;
                    }
                }
                info.mBase = mTakeOffGroup;
                info.mRespawnTime = time;
                RespawnUnitAfterDeath(ai.UnitData(), respawn ? RespawnType.rtRespawnFromTime : RespawnType.rtRespawnNever, info, OverwriteType.otOverwriteRespawnInfo);
                //Debug.Log(mpUnits[i].GetRespawnInfo());

            }
        }
        for (i = 0; i < mActions.Count; ++i)
            mActions[i].OnGroupReborn();

    }

    bool IsEngaged(float dist)
    {
        bool ret = false;
        for (int i = 0; i < mGhostCount; i++)
        {
            AiUnit ai = mpUnits[i];
            if (ai != null)
            {
                iContact cnt = ai.GetAI().GetContact();
                if (cnt != null)
                {
                    iContact thr = cnt.GetThreat();
                    if (thr != null && thr.GetSideCode() != cnt.GetSideCode() && (thr.GetOrg() - cnt.GetOrg()).sqrMagnitude < Mathf.Pow(dist, 2))
                    {
                        ret = true;
                        break;
                    }
                }
            }
        }
        if (!ret && mpLeader != null)
        {
            iSensors sensors = GetFirstSensors();
            if (sensors != null)
                ret = sensors.GetEnemyInZone(mpLeader.GetOrg(), dist, null, 0f) != null;
        }
        return ret;

    }

    bool myUnitsWasCreatedOnThisTick;
    bool myUnitsWasDeletedThisTick;
    bool myLeaderChanged;

    protected void UnitWasDeleted() { myUnitsWasDeletedThisTick = true; }
    void UnitWasCreated() { myUnitsWasCreatedOnThisTick = true; }
    void LeaderWasChanged()
    {
        myLeaderChanged = true;
        ResetNames();
    }

    void ClearFlags()
    {
        myUnitsWasDeletedThisTick = false;
        myUnitsWasCreatedOnThisTick = false;
        myLeaderChanged = false;
    }

    public bool isLeaderChanged() { return myUnitsWasCreatedOnThisTick || myUnitsWasDeletedThisTick || myLeaderChanged; }
    public bool isLeader(IBaseUnitAi ai) { return IsGroupAlive() ? mpLeaderAi == ai : false; }
    // return
    float mReturnDist;

    // player leader
    protected bool mPlayerIsLeader;
    bool mPlayerIsInGroup;
    bool mAiIsInGroup;
    void CheckPlayerLeadership()
    {
        if (mGhostCount == 0) return;
        bool ret = false;
        bool ret2 = false;
        bool ret3 = false;
        mpFirstPlayer = null;
        int n = 0;
        for (int i = 0; i < mGhostCount; i++)
            if (mpUnits[i] != null)
            {
                //Debug.Log((mpUnits[i].mpUnitData.CodedName.ToString("X8"), mpUnits[i].GetAI()));
                if (mpUnits[i].GetAI() == null)
                {
                    OBJECT_DATA myobj = OBJECT_DATA.GetByCode(mpUnits[i].mpUnitData.CodedName);
                    string myUnitName = myobj != null ? myobj.FullName : "Unknown";
                    //TODO!  Выяснить и устранить причины отсутствия AI для юнита
                    //Debug.Log("Empty AI for unit " + mpData.Callsign + " " + i + " " +  mpUnits[i].mpUnitData.CodedName.ToString("X8") + " [" + myUnitName + "]");
                    //Debug.Log(string.Format("Empty AI for unit '{0} {1}' ID {2} type [{3}]", mpData.Callsign, mpUnits[i].mpUnitData.Number, mpUnits[i].mpUnitData.CodedName.ToString("X8"), myUnitName));
                    continue;
                }
                iContact cnt = mpUnits[i].GetAI().GetContact();
                if (cnt != null)
                {
                    bool human = cnt.IsPlayedByHuman();
                    if (human)
                    {
                        if (mpFirstPlayer == null)
                            mpFirstPlayer = cnt;
                        if (n == 0)
                            ret |= human;
                        else
                            ret2 |= human;
                    }
                    else
                        ret3 |= true;
                    n++;
                }
            }

        DWORD ch = 0;
        if (ret2 != mPlayerIsInGroup)
        {
            mPlayerIsInGroup = ret2;
            ch = 1;
        }
        if (ret != IsPlayerLeader())
        {
            mPlayerIsLeader = ret;
            ch = 2;
        }
        if (ret3 != mAiIsInGroup)
            mAiIsInGroup = ret3;
        if (ch != 0)
            LeaderHumanityChanged(ch == 2);
    }
    public bool IsPlayerLeader() { return mPlayerIsLeader; }
    public bool IsPlayerInGroup() { return mPlayerIsInGroup; }
    protected bool IsAiInGroup() { return mAiIsInGroup; }
    protected bool IsRadioEnvForClients() { return IsPlayerLeader() || IsPlayerInGroup(); }

    // notify
    protected iEventDesigner mpEventDesigner;
    void CheckMessageMode()
    {
        bool silence = mpEventDesigner.CanProcessInternalEvents() == false;
        for (int i = 0; i < mGhostCount; ++i)
        {
            AiUnit ai = mpUnits[i];
            //if (ai != null) //TODO - Возможно, миенно здесь и должны прописываться сообщения AI
            if (ai.GetAI() != null)
            {
                Debug.Log(string.Format("AIUnit {0} for {1} AI {2}", ai, mpData.Callsign + " " + i, ai.GetAI() != null ? ai.GetAI() : "EMPTY"));
                ai.GetAI().SetMessagesMode(silence);
            }
        }

    }
    public void SetAutoMessages(DWORD mExternal, DWORD mInternal, DWORD baseId)
    {
        mpEventDesigner.SetInternalProperties(mExternal, mInternal, baseId);
        CheckMessageMode();

    }

    void GNotify(string msg_code, ref RadioMessage Info)
    {
        Info.CallerCallsign = mpData.Callsign;
        mpMission.ProcessRadioMessage(msg_code, GetIAi(), Info, true, false);

    }
    public void SendNotify(int type, DWORD code, DWORD ci = 0, string name = "")
    {
        if (name !=null)
            if (AICommon.IsLogged(AICommon.DEBUG_HARD))
            {
                //AICommon.AiMessage(type, "", "exec NotifyAll ( Code=\"%s\" Caller=\"%s\" CallerIndex=%d)", name != "" ? name : Parsing.sAiNothing, mpData.Callsign, ci.ToString("X8"));
                //AICommon.AiMessage(type, "", "exec NotifyAll ( Code=\"{0}\" Caller=\"{1}\" CallerIndex={2})", name != "" ? name : Parsing.sAiNothing, mpData.Callsign, ci.ToString("X8"));
                AICommon.AiMessage(type, null, "exec NotifyAll ( Code=\"{0}\" Caller=\"{1}\" CallerIndex={2})", name != "" ? name : Parsing.sAiNothing, mpData.Callsign, ci.ToString("X8"));
            }
        RadioMessage mInfo = new RadioMessage();
        mInfo.Org = mpLeader != null ? mpLeader.GetOrg() : Vector3.zero;
        mInfo.Code = code;
        mInfo.CallerIndex = (int)ci;
        GNotify(name, ref mInfo);

    }
    UnitStatus ProcessLandedNotify(AiUnit ai)
    {
        bool hangaring = ai.GetAI().IsHangaringBeforeLastValidate();
        UnitStatus landed = hangaring ? UnitStatus.usLanded : UnitStatus.usKilled;
        if (!mpData.IsInCampaign())
        {
            if (hangaring)
            {
                if (ai.UnitData().IsRepaired())
                {
                    landed = UnitStatus.usRepaired;
                    SendNotify(MessageCodes.AUTO_UNIT, AIGroupsEvents.MC_UNIT_REPAIRED, ai.UnitData().Number + 1, "OnUnitRepair");
                }
                else
                    SendNotify(MessageCodes.AUTO_UNIT, AIGroupsEvents.MC_UNIT_LANDED, ai.UnitData().Number + 1, "OnUnitLand");
            }
            else
                SendNotify(MessageCodes.AUTO_UNIT, AIGroupsEvents.MC_UNIT_KILLED, ai.UnitData().Number + 1, "OnUnitKill");
        }
        return landed;

    }
    public DWORD GetUnitVoice(DWORD index)
    {
        DWORD voice = (uint)getVoice();

        if (0 != index && IsRadioEnvForClients())
        {
            for (int i = 0; i < mGhostCount; ++i)
            {
                AiUnit ai = mpUnits[i];
                if (ai.UnitData().Number + 1 == index)
                {
                    bool wrap = true;
                    if (ai != null)
                    {
                        iContact cnt = ai.GetAI().GetContact();
                        if (cnt != null && cnt.IsPlayedByHuman())
                        {
                            voice = 1;
                            wrap = false;
                        }
                    }
                    if (wrap)
                        voice = (index % (VoicesCount.gMaxRadioVoices - 2)) + 2;
                    break;
                }
            }
        }
        return voice;

    }

    // type cast
    public IGroupAi GetIGroupAi() { return this; }
    public IGroupAi GetIAi() { return this; }

    // actions
    //AnyRTab<iAction*> mActions;
    //AnyRTab<iAction*> mActionsCandidates;
    //protected List<iAction> mActions = new List<iAction>();
    protected List<iAction> mActions = new List<iAction>();
    protected List<iAction> mActionsCandidates = new List<iAction>();
    protected void ClearAllActions()
    {
        mActions.Clear();
        mpCurrentAction = null;
        mActionsCandidates.Clear();
        if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
            DescribeAction(mpData.Callsign, "all actions", "cleared");
        OnEmptyActions();

    }

    void DescribeAction(string grp, string name, string act)
    {
        AICommon.AiMessage(MessageCodes.AUTO_GROUP, "Action", "\"{0}\" {1}", name, act + " [" + grp + "]");
    }

    bool PushAction(iAction act)
    {
        if (act != null)
        {
            if (mpCurrentAction != null)
            {
                if (mpCurrentAction.IsDeleteOnPush())
                {
                    if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
                        DescribeAction(mpData.Callsign, mpCurrentAction.GetName(), "released");
                    mpCurrentAction.Release();
                    //mActions.Remove(mpCurrentAction);
                    mActions.RemoveAt(mActions.Count - 1);
                    //mActions.Remove(mActions.Count - 1, mActions.Count - 1);//TODO! Протестировать замену на  RemoveRange
                }
                else
                {
                    mpCurrentAction.DeActivate();
                    if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
                        DescribeAction(mpData.Callsign, mpCurrentAction.GetName(), "deactivated");
                }
            }
            mActions.Add(act);
            mpCurrentAction = act;
            if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
            {
                DescribeAction(mpData.Callsign, act.GetName(), "added");
            }
        }
        return act != null;

    }
    protected bool AddAction(iAction act)
    {
        mActionsCandidates.Add(act);
        return true;

    }
    void ProcessCandidates()
    {
        for (int i = 0; i < mActionsCandidates.Count; ++i)
            PushAction(mActionsCandidates[i]);
        //mActionsCandidates.SetCount(0);
        mActionsCandidates.Clear();

    }
    bool PopAction()
    {
        //Debug.Log(string.Format("Actions before popcount for {0} is {1} current [{2}]",mpData.Callsign, mActions.Count, mpCurrentAction));
        //string data = mpData.Callsign + " actions queue:";
        //foreach (var v in mActions)
        //{
        //    data += "\n" + v;
        //}
        //Debug.Log(data);
        Asserts.AssertBp(mpCurrentAction != null);
        if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
        {
            DescribeAction(mpData.Callsign, mpCurrentAction.GetName(), "released");
        }
        mpCurrentAction.Release();
        mActions.RemoveAt(mActions.Count - 1);
        mpCurrentAction = mActions.Count != 0 ? mActions[mActions.Count - 1] : null;
        if (mpCurrentAction != null)
        {
            //Debug.Log(string.Format("Actions after pop count for {0} is {1} current [{2}]", mpData.Callsign, mActions.Count, mpCurrentAction));
            if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
            {
                DescribeAction(mpData.Callsign, mpCurrentAction.GetName(), "activated");
            }
            mpCurrentAction.Activate();
        }
        return mpCurrentAction != null;

    }
    public iAction mpCurrentAction;

    public int GetGhostCount() { return mGhostCount; }
    public int GetAliveCount() { return mAliveCount; }
    public int GetReqGhostCount() { return mRequestedGhostCount; }
    public AiUnit GetAiUnit(DWORD num) { return mpUnits[num]; }
    public AiUnit GetAiUnit(int num) { return GetAiUnit((uint)num); }
    // ===================== api ============================
    //StdGroupAi() :mpUnits(0),mRefCount(1),mAttackedContact(0),mDefendedContact(0),mpEventDesigner(0),
    //mExcludeSide(THANDLE_INVALID),mpCurrentAction(0),myGroupService(this),mySender(this),
    //mySkillSrv(this),myContacts(this),myEngagers(this) { }
    public StdGroupAi()
    {
        mpUnits = null;
        mRefCount = 1;
        //mAttackedContact = new TContact();
        //mDefendedContact = new TContact();
        mAttackedContact.setPtr(null);
        mDefendedContact.setPtr(null);
        mpEventDesigner = null;
        mExcludeSide = Constants.THANDLE_INVALID;
        mpCurrentAction = null;
        //myGroupService = null;
        //mySender = this;
        //mySkillSrv = this;
        //myContacts = this;
        //myEngagers = this;

        myGroupService = new GroupService<StdGroupAi>(this);
        mySender = new GroupSender<StdGroupAi>(this);
        mySkillSrv = new SkillServiceNoBool<StdGroupAi>(this);
        myContacts = new ContactInfoHolder(this);
        myEngagers = new EngageInfoHolder(this);
    }
    ~StdGroupAi()
    {
        mlPriorityGroups.Clear();
        int n = 0;
        for (int i = 0; i < mpData.nUnits; i++)
        {
            if (mpData.Units[i].IsDestroyed())
                n++;
            else
                mpData.Units[i].Deleted();
        }
        if (n == mpData.nUnits)  // all destroyed
            mpData.Destroyed();
        if (mpUnits != null) mpUnits = null;
        //SafeRelease(mpEventDesigner);
        if (!myStdMsn.IsServerShutdowned() && AICommon.IsLogged(AICommon.DEBUG_HARD))
        {
            AICommon.AiMessage(MessageCodes.AUTO_GROUP, "Release", "success");
        }

    }
    // IMemory
    public virtual int Release()
    {
        mRefCount--;
        if (mRefCount == 0)
        {
            //delete this;
            return 0;
        }
        return mRefCount;

    }

    // IRefMem
    public virtual void AddRef()
    {
        mRefCount++;
    }

    // IObject
    public virtual object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case IGroupAi.ID: return GetIGroupAi();
            case StdGroupAi.ID: return this;
            case IGroupService.ID: return myGroupService;
            case IContactInfo.ID: return myContacts;
            case IEngageInfo.ID: return myEngagers;
            case ITimeService.ID: return myStdMsn.getTimer();
            case IRadioService.ID: return myStdMsn.getRadio();
            case IRadioSender.ID: return mySender;
            case ISkillService.ID: return mySkillSrv;
            case IErrorLog.ID: return myStdMsn.getErrorLog();
        }
        return 0;

    }

    // IAi
    public virtual void ProcessRadioMessage(string msg_code, IAi caller, RadioMessage Info, bool to_all, bool say_flag)
    {
        //Debug.Log(string.Format("Group {0} forwarding message to its units: {1}",mpData.Callsign, Info));
        // forward message
        for (int i = 0; i < mGhostCount; i++)
        {
            AiUnit ai = mpUnits[i];
            if (ai.GetAI() == null) continue;
            ai.GetAI().ProcessRadioMessage(msg_code, caller, Info, to_all, say_flag);
        }

    }
    public virtual bool Update(float scale)
    {
        myLastTakeoffRequestTime += scale;
        myStdMsn.setContext(mpData.Callsign);

        if (mFirstTick)
        {
            myVm.setFactory(getTopFactory());
            if (mGhostCount == 0)
                AICommon.AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, "no units on script executing");
            //Debug.Log(mpData.Callsign + " mpData.AiScript \n===start===\n" + mpData.AiScript + "\n===end===");
            executeScript(mpData.Callsign, mpData.AiScript, "GroupProperty");
            mFirstTick = false;
            return true;
        }
        UpdateLeader();

        bool someone_has_subobjects = processUnits(scale);

        if (IsGroupAlive() && mHasSubContactsOnLastTick && !someone_has_subobjects)
        {
            SendNotify(MessageCodes.AUTO_GROUP, AIGroupsEvents.MC_GROUP_LOSTDETACHED, 0, "OnGroupLostDetached");
            mHasSubContactsOnLastTick = false;
        }
        // check onmessage sequences
        if (mGhostCount != 0)
        {
            if (mpEventDesigner != null)
                mpEventDesigner.Update(scale);
        }
        mUpdateAlive = UpdateLeader();
        if (mUpdateAlive)
            UpdateAlive();

        ActionStatus status = processActions(scale);


        ClearFlags();
        EqualizeEnemies();

        myVm.run();

        return status.IsGroupDead() == false;
    }

    // IGroupAi
    public virtual GROUP_DATA GetGroupData()
    {
        return mpData;
    }
    public virtual bool HaveHangars(DWORD side_code, DWORD land_flags)
    {
        bool ret = false;
        if (MissionSideDefines.IsSideNeutral(mpData.Side) || side_code == mpData.Side)
        {
            for (int i = 0; i < mGhostCount; i++)
            {
                AiUnit ai = mpUnits[i];
                if (ai == null) continue;
                Debug.Log(string.Format("HangarAi for {0} {1} [{2}]", mpData.Callsign, i + 1, ai.GetAI()));
                Debug.Log(string.Format("HangarAi for {0} {1} [{2}]", mpData.Callsign, i + 1, ai.GetAI().Query(IHangarAi.ID)));
                IHangarAi hng = (IHangarAi)ai.GetAI().Query(IHangarAi.ID);
                if (hng == null) continue;
                if ((land_flags & HangarDefs.HANGARS_LAND_ONLY) != 0)
                    ret = hng.IsLandWork() ? true : false;
                if ((land_flags & HangarDefs.HANGARS_TAKEOFF_ONLY) != 0)
                    ret = hng.IsTakeoffWork() ? true : false;
                if (ret)
                    break;
            }
        }
        return ret;
    }
    public virtual iContact GetLeaderContact()
    {
        UpdateLeader();
        return mpLeader;
    }
    public virtual IBaseUnitAi GetNextUnit(IBaseUnitAi next)
    {
        if (mpLeaderAi == null) return null;
        if (next == null)
            return mpLeaderAi;
        else
        {
            int num = GetNumByAI(next);
            if (num >= mGhostCount - 1) return null;
            return mpUnits[num + 1].GetAI();
        }

    }
    public virtual UNIT_DATA GetUnitDataByIndex(DWORD index)
    {
        int n = 0;
        for (int i = 0; i < mGhostCount; i++)
        {
            AiUnit ai = mpUnits[i];
            if (ai == null) continue;
            n++;
            if (0 == index || index == n)
                return ai.UnitData();
        }
        return null;

    }
    public virtual Vector3 GetLeaderOrg()
    {
        UpdateLeader();
        if (mpLeader == null)
        {
            LeaderMissed("GetLeaderOrg");
            return Vector3.zero;
        }
        return mpLeader.GetOrg();

    }

    public static void LeaderMissed(string tok)
    {
        AICommon.AiMessage(MessageCodes.MSG_ERROR, "Error", sLeaderMissed, tok);
    }

    public virtual iSensors GetLeaderSensors()
    {
        return GetFirstSensors();
    }
    public virtual void OnSwapUnit(IBaseUnitAi old_ai, IBaseUnitAi new_ai)
    {
        Asserts.AssertBp((old_ai != null) || (new_ai != null));
        //Debug.Log(string.Format("Swapping AI for {2} from [{0}] to [{1}]", old_ai != null ? old_ai : "null", new_ai, mpData.Callsign + : ));
        int num;
        if (old_ai != null)
            num = GetNumByAI(old_ai);
        else
        {
            UNIT_DATA dt = new_ai.GetUnitData();
            num = GetNumByUnitData(dt);
        }
        Asserts.AssertBp(num != -1);
        AiUnit un;
        try
        {
            un = mpUnits[num];
        } catch
        {
            throw new System.Exception(string.Format("Can't find unit in group {3} with index {0} while switching from {1} to {2}", num, old_ai!=null? old_ai:"NULL", new_ai !=null ? new_ai:"NULL", mpData.Callsign));
        }

        if (new_ai != null)
        {        // add new element
            un.Alive(new_ai);
            UpdateAlive();
        }
        else
        {
            mpMission.OnDeleteContactHandle(un.GetHandle(), false);
            un.Dead();
            UpdateAlive();
        }
        Debug.Log(string.Format("Swapping AI for {2} from [{0}] to [{1}]", old_ai != null ? old_ai : "null", new_ai, mpData.Callsign + " " + num));
    }
    public virtual iEventDesigner GetEventDesigner()
    {
        mpEventDesigner.AddRef();
        return mpEventDesigner;
    }
    public virtual void OnCreateUnit(IBaseUnitAi bua)
    {
        UnitWasCreated();
    }

    // campaign support
    public virtual bool DeleteWithoutClients()
    {
        return true;
    }

    // skiill
    public virtual void SetSkill(DWORD name)
    {
        for (int i = 0; i < mGhostCount; i++)
        {
            AiUnit ai = mpUnits[i];
            if (ai.GetAI() != null)
                ai.GetAI().SetSkill(name);
        }
    }
    public virtual int GetMenuItems(IBaseUnitAi bua, uint id, AiMenuItem ami, int max_count, int page_index)
    {
        return 0;
    }
    public virtual int SelectItem(IBaseUnitAi bua, uint id)
    {
        //return (int) Constants.THANDLE_INVALID;
        return -1;
    }
    public virtual void DeleteUnitAfterDeath(UNIT_DATA dt)
    {
        string sFailedByUnitData = "me failed!";
        int num = GetNumByUnitData(dt);
        Asserts.AiAssertEx(num >= 0, sFailedByUnitData);
        mpUnits[num].SetRespawnInfo(RespawnType.rtDead, null, OverwriteType.otOverwriteFull);

    }
    public virtual void RespawnUnitAfterDeath(UNIT_DATA dt, RespawnType rt, RespawnInfo info, OverwriteType ot)
    {
        string sFailedByUnitData = "me failed!";
        int num = GetNumByUnitData(dt);
        Asserts.AiAssertEx(num >= 0, sFailedByUnitData);
        if (info != null) mpData.Side = (uint)info.SideCode;
        mpUnits[num].SetRespawnInfo(rt, info, ot);

    }
    public virtual IGame GetIGame()
    {
        return mpGame;
    }

    public virtual bool PlayerIsLeader(DWORD caller_index)
    {
        if (IsPlayerLeader() && mGhostCount != 0)
        {
            if (caller_index == 0 || mpUnits[0].UnitData().Number == caller_index)
                return true;
        }
        return false;

    }

    // StdGroupAi
    public virtual void SetInterface(GROUP_DATA _data, IMissionAi miss_ai)
    {

        mpData = _data;
        mpMission = miss_ai;
        mpGame = miss_ai.GetIGame();

        myName = Hasher.HshString(mpData.Callsign);


        //myStdMsn = (StdMissionAi)mpMission.Query(StdMissionAi.ID);
        myStdMsn = (StdMissionAi)mpMission;
        if (myStdMsn != null)
        {
            myVm = myStdMsn.createVm();
            myStdGroupFactory = Factories.createStdGroupFactory(getIQuery());
        }


        mUpdateAlive = true;

        // leadership
        mAttackRadius = 4000f;
        mAttackCoeff = 1f;
        mRandomBounds = 0.1f;
        mRadarCenter = Vector3.zero;
        mReturnDist = 2000f;
        mFirstTick = true;
        mRequestedGhostCount = 0;
        myLastTakeoffRequestTime = 0f;
        mIncludeSubobjects = INCLUDE_ONLY_OBJECTS;

        mpEventDesigner = ClassFactory.CreateEventDesigner(mpMission, GetIGroupAi());
        if (mpEventDesigner == null)
            throw new System.Exception(DesignerMissed);

        mPlayerIsLeader = false;
        mPlayerIsInGroup = false;


        // enemies
        mEnemyTimer = RandomGenerator.Rand01() * CHECK_ENEMY_TIME;

        mTakeOffScramble = false;
        mHasSubContactsOnLastTick = false;

        // рождаемся
        mpLeader = null;
        mpLeaderAi = null;
        mGhostCount = (int)mpData.nUnits;
        mpUnits = new AiUnit[mGhostCount];
        //Debug.Log("Creating new group " + mpData.Callsign);
        mAliveCount = 0;
        RespawnInfo info;
        for (int i = 0; i < mpData.nUnits; ++i)
        {
            UNIT_DATA un = mpData.Units[i];
            mpUnits[i] = new AiUnit();//Todo - как ни странно, в исходном коде используется без иницииализации. ВОзможно, стоит перенести на после проверки на убитость?
                                      // only not hidden
            if (un.IsDestroyed()) continue;   // skip destroyed

            mpUnits[i].Initialize(un);
            Parsing.FillRespawnInfoFromUnitData(out info, un);
            info.mOrg = un.Org;
            info.SideCode = mpData.Side;
            //StdGroupAi::RespawnUnitAfterDeath(un, rtRespawnNever, &info, otOverwriteFull);
            RespawnUnitAfterDeath(un, RespawnType.rtRespawnNever, info, OverwriteType.otOverwriteFull);
            //Debug.Log(mpData.Callsign + " "  + (i+1)+ " info @  setinterface " + mpUnits[i].GetRespawnInfo());
        }
        UpdateUnits();
        // appear
        mShowCallsign = mpData.IsInCampaign() == false; //TODO Выяснить, почему не работает установка позывного и вернуть на место
                                                        //mShowCallsign = mpData.IsInCampaign() == true;
        mHidden = mpData.GetFlag(CampaignDefines.CF_HIDDEN) == CampaignDefines.CF_HIDDEN;
        SetExcludeSide(MissionSideDefines.SIDE_MISSION);
        SetGroupAppear(null, false, !mHidden);
        OnGroupCreate();
    }


    public virtual IBaseUnitAi GetUnitAiByData(UNIT_DATA _dt)
    {
        int num = GetNumByUnitData(_dt);
        Asserts.AiAssertEx(num >= 0, "can't find unit by unit data");
        return mpUnits[num].GetAI();

    }
    public virtual IMissionAi GetIMissionAi()
    {
        return mpMission;
    }
    public virtual string GetStateName()
    {
        return "Empty";
    }

    public virtual string GetRequestEventCode(DWORD takeoff)
    {
        return "";
    }
    public virtual void LeaderHumanityChanged(bool leader_changed) { }
    public virtual void WorkContactIsLost() { }
    public virtual void OnUnitLost(AiUnit aiUnit, bool killed)
    {
        ResetNames();
        UnitWasDeleted();
    }

    public virtual void OnChangeSide(DWORD new_side)
    {
        if (mpData.Side == new_side) return;
        for (int i = 0; i < mGhostCount; i++)
        {
            AiUnit ai = mpUnits[i];
            if (ai != null)
            {
                iContact cnt = ai.GetAI().GetContact();
                if (cnt != null)
                {
                    Debug.Log("Switching side for " + cnt + " to " + new_side);
                    iContact new_cnt = cnt.ChangeSideTo((int)new_side);
                    ai.GetAI().SideChanged(new_cnt);
                }
            }
            else
                ai.GetRespawnInfo().SideCode = new_side;
        }
        mpData.Side = new_side;

    }

    //new actions api

    public virtual bool GetRadarCenter(out Vector3 center, out float radius)
    {
        center = Vector3.zero;
        radius = 0;
        bool ret = false;
        if (mpLeader != null)
        {
            center = mpLeader.GetOrg();
            radius = CCmp(mAttackRadius) ? 0f : mAttackRadius + RandomGenerator.Rand01() * mAttackRadius * mRandomBounds;
            ret = true;
        }
        if (mDefendedContact.Ptr() != null)
        {
            mDefendedContact.Validate();
            if (mDefendedContact.Ptr() != null)
            {
                center = mDefendedContact.Ptr().GetOrg();
                ret = true;
            }
            else
                WorkContactIsLost();
        }
        else if (mAttackedContact.Ptr() != null)
        {
            mAttackedContact.Validate();
            if (mAttackedContact.Ptr() != null)
                ret = true;
            else
                WorkContactIsLost();
        }
        return ret;

    }
    public virtual float ScanEnemies(Vector3 center, float radius)
    {
        mCurrentEnemyCount.Clear();
        if (!IsGroupAlive()) return CHECK_ENEMY_TIME;

        // есть ли defended group
        if (mDefendedContact.Ptr() != null)
        {
            EnemyCount enemy_count = ScanTargets(center, radius, 1f, out myScannedTargets, out myScannedWeights);
            DWORD threat_handle = Constants.THANDLE_INVALID;
            iContact cnt = mDefendedContact.Ptr().GetThreat();
            if (cnt != null)
                threat_handle = cnt.GetHandle();
            for (int i = 0; i < enemy_count.mEnemyCount; ++i)
            {
                if (myScannedTargets[i].GetHandle() == threat_handle)
                    myScannedWeights[i] *= 10f;
                //SendTargets(SEND_TO_MEMBERS, enemy_count.mEnemyCount, myScannedTargets.Begin(), myScannedWeights.Begin());
                SendTargets(SEND_TO_MEMBERS, (int)enemy_count.mEnemyCount, myScannedTargets.ToArray(), myScannedWeights.ToArray()); //TODO! Проверить корректность отправки целей членам группы!
            }
        }
        else if (mAttackedContact.Ptr() != null)
        {
            // проверяем жив ли attacked unit
            iContact cnt = mAttackedContact.Ptr();
            float wgt = 1f;
            SendTargets(SEND_TO_MEMBERS, 1, new iContact[] { cnt }, new float[] { wgt });
        }
        DWORD mode = (uint)(((mDefendedContact.Ptr() != null) || (mAttackedContact.Ptr() != null)) ? SEND_TO_LEADER : SEND_TO_ALL);
        // считаем радиус
        if (CCmp(radius) == false)
            mCurrentEnemyCount = ScanTargets(center, radius, mAttackCoeff, out myScannedTargets, out myScannedWeights);
        else
            mCurrentEnemyCount.Clear();

        SendTargets(mode, (int)mCurrentEnemyCount.mEnemyCount, myScannedTargets.ToArray(), myScannedWeights.ToArray());
        //if (mCurrentEnemyCount.mEnemyCount > 0)
            // Debug.Log(string.Format("Group {0} processed {1} enemy targets", mpData.Callsign, mCurrentEnemyCount.mEnemyCount));

        return CHECK_ENEMY_TIME;

    }

    public virtual void OnGroupCreate() { }
    public virtual void OnActionDeath() { }
    public virtual void OnActionDeactivate() { }
    public virtual void OnEmptyActions() { }
    public virtual void OnStartAppear(iContact hng)
    {
        Debug.Log(string.Format("OnStartAppear: \"{0}\" IsInCMP {1} Scramble {2}", mpData.Callsign, mpData.IsInCampaign() ,mTakeOffScramble));
        UpdateLeader();
        myAppeared = true;
        // если мы не кампанейские и не находимся в Scramble, то посылаем
        if (!mpData.IsInCampaign() && mTakeOffScramble)
            SendNotify(MessageCodes.AUTO_GROUP, AIGroupsEvents.MC_GROUP_APPEAR, 0, "OnGroupAppear");
        
    }

    public virtual void OnFinishAppear()
    {
        Debug.Log(string.Format("OnFinishAppear: {0} IsInCMP {1} Scramble {2}", mpData.Callsign, mpData.IsInCampaign(), mTakeOffScramble));
        // если мы не кампанейские и находимся в Scramble , то посылаем
        if (!mpData.IsInCampaign() && !mTakeOffScramble)
            SendNotify(MessageCodes.AUTO_GROUP, AIGroupsEvents.MC_GROUP_APPEAR, 0, "OnGroupAppear");
    }

    public virtual bool SetGroupAppear(string base_name, bool scramble, bool process_appear)
    {
        Debug.Log(string.Format("Spawning group {0} @ [{1}] scramble {2} process_appear {3}", mpData.Callsign,base_name == null ? "Not in base": base_name,scramble,process_appear));
        if (IsGroupAlive())
        {
            if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
            {
                AICommon.AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, "already exists, appear skipped");
            }
            return false;
        }
        DWORD baseId = Hasher.HshString(base_name);
        //if (base_name != "")
        if (base_name != null)
        {
            GROUP_DATA dt = mpMission.GetExistedGroupData(baseId);
            if (dt == null)
            {
                if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
                {
                    AICommon.AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, "base \"{0}\" not exists, skipped", base_name);
                }
                return false;
            }
        }
        mTakeOffGroup = baseId;
        mTakeOffScramble = scramble;

        if (AICommon.IsLogged(AICommon.DEBUG_HARD))
        {
            if (baseId != Constants.THANDLE_INVALID)
                AICommon.AiMessage(MessageCodes.AUTO_GROUP, "Appear", "will appear on Base \"{0}\", Scramble {1}", base_name, mTakeOffScramble.ToString());
        }
        if (process_appear)
            StartAppearing(-1f, mpData.nUnits);

        Debug.Log(string.Format("Spawned group {0} @ [{1}] scramble {2} process_appear {3}", mpData.Callsign, base_name == null ? "Not in base" : base_name, scramble, process_appear));
        return true;

    }
    public abstract void SetGroupDisappear(bool base_ret, DWORD baseId, float dist, DWORD ultimate, string base_name);


    // new
    protected IVmFactory myStdGroupFactory;
    IVm myVm;
    protected StdMissionAi myStdMsn;
    GroupService<StdGroupAi> myGroupService;
    GroupSender<StdGroupAi> mySender;
    SkillServiceNoBool<StdGroupAi> mySkillSrv;

    protected IQuery getIQuery() { return this; }
    bool myTakeoffed = true;
    public virtual IVmFactory getTopFactory() { Debug.Log("getTopFactory() Not implemented for " + this.GetType()); return null; }

    public IErrorLog getErrorLog()
    {
        return myStdMsn.getErrorLog();
    }
    public void setAttackRadius(float radius)
    {
        mAttackRadius = radius;
    }
    public void setAttackCoeff(float coeff, bool clear)
    {
        mAttackCoeff = coeff;
        if (clear)
            mlPriorityGroups.Clear();
    }
    public int getVoice() { return (int)mpData.Voice; }

    bool myAppeared = false;
    public bool isAppeared() { return myAppeared; }

    public bool getLeaderOrg(out Vector3 org)
    {
        org = Vector3.zero;
        if (mpLeader != null)
            org = mpLeader.GetOrg();
        return (mpLeader != null);

    }
    public bool getPlayerOrg(out Vector3 org)
    {
        org = Vector3.zero;
        if (mpFirstPlayer != null)
            org = mpFirstPlayer.GetOrg();
        return (mpFirstPlayer != null);

    }

    ContactInfoHolder myContacts;
    EngageInfoHolder myEngagers;

    bool processUnits(float scale)
    {
        //Debug.LogFormat("Processing Units of group {0} mGhostCount {1} hidden {2}",mpData.Callsign,mGhostCount,mHidden);
        bool someone_has_subobjects = false;

        if (mGhostCount != 0 && !mHidden)
        {
            bool update_units = false;
            GroupStatus someone_landed_or_repaired = GroupStatus.GROUP_STATUS_GHOST;
            bool someone_updated = false;
            for (int i = 0; i < mGhostCount; i++)
            {
                AiUnit un = mpUnits[i];
                bool process_respawn = false;
                UnitStatus last_unit_status = UnitStatus.usGhost;
                someone_updated |= !un.IsGhost();
                //if (mpData.Callsign == "Alpha") Debug.LogFormat("Unit {0} Ghost: {1} can respawn: {2} ", mpData.Callsign,un.IsGhost(),un.CanRespawnNow().ToString());
                if (un.IsGhost())
                {
                    process_respawn = un.CanRespawnNow();
                }
                else
                {
                    bool ret = un.GetAI().Update(scale);
                    myStdMsn.setContext(mpData.Callsign);
                    if (!ret)
                    {
                        last_unit_status = ProcessLandedNotify(un);
                        OnUnitLost(un, last_unit_status == UnitStatus.usKilled);
                                                mpMission.OnDeleteContactHandle(un.GetHandle(), last_unit_status != UnitStatus.usKilled);
                        if (un.CanRespawn())
                        {
                            mAliveCount--;
                            mUpdateAlive = true;
                            un.UnitData().Cleared();
                            un.Ghost();
                        }
                        else
                            process_respawn = true;
                    }
                    else
                    {
                        if (un.HasSubContacts())
                        {
                            iContact cnt = un.GetAI().GetContact();
                            if (!cnt.HasSubContacts())
                            {
                                SendNotify(MessageCodes.AUTO_UNIT, AIGroupsEvents.MC_UNIT_LOSTDETACHED, un.UnitData().Number + 1, "OnUnitLostDetached");
                                un.SetSubContacts(false);
                            }
                            else
                                someone_has_subobjects |= true;
                        }
                    }
                }
                //Debug.Log("Unit [" + mpData.Callsign + " " + (i+1) + "] process_respawn " + (process_respawn ? "true":"false"));
                someone_landed_or_repaired = (GroupStatus)Mathf.Max((int)someone_landed_or_repaired, (int)getGroupStatus(last_unit_status));
                if (process_respawn)
                {
                    update_units |= ProcessRespawn(ref un, scale);
                    if (!un.IsGhost())
                        someone_has_subobjects |= un.HasSubContacts();
                    CheckPlayerLeadership();
                }
            }
            processGroupNotify(someone_updated, (uint)someone_landed_or_repaired);
            if (update_units)
                UpdateUnits();
        }
        return someone_has_subobjects;
    }

    static GroupStatus getGroupStatus(UnitStatus stat)
    {
        switch (stat)
        {
            case UnitStatus.usKilled: return GroupStatus.GROUP_STATUS_KILLED;
            case UnitStatus.usRepaired: return GroupStatus.GROUP_STATUS_REPAIRED;
            case UnitStatus.usLanded: return GroupStatus.GROUP_STATUS_LANDED;
        }
        return GroupStatus.GROUP_STATUS_GHOST;
    }

    void processGroupNotify(bool someone_updated, DWORD someone_landed_or_repaired)
    {
        if (!IsGroupAlive() && someone_updated && !mNotifiedAboutDisappear && someone_landed_or_repaired != (uint)GroupStatus.GROUP_STATUS_GHOST)
        {
            if (!mpData.IsInCampaign())
            {
                switch ((GroupStatus)someone_landed_or_repaired)
                {
                    case GroupStatus.GROUP_STATUS_KILLED: SendNotify(MessageCodes.AUTO_GROUP, AIGroupsEvents.MC_GROUP_KILLED, 0, "OnGroupKill"); break;
                    case GroupStatus.GROUP_STATUS_LANDED: SendNotify(MessageCodes.AUTO_GROUP, AIGroupsEvents.MC_GROUP_LANDED, 0, "OnGroupLand"); break;
                    case GroupStatus.GROUP_STATUS_REPAIRED: SendNotify(MessageCodes.AUTO_GROUP, AIGroupsEvents.MC_GROUP_REPAIR, 0, "OnGroupRepair"); break;
                }
            }
            mNotifiedAboutDisappear = true;
        }

    }
    ActionStatus processActions(float scale)
    {
        //Debug.Log(string.Format("Processing Actions for group {0} mpCurrentAction {1}", mpData.Callsign, mpCurrentAction));
        ActionStatus status = new ActionStatus();
        if (mpCurrentAction != null)
        { // if action exists
            status = mpCurrentAction.Update(scale);  // update action
            if (status.IsActionDead())
            {     // if action dead so process this
                Debug.Log(string.Format("Action {0} is dead and should be popped for group {1}", mpCurrentAction, mpData.Callsign));
                if (PopAction())                    // pop this action
                    OnActionDeath();                // query for actions
                else
                    OnEmptyActions();
            }
            else if (status.IsActionDeactivated())
            {
                mpCurrentAction.DeActivate();  // deactivate action
                OnActionDeactivate();           // query for actions
            }
        }
        if (mActionsCandidates.Count != 0)
            ProcessCandidates();

        return status;

    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }
}
enum AiUnitState : DWORD
{
    ausGhost,
    ausAlive,
    ausDead,
    ausForceDword = Constants.THANDLE_INVALID
};

public class AiUnit
{
    AiUnitState mState;
    DWORD mHandle;
    public UNIT_DATA mpUnitData;
    IBaseUnitAi mpAi;
    RespawnType mType;
    RespawnInfo mpInfo;
    bool mHasSubContactsOnLastTick;


    void ReleaseInfo()
    {
        if (mpInfo != null)
        {
            //delete mpInfo;
            mpInfo = null;
        }
    }
    void Set(IBaseUnitAi p)
    {
        if (mpAi != p)
        {
            SafeRelease(mpAi);
            //if (mpAi = p)
            //    mpAi->AddRef();
            if (p != null)
            {
                mpAi = p;
                mpAi.AddRef();
            }
        }

    }
    // init
    public AiUnit()
    {
        mpAi = null;
        mState = AiUnitState.ausDead;
        mpUnitData = null;
        mType = RespawnType.rtDead;
        mHandle = Constants.THANDLE_INVALID;
        mHasSubContactsOnLastTick = false;
    }
    ~AiUnit()
    {
        //SafeRelease(mpAi);
        ReleaseInfo();
    }
    public void Initialize(UNIT_DATA dt) { mpUnitData = dt; mState = AiUnitState.ausGhost; }

    public IBaseUnitAi GetAI() { return mpAi; }
    // operators
    //AiUnit & operator = (const AiUnit& p);
    //IBaseUnitAi* operator -> () const { return mpAi; }
    //IBaseUnitAi &    operator *() const { return *mpAi;}
    //operator IBaseUnitAi*() const { return mpAi; }

    // contacts
    public bool HasSubContacts() { return mHasSubContactsOnLastTick; }
    public void SetSubContacts(bool fl) { mHasSubContactsOnLastTick = fl; }

    public void SafeRelease(object obj)
    {
        //STUB! do nothing;
    }
    // states
    public void Alive(IBaseUnitAi ai)
    {
        mState = AiUnitState.ausAlive;
        //        SafeRelease(mpAi);
        mpAi = ai; iContact cnt = ai.GetContact();
        if (cnt != null)
        {
            mHandle = cnt.GetHandle();
            mHasSubContactsOnLastTick = cnt.HasSubContacts();
        }
    }
    public void Ghost() { mState = AiUnitState.ausGhost; SafeRelease(mpAi); mpAi = null; mHandle = Constants.THANDLE_INVALID; }
    public void Dead() { mState = AiUnitState.ausDead; SafeRelease(mpAi); mpAi = null; mHandle = Constants.THANDLE_INVALID; }

    public bool IsAlive() { return mState == AiUnitState.ausAlive; }
    public bool IsGhost() { return mState == AiUnitState.ausGhost; }
    public bool IsDead() { return mState == AiUnitState.ausDead; }

    public bool CanRespawnNow() { return (mType != RespawnType.rtDead && mType != RespawnType.rtRespawnNever); }
    public bool CanRespawn() { return (mType != RespawnType.rtDead); }

    public void HasBeenRespawned() { mType = RespawnType.rtDead; }
    public RespawnInfo GetRespawnInfo() { return mpInfo; }
    public void SetRespawnInfo(RespawnType type, RespawnInfo info, OverwriteType ot)
    {
        mType = type;
        if (mpInfo == null)
            ot = OverwriteType.otOverwriteFull;
        switch (ot)
        {
            case OverwriteType.otOverwriteFull:
                {
                    RespawnInfo tmp = (info != null) ? new RespawnInfo(info) : null;
                    ReleaseInfo();
                    mpInfo = tmp;
                }
                break;
            case OverwriteType.otOverwriteUnitSpawnData:
                {
                    if (info == null) throw new System.Exception("Incorrect info value");
                    //AssertBp(info);
                    //mpInfo.Merge((const UnitSpawnData*)info);
                    mpInfo.Merge((UnitSpawnData)info);
                }
                break;
            case OverwriteType.otOverwriteRespawnInfo:
                {
                    if (info == null) throw new System.Exception("Incorrect info value");
                    mpInfo.Merge(info);
                    //AssertBp(info);
                    //mpInfo->Merge(info);
                }
                break;
        }
    }

    public void SetOrg(Vector3 org)
    {
        if (mpInfo != null)
            mpInfo.mOrg = org;
    }

    public RespawnType GetRespawnType() { return mType; }
    public DWORD GetHandle() { return mHandle; }
    public UNIT_DATA UnitData() { return mpUnitData; }
};

public class PriorityGroup
{
    public DWORD grp;
    public float coeff;
    public PriorityGroup(DWORD _grp, float cf)
    {
        grp = _grp;
        coeff = cf;
    }
};

public class EnemyCount
{
    public DWORD mEnemyCount;
    public DWORD mRealEnemyCount;

    public void Inc() { mEnemyCount++; }
    public void IncReal() { mRealEnemyCount++; }
    public void Clear() { mEnemyCount = mRealEnemyCount = 0; }
    public EnemyCount() { Clear(); }
};


enum GroupStatus
{
    GROUP_STATUS_GHOST = 0,
    GROUP_STATUS_KILLED,
    GROUP_STATUS_LANDED,
    GROUP_STATUS_REPAIRED
};

enum UnitStatus
{
    usGhost,
    usKilled,
    usLanded,
    usRepaired
};

public abstract class StdGroupAiDefs
{
    public const float CHECK_ENEMY_TIME = 4f;


    public const int ALL_ENGAGE = 0;
    public const int SELF_ENGAGE = 1;
    public const int ENEMY_ENGAGE = 2;

    public const int SEND_TO_ALL = 0;
    public const int SEND_TO_LEADER = 1;
    public const int SEND_TO_MEMBERS = 2;

    public const int INCLUDE_ONLY_OBJECTS = 0;
    public const int INCLUDE_ONLY_SUBOBJECTS = 1;
    public const int INCLUDE_OBJECTS_AND_SUBOBJECTS = 2;

}

public abstract class MissionSideDefines
{
    public const int SIDE_MISSION = 555;
    public static bool IsSideNeutral(DWORD side) { return (side == SIDE_MISSION || side == CampaignDefines.CS_SIDE_NEUTRAL); }

}

