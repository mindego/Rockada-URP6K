
using UnityEngine;
/// <summary>
/// Класс-заглушка, т.к. в оригинальном шторме HTGRов нет
/// </summary>
public class WeaponSystemForTurretWithHTGR : WeaponSystemForTurret
{
    public WeaponSystemForTurretWithHTGR(BaseTurret t) : base(t)
    {
    }

    public override iContact SelectTargetFromLocal(int nTargets, iContact[] Targets, Vector3[] Orgs, float[] TargetWeights, float MaxAge)
    {
        throw new System.NotImplementedException();
    }
}
