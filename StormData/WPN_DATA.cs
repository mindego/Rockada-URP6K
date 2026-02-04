using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using DWORD = System.UInt32;
using WORD = System.UInt16;

/// <summary>
/// базовый класс
/// </summary>
public class WPN_DATA : SUBOBJ_DATA, iSTORM_DATA<WPN_DATA>
{
    public const float WPN_LIGHT_CUT_RADIUS = 0.1f;
    public static string[] WeaponWeightNames = { "small", "medium", "heavy", "static" };
    // internal part

    public WPN_DATA()
    {
        //Конструктор-заглушка для временных переменных
    }
    public WPN_DATA(string name) : base(name)
    {
        Flags |= SC_WEAPON_SLOT;
        // информация по сетевой обработке
        SeparatedPacket = false;
        // информация по подвеске
        Type = 0;
        Weight = WpnDataDefines.WW_STATIC;
        IsHuman = false;
        IsChained = false;
        // информация по эффективности
        CraftEffectivness1 = 1;
        CraftEffectivnessDist = 500;
        CraftEffectivness2 = .1f;
        // информация по объекту, представляющему снаряд
        MeshName = 0xFFFFFFFF;
        ParticleName = 0xFFFFFFFF;
        // информация по повреждениям
        Damage = 0;
        Range = 0;
        XDamage = 0;
        XRadius = 0;
        // взрывы
        ExplOnEnd = null;
        ExplOnTarget = null;
        for (int i = 0; i < 8; i++)
            ExplOnGround[i] = null;
        myTurretPauseTimeBase = 1f;
        myTurretPauseTimeDelta = .0f;
        myTurretFireTimeBase = 3f;
        myTurretFireTimeDelta = .0f;
        myStaticError = myDynamicError = 0f;

    }

    //virtual ~WPN_DATA();
    public override void ProcessToken(READ_TEXT_STREAM st, string value)
    {
        do
        {
            // информация по сетевой обработке
            if (st.LoadBool(ref SeparatedPacket, "SeparatedPacket")) continue;
            // информация по подвеске
            int tmpWeight = 0;
            //st.LoadIndexFromTable(ref Weight, "Weight", WeaponWeightNames);
            if (st.LoadIndexFromTable(ref tmpWeight, "Weight", WeaponWeightNames)) continue;
            Weight = (WORD)tmpWeight;
            if (st.LoadBool(ref IsHuman, "IsHuman")) continue;
            if (st.LoadBool(ref IsChained, "IsChained")) continue;
            // информация по эффективности
            if (st.LoadFloat(ref CraftEffectivness1, "CraftEffectivness1")) continue;
            if (st.LoadFloat(ref CraftEffectivnessDist, "CraftEffectivnessDist")) continue;
            if (st.LoadFloat(ref CraftEffectivness2, "CraftEffectivness2")) continue;
            // информация по объекту, представляющему снаряд
            if (st.LdHS(ref MeshName, "MeshName")) continue;
            if (st.LdHS(ref ParticleName, "ParticleName")) continue;
            // информация по повреждениям
            if (st.LoadFloat(ref Damage, "EDamage")) continue;
            if (st.LoadFloat(ref Damage, "Damage")) continue;
            if (st.LoadFloat(ref XDamage, "Xdamage")) continue;
            if (st.LoadFloat(ref XRadius, "Xradius")) continue;
            // aim errors
            //Storm WW mode
            if (st.LoadFloat(ref myTurretPauseTimeBase, "TurretPauseTimeBase")) continue;
            if (st.LoadFloat(ref myTurretPauseTimeDelta, "TurretPauseTimeDelta")) continue;
            if (st.LoadFloat(ref myTurretFireTimeBase, "TurretFireTimeBase")) continue;
            if (st.LoadFloat(ref myTurretFireTimeDelta, "TurretFireTimeDelta")) continue;

            //// Storm original mode
            //if (st.LoadFloat(ref myTurretPauseTimeBase, "PauseTimeBase")) continue;
            //if (st.LoadFloat(ref myTurretPauseTimeDelta, "PauseTimeDelta")) continue;
            //if (st.LoadFloat(ref myTurretFireTimeBase, "FireTimeBase")) continue;
            //if (st.LoadFloat(ref myTurretFireTimeDelta, "FireTimeDelta")) continue;

            if (st.LoadFloat(ref myStaticError, "StaticAimError")) continue;
            if (st.LoadFloat(ref myDynamicError, "DynamicAimError")) continue;
            // взрывы
            if (st.Recognize("ExplOnEnd")) { ExplOnEnd = EXPLOSION_INFO.SafeExplosionInfoLoad(ExplOnEnd, st, XDamage, XRadius); continue; }
            if (st.Recognize("ExplOnTarget")) { ExplOnTarget = EXPLOSION_INFO.SafeExplosionInfoLoad(ExplOnTarget, st, XDamage, XRadius); continue; }
            if (st.Recognize("ExplOnGround[0]")) { ExplOnGround[0] = EXPLOSION_INFO.SafeExplosionInfoLoad(ExplOnGround[0], st, XDamage, XRadius); continue; }
            if (st.Recognize("ExplOnGround[1]")) { ExplOnGround[1] = EXPLOSION_INFO.SafeExplosionInfoLoad(ExplOnGround[1], st, XDamage, XRadius); continue; }
            if (st.Recognize("ExplOnGround[2]")) { ExplOnGround[2] = EXPLOSION_INFO.SafeExplosionInfoLoad(ExplOnGround[2], st, XDamage, XRadius); continue; }
            if (st.Recognize("ExplOnGround[3]")) { ExplOnGround[3] = EXPLOSION_INFO.SafeExplosionInfoLoad(ExplOnGround[3], st, XDamage, XRadius); continue; }
            if (st.Recognize("ExplOnGround[4]")) { ExplOnGround[4] = EXPLOSION_INFO.SafeExplosionInfoLoad(ExplOnGround[4], st, XDamage, XRadius); continue; }
            if (st.Recognize("ExplOnGround[5]")) { ExplOnGround[5] = EXPLOSION_INFO.SafeExplosionInfoLoad(ExplOnGround[5], st, XDamage, XRadius); continue; }
            if (st.Recognize("ExplOnGround[6]")) { ExplOnGround[6] = EXPLOSION_INFO.SafeExplosionInfoLoad(ExplOnGround[6], st, XDamage, XRadius); continue; }
            if (st.Recognize("ExplOnGround[7]")) { ExplOnGround[7] = EXPLOSION_INFO.SafeExplosionInfoLoad(ExplOnGround[7], st, XDamage, XRadius); continue; }
            base.ProcessToken(st, value);

        } while (false);
    }
    public override void Reference(SUBOBJ_DATA data)
    {
        base.Reference(data);
        // поля WPN_DATA
        if (data.GetClass() != SC_WEAPON_SLOT) return;
        WPN_DATA r = (WPN_DATA)data;
        SeparatedPacket = r.SeparatedPacket;
        Weight = r.Weight;
        IsChained = r.IsChained;
        CraftEffectivness1 = r.CraftEffectivness1;
        CraftEffectivnessDist = r.CraftEffectivnessDist;
        CraftEffectivness2 = r.CraftEffectivness2;
        MeshName = r.MeshName;
        ParticleName = r.ParticleName;
        Damage = r.Damage;
        Range = r.Range;
        XRadius = r.XRadius;
        XDamage = r.XDamage;
        ExplOnEnd = EXPLOSION_INFO.SafeExplosionInfoCopy(r.ExplOnEnd);
        ExplOnTarget = EXPLOSION_INFO.SafeExplosionInfoCopy(r.ExplOnTarget);
        for (int i = 0; i < 8; i++)
        {
            ExplOnGround[i] = EXPLOSION_INFO.SafeExplosionInfoCopy(r.ExplOnGround[i]);
        }
        myTurretPauseTimeBase = r.myTurretPauseTimeBase;
        myTurretPauseTimeDelta = r.myTurretPauseTimeDelta;
        myTurretFireTimeBase = r.myTurretFireTimeBase;
        myTurretFireTimeDelta = r.myTurretFireTimeDelta;
        myStaticError = r.myStaticError;
        myDynamicError = r.myDynamicError;

    }
    public override void MakeLinks()
    {
        base.MakeLinks();
        if ((MeshName & ParticleName) == 0xFFFFFFFF)
            stormdata_dll.StructError("weapon", FullName);
        EXPLOSION_INFO.SafeExplosionInfoMakeLinks(ExplOnEnd, "ExplOnEnd");
        EXPLOSION_INFO.SafeExplosionInfoMakeLinks(ExplOnTarget, "ExplOnTarget");
        for (int i = 0; i < 8; i++)
            EXPLOSION_INFO.SafeExplosionInfoMakeLinks(ExplOnGround[i], "ExplOnGround");
        if (XRadius != 0) XDamage /= Mathf.Pow(XRadius, 2);
        if (myTurretPauseTimeDelta == .0f) myTurretPauseTimeDelta = myTurretPauseTimeBase * .5f;
        if (myTurretFireTimeDelta == .0f) myTurretFireTimeDelta = myTurretFireTimeBase * .25f;

    }

    // defaults
    public virtual float GetLifeTime() { return 0; }
    public virtual float GetSpeed() { return 0; }
    public virtual float GetAmmoLoad() { return 0; }
    public virtual float GetReload() { return 0; }
    public virtual float GetAccel() { return 0; }
    public virtual float GetLightRadius() { return 0; }
    public virtual Color GetLightColor() { return Color.black; }
    //virtual const FVec4* GetLightColor() const;

    public virtual void GetAim(ref Vector3 TargetOrg, Vector3 TargetSpeed, MATRIX MyPos, Vector3 MySpeed)
    {
        // рассчитываем упреждение
        //# ifdef ImproveFiringConditions_Issue
        //float speedAddToWPN = MyPos.Dir * MySpeed;
        float speedAddToWPN = Vector3.Dot(MyPos.Dir, MySpeed);
        //#endif
        Vector3 dir = TargetOrg - MyPos.Org;
        //float dist = dir.Norma(); 
        float dist = dir.magnitude;
        if (dist == 0) return;
        dir /= dist;
        Vector3 tr = TargetSpeed;
        //Vector3 tmp = dir ^ tr;
        Vector3 tmp = Vector3.Cross(dir, tr);
        float f = tmp.magnitude;
        // определяем косинус угла наведения
        if (f > .1f)
        {
            //tmp /= f; tr = tmp ^ dir;
            tmp /= f; tr = Vector3.Cross(dir, tr);
            //# ifdef ImproveFiringConditions_Issue
            //f = Mathf.Sqr(TargetSpeed * tr / (GetSpeed() + speedAddToWPN));
            f = Mathf.Pow(Vector3.Dot(TargetSpeed, tr) / (GetSpeed() + speedAddToWPN), 2);
            //#else
            //f = sqr(TargetSpeed * tr / GetSpeed());
            //#endif

            if (f >= 1) return;
        }
        else
        {
            f = 0;
        }
        // определяем скорость продольного сближения снаряди и цели

        //# ifdef ImproveFiringConditions_Issue
        //f = sqrt(1.f - f) * (GetSpeed() + speedAddToWPN) - (TargetSpeed * dir);
        f = Mathf.Sqrt(1f - f) * (GetSpeed() + speedAddToWPN) - Vector3.Dot(TargetSpeed, dir);
        //#else
        //        f = sqrt(1.f - f) * GetSpeed() - (TargetSpeed * dir);
        //#endif
        // добавляем скорость_цели*время_полета_до_цели
        if (f > .0f) TargetOrg += TargetSpeed * (dist / f);

    }
    public float GetCraftEff(float Dist) { return (Dist < CraftEffectivnessDist ? CraftEffectivness1 : CraftEffectivness2); }
    public float GetNonCraftEff() { return (Damage + XDamage * Mathf.Pow(XRadius, 2)) / GetReload(); }
    // информация по сетевой обработке
    public bool SeparatedPacket;
    // информация по подвеске
    public WORD Type;
    public WORD Weight;
    public bool IsHuman;
    public bool IsChained;
    // информация по эффективности
    public float CraftEffectivness1;
    public float CraftEffectivnessDist;
    public float CraftEffectivness2;
    // информация по объекту, представляющему снаряд
    public DWORD MeshName;
    public DWORD ParticleName;
    // информация по повреждениям
    public float Damage;
    public float Range;
    public float XDamage, XRadius;
    // взрывы
    public EXPLOSION_INFO ExplOnEnd;
    public EXPLOSION_INFO ExplOnTarget;
    public EXPLOSION_INFO[] ExplOnGround = new EXPLOSION_INFO[8];
    // aim errors
    public float myTurretPauseTimeBase;
    public float myTurretPauseTimeDelta;
    public float myTurretFireTimeBase;
    public float myTurretFireTimeDelta;

    public float myStaticError;
    public float myDynamicError;

    public static void insertPlasmaData(READ_TEXT_STREAM st)
    {
        SUBOBJ_DATA.InsertSubobjData(new WPN_DATA_PLASMA(st.GetNextItem()), st.LineNumber()).Load(st);
    }

    public static void insertGunData(READ_TEXT_STREAM st)
    {
        SUBOBJ_DATA.InsertSubobjData(new WPN_DATA_GUN(st.GetNextItem()), st.LineNumber()).Load(st);
    }

    public static void insertRocketData(READ_TEXT_STREAM st)
    {
        SUBOBJ_DATA.InsertSubobjData(new WPN_DATA_ROCKET(st.GetNextItem()), st.LineNumber()).Load(st);
    }
    public static void insertMissileData(READ_TEXT_STREAM st)
    {
        SUBOBJ_DATA.InsertSubobjData(new WPN_DATA_MISSILE(st.GetNextItem()), st.LineNumber()).Load(st);
    }

    public static void insertProjectileData(READ_TEXT_STREAM st)
    {
        SUBOBJ_DATA.InsertSubobjData(new WPN_DATA(st.GetNextItem()), st.LineNumber()).Load(st);
    }

    public static void insertHTGRData(READ_TEXT_STREAM st)
    {
        SUBOBJ_DATA.InsertSubobjData(new WPN_DATA_HTGR(st.GetNextItem()), st.LineNumber()).Load(st);
    }

    public static void loadWeaponData(IMappedDb db)
    {
        string[] keys = { "Plasma", "Gun", "Rocket", "Missile", "HTGR", "Projectile" };
        LoadUtils.InsertCall[] calls = { insertPlasmaData, insertGunData, insertRocketData, insertMissileData, insertHTGRData, insertProjectileData };
        LoadUtils.parseMultiData(db, "weapon", "Weapons", "Weapons.txt", "[STORM WEAPON DATA FILE V1.1]", keys, calls);
    }

    WPN_DATA iSTORM_DATA<WPN_DATA>.GetByCodeLocal(uint Code, bool MustExist)
    {
        return (WPN_DATA)GetByCode(Code, MustExist);
    }
}
public class WpnDataDefines
{
    public const int WT_PLASMA = 0;
    public const int WT_GUN = 1;
    public const int WT_ROCKET = 2;
    public const int WT_MISSILE = 3;
    public const int WT_HTGR = 4;

    public const int WW_SMALL = 0;
    public const int WW_MEDIUM = 1;
    public const int WW_HEAVY = 2;
    public const int WW_STATIC = 3;

    public static string GetWTString(int id)
    {
        switch(id)
        {
            case WT_PLASMA: return ("WT_PLASMA");
            case WT_GUN: return ("WT_GUN");
            case WT_ROCKET: return ("WT_ROCKET");
            case WT_MISSILE: return ("WT_MISSILE");
            case WT_HTGR: return ("WT_HTGR");
        }

        return "UNKNOWN";
    }
}

/// <summary>
/// ствольное оружие
/// </summary>
public class WPN_DATA_BARREL : WPN_DATA
{
    // internal part
    public WPN_DATA_BARREL(string name) : base(name)
    {
        // автонаведение и разброс
        AutoAimLimit = 0;
        DispersionX = 0;
        DispersionY = 0;
        // вспышка
        Flash = 0xFFFFFFFF;
        // движение ствола
        Barrel = 0;
        RecoilDelta = 0;
        RecoilMod = 1;
        RotationSpeed = 0;
        RotationMod = 1;

    }
    public override void ProcessToken(READ_TEXT_STREAM st, string value)
    {
        do
        {
            // автонаведение и разброс
            if (st.LoadFloat(ref AutoAimLimit, "AutoAimLimit")) continue;
            if (st.LoadFloat(ref DispersionX, "DispersionY")) continue;
            if (st.LoadFloat(ref DispersionY, "DispersionX")) continue; ;
            if (st.Recognize("Dispersion"))
            {
                DispersionX = st.AtoF(st.GetNextItem());
                DispersionY = DispersionX;
                continue;
            }
            // вспышка
            if (st.LdHS(ref Flash, "Flash")) continue;
            // движение ствола
            if (st.LdCS(ref Barrel, "Barrel"))
            {
                continue;
            }
            if (st.Recognize("Recoil"))
            {
                RecoilDelta = st.AtoF(st.GetNextItem());
                RecoilMod = st.AtoF(st.GetNextItem());
                return;
            }
            if (st.Recognize("BarrelRotation"))
            {
                RotationSpeed = st.AtoF(st.GetNextItem());
                RotationMod = st.AtoF(st.GetNextItem());
                return;
            }
            base.ProcessToken(st, value);

        } while (false);
    }
    public override void Reference(SUBOBJ_DATA data)
    {
        base.Reference(data);
        // поля WPN_DATA
        if (data.GetClass() != SC_WEAPON_SLOT) return;
        WPN_DATA r = (WPN_DATA)data;
        // поля WPN_DATA_BARREL
        if (r.Type != WpnDataDefines.WT_PLASMA && r.Type != WpnDataDefines.WT_GUN && r.Type != WpnDataDefines.WT_ROCKET) return;
        WPN_DATA_BARREL rr = (WPN_DATA_BARREL)data;
        AutoAimLimit = rr.AutoAimLimit;
        DispersionX = rr.DispersionX;
        DispersionY = rr.DispersionY;
        Flash = rr.Flash;
        RecoilDelta = rr.RecoilDelta;
        RecoilMod = rr.RecoilMod;
        RotationSpeed = rr.RotationSpeed;
        RotationMod = rr.RotationMod;

    }
    public override void MakeLinks()
    {
        base.MakeLinks();
        AutoAimLimit = Storm.Math.GRD2RD(AutoAimLimit);
        DispersionX = Storm.Math.GRD2RD(DispersionX * .5f);
        DispersionY = Storm.Math.GRD2RD(DispersionY * .5f);
        RotationSpeed = Storm.Math.GRD2RD(RotationSpeed * 360f);
        RotationMod = RotationSpeed / RotationMod;

    }
    // автонаведение и разброс
    public float AutoAimLimit;
    public float DispersionX;
    public float DispersionY;
    // вспышка и гильзы
    public DWORD Flash;
    // движение ствола
    public DWORD Barrel;
    public float RecoilDelta;
    public float RecoilMod;
    public float RotationSpeed;
    public float RotationMod;



};
