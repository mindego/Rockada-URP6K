
/// <summary>
/// iRadioEnvironment - интерфейс казалки и говорилки радиоокружения
/// </summary>
public interface iRadioEnvironment
{
    public void AddRadioMessage(string pFormat, RadioMessage pData);
    public bool IsRadioFree();
    public IWave CreateWave(int DbIndex, string pWaveName);
    public bool PlayWave(int DbIndex, string pName, int Volume);
    public bool PlayWave(IWave pPhrase, int Volume);
    public IWave CreatePhrase(int DbIndex, string pText);
    public bool PlayPhrase(int DbIndex, string pText, int Volume);
};
