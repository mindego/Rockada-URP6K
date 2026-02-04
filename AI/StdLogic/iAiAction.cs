using UnityEngine;
using DWORD = System.UInt32;

public interface iAction : IObject
{
    public const uint GROUP_DEAD = 0x00000001;
    public const uint ACTION_DEAD = 0x00000002;
    public const uint ACTION_DEACTIVATE = 0x00000004;

    public ActionStatus Update(float scale);
    public bool IsDeleteOnPush();
    public string GetName();
    public DWORD GetCode();
    public void Activate();
    public void DeActivate();
    public bool IsSwitching();
    public void Switch(DWORD num, float speed);
    public void OnGroupReborn();
    public void Dead();
    public bool IsCanBeBreaked();
    public Vector3 getExecutionPos();
    public void updatePoint();
};

public class ActionStatus
{
    DWORD Flags;
    public void SetFlag(DWORD Flag) { Flags |= Flag; }
    public void ClearFlag(DWORD Flag) { Flags &= ~Flag; }
    public DWORD GetFlag(DWORD Flag) { return Flags & Flag; }
    public ActionStatus()
    {
        Flags = 0;
    }
    public void GroupDead() { SetFlag(iAction.GROUP_DEAD); }
    //public void ActionDead() { Debug.Log("Set Action as DEAD"); SetFlag(iAction.ACTION_DEAD); }
    public void ActionDead() { SetFlag(iAction.ACTION_DEAD); }
    public void ActionDeactivate() { SetFlag(iAction.ACTION_DEACTIVATE); }
    public bool IsGroupDead() { return GetFlag(iAction.GROUP_DEAD) != 0; }
    public bool IsActionDead() { return GetFlag(iAction.ACTION_DEAD) !=0; }
    //public bool IsActionAlive() { return (GetFlag(iAction.ACTION_DEAD) !=0)== false; }
    public bool IsActionAlive() { return !IsActionDead(); }
    public bool IsActionDeactivated() { return GetFlag(iAction.ACTION_DEACTIVATE) !=0; }
};

