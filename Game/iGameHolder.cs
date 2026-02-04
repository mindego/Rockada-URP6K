
/// <summary>
/// для работы с игрой
/// </summary>
public interface iGameHolder
{
    // general
    public void Open(string pCmpName, string pMsnName);
    public void Host(string pCmpName, string pMsnName, int nPort, bool dedicated);
    public void Connect(string pAddress);
    public bool IsDataReady();
    public void Close();
    public void Update(float Scale, bool UseControls);
    public bool Draw(float[] ViewPort);
    public string GetError();
    // data access
    public iUnifiedVariableString GetBriefingText();
    public iUnifiedVariableString GetBriefingCommands();
    public iUnifiedVariableString GetMissionDescription();
    public iUnifiedVariableString GetDebriefingOnSuccessText();
    public iUnifiedVariableString GetDebriefingOnSuccessCommands();
    public iUnifiedVariableArray GetSuccessorsOnSuccess();
    public iUnifiedVariableString GetDebriefingOnFailureText();
    public iUnifiedVariableString GetDebriefingOnFailureCommands();
    public iUnifiedVariableArray GetSuccessorsOnFailure();
    // game
    public bool IsGameReady();
    public void OpenGame(iUnifiedVariableDB pLocDb, iUnifiedVariableDB pVarDb);
    public bool IsInGame();
    public bool ShouldDrawGame();
    public void CloseGame();

    // line editing
    //public iLineEditor* getLineEditor();
    //public LogListHolder<>* getRadioLog();
};



