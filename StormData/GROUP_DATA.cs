using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DWORD = System.UInt32;

public interface AIData
{
    // item is hidden
    public const uint CF_HIDDEN = 0x00001000;
    // item from default mission
    public const uint CF_DELETED = 0x00002000;
    // item from this data was created
    public const uint CF_CREATED = 0x00004000;
    // item from this data was destroyed
    public const uint CF_DESTROYED = 0x00008000;
    // item from this data was landed
    public const uint CF_LANDED = 0x00010000;
    // item from this data was landed
    public const uint CF_REPAIRED = 0x00020000;

    public DWORD GetID();
}
public class GROUP_DATA : AIData
{
    public DWORD Flags;                // флаги
    public DWORD ID;                   // HashString(Callsing)
    public DWORD Side;                 // 0 = Neutral
    public string AI;                   // Имя ИИ
    public DWORD nUnits;               // количество юнитов
    public UNIT_DATA[] Units;                // записи для юнитов
    public DWORD nPoints;              // количествго точек маршрута (0 для STATIC_GROUP)
    public POINT_DATA[] Points;               // записи для точек
    public string Callsign;             // позывной группы
    public DWORD Voice;                // голос группы (для playable group - свой алгоритм)
    public string AiScript;             // пользовательская строчная переменная

    // доп. данные - центр и радиус
    public float CenterX;
    public float CenterZ;
    public float Radius;
    //c# addon
    private const uint CF_CAMPAIGN = 0x00010000;
    public string GetFlags()
    {
        return Flags.ToString("X8");
    }

    // работа с флагами
    public void SetFlag(DWORD Flag) { Flags |= Flag; }
    public void ClearFlag(DWORD Flag) { Flags &= ~Flag; }
    public DWORD GetFlag(DWORD Flag) { return Flags & Flag; }

    public void Deleted() { ClearFlag((uint)UnitFlags.CF_CREATED); SetFlag((uint)UnitFlags.CF_DELETED); }
    public void Created() { ClearFlag((uint)UnitFlags.CF_DELETED | (uint)UnitFlags.CF_DESTROYED); SetFlag((uint)UnitFlags.CF_CREATED); }
    public void Destroyed() { ClearFlag((uint)UnitFlags.CF_CREATED); SetFlag((uint)UnitFlags.CF_DESTROYED); }
    public void Cleared() { ClearFlag((uint)UnitFlags.CF_DELETED | (uint)UnitFlags.CF_DESTROYED | (uint)UnitFlags.CF_CREATED); }

    public bool IsCreated() { return GetFlag((uint)UnitFlags.CF_CREATED) != 0; }
    public bool IsDeleted() { return GetFlag((uint)UnitFlags.CF_DELETED) != 0; }
    /// <summary>
    /// Отмечает группу как участника "миссии по умолчанию". Такие участники не посылают уведомления о создании 
    /// </summary>
    public void InCampaign() { SetFlag((uint)CF_CAMPAIGN); }
    public bool IsInCampaign() { return GetFlag(CF_CAMPAIGN) != 0; }
    public bool IsDestroyed() { return GetFlag((uint)UnitFlags.CF_DESTROYED) != 0; }
    public bool IsHidden() { return GetFlag((uint)UnitFlags.CF_HIDDEN) != 0; }

    // создание/удаление
    //GROUP_DATA():AI(0),Units(0),Points(0),Callsign(0),
    //             AiScript(0),ID(0xFFFFFFFF),nPoints(0),nUnits(0),
    //             Flags(0),Voice(1),CenterX(0),CenterZ(0),Radius(0) { }



    //}

    //TODO! Обдумать как нормально сравнивать при NULL
    //public static bool operator == (GROUP_DATA ldata,GROUP_DATA rdata)
    //{
    //    //Debug.Log(("cmp data: ",ldata, rdata));
    //    return ldata.Equals(rdata);
    //}
    //public static bool operator != (GROUP_DATA ldata, GROUP_DATA rdata)
    //{
    //    return ldata.Equals(rdata);
    //}
    public void Init(iUnifiedVariableContainer pCtr, string pName)
    {
        //Storm.CRC32 crc = new Storm.CRC32();
        UniVarInt pInt;
        UniVarString pString;
        UniVarArray pArray;
        // Flags
        pInt = (UniVarInt)pCtr.GetVariableTpl<iUnifiedVariableInt>("Flags");
        Flags = (uint)pInt.GetValue();
        // Side
        pInt = (UniVarInt)pCtr.GetVariableTpl<iUnifiedVariableInt>("Side");
        Side = (uint)pInt.GetValue();
        // AI
        pString = (UniVarString)pCtr.GetVariableTpl<iUnifiedVariableString>("Ai");
        pString.StrCpy(out AI);
        // Callsign
        Callsign = pName;
        // Voice
        Voice = 1;
        pInt = (UniVarInt)pCtr.GetVariableTpl<iUnifiedVariableInt>("Voice");
        Voice = (pInt != default ? (uint)pInt.GetValue() : 6);
        // Name
        //ID = crc.HashString(Callsign);
        ID = Hasher.HshString(Callsign);
        // AiScript
        pString = (UniVarString)pCtr.GetVariableTpl<iUnifiedVariableString>("AiScript");
        pString.StrCpy(out AiScript);
        //TODO! Возможно, триммить AiScript не нужно
        AiScript = AiScript.TrimEnd();
        // Units
        pArray = (UniVarArray)pCtr.GetVariableTpl<iUnifiedVariableArray>("Units");
        Units = new UNIT_DATA[0];
        nUnits = pArray.GetSize();
        if (nUnits > 0)
        {
            Units = new UNIT_DATA[nUnits];
            float MinX, MaxX;
            float MinZ, MaxZ;
            MinX = MaxX = 0;
            MinZ = MaxZ = 0;
            for (int i = 0; i < nUnits; i++)
            {
                //UniVarContainer pCtr1 = (UniVarContainer)pArray.GetVariableTpl<iUnifiedVariableContainer>((uint)i + 1);
                UniVarContainer pCtr1 = (UniVarContainer)pArray.GetVariableTpl<iUnifiedVariableContainer>((uint)i);
                //Debug.Log(pCtr1);
                Units[i] = new UNIT_DATA();
                Units[i].Init(pCtr1, i);
                if (i == 0)
                {
                    MinX = MaxX = Units[i].Org.x;
                    MinZ = MaxZ = Units[i].Org.z;
                }
                else
                {
                    if (MinX > Units[i].Org.x) MinX = Units[i].Org.x;
                    if (MaxX < Units[i].Org.x) MaxX = Units[i].Org.x;
                    if (MinZ > Units[i].Org.z) MinZ = Units[i].Org.z;
                    if (MaxZ < Units[i].Org.z) MaxZ = Units[i].Org.z;
                }
            }
            CenterX = (MinX + MaxX) * .5f;
            CenterZ = (MinZ + MaxZ) * .5f;
            Radius = Mathf.Sqrt(Mathf.Pow((MaxX - CenterX), 2) + Mathf.Pow((MaxZ - CenterZ), 2));
            // Points
            Points = new POINT_DATA[0]; nPoints = 0;
            pArray = (UniVarArray)pCtr.GetVariableTpl<iUnifiedVariableArray>("Points");
            if (pArray != null)
            {
                nPoints = pArray.GetSize();
                if (nPoints > 0)
                {
                    Points = new POINT_DATA[nPoints];
                    for (int i = 0; i < nPoints; i++)
                    {
                        //UniVarContainer pCtr1 = (UniVarContainer)pArray.GetVariableTpl<iUnifiedVariableContainer>((uint)i + 1);
                        UniVarContainer pCtr1 = (UniVarContainer)pArray.GetVariableTpl<iUnifiedVariableContainer>((uint)i );
                        Points[i] = new POINT_DATA();
                        Points[i].Init(pCtr1);
                    }
                }
            }
        }

    }

    public uint GetID()
    {
        return ID;
    }

    public override string ToString()
    {
        return "Group " + Callsign;
    }

    public override bool Equals(object obj)
    {
        return obj is GROUP_DATA dATA &&
               Callsign == dATA.Callsign;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Callsign);
    }
}

public static class UnitFlags
{
    public const uint CF_HIDDEN = 0x00001000;
    // item from default mission
    public const uint CF_DELETED = 0x00002000;
    // item from this data was created
    public const uint CF_CREATED = 0x00004000;
    // item from this data was destroyed
    public const uint CF_DESTROYED = 0x00008000;
    // item from this data was landed
    public const uint CF_LANDED = 0x00010000;
    // item from this data was landed
    public const uint CF_REPAIRED = 0x00020000;
}
//public enum UnitFlags : uint
//{
//    CF_HIDDEN = 0x00001000,
//    // item from default mission
//    CF_DELETED = 0x00002000,
//    // item from this data was created
//    CF_CREATED = 0x00004000,
//    // item from this data was destroyed
//    CF_DESTROYED = 0x00008000,
//    // item from this data was landed
//    CF_LANDED = 0x00010000,
//    // item from this data was landed
//    CF_REPAIRED = 0x00020000

//}
