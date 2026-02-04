using UnityEngine;
using DWORD = System.UInt32;

public class SeaCarrier : BaseCarrier
{
    protected override void BasePrepare()
    {

        // setup under earth
        pFPO.Org.y = rScene.WaterLevel(pFPO.Org.x, pFPO.Org.z);
        if (pFPO.Org.y <= rScene.GroundLevel(pFPO.Org.x, pFPO.Org.z))
            throw new System.Exception("create : SeaCarrier can't appear on earth!");
    }
    protected override void updateTargetDest(ref Vector3 org, ref Vector3 dir, float scale)
    {
        Vector3 test = pFPO.Org + pFPO.Dir * pFPO.MaxZ();
        pFPO.Org.y = rScene.WaterLevel(test.x, test.z);
        if (pFPO.Org.y + pFPO.MinY() - 2f <= rScene.GroundLevel(test.x, test.z))
        {
            org = pFPO.Org;
            dir = pFPO.Dir;
            myLSpeed.x = 0;
        }
        dir.y = 0;
    }
    public override object GetInterface(DWORD id)
    {
        switch (id)
        {
            case SeaCarrier.ID: return this;
            default: return base.GetInterface(id);
        }
    }
    public SeaCarrier(BaseScene s, DWORD d, OBJECT_DATA o) : base(s, d, o) { }
    public override void HostPrepare(HostScene s, UnitSpawnData sd, Vector3 v, float a, iContact hangar)
    {
        // координаты появления
        base.HostPrepare(s, sd, v, a, hangar);
        BasePrepare();
    }
}
