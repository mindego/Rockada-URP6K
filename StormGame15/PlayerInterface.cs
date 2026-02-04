using DWORD = System.UInt32;
using static RadioMessage;
using static VoicesCount;
using UnityEngine;
using static iSensorsDefines;
/// <summary>
/// PlayerInterface - базовый класс для управления различными типами юнитов
/// </summary>
public class PlayerInterface : iRadioEnvironment, iBaseInterface, CommLink
{
    public const int MAX_AI_MENU_ITEMS = 9;
    public const int MAX_OBJECTIVES_LENGTH = 64;

    public const string sClPrev = "cl_target_prev";
    public const uint iClPrev = 0x1E23A51A;
    public const string sClNext = "cl_target_next";
    public const uint iClNext = 0xA6EE3D73;
    public const string sClRadio = "cl_radio";
    public const uint iClRadio = 0x5313F76F;
    public const string sClRadioCommand = "cl_radio_command";
    public const uint iClRadioCommand = 0xCD43A7AE;

    const string sClRadiologMode = "cl_radiolog_mode";
    const uint iClRadiologMode = 0xB16505D9;
    const string sClRadiologDelay = "cl_radiolog_delay";
    const uint iClRadiologDelay = 0xDEB5D044;
    const float time_to_wait_ous = 10f;

    // от iRadioEnvironment
    public virtual void AddRadioMessage(string pFormat, RadioMessage pData)
    {
        string pBuffer = ""; // [1024];
                             // переводим текст
        if (mrScene.AddLocalizedMessage(ref pBuffer, pFormat, pData, false) == null) pBuffer = "NO_MSG";
        Debug.Log(string.Format("pBuffer [{0}] pFormat [{1}] {2}", pBuffer,pFormat, pData.String1));
        // добавляем в радио-лог
        ProcessMessageBuffer(pData.mFlags, pBuffer);
        // переводим звук
        if ((pData.mFlags & RMF_SAY) != 0)
        {
            if (pFormat == null && mrScene.AddLocalizedMessage(ref pBuffer, pFormat, pData, true) != null)
            {
                if (PlayPhrase(pData.VoiceCode, pBuffer, mrScene.GetSceneVisualizer().GetSceneConfig().s_vradio)) return;
                mrScene.Message("Failed to process \"{0}\" with voice {1}", pBuffer, pData.VoiceCode.ToString()); ;
            }
            // сообщаем об ошибке
            PlayWave(mpRadioMessageSound, mrScene.GetSceneVisualizer().GetSceneConfig().s_vradio);
        }
    }
    public virtual bool IsRadioFree()
    {
        return (mpCurrentPhrase == null);
    }
    public virtual IWave CreateWave(int DbIndex, string WaveName)
    {
        if (DbIndex < 0 || DbIndex >= gMaxRadioVoices) return null;
        if (mpVoiceDBs[DbIndex] == null) return null;
        Debug.Log("Creating wave \"" + WaveName + "\" Voice " + DbIndex);
        return mpVoice.CreateWave(mpVoiceDBs[DbIndex], WaveName);
    }
    public virtual bool PlayWave(int DbIndex, string pName, int Volume)
    {
        if (mpCurrentPhrase != null)
        {
            mpCurrentPhrase.Stop();
            mpCurrentPhrase.Release();
        }
        mpCurrentPhrase = CreateWave(DbIndex, pName);
        if (mpCurrentPhrase == null) return false;
        mpCurrentPhrase.Play(false, Volume);
        return true;
    }
    public virtual bool PlayWave(IWave pPhrase, int Volume)
    {
        if (pPhrase == null) return false;
        if (mpCurrentPhrase != null)
        {
            mpCurrentPhrase.Stop();
            mpCurrentPhrase.Release();
        }
        mpCurrentPhrase = pPhrase;
        mpCurrentPhrase.AddRef();
        mpCurrentPhrase.Play(false, Volume);
        return true;
    }
    public virtual IWave CreatePhrase(int DbIndex, string Text)
    {
        Debug.Log("Creating phrase " + Text);
        if (DbIndex < 0 || DbIndex >= gMaxRadioVoices) return null;
        if (mpVoiceDBs[DbIndex] == null) return null;
        return mpVoice.CreatePhrase(mpVoiceDBs[DbIndex], Text);
    }
    public virtual bool PlayPhrase(int DbIndex, string pText, int Volume)
    {
        Debug.Log(string.Format("Playing Phrase voice index {0} {1} vol {2}",DbIndex,pText,Volume));
        if (mpCurrentPhrase != null)
        {
# if _DEBUG
            mrScene.Message("PlayPhrase::Violation in \"{0}\"!", pText);
#endif
            mpCurrentPhrase.Stop();
            mpCurrentPhrase.Release();
            mpCurrentPhrase = null;
        }
        //if (pText && !*pText) return true;
        if (pText == null) return true;
        mpCurrentPhrase = CreatePhrase(DbIndex, pText);
        if (mpCurrentPhrase == null) return false;
        mpCurrentPhrase.Play(false, Volume);
        return true;
    }

    // от iBaseInterface
    public const uint ID = 0xE928AB9F;
    public virtual object GetInterface(DWORD id)
    {
        return (id == ID ? this : null);
    }


    // от CommLink
    public virtual void OnTrigger(uint code, bool on) { }
    public virtual void OnCommand(uint code, string arg1, string arg2)
    {
        switch (code)
        {//TODO - возможно, стоит приводить к lowercase
            case iClRadiologMode:
                if (arg1 == "objectives")
                {
                    mRadioDeviceFixed = false;
                    SetRadioLog(mpObjectivesLog);
                    mRadioDeviceFixed = true;
                    return;
                }
                if (arg1 == "messages")
                {
                    mRadioDeviceFixed = false;
                    SetRadioLog(mpRadioLog);
                    mRadioDeviceFixed = true;
                    return;
                }
                if (arg1 == "everything")
                {
                    mRadioDeviceFixed = false;
                    return;
                }
                mrScene.Message("wrong argument");
                return;
            case iClNext:
                mpCameraData.SetRef(NextTarget(mpCameraData.GetRef()));
                return;
            case iClPrev:
                mpCameraData.SetRef(PrevTarget(mpCameraData.GetRef()));
                return;
            case iClRadio:
                {
                    myMenuTab--;
                    if (myMenuTab < 0) myMenuTab = 2;
                    if (mnMenuItems != 0)
                        DeleteMenu();
                    if (myMenuTab != 2)
                        StartMenu(-1);
                }
                return;
            case iClRadioCommand:
                {
                    //int i = atoi(arg1) - 1;
                    int i = int.Parse(arg1) - 1;
                    if (mpMenuDevice != null)
                    { // если в меню - разбор команды
                        if (i >= 0 && i < mnMenuItems && mpMenuItems[i].myEnabled)
                        {
                            DeleteMenu();
                            OnMenuItem(mpMenuItems[i].MenuID);
                        }
                    }
                    else
                        onMenuShortcut(i);
                }
                break;
            default:
# if _DEBUG
                _asm int 3;
#endif
                return;
        }
    }
    public virtual object OnVariable(uint code, object data)
    {
        switch (code)
        {
            case iClRadiologDelay:
                if (data != null)
                {
                    mRadioDeviceDelay = Mathf.Clamp((float)data, .0f, 30f);
                    SetRadioLog(mpRadioDeviceLog);
                }
                return mRadioDeviceDelay;
            default:
                Asserts.Assert(false);
                return null;
        }
    }

    // API
    public PlayerInterface(BaseScene s, iContact p)
    {
        mrScene = s;
        mpRadioMessageSound = null;
        mpObjectivesUpdatedSound = null;
        mpCurrentPhrase = null;
        mpObjectivesLog = null;
        mpRadioLog = null;
        //mpRadioDevice = null;
        mpRadioDevice = new DebugLogRadio(new HUDTree());//TODO Убрать после реализации приёмника
        mpRadioDeviceLog = null;
        mRadioDeviceFixed = false;
        mRadioDeviceTime = 0;
        mRadioDeviceDelay = 5f;
        //mPlayer = new TContact(p);
        mPlayer.setPtr(p);
        mpMenuDevice = null;
        mnMenuItems = 0;
        mOldFps = .0f;
        // mpCollider = null;
        mObjectivesStateTime = (-1);
        mMenuState = (0);
        mNeedToSayMenuState = false;
        //mCurrentCode = Constants.THANDLE_INVALID;
        mCurrentCode = -1;
        mpMenuLog = null;
        myTimeToForceOUS = 0;

        mpVoice = null;
    }

    ~PlayerInterface()
    {
        Dispose();
    }

    private bool IsDisposed = false;
    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
        Debug.Log("Clearing PlayerInterface instance " + this);
        // очищаем параметры контакта
        if (mPlayer.Ptr() != null)
        {
            BaseObject pObj = (BaseObject)mPlayer.Ptr().GetInterface(BaseObject.ID);
            if (pObj != null) pObj.SetPlayedByHuman(false);
        }
        // удаляем свои команды
        Debug.Log("Удаляем свои команды PlayerInterface: " + mrScene.GetCommandsApi());
        mrScene.GetCommandsApi().UnRegister(this);
        // освобожнаем Collider
        if (mpCollider != null) mpCollider.Release();
        // удаляем fps
        //if (mpFpsMeter != null) mrScene.GetSceneVisualizer().GetHud().ReleaseDevice(mpFpsMeter); //TODO Восстанеовить после реализации
        // удаляем меню
        DeleteMenu();
        // удаляем радиолог
        //if (mpRadioDevice != 0) mrScene.GetSceneVisualizer()->GetHud()->ReleaseDevice(mpRadioDevice); //TODO Восстанеовить после реализации
        // отпускаем objectives и радиосообщения
        mpObjectivesLog = null;
        mpRadioLog = null;
        // отпускаем menulog
        mpMenuLog = null;
        // удаляем звук(и)
        //TODO восстановить очистку звуков.
        //mpCurrentPhrase.Release();
        //mpRadioMessageSound.Release();
        //mpObjectivesUpdatedSound.Release(); 
        // удаляем базы
        //for (int i = 0; i != gMaxRadioVoices; ++i)
        //    mpVoiceDBs[i].Release();
        // удаляем звук
        //mpVoice.Release();

    }
    public virtual void Init(PlayerInterface pPrev)
    {
        Debug.Log("mPlayer " + mPlayer);
        //Реализовать инициализацию интерфейса пользователя
        if (pPrev == null)
        {
            InitNew();
        }
        else
        {
            InitOld(pPrev);
        }

        // создаем Collider (если надо)
        if (mPlayer.Ptr() != null)
        {
            iBaseColliding pColl = (iBaseColliding)mPlayer.Ptr().GetInterface(iBaseColliding.ID);
            if (pColl != null) mpCollider = new BaseColliderObjects(mrScene, pColl);
        }
        // регистрируем всякие команды
        if (mPlayer.Ptr() == null)
        {
            mrScene.GetCommandsApi().RegisterCommand(sClPrev, this, 0, "select next target");
            mrScene.GetCommandsApi().RegisterCommand(sClNext, this, 0, "select previous target");
        }
        mrScene.GetCommandsApi().RegisterCommand(sClRadio, this, 0, "show/hide AI COMMANDS menu");
        mrScene.GetCommandsApi().RegisterCommand(sClRadioCommand, this, 1, "select item from AI COMMANDS, either by index or by name");
        mrScene.GetCommandsApi().RegisterCommand(sClRadiologMode, this, 1, "set radio log window behavior");
        mrScene.GetCommandsApi().RegisterVariable(sClRadiologDelay, this, VType.VAR_FLOAT, "radio log window hide delay (0=no hide)");

        // инитим параметры контакта
        if (mPlayer.Ptr() != null)
        {
            BaseObject pObj = (BaseObject)mPlayer.Ptr().GetInterface(BaseObject.ID);
            Asserts.AssertEx(pObj != null);
            pObj.SetPlayedByHuman(true);
        }
    }

    private void moveValue<T>(ref T from,ref T to) where T:class
    {
        to = from;
        from = null;
    }
    private void InitOld(PlayerInterface pPrev)
    {
        //moveValue(ref pPrev.mpVoice, ref mpVoice);
        mpVoice = pPrev.mpVoice;

        for (int i = 0; i != gMaxRadioVoices; ++i)
            //moveValue(ref pPrev.mpVoiceDBs[i],ref mpVoiceDBs[i]);
            mpVoiceDBs[i] = pPrev.mpVoiceDBs[i];

        //moveValue( ref pPrev.mpRadioMessageSound, ref mpRadioMessageSound);
        //moveValue( ref pPrev.mpObjectivesUpdatedSound, ref mpObjectivesUpdatedSound);
        //moveValue( ref pPrev.mpCurrentPhrase, ref mpCurrentPhrase);
        mpRadioMessageSound = pPrev.mpRadioMessageSound;
        mpObjectivesUpdatedSound = pPrev.mpObjectivesUpdatedSound;
        mpCurrentPhrase = pPrev.mpCurrentPhrase;
        // забираем с objectives и радио-сообщениями
        //moveValue( ref pPrev.mpRadioLog, ref mpRadioLog);
        //moveValue( ref pPrev.mpObjectivesLog, ref mpObjectivesLog);
        mpRadioLog = pPrev.mpRadioLog;
        mpObjectivesLog = pPrev.mpObjectivesLog;
        // забираем очко радиолога
        //moveValue( ref pPrev.mpRadioDevice, ref mpRadioDevice);
        //moveValue( ref pPrev.mpRadioDeviceLog, ref mpRadioDeviceLog);
        //moveValue( ref pPrev.mRadioDeviceFixed, ref mRadioDeviceFixed);
        //moveValue( ref pPrev.mRadioDeviceTime, ref mRadioDeviceTime);
        //moveValue( ref pPrev.mRadioDeviceDelay, ref mRadioDeviceDelay);
        mpRadioDevice = pPrev.mpRadioDevice;
        mpRadioDeviceLog = pPrev.mpRadioDeviceLog;
        mRadioDeviceFixed = pPrev.mRadioDeviceFixed;
        mRadioDeviceTime = pPrev.mRadioDeviceTime;
        mRadioDeviceDelay = pPrev.mRadioDeviceDelay;

        // забираем menu log
        //moveValue( ref pPrev.mpMenuLog, ref mpMenuLog);
        mpMenuLog = pPrev.mpMenuLog;
        // забираем FPS-meter
        //moveValue( ref pPrev.mpFpsMeter, ref mpFpsMeter);
        //moveValue( ref pPrev.mOldFps, ref mOldFps);
        mpFpsMeter = pPrev.mpFpsMeter;
        mOldFps = pPrev.mOldFps;
        // забираем CameraData
        //moveValue( ref pPrev.mpCameraData, ref mpCameraData);
        //moveValue( ref pPrev.mObjectivesStateTime, ref mObjectivesStateTime);
        //moveValue( ref pPrev.mMenuState, ref mMenuState);

        mpCameraData = pPrev.mpCameraData;
        mObjectivesStateTime = pPrev.mObjectivesStateTime;
        mMenuState = pPrev.mMenuState;

        // копируем строки
        mpStrObjectivesPrimary= pPrev.mpStrObjectivesPrimary;
        mpStrObjectivesSecondary= pPrev.mpStrObjectivesSecondary;
        mpStrObjectivesStatus= pPrev.mpStrObjectivesStatus;
        mpStrObjectivesFailed= pPrev.mpStrObjectivesFailed;
        mpStrObjectivesInProgress= pPrev.mpStrObjectivesInProgress;
        mpStrObjectivesCompleted= pPrev.mpStrObjectivesCompleted;
        mpInFlightMenuTitle= pPrev.mpInFlightMenuTitle;
        myBattleMenuTitle= pPrev.myBattleMenuTitle;
    }
    private void InitNew()
    {
        {
            // создаем звук
            mpVoice = mrScene.GetSoundApi().CreateVoice();
            if (mpVoice != null)
            {
                // открываем базы
                for (int i = 0; i < gMaxRadioVoices; i++)
                {
                    string voice_name = mrScene.getGameset().getVoice(i);
                    mpVoiceDBs[i] = voice_name != null ? mpVoice.OpenVoiceDB(voice_name) : null;
                    if (voice_name != null && mpVoiceDBs[i] == null)
                        mrScene.Message("Error : can't open voice DB \"{0}\"!", voice_name);
                }
                // создаем звуки
                mpRadioMessageSound = CreateWave(0, "RadioMessage");
                mpObjectivesUpdatedSound = CreateWave(0, "ObjectivesUpdated");
            }
            else
            {
                // обнуляем базы
                for (int i = 0; i != gMaxRadioVoices; ++i)
                    mpVoiceDBs[i] = null;
            }
            // создаем menu log
            mpMenuLog = new LogListHolder("Menu", 2, 32);
            // создаем objectives и радио-сообщениями
            mpRadioLog = new LogListHolder("radio", 2, 32); mpRadioLog.Clear();
            Debug.Log("Creating mpObjectivesLog");
            mpObjectivesLog = new LogListHolder("objectives", 2, 32); mpObjectivesLog.Clear();
            mpRadioDeviceLog = mpRadioLog;
            //TODO Восстановить создание девайсов HUDа по их реализации
            // создаем радиолог
            //mpRadioDevice = mrScene.GetSceneVisualizer().GetHud().CreateLog(iRadio, mpRadioDeviceLog.getList());
            //mpRadioDevice.Hide(true);
            // создаем FPS-meter
            //mpFpsMeter = mrScene.GetSceneVisualizer()->GetHud()->CreateFPSmeter(iFPS);
            // получаем CameraData
            mpCameraData = (CameraData)mrScene.GetBaseData(CameraData.CAMERA_DATA);
            // локализуем строки
            if (mrScene.AddLocalizedText(ref mpStrObjectivesPrimary, 64, 0, "mc_ObjectivesPrimary") == null) mpStrObjectivesPrimary = "mc_ObjectivesPrimary";
            if (mrScene.AddLocalizedText(ref mpStrObjectivesSecondary, 64, 0, "mc_ObjectivesSecondary") == null) mpStrObjectivesSecondary = "mc_ObjectivesSecondary";
            if (mrScene.AddLocalizedText(ref mpStrObjectivesStatus, 64, 0, "mc_ObjectivesStatus") == null) mpStrObjectivesStatus = "mc_ObjectivesStatus";
            if (mrScene.AddLocalizedText(ref mpStrObjectivesFailed, 64, 0, "mc_ObjectivesFailed") == null) mpStrObjectivesFailed = "mc_ObjectivesFailed";
            if (mrScene.AddLocalizedText(ref mpStrObjectivesInProgress, 64, 0, "mc_ObjectivesInProgress") == null) mpStrObjectivesInProgress = "mc_ObjectivesInProgress";
            if (mrScene.AddLocalizedText(ref mpStrObjectivesCompleted, 64, 0, "mc_ObjectivesCompleted") == null) mpStrObjectivesCompleted = "mc_ObjectivesCompleted";
            if (mrScene.AddLocalizedText(ref mpInFlightMenuTitle, 64, 0, "mf_Title") == null) mpInFlightMenuTitle = "mf_Title";
            if (mrScene.AddLocalizedText(ref myBattleMenuTitle, 64, 0, "mb_Title") == null) myBattleMenuTitle = "mb_Title";
            //dprintf("mpInFlightMenuTitle=%s\n",mpInFlightMenuTitle);
            //dprintf("myBattleMenuTitle=%s\n",myBattleMenuTitle);
            //TODO реализовать очистку заголовков меню от.. эээ.. переносов строки?
            //prepareMenuCaption(mpInFlightMenuTitle);
            //prepareMenuCaption(myBattleMenuTitle);
        }
    }
    public virtual void Process(float scale)
    {
        // проверяем живость игрока
        mPlayer.Validate();
        // если игрок умер - отпускаем mpCollider 
        if (mPlayer.Ptr() == null && mpCollider != null)
        {
            mpCollider.Release();
            mpCollider = null;
        }

        if (mPlayer.Ptr() != null && myTimeToForceOUS != 0)
        {
            if (myTimeToForceOUS < 0 || IsRadioFree())
            {
                playObjectivesUpdated();
                myTimeToForceOUS = 0;
            }
        }

        // апдейты
        ProcessFps(scale);
        ProcessPhrases(scale);
        ProcessRadioLog(scale);
    }

    public iContact GetBaseContact() { return mPlayer.Ptr(); }

    // voices
    private IVoice mpVoice;

    private IObject[] mpVoiceDBs = new IObject[VoicesCount.gMaxRadioVoices];

    private IWave mpRadioMessageSound;
    private IWave mpObjectivesUpdatedSound;
    private IWave mpCurrentPhrase;
    protected IWave GetRadioMessageSound() { return mpRadioMessageSound; }
    protected IWave GetObjectivesUpdatedSound() { return mpObjectivesUpdatedSound; }
    void ProcessPhrases(float scale)
    {
        if (mpCurrentPhrase != null && mpCurrentPhrase.IsPlaying() == false)
        {
            mpCurrentPhrase.Release();
            //Debug.Log("Clearing phrase " + mpCurrentPhrase);
            mpCurrentPhrase = null;
        }
        // проверяем необходимость сказать об обновлении меню
        if (mNeedToSayMenuState == true && IsRadioFree() == true)
        {
            PlayWave(0, "MenuUpdated", mrScene.GetSceneVisualizer().GetSceneConfig().s_vhudspeach);
            mNeedToSayMenuState = false;
        }
    }

    // очко радиолога
    private LogListHolder mpRadioLog;
    private LogListHolder mpObjectivesLog;
    private LogListHolder mpRadioDeviceLog;
    HudDevice mpRadioDevice;
    bool mRadioDeviceFixed;
    float mRadioDeviceTime;
    float mRadioDeviceDelay;
    void ProcessMessageBuffer(DWORD flags, string pBuffer)
    {
        if ((flags & RMF_CONSOLE) != 0)
            mrScene.getChatLog().Message("{0}", pBuffer);
        else if ((flags & RMF_LOG) != 0)
        {
            mpRadioLog.getLog().Message(pBuffer);
            SetRadioLog(mpRadioLog);
        }
    }

    protected LogListHolder GetObjectivesLog() { return mpObjectivesLog; }
    void ProcessRadioLog(float scale)
    {
        if (mRadioDeviceDelay <= .0f) return;
        if (mRadioDeviceTime > mrScene.GetTime()) return;
        mpRadioDevice.Hide(true);
    }

    protected void SetRadioLog(LogListHolder log)
    {
        if (mRadioDeviceFixed == true)
        {
            if (mpRadioDeviceLog != log) return;
        }
        mpRadioDeviceLog = log;
        //TODO Восстановить работу радиолога
        //HudLogData d = (HudLogData) mpRadioDevice.GetData();
        //d.Log = log.getList();
        //d.fifo = (mpRadioDeviceLog == mpObjectivesLog);
        //UpdateRadioDevice();
    }
    void UpdateRadioDevice()
    {
        mpRadioDevice.Hide(false);
        //HudLogData d = (HudLogData)mpRadioDevice.GetData();
        //d.changed = true;
        mRadioDeviceTime = mrScene.GetTime() + mRadioDeviceDelay;
    }


    // меню
    private LogListHolder mpMenuLog;
    private HudDevice mpMenuDevice;
    private HudDevice mpFpsMeter;
    //private LineEditDispatcher mpLineEditor;
    private float mOldFps;
    protected int mnMenuItems;
    protected AiMenuItem[] mpMenuItems = new AiMenuItem[MAX_AI_MENU_ITEMS];
    protected int mCurrentCode;
    protected void ShowMenu()
    {
        // заполняем лог
        mpMenuLog.Clear();
        mpMenuLog.getLog().SetIdent(0);
        mpMenuLog.getLog().Message(myMenuTab == 1 ? myBattleMenuTitle : mpInFlightMenuTitle);
        mpMenuLog.getLog().IncIdent();
        for (int i = 0; i < mnMenuItems; i++)
        {
            //char pBuffer[1024];
            string pBuffer = "";
            if (mrScene.AddLocalizedMessage(ref pBuffer, mpMenuItems[i].pFormat, mpMenuItems[i].mMessage, false, false) == null)
                pBuffer = "NO_MSG";
            mpMenuLog.getLog().Message("{0}{1}: {2}", mpMenuItems[i].myEnabled ? "" : "^F", i + 1, pBuffer);
        }
        // создаем прибор
        if (mpMenuDevice == null)
        {
            //TODO Восстановить создание прибора ведения лога
            //mpMenuDevice = mrScene.GetSceneVisualizer().GetHud().CreateLog(iAiMenu, mpMenuLog.getList());
        }
    }

    protected virtual void StartMenu(int Code)
    {
        mCurrentCode = Code;
        if (mMenuState == 2) SetMenuState(1);
    }
    public virtual void DeleteMenu()
    {
        if (mpMenuDevice != null) { mrScene.GetSceneVisualizer().GetHud().ReleaseDevice(mpMenuDevice); mpMenuDevice = null; }
        mnMenuItems = 0;
    }
    public virtual void OnMenuItem(uint Code) { }
    public virtual void onMenuShortcut(int index) { }
    public void ProcessFps(float scale)
    {
        //if (scale > .0f) mOldFps += (1.f / scale - mOldFps) * .1f;
        //HudStringData* hsd = (HudStringData*)mpFpsMeter->GetData();
        //Sprintf(hsd->text, "%02d", (int)mOldFps);
        //hsd->colour = (mOldFps < 12.f ? HUDCOLOR_DANGER : HUDCOLOR_NORMAL);
    }
    public virtual void OnMenuUpdate(bool HasNewItem)
    {
        if (HasNewItem && mrScene.GetTimeScale() > 1)
            mrScene.SetTimeScale(1);
        SetMenuState(HasNewItem ? 2 : 0);
        if (IsMenuOnScreen() == true) StartMenu(mCurrentCode);
    }
    public bool IsMenuOnScreen() { return (mpMenuDevice != null); }

    // API for CameraLogicCockpit
    public int GetObjectivesState()
    {
        if (mObjectivesStateTime < 0) return 0;
        return (mObjectivesStateTime < mrScene.GetTime() ? 1 : 2);
    }
    public int GetMenuState() { return mMenuState; }
    public void needPlayObjectiveUpdated() { myTimeToForceOUS = time_to_wait_ous; }
    public void playObjectivesUpdated()
    {
        IWave ous;
        if ((ous = GetObjectivesUpdatedSound()) != null)
            ous.Play(false, mrScene.GetSceneVisualizer().GetSceneConfig().s_vhudspeach);
    }
    // свое
    protected BaseScene mrScene;
    protected CameraData mpCameraData;
    protected BaseColliderObjects mpCollider;
    protected readonly TContact mPlayer = new TContact();
    protected float mObjectivesStateTime;
    protected int mMenuState;
    protected int myMenuTab = 2;
    protected bool mNeedToSayMenuState;
    protected string mpStrObjectivesPrimary; //[MAX_OBJECTIVES_LENGTH];
    protected string mpStrObjectivesSecondary; //[MAX_OBJECTIVES_LENGTH];
    protected string mpStrObjectivesStatus; //[MAX_OBJECTIVES_LENGTH];
    protected string mpStrObjectivesFailed; //[MAX_OBJECTIVES_LENGTH];
    protected string mpStrObjectivesInProgress; //[MAX_OBJECTIVES_LENGTH];
    protected string mpStrObjectivesCompleted; //[MAX_OBJECTIVES_LENGTH];
    protected string mpInFlightMenuTitle; //[MAX_OBJECTIVES_LENGTH];
    protected string myBattleMenuTitle; //[MAX_OBJECTIVES_LENGTH];

    //TODO Нужно где-то в кучку сложить константы:
    //#define THANDLE_INVALID     0xFFFFFFFF
    //#define THANDLE_POS_MASK    0x0000FFFF
    //#define THANDLE_FRAME_MASK  0xFFFF0000
    //#define THANDLE_FRAME_UNIT  0x00010000

    public const DWORD THANDLE_INVALID = 0xFFFFFFFF;
    public const DWORD THANDLE_POS_MASK = 0x0000FFFF;
    public const DWORD THANDLE_FRAME_MASK = 0xFFFF0000;
    public const DWORD THANDLE_FRAME_UNIT = 0x00010000;

    // PlayerInterface - взаимодействие со сценой
    protected DWORD PrevTarget(DWORD hRef)
    {
        int pos = (int) (hRef & THANDLE_POS_MASK);
        if (hRef == THANDLE_INVALID)
        {
            IBaseItem i = mrScene.GetItemsArray()[pos = (int)mrScene.GetItemsArray().GetSize() - 1];
            if (i!=null) return i.GetHandle();
        }
        bool over = false;
        int old_pos = pos;
        while (true)
        {
            pos--;
            if (pos < 0) { pos = (int)mrScene.GetItemsArray().GetSize() - 1; over = true; }
            if (pos == old_pos && over) return THANDLE_INVALID;
            IBaseItem i = mrScene.GetItemsArray()[pos];
            if (i==null) continue;
            iContact u = (iContact) i.GetInterface(iContact.ID);
            if (u==null || u.GetState() == CS_DEAD || u.GetHandle() != i.GetHandle()) continue;
            //return mrScene.GetItemsArray().GetHandle(pos);
            return u.GetHandle();
        }
    }
    protected DWORD NextTarget(DWORD hRef)
    {
        return 0;
    }
    protected void SetMenuState(int s)
    {
        mMenuState = s;
        mNeedToSayMenuState = (mMenuState == 2);
    }
    private float myTimeToForceOUS;
};

public static class ClientDeviceFactory
{
    public static iClientDevice CreateClientDevice(SceneVisualizer pVis, ClientDeviceDataCreate data)
    {
        iClientDevice myobject = null;
        //bool init_res = false;

        //switch (data->mDeviceType)
        //{
        //    case CLIENT_DEVICE_TABLE:
        //        object = CreateClientDeviceTable(pVis, data);
        //        init_res = object;
        //        break;
        //    case CLIENT_DEVICE_LABEL:
        //        {
        //            ClientDeviceLabel* label = new SRefMem<ClientDeviceLabel>;
        //            init_res = label->Initialize(pVis, data);
        //            object  = label;
        //        }
        //        break;
        //    case CLIENT_DEVICE_CONFIG:
        //        {
        //            ClientDeviceConfigController* label = new SRefMem<ClientDeviceConfigController>;
        //            init_res = label->Initialize(pVis, data);
        //            object  = label;
        //        }
        //        break;
        //    case CLIENT_DEVICE_TIMELEFT:
        //        {
        //            ClientDeviceTimeLeft* label = new SRefMem<ClientDeviceTimeLeft>;
        //            init_res = label->Initialize(pVis, data);
        //            object  = label;
        //        }
        //        break;
        //    case CLIENT_DEVICE_CLINFO:
        //        object = createClientDeviceClientInfo(pVis);
        //        init_res = object;
        //        break;
        //    case CLIENT_DEVICE_PRINTER:
        //        object = CreateClientDevicePrinter(pVis);
        //        init_res = object;
        //        break;
        //    case CLIENT_DEVICE_MENU:
        //        object = CreateClientDeviceMenu(pVis);
        //        init_res = object;
        //        break;
        //}
        //if (object && false == init_res)
        //{
        //    object->Release();
        //    object= 0;
        //}
        return myobject;
    }
}

public class DebugLogRadio : HudDevice
{
    private bool hidden = false;
    public DebugLogRadio(HUDTree _Dev) : base(_Dev)
    {

    }

    public override void Hide(bool off)
    {
        if (hidden != off)
        {
            Debug.Log(string.Format("Switching hidden from {0} to {1}", hidden, off));
            hidden = off;
        }
    }

    public override HudDeviceData GetData()
    {
        return default;
    }
}