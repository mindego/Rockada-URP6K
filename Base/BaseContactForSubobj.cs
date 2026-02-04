using UnityEngine;
using DWORD = System.UInt32;
using static iSensorsDefines;
using UnityEngine.Assertions;

public class BaseContactForSubobj : iContact
{
    // свое
    private BaseContact mrOwner;          // owner
    int mRefCounter;      // reference counter
    Vector3 mOrg;             // last seen org
    float mLastRecalcTime;  // time of last Org recalc
    BaseSubobj mpSubobj;
    private bool IsAlive()
    {
        return (mpSubobj != null && mpSubobj.GetFpo() != null);
    }
    public BaseContactForSubobj(BaseContact c, BaseSubobj s)
    {
        mrOwner = c;
        mRefCounter = 0;
        mpSubobj = s;
        mLastRecalcTime = -1000f;
    }
    ~BaseContactForSubobj()
    {
        Asserts.AssertBp(mRefCounter == 0);
    }
    public void Zero() { mpSubobj = null; }
    public bool Update() { return (mRefCounter > 0); }
    public BaseSubobj GetSubobj() { return mpSubobj; }

    public DWORD GetHandle()
    {
        return (mpSubobj != null ? mpSubobj.GetHandle() : Constants.THANDLE_INVALID);
    }
    public void AddThreatC(float f)
    {
        mrOwner.AddThreatC(f);
    }

    public iContact ChangeSideTo(int SideCode)
    {
        throw new System.NotImplementedException();
    }

    public void DecMissileCount()
    {
        mrOwner.DecMissileCount();
    }

    public void EndPrediction() { }

    public float GetAge()
    {
        throw new System.NotImplementedException();
    }

    public float GetCondition()
    {
        return (IsAlive() ? mpSubobj.GetLife() / mpSubobj.GetData().Armor : -1.0f); ;
    }

    public Vector3 GetDir()
    {
        throw new System.NotImplementedException();
    }

    public FPO GetFpo()
    {
        throw new System.NotImplementedException();
    }

    public float GetHeadingAngle()
    {
        throw new System.NotImplementedException();
    }

    public HMember GetHMember()
    {
        throw new System.NotImplementedException();
    }

    public float GetImportanceFor(iContact i)
    {
        throw new System.NotImplementedException();
    }

    public object GetInterface(uint id)
    {
        return (mpSubobj != null ? mpSubobj.GetInterface(id) : null);
    }

    public T GetInterface<T>() where T : iBaseInterface
    {
        throw new System.NotImplementedException();
    }

    public int getLifeState()
    {
        return (IsAlive() ? mpSubobj.GetState() : -1);
    }

    public ILog GetLog()
    {
        throw new System.NotImplementedException();
    }

    public float GetMaxCornerSpeed()
    {
        throw new System.NotImplementedException();
    }

    public float GetMaxSpeed()
    {
        throw new System.NotImplementedException();
    }

    public float getMinRadius()
    {
        throw new System.NotImplementedException();
    }

    public int GetMissileCount()
    {
        throw new System.NotImplementedException();
    }

    public string GetName()
    {
        const string spNoName = "";
        return spNoName;
    }

    public iContact GetNextSubContact(iContact sc = null, uint Name = uint.MaxValue, bool only_detached = true)
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetOrg()
    {
        throw new System.NotImplementedException();
    }

    public float GetPitchAngle()
    {
        throw new System.NotImplementedException();
    }

    public float GetPowerFor(iContact i)
    {
        throw new System.NotImplementedException();
    }

    public iContact GetPrevSubContact(iContact sc = null, uint Name = uint.MaxValue, bool only_detached = true)
    {
        throw new System.NotImplementedException();
    }

    public int GetProtoType()
    {
        throw new System.NotImplementedException();
    }

    public float GetRadius()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetRight()
    {
        throw new System.NotImplementedException();
    }

    public float GetRollAngle()
    {
        throw new System.NotImplementedException();
    }

    public iSensors GetSensors()
    {
        return mrOwner.GetSensors();
    }

    public float GetSensorsRange()
    {
        throw new System.NotImplementedException();
    }

    public float GetSensorsVisibility()
    {
        throw new System.NotImplementedException();
    }

    public int GetSideCode()
    {
        return mrOwner.GetSideCode();
    }

    public Vector3 GetSpeed()
    {
        return mrOwner.GetSpeed();
    }

    public int GetState()
    {
        return (IsAlive() ? CS_IN_GAME : CS_DEAD);
    }

    public iContact GetThreat()
    {
        return mrOwner.GetThreat(); 
    }

    public float GetThreatC()
    {
        return mrOwner.GetThreatC();
    }

    public float GetThreatF()
    {
        return mrOwner.GetThreatF();
    }

    public uint GetThreatHandle()
    {
        throw new System.NotImplementedException();
    }

    public iContact GetTopContact()
    {
        return mrOwner;
    }

    public int GetTypeIndex()
    {
        throw new System.NotImplementedException();
    }

    public string GetTypeName()
    {
        throw new System.NotImplementedException();
    }

    public int getUnitType()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetUp()
    {
        throw new System.NotImplementedException();
    }

    public bool HasSubContacts()
    {
        throw new System.NotImplementedException();
    }

    public void IncMissileCount()
    {
        mrOwner.IncMissileCount();
    }

    public bool IsInSF()
    {
        throw new System.NotImplementedException();
    }

    public bool IsOnlyVisual()
    {
        throw new System.NotImplementedException();
    }

    public bool IsPlayedByHuman()
    {
        return false;
    }

    public bool IsSurfaced()
    {
        throw new System.NotImplementedException();
    }

    public void MakePrediction(float scale) { }

    public object queryObject(uint id, int num = 0)
    {
        return (mpSubobj != null ? mpSubobj.queryObject(id, num) : null);
    }

    public object queryObject<T>(int num = 0)
    {
        throw new System.NotImplementedException();
    }

    public void SetLog(ILog i)
    {
        throw new System.NotImplementedException();
    }

    public void SetName(string pName, bool ShouldLocalize)
    {
        throw new System.NotImplementedException();
    }

    public void SetThreat(uint thr, float f)
    {
        throw new System.NotImplementedException();
    }

    public void StartPrediction() { }

    public int Release()
    {
        mRefCounter--;
        Assert.IsTrue(mRefCounter >= 0);
        return mRefCounter;

    }

    public void AddRef()
    {
        mRefCounter++; 
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



