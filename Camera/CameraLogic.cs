using System.Collections;
using TMPro;
using UnityEngine;
using DWORD = System.UInt32;
public class SoundApiDefines
{
    public const uint SM_COCKPIT = 0x00000000;
    public const uint SM_EXTERN = 0x00000001;
    public const uint SM_NOCHAGE = 0xFFFFFFFF;
}
/// <summary>
/// базовый режим камеры
/// </summary>
public class CameraLogic
{
    // взаимодействие с хозяевами
    public BaseScene rScene;
    protected SceneVisualizer rVisual;
    protected CameraData rData;
    protected DWORD mDesiredConfig;
    protected bool mDesiredInfra;
    public CameraLogic(SceneVisualizer v)
    {
        rScene = v.rScene;
        rVisual = v;
        rData = v.GetCameraData();
        //mDesiredConfig = rVisual.GetSceneNormalConfig();
        mDesiredInfra = false;

    }
    public CameraLogic(CameraLogic c)
    {
        rScene = c.rScene;
        rVisual = c.rVisual;
        rData = c.rData;
        //mDesiredConfig = rVisual.GetSceneNormalConfig();
        mDesiredInfra = false;

    }

    public virtual void Dispose() {
        Debug.Log("Disposed of CameraLogic " + this);
    }
    //virtual ~CameraLogic();
    /// <summary>
    /// обновиться
    /// </summary>
    /// <param name="f"></param>
    public virtual void Update(float scale) { 
        rVisual.Get3DSound().SetCamera(rData.myCamera, Vector3.zero, (int)SoundApiDefines.SM_EXTERN); 
    }
    /// <summary>
    /// отрисовать сцену в очке
    /// </summary>
    /// <param name="f"></param>
    public virtual void Draw(float[] Viewport)
    {
        // проверка высоты над террейном
        float alt = rScene.SurfaceLevel(rData.myCamera.Org.x, rData.myCamera.Org.z) + 2f;
        if (rData.myCamera.Org.y < alt) rData.myCamera.Org.y = alt;
        // проверка конфига сцены
        //rVisual.GetSceneApi().SetViewDist(0, rData.GetCameraRanges()[0]);
        //rVisual.GetSceneApi().SetViewDist(1, rData.GetCameraRanges()[1]);
        //if (rScene.IsInSfg(rData.myCamera.Org, true))
        //{
        //    rVisual.SetSceneConfig(rVisual.GetSceneSfgConfig(), false);
        //}
        //else
        //{
        //    rVisual.SetSceneConfig(mDesiredConfig, mDesiredInfra);
        //}

        //// проверка аспекта
        float max_aspect = Storm.Math.aspect(Storm.Math.GRD2RD(10));
        //float min_aspect = Storm.Math.aspect(Storm.Math.GRD2RD(120));
        float min_aspect = 1080 / 1920;
        rData.CameraAspect = Mathf.Clamp(rData.CameraAspect, min_aspect, max_aspect);

        rVisual.GetSceneApi().SetCamera(rData.myCamera);
        rVisual.GetSceneApi().SetViewport(rData.CameraAspect, Viewport);
        rVisual.GetSceneApi().Draw();
        Draw2(Viewport);

        rVisual.GetHud().Draw(Viewport);

    }

    

    public virtual void Draw2(float[] f) { }// доп отрисовка

    /// <summary>
    /// Имя режима
    /// </summary>
    /// <returns></returns>
    public virtual string GetName()
    {
        return "none";
    }

    /// <summary>
    /// смена режимов камеры
    /// </summary>
    /// <param name="prev"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public static CameraLogic Create(CameraLogic prev, DWORD code)
    {
        switch (code)
        {
            case CameraDefines.iCmCockpit:
            case CameraDefines.iCmCockpitTracking:
                return new CameraLogicCockpit(prev, code); 
            case CameraDefines.iCmAttached:
            case CameraDefines.iCmAttachedXZ:
            case CameraDefines.iCmOrbite:
                return new CameraLogicAttached(prev, code);
            case CameraDefines.iCmFree:
                return new CameraLogicFree(prev);
            case CameraDefines.iCmMap:
                //return new CameraLogicMap(prev);
                return new CameraLogicStrategic(prev);
            //case CameraDefines.iCmTactical:
            //case CameraDefines.iCmTacticalInversed:
            //    return new CameraLogicTactical(prev, code);
            //case CameraDefines.iCmTracking:
            //case CameraDefines.iCmTV:
            //    return new CameraLogicTracking(prev, code);
            //case CameraDefines.iCmAuto:
            //    return new CameraLogicAuto(prev);
            case CameraDefines.iCmNone:
                return new CameraLogic(prev);
            //case CameraDefines.iCmStrategic:
            //    return new CameraLogicStrategic(prev);
            //case CameraDefines.iCmManual:
            //    return new CameraLogicManual(prev, code);
            case CameraDefines.iCmFPS:
                return new CameraLogicFPS(prev);
            default:
                return null;

        }
    }


    public virtual CameraLogic GetCameraLogic(DWORD code)
    {
        return code == CameraDefines.iCmNone ? this : null;
    }
}

public struct CamViewPort
{
    float x, y, w, h;


    public void convertFrom(float[] ParentVP, ref float[] DestVp)
    {
        float x_scale = ParentVP[2];
        float y_scale = ParentVP[3];
        DestVp[0] = ParentVP[0] + x * x_scale;
        DestVp[1] = ParentVP[1] + y * y_scale;
        DestVp[2] = w * x_scale;
        DestVp[3] = h * y_scale;
    }
}