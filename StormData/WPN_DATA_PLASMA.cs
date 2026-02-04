using UnityEngine;
/// <summary>
/// описание PLASMAs
/// </summary>
public class WPN_DATA_PLASMA : WPN_DATA_BARREL
{
    // internal part
    public WPN_DATA_PLASMA(string name) : base(name)
    {
        Type = WpnDataDefines.WT_PLASMA;
        // зарядка/разрядка
        Charge = 0;
        RechargeSpeed = 0;

        EnergyPerShot = 0;
        ReloadTime = 0;

        // сам выстрел
        Speed = 0;
        LifeTime = 0;
        // свет от выстрела
        //LightColor.Set(0, 0, 0, 0);
        LightColor = Color.black;
        LightRadius = 0;
    }
    public override void ProcessToken(READ_TEXT_STREAM st, string value)
    {
        do
        {
            // зарядка/разрядка
            if (st.LoadFloat(ref Charge, "Charge")) continue;
            if (st.LoadFloat(ref RechargeSpeed, "RechargeSpeed")) continue;

            if (st.LoadFloat(ref EnergyPerShot, "EnergyPerShot")) continue;
            if (st.LoadFloat(ref ReloadTime, "ReloadTime")) continue;
            // сам выстрел
            if (st.LoadFloat(ref Speed, "Speed")) continue;
            if (st.LoadFloat(ref LifeTime, "LifeTime")) continue;
            // свет от выстрела
            if (st.LoadColorF4(ref LightColor, "LightColor")) continue;
            if (st.LoadFloat(ref LightRadius, "LightRadius")) continue;
            base.ProcessToken(st, value);
        } while (false);

    }
    public override void Reference(SUBOBJ_DATA data)
    {
        base.Reference(data);
        // поля WPN_DATA
        if (data.GetClass() != SC_WEAPON_SLOT) return;
        WPN_DATA r = (WPN_DATA)data;
        // поля WPN_DATA_PLASMA
        if (r.Type != WpnDataDefines.WT_PLASMA) return;
        WPN_DATA_PLASMA rr = (WPN_DATA_PLASMA)data;
        Charge = rr.Charge;
        RechargeSpeed = rr.RechargeSpeed;
        Speed = rr.Speed;

        EnergyPerShot = rr.EnergyPerShot;
        ReloadTime = rr.ReloadTime;

        LifeTime = rr.LifeTime;
        LightColor = rr.LightColor;
        LightRadius = rr.LightRadius;

    }

    public override void MakeLinks()
    {
        base.MakeLinks();
        Range = Speed * LifeTime;

        if (EnergyPerShot == 0)
            EnergyPerShot = Charge;
    }
    public override float GetLifeTime() { return LifeTime; }
    public override float GetSpeed() { return Speed; }
    public override float GetReload() { return Charge / RechargeSpeed; }
    public override float GetLightRadius() { return LightRadius; }
    public override Color GetLightColor() { return LightColor; }
    // зарядка/разрядка
    public float Charge;
    public float RechargeSpeed;

    public float EnergyPerShot;
    public float ReloadTime;
    // сам выстрел
    public float Speed;
    public float LifeTime;
    // свет от выстрела
    public Color LightColor;
    public float LightRadius;
};
