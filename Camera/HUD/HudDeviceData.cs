using DWORD = System.UInt32;
using static HudData;
using static VType;

public class HudDeviceData : BaseData, CommLink
{
    public float x, y, h, w;
    public HudColors colors;
    public int colour;
    public float opacity;
    public int state;
    public int var_x, var_y, var_h, var_w;
    public int var_state, var_colour, var_opacity;
    public int cmd_view, cmd_changed;
    public bool changed;
    public HudDeviceData(BaseScene pScene, DWORD iname, string sname) : base(pScene, iname)
    {
        opacity = 1f;
        string Buff;
        Buff = string.Format("hud_{0}_x", sname); var_x = (int)rScene.GetCommandsApi().RegisterVariable(Buff, this, VType.VAR_FLOAT);
        Buff = string.Format("hud_{0}_y", sname); var_y = (int)rScene.GetCommandsApi().RegisterVariable(Buff, this, VType.VAR_FLOAT);
        Buff = string.Format("hud_{0}_w", sname); var_w = (int)rScene.GetCommandsApi().RegisterVariable(Buff, this, VType.VAR_FLOAT);
        Buff = string.Format("hud_{0}_h", sname); var_h = (int)rScene.GetCommandsApi().RegisterVariable(Buff, this, VType.VAR_FLOAT);
        Buff = string.Format("hud_{0}_colour", sname); var_colour = (int)rScene.GetCommandsApi().RegisterVariable(Buff, this, VType.VAR_INT);
        Buff = string.Format("hud_{0}_opacity", sname); var_opacity = (int)rScene.GetCommandsApi().RegisterVariable(Buff, this, VType.VAR_FLOAT);
        Buff = string.Format("hud_{0}_state", sname); var_state = (int)rScene.GetCommandsApi().RegisterVariable(Buff, this, VType.VAR_INT);
        Buff = string.Format("hud_{0}_view", sname); cmd_view = (int)rScene.GetCommandsApi().RegisterCommand(Buff, this, 1);
        Buff = string.Format("hud_{0}_changed", sname); cmd_changed = (int)rScene.GetCommandsApi().RegisterCommand(Buff, this, 0);
    }
    ~HudDeviceData()
    {
        rScene.GetCommandsApi().UnRegister(this);
    }

    public virtual object OnVariable(uint code, object data) {
        { if (code == var_x) { if (data!=null) { x = (float) data; changed = true; } return x; } }
        { if (code == var_y) { if (data!=null) { y = (float)data; changed = true; } return y; } }
        { if (code == var_w) { if (data!=null) { w = (float)data; changed = true; } return w; } }
        { if (code == var_h) { if (data!=null) { h = (float)data; changed = true; } return h; } }
        { if (code == var_colour) { if (data!=null) { uint v = (uint)data; if (v < 40) { colour = (int)v; changed = true; } else { rScene.Message("Colour out of range\n"); } } return colour; } }
        { if (code == var_opacity) { if (data!=null) { opacity = (float)data; changed = true; } return opacity; } }
        { if (code == var_state) { if (data!=null) { state = (int)data; changed = true; } return state; } }
        return null;
    }

    public virtual void OnCommand(uint code, string arg1, string arg2)
    {
        //const string Sep= ",";
        //if (cmd_changed == code) changed = true;
        //TODO Реализовать выполнение команды в HudDevice
    }
    public virtual void OnTrigger(uint code, bool on) { }
};

public class HudTargetBoxData : HudDeviceData
{
    public float wb;
    public float maxsize;
    public float minsize;

    //char text[256];
    //char textup[256];
    public string text;
    public string textup;
    uint var_wb;
    uint var_maxsize;
    uint var_minsize;

    public HudTargetBoxData(BaseScene pScene, DWORD iname = iTarget,string sname = sTarget): base(pScene, iname, sname) {
        string Buff;
        Buff = string.Format("hud_{0}_wb", sname); var_wb = rScene.GetCommandsApi().RegisterVariable(Buff, this, VAR_FLOAT);
        Buff = string.Format("hud_{0}_minsize", sname); var_minsize = rScene.GetCommandsApi().RegisterVariable(Buff, this, VAR_FLOAT);
        Buff = string.Format("hud_{0}_maxsize", sname); var_maxsize = rScene.GetCommandsApi().RegisterVariable(Buff, this, VAR_FLOAT);
    }
    public override object OnVariable(uint code,object data)
    {
        { if (code == var_wb) { if (data!=null) { wb = (float)data; changed = true; } return wb; } }
        { if (code == var_minsize) { if (data != null) { minsize = (float)data; changed = true; } return minsize; } }
        { if (code == var_maxsize) { if (data != null) { maxsize = (float)data; changed = true; } return maxsize; } }

        return base.OnVariable(code, data);
    }

};

