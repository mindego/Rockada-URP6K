using UnityEngine;
using DWORD = System.UInt32;
// BaseCraftAutopilotFollow - полет за лидером
class BaseCraftAutopilotFollow : BaseCraftAutopilotFlyTo
{
    const float FollowCloseRange = 500f * 500f;
    // для кверенья
    new public const uint ID = 0xD9935FC7;
    public override object GetInterface(DWORD id)
    {
        return (id == ID ? this : null);
    }
    // интерфейс с BaseCraft
    public BaseCraftAutopilotFollow(BaseCraft c) : base(c)
    {
        myFlyDontUp = c.rScene.GetTime() + 2;
    }
    public override bool Move(float scale, bool pred)
    {
        bool LeaderValidated = Leader.Validate();
        if (Owner.GetCondition() <= .0f) return base.Move(scale, pred);
        float PredictionTime = Owner.AutopilotPredictionTime;
        if (LeaderValidated)
        {
            // куда лететь
            TgtSpeed = Leader.Ptr().GetOrg() - Owner.pFPO.Org;
            if (Leader.Ptr().GetInterface(BaseCraft.ID) != null)
            {
                TgtSpeed += Leader.Ptr().GetRight() * FollowDelta.x + Leader.Ptr().GetUp() * FollowDelta.y + Leader.Ptr().GetDir() * FollowDelta.z;
                TgtDir = TgtSpeed + Leader.Ptr().GetDir() * 500f; TgtDir.Normalize();
            }
            else
            {
                Vector3 v = Leader.Ptr().GetRight();
                if (Mathf.Abs(v.x) + Mathf.Abs(v.z) != 0) { v.y = 0; v.Normalize(); TgtSpeed += v * FollowDelta.x; }
                v = Leader.Ptr().GetDir();
                if (Mathf.Abs(v.x) + Mathf.Abs(v.z) != 0) { v.y = 0; v.Normalize(); TgtSpeed += v * FollowDelta.z; }
                TgtSpeed.y += FollowDelta.y;
                TgtDir = TgtSpeed + v * 500f; TgtDir.Normalize();
            }
            if (TgtSpeed.sqrMagnitude < FollowCloseRange)
            { // если близко
              // меньше смотрим на землю
                PredictionTime = 2.5f;
                // летим в строю
                TgtSpeed = Leader.Ptr().GetSpeed() + TgtSpeed;
            }
            else
            {
                TgtSpeed = Owner.pFPO.Dir * 1000f;
            }
        }
        return DoMove(scale, PredictionTime, pred, true);
    }
    public void Set(iContact l, Vector3 delta) { Leader.setPtr(l); FollowDelta = delta; }

    // свое
    protected readonly TContact Leader = new TContact();
    protected Vector3 FollowDelta;
};


