using UnityEngine;

class BaseCollidingForBaseCarrier : BaseCollidingForFPO
{
    BaseCarrier m_rObject;
    public BaseCollidingForBaseCarrier(BaseCarrier myObject) : base(myObject.GetFpo())
    {
        m_rObject = myObject;
    }

    public override Vector3 GetSpeed()
    {
        return m_rObject.GetSpeed();
    }
    public override Vector3 GetSpeedFor(Vector3 Org)
    {
        return m_rObject.GetSpeed();
    }
};
