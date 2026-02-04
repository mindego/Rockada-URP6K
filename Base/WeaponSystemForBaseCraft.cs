using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// WeaponSystemForBaseCraft - переходних между iWeaponSystemDedicated и BaseCraft
/// </summary>
public class WeaponSystemForBaseCraft : WeaponSystemDedicated
{
    private const float gsMaxRealodTime = 6;
    // от BaseInterface
    new public const uint ID = 0x4DA74A81;
    public virtual object GetInterface(DWORD cid)
    {
        switch (cid)
        {
            case iWeaponSystemDedicated.ID: return (iWeaponSystemDedicated)this;
            case ID: return this;
            default: return null;
        }
    }


    // от iWeaponSystemDedicated
    public override bool presentWeapon(int idx)
    {
        for (BaseWeaponSlot s = mpFirstWS; s != null; s = s.GetNext())
            if (s.GetGroup() == idx) return true;
        return false;
    }
    public override float GetCondition()
    {
        BaseWeaponSlot s;
        float c;
        int i;
        for (s = mpFirstWS, c = 0, i = 0; s != null; s = s.GetNext())
        {
            //Debug.Log(string.Format("weaponslot {0} slot group {1} mWeaponGroup {2}", s, s.GetGroup(), mWeaponGroup));
            if (s.GetGroup() != mWeaponGroup) continue;
            c += s.GetAmmoLoad();
            i++;
        }
        if (i > 0) c /= i;
        return c;
    }
    public override float GetEfficiency(iContact pTarget, float Dist)
    {
        BaseWeaponSlot s;
        float c;
        if (pTarget == null) { pTarget = GetTargetEx(); Dist = GetTargetDistEx(); }
        for (s = mpFirstWS, c = 0; s != null; s = s.GetNext())
        {
            if (s.GetGroup() != mWeaponGroup) continue;
            c += s.GetEffectivness(pTarget, Dist);
        }
        return c;
    }
    public override void SetTarget(iContact pTarget)
    {
        if (pTarget == GetTargetEx()) return;
        base.SetTarget(pTarget);
        if (pTarget != null) SetThreat(mpCraft.GetHandle(), 0, GetPowerFor(mpCraft.GetTypeIndex()));
        // уведомляем копии
        if (mpCraft.IsLocal() || mpCraft.rScene.IsHost())
        {
            //        CraftTargetPacket Pkt(mpCraft.GetHandle(), (pTarget! = 0 ? pTarget.GetHandle() : THANDLE_INVALID));
            ////dprintf("sended CDP_SET_TARGET\n");
            //mpCraft.rScene.SendItemData(&Pkt);
        }

        BaseWeaponSlotMissile pCurrentMissile = getCurMissile();
        if (pCurrentMissile != null && mWeaponGroup == 2)
            pCurrentMissile.initLockTimer();
    }
    public override iContact SelectTarget(int nTargets, iContact[] Targets, float[] TargetWeights, float AgeC)
    {
        iContact CurrentTop = (GetTargetEx() != null ? GetTargetEx().GetTopContact() : null);
        iContact best = null;
        float best_v = .0f;
        if (AgeC > .0f) AgeC = 1f / AgeC;
        for (int i = 0; i < nTargets; i++)
        {
            iContact c = Targets[i];
            // поправка на возраст контакта
            //Debug.Log("поправка на возраст контакта [" + c + "]");


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
            //Debug.Log("1 initial cv: " + cv);
            //Debug.Log("if (mpCraft.IsPlayedByHuman() == true && (c.IsOnlyVisual() == true || c.GetAge() > 0)) continue;");
            if (mpCraft.IsPlayedByHuman() == true && (c.IsOnlyVisual() == true || c.GetAge() > 0)) continue;
            // поправка на здоровье цели
            //Debug.Log("поправка на здоровье цели");
            if (c.GetCondition() <= 0) continue;
            // поправка на попадание в зону поражения
            //Debug.Log("// поправка на попадание в зону поражения");
            Vector3 delta = c.GetOrg() - mpCraft.pFPO.Org;
            cv *= Storm.Math.GetOneOverC(delta.magnitude, 2000f);
            //Debug.Log("2 initial cv: " + cv);
            // поправка на занятость контакта
            //Debug.Log("поправка на занятость контакта");
            float t = c.GetThreatC();
            if (CurrentTop == c.GetTopContact()) { t -= GetTargetThreatC(); cv *= iWeaponSystem.MY_TARGET_C; }
            if (c.GetHandle() == mpCraft.GetThreatHandle())
            {
                cv *= iWeaponSystem.MY_THREAT_C;
            }
            //Debug.Log("3 initial cv: " + cv + " TW " + TargetWeights[i]);
            cv *= iWeaponSystem.GetBusyC(t) * TargetWeights[i]; if (cv <= 0) continue;
            // поправка на эффективность нас против контакта
            //Debug.Log("// поправка на эффективность нас против контакта " + mpCraft.GetTypeIndex() + " vs " + c.GetTypeIndex() + " = " + GetImportanceFor(mpCraft.GetTypeIndex(), c.GetTypeIndex()));
            cv *= GetImportanceFor(mpCraft.GetTypeIndex(), c.GetTypeIndex()); if (cv <= 0) continue;
            //Debug.Log("4 initial cv: " + cv);
            // выбираем лучший
            if (cv <= best_v) continue;
            best = c;
            best_v = cv;
            //Debug.Log("Best is: " + best + " cv " + best_v);
        }
        //Debug.Log("Best target: " + (best == null ? "NO_TARGET" : best) + " for " + mpCraft.GetName() + " " + mpCraft.GetFpo().TextName);
        if (best != null) EngineDebug.DebugLine(mpCraft.GetOrg(), best.GetOrg(), Color.red, 3);
        return best;
    }

    public override float GetAim()
    {
        return myTraceFail ? 0 : mTargetAim;
    }
    public override float GetRange()
    {
        BaseWeaponSlot s;
        for (s = mpFirstWS; s != null; s = s.GetNext())
        {
            if (s.GetGroup() != mWeaponGroup) continue;
            return (s.GetWpnData().Range);
        }
        return .0f;
    }
    public override void SetTrigger(bool on)
    {
        mTrigger = on;
    }
    public override bool GetTrigger()
    {
        return mTrigger;
    }
    public override void SetWeapon(int wpn)
    {
        //Debug.Log(string.Format("Switching from {0} to {1} condition {2}",mWeaponGroup,wpn, GetCondition()));
        mWeaponGroup = Mathf.Clamp(wpn, 0, 2);
    }
    public override int GetWeapon()
    {
        return mWeaponGroup;
    }

    // дополнение к API
    public WeaponSystemForBaseCraft(BaseCraft c)
    {
        mpCraft = c;
        mTargetAim = .0f;
        mTrigger = false;
        mWeaponGroup = 2;
        myPreviousWeaponStatus = 2;
        mpFirstWS = null;
        myPrevTraceLineTime = 0;
        myTraceFail = false;
        for (int i = 0; i < 3; i++)
        {
            mpCurrentWS[i] = null;
            mLastFireTime[i] = 0;
        }
    }
    public virtual void OnAddSubobj(BaseSubobj s)
    {
        // если это оружие
        BaseWeaponSlot nws = (BaseWeaponSlot)s.GetInterface(BaseWeaponSlot.ID);
        if (nws == null) return;
        Debug.Log("Adding weapon " + nws);
        // то добавляем его в список оружия
        BaseWeaponSlot ws;
        BaseWeaponSlot pws;
        for (ws = mpFirstWS, pws = null; ws != null; pws = ws, ws = ws.GetNext())
            if (ws.GetLayoutID() > nws.GetLayoutID()) break;
        if (pws != null) pws.SetNext(nws); else mpFirstWS = nws;
        nws.SetNext(ws);
        // ставим ему группу
        LAYOUT_DATA pWeaponLayout = mpCraft.Dt().GetLayout(LAYOUT_DATA.WEAPON_LAYOUT);
        if (pWeaponLayout != null)
        {
            int i = nws.GetLayoutID();
            Debug.Log("Creating weapon in slot: [" + nws + "] " + " layout_id " + i + " " + pWeaponLayout);
            //for (LAYOUT_ITEM li = pWeaponLayout.Items.Head(); (li && i > 0); li = li.Next(), i--) ;
            //if (li)
            //{
            //    if (StriCmp(li.Value, "PRIMARY") == 0) nws.SetGroup(0);
            //    else
            //    if (StriCmp(li.Value, "SECONDARY") == 0) nws.SetGroup(1);
            //    else nws.SetGroup(2);
            //    if (mpCurrentWS[nws.GetGroup()] == 0) mpCurrentWS[nws.GetGroup()] = nws;
            //}
            //LAYOUT_ITEM li=null;
            //foreach (LAYOUT_ITEM li_search in pWeaponLayout.Items)
            //{
            //    if (i == 0 && li_search == null) break;
            //    li = li_search;
            //    i--;
            //}
            LAYOUT_ITEM li = null;
            if (i< pWeaponLayout.Items.Count) li=pWeaponLayout.Items[i];


            if (li != null)
            {
                if (li.Value == "PRIMARY") nws.SetGroup(0);
                else
                if (li.Value == "SECONDARY") nws.SetGroup(1);
                else nws.SetGroup(2);
                if (mpCurrentWS[nws.GetGroup()] == null) mpCurrentWS[nws.GetGroup()] = nws;
                Debug.Log("Created weapon " + nws.GetGroup() + " in group: [" + li.Value + "] " + pWeaponLayout);
            }
        }
        // проверяем установку mWeaponGroup
        if (mWeaponGroup > nws.GetGroup()) mWeaponGroup = nws.GetGroup();
    }
    public BaseWeaponSlot GetFirstWS() { return mpFirstWS; }
    public BaseWeaponSlot GetCurrentWS(int grp) { return mpCurrentWS[grp]; }
    public virtual void Update(float scale)
    {
        //if (mTrigger) Debug.Log(string.Format("Dakka with {0} ", mpCurrentWS[mWeaponGroup]));
        float TargetDifX = .0f, TargetDifY = .0f;
        BaseWeaponSlotMissile pCurrentMissile = getCurMissile();
        if (myPreviousWeaponStatus != mWeaponGroup && mWeaponGroup == 2 && pCurrentMissile != null)
            pCurrentMissile.initLockTimer();
        { // апдейт цели
            BaseWeaponSlot s = mpCurrentWS[mWeaponGroup];
            mTargetAim = .0f;
            if (mpCraft.IsPlayedByHuman() == true && GetTargetEx() != null && (GetTargetEx().IsOnlyVisual() == true || GetTargetEx().GetAge() > 0)) SetTarget(null);
            if (ProcessTarget(scale, (s != null ? s.GetWpnData() : null), mpCraft.pFPO, mpCraft.Speed) == false)
            {
                if (pCurrentMissile != null) pCurrentMissile.ProcessTarget(null, 0);
            }
            else
            {
                float dz = .0f;
                Vector3 Dif = GetTargetOrgEx() - mpCraft.pFPO.Org;
                if (s != null)
                {
                    if (GetTargetDistEx() > .0f) Dif /= GetTargetDistEx();
                    dz = Vector3.Dot(Dif, mpCraft.pFPO.Dir);
                    if (pCurrentMissile != null)
                    {
                        bool cngt = !mpCraft.IsPlayedByHuman() && GetTargetEx().GetMissileCount() >= GetPowerFor(pCurrentMissile.GetWpnData().UnitDataIndex, mpCraft.GetTypeIndex());
                        pCurrentMissile.ProcessTarget(cngt ? null : GetTargetEx(), dz);
                    }
                    if (dz > 0)
                    {
                        TargetDifY = Mathf.Atan2(Vector3.Dot(Dif, mpCraft.pFPO.Right), dz);
                        TargetDifX = Mathf.Asin(Vector3.Dot(Dif, mpCraft.pFPO.Up));
                        mTargetAim = s.GetAimC(dz, GetTargetDistEx());
                        dz *= Storm.Math.GetOneOverC(GetTargetDistEx(), s.GetWpnData().Range);
                        if (mTrigger) dz += 1f;
                    }
                }
                SetThreat(mpCraft.GetHandle(), dz);
            }
        }
        { // апдейт оружия
            float escale = 0;
            float[] ReloadDelta = new float[] { .0f, .0f, .0f };
            int[] nWeapons = new int[] { 0, 0, 0 };
            int i;
            float d;
            BaseWeaponSlot s;
            // определяем потребность в энергии
            for (s = mpFirstWS, d = 0; s != null; s = s.GetNext()) d += s.GetEnergyUsage();
            d *= scale;
            if (mpCraft.BatteryCharge > 0)
            {
                if (mpCraft.BatteryCharge < d)
                {
                    escale = mpCraft.BatteryCharge / d;
                    mpCraft.BatteryCharge = 0;
                }
                else
                {
                    escale = 1;
                    mpCraft.BatteryCharge -= d;
                }
            }
            // перезаряжаем и заодно определяем максимальное время перезарядки 
            for (s = mpFirstWS; s != null; s = s.GetNext())
            {
                float t = s.Reload(scale, escale);
                if (ReloadDelta[s.GetGroup()] < t) ReloadDelta[s.GetGroup()] = t;
                nWeapons[s.GetGroup()]++;
                BaseWeaponSlotMissile msl = (BaseWeaponSlotMissile)s.GetInterface(BaseWeaponSlotMissile.ID);

                // hacka
                if (msl != null && pCurrentMissile != null && pCurrentMissile != msl && mWeaponGroup == 2)
                    msl.setLockTimer(pCurrentMissile.getLockTimer());
                else if (msl != null && mWeaponGroup != 2)
                    msl.setLockTimer(0, false);
            }

            for (i = 0; i < 3; i++)
            {
                if (nWeapons[i] != 0) ReloadDelta[i] /= nWeapons[i];
                if (ReloadDelta[i] > gsMaxRealodTime) ReloadDelta[i] = gsMaxRealodTime;
            }
            // стрельба  
            d = 1f / Mathf.Clamp(GetTargetDistEx(), 200f, 1000f);
            if (mTrigger == true && mpCurrentWS[mWeaponGroup] != null)
            {
                if (mpCurrentWS[mWeaponGroup].GetWpnData().IsChained == true)
                { // стрельба по очереди
                    if (mLastFireTime[mWeaponGroup] + ReloadDelta[mWeaponGroup] <= mpCraft.rScene.GetTime())
                    {
                        if (mpCurrentWS[mWeaponGroup].Fire(GetTargetEx(), d, TargetDifX, TargetDifY))
                        {
                            SwitchToNextSlot();
                            mLastFireTime[mWeaponGroup] = mpCraft.rScene.GetTime();
                        }
                    }
                }
                else
                { // обычный режим
                    for (s = mpFirstWS; s != null; s = s.GetNext())
                    {
                        if (s.GetGroup() != mWeaponGroup) continue;
                        s.Fire(GetTargetEx(), d, TargetDifX, TargetDifY);
                    }
                }
            }
        }
        myPreviousWeaponStatus = mWeaponGroup;
        iContact trg = GetTargetEx();
        if (!mpCraft.IsPlayedByHuman() && trg != null)
        {
            float stime = mpCraft.rScene.GetTime();
            if (stime > myPrevTraceLineTime + 3)
            {
                myPrevTraceLineTime = stime;
                TraceTarget(trg);
            }
        }
    }
    public void correctWeapons()
    {
        if (mpCurrentWS[0] != null && mpCurrentWS[1] != null && mpCurrentWS[0].GetWpnData() == mpCurrentWS[1].GetWpnData())
            for (BaseWeaponSlot ws = mpFirstWS; ws != null; ws = ws.GetNext())
                if (ws.GetGroup() == 1)
                    ws.SetGroup(0);
    }
    void TraceTarget(iContact cnt)
    {
        if (mpCraft.pFPO != null)
        {
            Vector3 dir = cnt.GetOrg() - mpCraft.pFPO.Org;
            float d = dir.magnitude;
            dir /= d;
            Vector3 org = mpCraft.pFPO.Org + mpCraft.pFPO.Dir * mpCraft.GetRadius();
            TraceInfo res = mpCraft.rScene.TraceLine(new Geometry.Line(org, dir, 500f), cnt.GetTopContact().GetHMember(), (int)CollisionDefines.COLLF_ALL);
            myTraceFail = res.count != 0;
        }
    }

    // own

    bool myTraceFail;
    float myPrevTraceLineTime;
    BaseCraft mpCraft;
    float mTargetAim;
    bool mTrigger;
    int mWeaponGroup;
    int myPreviousWeaponStatus;
    BaseWeaponSlot mpFirstWS;
    BaseWeaponSlot[] mpCurrentWS = new BaseWeaponSlot[3];
    float[] mLastFireTime = new float[3];
    void SwitchToNextSlot()
    {
        BaseWeaponSlot r = mpCurrentWS[mWeaponGroup];
        do
        {
            r = (r != null ? r.GetNext() : mpFirstWS);
            if (r == null || r.GetGroup() != mWeaponGroup) continue;
            if (r.GetStatus() != BaseWeaponSlot.WPN_DISABLED) { mpCurrentWS[mWeaponGroup] = r; return; }
        } while (r != mpCurrentWS[mWeaponGroup]);
    }
    BaseWeaponSlotMissile getCurMissile()
    {
        return (BaseWeaponSlotMissile)(mpCurrentWS[2] != null ? mpCurrentWS[2].GetInterface(BaseWeaponSlotMissile.ID) : null);
    }

    internal void Dispose()
    {
        return;
    }
}