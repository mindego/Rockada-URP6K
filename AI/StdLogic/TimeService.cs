public class TimeService : ITimeService
{
    public  float getTime()
    {
        return myTime;
    }

    public float getDelta()
    {
        return myDelta;
    }

    public void update(float delta)
    {
        myTime += delta;
        myDelta = delta;
    }

    void reset()
    {
        myTime = 0f;
        myDelta = 0f;
    }

    public TimeService() { reset(); }

    public float myTime;
    public float myDelta;
};
