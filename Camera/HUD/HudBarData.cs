using DWORD = System.UInt32;
using static VType;

public class HudBarData : HudDeviceData
{

    //char text[256];
    public string text;
    public float hightmin;
    public float lowmax;
    public float value;
    public int scale;
    public bool vert;
    public uint colour_hight;
    public uint colour_normal;
    public uint colour_low;
    public uint colour_off;

    int var_text;
    uint var_hightmin;
    uint var_lowmax;
    uint var_value;
    uint var_scale;
    uint var_vert;
    uint var_colour_hight;
    uint var_colour_normal;
    uint var_colour_low;
    uint var_colour_off;


    public HudBarData(BaseScene pScene, DWORD iname, string sname) : base(pScene, iname, sname)
    {
        //text[sizeof(text) - 1] = 0;
        //char Buff[256];
        //wsprintf(Buff, "hud_%s_""hightmin", sname); var_hightmin = rScene.GetCommandsApi()->RegisterVariable(Buff, this, VAR_FLOAT);
        //wsprintf(Buff, "hud_%s_""lowmax", sname); var_lowmax = rScene.GetCommandsApi()->RegisterVariable(Buff, this, VAR_FLOAT);
        //wsprintf(Buff, "hud_%s_""value", sname); var_value = rScene.GetCommandsApi()->RegisterVariable(Buff, this, VAR_FLOAT);
        //wsprintf(Buff, "hud_%s_""scale", sname); var_scale = rScene.GetCommandsApi()->RegisterVariable(Buff, this, VAR_INT);
        //wsprintf(Buff, "hud_%s_""vert", sname); var_vert = rScene.GetCommandsApi()->RegisterVariable(Buff, this, VAR_INT);

        //wsprintf(Buff, "hud_%s_""colour_hight", sname); var_colour_hight = rScene.GetCommandsApi()->RegisterVariable(Buff, this, VAR_INT);
        //wsprintf(Buff, "hud_%s_""colour_normal", sname); var_colour_normal = rScene.GetCommandsApi()->RegisterVariable(Buff, this, VAR_INT);
        //wsprintf(Buff, "hud_%s_""colour_low", sname); var_colour_low = rScene.GetCommandsApi()->RegisterVariable(Buff, this, VAR_INT);
        //wsprintf(Buff, "hud_%s_""colour_off", sname); var_colour_off = rScene.GetCommandsApi()->RegisterVariable(Buff, this, VAR_INT)

        //example
        // string Buff;
        // Buff = string.Format("hud_{0}_x", sname); var_x = (int)rScene.GetCommandsApi().RegisterVariable(Buff, this, VType.VAR_FLOAT);
        string Buff;
        Buff = string.Format("hud_{0}_hightmin", sname); var_hightmin = rScene.GetCommandsApi().RegisterVariable(Buff, this, VAR_FLOAT);
        Buff = string.Format("hud_{0}_lowmax", sname); var_lowmax = rScene.GetCommandsApi().RegisterVariable(Buff, this, VAR_FLOAT);
        Buff = string.Format("hud_{0}_value", sname); var_value = rScene.GetCommandsApi().RegisterVariable(Buff, this, VAR_FLOAT);
        Buff = string.Format("hud_{0}_scale", sname); var_scale = rScene.GetCommandsApi().RegisterVariable(Buff, this, VAR_FLOAT);
        Buff = string.Format("hud_{0}_vert", sname); var_vert = rScene.GetCommandsApi().RegisterVariable(Buff, this, VAR_FLOAT);

        Buff = string.Format("hud_{0}_colour_hight", sname); var_colour_hight = rScene.GetCommandsApi().RegisterVariable(Buff, this, VAR_INT);
        Buff = string.Format("hud_{0}_colour_normal", sname); var_colour_normal = rScene.GetCommandsApi().RegisterVariable(Buff, this, VAR_INT);
        Buff = string.Format("hud_{0}_colour_low", sname); var_colour_low = rScene.GetCommandsApi().RegisterVariable(Buff, this, VAR_INT);
        Buff = string.Format("hud_{0}_colour_off", sname); var_colour_off = rScene.GetCommandsApi().RegisterVariable(Buff, this, VAR_INT);
    }
    ~HudBarData() { }

    public override object OnVariable(uint code, object data)
    {
        { if (code == var_hightmin) { if (data != null) { hightmin = (float)data; changed = true; } return hightmin; } }
        { if (code == var_lowmax) { if (data != null) { lowmax = (float)data; changed = true; } return lowmax; } }
        { if (code == var_value) { if (data != null) { value = (float)data; changed = true; } return value; } }
        { if (code == var_scale) { if (data != null) { scale = (int)data; changed = true; } return scale; } }
        { if (code == var_vert) { if (data != null) { vert = (bool)data; changed = true; } return vert; } }

        { if (code == var_colour_hight) { if (data != null) { uint v = (uint)data; if (v < 40) { colour_hight = v; changed = true; } else { rScene.Message("Colour out of range\n"); } } return colour_hight; } }
        { if (code == var_colour_normal) { if (data != null) { uint v = (uint)data; if (v < 40) { colour_normal = v; changed = true; } else { rScene.Message("Colour out of range\n"); } } return colour_normal; } }
        { if (code == var_colour_low) { if (data != null) { uint v = (uint)data; if (v < 40) { colour_low = v; changed = true; } else { rScene.Message("Colour out of range\n"); } } return colour_low; } }
        { if (code == var_colour_off) { if (data != null) { uint v = (uint)data; if (v < 40) { colour_off = v; changed = true; } else { rScene.Message("Colour out of range\n"); } } return colour_off; } }

        return base.OnVariable(code, data);
    }

    public void SetText(string _text)
    {
        text = _text;
    }
};