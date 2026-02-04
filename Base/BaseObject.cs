using System.Collections.Generic;
using UnityEngine;
using DWORD = System.UInt32;
using crc32 = System.UInt32;
using static iSensorsDefines;
using static AnimationParams;
using static PeriodicCounter;
using UnityEngine.Assertions;

public partial class BaseObject : iBaseActor
{
    BaseActor myBaseActor;

    public void BaseActorInit(BaseScene s)
    {
        myBaseActor = new BaseActor(s);
        myBaseActor.SetOwner(this);
    }

    public void BaseActorDispose()
    {
        myBaseActor.Dispose();
    }

    //public iBaseActor Next()
    //{
    //    return myBaseActor.Next();
    //}

    //public iBaseActor Prev()
    //{
    //    return myBaseActor.Prev();
    //}

    //public void SetNext(iBaseActor t)
    //{
    //    myBaseActor.SetNext(t);
    //}

    //public void SetPrev(iBaseActor t)
    //{
    //    myBaseActor.SetPrev(t);
    //}

    public BaseScene rScene
    {
        get
        {
            return myBaseActor.rScene;
        }
    }
}

public partial class BaseObject : IAnimationServer
{
    // animation
    //AnyRTab<IAnimation> myLCA = new AnyRTab<IAnimation>();
    List<IAnimation> myLCA = new List<IAnimation>();
    //public void prepareAnimation(Tab<AnimationPackage> datas,IAnimationServer ias) { //AnyRTab
    public void prepareAnimation(List<AnimationPackage> datas, IAnimationServer ias)
    { //LIST
        if (rScene.GetSceneVisualizer() != null)
        {
            for (int i = 0; i < datas.Count; i++)
            {
                AnimationPackage datap = datas[i];
                for (int j = 0; j < datap.myAnimations.Count; j++)
                {
                    AnimationData data = datap.myAnimations[j];
                    IAnimation an = rScene.GetSceneVisualizer().createAnimation(iPulseEmitter, ias, data);
                    if (an != null)
                        //myLCA.New(an); // AnyRTab
                        myLCA.Add(an);//List

                }
            }
        }
    }

    public IAnimationSupport getSupport()
    {
        return rScene.getSV().getIAnimationSupport();
    }

    public void addWeight(float weight)
    {
        myAnimationWeight += weight;
    }

    //AnyRTab<IAnimation*> myLCA;
    IAnimationServer getIAnimationServer() { return this; }

    public virtual float getFloat(crc32 name)
    {
        switch (name)
        {
            case iSmokeCounter: return getPeriodic(rScene.GetTime() + myAnimationStartTime, 10);
            case iBlinkCounter: return getPeriodic(rScene.GetTime() + myAnimationStartTime, 1);
            case iCycledCounter: return getPeriodic(rScene.GetTime() + myAnimationStartTime, 4);
            case iDarkness: return rScene.getSV().getDarkness();
            case iThreatC: return GetThreatC();
            //case iHtgrState: return checkHTGRState(); //TODO расскомментировать после реализации ОТРК (ракет земля-земля)
            //case iHtgrTimer:
            //    //rScene.Message("HTRG %p, timer %f", this, getHtgrTimer());
            //    return getHtgrTimer();
            default: return 0f;
        }
    }
    public Vector3 getVector(crc32 name)
    {
        switch (name)
        {
            case iOSpeedName: return GetSpeed();
            default: return Vector3.zero;
        }
    }

    public float getWeight() { return myAnimationWeight; }

    public void enumerateSlots(string slot_id, IAnimationSlotEnum an, bool recurse)
    {
        AnimationEnumSubobj Helper = new AnimationEnumSubobj(slot_id, an);
        GetFpo().EnumerateSlots(Helper, recurse);
    }

    public ILog getLog() { return rScene.GetLog(); }

    private float myAnimationWeight;
    protected float myAnimationStartTime;
}
public partial class BaseObject : IBaseUnit
{
    private BaseUnit myBaseUnit;
    public void BaseUnitInit()
    {
        myBaseUnit = new BaseUnit();
    }

    public iContact pContact
    {
        get => myBaseUnit.pContact;
        set => myBaseUnit.pContact = value;
    }
    public int UnitDataIndex
    {
        get => myBaseUnit.UnitDataIndex;
        set => myBaseUnit.UnitDataIndex = value;
    }
    public float ThreatF
    {
        get => myBaseUnit.ThreatF;
        set => myBaseUnit.ThreatF = value;
    }
    public DWORD ThreatHandle
    {
        get => myBaseUnit.ThreatHandle;
        set => myBaseUnit.ThreatHandle = value;

    }
    public float ThreatC
    {
        get => myBaseUnit.ThreatC;
        set => myBaseUnit.ThreatC = value;
    }
    public float Condition
    {
        get => myBaseUnit.Condition;
        set => myBaseUnit.Condition = value;

    }
    public int MissileCount
    {
        get => myBaseUnit.MissileCount;
        set => myBaseUnit.MissileCount = value;
    }
    public bool IsSupressed
    {
        get => myBaseUnit.IsSupressed;
        set => myBaseUnit.IsSupressed = value;
    }
    public ILog mpLog
    {
        get => myBaseUnit.mpLog;
        set => myBaseUnit.mpLog = value;
    }

    public virtual DWORD GetHandle() //TODO - предположительно дублируется в BaseItem
    {
        return myBaseItem.GetHandle();
    }
    public virtual int GetState()
    {
        return (pFPO != null ? CS_IN_GAME : CS_DEAD);
    }
    public virtual Vector3 GetOrg()
    {
        //#pragma message("  MIKHA: bounding sphere org")
        return pFPO.Org;
    }
    public virtual Vector3 GetDir()
    {
        return pFPO.Dir;
    }
    public virtual Vector3 GetUp()
    {
        return pFPO.Up;
    }
    public virtual Vector3 GetRight()
    {
        return pFPO.Right;
    }
    public virtual float GetHeadingAngle()
    {
        return HeadingAngle;
    }
    public virtual float GetPitchAngle()
    {
        return PitchAngle;
    }
    public virtual float GetRollAngle()
    {
        return RollAngle;
    }
    public virtual float GetRadius()
    {
        //#pragma message("  MIKHA: bounding sphere radius")
        return pFPO.HashRadius;
    }
    public virtual float getMinRadius()
    {
        return pFPO.HashRadius;
    }
    public virtual float GetSensorsRange()
    {
        return SensorsRange;
    }
    public virtual float GetSensorsVisibility()
    {
        return ObjectData.SensorsVisibility;
    }
    public virtual iContact ChangeSideTo(int SideCode)
    {
        // посылаем пакет
        //if (rScene.IsHost())
        //{
        //    ObjectChangeSidePacket Pkt(GetHandle(), SideCode);
        //    rScene.SendItemData(&Pkt);
        //}
        // перерегистрируемся
        rScene.SubUnit(this);
        pContact = rScene.AddUnit(this, SideCode);
        return pContact;
    }
    public virtual string GetName()
    {
        return mpLocalizedName;
    }
    public virtual void SetName(string pName, bool ShouldLocalize)
    {
        //TODO Переделать на корректую установку имени из БД локализации
        //myBaseUnit.SetName(pName, ShouldLocalize);
        Debug.Log("Setting name: " + pName);

        mpName = pName;
        mpLocalizedName = pName;
    }
    // subcontacts
    public virtual bool HasSubContacts()
    {
        return HasDetachedSubobjs;
    }
    public virtual BaseSubobj GetFirstSubContact()
    {
        return SubobjsList.Head();
    }
    public virtual BaseSubobj GetLastSubContact()
    {
        return SubobjsList.Tail();
    }
    // internal part
    public virtual FPO GetFpo()
    {
        return pFPO;
    }
    public virtual HMember GetHMember()
    {
        return pHash;
    }
    public virtual bool IsPlayedByHuman()
    {
        return mIsPlayedByHuman;
    }
    public virtual void SetLog(ILog l)
    {
        mpLog = l;
    }

    public float GetCondition()
    {
        return myBaseUnit.GetCondition();
    }

    public bool IsInSF()
    {
        return myBaseUnit.IsInSF();
    }

    public virtual Vector3 GetSpeed()
    {
        return myBaseUnit.GetSpeed();
    }

    public int GetSideCode()
    {
        return myBaseUnit.GetSideCode();
    }

    public float GetThreatC()
    {
        return myBaseUnit.GetThreatC();
    }

    public float GetThreatF()
    {
        return myBaseUnit.GetThreatF();
    }

    public virtual void StartPrediction()
    {
        myBaseUnit.StartPrediction();
    }

    public virtual void MakePrediction(float scale)
    {
        myBaseUnit.MakePrediction(scale);
    }

    public virtual void EndPrediction()
    {
        myBaseUnit.EndPrediction();
    }

    public virtual bool IsSurfaced()
    {
        return myBaseUnit.IsSurfaced();
    }

    public virtual float GetMaxSpeed()
    {
        return myBaseUnit.GetMaxSpeed();
    }

    public virtual float GetMaxCornerSpeed()
    {
        return myBaseUnit.GetMaxCornerSpeed();
    }

    public int GetTypeIndex()
    {
        return myBaseUnit.GetTypeIndex();
    }

    public string GetTypeName()
    {
        return myBaseUnit.GetTypeName();
    }

    public int GetProtoType()
    {
        return myBaseUnit.GetProtoType();
    }

    public float GetPowerFor(iContact c)
    {
        return myBaseUnit.GetPowerFor(c);
    }

    public float GetImportanceFor(iContact c)
    {
        return myBaseUnit.GetImportanceFor(c);
    }

    public virtual int getUnitType()
    {
        return myBaseUnit.getUnitType();
    }

    public uint GetThreatHandle()
    {
        return myBaseUnit.GetThreatHandle();
    }

    public void SetThreat(uint thr, float f)
    {
        myBaseUnit.SetThreat(thr, f);
    }

    public void AddThreatC(float t)
    {
        myBaseUnit.AddThreatC(t);
    }

    public int GetMissileCount()
    {
        return myBaseUnit.GetMissileCount();
    }

    public void IncMissileCount()
    {
        myBaseUnit.IncMissileCount();
    }

    public void DecMissileCount()
    {
        myBaseUnit.DecMissileCount();
    }

    public iSensors GetSensors()
    {
        return myBaseUnit.GetSensors();
    }

    public ILog GetLog()
    {
        return myBaseUnit.GetLog();
    }

    public object queryObject(uint id, int num)
    {
        int n = 0;
        for (BaseSubobj head = SubobjsList.Head(); head != null; head = head.Next())
        {
            object ptr = head.queryObject(id, 0);
            if (ptr != null)
            {
                if (n++ == num)
                    return ptr;
            }
        }
        return null;
    }
}

//TODO сменить iWeaponSystemTurret на WeaponSystemForBaseObject
/// <summary>
/// BaseObject: базовый класс для всех объектов в рамках BaseScene
/// </summary>
public partial class BaseObject : iBaseActor, iWeaponSystemTurrets, IUsesFPO
{
    // от iBaseInterface
    new public const uint ID = 0x28B02FBB;

    public virtual object GetInterface(uint id)
    {
        switch (id)
        {
            case iWeaponSystemTurrets.ID: return (pContact != null ? (iWeaponSystemTurrets)this : null);
            case IBaseUnit.ID: return (pContact != null ? (IBaseUnit)this : null);
            case BaseObject.ID: return (pContact != null ? this : null);
            case iSensors.ID: return (pContact != null ? pContact.GetSensors() : null);
            case iContact.ID: return (pContact != null ? pContact : null);
            case IBaseItem.ID: return (IBaseItem)this;
            case iBaseActor.ID: return (iBaseActor)this;
            //LinkedList
            //case iBaseVictim.ID: return SubobjsList.First.Value.GetInterface(id);
            //case BaseSubobj.ID: return SubobjsList.First.Value.GetInterface(id);
            //List
            //case iBaseVictim.ID: return SubobjsList[0].GetInterface(id); 
            //case BaseSubobj.ID: return SubobjsList[0].GetInterface(id);
            //TLIST
            case iBaseVictim.ID: return SubobjsList.Head().GetInterface(id);
            case BaseSubobj.ID: return SubobjsList.Head().GetInterface(id);

            default: return null;
        }
    }

    public string UnitName;
    public OBJECT_DATA ObjectData;
    float SensorsRange;
    public UNIT_DATA unitData;
    //public Vector3 Org;
    //public Storm.Matrix myOrg;

    public int DetailStage;
    //public List<BaseSubobj> SubobjsList = new List<BaseSubobj>();
    //public LinkedList<BaseSubobj> SubobjsList = new LinkedList<BaseSubobj>();
    public TLIST<BaseSubobj> SubobjsList = new TLIST<BaseSubobj>();
    public LinkedList<BaseSubobj> mySubobjsLL = new LinkedList<BaseSubobj>();


    // кто мной управляет
    protected string mpName;
    protected string mpLocalizedName;
    protected bool mIsPlayedByHuman;

    // положение FPO в игровой сцене
    public FPO pFPO;
    protected HMember pHash;
    public float HeadingAngle;
    public float PitchAngle;
    public float RollAngle;

    public virtual void SubWeight(float w) { }
    public int GetDetailStage() { return DetailStage; }
    #region от BaseUnit
    //private BaseUnit myBaseUnit;
    #endregion

    #region работа с подобъектами
    protected BaseTurret FirstTurret;
    bool HasDetachedSubobjs;
    bool NeedRecalSubobjs = true;
    protected float MinX, MinY, MinZ, MaxX, MaxY, MaxZ;
    protected float mCriticalDamagedTime;
    #endregion

    public virtual void OnSubSubobj(BaseSubobj s, bool call_from_destructor = false)
    {

        //TODO Восстановить анимацию
        if (!call_from_destructor)
            for (int i = 0; i < myLCA.Count; i++)
                myLCA[i].onDestroyFpo(s.GetFpo(), rScene.GetSceneVisualizer().getParticleKeeper());

        // if need remove it from linked subobj list
        if (isInLL(s.GetData().GetName()))
        {
            //int pos = mySubobjsLL.find(s);

            //Asserts.Assert(pos != -1);
            //mySubobjsLL.Remove(pos);
            mySubobjsLL.Remove(s);
        }

        // если это - башня
        if (s.GetInterface(BaseTurret.ID) != null)
        {
            BaseTurret t = (BaseTurret)s.GetInterface(BaseTurret.ID);
            if (t == FirstTurret)
            {
                FirstTurret = t.GetNextTurret();
            }
            else
            {
                //for (BaseTurret prev = FirstTurret; (prev!=null && prev.GetNextTurret() != t); prev = prev.GetNextTurret()) ;
                //if (prev!=null) prev.SetNextTurret(t.GetNextTurret());

                for (BaseTurret prev = FirstTurret; (prev != null && prev.GetNextTurret() != t); prev = prev.GetNextTurret())
                {
                    if (prev != null && prev.GetNextTurret() == t)
                    {
                        prev.SetNextTurret(t.GetNextTurret());
                        break;
                    }

                }

            }
        }
        // если это root
        if (s.GetFpo() == pFPO)
        {
            // обнуляем указатель на FPO
            pFPO = null;
            // удаляем pHash
            DeleteShadow();
            rScene.DeleteHM(pHash);
            pHash = null;
            // накрываемся
            //rScene.SubUnit(this.myBaseUnit); myBaseUnit.pContact = null;
            rScene.SubUnit(this); myBaseUnit.pContact = null;
        }
    }

    private void DeleteShadow()
    {
        //Тени рисует Unity.
        return;
    }

    //BaseUnit как отдельный класс
    //public override object GetInterface(uint id)
    //{
    //    switch (id)
    //    {
    //        case iWeaponSystemTurrets.ID: return (myBaseUnit.pContact != null ? (iWeaponSystemTurrets)this : null);
    //        case IBaseUnit.ID: return (myBaseUnit.pContact != null ? (IBaseUnit)this : null);
    //        case BaseObject.ID: return (myBaseUnit.pContact != null ? this : null);
    //        case iSensors.ID: return (myBaseUnit.pContact != null ? myBaseUnit.pContact.GetSensors() : null);
    //        case iContact.ID: return (myBaseUnit.pContact != null ? myBaseUnit.pContact : null);
    //        case IBaseItem.ID: return (IBaseItem)this;
    //        case BaseActor.ID: return (BaseActor)this;
    //        //LinkedList
    //        //case iBaseVictim.ID: return SubobjsList.First.Value.GetInterface(id);
    //        //case BaseSubobj.ID: return SubobjsList.First.Value.GetInterface(id);
    //        //List
    //        //case iBaseVictim.ID: return SubobjsList[0].GetInterface(id); 
    //        //case BaseSubobj.ID: return SubobjsList[0].GetInterface(id);
    //        //TLIST
    //        case iBaseVictim.ID: return SubobjsList.Head().GetInterface(id);
    //        case BaseSubobj.ID: return SubobjsList.Head().GetInterface(id);

    //        default: return null;
    //    }

    //}

    //public BaseSubobj GetRoot() { return SubobjsList.Count > 0 ? SubobjsList[0] : null; }
    //public BaseSubobj GetRoot() { return SubobjsList.Count > 0 ? SubobjsList.First.Value : null; }
    public BaseSubobj GetRoot() { return SubobjsList.Head(); }
    public virtual void OnAddSubobj(BaseSubobj s)
    {
        // if need add it to linked subobj list
        if (isInLL(s.GetData().GetName()))
            mySubobjsLL.AddLast(s);

        bool isRoot = s == GetRoot();
        // если это - рут, проставляем Condition
        if (isRoot) Condition = s.GetCondition();
        // если это - башня
        if (s.GetInterface(BaseTurret.ID) != null)
        {
            BaseTurret t = (BaseTurret)s.GetInterface(BaseTurret.ID);
            if (FirstTurret != null)
            {
                BaseTurret prev;
                for (prev = FirstTurret; prev.GetNextTurret() != null; prev = prev.GetNextTurret()) ;
                prev.SetNextTurret(t);
            }
            else
            {
                FirstTurret = t;
            }
        }
        // проверяем детачность (для того, чтобы на первом тике иметь флаг)
        if (s.GetData().GetFlag(SUBOBJ_DATA.SF_DETACHED) != 0) HasDetachedSubobjs = true;
        // проставляем необходимость пересчета
        NeedRecalSubobjs = true;
        prepareAnimation(s.GetData().myAnimations, s.getIAnimationServer()); //TODO Реализовать отрисовку эффектов анимации для субобъектов

    }

    private bool isInLL(uint name)
    {
        int count = ObjectData.myLinkedSubObj.Count;
        for (int i = 0; i < count; i++)
            if (ObjectData.myLinkedSubObj[i] == name)
                return true;
        return false;
    }

    public float SetCondition(float NewCondition, bool IsCritical)
    {
        Debug.Log(string.Format("Switching condition from {0} to {1} {2} for {3}", Condition, NewCondition, IsCritical ? "Critical" : "NonCritical", GetFpo().Top().TextName));
        if (IsCritical && Condition > NewCondition)
            Condition = NewCondition > 0 ? NewCondition : 0;

        makeLLCondition();

        if (rScene.IsHost() && Condition == 0 && mCriticalDamagedTime == 0)
            mCriticalDamagedTime = rScene.GetTime() + ObjectData.CriticalDamagedTime;

        return Condition;
    }

    public virtual void onHullDamaged() { }
    public void makeLLCondition()
    {
        float cond = getLLCondition();
        if (cond != -1)
            Condition = Mathf.Min(cond, Condition);
    }

    protected float getLLCondition()
    {
        float cond = -1;
        // ASSUMPTION : NO OBJECT WITH CRITICAL PARAMS IN LINKED GROUP
        //for (int i = 0; i < mySubobjsLL.Count; i++)
        //{
        //    // if happened => designers are wrong
        //    //Assert(!mySubobjsLL[i]->GetData()->GetFlag(SF_CRITICAL));
        //    cond = Mathf.Max(cond, mySubobjsLL[i].GetCondition());
        //}
        foreach (BaseSubobj z in mySubobjsLL)
        {
            cond = Mathf.Max(cond, z.GetCondition());
        }
        return cond;
    }

    /// <summary>
    /// инициализация на хосте
    /// </summary>
    /// <param name="s"></param>
    /// <param name="sd"></param>
    /// <param name="org"></param>
    /// <param name="ang"></param>
    /// <param name="h"></param>
    public virtual void HostPrepare(HostScene s, UnitSpawnData sd, Vector3 org, float ang, iContact h)
    {
        // создаем FPO
        BasePrepare(ObjectData.RootData.FileName);
        // ставимся в указанные координаты
        //myOrg = new Storm.Matrix(org); //Исправить!
        pFPO.Set(org, Vector3.forward, Vector3.up);
        pFPO.TurnRightPrec(ang);
        //Debug.Log("Placing " + pFPO.TextName + " " + pHash.GetHashCode().ToString("X8") + " @ " + pFPO.Org);
        pContact = rScene.AddUnit(this, (int)sd.SideCode);

        // создаем подобъекты
        s.CreateSubobj(this, null, ObjectData.RootData, pFPO);
        prepareAnimation(ObjectData.myAnimations, getIAnimationServer());
    }

    public OBJECT_DATA GetObjectData() { return ObjectData; }
    public virtual void BasePrepare(string name)
    {
        //Debug.Log("Creating " + name);
        // создаем физический объект
        pFPO = rScene.CreateFPO(name);


        if (pFPO == null) throw new System.Exception(string.Format("Object \"{0}\": cannot load FPO \"{1}\"!", ObjectData.FullName, name));
        pHash = rScene.ConstructHM(pFPO);
        myBaseUnit.UnitDataIndex = ObjectData.UnitDataIndex;
        SensorsRange = ObjectData.SensorsRange;

        //myAnimationStartTime = Rand01() * 40;

    }

    public void SetPlayedByHuman(bool p) { mIsPlayedByHuman = p; }

    public BaseObject(BaseScene s, DWORD h, OBJECT_DATA od) 
    {
        BaseActorInit(s);
        ObjectData = od;
        mIsPlayedByHuman = false;
        BaseItemInit(s, h);
        BaseUnitInit();
    }

    public virtual bool Move(float scale)
    {
        if (pFPO == null) return false;

        //TODO Реализовать самоубиение при потере критического подобъекта
        //if (mCriticalDamagedTime > 0 && mCriticalDamagedTime < rScene.GetTime())
        //    GetRoot()->AddDamage(THANDLE_INVALID, WeaponCodeUltimateDeath, 0);
        if (NeedRecalSubobjs == true && RecalSubobjs() == false) return false;
        ThreatF -= Mathf.Clamp(ThreatF, 0, scale * .5f);
        return true;

    }

    public void RequestSubobjRecalc() { NeedRecalSubobjs = true; }
    /// <summary>
    /// работа с подобъектами
    /// </summary>
    /// <returns></returns>
    //public virtual bool RecalSubobjs()
    //{
    //    NeedRecalSubobjs = false;
    //    HasDetachedSubobjs = false;

    //    if (pFPO != null)
    //    {
    //        pFPO.RecalcRadius();
    //        rScene.UpdateHM(pHash);
    //        return true;
    //    }
    //    return false;

    //}

    public virtual bool RecalSubobjs()
    {
        // проставляем начальное значение SensorsRange
        SensorsRange = ObjectData.SensorsRange;
        // сбрасываем флаг пересчета
        NeedRecalSubobjs = false;
        HasDetachedSubobjs = false;
        // перебираем все живые подобъекты
        for (BaseSubobj s = GetRoot(); s != null; s = s.Next())
        {
            if (s.ResetState() == false) continue;
            // пересчитываем BoundingBoxes
            if (s == GetRoot())
            {
                // проставляем начальные значения
                MinX = GetRoot().GetMin().x;
                MinY = GetRoot().GetMin().y;
                MinZ = GetRoot().GetMin().z;
                MaxX = GetRoot().GetMax().x;
                MaxY = GetRoot().GetMax().y;
                MaxZ = GetRoot().GetMax().z;
            }
            else
            {
                // переводим в top
                Vector3 vMin = s.GetMin();
                Vector3 vMax = s.GetMax();
                for (RO f = s.GetFpo(); f.Parent != null; f = f.Parent)
                {
                    vMin = f.ProjectPoint(vMin);
                    vMax = f.ProjectPoint(vMax);
                }
                // проверяем на пределы
                if (vMin.x > vMax.x) { float t = vMax.x; vMax.x = vMin.x; vMin.x = t; }
                if (vMin.y > vMax.y) { float t = vMax.y; vMax.y = vMin.y; vMin.y = t; }
                if (vMin.z > vMax.z) { float t = vMax.z; vMax.z = vMin.z; vMin.z = t; }
                if (MinX > vMin.x) MinX = vMin.x; if (MaxX < vMax.x) MaxX = vMax.x;
                if (MinY > vMin.y) MinY = vMin.y; if (MaxX < vMax.x) MaxX = vMax.x;
                if (MinZ > vMin.z) MinZ = vMin.z; if (MaxZ < vMax.z) MaxZ = vMax.z;
            }
            // проверяем радарность
            if (s.GetInterface(BaseRadar.ID) != null)
            {
                BaseRadar r = (BaseRadar)s.GetInterface(BaseRadar.ID);
                if (SensorsRange < r.Dt().SensorRange) SensorsRange = r.Dt().SensorRange;
            }
            // проверяем детачность
            if (s.GetData().GetFlag(SUBOBJ_DATA.SF_DETACHED) != 0) HasDetachedSubobjs = true;
        }
        // пересчитываем радиус
        if (pFPO != null)
        {
            pFPO.RecalcRadius();
            rScene.UpdateHM(pHash);
            return true;
        }
        return false;
    }
    public virtual void Update(float scale)
    {
        //if (mpLog != 0 && mpLog->IsWindowOpen() == false) mpLog = 0;
        if (pFPO == null) return ;
        //DetailStage = rScene.GetDetailStage(pFPO.Org);
        IsSupressed = rScene.IsInSfg(pFPO.Org);
        IParticleAnimation.updateAnimation(myLCA, scale, true);
        //IParticleAnimation.updateAnimation(myLCA, scale, DetailStage != DETAIL_STAGE_NONE);
        //return true;
    }

    #region эмуляция WeaponSystemForBaseObject
    //public virtual float GetCondition()
    //{
    //    return 1f;
    //}
    public virtual void SetAimError(float err)
    {
        BaseTurret t;
        // проставляем всем башням
        for (t = this.FirstTurret; t != null; t = t.GetNextTurret())
            if (t.GetWeaponSystem() != null)
                t.GetWeaponSystem().SetAimError(err);

    }
    public virtual void SetTargets(int nTargets, iContact[] Targets, float[] TargetWeights)
    {
        BaseTurret t;
        // если целей нет
        if (nTargets == 0)
        { // глушим все башни
            for (t = this.FirstTurret; t != null; t = t.GetNextTurret())
                if (t.GetWeaponSystem() != null)
                    t.GetWeaponSystem().SetTarget(null);
            return;
        }
        // переводим все цели в локальную систему координат
        MATRIX pos = this.GetPosition();
        Vector3[] Orgs = new Vector3[nTargets];
        for (int i = 0; i < nTargets; i++)
            Orgs[i] = pos.ExpressPoint(Targets[i].GetOrg());
        // вызваем всем башням SelectTarget
        for (t = this.FirstTurret; t != null; t = t.GetNextTurret())
            if (t.GetWeaponSystem() != null)
                t.GetWeaponSystem().SetTarget(t.GetWeaponSystem().SelectTargetFromLocal(nTargets, Targets, Orgs, TargetWeights, .0f));

    }
    public iWeaponSystemDedicated GetNextTurret(iWeaponSystemDedicated prev)
    {
        BaseTurret t = null;
        if (prev == null)
        {
            t = this.FirstTurret;
        }
        else
        {
            for (t = this.FirstTurret; t != null; t = t.GetNextTurret())
                if (t.GetWeaponSystem() == prev)
                    break;
            if (t == null) t = this.FirstTurret;
            else t = t.GetNextTurret();
        }
        return (t != null ? t.GetWeaponSystem() : null);
    }
    #endregion

    public MATRIX GetPosition()
    {
        return (MATRIX)pFPO;
    }

    public void updateAngles()
    {
        pFPO.Vectors2Angles(ref HeadingAngle, ref PitchAngle, ref RollAngle);
    }

    public virtual void Dispose()
    {
        //Debug.LogFormat("Disposing of {0} {1} as BaseObject ",this.GetType().ToString(), ObjectData.FullName);
        if (pContact != null) rScene.SubUnit(this);
        // если удаляется здоровый объект - послать всем клиентам уведомление
        //if (pFPO != null && (rScene.IsHost() || IsLocal()))
        //{
        //    ItemDataPacket p = new(GetHandle(), ODP_DELETE, sizeof(ItemDataPacket));
        //    rScene.SendItemData(&p);
        //}
        //SubobjsList.Clear(); //TODO Возможно, стоит очищать корректно.
        SubobjsList.Free();
        DeleteShadow();
        if (pHash != null) rScene.DeleteHM(pHash);
        if (pFPO != null) pFPO.Release();
        //BaseItem::Release();
        Release();
        //if (mpLocalizedName != mpName) delete[] mpLocalizedName;
        if (mpLocalizedName != mpName) mpLocalizedName = null;
        //        base.Dispose();
        BaseActorDispose();
    }

    public float GetMinX() { return MinX; }
    public float GetMinY() { return MinY; }
    public float GetMinZ() { return MinZ; }
    public float GetMaxX() { return MaxX; }
    public float GetMaxY() { return MaxY; }
    public float GetMaxZ() { return MaxZ; }
}

public partial class BaseObject : IBaseItem
{
    private IBaseItem myBaseItem;

    //public uint GetHandle() //дублируется из IBaseUnit
    //{
    //    return myBaseItem.GetHandle();
    //}

    public BaseScene getScene()
    {
        return myBaseItem.getScene();
    }

    public bool IsLocal()
    {
        return myBaseItem.IsLocal();
    }

    public bool IsRemote()
    {
        return myBaseItem.IsRemote();
    }

    public void Release()
    {
        myBaseItem.Release();
    }

    public bool SetLocal(int DataLength, char[] pData)
    {
        return myBaseItem.SetLocal(DataLength, pData);
    }

    public int SetRemote(char[] pData)
    {
        return myBaseItem.SetRemote(pData);
    }

    private void BaseItemInit(BaseScene s, DWORD h)
    {
        myBaseItem = new BaseItem(s, h, this);
    }
}


/// <summary>
/// WeaponSystemForBaseObject: переходник между iWeaponSystemTurrets и BaseObject
/// </summary>
//class WeaponSystemForBaseObject : iWeaponSystemTurrets
//{
//    public virtual float GetCondition()
//    {
//        return 1f;
//    }
//    public virtual void SetAimError(float err)
//    {
//        BaseObject o = (BaseObject)this;
//        BaseTurret t;
//        // проставляем всем башням
//        for (t = o.FirstTurret; t!=null; t = t.GetNextTurret())
//            if (t.GetWeaponSystem() != null)
//                t.GetWeaponSystem()->SetAimError(err);

//    }
//    public virtual void SetTargets(int nTargets, iContact[] Targets, float[] TargetWeights)
//    {
//        BaseObject o = (BaseObject) this;
//        BaseTurret t;
//        // если целей нет
//        if (nTargets == 0)
//        { // глушим все башни
//            for (t = o.FirstTurret; t!=null; t = t.GetNextTurret())
//                if (t.GetWeaponSystem() != null)
//                    t.GetWeaponSystem().SetTarget(null);
//            return;
//        }
//        // переводим все цели в локальную систему координат
//        const MATRIX&pos = o.GetPosition();
//        VECTOR* Orgs = (VECTOR*)_alloca(sizeof(VECTOR) * nTargets);
//        for (int i = 0; i < nTargets; i++)
//            Orgs[i] = pos.ExpressPoint(Targets[i].GetOrg());
//        // вызваем всем башням SelectTarget
//        for (t = o.FirstTurret; t!=null; t = t.GetNextTurret())
//            if (t.GetWeaponSystem() != 0)
//                t.GetWeaponSystem()->SetTarget(t.GetWeaponSystem().SelectTargetFromLocal(nTargets, Targets, Orgs, TargetWeights, .0f));

//    }
//    public iWeaponSystemDedicated GetNextTurret(iWeaponSystemDedicated prev)
//    {
//        BaseObject  o = (BaseObject*)this;
//        BaseTurret t = null;
//        if (prev == null)
//        {
//            t = o.FirstTurret;
//        }
//        else
//        {
//            for (t = o.FirstTurret; t != null; t = t->GetNextTurret())
//                if (t.GetWeaponSystem() == prev)
//                    break;
//            if (t == null) t = o.FirstTurret;
//            else t = t.GetNextTurret();
//        }
//        return (t != null ? t.GetWeaponSystem() : null);

//    }
//};



