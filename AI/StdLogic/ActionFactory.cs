using System.Collections.Generic;
using UnityEngine;
using DWORD = System.UInt32;

public static class ActionFactory
{
    //# include "AiCommon.h"
    //# include "ActionFactory.h"

    //# include "IdleAction.h"
    //# include "MoveAction.h"
    //# include "PauseAction.h"
    //# include "CraftTakeoffAction.h"
    //# include "CraftParkAction.h"
    //# include "MoveToAction.h"
    //# include "RouteToAction.h"
    //# include "EscortAction.h"
    //# include "PatrolAction.h"
    //# include "AlertAction.h"
    //# include "DuelAction.h"
    //# include "CaptureAction.h"
    //# include "DefendAction.h"

    public static iAction CreateIdleAction(IGroupAi grp)
    {
        IdleAction myObject = new IdleAction();
        if (!myObject.Initialize(grp))
        {
            myObject.Release();
            myObject = null;
        }
        return myObject;
    }

    public static iAction CreateCaptureAction(IGroupAi grp, CaptureInfo cpt, List<DWORD> grps)
    {
        CaptureAction myObject = new CaptureAction();
        if (!myObject.Initialize(grp, cpt, grps))
        {
            myObject.Release();
            myObject = null;
        }
        return myObject;
    }

    public static iAction CreateMoveAction(IGroupAi grp, DWORD pnt_count, POINT_DATA[] pts)
    {
        MoveAction myObject = new MoveAction();
        if (!myObject.Initialize(grp, pnt_count, pts))
        {
            myObject.Release();
            myObject = null;
        }
        return myObject;
    }

    public static iAction CreatePauseAction(IGroupAi grp, float time, Vector3 org)
    {
        PauseAction myObject = new PauseAction();
        if (!myObject.Initialize(grp, time, org))
        {
            myObject.Release();
            myObject = null;
        }
        return myObject;
    }

    public static iAction CreateCraftTakeoffAction(IGroupAi grp)
    {
        CraftTakeoffAction myObject = new CraftTakeoffAction();
        if (!myObject.Initialize(grp))
        {
            myObject.Release();
            myObject = null;
        }
        return myObject;
    }

    public static iAction CreateMoveToAction(IGroupAi grp, Vector3 org, float time, string script)
    {
        Debug.Log("MoveToAction created for " + grp.GetGroupData().Callsign + " with script " + script);
        MoveToAction myObject = new MoveToAction();
        if (!myObject.Initialize(grp, org, time, script))
        {
            myObject.Release();
            myObject = null;
        }
        return myObject;
    }

    public static iAction CreateRouteToAction(IGroupAi grp, Vector3 org, float time, string script)
    {
        RouteToAction myObject = new RouteToAction();
        if (!myObject.Initialize(grp, org, time, script))
        {
            myObject.Release();
            myObject = null;
        }
        return myObject;
    }

    public static iAction CreateCraftParkAction(IGroupAi grp, string base_name, DWORD ultimate)
    {
        CraftParkAction myObject = new CraftParkAction();
        if (!myObject.Initialize(grp, base_name, ultimate))
        {
            myObject.Release();
            myObject = null;
        }
        return myObject;
    }

    public static iAction CreateParkAction(IGroupAi grp, string base_name, DWORD ultimate)
    {
        ParkAction myObject = new ParkAction();
        if (!myObject.Initialize(grp, base_name, ultimate))
        {
            myObject.Release();
            myObject = null;
        }
        return myObject;
    }

    public static iAction CreateEscortAction(IGroupAi grp, List<string> grps, float delta) //TODO Возможно, тип списка должен быть другой
    {
        EscortAction myObject = new EscortAction();
        if (!myObject.Initialize(grp, grps, delta))
        {
            myObject.Release();
            myObject = null;
        }
        return myObject;
    }

    public static iAction CreatePatrolAction(IGroupAi grp, Vector3 org, float dist)
    {
        PatrolAction myObject = new PatrolAction();
        if (!myObject.Initialize(grp, org, dist))
        {
            myObject.Release();
            myObject = null;
        }
        return myObject;
    }

    public static iAction CreateAlertAction(IGroupAi grp, Vector3 org)
    {
        AlertAction myObject = new AlertAction();
        if (!myObject.Initialize(grp, org))
        {
            myObject.Release();
            myObject = null;
        }
        return myObject;
    }

    public static iAction CreateDuelAction(IGroupAi grp, DWORD pnt_count, POINT_DATA[] pts, float FightTime, float FightTimeBnd, float IdleTime, float IdleTimeBnd)
    {
        DuelAction myObject = new DuelAction();
        if (!myObject.Initialize(grp, pnt_count, pts, FightTime, FightTimeBnd, IdleTime, IdleTimeBnd))
        {
            myObject.Release();
            myObject = null;
        }
        return myObject;
    }

    public static iAction CreateDefendAction(IGroupAi grp, DefendInfo info, List<EngageInfo> lst) //TODO Возможно, тут правиьноее LinkedList
    {
        DefendAction myObject = new DefendAction();
        if (!myObject.Initialize(grp, info, lst))
        {
            myObject.Release();
            myObject = null;
        }
        return myObject;
    }
}

class DefendAction : IdleAction
{
    public const uint ID = 0x17FBCDC1;

    //Tab<iContact> myScannedTargets;
    //Tab<float> myScannedWeights;
    List<iContact> myScannedTargets;
    List<float> myScannedWeights;

    DefendInfo mpDefendInfo;
    //TLIST<EngageInfo>* mpEngagers;
    List<EngageInfo> mpEngagers;

    public bool Initialize(IGroupAi grp, DefendInfo info, List<EngageInfo> tb)
    {
        bool ret = base.Initialize(grp);
        if (ret)
        {
            mpDefendInfo = info;
            mpEngagers = tb;
        }
        return ret;
    }

    // API
    public override ActionStatus Update(float scale)
    {
        Asserts.AssertBp(mActive);
        ActionStatus status = GetAliveStatus();
        if (mpStdGroup.GetGhostCount()!=0)
        {
            if (status.IsActionAlive() && mpStdGroup.IsGroupAlive())
            {
                mEnemyTimer -= scale;
                if (mEnemyTimer <= 0f && mpEngagers.Count!=0)
                {
                    iContact lead = mpGroup.GetLeaderContact();
                    EnemyCount enemies_count = mpStdGroup.ScanTargets(lead.GetOrg(), mpDefendInfo.mScanRadius, 0f, out myScannedTargets, out myScannedWeights);
                    if (enemies_count.mEnemyCount!=0)
                    {
                        int engagers_avaible = 0;
                        for (int i = 0; i < enemies_count.mEnemyCount; ++i)
                        {
                            if (EngageInfo.InEngagers(mpEngagers, myScannedTargets[i].GetHandle()))
                            {
                                engagers_avaible++;
                                myScannedWeights[i] = 40f;
                            }
                        }
                        if (engagers_avaible!=0)
                            //mpStdGroup.SendTargets(SEND_TO_ALL, enemies_count.mEnemyCount, myScannedTargets.Begin(), myScannedWeights.Begin());
                            mpStdGroup.SendTargets(StdGroupAiDefs.SEND_TO_ALL, (int)enemies_count.mEnemyCount, myScannedTargets.ToArray(), myScannedWeights.ToArray());
                    }
                    mEnemyTimer = 3f;
                }
            }
        }
        else
            status.GroupDead();
        return status;
    }

    const string sActionName = "Defend";
    public override  string GetName()
    {
        return sActionName;
    }
    public override DWORD GetCode()
    {
        return Hasher.HshString(sActionName);
    }
};
