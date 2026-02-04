public  class ItemDataPacket
{
    // типы пакетов создания
    public const uint IDP_OBJECT = 0;
    public const uint IDP_SUBOBJ = (IDP_OBJECT + 1);
    public const uint IDP_MISSILE   =  (IDP_SUBOBJ + 2);

    // мнимальный тип юзерских пакетов
    public const uint IDP_LAST = 31;
}