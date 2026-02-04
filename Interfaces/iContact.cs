using UnityEngine;
using DWORD = System.UInt32;

public interface iContact : IRefMem,iBaseInterface, IQueryInterface
{
    public new const uint ID = 0x4BCA9BDC;

    /// <summary>
    /// internal part - for sychronization
    /// </summary>
    /// <returns></returns>
    public DWORD GetHandle();
    /// <summary>
    /// unit's state
    /// </summary>
    /// <returns>(0 - should be released)</returns>
    public int GetState();
    /// <summary>
    /// life state
    /// </summary>
    /// <returns></returns>
    public int getLifeState();
    /// <summary>
    /// unit's condition
    /// </summary>
    /// <returns>(0 - dead, 1 - unscratched)</returns>
    public float GetCondition();
    /// <summary>
    /// how many seconds ago was last observed
    /// </summary>
    /// <returns></returns>
    public float GetAge();
    /// <summary>
    /// true - was observed only visual
    /// </summary>
    /// <returns></returns>
    public bool IsOnlyVisual();
    /// <summary>
    /// Is in supressing field ?
    /// </summary>
    /// <returns>true - is in supressing field</returns>
    public bool IsInSF();
    /// <summary>
    /// where this unit was last seen
    /// </summary>
    /// <returns></returns>
    public Vector3 GetOrg();
    /// <summary>
    /// orientation in world (vectors)
    /// </summary>
    /// <returns></returns>
    public Vector3 GetDir();
    /// <summary>
    /// orientation in world (vectors)
    /// </summary>
    /// <returns></returns>
    public Vector3 GetUp();
    /// <summary>
    /// orientation in world (vectors)
    /// </summary>
    /// <returns></returns>
    public Vector3 GetRight();
    /// <summary>
    /// orientation in world (angles)
    /// </summary>
    /// <returns></returns>
    public float GetHeadingAngle();
    /// <summary>
    /// orientation in world (angles)
    /// </summary>
    /// <returns></returns>
    public float GetPitchAngle();
    /// <summary>
    /// orientation in world (angles)
    /// </summary>
    /// <returns></returns>
    public float GetRollAngle();
    /// <summary>
    /// physical radius
    /// </summary>
    /// <returns></returns>
    public float GetRadius();
    /// <summary>
    /// speed
    /// </summary>
    /// <returns></returns>
    public Vector3 GetSpeed();
    /// <summary>
    /// code of unit's side
    /// </summary>
    /// <returns></returns>
    public int GetSideCode();
    /// <summary>
    /// for target distriubution
    /// </summary>
    /// <returns></returns>
    public float GetThreatC();
    /// <summary>
    /// radar contact for most threating unit
    /// </summary>
    /// <returns></returns>
    public iContact GetThreat();  // radar contact for most threating unit
    /// <summary>
    /// relative threatness of this unit 
    /// </summary>
    /// <returns>(0 - clear, 1 - moderate threat, >1 - under fire)</returns>
    public float GetThreatF();
    /// <summary>
    /// to change unit's side. 
    /// </summary>
    /// <param name="SideCode"></param>
    /// <returns>new iContact if success, null otherwise</returns>
    public iContact ChangeSideTo(int SideCode);
    /// <summary>
    /// get assotiated name
    /// </summary>
    /// <returns></returns>
    public string GetName();  // get assotiated name
    /// <summary>
    /// set assotiated name, no more than CONTACT_NAME_LENGTH
    /// </summary>
    /// <param name="pName"></param>
    /// <param name="ShouldLocalize"></param>
    public void SetName(string pName, bool ShouldLocalize);
    // subcontacts
    /// <summary>
    /// returns root contact
    /// </summary>
    /// <returns></returns>
    public iContact GetTopContact();
    /// <summary>
    /// has sub contacts?
    /// </summary>
    /// <returns></returns>
    public bool HasSubContacts();
    /// <summary>
    /// access to different parts of unit
    /// </summary>
    /// <param name="sc"></param>
    /// <param name="Name"></param>
    /// <param name="only_detached"></param>
    /// <returns></returns>
    public iContact GetNextSubContact(iContact sc = null, DWORD Name = 0xFFFFFFFF, bool only_detached = true);
    /// <summary>
    /// access to different parts of unit
    /// </summary>
    /// <returns></returns>
    public iContact GetPrevSubContact(iContact sc = null, DWORD Name = 0xFFFFFFFF, bool only_detached = true);
    // prediction
    /// <summary>
    /// must be called before MakePrediction()
    /// </summary>
    public void StartPrediction();
    /// <summary>
    /// make one step of rough prediction
    /// </summary>
    /// <param name="scale"></param>
    public void MakePrediction(float scale);
    /// <summary>
    /// must be called after MakePrediction()
    /// </summary>
    public void EndPrediction();
    // perfomance data
    public bool IsSurfaced();
    public float GetMaxSpeed();
    public float GetMaxCornerSpeed();
    /// <summary>
    /// unit's nominal sensor range
    /// </summary>
    /// <returns></returns>
    public float GetSensorsRange();
    /// <summary>
    /// unit's nominal sensor visibvlity
    /// </summary>
    /// <returns></returns>
    public float GetSensorsVisibility();
    /// <summary>
    /// index of unit's type
    /// </summary>
    /// <returns></returns>
    public int GetTypeIndex();
    /// <summary>
    /// character name of type
    /// </summary>
    /// <returns></returns>
    public string GetTypeName();
    /// <summary>
    /// unit's prototype
    /// </summary>
    /// <returns></returns>
    public int GetProtoType();
    /// <summary>
    /// for target distrubution
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public float GetPowerFor(iContact i);
    /// <summary>
    /// for target distrubution
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public float GetImportanceFor(iContact i);

    // internal part
    /// <summary>
    /// for internal purposes
    /// </summary>
    /// <returns></returns>
    public DWORD GetThreatHandle();
    public void SetThreat(DWORD thr, float f);
    public void AddThreatC(float f);
    public int GetMissileCount();
    public void IncMissileCount();
    public void DecMissileCount();
    public FPO GetFpo();
    public HMember GetHMember();
    public iSensors GetSensors();
    public bool IsPlayedByHuman();
    public ILog GetLog();
    public void SetLog(ILog i);

    bool IsManualCotrolled()
    {
        iMovementSystem s = (iMovementSystem)GetInterface(iMovementSystem.ID);
        return s != null ? s.IsManual() : false;
    }

    bool isDead()
    {
        return GetState() == iSensorsDefines.CS_DEAD;
    }

    bool isInGame()
    {
        return GetState() == iSensorsDefines.CS_IN_GAME;
    }
    public float getMinRadius();
    /// <summary>
    /// Получить тип юнита из iSensorsDefines.UT_*
    /// </summary>
    /// <returns></returns>
    public int getUnitType();
};
