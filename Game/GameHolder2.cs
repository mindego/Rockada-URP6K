using crc32 = System.UInt32;
using DWORD = System.UInt32;
using tuvc = iUnifiedVariableContainer;
using tuva = iUnifiedVariableArray;
using UnityEngine;

public static class GameHolder2Api
{
    public const DWORD GameNetworkVersion = 0xE9EA7005; // StormGameNetwork V15.1
    public const DWORD MAD_SOCKETS2_VERSION = 0xDAFB13EA; // MadSockets2 V2.0
    public const DWORD gAppId = MAD_SOCKETS2_VERSION ^ GameNetworkVersion ^ STORM_DATA.crc;

    public static ILocationHolder createLocationHolder(RendererApi rend, uint version = GameApi.GAME_API_VERSION)
    {
        //LogMessage("Game protool %p", gAppId);
        Debug.Log(string.Format("Game protool {0}", gAppId.ToString("X8")));
        if (version != GameApi.GAME_API_VERSION) return null;
        return new LocationHolder(rend);
    }

    public static IGameHolder createGameHolder2(GameCreateParams gcp, uint version = GameApi.GAME_API_VERSION)
    {
        if (version != GameApi.GAME_API_VERSION) return null;
        return new GameHolder2(gcp);
    }

    //IConnectionManager* CreateConnectionManager(int port, iMadSockets2Manager* m, IConnectionAcceptor* acc, int bport, int eport, bool lan_server, IGSNameHolder* hld, IpAddress* addr, crc32 mission_crc)
    //{
    //    return createConnectionManager(port, m, acc, gAppId, bport, eport, lan_server, hld, addr, mission_crc);
    //}


    public static uint getProtocolVersion()
    {
        return gAppId;
    }
}
public class GameHolder2 : IGameHolder
{
    StormGameData myData;

    UnivarDB myDb;
    UnivarDB myVars;
    //iUnifiedVariableDB myDb;
    //iUnifiedVariableDB myVars;

    BaseScene myGame;
    crc32 myMissionCrc;
    int myPort;
    bool myDedicated;
    bool myHost;
    //IGSNameHolder myGSNameHolder;
    string myHostName;
    //IpAddress myIpAddress;//отброшено в связи с отсутвием желания реализовать сеть
    //TPtr<ExtensionHelper> myCampaignName;
    ExtensionHelper myCampaignName;

    int mySavedTimeScale;

    ///TRef<IConnectionManager> myManager;
    //iMadSockets2Manager* mySockets;
    //IConnectionAcceptor* getIConnectionAcceptor() { return this; }

    //public void addSocket(iMadDedicatedSocket2 skt)
    //{
    //    myGame.AddSocket(skt);
    //}

    public virtual ServerParams getParams()
    {
        ServerParams myparams = new ServerParams();
        myparams.myCmpName = myCampaignName.getShortName();
        myparams.myMsnName = myData.mpMissionName;
        myparams.myHostName = myHostName;
        myparams.myDedicated = myDedicated;
        addOption(MsnDataFlags.MSDPO_SHOW_LEAD, myData.mShowLead ? 1 : 0, ref myparams.myFlag);
        addOption(MsnDataFlags.MSDPO_SHOW_THREAT, myData.mShowThreat ? 1 : 0, ref myparams.myFlag);
        addOption(MsnDataFlags.MSDPO_UNLIMITED_ARMOR, myData.mUnlimitedArmor ? 1 : 0, ref myparams.myFlag);
        addOption(MsnDataFlags.MSDPO_UNLIMITED_AMMO, myData.mUnlimitedAmmo ? 1 : 0, ref myparams.myFlag);
        addOption(MsnDataFlags.MSDPO_NODAM_OBJECTS, myData.mNoDamObjects ? 1 : 0, ref myparams.myFlag);
        addOption(MsnDataFlags.MSDPO_NODAM_GROUND, myData.mNoDamGround ? 1 : 0, ref myparams.myFlag);
        addOption(MsnDataFlags.MSDPO_ALLOW_ENAME, myData.mAllowEnemyName ? 1 : 0, ref myparams.myFlag);
        addOption(MsnDataFlags.MSDPO_ALLOW_ETYPE, myData.mAllowEnemyType ? 1 : 0, ref myparams.myFlag);
        addOption(MsnDataFlags.MSDPO_ALLOW_EDIST, myData.mAllowEnemyDist ? 1 : 0, ref myparams.myFlag);
        addOption(MsnDataFlags.MSDPO_NODAM_FRIENDS, myData.mNoDamFriends ? 1 : 0, ref myparams.myFlag);
        return myparams;
    }

    public void addOption(int flag, int value, ref Flags flags)
    {
        if (value != 0)
            flags.set(flag);
    }

    public int getOption(int flag, Flags flags)
    {
        return flags.get(flag);
    }


    public GameHolder2(GameCreateParams gcp)
    {
        myGame = default; //TODO - тут правильнее null
        if (myData == null) myData = new StormGameData();
        myData.mpLog = gcp.pLog;
        myData.mpInput = gcp.pInput;
        myData.myTranslator = gcp.pTranslator;
        myData.mpCommands = gcp.pCommands;
        myData.mpRenderer = gcp.pRenderer;
        myData.mpSound = gcp.pSound;
        myData.myFeedback = gcp.myFeedback;

        myDedicated = gcp.myDedicated;
        myPort = gcp.myPort;
        //mySockets = gcp.pSockets;
        //myGSNameHolder = gcp.myGSNameHolder;
        //myIpAddress = gcp.myIpAddress;
        myMissionCrc = CRC32.CRC_NULL;

        //myDb.open(ProductDefs.GetPI().getHddFile("GameData.dat"), true);
        myDb = new UnivarDB();
        myDb.open(ProductDefs.getHddFile("GameData.dat"), true);
        Debug.Log("myDb: " + myDb.getDB());
        myData.mpLocalizationDb = myDb.getDB();

        myVars = new UnivarDB();
    }

    ~GameHolder2()
    {
        close();
    }

    string getProfileFileName(out string f_name)
    {
        //wsprintf(f_name, "%s.epp", cstr(myData.myProfileName));
        f_name = string.Format("{0}.epp", myData.myProfileName);
        return f_name;
    }


    void applyKeysConfig()
    {
        tuvc keys = myData.mpControls.openContainer("Keys");
        //myData.myTranslator.UseShift(keys.getInt("ShiftAsModifier") != 0); //TODO вернуть переводилку как только будет возможно.
        //myData.myTranslator.UseCtrl(keys.getInt("CtrlAsModifier") != 0);
        //myData.myTranslator.UseAlt(keys.getInt("AltAsModifier") != 0);

        tuvc actions = keys.openContainer("ActionsList");
        tuvc gdata = myDb.GetRoot().openContainer("ActionsList");

        for (DWORD handle = 0; (handle = actions.GetNextHandle(handle)) != 0;)
        {
            tuva bk = actions.openArray(handle);
            string bk_name = bk.getNameShort();
            tuvc key = gdata.openContainer(bk_name);
            if (key != null)
            {
                string action = key.getString("Command");
                if (action != null)
                {
                    for (uint i = 0; i < bk.GetSize(); i++)
                    {
                        //Debug.Log(string.Format("Binding {0} to {1}", bk.getString(i), action));
                        myData.myTranslator.SetKeyEx(bk.getString(i), action);//TODO вернуть переводилку как только будет возможно.
                    }
                }
            }
        }
    }
//    private UnivarDB CreateDefaultProfile()
//    {
//        /* python
//          def createDefault():
//     gd = GameData.get()
//    controls = gd['DefaultControls']
//    options = gd['DefaultOptions']

//    return {'Campaigns':{},
//            'Options':options,
//            'Controls':controls,
//            'ServersList.refreshFrom':'GameSpy'}
//*/
//        UnivarDB profile = new UnivarDB();
//        profile.create();
//        if (!profile.openRoot()) throw new System.Exception("Failed to create root for profile");
//        profile.GetRoot().createString("Callsign", "Джейсон \"Wolf\" Скотт");
//        var options = profile.GetRoot().addContainer("Options",myData.mpLocalizationDb.openContainer("Options")-) ;
//        options.`
//    }
    private UnivarDB CreateDefaultProfile()
    {
        UnivarDB profile = new UnivarDB();
        profile.create();
        if (!profile.openRoot()) throw new System.Exception("Failed to create root for profile");
        //Debug.Log(profile);
        profile.GetRoot().createString("Callsign", "Джейсон \"Wolf\" Скотт");

        //tuvc options = profile.GetRoot().createContainer("Options");
        //tuvc default_options = myData.mpLocalizationDb.openContainer("\\Root\\DefaultOptions");
        tuvc default_options = (iUnifiedVariableContainer)myData.mpLocalizationDb.GetRoot();
        Debug.Log("Default Options: " + (default_options !=null ? default_options:"Failed:\n" + myData.mpLocalizationDb));

        UniCopier un = new UniCopier(default_options, profile.GetRoot());
        un.copy("DefaultOptions", "Options");
        un.copy("DefaultControls", "Controls");
        Debug.Log("Generated profile: " + profile);

        //tuvc video = options.createContainer("Video");
        //tuvc sound = options.createContainer("Sound");

        //tmpInt = sound.createInt("GameRadioVolume");
        //tmpInt.SetValue(5);
        //sound.setInt("GameRadioVolume", 5);
        //sound.setInt("GameHudVolume", 5);

        //Debug.Log("GameHudVolume: " + sound.getInt("GameHudVolume"));
        //Debug.Log("GameRadioVolume: " + sound.getInt("GameRadioVolume"));


        //tuvc game = options.createContainer("Game");
        //profile.GetRoot().openContainer("Options").createContainer("Game");
        //tuvc controls = profile.GetRoot().createContainer("Controls");
        //tuvc keys = controls.createContainer("Keys");
        //Debug.Log("Generated profile: " + profile);
        //Debug.Log(profile.GetRoot().createString("Callsign"));


        //tuvc actions = keys.createContainer("ActionsList");
        return profile;
    }
    public void initUDBPart()
    {
        string f_name;
        //TODO Загружать, а не генерировать профиль
        myVars = CreateDefaultProfile();
        //if (!myVars.open(ProductDefs.getHddFile(getProfileFileName(out f_name)), false))
        //{
        //    Debug.Log("Failed to load profile data");
        //    Debug.Log(myVars.GetRoot());
        //}
        //if (!myVars.create())
        //{
        //    Debug.Log("Failed to crate profile data");
        //    Debug.Log(myVars.GetRoot());
        //}
        //if (!myVars.open(ProductDefs.getHddFile("GameData.dat"), false))
        //{
        //    Debug.Log("Failed to load profile data");
        //    Debug.Log(myVars.GetRoot());
        //}
        Asserts.Assert(myVars.GetRoot() != null);

        myData.mpCurrentEventData = (UniVarContainer)myVars.GetRoot().createContainer("CurrentMission");

        myData.mpOptions = (UniVarContainer)myVars.GetRoot().openContainer("Options");
        myData.mpControls = (UniVarContainer)myVars.GetRoot().openContainer("Controls");

        myData.myPlayerName = myVars.GetRoot().getString("Callsign");

        tuvc cnt = myData.mpOptions.openContainer("Game");

        myData.mDifficulty = cnt.getInt("Difficulty");
        myData.mShowLead = cnt.getInt("ShowLead") != 0;
        myData.mShowThreat = cnt.getInt("ShowThreat") != 0;
        myData.mUnlimitedArmor = cnt.getInt("UnlimitedArmor") != 0;
        myData.mUnlimitedAmmo = cnt.getInt("UnlimitedAmmo") != 0;

        myData.mNoDamObjects = cnt.getInt("NoDamageFromObjects") != 0;
        myData.mNoDamGround = cnt.getInt("NoDamageFromGround") != 0;
        myData.mNoDamFriends = cnt.getInt("NoDamageFromFriends") != 0;

        myData.mAllowEnemyDist = cnt.getInt("AllowEnemiesDist") != 0;
        myData.mAllowEnemyName = cnt.getInt("AllowEnemiesName") != 0;
        myData.mAllowEnemyType = cnt.getInt("AllowEnemiesType") != 0;

        myData.mShowEnemyDist = cnt.getInt("ShowEnemiesDist") != 0;
        myData.mShowEnemyName = cnt.getInt("ShowEnemiesName") != 0;
        myData.mShowEnemyType = cnt.getInt("ShowEnemiesType") != 0;

        myData.mShowFriendlyDist = cnt.getInt("ShowFriendsDist") != 0;
        myData.mShowFriendlyName = cnt.getInt("ShowFriendsName") != 0;
        myData.mShowFriendlyType = cnt.getInt("ShowFriendsType") != 0;

        myData.mShouldDraw = false;

        if (myData.mpControls != null)
            applyKeysConfig();
    }

    void doneUDBPart()
    {
        myData.mpCurrentEventData = null;
        myData.mpOptions = null;
        myData.mpControls = null;
    }

    public virtual void open(IGameset gms, ILocationHolder hld, string profile_name, bool host)
    {
        myData.myHost = myHost = host;

        ILocation loc = hld.getCurrentLocation();
        myCampaignName = new ExtensionHelper(gms.getName());

        IMission msn = gms.getMission(0);
        myData.myGameset = gms;



        myData.myProfileName = profile_name;
        myData.mpGameMapName = loc.getGameMapName();
        myData.mpTerrainName = loc.getTerrainName();
        myData.mpMissionName = msn.getName();
        myData.mGameMapSizeX = loc.getGameMapSizeX();
        myData.mGameMapSizeZ = loc.getGameMapSizeZ();
        myData.mpVisConfigName = msn.getVisConfigName();
        myData.mpNonLocalizedMission = (UniVarContainer)gms.getOnlyMission();
        myData.mpDefaultMission = gms.getOnlyLocation();

        // переменные обмена
        myData.mShouldDraw = false;
        myData.mUseControls = true;

        // модули, общие дла кампании
        myData.mpTerrain = hld.getTerrain();
        myData.mpHash = hld.getHash();
        myData.mpCollision = hld.getCollision();
        myData.mpFpoLoader = hld.getFpoLoader();
        myData.mpGameDatas = hld.getGameDatas();
        myData.mpDataHasher = hld.getDataHasher();
        myData.mpNavigation = hld.getNavigation();

        crc32 _cmp = getCrc(myCampaignName.getShortName(), "gsd");
        crc32 _trn = getCrc(loc.getTerrainName(), "sq");
        crc32 _tbx = getCrc(loc.getTerrainName(), "bx");

        myMissionCrc = _cmp + _trn + _tbx;

        myData.mpLog.Message(string.Format("Checksum cmp: {0}, trm: {1}, tbx: {2}, all {3}",
            _cmp, _trn, _tbx, myMissionCrc));
    }

    public crc32 getCrc(string name, string ext)
    {
        string buffer = string.Format("{0}.{1}", ProductDefs.getHddFile(name), ext); // TODO - Корректно получать файловые имена из _PI
        //char buffer[256];
        //wsprintf(buffer, "%s.%s", fname(name), ext);
        //return crcFile(buffer);
        return CRC32.CRC_NULL;//TODO - Корректно считать CRC32 файла
    }

    public void PatchMission()
    {

    }
    public virtual bool start(object skt, Flags flags)
    {
        //FnInit();
        initUDBPart();
        if (flags != null)
        {
            myData.mShowLead = getOption(MsnDataFlags.MSDPO_SHOW_LEAD, flags) != 0;
            myData.mShowThreat = getOption(MsnDataFlags.MSDPO_SHOW_THREAT, flags) != 0;
            myData.mUnlimitedArmor = getOption(MsnDataFlags.MSDPO_UNLIMITED_ARMOR, flags) != 0;
            myData.mUnlimitedAmmo = getOption(MsnDataFlags.MSDPO_UNLIMITED_AMMO, flags) != 0;
            myData.mNoDamObjects = getOption(MsnDataFlags.MSDPO_NODAM_OBJECTS, flags) != 0;
            myData.mNoDamGround = getOption(MsnDataFlags.MSDPO_NODAM_GROUND, flags) != 0;
            myData.mAllowEnemyName = getOption(MsnDataFlags.MSDPO_ALLOW_ENAME, flags) != 0;
            myData.mAllowEnemyType = getOption(MsnDataFlags.MSDPO_ALLOW_ETYPE, flags) != 0;
            myData.mAllowEnemyDist = getOption(MsnDataFlags.MSDPO_ALLOW_EDIST, flags) != 0;
            myData.mNoDamFriends = getOption(MsnDataFlags.MSDPO_NODAM_FRIENDS, flags) != 0;
        }
        if (skt != null)
            myGame = new RemoteScene(myData, true);
        else
            myGame = new HostScene(myData, false);
        // инитим игру
        //try //TODO Восстановить обработку исключений в самой игре
        {
            ((HostScene)myGame).Patch();
            myGame.Init();

            //TODO Реализовать выполнение dlc скриптов
            //execDlc<rname>(myData.mpCommands, "Profiles\\common", 0, false);
            ExecDlc.execDlc(myData.mpCommands, "Profiles\\common", null, false);
            //execDlc<rname>(myData.mpCommands, myData.myProfileName, 0, true);

            if (myGame.GetMissionAI() != null)
                myData.mAllowEnemyName &= myGame.GetMissionAI().canDrawEnemy();

            if (myHost)
            {
                tuvc cnt = myData.mpOptions.openContainer("Server");
                myHostName = cnt.getString("Name");
                //if (myManager = CreateConnectionManager(myPort, mySockets, getIConnectionAcceptor(), 7888, 7988, cnt->getInt("PublishOnGameSpy") == 0, myGSNameHolder, myIpAddress, myMissionCrc))
                //    myManager->setRules(myGame->GetMissionAI());
                //else
                //    throw GENERIC_EXCEPTION("Error : port %d is locked by another application!", myPort);

            }
        }
        //catch (System.Exception e)
        //{
        //    myData.mpLog.Message("startGame: ");
        //    myData.mpLog.AddException(e);
        //    close();
        //    return false;
        //}
        return true;
    }


    public virtual void close()
    {
        //myManager = null;

        if (myGame == null)
            return;

        //FnInit();

        try
        {
            myGame.Done();
        }
        catch (System.Exception e)
        {
            myData.mpLog.Message("doneGame: ");
            myData.mpLog.AddException(e);
        }
        myGame = null;

        doneUDBPart();
        //реализовать сохранение пользовательского профиля
        //RefsEnumer en;
        //enumUDBRefItems(myVars(), &en);
        //if (en.myNames.Count() == 0)
        //{
        //    string f_name;
        //    if (!myVars.save(rname(getProfileFileName(f_name))))
        //    {
        //        LogFactory.GetLog().Message("Error : can't save player profile \"%s\"", f_name);
        //    }
        //}
        //else
        //{
        //    LogFactory.GetLog().Message("List of unreleased variables in GameVars:");
        //    for (int i = 0; i < en.myNames.Count(); ++i)
        //        LogFactory.GetLog().Message("(%d) %s", en.myRefs[i], en.myNames[i]);
        //}

        myVars.close();
    }

    public virtual bool update(float Scale, bool UseControls)
    {
        //FnInit();
        //if (myManager) myManager->update();
        if (myGame != null)
        {
            //try
            //{
            //    myData.mUseControls = UseControls;
            //    if (myGame.Update(Scale))
            //        return true;
            //}
            //catch (System.Exception e)
            //{
            //    myData.mpLog.Message("updateGame: ");
            //    myData.mpLog.AddException(e);
            //}
            //TODO Вернуть обработку исключений кодом движка
            myData.mUseControls = UseControls;
            if (myGame.Update(Scale))
                return true;

            Debug.Log("Game closed!");
            close();
            return false;
        }
        return true;
    }


    public virtual void draw(float[] pViewport)
    {
        //FnInit();
        if (myGame != null)
        {
            //TODO восстановить корректную обработку исключений
            //try
            //{
                myGame.Draw(pViewport);
            //}
            //catch (System.Exception e)
            //{
            //    close();
            //    throw e;
            //}
        }
    }

    public virtual bool isStarted()
    {
        //return myGame != null && myData.mShouldDraw;
        //TODO! Корректно читать разрешение отрисовки
        return myGame != null;

    }

    public virtual bool isRestartSupported()
    {
        if (!myHost && myGame != null && myGame.GetMissionAI() != null)
            return myGame.GetMissionAI().isRestartSupported();
        return false;
    }

    public virtual void applyCraftSelection(ref Selection sel)
    {
        UnitSpawnData dt = new UnitSpawnData();
        dt.ObjectName = sel.myCraft;
        dt.Layout1Name = sel.mySlots[0];
        dt.Layout2Name = sel.mySlots[1];
        dt.Layout3Name = sel.mySlots[2];
        dt.Layout4Name = Constants.THANDLE_INVALID; //TODO = вохможно, тут правильнее преобразовать всё в uint
        myGame.applyCraftSelection(ref dt);
    }

    public virtual uint getId()
    {
        return GameHolder2Api.gAppId;
    }



    public virtual crc32 getMissionId()
    {
        return myMissionCrc;
    }

    public virtual void pause(bool on)
    {
        if (!isRestartSupported()) return;
        if (on)
            mySavedTimeScale = myGame.GetTimeScale();
        myGame.SetTimeScale(on ? 0 : mySavedTimeScale);
    }

    public virtual ILog getChatLog()
    {
        return myGame != null ? myGame.getChatLog() : null;
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }


}

public static class MsnDataFlags
{
    public const int MSDPO_SHOW_LEAD = 0x0001;
    public const int MSDPO_SHOW_THREAT = 0x0002;
    public const int MSDPO_UNLIMITED_ARMOR = 0x0004;
    public const int MSDPO_UNLIMITED_AMMO = 0x0008;
    public const int MSDPO_NODAM_OBJECTS = 0x0010;
    public const int MSDPO_NODAM_GROUND = 0x0020;
    public const int MSDPO_ALLOW_ENAME = 0x0100;
    public const int MSDPO_ALLOW_ETYPE = 0x0200;
    public const int MSDPO_ALLOW_EDIST = 0x0400;
    public const int MSDPO_NODAM_FRIENDS = 0x0800;
}

public struct ServerParams
{
    public string myCmpName;
    public string myMsnName;
    public string myHostName;
    public bool myDedicated;
    public Flags myFlag;
};

