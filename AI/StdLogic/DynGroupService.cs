using crc32 = System.UInt32;
using UnityEngine;

public class DynGroupService<T> : IDynGroupService where T : StdDynamicGroupAi
{
    public void killAll(bool flag)
    {
        myMsn.SetKillAll(flag);
    }

    public void setRouteDelta(float delta)
    {
        myMsn.SetRouteDelta(delta);
    }

    public void setReachRadius(float radius)
    {
        myMsn.Script_SetReachRadius(radius);
    }
    public void resumeGroup()
    {
        myMsn.Script_SetResume(true);
    }
    public void setFormation(string name, float dist)
    {
        myMsn.SetFormation(Hasher.HshString(name), dist, name, false, true);
    }
    public void switchPoint(int num)
    {
        myMsn.Script_SetSwitch(num);
    }
    public void pauseGroup(float time)
    {
        myMsn.Script_SetPause(time);
    }
    public void breakAction()
    {
        myMsn.Script_SetResume(false);
    }

    public void routeTo(Vector3 org, float time, bool clear_all)
    {
        myMsn.Script_SetRouteTo(org, time, clear_all);
    }

    public float getReachRadius()
    {
        return myMsn.getReachRadius();
    }

    public MARKER_DATA getMarkerData(crc32 id)
    {
        return myMsn.getMarkerData(id);
    }
    public void alert(Vector3 org)
    {
        myMsn.OnAlert(org);
    }

    public void park(string baseName, int ultimate)
    {
        myMsn.Script_SetPark(baseName, (uint) ultimate);
    }

    public void setAutoReformat(bool enable)
    {
        myMsn.setAutoReformat(enable);
    }

    public void setSpeed(float speed)
    {
        myMsn.SetSpeed(speed);
    }

    public int GetMarkersCount()
    {
        return myMsn.GetMarkersCount();
    }

    public DynGroupService(T imp)
    {
        myMsn = imp;
    }

    T myMsn;
}