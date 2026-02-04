using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using crc32 = System.UInt32;

public class StdGroupFactory : BaseFactory
{
    const string cmdname_Delay = "Delay"; const crc32 cmd_Delay = 0x8CA45060;
    const string cmdname_SendMessage = "SendMessage"; const crc32 cmd_SendMessage = 0x668618E8;
    const string cmdname_NotifyAll = "NotifyAll"; const crc32 cmd_NotifyAll = 0xCDE0A8AD;
    const string cmdname_RandomCode = "RandomCode"; const crc32 cmd_RandomCode = 0x3E77C0AD;
    const string cmdname_AttackRadius = "AttackRadius"; const crc32 cmd_AttackRadius = 0x8F6A26F3;
    const string cmdname_Appear = "Appear"; const crc32 cmd_Appear = 0x942584FB;
    const string cmdname_AddPriority = "AddPriority"; const crc32 cmd_AddPriority = 0xA4CF9D86;
    const string cmdname_ChangeSide = "ChangeSide"; const crc32 cmd_ChangeSide = 0xAF5F3343;
    const string cmdname_Disappear = "Disappear"; const crc32 cmd_Disappear = 0x551817ED;
    const string cmdname_SetSkill = "SetSkill"; const crc32 cmd_SetSkill = 0x5197C448;
    const string cmdname_RandomBounds = "RandomBounds"; const crc32 cmd_RandomBounds = 0x67FCBCF8;
    const string cmdname_Suicide = "Suicide"; const crc32 cmd_Suicide = 0xEDBFC9D3;
    const string cmdname_IncludeSubobjects = "IncludeSubobjects"; const crc32 cmd_IncludeSubobjects = 0x1EEB08D9;
    const string cmdname_AutoMessages = "AutoMessages"; const crc32 cmd_AutoMessages = 0x9BB7F480;
    const string cmdname_PriorityGroup = "PriorityGroup"; const crc32 cmd_PriorityGroup = 0x75DD0209;
    const string cmdname_ShowCallsign = "ShowCallsign"; const crc32 cmd_ShowCallsign = 0x86EBAE20;

    const string cmdname_OnMessage = "OnMessage"; const crc32 cmd_OnMessage = 0x39D04393;
    const string cmdname_OnContact = "OnContact"; const crc32 cmd_OnContact = 0xC30F95D4;
    const string cmdname_OnAppear = "OnAppear"; const crc32 cmd_OnAppear = 0x6BF11DC8;
    const string cmdname_OnEngage = "OnEngage"; const crc32 cmd_OnEngage = 0x30FE9386;
    const string cmdname_OnNeutral = "OnNeutral"; const crc32 cmd_OnNeutral = 0x2B49961C;
    const string cmdname_OnFriendly = "OnFriendly"; const crc32 cmd_OnFriendly = 0x79FF33AA;
    const string cmdname_OnDeath = "OnDeath"; const crc32 cmd_OnDeath = 0x028A460C;
    const string cmdname_OnLostContact = "OnLostContact"; const crc32 cmd_OnLostContact = 0xA82CF680;

    private Dictionary<crc32, string> CmdDictionary;
    private string GetCommandName(crc32 id)
    {
        if (CmdDictionary == null) CreateCmdDictionary();

        return CmdDictionary.ContainsKey(id) ? CmdDictionary[id] : "NOT FOUND";
    }

    private void CreateCmdDictionary()
    {
        CmdDictionary = new Dictionary<crc32, string>();

        FieldInfo[] fieldInfos = typeof(StdGroupFactory).GetFields(BindingFlags.Public | BindingFlags.Static| BindingFlags.FlattenHierarchy);

        foreach (FieldInfo fi in fieldInfos) {
            if (!fi.IsLiteral || fi.IsInitOnly) continue;

            string[] vals = fi.Name.Split('_');
            if (vals[0]=="cmd")
            {
                CmdDictionary.Add((crc32)fi.GetValue(null), vals[1]);
            }
        }
    }
    public StdGroupFactory(IQuery q, IVmFactory prev_factory) : base(q, prev_factory) { }

    public override IVmCommand createCommand(crc32 name, IVmVariablePool pool, IVmController cont)
    {
        Asserts.Assert(myPrevFactory == null);
        //Debug.Log(string.Format("Creating {0} in {1}", name.ToString("X8"), this));
        switch (name)
        {
            case cmd_Delay:
                return CommandsUtils.createScriptCommand<CmdDelay>(cmdname_Delay, myQuery, pool);
            case cmd_SendMessage:
                return CommandsUtils.createScriptCommand<CmdSendMessage>(cmdname_SendMessage, myQuery, pool);
            case cmd_NotifyAll:
                return CommandsUtils.createScriptCommand<CmdNotifyAll>(cmdname_NotifyAll, myQuery, pool);
            case cmd_RandomCode:
                return CommandsUtils.createScriptCommand<CmdRandomCode>(cmdname_RandomCode, myQuery, pool);
            case cmd_AttackRadius:
                return CommandsUtils.createScriptCommand<CmdAttackRadius>(cmdname_AttackRadius, myQuery, pool);
            case cmd_Appear:
                return CommandsUtils.createScriptCommand<CmdAppear>(cmdname_Appear, myQuery, pool);
            case cmd_AddPriority:
                return CommandsUtils.createScriptCommand<CmdAddPriority>(cmdname_AddPriority, myQuery, pool);
            case cmd_ChangeSide:
                return CommandsUtils.createScriptCommand<CmdChangeSide>(cmdname_ChangeSide, myQuery, pool);
            case cmd_Disappear:
                return CommandsUtils.createScriptCommand<CmdDisappear>(cmdname_Disappear, myQuery, pool);
            case cmd_SetSkill:
                return CommandsUtils.createScriptCommand<CmdSetSkill>(cmdname_SetSkill, myQuery, pool);
            case cmd_RandomBounds:
                return CommandsUtils.createScriptCommand<CmdRandomBounds>(cmdname_RandomBounds, myQuery, pool);
            case cmd_Suicide:
                return CommandsUtils.createScriptCommand<CmdSuicide>(cmdname_Suicide, myQuery, pool);
            case cmd_IncludeSubobjects:
                return CommandsUtils.createScriptCommand<CmdIncludeSubobjects>(cmdname_IncludeSubobjects, myQuery, pool);
            case cmd_AutoMessages:
                return CommandsUtils.createScriptCommand<CmdAutoMessages>(cmdname_AutoMessages, myQuery, pool);
            case cmd_PriorityGroup:
                return CommandsUtils.createScriptCommand<CmdPriorityGroup>(cmdname_PriorityGroup, myQuery, pool);
            case cmd_ShowCallsign:
                return CommandsUtils.createScriptCommand<CmdShowCallsign>(cmdname_ShowCallsign, myQuery, pool);

            case cmd_OnMessage:
                return CommandsUtils.createControllerCommand<CmdOnMessage>(cmdname_OnMessage, myQuery, pool, cont);
            case cmd_OnContact:
                return CommandsUtils.createControllerCommand<CmdOnContact>(cmdname_OnContact, myQuery, pool, cont);
            case cmd_OnAppear:
                return CommandsUtils.createControllerCommand<CmdOnAppear>(cmdname_OnAppear, myQuery, pool, cont);
            case cmd_OnEngage:
                return CommandsUtils.createControllerCommand<CmdOnEngage>(cmdname_OnEngage, myQuery, pool, cont);
            case cmd_OnNeutral:
                return CommandsUtils.createControllerCommand<CmdOnNeutral>(cmdname_OnNeutral, myQuery, pool, cont);
            case cmd_OnFriendly:
                return CommandsUtils.createControllerCommand<CmdOnFriendly>(cmdname_OnFriendly, myQuery, pool, cont);
            case cmd_OnDeath:
                return CommandsUtils.createControllerCommand<CmdOnDeath>(cmdname_OnDeath, myQuery, pool, cont);
            case cmd_OnLostContact:
                return CommandsUtils.createControllerCommand<CmdOnLostContact>(cmdname_OnLostContact, myQuery, pool, cont);
            default:
                {
                    Debug.Log(string.Format("Code {0} not found in {1}", name.ToString("X8"),this.GetType().ToString()));
                    return null;
                }
        }
    }

    public override IVmController createController(crc32 name)
    {
        Asserts.Assert(myPrevFactory == null);
        switch (name)
        {
            case cmd_OnMessage: return Controllers.createCountController();
            case cmd_OnContact: return Controllers.createInfiniteController();
            case cmd_OnAppear: return Controllers.createOnceController();
            case cmd_OnEngage: return Controllers.createInfiniteController();
            case cmd_OnNeutral: return Controllers.createOnceController();
            case cmd_OnFriendly: return Controllers.createOnceController();
            case cmd_OnLostContact: return Controllers.createCountController();
            case cmd_OnDeath: return Controllers.createCountController();
            default: return null;
        }

    }

    //public static IVmFactory createStdGroupFactory(IQuery qr)
    //{
    //    IVmFactory fake = null;
    //    return new StdGroupFactory(qr, fake);
    //}

}
