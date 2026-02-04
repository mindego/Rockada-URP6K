public static class BaseStaticPackets
{
    //// пакет создания статика
    //struct StaticCreatePacket : ObjectCreatePacket
    //{
    //    BYTE DevicesFlags; // флаги отдельных устройств
    //};
    public const uint SDF_SFG_ON = 0x01;
    public const uint SDF_HANGAR_DOOR_OPEN = 0x02;
    // ******************************************************************************************
    // охерительная кривизна во всей своей красе!
    // mindego: комментарий сохранён для нагляжности!

    // пакеты переключения состояния SFG
    public const uint SDP_SFG_TURN_ON = (BaseObjectPackets.ODP_LAST + 1);
    public const uint SDP_SFG_TURN_OFF = SDP_SFG_TURN_ON + 1;

    // пакеты переключения состояния ангаров
    public const uint SDP_HANGAR_OPEN_DOOR = (SDP_SFG_TURN_OFF + 1);
    public const uint SDP_HANGAR_CLOSE_DOOR = (SDP_HANGAR_OPEN_DOOR + 1);


    // ******************************************************************************************
    // с этого кода можно продолжать нумерацию пакетов для наследников BaseStatic
    public const uint SDP_TLAST = SDP_HANGAR_CLOSE_DOOR;
}
