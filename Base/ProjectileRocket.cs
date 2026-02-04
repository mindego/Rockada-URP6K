using UnityEngine;
/// <summary>
/// ProjectileRocket - неуправляемые ракеты
/// </summary>
class ProjectileRocket : Projectile
{
    // от BaseActor
    public override bool Move(float scale)
    {
        // изменение скорости
        float d = Vector3.Dot(speed, pos.Dir);
        float p = GetWeaponData<WPN_DATA_ROCKET>().MaxSpeed - d;
        float l = GetWeaponData<WPN_DATA_ROCKET>().Accel * scale;
        if (p < -l) p = -l; else if (p > l) p = l;
        d += p;
        speed = pos.Right * RocketStrafeBrake(scale, Vector3.Dot(speed, pos.Right), GetWeaponData<WPN_DATA_ROCKET>().StrafeBrakeAccel) +
              pos.Up * RocketStrafeBrake(scale, Vector3.Dot(speed, pos.Up), GetWeaponData<WPN_DATA_ROCKET>().StrafeBrakeAccel) +
              pos.Dir * d;
        speedf = speed.magnitude;
        // проверка столкновения
        if (ProcessTrace(scale) != null) return false;
        // перемещение
        pos.Org += speed * scale;
        return ProcessTimer(scale);
    }

    // создание\удаление
    public ProjectileRocket(BaseScene _scene, WPN_DATA _wpndata, iContact _owner, Vector3 _org, Vector3 _dir, bool r) : base(_scene, _wpndata, _owner, _org, _dir, r)
    {
        // setup speeds 
        speed = owner.Ptr().GetSpeed();
        speedf = speed.magnitude;
        visual = ProjectileVisual.Create(rScene.GetSceneVisualizer(), this, GetWeaponData());
    }
    private WPN_DATA_ROCKET GetWeaponData<T>() where T : WPN_DATA_ROCKET { return (WPN_DATA_ROCKET)wpndata; }
    static float RocketStrafeBrake(float scale, float d, float acc)
    {
        if (d > .0f)
        {
            d -= acc * scale;
            return (d < .0f ? .0f : d);
        }
        else
        {
            d += acc * scale;
            return (d > .0f ? .0f : d);
        }
    }
};