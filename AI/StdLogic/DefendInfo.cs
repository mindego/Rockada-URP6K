using System.Collections.Generic;
using UnityEngine;
using DWORD = System.UInt32;
public class CaptureInfo
{
    public float mHeight;
    public float mSpeed;
    public DWORD mSide;
    public DWORD mMode;
    public StdTeamplayAiSpecs.iNotifyCapture mpNotify;
    public string mpName;

    public CaptureInfo()
    {
        mpName = null;
        mMode = StdTeamplayAiSpecs.CAPTURE_COMMON;
        mSide = Constants.THANDLE_INVALID;
        mpNotify = null;
        mHeight = 0;
        mSpeed = 0;
    }


    public void SetSide(DWORD sd) { mSide = sd; }
    public void SetMode(DWORD sd, StdTeamplayAiSpecs.iNotifyCapture nt) { mMode = sd; mpNotify = nt; }
    public void SetName(string nm) { mpName = nm!=null ? nm : null; }

    public void ClearSide() { SetSide(Constants.THANDLE_INVALID); }

    public void SetHeight(float hgt) { mHeight = hgt; }
    public void SetSpeed(float hgt) { mSpeed = hgt; }
};

public class EngageInfo
{
    DWORD mHandle;
    readonly TContact mEngager=new TContact();
    float mTime;
    public EngageInfo() { mEngager.setPtr(null); mHandle = Constants.THANDLE_INVALID;  }
    public EngageInfo(iContact self, DWORD id, float time) {
        //mEngager = new TContact(self);
        mEngager.setPtr(null);
        mTime = time;
        mHandle = id; 
    }

    public DWORD GetHandle() { return mHandle; }
    void SetTime(float time) { mTime = time; }
    bool IsAlive() { return mEngager.Validate(); }
    bool Update(float scale) { mTime -= scale; return mTime > 0; }

    public static bool InEngagers(List<EngageInfo> lst, DWORD engage_id)
    {
        // является ли данная цель агрессором
        //for (EngageInfo* info = lst.Head(); info; info = info->Next())
        //    if (info->GetHandle() == engage_id)
        //        return true;
        foreach (EngageInfo info in lst)
        {
            if (info.GetHandle() == engage_id) return true;
        }
        return false;
    }
}
public class DefendInfo
{
    public DWORD mDefendMode;
    public float mKillRadius;
    public float mScanRadius;

    DefendInfo() { Set(StdTeamplayAiSpecs.DEFEND_NONE, 0f, 0f); }

    void Set(DWORD mode, float kill_r, float scan_r)
    {
        mDefendMode = mode;
        mKillRadius = kill_r;
        mScanRadius = scan_r;
    }
};


public static class StdTeamplayAiSpecs
{
    public const int CAPTURE_NONE = 0;
    public const int CAPTURE_COMMON = 1;

    public interface iNotifyCapture
    {
        public void FieldCaptured(DWORD contact_handle, string name, DWORD old_side, DWORD new_side);
    };

    public interface iCaptureSpec
    {
        public const uint ID = 0x70F7EF71;
        public void SetCaptureMode(DWORD mode, iNotifyCapture c);
        public void CapturedBySide(DWORD side);
        public void SetCaptureProp(DWORD capture_count, DWORD[] groups, string name);
        public void SetCaptureParams(float height, float speed);
    };

    public const int REPAIR_NONE = 0;
    public const int REPAIR_COMMON = 1;
    public const int REPAIR_FRIENDS = 2;

    public interface iRepairSpec
    {
        public const uint ID = 0x78F428D0;
        public void SetRepairMode(DWORD mode);
    };

    public const int DEFEND_NONE = 0;
    public const int DEFEND_FRIENDLY = 1;
    public const int DEFEND_AGRESSOR = 2;

    public interface iDefendSpec
    {
        public const uint ID = 0xEE7A975A;
        public void SetDefendMode(DWORD mode, float kill_radius, float scan_radius);
        public void RegisterEngage(iContact engager, Vector3 org, DWORD id, float engage_time);
    };
}