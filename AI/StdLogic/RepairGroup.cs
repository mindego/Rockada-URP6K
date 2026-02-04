#define _DEBUG
using System;
//public class StandartStaticGroupAi : StdGroupAi { new public const uint ID = 0xA3A6554F; }
public class RepairGroup : StdGroupAi { 
    new public const uint ID = 0x52247BC6;

    public override void SetGroupDisappear(bool base_ret, uint baseId, float dist, uint ultimate, string base_name)
    {
        throw new NotImplementedException();
    }
}
