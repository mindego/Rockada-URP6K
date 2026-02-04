using System.Collections.Generic;
using UnityEngine;
using DWORD = System.UInt32;

public class DEBRIS_SET
{
    public float MinSpeed;
    public float MaxSpeed;
    public Vector3 Coeff;
    //public List<DEBRIS_SET> Debrises;
    public List<DWORD> Debrises;
    public DEBRIS_SET()
    {
        MinSpeed = (0);
        MaxSpeed = (0);
        Coeff = Vector3.one;
        Debrises = new List<DWORD>();
    }
    public DEBRIS_SET(DEBRIS_SET dataref) : this()
    {
        MinSpeed = dataref.MaxSpeed;
        MaxSpeed = dataref.MaxSpeed;
        Coeff = dataref.Coeff;
        foreach (DWORD le in dataref.Debrises)
        {
            Debrises.Add(le);
        }

    }
    public void Create(SceneVisualizer rVis, MATRIX m)
    {
        foreach (var le in Debrises)
        {
            BaseDebris d = rVis.rScene.CreateBaseDebris(m, DEBRIS_DATA.GetByCode(le));

            Vector3 s;
            do
            {
                s = Distr.Sphere();
                s.x *= Coeff.x; if (Coeff.x < 0 && s.x < 0) s.x = -s.x;
                s.y *= Coeff.y; if (Coeff.y < 0 && s.y < 0) s.y = -s.y;
                s.z *= Coeff.z; if (Coeff.z < 0 && s.z < 0) s.z = -s.z;
            } while (Storm.Math.NormaFAbs(s) == 0);
            s.Normalize();
            s *= MinSpeed + (MaxSpeed - MinSpeed) * Storm.Math.norm_rand();
            d.SetSpeed(m.Right * s.x + m.Up * s.y + m.Dir * s.z);
        }
    }
};
