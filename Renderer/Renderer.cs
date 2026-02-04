using System.Collections.Generic;
using UnityEngine;
using DWORD = System.UInt32;
using static renderer_dll;
//using NonHashList = System.Collections.Generic.List<IHashObject>;
public class Renderer : RendererApi
{
    LOG std_out;
    CommandsApi cmd;
    #region Unity
    public static UIDrawer StormUI;
    #endregion
    public SceneApi CreateScene(SceneData si)
    {
        Scene s = new SceneObject();
        if (s.Initialize(si, null)) return s;
        return null;
    }

    public bool OpenMaterialEditor()
    {
        throw new System.NotImplementedException();
    }

    public IRData LoadTMTData(ObjId id)
    {
        Debug.Log("LoadTMTData id [" + id.ToString() + "]");
        IObject myObject = Engine.CreateFpoImage(id);
        IRData ret = null;
        if (myObject != null)
        {
            //Debug.Log("Load TMNT: " + myObject.Query(IRData.ID));
            ret = (IRData)myObject.Query(IRData.ID);
            myObject.Release();
        }
        Debug.LogFormat("Loaded TMT {0}: {1}",id, ret);
        return ret;
    }

    #region переменные
    bool mShowStats;
    bool mWireFrame;
    bool Inited;
    #region helper
    //public static StormRendererData sdr;
    #endregion
    #endregion
    public Renderer()
    {
        Inited = false;
        //default_rs = 0;
        //mungecw=1;
        //bill = 0;
        //mBillLocal = 0;
        //mFont = 0;
        mShowStats = false;
        mWireFrame = false;
        //mtl_editor = null;
        //cmd = null;
        //avi_demo = false;

        //sdr = new StormRendererData();
        //sdr.Initialize();

        //unity
        InitUI();
    }

    private void InitUI()
    {
        StormUI = new UIDrawer();
    }

    public static Renderer CreateRenderer(IRendererConfig cfg, LOG _stdout, CommandsApi cmd, DWORD version)
    {
        //Debug.Log("_stdout" + _stdout);
        if (_stdout == null) _stdout = new LOG();
        if (RendererApi.RENDERER_VERSION != version)
        {
            //Log.OpenLogFile();
            //Log.Message("Error : Incompatible interface version!");
            Debug.Log("Error : Incompatible interface version!");
            return null;
        }

        Renderer r = new Renderer();
        if (r.Initialize(cfg, _stdout, cmd))
        {
            return r;
        }
        //r.Release();
        return null;   // or new DummyRenderer;
    }

    bool Initialize(IRendererConfig config, LOG _stdout, CommandsApi _cmd)
    {
        Asserts.AssertBp(!Inited);

        Debug.Log(" Initing Rendering Engine Stage begins...");

        //const char* er = d3d.Init(hwnd, config, CD3D_ZBUFFER);
        if (d3d == null) d3d = new Cd3d();
        if (d3dunity == null) d3dunity = new StormUnityRendererD3D();
        string er = d3d.Init(null, config, 0);
        //if (!StormRenderer.Open(config->GetDeveloper())) return false;
        if (!dll_data.Open(false)) return false;

        //DevOpt = config->GetDeveloper();

        Debug.Log(" Data files open success");

        std_out = _stdout;
        string THello1 = "Video : {0} {1:D}x{2:D}x{3:D}";
        string THello2 = "Video : {0} {1:D}x{2:D}";
        //std_out.Message(
        //  config->GetInWindow() ? THello2 : THello1, d3d.driver_name, d3d.Dx(), d3d.Dy(), d3d.Bpp()
        //);
        Debug.Log("Screen " + std_out);
        std_out.Message(THello1, "Unity Renderer",Screen.currentResolution.width,Screen.currentResolution.height,Screen.currentResolution.refreshRateRatio); 

        Engine.Init(config);

        // default_rs=Engine::CreateRS("default");

        if ((cmd = _cmd)!=null)
        {
            cmd.AddRef();
            //TODO Реализовать регистрацию команд рендерера
            //RegisterCmd();
        }

        return true;
    }

    public void SetGamma(float gamma)
    {
        throw new System.NotImplementedException();
    }

    public bool StartFrame()
    {
        return true;
    }

    public IBill CreateBill()
    {
        return new Bill();
    }

    public void ClearScreen(Vector4 vector4)
    {
        //STUB
    }

    public void InitFpo(Fpo fpo)
    {
        //fpo->r_flags.Set();
    }
}


public struct BillVertex
{
    public Vector2 position;
    public float depth;
    //DWORD color;
    //DWORD specular;
    public Color32 color;
    public Color32 specular;
    public Vector2 texcoords;
    //static const DWORD FVF = D3DFVF_XYZ | D3DFVF_DIFFUSE | D3DFVF_SPECULAR | D3DFVF_TEX1;

    public override string ToString()
    {
        return new Vector3(position.x, position.y, depth).ToString();
    }
}

public interface IRendererConfig
{
    //STUB!
}
public class SceneObject : Scene { };//Глупость. Но оставлено для совместимости кода.

public enum BlendMode
{
    BLEND_ADDA = 0,      // src*alpha+dst
    BLEND_ADD = 1,      // src+dst
    BLEND_BLEND = 2,      // src*alpha+dst*(1-alpha)
    BLEND_NONE = 3,      // src
    BLEND_TRANSA = 4,     // src+dst*(1-alpha)
    BLEND_TRANSC = 5,     // src+dst*(1-abs(src))
};

public class BillStyle
{
    public BlendMode blend;   // Values of BlendMode enum
    public bool write;   // write down z-buffer? 
    public bool force;   // ignore z-buffer("always-on-top")? 
    public BillStyle() { }
    public BillStyle(BlendMode _blend, bool _write, bool _force)
    {
        blend = (_blend);
        write = (_write);
        force = (_force);
    }
};
