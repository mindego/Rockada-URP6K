using UnityEngine;

/// <summary>
/// пакет обновления корабля
/// </summary>
public class CarrierUpdatePacket : ItemDataPacket
{
    public const uint CDP_UPDATE = BaseObjectPackets.ODP_LAST + 1;
    Vector3 Org;
    Vector3 Speed;
    float HeadingAngle;
    float PitchAngle;
};


/// <summary>
/// пакет создания корабля
/// </summary>
class CarrierCreatePacket : ObjectCreatePacket
{
    public const uint CDF_SFG_ON            = 0x01;
    public const uint CDF_HANGAR_DOOR_OPEN = 0x02;

    // пакеты переключения состояния ангаров
    public const uint CDP_HANGAR_OPEN_DOOR = CarrierUpdatePacket.CDP_UPDATE + 1;
    public const uint CDP_HANGAR_CLOSE_DOOR = CDP_HANGAR_OPEN_DOOR + 1;


    // ******************************************************************************************
    // последний пакет корабля
    public const uint CDP_LAST              = CDP_HANGAR_CLOSE_DOOR;
    byte DevicesFlags; // флаги отдельных устройств
};





