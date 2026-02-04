using UnityEngine;
public class CameraLogicFPS : CameraLogic
{
    private GameObject FPSunit;
    public CameraLogicFPS(CameraLogic prev) : base(prev) {
        if (FPSunit == null) CreateFPSUnit();
    }

    private GameObject CreateFPSUnit()
    {
        FPSunit = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Rigidbody rd = FPSunit.AddComponent<Rigidbody>();
        rd.MovePosition(Engine.ToCameraReference(rData.myCamera.Org));
        rd.MoveRotation(Quaternion.LookRotation(rData.myCamera.Dir, Vector3.up));
        rd.mass = 10;
        rd.constraints = RigidbodyConstraints.FreezeRotationZ;
        
        return FPSunit;
    }
    public override void Update(float scale)
    {
        base.Update(scale);
    }
    public override string GetName()
    {
        return "fps";
    }

    public override CameraLogic GetCameraLogic(uint code)
    {
        return (code == CameraDefines.iCmFPS ? this : base.GetCameraLogic(code));
    }
}
