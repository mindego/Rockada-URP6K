using UnityEngine;
using DWORD = System.UInt32;


public class BaseSkill : IQuery
{
    public const string sNovice = "Novice";
    public const string sVeteran = "Veteran";
    public const string sElite = "Elite";

    // fire
    public float valFireAimError;            // злобность стрельбы
    public float valTurretFireAimError;      // злобность стрельбы турелей

    // query
    public const uint ID = 0x29B408C4;
    public virtual object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case BaseSkill.ID: return this;
            default: return null;
        }

    }
    //template<class C> C* Query() { return (C*)Query(C::ID); }

    public BaseSkill(iUnifiedVariableContainer skill)
    {
        valFireAimError = LFV(skill, "FireAimError", 0.5f);
        valTurretFireAimError = LFV(skill, "TurretFireAimError", 0.5f);
    }
    ~BaseSkill() { Dispose(); }
    public virtual void Dispose() { }

    //LoadFloatValue?
    protected float LFV(iUnifiedVariableContainer skill, string name, float def_value)
    {
        float data = def_value;
        if (skill != null)
            skill.getFloat(name, ref data, def_value);
        return data;
    }

    public static iUnifiedVariableContainer GetSkillContainer(DWORD skill)
    {
        iUnifiedVariableContainer skills = stdlogic_dll.mpAiData != null ? stdlogic_dll.mpAiData.GetVariableTpl<iUnifiedVariableContainer>("Skills") : null;
        return skills != null ? skills.GetVariableTpl<iUnifiedVariableContainer>(GetSkillName(skill)) : null;
    }

    public static string GetSkillName(DWORD name)
    {
        switch (name)
        {
            case SkillDefines.SKILL_NOVICE: return sNovice;
            case SkillDefines.SKILL_VETERAN: return sVeteran;
            case SkillDefines.SKILL_ELITE: return sElite;
        }
        return Parsing.sAiEmpty;
    }

    public iUnifiedVariableContainer GetOrCreateDir(iUnifiedVariableContainer skill, string name)
    {
        return skill !=null ? skill.createContainer(name) : null;
    }
};

public class BaseCraftSkill : BaseSkill
{
    // avoiding bullets
    public float valAvoidStartMin;           // значение threat при котором начинается уклонение
    public float valAvoidStartBnd;           // границы колебания значение threat 
    public float valMissileAvoidStartMin;    // значение threat при котором начинается уклонение от ракет
    public float valMissileAvoidStartBnd;    // границы колебания значение threat 

    // checkpoints
    public float valCheckPointMin;           // значение интервала checkpoint
    public float valCheckPointBnd;           // границы колебания valCheckPointLen

    // visibility parameters
    public float valAngleVisibilityMin;      // угол видимости цели
    public float valAngleAlmostAhead;        // угол лобовой атака
    public float valAngleAhead;              // угол передней полусферы
    public float valAngleMiddle;             // цель сбоку

    // firing
    public float valFireAimMin;              // коэффициент открытия огня (в радиусах цели)
    public float valFireAimBnd;              // границы изменения valFireAimMin
    public float valFireDistanceMin;         // дистанция открытия огня
    public float valFireDistanceBnd;         // границы изменения valFireDistanceMin

    public float valFireRocketBurstTimeMin;     // время одного раектного залпа
    public float valFireRocketBurstTimeBnd;     // границы изменения valFireRocketBurstTimeMin
    public float valFireCannonBurstTimeMin;     // время одного пушечного залпа
    public float valFireCannonBurstTimeBnd;     // границы изменения valFireCannonBurstTimeMin
    public float valFireRelaxMin;             // время паузы между залпами
    public float valFireRelaxBnd;             // границы изменения valFireRelaxMin

    public float valControlScale;            // маневренность
    public float valMissileCheckTimeMin;     // периодичность проверки на ракетную угрозу
    public float valMissileCheckTimeBnd;     // границы изменения valMissileCheckTimeMin

    // search
    public float valTargetSearchMin;         // время поиска потерянной цели
    public float valTargetSearchBnd;         // границы изменения valTargetSearchMin

    public float valMissileAvoidTimeMin;
    public float valMissileAvoidTimeBnd;

    // query
    new public const uint ID = 0xB9332272;
    public override object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case BaseCraftSkill.ID: return this;
            default: return base.Query(cls_id);
        }
    }


    public BaseCraftSkill(iUnifiedVariableContainer skill) : base(skill)
    {
        iUnifiedVariableContainer craft_skill = GetOrCreateDir(skill, "Craft");
        valCheckPointMin = LFV(craft_skill, "CheckPointMin", 0.4f);     // время проверки и изменения параметров
        valCheckPointBnd = LFV(craft_skill, "CheckPointBnd", 0.1f);     // границы изменения valCheckPointMin

        // avoiding bullets
        valAvoidStartMin = LFV(craft_skill, "AvoidStartMin", 555f);    // значение threat при котором начинается уклонение
        valAvoidStartBnd = LFV(craft_skill, "AvoidStartBnd", 0f);      // границы изменения valAvoidStartBnd
        valMissileAvoidStartMin = LFV(craft_skill, "MAvoidStartMin", 555f);    // значение threat при котором начинается уклонение
        valMissileAvoidStartBnd = LFV(craft_skill, "MAvoidStartBnd", 0f);      // границы изменения valAvoidStartBnd


        // firing
        valFireAimMin = LFV(craft_skill, "FireAimMin", 0.9f);             // коэффициент открытия огня (в радиусах цели)
        valFireAimBnd = LFV(craft_skill, "FireAimBnd", 0.1f);             // границы изменения valFireAimMin
        valFireDistanceMin = LFV(craft_skill, "FireDistanceMin", 1f);    // дистанция открытия огня
        valFireDistanceBnd = LFV(craft_skill, "FireDistanceBnd", 0.05f);  // границы изменения valFireDistanceMin

        valFireRocketBurstTimeMin = LFV(craft_skill, "FireRocketBurstTimeMin", 5f);  // время одного раектного залпа
        valFireRocketBurstTimeBnd = LFV(craft_skill, "FireRocketBurstTimeBnd", 01f); // границы изменения valFireRocketBurstTimeMin
        valFireCannonBurstTimeMin = LFV(craft_skill, "FireCannonBurstTimeMin", 3f);  // время одного пушечного залпа
        valFireCannonBurstTimeBnd = LFV(craft_skill, "FireCannonBurstTimeBnd", 2f);  // границы изменения valFireCannonBurstTimeMin
        valFireRelaxMin = LFV(craft_skill, "FireRelaxMin", 1f);  // время паузы между залпами
        valFireRelaxBnd = LFV(craft_skill, "FireRelaxBnd", 1f);   // границы изменения valFireRelaxMin

        // maneur
        valControlScale = LFV(craft_skill, "ControlScale", 0.75f);  // маневренность крафта

        // missile
        valMissileCheckTimeMin = LFV(craft_skill, "MissileCheckTimeMin", 1f); // периодичность проверки на ракетную угрозу
        valMissileCheckTimeBnd = LFV(craft_skill, "MissileCheckTimeBnd", 0.5f); // границы изменения valMissileCheckTimeMin

        valMissileAvoidTimeMin = LFV(craft_skill, "MissileAvoidTimeMin", 30);
        valMissileAvoidTimeBnd = LFV(craft_skill, "MissileAvoidTimeBnd", 1.0f);

        // search
        valTargetSearchMin = LFV(craft_skill, "TargetSearchMin", 35f);            // время поиска потерянной цели
        valTargetSearchBnd = LFV(craft_skill, "TargetSearchBnd", 5f);             // границы изменения valTargetSearchMin

        // visibility parameters
        valAngleVisibilityMin = Mathf.Cos(Storm.Math.GRD2RD(90f));
        valAngleAlmostAhead = Mathf.Cos(Storm.Math.GRD2RD(8f));
        valAngleAhead = Mathf.Cos(Storm.Math.GRD2RD(45f));
        valAngleMiddle = Mathf.Cos(Storm.Math.GRD2RD(135f));
    }
    ~BaseCraftSkill() { }
}
public class BFSkill : BaseCraftSkill
{
    // target tactics
    public float valTargetSearchAltitude;    // высота взлета на поверхностью при потере цели

    // threat avoid for BF type
    public float valTargetStrafeTimeMin;     // время смены направления смещения
    public float valTargetStrafeTimeBnd;     // границы изменения valTargetStrafeTimeMin

    public float valTargetStrafeIntensityMin; // интенсивность смещения
    public float valTargetStrafeIntensityBnd; // границы изменения valTargetStrafeIntensityMin

    public float valTargetLeaderDelta;        // значение колебания Delta, если мы находимся в строю

    public float valAwayThreatFactor;         // значение threat при котором отходим
    public float valAwayThreatMul;            // значение на которое умножается Range оружия при определении расстояния с которого нужно отходить 

    public float valStayOnSixThreatFactor;    // пороговое значение threat при котором отстаемся на хвосте


    // threat avoid for plane type
    public float valThreatAmplitudeMin;       // амплитуда колебания по осям
    public float valThreatPeriodMin;          // период колебания  по осям
    public float valThreatSpeed;              // скорость изменения вектора
    public float valThreatLeaderAmplitudeMin;  // амлитуда колебания если мы находимся в строю
    public float valThreatLeaderSpeed;        // скорость изменения вектора если мы находимся в строю

    // query
    new public const uint ID = 0x31D891CE;
    public override object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case BFSkill.ID: return this;
            default: return base.Query(cls_id);
        }
    }

    public BFSkill(iUnifiedVariableContainer skill) : base(skill)
    {
        iUnifiedVariableContainer bf_skill = GetOrCreateDir(skill, "Craft");
        // target tactics
        valTargetSearchAltitude = LFV(bf_skill, "TargetSearchAltitude", 150f); // высота взлета на поверхностью при потере цели

        // threat avoid for BF type
        valTargetStrafeTimeMin = LFV(bf_skill, "TargetStrafeTimeMin", 3f);       // граница смещения по оси X
        valTargetStrafeTimeBnd = LFV(bf_skill, "TargetStrafeTimeBnd", 1f);       // граница смещения по оси Y
        valTargetStrafeIntensityMin = LFV(bf_skill, "TargetStrafeIntensityMin", 0f); // интенсивность смещения
        valTargetStrafeIntensityBnd = LFV(bf_skill, "TargetStrafeIntensityBnd", 0f); // границы изменения valTargetStrafeIntensityMin
        valTargetLeaderDelta = LFV(bf_skill, "TargetLeaderDelta", 0f);        // значение колебания Delta, если мы находимся в строю
        valAwayThreatFactor = LFV(bf_skill, "AwayThreatFactor", 1.8f);        // значение колебания Delta, если мы находимся в строю 
        valAwayThreatMul = LFV(bf_skill, "AwayThreatMul", 0.7f);           // значение на которое умножается Range оружия при определении расстояния с которого нужно отходить    
        valStayOnSixThreatFactor = LFV(bf_skill, "StayOnSixThreatFactor", 1.2f);         // пороговое значение threat при котором отстаемся на хвосте

        // threat avoid for plane type
        valThreatAmplitudeMin = LFV(bf_skill, "ThreatAmplitudeMin", 100f);            // амплитуда колебания по осям
        valThreatPeriodMin = LFV(bf_skill, "ThreatPeriodMin", 1000f);              // период колебания  по осям
        valThreatSpeed = LFV(bf_skill, "ThreatSpeed", 0.15f);                   // скорость изменения вектора
        valThreatLeaderAmplitudeMin = LFV(bf_skill, "ThreatLeaderAmplitudeMin", 20f); // амлитуда колебания если мы находимся в строю
        valThreatLeaderSpeed = LFV(bf_skill, "ThreatLeaderSpeed", 50f);            // скорость изменения вектора если мы находимся в строю
    }
    ~BFSkill() { }
};

