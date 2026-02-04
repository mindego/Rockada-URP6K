using UnityEngine;
using DWORD = System.UInt32;
using crc32 = System.UInt32;

/// <summary>
/// Базовый класс для пехоты
/// </summary>
public class BaseInfantry : BaseObject
{
    new public const uint ID = 0xe9d76de3; //CRC от BaseInfantry
    public BaseInfantry(BaseScene s, DWORD h, OBJECT_DATA od) : base(s, h, od)
    {

    }
}
public class BaseCraft : BaseObject
{
    new public const uint ID = 0x83D5A637;
    public const float CraftIdleVolume = -900f;
    public const float VISUAL_CHECK_TIME = 0.2f;
    public const float DustFromWeightC = 600f / 10000f;
    public const float DustDistMin = 10f;
    //ILog mpLog;

    public WeaponSystemForBaseCraft GetWeaponSystem() { return mpWeaponSystem; }
    public override object GetInterface(uint id)
    {
        switch (id)
        {
            case 0xE3D62422: /* BF */       return (Dt().IsPlane ? null : this);
            case 0x7D0967C4: /* INT */      return (Dt().IsPlane ? this : null);
            case BaseCraft.ID: return this;
            case iMovementSystem.ID: return (iMovementSystem)mpMoveSystem;
            case iMovementSystemCraft.ID: return (iMovementSystemCraft)mpMoveSystem;
            case iWeaponSystemDedicated.ID: return (iWeaponSystemDedicated)mpWeaponSystem;
            case iBaseColliding.ID:
                {
                    BaseCraft c = (BaseCraft)this;
                    if (c.mpColliding == null) c.mpColliding = new BaseCollidingForBaseCraft(c);
                }
                return mpColliding;
            default:
                if (pAutopilot != null)
                {
                    object r = pAutopilot.GetInterface(id);
                    if (r != null) return r;
                }
                if (mpWeaponSystem != null)
                {
                    object r = mpWeaponSystem.GetInterface(id);
                    if (r != null) return r;
                }
                return base.GetInterface(id);
        }

    }

    // от BaseItem
    //public virtual int SetRemote(char* pData);
    //virtual bool SetLocal(int DataLength,const char* pData);
    //virtual DWORD GetCreatePktLength(DWORD Code, DWORD Offset)const;
    //virtual DWORD GetCreatePkt(ItemDataPacket*, DWORD Offset)const;
    //virtual bool OnDataPacket(float PacketDelay,const ItemDataPacket*);
    //virtual DWORD GetUpdatePktData(VECTOR& Org)const;
    //virtual void GetUpdatePkt(ItemDataPacket*);

    // от BaseActor
    public override bool Move(float scale)
    {
        if (base.Move(scale) == false) return false;
        if (scale == 0) return true;
        bool ret = pAutopilot.Move(scale, false);
        if (ret)
            updateAngles();
        return ret;
    }


    public override void Update(float scale)
    {
        if (pFPO == null || GetRoot() == null) return ;
        myBattleDelay -= scale;
        if (mpWeaponSystem.GetTargetEx() != null || GetThreatC() > 0)
            //myBattleDelay = 40f + getUniformRandom(0, 5f);
            myBattleDelay = 40f + Random.Range(0, 5f);

        base.Update(scale);
        mpWeaponSystem.Update(scale);
        // лечим части
        Regen(scale);
        //if (BatteryCharge > .0f)
        //{
        //    float RegenerateSpeed = Dt().RegenerateSpeed * scale;
        //    float RegenerateSum = GetRoot().Regenerate(RegenerateSpeed);
        //    Condition = GetRoot().Condition();
        //    for (BaseSubobj s = GetRoot().Next(); s != null; s = s.Next())
        //    {
        //        RegenerateSum += s.Regenerate(RegenerateSpeed);
        //        if (s.GetData().GetFlag(SUBOBJ_DATA.SF_CRITICAL) == 0) continue;
        //        float v = s.Condition();
        //        if (Condition > v) Condition = v;
        //    }
        //    if (Condition < 0) Condition = 0;
        //    makeLLCondition();
        //    BatteryCharge -= RegenerateSum * Dt().RegenerateC;
        //}
        //return true;
    }

    private void Regen(float scale)
    {
        if (BatteryCharge <= .0f) return;

        float RegenerateSpeed = Dt().RegenerateSpeed * scale;
        float RegenerateSum = GetRoot().Regenerate(RegenerateSpeed);
        Condition = GetRoot().Condition();
        for (BaseSubobj s = GetRoot().Next(); s != null; s = s.Next())
        {
            RegenerateSum += s.Regenerate(RegenerateSpeed);
            if (s.GetData().GetFlag(SUBOBJ_DATA.SF_CRITICAL) == 0) continue;
            float v = s.Condition();
            if (Condition > v) Condition = v;
        }
        if (Condition < 0) Condition = 0;
        makeLLCondition();
        BatteryCharge -= RegenerateSum * Dt().RegenerateC;
    }
    // от BaseUnit 
    public override int GetState()
    {
        return pAutopilot.GetState();
    }
    public override Vector3 GetSpeed()
    {
        return Speed;
    }

    private float SensorsAltMod(float dy)
    {
        float VisAlt1 = 50f;
        float VisMod1C = .0f;
        float VisMod1D = 1f / VisAlt1;
        if (dy >= VisAlt1) return 1f;
        return (dy > .0f ? VisMod1C + dy * VisMod1D : .0f);
    }
    public override float GetSensorsVisibility()
    {
        if (pFPO == null) return 0f;
        return (base.GetSensorsVisibility() * SensorsAltMod(pFPO.Org.y - rScene.GroundLevelMedian(pFPO.Org.x, pFPO.Org.z, 200f)));
    }
    // prediction
    public override void StartPrediction()
    {
        //return;
        //Debug.Log("Prediction started on " + this.GetHashCode().ToString("X8") + " Dir " + pFPO.Dir);
        //PredPos = (Storm.Matrix)pFPO; //Если делать так, то PredPos сохраняется по ссылке, что при предсказании колбасит крафт
        Debug.Log(string.Format("Saving Dir {0} Up {1} for {2} {3}", pFPO.Dir, pFPO.Up, ObjectData.FullName, this.UnitName));
        PredPos = new MATRIX(pFPO);
        PredSpeed = Speed;
        myPredDeltaY = DeltaY;
        PredCorverSpeed = CornerSpeed;
    }

    public override void MakePrediction(float scale)
    {
        // скорость в локальной системе
        Vector3 spd = pFPO.ExpressVector(Speed);
        //Debug.Log(string.Format("ExpressVector {0} GetSpeedInLocal {1}",spd,GetSpeedInLocal()));
        //public Vector3 GetSpeedInLocal() { return new Vector3(Vector3.Dot(Speed, pFPO.Right), Vector3.Dot(Speed, pFPO.Up), Vector3.Dot(Speed, pFPO.Dir)); }
        // вращаем крафт
        MakeRotation(spd, scale);
        // пересчитываем скорость
        Speed = pFPO.ProjectVector(spd);
        // дивгаемся
        MakeMove(scale, true);
        CheckTerrainRough(0, true);
    }
    public override void EndPrediction()
    {
        //Debug.Log(string.Format("Restoring Dir {0}/{1} Up {2}/{3} for {4}", pFPO.Dir, PredPos.Dir, pFPO.Up,PredPos.Up,ObjectData.FullName, this.UnitName));
        pFPO.Set(PredPos);
        Speed = PredSpeed;
        SpeedF = Speed.magnitude;
        DeltaY = myPredDeltaY;
        CornerSpeed = PredCorverSpeed;
    }
    // perfomance data
    public override bool IsSurfaced()
    {
        return false;
    }
    public override float GetMaxSpeed()
    {

        return Dt().MaxSpeed.z;
    }
    public override float GetMaxCornerSpeed()
    {
        return myMaxASpeed.x;
    }
    public override int getUnitType() { return (int)iSensorsDefines.UT_AIR; }

    // работа с подобъектами
    public override bool RecalSubobjs()
    {
        if (base.RecalSubobjs() == false) return false;

        // вес
        W = Dt().W;
        // линейные значения
        posThrustC.Set(0, 0, 0);
        negThrustC.Set(0, 0, 0);
        AeroC.Set(0, 0, 0);
        // угловые значения
        posRotateC[0].Set(0, 0, 0);
        posRotateC[1].Set(0, 0, 0);
        posRotateC[2].Set(0, 0, 0);
        negRotateC[0].Set(0, 0, 0);
        negRotateC[1].Set(0, 0, 0);
        negRotateC[2].Set(0, 0, 0);
        AeroRotateC[0].Set(0, 0, 0);
        AeroRotateC[1].Set(0, 0, 0);
        AeroRotateC[2].Set(0, 0, 0);
        // перебираем все подобъекты
        for (BaseSubobj s = SubobjsList.Head(); s != null; s = s.Next())
        //foreach (BaseSubobj s in SubobjsList)
        {
            if (s.GetLife() <= 0) continue;
            // добавляем вес
            W += s.GetWeight();
            // если это CRAFT_PART
            if (s.GetData().GetClass() == SUBOBJ_DATA.SC_CRAFT_PART)
            {
                CRAFT_PART_DATA cpd = (CRAFT_PART_DATA)s.GetData();
                // линейные значения
                posThrustC += cpd.PlusC;
                negThrustC += cpd.MinusC;
                AeroC += cpd.AeroC;
                // угловые значения
                // x axis
                posRotateC[0].y += cpd.RotateC.z * cpd.PlusC.x;
                posRotateC[0].z += cpd.RotateC.y * cpd.PlusC.x;
                negRotateC[0].y += cpd.RotateC.z * cpd.MinusC.x;
                negRotateC[0].z += cpd.RotateC.y * cpd.MinusC.x;
                AeroRotateC[0].y += cpd.RotateC.z * cpd.AeroC.x;
                AeroRotateC[0].z += cpd.RotateC.y * cpd.AeroC.x;
                // y axis
                posRotateC[1].x += cpd.RotateC.z * cpd.PlusC.y;
                posRotateC[1].z -= cpd.RotateC.x * cpd.PlusC.y;
                negRotateC[1].x += cpd.RotateC.z * cpd.MinusC.y;
                negRotateC[1].z -= cpd.RotateC.x * cpd.MinusC.y;
                AeroRotateC[1].x += cpd.RotateC.z * cpd.AeroC.y;
                AeroRotateC[1].z -= cpd.RotateC.x * cpd.AeroC.y;
                // z axis
                posRotateC[2].x -= cpd.RotateC.y * cpd.PlusC.z;
                posRotateC[2].y -= cpd.RotateC.x * cpd.PlusC.z;
                negRotateC[2].x -= cpd.RotateC.y * cpd.MinusC.z;
                negRotateC[2].y -= cpd.RotateC.x * cpd.MinusC.z;
                AeroRotateC[2].x -= cpd.RotateC.y * cpd.AeroC.z;
                AeroRotateC[2].y -= cpd.RotateC.x * cpd.AeroC.z;
            }
        }
        // окончательная коррекция значений
        Vector3 t;
        // pos thrust
        t = posThrustC * 10f;
        if (t.x != 0) { posRotateC[0].y *= myMaxASpeed.y / t.x; posRotateC[0].z *= myMaxASpeed.z / t.x; }
        else { posRotateC[0].y = 0; posRotateC[0].z = 0; }
        if (t.y != 0) { posRotateC[1].x *= myMaxASpeed.x / t.y; posRotateC[1].z *= myMaxASpeed.z / t.y; }
        else { posRotateC[1].x = 0; posRotateC[1].z = 0; }
        if (t.z != 0) { posRotateC[2].x *= myMaxASpeed.x / t.z; posRotateC[2].y *= myMaxASpeed.y / t.z; }
        else { posRotateC[2].x = 0; posRotateC[2].y = 0; }
        // neg thrust
        t = negThrustC * 10f;
        if (t.x != 0) { negRotateC[0].y *= myMaxASpeed.y / t.x; negRotateC[0].z *= myMaxASpeed.z / t.x; }
        else { negRotateC[0].y = 0; negRotateC[0].z = 0; }
        if (t.y != 0) { negRotateC[1].x *= myMaxASpeed.x / t.y; negRotateC[1].z *= myMaxASpeed.z / t.y; }
        else { negRotateC[1].x = 0; negRotateC[1].z = 0; }
        if (t.z != 0) { negRotateC[2].x *= myMaxASpeed.x / t.z; negRotateC[2].y *= myMaxASpeed.y / t.z; }
        else { negRotateC[2].x = 0; negRotateC[2].y = 0; }
        // aero
        t = AeroC * 10f;
        if (t.x != 0) { AeroRotateC[0].y /= t.x; AeroRotateC[0].z /= t.x; }
        else { AeroRotateC[0].y = 0; AeroRotateC[0].z = 0; }
        if (t.y != 0) { AeroRotateC[1].x /= t.y; AeroRotateC[1].z /= t.y; }
        else { AeroRotateC[1].x = 0; AeroRotateC[1].z = 0; }
        if (t.z != 0) { AeroRotateC[2].x /= t.z; AeroRotateC[2].y /= t.z; }
        else { AeroRotateC[2].x = 0; AeroRotateC[2].y = 0; }
        if (mpLog != null)
        {
            mpLog.Message("New FM:");
            mpLog.IncIdent();
            mpLog.Message("W: %f", W);
            mpLog.Message("posThrustC: %f %f %f", posThrustC.x, posThrustC.y, posThrustC.z);
            mpLog.Message("negThrustC: %f %f %f", negThrustC.x, negThrustC.y, negThrustC.z);
            mpLog.Message("posRotateC[0]: %f %f %f", posRotateC[0].x, posRotateC[0].y, posRotateC[0].z);
            mpLog.Message("posRotateC[1]: %f %f %f", posRotateC[1].x, posRotateC[1].y, posRotateC[1].z);
            mpLog.Message("posRotateC[2]: %f %f %f", posRotateC[2].x, posRotateC[2].y, posRotateC[2].z);
            mpLog.Message("AeroC: %f %f %f", AeroC.x, AeroC.y, AeroC.z);
            mpLog.Message("AeroRotateC[0]: %f %f %f", AeroRotateC[0].x, AeroRotateC[0].y, AeroRotateC[0].z);
            mpLog.Message("AeroRotateC[1]: %f %f %f", AeroRotateC[1].x, AeroRotateC[1].y, AeroRotateC[1].z);
            mpLog.Message("AeroRotateC[2]: %f %f %f", AeroRotateC[2].x, AeroRotateC[2].y, AeroRotateC[2].z);
            mpLog.DecIdent();
        }
        return true;
    }
    public override void OnAddSubobj(BaseSubobj s)
    {
        base.OnAddSubobj(s);
        mpWeaponSystem.OnAddSubobj(s);
    }
    public override void OnSubSubobj(BaseSubobj s, bool call_from_destructor = false)
    {
        // удаляем приделаные дюзы (если надо)
        // если это был рут - убиваем звук
        if (s == GetRoot()) { pSound.Release(); pSound = null; } //TODO Придумать безопасное освобождение объектов
        // если это был критикал (или соответсвенно группа критикалов)
        if (s.GetData().GetFlag(SUBOBJ_DATA.SF_CRITICAL) != 0 || getLLCondition() == 0) Thrust = ThrustOut;
        // передаем объекту
        base.OnSubSubobj(s, call_from_destructor);
    }
    public override void onHullDamaged()
    {
        //В ОШ дырки в стекле отсутствуют.
        //TODO Реализовать дырки при переходе к СН
        //if (mIsPlayedByHuman && myHoles == 0)
        //{
        //    myHoles = Dt().getNumHoles(RandomGenerator.Rand01());
        //    if (myHoles!=0)
        //    {
        //        FPO coc = (FPO)pFPO.GetSubObject(CockpitCode);
        //        if (coc)
        //        {
        //            int* iSlots = ANewN(int, coc.ObjectData.nSlots);
        //            int nSlots = selectHoleSlots(coc.ObjectData.pSlots, coc.ObjectData.nSlots, iSlots);
        //            int nHoles = tmin(myHoles, nSlots);

        //            for (int i = 0; i != nHoles; ++i)
        //            {
        //                int slotIdx = RandN(nSlots - i);

        //                addHole(coc, rScene.CreateFPO(Dt().HoleObject),
        //                    coc.ObjectData.pSlots[iSlots[slotIdx]]);

        //                removeE(iSlots, slotIdx, nSlots - i);
        //            }
        //        }
        //    }
        //    else myHoles = -1;
        //}
    }

    // создание/удаление
    public BaseCraft(BaseScene s, DWORD h, OBJECT_DATA od) : base(s, h, od)
    {
        //dust = null;
        mVisualsNextCheckTime = .0f;
        pSound = null;
        //mpColliding = null;
        mpMoveSystem = null;
        pAutopilot = null;
        Thrust = Vector3.zero;
        ThrustOut = Vector3.zero;
        Controls = Vector3.zero;
        DeltaY = 0;
        SpeedF = 0;
        AutopilotDeltaY = 50f;
        ControlScale = 1f;
        AutopilotPredictionTime = 6f;
        //mpWeaponSystem = null;
        myMissile = null;
        myMaxEnginePower = 1;
        myBattleDelay = -1;
        Speed.Set(0, 0, 0);
        CornerSpeed.Set(0, 0, 0);
        float rnd1 = 1f + RandomGenerator.getUniformRandom(-0.1f, 0.1f);
        float rnd2 = 1f + RandomGenerator.getUniformRandom(-0.03f, 0.03f);
        myTP = Dt().TP * rnd1;
        myTM = Dt().TM * rnd1;
        myMaxASpeed = Dt().MaxASpeed * rnd2;
        W = Dt().W;
        BatteryCharge = Dt().BatteryCharge;
        // воспомогательные системы
        mpMoveSystem = new MovementSystemCraftForBaseCraft(this);
        mpWeaponSystem = new WeaponSystemForBaseCraft(this);
        myHoles = 0;
        myTimeStamp = 0;
    }

    public override void HostPrepare(HostScene s, UnitSpawnData sd, Vector3 v, float a, iContact hangar)
    {
        // координаты появления
        Vector3 v1 = v;
# if _DEBUG
        if (v.y < 20) rScene.Message("WARNING: craft \"%s\": appears on alt of %fm!", GetObjectData().FullName, v.y);
#endif
        setDeltaY(v.y);
        v1.y += rScene.SurfaceLevel(v1.x, v1.z);
        base.HostPrepare(s, sd, v1, a, hangar);
        BasePrepare(sd);
        // создаем автопилот взлета
        BaseHangar h = null;
        if (hangar != null)
        {
            h = (BaseHangar)hangar.GetInterface(BaseHangar.ID);
            if (h == null || h.canHandleUnit((iContact)GetInterface(iContact.ID)) == 0)
                //throw new System.Exception(string.Format("Craft \"{0}\": incompatible hangar!", GetObjectData().FullName));
                throw new System.Exception(string.Format("Craft \"{0}\": incompatible hangar!", GetObjectData().FullName));
            h.setStatus(HangarStatus.hsTakeoffing);
        }
        pAutopilot = new BaseCraftAutopilotTakeoff(this, hangar, h);
        Debug.Log(GetName() + " бортовой номер  " + GetHashCode().ToString("X8"));
    }
    //virtual void RemotePrepare(RemoteScene*, ObjectCreatePacket*, DWORD); // инициализация на клиенте
    ~BaseCraft()
    {
        Dispose();
    }

    private bool IsDisposed = false;
    public override void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
        Debug.Log("Disposing of Craft " + GetObjectData().FullName);
        //TODO корректно освобождать ресурсы крафта
        //SafeRelease(pSound);
        Debug.Log("Releasing snd " + pSound);
        IMemory.SafeRelease(pSound);
        //if (dust) dust.Die(1);
        if (mpWeaponSystem != null) mpWeaponSystem.Dispose();
        if (mpMoveSystem != null) mpMoveSystem.Dispose(); ;
        if (pAutopilot != null) pAutopilot.Dispose();
        if (mpColliding!=null) mpColliding.Dispose();
        base.Dispose();
    }
    public override void SetLog(ILog pLog)
    {
        base.SetLog(pLog);
        if (mpLog != null)
        {
            mpLog.Message("BaseCraft.CornerSpeed=({0},{1},{2})", CornerSpeed.x, CornerSpeed.y, CornerSpeed.z);
        }
    }

    // own
    private WeaponSystemForBaseCraft mpWeaponSystem;
    private BaseCollidingForBaseCraft mpColliding;
    public virtual void BasePrepare(UnitSpawnData sd)
    {
        pVis = rScene.GetSceneVisualizer();

        // оружие && партикли
        BaseCraftEnumSubobj Helper = new BaseCraftEnumSubobj(rScene, this);
        if (sd != null)
            Helper.Init(sd.Layout1Name, sd.Layout2Name, sd.Layout3Name);
        pFPO.EnumerateSlots(Helper);
        mpWeaponSystem.correctWeapons();

        // эффекты
        if (pVis != null)
        {
            //TODO вернуть пыль
            //if (Dt().Dust != null && pVis.GetSceneConfig().v_dust!=0) dust = pVis.CreateDust(Dt().Dust);
            //if (pVis.Get3DSound() != null && pVis.GetSceneConfig().s_vcraftengine > 0) //Вернуть управление громкостью
            if (pVis.Get3DSound() != null)
            {
                //TODO вернуть громкость звука двигателя крафту
                //int idle_volume = (int)(2 * (float)(pVis.GetSceneConfig().s_vcraftengine) / 3.0);
                int idle_volume = 9;
                CraftSoundController ctr = new CraftSoundController(this, pVis.GetSoundApi().convertVolume(idle_volume), pVis.GetSoundApi().convertVolume(pVis.GetSceneConfig().s_vcraftengine));
                pSound = pVis.Get3DSound().LoadEvent("Craft", Dt().FullName, "Fly", true, false, ctr);
                if (pSound != null)
                {
                    pSound.Start();
                }
                else
                {
                    Debug.Log("Sound load failure");
                }

                ctr.Release();
            }
            //Тень будет рисовать движок
            //CreateShadow(true); 
        }
    }
    public override void SubWeight(float w)
    {
        W -= w;
    }

    // iMovementSystemCraft
    public virtual void setMaxEnginePower(float power)
    {

        myMaxEnginePower = Mathf.Clamp(power, 0, 1);
    }

    // физика полета
    // входные значения
    private int myTimeStamp;
    public BaseCraftAutopilot pAutopilot;
    public MovementSystemCraftForBaseCraft mpMoveSystem;
    public Vector3 Thrust;
    public Vector3 Controls;
    public float DeltaY;
    public float SpeedF;
    public float AutopilotPredictionTime;
    public float AutopilotDeltaY;
    public float BatteryCharge;
    // параметры полета
    public Vector3 Speed;            // линейная скорость по трем осям
    public Vector3 CornerSpeed;      // угловая скорость по трем осям
    public float W;                // вес
    public float ControlScale;
    void setDeltaY(float delta)
    {
        //rScene.Message("%p DeltaY %f",mpMoveSystem,delta);
        DeltaY = delta;
    }
    // для предсказания
    MATRIX PredPos;
    Vector3 PredSpeed;
    Vector3 PredCorverSpeed;
    float myPredDeltaY;
    // линейные значения
    Vector3 posThrustC;
    Vector3 negThrustC;
    Vector3 AeroC;
    // угловые значения
    Vector3[] posRotateC = new Vector3[3];
    Vector3[] negRotateC = new Vector3[3];
    Vector3[] AeroRotateC = new Vector3[3];
    // ф-ции физики
    Vector3 myTP;
    Vector3 myTM;
    Vector3 myMaxASpeed;
    public Vector3 GetSpeedInLocal() { return new Vector3(Vector3.Dot(Speed, pFPO.Right), Vector3.Dot(Speed, pFPO.Up), Vector3.Dot(Speed, pFPO.Dir)); }
    // BaseCraft - физика полета
    float GetTerrainAngle(float time)
    {
        Vector3 HSpeedV = new Vector3(Speed.x, 0, Speed.z);
        float HSpeedF = HSpeedV.magnitude;
        float res = -1E9f;
        if (HSpeedF > .1f)
        {
            // смотрим вперед
            float t, t1;
            float ox = pFPO.Org.x;
            float oz = pFPO.Org.z;
            int i, max = (int)(HSpeedF * time * TerrainDefs.OO_SQUARE_SIZE);
            HSpeedV *= (TerrainDefs.SQUARE_SIZE / HSpeedF);
            for (i = 0, t = 10; i < max; i++, t += TerrainDefs.SQUARE_SIZE, ox += HSpeedV.x, oz += HSpeedV.z)
            {
                t1 = (AutopilotDeltaY + rScene.SurfaceLevelObjects(ox, oz) - pFPO.Org.y) / t;
                if (res < t1) res = t1;
            }
        }
        //return atan(res);
        return Mathf.Atan(res);
    }
    public void CorrectDirAndSpeed(ref Vector3 Dir, ref Vector3 Spd, float Time)
    {
        // получаем угол террейна
        float a = GetTerrainAngle(Time);
        float SpdNorma = Speed.magnitude;
        if (SpdNorma >= 0)
        {
            float b = Mathf.Asin(Speed.y / SpdNorma);
            if (a > b + Storm.Math.GRD2RD(10)) a = 1.475f;
        }
        if (Dt().IsPlane == false)
        {
            // проверяем скорость
            SpdNorma = Spd.magnitude;
            if (SpdNorma == 0) return;
            if (Mathf.Asin(Spd.y / SpdNorma) > a) return;
            // корректируем скорость
            SetVector(ref Spd, Mathf.Atan2(Spd.x, Spd.z), a);
            Spd *= SpdNorma;
        }
        // проверяем направление
        if (Mathf.Asin(Dir.y) < a) SetVector(ref Dir, Mathf.Atan2(Dir.x, Dir.z), a);
    }

    // SetVector - воспомогательная ф-ция
    public static void SetVector(ref Vector3 Res, float Hdg, float Pitch)
    {
        Storm.Math.SinCos(Hdg, out Res.x, out Res.z);
        Res.y = Mathf.Sin(Pitch);
        float c = Mathf.Cos(Pitch);
        Res.x *= c;
        Res.z *= c;
    }
    public Vector3 GetTreqForSpeed(Vector3 Faero, Vector3 spd)
    {
        // требуемое ускорение в мировой системе координат
        Vector3 Treq = spd - Speed;
        Treq.y += STORM_DATA.GAcceleration;
        // переводим в локальную сисетму координат
        Vector3 tmp = new Vector3(Vector3.Dot(Treq, pFPO.Right), Vector3.Dot(Treq, pFPO.Up), Vector3.Dot(Treq, pFPO.Dir));
        // получает требуемую тягу (с учетом Aero)
        Treq.x = tmp.x * W - Faero.x;
        Treq.y = tmp.y * W - Faero.y;
        Treq.z = tmp.z * W - Faero.z;
        // возвращаем результат
        return Treq;
    }
    public Vector3 GetTreqForThrust(Vector3 thr, Vector3 spd)
    {
        float ThrustC = Dt().ThrustCFromAlt(DeltaY);
        return new Vector3(
            (thr.x >= .0f ? SelectThrust(thr.x, myTP.x, ThrustC, Dt().TSC, spd.x) : -SelectThrust(-thr.x, myTM.x, ThrustC, Dt().TSC, -spd.x)),
            (thr.y >= .0f ? SelectThrust(thr.y, myTP.y, ThrustC, Dt().TSC, spd.y) : -SelectThrust(-thr.y, myTM.y, ThrustC, Dt().TSC, -spd.y)),
            (thr.z >= .0f ? SelectThrust(thr.z, myTP.z, ThrustC, Dt().TSC, spd.z) : -SelectThrust(-thr.z, myTM.z, ThrustC, Dt().TSC, -spd.z))
            );
    }

    private static float SelectThrust(float t, float tmax, float tac, float tsc, float spd)
    {
        if (spd > .0f) { tmax *= (1f - tsc * spd); if (tmax <= .0f) return .0f; }
        tmax *= tac; if (tmax <= .0f) return .0f;
        return (t * tmax);
    }

    public static float ClampThrust(float t, ref float tmax, float tac, float tsc, float spd, float engine_coeff)
    {
        if (spd > .0f) { tmax *= (1f - tsc * spd); if (tmax <= .0f) return .0f; }
        tmax *= tac; if (tmax <= .0f) return .0f;
        //Debug.Log(string.Format("ClampThrust t {0} tmax {1} tac {2} tsc {3} spd {4} engine_coeff {5}", t, tmax, tac, tsc, spd, engine_coeff));
        return (t < tmax * engine_coeff ? t : tmax * engine_coeff);
    }

    public Vector3 GetFaero(Vector3 spd, float scale)
    {
        Vector3 t;
        scale = W / scale;
        if (Dt().IsPlane == true)
        {
            t.x = Dt().TailLiftC * Mathf.Atan2(-spd.x, spd.z) * Mathf.Pow(SpeedF, 2);
            t.y = Dt().WingLiftC * Mathf.Atan2(-spd.y, spd.z) * Mathf.Pow(SpeedF, 2);
            t.z = -Mathf.Abs(spd.z) * spd.z * Dt().DragC.z;
        }
        else
        {
            t.x = -Mathf.Abs(spd.x) * spd.x * Dt().DragC.x;
            t.y = -Mathf.Abs(spd.y) * spd.y * Dt().DragC.y;
            t.z = -Mathf.Abs(spd.z) * spd.z * Dt().DragC.z;
        }
        t.x = ClampFaero((t.x > 0 ? t.x + CRAFT_DATA.BaseDragC : t.x - CRAFT_DATA.BaseDragC) * AeroC.x, spd.x * scale);
        t.y = ClampFaero((t.y > 0 ? t.y + CRAFT_DATA.BaseDragC : t.y - CRAFT_DATA.BaseDragC) * AeroC.y, spd.y * scale);
        t.z = ClampFaero((t.z > 0 ? t.z + CRAFT_DATA.BaseDragC : t.z - CRAFT_DATA.BaseDragC) * AeroC.z, spd.z * scale);
        return t;
    }

    private static float ClampFaero(float f, float spd)
    {
        if (spd > .0f) return (f > -spd ? f : -spd);
        else return (f < -spd ? f : -spd);
    }
    public Vector3 ApplyForces(Vector3 Faero, Vector3 Treq, Vector3 spd, float scale)
    {
        float ThrustC = Dt().ThrustCFromAlt(DeltaY); // зависимость тяги от высоты
        Vector3 Feng = Vector3.zero;
        Vector3 Fmax;
        // накладываем на тягу ограничения по высоте и скорости
        Fmax.x = Treq.x >= .0f ? myTP.x : myTM.x;
        Fmax.y = Treq.y >= .0f ? myTP.y : myTM.y;
        Fmax.z = Treq.z >= .0f ? myTP.z : myTM.z;

        Feng.x = (Treq.x >= .0f ? ClampThrust(Treq.x, ref Fmax.x, ThrustC, Dt().TSC, spd.x, myMaxEnginePower) : -ClampThrust(-Treq.x, ref Fmax.x, ThrustC, Dt().TSC, -spd.x, myMaxEnginePower));
        Feng.y = (Treq.y >= .0f ? ClampThrust(Treq.y, ref Fmax.y, ThrustC, Dt().TSC, spd.y, myMaxEnginePower) : -ClampThrust(-Treq.y, ref Fmax.y, ThrustC, Dt().TSC, -spd.y, myMaxEnginePower));
        Feng.z = (Treq.z >= .0f ? ClampThrust(Treq.z, ref Fmax.z, ThrustC, Dt().TSC, spd.z, myMaxEnginePower) : -ClampThrust(-Treq.z, ref Fmax.z, ThrustC, Dt().TSC, -spd.z, myMaxEnginePower));

        //Feng.x = (Treq.x >= .0f ? ClampThrust(Treq.x, Fmax.x = myTP.x, ThrustC, Dt().TSC, spd.x, myMaxEnginePower) : -ClampThrust(-Treq.x, Fmax.x = myTM.x, ThrustC, Dt().TSC, -spd.x, myMaxEnginePower));
        //Feng.y = (Treq.y >= .0f ? ClampThrust(Treq.y, Fmax.y = myTP.y, ThrustC, Dt().TSC, spd.y, myMaxEnginePower) : -ClampThrust(-Treq.y, Fmax.y = myTM.y, ThrustC, Dt().TSC, -spd.y, myMaxEnginePower));
        //Feng.z = (Treq.z >= .0f ? ClampThrust(Treq.z, Fmax.z = myTP.z, ThrustC, Dt().TSC, spd.z, myMaxEnginePower) : -ClampThrust(-Treq.z, Fmax.z = myTM.z, ThrustC, Dt().TSC, -spd.z, myMaxEnginePower));
        //Debug.Log("Feng " + Feng + " Fmax " + Fmax + " Dt().TP " + Dt().TP + " Dt().TM " + Dt().TM);

        // устанавливаем тягу
        Vector3 t = Vector3.zero;
        if (Fmax.x > 0) t.x = Feng.x / Fmax.x;
        if (Fmax.y > 0) t.y = Feng.y / Fmax.y;
        if (Fmax.z > 0) t.z = Feng.z / Fmax.z;
        SetThrust(t, scale);
        // влияние повреждений на FM
        Feng.x *= (Feng.x > 0 ? posThrustC.x : negThrustC.x);
        Feng.y *= (Feng.y > 0 ? posThrustC.y : negThrustC.y);
        Feng.z *= (Feng.z > 0 ? posThrustC.z : negThrustC.z);
        // получаем ускорение
        t = (Faero + Feng) * (scale / W);
        Speed += (pFPO.Right * t.x + pFPO.Up * t.y + pFPO.Dir * t.z);
        Speed.y -= STORM_DATA.GAcceleration * scale;
        SpeedF = Speed.magnitude;
        // влияние повреждений на FM - угловые значения
        Vector3 R;
        R.x = Faero.y * AeroRotateC[1].x + Faero.z * AeroRotateC[2].x;
        R.y = Faero.x * AeroRotateC[0].y + Faero.z * AeroRotateC[2].y;
        R.z = Faero.x * AeroRotateC[0].z + Faero.y * AeroRotateC[1].z;
        if (Feng.x > 0)
        {
            R.y += Feng.x * posRotateC[0].y;
            R.z += Feng.x * posRotateC[0].z;
        }
        else
        {
            R.y += Feng.x * negRotateC[0].y;
            R.z += Feng.x * negRotateC[0].z;
        }
        if (Feng.y > 0)
        {
            R.x += Feng.y * posRotateC[1].x;
            R.z += Feng.y * posRotateC[1].z;
        }
        else
        {
            R.x += Feng.y * negRotateC[1].x;
            R.z += Feng.y * negRotateC[1].z;
        }
        if (Feng.z > 0)
        {
            R.x += Feng.z * posRotateC[2].x;
            R.y += Feng.z * posRotateC[2].y;
        }
        else
        {
            R.x += Feng.z * negRotateC[2].x;
            R.y += Feng.z * negRotateC[2].y;
        }
        // для самолетной модели
        if (Dt().IsPlane == true)
        {
            // ограничения углов 
            if (spd.z < Dt().StallSpeed)
            {
                float d = (Dt().StallSpeed - spd.z) * 100;
                Vector3 tmp = pFPO.ToLocalVector(Vector3.up);
                R.x -= tmp.y * d;
                R.y -= tmp.x * d;
            }
        }
        return R;
    }
    public void MakeMove(float scale, bool pred)
    {
        // перемещение
        pFPO.Org += Speed * scale;
        //float slevel = ;
        /*
        iContact* cnt = rScene.getSV().GetCameraData().GetContact();
        if (cnt && cnt.GetInterface<BaseCraft>()==this)
            rScene.Message("Surface level %f",slevel);*/
        setDeltaY(pFPO.Org.y - rScene.SurfaceLevel(pFPO.Org.x, pFPO.Org.z));
        if (pred == true) return;
        rScene.UpdateHM(pHash);
        //Тень, знай своё место! В движке Unity!
        //UpdateShadow();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="t">Вектор "ручек управления"</param>
    /// <param name="ExtRotate"></param>
    /// <param name="scale"></param>
    public void ApplyControls(Vector3 t, Vector3 ExtRotate, float scale)
    {
        float d;
        // переводим угловую скорость в локальную систему (T)
        //VECTOR T(-CornerSpeed*pFPO.Right,CornerSpeed*pFPO.Up,-CornerSpeed*pFPO.Dir);
        //Vector3 T = new Vector3(-Vector3.Dot(CornerSpeed, pFPO.Right), Vector3.Dot(CornerSpeed, pFPO.Up), -Vector3.Dot(CornerSpeed, pFPO.Dir));
        Vector3 T = new Vector3(Vector3.Dot(-CornerSpeed, pFPO.Right), Vector3.Dot(CornerSpeed, pFPO.Up), Vector3.Dot(-CornerSpeed, pFPO.Dir));
        // определяем угловое ускорение в локальной системе и добавляем к скорости
        d = Dt().CornerCFromSpeed(SpeedF); if (d < .1f) d = .1f;
        Vector3 MaxASpeed = myMaxASpeed * d;
        Vector3 MaxAAccel = Dt().MaxAAccel * d;
        d = 1f / W;
        // меняем угловые скорости
        ProcessCornerValue(ref T.x, Mathf.Clamp(t.x, -1f, 1f), MaxASpeed.x, MaxAAccel.x * scale, ExtRotate.x * d);
        ProcessCornerValue(ref T.y, Mathf.Clamp(t.y, -1f, 1f), MaxASpeed.y, MaxAAccel.y * scale, ExtRotate.y * d);
        ProcessCornerValue(ref T.z, Mathf.Clamp(t.z, -1f, 1f), MaxASpeed.z, MaxAAccel.z * scale, ExtRotate.z * d);
        // переводим скорость в глобальную систему
        CornerSpeed = pFPO.Up * T.y - pFPO.Right * T.x - pFPO.Dir * T.z;
    }

    private static void ProcessCornerValue(ref float v, float c, float maxv, float maxa, float ext)
    {
        if (c * v < .0f) maxa *= 2f;
        v += Mathf.Clamp(c * maxv + ext - v, -maxa, maxa);
        maxv *= 5f; v = Mathf.Clamp(v, -maxv, maxv);
    }
    private static void ProcessCornerValue(ref float v, float d, float maxv, float maxa, float ext, float scale)
    {
        float tv = (d >= 0 ? GetNextBrakeV(d, maxa, scale) : -GetNextBrakeV(-d, maxa, scale));
        tv = Mathf.Clamp(tv - ext, -maxv, maxv);
        v += Mathf.Clamp(tv - v + ext, -maxa * scale, maxa * scale);
        maxv *= 5f; v = Mathf.Clamp(v, -maxv, maxv);
    }

    private static float GetNextBrakeV(float d, float maxa, float scale)
    {
        return maxa * (Mathf.Sqrt(scale * scale + 2f * d / maxa) - scale);
    }
    public void ApplyControlsDelta(Vector3 delta, Vector3 ExtRotate, float scale)
    {
        float d;
        // переводим угловую скорость в локальную систему (T)
        //VECTOR T(-CornerSpeed*pFPO.Right,CornerSpeed*pFPO.Up,-CornerSpeed*pFPO.Dir);
        //Vector3 T = new Vector3(-Vector3.Dot(CornerSpeed, pFPO.Right), Vector3.Dot(CornerSpeed, pFPO.Up), -Vector3.Dot(CornerSpeed, pFPO.Dir));
        Vector3 T = new Vector3(Vector3.Dot(-CornerSpeed, pFPO.Right), Vector3.Dot(CornerSpeed, pFPO.Up), Vector3.Dot(-CornerSpeed, pFPO.Dir));

        // определяем угловое ускорение в локальной системе и добавляем к скорости
        d = Dt().CornerCFromSpeed(SpeedF); if (d < .1f) d = .1f;
        Vector3 MaxASpeed = myMaxASpeed * d;
        Vector3 MaxAAccel = Dt().MaxAAccel * d;
        // меняем угловые скорости
        d = 1 / W;
        ProcessCornerValue(ref T.x, delta.x, MaxASpeed.x * ControlScale, MaxAAccel.x * ControlScale, ExtRotate.x * d, scale);
        ProcessCornerValue(ref T.y, delta.y, MaxASpeed.y * ControlScale, MaxAAccel.y * ControlScale, ExtRotate.y * d, scale);
        ProcessCornerValue(ref T.z, delta.z, MaxASpeed.z * ControlScale, MaxAAccel.z * ControlScale, ExtRotate.z * d, scale);
        // переводим скорость в глобальную систему
        CornerSpeed = pFPO.Up * T.y - pFPO.Right * T.x - pFPO.Dir * T.z;
    }
    /// <summary>
    /// Поворот крафта
    /// </summary>
    /// <param name="spd">SpeedInLocal</param>
    /// <param name="scale"></param>
    public void MakeRotation(Vector3 spd, float scale)
    {
        float d = CornerSpeed.magnitude;
        // вращаем крафт
        if (d > Storm.Math.GRD2RD(1f) * 0.001f)
            pFPO.Rotate(CornerSpeed / d, d * scale);
        // плоское вращение
        d = Dt().CornerCFromSpeed(SpeedF); if (d < .1f) d = .1f;
        d *= Dt().YawC * spd.z * Mathf.Sin(RollAngle) * scale;
        if (Mathf.Abs(d) > .001)
        {

            Vector2 cosine = Storm.Math.getSinCos(d);
            Vector2 res1 = Storm.Math.Rotate2d(new Vector2(pFPO.Dir.x, pFPO.Dir.z), cosine);
            Vector2 res2 = Storm.Math.Rotate2d(new Vector2(pFPO.Right.x, pFPO.Right.z), cosine);
            pFPO.Dir.x = res1.x;
            pFPO.Dir.z = res1.y;
            pFPO.Right.x = res2.x;
            pFPO.Right.z = res2.y;
            pFPO.Up = Vector3.Cross(pFPO.Dir, pFPO.Right);
        }
    }
    public bool CheckTerrainZero() { return false; }

    const float MinRoughtAlt = 1f;
    public void CheckTerrainRough(float t, bool isPrediction)
    {
        //AssertBp(pFPO.Org.NormaFAbs()<1E9);

        if (DeltaY > MinRoughtAlt + pFPO.HashRadius) return;
        if (rScene.GetTime() > t || IsPlayedByHuman() == false)
        {
            bool damaged = false;

            if (!isPrediction && !IsRemote() && !IsPlayedByHuman())
            {

                TraceResult tr = rScene.SurfaceLevelTr(pFPO.Org.x, pFPO.Org.z);
                Vector3 groundNormal = tr.Normal(true);
                float normSpeed = -(Vector3.Dot(Speed, groundNormal));

                damaged = normSpeed > Dt().getDeepSpeed(GetThreatF(), GetCondition());
                /*
                if (damaged) {
                    rScene.Message("Collision %p, normSpeed=%f, allowedSpeed=%f, thread=%f, cond=%f",
                        this, normSpeed, Dt().getDeepSpeed(GetThreatF(), GetCondition()), GetThreatF(), GetCondition());
                } */
            }

            if (!damaged)
            {
                pFPO.Org.y += MinRoughtAlt + pFPO.HashRadius - DeltaY;
                setDeltaY(MinRoughtAlt + pFPO.HashRadius);
            }
            else
                GetRoot().AddDamage(GetHandle(), iBaseVictim.WeaponCodeCollisionGround, 1E9f);
        }
    }
    public void CheckTerrainPrecise()
    {
        if (IsRemote()) return;

        float dy = DeltaY;

        if (mCriticalDamagedTime > 0)
        {
            float delta_x = MaxX - MinX;
            float delta_y = MaxY - MinY;
            float delta_z = MaxZ - MinZ;
            if (Storm.Math.isInRange(delta_x / delta_y, 0.5f, 2f) && Storm.Math.isInRange(delta_x / delta_z, 0.5f, 2f) && Storm.Math.isInRange(delta_y / delta_z, 0.5f, 2f))
                dy -= pFPO.HashRadius / 2;
        }

        if (dy < 10 && mCriticalDamagedTime > 0 && mCriticalDamagedTime > rScene.GetTime() + 3)
            mCriticalDamagedTime = rScene.GetTime() + 3;
        if (IsPlayedByHuman()) return;
        if (dy < 0) GetRoot().AddDamage(GetHandle(), iBaseVictim.WeaponCodeCollisionGround, 1E9f);
    }
    public float GetSpeedF() { return SpeedF; }
    public float GetBattaryCharge() { return BatteryCharge; }
    public void setActiveMissile(ProjectileMissile missile) { myMissile = missile; }
    public ProjectileMissile getActiveMissile() { return myMissile; }

    public Vector3 getTP() { return myTP; }
    public Vector3 getTM() { return myTM; }
    public Vector3 getMaxASpeed() { return myMaxASpeed; }
    // эффекты - дюзы, пыль, звук
    public override float getFloat(crc32 name)
    {
        switch (name)
        {
            case AnimationParams.iFrontDuza:
                return Vector3.Dot(ThrustOut, Vector3.back);//VECTOR(0, 0, -1);
            case AnimationParams.iBackDuza:
                return Vector3.Dot(ThrustOut, Vector3.forward);//* VECTOR(0, 0, 1);
            case AnimationParams.iRightDuza:
                return Vector3.Dot(ThrustOut, Vector3.left);// * VECTOR(-1, 0, 0);
            case AnimationParams.iLeftDuza:
                return Vector3.Dot(ThrustOut, Vector3.right);  // VECTOR(1, 0, 0);
            case AnimationParams.iTopDuza:
                return Vector3.Dot(ThrustOut, Vector3.down); //* VECTOR(0, -1, 0);
            case AnimationParams.iBottomDuza:
                return Vector3.Dot(ThrustOut, Vector3.up);// * VECTOR(0, 1, 0);
            case AnimationParams.iHasTarget:
                return (mpWeaponSystem.GetTargetEx() != null) ? 1 : 0;
            case AnimationParams.iDelay:
                return myBattleDelay;
            //case iBlinkCounter: //TODO и мигалки 
            //    return getPeriodic(rScene.GetTime() + myAnimationStartTime, 2);
            default: return base.getFloat(name);
        }
    }
    public void setTakeoffTime(float t) { myTakeoffTime = t; }
    public float getTakeoffTime() { return myTakeoffTime; }
    private int myHoles;//YEAH, BABY!!!

    private ProjectileMissile myMissile;
    private SceneVisualizer pVis;
    //private Dust dust; //TODO вернуть пыль!
    float mVisualsNextCheckTime;
    I3DSoundEvent pSound;
    public Vector3 ThrustOut;
    float myMaxEnginePower;
    float myTakeoffTime;
    float myBattleDelay;
    public void SetThrust(Vector3 Tout, float scale)
    {
        ThrustOut += (Tout - ThrustOut) * scale;
        // перезарядка двигателя
        BatteryCharge += (1 - (Mathf.Abs(Tout.x) * Dt().TC.x + Mathf.Abs(Tout.y) * Dt().TC.y + Mathf.Abs(Tout.z) * Dt().TC.z)) * Dt().BatteryRechargeC * scale;
        if (BatteryCharge > Dt().BatteryCharge) BatteryCharge = Dt().BatteryCharge;
        if (BatteryCharge < 0) BatteryCharge = 0;
        // визуальные эффекты тяги
        // периодичность
        //TODO Вернуть на место пыль крафта
        //if (mVisualsNextCheckTime > rScene.GetTime())
        //{
        //    if (dust!=null) dust.Interpolate(scale, Speed);
        //    return;
        //}
        // проставляем время следующей проверки
        //mVisualsNextCheckTime = rScene.GetTime() + VISUAL_CHECK_TIME;
        // обновляем пыль
        //if (dust != null)
        //{
        //    // если мы видны
        //    if (DetailStage < DETAIL_STAGE_NONE)
        //    {
        //        TraceResult r = new TraceResult();
        //        Vector3 Temp = pFPO.ToWorldVector(-ThrustOut);
        //        float d = Temp.magnitude;
        //        if (d > .0f)
        //        {
        //            Temp /= d;
        //            d *= W * DustFromWeightC;
        //            TraceInfo info = rScene.TraceLine(new Storm.Line(pFPO.Org, Temp, d), pHash, (int) CollisionDefines.COLLF_ALL);
        //            if (info.count != 0 && info.results[0].coll_object== null) r = info.results[0];
        //        }
        //        if (r != null) dust.Setup(r.coll_object, r.ground_type, r.org, r.Normal(true), Speed, DustInt(r.dist, d));
        //        else dust.Die(0);
        //    }
        //    else
        //    {
        //        dust.Destroy();
        //    }
        //}
    }

    // пыль
    static float DustInt(float d, float dmax)
    {
        d -= DustDistMin;
        if (d <= .0f) return 1f;
        dmax -= DustDistMin;
        if (d >= dmax) return .0f;
        return (1f - d / dmax);
    }
    public CRAFT_DATA Dt<T>() where T : CRAFT_DATA { return (CRAFT_DATA)ObjectData; }
    public CRAFT_DATA Dt()
    {
        return (CRAFT_DATA)ObjectData;
    }
}

public struct CraftUpdatePacketStd
{
    public Vector3 Org;
    public short SpeedX, SpeedY, SpeedZ;
    public DWORD Angles;
    public byte Flags;
    public int Stamp;
};

public static class BaseCraftPackets
{
    public const uint CDP_SET_TARGET = BaseObjectPackets.ODP_LAST + 1;
    public const uint CDP_UPDATE_STD = CDP_SET_TARGET + 1;
    public const uint CUPS_DUSES_MASK = 0x3F;
    public const uint CUPS_STATE_MASK = 0xC0;

    public const float PACK_SPEED_MOD = (32767f / 1000f);
    public const float UNPACK_SPEED_MOD = (1000f / 32767f);
    public static short PackSpeed(float v)
    {
        return (short)Mathf.Clamp(v * PACK_SPEED_MOD, -32767, 32767);
    }
    public static float UnpackSpeed(short v)
    {
        return v * UNPACK_SPEED_MOD;
    }

    public static float PACK_ANGLES_MOD = (1024f / (3.1416f * 2));
    public static float UNPACK_ANGLES_MOD = ((3.1416f * 2) / 1024f);

    public static DWORD PackAngles(float h, float p, float r)
    {
        DWORD r0 = 0;
        DWORD r1;
        // heading
        if (h < 0) h += Storm.Math.PI_2;
        r1 = (uint)(h * PACK_ANGLES_MOD);
        r0 |= r1;
        // pitch
        if (p < 0) p += Storm.Math.PI_2;
        r1 = (uint)(p * PACK_ANGLES_MOD);
        r0 |= r1 << 10;
        // roll
        if (r < 0) r += Storm.Math.PI_2;
        r1 = (uint)(r * PACK_ANGLES_MOD);
        r0 |= r1 << 20;
        return r0;
    }

    public static void UnpackAngles(out float h, out float p, out float r, DWORD r0)
    {
        // heading
        h = (r0 & 0x3FF) * UNPACK_ANGLES_MOD;
        if (h > Storm.Math.PI) h -= Storm.Math.PI_2;
        // pitch
        p = ((r0 >> 10) & 0x3FF) * UNPACK_ANGLES_MOD;
        if (p > Storm.Math.PI) p -= Storm.Math.PI_2;
        // roll
        r = ((r0 >> 20) & 0x3FF) * UNPACK_ANGLES_MOD;
        if (r > Storm.Math.PI) r -= Storm.Math.PI_2;
    }
}


public interface iWeaponSystem
{
    public const float MY_TARGET_C = 2.0f;
    public const float MY_THREAT_C = 1.5f;
    public static float GetBusyC(float b)
    {
        if (b == .0f) return 1f;
        if (b < 1f) return 2f;
        return .5f;
    }
}