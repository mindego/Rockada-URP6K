using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// WeaponSystemForTurret - переходник между iWeaponSystemDedicated и BaseTurret
/// </summary>
public abstract class WeaponSystemForTurret : WeaponSystemDedicated
{
    #region от BaseInterface
    new public const uint ID = 0x6B339F27;

    // WeaponSystemForTurret - от BaseInterface
    public virtual object GetInterface(DWORD cid)
    {
        switch (cid)
        {
            case iWeaponSystemDedicated.ID: return (iWeaponSystemDedicated)this;
            case ID: return this;
            default: return null;
        }
    }
    #endregion
    #region  от WeaponSystemDedicated
    // WeaponSystemForTurret - от WeaponSystemDedicated
    public WeaponSystemForTurret(BaseTurret t)
    {
        mrScene = t.rScene;
        mpWpnData = ((TURRET_DATA)t.GetData()).WeaponData;
        mrTurret = t;
        mTurretAim = .0f;
        mpRotateSound = null;
        mRotateSoundStopTime = 0;
        myRotSpeedY = 0;
    }

    ~WeaponSystemForTurret()
    {
        //SafeRelease(mpRotateSound);
        mpRotateSound = null;
    }

    public override float GetCondition()
    {
        return (GetFpo() == null ? .0f : 1f);
    }

    public override float GetEfficiency(iContact pTarget, float Dist)
    {
        return (GetFpo() == null ? .0f : 1f);
    }

    public override void SetTarget(iContact pTarget)
    {
        if (GetTargetEx() == pTarget) return;
        base.SetTarget(pTarget);
        mTurretAim = .0f;
    }

    public override iContact SelectTarget(int nTargets, iContact[] Targets, float[] TargetWeights, float MaxAge)
    {
        // если целей нет
        if (nTargets == 0) return null;
        // переводим все цели в локальную систему координат
        MATRIX pos = GetOwner().GetPosition();
        //VECTOR* Orgs = (VECTOR*)_alloca(sizeof(VECTOR) * nTargets);
        Vector3[] Orgs = new Vector3[nTargets];
        for (int i = 0; i < nTargets; i++)
            Orgs[i] = pos.ExpressPoint(Targets[i].GetOrg());
        // вызваем SelectTargetFromLocal
        return SelectTargetFromLocal(nTargets, Targets, Orgs, TargetWeights, MaxAge);
    }

    public override float GetAim()
    {
        return mTurretAim;
    }
    public override float GetRange()
    {
        return GetWpnData().Range;
    }

    #endregion

    #region дополнение к API
    public virtual void Prepare(SLOT_DATA sld)
    {
        if (GetFpo() == null) return;
        // определяем матрицы переходов
        // положение
        mOrgDir = GetFpo().Dir;
        mOrgRight = GetFpo().Right;
        mRelPos = new MATRIX(GetFpo());
        GetFpo().Parent.MatrixToWorld(ref mRelPos);
        GetFpo().Top().MatrixToLocal(ref mRelPos);
        // углы
        mYangle = 0;
        mMinYangle = GetTurretData().MinY;
        mMaxYangle = GetTurretData().MaxY;
        if (sld != null)
        {
            Debug.Log("Using slot " + new string(sld.Name) + " for " + this.mpWpnData.FullName );
            //TODO Реаализовать корректно максимальные углы
            //const char Dels[] = " (,";
            //char buf[256];
            //StrCpy(buf, sld->Name);
            //const char* c = StrTok(buf, Dels);
            //if (c = StrTok(0, Dels))
            //{
            //    float t = GRD2RD(atof(c));
            //    if (mMinYangle < t) mMinYangle = t;
            //    if (c = StrTok(0, Dels))
            //    {
            //        t = GRD2RD(atof(c));
            //        if (mMaxYangle > t) mMaxYangle = t;
            //    }
            //}
        }
        if (mMinYangle > mMaxYangle)
            throw new System.Exception(string.Format("Turret \"{0}\" on object \"{1}\", slot \"{1}\": MinYangle={2}, MaxYangle={3}!",
              GetTurretData().FullName, GetOwner().GetObjectData().FullName,
              (sld != null ? sld.Name : "<N/A>"),
              Storm.Math.RD2GRD(mMinYangle), Storm.Math.RD2GRD(mMaxYangle)));
        // коэфф. наведения
        mCenterY = (mMaxYangle + mMinYangle) * .5f;
        mDiffY = .75f / (mMaxYangle - mCenterY);
        mCenterX = (GetTurretData().MaxX + GetTurretData().MinX) * .5f;
        mDiffX = .75f / (GetTurretData().MaxX - mCenterX);
        // пересчитываем положение
        UpdateWorldPos();
        // звук
        if (mrScene.GetSceneVisualizer() != null)
        {
            
            Vector3 zvec = Vector3.zero;
            I3DSoundEventController pCtr = RefSoundCtrWrapper.CreateSoundCtrWrapper(mWorldPos, zvec, (DWORD)GetFpo().Top());
            mpRotateSound = mrScene.GetSceneVisualizer().Get3DSound().LoadEvent("Turret", mrTurret.GetData().FullName, "Rotate", true, false, pCtr);

            pCtr.Release();
//            Debug.Log("TODO! Load rotating sound for " + GetTurretData().FullName);
        }

    }
    public virtual void Explode(bool b1, bool b2) { }

    public virtual void UpdateWorldPos()
    {
        if (GetFpo() != null) mWorldPos = GetOwner().GetPosition().ProjectPoint(mRelPos.Org);
        if (mpRotateSound != null && mRotateSoundStopTime < mrScene.GetTime())
        {
            mRotateSoundStopTime += 100000;
            mpRotateSound.Stop();
        }
    }
    public virtual void Update(float scale) { }
    public abstract iContact SelectTargetFromLocal(int nTargets, iContact[] Targets, Vector3[] Orgs, float[] TargetWeights, float MaxAge);
    
    public virtual WPN_DATA GetWpnData() { return mpWpnData; }
    public TURRET_DATA GetTurretData() { return (TURRET_DATA)mrTurret.GetData(); }
    public FPO GetFpo() { return mrTurret.pFPO; }
    public BaseObject GetOwner() { return mrTurret.Owner; }
    public MATRIX GetRelPos() { return mRelPos; }
    protected Vector3 GetWorldPos() { return mWorldPos; }
    protected float GetCenterY() { return mCenterY; }
    protected float GetDiffY() { return mDiffY; }
    protected float GetCenterX() { return mCenterX; }
    protected float GetDiffX() { return mDiffX; }
    protected float GetYangle() { return mYangle; }
    protected float GetMinYangle() { return mMinYangle; }
    protected float GetMaxYangle() { return mMaxYangle; }
    #endregion
    #region own
    // own
    protected float myRotSpeedY;
    private WPN_DATA mpWpnData;
    private float mCenterY, mDiffY, mCenterX, mDiffX;
    private Vector3 mOrgDir, mOrgRight;
    protected MATRIX mRelPos; //TODO вернуть private после отладки.
    private Vector3 mWorldPos;
    private float mYangle;
    private float mMinYangle, mMaxYangle;
    private I3DSoundEvent mpRotateSound;
    //private AudioClip mpRotateSound;
    private float mRotateSoundStopTime;
    public BaseScene mrScene;
    public BaseTurret mrTurret;
    protected float mTurretAim;
    public void GetAngles(ref float x, ref float y, ref float d, Vector3 delta)
    {
        Vector3 dif = mRelPos.ExpressPoint(delta);
        float dst = dif.magnitude;
        //В исходниках "Шторма" в метод передаются указатели, поэтому сравнивается с нулём указатель, а не значение.
        //if (x != 0) x = Mathf.Asin(dif.y / dst);
        //if (y != 0) y = Mathf.Atan2(dif.x, dif.z);
        //if (d != 0) d = dst;
        x = Mathf.Asin(dif.y / dst);
        y = Mathf.Atan2(dif.x, dif.z);
        d = dst;
    }
    public void TurnTurret(float scale, float TgtYangle)
    {
        // поворот башни
        float d = Mathf.Clamp(TgtYangle, mMinYangle, mMaxYangle) - mYangle;
        if (mMaxYangle - mMinYangle >= Storm.Math.PI_2) { if (d < -Storm.Math.PI) d += Storm.Math.PI_2; else if (d > Storm.Math.PI) d -= Storm.Math.PI_2; }
        float l = GetTurretData().SpeedY * scale;
        float t = Mathf.Clamp(d, -l, l);
        mYangle += t;
        myRotSpeedY = t / scale;
        if (mYangle < -Storm.Math.PI) mYangle += Storm.Math.PI_2; else if (mYangle > Storm.Math.PI) mYangle -= Storm.Math.PI_2;
        GetFpo().Dir = mOrgDir;
        GetFpo().Right = mOrgRight;
        GetFpo().TurnRightPrec(mYangle);
        // запускаем звук
        if (mpRotateSound != null)
        {
            mpRotateSound.UpdateController(mWorldPos);
            if (Mathf.Abs(d) > 0.001f)
            {
                mpRotateSound.Start();
                mRotateSoundStopTime = mrScene.GetTime() + .5f;
            }
        }
    }
    #endregion


}
