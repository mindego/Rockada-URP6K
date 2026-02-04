using UnityEngine;
using DWORD = System.UInt32;
using static HudData;
using System.Collections.Generic;

public class HudRadarData : HudDeviceData
{
    public float range;
    public float angle;
    public Vector3 org;
    public float iconsize;
    public float wpt_period_on, wpt_period_off, wpt_size;
    public float linewidth;
    public new HudColors colors;
    int var_range;
    int var_angle;
    int var_org;
    int var_iconsize;
    int var_linewidth;
    int var_wpt_period_on;
    int var_wpt_period_off;
    int var_wpt_size;
    public HudRadarData(BaseScene pScene, DWORD iname = iRadar, string sname = sRadar) : base(pScene, iname, sname)
    {
        //TODO реализовать подписывание HudRadarData на события.
    }
    public override object OnVariable(uint code, object data)
    {
        //TODO Реализовать реагирование на подписанные события
        return base.OnVariable(code, data);
    }

    public TLIST<RadarData> Items = new TLIST<RadarData>();
    public void ClearList()
    {
        Items.Free();
    }
    public void AddStaticRadarItem(DWORD ColorIndex, float MinX, float MinZ, float MaxX, float MaxZ,Vector3 _Org,float _Angle)
    {
        Items.AddToHead(new RadarData(_Org, _Angle, ColorIndex, MinX, MinZ, MaxX, MaxZ));
    }
    public void AddDynamicRadarItem(RadarData.TYPE ObjectType, DWORD ColorIndex,Vector3 Org,float Angle)
    {
        Items.AddToTail(new RadarData(Org, Angle, ColorIndex, ObjectType));
    }
    public void AddRadarLine(DWORD ColorIndex, float X1, float Z1, float X2, float Z2)
    {
        Items.AddToTail(new RadarData(ColorIndex, X1, Z1, X2, Z2));
    }
    public float GetFormFactor()
    {
        if (w > h) return Mathf.Sqrt(1 + Mathf.Pow(w / h,2));
        else return Mathf.Sqrt(1 + Mathf.Pow(h / w,2));
    }
    ~HudRadarData()
    {
        ClearList();
    }
};
