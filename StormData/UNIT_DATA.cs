using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// UNIT_DATA - представление юнита для ИИ
/// </summary>
public class UNIT_DATA
{
    public DWORD Flags;
    public DWORD CodedName;
    public string AI;
    public DWORD Number;
    public Vector3 Org;
    public float Angle;
    public string AiScript;
    public DWORD Layout1;
    public DWORD Layout2;
    public DWORD Layout3;
    public DWORD Layout4;
    // работа с флагами
    public void SetFlag(DWORD Flag) { Flags |= Flag; }
    public void ClearFlag(DWORD Flag) { Flags &= ~Flag; }
    public DWORD GetFlag(DWORD Flag) { return Flags & Flag; }

    public void Destroyed() { ClearFlag(UnitFlags.CF_CREATED); if (GetFlag(UnitFlags.CF_DELETED) == 0) SetFlag(UnitFlags.CF_DESTROYED); }
    public void Cleared() { ClearFlag(UnitFlags.CF_CREATED | UnitFlags.CF_DESTROYED | UnitFlags.CF_DELETED | UnitFlags.CF_LANDED | UnitFlags.CF_REPAIRED); }
    public void Deleted() { ClearFlag(UnitFlags.CF_CREATED); SetFlag(UnitFlags.CF_DELETED); }
    public void Landed(bool repair) { SetFlag(UnitFlags.CF_LANDED); if (repair) SetFlag(UnitFlags.CF_REPAIRED); }
    public void NotLanded() { ClearFlag(UnitFlags.CF_LANDED | UnitFlags.CF_REPAIRED); }

    public void Created() { ClearFlag(UnitFlags.CF_DELETED | UnitFlags.CF_DESTROYED); SetFlag(UnitFlags.CF_CREATED); }

    public bool IsCreated() {
        return GetFlag(UnitFlags.CF_CREATED) != 0; 
    }
    public bool IsDeleted() { return GetFlag(UnitFlags.CF_DELETED) != 0; }
    public bool IsDestroyed() { return GetFlag(UnitFlags.CF_DESTROYED) != 0; }
    public bool IsLanded(bool repair) { return repair ? GetFlag(UnitFlags.CF_REPAIRED) != 0 : GetFlag(UnitFlags.CF_LANDED) != 0; }
    public bool IsRepaired() { return IsLanded(true); }

    // создание/удаление
    //UNIT_DATA() :AI(0),AiScript(0),Flags(0),Number(0) { }

    //~UNIT_DATA() {
    //    if (AI != 0) delete AI;
    //    if (AiScript != 0) delete AiScript;
    //}

    public void Init(iUnifiedVariableContainer pCtr, int Num)
    {
        //Debug.Log((pCtr, Num));
        UniVarInt pInt;
        UniVarString pString;
        UniVarFloat pFloat;
        UniVarVector pVector;
        // Flags
        pInt = (UniVarInt) pCtr.GetVariableTpl<iUnifiedVariableInt>("Flags");
        Flags = (uint) pInt.GetValue();
        // CodedName
        pInt = (UniVarInt) pCtr.GetVariableTpl<iUnifiedVariableInt>("Name");
        CodedName = (uint) pInt.GetValue();
        // AI
        pString = (UniVarString) pCtr.GetVariableTpl<iUnifiedVariableString>("Ai");
        pString.StrCpy(out AI);
        // Number
        Number = (uint) Num;
        // Org
        pVector = (UniVarVector) pCtr.GetVariableTpl<iUnifiedVariableVector>("Org");
        Org = pVector.GetValue();
        // Angle
        pFloat = (UniVarFloat) pCtr.GetVariableTpl<iUnifiedVariableFloat>("Angle");
        Angle = Storm.Math.GRD2RD(pFloat.GetValue()); //Углы в радианах
        //Angle = pFloat.GetValue(); //Углы в градусах
        // AiScript
        pString = (UniVarString) pCtr.GetVariableTpl<iUnifiedVariableString>("AiScript");
        pString.StrCpy(out AiScript);
        // Layout1
        pInt = (UniVarInt) pCtr.GetVariableTpl<iUnifiedVariableInt>("Layout1");
        Layout1 = (uint) pInt.GetValue();
        // Layout2
        pInt = (UniVarInt) pCtr.GetVariableTpl<iUnifiedVariableInt>("Layout2");
        Layout2 = (uint)pInt.GetValue();
        // Layout3
        pInt = (UniVarInt) pCtr.GetVariableTpl<iUnifiedVariableInt>("Layout3");
        Layout3 = (uint)pInt.GetValue();
        // Layout4
        pInt = (UniVarInt) pCtr.GetVariableTpl<iUnifiedVariableInt>("Layout4");
        Layout4 = (uint) pInt.GetValue();

    }

    public override string ToString()
    {
        string res = GetType().ToString();
        res += "\n" + CodedName.ToString("X8") + " " + Number;
        return res;
    }
}