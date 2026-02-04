#define _DEBUG
using System;
using crc32 = System.UInt32;

public class StdAiModule : IAiModule
{
    //static string MyDllMsg = "^DStandart Logic module (C) by MADia,Ltd " + __TIME__  + " "  + __DATE__;
    static string MyDllMsg = "Standart Logic module (C) by MADia,Ltd (mindego remake)";
    static string sUnknownAiName = "unknown name \"{0}\"";
    IServer iserver;
    IAiRegister mpAiReg;
    bool initialized;
    ILog gamelog;

    void ParseString(string s) { }

    public StdAiModule()
    {
        iserver = null;
        mpAiReg = null;
    }
    public StdAiModule(LOG log)
    {
        gamelog = log;
        if (log != null)
            log.Message(MyDllMsg);
    }
    ~StdAiModule() { }

    public virtual IMissionAi CreateMissionAi(crc32 code, IGame igame, IServer iserver, AiMissionData data)
    {
        //stdlogic_dll.DllMain();
        StdMissionAi ret = null;
        switch (code)
        {
            case StdCooperativeAi.ID: ret = new StdCooperativeAi(); break; //StdCooperative
            case StdTeamplayAi.ID: ret = new StdTeamplayAi(); break; //StdTeamplay
            case InstantActionAi.ID: ret = new InstantActionAi(); break; //InstantAction
            //default: throw GENERIC_EXCEPTION(sUnknownAiName, data->GetAi());
            default: throw new System.Exception(string.Format(sUnknownAiName, data.GetAi()));
        }
        //try
        {
            ret.SetInterface(igame, iserver, data);
        }
        //catch (Exception e)
        //{
        //    ret.Release();
        //    ret = null;
        //    if (gamelog != null)
        //        gamelog.AddException(e);
        //}
        return ret;
    }

    public virtual IGroupAi CreateGroupAi(crc32 code, GROUP_DATA grp_data, IMissionAi miss_ai)
    {
        StdGroupAi ret;
        switch (code)
        {
            case StdCraftGroupAi.ID: ret = new StdCraftGroupAi(); break; // StdDynamicGroup
            case StdTankGroupAi.ID: ret = new StdTankGroupAi(); break; // StdDynamicGroup
            case StandartStaticGroupAi.ID: ret = new StandartStaticGroupAi(); break; // StdStaticGroup
            case RepairGroup.ID: ret = new RepairGroup(); break; // RepairGroup
            case CaptureGroup.ID: ret = new CaptureGroup(); break; // CaptureGroup
            case DefendGroup.ID: ret = new DefendGroup(); break; // DefendGroup
            default: throw new Exception(string.Format(sUnknownAiName, grp_data.AI));
        }
        try
        {
            ret.SetInterface(grp_data, miss_ai);
        }
        catch (Exception e)
        {
            throw;
            ret.Release();
            ret = null;
            if (gamelog != null)
                gamelog.AddException(e);
        }
        return ret;
    }

    public virtual IBaseUnitAi CreateUnitAi(crc32 code, UNIT_DATA unit_data, iContact cnt, IGroupAi grp_ai, IGame igame)
    {
        BaseAi ret = null;
        switch (code)
        {
            case StdStaticAi.ID: ret = new StdStaticAi(); break;// StdStatic
            case StdStaticHangarAi.ID: ret = new StdStaticHangarAi(); break;// StdStaticHangar
            case StdTankAi.ID: ret = new StdTankAi(); break;// StdTank
            case StdCarrierAi.ID: ret = new StdCarrierAi(); break;// StdCarrier
            case StdSfgAi.ID: ret = new StdSfgAi(); break;// StdStaticSfg
            case SingleBFAi.ID: ret = new SingleBFAi(); break;// SingleBF
            case SingleCargoAi.ID: ret = new SingleCargoAi(); break;// SingleCargo
            case StdHTGRAi.ID: ret = new StdHTGRAi(); break;// StdHTGR
            case StdMobSfgAi.ID: ret = new StdMobSfgAi(); break;// VehicleSfg
            default: throw new Exception(string.Format(sUnknownAiName, unit_data.AI));
        }
#if _DEBUG
        if (ret != null)
            ret.SetInterface(igame, cnt, unit_data, grp_ai);
        else
            throw new Exception(String.Format("can't create specified AI \"{0}\"", unit_data.AI != "" ? unit_data.AI : Parsing.sAiEmpty));
#else
        try
        {
            if (ret != null)
                ret.SetInterface(igame, cnt, unit_data, grp_ai);
            else
                throw new Exception(String.Format("can't create specified AI \"{0}\"", unit_data.AI != "" ? unit_data.AI : Parsing.sAiEmpty));
        }
        catch (Exception e)
        {
            ret.Release();
            ret = null;
            if (gamelog != null)
                gamelog.AddException(e);
        }
#endif
        return ret;
    }

    public virtual bool Initialize(IServer server, IAiRegister aireg)
    {
        if (initialized) return true;
        iserver = server;
        mpAiReg = aireg;
        initialized = true;

        if (mpAiReg != null)
        {
            // регистрируем типы

            // missions
            mpAiReg.RegisterAi(this, "StdCooperative", (int)CampaignDefines.CF_CLASS_MISSION);
            mpAiReg.RegisterAi(this, "StdTeamplay", (int)CampaignDefines.CF_CLASS_MISSION);
            mpAiReg.RegisterAi(this, "StdInstantAction", (int)CampaignDefines.CF_CLASS_MISSION);

            // groups
            mpAiReg.RegisterAi(this, "StdTankGroup", (int)CampaignDefines.CF_CLASS_DYNAMIC_GROUP);
            mpAiReg.RegisterAi(this, "StdCraftGroup", (int)CampaignDefines.CF_CLASS_DYNAMIC_GROUP);
            mpAiReg.RegisterAi(this, "StdStaticGroup", (int)CampaignDefines.CF_CLASS_STATIC_GROUP);

            mpAiReg.RegisterAi(this, "StdTeamplayGroupCapture", (int)CampaignDefines.CF_CLASS_STATIC_GROUP);
            mpAiReg.RegisterAi(this, "StdTeamplayGroupRepair", (int)CampaignDefines.CF_CLASS_STATIC_GROUP);
            mpAiReg.RegisterAi(this, "StdTeamplayGroupDefend", (int)CampaignDefines.CF_CLASS_STATIC_GROUP);

            // units
            mpAiReg.RegisterAi(this, "VehicleSfg", (int)CampaignDefines.CF_CLASS_VEHICLE);
            mpAiReg.RegisterAi(this, "StdHTGR", (int)CampaignDefines.CF_CLASS_VEHICLE);
            mpAiReg.RegisterAi(this, "StdTank", (int)CampaignDefines.CF_CLASS_VEHICLE);
            mpAiReg.RegisterAi(this, "StdStatic", (int)CampaignDefines.CF_CLASS_STATIC);
            mpAiReg.RegisterAi(this, "StdStaticSfg", (int)CampaignDefines.CF_CLASS_STATIC);
            mpAiReg.RegisterAi(this, "StdStaticHangar", (int)CampaignDefines.CF_CLASS_STATIC);
            mpAiReg.RegisterAi(this, "StdCarrier", (int)CampaignDefines.CF_CLASS_AIR_CARRIER);
            mpAiReg.RegisterAi(this, "StdCarrier", (int)CampaignDefines.CF_CLASS_SEA_CARRIER);
            mpAiReg.RegisterAi(this, "StdCraft", (int)CampaignDefines.CF_CLASS_CRAFT);
            mpAiReg.RegisterAi(this, "StdSingleCargo", (int)CampaignDefines.CF_CLASS_CRAFT);
        }
        return true;

    }
    public virtual int Release()
    {
        //delete this;
        return 0;
    }

    public virtual LOG GetLog()
    {
        return null;
    }
};
