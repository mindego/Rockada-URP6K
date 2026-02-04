using DWORD = System.UInt32;
using static CameraDefines;
using static SoundApiDefines;
using UnityEngine;

public class CameraLogicCockpit : CameraLogicAttached
{
    readonly DWORD CockpitCode = Hasher.CodeString("cockpit");

    // от CameraLogic
    public CameraLogicCockpit(CameraLogic prev, DWORD c) : base(prev, iCmAttached)
    {
        Code = c;
        rOwnData = rVisual.GetCameraDataCockpit();
        LastHandle = Constants.THANDLE_INVALID;
        pCockpit = null;

        rVisual.GetScene().showSfgs(true);
    }
    ~CameraLogicCockpit()
    {
        Dispose();
    }

    public override void Dispose()
    {
        rVisual.GetScene().showSfgs(false);
        rOwnData.SetCraft(null);
        ResetLastCraft();
    }
    public override void Update(float scale)
    {
        BaseCraft pCraft = GetCraft(rData.GetRef());
        rOwnData.SetCraft(pCraft);
        if (pCraft == null)
        {
            ResetLastCraft();
            LastHandle = Constants.THANDLE_INVALID;
            mDesiredConfig = rVisual.GetSceneNormalConfig();
            mDesiredInfra = false;
            base.Update(scale);
            return;
        }
        // если поменялся крафт
        if (LastHandle != pCraft.GetHandle())
        {
            ResetLastCraft();
            LastHandle = pCraft.GetHandle();
            pCockpit = (FPO)pCraft.GetFpo().GetSubObject(CockpitCode);
        }
        // ставим имидж кабины
        if (pCockpit != null)
        {
            if (pCockpit.SetImage(3, (int)RoFlags.FSI_EQUAL_LINKS, pCockpit.Link, false) == 0)
            {
                pCockpit = null;
                ResetLastCraft();
            }
        }
        else Debug.Log("Cockpit image not found");
        // определяем положение камеры
        MATRIX pos = new MATRIX(pCraft.GetPosition());
        //Storm.Matrix pos = pCraft.GetPosition();
        iContact r = rData.GetContact();
        if (r == null) return;
        iContact pTarget = null;
        iWeaponSystemDedicated ws = (iWeaponSystemDedicated)pCraft.GetInterface(iWeaponSystemDedicated.ID);
        if (ws.GetTarget() != null && ws.GetTarget().GetAge() == 0)
            pTarget = ws.GetTarget();
        bool LookBehind = false;
        // если следим за целью и целеуказание работает
        if (Code == iCmCockpitTracking && pTarget != null)
        {
            // слежение за целью
            Vector3 d = pCraft.Dt().ViewDelta;
            //rData.myCamera = pos;
            //rData.myCamera.Org += pos.Dir * d.z + pos.Right * d.x + pos.Up * d.y;
            //d = pTarget.GetOrg() - pos.Org; d.Normalize();
            //d.Set(Vector3.Dot(d, pos.Right), Vector3.Dot(d, pos.Up), Vector3.Dot(d, pos.Dir));

            rData.myCamera.Set(r.GetOrg(), r.GetDir(), r.GetUp());
            rOwnData.Smooth(Mathf.Asin(d.y), (Mathf.Abs(d.y) < .99 ? Mathf.Atan2(d.x, d.z) : 0), rData.GetCameraCockpitSpeed() * scale);
        }
        else
        {
            // если смотрим строго назад
            if (rData.GetCameraUpDown() < 0 && rData.GetCameraRightLeft() == 0)
            {
                // если вид назад сломан
                if (pCraft.IsInSF() == true)
                {
                    // вид через левое плечо
                    Vector3 d = pCraft.Dt().ViewDelta;
                    //rData.myCamera = pos;
                    //rData.myCamera.Org += pos.Dir * d.z + pos.Right * d.x + pos.Up * d.y;
                    
                    rData.myCamera.Set(r.GetOrg(), r.GetDir(), r.GetUp());
                    rData.myCamera.Org += r.GetOrg() * d.z + r.GetRight() * d.x + r.GetUp() * d.y;
                    rOwnData.Smooth(0, Mathf.PI, rData.GetCameraCockpitSpeed() * scale);
                }
                else
                { // иначе - rear view camera
                    Vector3 d = pCraft.Dt().BackViewDelta;
                    rData.myCamera.Org = pos.Org + (pos.Dir * d.z + pos.Right * d.x + pos.Up * d.y);
                    rData.myCamera.Dir = -pos.Dir;
                    rData.myCamera.Up = pos.Up;
                    rData.myCamera.Right = -pos.Right;
                    
                    //rData.myCamera.Set(r.GetOrg(), r.GetDir(), r.GetUp());
                    rOwnData.Smooth(0, 0, rData.GetCameraCockpitSpeed() * scale);
                    LookBehind = true;
                }
            }
            else
            {
                // обычные виды
                Vector3 d = pCraft.Dt().ViewDelta;
                rData.myCamera = pos;
                rData.myCamera.Org += pos.Dir * d.z + pos.Right * d.x + pos.Up * d.y;

                //rData.myCamera.Set(r.GetOrg(), r.GetDir(), r.GetUp());
                //rData.myCamera.Org += r.GetDir() * d.z + r.GetRight() * d.x + r.GetUp() * d.y;
                float cay = 0;
                float cax = 0;
                if (rData.GetCameraUpDown() > 0)
                {
                    if (rData.GetCameraRightLeft() > 0) cay = Storm.Math.GRD2RD(45);
                    else
                      if (rData.GetCameraRightLeft() < 0) cay = Storm.Math.GRD2RD(-45);
                    else cax = Storm.Math.GRD2RD(45);
                }
                else
                {
                    if (rData.GetCameraUpDown() < 0)
                    {
                        if (rData.GetCameraRightLeft() > 0) cay = Mathf.PI;
                        else
                          if (rData.GetCameraRightLeft() < 0) cay = -Mathf.PI;
# if _DEBUG
                        else _asm int 3;
#endif
                    }
                    else
                    {
                        if (rData.GetCameraRightLeft() > 0) cay = Storm.Math.GRD2RD(90);
                        else
                          if (rData.GetCameraRightLeft() < 0) cay = Storm.Math.GRD2RD(-90);
                    }
                }
                if (rData.GetCameraMoveZ() > 0)
                {
                    if (rData.GetCameraRightLeft() != 0 || rData.GetCameraUpDown() != 0)
                    {
                        cax = Storm.Math.GRD2RD(45);
                    }
                    else
                    {
                        cax = Storm.Math.GRD2RD(90);
                    }
                }
                else
                {
                    if (rData.GetCameraMoveZ() < 0)
                    {
                        if (rData.GetCameraRightLeft() != 0 || rData.GetCameraUpDown() != 0) cax = -Storm.Math.GRD2RD(45);
                    }
                }
                rOwnData.Smooth(cax, cay, rData.GetCameraCockpitSpeed() * scale);
            }
        }

        // поворачиваем камеру
        if (LookBehind == false)
        {
            rData.myCamera.TurnRightPrec(rOwnData.GetAngleY()); 
            rData.myCamera.TurnUpPrec(rOwnData.GetAngleX());
        }
        rOwnData.Update(scale, LookBehind);
        // положение звуковой камеры
        rVisual.Get3DSound().SetCamera(rData.myCamera, pCraft.GetSpeed(), (int)SM_COCKPIT, (DWORD)pCraft.GetFpo()); //TODO Возможно, здесь требуется передавать сам объект, а не его хэш
        // Кроме того, звуковая камера ставится здесь, а обсчитывается положение реальной камеры с приёмником звука в другом месте

        // конфиг сцены
        mDesiredConfig = rVisual.GetSceneNormalConfig();
        mDesiredInfra = false;
        // если работают телекамеры
        if (pCraft.IsInSF() == false)
        {
            // если включено ночное видение
            if (rData.GetNightVision() == true)
            {
                mDesiredConfig = rVisual.GetSceneNvConfig();
                mDesiredInfra = true;
            }
            // если смотрим в камеру назад
            if (LookBehind)
                mDesiredInfra = true;
        }
    }
    public override string GetName()
    {
        return (Code == iCmCockpit ? "cockpit" : "cockpit_tracking");
    }
    public override CameraLogic GetCameraLogic(DWORD code)
    {
        return ((code == iCmCockpit || code == iCmCockpitTracking) ? this : base.GetCameraLogic(code));
    }

    public override void Draw2(float[] ViewPort)
    {
        if (rOwnData.mCam2Active)
        {
            float[] ViewPort2 = new float[4];
            rOwnData.mCamViewPort.convertFrom(ViewPort, ref ViewPort2);

            rVisual.GetSceneApi().SetCamera(rOwnData.mCamera2);
            rVisual.GetSceneApi().SetViewport(rOwnData.mCameraAspect2, ViewPort2);
            rVisual.GetSceneApi().SetVision(SCENE_VISION.SV_INFRA);
            rVisual.GetSceneApi().Draw();
        }
    }

    // own
    protected DWORD LastHandle;
    protected DWORD Code;
    protected CameraDataCockpit rOwnData;
    protected FPO pCockpit;
    protected BaseCraft GetCraft(DWORD h)
    {
        iContact c = rScene.GetContact(h);
        return (c != null ? (BaseCraft)c.GetInterface(BaseCraft.ID) : null);
    }
    protected void ResetLastCraft()
    {
        if (LastHandle == Constants.THANDLE_INVALID) return;
        BaseCraft pCraft = GetCraft(LastHandle);
        if (pCraft == null) return;
        pCraft.GetRoot().ResetImage();
    }

    protected void SetVision(int vis_mode)
    {
        //Реализовать установку режима кокпитной камеры
    }
};

