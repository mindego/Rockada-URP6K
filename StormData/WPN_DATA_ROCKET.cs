using UnityEngine;
/// <summary>
/// описание ROCKETs
/// </summary>
public class WPN_DATA_ROCKET : WPN_DATA_BARREL
{
    // internal part
    public WPN_DATA_ROCKET(string name) : base(name)
    {
        Type = WpnDataDefines.WT_ROCKET;
        // перезарядка
        Mass = 50;
        AmmoLoad = 0;
        ReloadTime = 0;
        // сам выстрел
        Accel = 0;
        MaxSpeed = 0;
        LifeTime = 0;
        StrafeBrakeAccel = 10;
        // свет от выстрела
        LightColor = Color.black;
        LightRadius = 0;

    }
    public override void ProcessToken(READ_TEXT_STREAM st, string value)
    {
        do
        {
            // перезарядка
            if (st.LoadFloat(ref Mass, "Mass")) continue;
            //if (st.LoadBool(ref myWorkInSFG, "WorkInSfg")) continue;
            if (st.LoadBool(ref myWorkInSFG, "WorkInSFG")) continue;
            if (st.LoadInt(ref AmmoLoad, "Load")) continue;
            if (st.LoadFloat(ref ReloadTime, "ReloadTime")) continue;
            // сам выстрел
            if (st.LoadFloat(ref Accel, "Acceleration")) continue;
            if (st.LoadFloat(ref MaxSpeed, "MaxSpeed")) continue;
            if (st.LoadFloat(ref LifeTime, "LifeTime")) continue;
            if (st.LoadFloat(ref StrafeBrakeAccel, "StrafeBrakeAccel")) continue;
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
        // поля WPN_DATA_ROCKET
        if (r.Type != WpnDataDefines.WT_ROCKET) return;
        WPN_DATA_ROCKET rr = (WPN_DATA_ROCKET)data;
        Mass = rr.Mass;
        AmmoLoad = rr.AmmoLoad;
        ReloadTime = rr.ReloadTime;
        Accel = rr.Accel;
        MaxSpeed = rr.MaxSpeed;
        LifeTime = rr.LifeTime;
        LightColor = rr.LightColor;
        LightRadius = rr.LightRadius;
        myWorkInSFG = rr.myWorkInSFG;

    }
    public override void MakeLinks()
    {
        base.MakeLinks();
        float t = MaxSpeed / Accel; if (t > LifeTime) t = LifeTime;
        Range = MaxSpeed * (LifeTime - t) + Accel * Mathf.Pow(t, 2) * .5f;

    }
    public override float GetLifeTime() { return LifeTime; }
    public override float GetSpeed() { return MaxSpeed; }
    public override float GetReload() { return ReloadTime; }
    public override float GetAmmoLoad() { return AmmoLoad; }
    public override float GetAccel() { return Accel; }
    public override float GetLightRadius() { return LightRadius; }
    public override Color GetLightColor() { return LightColor; }


    public override void GetAim(ref Vector3 TargetOrg, Vector3 TargetSpeed, MATRIX MyPos, Vector3 MySpeed)
    {
        // готовим систему координат
        Vector3 td = TargetOrg - MyPos.Org;
        Vector3 tr = TargetSpeed;
        float d = td.magnitude;
        if (d < .1f) return;
        td /= d;
        Vector3 tmp = Vector3.Cross(td, tr);
        float f = tmp.magnitude;
        if (f > .1f)
        {
            tmp /= f;
            tr = Vector3.Cross(tmp, td);
            // определяем Y состовляющую скорости снаряда
            f = Mathf.Pow(Vector3.Dot(TargetSpeed, tr) / GetSpeed(), 2);
            if (f > 1f) return;
            f = Mathf.Sqrt(1f - f) * GetSpeed() - (Vector3.Dot(TargetSpeed, td));
            if (f <= .0f) return;
            // определяем упреждение
            TargetOrg += TargetSpeed * (d / f);
        }
        // определяем боковое смещение
        f = d / MaxSpeed;
        TargetOrg -= MyPos.Right * GetRocketStrafe(Vector3.Dot(MySpeed, MyPos.Right), f, StrafeBrakeAccel);
        TargetOrg -= MyPos.Up * GetRocketStrafe(Vector3.Dot(MySpeed, MyPos.Up), f, StrafeBrakeAccel);

    }
    // перезарядка
    public float Mass;
    public int AmmoLoad;
    public float ReloadTime;
    // сам выстрел
    public float Accel;
    public float MaxSpeed;
    public float LifeTime;
    public float StrafeBrakeAccel;
    // свет от выстрела
    public Color LightColor;
    public float LightRadius;
    public bool myWorkInSFG = true;

    private float GetRocketStrafe(float spd, float t, float acc)
    {
        float st = Mathf.Abs(spd) / acc;
        if (st > t) st = t;
        return (spd * st - acc * Mathf.Pow(st, 2) * (spd > .0f ? .5f : -.5f));
    }

};
