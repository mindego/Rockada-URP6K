using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// BaseCraftAutopilotBattle - боевое маневрирование
/// </summary>
class BaseCraftAutopilotBattle : BaseCraftAutopilot
{
    // для кверенья
    new public const uint ID = 0xA23E0C83;
    public override object GetInterface(DWORD id)
    {
        return (id == ID) ? this : null;
    }

    // интерфейс с BaseCraft
    public BaseCraftAutopilotBattle(BaseCraft c) : base(c) { }
    public override bool Move(float scale, bool pred)
    {
        if (Owner.GetCondition() <= .0f) return base.Move(scale, pred);
        Vector3 spd = Owner.GetSpeedInLocal();
        Vector3 Faero = Owner.GetFaero(spd, scale);
        Vector3 td = TgtDir;
        Vector3 ts = Owner.pFPO.Right * TgtSpeed.x + Owner.pFPO.Up * TgtSpeed.y + Owner.pFPO.Dir * TgtSpeed.z;
        // вертикальная коррекция скорости
        if (pred == false) Owner.CorrectDirAndSpeed(ref td, ref ts, 4);
        // заканчиваем линейную часть
        Vector3 Treq = Owner.GetTreqForSpeed(Faero, ts);
        Vector3 ExtRotate = Owner.ApplyForces(Faero, Treq, spd, scale);
        { // поворот на цель
          // ограничиваем вектор в задней полусфере
            float d = Vector3.Dot(td, Owner.pFPO.Dir);
            if (d < 0)
            {
                td -= Owner.pFPO.Dir * d;
                d = td.magnitude;
                if (d > .005)
                {
                    if (Vector3.Dot(td, Owner.pFPO.Up) < 0) td = -td;
                    td /= d;
                }
                else
                {
                    td = Owner.pFPO.Up;
                }
            }
            // поворот на цель
            Vector3 Delta;
            // определяем дельту по вертикали
            Delta.x = Mathf.Asin(Vector3.Dot(td, Owner.pFPO.Up));
            // определяем дельту по горизонтали
            Delta.y = Mathf.Asin(Vector3.Dot(td, Owner.pFPO.Right));
            // определяем дельту по крену
            Delta.z = Mathf.Clamp(Owner.RollAngle * (Mathf.Abs(Delta.y) - 1) * .25f + Delta.y, -1, 1);
            // минимизируем их
            Owner.ApplyControlsDelta(Delta, ExtRotate, scale);
        }
        Owner.MakeRotation(spd, scale);
        Owner.MakeMove(scale, pred);
        Owner.CheckTerrainRough(myFlyDontUp, pred);
        return true;
    }
    public void Set(Vector3 dir, Vector3 spd) { TgtDir = dir; TgtSpeed = spd; }

    public override bool IsManual() { return false; }

    // свое
    protected Vector3 TgtDir;
    protected Vector3 TgtSpeed;
};


