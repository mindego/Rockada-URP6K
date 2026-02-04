using UnityEngine;
/// <summary>
/// ProjectileLine - прямо летящее оружие
/// </summary>
class ProjectileLine : Projectile
{


    // ******************************************************************************************
    // от BaseActor
    public override bool Move(float scale)
    {
        if (ProcessTrace(scale)!=null) return false;
        pos.Org += speed * scale;
        return ProcessTimer(scale);
    }


    // ******************************************************************************************
    // создание\удаление
    public ProjectileLine(BaseScene _scene, WPN_DATA _wpndata, iContact _owner, Vector3 _org,Vector3 _dir,float add_speed,bool r,float t=0):base(_scene, _wpndata, _owner, _org, _dir, r)
    {
        speedf = wpndata.GetSpeed() + add_speed;
        speed = _dir * speedf;
        if (rScene.GetSceneVisualizer()!=null)
            visual = ProjectileVisual.Create(rScene.GetSceneVisualizer(), this, GetWeaponData());
        if (t > 0) timer = t;
    }

};
