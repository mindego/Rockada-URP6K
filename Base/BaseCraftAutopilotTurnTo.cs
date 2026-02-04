using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// BaseCraftAutopilotTurnTo - ручное управление тягой, автоматический поворот
/// </summary>
class BaseCraftAutopilotTurnTo : BaseCraftAutopilot
{
    // для кверенья
    new public const uint ID = 0x2141F0D8;
    public override object GetInterface(DWORD id)
    {
        return (id == ID ? this : null);
    }

    // интерфейс с BaseCraft
    public BaseCraftAutopilotTurnTo(BaseCraft c) : base(c)
    {
        mTgtDir = c.GetDir();
        mDirectMode = true;
    }
    public override bool Move(float scale, bool pred)
    {
        if (Owner.GetCondition() <= .0f) return base.Move(scale, pred);
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
        // угловая лабуда
        if (mDirectMode == false)
        { // псевдо-ручное управление
            Vector3 Delta;
            // определяем дельту по вертикали
            Delta.x = Mathf.Asin(Mathf.Cos(mRollAngle));
            // определяем дельту по горизонтали
            Delta.y = mRollAngle;
            // определяем дельту по крену
            Delta.z = mRollAngle;
            // минимизируем их
            Owner.ApplyControlsDelta(Delta, ExtRotate, scale);
            // пересчитываем новый угол крена
            mRollAngle += Vector3.Dot(Owner.CornerSpeed, Owner.pFPO.Dir) * scale;
        }
        else
        { // прямой поворот к цели
            Vector3 Delta;
            if (Vector3.Dot(mTgtDir, Owner.pFPO.Dir) < .0f)
            {
                Delta.y = (Vector3.Dot(mTgtDir, Owner.pFPO.Right) > .0f ? 1f : -1f);
                Delta.x = 1f;
            }
            else
            {
                Delta.y = Mathf.Asin(Vector3.Dot(mTgtDir, Owner.pFPO.Right));
                Delta.x = Mathf.Asin(Vector3.Dot(mTgtDir, Owner.pFPO.Up));
            }
            Delta.z = Mathf.Clamp(Owner.RollAngle * (Mathf.Abs(Delta.y) - 1f) * .25f + Delta.y, -1f, 1f);
            // минимизируем их
            Owner.ApplyControlsDelta(Delta, ExtRotate, scale);
        }
        Owner.MakeRotation(spd, scale);
        Owner.MakeMove(scale, pred);
        if (pred == true) Owner.CheckTerrainRough(myFlyDontUp, true); else Owner.CheckTerrainPrecise();
        return true;
    }
    public void Set(Vector3 dir, float Limit)
    {
        mLimitCos = Mathf.Cos(Mathf.Abs(Limit));
        mDirectMode = (Limit > .0f && Vector3.Dot(dir, Owner.pFPO.Dir) > mLimitCos);
        if (mDirectMode == true)
        {
            mTgtDir = dir;
        }
        else
        {
            float dx = Vector3.Dot(dir, Owner.pFPO.Right);
            float dz = Vector3.Dot(dir, Owner.pFPO.Up);
            mRollAngle = (Mathf.Abs(dx) + Mathf.Abs(dz) < .01f ? .0f : Mathf.Atan2(dx, dz));
        }
    }
    public Vector3 GetDir()
    {
        if (mDirectMode == true) return mTgtDir;
        float mLimitSin = Mathf.Sqrt(1f - Mathf.Pow(mLimitCos, 2));
        Vector3 r = new Vector3(mLimitSin * Mathf.Sin(mRollAngle), mLimitSin * Mathf.Cos(mRollAngle), mLimitCos);
        return Owner.pFPO.ProjectVector(r);
    }
    float GetLimit()
    {
        return Mathf.Acos(mLimitCos);
    }
    bool IsDirectMode() { return mDirectMode; }

    // свое
    protected Vector3 mTgtDir;
    protected float mLimitCos;
    protected float mRollAngle;
    protected bool mDirectMode;
};


