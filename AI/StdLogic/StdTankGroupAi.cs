using UnityEngine;
using DWORD = System.UInt32;

public class StdTankGroupAi : StdDynamicGroupAi
{
    new public const uint ID = 0xB95ED0B5;

    public const string NotVehicleUnit = "create : not vehicle unit in vehicle group \"{0}\"";

    public bool SetFightMode(DWORD mode)
    {
        for (int i = 0; i < mGhostCount; ++i)
        {
            AiUnit ai = mpUnits[i];
            if (ai==null || ai.GetAI() == null) continue;
            IVehicleAi veh = (IVehicleAi)ai.GetAI().Query(IVehicleAi.ID);
            if (veh != null)
                veh.SetFightMode(mode);
        }
        return true;
    }
    public bool SetUseRoadsMode(DWORD mode)
    {
        for (int i = 0; i < mGhostCount; ++i)
        {
            AiUnit ai = mpUnits[i];
            if (ai == null || ai.GetAI() == null) continue;
            IVehicleAi veh = (IVehicleAi)ai.GetAI().Query(IVehicleAi.ID);
            if (veh != null)
                veh.SetUseRoadsMode(mode);
        }
        return true;
    }

    public StdTankGroupAi()
    {
        myStdTankGroupService = new TankGroupService<StdTankGroupAi>(this);
    }

    // ========================================================================

    // api
    // common
    public override object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case StdTankGroupAi.ID: return this;
            case ITankGroupService.ID: return myStdTankGroupService;
            default: return base.Query(cls_id);
        }
    }


    // StdGroupAi
    public override void OnCreateUnit(IBaseUnitAi ai)
    {
        base.OnCreateUnit(ai);
        if (ai.Query(IVehicleAi.ID) == null)
            throw new System.Exception(string.Format(NotVehicleUnit, ai.GetType().ToString()+ " " + mpData.Callsign));
    }

    // park
    public override bool OnPark(string base_name, DWORD ultimate)
    {
        return AddAction(ActionFactory.CreateParkAction(GetIGroupAi(), base_name, ultimate));
    }


    // on StdDynamicGroupAi
    public override void SetInterface(GROUP_DATA data, IMissionAi miss_ai)
    {
        base.SetInterface(data, miss_ai);
        myStdTankGroupFactory = Factories.createStdTankGroupFactory(getIQuery(), myStdDynGroupFactory);
    }


    // new
    TankGroupService<StdTankGroupAi> myStdTankGroupService;
    IVmFactory myStdTankGroupFactory;

    public override IVmFactory getTopFactory() { return myStdTankGroupFactory; }
};
