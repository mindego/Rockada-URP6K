using UnityEngine;
using cstr = System.String;
public static class GameCore
{
    //Из PyMenu.cpp
    public static IGamePy Game = null;
    static ILog Log = null;
    public static cstr getPath()
    {
        return ProductDefs.getRootFile();
    }

    public static cstr getLastProfile()
    {
        string profileGetter;
        //RegKey key(HKEY_CURRENT_USER, GetPI()->RegKeyConfig(), false);
        //if (key())
        //{
        //    DWORD size = sizeof(profileGetter);
        //    key.ReadString(LastProfileKey, profileGetter, size);
        //}

        //TODO Реализовать корректное получение профиля из реестра (или конфига);
        profileGetter = "Default";
        return profileGetter;
    }
    public static void setLastProfile(cstr name)
    {
        //    RegKey key(HKEY_CURRENT_USER, GetPI()->RegKeyConfig(), true);
        //    key() && key.WriteString(LastProfileKey, name);
        //TODO Реализовать корректное сохранение профиля в реестр или конфиг;
    }

    public static void openGame(cstr gameset, cstr mis, cstr profile, bool host)
    {
        try
        {
            Game.openGame(gameset, mis, profile, host);
        }
        catch
        {
            //printf("Call to GameCore::openGame raise an exception\n");
            Debug.Log("Call to GameCore::openGame raise an exception\n");
        }
    }
    public static void setGamma(float gamma)
    {
        Game.setGamma(gamma);
    }
    public static bool startGame()
    {
        try
        {
            return Game.startGame();
        }
        catch
        {
            Debug.Log("Call to GameCore::start raise an exception\n");
        }
        return false;
    }
    public static void closeGame()
    {
        Game.closeGame();
    }
    public static void resumeGame()
    {
        Game.resumeGame();
    }
    public static void applyCraftSelection(Selection sel)
    {
        Game.applyCraftSelection(ref sel);
    }
    public static void quit()
    {
        Game.quit();
    }
    public static void playVideo(cstr name)
    {
        Game.playVideo(name);
    }
    //public static ITranslator getTranslator()
    //{
    //    return Game.getTranslator();
    //}
    public static int getJoysticksCount()
    {
        return Game.getJoysticksCount();
    }
    public static int getRuddersCount()
    {
        return Game.getRuddersCount();
    }
    public static cstr getJoystick(int n)
    {
        return Game.getJoystick(n);
    }
    public static cstr getRudder(int n)
    {
        return Game.getRudder(n);
    }
    public static void startRefreshLAN()
    {
        //Game.refreshLAN();
    }
    public static void startRefreshGameSpy()
    {
        //Game.refreshGameSpy();
    }
    //public static void connect(IpAddress addr)
    //{
    //    Game.connect(addr);
    //}

    public static bool isNetworkSupported()
    {
        return false;
        //return Game.isNetworkSupported();
    }
    public static void playSound(cstr sound)
    {
        Game.playSound(sound);
    }
    public static void setMenuEffectsVolume(int vol) { Game.setVolume(vol); }
    public static void setMenuMusicVolume(int vol) { Game.setMusicVolume(vol); }
    public static void setGeneralVolume(int vol) { Game.setMasterVolume(vol); }
    public static int getProductState()
    {
        return Game.getProductState();
    }
}