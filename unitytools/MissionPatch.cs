using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using UnityEditor.PackageManager;
using UnityEditor.Timeline;
using UnityEngine;
using static CameraLogicStrategic;
using static CampaignDefines;
using static UnityEngine.Rendering.DebugUI;
using DWORD = System.UInt32;
using static MissionPatch.Maps;
using UnityEditor;

public static class MissionPatchFactory
{
    public static GROUP_DATA GetEscortGroup(string groupName, uint Side, Vector3 Org, int angle = 45, string ActiveLayout = "Turrets", POINT_DATA[] points = null)
    {
        GROUP_DATA gd = GetGroupData(groupName, CF_CLASS_DYNAMIC_GROUP + CF_APPEAR_MASK, "StdCraftGroup", null, Side, 1);
        AddUnits(ref gd, 1, "Human Avia Escort Vessel", "StdCarrier", null, 0x0F000F83, Org, Storm.Math.GRD2RD(angle), ActiveLayout);
        //"Human Avia Escort Vessel",1);

        if (points != null)
        {
            gd.Points = points;
            gd.nPoints = (DWORD)points.Length;
        }
        return gd;

    }

    public static GROUP_DATA GetBunkerGroup(string groupName, uint Side, Vector3 Org, int angle = 45, string ActiveLayout = "AA Rockets & Radar 15000m")
    {
        GROUP_DATA gd = GetGroupData(groupName, CF_CLASS_STATIC_GROUP + CF_APPEAR_MASK, "StdStaticGroup", null, Side, 1);
        AddUnits(ref gd, 1, "Bunker Heavy", "StdStatic", null, 0x0F000F13, Org, Storm.Math.GRD2RD(angle), ActiveLayout);
        //"Human Avia Escort Vessel",1);

        return gd;

    }
    public static GROUP_DATA GetMissileTurretGroup(string groupName, uint Side, Vector3 Org, int angle = 45, string ActiveLayout = null)
    {
        GROUP_DATA gd = GetGroupData(groupName, CF_CLASS_STATIC_GROUP + CF_APPEAR_MASK, "StdStaticGroup", null, Side, 1);
        AddUnits(ref gd, 1, "Heavy Rocket Turret", "StdStatic", null, 0x0F000F13, Org, Storm.Math.GRD2RD(angle), null);
        //"Human Avia Escort Vessel",1);

        return gd;

    }
    public static GROUP_DATA GetTurretGroup(string groupName, uint Side, Vector3 Org, int angle = 45, string ActiveLayout = null)
    {
        GROUP_DATA gd = GetGroupData(groupName, CF_CLASS_STATIC_GROUP + CF_APPEAR_MASK, "StdStaticGroup", null, Side, 1);
        //AddUnits(ref gd, 1, "AAC Heavy Turret Plasma", "StdStatic", null, 0x0F000F13, Org, Storm.Math.GRD2RD(angle), null);
        AddUnits(ref gd, 1, "AAC Light Turret Plasma", "StdStatic", null, 0x0F000F13, Org, Storm.Math.GRD2RD(angle), null);
        //"Human Avia Escort Vessel",1);

        return gd;

    }
    public static GROUP_DATA GetSHABORGroup(string groupName, uint Side, Vector3 Org, int angle = 45, string ActiveLayout = null, POINT_DATA[] points = null)
    {
        GROUP_DATA gd = GetGroupData(groupName, CF_CLASS_DYNAMIC_GROUP + CF_APPEAR_MASK, "StdTankGroup", null, Side, 1);
        //AddUnits(ref gd, 1, "ShaBoR", "StdTank", null, 0x0F000F13, Org, Storm.Math.GRD2RD(angle), null);
        AddUnits(ref gd, 1, "ShaBoR", "StdTank", null, CF_APPEAR_ARCADE + CF_APPEAR_ONE_CLIENT + CF_CLASS_VEHICLE, Org, Storm.Math.GRD2RD(angle), null);

        if (points != null)
        {
            gd.Points = points;
            gd.nPoints = (DWORD)points.Length;
        }
        return gd;

    }

    public static GROUP_DATA GetNuclearExplGroup(string groupName, uint Side, Vector3 Org, int angle = 45, string ActiveLayout = null)
    {
        GROUP_DATA gd = GetGroupData(groupName, CF_CLASS_STATIC_GROUP + CF_APPEAR_MASK, "StdStaticGroup", null, Side, 1);
        AddUnits(ref gd, 1, "NuclearExpl", "StdStatic", null, 0x0F000F13, Org, Storm.Math.GRD2RD(angle), null);
        //"Human Avia Escort Vessel",1);

        return gd;

    }
    public static UNIT_DATA GetUnitData(string name, uint flags, string ai, int number, Vector3 Org, float angle, string AiScript, string active_layout)
    {

        UNIT_DATA ud = new UNIT_DATA();
        ud.Flags = flags;
        ud.CodedName = Hasher.HshString(name);
        ud.AI = ai;
        ud.Number = (uint)number;
        ud.Org = Org;
        ud.Angle = angle;
        ud.AiScript = AiScript == null ? "" : AiScript;
        ud.Layout1 = active_layout == null ? 0xFFFFFFFF : Hasher.HshString(active_layout);
        ud.Layout2 = 0;
        ud.Layout3 = 0;
        ud.Layout4 = 0;
        return ud;
    }

    public static void AddUnits(ref GROUP_DATA gd, int GroupSize, string UnitName, string UnitAI, string AiScript, uint UnitFlags, Vector3 Org, float angle, string UnitActiveLayout)
    {
        gd.nUnits = (uint)GroupSize;
        gd.Units = new UNIT_DATA[gd.nUnits];

        if (gd.nUnits > 0)
        {
            float MinX, MaxX;
            float MinZ, MaxZ;
            MinX = MaxX = 0;
            MinZ = MaxZ = 0;
            for (int i = 0; i < gd.nUnits; i++)
            {
                //UniVarContainer pCtr1 = (UniVarContainer)pArray.GetVariableTpl<iUnifiedVariableContainer>((uint)i + 1);
                //Debug.Log(pCtr1);
                Debug.Log("Unit added with layout [" + UnitActiveLayout + "]");
                gd.Units[i] = GetUnitData(UnitName, UnitFlags, UnitAI, i, Org, angle, AiScript, UnitActiveLayout);
                if (i == 0)
                {
                    MinX = MaxX = gd.Units[i].Org.x;
                    MinZ = MaxZ = gd.Units[i].Org.z;
                }
                else
                {
                    if (MinX > gd.Units[i].Org.x) MinX = gd.Units[i].Org.x;
                    if (MaxX < gd.Units[i].Org.x) MaxX = gd.Units[i].Org.x;
                    if (MinZ > gd.Units[i].Org.z) MinZ = gd.Units[i].Org.z;
                    if (MaxZ < gd.Units[i].Org.z) MaxZ = gd.Units[i].Org.z;
                }
            }
            gd.CenterX = (MinX + MaxX) * .5f;
            gd.CenterZ = (MinZ + MaxZ) * .5f;
            gd.Radius = Mathf.Sqrt(Mathf.Pow((MaxX - gd.CenterX), 2) + Mathf.Pow((MaxZ - gd.CenterZ), 2));
        }
    }
    public static GROUP_DATA GetGroupData(string Callsign, uint flags, string AI, string AiScript, uint Side, uint Voice)
    {
        GROUP_DATA gd = new GROUP_DATA();
        gd.Flags = flags;
        gd.AI = AI;
        gd.AiScript = AiScript == null ? "" : AiScript;
        gd.Callsign = Callsign;
        gd.Side = Side;
        gd.Voice = Voice;
        gd.ID = Hasher.HshString(gd.Callsign);

        return gd;
    }

    public static void PatchLayouts()
    {
        AddTurretData();
        //OBJECT_DATA OD = OBJECT_DATA.GetByName("Human Avia Escort Vessel");
        //Debug.Log("Patching OBJECT_DATA " + OD);

        //OD.Layouts.Add(GenLayout("Radars"));
        //OD.Layouts.Add(GenLayout("Missiles"));
        //OD.Layouts.Add(GenLayout("Torpedoes"));
        //OD.Layouts.Add(GenLayout("Velian Radars"));
        //OD.Layouts.Add(GenLayout("HS Radar 100000m special"));
        AddUnitLayouts("Human Avia Escort Vessel",
            new LAYOUT_DATA[] {
                GenLayout("Radars"),
                GenLayout("Missiles"),
                GenLayout("Torpedoes"),
                GenLayout("Velian Radars"),
                GenLayout("HS Radar 100000m special")
            }
        );
    }

    public static void AddUnitLayouts(string UnitName, LAYOUT_DATA[] layouts)
    {
        OBJECT_DATA OD = OBJECT_DATA.GetByName(UnitName);
        if (OD == null)
        {
            Debug.LogFormat("OBJECT_DATA {0} for unit {1} not found", OD == null ? "NOT_FOUND" : OD, UnitName);
            return;
        }
        foreach (var layout in layouts)
        {
            OD.Layouts.Add(layout);
        }
        Debug.LogFormat("Patched OBJECT_DATA for unit {1}, result: {0}", OD.FullName, UnitName);
    }

    public static LAYOUT_DATA GenLayout(string name)
    {
        LAYOUT_DATA newLayout = new LAYOUT_DATA();
        newLayout.Type = LAYOUT_DATA.SLOTS_LAYOUT;
        newLayout.Name = Hasher.HshString(name);
        newLayout.FullName = name;
        newLayout.Items = new List<LAYOUT_ITEM>();
        switch (name)
        {
            case "Radars":
                newLayout.Items.Add(new LAYOUT_ITEM("hs_htrs", "HS Radar 100000m"));
                newLayout.Items.Add(new LAYOUT_ITEM("hs_aahp", "HS Radar 100000m"));
                newLayout.Items.Add(new LAYOUT_ITEM("hradar1", "HS Radar 100000m"));
                newLayout.Items.Add(new LAYOUT_ITEM("hs_aamp", "HS Radar 100000m"));
                break;
            case "Missiles":
                newLayout.Items.Add(new LAYOUT_ITEM("hs_htrs", "HS AAC Heavy Rocket Turret"));
                newLayout.Items.Add(new LAYOUT_ITEM("hs_aahp", "HS AAC Heavy Rocket Turret"));
                newLayout.Items.Add(new LAYOUT_ITEM("hradar1", "HS AAC Heavy Rocket Turret"));
                newLayout.Items.Add(new LAYOUT_ITEM("hs_aamp", "HS AAC Heavy Rocket Turret"));
                break;
            case "Torpedoes":
                newLayout.Items.Add(new LAYOUT_ITEM("hs_htrs", "Torpedo launcher"));
                newLayout.Items.Add(new LAYOUT_ITEM("hs_aahp", "Torpedo launcher"));
                newLayout.Items.Add(new LAYOUT_ITEM("hradar1", "Torpedo launcher"));
                newLayout.Items.Add(new LAYOUT_ITEM("hs_aamp", "Torpedo launcher"));
                break;
            case "Velian Radars":
                newLayout.Items.Add(new LAYOUT_ITEM("hs_htrs", "VS Radar A"));
                newLayout.Items.Add(new LAYOUT_ITEM("hs_aahp", "VS Radar A"));
                newLayout.Items.Add(new LAYOUT_ITEM("hradar1", "VS Radar A"));
                newLayout.Items.Add(new LAYOUT_ITEM("hs_aamp", "VS Radar A"));
                break;
            case "HS Radar 100000m special":
                newLayout.Items.Add(new LAYOUT_ITEM("hs_htrs", "HS Radar 100000m special"));
                newLayout.Items.Add(new LAYOUT_ITEM("hs_aahp", "HS Radar 100000m special"));
                newLayout.Items.Add(new LAYOUT_ITEM("hradar1", "HS Radar 100000m special"));
                newLayout.Items.Add(new LAYOUT_ITEM("hs_aamp", "HS Radar 100000m special"));
                break;
            default:
                return null;
        }
        return newLayout;
    }

    public static void AddTurretData()
    {
        TURRET_DATA TD = new TURRET_DATA("Torpedo launcher");
        //SUBOBJ_DATA
        TD.UnitDataIndex = UnitDataTable.GetIdxByName("HTA");
        TD.FileName = "hs_aahp";
        TD.SetFlag(SUBOBJ_DATA.SF_DETACHED);
        TD.DeltaY = 3.27f;
        TD.Armor = 4000;
        TD.Debris = (DEBRIS_DATA)DEBRIS_DATA.GetByCode(Hasher.HshString("TankTurret_debris"));
        TD.DetachedDebris = (DEBRIS_DATA)DEBRIS_DATA.GetByCode(Hasher.HshString("TankTurret_debris"));
        //TURRET_DATA
        //TD.WeaponData = (WPN_DATA)WPN_DATA.GetByCode(Hasher.HshString("AAAPC"));
        TD.WeaponData = (WPN_DATA)WPN_DATA.GetByCode(Hasher.HshString("TGM"));
        TD.SpeedX = Storm.Math.GRD2RD(60); //TurretSpeed
        TD.SpeedY = Storm.Math.GRD2RD(90); //GunSpeed
        TD.MinY= Storm.Math.GRD2RD(0); //TurretMin
        TD.MaxY = Storm.Math.GRD2RD(0); //TurretMax
        TD.MinX = Storm.Math.GRD2RD(-25); //GunMin
        TD.MaxX = Storm.Math.GRD2RD(85); //GunMax
        TD.GunNamesList = new List<string>
        {
            "gunA",
            "gunB",
            "gunC",
            "gunD"
        };

        TURRET_DATA.Datas.Add(TD);
    }
}

public class UnitPatch
{
    public void CreateUnit()
    {
        STATIC_DATA sdata = new STATIC_DATA("AMS bunker");
        sdata.Avoidable = true;
        sdata.Angle = 15;
    }

    public void CreateTurret()
    {
        TURRET_DATA tData = new TURRET_DATA("AMS turret");

        //SUBOBJ_DATA
        tData.UnitDataIndex = UnitDataTable.GetIdxByName("HTR");
        tData.FileName = "hs_rocket";
        tData.SetFlag(SUBOBJ_DATA.SF_DETACHED);
        tData.DeltaY = 0.7f;
        tData.Armor = 4000;
        tData.Debris = (DEBRIS_DATA)DEBRIS_DATA.GetByCode(Hasher.HshString("TankTurret_debris"));
        tData.DetachedDebris = (DEBRIS_DATA)DEBRIS_DATA.GetByCode(Hasher.HshString("TankTurret_debris"));
        //TURRET_DATA
        tData.WeaponData = (WPN_DATA)WPN_DATA.GetByCode(Hasher.HshString("TGM"));
        tData.SpeedX = Storm.Math.GRD2RD(60);
        tData.SpeedY = Storm.Math.GRD2RD(90);
        //tData.MinY= Storm.Math.GRD2RD(90);
        //tData.MaxY = Storm.Math.GRD2RD(90);
        tData.MinX = Storm.Math.GRD2RD(-25);
        tData.MaxX = Storm.Math.GRD2RD(85);

    }
}


public class MissionPatch
{
    public enum Maps
    {
        CONTINTENT,
        NetArena
    }
    public static Vector3 GetCoords(Maps map, string p)
    {
        string key = map.ToString() + "#" + p.ToUpper();
        if (POIList.ContainsKey(key)) return POIList[key];
        Debug.LogFormat("Key {0} not found", key);
        return Vector3.zero;
    }

    public static void AddCoords(Maps map, string p, Vector3 coords)
    {
        string key = map.ToString() + "#" + p.ToUpper();
        if (POIList.ContainsKey(key)) POIList.Remove(key);
        POIList.Add(key, coords);
    }

    public static void InitPOIs()
    {
        if (POIList == null) POIList = new Dictionary<string, Vector3>();
        POIList.Clear();
        AddCoords(CONTINTENT, "TICHY BASE", new Vector3(44418, 200, 32000)); //Над ФУТЦ "Озеро Тихого"
        AddCoords(CONTINTENT, "TICHY LAKE", new Vector3(44000, 500, 36456)); //над озером Тихого
        AddCoords(CONTINTENT, "TICHY RUNWAY", new Vector3(46205.34f, 93.88835f, 33152.31f)); //  ВПП ФУТЦ "Озеро Тихого"
        AddCoords(CONTINTENT, "TICHY LAKE RIDGE", new Vector3(45800f, 448.7f, 33800)); //южный берег озера Тихого рядом с ВПП
        AddCoords(CONTINTENT, "TRAINING CENTER", new Vector3(45970.796875f, 200, 32849.61328125f));
        AddCoords(CONTINTENT, "TRAINING CENTER FOREST", new Vector3(42832, 0, 31000));//Лес на западной стороне ФУТЦ "Озеро Тихого"
        AddCoords(CONTINTENT, "FLEET OSCAR", new Vector3(206208f, 200, 101760f)); //Место Флота Оскар в кампании ОЩ
        AddCoords(CONTINTENT, "NEW AVALON", new(134195.671875f, 200f, 357748.5f)); //Город Новый Авалон
        AddCoords(CONTINTENT, "NORTH CITADEL", new Vector3(155504f, 1000f, 362112f)); //портал в Северной цитадели

        AddCoords(NetArena, "BASE TANGO", new Vector3(26854f, 0, 22146f)); //База Танго в IA-2
        AddCoords(NetArena, "SPAWN A", new Vector3(29556f, 0, 25886f)); //Точка спавна А в IA-2
        AddCoords(NetArena, "SPAWN B", new Vector3(29648f, 0, 22682f)); //Точка спавна B в IA-2
        AddCoords(NetArena, "SPAWN C", new Vector3(27456f, 0, 18132f)); //Точка спавна C в IA-2
        AddCoords(NetArena, "SPAWN D", new Vector3(25364f, 0, 20444f)); //Точка спавна D в IA-2
    }
    public GROUP_DATA[] Groups;
    public static Dictionary<string, Vector3> POIList;
    public Vector3 posTichyBase => GetCoords(CONTINTENT, "Tichy Base");
    public Vector3 posTichyLake => GetCoords(CONTINTENT, "Tichy Lake");
    public Vector3 posTichyRunway => GetCoords(CONTINTENT, "Tichy Runway");
    public Vector3 posTichyLakeRidge => GetCoords(CONTINTENT, "Tichy Lake Ridge");
    public Vector3 posTrainingCenter => GetCoords(CONTINTENT, "Training Center");
    public Vector3 posTrainingCenterForest => GetCoords(CONTINTENT, "Training Center Forest");
    public Vector3 posFleetOscar => GetCoords(CONTINTENT, "Fleet Oscar");
    public Vector3 posNewAvalon => GetCoords(CONTINTENT, "New Avalon");

    //public static Vector3 posTichyBase = new Vector3(44418, 200, 32000);//Над ФУТЦ "Озеро Тихого"
    //public static Vector3 posTichyLake = new Vector3(44000, 500, 36456); //над озером Тихого
    //public static Vector3 posTichyRunway = new Vector3(46205.34f, 93.88835f, 33152.31f); //  ВПП ФУТЦ "Озеро Тихого"
    //public static Vector3 posTichyLakeRidge = new Vector3(45800f, 448.7f, 33800); //южный берег озера Тихого рядом с ВПП
    //public static Vector3 posTrainingCenter = new Vector3(45970.796875f, 200, 32849.61328125f);
    //public static Vector3 posTrainingCenterForest = new Vector3(42832, 0, 31000);//Лес на западной стороне ФУТЦ "Озеро Тихого"
    //public static Vector3 posFleetOscar = new Vector3(206208f, 200, 101760f); //Место Флота Оскар в оригинальном "Шторме"
    //public static Vector3 posNewAvalon = new(134195.671875f, 200f, 357748.5f); //Город Новый Авалон
    //public static Vector3 posNorthCitadel = new Vector3(155504f, 1000f, 362112f); //портал в Северной цитадели


    public MissionPatch(Maps map)
    {
        InitPOIs();
        MissionPatchFactory.PatchLayouts();

        switch (map)
        {
            case CONTINTENT:
                Groups = GenPatchGroupsDataContinent();
                break;
            case NetArena:
                Groups = GenPatchGroupsDataNetArena();
                break;
            default:
                break;
        }
    }
    public MissionPatch() : this(NetArena)
    {
    }

    private POINT_DATA[] GenZeroPathContinent()
    {
        POINT_DATA[] res =
        {
            new POINT_DATA(posTichyBase,0,null),
            new POINT_DATA(posTichyRunway,0,null)
        };
        return res;
    }

    private POINT_DATA[] GenZeroPathNetArena()
    {
        POINT_DATA[] res =
        {
            new POINT_DATA(GetCoords(NetArena,"BASE TANGO"),0,null),
            new POINT_DATA(GetCoords(NetArena,"SPAWN A"),0,null),
            new POINT_DATA(GetCoords(NetArena,"SPAWN B"),0,null),
            new POINT_DATA(GetCoords(NetArena,"SPAWN C"),0,null),
            new POINT_DATA(GetCoords(NetArena,"SPAWN D"),0,null),
            new POINT_DATA(GetCoords(NetArena,"BASE TANGO"),0,null),
        };
        return res;
    }

    public GROUP_DATA[] GenPatchGroupsDataNetArena()
    {
        //MissionPatchFactory.PatchLayouts();
        List<GROUP_DATA> result = new List<GROUP_DATA>
        {
            //MissionPatchFactory.GetEscortGroup("Velian Radar escort I", CampaignDefines.CS_SIDE_VELIANS, GetCoords(NetArena,"BASE TANGO"), 90, "HS Radar 100000m special", GenZeroPathNetArena()),
            MissionPatchFactory.GetEscortGroup("Velian Torpedo destroyer", CampaignDefines.CS_SIDE_VELIANS, GetCoords(NetArena,"BASE TANGO"), 90, "Torpedoes", GenZeroPathNetArena()),
            //MissionPatchFactory.GetEscortGroup("Ushat Pomoev", CampaignDefines.CS_SIDE_VELIANS, GetCoords(NetArena,"BASE TANGO"), 90, "Turrets Light", GenZeroPathNetArena()),
            MissionPatchFactory.GetBunkerGroup("Bunker Heavy", CampaignDefines.CS_SIDE_HUMANS, GetCoords(NetArena,"BASE TANGO") + new Vector3(-500,0,900), 90, "AA Rockets & Radar 15000m"),
        };
        return result.ToArray();
    }
    public GROUP_DATA[] GenPatchGroupsDataContinent()
    {
        //MissionPatchFactory.PatchLayouts();


        List<GROUP_DATA> result = new List<GROUP_DATA>
        {
            //MissionPatchFactory.GetEscortGroup("Ulov Nalimov", CampaignDefines.CS_SIDE_VELIANS, posTichyBase, "Turrets");
            //MissionPatchFactory.GetEscortGroup("Velian missile escort", CampaignDefines.CS_SIDE_VELIANS, posTichyLake + Vector3.forward * 100 + Vector3.up * 500, 90, "Missiles");
            MissionPatchFactory.GetEscortGroup("Velian Radar escort I", CampaignDefines.CS_SIDE_VELIANS, posTichyLake + Vector3.forward * 100 + Vector3.up * 500, 90, "HS Radar 100000m special", GenZeroPathContinent()),
            //MissionPatchFactory.GetEscortGroup("Federation radar escort", CampaignDefines.CS_SIDE_HUMANS, posTichyLake + Vector3.right * 1000, 0, "Radars");
            MissionPatchFactory.GetEscortGroup("Federation radar escort", CampaignDefines.CS_SIDE_HUMANS, posTichyLake + Vector3.right * 1000, 0, "HS Radar 100000m special", new POINT_DATA[] { new POINT_DATA(posFleetOscar, 0, null), new POINT_DATA(posTrainingCenter, 0, null) }),
            MissionPatchFactory.GetEscortGroup("Velian Radar escort II", CampaignDefines.CS_SIDE_VELIANS, posTichyLake + Vector3.right * 2000, 0, "Velian Radars", GenZeroPathContinent()),
            MissionPatchFactory.GetTurretGroup("Turret group 1", CampaignDefines.CS_SIDE_HUMANS, posTichyLakeRidge, 0, null),
            MissionPatchFactory.GetSHABORGroup("Shabor", CampaignDefines.CS_SIDE_HUMANS, posTrainingCenterForest, 0, null,new POINT_DATA[] {new POINT_DATA(posNewAvalon, 0, null) })

        };

        return result.ToArray();

        //return new GROUP_DATA[] { MissionPatchFactory.GetEscortGroup("Ulov Nalimov", CampaignDefines.CS_SIDE_VELIANS, pos, "Turrets") };
        //return new GROUP_DATA[] { MissionPatchFactory.GetEscortGroup("Ulov Nalimov", CampaignDefines.CS_SIDE_VELIANS, pos, "Turrets Light") };
        //return new GROUP_DATA[] { MissionPatchFactory.GetEscortGroup("Ulov Nalimov", CampaignDefines.CS_SIDE_VELIANS, pos, "Turrets Rockets & Radar 10000m") };
        //return new GROUP_DATA[] { MissionPatchFactory.GetBunkerGroup("Ushat Pomoev", CampaignDefines.CS_SIDE_VELIANS, pos, "AA Rockets & Radar 15000m") };
        //return new GROUP_DATA[] { MissionPatchFactory.GetTurretGroup("Parad Urodov", CampaignDefines.CS_SIDE_VELIANS, pos) };
        //return new GROUP_DATA[] { MissionPatchFactory.GetMissileTurretGroup("Parad Urodov", CampaignDefines.CS_SIDE_VELIANS, pos) };
        //return new GROUP_DATA[] { MissionPatchFactory.GetNuclearExplGroup("Parad Urodov", CampaignDefines.CS_SIDE_VELIANS, pos) };
    }
}
