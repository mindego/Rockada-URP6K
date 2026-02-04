using UnityEngine;
using DWORD = System.UInt32;
public class AirCarrier : BaseCarrier
{
    const float gMinHeight = 100.0f;
    const float gTracePeriod = 5.0f;
    const float gTraceDelta = 200.0f;
    const int gTraceCount = 15;

    float myCurHeight;
    float myTraceTimer;
    I3DSoundEvent mySound;
    public AirCarrier(BaseScene s, uint h, OBJECT_DATA od) : base(s, h, od) { }

    /// <summary>
    /// общая часть инициализации
    /// </summary>
    protected override void BasePrepare()
    {
        // setup under earth
        float y = pFPO.Org.y + pFPO.MinY() - rScene.SurfaceLevel(pFPO.Org.x, pFPO.Org.z) - gMinHeight;
        if (y < 0.0f)
            pFPO.Org.y -= y;
        myCurHeight = -10000.0f;
        myTraceTimer = -1.0f;

        if (rScene.GetSceneVisualizer() != null)
        {
            //Приведено в соответсвие всему остальному
            I3DSoundEventController pCtr = RefSoundCtrWrapper.CreateSoundCtrWrapper(pFPO.Org, mySpeed, (DWORD)pFPO);


            mySound = rScene.GetSceneVisualizer().Get3DSound().LoadEvent(
                "Carrier", ObjectData.FullName, "Fly", true, true, pCtr);
            if (mySound != null)
                mySound.Start();
        }
    }

    /// <summary>
    /// инициализация на хосте
    /// </summary>
    /// <param name="s"></param>
    /// <param name="sd"></param>
    /// <param name="v"></param>
    /// <param name="a"></param>
    /// <param name="hangar"></param>
    public override void HostPrepare(HostScene s, UnitSpawnData sd, Vector3 v, float a, iContact hangar)
    { // инициализация на хосте
      // координаты появления
        base.HostPrepare(s, sd, v, a, hangar);
        BasePrepare();
    }

    float traceDirection(Vector3 start_org, Vector3 target_org, float trace_len, int trace_count)
    {
        Vector3 diff = target_org - start_org;
        diff.y = 0;
        float nrm = diff.magnitude;
        float max_height = -10000.0f;
        if (nrm > 0)
        {
            diff /= nrm;
            diff *= trace_len;
            Vector3 start = start_org;

            for (int i = 0; i < trace_count; ++i)
            {
                float y = rScene.GroundLevelMedian(start.x, start.z, pFPO.SelfRadius * 0.5f);
                if (y > max_height)
                    max_height = y;
                start += diff;
            }
        }
        return max_height + gMinHeight;
    }

    protected override void updateTargetDest(ref Vector3 target_org, ref Vector3 dir, float scale)
    {
        myTraceTimer -= scale;
        if (myTraceTimer < 0.0f)
        {
            myCurHeight = traceDirection(pFPO.Org, target_org, gTraceDelta, gTraceCount);
            myTraceTimer = gTracePeriod;
        }
        float terrain_level = rScene.SurfaceLevel(target_org.x, target_org.z) + gMinHeight;
        target_org.y = Mathf.Max(myCurHeight, Mathf.Max(target_org.y, terrain_level));
        dir.y = 0;
    }

}
