public static class PeriodicCounter
{
    public static float getPeriodic(float ctime, int period)
    { // 0,1,2,3 ...
        return ctime - System.MathF.Floor(ctime / period) * period;
    }

    public static  float getPeriodicInv(float ctime, int period)
    { // 3,2,1,0 ...
        return period - getPeriodic(ctime, period);
    }
}