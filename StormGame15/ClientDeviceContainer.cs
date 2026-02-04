using System;
using DWORD = System.UInt32;
/// <summary>
/// ClientDeviceContainer - для огранизации ClientDevice в списки
/// </summary>
class ClientDeviceContainer : TLIST_ELEM<ClientDeviceContainer>,IDisposable
{

    // API
    public ClientDeviceContainer(DWORD DeviceID, iClientDevice pDevice)
    {
        mDeviceID = DeviceID;
        mpDevice = pDevice;
    }
    ~ClientDeviceContainer() { mpDevice.Release(); }

    public void Dispose()
    {
        mpDevice.Release();
    }
    public void ProcessData(ClientDeviceData pData)
    {
        mpDevice.ProcessData(pData);
    }
    public DWORD GetDeviceID() { return mDeviceID; }
    public void updateDevice(float scale) { mpDevice.updateDevice(scale); }

    // own
    private DWORD mDeviceID;
    private iClientDevice mpDevice;

    private ClientDeviceContainer prev, next;
    public void SetPrev(ClientDeviceContainer c)
    {
        prev = c;
    }

    public void SetNext(ClientDeviceContainer c)
    {
        next = c;
    }

    public ClientDeviceContainer Next() { return next; }
    public ClientDeviceContainer Prev() { return prev; }

};
