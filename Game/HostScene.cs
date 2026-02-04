#define _DEBUG

using System;
using System.Collections.Generic;
using UnityEngine;
using crc32 = System.UInt32;
using DWORD = System.UInt32;
using static WpnDataDefines;



public class HostScene : BaseScene, IGame, IServer, IAiRegister
{
    List<AiModuleData> ai_modules = new List<AiModuleData>();
    List<AiData> ai_datas = new List<AiData>();

    private IMissionAi ai_mission;
    public AiMissionData mpAiData;
    BaseClient mpLocalClient;
    //STUB
    public HostScene(StormGameData d, bool rm) : base(d, rm) { }

    public override void applyCraftSelection(ref UnitSpawnData dy)
    {
        mpLocalClient.applyCraftSelection(dy);
    }
    public string AddLocalizedText(ref string pDst, int DstLength, DWORD Code, string pString = null)
    {
        return base.AddLocalizedText(ref pDst, DstLength, Code, pString);
    }
    public BaseClient GetLocalClient() { return mpLocalClient; }

    public iUnifiedVariableContainer GetCurrentEventData()
    {
        return getCurrentEventData();
    }

    public void onMenuShortcut(int index, iClient cl)
    {
        AiMenuItem[] battleMenu = new AiMenuItem[PlayerInterface.MAX_AI_MENU_ITEMS];

        int nItems = GetMissionAI().GetMenuItems(cl, IBase.UndefinedID, battleMenu, PlayerInterface.MAX_AI_MENU_ITEMS, 1);
        //dprintf("onMenuShortcut: nItems=%d\n",nItems);
        if (Storm.Math.isInRange(index, 0, nItems - 1) && battleMenu[index].myEnabled)
        {
            //dprintf("onMenuShortcut: item is enabled\n");
            GetMissionAI().SelectItem(cl, battleMenu[index].MenuID);
        } //else
          //dprintf("onMenuShortcut: item is disabled\n");
    }
    public virtual GameOptions getGameOptions() { return GetGameData(); }
    static bool IsSideMatched(int DataSide, int SideCode)
    {
        switch (DataSide)
        {
            case 0: return (SideCode == 1 || SideCode == 0);
            case 1: return (SideCode == 2 || SideCode == 0);
            case 2: return (SideCode == 0 || SideCode == 1);
            default: return (SideCode == DataSide || SideCode == 0);
        }
    }

    void PrepareAIDlls(iUnifiedVariableContainer pCtr)
    {
        //STUB!
        iUnifiedVariableString pString = pCtr.GetVariableTpl<iUnifiedVariableString>("AiDlls");

        if (pString == null) return;

        string[] dllNames = pString.GetValue().Split(' ');
        foreach (string dllName in dllNames)
        {
            if (dllName == string.Empty) continue;
            Debug.Log(string.Format("Loading AiDLL: \"{0}\"", dllName));
            AddDll(dllName);
        }


    }

    /// <summary>
    /// // один тик игры (false => если сервер хочет закончить)
    /// </summary>
    /// <param name="scale"></param>
    /// <returns>false => если сервер хочет закончить</returns>
    public override bool Update(float scale)
    {
        //stub!
        bool mpLocalClient = true;
        int TimeScale = 1;

        if (GetSceneVisualizer() != null && mpLocalClient == false) return false;

        if (!base.Update(scale)) return false;
        ai_mission.setFcScale(scale);

        if (ai_mission.Update(scale * TimeScale) == false)
            return false;
        // двигаем и переключаем камеру
        if (pScene != null) pScene.Update(scale);
        return true;

    }
    /// <summary>
    /// Add AI dll
    /// </summary>
    /// <param name="dll"></param>
    /// <returns>true if new dll</returns>
    bool AddDll(string dll)
    {
        foreach (AiModuleData h in ai_modules)
        {
            if (h.filename == dll) return false;
        }
        ai_modules.Add(new AiModuleData(dll));
        return true;
    }

    TLIST<BaseClient> ClientsList = new TLIST<BaseClient>(true);
    public void AddClient(BaseClient _cl)
    {
        ClientsList.AddToTail(_cl);
    }

    public void SubClient(BaseClient _cl)
    {
        ClientsList.Sub(_cl);
    }


    public override void Init()
    {
        Debug.Log("Host scene init!");
        base.Init();

        InitAIEnvironment();

        if (GetSceneVisualizer() != null)
        {
            mpLocalClient = new LocalClient(this);
        }
        else
        {
            //mpLocalClient = new DedicatedClient(this);
        }
        //registerCommands();

    }

    int LoadAIDlls()
    {
        int n = 0;
        //for (AiModuleData h = ai_modules.Head(); h; h = ai_modules.Next(h))
        //{
        //    if (h->Initialize(this, this, GetLog()))
        //        n++;
        //    else
        //        Message("Warning: could't load AI module \"%s\"", h->filename);
        //}
        foreach (AiModuleData h in ai_modules)
        {
            Debug.Log("Loading " + h.filename);
            if (h.Initialize(this, this, GetLog()))
                n++;
            else
                Debug.Log(String.Format("Warning: could't load AI module \"{0}\"", h.filename));
        }
        return n;
    }

    public void Patch()
    {

    }
    void InitAIEnvironment()
    {
        Debug.Log("Initializing AI dlls:");
        //int m = GetLog().GetIdent();
        //GetLog()->SetIdent(m + 1);
        Debug.Log("Loading default mission AIs");
        PrepareAIDlls(GetGameData().mpDefaultMission);
        Debug.Log("Loading selected mission AIs");
        //PrepareAIDlls(GetGameData().mpMission); //ОШ
        PrepareAIDlls(GetGameData().mpNonLocalizedMission);//СН
        int n = LoadAIDlls();
        //GetLog()->SetIdent(m);
        Debug.Log(string.Format("DLLs : {0} resolved , {1} loaded", ai_modules.Count, n));
        Debug.Log(string.Format("AIs  : {0} registered", ai_datas.Count));
        Debug.Log("Initializing AI data:");
        if (n != 0)
        {
            mpAiData = new AiMissionData();
            //mpAiData.Init(GetGameData().mpDefaultMission, GetGameData().mpMission); //ОШ
            mpAiData.Patch(new MissionPatch());
            mpAiData.Init(GetGameData().mpDefaultMission, GetGameData().mpNonLocalizedMission); //СН
            AppendSucceeded();
            ai_mission = CreateMissionAi();
            Debug.Log("InitAIEnvironment: " + (mpAiData != null ? mpAiData : "FAILED!"));
        }
        else
        {
            mpAiData = null;
            ai_mission = null;
        }
        if (ai_mission == null)
            throw new Exception("Error : unable to create mission AI!");
    }

    void DoneAIEnvironment()
    {
        //SafeRelease(ai_mission);
        ai_datas.Clear();
        ai_modules.Clear();
        if (mpAiData != null) { mpAiData = null; }
    }

    AiData FindAiData(int cls_id, crc32 code)
    {
        foreach (AiData h in ai_datas)
        {
            if (h.Equal(cls_id, code)) return h;
        }
        //for (AiData* h = ai_datas.Head(); h; h = ai_datas.Next(h))
        //    if (h->Equal(cls_id, code))
        //        return h;
        return null;
    }

    public bool RegisterAi(IAiModule module, string name, int lev_id)
    {
        crc32 t = Hasher.HshString(name);
        AiData aidata = FindAiData(lev_id, t);
        if (aidata != null)
        {
#if _DEBUG
            GetLog().Message(String.Format("Warning: AI ({0} \"{1}\") already registered.", GetClName(lev_id), name));
#endif //_DEBUG
            return false;
        }
        AiModuleData dll = FindAiModule(module);
        if (dll == null) return false;
        aidata = new AiData(name, lev_id, dll, t);
        ai_datas.Add(aidata);
#if _DEBUG
        GetLog().Message(String.Format("{0}.{1} \"{2}\"", ai_datas.Count, GetClName(lev_id), name));
#endif
        return true;
    }

    AiModuleData FindAiModule(IAiModule module)
    {
        foreach (AiModuleData h in ai_modules)
        {
            if (h.ai_module == module) return h;
        }
        return null;

    }

    public string GetClName(int cls_id)
    {
        const string clMission = "mission";
        const string clDGroup = "dgroup";
        const string clSGroup = "sgroup";
        const string clStatic = "static";
        const string clVehicle = "vehicle";
        const string clCraft = "craft";
        const string clCarrier = "carrier";
        const string clEmpty = "unknown";

        switch ((uint)cls_id)
        {
            case CampaignDefines.CF_CLASS_MISSION: return clMission;
            case CampaignDefines.CF_CLASS_STATIC_GROUP: return clSGroup;
            case CampaignDefines.CF_CLASS_DYNAMIC_GROUP: return clDGroup;
            case CampaignDefines.CF_CLASS_STATIC: return clStatic;
            case CampaignDefines.CF_CLASS_VEHICLE: return clVehicle;
            case CampaignDefines.CF_CLASS_CRAFT: return clCraft;
            case CampaignDefines.CF_CLASS_AIR_CARRIER: case CampaignDefines.CF_CLASS_SEA_CARRIER: return clCarrier;
        }
        return clEmpty;
    }

    public IMissionAi CreateMissionAi()
    {
        iUnifiedVariableString pAi = GetNonLocalizedData().GetVariableTpl<iUnifiedVariableString>("Ai"); //СН
        //iUnifiedVariableString pAi = GetGameData().mpMission.GetVariableTpl<iUnifiedVariableString>("Ai"); //ОШ
        //char pName[MAX_PATH];
        //pAi->StrnCpy(pName, MAX_PATH);
        string pName = pAi.GetValue();
        // ищем данные по AI
        crc32 t = Hasher.HshString(pName);
        AiData aidata = FindAiData((int)CampaignDefines.CF_CLASS_MISSION, t);
        if (aidata == null)
        {
            //Log().Message(string.Format("Warning: AI (mission \"{0}\") not found, skipped.", pName));
            GetLog().Message(string.Format("Warning: AI (mission \"{0}\") not found, skipped.", pName));
            return null;
        }
        // создаем AI
        //return new StdCooperativeAi();
        return aidata.dll.ai_module.CreateMissionAi(t, this, this, mpAiData);
    }


    private enum MissileType
    {
        TORPEDO,
        ANTIAIR,
        ANTIGROUND,
        HTGR
    }
    private ProjectileMissile GetMissile(WPN_DATA_MISSILE dt)
    {
        //ProjectileMissile ps = null;
        //switch (missileType)
        //{
        //    case MissileType.TORPEDO:
        //        ps=new ProjectileTorpedo(this, Constants.THANDLE_INVALID); break;
        //    case MissileType.ANTIAIR:
        //        ps=new ProjectileMissile(this, Constants.THANDLE_INVALID); break;
        //    //case MissileType.HTGR:
        //    //    ps = new ProjectileHTGR(this, THANDLE_INVALID); break;
        //}
        //if (dt.Type == WT_HTGR) return new ProjectileHTGR(this, THANDLE_INVALID);

        //if (dt.LockAngle == 0f) return new ProjectileTorpedo(this, Constants.THANDLE_INVALID);
        return new ProjectileGuidedMissile(this, Constants.THANDLE_INVALID);

    }

    internal ProjectileMissile CreateMissile(WPN_DATA_MISSILE dt, iContact owner, iContact tgt, MATRIX Pos)
    {
        ProjectileMissile ps = null;
        // зависимость от типа
        switch (dt.Type)
        {
            case WT_MISSILE: ps = new ProjectileMissile(this, Constants.THANDLE_INVALID); break;
            //case WT_HTGR: ps = new ProjectileHTGR(this, THANDLE_INVALID); break;
            default: Asserts.AssertEx(false); break;
        }

        //ps = GetMissile(dt);

        // готовим к игре
        //try
        //{
        ps.HostPrepare(dt, owner, tgt, Pos);
        //}
        //catch (System.Exception e)
        //{
        //    //delete ps;
        //    ps.Dispose();
        //    throw e;
        //}
        OnCreateItem(ps);
        return ps;
    }

    // послать сообщение о создании
    void OnCreateItem(BaseItem i)
    {
        for (BaseClient cl = ClientsList.Head(); cl != null; cl = cl.Next())
        {
            if (cl.IsInGame() == false) continue;
            //cl.SendItemCreate(i);
        }
    }

    public BaseSubobj CreateSubobj(BaseObject o, BaseSubobj s, SUBOBJ_DATA sd, FPO fpo, SLOT_DATA sld = null, int slot_id = -1, int layout_id = -1)
    {
        BaseSubobj so = SubobjFactory.CreateSubobj(this, sd, Constants.THANDLE_INVALID);
        // готвим к игре
        //try
        //{
        so.HostPrepare((HostScene)this, o, s, fpo, sld, slot_id, layout_id);
        //}
        //catch
        //{
        //    throw;
        //}

        OnCreateItem(so);
        return so;
    }

    /// <summary>
    /// Respawn
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="org"></param>
    /// <param name="angle"></param>
    /// <param name="hangar"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public iContact CreateUnit(UnitSpawnData dt, Vector3 org, float angle, iContact hangar)
    {
        //Debug.Log("Creating unit " + dt.ObjectName.ToString("X8"));
        OBJECT_DATA od = OBJECT_DATA.GetByCode((uint)dt.ObjectName);
        //Debug.Log("unit name " + od.FullName);

        //Debug.Log("Creating unit " + dt.ObjectName.ToString("X8") + " " + od.FullName + " type " + od.GetClassName() );
        //Debug.Log(string.Format("Creating unit {0} {1} type {2} sides IsSideMatched {3}", dt.ObjectName.ToString("X8"),od.FullName, od.GetClassName(), IsSideMatched(od.Side, (int)dt.SideCode).ToString()));
        if (GetMissionAI().canUseOtherSideData() &&
            od.pOtherSideData != null &&
            IsSideMatched(od.Side, (int)dt.SideCode) == false) od = od.pOtherSideData;
        BaseObject o;
        switch (od.GetClass())
        {
            case OBJECT_DATA.OC_CRAFT: o = new BaseCraft(this, Constants.THANDLE_INVALID, od); break;
            case OBJECT_DATA.OC_STATIC: o = new BaseStatic(this, Constants.THANDLE_INVALID, od); break;
            case OBJECT_DATA.OC_VEHICLE: o = new BaseVehicle(this, Constants.THANDLE_INVALID, od); break;
            case OBJECT_DATA.OC_AIRSHIP: o = new AirCarrier(this, Constants.THANDLE_INVALID, od); break;
            case OBJECT_DATA.OC_SEASHIP: o = new SeaCarrier(this, Constants.THANDLE_INVALID, od); break;
            //case OBJECT_DATA.OC_HANGAR: o = new BaseStatic(this, Constants.THANDLE_INVALID, od); break;
            //case OBJECT_DATA.OC_HANGAR: o = new BaseStaticHangar(this, Constants.THANDLE_INVALID, od); break;//TODO - возможно, потребуется изменить создание статических ангаров  для работы с СН

            default: throw new Exception("Unknown unit class " + od.GetClass().ToString("X8"));
        }
#if _DEBUG
        o.HostPrepare(this, dt, org, angle, hangar);
#else
        try
        {
            o.HostPrepare(this, dt, org, angle, hangar);
        }
        catch (Exception e)
        {
            o = null;
            throw e;
        }
#endif
        OnCreateItem(o);
        //return o->GetInterface<iContact>();
        //return (iContact)o;
        return (iContact)o.GetInterface(iContact.ID);
    }

    public override IMissionAi GetMissionAI() { return ai_mission; }

    //public IMissionAi CreateMissionAi()
    //{
    //    return new StdCooperativeAi();
    //}

    public float Time()
    {
        throw new NotImplementedException();
    }

    public ILog Log()
    {
        throw new NotImplementedException();
    }

    public string GetObjectDataName(uint name)
    {
        throw new NotImplementedException();
    }

    public uint ObjectDataExists(uint name)
    {
        throw new NotImplementedException();
    }

    public FormationInfo GetFormationInfo(ObjId id)
    {
        return base.LoadGameData<FormationInfo>(id);
    }

    public PatrolInfo GetPatrolInfo(ObjId id)
    {
        return base.LoadGameData<PatrolInfo>(id);
    }

    public float GetGroundLevelMedian(Vector3 org, float r)
    {
        throw new NotImplementedException();
    }

    public bool RadioChannelIsFree()
    {
        return (GetRadioEnvironment() != null ? GetRadioEnvironment().IsRadioFree() : true);

        //TODO! Сделать как нужно проверку тишины в эфире!
        //return true;
    }

    public int getPhrasesCount(uint code)
    {
        PhraseHolder ph = getPhrase(code);
        return ph != null ? ph.getCount() : 0;
    }

    public IGroupAi CreateGroupAi(string name, GROUP_DATA data)
    {
        // ищем данные по AI
        crc32 t = Hasher.HshString(name);
        AiData aidata = FindAiData((int)CampaignDefines.CF_LEVEL_GROUP, t);
        if (aidata == null)
        {
            //Message("Warning: AI (group \"%s\") not found, skipped.", name);
            Debug.Log(String.Format("Warning: AI (group \"{0}\") not found, skipped.", name));
            return null;
        }
        // создаем AI
        //Debug.Log("HostScene CreateGroupAi " + data.Callsign +"\n===start===\n" + data.AiScript + "\n===end===");
        return aidata.dll.ai_module.CreateGroupAi(t, data, GetMissionAI());
    }



    public IBaseUnitAi CreateUnitAi(iClient client, UNIT_DATA data, iContact contact, IGroupAi grp)
    {
        return ((BaseClient)client).Capture(contact, data, grp);
    }

    public void DeleteUnit(iContact cnt, bool explode)
    {
        if (explode)
        {
            iBaseVictim victim = (iBaseVictim)cnt.GetInterface(iBaseVictim.ID);
            if (victim != null)
            {
                victim.AddDamage(Constants.THANDLE_INVALID, iBaseVictim.WeaponCodeUltimateDeath, 0);
                return;
            }
        }
        else
        {
            GetDamage(cnt.GetHandle(), Constants.THANDLE_INVALID, iBaseVictim.WeaponCodeCollisionGround, 0);
            OnAddDamage(cnt.GetHandle(), Constants.THANDLE_INVALID, iBaseVictim.WeaponCodeCollisionGround, 0, true);
        }
        //BaseUnit unit = cnt.GetInterface<BaseUnit>();
        //if (unit != null) delete unit;
        IBaseUnit unit = (IBaseUnit)cnt.GetInterface(IBaseUnit.ID);
        if (unit != null)
        {
            Debug.Log("Unit should be deleted" + (explode ? " with blast" : " silently"));
            unit.Dispose();
        }

    }

    public override float GetDamage(DWORD VictimHandle, DWORD GadHandle, DWORD WeaponCode, float Damage)
    {
        iContact pVictim = GetContact(VictimHandle);
        iContact pGad = GetContact(GadHandle);
        // проверяем сложность
        switch (GetGameData().mDifficulty)
        {
            case 0:
                if (pVictim != null && pVictim.IsPlayedByHuman() == true) Damage *= .3f;
                if (pGad != null && pGad.IsPlayedByHuman() == true) Damage *= 4f;
                break;
            case 1:
                if (pVictim != null && pVictim.IsPlayedByHuman() == true) Damage *= .6f;
                if (pGad != null && pGad.IsPlayedByHuman() == true) Damage *= 2f;
                break;
        }
        // проверяем опции
        if (pVictim != null && pVictim.IsPlayedByHuman() == true)
        {
            if (GetGameData().mUnlimitedArmor == true) Damage = 0;
            if (WeaponCode == iBaseVictim.WeaponCodeCollisionGround && GetGameData().mNoDamGround == true) Damage = 0;
            if (WeaponCode == iBaseVictim.WeaponCodeCollisionObject && GetGameData().mNoDamObjects == true) Damage = 0;
        }
        // стандартная обработка
        Damage = base.GetDamage(VictimHandle, GadHandle, WeaponCode, Damage);
        // уведомляем АИ
        return ai_mission.QueryDamage(VictimHandle, GadHandle, WeaponCode, Damage);
    }
    public override void OnAddDamage(DWORD VictimHandle, DWORD GadHandle, DWORD WeaponCode, float Damage, bool IsFinal)
    {
        // уведомляем АИ
        ai_mission.OnAddDamage(VictimHandle, GadHandle, WeaponCode, Damage, IsFinal);
        // стандартная обработка
        base.OnAddDamage(VictimHandle, GadHandle, WeaponCode, Damage, IsFinal);
    }

    public override int GetTimeScale()
    {
        return TimeScale;
    }

    public override int SetTimeScale(int sc)
    {

        int t = Mathf.Clamp(sc, 0, 4);
        if (t != TimeScale)
        {
            TimeScale = t;
            //SetTimeScalePacket Pkt(TimeScale);
            //SendItemData(&Pkt);
        }
        Debug.Log("TimeScale set: " + TimeScale);
        return TimeScale;
    }

    public int GetDifficulty()
    {
        throw new NotImplementedException();
    }

    public IDataHasher GetDataHash()
    {
        return GetDataHasher();
    }

    public CommandsApi GetCommands()
    {
        return GetCommandsApi();
    }

    //public iUnifiedVariableContainer GetOptions(string pCtrName = "")
    //{
    //    return base.GetOptions(pCtrName);
    //}

    public iContact GetContactByHandle(uint hndl)
    {
        return GetContact(hndl);
    }

    public IBaseUnitAi CreateUnitAi(string name, UNIT_DATA data, iContact contact, IGroupAi grp)
    {
        // создаем стандартный AI
        crc32 t = Hasher.HshString(name);
        AiData aidata = FindAiData((int)CampaignDefines.CF_LEVEL_UNIT, t);
        if (aidata == null)
        {
            //LOG.Message("Warning: AI (unit \"%s\") not found, skipped.", name);
            Debug.Log(string.Format("Warning: AI (unit \"{0}\") not found, skipped.", name));
            return null;
        }
        return aidata.dll.ai_module.CreateUnitAi(t, data, contact, grp, this);
    }

}



public class RemoteScene : BaseScene
{
    public RemoteScene(StormGameData d, bool rm) : base(d, rm)
    {
    }

    public override void applyCraftSelection(ref UnitSpawnData spd)
    {
        throw new NotImplementedException();
    }

    public override float GetDamage(uint VictimHandle, uint GadHandle, uint WeaponCode, float Damage)
    {
        throw new NotImplementedException();
    }
}

