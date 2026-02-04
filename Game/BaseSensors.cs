using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseSensors : iSensors, TLIST_ELEM<BaseSensors>,IDisposable
{
    private TLIST_ELEM_IMP<BaseSensors> myTLIST = new TLIST_ELEM_IMP<BaseSensors>();
    public const float SIDE_ACQUISITION_PERIOD = 2.5f;


    int mSideCode;
    public float mLastUpdateTime;
    //public LinkedList<BaseContact> mFriendsList = new LinkedList<BaseContact>();
    //public LinkedList<BaseContact> mEnemiesList = new LinkedList<BaseContact>();
    private TLIST<BaseContact> mFriendsList = new TLIST<BaseContact>();
    private TLIST<BaseContact> mEnemiesList = new TLIST<BaseContact>();
    public BaseScene mrScene;

    #region BaseSensors - от ISensors
    public int GetSideCode()
    {
        return mSideCode;
    }
    #endregion
    /// <summary>
    /// BaseSensors - own
    /// </summary>
    /// <param name="s"></param>
    /// <param name="c"></param>
    public BaseSensors(BaseScene s, int c)
    {
        mrScene = s;
        mSideCode = c;
        // копируем уже существующий список юнитов, как врагов
        if (mrScene.SensorsList.Head() != null)
        {
            BaseContact bsc;
            for (bsc = mrScene.SensorsList.Head().mFriendsList.Head(); bsc != null; bsc = bsc.Next())
            {
                if (bsc.GetUnit() == null) continue;
                mEnemiesList.AddToTail(new BaseContact(this, bsc.GetUnit()));
            }
            for (bsc = mrScene.SensorsList.Head().mEnemiesList.Head(); bsc != null; bsc = bsc.Next())
            {
                if (bsc.GetUnit() == null) continue;
                mEnemiesList.AddToTail(new BaseContact(this, bsc.GetUnit()));
            }
        }
        // добавляемся к списку
        mrScene.SensorsList.AddToTail(this);
        // ставим таймер
        mLastUpdateTime = mrScene.GetTime() - RandomGenerator.Rand01() * SIDE_ACQUISITION_PERIOD;
    }

    ~BaseSensors()
    {
        Dispose();
    }

    public void Dispose()
    {
        mrScene.SensorsList.Sub(this);
    }
    //public BaseSensors(BaseScene s, int c)
    //{
    //    mrScene = s;
    //    mSideCode = c;
    //    // копируем уже существующий список юнитов, как врагов
    //    if (mrScene.SensorsList.First != null)
    //    {
    //        LinkedListNode<BaseContact> bsc;
    //        for (bsc = mrScene.SensorsList.First.Value.mFriendsList.First; bsc != null; bsc = bsc.Next)
    //        {
    //            if (bsc.Value.GetUnit() == null) continue;
    //            mEnemiesList.AddLast(new BaseContact(this, bsc.Value.GetUnit()));
    //        }
    //        for (bsc = mrScene.SensorsList.First.Value.mEnemiesList.First; bsc != null; bsc = bsc.Next)
    //        {
    //            if (bsc.Value.GetUnit() == null) continue;
    //            mEnemiesList.AddLast(new BaseContact(this, bsc.Value.GetUnit()));
    //        }
    //    }
    //    // добавляемся к списку
    //    mrScene.SensorsList.AddLast(this);
    //    // ставим таймер
    //    mLastUpdateTime = mrScene.GetTime() - RandomGenerator.Rand01() * SIDE_ACQUISITION_PERIOD;

    //}

    public iContact AddUnit(IBaseUnit u, int UnitSideCode)
    {
        Asserts.AssertBp(u != null);
        BaseContact bsc = new BaseContact(this, u);

        if (UnitSideCode == mSideCode)
        {
            bsc.SetVisible(BaseContact.BCV_ALWAYS);
            mFriendsList.AddToTail(bsc);
        }
        else
        {
            mEnemiesList.AddToTail(bsc);
        }
        return bsc;
    }

    public void SubUnit(IBaseUnit u)
    {
        //Asserts.AssertBp(u != null);
        //LinkedListNode<BaseContact> bsc;
        //for (bsc = mFriendsList.First; bsc != null; bsc = bsc.Next)
        //    bsc.Value.SubUnit(u);
        //for (bsc = mEnemiesList.First; bsc != null; bsc = bsc.Next)
        //    bsc.Value.SubUnit(u);

        Assert.IsNotNull(u);
        BaseContact bsc;
        for (bsc = mFriendsList.Head(); bsc != null; bsc = bsc.Next())
            bsc.SubUnit(u);
        for (bsc = mEnemiesList.Head(); bsc != null; bsc = bsc.Next())
            bsc.SubUnit(u);
    }

    public iContact GetContact(uint handle)
    {
        //TODO Возможно, здесь требуется получение контакта с учётом его сенсорной видимости
        iContact i = mrScene.GetContact(handle);
        if (i == null) return null;

        //return (i.GetState() != iSensorsDefines.CS_DEAD) ? i : null;

        IBaseUnit u = null;
        try
        {
            u = (IBaseUnit)i.GetInterface(BaseUnit.ID);
        }
        catch
        {
            Debug.Log("Failed to cast " + i + " to BaseUnit");
            throw;
        }

        if (u == null) return null;
        //BaseContact c = (u.GetSideCode() == mSideCode ? mFriendsList.Head() : mEnemiesList.Head());
        //LinkedListNode<BaseContact> c = (u.GetSideCode() == mSideCode ? mFriendsList.   First : mEnemiesList.First);
        BaseContact c = (u.GetSideCode() == mSideCode ? mFriendsList.Head() : mEnemiesList.Head());
        for (; c != null; c = c.Next())
            if (c.GetUnit() == u) break;
        return (c != null && c.GetState() != iSensorsDefines.CS_DEAD ? c : null);
    }

    public iContact GetContactEx(uint handle)
    {
        throw new NotImplementedException();
    }

    public iContact GetFriend(iContact prev = null)
    {
        BaseContact p = (BaseContact)prev;
        return (p != null ? p.Next() : mFriendsList.Head());
    }

    public iContact GetEnemy(iContact prev = null)
    {
        BaseContact p = (BaseContact)prev;
        return (p != null ? p.Next() : mEnemiesList.Head());
    }


    public iContact GetEnemyInZone(Vector3 org, float radius, iContact prev = null, float LookBackTime = 0, bool IncludeNeutrals = false)
    {
        //BaseContact c = (BaseContact) prev;
        //radius *= radius;
        //for (c = (c!=null ? c.Next() : mEnemiesList.Head()); c; c = c->Next())
        //{
        //    if (c->Vis(LookBackTime) == false) continue;
        //    if (IncludeNeutrals == false && c->GetSideCode() == 0) continue;
        //    if (sqr(c->GetOrg().x - org.x) + sqr(c->GetOrg().z - org.z) >= radius) continue;
        //    return c;
        //}
        //return 0;


        //LinkedListNode<BaseContact> c = mEnemiesList.Find((BaseContact)prev);
        //if (c == null) c = mEnemiesList.First; else c = c.Next;
        //radius *= radius;

        ////Debug.Log("processing mEnemiesList of " + mEnemiesList.Count + " in " + radius + " c " + c);
        //for (;c!=null;c=c.Next)
        //{
        //    if (c.Value.Vis(LookBackTime) == false) continue;
        //    if (IncludeNeutrals == false && c.Value.GetSideCode() == 0) continue;
        //    if (Mathf.Pow(c.Value.GetOrg().x - org.x, 2) + Mathf.Pow(c.Value.GetOrg().z - org.z, 2) >= radius) continue;
        //    //Debug.Log("Locked on: " + c.Value);
        //    return c.Value;
        //}
        //return null;

        //foreach (BaseContact c in mEnemiesList)
        //{
        //    if (c.Vis(LookBackTime) == false) continue;
        //    if (IncludeNeutrals == false && c.GetSideCode() == 0) continue;
        //    if (Mathf.Pow(c.GetOrg().x - org.x, 2) + Mathf.Pow(c.GetOrg().z - org.z,2) >= radius) continue;
        //    Debug.Log("Locked on: " + c);
        //    return c;
        //}
        //return null;

        BaseContact c = (BaseContact)prev;
        radius *= radius;
        for (c = (c != null ? c.Next() : mEnemiesList.Head()); c != null; c = c.Next())
        {
            if (c.Vis(LookBackTime) == false) continue;
            if (IncludeNeutrals == false && c.GetSideCode() == 0) continue;
            if (Mathf.Pow(c.GetOrg().x - org.x, 2) + Mathf.Pow(c.GetOrg().z - org.z, 2) >= radius) continue;
            //Debug.Log("Locked on: " + c);
            return c;
        }
        return null;
    }

    //public bool Update()
    //{
    //    //BaseContact bsc;
    //    BaseContact bsc1;
    //    // раз в n секунд
    //    if (mLastUpdateTime + SIDE_ACQUISITION_PERIOD > mrScene.GetTime()) return true;
    //    mLastUpdateTime = mrScene.GetTime();

    //    // для FriendList Reserved=SernsorsEff(без учета угла места)

    //    //Свои
    //    //        foreach(BaseContact bsc in mFriendsList)
    //    for (LinkedListNode<BaseContact> bsc = mFriendsList.First; bsc != null; bsc = bsc.Next)
    //    {
    //        if (bsc.Value.Update() == false)
    //        {
    //            bsc.Value.Dispose();
    //            mFriendsList.Remove(bsc);
    //            continue;
    //        }

    //        bsc.Value.Reserved = Mathf.Pow(bsc.Value.GetSensorsRange(), 2);
    //    }

    //    // перебор всех зарегистрированных целей
    //    //foreach (BaseContact bsc in mEnemiesList)
    //    for (LinkedListNode<BaseContact> bsc = mEnemiesList.First; bsc != null; bsc = bsc.Next)
    //    {
    //        // чужие
    //        if (bsc.Value.Update() == false)
    //        {
    //            bsc.Value.Dispose();
    //            mEnemiesList.Remove(bsc);
    //            continue;
    //        }
    //        // для нейтралов ничего не обновляется
    //        if (mSideCode == CampaignDefines.CS_SIDE_NEUTRAL) continue;

    //        // для mEnemiesList Reserved=SernsorsVis(без учета угла места)
    //        bsc.Value.Reserved = Mathf.Pow(bsc.Value.GetSensorsRange(), 2);

    //        // затем - все подряд
    //        for (LinkedListNode<BaseContact> msc = mFriendsList.First; msc!=null; msc = msc.Next)
    //            if (bsc.Value.CheckVisibility(msc.Value)) break;
    //        //bsc.Value.CheckVisibility(mFriendsList.First.Value);
    //    }

    //    //# ifdef _DEBUG1
    //    //        mrScene.Message("BaseSensors for side %X: nFriends=%i nEnemies=%i Total=%i",
    //    //                mSideCode, mFriendsList.Counter(), mEnemiesList.Counter(), mFriendsList.Counter() + mEnemiesList.Counter());
    //    //#endif
    //    return (mFriendsList.Counter + mEnemiesList.Counter > 0);
    //}

    public bool Update()
    {
        BaseContact bsc;
        BaseContact bsc1;
        // раз в n секунд
        if (mLastUpdateTime + SIDE_ACQUISITION_PERIOD > mrScene.GetTime()) return true;
        mLastUpdateTime = mrScene.GetTime();
        // для FriendList Reserved=SernsorsEff(без учета угла места)
        for (bsc = mFriendsList.Head(); bsc != null; bsc = bsc1)
        { // свои
            bsc1 = bsc.Next();
            if (bsc.Update() == false)
            {
                mFriendsList.Sub(bsc);
                bsc.Dispose();
                continue;
            }
            bsc.Reserved = Mathf.Pow(bsc.GetSensorsRange(), 2);
        }
        // перебор всех зарегистрированных целей
        for (bsc = mEnemiesList.Head(); bsc != null; bsc = bsc1)
        { // чужие
            bsc1 = bsc.Next();
            if (bsc.Update() == false)
            {
                mEnemiesList.Sub(bsc);
                bsc.Dispose();
                continue;
            }
            // для нейтралов ничего не обновляется
            if (mSideCode == 0) continue;
            // для mEnemiesList Reserved=SernsorsVis(без учета угла места)
            bsc.Reserved = Mathf.Pow(bsc.GetSensorsVisibility(), 2);
            // затем - все подряд
            for (BaseContact msc = mFriendsList.Head(); msc != null; msc = msc.Next())
                if (bsc.CheckVisibility(msc)) break;
        }
# if _DEBUG1
        mrScene.Message("BaseSensors for side %X: nFriends=%i nEnemies=%i Total=%i",
            mSideCode, mFriendsList.Counter(), mEnemiesList.Counter(), mFriendsList.Counter() + mEnemiesList.Counter());
#endif
        return (mFriendsList.Counter() + mEnemiesList.Counter() > 0);
    }


    public override string ToString()
    {
        return string.Format("BaseSensors for side {0}: nFriends={1} nEnemies={2} Total={3}",
                mSideCode.ToString("X8"), mFriendsList.Counter(), mEnemiesList.Counter(), mFriendsList.Counter() + mEnemiesList.Counter());

    }

    public BaseSensors Next()
    {
        return ((TLIST_ELEM<BaseSensors>)myTLIST).Next();
    }

    public BaseSensors Prev()
    {
        return ((TLIST_ELEM<BaseSensors>)myTLIST).Prev();
    }

    public void SetNext(BaseSensors t)
    {
        ((TLIST_ELEM<BaseSensors>)myTLIST).SetNext(t);
    }

    public void SetPrev(BaseSensors t)
    {
        ((TLIST_ELEM<BaseSensors>)myTLIST).SetPrev(t);
    }
}

