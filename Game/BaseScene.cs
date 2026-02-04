//#define DAMAGE_REPORT
using geombase;
using Storm;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
using static HashFlags;
using crc32 = System.UInt32;
using DWORD = System.UInt32;
using PhraseMap = System.Collections.Generic.Dictionary<System.UInt32, PhraseHolder>;


/// <summary>
/// базовый класс менеджера игровой сцены
/// </summary>
public abstract partial class BaseScene
{
    public const int DETAIL_STAGE_FULL = 0;
    public const int DETAIL_STAGE_HALF = 1;
    public const int DETAIL_STAGE_QUARTER = 2;
    public const int DETAIL_STAGE_NONE = 3;

    protected StormGameData pData;

    public void setCameraDraw(bool draw)
    {
        GetGameData().mShouldDraw = draw;
        if (draw)
            getMenuFeedback().onStartDraw();
    }

    public iUnifiedVariableContainer getCurrentEventData() { return pData.mpCurrentEventData; }
    public void showEngbay(int nCrafts, crc32[] pCrafts, int nWeapons, crc32[] pWeapons, UnitSpawnData mUnitSpawnData)
    {
        EngBayParams prms = new EngBayParams();
        prms.nCrafts = nCrafts;
        prms.pCrafts = pCrafts;
        prms.nWeapons = nWeapons;
        prms.pWeapons = pWeapons;
        prms.mySelection = new DWORD[4];
        prms.mySelection[0] = (uint)mUnitSpawnData.ObjectName;
        prms.mySelection[1] = (uint)mUnitSpawnData.Layout1Name;
        prms.mySelection[2] = (uint)mUnitSpawnData.Layout2Name;
        prms.mySelection[3] = (uint)mUnitSpawnData.Layout3Name;
        getMenuFeedback().showEngBay(prms);
    }
    internal void showSfgs(bool showOn)
    {
        mySfgShowing += (showOn ? 1 : 0) * 2 - 1;

        if ((mySfgShowing == 1 && showOn) || (mySfgShowing == 0 && !showOn))
            //for (int i = 0; i != mySfgs.Count(); ++i)
            //    mySfgs[i]->show(showOn);
            foreach (iSfg sfg in mySfgs) sfg.show(showOn);

    }

    public StormGameData GetGameData() { return pData; }
    public iUnifiedVariableContainer GetNonLocalizedData() { return pData.mpNonLocalizedMission; }
    iUnifiedVariableDB GetLocalizationDb() { return pData.mpLocalizationDb; }


    //iUnifiedVariableContainer getCurrentEventData() const { return SafeAddRef(pData.mpCurrentEventData.Ptr());  }
    public ILog GetLog() { return pData.mpLog; }
    public RendererApi GetRendererApi() { return pData.mpRenderer; }
    public ISound GetSoundApi() { return pData.mpSound; }

    internal Fpo CreateFPO2(ObjId Name)
    {
        return GetGameData().mpFpoLoader.CreateFpo(Name);
    }

    public EInput GetInputApi() { return pData.mpInput; }
    public IGameset getGameset() { return pData.myGameset; }
    IMenuFeedback getMenuFeedback() { return pData.myFeedback; }
    //ITranslator2* getTranslator() const { return pData.myTranslator; }

    protected bool Shutdowned;
    protected SceneVisualizer pScene;
    public SceneVisualizer GetSceneVisualizer() { return pScene; }
    public List<BaseData> DatasList = new List<BaseData>();
    public CommandsApi GetCommandsApi() { return pData.mpCommands; }

    //public LinkedList<iBaseActor> ActorsList = new LinkedList<iBaseActor>();
    //public TLIST<iBaseActor> ActorsList = new TLIST<iBaseActor>(true);
    public TLIST<BaseActor> ActorsList = new TLIST<BaseActor>(true);
    //public LinkedList<BaseSensors> SensorsList = new LinkedList<BaseSensors>();
    //auto_sub=true
    public TLIST<BaseSensors> SensorsList = new TLIST<BaseSensors>(true);
    public IDataHasher GetDataHasher() { return GetGameData().mpDataHasher; }
    public virtual IMissionAi GetMissionAI() { return null; }

    public IHash GetHash() { return GetGameData().mpHash; }

    //public List<BaseItem> ItemsArray = new ItemsArray();
    //public Dictionary<DWORD, IBaseItem> ItemsArray = new Dictionary<DWORD, IBaseItem>();
    public TArray<IBaseItem> ItemsArray;

    //синхронизация сцены
    bool Remote;
    public bool IsClient() { return Remote; }
    public bool IsHost() { return (!Remote); }

    public abstract void applyCraftSelection(ref UnitSpawnData spd);
    public virtual void setSwapComplete() { }

    public static float SceneTime;
    public int TimeScale = 1;
    public float GetTime() { return SceneTime; }
    public virtual void Init()
    {
        //AssertBp(GetGameData()->mpOptions != 0);
        if (GetGameData().mpOptions == null) throw new Exception("Can not load game options");
        Shutdowned = false;
        OpenLocalizer();
        //PARTICLE_SYSTEM::SetDistr(&Distr);
        if (GetRendererApi() != null)
        {
            pScene = new SceneVisualizer(this);
            pScene.Initialize();
        }
    }


    // упрощение работы с логом
    public void Message(string format, params object[] v)
    {
        GetLog().VMessage(format, v);
        //TODO Реализовать журналирование в базовой сцене
        //Debug.Log(string.Format(format, v));
    }
    public iUnifiedVariableContainer GetOptions(string pCtrName = null)
    {
        return pCtrName != null ?
            GetGameData().mpOptions.openContainer(pCtrName) :
            GetGameData().mpOptions;
    }

    public void Done()
    {
        //GetCommandsApi().UnRegister(this);
        //ActorsList.Clear();
        ActorsList.Free();
        //pScene.Dispose();
        pScene = null;
        //del(pScene);
        //SensorsList.Clear();
        //CloseLocalizer();
    }


    public iContact GetContact(DWORD h)
    {
        //if (!ItemsArray.ContainsKey(h)) return null;
        //IBaseItem i = ItemsArray[h];
        ////Debug.Log(string.Format("Contact found for {0} {1}", h.ToString("X8"), i));
        ////return ((iContact)i.GetInterface(iContact.ID);
        //return (iContact)i.GetInterface(iContact.ID);

        IBaseItem i = ItemsArray.Get(h);
        //if (i==null)
        //{
        //    ItemsArray.Debug(h);
        //}
        return i != null ? (iContact)i.GetInterface(iContact.ID) : null;
    }

    public void AppendSucceeded()
    {
        GetLog().AppendSucceeded();
    }

    //public iContact AddUnit(IBaseUnit u, int SideCode)
    //{
    //    BaseSensors s = null;

    //    // ищем side
    //    foreach (var sl in SensorsList)
    //    {
    //        if (sl.GetSideCode() == SideCode)
    //        {
    //            s = sl;
    //            break;
    //        }
    //    }

    //    // не нашли - создаем новый
    //    BaseSensors res = (s != null ? s : new BaseSensors(this, SideCode));
    //    // ищем side

    //    // добавляем во все sensors
    //    foreach (var sl in SensorsList)
    //    {
    //        if (sl == res) continue;
    //        // Debug.Log(GetType() + " addunit " + u.GetType() + " sidecode " + SideCode);
    //        sl.AddUnit(u, SideCode);
    //    }
    //    // возвращаем iContact для своего сайда
    //    //Debug.Log("Unit: " + u.GetType() );
    //    return res.AddUnit(u, SideCode);
    //}

    //public void SubUnit(IBaseUnit u)
    //{
    //    //BaseSensors* s;
    //    LinkedListNode<BaseSensors> s;
    //    for (s = SensorsList.First; s != null; s = s.Next)
    //    {
    //        s.Value.SubUnit(u);
    //    }
    //}

    public iContact AddUnit(IBaseUnit u, int SideCode)
    {
        BaseSensors s;
        // ищем side
        for (s = SensorsList.Head(); s != null; s = s.Next())
        {
            Debug.Log("Processing sensors " + s);
            if (s.GetSideCode() == SideCode) break;
        }
        // не нашли - создаем новый
        BaseSensors res = (s != null) ? s : new BaseSensors(this, SideCode);
        // добавляем во все sensors
        for (s = SensorsList.Head(); s != null; s = s.Next())
        {
            if (s == res) continue;
            s.AddUnit(u, SideCode);
        }
        // возвращаем iContact для своего сайда
        return res.AddUnit(u, SideCode);
    }

    public void SubUnit(IBaseUnit u)
    {
        BaseSensors s;
        // вычитаем из всех sensors
        for (s = SensorsList.Head(); s != null; s = s.Next())
            s.SubUnit(u);
    }


    internal bool Draw(float[] Viewport)
    {
        if (pScene == null) return false;
        pScene.Draw(Viewport);

        //mySwapCriterion--;
        //if (!mySwapCriterion)
        //    setSwapComplete();

        return true;

    }

    public Dictionary<uint, GameObject> UnitTemplates = new Dictionary<uint, GameObject>();
    public GameObject templateHolder = new GameObject("Scene Templates holder");
    //STUB!

    SyncTimer myTimer = new SyncTimer();
    public virtual bool Update(float real_scale)
    {
        const float MIN_TIME_DELTA = 0.05f;
        int moved = 0, disposed = 0;
        float scale = myTimer.updateRealDelta(real_scale);
        // апдейт всех актеров
        float t = scale * TimeScale;
        //SceneTime += scale;
        do
        {
            float dt = (t > MIN_TIME_DELTA ? MIN_TIME_DELTA : t);
            t -= dt; SceneTime += dt;
            // двигаем
            //for (var ActorContainer = ActorsList.First; ActorContainer != null; ActorContainer = ActorContainer.Next)
            //{
            //    //if (!ActorContainer.Value.Move(scale)) ActorsList.Remove(ActorContainer);
            //    if (!ActorContainer.Value.Move(scale)) ActorContainer.Value.Dispose();
            //}
            BaseActor a;
            BaseActor last = null;
            for (a = ActorsList.Head(); a != null; a = last != null ? last.Next() : ActorsList.Head())
            {
                if (!a.Move(dt))
                {
                    disposed++;
                    a.Dispose();
                    continue;
                }
                last = a;
                moved++;
            }

            // обновляем
            for (a = ActorsList.Head(); a != null; a = a.Next())
            {
                a.Update(dt);
            }

            // апдейт всех сторон
            BaseSensors s, s1;
            for (s = SensorsList.Head(); s != null; s = s1)
            {
                s1 = s.Next();
                if (s.Update()) continue;
                s.Dispose();
            }
        } while (t > 0);

        // считаем пути
        if (GetGameData().mpNavigation != null)
            GetGameData().mpNavigation.Update(scale);

        //DWORD nfc = getServerTime() * FrameRate;
        //if (nfc != FrameCounter)
        //{
        //    nfc -= FrameCounter;
        //    FrameCounter += nfc;
        //    if (FrameUpdate(FrameCounter, nfc) == false)
        //        Shutdowned = true;
        //}
        //Debug.LogFormat("BaseActors Moved {0}/{1} disposed {2}", moved, ActorsList.Counter(), disposed);
        return true;
    }

    public FPO CreateFPO(ObjId Name)
    {
        return GetGameData().mpFpoLoader.CreateFPO(Name);
    }

    public BaseScene(StormGameData d, bool rm)
    {
        //pData = d;
        //SceneTime = 0;
        ////stub! delme!
        //pData.mpCommands = new Commands();
        //ActorsList = new LinkedList<iBaseActor>();

        pData = d;
        SceneTime = 0; //Time в исходниках "Шторма"
        TimeScale = 1;
        pScene = null;
        //ItemsArray = new Dictionary<DWORD, IBaseItem>();
        ItemsArray = new TArray<IBaseItem>(16384);
        Remote = rm;
        //FrameCounter = 0;
        //SetFrameRate(4);
        //myChatLog = createLOG("chat");



    }






    protected void OnCreateItem(object i)
    {
        //STUB!
    }

    public SceneVisualizer getSV() { return pScene; }

    public TERRAIN_DATA GetTerrain() { return GetGameData().mpTerrain; }
    public TraceResult GroundLevelTr(float x, float z)
    {
        TraceResult tr;
        GetGameData().mpTerrain.GroundLevel(x, z, out tr);
        return tr;

    }
    public float GroundLevel(float x, float z)
    {
        TraceResult tr;
        GetGameData().mpTerrain.GroundLevel(x, z, out tr);
        return tr.dist;
    }
    public float WaterLevel(float x, float z)
    {
        TraceResult tr;
        GetGameData().mpTerrain.WaterLevel(x, z, out tr);
        return tr.dist;

    }

    public bool IsInSfg(Vector3 org, bool IsCamera = false)
    {

        if (IsCamera == false)
        {
            for (int i = 0; i < mySfgs.Count; ++i)
            {
                if (mySfgs[i].IsInRange(org))
                    return true;
            }
            return false;
        }
        bool inSfg = false;
        for (int i = 0; i < mySfgs.Count; ++i)
        {
            if (mySfgs[i].IsCameraInRange(org))
                inSfg = true;
        }
        return inSfg;
    }


    public float SurfaceLevel(float x, float z)
    {
        TraceResult tr;
        GetGameData().mpTerrain.GroundLevel(x, z, out tr);
        float r = tr.dist;
        GetGameData().mpTerrain.WaterLevel(x, z, out tr);
        return (r > tr.dist ? r : tr.dist);


    }
    public TraceResult SurfaceLevelTr(float x, float z)
    {
        TraceResult trg;
        GetGameData().mpTerrain.GroundLevel(x, z, out trg);
        TraceResult trw;
        GetGameData().mpTerrain.WaterLevel(x, z, out trw);
        return (trw.dist > trg.dist ? trw : trg);
    }

    List<iSfg> mySfgs = new List<iSfg>();
    int mySfgShowing = 0;
    public void registerSfg(iSfg sf)
    {
        mySfgs.Add(sf);
        if (mySfgShowing <= 0)
            sf.show(false);
    }

    public void unregisterSfg(iSfg sf)
    {
        //int idx = mySfgs.find(sf);
        //Assert(idx >= 0);
        //mySfgs.Remove(idx, idx);
        mySfgs.Remove(sf);
    }

    public float GroundLevelMedian(float x, float z, float r)
    {
        return GetGameData().mpTerrain.GroundLevelMedian(x, z, r);
    }
    public int GroundPass(int x, int z)
    {
        return GetGameData().mpTerrain.GroundPass(x, z);
    }
    public float SurfaceLevelObjects(float x, float z)
    {
        float y = SurfaceLevel(x, z);
        TraceInfo info = GetGameData().mpCollision.TraceLine(new Geometry.Line(new Vector3(x, y + 2000f, z), -Vector3.up, 2000f), null, (int)CollisionDefines.COLLF_WITH_OBJECT2);
        //TODO Трейс не может быть null, так что после корректной реализации TraceLine удалить проверку 
        if (info != null && info.count != 0)
        {
            float y2 = info.results[0].org.y + info.results[0].coll_object.MaxY();
            if (y2 > y)
                y = y2;
        }
        return y;

    }

    public float GetGroundLevel(Vector3 org)
    {
        return SurfaceLevel(org.x, org.z);
    }

    internal void SendItemData(WeaponSlotDetachingNObjects v)
    {
        throw new NotImplementedException();
    }

    public BaseDebris CreateBaseDebris(FPO g, DEBRIS_DATA _data, DWORD _client_handle, float _mass)
    {
        return new BaseDebris(this, g, _data, _client_handle, _mass);
    }
    public BaseDebris CreateBaseDebris(MATRIX m, DEBRIS_DATA _data, DWORD _client_handle = 0xFFFFFFFF)
    {
        return new BaseDebris(this, m, _data, _client_handle);
    }


    /// <summary>
    /// BaseData-ы
    /// </summary>
    /// <param name="DataName"></param>
    /// <param name="MustExist"></param>
    /// <returns></returns>
    public BaseData GetBaseData(DWORD DataName, bool MustExist = true)
    {
        foreach (BaseData d in DatasList)
        {
            if (d.GetName() == DataName) return d;
        }

        Asserts.AssertEx(MustExist == false);
        return null;
    }

    internal T LoadGameData<T>(uint name) where T : IStormImportable<T>, new()
    {
        MemBlock block = GetGameData().mpGameDatas.GetBlock(name);
        if (block == null) return default;
        T data = new T();
        //data.Import(block.myStream);
        return data.Import(block.myStream);
    }

    internal TArray<IBaseItem> GetItemsArray() { return ItemsArray; }

    internal IFpoLoader GetFpoLoader()
    {
        return GetGameData().mpFpoLoader;
    }
}

/// <summary>
/// BaseScene - переводилка
/// </summary>
public partial class BaseScene
{
    public const uint FromCallerIndexToRecipientIndex = 0x22BB6D48;
    public const uint FromUnitToGroup = 0xACB73280;
    public const uint FromCallerIndexToRecipient = 0x97702E50;
    public const uint FromCallerIndexToNone = 0x21DC6E25;
    public const uint FromCallerToRecipientIndex = 0xB2FA32FE;
    public const uint FromCallerToRecipient = 0x4268C649;
    public const uint FromCallerToNone = 0x283F7E83;
    public const uint FromNoneToRecipientIndex = 0xC585F53F;
    public const uint FromNoneToRecipient = 0xFC38A1BF;
    public const uint FromNoneToNone = 0xABAF21DE;

    PhraseMap myGamesetPhrases = new();
    PhraseMap myMissionPhrases = new();
    PhraseMap myGameDataPhrases = new();
    protected PhraseHolder getPhrase(crc32 code)
    {
        //Debug.Log("getPhrase: " + code.ToString("X8"));

        PhraseMap[] sets = { myMissionPhrases, myGamesetPhrases, myGameDataPhrases };

        //for (int i = 0; i != sizeof(sets) / sizeof(sets[0]); ++i)
        //{
        //    PhraseMapIter iter = sets[i]->find(code);
        //    if (iter != sets[i]->end())
        //        return (*iter).second;
        //}

        foreach (PhraseMap set in sets)
        {
            if (set.ContainsKey(code))
            {
                //Debug.Log("Phrase found: " + set[code].getText(0));
                return set[code];
            }
        }

        return null;
    }

    public void storePhrase(ref PhraseMap map, string name, string text)
    {
        //Debug.Log(string.Format("Storing message [{0}]: {1}", name, text));
        PhraseHolder phr = new PhraseHolder(text, name);
        if (phr.getCount() == 0)
        {
            Message("Error : missed phrase \"{0}\"", name);
            phr = null; ;
        }
        else
            //map[HashString(name)] = phr;
            map.Add(Hasher.HshString(name), phr);
    }

    void OpenLocalizer()
    {
        int n;
        IMessage msg;
        IMission msn = getGameset().getMission(0);
        for (n = 0; (msg = msn.getMessage(n++)) != null;)
            storePhrase(ref myMissionPhrases, msg.getName(), msg.getText());

        for (n = 0; (msg = getGameset().getMessage(n++)) != null;)
            storePhrase(ref myGamesetPhrases, msg.getName(), msg.getText());

        iUnifiedVariableContainer msgs = GetLocalizationDb().openContainer("\\Root\\Game Messages");

        if (msgs != null)
        {
            string val;
            for (DWORD handle = 0; (handle = msgs.GetNextHandle(handle)) != 0;)
            {
                iUnifiedVariableString tmp_msg = msgs.GetVariableTpl<iUnifiedVariableString>(handle);
                tmp_msg.StrCpy(out val);
                //val(msg->StrLen() + 1); msg->StrCpy(val.Begin());
                storePhrase(ref myGameDataPhrases, tmp_msg.getNameShort(), val);
            }
        }

    }

    public string AddLocalizedString(ref string pDst, int Length, DWORD Code, string pString, bool snd, int index, bool real_name = false)
    {
        pDst = "";//TODO - возможно, тогда лучше не ref, a out?
        if (pString != null) Code = Hasher.HshString(pString);
        PhraseHolder ph = getPhrase(Code);
        int count = ph != null ? ph.getCount() : 0;
        if (count != 0)
        {
            index = Mathf.Clamp(index, 0, count - 1);
            string src = real_name ? ph.getName(index) : (snd ? ph.getWave(index) : ph.getText(index));
            //return _addstrn(pDst, src, Length); // TODO Вот тут что-то не то с локализацией.
            pDst += src;
            return pDst;
        }
        return null;
    }

    public string AddLocalizedMessage(ref string pDst, string pFormat, RadioMessage pMsg, bool snd, bool add_callsigns = true)
    {
        //Debug.Log(string.Format("Early AddLocalizedMessage pFormat [{0}] Code {1} pDst [{2}]", pFormat,pMsg.Code.ToString("X8"),pDst));
        // разбираем формат
        string pFmt;
        if (pFormat != null)
        {
            if (snd) return null;
            pFmt = pFormat;
        }
        else
        {
            pFmt = "";
            if (AddLocalizedString(ref pFmt, 2048, pMsg.Code, null, snd, (int)pMsg.myPhraseIndex) == null) return null;
        }
        // добавляем позывные
        if (add_callsigns == true)
            pDst = addCallsigns(pDst, pFormat, pMsg, snd);
        //Debug.Log(string.Format("Late AddLocalizedMessage pFormat [{0}] Code {1} pDst [{2}]", pFormat, pMsg.Code.ToString("X8"), pDst));
        return pDst != null ? AddLocalizedString(ref pDst, pFmt, pMsg, snd, pFormat == null, (int)pMsg.myPhraseIndex) : null;
    }

    public string addCallsigns(string pDst, string pFormat, RadioMessage pMsg, bool snd)
    {
        Debug.Log(string.Format("addCallsigns early pDst [{0}] pFormat [{1}]\npMsg:\n{2}", pDst, pFormat, pMsg));
        string dst = "";
        //if (pFormat == null && pFormat == "")
        if (string.IsNullOrEmpty(pFormat))
        {
            DWORD Code;
            if (pMsg.CallerCallsign != null)
            {
                if (pMsg.CallerIndex != 0)
                { // от юнита
                    if (pMsg.RecipientCallsign != null) Code = (pMsg.RecipientIndex != 0 ? FromCallerIndexToRecipientIndex : (pMsg.CallerCallsign == pMsg.RecipientCallsign ? FromUnitToGroup : FromCallerIndexToRecipient));
                    else Code = FromCallerIndexToNone;
                }
                else
                {
                    if (pMsg.RecipientCallsign != null) Code = (pMsg.RecipientIndex != 0 ? FromCallerToRecipientIndex : FromCallerToRecipient);
                    else Code = FromCallerToNone;
                }
            }
            else
            {
                if (pMsg.RecipientCallsign != null) Code = (pMsg.RecipientIndex != 0 ? FromNoneToRecipientIndex : FromNoneToRecipient);
                else Code = FromNoneToNone;
            }
            // ищем такую строку
            //char fmt[1024];
            string fmt = "";
            if (AddLocalizedString(ref fmt, 1024, Code, null, snd, (int)pMsg.myPhraseIndex) != null)
                dst = AddLocalizedString(ref pDst, fmt, pMsg, snd, true, (int)pMsg.myPhraseIndex); // переводим позывные
            //Debug.Log(string.Format("addCallsigns late pDst [{0}] pFormat [{1}]\npMsg:\n{2}", pDst, pFormat, pMsg));
            Debug.Log(string.Format("addCallsigns late dst [{0}] fmt [{1}]\npMsg:\n{2}", dst, fmt, pMsg));
            //Debug.Log("addCallsigns late fmt " + fmt + " dst " + dst + " code " + Code.ToString("X8") + pMsg);
        }
        if (!string.IsNullOrEmpty(dst))
            pDst = dst;
        else
        {
            //// при обработке звука нельзя пользоваться default
            //if (snd) return null;

            //// добавляем позывные Caller-a
            //if (pMsg.CallerCallsign != null)
            //{
            //    pDst = pMsg.CallerCallsign;

            //    if (pMsg.CallerIndex != 0)
            //    { // от юнита
            //        pDst+= ' ';
            //        pDst+= pMsg.CallerIndex.ToString();
            //    }
            //    string new_pos=null;
            //    if (pMsg.GetFlag(RadioMessage.RMF_TEAM)!=0)
            //        new_pos = AddLocalizedString(ref pDst, 32, 0xF90C2AEE, null, snd, 0);
            //    else if (pMsg.GetFlag(RadioMessage.RMF_ALL)!=0)
            //        new_pos = AddLocalizedString(ref pDst, 32, 0x006B279E, null, snd, 0);
            //    if (new_pos!=null)
            //        pDst = new_pos;
            //    // добавляем ': '
            //    pDst+= ':';
            //    pDst+= ' ';
            //}

            //// добавляем позывные Recipient-a
            //if (pMsg.RecipientCallsign != null)
            //{
            //    if (pMsg.RecipientIndex != 0)
            //    { // юниту
            //      // Alpha 1
            //        pDst = _addstr(pDst, pMsg->RecipientCallsign);
            //        pDst+= ' ';
            //        pDst = _addint(pDst, pMsg->RecipientIndex, 0, 10, 0, 0);
            //    }
            //    else
            //    { // группе
            //        if (pMsg.CallerCallsign == pMsg.RecipientCallsign)
            //        { // обращение внутри группы
            //            pDst = _addstr(pDst, "group, ");
            //        }
            //        else
            //        {
            //            pDst = _addstr(pDst, pMsg.RecipientCallsign);
            //        }
            //    }
            //    // добавляем ', '
            //    pDst+= ',';
            //    pDst+= ' ';
            //}

            //TODO Добавить звуковые позывные
            //// при обработке звука нельзя пользоваться default
            //if (snd) return null;
            //// добавляем позывные Caller-a
            //if (pMsg.CallerCallsign != null)
            //{
            //    pDst = _addstr(pDst, pMsg->CallerCallsign);
            //    if (pMsg->CallerIndex != 0)
            //    { // от юнита
            //        *pDst++ = ' ';
            //        pDst = _addint(pDst, pMsg->CallerIndex, 0, 10, 0, 0);
            //    }
            //    char* new_pos;
            //    if (pMsg->GetFlag(RMF_TEAM))
            //        new_pos = AddLocalizedString(pDst, 32, 0xF90C2AEE, 0, snd, 0);
            //    else if (pMsg->GetFlag(RMF_ALL))
            //        new_pos = AddLocalizedString(pDst, 32, 0x006B279E, 0, snd, 0);
            //    if (new_pos)
            //        pDst = new_pos;
            //    // добавляем ': '
            //    *pDst++ = ':';
            //    *pDst++ = ' ';
            //}
            //// добавляем позывные Recipient-a
            //if (pMsg->RecipientCallsign != 0)
            //{
            //    if (pMsg->RecipientIndex != 0)
            //    { // юниту
            //      // Alpha 1
            //        pDst = _addstr(pDst, pMsg->RecipientCallsign);
            //        *pDst++ = ' ';
            //        pDst = _addint(pDst, pMsg->RecipientIndex, 0, 10, 0, 0);
            //    }
            //    else
            //    { // группе
            //        if (pMsg->CallerCallsign == pMsg->RecipientCallsign)
            //        { // обращение внутри группы
            //            pDst = _addstr(pDst, "group, ");
            //        }
            //        else
            //        {
            //            pDst = _addstr(pDst, pMsg->RecipientCallsign);
            //        }
            //    }
            //    // добавляем ', '
            //    *pDst++ = ',';
            //    *pDst++ = ' ';
            //}
        }
        return pDst;
    }

    static string InsertString(string pDst, string pSrc, bool trans, bool snd, BaseScene pScene, int index, bool InsertOriginal = false)
    {
        pDst = "";
        if (pSrc != null)
        {
            if (trans == false)
            {
                pDst = pSrc;
            }
            else
            {
                string p = pScene.AddLocalizedString(ref pDst, 2048, 0, pSrc, snd, index);
                pDst = p != null ? p : (InsertOriginal ? pSrc : "NO_DATA");
            }
        }
        return pDst;
    }
    //string AddLocalizedString(ref string pDst, string pFormat, RadioMessage pMsg, bool snd, bool translate, int index)
    string AddLocalizedString(ref string pDst, string pFormat, RadioMessage pMsg, bool snd, bool translate, int index)
    {
        pFormat = pFormat.Replace("%ORG%", AddLocalizedOrg(pFormat, pMsg.Org, snd));
        pFormat = pFormat.Replace("%STR1%", InsertString(pFormat, pMsg.String1, translate, snd, this, index));
        pFormat = pFormat.Replace("%STR2%", InsertString(pFormat, pMsg.String2, translate, snd, this, index));
        pFormat = pFormat.Replace("%CallerCallsign%", InsertString(pFormat, pMsg.CallerCallsign, translate, snd, this, index, true));
        pFormat = pFormat.Replace("%RecipientCallsign%", InsertString(pFormat, pMsg.RecipientCallsign, translate, snd, this, index, true));
        pFormat = pFormat.Replace("%CallerIndex%", pMsg.CallerIndex.ToString());
        pFormat = pFormat.Replace("%RecipientIndex%", pMsg.RecipientIndex.ToString());

        //Debug.Log("AddLocalizedString pFormat [" + pFormat +  "] pDst [" + pDst + "]");

        pDst = pFormat;
        return pDst;
        //char pKeyword[1024]; // буффер для ключевого слова
        //        string pKeyWord;
        //    string src;
        //    char* dst = pDst;
        //    char* key_dst = 0;
        //        string src = pFormat;
        //    // парсим строку
        //    for (src=pFormat,key_dst=0; *src!=0; src++) {
        //        if (key_dst!=0) { // keyword в процессе
        //            if (* src=='%') { // конец кейворда
        //                switch (Crc32.Code(pKeyword, key_dst-pKeyword)) {
        //                case 0x1B4D07F5: // ORG
        //                    dst=AddLocalizedOrg(dst, pMsg->Org, snd);
        //                    break;
        //                case 0xB5E5392F: // STR1
        //                    dst=InsertString(dst, pMsg->String1, translate, snd, this, index);
        //                    break;
        //                case 0x2CEC6895: // STR2
        //                    dst=InsertString(dst, pMsg->String2, translate, snd, this, index);
        //                    break;
        //                case 0x49E9CF08: // CallerCallsign
        //                    dst=InsertString(dst, pMsg->CallerCallsign, translate, snd, this, index,true);
        //                    break;
        //                case 0xC11B42C3: // RecipientCallsign
        //                    dst=InsertString(dst, pMsg->RecipientCallsign, translate, snd, this, index,true);
        //                    break;
        //                case 0xC010E967: // CallerIndex
        //                    dst=_addint(dst, pMsg->CallerIndex,0,10,0,0);
        //                    break;
        //                case 0xDD802DCF: // RecipientIndex
        //                    dst=_addint(dst, pMsg->RecipientIndex,0,10,0,0);
        //                    break;
        //                case 0xFFFFFFFF: // %%
        //                    dst=_addstr(dst,"%%");
        //                    break;
        //                default:
        //                    dst=0;
        //                }
        //if (dst == 0) return 0;
        //key_dst = 0; // сбрасываем флаг разборки
        //continue;
        //            }
        //            // копируем
        //            *key_dst++ = *src;
        //        } else
        //{ // нет кейворда в процессе разборки
        //    if (*src == '%')
        //    { // начало кейворда?
        //        key_dst = pKeyword;
        //        continue;
        //    }
        //    // копируем
        //    *dst++ = *src;
        //}
        //    }
        //    // еще одна проверка
        //    if (key_dst != 0) return 0;
        //// добавляем само сообщение
        //*dst = 0;
        //return dst;
        //}
    }

    public string AddLocalizedText(ref string pDst, int DstLength, DWORD Code, string pString = null, bool real_name = false)
    {
        return AddLocalizedString(ref pDst, DstLength, Code, pString, false, 0, real_name);
    }

    string AddLocalizedOrg(string pDst, Vector3 org, bool snd)
    {
        const float sStep = 14976f;
        const float s1oStep = 1f / sStep;
        const string pKeypad = "789456123";
        float fx = org.x * s1oStep;
        float fz = (GetTerrain().GetZSize() - org.z) * s1oStep;
        int ix = (int)Mathf.Floor(fx);
        int iz = (int)Mathf.Floor(fz);

        if (snd)
        {
            pDst = string.Format("{0}_+{1}_+", iz + 1, ix + 2);
            //pDst=_addint(pDst, iz+1,0,10,0,0);
            //*pDst++='_';
            //*pDst++='+';
            //pDst=_addint(pDst, ix+1,0,10,0,0);
            //*pDst++='_';
            //*pDst++='+';
        }
        else
        {
            pDst = string.Format("{0}.{1}.", iz + 1, ix + 2);
            //pDst = _addint(pDst, iz + 1, 2, 10, '0', 0);
            //*pDst++ = '.';
            //pDst = _addint(pDst, ix + 1, 2, 10, '0', 0);
            //*pDst++ = '.';
        }
        // разбиение по кейпаду
        pDst += pKeypad[(int)(Mathf.Floor((fz - iz) * 3) * 3 + Mathf.Floor((fx - ix) * 3))];
        //if (snd) *pDst++='_';
        return pDst;
    }
}
/// <summary>
/// Hash functions
/// </summary>
public partial class BaseScene
{
    public HMember ConstructHM(IHashObject r)
    {
        return new HMember(r);
    }

    public HMember CreateHM(IHashObject r)
    {
        return UpdateHM(new HMember(r));
    }

    public void DeleteHM(HMember h)
    {
        RemoveHM(h);
    }

    public void DeleteHMByLine(HMember h)
    {
        GetGameData().mpHash.RemoveMemberByLine(h);
    }

    public HMember UpdateHM(HMember h)
    {
        return GetGameData().mpHash.UpdateMember(h);
    }

    public HMember RemoveHM(HMember h)
    {
        return GetGameData().mpHash.RemoveMember(h);
    }

    public HMember CreateHMByLine(IHashObject r)
    {
        return GetGameData().mpHash.UpdateMemberByLine(new HMember(r));
    }

}

/// <summary>
/// BaseSceneCollision
/// </summary>
public partial class BaseScene
{
    public TraceInfo TraceLine(Geometry.Line line, HMember ignored, int Flags)
    {
        return GetGameData().mpCollision.TraceLine(line, ignored, Flags);
    }

    public CollideInfo Collide(HMember who_collided, int Flags)
    {
        return GetGameData().mpCollision.Collide(who_collided, Flags);
    }

    public bool CollideObjSphere(IHashObject obj, Vector3 o, float r)
    {
        return GetGameData().mpCollision.CollideObjSphere(obj, o, r);
    }
}
/// <summary>
/// GameDataHolder2
/// </summary>
public partial class BaseScene
{
    public INavigation GetNavigationApi() { return GetGameData().mpNavigation; }
    public virtual int GetTimeScale() { return 1; }
    public virtual int SetTimeScale(int sc) { return sc; }
    public ILog getChatLog()
    {
        return null; //TODO вернуть чатлог базовой сцены
                     //    return myChatLog; 
    }
}

/// <summary>
/// BaseSceneEffects
/// </summary>
public partial class BaseScene
{

    public void MakeAreaDamage(DWORD GadHandle, DWORD WeaponCode, Vector3 _org, float Xr, float Xd)
    {
#if DAMAGE_REPORT
#warning "  MIKHA: log"
    Message("AreaDamage (Xr={0},Xd={1}):", Xr, Xd* sqr(Xr));
    IncIdent();
#endif
        EngineDebug.DebugSphere(_org, string.Format("AreaDamage area {0} damage {1}", Xr, Xd), Xr, 5);
        AreaDamageEnumer a = new AreaDamageEnumer(GadHandle, WeaponCode, _org, Xr, Xd);
        GetHash().EnumSphere(new geombase.Sphere(_org, Xr), ROObjectId(RoFlags.ROFID_FPO), a);
#if DAMAGE_REPORT
    DecIdent();
#endif
    }

    int GetDetailStage(Vector3 Org)
    {
        if (GetSceneVisualizer() == null) return DETAIL_STAGE_NONE;
        return GetSceneVisualizer().GetDetailStage(Org);
    }

    public virtual float GetDamage(DWORD VictimHandle, DWORD GadHandle, DWORD WeaponCode, float Damage)
    {
        return Damage;
    }

    public virtual void OnAddDamage(uint VictimHandle, uint GadHandle, uint WeaponCode, float Damage, bool IsFinal)
    {
#if DAMAGE_REPORT
        string unk = "<unknown>";
        iContact pVictim = GetContact(VictimHandle);
        iContact pGad = GetContact(GadHandle);
        string s = unk;
        switch (WeaponCode)
        {
            case Constants.WeaponCodeCollisionGround: s = "<ground collision>"; break;
            case Constants.WeaponCodeCollisionObject: s = "<object collision>"; break;
            case Constants.WeaponCodeUltimateDeath: s = "<ultimate death>"; break;
            default:
                {
                    SUBOBJ_DATA d = SUBOBJ_DATA.GetByCode(WeaponCode, false);
                    if (d != null) s = d.FullName;
                }
                break;
        }
        Message("\"{0}\" hit \"{1}\" {2} damage by \"{3}\"",
          (pGad != null ? pGad.GetTypeName() : unk),
          (pVictim != null ? pVictim.GetTypeName() : unk),
          Damage.ToString(),
          s);
        ((HostScene)this).GetMissionAI().GetContactInfo(pGad, out DWORD grp_id, out DWORD un_index, out DWORD side);
        var GroupAi = ((HostScene)this).GetMissionAI().GetGroupByID(grp_id);

        if (GroupAi != null)
        {
            Debug.Log("Gad was " + GroupAi.GetGroupData().Callsign + " " + un_index );
        }
        //if (IsFinal) Append(" and killed!");
        if (IsFinal) Message(" and killed!");
#endif
    }

    public iRadioEnvironment GetRadioEnvironment()
    {
        return (pScene != null ? pScene.GetRadioEnvironment() : null);
    }
}
public class AreaDamageEnumer : HashEnumer
{
    float radius;
    Vector3 org;
    float damage;
    DWORD mGadHandle;
    DWORD mWeaponCode;
    public virtual bool ProcessElement(HMember h)
    {
        FPO f = (FPO)(h.Object());
        iBaseInterface uf = null;
        try
        {
            uf = (iBaseInterface)(f.Link);
        }
        catch
        {
            //Debug.LogErrorFormat("Failed to convert {0} to iBaseInterface", f.Link);
        }
        if (uf == null) return true;
        iBaseVictim vict = null;
        try
        {
            vict = (iBaseVictim)uf.GetInterface(iBaseVictim.ID);
        }
        catch
        {
            //Debug.LogErrorFormat("Failed to convert {0} to iBaseVictim hMember flag {1} vs {2}", uf, h.hash_object.GetFlags().ToString("X8"), ROObjectId(RoFlags.ROFID_FPO).ToString("X8"));
        }
        if (vict != null)
            vict.AddRadiusDamage(mGadHandle, mWeaponCode, org, radius, damage);
        return true;
    }
    public AreaDamageEnumer(DWORD GadHandle, DWORD WeaponCode, Vector3 _org, float _radius, float _damage)
    {
        mGadHandle = GadHandle;
        mWeaponCode = WeaponCode;
        org = _org;
        radius = _radius;
        damage = _damage;
    }
};



public class SyncTimer
{
    const float gMaxCoeffDelta = 0.1f;
    public float updateRealDelta(float real_dt)
    {
        myDelta = calcDelta(real_dt);
        myServerTime += real_dt;
        myTime += myDelta;
        return getDelta();
    }

    public float getTime() { return myTime; }
    public float getDelta() { return myDelta; }

    public float getLag() { return myServerTime - myTime; }

    public float getServerTime() { return myServerTime; }

    public SyncTimer()
    {
        myTime = 0;
        myDelta = 0;
        myServerTime = 0;
    }

    public void setServerTime(float time, bool both = false) { myServerTime = time; if (both) { myTime = time; myDelta = 0; } }
    private float calcDelta(float real_dt)
    {
        float lag = myServerTime - myTime;
        float k = real_dt * gMaxCoeffDelta;
        float sgn = lag > 0 ? 1 : -1;
        return real_dt + (k > sgn * lag ? lag : sgn * k);
    }

    float myTime;
    float myDelta;
    float myServerTime;
};