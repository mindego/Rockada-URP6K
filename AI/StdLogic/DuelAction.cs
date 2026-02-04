using UnityEngine;
using DWORD = System.UInt32;

class DuelAction : RouteAction
{
    float mFightTime;
    float mFightTimeBnd;
    float mIdleTime;
    float mIdleTimeBnd;
    float mSavedRouteDelta;
    bool mEngaging;
    float mCurrentTimer;

    ~DuelAction()
    {
        mpDynGroup.SetRouteDelta(mSavedRouteDelta);
    }

    public bool Initialize(IGroupAi grp, DWORD pts_count, POINT_DATA[] pts, float FightTime, float FightTimeBnd, float IdleTime, float IdleTimeBnd)
    {
        bool ret = base.Initialize(grp, pts_count, pts);
        if (ret)
        {
            mFightTime = FightTime;
            mFightTimeBnd = FightTimeBnd;
            mIdleTime = IdleTime;
            mIdleTimeBnd = IdleTimeBnd;
            mSavedRouteDelta = mpDynGroup.GetRouteDelta();
            mpDynGroup.SetRouteDelta(20000f);
            mEngaging = false;
            mCurrentTimer = -1;
        }
        return ret;
    }

    // API
    public override ActionStatus Update(float scale)
    {
        ActionStatus status = base.Update(scale);
        if (status.IsActionAlive() && mpStdGroup.IsGroupAlive())
        {   // already alive
            mCurrentTimer -= scale;
            if (mCurrentTimer < 0f)
            {
                if (mEngaging)
                {
                    SearchMinResult angled = new SearchMinResult();
                    SearchMinResult nonangled = new SearchMinResult();
                    DWORD n = 0;
                    for (int i = 0; i < mPoints.Count(); ++i, ++n)
                    {
                        Vector3 n1 = mPoints[i].Org - mpGroup.GetLeaderOrg();
                        n1.y = 0f;
                        int j = i + 1;
                        if (j >= mPoints.Count())
                            j = 0;
                        Vector3 n2 = mPoints[j].Org - mPoints[i].Org;
                        n2.y = 0f;
                        float nrm1 = n1.magnitude;
                        float nrm2 = n2.magnitude;
                        float ang = (Vector3.Dot(n1,n2)) / (nrm1 * nrm2);
                        if (ang > 0f)
                            if (angled.Check(nrm1))
                                angled.Set(n, mPoints[i], nrm1);
                        if (nonangled.Check(nrm1))
                            nonangled.Set(n, mPoints[i], nrm1);
                    }
                    if (angled.Finded() == false)
                        angled = nonangled;
                    if (angled.Finded())
                    {
                        mpDynGroup.SetRouteDelta(-1f);
                        mDistTimer = -1f;
                        mEngaging = false;
                        Switch(angled.mNum, -1f);
                        mCurrentTimer = RandomGenerator.Rnd(mIdleTime, mIdleTimeBnd);
                    }
                }
                else
                {
                    mpDynGroup.SetRouteDelta(20000f);
                    mDistTimer = -1f;
                    mEngaging = true;
                    mCurrentTimer = RandomGenerator.Rnd(mFightTime, mFightTimeBnd);
                }
            }
        }
        return status;
    }
    public override bool IsDeleteOnPush()
    {
        return true;
    }
    const string sActionName = "Duel";
    public override string GetName()
    {
        return sActionName;
    }
    public override DWORD GetCode()
    {
        return Hasher.HshString(sActionName);
    }
    public override bool IsSwitching()
    {
        return true;
    }
};

class  SearchMinResult
{
    public DWORD mNum;
    POINT_DATA mPoint;
    float mDist;

    public SearchMinResult() {
        mNum = Constants.THANDLE_INVALID;
        mPoint = null;
        mDist = 100000f;
    }

    public bool Check(float dist) { return dist < mDist; }
    public bool Finded() { return mPoint!=null; }
    public void Set(DWORD num, POINT_DATA point, float dist)
    {
        mNum = num;
        mPoint = point;
        mDist = dist;
    }
};