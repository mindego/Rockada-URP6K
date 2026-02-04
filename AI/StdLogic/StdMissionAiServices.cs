using UnityEngine;

public partial class StdMissionAi 
{
    public const uint MT_MISSION_QUIT = 0x4B4A884A;
    public object Query2(uint cls_id)
    {
        switch (cls_id)
        {
            case ITimeService.ID: return getTimer();
            case IRadioService.ID: return getRadio();
            case IRadioSender.ID: return mySender;
            case ITriggerService.ID: return myTriggers;
            case IMissionService.ID: return myMission;
            case IMenuService.ID: return getMenu();
            case IErrorLog.ID: return getErrorLog();
            case ISkillService.ID: return mySkillSrv;
            default: return null;
        }
    }

    public void quitMission()
    {
        Quit(MT_MISSION_QUIT, true, false);
    }

}