public interface iClientDevice : IObject
{
    public void ProcessData(ClientDeviceData icd);
    public virtual void updateDevice(float scale) { }
};