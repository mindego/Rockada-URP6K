using System;
using UnityEngine;
using DWORD = System.UInt32;

public class RadarData : TLIST_ELEM<RadarData>, IDisposable
{
    public enum TYPE { LINE, WAYPOINT, STATIC, CRAFT, TANK, WEAPON, SEASHIP, AIRSHIP, TURET, GPS, ROCKET };
    public Vector3 Org;
    public float Angle;
    public DWORD ColorIndex;
    public TYPE Type;
    public float X1, X2;
    public float Z1, Z2;
    //Эмуляция Union
    public float MinX
    {
        get
        {
            return X1;
        }
    }
    public float MinZ
    {
        get
        {
            return Z1;
        }
    }
    public float MaxX
    {
        get
        {
            return X2;
        }
    }
    public float MaxZ
    {
        get
        {
            return Z2;
        }
    }

    public override string ToString()
    {
        string res = GetType().ToString() + " " + Type + " @ " + Org + " angle " + Angle;
        return res;

    }
    //union
    //{
    //    float MinX;
    //    float X1;
    //};
    //union
    //{
    //    float MinZ;
    //    float Z1;
    //};
    //union
    //{
    //    float MaxX;
    //    float X2;
    //};
    //union
    //{
    //    float MaxZ;
    //    float Z2;
    //};
    public RadarData(Vector3 _Org, float _Angle, DWORD cidx, TYPE _Type)
    {
        Org = (_Org);
        Angle = (_Angle);
        ColorIndex = (cidx);
        Type = (_Type);
        //MinX(0.),MinZ(0.),MaxX(0.),MaxZ(0.)
        X1 = 0f;
        X2 = 0f;
        Z1 = 0f;
        Z2 = 0f;
    }
    public RadarData(Vector3 _Org, float _Angle, DWORD cidx, float _MinX, float _MinZ, float _MaxX, float _MaxZ)
    {
        Org = (_Org);
        Angle = (_Angle);
        ColorIndex = (cidx);
        Type = TYPE.STATIC;
        X1 = (_MinX);
        Z1 = (_MinZ);
        X2 = _MaxX;
        Z2 = (_MaxZ);
    }

    public RadarData(DWORD cidx, float _X1, float _Z1, float _X2, float _Z2)
    {
        Org = Vector3.zero;
        Angle = (0);
        ColorIndex = cidx;
        Type = (TYPE.LINE);
        X1 = (_X1);
        Z1 = (_Z1);
        X2 = (_X2);
        Z2 = (_Z2);
    }

    RadarData next, prev;
    public RadarData Next()
    {
        return next;
    }

    public RadarData Prev()
    {
        return prev;
    }

    public void SetNext(RadarData t)
    {
        next = t;
    }

    public void SetPrev(RadarData t)
    {
        prev = t;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}