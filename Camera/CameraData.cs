using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// общие данные по камере
/// </summary>
public class CameraData : BaseData, CommLink
{
    public const uint CAMERA_DATA = 0x7EBD1993;  // CameraData
    /// <summary>
    /// defines команд и перменных
    /// </summary>
    public const string sCmMoveForward = "cm_move_forward";
    public const uint iCmMoveForward = 0x003BE10A;
    public const string sCmMoveBackward = "cm_move_backward";
    public const uint iCmMoveBackward = 0x646F0919;
    public const string sCmMoveLeft = "cm_move_left";
    public const uint iCmMoveLeft = 0x9ABF4EE0;
    public const string sCmMoveRight = "cm_move_right";
    public const uint iCmMoveRight = 0x854B4922;
    public const string sCmMoveUp = "cm_move_up";
    public const uint iCmMoveUp = 0x0B8BB895;
    public const string sCmMoveDown = "cm_move_down";
    public const uint iCmMoveDown = 0xFC2739B3;
    public const string sCmTurnDown = "cm_turn_down";
    public const uint iCmTurnDown = 0x8FB734AA;
    public const string sCmTurnLeft = "cm_turn_left";
    public const uint iCmTurnLeft = 0xE92F43F9;
    public const string sCmTurnRight = "cm_turn_right";
    public const uint iCmTurnRight = 0xE15371EF;
    public const string sCmTurnUp = "cm_turn_up";
    public const uint iCmTurnUp = 0x790161B4;
    public const string sCmSpeed = "cm_speed";
    public const uint iCmSpeed = 0x1A041922;
    public const string sCmAngleSpeed = "cm_angle_speed";
    public const uint iCmAngleSpeed = 0xD5A8A7BE;
    public const string sCmCockpitSpeed = "cm_cockpit_speed";
    public const uint iCmCockpitSpeed = 0xD650B0B8;
    public const string sCmFov = "cm_fov";
    public const uint iCmFov = 0x25C89B53;
    public const string sCmPos = "cm_pos";
    public const uint iCmPos = 0x4D09B01E;
    public const string sCmDir = "cm_dir";
    public const uint iCmDir = 0x777B2CA2;
    public const string sCmNightVision = "cm_night_vision";
    public const uint iCmNightVision = 0x92F5DC94;

    public static readonly string[] sCmRanges = { "cm_range0", "cm_range1" };
    public static readonly uint[] iCmRanges = { 0xA8E17AE8, 0xDFE64A7E };
    public static readonly string[] sCmLdRadiuses = { "cm_ldradius0", "cm_ldradius1", "cm_ldradius2" };
    public static readonly uint[] iCmLdRadiuses = { 0x047D99B8, 0x737AA92E, 0xEA73F894 };

    ///  // CameraData
    //положение камеры
    public MATRIX myCamera;
    /// <summary>
    /// Аспект камеры, то есть отношение высоты объектива к его ширине.
    /// </summary>
    public float CameraAspect;

    // управление камерой
    private float CameraUpDown;
    private float CameraRightLeft;
    private float CameraMoveX;
    private float CameraMoveY;
    private float CameraMoveZ;
    private float CameraAngleSpeed;
    private float CameraCockpitSpeed;
    private float CameraSpeed;
    private float[] CameraRanges = new float[CameraDefines.MAX_RANGES];
    private float[] CameraLdRadiuses = new float[CameraDefines.MAX_LD_RADIUSES];
    private bool NightVision;
    public float GetCameraUpDown() { return CameraUpDown; }
    public float GetCameraRightLeft() { return CameraRightLeft; }
    public float GetCameraMoveX() { return CameraMoveX; }
    public float GetCameraMoveY() { return CameraMoveY; }
    public float GetCameraMoveZ() { return CameraMoveZ; }
    public float GetCameraAngleSpeed() { return CameraAngleSpeed; }

    public float GetCameraCockpitSpeed() { return CameraCockpitSpeed; }
    public float GetCameraSpeed() { return CameraSpeed; }
    public float GetUserCameraRange() { return myCameraRangesUser; }
    public float GetCameraRange() { return CameraRanges[CameraDefines.MAX_RANGES - 1]; }
    public void SetCameraRange(float range) { CameraRanges[CameraDefines.MAX_RANGES - 1] = range; }
    public float[] GetCameraRanges() { return CameraRanges; }
    public float GetCameraLdRadius() { return CameraLdRadiuses[0]; }
    public bool GetNightVision() { return NightVision; }

    private float myCameraRangesUser;

    // объекты привязки камеры
    protected DWORD hRef;
    public BaseObject attachedObject;
    //public iContact GetContact()
    //{
    //    return rScene.GetContact(hRef);
    //}
    public void SetRef(DWORD h) { hRef = h; }
    public DWORD GetRef() { return hRef; }
    // от BaseData
    protected SceneVisualizer pVis;
    public CameraData(BaseScene s) : base(s, CameraDefines.CAMERA_DATA)
    {
        //int i;
        pVis = rScene.GetSceneVisualizer();
        myCamera = new MATRIX();

        //CameraAspect = Storm.Math.aspect(Storm.Math.GRD2RD(90f)); //Так в оригинале
        //CameraAspect = Screen.height / Screen.width;
        //CameraAspect = Storm.Math.aspect(Storm.Math.GRD2RD(Engine.UnityCamera.fieldOfView));
        CameraAspect = 1/Engine.UnityCamera.aspect; //Да, странное, но в коде "Шторма" аспект почему-то наоборот - отношение высота, делённая на ширину.
        //CameraAspect = 90f;
        CameraUpDown = 0f;
        CameraRightLeft = 0f;
        CameraMoveX = 0f;
        CameraMoveY = 0f;
        CameraMoveZ = 0f;
        CameraAngleSpeed = Storm.Math.GRD2RD(90f);
        CameraCockpitSpeed = Storm.Math.GRD2RD(180f);
        CameraSpeed = 200f;
        CameraRanges[0] = .2f;
        CameraRanges[1] = pVis.GetViewDist();
        myCameraRangesUser = CameraRanges[1];
        //pVis.recalcMaxDist(CameraRanges[1], myCameraRangesUser);

        //CameraLdRadiuses[0] = 100 * (1 - pVis.GetSceneConfig().v_objects_detail);
        //CameraLdRadiuses[1] = 4f;
        //CameraLdRadiuses[2] = 1f;

        NightVision = false;
        // объекты привязки камеры
        hRef = Constants.THANDLE_INVALID;

        rScene.GetCommandsApi().RegisterTrigger(sCmMoveForward, this);
        rScene.GetCommandsApi().RegisterTrigger(sCmMoveBackward, this);
        rScene.GetCommandsApi().RegisterTrigger(sCmMoveLeft, this);
        rScene.GetCommandsApi().RegisterTrigger(sCmMoveRight, this);
        rScene.GetCommandsApi().RegisterTrigger(sCmMoveUp, this);
        rScene.GetCommandsApi().RegisterTrigger(sCmMoveDown, this);
        rScene.GetCommandsApi().RegisterTrigger(sCmTurnLeft, this);
        rScene.GetCommandsApi().RegisterTrigger(sCmTurnRight, this);
        rScene.GetCommandsApi().RegisterTrigger(sCmTurnUp, this);
        rScene.GetCommandsApi().RegisterTrigger(sCmTurnDown, this);
        rScene.GetCommandsApi().RegisterVariable(sCmSpeed, this, VType.VAR_FLOAT);
        rScene.GetCommandsApi().RegisterVariable(sCmAngleSpeed, this, VType.VAR_FLOAT);
        rScene.GetCommandsApi().RegisterVariable(sCmCockpitSpeed, this, VType.VAR_FLOAT);
        rScene.GetCommandsApi().RegisterVariable(sCmFov, this, VType.VAR_FLOAT);
        rScene.GetCommandsApi().RegisterVariable(sCmPos, this, VType.VAR_VECTOR);
        rScene.GetCommandsApi().RegisterVariable(sCmDir, this, VType.VAR_VECTOR);
        rScene.GetCommandsApi().RegisterVariable(sCmNightVision, this, VType.VAR_INT);
        //for (i = 0; i < CameraDefines.MAX_RANGES; i++)
        //{
        //    rScene.GetCommandsApi().RegisterVariable(sCmRanges[i], this, VType.VAR_FLOAT);
        //    pVis.GetSceneApi().SetViewDist(i, CameraRanges[i]);
        //}
        //for (i = 0; i < CameraDefines.MAX_LD_RADIUSES; i++)
        //{
        //    rScene.GetCommandsApi().RegisterVariable(sCmLdRadiuses[i], this, VType.VAR_FLOAT);
        //    pVis.GetSceneApi().SetLdRadius(i, CameraLdRadiuses[i]);
        //}

    }
    ~CameraData()
    {
        Dispose();
    }

    public void Dispose()
    {
        rScene.GetCommandsApi().UnRegister(this);
    }

    // от CommLink
    public virtual object OnVariable(uint code, object data)
    {
        float tmp;
        Vector3 vect_temp=Vector3.zero;
        switch (code)
        {
            case iCmSpeed:
                if (data!=null)
                    CameraSpeed = Mathf.Clamp((float)data, .1f, 5000.0f);
                tmp = CameraSpeed;
                return tmp;
            case iCmAngleSpeed:
                if (data!=null)
                    CameraAngleSpeed = Storm.Math.GRD2RD(Mathf.Clamp((float)data, 10.0f, 180.0f));
                tmp = Storm.Math.RD2GRD(CameraAngleSpeed);
                return tmp;
            case iCmCockpitSpeed:
                if (data!=null)
                    CameraCockpitSpeed = Storm.Math.GRD2RD(Mathf.Clamp((float)data, 0.0f, 720.0f));
                tmp = Storm.Math.RD2GRD(CameraCockpitSpeed);
                return tmp;
            case iCmFov:
                if (data!=null)
                    CameraAspect = Storm.Math.aspect(Storm.Math.GRD2RD(Mathf.Clamp((float)data, 20.0f, 120.0f)));
                tmp = Storm.Math.RD2GRD(Storm.Math.fov(CameraAspect));
                return tmp;
            case iCmPos:
                if (data!=null)
                    myCamera.Org = (Vector3)data;
                return myCamera.Org;
            case iCmDir:
                myCamera.Vectors2Angles(ref vect_temp.x, ref vect_temp.y, ref vect_temp.z);
                vect_temp.x = Storm.Math.RD2GRD(vect_temp.x);
                vect_temp.y = Storm.Math.RD2GRD(vect_temp.y);
                vect_temp.z = Storm.Math.RD2GRD(vect_temp.z);
                return vect_temp;
            case iCmNightVision:
                if (data!=null)
                    NightVision = (int)data ==1;
                return NightVision;
            default:
                {
                    //уровнем детальностей будет заведовать Unity
                    //int i;
                    //for (i = 0; i < MAX_RANGES; i++)
                    //{
                    //    if (code == iCmRanges[i])
                    //    {
                    //        if (data != 0)
                    //        {
                    //            CameraRanges[i] = ClampFloat(*((float*)data), 0.f, 8000.f);
                    //            if (i == 1)
                    //            {
                    //                myCameraRangesUser = CameraRanges[1];
                    //                pVis->recalcMaxDist(CameraRanges[1], myCameraRangesUser);
                    //            }
                    //            pVis->GetSceneApi()->SetViewDist(i, CameraRanges[i]);
                    //        }
                    //        tmp = CameraRanges[i];
                    //        return &tmp;
                    //    }
                    //}
                    //for (i = 0; i < MAX_LD_RADIUSES; i++)
                    //{
                    //    if (code == iCmLdRadiuses[i])
                    //    {
                    //        if (data != 0)
                    //            pVis->GetSceneApi()->SetLdRadius(i, ClampFloat(*((float*)data), .0f, 1000.f));
                    //        tmp = pVis->GetSceneApi()->GetLdRadius(i);
                    //        return &tmp;
                    //    }
                    //}
                }
                break;
        }
        return null;
    }
    public void OnCommand(uint code, string arg1, string arg2)
    {
        //Да, в оригинале (в релизе) из-за директив компилятора сворачивается до этого:
        switch (code)
        {

        }
    }

    public void OnTrigger(uint code, bool on)
    {
        //Debug.Log("code " + code.ToString("X8"));
        switch (code)
        {
            case iCmMoveForward:
                CameraMoveZ = on ? CameraSpeed : 0;
                return;
            case iCmMoveBackward:
                CameraMoveZ = on ? -CameraSpeed : 0;
                return;
            case iCmMoveRight:
                CameraMoveX = on ? CameraSpeed : 0;
                return;
            case iCmMoveLeft:
                CameraMoveX = on ? -CameraSpeed : 0;
                return;
            case iCmMoveUp:
                CameraMoveY = on ? CameraSpeed : 0;
                return;
            case iCmMoveDown:
                CameraMoveY = on ? -CameraSpeed : 0;
                return;
            case iCmTurnUp:
                CameraUpDown = on ? CameraAngleSpeed : 0;
                return;
            case iCmTurnDown:
                CameraUpDown = on ? -CameraAngleSpeed : 0;
                //if (on) Engine.DumpHashTexture = true; //Debug: Дамп хэша в виде текстуры
                return;
            case iCmTurnRight:
                CameraRightLeft = on ? CameraAngleSpeed : 0;
                return;
            case iCmTurnLeft:
                CameraRightLeft = on ? -CameraAngleSpeed : 0;
                return;
        }

    }

    /// <summary>
    /// объекты привязки камеры
    /// </summary>
    /// <returns></returns>
    public iContact GetContact()
    {
        return rScene.GetContact(hRef);
    }
}
