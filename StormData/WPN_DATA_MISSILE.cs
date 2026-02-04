using UnityEngine;
using static UnitDataDefines;
/// <summary>
/// описание MISSILEs
/// </summary>
public class WPN_DATA_MISSILE : WPN_DATA_DETACHING
{
    // internal part
    public WPN_DATA_MISSILE(string name) : base(name)
    {
        Type = WpnDataDefines.WT_MISSILE;
        // наведение
        LockAngle = 10f;
        LockTime = 3f;
        // сам выстрел
        Accel = 100f;
        MaxSpeed = 300f;
        LifeTime = 30f;
        TriggerTime = 2f;
        ProximityTime = 3f;
        // свет от выстрела
        LightColor = Color.black;
        LightRadius = 0;

    }
    public override void ProcessToken(READ_TEXT_STREAM st, string value)
    {
        do
        {
            // наведение
            if (st.LoadFloat(ref LockAngle, "LockAngle")) continue;
            if (st.LoadFloat(ref LockTime, "LockTime")) continue;
            // сам выстрел
            if (st.LoadFloat(ref Accel, "Acceleration")) continue;
            if (st.LoadFloat(ref MaxSpeed, "MaxSpeed")) continue;
            if (st.LoadFloat(ref LifeTime, "LifeTime")) continue;
            if (st.LoadFloat(ref TriggerTime, "TriggerTime")) continue;
            if (st.LoadFloat(ref ProximityTime, "ProximityTime")) continue;
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
        // поля WPN_DATA_MISSILE
        if (r.Type != WpnDataDefines.WT_MISSILE) return;
        WPN_DATA_MISSILE rr = (WPN_DATA_MISSILE)data;
        LockAngle = rr.LockAngle;
        LockTime = rr.LockTime;
        Accel = rr.Accel;
        MaxSpeed = rr.MaxSpeed;
        LifeTime = rr.LifeTime;
        TriggerTime = rr.TriggerTime;
        ProximityTime = rr.ProximityTime;
        LightColor = rr.LightColor;
        LightRadius = rr.LightRadius;

    }
    public override void MakeLinks()
    {
        base.MakeLinks();
        Range = MaxSpeed * LifeTime;
        LockAngle = Storm.Math.GRD2RD(LockAngle);
        LockAngleCos = Mathf.Cos(LockAngle);
        MinRange = MaxSpeed * TriggerTime;
        float t = Accel * Mathf.Pow(MaxSpeed / Accel, 2) * .5f + MaxSpeed * ProximityTime;
        if (MinRange < t) MinRange = t;
        // информация по юниту
        UnitDataIndex = UnitDataTable.GetIdxByType(UT_MISSILE);
        if (UnitDataIndex < 0) stormdata_dll.StructError("Weapon", FullName);

    }
    public override float GetLifeTime() { return LifeTime; }
    public override float GetSpeed() { return MaxSpeed; }
    public override float GetReload() { return 3f; }
    public override float GetAccel() { return Accel; }
    public override float GetLightRadius() { return LightRadius; }
    public override Color GetLightColor() { return LightColor; }
    public override void GetAim(ref Vector3 TargetOrg, Vector3 TargetSpeed, MATRIX MyPos, Vector3 MySpeed)
    {
        //Действительно пусток.
    }

    // наведение
    public float LockAngle;
    public float LockAngleCos;
    public float LockTime;
    // сам выстрел
    public float Accel;
    public float MaxSpeed;
    public float LifeTime;
    public float TriggerTime;
    public float ProximityTime;
    public float MinRange;
    // свет от выстрела
    Color LightColor;
    float LightRadius;
};
