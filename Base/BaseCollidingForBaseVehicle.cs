using UnityEngine;

public class BaseCollidingForBaseVehicle : BaseCollidingForFPO
{
    public BaseCollidingForBaseVehicle(BaseVehicle myobject ) : base(myobject.GetFpo())
    {
        m_rObject = myobject;
    }
    
    public override Vector3 GetSpeed()
    {
        return m_rObject.GetSpeed();
    }
    public override Vector3 GetSpeedFor(Vector3 Org)
    {
        return m_rObject.GetSpeed();
    }

    private BaseVehicle m_rObject;
};