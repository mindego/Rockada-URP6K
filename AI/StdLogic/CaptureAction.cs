using System.Collections.Generic;
using UnityEngine;
using DWORD = System.UInt32;

public class CaptureAction : IdleAction
{
    const uint ID = 0x3ED6485C;

    CaptureInfo mpCaptureMode;
    List<DWORD> mpGroups;

    public void ProcessCapture(DWORD side, iContact cnt, string name)
    {
        Asserts.AssertBp(side != 0);
        DWORD old_side = mpData.Side;
        mpStdGroup.OnChangeSide(side);
        for (int i = 0; i < mpGroups.Count; ++i)
        {
            IGroupAi grp_ai = mpStdGroup.GetIMissionAi().GetGroupByID((mpGroups)[i]);
            if (grp_ai != null)
            {
                StdGroupAi std_grp_ai = (StdGroupAi)grp_ai.Query(StdGroupAi.ID);
                if (std_grp_ai != null)
                    std_grp_ai.OnChangeSide(side);
            }
        }
        if (mpCaptureMode.mpNotify != null && cnt != null && name != null)
            mpCaptureMode.mpNotify.FieldCaptured(cnt.GetHandle(), name, old_side, side);
    }

    public bool Initialize(IGroupAi grp, CaptureInfo cpt, List<DWORD> grps)
    {
        bool ret = base.Initialize(grp);
        if (ret)
        {
            mpCaptureMode = cpt;
            mpGroups = grps;
        }
        return ret;
    }

    // API
    public override object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case CaptureAction.ID: return this;
            default: return base.Query(cls_id);
        }
    }
    public override ActionStatus Update(float scale)
    {
        Asserts.AssertBp(mActive);
        ActionStatus status = new ActionStatus();
        if (mpStdGroup.GetGhostCount()!=0)
        {
            if (status.IsActionAlive() && mpStdGroup.IsGroupAlive())
            {
                if (mpCaptureMode.mSide != Constants.THANDLE_INVALID)
                {
                    Asserts.AssertBp(mpCaptureMode.mSide != 0) ;
                    ProcessCapture(mpCaptureMode.mSide, null, null);
                    mpCaptureMode.ClearSide();
                }
                iContact lead = mpGroup.GetLeaderContact();
                if (lead!=null && mpCaptureMode.mMode == StdTeamplayAiSpecs.CAPTURE_COMMON)
                {
                    List<iContact> targets;
                    List<float> weights;
                    EnemyCount enemies_count = mpStdGroup.ScanTargets(lead.GetOrg(), 300f, 1f, out targets, out weights);
                    if (enemies_count.mEnemyCount!=0)
                    {
                        iContact capture = null;
                        for (int i = 0; i < enemies_count.mEnemyCount; ++i)
                        {
                            if (Hasher.HshString(targets[i].GetTypeName()) == 0x113A495A)
                            {  // ATr 0x113A495A   BF 0xE3D62422
                                Vector3 diff = targets[i].GetOrg() - lead.GetOrg();
                                if (diff.magnitude < mpCaptureMode.mHeight && targets[i].GetSpeed().magnitude < mpCaptureMode.mSpeed)
                                {
                                    capture = targets[i];
                                    break;
                                }
                            }
                        }
                        if (capture!=null)
                            ProcessCapture((uint)capture.GetSideCode(), capture, mpCaptureMode.mpName);
                    }
                }
            }
        }
        else
            status.GroupDead();
        return status;
    }

    const string sActionName = "Capture";
    public override string  GetName()
    {
        return sActionName;
    }
    public override DWORD GetCode()
    {
        return Hasher.HshString(sActionName);
    }
};
