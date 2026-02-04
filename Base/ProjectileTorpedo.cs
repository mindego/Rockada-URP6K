using UnityEngine;
using static iSensorsDefines;
using DWORD = System.UInt32;


/// <summary>
/// Набросок класса-прототипа (он же - торпеда) для дополнительных типов ракет
/// </summary>
public class ProjectileTorpedo : ProjectileMissile
{
    public ProjectileTorpedo(BaseScene s, uint h) : base(s, h) { }

    public override bool Move(float scale)
    {
        // object movement
        float p = wpndata.GetSpeed() - speedf;
        float l = wpndata.GetAccel() * scale;
        speedf += (p < l ? p : l);
        speed = pos.Dir * speedf;
        bool ret = ProcessTrace(scale) == null;
        if (ret)
        {
            // перемещение
            pos.Org += speed * scale;
            if (mpFPO != null) { mpFPO.Set(pos); rScene.UpdateHM(mpHash); }
            ret = ProcessTimer(scale);
        }
        if (ret == false)
            setActiveMissileEnabled(false);
        return ret;
    }
}

public class ProjectileGuidedMissile : ProjectileTorpedo
{
    public ProjectileGuidedMissile(BaseScene s, uint h) : base(s, h) { }

    public override bool Move(float scale)
    {
        // проверка видимости цели
        if (mpTarget.Ptr() != null)
        {
            if (mpTarget.Ptr().GetState() == CS_DEAD || mpTarget.Ptr().GetAge() > .0f || mpTarget.Ptr().IsOnlyVisual())
            {
                //rScene.Message("State %d Age %f OnlyVisual %d",mpTarget->GetState(),mpTarget->GetAge(),mpTarget->IsOnlyVisual());
                ZeroTarget(/*"Target DEAD or  Target Age > 0 or Target Only Visual"*/);
            }
        }
        // если в поле
        if (rScene.IsInSfg(pos.Org) == true)
            ZeroTarget(/*"InSfg"*/);
        // взрыв, если цель потеряна
        if (mpTarget.Ptr() == null && Remote == false && timer > GetMissileData().TriggerTime)
            setTimer(GetMissileData().TriggerTime);
        // наведение
        if (mpTarget.Ptr() != null && timer < GetMissileData().LifeTime - GetMissileData().TriggerTime)
        {
            Vector3 Dif;
            if (Storm.Math.NormaFAbs(mTargetDir) == 0)
            {
                Dif = mpTarget.Ptr().GetOrg() - pos.Org;
                float dist = Dif.magnitude;
                mpTarget.Ptr().SetThreat(GetHandle(), Mathf.Clamp(2 - dist * 0.001f, 0.1f, 2));
                MakeAim(ref Dif, ref dist);
                dist = Dif.magnitude;
                if (dist != 0) Dif /= dist;

                float acc_time = (GetMissileData().MaxSpeed - speedf) / wpndata.GetAccel();
                float acc_len = speedf * acc_time + wpndata.GetAccel() * Mathf.Pow(acc_time, 2) * 0.5f;
                if (acc_len > dist)
                {
                    Vector2 solve;
                    if (Storm.Math.solveSquare(wpndata.GetAccel() * 0.5f, speedf, -dist, out solve))
                        acc_time = solve[solve.y > 0 ? 1 : 0];
                }
                else
                    acc_time += (dist - acc_len) / GetMissileData().MaxSpeed;

                float tlim = Mathf.Clamp(
                    GetMissileData().ProximityTime * (2f - mpTarget.Ptr().GetSensorsVisibility()),
                    sMinProximityTime, sMaxProximityTime);

                if (acc_time < tlim)
                {
                    mTargetDir = Dif;
                    setTimer(acc_time);
                }
            }
            else
            {
                Dif = mTargetDir;
                mpTarget.Ptr().SetThreat(GetHandle(), 2.0001f);
            }
            // turn to target
            float max_Angle = sCornerSpeed * scale;
            pos.TurnRightPrec(Mathf.Clamp(Mathf.Asin(Vector3.Dot(pos.Right, Dif)), -max_Angle, max_Angle));
            pos.TurnUpPrec(Mathf.Clamp(Mathf.Asin(Vector3.Dot(pos.Up, Dif)), -max_Angle, max_Angle));
        }

        return base.Move(scale);
    }
}
