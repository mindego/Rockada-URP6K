using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using DWORD = System.UInt32;

/// <summary>
/// GameHolder - переходник между меню и игрой
/// </summary>
public class GameHolder : StormGameData, iGameHolder
{
    const string gspErrStartGame = "CannotStartGame";
    const string gspErrGameAborted = "GameAborted";
    const string gspErrNoConfigs = "NoConfigs";
    const string gspErrLoading = "ErrorLoadingMission";

    // от iGameHolder
    // general
    public void Open(string pCmpName, string pMsnName)
    {
        Debug.Log("Open Campaign " + pCmpName + " mission " + pMsnName);
        if (GetError() != string.Empty) return;
        //Debug.Log("Resetting");
        CloseGame();
        //Debug.Log("Loading data");
        OpenData(pCmpName, pMsnName);
        mIsGameReady = IsDataReady();
        //Debug.Log("Result: " + mIsGameReady);
    }
    public void Host(string pCmpName, string pMsnName, int nPort, bool dedicated)
    {
        if (GetError() != string.Empty) return;
        Open(pCmpName, pMsnName);
        if (IsGameReady() == false) return;
        //сетевая часть отключена
        //if (mpSockManager != 0) mpListenSocket = mpSockManager->CreateListenSocket(nPort, false, true);
        //if (mpListenSocket == 0) { mpError = gspErrListenSocket; return; }
        //mpGameReportInfo->setServerType(dedicated ? "D" : "L");
        //mpGameReportInfo->setCampaignName(pCmpName);
        //mpGameReportInfo->setMissionName(pMsnName);
        //TRef<iUnifiedVariableString> str = GetString(mpOptions, "Server\\Host Name");
        //if (str)
        //{
        //    char buffer[256];
        //    str->StrCpy(buffer);
        //    mpGameReportInfo->setHostName(buffer);
        //}
        //bool lan_server = (GetIntValue(mpOptions, "Server\\PublishOnGameSpy", 0) == 0);
        //int baseport = 7778;
        //int endport = baseport + 100;
        //mpGameSpyReporter = createGameSpyReporter(baseport, endport, mpGameReportInfo, lan_server, nPort);
        //if (mpGameSpyReporter)
        //{
        //    mpLog.Message("GameSpy support initialized for %s.", lan_server ? "LAN" : "MasterServer");
        //    mpGameSpyReporter->open();
        //}
        //else
        //{
        //    mpLog.Message("Error : GameSpy initializing failed for ports %d-%d.", baseport, endport);
        //}

    }
    public void Connect(string pAddress)
    {
        throw new NotImplementedException("Network gaming is not implemented");
    }
    public bool IsDataReady()
    {
        return (mpError == string.Empty && mMsnName != CRC32.CRC_NULL);
    }

    public void Close()
    {
        CloseGame();
        CloseMission();
        CloseCampaign();
        mpError = string.Empty;
    }
    public void Update(float Scale, bool UseControls)
    {
        //FnInit();
        //if (mpGameSpyReporter)
        //    mpGameSpyReporter->update();
        //UpdateSockets();
        if (mpGame != null)
        {
            try
            {
                mUseControls = UseControls;
                if (mpGame.Update(Scale) == true) return;
            }
            catch (Exception e)
            {
                mpLog.Message("Game aborted on Update because of");
                mpLog.Message(e.ToString());
                mpLog.FlushLogFile();
                mpError = gspErrGameAborted;
                throw;
            }
            CloseGame();
        }

    }

    public bool Draw(float[] pViewport)
    {
        if (mpGame != null)
        {
            try
            {
                mpGame.Draw(pViewport);
            }
            catch (Exception e)
            {
                mpLog.Message("Game aborted on Draw because of");
                mpLog.Message(e.ToString());
                mpLog.FlushLogFile();
                mpError = gspErrGameAborted;
                CloseGame();
                throw;
                return false;
            }
        }
        return true;
    }
    public string GetError()
    {
        return mpError;
    }
    ~GameHolder()
    {
        Close();
    }
    // data access
    public iUnifiedVariableString GetBriefingText() { return mpBriefingText; }
    public iUnifiedVariableString GetBriefingCommands() { return mpBriefingCommands; }
    public iUnifiedVariableString GetMissionDescription() { return mpMissionDescription; }
    public iUnifiedVariableString GetDebriefingOnSuccessText() { return mpDebriefingOnSuccessText; }
    public iUnifiedVariableString GetDebriefingOnSuccessCommands() { return mpDebriefingOnSuccessCommands; }
    public iUnifiedVariableArray GetSuccessorsOnSuccess() { return mpSuccessorsOnSuccess; }
    public iUnifiedVariableString GetDebriefingOnFailureText() { return mpDebriefingOnFailureText; }
    public iUnifiedVariableString GetDebriefingOnFailureCommands() { return mpDebriefingOnFailureCommands; }
    public iUnifiedVariableArray GetSuccessorsOnFailure() { return mpSuccessorsOnFailure; }
    // game
    public bool IsGameReady()
    {
        //return (mpError == null && mIsGameReady == true);
        return (mpError == string.Empty && mIsGameReady == true);
    }
    /// <summary>
    /// Open game
    /// </summary>
    /// <param name="pLocDb">Localization database</param>
    /// <param name="pVarDb">>Variables database</param>
    /// <exception cref="Exception">Triggers if could not start a game</exception>
    public void OpenGame(iUnifiedVariableDB pLocDb, iUnifiedVariableDB pVarDb)
    {
        if (!IsGameReady()) throw new Exception(gspErrStartGame);

        mpLog.Message("Creating host...");
        mpGame = new HostScene(this, false);
        //int id = mpLog.GetIdent();
        //TODO!  Вернуть на место оборачивание в try
        //try
        //{
        mpLog.Message("Starting game...");
        //mpLog->IncIdent();
        mpGame.Init();
        //mpLog->DecIdent();
        mpLog.Message("Game started.");
        mpCommands.ProcessString("exec *common.dlc;");
        //}
        //catch (Exception e)
        //{
        //    mpLog.Message("Game not started because of");
        //    //mpLog.AddException(e);
        //    mpLog.Message(e.Message);
        //    //mpLog->SetIdent(id);
        //    CloseGame();
        //    mpError = gspErrStartGame;
        //    throw e;
        //}
        //if (mpGame!=null)
    }
    public bool IsInGame()
    {
        return (mpGame != null);
    }
    public bool ShouldDrawGame()
    {
        return (mpGame != null && mShouldDraw);
    }
    public void CloseGame() { }
    // own - general
    //protected BaseScene mpGame;
    public BaseScene mpGame;
    protected string mpError = string.Empty;
    protected bool mIsGameReady;
    public GameHolder(LOG pLog)
    {
        mpLog = pLog;
        mpSound = ISoundApi.CreateSInstance(null, pLog, null, false, (int)SoundApi.SOUND_ENGINE_VERSION);
        //mpRenderer = new Renderer();
        mpRenderer = Renderer.CreateRenderer(null, null, null, RendererApi.RENDERER_VERSION);
    }
    //public GameHolder(LOG pLog,const TRef<iMadSockets2Manager>& pSockets, MadInput* pInput, CommandsApi* pCommands, RendererApi* pRenderer, ISound* pSou
    //nd, ITranslator* le);
    // own - data
    protected DWORD mCmpVersion;
    DWORD mCmpName;
    DWORD mMsnName;
    iUnifiedVariableString mpBriefingText;
    iUnifiedVariableString mpBriefingCommands;
    iUnifiedVariableString mpMissionDescription;
    iUnifiedVariableString mpDebriefingOnSuccessText;
    iUnifiedVariableString mpDebriefingOnSuccessCommands;
    iUnifiedVariableArray mpSuccessorsOnSuccess;
    iUnifiedVariableString mpDebriefingOnFailureText;
    iUnifiedVariableString mpDebriefingOnFailureCommands;
    iUnifiedVariableArray mpSuccessorsOnFailure;

    public void OpenData(string pCmpName, string pMsnName)
    {
        if (GetError() != String.Empty) return;
        DWORD CmpName = Hasher.HshString(pCmpName);
        DWORD MsnName = Hasher.HshString(pMsnName);
        if (CmpName != mCmpName)
        {
            CloseMission();
            CloseCampaign();
            OpenCampaign(pCmpName, CmpName);
            if (GetError() != String.Empty) return;
            OpenMission(pMsnName, MsnName);
        }
        else
        {
            if (MsnName != mMsnName)
            {
                CloseMission();
                OpenMission(pMsnName, MsnName);
            }
        }
        if (GetError() != string.Empty) return;

        OpenOptions();
    }

    public void OpenOptions()
    {
        mpLog.Message("Loading options...");
        iUnifiedVariableDB pDb = UniVarUtils.CreateUnifiedVariableDB(Constants.UNIVARS_VERSION, "Configs.cfg", false);
        if (pDb == null) { mpError = gspErrNoConfigs; return; }
        mpLog.LogMessage("Parsing container");
        mpOptions = pDb.GetVariableTpl<iUnifiedVariableContainer>("\\Root\\Options");
        if (mpOptions == null) { mpError = gspErrNoConfigs; return; }
        mpLog.LogMessage("Parsing options");
        //TODO! Убрано при переходе на СН. Скорее всего потребуется вернуть позднее
        //mDifficulty = UniVarUtils.GetIntValue(mpOptions, "Game\\Skill", 0);
        //mShowLead = UniVarUtils.GetIntValue(mpOptions, "Game\\ShowLead", 1) != 0;
        //mShowThreat = UniVarUtils.GetIntValue(mpOptions, "Game\\ShowThreat", 1) != 0;
        //mUnlimitedArmor = UniVarUtils.GetIntValue(mpOptions, "Game\\UnlimitedArmor", 0) != 0;
        //mUnlimitedAmmo = UniVarUtils.GetIntValue(mpOptions, "Game\\UnlimitedAmmo", 0) != 0;
        //mNoDamObjects = UniVarUtils.GetIntValue(mpOptions, "Game\\ObjectCollision", 0) != 0;
        //mNoDamGround = UniVarUtils.GetIntValue(mpOptions, "Game\\GroundCollision", 0) != 0;
        //mAllowEnemyName = UniVarUtils.GetIntValue(mpOptions, "Game\\EC Allow Name", 1) != 0;
        //mAllowEnemyType = UniVarUtils.GetIntValue(mpOptions, "Game\\EC Allow Type", 1) != 0;
        //mAllowEnemyDist = UniVarUtils.GetIntValue(mpOptions, "Game\\EC Allow Dist", 1) != 0;
        //mShowFriendlyName = UniVarUtils.GetIntValue(mpOptions, "Game\\FC Show Name", 1) != 0;
        //mShowFriendlyType = UniVarUtils.GetIntValue(mpOptions, "Game\\FC Show Type", 0) != 0;
        //mShowFriendlyDist = UniVarUtils.GetIntValue(mpOptions, "Game\\FC Show Dist", 0) != 0;
        //mShowEnemyName = UniVarUtils.GetIntValue(mpOptions, "Game\\EC Show Name", 1) != 0;
        //mShowEnemyType = UniVarUtils.GetIntValue(mpOptions, "Game\\EC Show Type", 1) != 0;
        //mShowEnemyDist = UniVarUtils.GetIntValue(mpOptions, "Game\\EC Show Dist", 0) != 0;
        mpLog.Message("Options loaded.");
    }

    public string PrintOptions()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("mDifficulty: " + mDifficulty);
        sb.AppendLine("mShowLead: " + mShowLead);
        sb.AppendLine("mShowThreat: " + mShowThreat);
        sb.AppendLine("mUnlimitedArmor: " + mUnlimitedArmor);
        sb.AppendLine("mUnlimitedAmmo: " + mUnlimitedAmmo);
        sb.AppendLine("mNoDamObjects: " + mNoDamObjects);
        sb.AppendLine("mNoDamGround: " + mNoDamGround);
        sb.AppendLine("mAllowEnemyName: " + mAllowEnemyName);
        sb.AppendLine("mAllowEnemyType: " + mAllowEnemyType);
        sb.AppendLine("mAllowEnemyDist: " + mAllowEnemyDist);
        sb.AppendLine("mShowFriendlyName: " + mShowFriendlyName);
        sb.AppendLine("mShowFriendlyType: " + mShowFriendlyType);
        sb.AppendLine("mShowFriendlyDist: " + mShowFriendlyDist);
        sb.AppendLine("mShowEnemyName: " + mShowEnemyName);
        sb.AppendLine("mShowEnemyType: " + mShowEnemyType);
        sb.AppendLine("mShowEnemyDist : " + mShowEnemyDist);

        return sb.ToString();

    }

    public string PrintCampaign()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("mpCampaignName: " + mpCampaignName);
        sb.AppendLine("mpGameMapName: " + mpGameMapName);
        sb.AppendLine("mGameMapSizeX: " + mGameMapSizeX);
        sb.AppendLine("mGameMapSizeZ: " + mGameMapSizeZ);
        sb.Append("mpTerrainName: " + mpTerrainName);
        return sb.ToString();
    }
    public void OpenCampaign(string pCmpName, DWORD CmpName)
    {
        // готовим ошибку :)
        mpError = gspErrLoading;
        // грузим базу
        mpLog.Message(String.Format("Opening DB \"{0}\"...", pCmpName));
        //iUnifiedVariableDB pDb = UniVarUtils.CreateUnifiedVariableDB(Constants.UNIVARS_VERSION, pCmpName + ".cmp", true);
        //iUnifiedVariableDB pDb = UniVarUtils.CreateUnifiedVariableDB(Constants.UNIVARS_VERSION, _PI.mHddPath+ "/" + pCmpName + ".cmp", true,false); //TODO надо правильно раскукоживать кампанию
        iUnifiedVariableDB pDb = UniVarUtils.CreateUnifiedVariableDB(Constants.UNIVARS_VERSION, pCmpName, true, false); //TODO надо правильно раскукоживать кампанию
        if (pDb == null) return;
        // грузим кампанию
        mpLog.Message("Loading campaign...");

        //TODO! Найти и исправить iUnifiedVariableContainer на корректное!
        mpCampaign = (UniVarContainer)pDb.GetRootTpl<iUnifiedVariableContainer>();
        pDb.Release();
        if (mpCampaign == null) return;
        // грузим дефолтную миссию
        mpLog.Message("Loading default mission...");
        mpDefaultMission = (iUnifiedVariableContainer)mpCampaign.GetVariableTpl<iUnifiedVariableContainer>("Default Mission");
        if (mpDefaultMission == null) return;
        // читаем из базы
        iUnifiedVariableString pString;
        iUnifiedVariableInt pInt;
        // mCmpVersion
        mCmpVersion = (uint)UniVarUtils.GetIntValue((UniVarContainer)mpCampaign, "Version", 0);
        // mpCampaignName
        mpCampaignName = pCmpName;
        // mpGameMapName
        pString = mpCampaign.GetVariableTpl<iUnifiedVariableString>("GameMapName");
        if (pString == null) return;
        //char buffer[256];
        //pString.StrCpy(buffer);
        mpGameMapName = pString.GetValue(); ;
        // mGameMapSizeX
        pInt = mpCampaign.GetVariableTpl<iUnifiedVariableInt>("GameMapSizeX");
        if (pInt == null) return;
        mGameMapSizeX = pInt.GetValue();
        // mGameMapSizeZ
        pInt = mpCampaign.GetVariableTpl<iUnifiedVariableInt>("GameMapSizeZ");
        if (pInt == null) return;
        mGameMapSizeZ = pInt.GetValue();
        // mpTerrainName
        pString = mpCampaign.GetVariableTpl<iUnifiedVariableString>("TerrainName");
        if (pString == null) return;
        mpTerrainName = pString.GetValue();

        // грузим землю
        mpLog.Message(String.Format("Loading terrain \"{0}\"...", mpTerrainName));
        mpTerrain = new TERRAIN_DATA();
        if (!mpTerrain.Open(mpTerrainName)) return;
        if (!mpTerrain.OpenSq(true, false)) return;
        if (!mpTerrain.OpenBx(true, false)) return;
        if (!mpTerrain.OpenVb(true, false)) return;
        //if (!pTerrain.OpenLight(true,false,0,0)) return;
        // создаем хэш
        mpLog.Message("Creating hash system...");
        mpHash = IHashApi.CreateHasher2(IHashApi.HASH_VERSION, mpTerrain, 400000f, 512f, 1024 * 300, mpLog, false);
        //if (mpHash != 0) mpHash.SetSecondCache(ROObjectId(ROFID_LIGHT));
        // создаем столкновения
        mpLog.Message("Creating collision system...");
        //TODO - загружать систему коллизий
        mpCollision = CollisionModuleAPI.CreateCl(CollisionDefines.COLL_VERSION, mpHash, mpTerrain);
        // грузим объекты
        mpLog.Message("Opening objects DB...");

        mpFpoLoader = (IFpoLoader)FpoLoader.CreateFpoLoader(mpRenderer, mpCollision, "objects2.dat", "objects.dat");
        if (mpFpoLoader == null) return;
        mpLog.Message("Opening game DB...");
        mpGameDatas = IMappedDb.CreateMappedDb(IMappedDb.DBFORMAT_NAKED);
        //if (mpGameDatas.Open("gdata.dat") != DBDef.DB_OK) return;
        if (mpGameDatas.Open(ProductDefs.GetPI().getHddFile("gdata.dat")) != DBDef.DB_OK) return;
        // создаем систему дорог
        mpLog.Message("Loading road system...");
        //mpDataHasher = EnvironmentApi.CreateDH(20480 * 2, null,mpLog);
        mpDataHasher = EnvironmentApi.CreateDH(20480 * 2, new myRoadStore(), mpLog);
        //tuvb pRoads = mpCampaign.openBlock("RoadNet");
        //mpDataHasher.HashRoads(createObject<UnivarsDataBlock>(pRoads));

        System.IO.Stream pRoads = mpCampaign.openStream("RoadNet");
        mpDataHasher.HashRoads(pRoads);
        //mpNavigation = CreateNS(mpDataHasher, mpTerrain, mpLog);
        //mpNavigation.Initialize(0.2f, 4096);
        // все в относительном порядке
        mpLog.Message("Campaign loaded.");
        mpError = String.Empty;
        mCmpName = CmpName;
    }
    protected void CloseCampaign()
    {

        // удаляем систему дорог
        if (mpNavigation != null)
        {
            mpLog.Message("Releasing road system...");
            mpNavigation.Release();
            mpNavigation = null;
        }
        if (mpDataHasher != null)
        {
            mpDataHasher.Release();
            mpDataHasher = null;
        }
        // удаляем объекты
        if (mpGameDatas != null)
        {
            mpLog.Message("Releasing game DB...");
            mpGameDatas.Release();
            mpGameDatas = null;
        }
        if (mpFpoLoader != null)
        {
            mpLog.Message("Releasing objects DB...");
            mpFpoLoader.Release();
            mpFpoLoader = null;
        }
        // удаляем столкновения
        if (mpCollision != null)
        {
            mpLog.Message("Releasing collision system...");
            mpCollision.Release();
            mpCollision = null;
        }
        // удаляем хэш
        if (mpHash != null)
        {
            mpLog.Message("Releasing hash system...");
            mpHash.Release();
            mpHash = null;
        }
        // выгружаем землю
        if (mpTerrain != null)
        {
            mpLog.Message("Releasing terrain...");
            mpTerrain.Close();
            //delete mpTerrain;
            mpTerrain = null;
        }
        // чистим данные
        mpLog.Message("Releasing data...");
        mpTerrainName = String.Empty;
        mpGameMapName = String.Empty;
        mpCampaignName = String.Empty;
        mpDefaultMission = null;
        mpCampaign = null;
        mCmpName = CRC32.CRC_NULL;
        mpLog.Message("Campaign closed.");

    }
    protected void OpenMission(string pMsnName, DWORD MsnName)
    {
        // готовим ошибку :)
        mpError = gspErrLoading;
        mpMission = null;
        // грузим миссию
        mpLog.Message(String.Format("Loading missions \"{0}\"...", pMsnName));
        iUnifiedVariableContainer pMsns = mpCampaign.GetVariableTpl<iUnifiedVariableContainer>("Missions");
        if (pMsns == null) return;
        iUnifiedVariableContainer pMsn = pMsns.GetVariableTpl<iUnifiedVariableContainer>(pMsnName);
        if (pMsn == null) return;
        // читаем из базы
        iUnifiedVariableString pString;
        //iUnifiedVariableInt pInt;
        // mpMissionName
        mpMissionName = pMsnName;
        // mpVisConfigName
        pString = pMsn.GetVariableTpl<iUnifiedVariableString>("VisConfigName");
        //char buffer[256];
        //pString.StrCpy(buffer);
        mpVisConfigName = pString.GetValue();
        // все в порядке
        mpError = String.Empty;
        mMsnName = MsnName;
        mpMission = (UniVarContainer)pMsn;
        mpBriefingText = mpMission.GetVariableTpl<iUnifiedVariableString>("BriefingText");
        mpBriefingCommands = mpMission.GetVariableTpl<iUnifiedVariableString>("BriefingCommands");
        mpMissionDescription = mpMission.GetVariableTpl<iUnifiedVariableString>("Description");
        mpDebriefingOnSuccessText = mpMission.GetVariableTpl<iUnifiedVariableString>("DebrSuccessText");
        mpDebriefingOnSuccessCommands = mpMission.GetVariableTpl<iUnifiedVariableString>("DebrSuccessCommands");
        mpSuccessorsOnSuccess = mpMission.GetVariableTpl<iUnifiedVariableArray>("SuccessorsOnSuccess");
        mpDebriefingOnFailureText = mpMission.GetVariableTpl<iUnifiedVariableString>("DebrFailureText");
        mpDebriefingOnFailureCommands = mpMission.GetVariableTpl<iUnifiedVariableString>("DebrFailureCommands");
        mpSuccessorsOnFailure = mpMission.GetVariableTpl<iUnifiedVariableArray>("SuccessorsOnFailure");
        mpLog.Message("Mission loaded.");

    }
    protected void CloseMission() { }

    // own - network
    //protected:
    //  TRef<iMadSockets2Manager> mpSockManager;
    //iMadDedicatedSocket2* mpSocket;
    //iMadListenSocket2* mpListenSocket;
    //TPLIST<iMadDedicatedSocket2> mSocketsList;
    //ITranslator* mpTranslator;
    //iGameSpyReporter* mpGameSpyReporter;
    //QueryReportInfo <interface IMissionAi> *mpGameReportInfo;

    //protected:
    //  void OnMsnDataPacket(const MsnDataPacket* pPkt);
    //void OnConnect(iMadDedicatedSocket2* pSck);
    //void OnEnterGamePacket(iMadDedicatedSocket2* pSck,const GameHolderPacket* pPkt);
    //void UpdateSockets();
};

public class Selection
{
    public uint myCraft;
    public uint[] mySlots = new uint[3];
};

public enum LoadResult
{
    LR_SUCCESS = 0,
    LR_NO_TERRAIN = 1,
    LR_NO_OBJECTS = 2,
    LR_GAME_NOT_STARTED = 3,
    LR_DATA_NOT_LOADED = 4
};

public interface ILocationHolder : IObject
{

    public LoadResult loadLocation(ILocation loc);

    public TERRAIN_DATA getTerrain();
    public INavigation getNavigation();
    public IDataHasher getDataHasher();
    public IMappedDb getGameDatas();
    public IFpoLoader getFpoLoader();
    public ICollision getCollision();
    public IHash getHash();

    public ILocation getCurrentLocation();
};

public static class UniVarUtils
{
    public static int GetIntValue(UniVarContainer vcont, string name, int def = 0)
    {
        iUnifiedVariableInt vint = vcont.openInt(name);
        return vint != null ? vint.GetValue() : def;
    }

    public static iUnifiedVariableDB CreateUnifiedVariableDB(uint version, string filename, bool read_only = false, bool expand_filename = false)
    {
        if (expand_filename) filename = ProductDefs.GetPI().getHddFile(filename);
        if (version != Constants.UNIVARS_VERSION) return null;
        iUniVarMemManager pMemMgr;
        if (filename != null)
        {
            Debug.Log("Loading UVB database: " + filename);
            if (read_only == false)
                pMemMgr = new UniVarMemMemManager(filename);
            else
                pMemMgr = new UniVarMemROManager(filename);
        }
        else
        {
            pMemMgr = new UniVarMemMemManager();
        }
        //pMemMgr = new UniVarMemROManager(filename);
        if (pMemMgr.IsReady() == false)
        {
            Debug.Log("IsNotReady! " + pMemMgr);
            pMemMgr = null;
            return null;
        }
        return new UniVarDB(filename, pMemMgr);
    }

    internal static uint GetStringValueCrc(iUnifiedVariableContainer vcont, string name, uint def = CRC32.CRC_NULL)
    {
        iUnifiedVariableString vstr = vcont.openString(name);
        return vstr != null ? Hasher.HshString(vstr.GetValue()) : def;
    }
}
