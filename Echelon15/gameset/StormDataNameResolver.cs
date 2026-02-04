using crc32 = System.UInt32;

public class  StormDataNameResolver : IStormData
{
    public virtual string resolveObjectName(crc32 name)
    {
        OBJECT_DATA rd = OBJECT_DATA.GetByCode(name, false);
        return rd!=null ? rd.FullName : null;
    }

    public virtual string resolveWeaponName(crc32 name)
    {
        WPN_DATA rd = (WPN_DATA)WPN_DATA.GetByCode(name, false);
        return rd!=null ? rd.FullName : null;
    }

    public virtual string resolveRoadName(crc32 name)
    {
        ROADDATA rd = ROADDATA.GetByCode(name, false);
        return rd!=null ? rd.FullName : null;
    }
    public virtual UnitAttribute resolveUnitAttr(crc32 name)
    {
        OBJECT_DATA rd = OBJECT_DATA.GetByCode(name, false);
        if (rd!=null)
            return DataCore.resolveUnitAttribute<OBJECT_DATA>(rd);
        else
        {
            UnitAttribute attr = new UnitAttribute();
            attr.set(UnitType.utLast, SidesType.stLast);
            return attr;
        }
    }
    public virtual void processIntegrityFailure()
    {
        myIntegrity = true;
    }

    public virtual bool isIntegrityFailure()
    {
        return myIntegrity;
    }

    public virtual ROADDATA getRoadByCode(crc32 code, bool mustExist)
    {
        return ROADDATA.GetByCode(code, mustExist);
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

    bool myIntegrity = false;
}