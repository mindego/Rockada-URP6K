using UnityEngine;
using static iSensorsDefines;
using static HudDataColors;
/// <summary>
/// radar enumerator for CameraLogics
/// </summary>
public class CameraRadarEnumer
{
    /// <summary>
    /// Friendly name
    /// </summary>
    private const uint CRAHEO_FN = 0x0001;
    /// <summary>
    /// Friendly type
    /// </summary>
    private const uint CRAHEO_FT = 0x0002;
    /// <summary>
    /// Friendly dist
    /// </summary>
    private const uint CRAHEO_FD = 0x0003;
    /// <summary>
    /// Enemy name
    /// </summary>
    private const uint CRAHEO_EN = 0x0010;
    /// <summary>
    /// Enemy type
    /// </summary>
    private const uint CRAHEO_ET = 0x0020;
    /// <summary>
    /// Enemy dist
    /// </summary>
    private const uint CRAHEO_ED = 0x0030;


    // own
    protected HudRadarData mpRadar;
    protected iContact mpSelf;
    protected iContact mpTarget;
    protected iContact mpTargetTop;

    protected virtual void AddRadarItem(iContact c, float r2, bool my)
    {
        if (c.GetState() == CS_DEAD || c.IsOnlyVisual() == true) return;
        int ColorIdx = (my ? HUDCOLOR_HUMAN : HUDCOLOR_VELIAN);
        if (c == mpSelf) ColorIdx = HUDCOLOR_MEDIUM;
        RadarData.TYPE rtype = RadarData.TYPE.WAYPOINT;
        if (c.GetInterface(BaseStatic.ID) != null)
        { // BaseStatic
            BaseStatic s = (BaseStatic)c.GetInterface(BaseStatic.ID);
            mpRadar.AddStaticRadarItem((uint)ColorIdx, s.GetMinX(), s.GetMinZ(), s.GetMaxX(), s.GetMaxZ(), c.GetOrg(), c.GetHeadingAngle());
        }
        else
        {
            if (c.GetInterface(BaseVehicle.ID) != null)
            { // BaseVehicle
                rtype = RadarData.TYPE.TANK;
            }
            else
            if (c.GetInterface(BaseCraft.ID) != null)
            { // BaseCraft
                rtype = RadarData.TYPE.CRAFT;
            }
            else
            if (c.GetInterface(SeaCarrier.ID) != null) //TODO - тут возможна ошибка. в классе SeaCarrier нет ID, берётся из BaseCarrier
            { // SeaCarrier
                rtype = RadarData.TYPE.SEASHIP;
            }
            else
            if (c.GetInterface(AirCarrier.ID) != null) // TODO - тут возможна ошибка. в классе AirCarrier нет ID, берётся из BaseCarrier
            { // AirCarrier
                rtype = RadarData.TYPE.AIRSHIP;
            }
            else
            if (c.GetInterface(ProjectileMissile.ID) != null)
            { // ProjectileMissile
                rtype = RadarData.TYPE.ROCKET;
            }
            mpRadar.AddDynamicRadarItem(rtype, (uint)ColorIdx, c.GetOrg(), c.GetHeadingAngle());
        }
        if (c == mpTargetTop)
            mpRadar.AddDynamicRadarItem(RadarData.TYPE.GPS, (uint)ColorIdx, c.GetOrg(), c.GetHeadingAngle());
    }
    // api

    public CameraRadarEnumer(HudRadarData p)
    {
        mpRadar = p;
    }

    ~CameraRadarEnumer()
    {
        Dispose();
    }
    public void Dispose() { }
    public void Enumerate(iContact self, iContact tgt, iSensors s, float look_back = 0)
    {
        mpRadar.ClearList();
        if (s == null) return;
        mpSelf = self;
        mpTarget = tgt;
        mpTargetTop = (mpTarget != null ? mpTarget.GetTopContact() : null);
        iContact c;
        float r = mpRadar.range * mpRadar.GetFormFactor() + 1000;
        // добавляем своих
        c = null;
        float Radius2 = Mathf.Pow(r, 2);
        float r2;
        //int cnt = 0;
        while ((c = s.GetFriend(c)) != null)
        {
            r2 = (c.GetOrg() - mpRadar.org).sqrMagnitude;
            //Debug.Log(string.Format("r2 {0} > Radius2 {1} ? {2} " + (cnt++),r2,Radius2,(r2>Radius2).ToString()));
            if (r2 > Radius2) continue;
            AddRadarItem(c, r2, true);
        }
        // добавляем врагов
        c = null;
        while ((c = s.GetEnemy(c)) != null)
        {
            if (c.GetSideCode() == 0 || c.GetAge() > look_back || c.IsOnlyVisual() == true) continue;
            r2 = (c.GetOrg() - mpRadar.org).sqrMagnitude;
            if (r2 > Radius2) continue;
            AddRadarItem(c, r2, false);
        }
    }
    public void addWaypoint(Vector3 org)
    {
        mpRadar.AddDynamicRadarItem(RadarData.TYPE.WAYPOINT, HUDCOLOR_NORMAL, org, 0);
    }
}
