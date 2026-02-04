using DWORD = System.UInt32;
using UnityEngine;

public class IdleAction : iAction
{
    public const float ENEMY_TIME_CHECK = 4f;
    protected IGroupAi mpGroup;
    protected StdGroupAi mpStdGroup;
    protected float mEnemyTimer;
    protected bool mActive;
    protected bool mAlive;
    protected GROUP_DATA mpData;

    public IdleAction()
    {
        mpGroup = null;
        mpStdGroup = null;
        mActive = true;
        mpData = null;
        mAlive = true;
    }

    ~IdleAction() { }

    protected ActionStatus GetAliveStatus() { ActionStatus ret = new(); if (false == mAlive) ret.ActionDead(); return ret; }
    public bool IsActive() { return mActive; }
    public virtual bool Initialize(IGroupAi grp)
    {
        mpGroup = grp;
        mEnemyTimer = RandomGenerator.Rand01() * ENEMY_TIME_CHECK;
        mpStdGroup = (StdGroupAi)(mpGroup.Query(StdGroupAi.ID));
        mpData = mpGroup.GetGroupData();
        return mpStdGroup != null;
    }

    // API
    public virtual ActionStatus Update(float scale)
    {
        Asserts.AssertBp(mActive);
        ActionStatus status = GetAliveStatus();
        if (mpStdGroup.GetGhostCount() != 0)
        {
            if (status.IsActionAlive() && mpStdGroup.IsGroupAlive())
            {
                mEnemyTimer -= scale;
                if (mEnemyTimer <= 0f)
                {
                    Vector3 radar_center;
                    float radar_radius;
                    if (mpStdGroup.GetRadarCenter(out radar_center, out radar_radius))
                    {
                        mEnemyTimer = mpStdGroup.ScanEnemies(radar_center, radar_radius);
                    }
                }
            }
        }
        else
            status.GroupDead();
        return status;
    }
    public virtual bool IsDeleteOnPush() { return true; }

    const string sActionName = "Idle";
    public virtual string GetName()
    {
        return sActionName;
    }
    public virtual DWORD GetCode()
    {
        return Hasher.HshString(sActionName);
    }
    public virtual void Activate()
    {
        mActive = true;
    }
    public virtual void DeActivate()
    {
        mActive = false;
    }
    public virtual bool IsSwitching()
    {
        return false;
    }
    public virtual void Switch(DWORD num, float speed)
    {
        AICommon.AiMessage(MessageCodes.MSG_ERROR, Parsing.sAiWarning, "switch in idle action");
    }

    public virtual void OnGroupReborn() { }
    public virtual void Dead() { mAlive = false; }
    public virtual bool IsCanBeBreaked()
    {
        return true;
    }
    public virtual Vector3 getExecutionPos() { return Vector3.zero; }
    public virtual void updatePoint() { }

    public void AddRef()
    {
        //throw new System.NotImplementedException();
    }

    public int RefCount()
    {
        return 0;
        //throw new System.NotImplementedException();
    }

    public int Release()
    {
        return 0;
        //throw new System.NotImplementedException();
    }

    public virtual object Query(uint cls_id)
    {
        throw new System.NotImplementedException();
    }
}