using UnityEngine;

public class WPN_DATA_HTGR : WPN_DATA
{
    // internal part
    public WPN_DATA_HTGR(string n) : base(n)
    {
        Type = WpnDataDefines.WT_HTGR;
        // сам выстрел
        MinRange = 2500f;
        MaxRange = 20000f;
        CountdownTime = 3f;

        myParams.Accel = 10f;
        myParams.AcsendTime = 3f;
        myParams.DropAngle = 70f;
        myParams.DropDist = 500f;

        myParams.AvrgSpeed = 0;
        myParams.DropSpeed = 0;

        WarheadsNumber = 0;
        WarheadDispersion = 100;
        Warhead = null;

        // свет от выстрела
        LightColor = new Color(0, 0, 0, 0);
    }

    public override void ProcessToken(READ_TEXT_STREAM st, string value) {
        do
        {
            // сам выстрел

            if (st.LoadFloat(ref MinRange, "MinRange")) continue;
            if (st.LoadFloat(ref MaxRange, "MaxRange")) continue;
            if (st.LoadFloat(ref CountdownTime, "CountdownTime")) continue;

            if (st.LoadFloat(ref myParams.Accel, "Acceleration")) continue;
            if (st.LoadFloat(ref myParams.AcsendTime, "AcsendTime")) continue;
            if (st.LoadFloat(ref myParams.AvrgSpeed, "AvrgSpeed")) continue;
            if (st.LoadFloat(ref myParams.DropAngle, "DropAngle")) continue;
            if (st.LoadFloat(ref myParams.DropDist, "DropDist")) continue;
            if (st.LoadFloat(ref myParams.DropSpeed, "DropSpeed")) continue;

            if (st.LoadInt(ref WarheadsNumber, "WarheadsNumber")) continue;
            if (st.LoadInt(ref WarheadDispersion, "WarheadDispersion")) continue;
            //Перенесено на этап линкования.
            //if (st.LdHST<WPN_DATA>(ref Warhead, "Warhead")) continue;
            if (st.Load_String(ref WarheadLate, "Warhead")) continue;
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
        // поля WPN_DATA_HTGR
        if (r.Type != WpnDataDefines.WT_HTGR) return;
        WPN_DATA_HTGR rr = (WPN_DATA_HTGR)data;
        MinRange = rr.MinRange;
        MaxRange = rr.MaxRange;
        CountdownTime = rr.CountdownTime;

        myParams = rr.myParams;

        WarheadsNumber = rr.WarheadsNumber;
        WarheadDispersion = rr.WarheadDispersion;
        Warhead = rr.Warhead;
        WarheadLate = rr.WarheadLate;
    }
    public override void MakeLinks()
    {
        base.MakeLinks();
        if (Warhead == null)
        {
            Warhead = (WPN_DATA)SUBOBJ_DATA.GetByName(WarheadLate);
        }
        myParams.DropAngle = Storm.Math.GRD2RD(myParams.DropAngle);

        if (myParams.AvrgSpeed * myParams.DropSpeed == 0)
            stormdata_dll.StructError("WeaponHTGR(bad speed)", FullName);

        if (Mathf.Abs(WarheadDispersion) >= myParams.DropDist)
            stormdata_dll.StructError("WeaponHTGR(DropDist too large)", FullName);

        StormDataHDR.RSLV<WPN_DATA>(ref Warhead);
        if (Warhead == null)
            stormdata_dll.StructError("WeaponHTGR(No warhead specified)", FullName);

        // информация по юниту
        //UnitDataIndex = UnitDataTable.pUnitDataTable.GetIdxByType(UnitDataDefines.UT_MISSILE);
        UnitDataIndex = UnitDataTable.GetIdxByType(UnitDataDefines.UT_MISSILE);
        //TODO Восстановить корректный тип (ракеты( при линковании HTGR
        //if (UnitDataIndex < 0) dllmain.StructError("Weapon", FullName);
    }
    public override float GetLifeTime()
    {
        return 2 * MaxRange / myParams.AvrgSpeed;
    }
    public override float GetSpeed()
    {
        return myParams.AvrgSpeed;
    }
    public override float GetReload() {
        return 3.0f; }
    public override float GetAccel()
    {
        return myParams.Accel;
    }
    public override float GetLightRadius() {
        return LightRadius;
    }
    public override Color GetLightColor()
    {
        return LightColor;
    }
    public override void GetAim(ref Vector3 TargetOrg, Vector3 TargetSpeed, MATRIX MyPos, Vector3 MySpeed) { }
    
    // сам выстрел

    // aim
    float MinRange;
    float MaxRange;
    float CountdownTime;

    // physic
    HTGRData myParams;

    // split
    int WarheadsNumber;
    int WarheadDispersion;
    WPN_DATA Warhead;

    // свет от выстрела
    Color LightColor;
    float LightRadius;

    //Данные для второго этапа загрузки
    string WarheadLate;
}

public struct HTGRData
{
    public float Accel;
    public float AcsendTime;
    public float DropSpeed;
    public float AvrgSpeed;
    public float DropAngle;
    public float DropDist;
};