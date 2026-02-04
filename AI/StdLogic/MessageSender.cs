public class RadioMessageInfo
{
    public const float DEFAULT_WAIT_TIME = 0.01f;
    public const float DEFAULT_POST_TIME = 0.1f;
    public float myWaitTime;
    public float myPostTime;
    public bool myIsCritical;
    public bool myToAll;
    public bool mySay;
    public bool mySayIfExceed = true;

    public override string ToString()
    {
        string res = GetType().ToString();
        res += "\nmyWaitTime " + myWaitTime;
        res += "\nmyPostTime " + myPostTime;
        res += "\nmyIsCritical " + myIsCritical;
        res += "\nmyToAll " + myToAll;
        res += "\nmySay " + mySay;
        res += "\nmySayIfExceed " + mySayIfExceed;
        return res;
    }
    public RadioMessageInfo() { }
    public RadioMessageInfo(float critical_time, float post_time, bool is_critical, bool to_all, bool say_flag)
    {
        myWaitTime = critical_time;
        myPostTime = post_time;
        myIsCritical = is_critical;
        myToAll = to_all;
        mySay = say_flag;
    }

    public RadioMessageInfo(bool is_critical, bool to_all, bool say_flag) :
        this(DEFAULT_WAIT_TIME, DEFAULT_POST_TIME, is_critical, to_all, say_flag)
    { }

    public RadioMessageInfo(float post_time, bool is_critical, bool to_all, bool say_flag) :
        this(DEFAULT_WAIT_TIME, post_time, is_critical, to_all, say_flag)
    { }

    public void set(float wait_time, float post_time, bool critical, bool all, bool say)
    {
        myWaitTime = wait_time;
        myPostTime = post_time;
        myIsCritical = critical;
        myToAll = all;
        mySay = say;
    }

    public MessageProcessResult update(float scale, bool radio_free)
    {
        myWaitTime -= scale;
        if (radio_free)
            myPostTime -= scale;
        if (myPostTime <= 0 || myWaitTime <= 0)
        {
            if (radio_free)
                return MessageProcessResult.rtSendAndExit;
            if (myIsCritical)
                return MessageProcessResult.rtCritical;
            return MessageProcessResult.rtExit;
        }
        return MessageProcessResult.rtContinue;
    }
}
public enum MessageProcessResult : uint
{
    rtSendAndExit,
    rtExit,
    rtContinue,
    rtCritical,
    rtDWORD = 0xFFffFFff
};



