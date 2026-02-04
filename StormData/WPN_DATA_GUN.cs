
/// <summary>
/// описание GUNs
/// </summary>
public class WPN_DATA_GUN : WPN_DATA_BARREL
{
    // internal part
    public WPN_DATA_GUN(string name) : base(name)
    {
        Type = WpnDataDefines.WT_GUN;
        // перезарядка
        AmmoLoad = 0;
        ReloadTime = 0;
        // сам выстрел
        Speed = 0;
        LifeTime = 0;
        // гильзы
        Shell = null;
        nShells = 1;

    }
    public override void ProcessToken(READ_TEXT_STREAM st, string value)
    {
        do
        {
            // перезарядка
            if (st.LoadInt(ref AmmoLoad, "Load")) continue;
            if (st.LoadFloat(ref ReloadTime, "ReloadTime")) continue;
            // сам выстрел
            if (st.LoadFloat(ref Speed, "Speed")) continue;
            if (st.LoadFloat(ref LifeTime, "LifeTime")) continue;
            // гильзы
            if (st.LdHST<DEBRIS_DATA>(ref Shell, "Shell")) continue;
            int tmpInt = 0;
            if (st.LoadInt(ref tmpInt, "nShells")) continue;
            nShells = (uint)tmpInt;
            base.ProcessToken(st, value);
        } while (false);

    }
    public override void Reference(SUBOBJ_DATA data)
    {
        base.Reference(data);
        // поля WPN_DATA
        if (data.GetClass() != SC_WEAPON_SLOT) return;
        WPN_DATA r = (WPN_DATA)data;
        // поля WPN_DATA_GUN
        if (r.Type != WpnDataDefines.WT_GUN) return;
        WPN_DATA_GUN rr = (WPN_DATA_GUN)data;
        AmmoLoad = rr.AmmoLoad;
        ReloadTime = rr.ReloadTime;
        Speed = rr.Speed;
        LifeTime = rr.LifeTime;
        Shell = rr.Shell;
        nShells = rr.nShells;

    }
    public override void MakeLinks()
    {
        base.MakeLinks();
        Range = Speed * LifeTime;
        if (Shell != null)
        {
            //#define RSLV(v,t)   { if (v) v=(t*)t::GetByCode((unsigned int)v); }
            //RSLV(Shell, DEBRIS_DATA);
            StormDataHDR.RSLV<DEBRIS_DATA>(ref Shell);
        }
        else
        {
            nShells = 0;
        }

    }
    public override float GetAmmoLoad() { return AmmoLoad; }
    public override float GetLifeTime() { return LifeTime; }
    public override float GetSpeed() { return Speed; }
    public override float GetReload() { return ReloadTime; }
    // перезарядка
    public int AmmoLoad;
    public float ReloadTime;
    // гильзы
    public DEBRIS_DATA Shell;
    public uint nShells;
    // сам выстрел
    public float Speed;
    public float LifeTime;
};
