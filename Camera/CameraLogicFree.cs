using DWORD = System.UInt32;
using UnityEngine;
public class CameraLogicFree : CameraLogic
{
    public CameraLogicFree(CameraLogic prev) : base(prev) { }
    /// <summary>
    /// обновиться
    /// </summary>
    /// <param name=""></param>
    public override void Update(float scale)
    {
        //rVisual.Get3DSound().SetCamera(rData.myCamera, new Vector3(rData.GetCameraMoveX(), rData.GetCameraMoveY(), rData.GetCameraMoveZ()), SoundApiDefines.SM_EXTERN);
        rData.myCamera.TurnUpPrec(rData.GetCameraUpDown() * scale);
        rData.myCamera.TurnRightPrec(rData.GetCameraRightLeft() * scale);
        rData.myCamera.SetHorizontal(rData.myCamera.Dir);
        rData.myCamera.Org += (rData.myCamera.Dir * (rData.GetCameraMoveZ() * scale));
        rData.myCamera.Org += (rData.myCamera.Up * (rData.GetCameraMoveY() * scale));
        rData.myCamera.Org += (rData.myCamera.Right * (rData.GetCameraMoveX() * scale));

    }
    /// <summary>
    /// имя режима
    /// </summary>
    public override string GetName()
    {
        return "free";
    }

    public override CameraLogic GetCameraLogic(DWORD code)
    {
        return (code == CameraDefines.iCmFree ? this : base.GetCameraLogic(code));

    }
};
