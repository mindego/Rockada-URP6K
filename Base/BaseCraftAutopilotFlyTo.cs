using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// BaseCraftAutopilotFlyTo - полет к цели
/// </summary>
public class BaseCraftAutopilotFlyTo : BaseCraftAutopilot
{
    // для кверенья
    new public const uint ID = 0x5AC26910;
    public override object GetInterface(DWORD id)
    {
        return (id == ID ? this : null);
    }

    // интерфейс с BaseCraft
    public BaseCraftAutopilotFlyTo(BaseCraft c) : base(c)
    {
        TgtDir = Owner.pFPO.Dir;
        TgtSpeed = Vector3.zero;
        myFlyDontUp = c.rScene.GetTime() + 2;
    }
    public override bool Move(float scale, bool pred)
    {
        return DoMove(scale, Owner.AutopilotPredictionTime, pred, false);
    }
    public void Set(Vector3 dir, Vector3 spd) { TgtDir = dir; TgtSpeed = spd; }

    public override bool IsManual()
    {
        return false;
    }

    // свое
    public Vector3 TgtDir;
    protected Vector3 TgtSpeed;
    protected bool DoMove(float scale, float time, bool pred, bool SpeedInGlobal)
    {
        if (Owner.GetCondition() <= .0f) return base.Move(scale, pred);
        Vector3 spd = Owner.GetSpeedInLocal();
        Vector3 Faero = Owner.GetFaero(spd, scale);
        Vector3 td = TgtDir;
        Vector3 ts;
        // определяем требуемую скорость
        if (SpeedInGlobal) ts = TgtSpeed;
        else ts = Owner.pFPO.Right * TgtSpeed.x + Owner.pFPO.Up * TgtSpeed.y + Owner.pFPO.Dir * TgtSpeed.z;
        // если надо избегать террейна
        if (pred == false && time > .0f) Owner.CorrectDirAndSpeed(ref td, ref ts, time);    
        // заканчиваем линейную часть
        Vector3 Treq = Owner.GetTreqForSpeed(Faero, ts);
        //Debug.DrawRay(localpos, td * 100, Color.white);
        Vector3 ExtRotate = Owner.ApplyForces(Faero, Treq, spd, scale);

        { // поворот на цель
            Vector3 Delta;
            // определяем дельту по вертикали
            float dx = Mathf.Clamp(Mathf.Asin(td.y), -Owner.Dt().PitchLimit, Owner.Dt().PitchLimit) - Owner.PitchAngle;
            // определяем дельту по горизонтали
            float dy = Mathf.Atan2(td.x, td.z) - Owner.HeadingAngle;
            if (dy < -Storm.Math.PI) dy += Storm.Math.PI_2; else if (dy > Storm.Math.PI) dy -= Storm.Math.PI_2;
            // корректируем дельты с учетом крена
            if (Mathf.Abs(Owner.RollAngle) < Storm.Math.GRD2RD(15))
            {
                Delta.x = dx;
                Delta.y = dy;
            }
            else
            {
                float s, c; Storm.Math.SinCos(-Owner.RollAngle, out s, out c);
                Delta.x = dx * c - dy * s;
                Delta.y = dx * s + dy * c;
            }
            // определяем дельту по крену
            Delta.z = Mathf.Clamp(Owner.RollAngle * (Mathf.Abs(dy) - 1) * .25f + dy, -1, 1);
            // ограничиваем угол крена
            dx = Owner.RollAngle + Delta.z;
            if (dx > Owner.Dt().RollLimit) Delta.z = Owner.Dt().RollLimit - Owner.RollAngle;
            else
                if (dx < -Owner.Dt().RollLimit) Delta.z = -Owner.Dt().RollLimit - Owner.RollAngle;
            // минимизируем их
            Owner.ApplyControlsDelta(Delta, ExtRotate, scale);
        }
        Owner.MakeRotation(spd, scale);
        Owner.MakeMove(scale, pred);
        Owner.CheckTerrainRough(myFlyDontUp, pred);
        return true;
    }

    public override string Describe()
    {
        string res = "";
        res += "\nTgtDir: " + TgtDir;
        res += "\nTgtSpeed: " + TgtSpeed;

        return res;
            
    }
}


