using static HudData;

public class HudCompasData : HudDeviceData
{
    public float angle_view;
    public float viewcompasangle;
    public int colour_pointer;
    public float pointer, angle;
    int var_angle_view, var_colour_pointer, var_viewcompasangle;
    int var_pointer, var_angle;

    public HudCompasData(BaseScene pScene) : base(pScene, iCompas, sCompas)
    {
        string sname = sCompas;
        string Buff;

        //char Buff[256];
        //
        Buff = string.Format("hud_{0}_angle_view", sname); var_angle_view = (int)rScene.GetCommandsApi().RegisterVariable(Buff, this, VType.VAR_FLOAT);
        Buff = string.Format("hud_{0}_angle", sname); var_angle = (int)rScene.GetCommandsApi().RegisterVariable(Buff, this, VType.VAR_FLOAT);
        Buff = string.Format("hud_{0}_pointer", sname); var_pointer = (int)rScene.GetCommandsApi().RegisterVariable(Buff, this, VType.VAR_FLOAT);
        Buff = string.Format("hud_{0}_viewcompasangle", sname); var_viewcompasangle = (int)rScene.GetCommandsApi().RegisterVariable(Buff, this, VType.VAR_FLOAT);
        Buff = string.Format("hud_{0}_colour_pointer", sname); var_colour_pointer = (int)rScene.GetCommandsApi().RegisterVariable(Buff, this, VType.VAR_INT);
    }


    public override object OnVariable(uint code, object data)
    {
        { if (code == var_angle_view) { if (data != null) { angle_view = (float)data; changed = true; } return angle_view; } }
        { if (code == var_angle) { if (data != null) { angle = (float)data; changed = true; } return angle; } }
        { if (code == var_pointer) { if (data != null) { pointer = (float)data; changed = true; } return pointer; } }
        { if (code == var_viewcompasangle) { if (data != null) { viewcompasangle = (float)data; changed = true; } return viewcompasangle; } }
        {
            if (code == var_colour_pointer) { if (data != null) { uint v = (uint)data; if (v < 40) { colour_pointer = (int)v; changed = true; } else { rScene.Message("Colour out of range\n"); } } return colour_pointer; }
        }
        return base.OnVariable(code, data);
    }
    public override void OnCommand(uint code, string arg1, string arg2)
    {
        base.OnCommand(code, arg1, arg2);
    }
    public override void OnTrigger(uint code, bool inv) { }
}