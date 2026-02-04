using DWORD = System.UInt32;
using crc32 = System.UInt32;
using WORD = System.UInt16;
using static InputClientDefs;
using System;
using UnityEngine;
using static ITranslator2;
using static DIKtoUnityMapping;
using static MMOD;
using static KeyStates;
using System.Collections.Generic;

public class Translator : ITranslator2, CommLink
{
    const string sPlusRus = "rus";
    const uint iPlusRus = 0xCC8F37E6;

    const string sSetKey = "setkey";
    const uint iSetKey = 0x03D59C7E;

    const string sClearKey = "clearkey";
    const uint iClearKey = 0xC0237BE7;

    CommandsApi cmd;
    ILog Log;

    bool RShift;
    bool LShift;
    bool RAlt;
    bool LAlt;
    bool RCtrl;
    bool LCtrl;
    bool Rus;
    bool CanRusSwitch;
    bool ShiftEnabled;
    bool CtrlEnabled;
    bool AltEnabled;
    bool Exec;
    int myLastModifier;

    //KeyBind[] KeyBinds = new KeyBind[DIK_LAST];
    Dictionary<int, KeyBind> KeyBinds = new Dictionary<int, KeyBind>(); //ключ - код клавиши, значение - бинд

    byte[] kbd = new byte[256];

    bool setModifier(ref bool var, bool value, int mod)
    {
        var = value;
        myLastModifier = value ? mod : (int)MMOD_NONE;
        return true;
    }



    public Translator(ILog _l, CommandsApi _cmd)
    {
        Log = _l;
        cmd = _cmd;
        myLastModifier = (int)MMOD_NONE;

        RShift = false;
        LShift = false;
        RAlt = false;
        LAlt = false;
        RCtrl = false;
        LCtrl = false;
        Rus = false;
        Exec = true;
        ShiftEnabled = true;
        CtrlEnabled = false;
        AltEnabled = false;
        CanRusSwitch = false;
        //Log.AddRef();
        if (cmd != null)
        {
            cmd.AddRef();
            cmd.RegisterTrigger(sPlusRus, this, "Enable use right Ctrl as switch to cyrilic keyboard mode");
            cmd.RegisterCommand(sSetKey, this, 2, "bind key");
            cmd.RegisterCommand(sClearKey, this, 1, "unbind key");
        }
        InitKeys();
        for (int i = 0; i != 256; ++i) kbd[i] = 0;
    }
    public virtual int Release()
    {
        //Log.Release();
        if (cmd != null)
        {
            cmd.UnRegister(this);
            cmd.Release();
        }
        //delete this;
        return 0;
    }

    const string CantFindKey = "Cann't find key \"{0}\"";

    void clearAll()
    {
        //for (int i = 0; i < DIK_LAST; i++)
        //    KeyBinds[i].clearAll(cmd);
        foreach (KeyValuePair<int, KeyBind> k in KeyBinds)
        {
            k.Value.clearAll(cmd);
        }
    }

    void InitKeys()
    {
        //for (int i = 0; i < sizeof(keybord) / sizeof(KeyDefinition); ++i)
        //    KeyBinds[keybord[i].code].init(keybord[i]);
        //for (int i=0;i<keybord.Length; i++)
        //{
        //    KeyBind v = new KeyBind();
        //    v.init(keybord[i]);
        //    KeyBinds[keybord[i].code] = v; //TODO Возможно, массив KeyBinds стоит "обжать"  до keybord.Length
        //}

        foreach (KeyDefinition k in ITranslator2.keybord)
        {
            KeyBind v = new KeyBind();
            v.init(k);
            //Debug.Log(string.Format("Adding key {0} {1}",k.code,k.name));
            KeyBinds.Add(k.code, v);
        }
    }
    bool AltCtrlShift(int _key, int value)
    {
        if (ShiftEnabled)
        {
            switch (_key)
            {
                case DIK_RSHIFT: return setModifier(ref RShift, value != 0, (int)MMOD_SHIFT);
                case DIK_LSHIFT: return setModifier(ref LShift, value != 0, (int)MMOD_SHIFT);
            }
        }

        if (CtrlEnabled)
        {
            switch (_key)
            {
                case DIK_RCONTROL: return setModifier(ref RCtrl, value != 0, (int)MMOD_CTRL);
                case DIK_LCONTROL: return setModifier(ref LCtrl, value != 0, (int)MMOD_CTRL);
            }
        }

        if (AltEnabled)
        {
            switch (_key)
            {
                case DIK_RMENU: return setModifier(ref RAlt, value != 0, (int)MMOD_ALT);
                case DIK_LMENU: return setModifier(ref LAlt, value != 0, (int)MMOD_ALT);
            }
        }

        return false;
    }

    void ProcessModPress(int n)
    {
        //if (cmd != null) for (int i = 0; i < DIK_LAST; ++i)
        //        if (KeyBinds[i].anyRunning != 0)
        //            KeyBinds[i].pressMod(n, true, cmd);
        foreach (var k in KeyBinds)
        {
            if (k.Value.anyRunning != 0)
            {
                k.Value.pressMod(n, true, cmd);
            }
        }
    }
    void ProcessModRelease(int n)
    {
        //if (cmd != null) for (int i = 0; i < DIK_LAST; ++i)
        //        KeyBinds[i].pressMod(n, false, cmd);
        foreach (var k in KeyBinds)
        {
            if (k.Value.anyRunning != 0)
            {
                k.Value.pressMod((int)k.Value.crc, false, cmd);
            }
        }
    }
    void ProcessKeyPress(KeyBind k)
    {
        k.pressKey(
            ShiftEnabled && (LShift | RShift),
            CtrlEnabled && (LCtrl | RCtrl),
            AltEnabled && (LAlt | RAlt), cmd
        );
    }
    void ProcessKeyRelease(KeyBind k)
    {
        k.releaseKey(cmd);
    }

    public virtual string GetBuffer() { return null; }
    public virtual string Describe(int _key)
    {
        string s = null;
        switch (myLastModifier)
        {
            case (int)MMOD_ALT:
                if (AltEnabled)
                    s = "ALT+";
                break;
            case (int)MMOD_CTRL:
                if (CtrlEnabled)
                    s = "CTRL+";
                break;
            case (int)MMOD_SHIFT:
                if (ShiftEnabled)
                    s = "SHIFT+";
                break;
        }

        //static char Buffer[128];
        //s = s ? _addstr(Buffer, s) : Buffer;

        //_addstr(s, KeyBinds[_key].name ? KeyBinds[_key].name : "Unknown");
        s = s != null ? s : "";
        return KeyBinds[_key].name != null ? s + KeyBinds[_key].name : "Unknown";
    }
    public virtual bool Translate(int _key, out char c)
    {
        c = KeySymbol(_key, GetState());

        switch (_key)
        {
            case DIK_RMENU:
            case DIK_LMENU:
                return !AltEnabled;
            case DIK_RSHIFT:
            case DIK_LSHIFT:
                return !ShiftEnabled;
            case DIK_RCONTROL:
            case DIK_LCONTROL:
                return !CtrlEnabled;
        }
        return true;
    }

    public virtual char KeySymbol(int _key, int _state)
    {
        //kbd[VK_SHIFT] = (_state!=0 & SHIFT_STATE!=0) ? 255 : 0;

        //WORD[] ch = { 0, 0 };
        //int v = MapVirtualKey(_key, 1);
        //int n = (v!=0) ? ToAscii(v, 0, kbd, ch, 0) : 0;

        /////Log->Message("About translate: Key %d, vKey=%d, n=%d, ch=%d('%c')", _key, v, n, ch[0], ch[0]);

        //return n == 1 ? (char)ch[0] : KeyBinds[_key].symbols[_state];
        //TODO  Реализовать возврат символа нажатой клавиши
        return '\0';
    }

    public virtual void SwitchRus()
    {
        Rus = !Rus;
    }

    public virtual int GetState()
    {
        return ((RShift | LShift) ? (int)SHIFT_STATE : 0) | (Rus ? (int)RUS_STATE : 0);
    }
    public virtual int GetShift()
    {
        return (RShift || LShift) ? 1 : 0;
    }
    public virtual int GetCtrl()
    {
        return (RCtrl || LCtrl) ? 1 : 0;
    }
    public virtual int GetAlt()
    {
        return (RAlt || LAlt) ? 1 : 0;
    }

    public virtual int UseShift(bool data)
    {
        if (ShiftEnabled != data)
        {
            int prev = ShiftEnabled ? 1 : 0;
            ShiftEnabled = data;
            if (RShift || LShift)
            {
                if (data) ProcessModPress((int)MMOD_SHIFT); else ProcessModRelease((int)MMOD_SHIFT);
            }
            return prev;
        }
        else return ShiftEnabled ? 1 : 0;
    }
    public virtual int UseCtrl(bool data)
    {
        CtrlEnabled = data;
        if (RCtrl || LCtrl)
        {
            if (data) ProcessModPress((int)MMOD_CTRL); else ProcessModRelease((int)MMOD_CTRL);
        }
        return CtrlEnabled ? 1 : 0;
    }
    public virtual int UseAlt(bool data)
    {
        AltEnabled = data;
        if (RAlt || LAlt)
        {
            if (data) ProcessModPress((int)MMOD_ALT); else ProcessModRelease((int)MMOD_ALT);
        }
        return AltEnabled ? 1 : 0;
    }

    public virtual bool SetExec(bool data)
    {
        bool ex = Exec; Exec = data; return ex;
    }


    public virtual bool SetKey(in string name, int n, in string value)
    {
        //Debug.Log(string.Format("Setting {0} {1} {2}",name,(MMOD)n,value));
        KeyBind kr = FindKey(name);

        if (kr != null)
            kr.bind(n, value, cmd);

        return kr != null;
    }
    public virtual bool ClearKey(in string name, int n)
    {
        //union { int x; char c[4]; };
        char[] c = new char[4];

        for (int i = 0; i < 4; ++i)
            c[i] = char.ToUpper(name[i]);

        //if (x == 'LLA')
        if (new string(c) == "LLA")  //Вот не так это должно быть
        {
            clearAll();
            return true;
        }
        else
        {
            KeyBind kr = FindKey(name);
            if (kr != null)
                kr.clear(n, cmd);
            return kr != null;
        }
    }


    public virtual bool ProcessKeyPress(int _key, int _code)
    {
        bool ret = !AltCtrlShift(_key, _code);
        if (_code != 0)
        {

            if (_key == DIK_RCONTROL && CanRusSwitch)
                SwitchRus();

            if (Exec && cmd != null)
            {
                //Debug.Log("ProcessKeyPress(KeyBinds[_key])" + _key + " " + _code);
                ProcessKeyPress(KeyBinds[_key]);
            }
        }
        else
            if (cmd != null) ProcessKeyRelease(KeyBinds[_key]);
        return ret;
    }

    //ComLink
    public virtual void OnTrigger(uint name, bool value)
    {
        if (name == iPlusRus)
        {
            CanRusSwitch = value;
            if (CanRusSwitch == false)
            {
                Rus = false;
                Log.Message("switch to cyrillic keyboard mode disabled");
            }
            else
                Log.Message("switch to cyrillic keyboard mode enabled");
        }
    }
    public virtual void OnCommand(uint cmd, string arg1, string arg2)
    {
        int mod;
        switch (cmd)
        {
            case iSetKey:
                SetKey(ParseMod(arg1, out mod), mod, arg2);
                break;

            case iClearKey:
                ClearKey(ParseMod(arg1, out mod), mod);
                break;
        }
    }

    // ITranslator2

    public virtual bool SetKeyEx(in string keyName, in string value)
    {
        int mod;
        return SetKey(ParseMod(keyName, out mod), mod, value);
    }
    public virtual bool ClearKeyEx(in string keyName)
    {
        int mod;
        return ClearKey(ParseMod(keyName, out mod), mod);
    }

    private string ParseMod(string bind, out int mod)
    {
        if (bind.ToUpper().StartsWith("SHIFT+"))
        {
            mod = (int)MMOD.MMOD_SHIFT;
            return bind.Remove(0, 6);
        }

        if (bind.ToUpper().StartsWith("CTRL+"))
        {
            mod = (int)MMOD.MMOD_CTRL;
            return bind.Remove(0, 5);
        }
        if (bind.ToUpper().StartsWith("ALT+"))
        {
            mod = (int)MMOD.MMOD_ALT;
            return bind.Remove(0, 4);
        }
        mod = 0;
        return bind;
    }

    KeyBind FindKey(string name)
    {
        //string NAME = ANewN(char, __strlen(name));
        //for (int i = 0; name[i]; ++i)
        //    NAME[i] = upperLetter(name[i]);
        string NAME = name.ToUpper();

        //crc32 val = Crc32.Code(NAME, i); //TODO СТРАННОЕ! наверное crc клавиши стоит вычислять как-то иначе
        crc32 val = Hasher.HshString(NAME);

        //for (int i = 0; i < DIK_LAST; ++i) //TODO - Правильнее перебирать ITranslator.KeyDefinition
        //    if (KeyBinds[i].crc == val)
        //        return KeyBinds[i] ;

        //if (KeyBinds.ContainsKey(val)) return KeyBinds[val];
        foreach (var k in KeyBinds)
        {
            if (k.Value.crc == val) return k.Value;
        }
        Log.Message(CantFindKey, name);
        return null;
    }

    public static ITranslator2 CreateTranslator(ILog _l, CommandsApi _cmd)
    {
        return new Translator(_l, _cmd);
    }
};

enum KeyStates
{
    SHIFT_STATE = 1,
    RUS_STATE = 2
};


public struct KeyDefinition
{
    public int code;
    public string name;
    public int alph;    // "dDвВ", "wWцЦ"

    public KeyDefinition(int code, string name, int alph)
    {
        this.code = code;
        this.name = name;
        this.alph = alph;
    }

    public KeyDefinition(int code, string name, string alph) : this(code, name, 0) { }
    public KeyDefinition(int code, string name) : this(code, name, 0) { }
}

public class KeyBind
{
    public string name;
    public crc32 crc;

    //Tab<char>[] binds = new Tab<char>[4];//[4];    // bind, shift-bind, ctrl-bind, alt-bind
    string[] binds = new string[4];

    //    union {
    public char[] symbols = new char[4];//[4];
    int alpha
    {
        get
        {
            byte[] bytes = { (byte)symbols[0], (byte)symbols[1], (byte)symbols[2], (byte)symbols[3] };
            return BitConverter.ToInt32(bytes);
        }
        set
        {
            byte[] bytes = BitConverter.GetBytes(value);
            symbols = new char[] { (char)bytes[0], (char)bytes[1], (char)bytes[2], (char)bytes[3] };
        }
    }
    //};

    //union{
    bool[] running = new bool[4];//[4];
    //public int anyRunning { get; set; }
    public int anyRunning
    {
        get
        {
            foreach (bool r in running)
            {
                if (r) return 1;
            }
            return 0;
        }
        set
        {
            for (int i = 0; i < 4; i++)
            {
                running[i] = value != 0;
            }
        }
    }
    //};

    public KeyBind()
    {
        name = null;
        crc = CRC32.CRC_NULL;
        alpha = 0;
        anyRunning = 0;
    }

    public void init(KeyDefinition def)
    {
        crc = Hasher.HshString(name = def.name);
        alpha = _bswap(def.alph);
    }

    private int _bswap(int value)
    {
        byte[] arr = BitConverter.GetBytes(value);
        Array.Reverse(arr);
        return BitConverter.ToInt32(arr);
    }

    public void clearAll(CommandsApi cmd)
    {
        for (int i = 0; i < 4; ++i)
            clear(i, cmd);
    }

    public void bind(int n, string value, CommandsApi cmd)
    {
        if (cmd != null && anyRunning != 0)
            releaseKey(cmd);

        binds[n] = value;
        //binds[n] = new Tab<char>();
        //foreach(char v in value) //TODO возможно, проже как string и хранить
        //{
        //    binds[n].Add(v);
        //}
        //binds[n](__strlen(value));
        //__strcpy(binds[n].Begin(), value);
    }
    bool hasBind(int mod)
    {
        if (binds[mod] == null) return false;
        //return binds[mod].Count != 0; 
        return binds[mod] != null;
    }
    public void clear(int n, CommandsApi cmd)
    {
        if (cmd != null && anyRunning != 0)
            releaseKey(cmd);

        //binds[n].Zero();
        binds[n] = null;
        running[n] = false;
    }

    public void pressMod(int mod, bool start, CommandsApi cmd)
    {
        if (running[mod] != start)
        {
            bool bnd = hasBind(mod);
            if (bnd)
            {
                Debug.Log("Processing string [" + binds[mod] + "] start? " + start.ToString());
                cmd.ProcessString(binds[mod], !start);
            }
            else
            {
                Debug.Log("key not binded");
            }
            running[mod] = start;
        }
    }

    public void pressKey(bool shift, bool ctrl, bool alt, CommandsApi cmd)
    {
        if (ctrl | shift | alt)
        {
            if (shift)
                pressMod(1, true, cmd);
            if (ctrl)
                pressMod(2, true, cmd);
            if (alt)
                pressMod(3, true, cmd);
        }
        else
            pressMod(0, true, cmd);
    }

    public void releaseKey(CommandsApi cmd)
    {
        if (anyRunning != 0)
            for (int i = 0; i < 4; ++i)
                pressMod(i, false, cmd);
    }
};

public static class DIKtoUnityMapping
{
    public const bool DIK_PRESSED = true;
    public const bool DIK_RELEASED = false;


    public const int DIK_ESCAPE = (int)KeyCode.Escape;
    public const int DIK_1 = (int)KeyCode.Alpha1;
    public const int DIK_2 = (int)KeyCode.Alpha2;
    public const int DIK_3 = (int)KeyCode.Alpha3;
    public const int DIK_4 = (int)KeyCode.Alpha4;
    public const int DIK_5 = (int)KeyCode.Alpha5;
    public const int DIK_6 = (int)KeyCode.Alpha6;
    public const int DIK_7 = (int)KeyCode.Alpha7;
    public const int DIK_8 = (int)KeyCode.Alpha8;
    public const int DIK_9 = (int)KeyCode.Alpha9;
    public const int DIK_0 = (int)KeyCode.Alpha0;
    public const int DIK_MINUS = (int)KeyCode.Minus;
    public const int DIK_EQUALS = (int)KeyCode.Equals;
    public const int DIK_BACK = (int)KeyCode.Backspace;
    public const int DIK_TAB = (int)KeyCode.Tab;
    public const int DIK_Q = (int)KeyCode.Q;
    public const int DIK_W = (int)KeyCode.W;
    public const int DIK_E = (int)KeyCode.E;
    public const int DIK_R = (int)KeyCode.R;
    public const int DIK_T = (int)KeyCode.T;
    public const int DIK_Y = (int)KeyCode.Y;
    public const int DIK_U = (int)KeyCode.U;
    public const int DIK_I = (int)KeyCode.I;
    public const int DIK_O = (int)KeyCode.O;
    public const int DIK_P = (int)KeyCode.P;
    public const int DIK_LBRACKET = (int)KeyCode.LeftBracket;
    public const int DIK_RBRACKET = (int)KeyCode.RightBracket;
    public const int DIK_RETURN = (int)KeyCode.Return;
    public const int DIK_LCONTROL = (int)KeyCode.LeftControl;
    public const int DIK_A = (int)KeyCode.A;
    public const int DIK_S = (int)KeyCode.S;
    public const int DIK_D = (int)KeyCode.D;
    public const int DIK_F = (int)KeyCode.F;
    public const int DIK_G = (int)KeyCode.G;
    public const int DIK_H = (int)KeyCode.H;
    public const int DIK_J = (int)KeyCode.J;
    public const int DIK_K = (int)KeyCode.K;
    public const int DIK_L = (int)KeyCode.L;
    public const int DIK_SEMICOLON = (int)KeyCode.Semicolon;
    public const int DIK_APOSTROPHE = (int)KeyCode.BackQuote;
    public const int DIK_GRAVE = (int)KeyCode.Quote;
    public const int DIK_LSHIFT = (int)KeyCode.LeftShift;
    public const int DIK_BACKSLASH = (int)KeyCode.Backslash;
    public const int DIK_Z = (int)KeyCode.Z;
    public const int DIK_X = (int)KeyCode.X;
    public const int DIK_C = (int)KeyCode.C;
    public const int DIK_V = (int)KeyCode.V;
    public const int DIK_B = (int)KeyCode.B;
    public const int DIK_N = (int)KeyCode.N;
    public const int DIK_M = (int)KeyCode.M;
    public const int DIK_COMMA = (int)KeyCode.Comma;
    public const int DIK_PERIOD = (int)KeyCode.Period;
    public const int DIK_SLASH = (int)KeyCode.Slash;
    public const int DIK_RSHIFT = (int)KeyCode.RightShift;
    public const int DIK_MULTIPLY = (int)KeyCode.KeypadMultiply;
    public const int DIK_LMENU = (int)KeyCode.LeftAlt;
    public const int DIK_SPACE = (int)KeyCode.Space;
    public const int DIK_CAPITAL = (int)KeyCode.CapsLock;
    public const int DIK_F1 = (int)KeyCode.F1;
    public const int DIK_F2 = (int)KeyCode.F2;
    public const int DIK_F3 = (int)KeyCode.F3;
    public const int DIK_F4 = (int)KeyCode.F4;
    public const int DIK_F5 = (int)KeyCode.F5;
    public const int DIK_F6 = (int)KeyCode.F6;
    public const int DIK_F7 = (int)KeyCode.F7;
    public const int DIK_F8 = (int)KeyCode.F8;
    public const int DIK_F9 = (int)KeyCode.F9;
    public const int DIK_F10 = (int)KeyCode.F10;
    public const int DIK_NUMLOCK = (int)KeyCode.Numlock;
    public const int DIK_SCROLL = (int)KeyCode.ScrollLock;
    public const int DIK_NUMPAD7 = (int)KeyCode.Keypad7;
    public const int DIK_NUMPAD8 = (int)KeyCode.Keypad8;
    public const int DIK_NUMPAD9 = (int)KeyCode.Keypad9;
    public const int DIK_SUBTRACT = (int)KeyCode.KeypadMinus;
    public const int DIK_NUMPAD4 = (int)KeyCode.Keypad4;
    public const int DIK_NUMPAD5 = (int)KeyCode.Keypad5;
    public const int DIK_NUMPAD6 = (int)KeyCode.Keypad6;
    public const int DIK_ADD = (int)KeyCode.KeypadPlus;
    public const int DIK_NUMPAD1 = (int)KeyCode.Keypad1;
    public const int DIK_NUMPAD2 = (int)KeyCode.Keypad2;
    public const int DIK_NUMPAD3 = (int)KeyCode.Keypad3;
    public const int DIK_NUMPAD0 = (int)KeyCode.Keypad0;
    public const int DIK_DECIMAL = (int)KeyCode.KeypadPeriod;
    //public const int  DIK_OEM_102     ,"MAGGIORE",'><  '}, такая кнопка нам не нужна!
    public const int DIK_F11 = (int)KeyCode.F11;
    public const int DIK_F12 = (int)KeyCode.F12;
    public const int DIK_PAUSE = (int)KeyCode.Pause;
    //public const int  DIK_F13         ,"F13"}, Япопские клавищи - к Годзилле!
    //public const int  DIK_F14         ,"F14"}, 
    //public const int  DIK_F15         ,"F15"},
    //public const int  DIK_KANA        ,"KANA"},
    //public const int  DIK_CONVERT     ,"CONVERT"},
    //public const int  DIK_NOCONVERT   ,"NOCONVERT"},
    //public const int  DIK_YEN         ,"YEN"},
    public const int DIK_NUMPADEQUALS = (int)KeyCode.KeypadEquals;
    //public const int  DIK_CIRCUMFLEX  = ,"CIRCUMFLEX"}, qwertz - тоже
    public const int DIK_AT = (int)KeyCode.At;
    public const int DIK_COLON = (int)KeyCode.Colon;
    public const int DIK_UNDERLINE = (int)KeyCode.Underscore;
    //public const int  DIK_KANJI       ,"KANJI"}, Нет!
    //public const int  DIK_STOP        ,"STOP"},
    //public const int  DIK_AX          ,"AX"},
    //public const int  DIK_UNLABELED   ,"UNLABELED"},
    public const int DIK_NUMPADENTER = (int)KeyCode.KeypadEnter;
    public const int DIK_RCONTROL = (int)KeyCode.RightControl;
    //public const int  DIK_NUMPADCOMMA ,"PAD_COMMA",',,  '},А нету
    public const int DIK_DIVIDE = (int)KeyCode.KeypadDivide;
    public const int DIK_SYSRQ = (int)KeyCode.SysReq;
    public const int DIK_RMENU = (int)KeyCode.RightAlt;
    public const int DIK_HOME = (int)KeyCode.Home;
    public const int DIK_UP = (int)KeyCode.UpArrow;
    public const int DIK_PRIOR = (int)KeyCode.PageUp;
    public const int DIK_LEFT = (int)KeyCode.LeftArrow;
    public const int DIK_RIGHT = (int)KeyCode.RightArrow;
    public const int DIK_END = (int)KeyCode.End;
    public const int DIK_DOWN = (int)KeyCode.DownArrow;
    public const int DIK_NEXT = (int)KeyCode.PageDown;
    public const int DIK_INSERT = (int)KeyCode.Insert;
    public const int DIK_DELETE = (int)KeyCode.Delete;
    public const int DIK_LWIN = (int)KeyCode.LeftMeta;
    public const int DIK_RWIN = (int)KeyCode.RightMeta;
    //public const int  DIK_APPS        ,"APPS"},

    //mouse
    public const int DIK_MOUSE1 = (int)KeyCode.Mouse0;
    public const int DIK_MOUSE2 = (int)KeyCode.Mouse1;
    public const int DIK_MOUSE3 = (int)KeyCode.Mouse2;
    public const int DIK_MOUSE4 = (int)KeyCode.Mouse3;
    //public const int  DIK_MOUSE_WUP   ,"MWHEEL_UP"},//в Unity это ось
    //public const int  DIK_MOUSE_WDOWN ,"MWHEEL_DOWN"},

    //В "Шторме" тоже, но тырцается как именно кнопка.
    public const int DIK_MOUSE_WUP = DIK_MOUSE1 + 6;
    public const int DIK_MOUSE_WDOWN = DIK_MOUSE1 + 7;

    // JOYSTICK
    public const int DIK_JOYBUTTON1 = (int)KeyCode.Joystick1Button0;
    public const int DIK_JOYBUTTON2 = (int)KeyCode.JoystickButton1;
    public const int DIK_JOYBUTTON3 = (int)KeyCode.JoystickButton2;
    public const int DIK_JOYBUTTON4 = (int)KeyCode.JoystickButton3;
    public const int DIK_JOYBUTTON5 = (int)KeyCode.JoystickButton4;
    public const int DIK_JOYBUTTON6 = (int)KeyCode.JoystickButton5;
    public const int DIK_JOYBUTTON7 = (int)KeyCode.JoystickButton6;
    public const int DIK_JOYBUTTON8 = (int)KeyCode.JoystickButton7;
    public const int DIK_JOYBUTTON9 = (int)KeyCode.JoystickButton8;
    public const int DIK_JOYBUTTON10 = (int)KeyCode.JoystickButton9;
    public const int DIK_JOYBUTTON11 = (int)KeyCode.JoystickButton10;
    public const int DIK_JOYBUTTON12 = (int)KeyCode.JoystickButton11;
    public const int DIK_JOYBUTTON13 = (int)KeyCode.JoystickButton12;
    public const int DIK_JOYBUTTON14 = (int)KeyCode.JoystickButton13;
    public const int DIK_JOYBUTTON15 = (int)KeyCode.JoystickButton14;
    public const int DIK_JOYBUTTON16 = (int)KeyCode.JoystickButton15;
    public const int DIK_JOYBUTTON17 = (int)KeyCode.JoystickButton16;
    public const int DIK_JOYBUTTON18 = (int)KeyCode.JoystickButton17;
    public const int DIK_JOYBUTTON19 = (int)KeyCode.JoystickButton18;
    //public const int  DIK_JOYBUTTON20 =(int) KeyCode.JoystickButton19;//В юнити столько нет!
    //public const int  DIK_JOYBUTTON21 =(int) KeyCode.JoystickButton21;
    //public const int  DIK_JOYBUTTON22 =(int) KeyCode.JoystickButton22;
    //public const int  DIK_JOYBUTTON23 =(int) KeyCode.JoystickButton23;
    //public const int  DIK_JOYBUTTON24 =(int) KeyCode.JoystickButton24;
    //public const int  DIK_JOYBUTTON25 =(int) KeyCode.JoystickButton25;
    //public const int  DIK_JOYBUTTON26 =(int) KeyCode.JoystickButton26;
    //public const int  DIK_JOYBUTTON27 =(int) KeyCode.JoystickButton27;
    //public const int  DIK_JOYBUTTON28 =(int) KeyCode.JoystickButton28;
    //public const int  DIK_JOYBUTTON29 =(int) KeyCode.JoystickButton29;
    //public const int  DIK_JOYBUTTON30 =(int) KeyCode.JoystickButton30;
    //public const int  DIK_JOYBUTTON31 =(int) KeyCode.JoystickButton31;
    //public const int  DIK_JOYBUTTON32 =(int) KeyCode.JoystickButton32;
    //public const int  DIK_JOYHAT0     ,"J_HAT0"}, //Хатка на дже обрабатыватся не так
    //public const int  DIK_JOYHAT45    ,"J_HAT45"},
    //public const int  DIK_JOYHAT90    ,"J_HAT90"},
    //public const int  DIK_JOYHAT135   ,"J_HAT135"},
    //public const int  DIK_JOYHAT180   ,"J_HAT180"},
    //public const int  DIK_JOYHAT225   ,"J_HAT225"},
    //public const int  DIK_JOYHAT270   ,"J_HAT270"},
    //public const int  DIK_JOYHAT315   ,"J_HAT315"},
}