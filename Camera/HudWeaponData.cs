using DWORD = System.UInt32;
using static HudData;

public class HudWeaponData : HudDeviceData
{

    public float scalesize;
    public float step;
    public float spacing;
    public float fontx;
    public float fonty;
    public int boxcolour;

    public int var_scalesize;
    public int var_step;
    public int var_spacing;
    public int var_fontx;
    public int var_fonty;
    public TLIST<WeaponData> Items = new TLIST<WeaponData>();

    public HudWeaponData(BaseScene pScene, DWORD iname = iWeapon, string sname = sWeapon) : base(pScene, iname, sname)
    {
    //    wsprintf(Buff, "hud_%s_""scalesize", sname); var_scalesize = rScene.GetCommandsApi()->RegisterVariable(Buff, this, VAR_FLOAT);
    //    wsprintf(Buff, "hud_%s_""step", sname); var_step = rScene.GetCommandsApi()->RegisterVariable(Buff, this, VAR_FLOAT);
    //    wsprintf(Buff, "hud_%s_""spacing", sname); var_spacing = rScene.GetCommandsApi()->RegisterVariable(Buff, this, VAR_FLOAT);
    //    wsprintf(Buff, "hud_%s_""fontx", sname); var_fontx = rScene.GetCommandsApi()->RegisterVariable(Buff, this, VAR_FLOAT);
    //    wsprintf(Buff, "hud_%s_""fonty", sname); var_fonty = rScene.GetCommandsApi()->RegisterVariable(Buff, this, VAR_FLOAT)
    }
    ~HudWeaponData() { }    

    public override object OnVariable(uint code, object data)
    {
        return base.OnVariable(code, data);
    }
};
