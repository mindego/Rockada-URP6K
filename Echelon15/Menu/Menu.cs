using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using crc32 = System.UInt32;

//public class Menu IGame,  CommLink, IMenuFeedback, IJoys, IConnectionClient, IAddServer
public class Menu : IGamePy, CommLink, IMenuFeedback, IJoys
{
    public const string TexturesPath = "Graphics\\textures.dat";
    public const string sIntroAvi = "Movies\\intro.avi";

    public const string sQuit = "quit";
    const crc32 iQuit = 0xBC55286A;

    public const string sCrash = "crash";
    const crc32 iCrash = 0x28170F20;

    public const string sSay = "say";
    const crc32 iSay = 0x0336639A;

    public const string sSayTeam = "say_team";
    const crc32 iSayTeam = 0xE6098CA4;

    public const string sConnect = "connect";
    const crc32 iConnect = 0x8B3006E0;

    public const string sPlayerName = "player_name";
    const crc32 iPlayerName = 0x03C6D1CA;

    const int port_Default = 28015;

    public static void playAvi(IAviPlayer pl, string name)
    {
        string file = ProductDefs.GetPI().getHddFile(name);
        if (!File.Exists(file)) file = null; //В исходном коде поиск ещё производится на компакт-диске, но где теперь их найти?

        if (file != null)
            pl.PlayAvi(file);
    }

    public static int ShowMenu(IAviPlayer pl)
    {
        int ret = 1;
        //playAvi(pl, sIntroAvi);
        LogFactory.GetLog();
        if (Menu15W.init())
        {
            Menu m = new Menu();

            if (m.initialize())
            {
                while (m.update())
                    PoolMessages(false);
            }
            else
                ret = 0;

            Menu15W.done();
        }
        else
            LogFactory.GetLog().CriticalMessage("ERROR: Menu15W::init() failed to initialize !");
        return ret;
    }

    static bool PoolMessages(bool Translate = true)
    {
        //TODO Возможно, стоит перенести в статический класс winmsg
        //MSG msg; msg.message = 0;
        //while (PeekMessage(&msg, 0, 0, 0, PM_REMOVE))
        //{
        //    if (Translate)
        //        TranslateMessage(&msg);
        //    DispatchMessage(&msg);
        //}
        //return msg.message != WM_QUIT;
        return true;
    }

    public bool initialize()
    {

        bool ok = myEnvironment.open(this) && Menu15W.reset(myEnvironment.myDialog, this, myEnvironment.myRenderer) && openGameEngine();
        if (ok)
        {
            myEnvironment.enableMenu(true);
            myEnvironment.myCommands.RegisterCommand(sConnect, this, 1);
            myEnvironment.myCommands.RegisterCommand(sQuit, this, 0);
            myEnvironment.myCommands.RegisterCommand(sCrash, this, 0);
            myEnvironment.myCommands.RegisterCommand(sSay, this, 0);
            myEnvironment.myCommands.RegisterCommand(sSayTeam, this, 0);
            myEnvironment.myCommands.RegisterCommand(sPlayerName, this, 1);
            myEnvironment.startMusic();
            //myGSNames.initialize(
            //    GetPI()->getGS().getGameName(),
            //    GetPI()->getGS().getGameName(),
            //    GetPI()->getGS().getSecretKey());
            //parseCommandLine(GetCommandLine());
        }
        return ok;
    }
    public bool update() {
        //double time;
        var time = UnityEngine.Time.deltaTime;
        //if (myConnection && !myConnection->update())
        //    myConnection = 0;

        bool ret =
            myEnvironment.update(ref time) &&
            myGameHolder.update(time, myEnvironment.isMenuEnabled() == false);

        //if (myRefresher && !myRefresher->update(time))
        //    myRefresher = 0;


//# ifdef _PYTHON_MENU_
//        if (myEnvironment.isMenuEnabled())
//            Menu15W::update(time);
//#endif

        myFramesToSkip--;

        if (ret)
            draw();
        return ret;
    }
    void draw() {
        float[] viewport = { 0, 0, 1, 1 };
        if (myEnvironment.beginDraw())
        {
            bool draw_game = myGameHolder.isStarted() && myFramesToSkip <= 0;
            if (!draw_game && (myEnvironment.isMenuEnabled() || myEnvironment.myVideo!=null))
                myEnvironment.myRenderer.ClearScreen(new Vector4(0, 0, 0, 0));
            if (draw_game)
                myGameHolder.draw(viewport);
            myEnvironment.endDraw();
        }
    }

    public  virtual void openGame(string gameset_name, string mission_name, string profile, bool host)
    {
        LoadResult ok = LoadResult.LR_DATA_NOT_LOADED;
        IGameset StormEvent = DataCoreFactory.CreateGameSet(gameset_name, mission_name, null);
        if (StormEvent != null)
        {
            //ok = myLocation.loadLocation(TRef<ILocation>(StormEvent.getLocation(0)) );
            Debug.Log("Map name: " + StormEvent.getLocation(0).getGameMapName());
            ok = myLocation.loadLocation(StormEvent.getLocation(0));
            if (ok == LoadResult.LR_SUCCESS)
            {
                string buffer = string.Format("Profiles\\{0}", profile);
                myGameHolder.open(StormEvent, myLocation, buffer, host);
                //Task openTask = new Task(()=>myGameHolder.open(StormEvent, myLocation, buffer, host));
                //await Task.Run(myGameHolder.open(StormEvent, myLocation, buffer, host));
                //if (myConnection!=null)
                //    myConnection.notifyOpenGame(myGameHolder.getMissionId());
            }
        }
        if (ok != LoadResult.LR_SUCCESS)
            myEnvironment.myLog.Message("ERROR: {0}", getErrorDescription(ok));
    }

    static string getErrorDescription(LoadResult res)
    {
        switch (res)
        {
            case LoadResult.LR_NO_TERRAIN: return "Can't open terrain.";
            case LoadResult.LR_NO_OBJECTS: return "Can't open objects.";
            case LoadResult.LR_GAME_NOT_STARTED: return "Game not started.";
            case LoadResult.LR_DATA_NOT_LOADED: return "Data not loaded.";
            default: return "Unknown error";
        }
    }

    public virtual bool startGame()
    {
        //bool ret = myGameHolder.start(myConnection ? myConnection->interceptSocket() : 0, myConnection ? myConnection->getServerFlags() : 0);
        bool ret = myGameHolder.start(null, null);
        if (!ret)
        {
            //myEnvironment.myConsole.setMaxHeight(1);
            //myEnvironment.myConsole.show(true);
        }
        else { }
        //myEnvironment.myChat.setLog(myGameHolder->getChatLog());
        return ret;
    }
    public virtual void closeGame()
    {
        myEnvironment.enableExitScreen(false);
        //myEnvironment.myChat.setLog(0);
        myGameHolder.close();
        //if (myConnection!=null)
        //    myConnection.shutdown();
        myEnvironment.enableMenu(true);
    }
    public virtual void resumeGame()
    {
        myEnvironment.stopMusic();
        myGameHolder.pause(false);
        myEnvironment.enableExitScreen(false);
        myEnvironment.enableMenu(false);
    }
    public virtual void quit()
    {
        //::SendMessage(myEnvironment.myWindow.getWindow(), WM_CLOSE, 0, 0);
    }

    public virtual bool showEngBay(EngBayParams gcp)
    {
        if (myPlayerNameFromCommandLine != null)
        {
            string cmd = string.Format("cl_name [{0}];", myPlayerNameFromCommandLine);
            myEnvironment.myCommands.ProcessString(cmd);
        }

        myEnvironment.enableMenu(true);
        Menu15W.showEngbay(gcp);
        return true;
    }
    public virtual void onServerShutdown()
    {
        //if (myConnection)
        //    myConnection->notifyServerShutdown();
    }
    public virtual void onStartDraw()
    {
        myEnvironment.stopMusic();
        myEnvironment.enableMenu(false);
        skipFrames(5);
    }

    public virtual void applyCraftSelection(ref Selection sel)
    {
        myGameHolder.applyCraftSelection(ref sel);
    }

    public virtual ITranslator getTranslator() { return myEnvironment.myTranslator; }
    public virtual void setGamma(float gamma)
    {
        myEnvironment.myRenderer.SetGamma(gamma);
    }

    public virtual int getJoysticksCount() { return myJoysticks.Count; }
    public virtual int getRuddersCount() { return myRudders.Count; }
    public virtual string getJoystick(int n) { return myJoysticks[n]; }
    public virtual string getRudder(int n) { return myRudders[n]; }
    public virtual void playVideo(string name)
    {
        if (name != null)
            myEnvironment.playVideo(name);
    }
    //public virtual void connect(IpAddress addr) { }

    //public virtual void onConnect(string gameset, string msn_name);
    public virtual void onError(string name)
    {
        closeGame();
        Menu15W.onError(name);
    }
    //public virtual void addServer(GSServerInfo info);
    //public virtual void onDisconnect();
    //public virtual bool isNetworkSupported() { return myEnvironment.mySockets; }
    public virtual bool isNetworkSupported() { return false; } //Сеть НЕ поддерживается

    //public virtual void refreshLAN();
    //public virtual void refreshGameSpy();

    public virtual void playSound(string sound)
    {
        if (sound != null)
            myEnvironment.mySounds.playSound(sound);
    }
    public virtual void setVolume(int vol) { myEnvironment.mySounds.setVolume(vol); }
    public virtual void setMusicVolume(int vol) { myEnvironment.myMusic.setVolume(vol); }
    public virtual void setMasterVolume(int vol) { myEnvironment.mySound.SetMasterVolume(vol); }
    public virtual int getProductState()
    {
        return (int)ProductDefs.GetPI().GetProductState();
    }

    private IMenuFeedback getIMenuFeedback() { return this; }
    //private IConnectionClient getIConnectionClient() { return this; }
    //private IAddServer getIAddServer() { return this; }

    // MenuEnv commands
    public virtual void OnCommand(int cls_id, string a, string b) { } //TODO Реализовать обработчик команд в меню

    public bool openGameEngine()
    {
        Debug.Log("Opening Game Engine");
        GameCreateParams par = new();
        getParams(ref par, myEnvironment);
        par.myFeedback = getIMenuFeedback();
        //par.myGSNameHolder = &myGSNames;
        myLocation = GameHolder2Api.createLocationHolder(par.pRenderer);
        myGameHolder = GameHolder2Api.createGameHolder2(par);
        return myGameHolder != null && myLocation != null;
    }

    void getParams(ref GameCreateParams p, MenuEnvironment e)
    {
        p.pLog = e.myLog;
        //p.pSockets = e.mySockets;
        p.pInput = e.myInput;
        p.pCommands = e.myCommands;
        p.pRenderer = e.myRenderer;
        p.pSound = e.mySound;
        p.pTranslator = e.myTranslator;
        p.myPort = port_Default;
        p.myDedicated = false;
        //p.myIpAddress = 0;
    }
    //public void parseCommandLine(string cmd);

    //public Echelon15GSNameHolder myGSNames;
    public MenuEnvironment myEnvironment;

    ILocationHolder myLocation;
    public IGameHolder myGameHolder;


    //IClientConnection myConnection;
    //IServerRefresher myRefresher;

    bool myGameEnabled = false;
    int myFramesToSkip = 0;

    string myPlayerNameFromCommandLine;

    private void skipFrames(int frames_to_skip) { myFramesToSkip = frames_to_skip; }

    List<string> myJoysticks;
    List<string> myRudders;

    public Menu()
    {
        myEnvironment = new MenuEnvironment();
    }

    public virtual void addDevice(string name, bool joy, bool rud)
    {
        if (joy)
            myJoysticks.Add(name);
        if (rud)
            myRudders.Add(name);
    }

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
        throw new NotImplementedException();
    }

    public PatrolInfo GetPatrolInfo(ObjId id)
    {
        throw new NotImplementedException();
    }

    public float GetGroundLevel(Vector3 org)
    {
        throw new NotImplementedException();
    }

    public float GetGroundLevelMedian(Vector3 org, float r)
    {
        throw new NotImplementedException();
    }

    public bool RadioChannelIsFree()
    {
        throw new NotImplementedException();
    }

    public int getPhrasesCount(uint code)
    {
        throw new NotImplementedException();
    }

    public GameOptions getGameOptions()
    {
        throw new NotImplementedException();
    }

    public void AddRef()
    {
        throw new NotImplementedException();
    }

    public int RefCount()
    {
        throw new NotImplementedException();
    }

    public int Release()
    {
        myEnvironment.myCommands.UnRegister(this);
        myGameHolder = null;
        myLocation = null;
        return 0;
    }
}

public interface IAviPlayer
{
    public int PlayAvi(string name);
};

public class myAviPlayer : IAviPlayer
{
    private UnityEngine.Video.VideoPlayer videoPlayer;

    private void createPlayerGOBJ()
    {
        if (videoPlayer != null) GameObject.Destroy(videoPlayer.gameObject);

        GameObject videoplayerGOBJ = new GameObject("Video Player");
        videoPlayer = videoplayerGOBJ.AddComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;
        videoPlayer.targetCamera = Engine.UnityCamera;
    }

    public int PlayAvi(string name)
    {
        createPlayerGOBJ();
        videoPlayer.url = name;
        videoPlayer.Play();

        return 0;
    }
}


public static class menu_dll
{
    public const string TexturesPath = "Graphics\\textures.dat";
}
