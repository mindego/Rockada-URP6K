using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType : uint
{
    utSeaShip,
    utAirShip,
    utVehicle,
    utCraft,
    utTurret,
    utStatic,
    utLast = 0xFFFFFFFF
};

public enum GroupType : uint
{
    gtStatic,
    gtDynamic,
    gtLast = 0xFFFFFFFF
};

public enum SidesType : uint
{
    stHuman,
    stVelian,
    stNeutral,
    stAlien,
    stLast = 0xFFFFFFFF
};

public struct UnitAttribute
{
    public UnitType myType;
    public SidesType mySide;
    public void set(UnitType tp, SidesType sd)
    {
        myType = tp;
        mySide = sd;
    }
    public void setType(UnitType tp)
    {
        myType = tp;
    }
    public void setSide(SidesType tp)
    {
        mySide = tp;
    }
};

