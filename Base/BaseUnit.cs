using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// BaseUnit: BaseContact для игрового объекта
/// </summary>
public class BaseUnit : IBaseUnit, IQueryInterface
{
    public const uint ID = 0x5D0D5974;

    // ****************************************************************************
    // от iContact
    public float GetCondition()
    { // unit's condation (0 - deadn and should be released, 1 - fully alive)
        return Condition;
    }

    public bool IsInSF()
    { // true - is in supressing field
        return IsSupressed;
    }

    public int GetSideCode()
    { // code of unit's side
        return pContact.GetSensors().GetSideCode();
    }

    public float GetThreatC()
    { // for target distriubution
        return ThreatC;
    }

    public float GetThreatF()
    { // relative threatness of this unit (0 - clear, 1 - moderate threat, >1 - under fire)
        return ThreatF;
    }

    // ****************************************************************************
    // prediction
    public void StartPrediction()
    { // must be called before MakePrediction()
    }

    public void MakePrediction(float scale)
    { // make one step of prediction
    }

    public void EndPrediction()
    { // must be called after MakePrediction()
    }

    // perfomance data
    public float GetSensorsRange()
    { // unit's nominal sensor range
        return UnitDataTable.pUnitDataTable.GetUD(UnitDataIndex).SensorRadius;
    }

    public float GetSensorsVisibility()
    { // unit's nominal sensor visibvlity
        return UnitDataTable.pUnitDataTable.GetUD(UnitDataIndex).SensorVisibility;
    }

    public int GetTypeIndex()
    { // index of unit's type 
        return UnitDataIndex;
    }

    public string GetTypeName()
    { // character name of type
        return new string(UnitDataTable.pUnitDataTable.GetUD(UnitDataIndex).Name).Trim('\0');
    }

    public int GetProtoType()
    { // unit's prototype
        return UnitDataTable.pUnitDataTable.GetUD(UnitDataIndex).Type;
    }

    public float GetPowerFor(iContact c)
    {// for target distrubution
        return UnitDataTable.pUnitDataTable.GetUDTE(c.GetTypeIndex(), UnitDataIndex).power;
    }

    public float GetImportanceFor(iContact c)
    { // for target distrubution
        return UnitDataTable.pUnitDataTable.GetUDTE(c.GetTypeIndex(), UnitDataIndex).importance;
    }

    // internal part
    public DWORD GetThreatHandle()
    { // for internal purposes
        return (ThreatF > .0f ? ThreatHandle : Constants.THANDLE_INVALID);
    }

    public void SetThreat(DWORD thr, float f)
    {
        if (f < ThreatF) return;
        ThreatF = f;
        ThreatHandle = thr;
    }

    public void AddThreatC(float f)
    {
        ThreatC += f;
    }

    public int GetMissileCount()
    {
        return MissileCount;
    }

    public void IncMissileCount()
    {
        MissileCount++;
    }

    public void DecMissileCount()
    {
        MissileCount--;
    }

    public iSensors GetSensors()
    {
        return pContact.GetSensors();
    }

    public ILog GetLog()
    {
        return mpLog;
    }

    public void SetLog(ILog l)
    {
        //mpLog = SafeAddRef(l);
        mpLog = l;
    }

    // subcontacts
    public bool HasSubContacts()
    {
        return false;
    }

    public BaseSubobj GetFirstSubContact()
    {
        return null;
    }

    public BaseSubobj GetLastSubContact()
    {
        return null;
    }

    // свое (было protected)
    public iContact pContact;
    public int UnitDataIndex;
    public float ThreatF;
    public DWORD ThreatHandle;
    public float ThreatC;
    public float Condition;
    public int MissileCount;
    public bool IsSupressed;
    public ILog mpLog;
    public BaseUnit()
    {
        pContact = null;
        UnitDataIndex = 0;
        ThreatF = 0;
        ThreatHandle = Constants.THANDLE_INVALID;
        ThreatC = 0;
        Condition = 1f;
        IsSupressed = false;
        MissileCount = 0;
    }
    ~BaseUnit() { }

    public int GetUnitDataIndex() { return UnitDataIndex; }

    public uint GetHandle()
    {
        throw new System.NotImplementedException();
    }

    public int GetState()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetOrg()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetDir()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetUp()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetRight()
    {
        throw new System.NotImplementedException();
    }

    public float GetHeadingAngle()
    {
        throw new System.NotImplementedException();
    }

    public float GetPitchAngle()
    {
        throw new System.NotImplementedException();
    }

    public float GetRollAngle()
    {
        throw new System.NotImplementedException();
    }

    public float GetRadius()
    {
        throw new System.NotImplementedException();
    }

    public float getMinRadius()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetSpeed()
    {
        throw new System.NotImplementedException();
    }

    public iContact ChangeSideTo(int SideCode)
    {
        throw new System.NotImplementedException();
    }

    public string GetName()
    {
        throw new System.NotImplementedException();
    }

    public void SetName(string pName, bool ShouldLocalize)
    {
        throw new System.NotImplementedException();
    }

    public bool IsSurfaced()
    {
        throw new System.NotImplementedException();
    }

    public float GetMaxSpeed()
    {
        throw new System.NotImplementedException();
    }

    public float GetMaxCornerSpeed()
    {
        throw new System.NotImplementedException();
    }

    public int getUnitType()
    {
        throw new System.NotImplementedException();
    }

    public FPO GetFpo()
    {
        throw new System.NotImplementedException();
    }

    public HMember GetHMember()
    {
        throw new System.NotImplementedException();
    }

    public bool IsPlayedByHuman()
    {
        throw new System.NotImplementedException();
    }

    public void Dispose()
    {
        throw new System.NotImplementedException();
    }

    public object queryObject(uint id, int num)
    {
        throw new System.NotImplementedException();
    }
}
