#define VISUALIZE_PATHS
using UnityEngine;
using DWORD = System.UInt32;
using System.Collections.Generic;

public class BaseVehicleAutopilotNonManual : BaseVehicleAutopilot
{
    protected readonly TContact mLeader = new TContact();
    protected INavigationOrder mpIncoming;
    //public List<NavOrderContainer> mlOrders = new List<NavOrderContainer>();
    public TLIST<NavOrderContainer> mlOrders = new TLIST<NavOrderContainer>();
    public void ClearOrders()
    {
        mlOrders.Free();
        ClearCurrentOrder();
    }
    protected void ClearAll()
    {
        ClearIncoming();
        ClearOrders();
        //GetLog().Message("%p clearing all",this);
    }

    public NavOrderContainer mpCurrentRouteContainer;
    int mCurrentRouteDim;
    protected bool mDestRecieved;

    protected bool CheckDest(Vector3 _newdest, bool addon)
    {
        _newdest.y = 0f;
        float rad = mpPredictionInfo.reach_radius;
        if (addon)
            rad += rScene.GetNavigationApi().GetSquareSize() + 5f;
        if (_newdest.sqrMagnitude <= Mathf.Pow(rad, 2))  // prediction radius
            return true;
        else
            return false;
    }
    RouteParam GetRouteAngle(ref Vector3 dest1, MATRIX pos)
    {
        RouteParam param;
        Vector3 now_dir = pos.Dir;
        now_dir.y = 0;
        float cos_angle;
        cos_angle = now_dir.magnitude;
        dest1 = dest1 - pos.Org;
        dest1.y = 0;
        APDebug(string.Format("Should turn {0} deg {1}->{2}", Vector3.Angle(now_dir, dest1), now_dir, dest1));
        param.cos_dist = dest1.magnitude;
        if (RouteMath.CCmp(param.cos_dist))
            param.cos_angle = 1f;
        else
            param.cos_angle = Vector3.Dot(dest1, now_dir) / (cos_angle * param.cos_dist);
        //ROADPOINT* pnt =  mpCurrentRouteContainer?mpCurrentRouteContainer->GetLast():0;
        //rOwner.rScene.Message("%p norma %f cos_angle %f",pnt,param.cos_dist,param.cos_angle);
        //if (pnt)
        //    rOwner.rScene.Message("%f %f %d",pnt->Pnt.x,pnt->Pnt.z,pnt->Flags);
        //rOwner.rScene.Message(string.Format("{0} norma {1} cos_angle {1}", pnt, param.cos_dist, param.cos_angle));
        APDebug(string.Format("norma {1} cos_angle {1}", param.cos_dist, param.cos_angle));
        return param;
    }
    public float RouteTo(Vector3 now_dest, ROADPOINT prev_dest)
    {
        Vector3 cur_dir = now_dest;

        //#pragma message ("     EEI: здесь можно убрать кое-что после Bet-ы")
        //вычисляем направление на юнит с точки маршрута
        Vector3 dest_diff = rOwner.pFPO.Org - now_dest;
        //# ifdef VISUALIZE_DIRS
        //        MATRIX m = *rOwner.pFPO;
        //        rOwner.mpCurDir->SetParams(m, 100.f, 0.2f, 0.2f, FVec4(1.f, 0.1f, 0.2f, 0.8f));
        //        VECTOR tmp = -dest_diff;
        //        float tmp_len = tmp.Norma();
        //        if (!CCmp(tmp_len))
        //        {
        //            m.Dir = tmp / tmp_len;
        //            m.Right = m.Up ^ m.Dir;
        //            rOwner.mpCurDest->SetParams(m, tmp_len, 0.2f, 0.2f, FVec4(1.f, 0.8f, 0.1f, 0.3f));
        //        }
        //#endif
        dest_diff.y = 0;
        APDebug(string.Format("mNowDest {0}->{1}", prev_dest == null ? "HERE" : prev_dest, now_dest));
        EngineDebug.DebugLine(rOwner.GetOrg(), now_dest, Color.magenta);
        EngineDebug.DebugLine(rOwner.GetOrg(), rOwner.GetOrg() + rOwner.GetDir() * 50, Color.green);
        //Engine.DebugLine(rOwner.GetOrg(), rOwner.GetOrg() + dest_diff.normalized * 50, Color.yellow);
        EngineDebug.DebugLine(now_dest, now_dest + dest_diff.normalized * 50, Color.yellow);
        // считаем отклонение от маршрута
        if (prev_dest != null)
        {
            Vector3 prev_dest_diff = prev_dest.Pnt - now_dest;
            prev_dest_diff.y = 0;
            float path_dist = dest_diff.magnitude;
            if (path_dist > 10f)
            {
                float prev_len = prev_dest_diff.magnitude;
                if (!RouteMath.CCmp(prev_len))
                {
                    float cosf = Vector3.Dot(prev_dest_diff, dest_diff) / (path_dist * prev_len);
                    if (cosf > 0)
                    {
                        float d = path_dist * cosf;
                        path_dist = Mathf.Sqrt(Mathf.Pow(path_dist, 2) - Mathf.Pow(d, 2));
                        if (path_dist > rOwner.mPathDist)
                        {
                            prev_dest_diff /= prev_len;
                            prev_dest_diff *= (d - rOwner.mPathDist * 5f);
                            cur_dir += prev_dest_diff;
                        }
                    }
                }
            }
        }
        VehicleControlsData data = getDataFromVectors(cur_dir, rOwner.pFPO);
        EngineDebug.DebugLine(rOwner.GetOrg(), rOwner.GetOrg()+rOwner.GetRight() * data.myStick * 50, Color.blue);
        APDebug("Stick for vehicle: " + data.myStick);
        rOwner.SetStick(data.myStick);
        APDebug("Speed before GetThrustCoeff: " + mTargetSpeed);
        mTargetSpeed = GetThrustCoeff(data.myCosAngle, rOwner.last_ground_cos);
        APDebug("Speed set: " + mTargetSpeed);
        return data.myCosAngle;
    }

    const float CUT_COS_ANGLE = 0.9998f;
    public VehicleControlsData getDataFromVectors(Vector3 cur_dest, MATRIX pos)
    {
        VehicleControlsData data;
        RouteParam param = GetRouteAngle(ref cur_dest, pos);

        if (param.cos_angle > CUT_COS_ANGLE || param.cos_dist < 2f)
            data.myStick = 0;
        else
        {
            float t = 1f - param.cos_angle;
            if (t < 0.5f) t = 0.5f;
            float de = cur_dest.x * pos.Dir.z - cur_dest.z * pos.Dir.x;
            //rOwner.rScene.Message(string.Format("dir {0} {1}",de,(de<0)?"to left":"to right"));
            //APDebug(string.Format("dir {0} {1}", de, (de < 0) ? "to left" : "to right"));
            if (de < 0f) t *= -1f;
            data.myStick = t;
        }
        data.myCosAngle = param.cos_angle;
        return data;
    }

    // wingmans
    //public List<BaseVehicleWingman> mlWingmans = new List<BaseVehicleWingman>();
    public TLIST<BaseVehicleWingman> mlWingmans = new TLIST<BaseVehicleWingman>();
    public virtual void RegisterWingman(BaseVehicleAutopilotNonManual m)
    {
        for (BaseVehicleWingman wm = mlWingmans.Head(); wm!=null; wm = wm.Next())
            if (wm.Wingman() == m) return;
        mlWingmans.AddToTail(new BaseVehicleWingman(m));

    }
    public virtual void UnRegisterWingman(BaseVehicleAutopilotNonManual m)
    {
        for (BaseVehicleWingman wm = mlWingmans.Head(); wm!=null; wm = wm.Next())
            if (wm.Wingman() == m)
            {
                mlWingmans.Sub(wm);
                wm.Dispose();
                return;
            }

    }

    public virtual void UploadRouteFromLeader(NavOrderContainer c) { }
    public virtual bool GetHeadAddingFlag(INavigationOrder no) { return false; }

    void DoneCurrentOrder()
    {
        //NavOrderContainer head = mlOrders.Head();
        //mlOrders.Sub(head);
        //delete head;
        //SetCurrentOrder();
        //if (mlOrders[0] != null)
        //if (mlOrders.Counter() > 0)
        //{
        //    //Debug.Log(string.Format("Order done. mlOrders size {0}", mlOrders.Count));
        //    mlOrders.RemoveAt(0);
        //}
        //SetCurrentOrder();
        NavOrderContainer head = mlOrders.Head();
        mlOrders.Sub(head);
        head.Dispose();
        SetCurrentOrder();
    }

    void SetCurrentOrder()
    {
        if (mlOrders.Head()==null)
        {
            ClearCurrentOrder();
            return;
        }
        //Debug.Log(string.Format("Order {0} set for {1}", mlOrders[0].Order(),rOwner.GetVehicleData().FullName));
        mCheckDestTimer = -1f;
        mpCurrentRouteContainer = mlOrders.Head();
        INavigationOrder order = mpCurrentRouteContainer.Order();
        mCurrentRouteDim = order.GetRouteDimension();
        ROADPOINT[] buffer = mpCurrentRouteContainer.GetBuffer();
        ROADPOINT setted = mpCurrentRouteContainer.GetLast();
        if (mCurrentRouteDim > 0 && setted != null)
        {
            if (CheckDest(setted.Pnt - mNowDest, true))
                setted.Pnt = mNowDest;
            /*else 
              rOwner.rScene.Message("dist %f",df.Norma());*/
        }

        int setted_pos = mpCurrentRouteContainer.GetIndex(buffer, setted);

        for (int i = 0; i < mCurrentRouteDim; i++)
        {
            //ROADPOINT curp = buffer + i;
            ROADPOINT curp = buffer[i];
            //if (curp < setted) continue; //TODO Здесь сравниваются указатели, а не сами значениея
            if (i < setted_pos) continue;

            if (curp.GetFlag(RoadpointDefines.NAV_GROUND_LEVEL) == 0)
            {
                curp.Pnt.y += rOwner.GetGroundLevelWithoutNormal(curp.Pnt, out _) - rOwner.GetMinY();
                curp.SetFlag(RoadpointDefines.NAV_GROUND_LEVEL);
            }
            if (CheckDest(rOwner.pFPO.Org - curp.Pnt, false))
                mpCurrentRouteContainer.SetLast(curp);
        }
#if VISUALIZE_PATHS
        for (int j = 0; j < mCurrentRouteDim - 1; j++)
        {
            rScene.GetSceneVisualizer().CreatePath(buffer[j].Pnt, buffer[j + 1].Pnt,new FVec4(1, 1, 1, 0), 120);
        }
#endif 
        mpCurrentRouteContainer.Normalize();
        ProcessRoadPoint(mpCurrentRouteContainer.GetLast());
        mNowDest = mpCurrentRouteContainer.GetLast().Pnt;
    }
    void ClearCurrentOrder()
    {
        mpCurrentRouteContainer = null;
        mCurrentRouteDim = 0;
        rOwner.mReachedFlag = true;
    }
    void ClearIncoming()
    {
        if (mpIncoming != null)
        {
            //GetLog().Message("%p clearing incoming",this);
            mpIncoming.Release();
            mpIncoming = null;
        }
    }
    protected NavOrderContainer ProcessIncoming(INavigationOrder ord)
    {
        //Debug.Log("mpIncoming " + ord);

        bool ret = false;
        if (ord.GetRouteDimension() == 0)
            ret = true;

        //GetLog().Message("{0} processing incoming dim {1}",this,ord.GetRouteDimension());

        bool head = GetHeadAddingFlag(ord);
        bool set_as_current = mpCurrentRouteContainer == null || head;

        bool clear = ord == mpIncoming; // если это тот который ждали то обнуляем

        if (ret)
        {
            if (clear)
                ClearIncoming();
            return null;
        }

        NavOrderContainer cont = new NavOrderContainer(ord);

        if (!head)
            mlOrders.AddToTail(cont);    // добавляем в список
        else
            mlOrders.AddToHead(cont);   // добавляем в список

        if (clear)
            ClearIncoming();

        if (set_as_current)               // если была очередь пустая то устанавливаем как текущий
            SetCurrentOrder();

        for (BaseVehicleWingman man = mlWingmans.Head(); man!=null; man = man.Next())
            man.Wingman().UploadRouteFromLeader(cont);

        return cont;
    }

    public BaseVehicle GetLeaderVehicle() { return mLeader.Ptr() != null ? (BaseVehicle)mLeader.Ptr().GetInterface(BaseVehicle.ID) : null; }
    public BaseVehicleAutopilotNonManual GetLeaderAutopilot(BaseVehicle pveh = null)
    {
        BaseVehicle veh = pveh != null ? pveh : GetLeaderVehicle();
        BaseVehicleAutopilotNonManual at = null;
        if (veh != null)
        {
            at = veh.mpAutopilot != null ? (BaseVehicleAutopilotNonManual)veh.mpAutopilot.GetInterface(BaseVehicleAutopilotNonManual.ID) : null;
        }
        return at;
    }

    // параметры движения
    float mCheckDestTimer;

    public float Stop()
    {
        mTargetSpeed = 0;
        return 0;
    }

    public float VEHICLE_ARRIVE_THRUST = 0.4f;
    public float STOP_CUT_COS_ANGLE = 0.95f;
    public float STOP_COS_ANGLE = 0.0f;
    public float COLLIDE_TIME = 2f;

    float GetThrustCoeff(float course_cos_angle, float ground_cos_angle)
    {
        APDebug(string.Format("course_cos_angle {0} ground_cos_angle {1}", course_cos_angle, ground_cos_angle));
        float ret1 = mTargetSpeed;
        float ret2;
        APDebug("GetThrustCoeff ret1: " + ret1);
        VEHICLE_DATA data = rOwner.GetVehicleData();
        ret2 = ground_cos_angle * data.MaxSpeed;
        APDebug("Ground cos speed: " + ret2);
        if (ret2 < ret1)
            ret1 = ret2;
        ret2 = mpPredictionInfo.max_speed;
        APDebug("mpPredictionInfo.max_speed: " + mpPredictionInfo.max_speed);
        if (ret2 < ret1)
            ret1 = ret2;
        if (course_cos_angle < STOP_CUT_COS_ANGLE)
        {
            if (course_cos_angle < STOP_COS_ANGLE)
                course_cos_angle = STOP_COS_ANGLE;
            ret2 = mpPredictionInfo.precise_turn_speed * course_cos_angle;
            APDebug("GetThrustCoeff ret1 " + ret1 + " precise_turn_speed: " + mpPredictionInfo.precise_turn_speed + "  course_cos_angle " + course_cos_angle);
            if (ret2 < ret1)
                ret1 = ret2;
        }
        APDebug("ret1 late: " + ret1);
        return ret1;
    }

    protected Prediction mpPredictionInfo;
    protected bool mSetPredictions;

    void ProcessRoadPoint(ROADPOINT p)
    {
        if (!mSetPredictions) return;
        uint state = p.GetFlag(RoadpointDefines.NAV_STATE_MASK);
        uint mod = p.GetFlag(RoadpointDefines.NAV_MOD_MASK);

        switch (state)
        {
            case RoadpointDefines.NAV_ROAD:
                rOwner.SetTraceMode(true);
                mpPredictionInfo = rOwner.LoadPrediction(Prediction.iPredictionRoad);
                break;
            case RoadpointDefines.NAV_BRIDGE:
                rOwner.SetTraceMode(false);
                mpPredictionInfo = rOwner.LoadPrediction(Prediction.iPredictionGate);
                break;
            case RoadpointDefines.NAV_TUNNEL:
                rOwner.SetTraceMode(false, true);
                mpPredictionInfo = rOwner.LoadPrediction(Prediction.iPredictionGate);
                break;
            case RoadpointDefines.NAV_PLAIN:
                rOwner.SetTraceMode(true);
                if (mod == RoadpointDefines.NAV_DANGER)
                    mpPredictionInfo = rOwner.LoadPrediction(Prediction.iPredictionPlainDanger);
                else
                    mpPredictionInfo = rOwner.LoadPrediction(Prediction.iPredictionPlainGood);
                break;
            //#pragma message ("     EEI: plain aseert")
            default:
                //AssertBp(0);
                break;
        }
        //AssertBp(mpPredictionInfo);
    }

    //private int GetIndex(ROADPOINT[] points, ROADPOINT point)
    //{
    //    int setted_pos = -1;
    //    for (int i = 0; i < points.Length; i++) { if (points[i] == point) setted_pos = i; }
    //    return setted_pos;
    //}
    const float CHECK_DEST_TIME = 0.1f;
    //const float CHECK_DEST_TIME = 5f; //TODO вернуть как было значение CHECK_DEST_TIME после отладки прокладки маршрута
    void ProcessPoints(float scale)
    {
        mCheckDestTimer = CHECK_DEST_TIME;
        if (mpCurrentRouteContainer == null)
        {
            mTargetSpeed = 0;
            mNowDest = rOwner.pFPO.Org;
            return;
        }
        //Debug.Log("Processing order " + mpCurrentRouteContainer);
        bool order_done = true;
        int i = 0;
        ROADPOINT cur_point = mpCurrentRouteContainer.GetLast();

        //ROADPOINT fin_point = mpCurrentRouteContainer.GetBuffer() + mCurrentRouteDim; //GetBuffer возвращал указатель, так что это, фактически, получение элемента массива по индексу.

        //ROADPOINT fin_point = mpCurrentRouteContainer.GetBuffer()[mCurrentRouteDim-1]; //TODO Возможно, тут правильнее -1
        ROADPOINT fin_point = mpCurrentRouteContainer.GetBuffer()[mCurrentRouteDim]; //TODO Возможно, тут правильнее -1
        APDebug("mCurrentRouteDim " + mCurrentRouteDim + "/" + mpCurrentRouteContainer.GetBuffer().Length);
        //for (i=0;i< mpCurrentRouteContainer.GetBuffer().Length;i++)
        //{
        //    if (mpCurrentRouteContainer.GetBuffer()[i] == null) continue;

        //    Engine.DebugLine(rOwner.GetOrg(), mpCurrentRouteContainer.GetBuffer()[i].Pnt, Color.yellow);
        //    //Debug.Log(i + ":" + mpCurrentRouteContainer.GetBuffer()[i]!=null? mpCurrentRouteContainer.GetBuffer()[i].Pnt:"UNSET");
        //}
        //i = 0;
        //int cur_point_index = GetIndex(mpCurrentRouteContainer.GetBuffer(), cur_point);
        int cur_point_index = mpCurrentRouteContainer.GetIndex(cur_point);
        //Debug.Log("cur_point_index " + cur_point_index + " mCurrentRouteDim " + mCurrentRouteDim + " mpPredictionInfo " + mpPredictionInfo);
        //while (cur_point < fin_point && i < mpPredictionInfo.prediction_count)
        //while (cur_point_index < mCurrentRouteDim-1 && i < mpPredictionInfo.prediction_count)
        //while (cur_point != fin_point && i < mpPredictionInfo.prediction_count)
        while (cur_point_index < mCurrentRouteDim && i < mpPredictionInfo.prediction_count)
        { // prediction count
            APDebug(string.Format("cur_point_index {0} < mCurrentRouteDim {1}", cur_point_index, mCurrentRouteDim));
            order_done = false;
            if (CheckDest(rOwner.pFPO.Org - cur_point.Pnt, false))
            {
                //mpCurrentRouteContainer.SetLast(cur_point + 1);
                mpCurrentRouteContainer.SetLast(mpCurrentRouteContainer.GetBuffer()[cur_point_index + 1]);
                if (cur_point_index + 1 >= mCurrentRouteDim)
                {
                    order_done = true;
                    break;
                }
                ProcessRoadPoint(mpCurrentRouteContainer.GetBuffer()[cur_point_index + 1]);
            }
            //cur_point_index++;
            cur_point = mpCurrentRouteContainer.GetBuffer()[++cur_point_index];
            i++;
        }
        if (order_done)
        {
            APDebug("Order Done");
            DoneCurrentOrder();
            OnFinishRoute();
            mCheckDestTimer = -1f;
        }
        else
            mNowDest = mpCurrentRouteContainer.GetLast().Pnt;
        //AssertBp(mNowDest.Norma()>5.f);
        //Engine.DebugLine(rOwner.GetOrg(), mNowDest, Color.cyan, CHECK_DEST_TIME);
        APDebug(string.Format("mNowDest is  {0}->{1}", rOwner.GetOrg(), mNowDest));
    }
    // для кверенья
    new public const uint ID = 0x41F756AC;
    public override object GetInterface(DWORD id)
    {
        return (id == ID ? this : null);
    }

    // интерфейс с BaseVehicle
    ~BaseVehicleAutopilotNonManual()
    {
        Dispose();
    }

    public override void Dispose()
    {
        if (isDisposed) return;
        ClearAll();
        mlWingmans.Free();
        mLeader.Validate();
        BaseVehicleAutopilotNonManual at = GetLeaderAutopilot();
        if (at != null)
            at.UnRegisterWingman(this);

        base.Dispose();
        isDisposed = true;
    }

    public BaseVehicleAutopilotNonManual(BaseScene sc, BaseVehicle c) : base(sc, c)
    {
        mpIncoming = null;
        ClearCurrentOrder();
        mDestRecieved = false;
        mCheckDestTimer = 0;
        mpPredictionInfo = rOwner.LoadPrediction(Prediction.iPredictionPlainGood);
        mSetPredictions = true;
    }
    public override bool Move(float scale)
    {

        if (mpIncoming != null && mpIncoming.GetState() == NavigationOrderState.Ready)    // если мы ожидаем подсчета и путь посчтитался
            ProcessIncoming(mpIncoming);        // запихиваем его к себе
        if (mDestRecieved)
        {
            mDestRecieved = false;
            if (mlOrders.Head() == null)
                OnFinishRoute();
        }
        // транспорт едет на nowdest, так что обе функции могут выставлять его
        mCheckDestTimer -= scale;
        if (mCheckDestTimer <= 0 || mpCurrentRouteContainer == null)
            ProcessPoints(scale);    // обработка маршрута
        return true;
    }
    public virtual bool IsOnTheFormation() { return false; }
    public override int GetState()
    {
        if (rOwner.pFPO == null) return iSensorsDefines.CS_DEAD;

        if (rOwner.mInTunnel) return iSensorsDefines.CS_IN_TUNNEL;

        return iSensorsDefines.CS_IN_GAME;
    }
    iContact GetLeaderContact() { return mLeader.Ptr(); }

    public virtual void OnStopRoute()
    {
        mTargetSpeed = 0;
        ClearAll();
    }
    public virtual void OnFinishRoute() { }
    public override bool IsOnEarth()
    {
        if (mpCurrentRouteContainer == null) return true;
        ROADPOINT Pnt = mpCurrentRouteContainer.GetLast();
        if (Pnt == null) return true;
        DWORD state = Pnt.GetFlag(RoadpointDefines.NAV_STATE_MASK);
        switch (state)
        {
            case RoadpointDefines.NAV_ROAD: case RoadpointDefines.NAV_BRIDGE: case RoadpointDefines.NAV_TUNNEL: return false;
        }
        return true;
    }
    public override bool IsOnBridge()
    {
        if (mpCurrentRouteContainer == null) return false;
        ROADPOINT Pnt = mpCurrentRouteContainer.GetLast();
        if (Pnt == null) return false;
        DWORD state = Pnt.GetFlag(RoadpointDefines.NAV_STATE_MASK);
        switch (state)
        {
            case RoadpointDefines.NAV_BRIDGE: case RoadpointDefines.NAV_TUNNEL: return true;
        }
        return false;
    }
};
