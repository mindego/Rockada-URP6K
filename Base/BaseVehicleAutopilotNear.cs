using UnityEngine;
using DWORD = System.UInt32;

public class BaseVehicleAutopilotNear : BaseVehicleAutopilotNonManual
{
    // для кверенья
    new public const uint ID = 0x39014CE8;
    public override object GetInterface(DWORD id)
    {
        switch (id)
        {
            case ID: return this;
            default: return base.GetInterface(id);
        }
    }

    // интерфейс с BaseVehicle
    public BaseVehicleAutopilotNear(BaseScene sc, BaseVehicle c) : base(sc, c) { }
    public override bool Move(float scale)
    {
        //Asserts.AssertBp(rOwner.mpAutopilot != (BaseVehicleAutopilot)(0xbaadf00d));
        if (mLeader.Validate() == false)
        {
            mTargetSpeed = 0;
            rOwner.mTargetThrust = 0;
            return rOwner.ProcessPhysic(scale);
        }
        Vector3 dlt;

        mNowDest = mLeader.Ptr().GetOrg() + mLeader.Ptr().GetRight() * FollowDelta.x + mLeader.Ptr().GetDir() * FollowDelta.z;
        mNowDest.y = rOwner.GetGroundLevelWithoutNormal(mNowDest, out dlt) - rOwner.GetMinY();
        Vector3 temp = mNowDest + rOwner.pFPO.Dir * 86f;
        if (!rScene.GetNavigationApi().SquareIsFree(mNowDest) || !rScene.GetNavigationApi().SquareIsFree(temp))
        {
            rOwner.FollowUnit(mLeader.Ptr(), FollowDist);
            return true;
        }
        BaseVehicle lveh = GetLeaderVehicle();
        dlt = mNowDest - rOwner.pFPO.Org;
        dlt.y = 0;
        float dlt_nrm = dlt.magnitude;
        float cosa = Vector3.Dot(dlt, rOwner.pFPO.Dir) / dlt_nrm;
        if (dlt_nrm < 50f)
        {     // если мы в близости от точки 
            if (dlt_nrm < 20f)
                mTargetSpeed = lveh.mCurSpeed;
            else
                mTargetSpeed = lveh.mCurSpeed * 0.75f;
        }
        else
        {
            mTargetSpeed = rOwner.GetVehicleData().MaxSpeed;
        }
        if (cosa < 0)
            mTargetSpeed = lveh.mCurSpeed * 0.3f;
        mNowDest += lveh.pFPO.Dir * 500f;
        RouteTo(mNowDest, null);
        rOwner.mTargetThrust = mTargetSpeed * rOwner.GetVehicleData().OO_MaxSpeed;
        return rOwner.ProcessPhysic(scale);
    }
    public override bool IsOnTheFormation()
    {
        return true;
    }
    public void SetNear(iContact l, float dist, Vector3 delta)
    {
        //AssertBp(l);
        if (mLeader.Ptr() != l)
            mLeader.setPtr(l);
        mLeader.Validate();
        if (mLeader.Ptr() != null)
        {
            FollowDelta = delta;
            FollowDist = dist;
            mTime = 0;
            BaseVehicleAutopilotNonManual at = GetLeaderAutopilot();
            if (at != null)
                at.RegisterWingman(this);
        }
    }

    // свое
    protected float FollowDist;
    protected Vector3 FollowDelta;
};
