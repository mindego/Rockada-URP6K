
using UnityEngine;

public class StormInput : EInput
{
    //Input8 myDI;
    InputClient myUser;

    // static devices
    Keybrd myKbd;
    Mouse myMice;
    Joystiks myJoys;

    public StormInput(InputCreateParams p) //: myDI(p.di), myhWnd(p.hWnd), myUser(p.cl),
                                           //myKbd(p.hWnd, p.di), myMice(p.hWnd, p.di), myJoys(p.di, p.joys) 
    {
        myKbd = new Keybrd(null, p.di);
        myMice = new Mouse(null, p.di);
        myJoys = new Joystiks(p.di, p.joys);
        myUser = p.cl;
    }

    //EInput * create(HWND w, InputClient *u, IJoys *j, IDirectInput8A *di) {
    public static EInput create(object w, InputClient u, IJoys j, IDirectInput8A di)
    {
        InputCreateParams cp = new InputCreateParams(w, u, j, di);
        return (EInput)new StormInput(cp);
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public float getJoystickX()
    {
        throw new System.NotImplementedException();
    }

    public float getJoystickY()
    {
        throw new System.NotImplementedException();
    }

    public int getMouseDX()
    {
        return myMice.getDX(); 
    }

    public int getMouseDY()
    {
        return myMice.getDY();
    }

    public float getRudder()
    {
        throw new System.NotImplementedException();
    }

    public float getThrottle()
    {
        throw new System.NotImplementedException();
    }

    public bool isJoystickPresent()
    {
        throw new System.NotImplementedException();
    }

    public bool isPOVPresent()
    {
        throw new System.NotImplementedException();
    }

    public bool isRudderPresent()
    {
        throw new System.NotImplementedException();
    }

    public bool isThrottlePresent()
    {
        throw new System.NotImplementedException();
    }

    public void poll()
    {
        //myKbd.poll(myhWnd, myUser);
        //myMice.poll(myhWnd, myUser);
        //myJoys.poll(myhWnd, myUser);
        myKbd.poll(null, myUser);
        myMice.poll(null, myUser);
        myJoys.poll(null, myUser);
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }

    public void setJoystickZone(float f)
    {
        throw new System.NotImplementedException();
    }

    public void setRudderZone(float f)
    {
        throw new System.NotImplementedException();
    }

    public bool useJoysticks(int nJoy, int nRudder)
    {
        return true;
        // return myJoys.open(nJoy, nRudder, myhWnd, myDI);
    }
}

public class InputCreateParams
{
    //HWND hWnd;
    public InputClient cl;
    public IJoys joys;
    public IDirectInput8A di;
    public InputCreateParams(object w, InputClient u, IJoys j, IDirectInput8A di)
    {
        cl = u;
        joys = j;
    }
};

/// <summary>
/// Эмуляция IDirectInput8A
/// </summary>
public interface IDirectInput8A
{

}

public class Keybrd
{
    public Keybrd(object hWnd, IDirectInput8A di) { }
    public void poll(object hWnd, InputClient user)
    {
        foreach (var k in ITranslator2.keybord)
        {
            if (Input.GetKeyDown((KeyCode)k.code))
            {
                //Debug.Log(k.name + " pressed for " + user);
                user.processKeyPress(k.code, true);
            }
            if (Input.GetKeyUp((KeyCode)k.code))
            {
                //Debug.Log(k.name + " unpressed for " + user);
                user.processKeyPress(k.code, false);
            }
        }
        //        user.processKeyPress(didod[i].dwOfs, bool(didod[i].dwData & 0x80));
    }
}

public class Mouse
{
    //    union {
    //        BYTE myButtons[4];
    //    DWORD myAllButtons;
    //};
    int mySysSpeed;
    int myX, myY;
    bool myWUp, myWDown;

    bool myIsLost;
    public Mouse(object hWnd, IDirectInput8A di)
    {
        myX = 0; myY = 0; myWUp = false; myWDown = false;
    }

    public void poll(object hWnd, InputClient user)
    {
        float mDelta;// = Input.mouseScrollDelta.y;
        myX += (int)(Input.GetAxis("Mouse X") * 10); //TODO Обдумать и установить корректный множитель
        myY += (int)(Input.GetAxis("Mouse Y") * 10);

        // buttons section
        //for (int i = 0; i < 4; i++)
        //    if (myButtons[i] != st.rgbButtons[i])
        //        user->processKeyPress(i + DIK_MOUSE1, bool(myButtons[i] = st.rgbButtons[i]));

        // wheels section
        //if (st.lZ > 0)
        if (Input.mouseScrollDelta.y > 0)
        {  // WheelUp
            if (!myWUp) user.processKeyPress(DIKtoUnityMapping.DIK_MOUSE_WUP, myWUp = true);
            if (myWDown) user.processKeyPress(DIKtoUnityMapping.DIK_MOUSE_WDOWN, myWDown = false);
        }
        else

        if (Input.mouseScrollDelta.y < 0)
        {  // WheelDown
            if (myWUp) user.processKeyPress(DIKtoUnityMapping.DIK_MOUSE_WUP, myWUp = false);
            if (!myWDown) user.processKeyPress(DIKtoUnityMapping.DIK_MOUSE_WDOWN, myWDown = true);
        }
        else
        {
            if (myWUp) user.processKeyPress(DIKtoUnityMapping.DIK_MOUSE_WUP, myWUp = false);
            if (myWDown) user.processKeyPress(DIKtoUnityMapping.DIK_MOUSE_WDOWN, myWDown = false);
        }
        //Debug.Log((myWDown, myWUp));
    }

    public int getDX() { int dx = myX; myX = 0; return dx; }

    public int getDY() { int dY = myY; myY = 0; return dY; }
}

public class Joystiks : IJoys
{
    public Joystiks(IDirectInput8A di, IJoys joys) { }

    public void addDevice(string name, bool joy, bool rudder)
    {
        throw new System.NotImplementedException();
    }

    public void poll(object hWnd, InputClient user)
    {
        //STUB
    }
}