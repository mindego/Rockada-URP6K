public interface EInput : IObject
{
    public const uint EINPUT_VERSION = 0xC0264FF7;// EInput 1.0
    public void poll();

    public int getMouseDX();
    public int getMouseDY();

    public bool useJoysticks(int nJoy, int nRudder);

    // is-ers
    public bool isJoystickPresent();   // есть ли джой?
    public bool isThrottlePresent();   // есть ли РУД?
    public bool isRudderPresent();   // есть ли педали?
    public bool isPOVPresent();   // есть ли хатка?
    // get-ers
    public float getJoystickX();   // положения оси X джоя
    public float getJoystickY();   // положения оси Y джоя
    public float getThrottle();   // положения РУДа
    public float getRudder();   // положения педалей
    // set-ers
    public void setJoystickZone(float f);     // установить мертвую зону джоя
    public void setRudderZone(float f);     // установить мертвую зону педалей

    public static EInput createInput(object h, InputClient u, IJoys j, uint version = EINPUT_VERSION)
    {
        //if (!h || !u || version != EINPUT_VERSION)
        if (u == null || version != EINPUT_VERSION)
            return null;

        //IDirectInput8A* pInput;

        //HRESULT hr = DirectInput8Create(dllHandle, DIRECTINPUT_VERSION,
        //    IID_IDirectInput8A, (void**)&pInput, 0);

        //if (DIFAIL(hr, "DirectInput8Create"))
        //{
        //    GetLog().Message("DirectInput fatal error: Can not create DirectInput8 object!");
        //    return 0;
        //}

        return StormInput.create(h, u, j, null);
    }

};

public interface IJoys
{
    public void addDevice(string name, bool joy, bool rudder);
}
