using System.Collections.Generic;
using UnityEngine;
using crc32 = System.UInt32;
using DWORD = System.UInt32;
using static StrParam;
using static putils;
using static ComType;

public class Commands : CommandsApi, CommLink
{
    public void AddRef()
    {

    }

    public bool ExecFile(string s)
    {
        //TODO реализовать загрузку .dlc файлов
        Debug.Log("STUB!! Loading file " + s);
        return true;
    }


    public void ProcessString(string str, bool inv = false)
    {
        str = str.Trim();//TODO Сомнительное. правильнее передавать то что нужно
        do
        {
            StrWord2 word = new StrWord2(str);
            if (!word.Valid())
            {
                if (word.Start() != null) str = word.Start().Remove(0, 1); else break;
            }
            else
            {
                ComBase c = GetCommand(word.Code());
                //Debug.Log(c);

                if (c != null)
                {
                    // log->Message("exec \"%s\" inv=%d", c->sname, inv);
                    //log.Message("exec \"{0}\" inv={1}", c.name, inv);
                    string str2 = c.Exec(word.End(), inv);
                    //Debug.Log("Result: " + str2);
                    if (str2 == null)
                    {

                        //string error = ANewN(char, word.WordLen() + 24);
                        string error;
                        string buf = word.CopyTo(out error);
                        string from = word.End();

                        //for (; *from && *from != 13 && (from - word.End()) < 24;) *buf++ = *from++;

                        //__nstrcpy(word.CopyTo(error), word.End(), 20);

                        log.Message("%s error in command: '%s'", com_err, error);

                    }
                    str = str2;
                }
                else
                {
                    // command not found
                    str = FindEndOfCommand(word.End());
                    if (!inv) InvalidCommandMSG(word, str);
                }

                if (str != null)
                {
                    StrChar eoc = new StrChar(str);
                    //Debug.Log("EOC [" + eoc.Char() + "]");
                    if (eoc.Char() != ';')
                        log.Message("{0} ';' expected", com_err);
                    else
                        str = eoc.End();
                }
            }
            } while (str!=null);
        //} while (str.Length > 0);
    }

    void InvalidCommandMSG(StrWord2 cmd, string end)
    {
        Debug.Log("Invalid command " + cmd.Word() + " of " + this.cmd.Count());
        //string res = "Commands list:";
        //foreach (var c in this.cmd)
        //{
        //    res += string.Format("\n{0} {1}", c.ctype, c.name);
        //}
        //Debug.Log(res);
    }
    const string com_err = "DLC Error:";

    string FindEndOfCommand(string str)
    {
        // we need to find ';' but string can contain hested '[' ']'
        while (str.Length > 0)
        {
            switch (str[0])
            {
                case '\0': return null;
                case ';': return str;
                case '[':
                    str = CloseBracket(str.Remove(0, 1), '[', ']', 1);
                    break;
                default:
                    str = str.Remove(0, 1);
                    break;

            }
        }
        return str;
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    crc32 AddCommand(ComBase com)
    {
        int cmn = FindCommand(com.pname);

        if (cmn < 0)
            cmd.New(com);
        else
        {
            com.SetStacked(cmd[cmn]); 
            //cmd[cmn] = com;
        }
        return com.cname;
    }

    public uint RegisterCommand(string nm, CommLink l, int nArgs, string help = "")
    {
        Debug.Log("Registering command " + nm + " " + Hasher.HshString(nm).ToString("X8"));
        return AddCommand(new Command(nm, Hasher.HshString(nm), help, l, nArgs)); //Возможно, лучше Code?
    }

    public uint RegisterTrigger(string nm, CommLink l, string help = "")
    {
        Debug.Log("Registering trigger " + nm);
        crc32 cname = Hasher.HshString(nm);

        AddCommand(new Trigger(nm, cname, Hasher.HshString("+" + nm), help, l, true));
        AddCommand(new Trigger(nm, cname, Hasher.HshString("-" + nm), help, l, false));

        return cname;
    }

    public uint RegisterVariable(string nm, CommLink cs, VType vt, string help = "")
    {
        Debug.Log("Registering variable " + nm);
        //return Constants.THANDLE_INVALID;
        return AddCommand(new Variable(nm, Hasher.HshString(nm), help, cs, vt, log));
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }

    public string SuggestSpelling(string source, string limit)
    {
        throw new System.NotImplementedException();
    }

    //Unregister procedure
    bool CanUnreg(ComBase c, CommLink l, crc32 name)
    {
        return (c.link == l) && (name == CRC32.CRC_NULL || c.cname == name);
    }
    public void UnRegister(CommLink l, uint name = uint.MaxValue)
    {
        Debug.LogFormat("Unregistering command {0} {1}",Hasher.StringHsh(name),name.ToString("X8"));
        for (int i = 0; i < cmd.Count();)
            if (CanUnreg(cmd[i], l, name))
            {
                //Debug.LogFormat("Can unreg. Deleting command {0} {1} {2} ", cmd[i], cmd[i].name, name.ToString("X8"));
                ComBase st = GetStackedAndDelete(cmd[i]);

                if (st != null)
                {
                    cmd[i] = st;
                }
                else
                {
                    cmd.Remove(i, i);
                }

            }
            else
            {
                // search for stacked to delete
                for (ComBase c = cmd[i]; c.GetStacked()!=null;)
                {
                    Debug.LogFormat("Can't unreg. Deleting stacked command {0} {1} {2} ",cmd[i], cmd[i].name, name.ToString("X8"));
                    if (CanUnreg(c.GetStacked(), l, name))
                    {
                        Debug.Log("Can!");
                        var tmp = c.GetStacked();
                        c.SetStacked(GetStackedAndDelete(tmp));
                    }
                    else
                    {
                        Debug.Log("Can't!");
                        c = c.GetStacked();
                    }
                }
                ++i; // increment for
            } // if
    }

    int FindCommand(crc32 name)
    {
        for (int i = 0; i < cmd.Count(); ++i)
            if (cmd[i].pname == name) return i;
        return -1;
    }

    string GetComType(ComType t)
    {
        switch (t)
        {
            case CM_TRIGGER: return "trigger";
            case CM_COMMAND: return "command";
            case CM_ACTION: return "action";
            case CM_VARIABLE: return "variable";
            default: return "unknown command type";
        }
    }

    //ComBase GetStackedAndDelete(ref ComBase c)
    ComBase GetStackedAndDelete(ComBase c) //TODO вернуть ref, если будет глючить
    {
        ComBase st = c.GetStacked();
        c.SetStacked(null);
        c.Dispose();
        return st;
    }

    //List<ComBase> cmd = new List<ComBase>();
    Tab<ComBase> cmd = new Tab<ComBase>();
    ILog log;
    ProductInfo.IFileSystem ifs;

    public int pls_code;
    public int mns_code;
    string myDlcPath;

    public Commands(ILog l, ProductInfo.IFileSystem fs, string dlc_path)
    {
        log = l;
        ifs = fs;
        myDlcPath = dlc_path;
        RegisterCommand("help", this, 1, "help <command> - show help on a command");
        RegisterCommand("action", this, 2, "action <action_name> <action_value> - string <action_value> associated with a <action_name>");
        RegisterCommand("pexit", this, 0, "call TerminateProcess windows kernel function");
        RegisterCommand("exec", this, 1, "execute a specified file");
    }
    public Commands()
    {
    }

    ~Commands()
    {
        UnRegister(this, CRC32.CRC_NULL);

        for (int i = 0; i < cmd.Count(); ++i)
        {
            do
            {
                if (cmd[i].ctype != CM_ACTION)
                    log.Message("{0} Unregistered {1} \"{2}\"", com_err, GetComType(cmd[i].ctype), cmd[i].name);
                cmd[i] = GetStackedAndDelete(cmd[i]);
            } while (cmd[i] != null);
        }
    }

    public ComBase GetCommand(uint name)
    {
        for (int i = 0; i < cmd.Count(); ++i)
            if (cmd[i].pname == name) return cmd[i];
        return null;
    }
}

public static class CommandsUtils
{
    public struct StormVMcmd
    {
        string name;
        crc32 code;

        public StormVMcmd(string _name, crc32 _code)
        {
            name = _name;
            code = _code;
        }
    }


    public static T createScriptCommand<T>(string name, IQuery tm, IVmVariablePool pool = null) where T : BaseCommand, new()
    {
        //Debug.Log(string.Format("createScriptCommand<{0}> {1}",typeof(T),name));
        //T cmd = createObject<T>();
        T cmd = new T();
        if (!cmd.initialize(name, tm, pool))
        {
            cmd.Release();
            cmd = null;
        }
        Debug.Log(string.Format("createScriptCommand<{0}> {1} cmd: {2} {3}", typeof(T), name, cmd != null ? "SUCCESS" : "FAILURE", tm));
        return cmd;
    }

    public static T createControllerCommand<T>(string name, IQuery tm, IVmVariablePool pool, IVmController cont = null) where T : BaseControllerCommand, new()
    {
        Debug.Log("Creating Controller Command " + name + " for " + typeof(T));
        T cmd = createScriptCommand<T>(name, tm, pool);
        if (cmd != null)
            cmd.setController(cont);
        if (cmd == null) Debug.Log(string.Format("Failed to create controller command [{0}] [{1}] for {2}", name, tm, typeof(T)));

        return cmd;
    }
    //#define CREATE_CMD(name,code) static const char cmdname_##name[] = #name; \
    //    static const crc32 cmd_##name=code; \
    //                              case cmd_##name: return createScriptCommand<Cmd##name>(cmdname_##name,myQuery,pool)
    //#define CREATE_CNT(name,code,creator) static const crc32 cmd_##name=code; \
    //                         case cmd_##name : return create##creator()
    //#define CREATE_SEQCMD(name,code) static const char cmdname_##name[] = #name; \
    //                                 static const crc32 cmd_##name=code; \
    //                                 case cmd_##name: return createControllerCommand<Cmd##name>(cmdname_##name,myQuery,pool,cont)

    public static DWORD GetSkillCodeFromName(string mpName)
    {
        DWORD hs = Hasher.HshString(mpName);
        switch (hs)
        {
            case 0x1A577780: // Novice
                return SkillDefines.SKILL_NOVICE;
            case 0xFB9C895C: // Veteran
                return SkillDefines.SKILL_VETERAN;
            case 0xE9B34EEA: // Elite
                return SkillDefines.SKILL_ELITE;
        }
        return CRC32.CRC_NULL;
    }
}


