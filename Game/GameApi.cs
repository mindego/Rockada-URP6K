using UnityEngine;


public class GameApi
{
    public const uint GAME_API_VERSION = 0xC3F5C095; // GameApi 0.1  
    private const uint gAppId = 0xBAADBAAD; // TODO генерировать 

    //public static ILocationHolder createLocationHolder(RendererApi rend, uint version = GAME_API_VERSION)
    //{
    //    //LogMessage("Game protool %p", gAppId);
    //    Debug.Log(string.Format("Game protool {0}", gAppId));
    //    if (version != GAME_API_VERSION) return null;
    //    return new LocationHolder(rend);
    //}

    //public static IGameHolder createGameHolder2(GameCreateParams gcp, uint version = GAME_API_VERSION)
    //{
    //    if (version != GAME_API_VERSION) return null;
    //    //return new GameHolder2<BaseScene, HostScene, RemoteScene>(gcp);
    //    return new GameHolder2(gcp);
    //}

    //public static uint getProtocolVersion()
    //{
    //    return gAppId;
    //}

}

//#ifdef STORM_GAME_DLL
//#define GAME_API __declspec(dllexport)
//#else
//#define GAME_API __declspec(dllimport)
//#endif

public struct GameCreateParams
{
    public ILog pLog;
    //iMadSockets2Manager* pSockets;
    public EInput pInput;
    public CommandsApi pCommands;
    public RendererApi pRenderer;
    public ISound pSound;
    public ITranslator2 pTranslator;
    public IMenuFeedback myFeedback;
    public int myPort;
    public bool myDedicated;
    //IGSNameHolder* myGSNameHolder;
    //IpAddress* myIpAddress;
}

