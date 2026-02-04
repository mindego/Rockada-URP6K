using System.Collections.Generic;
using crc32 = System.UInt32;
using GamesetReal = Gameset<IRoadBuilder, RoadNetData>;
public static class DataCoreFactory
{
    // typedef Gameset<IRoadBuilder, RoadNetData, CreateRB> GamesetReal;
    public static IGameset CreateGameSet(string name, string msn_name, ILoadErrorLog log, uint version = IGameset.GAMESET_VERSION)
    {
        GamesetReal gs = null;
        if (version == IGameset.GAMESET_VERSION)
        {
            StormDataNameResolver nm = new StormDataNameResolver();
            gs = new GamesetReal(nm);
            if (!gs.initialize(name, log, msn_name))
            {
                gs.Release();
                gs = null;
            }
        }
        return gs;
    }
    public static IStormDataEnumer CreateStormDataEnumer(uint version = IGameset.GAMESET_VERSION)
    {
        return (version == IGameset.GAMESET_VERSION) ? new StormDataEnumer() : null;
    }
    public static IAiEnumerator CreateAiEnumerator(string dll_names, uint version = IGameset.GAMESET_VERSION)
    {
        return (version == IGameset.GAMESET_VERSION) ? new AiEnumerator(dll_names) : null;
    }
}

public class AiEnumerator : IAiEnumerator, IAiRegister
{
    bool enumAiModule(object dll)
    {
        throw new System.NotImplementedException();
        //createAiModuleFun fun = createAiModuleFun(GetProcAddress(dll, "CreateAiModule"));
        //if (fun)
        //{
        //    TRef<IAiModule> mod = fun(0);
        //    mod->Initialize(0, this);
        //}
        //return fun;
    }

    public virtual bool RegisterAi(IAiModule module, string name, int lev_id)
    {
        switch ((uint)lev_id)
        {
            case CampaignDefines.CF_CLASS_MISSION: return addMissionAi(name);
            case CampaignDefines.CF_CLASS_STATIC_GROUP: return addGroupAi(name, GroupType.gtStatic);
            case CampaignDefines.CF_CLASS_DYNAMIC_GROUP: return addGroupAi(name, GroupType.gtDynamic);
            case CampaignDefines.CF_CLASS_STATIC: return addUnitAi(name, UnitType.utStatic);
            case CampaignDefines.CF_CLASS_VEHICLE: return addUnitAi(name, UnitType.utVehicle);
            case CampaignDefines.CF_CLASS_CRAFT: return addUnitAi(name, UnitType.utCraft);
            case CampaignDefines.CF_CLASS_AIR_CARRIER: return addUnitAi(name, UnitType.utAirShip);
            case CampaignDefines.CF_CLASS_SEA_CARRIER: return addUnitAi(name, UnitType.utSeaShip);
        }
        return true;
    }

    public AiEnumerator(string dll_names)
    {
        //string text = dll_names;
        //char ai_dll[256];

        //for (; (text = ParseFileName(text, ai_dll)) && ai_dll[0];)
        //{
        //    Dll ai = ai_dll;
        //    if (!ai() || !enumAiModule(ai()))
        //    {
        //        myErrorName = ai_dll;
        //        break;
        //    }
        //}
    }

    bool addGroupAi(string name, GroupType gt)
    {
        myGroupAis.Add(name);
        myGroupTypes.New(gt);
        return true;
    }

    bool addUnitAi(string name, UnitType gt)
    {
        myUnitAis.Add(name);
        myUnitTypes.New(gt);
        return true;
    }

    bool addMissionAi(string name)
    {
        myMissionAis.Add(name);
        return true;
    }

    public virtual int enumMissionAis(IStringEnum enumer)
    {
        int i;
        for ( i = 0; i < myMissionAis.Count; ++i)
            enumer.AddString(myMissionAis[i]);
        return i;
    }

    public virtual int enumGroupAis(GroupType gt, IStringEnum enumer)
    {
        //return enumAi<GroupType>(gt, enumer, myGroupAis, myGroupTypes);
        int n = 0;
        for (int i = 0; i < myGroupTypes.Count(); i++)
        {
            if (myGroupTypes[i] == gt)
            {
                enumer.AddString(myGroupAis[i]);
                n++;
            }
        }
        return n;
    }

    public virtual string getError()
    {
        if (myErrorName!=null)
            return myErrorName;
        else
            return null;
    }

    int enumAi<myUnitType>(myUnitType ut, IStringEnum enumer, List<string> names, Tab<myUnitType> types) where myUnitType:class
    {
        int n = 0;
        for (int i = 0; i < types.Count(); ++i)
        {
            myUnitType tmp = types[i];
            if (tmp == ut)
            {
                enumer.AddString(names[i]);
                n++;
            }
        }

        return n;
    }

    public virtual int enumUnitAis(UnitType ut, IStringEnum enumer)
    {
        //return enumAi<UnitType>(ut, enumer, myUnitAis, myUnitTypes);
        int n = 0;
        for (int i=0;i<myUnitTypes.Count();i++)
        {
            if (myUnitTypes[i] == ut)
            {
                enumer.AddString(myUnitAis[i]);
                n++;
            }
        }
        return n;
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }

    List<string> myMissionAis;
    List<string> myGroupAis;
    List<string> myUnitAis;
    //Tab<string> myMissionAis = new();
    //Tab<string> myGroupAis = new();
    //Tab<string> myUnitAis = new();
    Tab<GroupType> myGroupTypes;
    Tab<UnitType> myUnitTypes;
    string myErrorName;
}
public class StormDataEnumer : IStormDataEnumer
{
    public virtual int getRoadTypesCount()
    {
        return ROADDATA.nItems();
    }

    int getDataCount(UnitType ut)
    {
        int n = 0;

        foreach(OBJECT_DATA data in OBJECT_DATA.Datas)
        {
            UnitAttribute attr = DataCore.resolveUnitAttribute<OBJECT_DATA>(data);
            if (attr.myType == ut) n++;
        }
        return n;
    }

    
    OBJECT_DATA getDataByIndex<DataClass, IterClass>(UnitType ut, int i) where IterClass :OBJECT_DATA where DataClass:OBJECT_DATA
    {
        // TODO - Что-то здесь не так. Надо обдумать, как вытаскивать данные типа по индексу.
        //    int n = 0;
        //    IterClass data = IterClass.GetFirstItem();
        //    while (data!=null)
        //    {
        //        UnitAttribute attr = DataCore.resolveUnitAttribute<IterClass>(data);
        //        if (attr.myType == ut)
        //        {
        //            if (n == i)
        //                break;
        //            n++;
        //        }
        //        data = data->Next();
        //    }
        //    return (DataClass)data;

        int n = 0;
        foreach(IterClass data in OBJECT_DATA.Datas)
        {
            UnitAttribute attr = DataCore.resolveUnitAttribute<IterClass>(data);
            if (attr.myType == ut)
            {
                if (n == i)
                {
                    return (OBJECT_DATA) data;
                }
                n++;
            }
        }
        return null;
    }


    public virtual string getRoadType(int i)
    {
        //int n = 0;
        //ROADDATA data = ROADDATA.GetFirstItem();
        //while (data)
        //{
        //    if (n == i)
        //        return data.FullName;
        //    n++;
        //    data = data.Next();
        //}
        //return null;
        int n = 0;
        foreach (ROADDATA data in ROADDATA.Datas)
        {
            if (n == i) return data.FullName;
            n++;
        }
        return null;
    }

    ReturnClass getType<DataClass,ReturnClass>(UnitType ut, int i) where DataClass:OBJECT_DATA where ReturnClass: BaseUnitType, new()
    {
        DataClass data = (DataClass) getDataByIndex<DataClass, OBJECT_DATA>(ut, i);
        var tmp= new ReturnClass();
        tmp.init(data.FullName, DataCore.getSideFromInt(data.Side));
        return tmp;
        //return new ReturnClass(data.FullName, DataCore.getSideFromInt(data.Side));
    }

    public virtual BaseUnitType getStaticType(int i)
    {
        return getType<STATIC_DATA, BaseUnitType>(UnitType.utStatic, i);
    }

    public virtual int getStaticTypesCount()
    {
        return getDataCount(UnitType.utStatic);
    }

    public virtual int getCraftTypesCount()
    {
        return getDataCount(UnitType.utCraft);
    }

    public virtual BaseUnitType getCraftType(int i)
    {
        return getType<CRAFT_DATA, BaseUnitType>(UnitType.utCraft, i);
    }

    public virtual int getVehicleTypesCount()
    {
        return getDataCount(UnitType.utVehicle);
    }

    public virtual BaseUnitType getVehicleType(int i)
    {
        return getType<VEHICLE_DATA, BaseUnitType>(UnitType.utVehicle, i);
    }

    public virtual int getSeaShipTypesCount()
    {
        return getDataCount(UnitType.utSeaShip);
    }

    public virtual BaseUnitType getSeaShipType(int i)
    {
        return getType<CARRIER_DATA, BaseUnitType>(UnitType.utSeaShip, i);
    }

    public virtual int getAirShipTypesCount()
    {
        return getDataCount(UnitType.utAirShip);
    }

    public virtual BaseUnitType getAirShipType(int i)
    {
        return getType<CARRIER_DATA, BaseUnitType>(UnitType.utAirShip, i);
    }

    public virtual UnitType getGroupTypeExByName(string name)
    {
        // static
        int n = getStaticTypesCount();
        int i;
        for (i = 0; i < n; i++)
        {
            if (getStaticType(i).myName == name)
                return UnitType.utStatic;
        }
        // airship
        n = getAirShipTypesCount();
        for (i = 0; i < n; i++)
        {
            if (getAirShipType(i).myName ==  name)
                return UnitType.utAirShip;
        }
        // seaship
        n = getSeaShipTypesCount();
        for (i = 0; i < n; i++)
        {
            if (getSeaShipType(i).myName == name)
                return UnitType.utSeaShip;
        }
        // craft
        n = getCraftTypesCount();
        for (i = 0; i < n; i++)
        {
            if (getCraftType(i).myName == name)
                return UnitType.utCraft;
        }
        // vehicle
        n = getVehicleTypesCount();
        for (i = 0; i < n; i++)
        {
            if (getVehicleType(i).myName ==  name)
                return UnitType.utVehicle;
        }
        return UnitType.utLast;
    }

    public virtual void getPreviewData(string name, ref PreviewData pd)
    {
        crc32 hash = Hasher.HshString(name);
        foreach (OBJECT_DATA data in OBJECT_DATA.Datas)
        {
            if (hash==data.Name)
            {
                pd.myFileName = data.RootData.FileName;
                pd.myDelta = (data.GetClass() == OBJECT_DATA.OC_STATIC) ? ((STATIC_DATA)data).Delta.y : 0;
            }
        }
        //OBJECT_DATA data = OBJECT_DATA.GetFirstItem();
        //while (data)
        //{
        //    if (hash == data->Name)
        //    {
        //        pd.myFileName = data->RootData->FileName;
        //        pd.myDelta = data->GetClass() == OC_STATIC ? ((STATIC_DATA*)data)->Delta.y : 0;
        //        return;
        //    }
        //    data = data->Next();
        //}
    }

    public virtual float getMaxSpeed(string name)
    {
        crc32 hash = Hasher.HshString(name);

        foreach (OBJECT_DATA data in OBJECT_DATA.Datas)
        {
            if (hash == data.Name)
            {
                switch (data.GetClass())
                {
                    case OBJECT_DATA.OC_VEHICLE:
                        return ((VEHICLE_DATA)data).MaxSpeed;
                    case OBJECT_DATA.OC_CRAFT:
                        return ((CRAFT_DATA)data).MaxSpeed.z;
                    case OBJECT_DATA.OC_AIRSHIP:
                    case OBJECT_DATA.OC_SEASHIP:
                        return 1 / ((CARRIER_DATA)data).OOMaxSpeedZ;
                }
            }
        }
        return 0;
    }

    public virtual bool enumWeapons(string name, int idx, IEnumer<EnumItemInfo> enumer)
    {
        CRAFT_DATA data = (CRAFT_DATA)CRAFT_DATA.GetByName(name, false);
        return data!=null ? LayoutEnumerator.enumWeapons<WPN_DATA>(data, SUBOBJ_DATA.Datas, idx, enumer) : false;
    }

    void enumOnlyLayout<T>(string name, IEnumer<EnumItemInfo> enumer) where T:OBJECT_DATA
    {
        T data = (T)OBJECT_DATA.GetByName(name, false);
        if (data!=null)
            LayoutEnumerator.enumLayouts<T>(data, enumer);
    }

    public virtual void enumStaticLayouts(string name, IEnumer<EnumItemInfo> enumer)
    {
        if (getGroupTypeExByName(name) == UnitType.utAirShip)
            enumOnlyLayout<CARRIER_DATA>(name, enumer);
        else
            enumOnlyLayout<STATIC_DATA>(name, enumer);
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }
}