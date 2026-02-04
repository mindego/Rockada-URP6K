using UnityEngine;
using DWORD = System.UInt32;
public static class BaseObjectPackets
{


    public const uint ODP_DELETE = (ItemDataPacket.IDP_LAST + 1);


    // ******************************************************************************************
    // пакет смены стороны
    public const uint ODP_CHANGE_NAME = ODP_DELETE + 1;

    // пакет смены стороны
    public const uint ODP_CHANGE_SIDE = ODP_CHANGE_NAME + 1;
    public const uint ODP_LAST = ODP_CHANGE_SIDE;
}

/// <summary>
/// пакет создания объекта
/// </summary>
public class ObjectCreatePacket : ItemDataPacket
{
    private int OBJECT_NAME_LENGTH = 16;

    DWORD CodedName;    // имя OBJECT_DATA
    int SideCode;     // код стороны
    Vector3 Org;
    float HeadingAngle;
    float PitchAngle;
    float RollAngle;
    //char Name[OBJECT_NAME_LENGTH];
    string Name;
    byte ShouldLocalize;
};
