using UnityEngine;

public class MissionSender<T> : IRadioSender where T: StdMissionAi
{
    public bool getOrg(out Vector3 org)
    {
        org = Vector3.zero;
        return true;
    }

    public  CallerInfo getInfo()
    {
        CallerInfo info = new CallerInfo
        {
            myCallsign = "",
            myIndex = 0
        };
        return info;
    }

    public IAi getAi()
    {
        return myAi;
    }

    public  int getSide()
    {
        return MissionSideDefines.SIDE_MISSION;
    }

    public  int getType()
    {
        return 4;
    }

    public  int getVoice()
    {
        return 0;
    }

    public bool isPlayable()
    {
        return false;
    }

    public IErrorLog getLog()
    {
        return myAi.getErrorLog();
    }

    public MissionSender(T ai)
    {
        myAi=(ai);
    }

    T myAi;
};
