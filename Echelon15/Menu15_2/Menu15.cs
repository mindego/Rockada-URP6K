
using Crafts = System.Collections.Generic.Dictionary<int, Craft>;
using Weapons = System.Collections.Generic.Dictionary<int, Weapon>;
using AllowedWeapons = System.Collections.Generic.List<int>;
public static class Menu15
{
    //struct Error;
    //struct UnableToLoadMainError;
    static bool Game_inited = false;
    static bool MDlg_inited = false;
    static bool Preview_inited = false;

    public static void init()
    {
        Game_inited = false;
        MDlg_inited = false;
        Preview_inited = false;

        //if (!MDlg_inited)
        //{
        //    initializeMDlg(); //Py_Initialize();
        //    MDlg_inited = true;
        //}

        //TODO Вот тут должна быть инициализация переменных
        Game_inited = true;

        if (!Preview_inited)
        {
            Preview.initialize();
            Preview_inited = true;
        }

        //if (!MainModule)
        //{
        //    MainModule = PyImport_ImportModule("Main");
        //    if (PyErr_Occurred())
        //    {
        //        MainModule = 0;
        //        PyErr_Print();
        //        throw Menu15::UnableToLoadMainError();
        //    }
        //}
    }
    public static void reset(ICtrlDialog CD, RendererApi rnd)
    {
        UnityEngine.Debug.Log("createMenu");
    }
    public static void done() { }
    public static void update(float delta) { }

    public static void showExitScreen(bool restart_enabled) { }

    public static void showGameResults() { }
    public static void showEngbay(Crafts crafts, Weapons weapons, Selection default_selection) { }
    public static void hideEngbay() { }

    public struct ServerInfo
    {
        string name;
        string type;
        int ping;
        int num_players;
        int num_players_max;

        string campaign;
        string mission;
        string mission_type;

        string addr;
        int enabled;
    };

    public static void addServer(ServerInfo si) { }

    public static void onConnect(string gameset, string mission) { }
    public static void onError(string error) { }
}

public class Weapon
{
    string myName;
    string myDesc;

    public Weapon(string name = "BG", string desc = "Big Gun")
    {
        myName = name;
        myDesc = desc;
    }

    public string getName()
    {
        return myName;
    }

    public string getDescription()
    {
        return myDesc;
    }
};

public class Craft
{
    string myName;
    AllowedWeapons[] mySlots = new AllowedWeapons[3];

    public Craft(string name = "IL2")
    {
        myName = name;
    }

    string getName()
    {
        return myName;
    }

    AllowedWeapons getAllowedWeapons(int slot_id)
    {
        return mySlots[slot_id];
    }
};
