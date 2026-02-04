using UnityEngine;
using crc32 = System.UInt32;
public interface IDynGroupService
{
    public const uint ID=0x779B49F1;
    public void setReachRadius(float radius);
    public void resumeGroup();
    public void setFormation(string name, float dist);
    public void switchPoint(int num);
    public void pauseGroup(float time);
    public void breakAction();
    public float getReachRadius();
    public MARKER_DATA getMarkerData(crc32 id);
    public void killAll(bool flag);
    public void setRouteDelta(float delta);
    public void routeTo(Vector3 org, float time, bool clear_all);
    public void alert(Vector3 org);
    public void park(string baseName, int ultimate);
    public void setAutoReformat(bool enable);
    public void setSpeed(float speed);
    public int GetMarkersCount();
}