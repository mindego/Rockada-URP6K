
/// <summary>
/// StormGameData - блок данных для фунционировнаия игры
/// </summary>
public class StormGameData : GameOptions
{
    // минимальные данные
    public string mpCampaignName;
    public string mpGameMapName;
    public int mGameMapSizeX;
    public int mGameMapSizeZ;
    public string mpTerrainName;
    public string mpMissionName;
    public string mpVisConfigName;
    public string myProfileName;
    // дополнительные данные
    public iUnifiedVariableDB mpLocalizationDb;

    public iUnifiedVariableContainer mpDefaultMission;
    public iUnifiedVariableContainer mpCampaign;

    public UniVarContainer mpMission;
    public UniVarContainer mpNonLocalizedMission;
    public UniVarContainer mpCurrentEventData;

    //public UniVarContainer mpOptions;
    public iUnifiedVariableContainer mpOptions;
    public iUnifiedVariableContainer mpControls;

    //TRef<IGameset> myGameset;
    public IGameset myGameset;
    // переменные обмена
    public bool mShouldDraw;
    public bool mUseControls;
    // модули, общие в игре
    public ILog mpLog;
    public CommandsApi mpCommands;
    public RendererApi mpRenderer;
    public ISound mpSound;
    public EInput mpInput;
    public ITranslator2 myTranslator;
    // модули, общие дла кампании
    public TERRAIN_DATA mpTerrain;
    public IHash mpHash;
    public ICollision mpCollision;
    public IFpoLoader mpFpoLoader;
    public IMappedDb mpGameDatas;
    public IDataHasher mpDataHasher;
    public INavigation mpNavigation;
    public IMenuFeedback myFeedback;
    public string myPlayerName;
    // создание/удаление

    //StormGameData() :mpCampaignName(0),mpGameMapName(0),mpTerrainName(0),mpMissionName(0),mpVisConfigName(0),
    //    mpLog(0),mpCommands(0),mpRenderer(0),mpSound(0),mpInput(0),myTranslator(0),
    //    mpTerrain(0),mpHash(0),mpCollision(0),mpFpoLoader(0),mpGameDatas(0),mpDataHasher(0),mpNavigation(0),myFeedback(0) { }
};
