using DWORD = System.UInt32;
using UnityEngine;

public class CameraLogicStrategic : CameraLogic
{
    private const float MAXHEIGHT = 2000;
    private float CameraHeight = 500;
    private Vector3 CameraAngle = Vector3.forward;

    private bool mUseControls;
    public CameraLogicStrategic(CameraLogic prev) : base(prev)
    {
        camPos = new MATRIX(rData.myCamera);
        Engine.UnityCamera.orthographic = true;
        Engine.UnityCamera.orthographicSize = 200f;

        Quaternion q = Quaternion.Euler(45, 45, 0);
        //camPos.Dir = Vector3.forward + Vector3.down * 0.5f + Vector3.right * 0.5f;

        //Quaternion q = Quaternion.LookRotation(camPos.Dir);
        //camPos.Up = Vector3.up * 0.5f + Vector3.forward;
        camPos.Dir = q * Vector3.forward;
        camPos.Up = q * Vector3.up;
        camPos.Right= Vector3.Cross(camPos.Up, camPos.Dir);

        PlayerInterface player = rScene.GetSceneVisualizer().GetPlayerInterface();
        if (player != null) ((PlayerInterfaceLocalCraft)player).SetAutopilotMode(true);

    }

    public override string GetName()
    {
        return "strategic";
    }

    public override CameraLogic GetCameraLogic(DWORD code)
    {
        return (code == CameraDefines.iCmStrategic ? this : base.GetCameraLogic(code));
    }

    protected BaseCraft GetCraft(DWORD h) //TODO Перенести в CameraLogic?
    {
        iContact c = rScene.GetContact(h);
        return (c != null ? (BaseCraft)c.GetInterface(BaseCraft.ID) : null);
    }
    MATRIX camPos;
    public override void Update(float scale)
    {
        //BaseCraft pCraft = GetCraft(rData.GetRef());

        float mySpeed = 10f;

        //float mDelta = Input.mouseScrollDelta.y;
        //if (mDelta != 0)
        //{
        //    CameraHeight -= mDelta * scale * 500 * (CameraHeight / 200);
        //    CameraHeight = Mathf.Clamp(CameraHeight, 1, MAXHEIGHT);
        //}
        //camPos.TurnRightPrec(rData.GetCameraRightLeft() * scale);
        //camPos.SetHorizontal(camPos.Dir);

        //camPos.Org += (camPos.Dir * (rData.GetCameraMoveZ() * scale));
        //camPos.Org += (camPos.Right * (rData.GetCameraMoveX() * scale));
        camPos.Org += (Vector3.forward * (rData.GetCameraMoveZ() * scale));
        //Debug.LogFormat("Camera moving MoveX {0} MoveY {1} MoveZ {2} RightLeft {3}", rData.GetCameraMoveX(), rData.GetCameraMoveY(), rData.GetCameraMoveZ(), rData.GetCameraRightLeft());
        camPos.Org += (Vector3.right * (rData.GetCameraRightLeft()*133f * scale));

        float groundLevel = rScene.GetGroundLevel(camPos.Org);
        camPos.Org.y = groundLevel + CameraHeight;

        rData.myCamera.Set(camPos);
        //        rData.myCamera.Org.y = groundLevel + CameraHeight;
        //Engine.UnityCamera.orthographicSize = Mathf.Lerp(100f, 5f, CameraHeight / MAXHEIGHT);

        //float flatView = Mathf.Lerp(0, .99f, CameraHeight / MAXHEIGHT);
        //rData.myCamera.Dir = (1 - flatView) * rData.myCamera.Dir + flatView * Vector3.down;
        //rData.myCamera.Dir = (1 - flatView) * camPos.Dir + flatView * Vector3.down;


        base.Update(scale);
    }


    public override void Dispose()
    {
        Engine.UnityCamera.orthographic = false;
        PlayerInterface player = rScene.GetSceneVisualizer().GetPlayerInterface();
        if (player != null) ((PlayerInterfaceLocalCraft)player).SetAutopilotMode(false);
        base.Dispose();
    }

}