using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using crc32 = System.UInt32;
using DWORD = System.UInt32;

public class Dust
{
    //TODO Реализовать подержку пыли
    //  const DUST_DATA* dust_data;
    //  TContact owner;

    //  MATRIX RelPos;
    //  const RO* owner_ro,*attach_to; 
    //VECTOR dust_speed;

    //  int current_gt;
    //  bool delete_flag_when_empty;


    //  struct DustController* mpLivingDust;
    //TLIST<DustController> mlDustEmitters;
    //  bool mAllDying;

    //  DustController* SpawnDust(DWORD dust_name);
    //  void SetupVariables();
    //  void DieDust();
    //  void UpdateDust(float);
    //  void DeleteDust();
    //  public:

    //bool IsAllDying() { return mAllDying; }
    //  void Destroy();
    //  void Die(int d_flag);
    //  void Setup(bool on_object, int ground_type, const VECTOR& org, const VECTOR& norm, const VECTOR& _speed, float BirthAct);
    //  void Interpolate(float scale,const VECTOR& _speed);


    // взаимодействие с окружающей средой
    //virtual bool Update(float scale);

    //  Dust(SceneVisualizer* _scene,const DUST_DATA* d, RO *_attach_to,const SLOT_DATA* sl_d);
    //  virtual ~Dust();
}

public struct DustCont
{
    Dust dust;
    public DustCont(Dust _dust)
    {
        dust = _dust;
    }
};
public class BaseVehicle : BaseObject, iMovementSystemVehicle
{
    // от iBaseInterface
    new public const uint ID = 0x3C0DD2D8;
    public override object GetInterface(DWORD id)
    {
        switch (id)
        {
            case iBaseColliding.ID:
                {
                    BaseVehicle s = (BaseVehicle)this;
                    if (s.m_pColliding == null)
                        s.m_pColliding = new BaseCollidingForBaseVehicle(s);
                }
                return m_pColliding;
            case iWeaponSystemHTGR.ID: return (MainTurret != null ? MainTurret.GetWeaponSystem().GetInterface(iWeaponSystemHTGR.ID) : null);
            case WeaponSystemForTurretWithHTGR.ID: return (MainTurret != null ? MainTurret.GetWeaponSystem().GetInterface(WeaponSystemForTurretWithHTGR.ID) : null);
            case iWeaponSystemDedicated.ID: return (MainTurret != null ? MainTurret.GetWeaponSystem() : null);
            case BaseVehicle.ID: return (pFPO != null ? this : null);
            case iMovementSystemVehicle.ID: return (pFPO != null ? (iMovementSystemVehicle)this : null);
            default: return base.GetInterface(id);
        }
    }
    //template <class C> C * GetInterface() const { return (C*)GetInterface(C::ID); }

    public override float getFloat(crc32 name)
    {
        switch (name)
        {
            case AnimationParams.iSpeedName:
                {
                    float sn = Speed.magnitude;
                    if (mySpeedValues.Count != 0)
                    {
                        if (mySpeedValues[mySpeedValues.Count - 1].mySpeed < sn)
                            return sn;
                        return mySpeedValues[0].mySpeed;
                    }
                    return sn;
                }
            default:
                return base.getFloat(name);
        }
    }

    // от BaseActor
    public override bool Move(float scale)
    {
        if (base.Move(scale) == false) return false;
        if (have_visuals)
            ProcessVisuals(scale);
        if (mFirstTick && !isHangaring())
        {
            SetupOnGround(pFPO.Org);
            mFirstTick = false;
        }
        updateSpeedValue(new SpeedValue(rScene.GetTime(), Speed.magnitude));
        bool ret = mpAutopilot.Move(scale);
        //if (pFPO!=null)
        //{
        //    rScene.UpdateHM(pHash);
        //}
        //return ret; //TODO Восстановить отрисовку ног? при движение наземного юнита
        if (pFPO != null)
        {
            // просчитываем ноги
            if (DetailStage < BaseScene.DETAIL_STAGE_NONE)
            {
                // если первый раз
                if (mFootStep < 0)
                {
                    mFootStep = 0;
                    for (BaseSubobj s = GetRoot().Next(); s != null; s = s.Next())
                    {
                        BaseFoot f = (BaseFoot)s.GetInterface(BaseFoot.ID);
                        if (f == null) continue;
                        if (mFootStep == 0) mFootStep = f.GetStepDist(GetVehicleData().HipAngle);
                        mFootsList.Add(f);
                    }
                }
                // устанавливаем положение ног
                if ((mFootStep > 0 && mFootsList.Count > 3) /*&& (mCurSpeed*scale>0 || mFootFactor<0)*/)
                {
                    if (mFootFactor < 0) mFootFactor = RandomGenerator.Rand01();
                    mFootFactor += (mCurSpeed * scale) / mFootStep;
                    if (mFootFactor > 1) mFootFactor -= 1;
                    //TPLIST_ELEM<BaseFoot>* t;
                    //float f = mFootFactor, c = 1f / mFootsList.Count;
                    //for (t = mFootsList.Head(); t != null; t = t->Next())
                    //{
                    //    t->Data()->ProcessStep(f, c, GetVehicleData()->HipAngle, GetVehicleData()->ThighHeight);
                    //    f += c; if (f > 1) f -= 1;
                    //}
                    float f = mFootFactor, c = 1f / mFootsList.Count;
                    foreach (BaseFoot t in mFootsList)
                    {
                        t.ProcessStep(f, c, GetVehicleData().HipAngle, GetVehicleData().ThighHeight);
                        f += c; if (f > 1) f -= 1;
                    }
                }
            }
            rScene.UpdateHM(pHash);
            //UpdateShadow();
        }

        return ret;
    }

    // от iContact
    public override Vector3 GetSpeed()
    {
        return Speed;
    }
    public override int GetState()
    {
        return mpAutopilot.GetState();
    }
    public override bool IsSurfaced()
    {
        return true;
    }
    public override float GetMaxSpeed()
    {
        return (GetMaxCoeff() * GetVehicleData().MaxSpeed);
    }
    public override float GetMaxCornerSpeed()
    {
        return GetVehicleData().MaxAngleSpeed;
    }

    public override int getUnitType() { return (int)iSensorsDefines.UT_VEHICLE; }

    // работа с подобъектами
    protected BaseTurret MainTurret;
    public override void OnAddSubobj(BaseSubobj s)
    {
        base.OnAddSubobj(s);
        if (MainTurret == null && FirstTurret != null)
        {
            MainTurret = FirstTurret;
            FirstTurret = null;
        }
    }
    public override void OnSubSubobj(BaseSubobj s, bool call_from_destructor = false)
    {
        if (s == GetRoot())
        {
            //SafeRelease(engine_sound);
            IRefMem.SafeRelease(engine_sound);
            engine_sound = null;
        }
        // это foot?
        BaseFoot f = (BaseFoot)s.GetInterface(BaseFoot.ID);
        if (f != null)
        {
            //TPLIST_ELEM<BaseFoot>* pf = mFootsList.Find(f);
            //if (pf != null) delete mFootsList.Sub(pf);
            mFootsList.Remove(f);
        }
        // передаем объекту
        base.OnSubSubobj(s, call_from_destructor);
    }

    // визуализация
    private bool have_visuals;
    //# ifdef VISUALIZE_DIRS
    //interface ILaser     *mpCurDest,*mpCurDir,*mpCurOrder;
    //#endif
    SceneVisualizer pVis;
    void ProcessVisuals(float scale)
    {
        //TODO вернуть обработку отрисовки пыли на место
        //if (DetailStage < DETAIL_STAGE_NONE)
        //{
        //    if (dust_update)
        //    {
        //        if (last_ground_type >= 0)
        //        {
        //            trace_result.object= 0;
        //            trace_result.ground_type = last_ground_type;
        //        }
        //        else
        //        {
        //            trace_result.object= pFPO;
        //        }
        //        for (DustCont* c = dusts.Head(); c; c = c->Next())
        //            c->dust->Setup(trace_result.object, trace_result.ground_type, trace_result.org, trace_result.normal, Speed, GetThrustCoefficient());
        //    }
        //}
        //else
        //{
        //    for (DustCont* c = dusts.Head(); c; c = c->Next())
        //        c->dust->Destroy();
        //}
    }
    void DeleteVisuals()
    {
        //# ifdef VISUALIZE_DIRS
        //        SafeRelease(mpCurDir);
        //        SafeRelease(mpCurDest);
        //        SafeRelease(mpCurOrder);
        //#endif
        //        for (DustCont* c = dusts.Head(); c; c = c->Next())
        //            c->dust->Die(1);
        //        dusts.Free();
        //        SafeRelease(engine_sound);
        IRefMem.SafeRelease(engine_sound);
    }


    // звук
    I3DSoundEvent engine_sound;

    // пыль
    TraceResult trace_result;
    bool dust_update;
    List<DustCont> dusts = new List<DustCont>();
    public void AddDust(Dust _dust)
    {
        dusts.Insert(0, new DustCont(_dust));
        have_visuals = true;
    }
    void ShowDust()
    {
        if (dusts.Count > 0)
            dust_update = true;
    }

    // ноги
    private float mFootFactor;
    private float mFootStep;
    private List<BaseFoot> mFootsList = new List<BaseFoot>();

    // физика езды
    protected AheadInfo mAheads = new AheadInfo();
    public bool mInTunnel;
    protected bool mUseTerrain;
    public float mPathDist;
    protected float max_z;
    protected Vector3 ahead_normal, temp;
    protected int last_ground_type;
    public float last_ground_cos;
    protected bool mFirstTick;
    public Vector3 Speed;                   // линейная скорость по трем осям

    const float CUT_ANGLE = 0.0005f;
    /// <summary>
    /// физика
    /// </summary>
    /// <param name="scale"></param>
    /// <returns></returns>
    public bool ProcessPhysic(float scale)
    {
        GetThrustCoeff();
        mCurrentThrust = RouteMath.ApproachValue(mCurrentThrust, mTargetThrust, GetVehicleData().MaxAccel * scale * GetVehicleData().OO_MaxSpeed);
        // обработка положения ручки     
        if (mAheads.mBadAngle)
            mTargetThrust = mCurrentThrust = 0f;

        mCurSpeed = GetVehicleData().MaxSpeed * mCurrentThrust;
        if (myMaxSpeed > 0 && mCurSpeed > myMaxSpeed)
            mCurSpeed = myMaxSpeed;

        //rScene.Message("speed %f",MPS2KPH(mCurSpeed));
        // если летим, то можно и не рулить 
        float angle = GetVehicleData().MaxAngleSpeed * scale * mStick;
        if (mPaused == false && mGrounded && (angle < -CUT_ANGLE || angle > CUT_ANGLE))
            pFPO.TurnRightPrec(angle);
        if (!mUseTerrain)
            ProcessOffSight(scale);
        else
            ProcessOnSight(scale, mCurSpeed);

        updateAngles();
        return true;
    } // физика

    /// <summary>
    /// физика простая
    /// </summary>
    /// <param name="scale"></param>
    /// <param name="speed"></param>
    /// <returns></returns>
    bool ProcessOnSight(float scale, float speed)
    {
        Vector3 ahead;
        Vector3 new_org;
        // если перед нами плохой наклон
        if (last_ground_cos < terrpdef.PASS_ANGLE)
            mAheads.mBadAngle = true;
        else
            mAheads.mBadAngle = false;

        last_ground_cos = 1f;

        // вычисляем новую позицию
        Speed = pFPO.Dir * speed;
        new_org = pFPO.Org + Speed * scale;

        // если нужно интерполировать то интерполируем 
        Vector3 adir = pFPO.Dir;
        ahead = new_org + adir * GetMaxZ() * 0.8f;
        float y1, y2;
        y1 = GetGroundLevelWithoutNormal(new_org, out temp, last_ground_type);
        y2 = GetGroundLevelWithNormal(ahead, out temp);
        new_org.y = y1 - GetMinY();
        ahead.y = y2 - GetMinY();

        last_ground_cos = temp.y;
        // считаем требуюмую нормаль
        MATRIX mtrx = pFPO; //Вот как-то странно это.
        RouteMath.SetupMatrix(ahead - new_org, temp, new_org, 1.2f * scale, ref mtrx);
        //pFPO = (FPO)mtrx;
        pFPO.Set(mtrx);
        mGrounded = true;
        if (mpAutopilot != null && !mpAutopilot.IsOnEarth())
            last_ground_type = -1;
        return true;
    }
    /// <summary>
    /// физика туннелей и мостов
    /// </summary>
    /// <param name="scale"></param>
    /// <returns></returns>
    bool ProcessOffSight(float scale)
    {
        mAheads.mBadAngle = false;
        last_ground_cos = 1f;
        mGrounded = true;

        // вычисляем текущую тягу 
        // проверка пустой тяги
        if (mCurSpeed < 0.01f) return true;

        // вычисляем новую позицию
        Vector3 new_org;
        Speed = pFPO.Dir * mCurSpeed;
        new_org = pFPO.Org + Speed * scale;

        Vector3 newdir = mpAutopilot.mNowDest - pFPO.Org;
        float d = newdir.magnitude;
        if (d > 1f)
            newdir /= d;
        else
            newdir = pFPO.Dir;
        pFPO.Dir = newdir;
        pFPO.Right = Vector3.Cross(pFPO.Up, pFPO.Dir);
        pFPO.Up = Vector3.Cross(pFPO.Dir, pFPO.Right);
        pFPO.Up.Normalize();
        pFPO.Right.Normalize();
        pFPO.Dir.Normalize();
        pFPO.Org = new_org;
        if (mpAutopilot != null && !mpAutopilot.IsOnEarth())
            last_ground_type = -1;
        return true;
    }
    public void SetTraceMode(bool use_terrain, bool in_tunnel = false) { mUseTerrain = use_terrain; mInTunnel = in_tunnel; }
    //public Prediction LoadPrediction(DWORD name) { return rScene.LoadGameData<Prediction>(name); }
    public Prediction LoadPrediction(DWORD name) { return rScene.GetGameData().mpGameDatas.GetBlock(name).Convert<Prediction>(); }

    float GetGroundLevelWithNormal(Vector3 _org, out Vector3 _norm)
    {
        float ret1 = GetGroundLevelWithoutNormal(_org, out _norm);
        _norm.Normalize();
        return ret1;
    }
    public float GetGroundLevelWithoutNormal(Vector3 _org, out Vector3 _norm, int last_gr_type = -1)
    {
        TraceResult res;
        res = rScene.GroundLevelTr(_org.x, _org.z);
        if (last_gr_type == -1)
            last_gr_type = res.ground_type;
        _norm = res.normal;
        return res.dist;
    }
    void SetupOnGround(Vector3 v1)
    {
        Vector3 norm;
        max_z = pFPO.MaxZ();
        mUseTerrain = true;
        float y = GetGroundLevelWithNormal(v1, out norm);
        pFPO.Up = norm;
        pFPO.Right = Vector3.Cross(pFPO.Up, pFPO.Dir);
        pFPO.Dir = Vector3.Cross(pFPO.Right, pFPO.Up);
        pFPO.Up.Normalize();
        pFPO.Right.Normalize();
        pFPO.Dir.Normalize();
        pFPO.Org.y = y - GetMinY();
    }

    // debug
    void PrintVector(Vector3 vect)
    {
        rScene.Message("{0} {1} {2} {3}", vect.x.ToString(), vect.y.ToString(), vect.z.ToString(), vect.magnitude.ToString());
    }

    BaseCollidingForBaseVehicle m_pColliding;


    // ручки управления 
    protected float mStick;            // положение ручки
    protected float mCurrentThrust;    // положение текущее РУДов
    public float mTargetThrust;     // положение требуемое РУДов

    public bool mReachedFlag;      // достигли ли точки
    protected bool mPaused;

    protected float mCurCoeff;
    public float mCurSpeed;
    protected bool mGrounded;
    protected bool myTakeoff;

    void GetThrustCoeff()
    {
        float ret1 = GetMaxCoeff();
        if (ret1 < mTargetThrust)
            mTargetThrust = ret1;
        if (IsSupressed)
            mTargetThrust = 0;
        if (mPaused)
            mTargetThrust = 0;
    }
    public VEHICLE_DATA GetVehicleData()
    {
        return (VEHICLE_DATA)GetObjectData();
    }
    float GetMaxCoeff()
    {
        if (last_ground_type >= 0)
            return GetVehicleData().SpeedCoeff[last_ground_type];
        return 1f;
    }

    public float GetThrustCoefficient() { return mCurrentThrust; }
    public BaseVehicleAutopilot GetAutopilot() { return mpAutopilot; }


    public void setHangarFlag(bool tr) { myTakeoff = tr; }
    bool isHangaring() { return myTakeoff; }

    // автопилоты
    public BaseVehicleAutopilot mpAutopilot;
    float myMaxSpeed;

    // от MovementSystemVehicle 
    public virtual void FollowUnit(iContact f, float dist)
    {
        if (mpAutopilot.GetState() != iSensorsDefines.CS_IN_GAME) return;
        BaseVehicleAutopilotFollow m = (BaseVehicleAutopilotFollow)mpAutopilot.GetInterface(BaseVehicleAutopilotFollow.ID);
        if (m == null)
        {
            //delete mpAutopilot;
            mpAutopilot = m = new BaseVehicleAutopilotFollow(rScene, this);
        }
        m.SetFollow(f, dist);
    }
    public virtual void NearUnit(iContact f, float dist, Vector3 delta)
    {
        if (mpAutopilot.GetState() != iSensorsDefines.CS_IN_GAME) return;
        BaseVehicleAutopilotNear m = (BaseVehicleAutopilotNear)mpAutopilot.GetInterface(BaseVehicleAutopilotNear.ID);
        if (m == null)
        {
            //delete mpAutopilot;
            mpAutopilot = m = new BaseVehicleAutopilotNear(rScene, this);
        }
        m.SetNear(f, dist, delta);
    }
    public virtual void MoveTo(Vector3 v, bool use_roads, float time)
    {
        if (mpAutopilot.GetState() != iSensorsDefines.CS_IN_GAME) return;
        BaseVehicleAutopilotMoveTo m = (BaseVehicleAutopilotMoveTo)mpAutopilot.GetInterface(BaseVehicleAutopilotMoveTo.ID);
        if (m == null)
        {
            //delete mpAutopilot;
            mpAutopilot = m = new BaseVehicleAutopilotMoveTo(rScene, this);
        }
        m.OnSetRoute(v, use_roads, time);
    }
    public virtual void Manual()
    {
        if (mpAutopilot.GetState() != iSensorsDefines.CS_IN_GAME) return;
        if (mpAutopilot.GetInterface(BaseVehicleAutopilotManual.ID) != null) return;
        //delete mpAutopilot;
        mpAutopilot = new BaseVehicleAutopilotManual(rScene, this);
    }
    public virtual bool Land(iContact c)
    {
        if (mpAutopilot.GetState() != iSensorsDefines.CS_IN_GAME) return false;
        BaseHangar h = (BaseHangar)c.GetInterface(BaseHangar.ID);
        // проверка на вшивость
        BaseVehicle cr = (BaseVehicle)this;
        if (h == null || h.canHandleUnit((iContact)cr.GetInterface(iContact.ID)) == 0)
            throw new System.Exception(string.Format("Vehicle \"{0}\": incompatible hangar!", cr.GetObjectData().FullName));
        // создаем автопилот посадки
        //delete mpAutopilot;
        mpAutopilot = new BaseVehicleAutopilotLand(rScene, cr, c, h);
        return true;
    }
    public virtual void Pause(bool pause)
    {
        mPaused = pause;
    }
    public virtual bool IsPaused()
    {
        return mPaused;
    }
    public virtual void setMaxSpeed(float speed) { myMaxSpeed = speed; }

    // от BaseUnit
    public override float GetSensorsVisibility()
    {
        return mInTunnel ? 0 : base.GetSensorsVisibility();
    }

    // непосредственное управление
    public virtual void SetStick(float v)
    {
        if (v < -1f) v = -1f;
        if (v > 1f) v = 1f;
        mStick = v;
    }
    public virtual void SetThrust(float v)
    {
        if (v < -1f) v = -1f;
        if (v > 1f) v = 1f;
        mTargetThrust = v;
    }
    public virtual float GetStick()
    {
        return mStick;
    }
    public virtual float GetThrust()
    {
        return mCurrentThrust;
    }

    // управление движением
    public virtual bool GetReachedFlag()
    {
        return mReachedFlag;
    }
    public virtual void ClearRoute()
    {
        if (mpAutopilot.IsOnBridge()) return;
        //Debug.Log("AP " + mpAutopilot);
        //Debug.Log("AP GI " + mpAutopilot.GetInterface(BaseVehicleAutopilotNonManual.ID));

        BaseVehicleAutopilotNonManual mp = (BaseVehicleAutopilotNonManual)mpAutopilot.GetInterface(BaseVehicleAutopilotNonManual.ID);

        if (mp != null)
            mp.OnStopRoute();
        //#pragma message ("     EEI: autopilot assert")
        //AssertBp(mp);
    }

    // от BaseItem
    //   public:
    //public override DWORD GetCreatePktLength(DWORD Code, DWORD Offset);
    //   public override DWORD GetCreatePkt(ItemDataPacket*, DWORD Offset);
    //   public override bool OnDataPacket(float PacketDelay, ItemDataPacket*);
    //   public override DWORD GetUpdatePktData(Vector3& Org);
    //   public override void GetUpdatePkt(ItemDataPacket*);

    // создание/удаление

    /// <summary>
    /// общая часть инициализации
    /// </summary>
    private void BasePrepare()
    {
        // визуализация - часть 1
        pVis = rScene.GetSceneVisualizer();
        have_visuals = false;
        dust_update = false;
        // визуализация - часть 2
        if (pVis != null)
        {
            //// GSParam sound_params(true,false,&pFPO->Org,&Speed,this,VehicleEngineUpdate::UpDate);
            VehicleSoundController ctr = new VehicleSoundController(this);

            engine_sound = pVis.Get3DSound().LoadEvent(
              "VEHICLE", GetVehicleData().FullName, "Move", true, false, ctr);
            if (engine_sound != null) engine_sound.Start();
            ctr.Release();

            //# ifdef VISUALIZE_DIRS
            //            mpCurDest = rScene.GetSceneVisualizer()->GetSceneApi()->CreateLaser();
            //            mpCurDest->SetParams(*pFPO, 100.f, 2.f, 2.f, FVec4(1.f, 0.1f, 0.2f, 0.8f));
            //            mpCurDir = rScene.GetSceneVisualizer()->GetSceneApi()->CreateLaser();
            //            mpCurDir->SetParams(*pFPO, 100.f, 2.f, 2.f, FVec4(1.f, 0.1f, 0.2f, 0.8f));
            //            mpCurOrder = rScene.GetSceneVisualizer()->GetSceneApi()->CreateLaser();
            //            mpCurOrder->SetParams(*pFPO, 1.f, 0.5f, 0.5f, FVec4(1.f, 0.1f, 0.2f, 0.8f));
            //#endif

            // слоты
            BaseVehicleSlotEnumer Helper = new BaseVehicleSlotEnumer(pVis, this);
            pFPO.EnumerateSlots(Helper);
            ShowDust();
            //CreateShadow(true);
        }
        // Vehicle
        mStick = 0;
        mReachedFlag = false;
        mCurrentThrust = 0;
        mTargetThrust = 0;
        mPaused = false;

        mCurSpeed = 0;
        mPathDist = 10f;
        // BaseVehicle
        // physics
        Speed = Vector3.zero;
        ahead_normal = Vector3.up;
        mInTunnel = false;
        mUseTerrain = true;
        // navigation
        last_ground_cos = 1f;
        mGrounded = true;
        last_ground_type = -1;
    }

    struct SpeedValue
    {
        public SpeedValue(float t, float s)
        {
            myTime = t;
            mySpeed = s;
        }
        public float myTime, mySpeed;
    };

    //AnyDTab<SpeedValue*> mySpeedValues;
    List<SpeedValue> mySpeedValues = new List<SpeedValue>();
    void updateSpeedValue(SpeedValue sv)
    {
        mySpeedValues.Add(sv);
        if (mySpeedValues[0].myTime < sv.myTime - 2)
            mySpeedValues.RemoveAt(0);
    }
    public BaseVehicle(BaseScene s, DWORD h, OBJECT_DATA od) : base(s, h, od)
    {
        MainTurret = null;
        mpAutopilot = null;
        engine_sound = null;
        pVis = null;
        mFootStep = -1;
        mFootFactor = -1;
        mFirstTick = true;
        m_pColliding = null;
        myTakeoff = false;
        myMaxSpeed = 0;
    }


    public override void HostPrepare(HostScene s, UnitSpawnData sd, Vector3 v, float a, iContact c)
    {
        // координаты появления
        Vector3 v1 = v;
        base.HostPrepare(s, sd, v1, a, c);
        BasePrepare();
        // создаен начальный автопилот
        if (c != null)
        {
            BaseHangar hangar = null;
            hangar = (BaseHangar)c.GetInterface(BaseHangar.ID);
            if (hangar == null || hangar.canHandleUnit((iContact)GetInterface(iContact.ID)) == 0)
                throw new System.Exception(string.Format("Vehicle \"{0}\": incompatible hangar!", GetObjectData().FullName));
            mpAutopilot = new BaseVehicleAutopilotTakeoff(rScene, this, c, hangar);
        }
        else
        {
            mpAutopilot = new BaseVehicleAutopilotManual(rScene, this);
        }
    }

    public bool IsManual()
    {
        throw new System.NotImplementedException();
    }

    //public override void RemotePrepare(RemoteScene*,const ObjectCreatePacket*, DWORD); // инициализация на клиенте
    ~BaseVehicle()
    {
        Dispose();
        //DeleteVisuals();
        //if (m_pColliding)
        //    delete m_pColliding;
        //if (mpAutopilot)
        //    delete mpAutopilot;
    }

    public override void Dispose()
    {
        DeleteVisuals();
        if (m_pColliding != null) m_pColliding.Dispose();
        if (mpAutopilot!=null) mpAutopilot.Dispose();
        base.Dispose();
    }

}
public enum AvoidSignal
{
    AngleBad,
    AngleGood,
    ObjectPossiblyCollide,
    ObjectCollide,
    ObjectNoOne,
    DestinationReached,
    DestinationNotReached
};

struct RouteParam
{
    public float cos_angle;
    public float cos_dist;
};

public struct VehicleControlsData
{
    public float myStick;
    public float myCosAngle;
};

public class BaseVehicleWingman : TLIST_ELEM<BaseVehicleWingman>,IDisposable
{
    private BaseVehicleAutopilotNonManual wingman;
    public BaseVehicleAutopilotNonManual Wingman() { return wingman; }

    public BaseVehicleWingman Next()
    {
        return next;
    }

    public BaseVehicleWingman Prev()
    {
        return prev;
    }

    public void SetNext(BaseVehicleWingman t)
    {
        next = t;
    }

    public void SetPrev(BaseVehicleWingman t)
    {
        prev = t;
    }

    public void Dispose()
    {
        Debug.Log("Disposing of tank wingman " + this);
    }

    public BaseVehicleWingman(BaseVehicleAutopilotNonManual _wingman) { wingman = _wingman; }

    private BaseVehicleWingman next,prev;
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class Prediction : IStormImportable<Prediction>
{
    public const uint iPredictionRoad = 0x3625BE7F;
    public const uint iPredictionGate = 0x1BCEE25A;
    public const uint iPredictionPlainDanger = 0x152D3185;
    public const uint iPredictionPlainGood = 0xE6A290EE;

    public float reach_radius;
    public float prediction_count;
    public float max_speed;
    public float OO_maxspeed;
    public float precise_turn_speed;
    public float OO_precise_turn_speed;

    public Prediction Import(Stream st)
    {
        return StormFileUtils.ReadStruct<Prediction>(st);
    }

    public void Prepare()
    {
        if (max_speed != 0) OO_maxspeed = 1f / max_speed; else OO_maxspeed = 0;
        if (precise_turn_speed != 0) OO_precise_turn_speed = 1f / precise_turn_speed; else OO_precise_turn_speed = 0;
    }

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendFormat("reach_radius {0}\n", reach_radius);
        sb.AppendFormat("prediction_count {0}\n", prediction_count);
        sb.AppendFormat("max_speed {0}\n", max_speed);
        return sb.ToString();
    }
}
public class AheadInfo
{
    public bool mBadAngle;
    public bool mObjectsAhead;
    public bool mObjectsNear;
    public Vector3 mRecommendedDiff;
    public AheadInfo()
    {
        mBadAngle = false;
        mObjectsAhead = false;
        mObjectsNear = false;
        mRecommendedDiff = Vector3.zero;
    }
};

public class BaseVehicleSlotEnumer : ISlotEnum
{
    SceneVisualizer pVis;
    BaseVehicle Owner;
    public bool ProcessSlot(SLOT_DATA sld, int slot_id, FPO r)
    {
        const crc32 DustSlotName = 0x24E2210B;
        Dust dust;
        switch (Hasher.HshString(new string(sld.Name)))
        {
            case DustSlotName:
                if (pVis != null && Owner.GetVehicleData().Dust != null)
                {
                    dust = pVis.CreateDust(Owner.GetVehicleData().Dust, r, sld);
                    if (dust != null)
                        Owner.AddDust(dust);
                }
                break;
        }
        return true;
    }
    public BaseVehicleSlotEnumer(SceneVisualizer _pVis, BaseVehicle o) { pVis = _pVis; Owner = o; }
}

//public class BaseUnit : IBaseUnit
//{
//    protected iContact pContact;
//    protected int UnitDataIndex;
//    protected float ThreatF;
//    protected DWORD ThreatHandle;
//    protected float ThreatC;
//    protected float Condition;
//    protected int MissileCount;
//    protected bool IsSupressed;
//    protected ILog mpLog;
//}
