using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// BaseCraftAutopilotManual - ручное управление
/// </summary>
public class BaseCraftAutopilotManual : BaseCraftAutopilot
{
    // для кверенья
    new public const uint ID = 0xA17CA573;
    public override object GetInterface(DWORD id)
    {
        return (id == ID ? this : null);
    }

    // интерфейс с BaseCraft
    public BaseCraftAutopilotManual(BaseCraft c) : base(c) { }
    public override bool Move(float scale, bool pred)
    {
        Vector3 spd = Owner.GetSpeedInLocal();
        Vector3 Faero = Owner.GetFaero(spd, scale);
        Vector3 Treq = Owner.GetTreqForThrust(Owner.Thrust, spd);
        if (Owner.Dt().IsPlane == false)
        {
            // автоматическое торможение по оси, если РУД отпущен
            if (Owner.Thrust.x == 0) Treq.x = -(Vector3.Dot(Owner.Speed, Owner.pFPO.Right)) * Owner.W;
            if (Owner.Thrust.z == 0) Treq.z = -(Vector3.Dot(Owner.Speed, Owner.pFPO.Dir)) * Owner.W;
            if (Owner.Thrust.y == 0)
            {
                Treq.y = -(Vector3.Dot(Owner.Speed, Owner.pFPO.Up)) * Owner.W;
                // компенсация силы тяжести
                if (Owner.pFPO.Up.y > .0f)
                    Treq.y += STORM_DATA.GAcceleration * Owner.W / Owner.pFPO.Up.y;
            }
        }
        Vector3 ExtRotate = Owner.ApplyForces(Faero, Treq, spd, scale);
        Owner.ApplyControls(Owner.Controls, ExtRotate, scale);
        Owner.MakeRotation(spd, scale);
        Owner.MakeMove(scale, pred);
        if (pred == true)
        {
            Owner.CheckTerrainRough(myFlyDontUp, true);
        }
        else Owner.CheckTerrainPrecise();
        // Debug.Log("\"Move\" phase Thrust " + Owner.Thrust + " controls " + Owner.Controls + " Treq " + Treq + " ThrustOut " + Owner.ThrustOut + " Speed " + Owner.Speed);
        return true;
    }
};


