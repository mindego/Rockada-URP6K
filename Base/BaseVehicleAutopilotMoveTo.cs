using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// BaseVehicleAutopilotMoveTo - езда к цели
/// </summary>
class BaseVehicleAutopilotMoveTo : BaseVehicleAutopilotNonManual
{

    protected readonly TContact Self = new TContact();
    protected float mCheckTimeTimer;

    // для кверенья
    new public const uint ID = 0xF294191B;
    public override object GetInterface(DWORD id)
    {
        switch (id)
        {
            case ID: return this;
            default: return base.GetInterface(id);
        }
    }

    // интерфейс с BaseVehicle
    public BaseVehicleAutopilotMoveTo(BaseScene sc, BaseVehicle c) : base(sc, c)
    {
        mCheckTimeTimer = -1f;
        //Self = new TContact((iContact)c.GetInterface(iContact.ID));
        Self.setPtr((iContact)c.GetInterface(iContact.ID));
    }
    public override bool Move(float scale)
    {
        if (Self.Validate() == false)
        {
            Self.setPtr(null);
            mlWingmans.Free();
        }
        mCheckTimeTimer -= scale;
        if (mCheckTimeTimer < 0)
        {
            mCheckTimeTimer = 2f;
            float dlt = mTime - rScene.GetTime();
            if (dlt <= 0)
                mTargetSpeed = rOwner.GetVehicleData().MaxSpeed;
            else
            {
                float dd = (mDest - rOwner.pFPO.Org).magnitude;
                mTargetSpeed = dd / dlt;
                if (dd > 100f)
                {
                    if (mTargetSpeed < Storm.Math.KPH2MPS(40f))
                        mTargetSpeed = Storm.Math.KPH2MPS(40f);
                }
                else
                    mTargetSpeed = rOwner.GetVehicleData().MaxSpeed;
            }
            APDebug("mTargetSpeed start : " + mTargetSpeed);
        }
        //Debug.Log(string.Format("{0} moving {1}->{2} {3}m left", rOwner.GetName(), rOwner.GetOrg(), mDest, (mDest - rOwner.pFPO.Org).magnitude));
        base.Move(scale);
        APDebug("mTargetSpeed after base.move: " + mTargetSpeed);

        RouteTo(mNowDest, mpCurrentRouteContainer != null ? mpCurrentRouteContainer.GetPrev() : null);
        float min_speed = rOwner.GetVehicleData().MaxSpeed;
        bool finded = false;

        for (BaseVehicleWingman man = mlWingmans.Head(); man!=null; man = man.Next())
        {
            if (min_speed > man.Wingman().rOwner.GetMaxSpeed())
                min_speed = man.Wingman().rOwner.GetMaxSpeed();
            if (!finded && !man.Wingman().IsOnTheFormation())
                finded = true;
        }

        if (finded)
        {      // someone not in formation
            min_speed = min_speed * 0.5f;
            if (min_speed < Storm.Math.KPH2MPS(20f))
                min_speed = Storm.Math.KPH2MPS(20f);
        }
        if (min_speed < mTargetSpeed)
            mTargetSpeed = min_speed;

        if (mpCurrentRouteContainer == null)
            mTargetSpeed = 0;

        rOwner.mTargetThrust = mTargetSpeed * rOwner.GetVehicleData().OO_MaxSpeed;

        return rOwner.ProcessPhysic(scale);
    }
    public override void OnFinishRoute()
    {
        APDebug("Finish route " + rOwner.mReachedFlag);
        //GetLog().Message("%p Checking self %f %f to dest %f %f",this,rOwner.pFPO->Org.x,rOwner.pFPO->Org.y,mDest.x,mDest.z);
        rOwner.mReachedFlag = CheckDest(mDest - rOwner.pFPO.Org, true);
        if (!rOwner.mReachedFlag)
        {
            //GetLog().Message("%p Not reached, calcing order dest %f %f, use roads %d",this,mDest.x,mDest.z,mUseRoads);
            mpIncoming = rScene.GetNavigationApi().CalcOrder(rOwner.pFPO.Org, mDest, mUseRoads);
            mpIncoming.AddRef();
        }
    }
    public override bool IsOnTheFormation()
    {
        return true;
    }
    // свое
    public override void OnSetRoute(Vector3 v, bool use_roads, float time)
    {
        base.OnSetRoute(v, use_roads, time);
        //#ifdef VISUALIZE_DIRS
        //  VECTOR start = v;
        //    start.y = rScene.GroundLevel(v.x, v.z);
        //  MATRIX m;
        //    m.Org = rOwner.pFPO->Org;
        //  m.Dir = start-m.Org;
        //  float nrm = m.Dir.Norma();
        //  if (nrm>0.01) 
        //    m.Dir/=nrm;
        //  else {
        //    m.Dir=VDir;
        //    nrm=1.f;
        //  }
        //m.Up = VUp;
        //m.Right = m.Up ^ m.Dir;
        //rOwner.mpCurOrder->SetParams(m, nrm, 0.2f, 0.2f, FVec4(1.f, 0.1f, 0.2f, 0.8f));
        //#endif
        rOwner.mReachedFlag = mlOrders.Counter() == 0;
        mDestRecieved = true;
    }
}