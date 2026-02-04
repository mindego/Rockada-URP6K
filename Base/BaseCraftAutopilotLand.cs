using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// BaseCraftAutopilotLand - посадка
/// </summary>
class BaseCraftAutopilotLand : BaseCraftAutopilotFlyTo
{
    const float gsLandGlissadeDeltaCos = .9f; // cos(25)
    const float gsLandDownwindDistCraft = 400;
    const float gsLandDownwindDistPlane = 600; 
    const float gsLandCrosswindDistCraft = 1400;
    const float gsLandCrosswindDistPlane = 2400;
    const float gsLandDownwindSpeed = 420 * 0.2778f;//KPH2MPS(420);
    const float gsLandRunBrakeAceel = 8;

    const float gsInterpolationLag = .5f;
    const float gsMaxPacketDelay = .3f;

    // для кверенья
    new public const uint ID = 0x8389CC9C;
    public override object GetInterface(DWORD id)
    {
        return (id == ID ? this : null);
    }

    // интерфейс с BaseCraft
    public BaseCraftAutopilotLand(BaseCraft cr, iContact c, BaseHangar h, bool final) : base(cr)
    {
        h.setStatus((cr.GetProtoType() == UnitDataDefines.UT_FLYING_FAST) ? HangarStatus.hsIntLanding : HangarStatus.hsBFLanding);
        mHangarHandle = c.GetHandle();
        mPos[0] = h.GetPointForStart(); mPos[0].y -= Owner.GetMinY();
        mPos[1] = h.GetPointFor(Owner.pContact, true); mPos[1].y -= Owner.GetMinY();

        //Owner.rScene.getSV()->CreatePath(mPos[1],mPos[1]+VDir,FVec4(1,1,1,0),90);

        mDir = mPos[0] - mPos[1]; mDir.Normalize();
        mRight = Vector3.Cross(Vector3.up, mDir); mRight.Normalize();
        HangarSlotData data = h.getDataFor(Owner.pContact);
        Asserts.AssertEx(data != null);
        calcGlissadeVector(out mGlissDir, -mDir, -data.myInfo.myLandAngle);
        //SetVector(mGlissDir,atan2(-mDir.x,-mDir.z),Asin(-mDir.y)+data->myInfo->myLandAngle);
        mPos[2] = mPos[1] + mGlissDir * (Owner.Dt().IsPlane ? gsLandCrosswindDistPlane : gsLandCrosswindDistCraft);
        myFlyDontUp = cr.rScene.GetTime() + 2;
        if (final) StartFinal(h); else StartCrosswind(h);
    }

    // SetVector - воспомогательная ф-ция
    private static void SetVector(out Vector3 Res, float Hdg, float Pitch)
    {
        Res = Vector3.zero;
        Storm.Math.SinCos(Hdg, out Res.x, out Res.z);
        Res.y = Mathf.Sin(Pitch);
        float c = Mathf.Cos(Pitch);
        Res.x *= c;
        Res.z *= c;
    }
    public static void calcGlissadeVector(out Vector3 res, Vector3 gliss, float angle)
    {
        SetVector(out res, Mathf.Atan2(gliss.x, gliss.z), Mathf.Asin(gliss.y) - angle);
    }

    public override void Dispose()
    {
        if (Owner.IsLocal())
        { // если мы управляли - закроем дверь
            iContact pHng = Owner.rScene.GetContact(mHangarHandle);
            BaseHangar h = (pHng != null ? (BaseHangar)pHng.GetInterface(BaseHangar.ID) : null);
            if (h != null)
            {
                h.CloseDoor();
                h.setStatus(HangarStatus.hsClosed);
            }
        }

        if (Owner.IsPlayedByHuman() && mStage == LandingStages.lsWait)
        {
            float delta = Owner.rScene.GetTime() - Owner.getTakeoffTime();
            int h, m, s, ms;
            extractTime(delta, out h, out m, out s, out ms);
            Owner.rScene.Message("FlightTime=%.2d:%.2d:%.2d.%.3d", h.ToString(), m.ToString(), s.ToString(), ms.ToString());
        }
    }

    static void extractTime(float time, out int hour, out int minute, out int second, out int msecond)
    {
        msecond = (int)(time - Mathf.Floor(time)) * 1000;
        Asserts.AssertBp(msecond >= 0);

        hour = (int)time / 3600;
        if (hour != 0) time -= hour * 3600;
        minute = (int)time / 60;
        if (minute != 0) time -= minute * 60;
        second = (int)time;
    }
    ~BaseCraftAutopilotLand()
    {
        Dispose();
    }
    public override bool Move(float scale, bool pred)
    {
        if (pred == true) return true;
        // получаем ангар
        iContact pHng = Owner.rScene.GetContact(mHangarHandle);
        BaseHangar h = (pHng != null ? (BaseHangar)pHng.GetInterface(BaseHangar.ID) : null);
        // проверка дееспособности
        if (h == null || Owner.GetCondition() <= .0f)
        {
            if (mHangarHandle != Constants.THANDLE_INVALID)
            {
                Owner.GetRoot().AddDamage(Owner.GetHandle(), iBaseVictim.WeaponCodeUltimateDeath, 0);
                mHangarHandle = Constants.THANDLE_INVALID;
            }
            if (h != null)
                h.CloseDoor();
        }
        // фазы
        Color[] colors = new Color[] { Color.red, Color.green, Color.blue };
        //Debug.Log("mStage: " + mStage);
        for(int i=0;i<mPos.Length;i++)
        {
            Debug.DrawLine(Engine.ToCameraReference(Owner.pFPO.Org),
                Engine.ToCameraReference(mPos[i]),
                colors[i]
                );
        }
        switch (mStage)
        {
            case LandingStages.lsCrosswind: return ProcessCrosswind(h, scale);
            case LandingStages.lsDownwind: return ProcessDownwind(h, scale);
            case LandingStages.lsFinal: return ProcessFinal(h, scale);
            case LandingStages.lsRun: return ProcessRun(h, scale);
            default: return ProcessWait(h, scale);
        }
    }
    public override int GetState()
    {
        return (mStage < LandingStages.lsFinal ? iSensorsDefines.CS_IN_GAME : iSensorsDefines.CS_ENTERING_HANGAR);
    }

    // свое
    protected DWORD mHangarHandle;
    //TODO реализовать по необходимости сглаживалку
    protected SmootherOrg1 mSmoother = new SmootherOrg1(); 
    protected LandingStages mStage;
    Vector3[] mPos = new Vector3[3];
    Vector3 mDir;
    Vector3 mRight;
    Vector3 mGlissDir;
    protected void StartCrosswind(BaseHangar h)
    {
        mStage = LandingStages.lsCrosswind;
    }
    protected bool ProcessCrosswind(BaseHangar h, float scale)
    {
        // проверяем условие перехода в следующее состояние
        TgtSpeed = Owner.pFPO.Org - mPos[1];
        float d = Vector3.Dot(TgtSpeed, mGlissDir);
        float d1 = (Owner.Dt().IsPlane ? gsLandCrosswindDistPlane : gsLandCrosswindDistCraft);
        if (d > d1 - 400 && d < d1 + 500 && d > gsLandGlissadeDeltaCos * TgtSpeed.magnitude)
        {
            StartDownwind(h);
            return Move(scale, false);
        }
        // летим к точке захода
        TgtSpeed = mPos[2] - Owner.pFPO.Org;
        d = TgtSpeed.magnitude;
        TgtDir = TgtSpeed / d;
        TgtSpeed.Set(0, 0, (d < 1000 ? Storm.Math.KPH2MPS(500) : Owner.Dt().MaxSpeed.z * .8f));
        Owner.AutopilotDeltaY = 50;
        Owner.AutopilotPredictionTime = 6;
        return base.DoMove(scale, Owner.AutopilotPredictionTime, false, false);
    }
    protected void StartDownwind(BaseHangar h)
    {
        mStage = LandingStages.lsDownwind;
    }

    protected bool ProcessDownwind(BaseHangar h, float scale)
    {
        // проверяем условие перехода в следующее состояние
        TgtSpeed = Owner.pFPO.Org - mPos[1];
        float d = Vector3.Dot(TgtSpeed, mGlissDir);
        float d1 = (Owner.Dt().IsPlane ? gsLandDownwindDistPlane : gsLandDownwindDistCraft);
        if (d < d1)
        {
            StartFinal(h);
            return Move(scale, false);
        }
        // летим к точке касания
        TgtSpeed = mPos[1] - Owner.pFPO.Org;
        d = TgtSpeed.magnitude;
        TgtDir = TgtSpeed / d;
        TgtDir += mRight * (Vector3.Dot(TgtDir, mRight) * 1.25f);
        TgtDir.Normalize();
        TgtSpeed.Set(0, 0, gsLandDownwindSpeed);
        Owner.AutopilotDeltaY = 50;
        Owner.AutopilotPredictionTime = 4;
        return base.DoMove(scale, Owner.AutopilotPredictionTime, false, false);
    }
    protected void StartFinal(BaseHangar h)
    {
        if (h == null) { Debug.LogError("hangar is null!!!!"); StartCrosswind(h); return; }
        h.OpenDoor();
        float d = (Owner.pFPO.Org - mPos[1]).magnitude;
        float t;
        float s;
        GetLandParams(out s, out t);
        Vector3 TgtSpd = mDir * s;
        t = 2 * d / (Owner.SpeedF + s);

        mSmoother.SetScale(Owner.rScene.GetTime(), Owner.rScene.GetTime() + t);
        mSmoother.SetOrg(Owner.pFPO.Org, Owner.Speed, mPos[1], TgtSpd);
        mStage = LandingStages.lsFinal;
    }

    protected bool ProcessFinal(BaseHangar h, float scale)
    {
        if (Owner.rScene.GetTime() > mSmoother.GetTime1())
        {
            StartRun(h);
            return Move(scale, false);
        }
        // летим к точке касания
        float s = mSmoother.GetScale(Owner.rScene.GetTime());
        mSmoother.GetOrg(ref Owner.pFPO.Org, s);
        mSmoother.GetSpeed(ref Owner.Speed, s); Owner.SpeedF = Owner.Speed.magnitude;
        TurnToSpeed(scale);
        Owner.MakeMove(0, false);
        // устанавливаем тягу
        Vector3 Acc=Vector3.zero;
        mSmoother.GetAccel(ref Acc, s);
        Acc.y += 9.8f; Acc *= Owner.Dt().W;
        Vector3 Thrust;
        Thrust.x = Mathf.Clamp(Vector3.Dot(Acc, Owner.GetFpo().Right) / Owner.getTP().x, -1, 1);
        Thrust.y = Mathf.Clamp(Vector3.Dot(Acc, Owner.GetFpo().Up) / Owner.getTP().y, -1, 1);
        Thrust.z = Mathf.Clamp(Vector3.Dot(Acc, Owner.GetFpo().Dir) / Owner.getTP().z, -1, 1);
        Owner.SetThrust(Thrust, scale);
        return true;
    }
    protected void StartRun(BaseHangar h)
    {
        float t;
        GetLandParams(out Owner.SpeedF, out t);
        // ставим в точку касания
        Owner.pFPO.Org = mPos[1];
        Owner.Speed = mDir * Owner.SpeedF;
        // рассчитываем параметры торможения
        Vector3 TgtSpd = Vector3.zero;
        mSmoother.SetScale(mSmoother.GetTime1(), mSmoother.GetTime1() + t);
        mSmoother.SetOrg(Owner.pFPO.Org, Owner.Speed, mPos[0], TgtSpd);
        mStage = LandingStages.lsRun;
    }
    protected bool ProcessRun(BaseHangar h, float scale)
    {
        // проверка конца фазы
        if (Owner.rScene.GetTime() > mSmoother.GetTime1())
        {
            StartWait(h);
            return Move(scale, false);
        }
        // летим к точке касания
        float s = mSmoother.GetScale(Owner.rScene.GetTime());
        mSmoother.GetOrg(ref Owner.pFPO.Org, s);
        mSmoother.GetSpeed(ref Owner.Speed, s); Owner.SpeedF = Owner.Speed.magnitude;
        TurnToSpeed(scale);
        Owner.MakeMove(0, false);
        // устанавливаем тягу
        Vector3 Acc = Vector3.zero;
        mSmoother.GetAccel(ref Acc, s);
        Acc *= Owner.Dt().W;
        Vector3 Thrust = Vector3.zero;
        Thrust.x = Mathf.Clamp(Vector3.Dot(Acc, Owner.GetFpo().Right) / Owner.getTP().x, -1, 1);
        Thrust.y = Mathf.Clamp(Vector3.Dot(Acc, Owner.GetFpo().Up) / Owner.getTP().y, -1, 1);
        Thrust.z = Mathf.Clamp(Vector3.Dot(Acc, Owner.GetFpo().Dir) / Owner.getTP().z, -1, 1);
        Owner.SetThrust(Thrust, scale);
        return true;
    }
    protected void StartWait(BaseHangar h)
    {
        if (h != null) h.CloseDoor();
        mStage = LandingStages.lsWait;
    }
    protected bool ProcessWait(BaseHangar h, float scale)
    {
        // устанавливаем тягу
        Vector3 Thrust = Vector3.zero;
        Owner.SetThrust(Thrust, scale);
        return (h == null || h.isDoorClosed() == false);
    }
    protected void GetLandParams(out float Speed, out float Time)
    {
        float d = (mPos[0] - mPos[1]).magnitude;
        Time = Mathf.Sqrt(2 * d / gsLandRunBrakeAceel);
        Speed = Time * gsLandRunBrakeAceel;
    }
    protected void TurnToSpeed(float scale)
    {
        Vector3 spd = Owner.GetSpeedInLocal();
        Vector3 Delta;
        Delta.y = Mathf.Asin(Vector3.Dot(mDir, Owner.pFPO.Right));
        Delta.x = Mathf.Asin(Vector3.Dot(mDir, Owner.pFPO.Up));
        Delta.z = Mathf.Clamp(Owner.RollAngle * (Mathf.Abs(Delta.y) - 1f) * .25f + Delta.y, -1f, 1f);
        Owner.ApplyControlsDelta(Delta, Vector3.zero, scale);
        Owner.MakeRotation(spd, scale);
    }
};


