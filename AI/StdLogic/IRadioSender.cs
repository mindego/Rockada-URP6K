using UnityEngine;

public struct CallerInfo
{
    public string myCallsign;
    public int myIndex;
};

public interface IRadioSender
{
    public const uint ID = 0x6DAC0634;
    public bool getOrg(out Vector3 v);
    public CallerInfo getInfo() ;
    public IAi getAi() ;
    public int getSide();
    public int getType();
    public IErrorLog getLog();
    public int getVoice();
    public bool isPlayable();
};

