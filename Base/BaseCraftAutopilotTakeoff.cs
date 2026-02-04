using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// BaseCraftAutopilotTakeoff - взлет
/// </summary>
class BaseCraftAutopilotTakeoff : BaseCraftAutopilotManual
{
    // для кверенья
    new public const uint ID = 0xF7B25C3C;
    public override object GetInterface(DWORD id)
    {
        return (id == ID ? this : null);
    }

    // интерфейс с BaseCraft
    public BaseCraftAutopilotTakeoff(BaseCraft cr, iContact c, BaseHangar h) : base(cr)
    {
        cr.setTakeoffTime(Owner.rScene.GetTime());
        // если появляемся в воздухе
        if (h == null)
        {
            mHangarHandle = Constants.THANDLE_INVALID;
            // устанавливает начальную скорость
            Owner.SpeedF = (Owner.Dt().IsPlane == true ? Storm.Math.KPH2MPS(400) : 0);
            // сразу готовы летать
            mStage = TakeOffStages.tsFlight;
        }
        else
        {
            mHangarHandle = c.GetHandle();
            //Debug.Log("Initial mHangarHandle for " + cr.pFPO.TextName + " - " + c + " : " + mHangarHandle.ToString("X8"));
            Asserts.Assert(((BaseHangar)c.GetInterface(BaseHangar.ID)).isDoorClosed());
            // начальная фаза взлета
            StartWait(h);
            HangarSlotData data = h.getDataFor(Owner.pContact);
            myTakeoffAngle = data.myInfo.myTakeoffAngle;
        }
        // инитим параметры крафта
        Owner.Speed = Owner.pFPO.Dir * Owner.SpeedF;
        Vector3 spd = Owner.GetSpeedInLocal();
        Owner.MakeRotation(spd, 0);
        Owner.MakeMove(0, false);
    }
    ~BaseCraftAutopilotTakeoff()
    {
        Dispose();
    }
    public override void Dispose()
    {
        iContact pHng = Owner.rScene.GetContact(mHangarHandle);
        BaseHangar h = (pHng != null ? (BaseHangar)pHng.GetInterface(BaseHangar.ID) : null);
        if (h != null) h.CloseDoor();
    }
    public override bool Move(float scale, bool pred)
    {
        if (pred == true) return true;
        // получаем ангар
        iContact pHng = Owner.rScene.GetContact(mHangarHandle);
        BaseHangar h = (pHng != null ? (BaseHangar)pHng.GetInterface(BaseHangar.ID) : null);
        //Debug.Log(string.Format("Hangar contact {0} mHangarHandle {1} Owner.GetCondition() {2}", pHng == null ? "NOT FOUND" : pHng, mHangarHandle.ToString("X8"), Owner.GetCondition()));
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
        switch (mStage)
        {
            case TakeOffStages.tsWaitForDoor: return ProcessWait(h, scale);
            case TakeOffStages.tsRun: return ProcessRun(h, scale);
            default: return ProcessClimb(h, scale);
        }
    }
    public override int GetState()
    {
        return (mStage == TakeOffStages.tsFlight ? iSensorsDefines.CS_IN_GAME : iSensorsDefines.CS_LEAVING_HANGAR);
    }

    public override bool IsManual() { return false; }

    // свое
    protected DWORD mHangarHandle;
    protected float myTakeoffAngle;
    protected float mLeft;
    protected TakeOffStages mStage;
    protected void StartWait(BaseHangar h)
    {
        // начальная стадия - ждать открытия двери
        mStage = TakeOffStages.tsWaitForDoor;
        h.OpenDoor();
        // определяем координаты появления
        Owner.pFPO.Org = h.GetPointForStart();

        Vector3 tmp = h.GetPointFor(Owner.pContact, false);
        tmp -= Owner.pFPO.Org;
        // сколько осталось проехать до точки отрыва
        mLeft = -tmp.magnitude;
        // поворячиваемся по полосе
        Owner.pFPO.Angles2Vectors(Mathf.Atan2(tmp.x, tmp.z), Mathf.Asin(tmp.y / tmp.magnitude), .0f);
        Owner.SpeedF = 0;
    }
    protected bool ProcessWait(BaseHangar h, float scale)
    {
        // проверка конца фазы
        if (h != null)
        {
            if (h.isDoorOpened() == true)
            {
                StartRun(h);
                return ProcessRun(h, scale);
            }
        }
        // ждём-с
        if (mLeft < 0)
        {
            Owner.pFPO.Org.y -= Owner.GetMinY();
            mLeft = -mLeft;
        }
        // устанавливаем тягу
        Vector3 Thrust = Vector3.zero;
        Owner.SetThrust(Thrust, scale);
        return true;
    }
    protected void StartRun(BaseHangar h) { }
    protected bool ProcessRun(BaseHangar h, float scale)
    {
        // устнавливаем управление
        Owner.Thrust.Set(0, 0, 1);
        Owner.Controls.Set(0, 0, 0);
        // относительно нормальная физика
        Vector3 spd = Owner.GetSpeedInLocal();
        Vector3 Faero = Owner.GetFaero(spd, scale);
        Vector3 Treq = Owner.GetTreqForThrust(Owner.Thrust, spd);
        Vector3 ExtRotate = Owner.ApplyForces(Faero, Treq, spd, scale);
        // компенсируем силу тяжести
        Owner.Speed.y += STORM_DATA.GAcceleration * scale;
        Owner.MakeMove(scale, false);
        // проверка конца фазы
        mLeft -= Owner.SpeedF * scale;
        if (mLeft <= .0f) StartClimb(h);
        // устанавливаем тягу
        Vector3 Thrust = Vector3.forward;
        Owner.SetThrust(Thrust, scale);
        return true;
    }
    protected void StartClimb(BaseHangar h)
    {
        if (h != null) h.CloseDoor();
        mHangarHandle = Constants.THANDLE_INVALID;
        mStage = TakeOffStages.tsClimb;
        HangarSlotData data = h.getDataFor(Owner.pContact);
        Asserts.AssertEx(data != null);
        mLeft = data.myInfo.myTakeoffTime;
    }
    protected bool ProcessClimb(BaseHangar h, float scale)
    {
        // проверка конца фазы
        mLeft -= scale;
        if (mLeft <= 0) mStage = TakeOffStages.tsFlight;
        // набираем высоту
        Owner.Thrust.Set(0, 0, 1);
        float y;
        if (myTakeoffAngle > 0)
            y = myTakeoffAngle > Owner.PitchAngle ? 1 : 0;
        else
            y = myTakeoffAngle < Owner.PitchAngle ? -1 : 0;
        Owner.Controls.Set(y, 0, 0);
        return base.Move(scale, false);
    }
    public DWORD GetHanagrHandle() { return mHangarHandle; }
    public override string Describe()
    {
        string res = GetType().ToString();
        res += "\nStage " + mStage;
        res += "\nmLeft" + mLeft;
        return res;
    }
}


