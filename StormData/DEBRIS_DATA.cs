using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DWORD = System.UInt32;

public partial class DEBRIS_DATA : TLIST_ELEM<DEBRIS_DATA>
{
    private DEBRIS_DATA next, prev;
    public DEBRIS_DATA Next()
    {
        return next;
    }

    public DEBRIS_DATA Prev()
    {
        return prev;
    }

    public void SetNext(DEBRIS_DATA t)
    {
        next = t;
    }

    public void SetPrev(DEBRIS_DATA t)
    {
        prev = t;
    }

    public void Dispose()
    {
        //ExplsOnStart.Free();
        //EXPLOSION_INFO::SafeExplosionInfoDelete(ExplOnEnd);
        //EXPLOSION_INFO::SafeExplosionInfoDelete(ExplOnTarget);
        //for (int i = 0; i < 8; i++)
        //    EXPLOSION_INFO::SafeExplosionInfoDelete(ExplOnGround[i]);
    }
}
public partial class DEBRIS_DATA : STORM_DATA, iSTORM_DATA<DEBRIS_DATA>
{
    //static STDT_LIST_HELPER<DEBRIS_DATA> Datas("debris");
    public static StormDataHolder<DEBRIS_DATA> Datas = new StormDataHolder<DEBRIS_DATA>("debris");

    public const int DISAPPEAR_AFTER_LAUNCH = 1;
    public const int DISAPPEAR_AFTER_TOUCH = 2;
    public const int DEBRIS_ROTATE_NOTHING = 0;
    public const int DEBRIS_ROTATE_IMMEDIATELY = 1;
    public const int DEBRIS_ROTATE_CONTINUES = 2;
    public const int DEBRIS_COLLTYPE_ROUGH = 0;
    public const int DEBRIS_COLLTYPE_PRECISE = 1;
    public const int DEBRIS_DEFAULT_LIFE = 5;

    //  // internal part
    //  DEBRIS_DATA(const char*);
    //  virtual void ProcessToken(READ_TEXT_FILE&,const char*);
    //  virtual void Reference(const DEBRIS_DATA* ref);
    //virtual void MakeLinks();
    //  virtual ~DEBRIS_DATA();
    //  // data section
    //  float AppearProbability;
    //  float GoukeMin, FrictionCoeff, GoukeMax;
    //  const char* FileName;
    //  unsigned int ParticleOnFly;

    //  // случайные взрывы
    //  LIST ExplsOnStart;
    //  // взрывы
    //  EXPLOSION_INFO* ExplOnEnd;
    //  EXPLOSION_INFO* ExplOnTarget;
    //  EXPLOSION_INFO* ExplOnGround[8];

    //  float XDamage, XRadius;

    //  int CollisionMethod;                                  // сталкивается ли обломок с другими объектами
    //  int DisappearType;                                    // тип исчезновения
    //  int AlwaysLie;
    //  float MinDisappearTimer, MaxDisappearTimer;            // времена исчезновения
    //  float MaxAppearTimer, MinAppearTimer;

    //  int VisibleDisappearFlag;                             // тип исчезновения
    //  float Massa;                                          // масса
    //  float RotateOffset;                                   // смещение пивота
    //  float WaterGravity;                                   // плавучесть
    //  int JumpOnCreate;                                   // ставить ли на землю после создания
    //  VECTOR RotateAxis;                                    // ось вращения
    //  int RandomRotateAxis;                                 // случайная ли ось вращения
    //  int RotateType;                                       // тип вращения
    //  float RotateMaxSpeed, RotateAccel, RotateMinSpeed;      // скорости вращения
    //  int CheckCollisionType;                               // тип проверки на столкновение
    //  float AirFriction;                                    // величина трения
    //  int AirFrictionAffected;                             // влияет ли на нас трение об воздух
    //  int RotateSlowing;                                   // замедляется ли вращение
    //  float MaxSmokeTimer, MinSmokeTimer;                    // настройки испускания пыли
    //  int ShadowFlag;                                       // отбрасываем ли тень
    //  VECTOR SmokeOffset;                                   // смещения для дыма
    //  float DisappearSpeed;                                 // скорость исчезания под землю
    //  float EffectsProbability;                             // вероятность появления эффектов( партикль, звук)
    //                                                        // data access
    //  static STORM_DATA_API DEBRIS_DATA*  __cdecl GetByName(const char* Name,bool MustExist = true);
    //  static STORM_DATA_API DEBRIS_DATA*  __cdecl GetByCode(unsigned int Code, bool MustExist = true);
    //  static STORM_DATA_API DEBRIS_DATA*  __cdecl GetFirstItem();
    //  static STORM_DATA_API int __cdecl nItems();
    // internal part

    //virtual void ProcessToken(READ_TEXT_FILE&,const char*);
    //virtual void Reference(const DEBRIS_DATA* ref);
    //virtual void MakeLinks();

    // data section
    public float AppearProbability;
    public float GoukeMin, FrictionCoeff, GoukeMax;
    public string FileName;
    public uint ParticleOnFly;

    // случайные взрывы
    //LIST ExplsOnStart;
    public List<object> ExplsOnStart;
    // взрывы
    public EXPLOSION_INFO ExplOnEnd;
    public EXPLOSION_INFO ExplOnTarget;
    public EXPLOSION_INFO[] ExplOnGround = new EXPLOSION_INFO[8];

    public float XDamage, XRadius;

    public int CollisionMethod;                                  // сталкивается ли обломок с другими объектами
    public int DisappearType;                                    // тип исчезновения
    public int AlwaysLie;
    public float MinDisappearTimer, MaxDisappearTimer;            // времена исчезновения
    public float MaxAppearTimer, MinAppearTimer;

    public int VisibleDisappearFlag;                             // тип исчезновения
    public float Massa;                                          // масса
    public float RotateOffset;                                   // смещение пивота
    public float WaterGravity;                                   // плавучесть
    public int JumpOnCreate;                                   // ставить ли на землю после создания
    public Vector3 RotateAxis;                                    // ось вращения
    public int RandomRotateAxis;                                 // случайная ли ось вращения
    public int RotateType;                                       // тип вращения
    public float RotateMaxSpeed, RotateAccel, RotateMinSpeed;      // скорости вращения
    public int CheckCollisionType;                               // тип проверки на столкновение
    public float AirFriction;                                    // величина трения
    public int AirFrictionAffected;                             // влияет ли на нас трение об воздух
    public int RotateSlowing;                                   // замедляется ли вращение
    public float MaxSmokeTimer, MinSmokeTimer;                    // настройки испускания пыли
    public int ShadowFlag;                                       // отбрасываем ли тень
    public Vector3 SmokeOffset;                                   // смещения для дыма
    public float DisappearSpeed;                                 // скорость исчезания под землю
    public float EffectsProbability;                             // вероятность появления эффектов( партикль, звук)
                                                                 // data access

    public DEBRIS_DATA() : this("undefined") { }
    public DEBRIS_DATA(string name) : base(name)
    {
        // инитим новый итем
        AppearProbability = 1;
        FileName = "";
        ParticleOnFly = 0;
        CollisionMethod = 0;
        DisappearType = DISAPPEAR_AFTER_LAUNCH;
        MaxDisappearTimer = DEBRIS_DEFAULT_LIFE;
        MinDisappearTimer = DEBRIS_DEFAULT_LIFE;
        Massa = 0;
        MaxAppearTimer = 0;
        MinAppearTimer = 0;
        WaterGravity = 0.07f;
        JumpOnCreate = 0;
        AlwaysLie = 0;
        VisibleDisappearFlag = 1;
        GoukeMin = 0;
        GoukeMax = 0;
        RotateOffset = 0;
        FrictionCoeff = 0;
        RotateAxis.Set(0, 0, 0);
        RandomRotateAxis = 0;
        RotateAccel = 0;
        RotateType = DEBRIS_ROTATE_NOTHING;
        RotateMaxSpeed = 0;
        RotateMinSpeed = 0;
        CheckCollisionType = DEBRIS_COLLTYPE_ROUGH;
        AirFriction = 0;
        AirFrictionAffected = 0;
        DisappearSpeed = 0.5f;
        RotateSlowing = 1;
        MaxSmokeTimer = 0;
        MinSmokeTimer = 0;
        ShadowFlag = 0;
        SmokeOffset = Vector3.zero;

        EffectsProbability = 1;
        // взрывы
        XDamage = 0;
        XRadius = 0;
        ExplOnEnd = null;
        ExplOnTarget = null;
        for (int i = 0; i < 8; i++)
            ExplOnGround[i] = null;

        ExplsOnStart = new List<object>();
    }

    public override void ProcessToken(READ_TEXT_STREAM st, string c)
    {
        do
        {
            if (st.LdAS(ref FileName, "FileName")) continue;
            if (st.LoadFloat(ref AppearProbability, "AppearProbability")) continue;
            if (st.LoadInt(ref CollisionMethod, "CollisionMethod")) continue;
            if (st.Recognize("DisappearType"))
            {
                c = st.GetNextItem();
                if (c == "LAUNCH") { DisappearType = DISAPPEAR_AFTER_LAUNCH; continue; }
                if (c == "TOUCH") { DisappearType = DISAPPEAR_AFTER_TOUCH; continue; }
                stormdata_dll.ParseError("Debris parse error", "type");
            }
            if (st.Recognize("RotateType"))
            {
                c = st.GetNextItem();
                if (c == "IMM") { RotateType = DEBRIS_ROTATE_IMMEDIATELY; continue; }
                if (c == "CONT") { RotateType = DEBRIS_ROTATE_CONTINUES; continue; }
                stormdata_dll.ParseError("Debris parse error", "type");
            }
            if (st.Recognize("CheckCollisionType"))
            {
                c = st.GetNextItem();
                if (c == "PRECISE") { CheckCollisionType = DEBRIS_COLLTYPE_PRECISE; continue; }
                if (c == "ROUGH") { CheckCollisionType = DEBRIS_COLLTYPE_ROUGH; continue; }
                stormdata_dll.ParseError("Debris parse error", "type");
            }
            if (st.LoadFloat(ref RotateOffset, "RotateOffset")) continue;
            if (st.LoadFloat(ref WaterGravity, "WaterGravity")) continue;
            if (st.LoadFloat(ref DisappearSpeed, "DisappearSpeed")) continue;
            if (st.LoadFloat(ref RotateMaxSpeed, "RotateMaxSpeed")) continue;
            if (st.LoadFloat(ref RotateMinSpeed, "RotateMinSpeed")) continue;
            if (st.LoadInt(ref RandomRotateAxis, "RandomRotateAxis")) continue;
            if (st.Recognize("RotateAxis"))
            {
                RotateAxis.x = st.AtoF(st.GetNextItem());
                RotateAxis.y = st.AtoF(st.GetNextItem());
                RotateAxis.z = st.AtoF(st.GetNextItem());
                continue;
            }
            if (st.Recognize("SmokeOffset"))
            {
                RotateAxis.x = st.AtoF(st.GetNextItem());
                RotateAxis.y = st.AtoF(st.GetNextItem());
                RotateAxis.z = st.AtoF(st.GetNextItem());
                continue;
            }
            if (st.LoadFloat(ref XDamage, "XDamage")) continue;
            if (st.LoadFloat(ref XRadius, "XRadius")) continue; ;
            if (st.Recognize("ExplOnStart")) { ExplsOnStart.Add(Hasher.HshString(st.GetNextItem())); continue; }
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

            if (st.LoadFloat(ref MaxDisappearTimer, "MaxDisappearTimer")) continue;
            if (st.LoadFloat(ref MinDisappearTimer, "MinDisappearTimer")) continue;
            if (st.LoadFloat(ref MaxAppearTimer, "MaxAppearTimer")) continue;
            if (st.LoadFloat(ref MinAppearTimer, "MinAppearTimer")) continue;
            if (st.LoadInt(ref VisibleDisappearFlag, "VisibleDisappearFlag")) continue;
            if (st.LoadInt(ref ShadowFlag, "ShadowFlag")) continue;
            if (st.LoadInt(ref RotateSlowing, "RotateSlowing")) continue;
            if (st.LoadFloat(ref RotateOffset, "RotateOffset")) continue;
            if (st.LoadFloat(ref WaterGravity, "WaterGravity")) continue;
            if (st.LoadInt(ref JumpOnCreate, "JumpOnCreate")) continue;
            if (st.LoadInt(ref AlwaysLie, "AlwaysLie")) continue;
            if (st.LoadFloat(ref GoukeMin, "GoukeMin")) continue;
            if (st.LoadFloat(ref GoukeMax, "GoukeMax")) continue;
            if (st.LoadFloat(ref FrictionCoeff, "FrictionCoeff")) continue;
            if (st.LoadFloat(ref Massa, "Massa")) continue;
            if (st.LoadFloat(ref RotateAccel, "RotateAccel")) continue;
            if (st.LoadFloat(ref AirFriction, "AirFriction")) continue;
            if (st.LoadInt(ref AirFrictionAffected, "AirFrictionAffected")) continue;
            if (st.LoadFloat(ref MaxSmokeTimer, "MaxSmokeTimer")) continue;
            if (st.LoadFloat(ref MinSmokeTimer, "MinSmokeTimer")) continue;
            if (st.LdHS(ref ParticleOnFly, "ParticleOnFly")) continue;
            if (st.LoadFloat(ref EffectsProbability, "EffectsProbability")) continue;
            if (st.Recognize("Reference"))
            {
                Reference(GetByName(st.GetNextItem()));
                return;
            }
            base.ProcessToken(st, c);
        } while (false);
    }

    public void Reference(DEBRIS_DATA refdata)
    {
        AppearProbability = refdata.AppearProbability;
        GoukeMin = refdata.GoukeMin;
        GoukeMax = refdata.GoukeMax;
        FrictionCoeff = refdata.FrictionCoeff;
        FileName = refdata.FileName;
        ParticleOnFly = refdata.ParticleOnFly;
        CollisionMethod = refdata.CollisionMethod;
        DisappearType = refdata.DisappearType;
        MinDisappearTimer = refdata.MinDisappearTimer;
        MaxDisappearTimer = refdata.MaxDisappearTimer;
        MaxAppearTimer = refdata.MaxAppearTimer;
        MinAppearTimer = refdata.MinAppearTimer;
        VisibleDisappearFlag = refdata.VisibleDisappearFlag;
        RotateOffset = refdata.RotateOffset;
        WaterGravity = refdata.WaterGravity;
        JumpOnCreate = refdata.JumpOnCreate;
        AlwaysLie = refdata.AlwaysLie;
        Massa = refdata.Massa;
        RotateAxis = refdata.RotateAxis;
        RandomRotateAxis = refdata.RandomRotateAxis;
        RotateType = refdata.RotateType;
        RotateMaxSpeed = refdata.RotateMaxSpeed;
        RotateAccel = refdata.RotateAccel;
        RotateMinSpeed = refdata.RotateMinSpeed;
        CheckCollisionType = refdata.CheckCollisionType;
        AirFriction = refdata.AirFriction;
        AirFrictionAffected = refdata.AirFrictionAffected;
        RotateSlowing = refdata.RotateSlowing;
        MaxSmokeTimer = refdata.MaxSmokeTimer;
        MinSmokeTimer = refdata.MinSmokeTimer;
        ShadowFlag = refdata.ShadowFlag;
        SmokeOffset = refdata.SmokeOffset;
        DisappearSpeed = refdata.DisappearSpeed;
        EffectsProbability = refdata.EffectsProbability;
        //взрывы
        XRadius = refdata.XRadius;
        XDamage = refdata.XDamage;
        // переписываем ExplsOnStart
        foreach (var le in ExplsOnStart)
        {
            ExplsOnStart.Add(le);
        }

        ExplOnEnd = EXPLOSION_INFO.SafeExplosionInfoCopy(refdata.ExplOnEnd);
        ExplOnTarget = EXPLOSION_INFO.SafeExplosionInfoCopy(refdata.ExplOnTarget);
        for (int i = 0; i < 8; i++)
            ExplOnGround[i] = EXPLOSION_INFO.SafeExplosionInfoCopy(refdata.ExplOnGround[i]);
    }

    public static DEBRIS_DATA GetByName(string Name, bool MustExist = true)
    {
        uint Code = Constants.THANDLE_INVALID;
        if (Name != "") Code = Hasher.HshString(Name);
        //if (Name != "") Code = Hasher.CodeString(Name);
        return GetByCode(Code, MustExist);
    }

    public static DEBRIS_DATA GetByCode(DWORD Code, bool MustExist = true)
    {
        foreach (DEBRIS_DATA data in Datas)
        {
            if (data.Name == Code) return data;
        }
        if (MustExist) throw new System.Exception($"Could not get DEBRIS_DATA {Code:X8}");
        return null;
    }
    public static void insertDebrisData(READ_TEXT_STREAM st)
    {
        DEBRIS_DATA dData = new DEBRIS_DATA(st.GetNextItem());
        StormLog.LogMessage("Loading debris " + dData.FullName + " [OK]", StormLog.logPriority.DEBUG);
        Datas.InsertItem(dData, st.LineNumber()).Load(st);
    }

    //public static void loadDebrData(int code, PackType db)
    //{
    //    switch (code)
    //    {
    //        case 0:
    //            LoadUtils.parseData(db, Datas.GetItemName(), "Debrises", "Debrises.txt", "[STORM DEBRIS DATA FILE V1.1]", "Debris", insertDebrisData);
    //            return;
    //        case 1:
    //            Datas.MakeLinks();
    //            return;
    //    }
    //}
    public static void loadDebrData(int code, IMappedDb db)
    {
        switch (code)
        {
            case 0:
                LoadUtils.parseData(db, Datas.GetItemName(), "Debrises", "Debrises.txt", "[STORM DEBRIS DATA FILE V1.1]", "Debris", insertDebrisData);
                return;
            case 1:
                Datas.MakeLinks();
                return;
        }
    }

    public DEBRIS_DATA GetByCodeLocal(uint Code, bool MustExist = true)
    {
        return DEBRIS_DATA.GetByCode(Code, MustExist);
    }

    public override void MakeLinks()
    {
        RotateMaxSpeed = Storm.Math.GRD2RD(RotateMaxSpeed);
        RotateMinSpeed = Storm.Math.GRD2RD(RotateMinSpeed);
        RotateAccel = Storm.Math.GRD2RD(RotateAccel);
        RotateAxis.Normalize();
        for (int i = 0; i < ExplsOnStart.Count; i++)
        {
            if (ExplsOnStart[i] != null) ExplsOnStart[i] = (object)EXPLOSION_DATA.GetByCode((uint)ExplsOnStart[i]);
            //{ if (le->Data()) le->SetData((void*)EXPLOSION_DATA::GetByCode((unsigned int)le->Data())); }
        }

        //EXPLOSION_INFO::SafeExplosionInfoMakeLinks(&ExplOnEnd, "ExplOnEnd");
        //EXPLOSION_INFO::SafeExplosionInfoMakeLinks(&ExplOnTarget, "ExplOnTarget");
        //for (int i = 0; i < 8; i++)
        //    EXPLOSION_INFO::SafeExplosionInfoMakeLinks(ExplOnGround + i, "ExplOnGround");
        //if (XRadius != 0) XDamage /= Mathf.Pow(XRadius, 2);
    }
}

