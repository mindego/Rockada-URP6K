using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// BaseVehicleAutopilotFollow - полет за лидером
/// </summary>
class BaseVehicleAutopilotFollow : BaseVehicleAutopilotNonManual
{
    // для кверенья
    new public const uint ID = 0x87F812C2;
    public override object GetInterface(DWORD id)
    {
        switch (id)
        {
            case ID: return this;
            default: return base.GetInterface(id);
        }
    }

    // интерфейс с BaseVehicle
    public BaseVehicleAutopilotFollow(BaseScene sc, BaseVehicle c) : base(sc, c)
    {
        LeaderDist = 0;
    }
    ~BaseVehicleAutopilotFollow()
    {
        Dispose();
    }
    public override void Dispose()
    {
        if (isDisposed) return;
        rOwner.Pause(false);
        base.Dispose();
        isDisposed = true;
    }

    public override bool Move(float scale)
    {
        if (mLeader.Validate() == false)
        {
            Stop();
            return rOwner.ProcessPhysic(scale);
        }
        BaseVehicle lveh = GetLeaderVehicle();
        BaseVehicleAutopilotNonManual at = GetLeaderAutopilot(lveh);
        if (at != null)
            rOwner.Pause(at.rOwner.IsPaused());

        base.Move(scale);

        if (mpCurrentRouteContainer != null)
        {
            Vector3 ldir = mLeader.Ptr().GetOrg() - rOwner.pFPO.Org;
            LeaderDist = ldir.magnitude;

            float tt = (Vector3.Dot(ldir, mLeader.Ptr().GetDir()) / LeaderDist);

            // если мы не рядом с лидером 
            if (LeaderDist > FollowDist)
                mTargetSpeed = rOwner.GetVehicleData().MaxSpeed;
            else
            {
                if (tt < 0.7f) // если мы не сонаправлены то стоим
                    mTargetSpeed = 0f;
                else
                {
                    float df = (LeaderDist - FollowDist + 15f) * 0.04f;
                    if (df < 0.75f) df = 0.75f;
                    else if (df > 1f) df = 1f;
                    mTargetSpeed = lveh != null ? lveh.mCurSpeed * df : 0f;
                }
            }
        }
        RouteTo(mNowDest, mpCurrentRouteContainer != null ? mpCurrentRouteContainer.GetPrev() : null);
        // обсчет текущей тяги
        rOwner.mTargetThrust = mTargetSpeed * rOwner.GetVehicleData().OO_MaxSpeed;
        return rOwner.ProcessPhysic(scale);
    }
    public void SetFollow(iContact l, float dist)
    {
        Asserts.AssertBp(l != null);
        FollowDist = dist;
        mTime = 0;
        if (mLeader.Ptr() != l)
        {
            ClearAll();
            mLeader.setPtr(l);
            BaseVehicle lveh = GetLeaderVehicle();
            if (lveh != null)
            {
                BaseVehicleAutopilotNonManual at = GetLeaderAutopilot(lveh);
                if (at != null)
                    Synchronize(lveh, at);
                else
                    mLeader.setPtr(null);
            }
            else
                mLeader.setPtr(null);
        }
    }

    public override bool IsOnTheFormation()
    {
        return LeaderDist < FollowDist + 35f;
    }
    public override bool GetHeadAddingFlag(INavigationOrder ord)
    {
        if (mpIncoming == ord)
            return true;
        else
            return false;
    }

    public override void UploadRouteFromLeader(NavOrderContainer cont)
    {
        INavigationOrder order = cont.Order();
        ProcessIncoming(order);
    }
    // свое
    protected void Synchronize(BaseVehicle lveh, BaseVehicleAutopilotNonManual at)
    {
        //AssertBp(!mlOrders.Counter());
        at.RegisterWingman(this);
        // enumerate all leader containers
        Vector3 join_dst = Vector3.zero;
        bool join_set = false;
        NavOrderContainer nav = at.mlOrders.Head();
        if (nav != null)
        {
            ROADPOINT pnt = nav.GetLast();
            if (pnt != null)
            {
                join_dst = pnt.Pnt;
                join_set = true;
            }
        }

        for (; nav != null; nav = nav.Next())
        {
            NavOrderContainer self_cont = ProcessIncoming(nav.Order());
            if (self_cont != null)
                self_cont.SetLast(nav.GetLast());
        }


        if (!join_set)
            join_dst = mLeader.Ptr().GetOrg() - mLeader.Ptr().GetDir() * FollowDist;
        mpIncoming = rScene.GetNavigationApi().CalcOrder(rOwner.pFPO.Org, join_dst, true);
        mpIncoming.AddRef();
        rOwner.mReachedFlag = false;
    }
    protected float FollowDist;
    protected float LeaderDist;
};