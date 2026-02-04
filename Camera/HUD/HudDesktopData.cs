using DWORD = System.UInt32;

public class HudDesktopData : HudDeviceData
{
    public float speed;
    public float CurrSize;

    DWORD var_speed;

    public HudDesktopData(BaseScene pScene, DWORD iname,string sname) : base(pScene, iname, sname)
    {
        string Buff;
        //wsprintf(Buff, "hud_%s_""speed", sname); var_speed = rScene.GetCommandsApi()->RegisterVariable(Buff, this, VAR_FLOAT)
        Buff=string.Format("hud_{0}_speed",sname);
        var_speed = rScene.GetCommandsApi().RegisterVariable(Buff, this, VType.VAR_FLOAT);
    }
    public override object OnVariable(uint code,object data)
    {
        { if (code == var_speed) { if (data!=null) { speed = (float)data; changed = true; } return speed; } }

        return base.OnVariable(code, data);
    }

};