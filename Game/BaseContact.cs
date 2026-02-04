using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using DWORD = System.UInt32;

public partial class BaseContact : TLIST_ELEM<BaseContact>,IDisposable
{
    private BaseContact next, prev;
    public BaseContact Next()
    {
        return next;
    }

    public BaseContact Prev()
    {
        return prev;
    }

    public void SetNext(BaseContact t)
    {
        next = t;
    }

    public void SetPrev(BaseContact t)
    {
        prev = t;
    }

}
public partial class BaseContact : iContact
{
    public const int BCV_HIDDEN = 0;
    public const int BCV_VISUAL_ONLY = 1;
    public const int BCV_ON_RADAR = 2;
    public const int BCV_ALWAYS = 3;

    public const float VIS_DISTANCE = 1500f;
    const float CheckVisibilityDistance = VIS_DISTANCE * VIS_DISTANCE;

    // свое
    private int mRefCounter;      // reference counter
    private IBaseUnit mpUnit;           // Unit handle
    private float mLastObserved;    // last seen time (compare with scene time and SIDE_ACQUISITION_PERIOD)
    private DWORD mVisibility;      // see defines
    private Vector3 mOrg;             // last seen org
    private Vector3 mpOrg;            // optimization
    private Vector3 mSpeed;           // last speed
    private Vector3 mpSpeed;          // optimization
    LinkedList<BaseContactForSubobj> mSubContactsList = new LinkedList<BaseContactForSubobj>();
    public BaseSensors mrSensors;        // side
    public float Reserved;         // для внутренних вычислений
    public BaseContact(BaseSensors s, IBaseUnit u)
    {
        mrSensors = s;
        mRefCounter = 0;
        mpUnit = u;
        mLastObserved = .0f;
        mVisibility = BCV_HIDDEN;
        mOrg = Vector3.zero;
        mpOrg = Vector3.zero;
        mSpeed = Vector3.zero;
        mpSpeed = Vector3.zero;
        Reserved = 0;

        if (mpUnit != null)
        {
            mpOrg = mpUnit.GetOrg();
            mpSpeed = mpUnit.GetSpeed();
        }
    }
    public void Dispose()
    {
        Assert.IsTrue(mRefCounter == 0);
    }
    ~BaseContact() { Dispose(); }
    public bool Vis(float dt = .0f)
    {
        return ((mpUnit != null) && ((mVisibility == BCV_ALWAYS) || (mVisibility != BCV_HIDDEN && (mrSensors.mLastUpdateTime - mLastObserved <= dt))));
    }

    public bool Update()
    {
        // обновляем SubContacts и удалаяем ненужные
        LinkedListNode<BaseContactForSubobj> pCurr, pNext;
        for (pCurr = mSubContactsList.First; pCurr != null; pCurr = pNext)
        {
            pNext = pCurr.Next;
            if (pCurr.Value.Update() == true) continue;
            mSubContactsList.Remove(pCurr);
        }
        // если еще живы
        if (mpUnit != null)
        {
            mpOrg = mpUnit.GetOrg();
            mpSpeed = mpUnit.GetSpeed();
            if (IsVisible()) { mOrg = mpOrg; mSpeed = mpSpeed; }
            return true;
        }
        // возвращаем невозможность удалить
        return (mRefCounter > 0 || mSubContactsList.Count != 0);
    }
    public void SubUnit(IBaseUnit u)
    {
        if (mpUnit != u) return;
        if (Vis()) { mOrg = mpOrg; mSpeed = mpSpeed; }
        mpUnit = null;
        //for (LinkedListNode<BaseContactForSubobj> pSub = mSubContactsList.First; pSub != null; pSub = pSub.Next)
        //{
        //    pSub.Value.Zero();
        //}

        //if (mSubContactsList == null) return; //Так быть не должно.
        foreach (BaseContactForSubobj pSub in mSubContactsList)
        {
            pSub.Zero();
        }

    }
    public void SetVisible(DWORD vis) { mVisibility = vis; mLastObserved = mrSensors.mLastUpdateTime; }
    bool IsVisible() { return (mVisibility != 0); }
    public bool CheckVisibility(BaseContact m)
    {
        if (mpUnit == null || mVisibility == BCV_ALWAYS) return true;
        if (m.mpUnit == null) return false;
        Vector3 Delta = mpUnit.GetOrg() - m.mpUnit.GetOrg();
        float r = Delta.sqrMagnitude;
        float f = Reserved * m.Reserved;
        if (f < r)
        {
            if (r > CheckVisibilityDistance) return false;
            // проверяем видимость глазами
            r = Mathf.Sqrt(r);
            float z = mpUnit.GetRadius() + 10f;
            if (r > z)
            {
                Delta /= r;
                TraceInfo res = mrSensors.mrScene.TraceLine(new Geometry.Line(m.mpUnit.GetOrg(), Delta, r - z), m.mpUnit.GetHMember(), (int)CollisionDefines.COLLF_ALL);
                if (res.count != 0) return false;
            }
            // виден только глазами
            SetVisible(BCV_VISUAL_ONLY);
            return false;
        }
        // виден на радаре
        SetVisible((mpUnit.GetInterface(0x00DDAC72) == null) ? (uint)BCV_ON_RADAR : (uint)BCV_ALWAYS); //0x00DDAC72 - BaseStatic.ID
        return true;

    }
    public IBaseUnit GetUnit() { return mpUnit; }


    public void AddThreatC(float f)
    {
        if (mpUnit != null) mpUnit.AddThreatC(f);
    }

    public iContact ChangeSideTo(int SideCode)
    {
        return (mpUnit != null ? mpUnit.ChangeSideTo(SideCode) : null);
    }

    public void DecMissileCount()
    {
        if (mpUnit != null) mpUnit.DecMissileCount();
    }

    public void EndPrediction()
    {
        if (mpUnit != null) mpUnit.EndPrediction();
    }

    public float GetAge()
    {
        return (mVisibility == BCV_ALWAYS ? .0f : mrSensors.mLastUpdateTime - mLastObserved);
    }

    /// <summary>
    /// unit's condition
    /// </summary>
    /// <returns>(0 - dead, 1 - unscratched)</returns>
    public float GetCondition()
    {
        return (mpUnit != null ? mpUnit.GetCondition() : .0f);
    }

    public Vector3 GetDir()
    {
        return (Vis() ? mpUnit.GetDir() : Vector3.forward);
    }

    public FPO GetFpo()
    {
        return (mpUnit != null ? mpUnit.GetFpo() : null);
    }

    public uint GetHandle()
    {
        return (mpUnit != null ? mpUnit.GetHandle() : Constants.THANDLE_INVALID);

    }

    public float GetHeadingAngle()
    {
        return (Vis() ? mpUnit.GetHeadingAngle() : .0f);
    }

    public HMember GetHMember()
    {
        return (mpUnit != null ? mpUnit.GetHMember() : null);
    }

    public float GetImportanceFor(iContact c)
    {
        return (mpUnit != null ? mpUnit.GetImportanceFor(c) : .0f);
    }

    public object GetInterface(uint id)
    {
        //Debug.Log("My interface " + id);
        //Debug.Log("My unit " + mpUnit);
        return (mpUnit != null ? mpUnit.GetInterface(id) : null);
        //throw new NotImplementedException();
    }

    public T GetInterface<T>() where T : iBaseInterface
    {
        return default;
    }

    public int getLifeState()
    {
        return 0; // not supported
    }

    public ILog GetLog()
    {
        return (mpUnit != null ? mpUnit.GetLog() : null);
    }

    public float GetMaxCornerSpeed()
    {
        return (mpUnit != null ? mpUnit.GetMaxCornerSpeed() : .0f);
    }

    public float GetMaxSpeed()
    {
        return (mpUnit != null ? mpUnit.GetMaxSpeed() : .0f);
    }

    public float getMinRadius()
    {
        return (Vis() ? mpUnit.getMinRadius() : .0f);
    }

    public int GetMissileCount()
    {
        return (mpUnit != null ? mpUnit.GetMissileCount() : 0);
    }

    public string GetName()
    {
        string spNoName = "";
        return (mpUnit != null ? mpUnit.GetName() : spNoName);
    }

    private string GetVisibilityDescription(int id)
    {

        string[] arr = new string[] { "BCV_HIDDEN", "BCV_VISUAL_ONLY", "BCV_ON_RADAR", "BCV_ALWAYS" };

        if (id < 0 || id >= arr.Length) return "Unknown";
        return arr[id];
    }
    public override string ToString()
    {
        //return ((mpUnit != null) && ((mVisibility == BCV_ALWAYS) || (mVisibility != BCV_HIDDEN && (mrSensors.mLastUpdateTime - mLastObserved <= dt))));
        string res = this.GetType().ToString() + " mpUnit " + mpUnit;
        res += "\nmVisibility: " + mVisibility + " " + GetVisibilityDescription((int)mVisibility);
        res += "\nmrSensors.mLastUpdateTime: " + mrSensors.mLastUpdateTime;
        res += "\nmLastObserved: " + mLastObserved;
        return res;
    }
    //public iContact GetNextSubContact(iContact sc = null, uint Name = uint.MaxValue, bool only_detached = true)
    public iContact GetNextSubContact(iContact sc, uint Name, bool only_detached)
    {
        if (mpUnit == null) return null;
        BaseSubobj pCurr;
        // находим текущий подобъект
        if (sc != null)
        {
            pCurr = (BaseSubobj)sc.GetInterface(BaseSubobj.ID);
            if (pCurr == null) return this;
            pCurr = pCurr.Next();
        }
        else
        {
            pCurr = mpUnit.GetFirstSubContact();
        }
        // перебираем подобъекты, пока не встретим Detached
        for (; pCurr != null; pCurr = pCurr.Next())
        {
            if (pCurr.GetFpo() == null) continue;
            if (Name != 0xFFFFFFFF)
            {
                if (pCurr.GetData().Name != Name) continue;
            }
            else
            {
                if (only_detached && pCurr.GetData().GetFlag(SUBOBJ_DATA.SF_DETACHED) == 0) continue;
            }
            // ищем среди моих SubContacts указывающий на этот Subobj
            //BaseContactForSubobj pSub;
            //for (pSub = mSubContactsList.Head(); pSub != 0; pSub = pSub->Next())
            //{
            //    if (pSub->GetSubobj() == pCurr) return pSub;
            //}
            foreach (BaseContactForSubobj pSub in mSubContactsList)
            {
                if (pSub.GetSubobj() == pCurr) return pSub;
            }
            // не нашли - создаем новый
            return mSubContactsList.AddLast(new BaseContactForSubobj(this, pCurr)).Value;
        }
        // не нашли detached подобъектов - возвращаем себя
        //Debug.Log(string.Format(" Not found in {0} ({1}) subobj id {2} name {3} of {4} {5}",
        //    mpUnit.GetFpo().TextName + " " + mpUnit.GetHashCode().ToString("X8"),
        //    mpUnit,
        //    Name.ToString("X8"),
        //    Hasher.StringHsh(Name),
        //    mSubContactsList.Count,
        //    sc == null ? "null" : sc
        //    ));
        return this;
    }

    public Vector3 GetOrg()
    {
        //return (Vis() ? mpOrg : mOrg);
        return (Vis() ? mpUnit.GetOrg() : mOrg);
    }

    public float GetPitchAngle()
    {
        return (Vis() ? mpUnit.GetPitchAngle() : .0f);
    }

    public float GetPowerFor(iContact c)
    {
        return (mpUnit != null ? mpUnit.GetPowerFor(c) : .0f);
    }

    public iContact GetPrevSubContact(iContact sc = null, uint Name = uint.MaxValue, bool only_detached = true)
    {
        return null;
    }

    public int GetProtoType()
    {
        return (mpUnit != null ? mpUnit.GetProtoType() : -1);
    }

    public float GetRadius()
    {
        return (Vis() ? mpUnit.GetRadius() : .0f);
    }

    public Vector3 GetRight()
    {
        return (Vis() ? mpUnit.GetRight() : Vector3.right);
    }

    public float GetRollAngle()
    {
        return (Vis() ? mpUnit.GetRollAngle() : .0f);
    }

    public iSensors GetSensors()
    {
        return mrSensors;
    }

    public float GetSensorsRange()
    {
        return (mpUnit != null ? mpUnit.GetSensorsRange() : .0f);
    }

    public float GetSensorsVisibility()
    {
        return (mpUnit != null ? mpUnit.GetSensorsVisibility() : .0f);
    }

    public int GetSideCode()
    {
        return (mpUnit != null ? mpUnit.GetSideCode() : 0);
    }

    public Vector3 GetSpeed()
    {
        return (Vis() ? mpSpeed : mSpeed);
    }

    public int GetState()
    {
        return (mpUnit != null ? mpUnit.GetState() : iSensorsDefines.CS_DEAD);
    }

    public iContact GetThreat()
    {
        if (mpUnit == null) return null;
        return mrSensors.GetContact(mpUnit.GetThreatHandle());

    }

    public float GetThreatC()
    {
        return (mpUnit != null ? mpUnit.GetThreatC() : .0f);
    }

    public float GetThreatF()
    {
        return (mpUnit != null ? mpUnit.GetThreatF() : .0f); ;
    }

    public uint GetThreatHandle()
    {
        return (mpUnit != null ? mpUnit.GetThreatHandle() : 0);
    }

    public iContact GetTopContact()
    {
        return this;
    }

    public int GetTypeIndex()
    {
        return (mpUnit != null ? mpUnit.GetTypeIndex() : 0);
    }

    public string GetTypeName()
    {
        return (mpUnit != null ? mpUnit.GetTypeName() : "");
    }

    public int getUnitType()
    {
        return mpUnit != null ? mpUnit.getUnitType() : (int)iSensorsDefines.UT_GROUND;
    }

    public Vector3 GetUp()
    {
        return (Vis() ? mpUnit.GetUp() : Vector3.up);
    }

    /// <summary>
    /// has sub contacts?
    /// </summary>
    /// <returns></returns>
    public bool HasSubContacts() { return false; }

    public void IncMissileCount()
    {
        if (mpUnit != null) mpUnit.IncMissileCount();
    }

    public bool IsInSF()
    {
        return (Vis() ? mpUnit.IsInSF() : false);
    }

    public bool IsOnlyVisual()
    {
        return (mVisibility == BCV_VISUAL_ONLY);
    }

    public bool IsPlayedByHuman()
    {
        return (mpUnit != null ? mpUnit.IsPlayedByHuman() : false);
    }

    public bool IsSurfaced()
    {
        return (mpUnit != null ? mpUnit.IsSurfaced() : false);
    }

    public void MakePrediction(float scale)
    {
        if (mpUnit != null) mpUnit.MakePrediction(scale);
    }

    public object queryObject(uint id, int num = 0)
    {
        //return null;
        //Debug.Log(mpUnit);
        return (mpUnit != null ? mpUnit.queryObject(id, num) : 0);
    }

    public object queryObject<T>(int num = 0)
    {
        return null;
    }

    public void SetLog(ILog l)
    {
        if (mpUnit != null) mpUnit.SetLog(l);

    }

    public void SetName(string pName, bool ShouldLocalize)
    {
        if (mpUnit != null) mpUnit.SetName(pName, ShouldLocalize);
    }

    public void SetThreat(uint thr, float f)
    {
        if (mpUnit != null) mpUnit.SetThreat(thr, f);
    }

    public void StartPrediction()
    {
        if (mpUnit != null) mpUnit.StartPrediction();
    }

    public int Release()
    {
        mRefCounter--;
        Asserts.AssertBp(mRefCounter >= 0);
        //if (mRefCounter >= 0) Debug.Log("Current mRefCounter " + mRefCounter + " for " + this);
        return mRefCounter;
    }

    public void AddRef()
    {
        mRefCounter++;
    }
}



