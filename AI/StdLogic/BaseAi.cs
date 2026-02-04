using UnityEngine;
using DWORD = System.UInt32;
using crc32 = System.UInt32;
//using AI_STATE = System.Boolean;
using System.Collections.Generic;

public abstract class BaseAi : IBaseUnitAi, IQuery, ISkillable
{
    public const uint ID = 0x1B17856D;

    const string ContactMissed = "create: can't create unit without iContact";
    const string SensorsMissed = "create: can't create unit without iSensors";

    //public delegate bool AI_STATE(StateFlag flag, float scale);
    public IBaseUnitAi.AI_STATE iState;

    public bool SafeCallUpdate(StateFlag flag, float scale) { if (iState != null) return iState(flag, scale); return false; }
    public bool CallUpdate(StateFlag flag, float scale) { return iState(flag, scale); }
    public void SetState(IBaseUnitAi.AI_STATE new_state) { SafeCallUpdate(StateFlag.sfDone, 0); iState = new_state; CallUpdate(StateFlag.sfInit, 0); }

    // external data and interfaces
    protected IGame igame;
    public UNIT_DATA mpData;
    int mRefCount;
    bool mSuicided;
    bool mSilence;
    protected bool IsTalking() { return mSilence == false; }

    bool myHangaring;

    // self group
    protected IGroupAi mpGroup;
    IMissionAi mpMission;


    // messages
    protected iEventDesigner mpEventDesigner;

    // sequences
    bool executeScript(string name, string text, string namesp) {
        if (myStdMsn !=null && text!=null && text !="")
        {
            myStdMsn.setSource(name, text);
            if (myVm!=null)
                return myVm.parseScript(text, namesp);
        }
        return true;

    } 
    public bool mScriptParsed;

    public void AddDamage(float coeff, DWORD name)
    {
        List<iContact> cnts;
        getSubObject(out cnts, name);
        for (int i = 0; i < cnts.Count; ++i)
            addDamage(cnts[i], coeff);
    }


    // self variables
    iSensors sensors;
    public readonly TContact self = new TContact();
    public int mType;

    // skills
    protected DWORD mSkill;
    public BaseSkill mpSkill;
    void ClearSkill()
    {
        if (mpSkill != null)
        {
            //delete mpSkill;
            mpSkill.Dispose();
            mpSkill = null;
        }

    }


    // type cast
    public IAi GetIAi() { return this; }
    public IBaseUnitAi GetIBaseUnitAi() { return this; }


    IBaseUnitAi.AI_STATE SetRandomState(IBaseUnitAi.AI_STATE state1, IBaseUnitAi.AI_STATE state2, float prob)
    {
        if (RandomGenerator.Rand01() < prob)
            return state1;
        return state2;

    }

    // api
    public BaseAi()
    {
        /*
             BaseAi() :self(0),igame(0),mpData(0),mRefCount(1),mpSkill(0),mSuicided(false),
           mpEventDesigner(0),mScriptParsed(false),mpMission(0),mSilence(true),myHangaring(false),mySender(this),
          mySkillService(this,false),myUnitService(this) { }
        */
        //self = new TContact();
        mRefCount = 1;
        //mySkillService
        mSuicided = false;
        mScriptParsed = false;
        mSilence = true;
        myHangaring = false;
        //mySender = this;
        //myUnitService = this;
        mySender = new UnitSender<BaseAi, StdGroupAi>(this);
        myUnitService = new UnitService<BaseAi>(this);
        mySkillService = new SkillService<BaseAi>(this, false);
    }

    ~BaseAi()
    {
        Dispose();
    }
    public virtual void Dispose() {
        ClearSkill();
        mpEventDesigner = null;
        //SafeRelease(mpEventDesigner);
        //self = null;
        self.setPtr(null);
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
        switch ((uint)cls_id)
        {
            case IAi.ID: return GetIAi();
            case IBaseUnitAi.ID: return GetIBaseUnitAi();
            case BaseAi.ID: return this;
            case ITimeService.ID: return myStdMsn.getTimer();
            case IRadioService.ID: return myStdMsn.getRadio();
            case IRadioSender.ID: return mySender;
            case ISkillService.ID: return mySkillService;
            case IUnitService.ID: return myUnitService;
            case IErrorLog.ID: return myStdMsn.getErrorLog();
        }
        return null;

    }

    // IAi
    public virtual bool Update(float scale)
    {
        bool ret = self.Validate();

        if (ret)
        {
            if (!mScriptParsed)
            {
                //Debug.Log("Creating new factory for " + " mpGroup " + mpGroup.GetGroupData().Callsign + " " + mpData.Number);
                //Debug.Log(string.Format("Creating new factory for {0} [{1} {2}]",mpGroup.GetType().ToString(),mpGroup.GetGroupData().Callsign,mpData.Number));
                myVm.setFactory(getTopFactory());
                executeScript(myNumber, mpData.AiScript, "UnitProperty");
                mScriptParsed = true;
            }

            if (myStdMsn != null)
            {
                myStdMsn.setContext(myNumber);
                myVm.run();
            }

            myHangaring = iSensors.IsHangaring(self.Ptr().GetState(), myHangaring);
            //ret = mSuicided ? false : CallUpdate(StateFlag.sfRun, scale);
            ret = false;
            //Debug.Log(string.Format("BaseAI update for {0} {1} {2}",mpGroup.GetGroupData().Callsign, mpData.Number+1,iState == null ? "Empty AI state":iState.Method));
            if (iState != null) //TODO! Выяснять, почему null
            {
                ret = mSuicided ? false : iState(StateFlag.sfRun, scale);
            }
        }
        myRefreshOnRequest = false;
        return ret;

    }
    public virtual void ProcessRadioMessage(string msg_code, IAi caller, RadioMessage Info, bool to_all, bool say_flag) { }

    // IBaseUnitAi
    public virtual IGroupAi GetIGroupAi()
    {
        return mpGroup;
    }

    public virtual DWORD GetSkill()
    {
        return mSkill;
    }

    public virtual void SetSkill(DWORD skill, bool already_setted = true)
    {
        mSkill = skill;
        if (!already_setted)
        {
            if (mpSkill != null)
                mpSkill = null;
            iUnifiedVariableContainer pskill = Skills.GetSkillContainer(skill);
            mpSkill = new BaseSkill(pskill);
        }

    }
    public virtual void Suicide()
    {
        mSuicided = true;
    }
    public virtual iSensors GetSensors()
    {
        return sensors;
    }
    public virtual UNIT_DATA GetUnitData()
    {
        return mpData;
    }
    public virtual iContact GetContact()
    {
        self.Validate();
        return self.Ptr();

    }
    public virtual string GetStateName()
    {
        return "Empty";
    }
    public virtual void OnDamage(DWORD gad, float damage, bool fatal)
    {
        if (IsTalking())
        {
            float cond = self.Ptr().GetCondition();
            if (cond <= 0f)
            {
                iContact main = self.Ptr().GetNextSubContact(null, 0xC9CAB1D8);
                float main_cond = main.GetCondition();
                if (main_cond > 0f)
                    mpEventDesigner.ProcessExternalEvent(AIGroupsEvents.HIT_FATAL, 0, "", new RadioMessageInfo(false, true, true), 0, "", false, EventType.etInternal);
            }
            else if (cond < 0.1f)
            {
            }
            else if (cond < 0.45f)
                mpEventDesigner.ProcessExternalEvent(AIGroupsEvents.HIT_HEAVY, 0, "", new RadioMessageInfo(false, true, true), 0, "", false, EventType.etInternal);
            else if (cond < 0.8f)
                mpEventDesigner.ProcessExternalEvent(AIGroupsEvents.HIT_LIGHT, 0, "", new RadioMessageInfo(false, true, true), 0, "", false, EventType.etInternal);
        }
        myRefreshOnRequest = true;

    }
    public virtual void OnKill(DWORD victim)
    {
        if (IsTalking())
        {
            mpEventDesigner.ProcessExternalEvent(AIGroupsEvents.DONE_ATTACK, 0, "", new RadioMessageInfo(false, true, true), 0, "", false, EventType.etInternal);
        }

    }
    public virtual void SetMessagesMode(bool silence)
    {
        mSilence = silence;
        if (IsTalking())
            mpMission.RegisterDamageUser(GetIBaseUnitAi());

    }
    public virtual bool IsHangaringBeforeLastValidate()
    {
        return myHangaring;
    }

    // IBaseAi
    public virtual void SideChanged(iContact new_cnt)
    {
        //self = new TContact(new_cnt);
        self.setPtr(new_cnt);
        if (self.Ptr() == null)
            throw new System.Exception(ContactMissed);
        mType = self.Ptr().GetProtoType();
        sensors = (iSensors)self.Ptr().GetInterface(iSensors.ID);
        if (sensors == null)
            throw new System.Exception(SensorsMissed);
    }
    public virtual void SetInterface(IGame _igame, iContact contact, UNIT_DATA _data, IGroupAi grp_ai)
    {
        igame = _igame;
        mpData = _data;
        mpGroup = grp_ai;
        StdGroupAi stdgrp = (StdGroupAi)mpGroup.Query(StdGroupAi.ID);
        mpMission = stdgrp != null ? stdgrp.GetIMissionAi() : null;
        SideChanged(contact);
        mpEventDesigner = mpGroup.GetEventDesigner();
        myStdMsn = mpMission != null ? (StdMissionAi)mpMission.Query(StdMissionAi.ID) : null;
        if (myStdMsn != null)
        {
            myVm = myStdMsn.createVm();
            myBaseAiFactory = Factories.createBaseAiFactory(getIQuery());
        }
        //Debug.Log(myNumber.lock(64),"Group \"%s\", Unit %d",stdgrp !=null ? stdgrp.mpData.Callsign: Parsing.sAiEmpty,mpData.Number + 1);
        //Debug.Log(string.Format("Group \"{0}\", Unit {1}", stdgrp != null ? stdgrp.mpData.Callsign : Parsing.sAiEmpty, mpData.Number + 1));
        mySender.setGroup(stdgrp);
    }



    // new
    public IVm myVm;
    public IVmFactory myBaseAiFactory;
    StdMissionAi myStdMsn;

    UnitService<BaseAi> myUnitService;
    UnitSender<BaseAi, StdGroupAi> mySender;
    SkillService<BaseAi> mySkillService;
    //public virtual IVmFactory getTopFactory() { return null; }
    public abstract IVmFactory getTopFactory();
    public virtual IQuery getIQuery() { return this; }

    public bool getOrg(out Vector3 org)
    {
        org = Vector3.zero;
        if (self.Ptr() != null)
            org = self.Ptr().GetOrg();
        //return self;
        return true;
    }

    //MString myNumber;
    string myNumber;
    bool myWeaponEnabled = true;
    public bool isWeaponsEnabled() { return myWeaponEnabled; }

    bool myRegistered = false;
    bool myRefreshOnRequest = true;
    void getSubObject(out List<iContact> contacts, DWORD name)
    {
        contacts = new List<iContact>();
        if (self.Ptr() != null)
        {
            if (Constants.THANDLE_INVALID == name)
                contacts.Add(self.Ptr().GetTopContact());
            else
            {
                iContact sub = null;
                while (true)
                {
                    sub = self.Ptr().GetNextSubContact(sub, name);
                    if (sub != null && sub != self.Ptr())
                        contacts.Add(sub);
                    else
                        break;
                }
            }
        }

    }
    void addDamage(iContact cnt, float coeff)
    {
        iBaseVictim vict = (iBaseVictim) cnt.GetInterface(iBaseVictim.ID);
        if (vict != null)
            vict.AddDamage(Constants.THANDLE_INVALID, 0, vict.GetTotalLife() * coeff);
    }

    public virtual void fillParts(string[] names, ref List<PartInfo> tab)
    {
        for (int i = 0; i < names.Length; ++i)
        {
            crc32 id = Hasher.HshString(names[i]);
            Debug.Log("Gettung Subobject " + names[i] + " id " + id.ToString("X8"));
            List<iContact> cnts;
            getSubObject(out cnts, id);
            int n = cnts.Count;
            if (n == 0) n++;
            for (int j = 0; j < n; ++j)
            {
                PartInfo part = new PartInfo
                {
                    myName = id,
                    myCondition = cnts.Count != 0 ? cnts[j].GetCondition() : 0
                };//{ id, cnts.Count() ? cnts[j]->GetCondition() : 0 };
                tab.Add(part);
            }
        }

    }
    public virtual void refreshParts(ref List<PartInfo> tab)
    {
        if (!myRegistered)
        {
            mpMission.RegisterDamageUser(GetIBaseUnitAi());
            myRegistered = true;
        }
        if (myRefreshOnRequest)
        {
            PartInfo tmpPartInfo;
            for (int i = 0; i < tab.Count;)
            {
                crc32 id = tab[i].myName;

                List<iContact> cnts;
                getSubObject(out cnts, id);
                int j = 0;
                while (i < tab.Count && tab[i].myName == id)
                {
                    tmpPartInfo = tab[i];
                    if (j < cnts.Count)
                    {
                        tmpPartInfo.myCondition = cnts[j].GetCondition();
                        j++;
                    }
                    else
                        tmpPartInfo.myCondition = 0;

                    tab[i] = tmpPartInfo;
                    i++;
                }
            }
        }

    }
    public virtual void stopRefresh()
    {
        if (myRegistered)
        {
            mpMission.UnRegisterDamageUser(GetIBaseUnitAi());
            myRegistered = false;
        }

    }
    public virtual void setFireMode(bool enable)
    {
        myWeaponEnabled = enable;
    }
    public virtual iContact GetTarget() { return null; }

    public virtual void enumTargets(ITargetEnumer en) { }

    public static void enumTurretsTargets(iWeaponSystemTurrets turrets, ITargetEnumer en)
    {
        if (turrets != null)
        {
            iWeaponSystemDedicated turr = null;
            while ((turr = turrets.GetNextTurret(turr)) != null)
            {
                iContact cnt = turr.GetTarget();
                if (cnt != null)
                    en.processTarget(cnt, turr.GetAim());
            }
        }
    }

    public virtual float GetAiming()
    {
        throw new System.NotImplementedException();
    }

    public virtual void SelectTarget(int nTargets, iContact[] Targets, float[] TargetWeights)
    {
        throw new System.NotImplementedException(this.GetType().ToString());
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }
}
public class UnitService<T> : IUnitService where T : BaseAi
{
    public virtual void addDamage(string name, float coeff)
    {
        myMsn.AddDamage(coeff, Hasher.HshString(name));
    }

    public virtual void fill(string[] names, ref List<PartInfo> tab)
    {
        myMsn.fillParts(names, ref tab);
    }

    public virtual void refresh(ref List<PartInfo> tab)
    {
        myMsn.refreshParts(ref tab);
    }

    public virtual void stopRefresh()
    {
        myMsn.stopRefresh();
    }

    public virtual void setFireMode(bool enable)
    {
        myMsn.setFireMode(enable);
    }

    public UnitService(T imp)
    {
        myMsn = imp;
    }

    T myMsn;
};

public class UnitSender<T1, T2> : IRadioSender
    where T1 : BaseAi
    where T2 : StdGroupAi
{
    public virtual bool getOrg(out Vector3 org)
    {
        return myAi.getOrg(out org);
    }

    public virtual CallerInfo getInfo()
    {
        CallerInfo info = new CallerInfo
        {
            myCallsign = myGrp.mpData.Callsign,
            myIndex = (int)myAi.mpData.Number
        };

        return info;
    }

    public virtual IAi getAi()
    {
        return myAi.GetIAi();
    }

    public virtual int getSide()
    {
        return (int)myGrp.mpData.Side;
    }

    public virtual int getType()
    {
        return MessageCodes.CMD_GROUP;
    }

    public virtual IErrorLog getLog()
    {
        return myGrp != null ? myGrp.getErrorLog() : null;
    }

    public virtual int getVoice()
    {
        return myGrp.getVoice();
    }

    public virtual bool isPlayable()
    {
        return false;
    }


    public void setGroup(T2 grp) { myGrp = grp; }
    public UnitSender(T1 d)
    {
        myAi = d;
    }

    T1 myAi;
    T2 myGrp;
};


public static class Skills
{
    public static iUnifiedVariableContainer GetSkillContainer(DWORD skill)
    {
        iUnifiedVariableContainer skills = stdlogic_dll.mpAiData != null ? stdlogic_dll.mpAiData.GetVariableTpl<iUnifiedVariableContainer>("Skills") : null;
        return skills != null ? skills.GetVariableTpl<iUnifiedVariableContainer>(GetSkillName(skill)) : null;
    }

    public static string GetSkillName(DWORD name)
    {
        switch (name)
        {
            case SkillDefines.SKILL_NOVICE: return BaseSkill.sNovice;
            case SkillDefines.SKILL_VETERAN: return BaseSkill.sVeteran;
            case SkillDefines.SKILL_ELITE: return BaseSkill.sElite;
        }
        return Parsing.sAiEmpty;
    }

}
