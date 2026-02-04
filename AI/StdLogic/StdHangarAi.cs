using System;
using System.Collections.Generic;
using crc32 = System.UInt32;
using DWORD = System.UInt32;
using AiLanding = Landing<ContactHolder>;
using AiLandingWait = LandingWait<ContactHolder>;
using AiHangarTerminal = HangarTerminal<iContact, ContactHolder, iContact>;
using UnityEngine;

//typedef Landing<ContactHolder> AiLanding;
//typedef LandingWait<ContactHolder> AiLandingWait;
//typedef HangarTerminal<iContact, ContactHolder, iContact> AiHangarTerminal;

public class StdHangarAi : BaseAi, IHangarAi, IIgnorePool<ContactHolder>
{

    new const crc32 ID = 0x4CBFD0E2;
    const string HangarMissed = "create: can't create hangar without subhangars";
    List<SendedInfo> mySendedClears = new List<SendedInfo>();
    void emulateMessage(IGroupAi gai, DWORD stevent, string cs)
    {
        StdGroupAi std_grp_ai = (StdGroupAi)gai.Query(StdGroupAi.ID);
        if (std_grp_ai != null && std_grp_ai.GetEventDesigner().canProcessBaseEvents())
        {
            RadioMessage msg = new RadioMessage();
            msg.CallerCallsign = cs;
            RadioMessageInfo rmi = new RadioMessageInfo(true, false, true);
            rmi.myWaitTime = 20;
            rmi.mySayIfExceed = false;
            // zzzzzzzzzzzzzzz
            msg.VoiceCode = (int)std_grp_ai.GetUnitVoice(0);
            Debug.Log("Emulate message: " + msg);
            mpEventDesigner.ProcessExternalEvent(std_grp_ai.GetRequestEventCode(stevent), msg, rmi, false, mpData.Number + 1, null, false, EventType.etNotCheck);
        }
    }
    public virtual int wantToTakeoff(crc32 callsign, int idx)
    {
        updateSended();
        SendedInfo sent = new SendedInfo(callsign, idx);
        if (mySendedClears.Count == 0)
            return -1;
        for (int i = 0; i < mySendedClears.Count; i++)
            if (mySendedClears[i] == sent)
                return 1;
        return 0;
    }
    // Hangar
    List<AiHangarTerminal> myTerminals = new List<AiHangarTerminal>();

    public virtual void handleTerminalsChange()
    {
        if (myTerminals.Count == 0)
            throw new Exception(HangarMissed);
    }

    // takeoff 
    AiHangarTerminal canHandleUnit(DWORD name, int ab, bool immediate)
    {
        for (int i = 0; i < myTerminals.Count; ++i)
        {
            AiHangarTerminal trm = myTerminals[i];
            if ((!immediate || trm.isFree()) && trm.canHandleUnit(name, ab))
                return trm;
        }
        return null;
    }
    AiHangarTerminal canHandleUnit(iContact cnt, int ab, bool immediate)
    {
        for (int i = 0; i < myTerminals.Count; ++i)
        {
            AiHangarTerminal trm = myTerminals[i];
            if ((!immediate || trm.isFree()) && trm.canHandleUnit(cnt, ab))
                return trm;
        }
        return null;
    }

    LandingQueue<ContactHolder> myLandQueue;

    void updateSended()
    {
        StdMissionAi std = (StdMissionAi)mpMission.Query(StdMissionAi.ID);
        if (std == null) return;
        for (int i = 0; i < mySendedClears.Count;)
        {
            bool alive = std.GetContactByIndex(mySendedClears[i].myName, mySendedClears[i].myIdx) != null;
            if (!alive)
            {
                if (mySendedClears[i].hasBeenAlive)
                {
                    mySendedClears.RemoveAt(i);
                    continue;
                }
            }
            else
                mySendedClears[i].hasBeenAlive = alive;
            ++i;
        }
    }

    public virtual void abortLanding(ContactHolder hld)
    {
        string player_name = hld.getPlayerName(mpMission);
        if (player_name != null)
            hld.sendMessage(mpEventDesigner, self.Ptr(), hld.getRequestEventCode(StdGroupAi.REQUEST_FAIL), true, 0.5f, player_name, 1, 0, true, RadioMessageInfo.DEFAULT_WAIT_TIME, EventType.etBase);

        hld.myData.NotLanded();
        if (hld.mpGrp != null && hld.mpGrp.mpCurrentAction != null)
            hld.mpGrp.mpCurrentAction.updatePoint();
        hld.sendMessage(mpEventDesigner, self.Ptr(), null, false, 0.2f, hld.getGroupCallsign(), 0, AiRadioMessages.RM_NOTIFY_LAND_FAILED, true, RadioMessageInfo.DEFAULT_WAIT_TIME, EventType.etBase);
    }

    StdGroupAi mStdGroup;
    GROUP_DATA mGrpData;
    IMissionAi mpMission;

    bool mLand, mTakeoff;

    //void               SendMessageToHangarUser(HangarUser* usr, const char*, bool, float post_time, const char*, DWORD code=0, bool critical=true);
    public int ProcessQueue(float scale) // обработка очереди
    {
        myLandQueue.update(); // eliminate dead users

        int ret = myLandQueue.getCount();

        for (int i = 0; i < myTerminals.Count; ++i)
        {
            AiHangarTerminal trm = myTerminals[i];
            trm.update(scale);
            bool free_term = trm.isFree();
            if (!free_term) ret++;
            if (myLandQueue.getCount() != 0)
            {
                if (free_term)
                {
                    for (int j = 0; j < myLandQueue.getCount();)
                    {
                        ContactHolder hld = myLandQueue.getContact(j);
                        bool can_land = trm.canHandleUnit(hld.myContact.Ptr(), (int)iSubHangar.CAN_ACCEPT_LAND);
                        crc32 hs1 = Hasher.HshString(hld.getGroupCallsign());
                        if (can_land && !mpMission.presentMessage(hld.mpGrp, hs1, mpMission.GetMessageCode(hld.mpGrp.GetRequestEventCode(StdGroupAi.REQUEST_IN)).mCode, true, (int)hld.myData.Number + 1))
                        {
                            string player_name = hld.getPlayerName(mpMission);
                            if (player_name != null && hld.mySpeakAnswer)
                            {
                                hld.sendMessage(mpEventDesigner, trm.getContact(), hld.getRequestEventCode(StdGroupAi.CLEAR_IN), true, 0.5f, player_name, (int)mpData.Number + 1, 0, true, 20, EventType.etExternal, false); //???
                                hld.mySpeakAnswer = false;
                            }
                            if (!mpMission.presentMessage(GetIGroupAi(), Hasher.HshString(player_name), mpMission.GetMessageCode(hld.getRequestEventCode(StdGroupAi.CLEAR_IN)).mCode, false, (int)hld.myData.Number + 1))
                            {
                                ContactHolder cnt = myLandQueue.extractContact(j);
                                trm.activateLand(hld);
                                Debug.Log("Hangar ai sending message for " + cnt.getGroupCallsign() + " " + cnt);
                                cnt.sendMessage(mpEventDesigner, trm.getContact(), null, false, 0.2f, cnt.getGroupCallsign(), (int)mpData.Number + 1, AiRadioMessages.RM_NOTIFY_LAND_CLEARED, true);
                            }
                            break;
                        }
                        else
                            ++j;
                    }
                }
            }
        }
        return ret;

    }

    // type cast
    public void SetStatus(bool land, bool takeoff)
    {
        mLand = land;
        mTakeoff = takeoff;
    }
    IHangarAi GetIHangarAi() { return this; }

    public StdHangarAi()
    {
        mStdGroup = null;
        mGrpData = null;
        mLand = true;
        mTakeoff = true;
        myHangarUnit = new HangarUnit<StdHangarAi>(this);
        myLandQueue = new LandingQueue<ContactHolder>();
    }

    // api
    ~StdHangarAi() { }

    // IAi
    public override object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case IHangarAi.ID: return GetIHangarAi();
            case IHangarUnit.ID: return myHangarUnit; //TODO Вернуть на место определение ангароюнита в группе
            default: return base.Query(cls_id);
        }
    }

    // IHangarAi 
    public virtual iContact RequestToTakeoff(IGroupAi grp_ai, UNIT_DATA dt, out bool appear)
    {
        Asserts.AssertBp(grp_ai != null);
        self.Validate();
        appear = self.Ptr() != null;

        if (appear)
        {
            AiHangarTerminal cnt = canHandleUnit(dt.CodedName, (int)iSubHangar.CAN_ACCEPT_TAKEOFF, true);
            //Debug.Log("Terminal " + cnt.ToString());
            if (cnt != null)
            {
                StdGroupAi std_grp_ai = (StdGroupAi)grp_ai.Query(StdGroupAi.ID);
                string discard = null;
                string player_name = mpMission.IsPlayer(dt, ref discard );
                if (player_name == null)
                    player_name = std_grp_ai.mpData.Callsign;

                //int send_idx = mySendedClears.find(new SendedInfo(Hasher.HshString(std_grp_ai.mpData.Callsign), (int)dt.Number + 1));
                int send_idx = -1;
                for (int i = 0; i < mySendedClears.Count; i++)
                {
                    if (mySendedClears[i] == new SendedInfo(Hasher.HshString(std_grp_ai.mpData.Callsign), (int)dt.Number + 1))
                    {
                        send_idx = i;
                        break;
                    }
                }
                if (-1 != send_idx && !mpMission.presentMessage(GetIGroupAi(), Hasher.HshString(player_name), mpMission.GetMessageCode(std_grp_ai.GetRequestEventCode(StdGroupAi.CLEAR_OUT)).mCode, true) && mpMission.RadioChannelIsFree())
                {
                    mySendedClears.RemoveAt(send_idx);
                    return cnt.getContact();
                }
                // it must be variable
                Asserts.Assert(mySendedClears.Count == 0 || mySendedClears.Count == 1);
                if (std_grp_ai != null && mySendedClears.Count == 0)
                {
                    RadioMessage msg = new RadioMessage();
                    msg.RecipientCallsign = player_name;
                    msg.RecipientIndex = (int)dt.Number + 1;
                    RadioMessageInfo rmi = new RadioMessageInfo(true, false, true);
                    rmi.myWaitTime = 20;
                    rmi.mySayIfExceed = false;
                    msg.VoiceCode = (int)mStdGroup.GetUnitVoice(mpData.Number + 1);
                    if (std_grp_ai.GetEventDesigner().canProcessBaseEvents())
                        mpEventDesigner.ProcessExternalEvent(std_grp_ai.GetRequestEventCode(StdGroupAi.CLEAR_OUT), msg, rmi, false, mpData.Number + 1, null, false, EventType.etNotCheck); //???
                    mySendedClears.Add(new SendedInfo(Hasher.HshString(std_grp_ai.mpData.Callsign), (int)dt.Number + 1));
                }
                appear = false;
            }
        }
        return null;
    }
    public virtual void RequestToLand(IGroupAi grp_ai, IBaseUnitAi ai, bool repair)
    {
        Asserts.AssertBp(grp_ai != null);
        self.Validate();
        if (self.Ptr() != null)
        {
            if (myLandQueue.hasContact<iContact>(ai.GetContact()) == false)
            {
                ContactHolder hld = new ContactHolder(grp_ai, ai, repair, true, grp_ai.GetEventDesigner().canProcessBaseEvents());
                myLandQueue.addContact(hld);
            }
        }
    }
    public virtual int GetCountToLand()
    {
        return myLandQueue.getCount();
    }
    public virtual bool AbleToTakeOff(GROUP_DATA gr, UNIT_DATA dt, bool immediate = true)
    {
        self.Validate();
        return (self.Ptr() != null && mScriptParsed &&
            mTakeoff &&
            canHandleUnit(dt.CodedName, (int)iSubHangar.CAN_ACCEPT_TAKEOFF, immediate) != null);
    }
    public virtual bool AbleToLand(iContact cnt, bool immediate = true)
    {
        self.Validate();
        return (self.Ptr() != null && mScriptParsed &&
            mLand &&
            canHandleUnit(cnt, (int)iSubHangar.CAN_ACCEPT_LAND, immediate) != null);
    }
    public virtual bool IsLandWork()
    {
        return mScriptParsed && mLand && myTerminals.Count != 0;
    }
    public virtual bool IsTakeoffWork()
    {
        return mScriptParsed && mTakeoff && myTerminals.Count != 0;
    }
    public virtual void SetTakeoffMember(iContact _ai)
    {
        AiHangarTerminal cnt = canHandleUnit(_ai, (int)iSubHangar.CAN_ACCEPT_TAKEOFF, true);
        if (cnt != null)
            cnt.activateTakeoff(_ai);
    }

    public virtual void emulateTakeoffRequest(IGroupAi gai, string cs)
    {
        emulateMessage(gai, StdGroupAi.REQUEST_OUT, cs);
    }


    // IBaseAi
    public override void SetInterface(IGame igame, iContact contact, UNIT_DATA unit_data, IGroupAi grp_ai)
    {
        base.SetInterface(igame, contact, unit_data, grp_ai);
        mStdGroup = (StdGroupAi)mpGroup.Query(StdGroupAi.ID);
        Asserts.AssertBp(mStdGroup != null);
        mGrpData = mStdGroup.GetGroupData();
        mpMission = mStdGroup.GetIMissionAi();
        myStdHangarAiFactory = Factories.createStdHangarAiFactory(getIQuery(), myBaseAiFactory);
    }
    public override void SideChanged(iContact new_cnt)
    {
        base.SideChanged(new_cnt);
        iSubHangar sub_hangar;
        myTerminals.Clear();
        //Debug.Log("Subhangar seme " + self);
        //Debug.Log("Subhangar seme " + self.Ptr());
        //for (int n = 0; (sub_hangar = (iSubHangar)self.Ptr().queryObject<iSubHangar>(n)) != null; ++n)
        for (int n = 0; (sub_hangar = (iSubHangar)self.Ptr().queryObject(iSubHangar.ID, n)) != null; ++n)
        {
            iContact hng = sub_hangar.getIContact();
            Debug.Log(string.Format("{0} subhangar: {1} icontact {2}", this.mpGroup, sub_hangar, hng));
            myTerminals.Add(new AiHangarTerminal(sub_hangar, this, hng));
        }
        handleTerminalsChange();
    }

    HangarUnit<StdHangarAi> myHangarUnit;

    IVmFactory myStdHangarAiFactory;
    public override IVmFactory getTopFactory() { return myStdHangarAiFactory; }
}

class SendedInfo
{
    public SendedInfo() { }
    public SendedInfo(crc32 n, int i)
    {
        myName = n;
        myIdx = (uint)i;
        hasBeenAlive = false;
    }
    public crc32 myName;
    public DWORD myIdx;
    public bool hasBeenAlive;

    //bool operator !=(SendedInfo si)
    //{
    //    return myName != si.myName || myIdx != si.myIdx;
    //}
    //bool operator ==(SendedInfo si)
    //{
    //    return myName == si.myName && myIdx == si.myIdx;
    //}
    public static bool operator !=(SendedInfo left, SendedInfo right)
    {
        return left.myName != right.myName || left.myIdx != right.myIdx;
    }
    public static bool operator ==(SendedInfo left, SendedInfo right)
    {
        return left.myName == right.myName && left.myIdx == right.myIdx;
    }



    public override int GetHashCode()
    {
        return HashCode.Combine(myName, myIdx, hasBeenAlive);
    }

    public override bool Equals(object obj)
    {
        return obj is SendedInfo info &&
               myName == info.myName &&
               myIdx == info.myIdx &&
               hasBeenAlive == info.hasBeenAlive;
    }
}

public class ContactHolder : IObject
{
    public ContactHolder(IGroupAi grp_ai, IBaseUnitAi ai, bool repair, bool set, bool speak_answer)
    {
        myAi = ai;
        mySpeakAnswer = speak_answer;
        ref_count = 1;
        //myContact = new TContact(myAi.GetContact());
        myContact.setPtr(myAi.GetContact());
        myData = myAi.GetUnitData();
        mpGrp = (StdGroupAi)(grp_ai.Query(StdGroupAi.ID));
        if (myData != null && set)
            myData.Landed(repair);
    }

    public StdGroupAi mpGrp;
    public IBaseUnitAi myAi;
    public bool mySpeakAnswer;
    public readonly TContact myContact = new TContact();
    public UNIT_DATA myData;
    int ref_count;

    public iContact getContact() { return myContact.Ptr(); }

    public virtual int Release()
    {
        Asserts.Assert(ref_count > 0);
        //if (!(--ref_count))
        //{
        //    delete this;
        //    return 0;
        //}
        return ref_count;
    }

    public virtual void AddRef()
    {
        ++ref_count;
    }

    public bool isDead()
    {

        myContact.Validate(); 
        return myContact.Ptr() == null;
    }
    public bool isLandingComplete()
    {
        return isDead();
    }
    public bool isManual()
    {
        return myContact.Ptr().IsManualCotrolled();
    }
    public void sendMessage(iEventDesigner des, iContact self, string event_code, bool say_flag, float post_time, string name, int caller_index, DWORD code = 0, bool critical = true, float wt = RadioMessageInfo.DEFAULT_WAIT_TIME, EventType lt_message = EventType.etExternal, bool sie = true)
    {
        RadioMessage Info = new RadioMessage();
        Info.RecipientCallsign = name;
        Info.RecipientIndex = (int)myData.Number + 1;
        Info.TargetContact = self;
        Info.TargetAi = myAi;

        Debug.Log("Sending message from hangar: " + Info + "\ncode " + event_code);
        RadioMessageInfo rmi = new RadioMessageInfo(wt, post_time, critical, false, say_flag);
        rmi.mySayIfExceed = sie;
        if (event_code != null)
            des.ProcessExternalEvent(event_code, Info, rmi, false, (uint)caller_index, null, true, lt_message);
        else
            des.ProcessExternalEvent(code, Info, rmi, false, (uint)caller_index, null, true, lt_message);
    }

    public string getRequestEventCode(DWORD evt)
    {
        return mpGrp.GetRequestEventCode(evt);
    }
    public string getPlayerName(IMissionAi msn)
    {
        string discard = null;
        string player_name = msn.IsPlayer(myData, ref discard);
        if (player_name == null)
            player_name = getGroupCallsign();
        return player_name;
    }
    public string getGroupCallsign()
    {
        return mpGrp.GetGroupData().Callsign;
    }

    public int RefCount()
    {
        return ref_count;
    }
}


class LandingQueue<T> where T : ContactHolder //TODO Вот странно, зачем так, когда можно просто ContactHolder делать?
{

    public void addContact(T contact)
    {
        myQueue.Add(new LandingQuery<T>(contact));
    }

    public T extractContact()
    {
        if (myQueue.Count == 0) return null;
        return removeElement(0);
    }

    public T extractContact(int i)
    {
        if (i >= myQueue.Count) return null;
        return removeElement(i);
    }

    public T getContact(int i)
    {
        if (i >= myQueue.Count) return null;
        return myQueue[i].getContact();
    }

    //public bool hasContact<M>(M contact) where M:iContact
    //{
    //    for (int i = 0; i < myQueue.Count; ++i)
    //        if (myQueue[i].getContact().getContact() == contact)
    //            return true;
    //    return false;
    //}

    public bool hasContact<M>(iContact contact)
    {
        for (int i = 0; i < myQueue.Count; ++i)
            if (myQueue[i].getContact().getContact() == contact)
                return true;
        return false;
    }

    public void update()
    {
        for (int i = 0; i < myQueue.Count;)
        {
            LandingQuery<T> q = myQueue[i];
            if (q.updateStatus() == LandingQuery<T>.Status.ABORTED)
            {
                myQueue.RemoveAt(i); //TODO - Возможен вечный цикл
            }
            else
                ++i;
        }
    }

    public int getCount()
    {
        return myQueue.Count;
    }

    private T removeElement(int i)
    {
        LandingQuery<T> ret = myQueue[i];
        T rezult = ret.getContact();
        rezult.AddRef();
        myQueue.RemoveAt(i);
        return rezult;
    }

    //AnyDTab<LandingQuery<T>*> myQueue;
    List<LandingQuery<T>> myQueue = new List<LandingQuery<T>>();
};

class LandingQuery<T> where T : ContactHolder
{
    //typedef T Contact;

    public enum Status { WAIT, ABORTED };

    public LandingQuery(T contact)
    {
        myContact = contact;
        myStatus = Status.WAIT;
        myContact.AddRef();
    }

    public Status updateStatus()
    {
        if (myContact.isDead())
            myStatus = Status.ABORTED;
        return myStatus;
    }
    public T getContact()
    {
        return myContact;
    }

    private T myContact;
    Status myStatus;

};

class HangarTerminal<TakeOff, Land, Hangar> where Land : ContactHolder where TakeOff : iContact where Hangar : class,iContact
{
    //typedef Landing<Land>     LocalLanding;
    //typedef LandingWait<Land> LocalLandingWait;

    public override string ToString()
    {
        string res = GetType().ToString();
        res += "\nBody=" + this.myBody;
        res += "\nmyHangar="+this.myHangar;
        return res;

    }
    public bool isFree()
    {
        return myBody != null && myHangar.isDoorClosed() && myTakeoff == null && myCurrentLandingWait == null && myCurrentLanding == null;

    }
    public void activateTakeoff(TakeOff tk)
    {
        myTakeoff = tk;
        tk.AddRef();
    }

    public void activateLand(Land tk)
    {
        //myCurrentLandingWait = new LocalLandingWait(tk);
        myCurrentLandingWait = new LandingWait<Land>(tk);
    }

    public bool canHandleUnit(DWORD CodedName, int wanted_ability)
    {
        int ab = myHangar.canHandleUnit(CodedName);
        return ab != 0 & wanted_ability != 0;
    }

    public bool canHandleUnit(iContact pUnit, int wanted_ability)
    {
        int ab = myHangar.canHandleUnit(pUnit);
        return ab != 0 & wanted_ability != 0;
    }

    public void update(float scale)
    {
        if (myBody != null && myBody.isDead())
            //myBody = null; //TODO Возможно, правильнее возвращать всё-таки null 
            myBody = default(Hangar);
        if (myTakeoff != null && (myTakeoff.isDead() || myTakeoff.isInGame()))
            //myTakeoff = null;
            myTakeoff = default(TakeOff);
        if (myCurrentLandingWait != null)
        {
            Land hld = myCurrentLandingWait.getContact();
            #region удалить после отладки
            var myStatus = myCurrentLandingWait.updateStatus(scale);
            Debug.Log("myCurrentLandingWait status: " + myStatus);
            switch (myStatus) 
            #endregion
            //switch (myCurrentLandingWait.updateStatus(scale))
            {
                case LandingWait<Land>.Status.WAIT:
                    break;
                case LandingWait<Land>.Status.COMPLETE:
                    myCurrentLanding = new Landing<Land>(hld);
                    clearLandWait();
                    break;
                case LandingWait<Land>.Status.ABORTED:
                    if (myPool != null) myPool.abortLanding(hld);
                    clearLandWait();
                    break;
                case LandingWait<Land>.Status.DEAD:
                    if (myPool != null) myPool.abortLanding(hld);
                    clearLandWait();
                    break;
            }
        }
        else if (myCurrentLanding != null)
        {
            #region удалить после отладки
            var myStatus = myCurrentLanding.updateStatus();
            //Debug.Log("myCurrentLanding status: " + myStatus + " " + myCurrentLanding.getContact().getContact());
            switch (myStatus)
            #endregion
            //switch (myCurrentLanding.updateStatus())
            {
                case Landing<Land>.Status.ABORTED:
                    if (myPool != null)
                        myPool.abortLanding(myCurrentLanding.getContact());
                    clearLand();
                    break;
                case Landing<Land>.Status.COMPLETE:
                    Debug.Log("Landing is complete for " + myCurrentLanding.getContact().getGroupCallsign() + " unit");
                    clearLand();
                    break;
            }
        }

    }

    void clearLandWait()
    {
        myCurrentLandingWait = null;
    }

    void clearLand()
    {
        myCurrentLanding = null;
    }

    public Hangar getContact() { return myBody; }


    ~HangarTerminal()
    {
        clearLandWait();
        clearLand();
        //myTakeoff = null;
        myTakeoff = default(TakeOff);
    }

    public HangarTerminal(iSubHangar hng, IIgnorePool<Land> pool, Hangar body)
    {
        myHangar = hng;
        myCurrentLandingWait = null;
        myCurrentLanding = null;
        myPool = pool;
        myBody = body;

        myBody.AddRef();
    }

    iSubHangar myHangar;
    TakeOff myTakeoff;
    Hangar myBody;

    LandingWait<Land> myCurrentLandingWait;
    Landing<Land> myCurrentLanding;
    IIgnorePool<Land> myPool;
}
interface IIgnorePool<T2>
{
    public void abortLanding(T2 t);
};


class HangarUnit<T> : IHangarUnit where T : StdHangarAi
{
    public virtual void setStatus(bool land, bool takeoff)
    {
        myMsn.SetStatus(land, takeoff);
    }

    public HangarUnit(T imp)
    {
        myMsn = imp;

    }

    T myMsn;
};
