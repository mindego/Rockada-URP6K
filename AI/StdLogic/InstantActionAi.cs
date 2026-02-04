//#define _DEBUG
using System;
using System.Reflection;
using UnityEngine;
using static IParamList;
using static SkillDefines;
using DWORD = System.UInt32;
using static AICommon;
using static Parsing;
using static MessageCodes;
using UnityEditor;
public class AppearInfo : TLIST_ELEM<AppearInfo>, IComparable<AppearInfo>, IDisposable
{
    private AppearInfo next, prev;
    public AppearInfo Next()
    {
        return next;

    }
    public AppearInfo Prev()
    {
        return prev;
    }

    public void SetNext(AppearInfo appearInfo)
    {
        next = appearInfo;
    }
    public void SetPrev(AppearInfo appearInfo)
    {
        prev = appearInfo;
    }

    float mTime;
    DWORD mCount;
    DWORD mSkill;
    GROUP_DATA mpData;
    public void SetTime(float tm) { mTime = tm; }
    public void SetCount(DWORD mc) { mCount = mc; }
    public void SetSkill(DWORD mc) { mSkill = mc; }
    public DWORD GetID() { return mpData.ID; }
    public string GetName() { return mpData.Callsign; }
    public bool IsHidden() { return mpData.IsHidden(); }
    public DWORD GetSkill() { return mSkill; }
    public DWORD GetCount() { return mCount; }
    public float GetTime() { return mTime; }

    public AppearInfo(GROUP_DATA dt)
    {
        mpData = dt;
        mTime = InstantActionAi.DEFAULT_APPEAR_TIME;
        mCount = 1;
        mSkill = SKILL_NOVICE;
    }
    public void NextTurn(GROUP_DATA grp_data)
    {
        mCount++;
        if (mCount > grp_data.nUnits)
        {
            DWORD skill = NextSkill(mSkill);
            if (skill != mSkill)
            {
                mCount = 1;
                mSkill = skill;
            }
            else
                mCount = grp_data.nUnits;
        }
    }
    DWORD NextSkill(DWORD skill)
    {
        switch (skill)
        {
            case SKILL_NONE: return SKILL_NOVICE;
            case SKILL_NOVICE: return SKILL_VETERAN;
            default: return SKILL_ELITE;
        }
    }

    public int CompareTo(AppearInfo other)
    {
        if (this.GetName() == null && other.GetName() == null) return 0;
        return this.GetName().CompareTo(other.GetName());
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
};
public class InstantActionAi : StdCooperativeAi,IDisposable
{
    public const uint INSTANT_ACTION_VERSION = 0x00000010;
    public const float DEFAULT_APPEAR_TIME = 10f;


    #region headers file
    
    new public const uint ID = 0xEF61B9D5;
    TLIST<AppearInfo> mlAppearData = new TLIST<AppearInfo>();
    AppearInfo[] mpAppearIndex;

    bool mGroupKilled;

    GroupAiCont mpCurrentGroup;

    InstantMission<InstantActionAi> myMission;
    IVmFactory myInstantMsnFactory;
    public override IVmFactory getTopVmFactory()
    {
        return myInstantMsnFactory;
    }

    public InstantActionAi() {
        mpCurrentGroup = null;
        mGroupKilled = false;
        mpAppearIndex = null;
        myMission = new InstantMission<InstantActionAi>(this);
    }
    #endregion headers file

    #region Section : common and initialize
    public override void SetInterface(IGame _igame, IServer _iserver, AiMissionData msn_data)
    {
        base.SetInterface(_igame, _iserver, msn_data);
        SetDeleteUnitFlag(false);
        InitializeAppearInfo();
        setMissionType('I');

        myInstantMsnFactory = InstantMsnFactory.createInstantMsnFactory(getIQuery(), myCoopFactory);
    }

    new public const string MyMissionMsg = "^DInstant Action v.0.1"; // TODO - надо бы как-то генерировать.  AssemblyFileVersionAttribute?
    //new static string MyMissionMsg = string.Format("^DInstant Action v.%d.%d " __TIME__ " " __DATE__);

    public override string GetMissionDescription()
    {
        return MyMissionMsg;
    }

    public override DWORD GetMissionVersion()
    {
        return INSTANT_ACTION_VERSION;
    }

    public override object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case InstantActionAi.ID: return this;
            case IInstantMission.ID: return myMission;
            default: return base.Query(cls_id);
        }
    }

    ~InstantActionAi()
    {
        Dispose();
    }
    public void Dispose()
    {
        mlAppearData.Free();
        if (mpAppearIndex != null) mpAppearIndex = null;
    }
    #endregion Section : common and initialize

    #region Section : appear management
    public GroupAiCont GetNextGroupToAppear(GroupAiCont cur)
    {
        DWORD num;
        if (cur == null)
            num = 0;
        else
        {
            FindAppearInfo(cur.mpGroupData.ID, out num);
            Asserts.AssertBp(num != Constants.THANDLE_INVALID);
            num++;
        }
        while (num < mlAppearData.Counter() && !mpAppearIndex[num].IsHidden())
            num++;
        return (num == mlAppearData.Counter()) ? null : FindGroupCont(mpAppearIndex[num].GetID());
    }

    public static int AppearCompare(object arg1, object arg2)
    {
        AppearInfo d1 = (AppearInfo)arg1;
        AppearInfo d2 = (AppearInfo)arg2;
        //return StrCmp(d1.GetName(), d2.GetName());
        //return d1.GetName() == d2.GetName() ? 0 : 1; //TODO возможно, сравнение строк нужно вынести в отдельный служебный статический метод
        return d1.GetName().CompareTo(d2.GetName());
    }

    public void InitializeAppearInfo()
    {
        for (int i = 0; i < mpData.GetGroupsCount(); ++i)
        {
            GROUP_DATA Grp = mpData.GetGroups()[i];
            mlAppearData.AddToTail(new AppearInfo(Grp));
        }
        if (mlAppearData.Counter() != 0)
        {
            mpAppearIndex = new AppearInfo[mlAppearData.Counter()];
            int n = 0;
            for (AppearInfo hd = mlAppearData.Head(); hd != null; hd = hd.Next())
                mpAppearIndex[n++] = hd;
            //qsort(mpAppearIndex, n, sizeof(AppearInfo), AppearCompare);
            Array.Sort(mpAppearIndex, AppearCompare);
        }
    }

    UNIT_DATA[] getPosition(GROUP_DATA grp_data, int offset)
    {
        return grp_data.Units;
    }

    public override bool Update(float scale)
    {
        if (mpCurrentGroup == null || mGroupKilled)
        {
            Debug.LogFormat("INSTANTAI: Group {0} Killed. Spawning new", mpCurrentGroup == null? "none":mpCurrentGroup.mpGroupData);
            mpCurrentGroup = GetNextGroupToAppear(mpCurrentGroup);
            Debug.LogFormat("INSTANTAI: mpCurrentGroup Killed. Spawning new {0}", mpCurrentGroup == null ? "none" : mpCurrentGroup.mpGroupData);
            if (mpCurrentGroup != null)
            {
                AppearInfo info = FindAppearInfo(mpCurrentGroup.mpGroupData.ID);
                Asserts.AssertBp(info);
                GroupAppear(mpCurrentGroup, (int)info.GetCount(), info.GetSkill());
            }
            mGroupKilled = false;
        }
        return base.Update(scale);
    }

    void GroupKilled(GroupAiCont cnt)
    {
        AppearInfo info = FindAppearInfo(cnt.mpGroupData.ID);
        Asserts.AssertBp(info);
        info.NextTurn(cnt.mpGroupData);
        mGroupKilled = true;
    }

    void GroupAppear(GroupAiCont cnt, int count, DWORD skill)
    {
        IGroupAi grp = cnt.ai;
        StdGroupAi std_grp = (StdGroupAi)grp.Query(StdGroupAi.ID);
        if (std_grp != null)
        {
            AppearInfo ainfo = FindAppearInfo(cnt.mpGroupData.ID);
            Asserts.AssertBp(ainfo);
            SetSkill(ainfo.GetSkill(), false);
            std_grp.StartAppearing(1.0f, ainfo.GetCount());
        }
    }

    void GroupKilledAndMustAppear(GroupAiCont cnt)
    {
        IGroupAi grp = cnt.ai;
        StdGroupAi std_grp = (StdGroupAi)grp.Query(StdGroupAi.ID);
        if (std_grp != null)
        {
            AppearInfo ap_info = FindAppearInfo(cnt.mpGroupData.ID);
            Asserts.AssertBp(ap_info);
            std_grp.StartAppearing(ap_info.GetTime(), cnt.mpGroupData.ID);
        }
    }

    float GetPrimaryCompletedCoeff(bool primary_completed)
    {
        return 1;
    }

    public override void ProcessContactDeath(ContactInfo info, bool landed_or_repaired)
    {
        base.ProcessContactDeath(info, landed_or_repaired);
        StdGroupAi grp = (StdGroupAi)info.mpAi.GetIGroupAi().Query(StdGroupAi.ID);
        if (grp != null && 1 == grp.mAliveCount)
        {
            if (info.mpGroupCont == mpCurrentGroup)
                GroupKilled(info.mpGroupCont);
            else if (!info.mpGroupCont.mpGroupData.IsHidden() && info.mpData != null && info.mpData.IsLanded(true) == false && landed_or_repaired == false)
                GroupKilledAndMustAppear(info.mpGroupCont);
        }
    }

    AppearInfo FindAppearInfo(DWORD id)
    {
        DWORD tmpnum;
        return FindAppearInfo(id, out tmpnum);
    }
    AppearInfo FindAppearInfo(DWORD id, out DWORD num)
    {
        num = 0;
        for (int i = 0; i < mlAppearData.Counter(); ++i)
            if (mpAppearIndex[i].GetID() == id)
            {
                num = (uint)i;
                return mpAppearIndex[i];
            }
        return null;
    }

    public void AddAppearTime(float tm, string name)
    {
        DWORD id = Hasher.HashString(name);
        GroupAiCont cont = FindGroupCont(id);
        if (cont != null)
        {
            AppearInfo info = FindAppearInfo(id);
            Asserts.AssertBp(info);
            info.SetTime(tm);
        }
        else if (IsLogged(DEBUG_MEDIUM))
        {
            AiMessage(MSG_ERROR, sAiWarning, "group \"{0}\" not found in AppearTime", name);
        }
    }
    #endregion Section : appear management


}
