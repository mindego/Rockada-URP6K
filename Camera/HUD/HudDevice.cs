using UnityEngine;

public class HudDevice
{
    HUDTree Dev;
    public HudDevice(HUDTree _Dev)
    {
        Dev = _Dev;
    }
    public virtual HudDeviceData GetData()
    {
        return Dev.GetData().GetData();
    }
    public virtual void Hide(bool off)
    {
        Dev.GetData().SetHide(off);
    }
    ~HudDevice()
    {
        Dispose();
    }
    public void Dispose()
    {
        Dev.Sub();
    }
};


