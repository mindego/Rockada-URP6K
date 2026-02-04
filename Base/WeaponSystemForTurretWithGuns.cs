using UnityEngine;
using DWORD = System.UInt32;
using System.Collections.Generic;
using System;

// TurretGun - у пушки дуло
public class TurretGun
{
    public WeaponSystemForTurretWithGuns mrOwner;
    public FPO mpGun;
    //TODO реализовать дульную вспышку
    //PARTICLE_SYSTEM mpFlash;
    public Vector3 mGunTip;
    public TurretGun(WeaponSystemForTurretWithGuns t)
    {
        mrOwner = t;
        mpGun = null;
    }
    public void SetGun(int GunName)
    {
        mpGun = (FPO)mrOwner.GetFpo().GetSubObject((uint)GunName);
        if (mpGun == null) throw new System.Exception(string.Format("Object \"{0}\", turret \"{0}\": cannot fing gun!",
            mrOwner.GetOwner().GetObjectData().FullName,
            mrOwner.GetTurretData().FullName));
        mGunTip = Vector3.zero;
        mGunTip.Set(mpGun.Org.x + ((mpGun.MaxX() + mpGun.MinX()) * .5f),
            mpGun.Org.y + ((mpGun.MaxY() + mpGun.MinY()) * .5f),
            mpGun.Org.z + mpGun.MaxZ());
    }
    public void Update(float scale, float angle)
    {
        if (mpGun == null) return;
        mpGun.Dir.Set(0, 0, 1); //Vector3.forward
        mpGun.Up.Set(0, 1, 0); //Vector3.up
        mpGun.TurnUpPrec(angle);
    }
    public void Fire(float x, float y, float t = 0)
    {
        if (mpGun == null) return;
        Vector3 org, dir;
        WPN_DATA_BARREL myWpnData = (WPN_DATA_BARREL)mrOwner.GetWpnData();
        CalcWorldVectors(out org, out dir, x + Storm.Math.norm_rand() * myWpnData.DispersionX, y + Storm.Math.norm_rand() * myWpnData.DispersionY);
        new ProjectileLine(
            mrOwner.mrScene,
            //mrOwner.GetWpnData(),
            myWpnData,
            //mrOwner.GetOwner().GetInterface<iContact>(),
            (iContact)mrOwner.GetOwner().GetInterface(iContact.ID),
            org,
            dir, 0,
            mrOwner.mrTurret.IsRemote(),
            t
            );
    }
    public void CalcWorldVectors(out Vector3 org, out Vector3 dir, float x, float y)
    {
        MATRIX tmp = new MATRIX(mrOwner.GetRelPos());
        tmp.TurnRightPrec(y);
        tmp.TurnUpPrec(x);
        tmp.Org += tmp.Right * mGunTip.x + tmp.Up * mGunTip.y + tmp.Dir * mGunTip.z;
        MATRIX pos = mrOwner.GetOwner().GetPosition();
        org = pos.ProjectPoint(tmp.Org);
        dir = pos.ProjectVector(tmp.Dir);
    }
};

/// <summary>
/// WeaponSystemForTurretWithGuns - оружейная система с пушками
/// </summary>
public class WeaponSystemForTurretWithGuns : WeaponSystemForTurret
{
    public const int TS_PAUSED = 0;
    public const int TS_BLOCKED = 1;
    public const int TS_FIRING = 2;
    public const int TS_SINGLE = 3;

    //public  const float ThreatAngleDiff = MathF.Pow(1f / Storm.Math.GRD2RD(60),2);
    public const float ThreatAngleDiff = 0.9118863880019679f; //^ это подсчитано. Возможно, с ошибкой
    public const float MinVisDist = 1;
    public const float sgBurstMinTime = .5f;
    public const float sgBurstDispTime = .1f;

    //friend class TurretGun;

    #region от BaseInterface
    new public const uint ID = 0x5C13CD75;

    public override object GetInterface(DWORD cid)
    {
        return (cid == ID ? this : base.GetInterface(cid));
    }
    #endregion
    #region от iWeaponSystemDedicated
    public override void SetTarget(iContact tgt)
    {
        if (GetTargetEx() == tgt) return;
        base.SetTarget(tgt);
        mTurretState = TS_BLOCKED;
        mNextSwitchTime = 0;
    }
    #endregion
    #region от WeaponSystemForTurret
    public override bool presentWeapon(int i) { return true; }
    public WeaponSystemForTurretWithGuns(BaseTurret t) : base(t)
    {
        mpCurrentGun = null;
        sound = null;
        mOldTgtHandle = Constants.THANDLE_INVALID;
        mOldTurretState = TS_PAUSED;
        myCycleErrorX = 0;
        myCycleErrorY = 0;
        myRotSpeedX = 0;
        myStaticError = Storm.Math.GRD2RD(GetTurretData().WeaponData.myStaticError);
        myDynamicError = Storm.Math.GRD2RD(GetTurretData().WeaponData.myDynamicError);
    }
    ~WeaponSystemForTurretWithGuns()
    {
        //SafeRelease(sound); 
        sound = null;
    }
    public override void Prepare(SLOT_DATA sld)
    {
        base.Prepare(sld);
        if (GetFpo() == null) return;
        // углы
        mXangle = 0;
        { // дула :)
            //for (LIST_ELEM* le = GetTurretData()->GunNamesList.Head(); le; le = le->Next())
            //{
            //    TurretGun* g = mGunsList.AddToTail(new TurretGun(this));
            //    g->SetGun((int)le->Data());
            //}
            //Asserts.AssertBp(mGunsList.Count >0);

            foreach (var le in GetTurretData().GunNamesList)
            {
                TurretGun g = new TurretGun(this);
                g.SetGun((int)Hasher.CodeString(le));
                //mGunsList.Add(g);
                mGunsList.AddLast(g);
            }
            Asserts.AssertBp(mGunsList.Count > 0);
        }
        // оружие
        mNextFireTime = 0;
        mReloadTime = GetWpnData().GetReload();
        if (GetWpnData().IsChained == true) mReloadTime /= mGunsList.Count;
        //mpCurrentGun = mGunsList[0];
        mpCurrentGun = mGunsList.First;
        // звук
        if (mrScene.GetSceneVisualizer() != null)
        {
            I3DSoundEventController ctr =
               RefSoundCtrWrapper.CreateSoundCtrWrapper(GetWorldPos(), GetOwner().GetSpeed(), (DWORD)GetFpo().Top());
            sound = mrScene.GetSceneVisualizer().Get3DSound().LoadEvent(
                "Weapon", GetWpnData().FullName, "Fire", false, true, ctr);
            ctr.Release();
        }
        // следующее переключение
        mTurretState = TS_BLOCKED;
        mNextSwitchTime = 0;
    }

    private string GetContactName(iContact tgt)
    {
        if (tgt == null) return "[NO TARGET]";

        return GetFPOName(tgt.GetFpo());
    }

    private string GetFPOName(FPO fpo)
    {
        if (fpo == null) return "[NO TARGET]";

        return fpo.TextName + " " + fpo.GetHashCode().ToString("X8");
    }
    public override void Update(float scale)
    {
        if (GetFpo() == null) return;

        float TgtXangle = GetCenterX(), TgtYangle = GetCenterY();
        float empty = 0;
        MATRIX tmp = new MATRIX(GetOwner().GetPosition());
        tmp.Org = GetWorldPos();
        if (ProcessTarget(scale, GetWpnData(), tmp, GetOwner().GetSpeed()) == false)
        {
            mTurretState = TS_BLOCKED;
        }
        else GetAngles(ref TgtXangle, ref TgtYangle, ref empty, GetOwner().GetPosition().ExpressPoint(GetTargetOrgEx()));
        // apply real error
        if (mrTurret.GetOwner().GetInterface(BaseCraft.ID) == null)
        {
            TgtXangle += myCycleErrorX;
            TgtXangle = Mathf.Clamp(TgtXangle, -3.14f / 2, 3.14f / 2);
            TgtYangle += myCycleErrorY;
            TgtYangle = Mathf.Clamp(TgtYangle, -3.14f, 3.14f);
        }
        if (mNextSwitchTime < mrScene.GetTime()) SwitchState(TgtXangle, TgtYangle, GetTargetDistEx());
        // поворот башни
        TurnTurret(scale, TgtYangle);
        // подъем стволов
        float d = Mathf.Clamp(TgtXangle, GetTurretData().MinX, GetTurretData().MaxX) - mXangle;
        float l = GetTurretData().SpeedX * scale;
        float t = Mathf.Clamp(d, -l, l);
        mXangle += t;
        myRotSpeedX = t / scale;
        // ошибка наведения
        float dx = TgtXangle - mXangle;
        float dy = TgtYangle - GetYangle();
        if (GetMaxYangle() - GetMinYangle() >= Storm.Math.PI_2) { if (dy < -Storm.Math.PI) dy += Storm.Math.PI_2; else if (dy > Storm.Math.PI) dy -= Storm.Math.PI_2; }
        // проверяем прицел
        mTurretAim = .0f;
        if (mTurretState != TS_BLOCKED && (GetWpnData().Type != WpnDataDefines.WT_PLASMA || GetOwner().IsInSF() == false))
        {
            l = Storm.Math.GetOneOverC(GetTargetDistEx(), GetWpnData().Range) * Storm.Math.GetSimpleC(Mathf.Pow(dx, 2) + Mathf.Pow(dy, 2), ThreatAngleDiff);
            if (l > .0f)
            {
                mTurretAim = l;
                switch (mTurretState)
                {
                    case TS_FIRING: // стреляем
                        if (mTurretAim > .999)
                        { // если навелись
                            l += 1f;
                            if ((mrTurret.IsLocal() || !GetWpnData().SeparatedPacket) && mNextFireTime < mrScene.GetTime()) Fire(TgtXangle, TgtYangle);
                        }
                        break;
                    case TS_SINGLE: // пришел пакет одиночного выстрела
                        l += 1f;
                        Fire(TgtXangle, TgtYangle);
                        mTurretState = TS_FIRING;
                        break;
                }
                SetThreat(GetOwner().GetHandle(), l, GetPowerFor(mrTurret.GetData().UnitDataIndex));
            }
            else
            {
                SetThreat(GetOwner().GetHandle());
            }
        }
        else
        {
            SetThreat(GetOwner().GetHandle());
        }
        // стволы
        //if (GetOwner().GetDetailStage() <= DETAIL_STAGE_HALF)
        if (true) //обновляем всегда!
        {
            //for (TurretGun* g = mGunsList.Head(); g; g = g->Next())
            //    g->Update(scale, mXangle);
            foreach (TurretGun g in mGunsList)
            {
                g.Update(scale, mXangle);
            }
        }
        // уведомляем копии
        if (mrTurret.IsLocal())
        {
            DWORD TgtHandle = (GetTargetEx() != null ? GetTargetEx().GetHandle() : Constants.THANDLE_INVALID);
            if (mOldTgtHandle != TgtHandle || mOldTurretState != mTurretState)
            {
                mOldTgtHandle = TgtHandle;
                mOldTurretState = mTurretState;
                //TurretSwitchStateAndTargetPacket Pkt(mrTurret.GetHandle(), mTurretState, TgtHandle);
                //mrScene.SendItemData(&Pkt);
                //TODO возможно, это надо исправить для корректной работы турелей
            }
        }
    }
    public override void Explode(bool CanStaySelf, bool CanKeepChildren)
    {
        ///SafeRelease(sound); 
        sound = null;
    }
    public override iContact SelectTargetFromLocal(int nTargets, iContact[] Targets, Vector3[] Orgs, float[] TargetWeights, float AgeC)
    {
        //        Debug.Log(string.Format("Processing {0} targets for {1} {2}",nTargets,GetOwner().GetFpo().TextName,this.GetWpnData().FullName));
        iContact CurrentTop = (GetTargetEx() != null ? GetTargetEx().GetTopContact() : null);
        iContact best = null;
        float best_v = .0f;
        if (AgeC > .0f) AgeC = 1f / AgeC;
        for (int i = 0; i < nTargets; i++)
        {
            iContact c = Targets[i];
            // поправка на возраст контакта
            float cv;
            if (AgeC > .0f)
            {
                cv = Storm.Math.GetSimpleC(c.GetAge(), AgeC);
                if (cv == .0f) continue;
            }
            else
            {
                if (c.GetAge() > .0f) continue;
                cv = 1f;
            }
            // поправка на здоровье цели
            if (c.GetCondition() <= 0) continue;
            // поправка на попадание в зону поражения
            float x = 0, y = 0, d = 0;
            GetAngles(ref x, ref y, ref d, Orgs[i]); //TODO возможно, тут будет всё по нулям/ нужно проверить
            if (d > 1f) cv /= d;
            cv *= BaseTurret.GetCenterC(x, GetCenterX(), GetDiffX()); if (cv <= 0) continue;
            cv *= BaseTurret.GetCenterC(y, GetCenterY(), GetDiffY()); if (cv <= 0) continue;
            // поправка на занятость контакта
            float t = c.GetThreatC();
            if (CurrentTop == c.GetTopContact()) { t -= GetTargetThreatC(); cv *= iWeaponSystemDefines.MY_TARGET_C; }
            cv *= iWeaponSystemDefines.GetBusyC(t) * ((GetTurretData().myCraftTurret != 0) ? 1 : TargetWeights[i]); if (cv <= 0) continue;
            // поправка на эффективность нас против контакта
            cv *= GetImportanceFor(GetTurretData().UnitDataIndex, c.GetTypeIndex()); if (cv <= 0) continue;
            // выбираем лучший
            if (cv <= best_v) continue;
            if (IsTargetVisible(c, x, y, d) == false) cv *= .001f;
            if (cv <= best_v) continue;
            best = c;
            best_v = cv;
        }
        // взвращаем наилучший результат
        return best;
    }
    #endregion
    #region от iWeaponSystemDedicated - сеть
    //отключено ввиду отсутствия сети
    //public override void GetCreatePkt(TurretSwitchStateAndTargetPacket* pPkt);
    //public override bool OnDataPacket(const TurretSwitchStatePacket* pPkt);
    #endregion
    #region  own
    private float myCycleErrorX, myCycleErrorY, myRotSpeedX;
    // оружие
    float mNextFireTime;
    float mReloadTime;
    // переключение состояний
    int mTurretState;
    float mNextSwitchTime;
    // сеть
    DWORD mOldTgtHandle;
    int mOldTurretState;
    // стволы
    public LinkedListNode<TurretGun> mpCurrentGun;
    public LinkedList<TurretGun> mGunsList = new LinkedList<TurretGun>();
    float mXangle;    // текущий угол поворота пушек
    // visual
    I3DSoundEvent sound;
    //AudioClip sound;
    private float myStaticError, myDynamicError;
    public void updateCycleError(bool sw = true)
    {
        float rai = GetAimError() * (myStaticError + myDynamicError * (Mathf.Abs(myRotSpeedX) + Mathf.Abs(myRotSpeedY)));
        myCycleErrorX = Storm.Math.norm_rand() * rai;
        myCycleErrorY = Storm.Math.norm_rand() * rai;
        if (sw) mTurretState = TS_PAUSED;
    }
    public override bool ProcessTarget(float scale, WPN_DATA weapon, MATRIX InWorld, Vector3 MySpeed)
    {
        iContact tgt = GetTarget();

        //проверяем наличие цели
        if (tgt != null)
        {
            if (tgt.GetState() == iSensorsDefines.CS_DEAD) { SetTarget(null); return false; }
            if (tgt.GetAge() > 0) { SetTarget(null); return false; }
            Vector3 o = tgt.GetOrg();
            if (weapon != null)
                weapon.GetAim(ref o, tgt.GetSpeed(), InWorld, MySpeed);
            float d = (o - InWorld.Org).magnitude;
            updateTargetParams(o, tgt.GetSpeed(), d);
            return true;
        }
        return false;
    }
    public void SwitchState(float x, float y, float d)
    {
        // на клиенте
        if (mrTurret.IsRemote())
        {
            // если стреляли - прекращаем
            if (mTurretState == TS_FIRING) updateCycleError();
            mNextSwitchTime = mrScene.GetTime() + 3600;
            return;
        }
        // на хосте
        if (GetTargetEx() == null)
        {
            mTurretState = TS_BLOCKED;
            mNextSwitchTime = mrScene.GetTime() + 3600;
            return;
        }
        if (mTurretState == TS_BLOCKED)
        {
            updateCycleError(false);
        }
        if (mTurretState == TS_FIRING)
        {
            updateCycleError();
            mNextSwitchTime = mrScene.GetTime() + GetTurretData().WeaponData.myTurretPauseTimeBase + RandomGenerator.Rand01() * GetTurretData().WeaponData.myTurretPauseTimeDelta;
            return;
        }
        mNextSwitchTime = mrScene.GetTime() + GetTurretData().WeaponData.myTurretFireTimeBase + RandomGenerator.Rand01() * GetTurretData().WeaponData.myTurretFireTimeDelta;
        mTurretState = (IsTargetVisible(GetTargetEx(), x, y, d) ? TS_FIRING : TS_BLOCKED);
        if (!IsTargetVisible(GetTargetEx(), x, y, d))
        {
            mTurretState = TS_BLOCKED;
        }
    }
    bool IsTargetVisible(iContact c, float x, float y, float d)
    {
        Vector3 org, dir;
        //mGunsList[0].CalcWorldVectors(out org, out dir, x, y);
        mGunsList.First.Value.CalcWorldVectors(out org, out dir, x, y);
        //mGunsList.Head()->CalcWorldVectors(org, dir, x, y);
        d = (c.GetOrg() - org).magnitude;
        if (d < MinVisDist || d > GetWpnData().Range + 1000f) return false;
        org += dir * MinVisDist;
        TraceInfo res = mrScene.TraceLine(new Geometry.Line(org, dir, d - MinVisDist), c.GetTopContact().GetHMember(), (int)CollisionDefines.COLLF_ALL);
        return (res.count == 0);
    }
    void Fire(float x, float y)
    {
        if (sound != null)
        {
            sound.UpdateController(GetWorldPos());
            sound.Start();
        }
        // разные типы выстрелов
        if (GetWpnData().IsChained == true)
        {
            mpCurrentGun.Value.Fire(x, y);
            mpCurrentGun = mpCurrentGun.Next != null ? mpCurrentGun.Next : mGunsList.First;
        }
        else
        {
            // рассчитываем время полета
            float t = GetTargetDistEx() / GetWpnData().GetSpeed() - sgBurstDispTime;
            if (t < sgBurstMinTime) t = sgBurstMinTime;
            // стреляем из всех стволов сразу

            for (mpCurrentGun = mGunsList.First; mpCurrentGun != null; mpCurrentGun = mpCurrentGun.Next)
            {
                mpCurrentGun.Value.Fire(x, y, t + RandomGenerator.Rand01() * sgBurstDispTime);
            }

            //foreach(TurretGun tmpGun in mGunsList)
            //{
            //    tmpGun.Fire(x, y, t + RandomGenerator.Rand01() * sgBurstDispTime);
            //}
            //for (mpCurrentGun = mGunsList.Head(); mpCurrentGun != 0; mpCurrentGun = mpCurrentGun->Next())
            //    mpCurrentGun.Fire(x, y, t + RandomGenerator.Rand01() * sgBurstDispTime);
        }
        // если управляемся сами и выстрелы - отдельными пакетами
        //TODO Возможно, стрельба требует допольнительной работы
        //if (mrTurret.IsLocal() && GetWpnData()->SeparatedPacket)
        //{
        //    TurretSwitchStatePacket Pkt(mrTurret.GetHandle(), TS_SINGLE);
        //    mrScene.SendItemData(&Pkt);
        //}
        // время следующего выстрела
        mNextFireTime = mrScene.GetTime() + mReloadTime;
    }
    public override WPN_DATA GetWpnData() { return base.GetWpnData(); }
    #endregion
}

public static class iWeaponSystemDefines
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