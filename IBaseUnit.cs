using UnityEngine;
using DWORD = System.UInt32;

public interface IBaseUnit : iBaseInterface
{
    new public const uint ID = 0x5D0D5974;
    /// <summary>
    /// internal part - for sychronization
    /// </summary>
    /// <returns></returns>
    public DWORD GetHandle();
    /// <summary>
    /// unit's state (0 - should be released)
    /// </summary>
    /// <returns></returns>
    public int GetState();
    /// <summary>
    /// unit's condation
    /// </summary>
    /// <returns>(0 - deadn and should be released, 1 - fully alive)</returns>
    public float GetCondition();
    /// <summary>
    /// true - is in supressing field
    /// </summary>
    /// <returns></returns>
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
    public float getMinRadius();
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
    /// relative threatness of this unit
    /// </summary>
    /// <returns> (0 - clear, 1 - moderate threat, >1 - under fire)</returns>
    public float GetThreatF();
    /// <summary>
    /// to change unit's side. Returns new iContact if success, null otherwise
    /// </summary>
    /// <param name="SideCode"></param>
    /// <returns></returns>
    public iContact ChangeSideTo(int SideCode);
    /// <summary>
    /// get assotiated name
    /// </summary>
    /// <returns></returns>
    public string GetName         ();
    /// <summary>
    /// set assotiated name, no more than CONTACT_NAME_LENGTH
    /// </summary>
    /// <param name="pName"></param>
    /// <param name="ShouldLocalize"></param>
    public void SetName(string pName,bool ShouldLocalize);
    // prediction
    /// <summary>
    /// must be called before MakePrediction()
    /// </summary>
    public void StartPrediction();
    /// <summary>
    /// make one step of prediction
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
    public string GetTypeName       ();
    /// <summary>
    /// unit's prototype
    /// </summary>
    /// <returns></returns>
    public int GetProtoType();
    /// <summary>
    /// for target distrubution
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public float GetPowerFor( iContact c);
    /// <summary>
    /// for target distrubution
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public float GetImportanceFor( iContact c);
    public int getUnitType() ;
    // internal part
    /// <summary>
    /// for internal purposes
    /// </summary>
    /// <returns></returns>
    public DWORD GetThreatHandle(); 
    public void SetThreat(DWORD thr, float f);
    public void AddThreatC(float t);
    public int GetMissileCount();
    public void IncMissileCount();
    public void DecMissileCount();
    public FPO GetFpo();
    public HMember GetHMember();
    public iSensors GetSensors();
    public bool IsPlayedByHuman();
    public ILog  GetLog();
    public void SetLog(ILog log);
    // subcontacts
    public bool HasSubContacts();
    public BaseSubobj GetFirstSubContact();
    public BaseSubobj GetLastSubContact();

    public void Dispose();

    public object queryObject(DWORD id, int num);
}
