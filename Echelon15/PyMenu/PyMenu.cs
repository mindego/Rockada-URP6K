public interface IGamePy
{
    public void openGame(string gs, string msn, string profile, bool host);
    public bool startGame();
    public void closeGame();
    public void resumeGame();
    public void setGamma(float gamma);
    public void quit();
    public void applyCraftSelection(ref Selection sel);
    //public ITranslator getTranslator();
    public int getJoysticksCount();
    public int getRuddersCount();
    public string getJoystick(int n);
    public string getRudder(int n);
    public void playVideo(string name);
    //public void connect(IpAddress addr);
    public bool isNetworkSupported();
    //public void refreshLAN(); //сеть НЕ поддерживается
    //public void refreshGameSpy();
    public void playSound(string sound);
    public void setVolume(int vol);
    public void setMusicVolume(int vol);
    public void setMasterVolume(int vol);
    public int getProductState();
};

public static class Menu15W
{
    public const int GPY_Version = 13;
    public static IGamePy Game = null;
    public static ILog Log = null;
    public static bool init(int Version = GPY_Version)
    {
        try {
            return Version == GPY_Version && Menu15_init_to_bool();
        } catch
        {

        }
        
        return false;

    }
    public static bool reset(ICtrlDialog CD, IGamePy gm, RendererApi rnd) {
        try {
            Game = gm;
            Menu15.reset(CD, rnd);
            return true;
        } catch { return false; }
        //__except(pyIntExceptHandler(GetExceptionInformation(), "Menu15W::reset")) { }
        //return false;
    }
    public static void done() { return; }
    public static void update(float delta) { return; }
    public static void showExitScreen(bool restart_enabled) { return; }
    public static void showEngbay(EngBayParams gcp) { return; }
    public static void showGameResults() { return; }
    public static void onConnect(string gm, string msn) { return; }
    public static void onError(string error) { return; }
    //void addServer(const GSServerInfo& info);

    private static bool Menu15_init_to_bool()
    {
        try
        {
            Menu15.init();
            return true;
        }
        //catch (Menu15.Error e)
        catch (System.Exception e)
        {
            //LogFactory.GetLog().CriticalMessage(" Menu15W::init ... throws following: {0}\n", e.what());
            LogFactory.GetLog().CriticalMessage(" Menu15W::init ... throws following: {0}\n", e.ToString());
        }
        return false;
    }
};

public interface ICtrlDialog
{
    void EnableMouse(bool on);
}
