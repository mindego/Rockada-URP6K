using DWORD = System.UInt32;
using WORD = System.UInt16;
using crc32 = System.UInt32;
using System.Collections.Generic;
using UnityEngine;
using static GameStatuses.ClientStatuses;
using static GameStatuses.MissionStatuses;
using static ServerMessages;
using static StdMissionAiMessages;
using static AICommon;
using static stdlogic_dll;


public abstract partial class StdMissionAi : CommLink, IQuery
{
    //Stub!
    public const uint ID = 0x5F1B3526;

    public const float ST_PLAIN_FONT_SIZE = 1.4f;
    public const float ST_PLAIN_FONT_SPACE = 0.2f;

    public const float ST_HEADER_FONT_SIZE = 1.4f;
    public const float ST_HEADER_FONT_SPACE = 0.2f;

    public const int MAX_CONNECTIONS = 128;

    public const string sDebugLevelName = "Debug";
    public const string sColorNames = "EnableColorNames";
    public const string sMaxPlayers = "NumberOfPlayers";
    public const string sMaxObservers = "NumberOfConnections";
    public const string sNoDamageFromFriends = "NoDamageFromFriends";

    public string sServerPassword = "srv_password";
    public static string sServerLogMaxTime = "srv_log_maxtime";
    public static string sServerLogMaxSize = "srv_log_maxsize";

    // common commands
    protected const string sServerInfo = "srv_info";
    protected const string sClientName = "cl_name";
    protected const string sCommonSay = "cl_say";
    protected const string sServerAdmin = "srv_admin";

    // admin commands
    protected const string sServerKick = "srv_kick";
    protected const string sServerBan = "srv_ban";
    protected const string sServerUnBan = "srv_unban";
    protected const string sServerLog = "srv_log";
    protected const string sServerMute = "srv_mute";
    protected const string sServerUnMute = "srv_unmute";
    protected const string sServerSaveBanList = "srv_save_banlist";

    // clients
    protected int myDifficulty;
    bool mNoDamageFromFriendly;
    DWORD mMaxPlayers;
    DWORD mMaxObservers;
    DWORD GetMaxPlayers() { return mMaxPlayers; }
    DWORD GetMaxObservers() { return mMaxObservers; }
    public bool CanRespawn() { return false; }
    bool IsNoDamageFromFriendly() { return mNoDamageFromFriendly; }
    protected LinkedList<MissionClient> mlClients = new();
    protected List<EnumPosition> mlPositions = new();
    public MissionClient FindClient(DWORD id)
    {
        for (LinkedListNode<MissionClient> mcl = mlClients.First; mcl != null; mcl = mcl.Next)
            if (mcl.Value.ID() == id) return mcl.Value;
        return null;

    }
    MissionClient FindClientH(DWORD hndl)
    {
        for (LinkedListNode<MissionClient> mcl = mlClients.First; mcl != null; mcl = mcl.Next)
            if (mcl.Value.Handle() == hndl) return mcl.Value;
        return null;
    }
    MissionClient FindClient(IBaseUnitAi ai)
    {
        for (LinkedListNode<MissionClient> mcl = mlClients.First; mcl != null; mcl = mcl.Next)
        {
            IBaseUnitAi cur_ai = mcl.Value.Client().GetAI();
            if (cur_ai == ai) return mcl.Value;
        }
        return null;

    }
    public MissionClient FindClient(UNIT_DATA data)
    {
        //for (LinkedListNode<MissionClient> mcl = mlClients.First; mcl != null; mcl = mcl.Next)
        //    if (mcl.Value.GetUnitData() == data) return mcl.Value;
        //return null;
        foreach (MissionClient mcl in mlClients)
        {
            //Debug.Log("MissionClient mcl:" + mcl + " data " + data);
            if (mcl.GetUnitData() == data) return mcl;
        }
        return null;

    }
    MissionClient FindClient(iClient client)
    {
        for (LinkedListNode<MissionClient> mcl = mlClients.First; mcl != null; mcl = mcl.Next)
            if (mcl.Value.Client() == client) return mcl.Value;
        return null;

    }
    public virtual MissionClient Connecting(iClient client)
    {
        Asserts.AssertBp(client != null);
        byte[] ip; ip = client.GetIpAddress();
        //for (int i = 0; i < mtBannedIPs.Count(); ++i) //Сеть не реализована
        //{
        //    IPHolder & ban_ip = mtBannedIPs[i];
        //    if (ban_ip.MatchByMask(ip) == true)
        //    {
        //        NotifyClient(client, GetMessage(AIMSG_OnBanConnection));
        //        NotifyHost(sConnectionWasBanned, ip[0], ip[1], ip[2], ip[3]);
        //        return 0;
        //    }
        //}
        MissionClient mclient = null;
        if (mlClients.Count < GetMaxObservers())
        {   // if can connect
            mclient = new MissionClient((ushort)GetUniqueClientID(), client, ref mlPositions); // add client
            SetClientName(mclient, client.GetPlayerName(), false == IsColorNamesEnabled());
            if (client.IsHidden() == false)
            {
                mpPrinter.Create(mclient);
                mpMenu.Create(mclient);
            }
            myClientInfo = new CDWrapperClientInfo();
            myClientInfo.create(client);
            myClientInfo.setPlayerName(client);
            setClientStatus(mclient, (int)CS_Leaved);
            myClientInfo.setMissionStatus(client, (uint)getMissionStatus());


            setMissionClose(isCanRespawn() == false);
            mclient.SetCanRespawn(mlClients.Count < GetMaxPlayers());
            //mlClients.AddToTail(mclient);
            mlClients.AddLast(mclient);
            if (mlClients.Count == 1)
                ExecuteScript();
            mclient.setStatus((int)CS_Connected);
            ScoreMessage(sAiConnected, "{0} \\name\\{1}", mclient.ID().ToString("X8"), mclient.Name());
            mpPrinter.TalkToClient(
                null,
                AIMSG_OnEnterGame,
            //Arg(client.GetPlayerName()),
            //Arg());
            client.GetPlayerName(),
            null);

            // registering common commands
            client.RegisterRemoteConsoleCommand(sServerInfo, 1);
            client.RegisterRemoteConsoleCommand(sClientName, 1);
            client.RegisterRemoteConsoleCommand(sServerAdmin, 2);
            OnMutePlayer(mclient, false, true);

            // setting default camera
            if (mLastCamera == null) mLastCamera = new CameraInfo();//TODO СТРАННОЕ! стоит перенести либо в метод инициализации или инитить сразу
            mclient.SetCamera(mLastCamera);
        }
        else
        {
            ScoreMessage(sAiConnectFail, "\\ip\\{0}.{1}.{2}.{3}", ip[0], ip[1], ip[2], ip[3]);
            NotifyClient(client, GetMessage(AIMSG_OnRejectConnection));
        }
        return mclient;
    }

    public abstract MissionClient Disconnecting(iClient client, bool dropped);
    int GetUniqueClientID()
    {
        for (int i = 0; i < GetMaxObservers(); i++)
        {
            LinkedListNode<MissionClient> mcl = null;
            for (mcl = mlClients.First; mcl != null; mcl = mcl.Next)
                if (mcl.Value.ID() == i) break;
            if (mcl == null) return i; //TODO возможно правильнее использовать mcl.Value?
        }
        if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
        {
            ScoreMessage("Error", "could't find ID for client");
        }
        return -1;

    }
    IMessageProcessor myMessageProcessor;

    // edvices
    CDWrapperPrinter mpPrinter;
    CDWrapperMenu mpMenu;

    protected bool isHost(MissionClient m)
    {
        MissionClient potential_host = getHost();
        return potential_host == m;
    }
    public MissionClient getHost()
    {
        return (mlClients.First != null && mlClients.First.Value.Client().IsHidden()) ? mlClients.First.Next.Value : mlClients.First.Value;
    }

    // logging
    LogLimitsInfo myLogLimits = new LogLimitsInfo();
    List<LogHolder> mClientLogs;



    // contacts
    LinkedList<ContactInfo> mlContacts = new LinkedList<ContactInfo>();
    public virtual ContactInfo FindContact(DWORD hndl)
    {
        //for (ContactInfo* info = mlContacts.Head(); info; info = info->Next())
        //    if (info->mHandle == hndl) return info;
        //return 0;
        foreach (ContactInfo info in mlContacts)
        {
            if (info.mHandle == hndl) return info;
        }
        return null;
    }
    public virtual ContactInfo FindContact(UNIT_DATA dt)
    {
        //for (ContactInfo* info = mlContacts.Head(); info; info = info->Next())
        //    if (info->mpData == dt) return info;
        //return 0;

        foreach (ContactInfo info in mlContacts)
        {
            if (info.mpData == dt) return info;
        }
        return null;
    }
    public virtual ContactInfo FindContact(IGroupAi grp, DWORD index)
    {
        //for (ContactInfo* info = mlContacts.Head(); info; info = info->Next())
        //    if (info->mpGroupCont->ai == grp && info->mpData->Number + 1 == index) return info;
        //return 0;

        foreach (ContactInfo info in mlContacts)
        {
            if (info.mpGroupCont.ai == grp && info.mpData.Number + 1 == index) return info;
        }
        return null;
    }
    public virtual ContactInfo FindContact(IBaseUnitAi bua)
    {
        //for (ContactInfo* info = mlContacts.Head(); info; info = info->Next())
        //    if (info->mpAi == ai) return info;
        //return 0;

        foreach (ContactInfo info in mlContacts)
        {
            if (info.mpAi == bua) return info;
        }
        return null;

    }
    void SetInvulnerable(IGroupAi grp_ai, DWORD index, bool god)
    {
        ContactInfo info = FindContact(grp_ai, index);
        if (info != null)
            info.SetGodness(god);

    }
    protected ContactInfo mpJustDamaged;

    // server parameters
    DWORD mAdminPassword;
    bool IsServerSetuping() { return stdlogic_dll.mCurrentTick <= 1 && mpData != null; }

    // notify
    char[] mNotifyBuffer = new char[1024];

    public abstract void Console(iClient cl, string format, params string[] v);
    public virtual void Say(DWORD side_code, string pFormat, RadioMessage pData) { }
    public abstract void NotifyClient(iClient cl, string format, params string[] args);
    public abstract void NotifyClients(string format, params string[] args);
    public abstract void NotifyHost(string format, params string[] args);
    void fillRandomPhrase(ref RadioMessage info)
    {
        // (const_cast<RadioMessage*>(info))->myPhraseIndex = RandN(igame->getPhrasesCount(info->Code));
        info.myPhraseIndex = (DWORD)RandomGenerator.RandN(igame.getPhrasesCount(info.Code));
    }

    public abstract void FinishListName(iClient pClient, DWORD d);
    bool mNotifyEnable;
    bool mEliminateColorNames;
    bool IsColorNamesEnabled() { return mEliminateColorNames == false; }




    protected void SetNotifyEnable(bool enable) { mNotifyEnable = enable; }
    bool IsNotifyEnable() { return mNotifyEnable; }
    // skill
    public DWORD mSkill;

    // events
    IEventProcessor mpEventProcessor;
    // external
    int mRefCount;
    public IGame igame;
    public IServer iserver;
    public AiMissionData mpData;
    bool mScriptExecuted;
    bool server_shutdown;
    public abstract void ExecuteBatch(string batch_name);
    public virtual void ExecuteScript()
    {
        if (!mScriptExecuted)
        {
            IVmFactory fact = getTopVmFactory();
            fact.AddRef();
            myVM.setFactory(fact);
            executeScript(mpData.GetAiScript(), "MissionProperty");

            myVM.run();
            mScriptExecuted = true;
        }
    }
    // ===============================

    protected iUnifiedVariableContainer mGameOptions;
    iUnifiedVariableContainer mServerOptions;
    public int GetIntValue(iUnifiedVariableContainer cnt, string name, int def_value)
    {
        iUnifiedVariableInt val = cnt != null ? cnt.GetVariableTpl<iUnifiedVariableInt>(name) : null;
        return val != null ? val.GetValue() : def_value;
    }


    protected CommandsApi GetCommandsApi()
    {
        return iserver.GetCommands();
    }
    public UniVarContainer GetVars(string name)
    {
        iUnifiedVariableContainer data = iserver.GetCurrentEventData();
        return data != null ? (UniVarContainer)data.createContainer(name) : null;
    }

    public bool IsServerShutdowned() { return server_shutdown; }

    // camera movement
    CameraInfo mLastCamera = new CameraInfo(); //TODO Ну не место здесь определению камеры

    // status
    bool mNeutralsHearAll;
    void SetNeutralHear(bool hear) { mNeutralsHearAll = hear; }
    bool IsNeutralHear() { return mNeutralsHearAll; }

    LinkedList<MissionTerminator> mlTerminators = new LinkedList<MissionTerminator>();

    void ClearTerminator(DWORD name)
    {
        if (Constants.THANDLE_INVALID == name)
            mlTerminators.Clear();
        else
        {
            //for (MissionTerminator* trm = mlTerminators.Head(); trm; trm = trm->Next())
            //    if (trm->Name() == name)
            //    {
            //        delete mlTerminators.Sub(trm);
            //        break;
            //    }
            for (LinkedListNode<MissionTerminator> trm = mlTerminators.First; trm != null; trm = trm.Next)
                if (trm.Value.Name() == name)
                {
                    mlTerminators.Remove(trm);
                    break;
                }
        }

    }
    void ReleaseTerminator(DWORD name)
    {
        for (LinkedListNode<MissionTerminator> trm = mlTerminators.First; trm != null; trm = trm.Next)
            if (trm.Value.Name() == name || Constants.THANDLE_INVALID == name)
            {
                trm.Value.Release();
                break;
            }

    }
    bool ProcessTerminators(float scale)
    {
        for (LinkedListNode<MissionTerminator> trm = mlTerminators.First; trm != null;)
        {
            LinkedListNode<MissionTerminator> nxt = trm.Next;
            if (trm.Value.IsReleased())
                mlTerminators.Remove(trm);
            else if (false == trm.Value.Update(scale))
                return false;
            trm = nxt;
        }
        return true;

    }

    #region Section : quit
    void Quit(DWORD name, bool terminate, bool call_quit)
    {
        mlTerminators.AddLast(new MissionTerminator(name, -1f, 1f, this, terminate, call_quit, Constants.THANDLE_INVALID));
    }
    void StartQuit(DWORD name, float call_time, float quit_time, bool terminate, bool call_quit, DWORD code)
    {
        mlTerminators.AddLast(new MissionTerminator(name, call_time, quit_time, this, terminate, call_quit, code));
    }
    #endregion //Section : quit
    public abstract DWORD GetClientsCountForAppear(DWORD flag);

//#if LOG_CREATING_UNITS
    void DescriptUnit(DWORD name)
    {
        string  ret = igame.GetObjectDataName(name);
        if (ret!=null)
            gamelog.Message("Create: %08X \"%s\"", name, ret);
        else
            gamelog.Message("Create: %08X not found", name);

    }
    //#endif // LOG_CREATING_UNITS
    // menu
    uint prev_selected_item;


    protected DWORD InEngBayCount()
    {
        int n = 0;
        for (LinkedListNode<MissionClient> mcl = mlClients.First; mcl != null; mcl = mcl.Next)
        {
            if (!isHost(mcl.Value) && mcl.Value.Client().IsInEngBay())
                n++;
        }
        return (DWORD)n;

    }

    char[] myMissionType = new char[3];
    protected void setMissionType(char ch) { myMissionType[0] = ch; }
    void setMissionClose(bool ch)
    {
        myMissionType[1] = ch ? 'O' : '\0';
        myMissionType[2] = '\0';
    }

    // groups
    List<DynamicGroupData> mlClientGroupDatas;
    protected List<GroupAiCont> myGroups = new List<GroupAiCont>();
    void CreateEnvironment()
    {
        Debug.Log("Creating groups: " + mpData.GetGroupsCount());
        for (int i = 0; i < mpData.GetGroupsCount(); ++i)
        {
            GROUP_DATA Grp = mpData.GetGroups()[i];
            // only not hidden
            if (Grp.IsDestroyed())
            {
                Debug.Log(string.Format("Group dead: {0} fl:{1} vs {2}", Grp.Callsign, Grp.GetFlag((uint)UnitFlags.CF_DESTROYED).ToString("X8"), UnitFlags.CF_DESTROYED.ToString("X8")));
                continue;
            }
            ;
            Debug.Log("StdMissionAi CreateEnvironment " + Grp.Callsign + "\n===start===\n" + Grp.AiScript + "\n===end===");
            CreateGroupAi(Grp.AI, ref Grp);
        }
        //Debug.Log("Groups Created: " + mpData.GetGroupsCount());

        ////для отладки загружаем группы по умолчанию (из Default Mission).
        //for (int i = 0; i < mpData.GetDefGroupsCount(); ++i)
        //{
        //    GROUP_DATA Grp = mpData.GetDefGroups()[i];
        //    // only not hidden
        //    if (Grp.IsDestroyed()) continue;
        //    CreateGroupAi(Grp.AI, Grp);
        //}
    }
    public abstract void ClearUnitDataHandles(iCheckAppear ca);
    protected IGroupAi CreateGroupAi(string name, ref GROUP_DATA data)
    {
        Debug.Log("StdMissionAi CreateGroupAi " + data.Callsign + "\n===start===\n" + data.AiScript + "\n===end===");
        IGroupAi grp_ai = iserver.CreateGroupAi(name, data);
        if (grp_ai != null)
        {
            data.Created();
            //Debug.Log(string.Format("AI {0} created for {1}, created status: {2}", name, data.Callsign,data.IsCreated().ToString()));
            OnCreateGroup(grp_ai, data);
        }
        else Debug.Log(string.Format("AI {0} NOT created for {1}", name, data.Callsign));
        return grp_ai;

    }
    public abstract void DeleteGroup(IGroupAi grp);
    public IGroupAi GetGroupByIDLocal(GROUP_DATA gd, DWORD id)
    {
        for (int i = 0; i < myGroups.Count; ++i)
        {
            if (myGroups[i].mpGroupData.ID == id)
                return myGroups[i].ai;
        }
        return null;
    }
    public abstract DWORD GetUniqueGroupID();
    public abstract void CheckUniqueCallsigns();
    public abstract GroupAiCont FindGroupCont(DWORD id);


    // mission status
    protected int myMissionStatus;
    protected CDWrapperClientInfo myClientInfo;





    protected int getMissionStatus() { return myMissionStatus; }
    // messages
    public char[] mMessageBuffer = new char[1024];
    iUnifiedVariableContainer mpMessages;
    #region messages
    public string GetMessage(DWORD msg_code)
    {
        //char* str = igame->AddLocalizedText(mMessageBuffer, 1024, msg_code);
        //return str ? mMessageBuffer : sAiNothing;

        //TODO! Сделать и проверить получение текста сообщения нормально!
        string buffer = "";
        string str = igame.AddLocalizedText(ref buffer, 1024, msg_code);
        if (str != null)
        {
            mMessageBuffer = buffer.ToCharArray();
        }
        return str != null ? buffer : Parsing.sAiNothing;
    }
    #endregion messages

    // admin commands
    List<IPHolder> mtBannedIPs = new List<IPHolder>();
    public abstract void AdminKick(MissionClient admin, string arg1, string arg2);
    public abstract void AdminBan(MissionClient admin, string arg1, string arg2);
    public abstract void AdminUnBan(MissionClient admin, string arg1, string arg2);
    public abstract void AdminLog(MissionClient admin, string arg1, string arg2);
    public abstract void AdminSaveBanList(MissionClient admin, string arg1, string arg2);
    public abstract void AdminMute(MissionClient admin, string arg1, string arg2, bool mute);


    // table working
    protected RowInfo mPlainColor;
    protected RowInfo mHeadColor;

    // type cast
    public IMissionAi GetIMissionAi() { return this; }
    public IAi GetIAi() { return this; }

    // menu
    IMenuHolder mpMenuHolder;

    // API
    public StdMissionAi()
    {
        mRefCount = 1;
        mpMenuHolder = null;
        mpEventProcessor = null;
        mpJustDamaged = null;
        mNotifyEnable = true;
        mScriptExecuted = false;
        mpMessages = null;
        mEliminateColorNames = true;
        mMaxPlayers = 4;
        mMaxObservers = 4;
        mNoDamageFromFriendly = false;
        mpPrinter = null;
        mpMenu = null;
        myVM = null;
        //myMission = this;
        myMission = new MissionService<StdMissionAi>(this);
        myRadio = new RadioService<IMissionAi>(GetIMissionAi());
        mySender = new MissionSender<StdMissionAi>(this);
        //mySkillSrv = new MySkillSrv(this, true);
        mySkillSrv = new SkillService<StdMissionAi>(this, true);
        myMessageProcessor = ClassFactory.CreateMessageProcessor(this);
    }
    //:mRefCount(1),mpMenuHolder(0),
    //                  mpEventProcessor(0),mpJustDamaged(0),mNotifyEnable(true),
    //                  mScriptExecuted(false),
    //                  mpMessages(0),mEliminateColorNames(true),
    //                  mMaxPlayers(4),mMaxObservers(4),mNoDamageFromFriendly(false),
    //                  mpPrinter(0),mpMenu(0),
    //                  myVM(0),
    //                  myMission(this),
    //                  myRadio(GetIMissionAi()),
    //                  mySender(this),
    //                  mySkillSrv(this,true)

    ~StdMissionAi()
    {
        saveMissionTriggers(myTriggers, "GlobalTriggers");

        GetCommandsApi().UnRegister((CommLink)this);
        myGroups.Clear();
        mlContacts.Clear();
        mlClientGroupDatas.Clear();
        mlClients.Clear();
        //SafeRelease(mpMenuHolder);
        //SafeRelease(mpEventProcessor);
        //SafeRelease(mpMessages);

        // devices
        if (mpPrinter != null) mpPrinter = null;
        if (mpMenu != null) mpMenu = null;

        ScoreMessage("ShutdownGame", Parsing.sAiEmpty);
        ScoreMessage(Parsing.sAiEmpty, ServerMessages.sStartDelimiter);
        mlTerminators.Clear();

        Parsing.closeErrorLog();
        Asserts.AssertBp(myVM.getThreadCount() == 0);
        myVM = null;
        int count = myErrorLog.RefCount();
        Asserts.AssertBp(count == 1);

    }

    // IMemory
    public int Release()
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
    public void AddRef() { mRefCount++; }

    // IObject
    public virtual object Query(uint cls_id)
    {
        switch ((uint)cls_id)
        {
            case IMissionAi.ID: return GetIMissionAi();
            case StdMissionAi.ID: return this;
            case IAi.ID: return GetIAi();
            default: return Query2(cls_id);
        }

    }

    // IAi
    public virtual bool Update(float scale)
    {
        myTimer.update(scale);
        myCurrentDelta = scale;
        stdlogic_dll.mCurrentTime += scale;
        stdlogic_dll.mCurrentTick++;
        setContext("Mission");
        myVM.run();

        // create on first tick
        if (IsServerSetuping())
        {
            mpEventProcessor = ClassFactory.CreateEventProcessor();
            if (mpEventProcessor != null)
                if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
                {
                    AICommon.AiMessage(MessageCodes.MSG_UNKNOWN, "InitEvents", "{0} events were loaded", mpEventProcessor.GetEventsCount().ToString());
                }
            CreateEnvironment();
            ExecuteScript();    // на случай если без клиентов
                                //ExecuteBatch(sDefaultBatch);
        }
        if (myLogLimits.processTime(scale))
        {
            //#pragma message ("EEI : ailog for servers")
            //ailog->ReOpenLogFile();
            if (stdlogic_dll.gamelog != null)
                stdlogic_dll.gamelog.Message(ServerMessages.sServerLogFileReopened);
        }

        // update groups
        for (int i = 0; i < myGroups.Count; i++)
        {
            if (!myGroups[i].ai.Update(scale))
            {
                OnDeleteGroup(myGroups[i]);
                i--;
            }
        }

        if (myMessageProcessor != null)
            myMessageProcessor.Update(scale);

        return ProcessTerminators(scale);

    }
    public void ProcessRadioMessage(string msg_code, IAi caller, RadioMessage Info, bool to_all, bool say_flag)
    {
        //Debug.Log(string.Format("Mission processing message {0}", Info));
        myRadio.notifyRadioMessage(Info);
        IGroupAi grp_ai = (IGroupAi)caller.Query(IGroupAi.ID);
        GROUP_DATA grp_data = grp_ai != null ? grp_ai.GetGroupData() : null;

        DWORD recipient_code = Hasher.HshString(Info.RecipientCallsign);
        // notify groups
        for (int i = 0; i < myGroups.Count; ++i)
        {
            GroupAiCont grp = myGroups[i];
            bool send = false;
            GROUP_DATA data = grp.mpGroupData;
            //Debug.Log(string.Format("Comparing {0} vs {1} ({2} vs {3}) {4}",recipient_code.ToString("X8"),data.ID.ToString("X8"),Info.CallerCallsign,data.Callsign, recipient_code == data.ID ? "MATCH":null));
            if (Constants.THANDLE_INVALID == recipient_code)
                send = to_all || grp_data == null || data.Side == grp_data.Side || (IsNeutralHear() && (MissionSideDefines.IsSideNeutral(data.Side) || MissionSideDefines.IsSideNeutral(grp_data.Side)));
            else
                send = (recipient_code == data.ID);
            if (send)
            {
                //Debug.Log(string.Format("Group {0} (ai {2}) processing message {1}", data.Callsign, msg_code,grp.ai));
                grp.ai.ProcessRadioMessage(msg_code, caller, Info, to_all, say_flag);
            }
        }
        if (say_flag)
        {
            fillRandomPhrase(ref Info);
            Say(grp_data != null ? grp_data.Side : MissionSideDefines.SIDE_MISSION, null, Info);
        }

    }


    //IMissionAi
    public virtual void setFcScale(float scale) { }
    public AiEventInfo GetMessageCode(string event_name)
    {
        AiEventInfo einfo = new AiEventInfo();
        einfo.mEventCode = event_name != null ? Hasher.HshString(event_name) : Constants.THANDLE_INVALID;
        if (Constants.THANDLE_INVALID != einfo.mEventCode && mpEventProcessor != null)
        {
            einfo = mpEventProcessor.GetEventInfo(einfo.mEventCode);
            //if (einfo.mCode == AIGroupsEvents.EMPTY_EVENT)
            if (einfo.mCode == AIGroupsEvents.EMPTY_CODE)
            {
                if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
                {
                    AICommon.AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, "message for event \"{0}\" not found", event_name != "" ? event_name : Parsing.sAiEmpty);
                }
            }
        }
        return einfo;
    }

    public IGroupAi GetGroupByID(DWORD id)
    {
        for (int i = 0; i < myGroups.Count; ++i)
        {
            if (myGroups[i].mpGroupData.ID == id)
                return myGroups[i].ai;
        }
        return null;

    }
    public int UnitExistsByIndex(DWORD grp_id, DWORD un_index)
    {
        IGroupAi grp = GetGroupByIDLocal(null, grp_id);
        if (Constants.THANDLE_INVALID != un_index)
        {
            ContactInfo info = FindContact(grp, un_index);
            return info != null ? (int)info.mpData.Number + 1 : 0;
        }
        else
            //return grp!=null ? true : false;
            return grp != null ? 1 : 0;
    }

    public int UnitExistsByHandle(DWORD grp_id, DWORD hndl_id)
    {
        ContactInfo info = FindContact(hndl_id);
        if (info == null) return 0;
        if (info.mpGroupCont.mpGroupData.ID == grp_id)
            return (int)info.mpData.Number + 1;
        return 0;

    }
    public abstract bool GetContactInfo(iContact cnt, out DWORD grp_id, out DWORD un_index, out DWORD side);
    public abstract GROUP_DATA GetExistedGroupData(DWORD grp_id);
    public virtual MARKER_DATA GetMarkerData(DWORD mrk_id)
    {
        return mpData != null ? mpData.GetMarkerByID(mrk_id) : null;
    }
    public IGame GetIGame()
    {
        return igame;
    }

    // campaign support
    public abstract DWORD CountClientsAround(Vector3 org, float radius);

    // menu
    public abstract int GetMenuItems(iClient pClient, uint id, AiMenuItem[] ami, int max_count, int page_index);

    // appear
    public bool RadioChannelIsFree()
    {
        return igame.RadioChannelIsFree();
    }

    const float REPAIR_BASE_SCAN_RADIUS = 50000f;
    // reports
    public DWORD GetRepairBase(ref RepairBaseInfo[] baseInfo, DWORD max_bases, iContact leader, IBaseUnitAi ai, bool nearest_in_repair_radius, bool report_about_nothing, DWORD land_flags)
    {
        if (leader == null) return 0;
        if (ai == null) return 0;
        iContact ai_cnt = ai.GetContact();
        if (ai_cnt == null) return 0;

        DWORD leader_side_code = (uint)leader.GetSideCode();

        float min_dist = 1000000;
        int n = 0;
        Vector3 org = ai_cnt.GetOrg();
        for (int i = 0; i < myGroups.Count; ++i)
        {
            GroupAiCont grp = myGroups[i];
            iContact cnt = grp.ai.GetLeaderContact();
            if (cnt != null)
            {
                Vector3 diff = cnt.GetOrg() - org;
                diff.y = 0;
                float len = diff.magnitude;
                if (len < REPAIR_BASE_SCAN_RADIUS && grp.ai.HaveHangars(leader_side_code, land_flags))
                { // if group have hangars
                    if (nearest_in_repair_radius)
                    {
                        if (len < min_dist && len < IHangarAi.SCAN_LAND_DISTANCE)
                        {
                            min_dist = len;
                            if (baseInfo != null)
                            {
                                baseInfo[0].mID = grp.mpGroupData.ID;
                                baseInfo[0].mOrg = cnt.GetOrg();
                            }
                            n = 1;
                        }
                    }
                    else if (n < max_bases || baseInfo == null)
                    {
                        if (baseInfo != null)
                        {
                            baseInfo[n].mID = grp.mpGroupData.ID;
                            baseInfo[n].mOrg = cnt.GetOrg();
                        }
                        n++;
                    }
                }
            }
        }
        if (report_about_nothing && n == 0 && nearest_in_repair_radius)
        {
            MissionClient mcl = FindClient(ai);
            if (mcl != null)
                say(StdMissionAiMessages.NO_BASE_AROUND, mcl.Client());
        }
        return (uint)n;
    }

    // CommLink
    public abstract void OnCommand(int i, string c, string s);

    // StdMissionAi
    public virtual void SetInterface(IGame _igame, IServer _iserver, AiMissionData msn_data)
    {
        igame = _igame;
        iserver = _iserver;
        mpData = msn_data;
        server_shutdown = false;
        mSkill = SkillDefines.SKILL_NOVICE;
        myScoreLog = LogFactory.CreateLOG("Scores");
        if (myScoreLog != null)
            myScoreLog.OpenLogFile(false);
        mpMenuHolder = ClassFactory.CreateMenuHolder("EchelonRootMenu");
        if (mpMenuHolder != null)
        {
            IMenuChanged mn = ClassFactory.CreateMenuChanger(null, GetIMissionAi());
            if (mn != null)
            {
                mn.AddRef();
                mpMenuHolder.SetMenuChangedCallback(mn);
                mn.Release();
            }
            myMenuService = new MenuService<IMenuHolder>();
            myMenuService.initialize(mpMenuHolder);
        }
        // devices
        mPlainColor = new RowInfo();
        mHeadColor = new RowInfo();
        mPlainColor.Set(HudDataColors.HUDCOLOR_PLAIN_TEXT, HudDataColors.HUDCOLOR_PLAIN_BACK, ST_PLAIN_FONT_SIZE, ST_PLAIN_FONT_SPACE);
        mHeadColor.Set(HudDataColors.HUDCOLOR_HEADER_TEXT, HudDataColors.HUDCOLOR_HEADER_BACK, ST_HEADER_FONT_SIZE, ST_HEADER_FONT_SPACE);

        mpPrinter = new CDWrapperPrinter("PRN", mlClients);
        mpMenu = new CDWrapperMenu("MNU", mlClients);

        SetNeutralHear(false);
        mServerOptions = iserver.GetOptions("Server");
        mGameOptions = iserver.GetOptions("Game");
        myDifficulty = GetIntValue(mGameOptions, "Difficulty", 2);
        mpMessages = stdlogic_dll.mpAiData.GetVariableTpl<iUnifiedVariableContainer>("Messages");
        //TODO! Вернуть как было уровень журналирования событий !
        AICommon.SetDebugLevel(AICommon.DEBUG_HARD);
        //AICommon.SetDebugLevel(AICommon.DEBUG_MEDIUM);
        //AICommon.SetDebugLevel((uint)GetIntValue(mGameOptions, sDebugLevelName, AICommon.DEBUG_NONE));

        mEliminateColorNames = GetIntValue(mServerOptions, sColorNames, 1) == 0;
        mMaxPlayers = (uint)GetIntValue(mServerOptions, sMaxPlayers, 4);
        mMaxObservers = (uint)GetIntValue(mServerOptions, sMaxObservers, 4);
        mNoDamageFromFriendly = GetIntValue(mGameOptions, sNoDamageFromFriends, 0) == 1;

        if (mMaxPlayers > MAX_CONNECTIONS)
            mMaxPlayers = MAX_CONNECTIONS;
        if (mMaxObservers > MAX_CONNECTIONS)
            mMaxObservers = MAX_CONNECTIONS;
        mAdminPassword = 0x99650CF9; // "echelon"

        ScoreMessage(Parsing.sAiEmpty, ServerMessages.sStartDelimiter);
        ScoreMessage(ServerMessages.sInitGame, "\\arenaname\\%s\\gamename\\%s\\maxplayers\\%d\\maxconnections\\%d\\colornames\\%d", mpData.GetTitle(), mpData.GetAi(), GetMaxPlayers().ToString(), GetMaxObservers().ToString(), !mEliminateColorNames ? "false" : "true");
        if (GetMissionDescription() != "" && stdlogic_dll.gamelog != null)
            stdlogic_dll.gamelog.Message(GetMissionDescription(), (GetMissionVersion() >> 16).ToString("X8"), (GetMissionVersion() & 0x0000FFFF).ToString("X8"));

#if LOG_CREATING_UNITS
        gamelog->Message("----- Mission units: -------");
        for (i = 0; i < mpData->GetGroupsCount(); ++i)
        {
            GROUP_DATA & Grp = mpData->GetGroups()[i];
            for (int j = 0; j < Grp.nUnits; ++j)
            {
                UNIT_DATA* un = Grp.Units + j;
                DescriptUnit(un->CodedName);
            }
        }
        gamelog->Message("----- Campaign units: -------");
        for (i = 0; i < mpData->GetDefGroupsCount(); ++i)
        {
            GROUP_DATA & Grp = mpData->GetDefGroups()[i];
            for (int j = 0; j < Grp.nUnits; ++j)
            {
                UNIT_DATA* un = Grp.Units + j;
                DescriptUnit(un->CodedName);
            }
        }
        gamelog->Message("-----------------------------");
#endif //LOG_CREATING_UNITS
        GetCommandsApi().RegisterCommand(sServerPassword, this, 1);
        GetCommandsApi().RegisterCommand(sServerLogMaxTime, this, 1);
        GetCommandsApi().RegisterCommand(sServerLogMaxSize, this, 1);

        setMissionType('S');
        setMissionClose(false);

        setMissionStatus((int)GameStatuses.MissionStatuses.MS_Normal);
        // vm
        myStdMsnFactory = Factories.createStdMsnFactory(getIQuery());
        myVM = createVm();

        loadMissionTriggers(myTriggers, "GlobalTriggers");
    }

    public void DeleteGroupUnits(IGroupAi ai, int unit_count, DWORD[] units, bool explode, bool physically)
    {
        IBaseUnitAi lead = null;
        while ((lead = ai.GetNextUnit(lead)) != null)
        {
            bool del = true;
            if (unit_count != 0)
            {  // if concrete units are selected (suicide)
                UNIT_DATA dt = lead.GetUnitData();     // try to find this unit in DEATH list
                int i;
                for (i = 0; i < unit_count; ++i)
                    if (units[i] == dt.Number)
                        break;
                if (i == unit_count)       // if not finded then don't kill them
                    del = false;
            }
            if (del)
            {    // delete contact with explode/ or not explode
                if (physically)
                {
                    iContact cnt = lead.GetContact();
                    DeleteContact(cnt, explode);
                }
                else
                {
                    lead.Suicide();
                }
            }
        }

    }
    public abstract string GetTeamName(DWORD team_code);
    public virtual void OnDeleteGroup(GroupAiCont grp_cont)
    {
        GROUP_DATA grp_data = grp_cont.mpGroupData;
        Debug.Log("Removing Contacts of " + grp_data.Callsign + "(" + grp_data.Units.Length + ")");
        for (LinkedListNode<ContactInfo> info = mlContacts.First; info != null;)
        {
            LinkedListNode<ContactInfo> nxt = info.Next;
            if (info.Value.mpGroupCont.mpGroupData == grp_data)
            {
                //Debug.Log("\tGroup data matched unit " + info.Value.mpData.Number + " mHandle "+ info.Value.mHandle.ToString("X8"));
                iContact cnt = iserver.GetContactByHandle(info.Value.mHandle);
                //Debug.Log("\tProcessing contact: " + (cnt == null? "FAILED":cnt));
                mlContacts.Remove(info.Value);
                if (cnt != null)
                    DeleteContact(cnt, false);
            }
            info = nxt;
        }

        myGroups.Remove(grp_cont);
        //// check if this is players group
        //for (MissionClient mcl = mlClients.Head(); mcl; mcl = mcl->Next())
        //{
        //    if (mcl->Group() == grp_cont->ai)
        //        mcl->SetGroup(0);
        //}

        //if (myMessageProcessor)
        //    myMessageProcessor->onGroupDestroy(grp_cont->ai);

        //GROUP_DATA grp_data = grp_cont.mpGroupData;

        // delete contacts
        //for (LinkedListNode<ContactInfo> info = mlContacts.First; info!=null;)
        //{
        //    ContactInfo nxt = info.Next.Value;
        //    if (info.Value.mpGroupCont.mpGroupData == grp_data)
        //    {
        //        iContact cnt = iserver.GetContactByHandle(info.mHandle);
        //        delete mlContacts.Sub(info);
        //        if (cnt)
        //            DeleteContact(cnt, false);
        //    }
        //    info = nxt;
        //}

        //myGroups.erase(myGroups.find(grp_cont));

        //for (DynamicGroupData* mdt = mlClientGroupDatas.Head(); mdt; mdt = mdt->Next())
        //{
        //    if (mdt->GetGroupData() == grp_data)
        //    {
        //        delete mlClientGroupDatas.Sub(mdt);
        //        break;
        //    }
        //}


    }
    public void OnCreateGroup(IGroupAi ai, GROUP_DATA data)
    {
        myGroups.Add(new GroupAiCont(ai, data));
    }

    public virtual ContactInfo OnCreateContact(IBaseUnitAi ai, iContact cnt, UNIT_DATA dt, IGroupAi grp)
    {
        GroupAiCont finded = null;
        for (int i = 0; i < myGroups.Count; ++i)
        {
            finded = myGroups[i];
            if (finded.ai == grp) break;
        }
        Asserts.AssertBp(finded != null);
        DWORD hndl = cnt.GetHandle();
        ContactInfo new_cnt = new ContactInfo(ai, hndl, dt, finded);
        //mlContacts.Add(new_cnt);
        mlContacts.AddLast(new_cnt);
        return new_cnt;

    }
    public virtual void DeleteContact(iContact cnt, bool explode)
    {
        Asserts.AssertBp(cnt!=null);
        if (cnt.GetState() != iSensorsDefines.CS_DEAD)
            iserver.DeleteUnit(cnt, explode);
    }
    public virtual void OnDeleteContactHandle(DWORD handle, bool landed_or_repaired)
    {
        Debug.Log("OnDeleteContactHandle " + handle.ToString("X8"));
        ContactInfo info = FindContact(handle);
        if (info!=null)
        {
            MissionClient mcl = FindClientH(handle);
            if (mcl!=null)
            {
                ProcessClientDeath(info, mcl, landed_or_repaired);
                mcl.Killed();
            }
            ProcessContactDeath(info, landed_or_repaired);
            //delete mlContacts.Sub(info);
            mlContacts.Remove(info);
            info.Dispose();
        }
    }

    public abstract string GetMissionDescription();
    public abstract DWORD GetMissionVersion();
    public virtual void ProcessClientDeath(ContactInfo info, MissionClient mcl, bool landed_or_repaired)
    {
        CameraInfo camera_info = new CameraInfo();
        Asserts.AssertBp(mcl.Contact() != null);
        if (Constants.THANDLE_INVALID == info.GetLastAttacker())
        {
            camera_info.mMode = Hasher.HshString("free");
            Vector3 speed = mcl.Contact().GetSpeed();
            float len = speed.magnitude;
            if (CCmp(len))
            {
                //speed = Distr.Sphere();
                //if (speed.y > 0.f) speed.y *= -1.f;
            }
            else
            {
                speed /= len;
            }
            camera_info.mOrg = mcl.Contact().GetOrg() - speed * (200f + RandomGenerator.Rand01() * 20f);
            if (Mathf.Abs(speed.y) < 0.99f)
            {
                camera_info.mPitch = Mathf.Asin(speed.y);
                camera_info.mHeading = Mathf.Atan2(speed.x, speed.z);
            }
            else
            {
                camera_info.mPitch = 0f;
                camera_info.mHeading = 0f;
            }

        }
        else
        {
            camera_info.mMode = Hasher.HshString("tracking");
            camera_info.mHandle = info.GetLastAttacker();
            camera_info.mOrg = mcl.Contact().GetOrg();
        }
        mcl.SetCamera(camera_info);

    }
    public abstract void ProcessContactDeath(ContactInfo info, bool landed_or_repaired);
    public abstract void ClientInfoChanged(MissionClient mcl);

    public abstract bool IsRepairEnabled();
    public abstract bool IsRespawnAfterRepair();
    public void OnQuit(DWORD name, DWORD code) { }
    // admin
    public abstract void OnChangeAdminStatus(MissionClient mcl, bool admin);
    public abstract bool OnMutePlayer(MissionClient mcl, bool mute, bool connecting = false);


    public abstract string GetMissionType();


    // table
    public abstract WORD GetFragRowsCount();
    public abstract bool FillFragRows(WORD rows_count, WORD[] rows);

    // message processor
    public IMessageProcessor MessageProcessor() { return myMessageProcessor; }
    public bool presentMessage(IGroupAi ai, crc32 caller_name, crc32 msg, bool sender, int index)
    {
        return myMessageProcessor.presentMessage(ai, caller_name, msg, sender, index);
    }


    // ================ new =======================
    MenuService<IMenuHolder> myMenuService;
    TimeService myTimer = new TimeService();
    MissionService<StdMissionAi> myMission;
    TriggersSystem myTriggers = new TriggersSystem();
    RadioService<IMissionAi> myRadio;
    MissionSender<StdMissionAi> mySender;
    SkillService<StdMissionAi> mySkillSrv;
    ILog myScoreLog;





    // VM
    IVm myVM;
    IErrorLog myErrorLog;
    protected IVmFactory myStdMsnFactory;

    public void setContext(string name) { myErrorLog.setExecuteContext(name); }
    public void setSource(string name, string text)
    {
        myErrorLog.setSource(name, text);
    }
    public IVm createVm()
    {
        if (myErrorLog == null)
        {
            //myErrorLog = createObject<ErrorLog>(&myTimer);
            myErrorLog = new ErrorLog(myTimer);
            if (AICommon.IsLogged(AICommon.DEBUG_HARD))
            {
                Parsing.openErrorLog(myErrorLog);
            }
        }
        return AiScriptVm.createAiScriptVm(myErrorLog);

    }
    public IErrorLog getErrorLog() { return myErrorLog; }
    public ITimeService getTimer() { return myTimer; }
    public IRadioService getRadio() { return (IRadioService)myRadio; }

    public IMenuService getMenu() { return (IMenuService)myMenuService; }

    bool executeScript(string text, string namesp)
    {
        if (text != null && text != "")
        {
            if (!text.EndsWith('\0')) text += '\0';
            setSource("Mission", text);
            return myVM.parseScript(text, namesp);//TODO возможно, тут \0 не нужен
        }
        else
            return true;

    }

    protected IQuery getIQuery() { return (IQuery)this; }
    float myCurrentDelta;

    public abstract IVmFactory getTopVmFactory();

    protected void loadMissionTriggers(ITriggerService srv, string folder, string second_folder = "")
    {
        iUnifiedVariableContainer root = GetVars(folder);
        if (root != null && second_folder != "")
            root = (UniVarContainer)root.createContainer(second_folder);
        if (root != null)
        {
            LoadTriggers enumer = new LoadTriggers(srv);
            LoadTriggers.enumGlobalTriggers((UniVarContainer)root, enumer);
        }

    }
    void saveMissionTriggers(ITriggerService srv, string folder, string second_folder = "")
    {
        iUnifiedVariableContainer root = GetVars(folder);
        if (root != null && second_folder != "")
            root = root.createContainer(second_folder);
        if (root != null)
            LoadTriggers.saveTriggers(srv, (UniVarContainer)root);

    }

    public void SetSkill(DWORD skill, bool reset_skills)
    {
        if (skill == Constants.THANDLE_INVALID)
        {
            //#pragma message "EEI : Here is bug reporting !"
            return;
        }
        mSkill = skill;
        if (reset_skills)
        {
            for (int i = 0; i < myGroups.Count; ++i)
                myGroups[i].ai.SetSkill(skill);
        }

    }

    public bool isRestartSupported() { return mlClients.Count == 1; }

    public void SetGodness(string mpGroup, DWORD mIndex, bool god)
    {
        DWORD hs = Hasher.HshString(mpGroup);
        IGroupAi grp_ai = GetGroupByIDLocal(null, hs);
        if (grp_ai != null)
        {
            if (0 == mIndex)
            {
                GROUP_DATA grp_data = grp_ai.GetGroupData();
                for (int i = 0; i < grp_data.nUnits; ++i)
                    SetInvulnerable(grp_ai, (uint)(i + 1), god);
            }
            else
                SetInvulnerable(grp_ai, mIndex, god);
        }
        else if (AICommon.IsLogged(AICommon.DEBUG_MEDIUM))
        {
            AICommon.AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, "can't set godness - group \"{0}\" not found", mpGroup);
        }

    }

}

public class GroupAiCont
{
    public GROUP_DATA mpGroupData;
    public IGroupAi ai;

    ~GroupAiCont()
    {
        ai.Release();
    }

    public GroupAiCont(IGroupAi _ai, GROUP_DATA grp_dt)
    {
        ai = _ai;
        mpGroupData = grp_dt;
    }

    public override string ToString()
    {
        return mpGroupData != null ? mpGroupData.ToString() : "Empty Container";
    }
};

public class RadioService<T> : HandlerService<IRadioService, IRadioHandler>, IRadioService where T : IMissionAi
{
    public bool isRadioFree()
    {
        return myMsn.RadioChannelIsFree();
    }

    public void sendRadioMessage(string msg_code, IAi caller, RadioMessage Info, bool to_all, bool say_flag)
    {
        //Debug.Log(string.Format("Broadcasting {0} from [{1} {2}]",msg_code,Info.CallerCallsign,Info.CallerIndex));
        myMsn.ProcessRadioMessage(msg_code, caller, Info, to_all, say_flag);
    }

    public void notifyRadioMessage(RadioMessage Info)
    {
        //Debug.Log(string.Format("Notifying to {0} handlers from {1} start", myHandlers.Count, Info.CallerCallsign + Info.CallerIndex));
        for (int i = 0; i < myHandlers.Count;)
        {
            //Debug.Log(string.Format("myHandler {0} {1}", i,myHandlers[i].checkMessage(Info)));
            //i = processRemove(myHandlers[i].checkMessage(Info), i);
            bool remove = myHandlers[i].checkMessage(Info);
            //Debug.Log(string.Format("myHandler {0} hit: {1}", i, remove));
            i = processRemove(remove, i);
        }
        //Debug.Log(string.Format("Notifying to {0} handlers from {1} end", myHandlers.Count, Info.CallerCallsign + Info.CallerIndex));
    }


    public RadioService(T imp)
    {
        myMsn = imp;
    }

    private T myMsn;
};


public class HandlerService<TParent, THandlerInterface> //: TParent 
{
    TParent par;
    public void registerHandler(THandlerInterface rh)
    {
        foreach (THandlerInterface result in myHandlers)
        {
            if (result.Equals(rh)) return;
        }
        myHandlers.Add(rh);
        //if (myHandlers.find(rh) < 0)
        //    myHandlers.New() = rh;
    }

    public void unregisterHandler(THandlerInterface rh)
    {
        //int n = myHandlers.find(rh);
        //if (n >= 0) myHandlers.Remove(n, n);
        myHandlers.Remove(rh);
    }

    int getCount()
    {
        return myHandlers.Count;
    }

    protected int processRemove(bool remove, int n)
    {
        if (remove)
            //myHandlers.Remove(n, n);
            myHandlers.RemoveAt(n);
        else
            n++;
        return n;
    }

    public List<THandlerInterface> myHandlers = new List<THandlerInterface>();
};

public abstract partial class StdMissionAi : IMissionAi, ISkillable
{
    public abstract bool canDrawEnemy();
    public abstract bool canUseOtherSideData();

    public int GetMarkersCount()
    {
        return (int)mpData.GetMarkersCount();
    }

    public abstract IBaseUnitAi CreateUnitAi(UNIT_DATA ud, int side, iContact hangar, IGroupAi grp);
    public abstract bool GetMessageMode(uint side);
    public abstract void GetPlayersInfo(out iAboutPlayer info);
    public abstract string IsPlayer(UNIT_DATA ud, ref string real_player_name);
    public abstract void OnAddDamage(uint VictimHandle, uint GadHandle, uint WeaponCode, float Damage, bool IsFinal);
    public abstract bool OnConnect(iClient cl);
    public abstract void OnDisconnect(iClient cl, bool dropped);

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    void IMissionAi.GetPlayersInfo(out int first, out int second)
    {
        throw new System.NotImplementedException();
    }
}

public abstract class iCheckAppear
{
    protected DWORD mClientCount;

    public iCheckAppear(DWORD num)
    {
        mClientCount = num;
    }

    protected bool isAppear(DWORD appear_flag, DWORD client_count)
    {
        Asserts.Assert(client_count < 5);
        uint[] appear_params = { CampaignDefines.CF_APPEAR_ONE_CLIENT, CampaignDefines.CF_APPEAR_TWO_CLIENTS, CampaignDefines.CF_APPEAR_THREE_CLIENTS, CampaignDefines.CF_APPEAR_FOUR_CLIENTS };

        //foreach (uint ap in appear_params)
        for (int i = 0; i < appear_params.Length; i++)
        {
            uint ap = appear_params[i];
            //if (((ap & appear_flag) == appear_flag) && (client_count == i + 1)) return true;
            if (((ap & appear_flag) != 0) && (client_count == i + 1)) return true;
        }

        return false;
        //for (int i = 0; i < sizeof(appear_params) / sizeof(appear_params[0]); i++)
        //    if ((appear_params[i] & appear_flag) && client_count == i + 1)
        //        return true;
        //return false;
    }

    iCheckAppear GetiCheckAppear() { return this; }

    // API
    public abstract bool IsGroupAppear(GROUP_DATA gd);
    public abstract bool IsUnitAppear(UNIT_DATA ud, int difficulty);
};

public static class StdMissionAiMessages
{
    //#define AIMSG(X) MSG_(x)
    //#define NO_BASE_AROUND 0xBFEAF6FA    // "mc_NoBaseAround"

    //    extern const char* sLocal;
    //    extern const char* sLocals;

    //    extern const char* sStartDelimiter;
    //    extern const char* sSrvSeparator;
    //    extern const char* sSrvSeparatorTotal;
    //    extern const char* sConnectionWasBanned;
    //    extern const char* sUnknownParameter;
    //    extern const char* sSrvBadPassword;
    //    extern const char* sAiDisconnected;
    //    extern const char* sSrvInfoUptime;
    //    extern const char* sSrvInfoContacts;
    //    extern const char* sSrvClientInfo;
    //    extern const char* sSrvBanInfo;
    //    extern const char* sAdminUnableKick;
    //    extern const char* sAdminUnableFind;
    //    extern const char* sAdminLogged;
    //    extern const char* sAdminBadIP;
    //    extern const char* sAdminBanListSaved;
    //    extern const char* sAdminBanListNSaved;
    //    extern const char* sAdminPlayerMuted;
    //    extern const char* sAdminPlayerUnMuted;
    //    extern const char* sSrvLogSizeSet;
    //    extern const char* sSrvLogTimeSet;
    //    extern const char* sServerLogFileReopened;

    //    extern const char* sServerPause;
    //    extern const char* sServerStop;
    //    extern const char* sClientScores;
    //    extern const char* sTeamSay;


    //    extern const char* sAiConnected;
    //    extern const char* sAiConnectFail;
    //    extern const char* sClientInfo;
    //    extern const char* sInitGame;

    public const uint AIMSG_OnEnterGame = 0x9B6A1FDE;
    public const uint AIMSG_OnLeaveByDisconnect = 0x0001BC27;
    public const uint AIMSG_OnLeaveByDrop = 0xD16A362F;
    public const uint AIMSG_OnNameChange = 0xA442AE3B;
    public const uint AIMSG_OnPlayerKick = 0x290AC316;
    public const uint AIMSG_OnRejectConnection = 0x70ABFBCA;
    public const uint AIMSG_OnBecomePlayer = 0x9DF35D4D;
    public const uint AIMSG_OnBecomeAdmin = 0x6D144697;
    public const uint AIMSG_OnBanConnection = 0xED1AB2DB;

    public const uint NO_BASE_AROUND = 0xBFEAF6FA;    // "mc_NoBaseAround"
}