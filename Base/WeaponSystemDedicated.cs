using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// WeaponSystemDedicated: автоматическое сопровождение цели (с эмуляцией ошибки наведения)
/// </summary>
public abstract class WeaponSystemDedicated : iWeaponSystemDedicated
{
    public const DWORD ID = iWeaponSystemDedicated.ID; //странный подход, но иначе унаследовать из интерфейса не получается.

    #region от iWeaponSystemDedicated
    // от iWeaponSystemDedicated
    public virtual void SetAimError(float err)
    {
        mAimError = Mathf.Clamp(err, .0f, 1f);
        updateTargetOrgAndSpeed();
        updateAimError();

    }
    public virtual float GetAimError()
    {
        return mAimError;
    }
    public virtual void SetTarget(iContact pTarget)
    {
        if (mpTarget == pTarget) return;
        if (mpTarget != null) { mpTarget.AddThreatC(-mThreatC); mpTarget.Release(); }
        mpTarget = pTarget;
        if (mpTarget != null) { mpTarget.AddRef(); }
        mThreatC = 0;
        updateTargetOrgAndSpeed();
        //Debug.Log("Target is set for GUNS!: " + mpTarget + " " +mpTarget.GetFpo());
        //Debug.Log(string.Format("Target is set for {0}: {1}",
        //    this,
        //    mpTarget != null ? mpTarget.GetType().ToString() + " " + mpTarget.GetHashCode().ToString("X8") + " " + mpTarget.GetFpo() : "NO TARGET"
        //    ));

    }
    public virtual iContact GetTarget()
    {
        return mpTarget;
    }
    public virtual Vector3 GetTargetOrg()
    {
        return mTargetOrg;
    }
    public virtual float GetTargetDist()
    {
        return mTargetDist;
    }
    public virtual void SetTrigger(bool on) { }
    public virtual bool GetTrigger() { return false; }
    public virtual void SetWeapon(int wpn) { }
    public virtual int GetWeapon() { return 1; }
    #endregion
    #region дополнение к API
    public WeaponSystemDedicated()
    {
        mpTarget = null;
        mTargetOrg = Vector3.zero;
        mTargetSpeed = Vector3.zero;
        mAimError = .0f;
        mTargetDist = .01f;
    }
    ~WeaponSystemDedicated()
    {
        if (mpTarget != null) { mpTarget.AddThreatC(-mThreatC); mpTarget.Release(); }
    }
    /// <summary>
    /// ProcessTarget
    /// </summary>
    /// <param name="scale"></param>
    /// <param name="weapon"></param>
    /// <param name="InWorld">Craft position</param>
    /// <param name="MySpeed">Craft Speed</param>
    /// <returns>true, если цель существует, иначе false</returns>
    public virtual bool ProcessTarget(float scale, WPN_DATA weapon, MATRIX InWorld, Vector3 MySpeed)
    {
        //{
        //    string res = "";
        //    if (mpTarget == null) res += "NO TARGET";
        //    else
        //    {
        //        res += mpTarget.GetFpo();
        //        res += "State: " + mpTarget.GetState() + "\n";
        //        res += "Age: " + mpTarget.GetAge();
        //    }
        //    Debug.Log(string.Format("{0} engaging {1}", this, res));
        //}

        // проверяем наличие цели

        if (mpTarget == null) return false;
        // проверяем состояние цели
        if (mpTarget.GetState() == iSensorsDefines.CS_DEAD) { SetTarget(null); return false; }
        // проверяем видимость цели
        if (mpTarget.GetAge() > 0) { SetTarget(null); return false; }

        // получаем координаты цели
        mTargetOrg = mpTarget.GetOrg();

        // интерполируем скорость
        float sc = (mAimError > .0f ? (1f - mAimError) * scale : 1f);
        mTargetSpeed += (mpTarget.GetSpeed() - mTargetSpeed) * sc;

        // считаем упреждение
        if (weapon != null)
            weapon.GetAim(ref mTargetOrg, mTargetSpeed, InWorld, MySpeed);

        // считаем расстояние до цели
        mTargetDist = (mTargetOrg - InWorld.Org).magnitude;

        // возвращаем true - признак того, что цель существует
        return true;
    }
    public iContact GetTargetEx() { return mpTarget; }
    public Vector3 GetTargetOrgEx() { return mTargetOrg; }
    public Vector3 GetTargetSpeedEx() { return mTargetSpeed; }
    public float GetTargetDistEx() { return mTargetDist; }
    public void SetTargetDistEx(float d) { mTargetDist = d; }
    public float GetTargetThreatC() { return mThreatC; }
    protected void SetThreat(DWORD MyHandle, float ThreatF = 0, float ThreatC = -1)
    {
        if (mpTarget == null) return;
        mpTarget.SetThreat(MyHandle, (ThreatF > iSensorsDefines.MIN_THREAT_F ? ThreatF : iSensorsDefines.MIN_THREAT_F));
        if (ThreatC >= 0)
        {
            float d = ThreatC - mThreatC;
            mpTarget.AddThreatC(d);
            mThreatC += d;
        }
    }
    protected float GetPowerFor(int MyTypeIndex, int TgtTypeIndex = -1)
    {
        if (TgtTypeIndex < 0)
        {
            Asserts.AssertBp(mpTarget != null);
            TgtTypeIndex = mpTarget.GetTypeIndex();
        }
        return UnitDataTable.pUnitDataTable.GetUDTE(TgtTypeIndex, MyTypeIndex).power;
    }
    public float GetImportanceFor(int MyTypeIndex, int TgtTypeIndex = -1)
    {
        if (TgtTypeIndex < 0)
        {
            Asserts.AssertBp(mpTarget != null);
            TgtTypeIndex = mpTarget.GetTypeIndex();
        }
        return UnitDataTable.pUnitDataTable.GetUDTE(TgtTypeIndex, MyTypeIndex).importance;
    }
    protected void updateTargetParams(Vector3 o, Vector3 s, float d) { mTargetOrg = o; mTargetSpeed = s; mTargetDist = d; }
    private iContact mpTarget;
    private Vector3 mTargetOrg;
    Vector3 mTargetSpeed;
    float mAimError;
    float mTargetDist;
    float mThreatC;
    public virtual void updateAimError() { }
    private void updateTargetOrgAndSpeed()
    {
        if (mpTarget != null) mTargetOrg = mpTarget.GetOrg();
        mTargetSpeed.Set(.0f, .0f, .0f);
    }
    #endregion


    public abstract float GetCondition();

    public virtual float GetEfficiency(iContact pTarget = null, float Dist = 0)
    {
        throw new System.NotImplementedException();
    }

    public virtual float GetRange()
    {
        throw new System.NotImplementedException();
    }

    public virtual bool presentWeapon(int idx)
    {
        throw new System.NotImplementedException();
    }

    public abstract iContact SelectTarget(int nTargets, iContact[] Targets, float[] TargetWeights, float MaxAge = 0);

    public abstract float GetAim();
}
