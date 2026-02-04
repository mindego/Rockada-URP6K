public class UnitSpawnData
{
    public uint ObjectName;
    public uint Layout1Name;
    public uint Layout2Name;
    public uint Layout3Name;
    public uint Layout4Name;
    public uint SideCode;

    public UnitSpawnData(uint objectName, uint layout1Name, uint layout2Name, uint layout3Name, uint layout4Name, uint sideCode)
    {
        //TODO Возможно, правильнее всё же сюда UINTы передавать и использовать.
        ObjectName = objectName;
        Layout1Name = layout1Name;
        Layout2Name = layout2Name;
        Layout3Name = layout3Name;
        Layout4Name = layout4Name;
        SideCode = sideCode;

        
    }

    //public UnitSpawnData(int objectName, int layout1Name, int layout2Name, int layout3Name, int layout4Name) : this(objectName, layout1Name, layout2Name, layout3Name, layout4Name,0) { }

    public UnitSpawnData(uint objectName) : this(objectName, Constants.THANDLE_INVALID, Constants.THANDLE_INVALID, Constants.THANDLE_INVALID, Constants.THANDLE_INVALID, 0)
    {
    }

    public UnitSpawnData() : this(Constants.THANDLE_INVALID)
    {
    }

    //public UnitSpawnData(int on = -1, int n1 = -1, int n2 = -1, int n3 = -1, int n4 = -1) : ObjectName(on),Layout1Name(n1),Layout2Name(n2),Layout3Name(n3),Layout4Name(n4),SideCode(0) { }


}

public abstract class VoicesCount
{
    public const int gMaxRadioVoices = 11; //TODO! Поменять нужное для оригинального "Шторма" 
}