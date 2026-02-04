using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// PlayerInterfaceLocal - класс для интерфейса между игрой и игроком на хосте
/// </summary>
class PlayerInterfaceLocal : PlayerInterface
{

    // от iBaseInterface
    new public const uint ID = 0xFCEB21C9;
    public override object GetInterface(DWORD id)
    {
        switch (id) // так нагляднее и чаще используется в других аналогичных методах
        {
            case ID: return this;
            default: return base.GetInterface(id);
        }
        //return (id == ID ? this : PlayerInterface::GetInterface(id));
    }

    // от iRadioEnvironment
    public override void AddRadioMessage(string pFormat, RadioMessage pData)
    {
        if (mrScene.GetTimeScale() > 1) mrScene.SetTimeScale(1);
        base.AddRadioMessage(pFormat, pData);
    }

    // от PlayerInterface
    public PlayerInterfaceLocal(BaseScene s, iContact p) : base(s, p)
    {
        mObjectivesLastCode = 0;
        mObjectivesNextTime = -1f;
        mpPrimaryObjectives = null;
        mpSecondaryObjectives = null;
        mpOthersObjectives = null;
    }
    ~PlayerInterfaceLocal()
    {
        mpPrimaryObjectives.Release();
        mpSecondaryObjectives.Release();
        mpOthersObjectives.Release();
    }
    public override void Init(PlayerInterface p)
    {
        base.Init(p);
        PlayerInterfaceLocal pPrev = (p != null ? (PlayerInterfaceLocal)p.GetInterface(PlayerInterfaceLocal.ID) : null);
        if (pPrev != null)
        {
            { mObjectivesLastCode = pPrev.mObjectivesLastCode; pPrev.mObjectivesLastCode = 0; };
            { mObjectivesNextTime = pPrev.mObjectivesNextTime; pPrev.mObjectivesNextTime = 0; };
            { mpPrimaryObjectives = pPrev.mpPrimaryObjectives; pPrev.mpPrimaryObjectives = null; };
            { mpSecondaryObjectives = pPrev.mpSecondaryObjectives; pPrev.mpSecondaryObjectives = null; };
            { mpOthersObjectives = pPrev.mpOthersObjectives; pPrev.mpOthersObjectives = null; };
        }
        else
        {
            iUnifiedVariableContainer pRoot = mrScene.getCurrentEventData();
            if (pRoot != null)
            {
                mpPrimaryObjectives = pRoot.createContainer("PrimaryObjectives");
                mpSecondaryObjectives = pRoot.createContainer("SecondaryObjectives");
                mpOthersObjectives = pRoot.createContainer("OtherObjectives");
            }
        }
    }
    public override void Process(float scale)
    {
        base.Process(scale);
        if (mObjectivesNextTime <= mrScene.GetTime())
        {
            GetObjectivesLog().Clear();
            uint code = 0;
            bool IsInProgress = false;
            code ^= AddObjectives(mpPrimaryObjectives, mpStrObjectivesPrimary, ref IsInProgress);
            code ^= AddObjectives(mpSecondaryObjectives, mpStrObjectivesSecondary, ref IsInProgress);
            code ^= AddObjectives(mpOthersObjectives, mpStrObjectivesStatus, ref IsInProgress);
            if (mObjectivesLastCode != code)
            {
                mObjectivesLastCode = code;
                SetRadioLog(GetObjectivesLog());
                needPlayObjectiveUpdated();
                mObjectivesStateTime = (IsInProgress == true ? mrScene.GetTime() + 10 : -1);
            }
            mObjectivesNextTime = mrScene.GetTime() + 3;
        }
    }

    protected override void StartMenu(int Code)
    {
        base.StartMenu(Code);
        HostScene s = (HostScene)mrScene;
        mnMenuItems = s.GetMissionAI().GetMenuItems(s.GetLocalClient(), (uint)Code, mpMenuItems, MAX_AI_MENU_ITEMS, myMenuTab);
        if (mnMenuItems == 0) return;
        ShowMenu();
    }
    public override void OnMenuItem(uint Code)
    {
        // если выбрано - уведомляем pGroup
        HostScene s = (HostScene)mrScene;
        int id = s.GetMissionAI().SelectItem(s.GetLocalClient(), Code);
        if (id != -1)
            StartMenu(id);
        else
            myMenuTab = 2;
    }
    public override void onMenuShortcut(int index)
    {
        HostScene hc = (HostScene)mrScene;
        hc.onMenuShortcut(index, hc.GetLocalClient());
    }

    // own
    private uint mObjectivesLastCode;
    private float mObjectivesNextTime;
    private iUnifiedVariableContainer mpPrimaryObjectives;
    private iUnifiedVariableContainer mpSecondaryObjectives;
    private iUnifiedVariableContainer mpOthersObjectives;
    private DWORD AddObjectives(iUnifiedVariableContainer c, string title, ref bool IsInPprogress)
    {
        if (c == null) return 0;
        DWORD code = 0;
        DWORD handle = 0;
        while ((handle = c.GetNextHandle(handle)) != 0)
        {
            // определяем состояние
            iUnifiedVariableInt i = c.GetVariableTpl<iUnifiedVariableInt>(handle);
            if (i == null) continue;
            if (code == 0 && title != null)
            {
                //TODO Вернуть отображение заданий
                //GetObjectivesLog().getLog().Message(title);
                GetObjectivesLog().getLog().IncIdent();
            }
            // определяем имя
            //char pName[1024];
            string pName = "NO_OBJ "; //Возможно, внесение данных в имя нужно делать не так.
            c.GetNameByHandle(ref pName, handle);
            uint crc = Hasher.HshString(pName);
            if (mrScene.AddLocalizedText(ref pName, 1024, crc) == null) c.GetNameByHandle(ref pName, handle);
            // печатаем
            //TODO Вернуть отображение заданий
            //GetObjectivesLog().getLog().Message(pName); 
            code ^= crc;
            //TODO Вернуть отображение заданий
            switch (i.GetValue())
            {
                case -1: code ^= 0xBAADF00D; break;
                case 0:  code ^= 0xFFFF0000; IsInPprogress = true; break;
                case 1:  code ^= 0x0000FFFF; break;
                default: code ^= 0xFEEEFEEE; break;
            }
            //switch (i.GetValue())
            //{
            //    case -1: GetObjectivesLog().getLog().Append(mpStrObjectivesFailed); code ^= 0xBAADF00D; break;
            //    case 0: GetObjectivesLog().getLog().Append(mpStrObjectivesInProgress); code ^= 0xFFFF0000; IsInPprogress = true; break;
            //    case 1: GetObjectivesLog().getLog().Append(mpStrObjectivesCompleted); code ^= 0x0000FFFF; break;
            //    default: code ^= 0xFEEEFEEE; break;
            //}
            i.Release();
        }
        if (code != 0) GetObjectivesLog().getLog().DecIdent();
        return code;
    }
};

public class PlayerInterfaceRemote : PlayerInterface
{
    public PlayerInterfaceRemote(BaseScene s, iContact p) : base(s, p)
    {
    }
}