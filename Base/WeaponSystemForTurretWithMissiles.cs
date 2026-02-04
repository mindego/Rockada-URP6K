using UnityEngine;
using DWORD = System.UInt32;
using System.Collections.Generic;
/// <summary>
/// WeaponSystemForTurretWithMissiles - оружейная система с ракетами
/// </summary>
class WeaponSystemForTurretWithMissiles : WeaponSystemForTurret
{
    public const float MaxRangeC = .8f;
    //public const float ThreatAngleDiff = Mathf.Pow(1f / Storm.Math.GRD2RD(90),2);
    public const float ThreatAngleDiff = 0.405282839f; // ^ Подсчитанное заранее

    #region  от BaseInterface
    new public const uint ID = 0x9E37C76E;
    public override object GetInterface(DWORD cid)
    {
        return (cid == ID ? this : base.GetInterface(cid));
    }

    #endregion
    #region от iWeaponSystemDedicated
    public override void SetTarget(iContact tgt)
    {
        if (tgt != null && tgt.GetInterface(BaseCraft.ID) == null) tgt = null; // умеем стрелять только по крафтам
        if (GetTargetEx() == tgt) return;
        base.SetTarget(tgt);
        if (GetTargetEx() != null) SetThreat(GetOwner().GetHandle(), 0, GetPowerFor(GetTurretData().UnitDataIndex));
        mLockTimer = .0f;
        // уведомляем копии
        if (mrTurret.IsLocal())
        {
            //TODO Нужно ли это вообще, если поддержка сети выкинута нафиг?
            //TurretSwitchStateAndTargetPacket Pkt(mrTurret.GetHandle(), mMissilesList.Counter(), (GetTargetEx()! = 0 ? GetTargetEx()->GetHandle() : THANDLE_INVALID));
            //mrScene.SendItemData(&Pkt);
        }


    }
    #endregion
    #region от WeaponSystemForTurret
    public WeaponSystemForTurretWithMissiles(BaseTurret t) : base(t)
    {

    }
    public override bool presentWeapon(int i) { return true; }
    public override void Prepare(SLOT_DATA sld)
    {
        base.Prepare(sld);
        if (GetFpo() == null) return;
        // заполняем список ракет
        int i = 0;
        foreach (var le in GetTurretData().GunNamesList)
        {
            i++;
            FPO f = (FPO)GetFpo().GetSubObject(le);

            if (f == null) throw new System.Exception(string.Format("Missile turret \"{0}\" on object \"{1}\": cannot find missile {2}",
                GetTurretData().FullName, GetOwner().GetObjectData().FullName, i));

            mMissilesList.Add(f);
        }
    }



    public override void Update(float scale)
    {
        if (GetFpo() == null) return;
        float TgtYangle = GetCenterY();
        
        float discard = 0;
        MATRIX tmp = new MATRIX(GetOwner().GetPosition());
        tmp.Org = GetWorldPos();
        if (ProcessTarget(scale, GetWpnData(), tmp, GetOwner().GetSpeed()) == true) GetAngles(ref discard, ref TgtYangle, ref discard, GetOwner().GetPosition().ExpressPoint(GetTargetOrgEx()));
        // поворот башни
        float dy = MakeTurretTurn(scale, TgtYangle);
        Color statusColor = Color.blue;
        //if (GetTarget() != null) Engine.DebugLine(GetWorldPos(), GetTarget().GetOrg(), Color.red);
        // проверяем прицел
        mTurretAim = .0f;
        WPN_DATA_MISSILE wpnData = (WPN_DATA_MISSILE)GetWpnData();
        if (GetTargetEx() != null && !GetTargetEx().IsInSF() && !GetTargetEx().IsOnlyVisual() && !GetOwner().IsInSF() && AreMissilesLeft())
        {
            // коэфф. наведения
            float l = Storm.Math.GetSimpleC(dy, ThreatAngleDiff, .9999f) * Storm.Math.GetOneOverC(GetTargetDistEx(), GetWpnData().Range);
            // наведение
            if (dy > wpnData.LockAngle) mLockTimer = .0f; else mLockTimer += scale;
            if (l > 0) mTurretAim = l;
            // если навелись
            if (mrScene.IsHost() &&
                mLockTimer > wpnData.LockTime &&
                GetTargetDistEx() < wpnData.Range * MaxRangeC &&
                GetTargetDistEx() > wpnData.MinRange &&
                GetTargetEx().GetMissileCount() < GetPowerFor(wpnData.UnitDataIndex, GetTargetEx().GetTypeIndex()))
            {
                statusColor = Color.red;
                Fire();
                l = 2f;
            }
            else
            {
                l = 0.1f;
                statusColor = Color.yellow;
                //string debug = "mLockTimer > wpnData.LockTime " + (mLockTimer > wpnData.LockTime).ToString();
                //debug += "\n" + "GetTargetDistEx() < wpnData.Range * MaxRangeC " + (GetTargetDistEx() < wpnData.Range * MaxRangeC).ToString();
                //debug += "\n" + "GetTargetDistEx() > wpnData.MinRange " + (GetTargetDistEx() > wpnData.MinRange).ToString();
                //debug += "\n" + "GetTargetEx().GetMissileCount() < GetPowerFor(wpnData.UnitDataIndex, GetTargetEx().GetTypeIndex()) " + (GetTargetEx().GetMissileCount() < GetPowerFor(wpnData.UnitDataIndex, GetTargetEx().GetTypeIndex())).ToString();
                //debug += string.Format("\n" + "GetTargetEx().GetMissileCount() {0} < GetPowerFor(wpnData.UnitDataIndex {2}, GetTargetEx().GetTypeIndex() {3}) {1}", GetTargetEx().GetMissileCount(), GetPowerFor(wpnData.UnitDataIndex, GetTargetEx().GetTypeIndex()), wpnData.UnitDataIndex, GetTargetEx().GetTypeIndex());
                //Debug.Log(debug);

            }
            // проставляем угрозу
            SetThreat(GetOwner().GetHandle(), l);
        }
        else
        {
            mLockTimer = .0f;
            // проставляем угрозу
            SetThreat(GetOwner().GetHandle());
        }
        if (GetTarget() != null) EngineDebug.DebugLine(GetWorldPos(), GetTarget().GetOrg(), statusColor);
    }
    public override iContact SelectTargetFromLocal(int nTargets, iContact[] Targets, Vector3[] Orgs, float[] TargetWeights, float AgeC)
    {
        iContact best = null;
        float best_v = .0f;
        if (AgeC > .0f) AgeC = 1f / AgeC;
        WPN_DATA_MISSILE wpnData = (WPN_DATA_MISSILE)GetWpnData();
        for (int i = 0; i < nTargets; i++)
        {
            iContact c = Targets[i];
            // проверка типа контакта
            if (c.GetInterface(BaseCraft.ID) == null || c.IsOnlyVisual()) continue;
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
            if (c.GetCondition() <= 0 || c.IsInSF()) continue;
            // поправка на попадание в зону поражения
            float x = 0, y = 0, d = 0; //TODO - Возможно, нужно исправить для корректной работы ракетных турелей
            GetAngles(ref x, ref y, ref d, Orgs[i]); if (d < wpnData.MinRange || d > wpnData.Range) continue; else cv /= d;
            cv *= GetAimC(y); if (cv <= 0) continue;
            // поправка на занятость контакта
            d = c.GetThreatC();
            if (GetTargetEx() == c) { d -= GetTargetThreatC(); cv *= iWeaponSystemDefines.MY_TARGET_C; }
            if (d >= 1) cv *= .1f;
            cv *= TargetWeights[i]; if (cv <= 0) continue;
            // в т.ч. по кол-ву ракет
            if (c.GetMissileCount() >= GetPowerFor(wpnData.UnitDataIndex, c.GetTypeIndex())) cv *= .01f;
            // поправка на эффективность нас против контакта
            cv *= GetImportanceFor(GetTurretData().UnitDataIndex, c.GetTypeIndex()); if (cv <= 0) continue;
            // выбираем лучший
            if (cv <= best_v) continue;
            best = c;
            best_v = cv;
        }
        // взвращаем наилучший результат
        return best;
    }
    #endregion
    #region от iWeaponSystemDedicated - сеть
    //public virtual void GetCreatePkt(TurretSwitchStateAndTargetPacket pPkt);
    //public virtual bool OnDataPacket(TurretSwitchStatePacket pPkt);
    #endregion

    #region own
    private float mLockTimer;
    private List<FPO> mMissilesList = new List<FPO>(); //TODO Возможно, тут лучше Linked List для удаления непосредственно в цикле
    //protected override WPN_DATA_MISSILE GetWpnData() { return (WPN_DATA_MISSILE)base.GetWpnData(); }
    public override WPN_DATA GetWpnData() { return (WPN_DATA_MISSILE)base.GetWpnData(); }
    private bool AreMissilesLeft()
    {
        //TODO Исправить при необходимости подсчёт ракет для ракетной турели
        return (GetTurretData().GunNamesList.Count > 0 ? mMissilesList[0] != null : true);
    }
    private float MakeTurretTurn(float scale, float TgtYangle)
    {
        if (GetTurretData().GunNamesList.Count > 0)
        {
            // поворот башни
            TurnTurret(scale, TgtYangle);
            // ошибка наведения
            float dy = TgtYangle - GetYangle();
            if (GetMaxYangle() - GetMinYangle() >= Storm.Math.PI_2) { if (dy < -Storm.Math.PI) dy += Storm.Math.PI_2; else if (dy > Storm.Math.PI) dy -= Storm.Math.PI_2; }
            return Mathf.Abs(dy);
        }
        else
        {
            return 0;
        }
    }
    private void Fire()
    {
        WPN_DATA_MISSILE wpnData = (WPN_DATA_MISSILE)GetWpnData();
        if (GetTurretData().GunNamesList.Count > 0)
        {
            // отрываем объект
            FPO f = mMissilesList[0];
            mMissilesList.RemoveAt(0); //TODO Возможно, сюда лучше Queue или LinkedList
            //delete mMissilesList.Sub(mMissilesList.Head());
            f.ToWorld();
            // создаем ракету
            ProjectileMissile pr = ((HostScene)mrScene).CreateMissile(wpnData, (iContact)GetOwner().GetInterface(iContact.ID), GetTargetEx(), f);
            Asserts.AssertEx(pr != null);
            // удаляем старый объект
            f.Release();
            // уведомляем копии

            //TODO Возможно, пакеты данных всё-таки нужны.
            //if (mrTurret.IsLocal())
            //{
            //    TurretSwitchStatePacket Pkt(mrTurret.GetHandle(), mMissilesList.Counter());
            //    mrScene.SendItemData(&Pkt);
            //}
        }
        else
        {
            // получаем мировую матрицу
            FPO f = mrTurret.GetFpo();
            MATRIX pos = new MATRIX(f.Org, f.Up, -f.Dir, f.Right);
            if (f.Parent != null) f.Parent.MatrixToWorld(ref pos);
            // создаем ракету
            ProjectileMissile pr = ((HostScene)mrScene).CreateMissile(wpnData, (iContact)GetOwner().GetInterface(iContact.ID), GetTargetEx(), pos);
            Asserts.AssertEx(pr != null);
        }
        // наводимся заново
        mLockTimer = .0f;
    }
    private float GetAimC(float y)
    {
        return (GetTurretData().GunNamesList.Count > 0 ? BaseTurret.GetCenterC(y, GetCenterY(), GetDiffY()) : 1);
    }
    #endregion
};
