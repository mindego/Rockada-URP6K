using UnityEngine;
using static HudData;
/// <summary>
/// карта
/// </summary>
class CameraLogicMap : CameraLogic
{
    public const uint iCmMap = 0x6C525544; // "map"
    HudDevice pRadar;
    bool TrackingLost;
    CameraDataMap rMapData;
    CameraRadarEnumer pTargetsEnumer;
    public CameraLogicMap(CameraLogic prev) : base(prev)
    {
        TrackingLost = false;
        rMapData = rScene.GetSceneVisualizer().GetCameraDataMap();

        MapData md;
        md.Name = rScene.GetGameData().mpGameMapName;
        md.MapSizeX = rScene.GetGameData().mGameMapSizeX;
        md.MapSizeY = rScene.GetGameData().mGameMapSizeZ;
        md.SizeX = rScene.GetTerrain().GetXSize();
        md.SizeY = rScene.GetTerrain().GetZSize();
        pRadar = rVisual.GetHud().CreateMap(iMap, md);
        pRadar.Hide(false);
        pTargetsEnumer = new CameraRadarEnumer((HudRadarData)pRadar.GetData());
    }
    ~CameraLogicMap()
    {
        Dispose();
    }

    public override void Dispose()
    {
        pTargetsEnumer.Dispose();
        rVisual.GetHud().ReleaseDevice(pRadar);
    }

    /// <summary>
    /// обновиться
    /// </summary>
    /// <param name="scale"></param>
    public override void Update(float scale)
    {
        iContact u = rData.GetContact();
        iContact t = null;
        if (u != null)
        {
            iWeaponSystemDedicated w = (iWeaponSystemDedicated)u.GetInterface(iWeaponSystemDedicated.ID);
            if (w != null) t = w.GetTarget();
        }
        // двигаем камеру
        if (rData.GetCameraRightLeft() != 0)
        {
            TrackingLost = true;
            rData.myCamera.Org.x += ((rMapData.GetMapRange() * .25f * scale) * (rData.GetCameraRightLeft() / rData.GetCameraAngleSpeed()));
        }
        if (rData.GetCameraUpDown() != 0)
        {
            TrackingLost = true;
            rData.myCamera.Org.z += ((rMapData.GetMapRange() * .25f * scale) * (rData.GetCameraUpDown() / rData.GetCameraAngleSpeed()));
        }
        if (rData.GetCameraMoveZ() != 0)
            rMapData.SetMapRange(rMapData.GetMapRange() * (1.0f - scale * (rData.GetCameraMoveZ() / rData.GetCameraSpeed())));
        if (TrackingLost == false && u != null)
            if (u != null) rData.myCamera.Org = u.GetOrg();
        HudRadarData d = (HudRadarData)pRadar.GetData();
        d.range = rMapData.GetMapRange();
        d.org = rData.myCamera.Org;
        d.angle = 0;
        d.changed = true;
        pTargetsEnumer.Enumerate(u, t, (u != null ? (iSensors)u.GetInterface(iSensors.ID) : null));
        PlayerInterface pi;
        if ((pi = rVisual.GetPlayerInterface()) != null)
        {
            //Debug.Log("pi.GetInterface(BaseCraftController.ID) " + pi.GetInterface(BaseCraftController.ID));
            IBaseCraftController cc = (IBaseCraftController)pi.GetInterface(BaseCraftController.ID);
            Vector3 org = Vector3.zero;
            if (cc != null && WaypointSuggester.getWaypoint(cc, ref org) != 0)
                pTargetsEnumer.addWaypoint(org);
        }
        rVisual.Get3DSound().SetCamera(rData.myCamera, Vector3.zero, (int)SoundApiDefines.SM_EXTERN);
        /*
          // The Sample of set waypoints and line between waypoints
          d->AddRadarLine(0,0,0,5000,0);
          d->AddRadarLine(0,5000,0,10000,5000);
          d->AddRadarLine(0,10000,5000,10000,10000);

          d->AddDynamicRadarItem(RadarData::WAYPOINT,0,VECTOR(0,0,0),0);
          d->AddDynamicRadarItem(RadarData::WAYPOINT,0,VECTOR(5000,0,0),0);
          d->AddDynamicRadarItem(RadarData::WAYPOINT,0,VECTOR(10000,0,5000),0);
          d->AddDynamicRadarItem(RadarData::WAYPOINT,0,VECTOR(10000,0,10000),0);
          d->AddDynamicRadarItem(RadarData::WAYPOINT,0,VECTOR(20000,0,20000),0);
        */
    }
    // отрисовать сцену в очке
    public override void Draw(float[] Viewport)
    {
        rVisual.GetHud().Draw(Viewport);
    }
    // имя режима
    public override string GetName()
    {
        return "map";
    }
    public override CameraLogic GetCameraLogic(uint code)
    {
        return (code == iCmMap ? this : base.GetCameraLogic(code));
    }
}