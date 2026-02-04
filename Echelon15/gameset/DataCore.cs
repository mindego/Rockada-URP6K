using crc32 = System.UInt32;
public interface IRecord : IGamesetMember,IScriptableEvent
{
    public string getText();
    public void setText(string text);

    public string getVideo();
    public void setVideo(string text);

    public string getAudio();
    public void setAudio(string text);
};

public interface ISelectionEvent : IEvent
{
    public void setText(int i, string text);
    public string getText(int i);
    public void setScript(int i, string script);
    public string getScript(int i);
};

public interface ILoadErrorLog : IObject
{
    public void addWarning(int error_code, string name, string action);
};

public interface IStormDataEnumer : IObject
{
    public int getRoadTypesCount();
    public string getRoadType(int i);

    public int getStaticTypesCount();
    public BaseUnitType getStaticType(int i);

    public int getCraftTypesCount();
    public BaseUnitType getCraftType(int i);

    public int getVehicleTypesCount();
    public BaseUnitType getVehicleType(int i);

    public int getSeaShipTypesCount();
    public BaseUnitType getSeaShipType(int i);

    public int getAirShipTypesCount();
    public BaseUnitType getAirShipType(int i);

    public UnitType getGroupTypeExByName(string name);
    public void getPreviewData(string name, ref PreviewData pd);
    public float getMaxSpeed(string name);

    public bool enumWeapons(string name, int idx, IEnumer<EnumItemInfo> enumer);
    public void enumStaticLayouts(string name, IEnumer<EnumItemInfo> enumer);
};

public class BaseUnitType
{
    public string myName;
    public SidesType mySide;

    public BaseUnitType()
    {
    }

    public BaseUnitType(string name, SidesType side)
    {
        myName = name;
        mySide = side;
    }

    internal void init(string name, SidesType side)
    {
        myName = name;
        mySide = side;
    }
}
public interface IAiEnumerator : IObject
{
    public string getError();
    public int enumMissionAis(IStringEnum enumer);
    public int enumGroupAis(GroupType gt, IStringEnum enumer);
    public int enumUnitAis(UnitType ut, IStringEnum enumer);
};

public struct EnumItemInfo
{
    public string myFullName;
    public crc32 myName;
    public string myDescription;
};

public struct PreviewData
{
    public string myFileName;
    public float myDelta;
};

public static class DataCore
{
    public const int LE_LOCALIZATION_FOR_EVENT_MISSED = 1;
    public const int LE_VAR_MISSED = 2;
    public const int LE_RESOLVE_ERROR = 3;

    public static UnitAttribute resolveUnitAttribute<Data>(Data data) where Data:OBJECT_DATA
    {
        UnitAttribute p=new UnitAttribute();
        p.setSide(getSideFromInt(data.Side));
        switch (data.GetClass())
        {
            case OBJECT_DATA.OC_STATIC:
            case OBJECT_DATA.OC_SFG:
            case OBJECT_DATA.OC_HANGAR:
                p.setType(UnitType.utStatic);
                break;
            case OBJECT_DATA.OC_CRAFT: p.setType(UnitType.utCraft); break;
            case OBJECT_DATA.OC_VEHICLE: p.setType(UnitType.utVehicle); break;
            case OBJECT_DATA.OC_AIRSHIP: p.setType(UnitType.utAirShip); break;
            case OBJECT_DATA.OC_SEASHIP: p.setType(UnitType.utSeaShip); break;
            default: p.setType(UnitType.utLast); break;
        }
        return p;
    }

    public static SidesType getSideFromInt(int side)
    {
        //return System.Enum.GetName(typeof(SidesType),side));
        return (SidesType)side;
    }
}

