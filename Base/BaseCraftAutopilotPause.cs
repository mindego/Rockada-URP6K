using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// BaseCraftAutopilotPause - пауза
/// </summary>
class BaseCraftAutopilotPause : BaseCraftAutopilotFlyTo
{
    // для кверенья
    new public const uint ID = 0x740271F0;
    public override object GetInterface(DWORD id)
    {
        return (id == ID ? this : null);
    }

    // интерфейс с BaseCraft
    public BaseCraftAutopilotPause(BaseCraft c) : base(c)
    {
        mPauseOrg = Owner.GetOrg();
        myFlyDontUp = c.rScene.GetTime() + 2;
    }
    public override bool Move(float scale, bool pred)
    {
        if (Owner.GetCondition() <= .0f) return base.Move(scale, pred);
        if (Owner.Dt().IsPlane == false)
        {
            TgtDir = Owner.GetDir(); TgtDir.y = 0;
            TgtSpeed = Vector3.zero;
        }
        else
        {
            TgtDir = mPauseOrg - Owner.GetOrg();
            TgtSpeed.Set(0, 0, 150);
        }
        if (Storm.Math.NormaFAbs(TgtDir) == 0) TgtDir = Vector3.forward; else TgtDir.Normalize();
        return base.Move(scale, pred);
    }

    // свое
    protected Vector3 mPauseOrg;
};


