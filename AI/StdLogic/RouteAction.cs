using System.Collections.Generic;
using UnityEngine;
using DWORD = System.UInt32;

public class RouteAction : DynamicAction
{
    protected Tab<POINT_DATA> mPoints = new Tab<POINT_DATA>();
    protected List<POINT_DATA> mDynPoints = new List<POINT_DATA>();

    protected DWORD mPrevPoint;
    protected DWORD mCurrentPoint;

    protected void SetPoints()
    {
        mPoints.SetCount(0);
        //mPoints.Clear();

        for (int i = 0; i < mDynPoints.Count; ++i)
            mPoints.New(mDynPoints[i]);
        mCurrentPoint = Constants.THANDLE_INVALID;
        SetNextPoint(Constants.THANDLE_INVALID);
        NextPoint();
    }
    void clearPrevPoint()
    {
        mPrevPoint = Constants.THANDLE_INVALID;
    }
    protected void SetNextPoint(DWORD pnt)
    {
        mPrevPoint = mCurrentPoint;
        mCurrentPoint = pnt;
    }
    bool GetCheckRouteDelta(out Vector3 def)
    {
        def = Vector3.zero;
        iContact cnt = mpGroup.GetLeaderContact();
        if (cnt == null)
            return false;

        Vector3 prev_org;
        float prev_time;

        if (IsPrevPointValid() == false)
        {
            prev_org = cnt.GetOrg();
            prev_time = stdlogic_dll.mCurrentTime;
        }
        else
        {
            prev_org = mPoints[(int)mPrevPoint].Org;
            prev_time = mPoints[(int)mPrevPoint].TimeToPoint;
        }
        POINT_DATA cur_point = mPoints[(int)mCurrentPoint];
        float tm2 = stdlogic_dll.mCurrentTime;
        float tm = cur_point.TimeToPoint - tm2;
        if (tm > 0)
        {
            def = cur_point.Org - prev_org;
            def.y = 0;
            tm2 = (tm2 - prev_time) / (cur_point.TimeToPoint - prev_time);
            def = prev_org + def * tm2;
        }
        else
            def = cur_point.Org;
        return true;
    }
    bool IsCurrentPointValid() { return mCurrentPoint != Constants.THANDLE_INVALID; }
    bool IsPrevPointValid() { return mPrevPoint != Constants.THANDLE_INVALID; }
    protected void NextPoint()
    {
        DWORD n;
        if (IsCurrentPointValid() == false)
            n = 0;
        else
            n = mCurrentPoint + 1;
        if (n >= mPoints.Count())
            n = Constants.THANDLE_INVALID;
        SetNextPoint(n);
    }

    public virtual bool Initialize(IGroupAi grp, DWORD pts_count, POINT_DATA[] pts)
    {
        bool ret = base.Initialize(grp);
        if (ret)
        {
            for (int i = 0; i < pts_count; ++i)
                mPoints.New(pts[i]);
            mCurrentPoint = Constants.THANDLE_INVALID;
            SetNextPoint(Constants.THANDLE_INVALID);
            NextPoint();
        }
        return ret;
    }

    // API
    public virtual bool IsDeleteOnFinish()
    {
        return true;
    }
    public override ActionStatus Update(float scale)
    {
        ActionStatus status = base.Update(scale);
        POINT_DATA cpData, ppData;
        for (int i = 1; i < mPoints.Count(); i++)
        {
            cpData = mPoints[i];
            ppData = mPoints[i - 1];
            Debug.DrawLine(Engine.ToCameraReference(ppData.Org), Engine.ToCameraReference(cpData.Org), Color.grey);
        }
        if (mCurrentPoint < mPoints.Count() - 1)
        {
            cpData = mPoints[(int)mCurrentPoint];
            //ppData = mpGroup.GetLeaderOrg();
            //Debug.DrawLine(Engine.ToCameraReference(ppData.Org), Engine.ToCameraReference(cpData.Org),Color.white);
            Debug.DrawLine(Engine.ToCameraReference(mpGroup.GetLeaderOrg()), Engine.ToCameraReference(cpData.Org), Color.green);
        }

        if (status.IsActionAlive() && mpStdGroup.IsGroupAlive())
        {   // already alive
            if (IsCurrentPointValid() == false)
                if (IsDeleteOnFinish()) status.ActionDead(); else status.ActionDeactivate();

            else
            {
                POINT_DATA cur_point = mPoints[(int)mCurrentPoint];
                if (mNeedToChangePoint)
                {  // point changed 
                    PauseGroup(false, false);     // not pausing
                    SetDestination(mPoints[(int)mCurrentPoint].Org, mPoints[(int)mCurrentPoint].TimeToPoint);    // setting dest
                    PointChanged(); // not changing not required
                }
                if (isNeedToChangeFormation())
                {
                    SetFormation();
                    FormationChanged();
                }
                Vector3 temp = Vector3.zero;
                if (mpDynGroup.IsPointReached(cur_point.Org, IsPrevPointValid() ? mPoints[(int)mPrevPoint].Org : temp, IsPrevPointValid()))
                {
                    DWORD old_point = mCurrentPoint;
                    string buffer;
                    //wsprintf(buffer, "point %d", mCurrentPoint);
                    buffer = string.Format("point {0}", mCurrentPoint);
                    executeScript(cur_point.AiScript, buffer);
                    if (old_point == mCurrentPoint) // new point not setted
                        NextPoint(); // next point
                    if (IsCurrentPointValid())   // valid point setted
                        NeedToChangePoint();  // change dest on next tick
                }
                mDistTimer -= scale;
                if (IsCurrentPointValid() && mDistTimer <= 0f)
                {
                    Vector3 def;
                    DWORD leader_near;
                    GetCheckRouteDelta(out def);
                    mDistTimer = mpDynGroup.CheckRouteDelta(def, out leader_near);
                }
            }
        }
        return status;
    }


    public override void Switch(DWORD num, float max_speed)
    {
        if (num >= mPoints.Count()) return;  // if out of bound
        float addon = 0f;
        if (IsCurrentPointValid())
        {
            Vector3 diff = mPoints[(int)num].Org - mPoints[(int)mCurrentPoint].Org;
            diff.y = 0f;
            float to_zero = stdlogic_dll.mCurrentTime + (AICommon.CCmp(max_speed) ? 0f : diff.magnitude / max_speed);
            addon = to_zero - mPoints[(int)num].TimeToPoint;
            if (addon < 0f)
                addon = 0f;
            POINT_DATA tmp = mPoints[(int)num];
            tmp.TimeToPoint = to_zero;
            //mPoints[(int)num].TimeToPoint = to_zero;
            mPoints[(int)num] = tmp;
        }
        for (int i = (int)num; i < mPoints.Count() - 1; ++i)
        {
            POINT_DATA tmp = mPoints[i + 1];
            tmp.TimeToPoint += addon;
            mPoints[i + 1] = tmp;
        }
        SetNextPoint(num);
        clearPrevPoint();
        NeedToChangePoint();
    }
    public override Vector3 getExecutionPos()
    {
        return IsCurrentPointValid() ? mPoints[(int)mCurrentPoint].Org : Vector3.zero;
    }
};
