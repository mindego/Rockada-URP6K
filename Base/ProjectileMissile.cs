using UnityEngine;
using static iSensorsDefines;
using DWORD = System.UInt32;

public partial class ProjectileMissile : IBaseItem
{
    private IBaseItem myBaseItem;

    public uint GetHandle()
    {
        return myBaseItem.GetHandle();
    }

    public BaseScene getScene()
    {
        return myBaseItem.getScene();
    }

    public override bool IsLocal()
    {
        return myBaseItem.IsLocal();
    }

    public override bool IsRemote()
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

public partial class ProjectileMissile : IBaseUnit
{
    private BaseUnit myBaseUnit;

    public void AddThreatC(float t)
    {
        myBaseUnit.AddThreatC(t);
    }

    public iContact ChangeSideTo(int SideCode)
    {
        return null;
    }

    public void DecMissileCount()
    {
        myBaseUnit.DecMissileCount();
    }

    public override void Dispose()
    {
        ZeroTarget();
        if (mpHash != null) rScene.DeleteHM(mpHash);
        if (mpFPO != null) mpFPO.Release();
        rScene.SubUnit(this);
        myBaseItem.Release();
        base.Dispose();
    }

    public void EndPrediction()
    {
        myBaseUnit.EndPrediction();
    }

    public Vector3 GetDir()
    {
        return pos.Dir;
    }

    public BaseSubobj GetFirstSubContact()
    {
        return myBaseUnit.GetFirstSubContact();
    }

    public FPO GetFpo()
    {
        return mpFPO;
    }

    public float GetHeadingAngle()
    {
        return Mathf.Atan2(pos.Dir.x, pos.Dir.z);
    }

    public HMember GetHMember()
    {
        return mpHash;
    }

    public float GetImportanceFor(iContact c)
    {
        return myBaseUnit.GetImportanceFor(c);
    }

    public BaseSubobj GetLastSubContact()
    {
        return myBaseUnit.GetLastSubContact();
    }

    public ILog GetLog()
    {
        return myBaseUnit.GetLog();
    }

    public float GetMaxCornerSpeed()
    {
        return sCornerSpeed;
    }

    public float GetMaxSpeed()
    {
        return GetMissileData().MaxSpeed;
    }

    public float getMinRadius()
    {
        return GetRadius();
    }

    public int GetMissileCount()
    {
        return myBaseUnit.GetMissileCount();
    }

    public string GetName()
    {
        return "";
    }

    public Vector3 GetOrg()
    {
        return pos.Org;
    }

    public float GetPitchAngle()
    {
        return Mathf.Asin(pos.Dir.y);
    }

    public float GetPowerFor(iContact c)
    {
        return myBaseUnit.GetPowerFor(c);
    }

    public int GetProtoType()
    {
        return myBaseUnit.GetProtoType();
    }

    public float GetRadius()
    {
        return 5f;
    }

    public Vector3 GetRight()
    {
        return pos.Right;
    }

    public float GetRollAngle()
    {
        return 0f;
    }

    public iSensors GetSensors()
    {
        return myBaseUnit.GetSensors();
    }

    public float GetSensorsRange()
    {
        return myBaseUnit.GetSensorsRange();
    }

    public float GetSensorsVisibility()
    {
        return myBaseUnit.GetSensorsVisibility();
    }

    public int GetSideCode()
    {
        return myBaseUnit.GetSideCode();
    }

    public override Vector3 GetSpeed()
    {
        return speed;
    }

    public int GetState()
    {
        return CS_IN_GAME;
    }

    public float GetThreatC()
    {
        return myBaseUnit.GetThreatC();
    }

    public float GetThreatF()
    {
        return myBaseUnit.GetThreatF();
    }

    public uint GetThreatHandle()
    {
        return myBaseUnit.GetThreatHandle();
    }

    public int GetTypeIndex()
    {
        return myBaseUnit.GetTypeIndex();
    }

    public string GetTypeName()
    {
        return myBaseUnit.GetTypeName();
    }

    public int getUnitType()
    {
        return myBaseUnit.getUnitType();
    }

    public Vector3 GetUp()
    {
        return pos.Up;
    }

    public bool HasSubContacts()
    {
        return myBaseUnit.HasSubContacts();
    }

    public void IncMissileCount()
    {
        myBaseUnit.IncMissileCount();
    }

    public bool IsInSF()
    {
        return myBaseUnit.IsInSF();
    }

    public bool IsPlayedByHuman()
    {
        return false;
    }

    public bool IsSurfaced()
    {
        return false;
    }

    public void MakePrediction(float scale)
    {
        myBaseUnit.MakePrediction(scale);
    }

    public object queryObject(uint id, int num)
    {
        return myBaseUnit.queryObject(id, num);
    }

    public void SetLog(ILog log)
    {
        myBaseUnit.SetLog(log);
    }

    public void SetName(string pName, bool ShouldLocalize)
    {

    }

    public void SetThreat(uint thr, float f)
    {
        myBaseUnit.SetThreat(thr, f);
    }

    public void StartPrediction()
    {
        myBaseUnit.StartPrediction();
    }

    private void BaseUnitInit()
    {
        myBaseUnit = new BaseUnit();
    }
}

public partial class ProjectileMissile : Projectile
{
    new public const uint ID = 0x1F912CF9;

    protected const float sNearRange = 2000f;
    protected const float sGlideC = .4f;
    protected const float sCornerSpeed = 0.785f;  // 45grd
    protected const float sMinProximityTime = 1f;
    protected const float sMaxProximityTime = 4f;

    public ProjectileMissile(BaseScene s, DWORD h) : base(s)
    {
        BaseItemInit(s, h);
        BaseUnitInit();

        mpFPO = null;
        mpHash = null;
        mTargetDir = Vector3.zero;
        mIsDead = false;
    }

    public void HostPrepare(WPN_DATA dt, iContact _owner, iContact tgt, MATRIX Pos)
    {
        base.Init(dt, _owner, Pos.Org, Pos.Dir, false);
        speed = owner.Ptr().GetSpeed();
        speedf = speed.magnitude;
        BasePrepare(tgt, (uint)owner.Ptr().GetSideCode());
    }

    public void BasePrepare(iContact tgt, DWORD SideCode)
    {
        myBaseUnit.UnitDataIndex = wpndata.UnitDataIndex;
        mpFPO = rScene.CreateFPO(GetWeaponData().MeshName);
        if (mpFPO == null)
            throw new System.Exception(string.Format("Missile \"{0}\" cannot find it's object!", GetMissileData().FullName));
        SetLink(mpFPO);
        mpHash = rScene.ConstructHM(mpFPO);
        if (GetWeaponData().ParticleName != 0xFFFFFFFF && rScene.GetSceneVisualizer() != null)
            visual = new ProjectileVisualOnlyParticle(rScene.GetSceneVisualizer(), this, GetWeaponData(), mpFPO.MinZ());
        mpTarget.setPtr(tgt);
        if (mpTarget.Ptr() != null) mpTarget.Ptr().IncMissileCount();
        myBaseUnit.pContact = rScene.AddUnit(this, (int)SideCode);
        setActiveMissileEnabled(true);
    }

    private void SetLink(RO r)
    {
        r.Link = (iBaseInterface)(iBaseVictim)this;
        for (r = r.SubObjects; r != null; r = r.Next)
            SetLink(r);
    }
    protected void ZeroTarget(/*cstr reason*/)
    {
        /*    if (reason)
        rScene.Message("Zero Target:%s",reason);*/
        if (mpTarget.Ptr() != null)
        {
            mpTarget.Ptr().DecMissileCount();
            mpTarget.setPtr(null);
        }
    }
    protected void MakeAim(ref Vector3 Dif, ref float dist)
    {
        // набор высоты для облета препятствий
        if (dist > sNearRange)
        {
            Dif.y += sNearRange * sGlideC;
        }
        else
        {
            // рассчитываем упреждение
            if (dist == 0) return;
            Vector3 dir = Dif / dist;
            Vector3 tr = mpTarget.Ptr().GetSpeed();
            Vector3 tmp = Vector3.Cross(dir, tr);
            float f = tmp.magnitude;
            // определяем косинус угла наведения
            if (f > .1f)
            {
                tmp /= f; tr = Vector3.Cross(tmp, dir);
                f = Mathf.Pow(Vector3.Dot(mpTarget.Ptr().GetSpeed(), tr) / GetMissileData().MaxSpeed, 2);
                if (f >= 1) return;
            }
            else
            {
                f = 0;
            }
            // определяем скорость продольного сближения снаряди и цели
            f = Mathf.Sqrt(1f - f) * GetMissileData().MaxSpeed - (Vector3.Dot(mpTarget.Ptr().GetSpeed(), dir));
            // добавляем скорость_цели*время_полета_до_цели
            if (f > .0f) Dif += mpTarget.Ptr().GetSpeed() * (dist / f);
        }
    }
    protected void setActiveMissileEnabled(bool flag)
    {
        if (owner.Ptr() != null)
        {
            BaseCraft craft = (BaseCraft)owner.Ptr().GetInterface(BaseCraft.ID);
            if (craft != null)
                craft.setActiveMissile(flag ? this : null);
        }
    }
    protected void setTimer(float time)
    {
        timer = time;
    }

    public WPN_DATA_MISSILE GetMissileData() { return (WPN_DATA_MISSILE)wpndata; }

    public override bool Move(float scale)
    {
        if (GetMissileData().LockAngle > .0f) //Если это ракета - подруливаем
        {
            // проверка видимости цели
            if (mpTarget.Ptr() != null)
            {
                if (mpTarget.Ptr().GetState() == CS_DEAD || mpTarget.Ptr().GetAge() > .0f || mpTarget.Ptr().IsOnlyVisual())
                {
                    //rScene.Message("State %d Age %f OnlyVisual %d",mpTarget->GetState(),mpTarget->GetAge(),mpTarget->IsOnlyVisual());
                    ZeroTarget(/*"Target DEAD or  Target Age > 0 or Target Only Visual"*/);
                }
            }
            // если в поле
            if (rScene.IsInSfg(pos.Org) == true)
                ZeroTarget(/*"InSfg"*/);
            // взрыв, если цель потеряна
            if (mpTarget.Ptr() == null && Remote == false && timer > GetMissileData().TriggerTime)
                setTimer(GetMissileData().TriggerTime);
            // наведение
            if (mpTarget.Ptr() != null && timer < GetMissileData().LifeTime - GetMissileData().TriggerTime)
            {
                Vector3 Dif;
                if (Storm.Math.NormaFAbs(mTargetDir) == 0)
                {
                    Dif = mpTarget.Ptr().GetOrg() - pos.Org;
                    float dist = Dif.magnitude;
                    mpTarget.Ptr().SetThreat(GetHandle(), Mathf.Clamp(2 - dist * 0.001f, 0.1f, 2));
                    MakeAim(ref Dif, ref dist);
                    dist = Dif.magnitude;
                    if (dist != 0) Dif /= dist;

                    float acc_time = (GetMissileData().MaxSpeed - speedf) / wpndata.GetAccel();
                    float acc_len = speedf * acc_time + wpndata.GetAccel() * Mathf.Pow(acc_time, 2) * 0.5f;
                    if (acc_len > dist)
                    {
                        Vector2 solve;
                        if (Storm.Math.solveSquare(wpndata.GetAccel() * 0.5f, speedf, -dist, out solve))
                            acc_time = solve[solve.y > 0 ? 1 : 0];
                    }
                    else
                        acc_time += (dist - acc_len) / GetMissileData().MaxSpeed;

                    float tlim = Mathf.Clamp(
                        GetMissileData().ProximityTime * (2f - mpTarget.Ptr().GetSensorsVisibility()),
                        sMinProximityTime, sMaxProximityTime);

                    if (acc_time < tlim)
                    {
                        mTargetDir = Dif;
                        setTimer(acc_time);
                    }
                }
                else
                {
                    Dif = mTargetDir;
                    mpTarget.Ptr().SetThreat(GetHandle(), 2.0001f);
                }
                // turn to target
                float max_Angle = sCornerSpeed * scale;
                pos.TurnRightPrec(Mathf.Clamp(Mathf.Asin(Vector3.Dot(pos.Right, Dif)), -max_Angle, max_Angle));
                pos.TurnUpPrec(Mathf.Clamp(Mathf.Asin(Vector3.Dot(pos.Up, Dif)), -max_Angle, max_Angle));
            }
        }
        // object movement
        float p = wpndata.GetSpeed() - speedf;
        float l = wpndata.GetAccel() * scale;
        speedf += (p < l ? p : l);
        speed = pos.Dir * speedf;
        bool ret = ProcessTrace(scale) == null;
        if (ret)
        {
            // перемещение
            pos.Org += speed * scale;
            if (mpFPO != null) { mpFPO.Set(pos); rScene.UpdateHM(mpHash); }
            ret = ProcessTimer(scale);
        }
        if (ret == false)
            setActiveMissileEnabled(false);
        return ret;
    }

    // ProjectileMissile - от iBaseInterface
    public override object GetInterface(DWORD id)
    {
        switch (id)
        {
            case ID: return this;
            case BaseItem.ID: return (IBaseItem)this;
            case BaseUnit.ID: return (IBaseUnit)this;
            case iSensors.ID: return myBaseUnit.pContact.GetSensors();
            case iContact.ID: return myBaseUnit.pContact;
            case iBaseVictim.ID: return (iBaseVictim)this;
            default: return base.GetInterface(id);
        }
    }

    public override TraceResult ProcessTrace(float scale)
    {
        if (Remote) return null;
        Vector3 trace_dir = speed / speedf;
        TraceInfo res = rScene.TraceLine(new Geometry.Line(pos.Org, trace_dir, speedf * scale), mpHash, (int)CollisionDefines.COLLF_ALL);
        for (int i = 0; i < res.count; i++)
        {
            pos.Org = res.results[i].org;
            if (res.results[i].coll_object != null && res.results[i].coll_object.Link != null)
            {
                iBaseInterface intr = (iBaseInterface)res.results[i].coll_object.Link;
                //iContact cnt = (iContact)intr.GetInterface(iContact.ID);
                iContact cnt = null;
                try
                {
                    cnt = (iContact)intr.GetInterface(iContact.ID);
                } catch
                {
                    Debug.LogFormat("Failed to convert {0}/{1} into {2}", res.results[i].coll_object.Link.GetType().ToString(),cnt==null? "null":cnt.GetType().ToString(),"iContact");
                }
                //if (cnt != null)Debug.Log("cnt.GetTopContact()", cnt != null ? cnt.GetTopContact():"EMPTY?");
                if (cnt != null && cnt.GetTopContact() == owner.Ptr()) continue;
            }
            Vector3 org, dir, spd;
            EXPLOSION_INFO info = GetExplInfo(res.results[i], out org, out dir, out spd);
            if (info != null)
            {
                MakeDamage(res.results[i], info);
                MakeExplosion(info.explosion, org, dir, spd, (DWORD)(res.results[i].coll_object != null ? ((FPO)res.results[i].coll_object).Top().GetHashCode() : 0)); //TODO возможно, здесь вместо хэша объекта нужен handle FPO
                                                                                                                                                                       //ProjectileMissileDiePacket pkt(GetHandle(), info->explosion->Name, org, dir, spd);
                                                                                                                                                                       //rScene.SendItemData(&pkt);
                return res.results[i];
            }
        }
        return null;
    }

    // own
    protected readonly TContact mpTarget = new TContact();
    protected FPO mpFPO;
    protected HMember mpHash;
    protected Vector3 mTargetDir;
    protected bool mIsDead;
}

/// <summary>
/// ProjectileMissile - базовый класс для синхронизируемого оружия
/// </summary>
public partial class ProjectileMissile : iBaseVictim
{
    // от BaseVictim
    public float GetCondition()
    {
        return 1f;
    }

    public float GetLife()
    {
        return 1f;
    }

    public float GetDamage()
    {
        return .0f;
    }

    public float GetTotalLife()
    {
        return 1f;
    }

    public void AddDamage(DWORD GadHandle, DWORD WeaponCode, float d)
    {
        if (mIsDead == true || timer <= .0f) return;
        if (Remote == true)
            //{
            //    ProjectileMissileKilledPacket Pkt(GetHandle(), GadHandle, WeaponCode, d);
            //    rScene.SendItemData(&Pkt);
            //    return;
            //}
            if (rScene.GetDamage(GetHandle(), GadHandle, WeaponCode, d) == 0 && WeaponCode != iBaseVictim.WeaponCodeUltimateDeath) return;
        setTimer(0);
        rScene.OnAddDamage(GetHandle(), GadHandle, WeaponCode, d, true);
    }

    public void AddRadiusDamage(DWORD GadHandle, DWORD WeaponCode, Vector3 Org, float Xr, float Xd)
    {
        if (mIsDead == true || timer <= .0f || GetMissileData().LockAngle == 0) return;
        //if (Remote == true)
        //{
        //    ProjectileMissileKilledPacket Pkt(GetHandle(), GadHandle, WeaponCode, Xd);
        //    rScene.SendItemData(&Pkt);
        //    return;
        //}
        if (rScene.GetDamage(GetHandle(), GadHandle, WeaponCode, Xd) == 0 && WeaponCode != iBaseVictim.WeaponCodeUltimateDeath) return;
        setTimer(0);
        rScene.OnAddDamage(GetHandle(), GadHandle, WeaponCode, Xd, true);
    }
}
