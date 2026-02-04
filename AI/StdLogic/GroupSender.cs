using UnityEngine;

public class GroupSender<T> : IRadioSender where T: StdGroupAi
{
    public virtual bool getOrg(out Vector3 org)
    {
        return myAi.getLeaderOrg(out org);
    }

    public virtual CallerInfo getInfo()
    {
        CallerInfo info = new CallerInfo
        {
            myCallsign = myAi.mpData.Callsign,
            myIndex = 0
        };
        return info;
    }

    public virtual IAi getAi()
    {
        return myAi.GetIAi();
    }

    public virtual int getSide()
    {
        return (int) myAi.mpData.Side;
    }

    public virtual int getType()
    {
        return 3;
    }

    public virtual IErrorLog getLog()
    {
        return myAi.getErrorLog();
    }

    public virtual int getVoice()
    {
        return myAi.getVoice();
    }

    public virtual bool isPlayable()
    {
        return myAi.IsPlayerLeader();
    }

    public GroupSender(T d)
    {
        myAi = d;
    }

    T myAi;
};
