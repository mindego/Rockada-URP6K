using geombase;
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static HashFlags;

public class SceneBuilder : MonoBehaviour
{
    public string CampaignName;
    public string MissionName; //C1-3C - плутония C1-0 - первая миссия
    public Material WaterMaterial;
    public static Material StaticWaterMaterial;

    private GameHolder gh;
    public bool LoadDefaultMission = true;
    public bool LoadMission = true;

    public ShaderVariantCollection sceneShaders;

    public StormRemakeRendererConfig renedererConfig;
    [Range(0.1f, 2f)]
    public float GameSpeed = 1;
    // Start is called before the first frame update
    void Start()
    {
        //NativeDialog.Error("This copy of game is not genuine.", "Error");
        //NativeDialog.OFM();

        Init();
    }

    // Update is called once per frame
    void Update()
    {

        gh.Update(Time.deltaTime * GameSpeed, true);
        gh.Draw(null);
    }

    //private void FixedUpdate()
    //{
    //    var renderers = FindObjectsOfType<MeshRenderer>();
    //    foreach (var renderer in renderers)
    //    {
    //        Debug.Log(string.Format("{0} {1} {2}",renderer.gameObject.name, renderer.material.shader,renderer.material.shader.name));
    //    }
    //}

    private void LoadShaders()
    {
        if (sceneShaders != null) sceneShaders.WarmUp();
    }
    public void Init()
    {
        StaticWaterMaterial = WaterMaterial;

        LoadShaders();
        //_PI.dataDir = _PI.mHddPathWW;
        stormdata_dll.DllMain();
        gh = new GameHolder(new LOG());
        //gh.OpenGame(null, null);
        gh.Open(CampaignName, MissionName);
        gh.OpenGame(null, null);
        //pScene = new BaseScene(gh, false);
        //BaseScene pScene = gh.mpGame;

        //AiMissionData aiMissionData = new AiMissionData();
        //aiMissionData.Init(gh.mpDefaultMission, gh.mpMission);

        ////await CreateObjectsPrefabs();
        //DataLoaded = true;


        //if (LoadDefaultMission)
        //{
        //    Debug.Log("Processing default groups");
        //    foreach (GROUP_DATA gData in aiMissionData.GetDefGroups())
        //    {
        //        LoadGroup(gData);
        //    }
        //}

        //if (LoadMission)
        //{
        //    Debug.Log("Processing mission groups");
        //    foreach (GROUP_DATA gData in aiMissionData.GetGroups())
        //    {
        //        LoadGroup(gData);
        //    }
        //}

        //pScene.GetSceneVisualizer().PartitionActors();
    }

}

public static class NativeDialog
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    public static System.IntPtr GetWindowHandle()
    {
        return GetActiveWindow();

    }
    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, StringBuilder lParam);
    private const int WM_SETTEXT = 12;

    public static void OpenFile()
    {
        var text = @"c:\devops\dev.log";
        Debug.Log("Opening file dialog");
        SendMessage(GetWindowHandle(), WM_SETTEXT, text.Length, new StringBuilder(text));
    }

    public static void OFM()
    {
        OpenFileName ofn = new OpenFileName();
        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.filter = "All Files\0*.*\0\0";
        ofn.file = new string(new char[256]);
        ofn.maxFile = ofn.file.Length;
        ofn.fileTitle = new string(new char[64]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        ofn.initialDir = UnityEngine.Application.dataPath;
        ofn.title = "Upload Image";
        ofn.defExt = "PNG";
        ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR
        if (GetOpenFileName(ofn))
        {
            //FileDialogResult("file:///" + ofn.file);
            Debug.Log(ofn.file);
        }

    }

    public static string GetEchelonPath()
    {
        OpenFileName ofn = new OpenFileName();
        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.filter = "Echelon Game.exe\0Game.exe\0\0";
        ofn.file = new string(new char[256]);
        ofn.maxFile = ofn.file.Length;
        ofn.fileTitle = new string(new char[64]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        ofn.initialDir = UnityEngine.Application.dataPath;
        ofn.title = "Upload Image";
        ofn.defExt = "PNG";
        ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR
        if (GetOpenFileName(ofn))
        {
            //FileDialogResult("file:///" + ofn.file);
            Debug.Log("File selected: " + ofn.file);
        }
        return ofn.file;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]

    public class OpenFileName
    {
        public int structSize = 0;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;
        public String filter = null;
        public String customFilter = null;
        public int maxCustFilter = 0;
        public int filterIndex = 0;
        public String file = null;
        public int maxFile = 0;
        public String fileTitle = null;
        public int maxFileTitle = 0;
        public String initialDir = null;
        public String title = null;
        public int flags = 0;
        public short fileOffset = 0;
        public short fileExtension = 0;
        public String defExt = null;
        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;
        public String templateName = null;
        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt = 0;
        public int flagsEx = 0;
    }

    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

    [DllImport("user32.dll", SetLastError = true)]
    static extern int MessageBox(IntPtr IntPtr, String lpText, String lpCaption, uint uType);
    /// <summary>
    /// Shows Error alert box with OK button.
    /// </summary>
    /// <param name="text">Main alert text / content.</param>
    /// <param name="caption">Message box title.</param>
    public static void Error(string text, string caption)
    {
        try
        {
            MessageBox(GetWindowHandle(), text, caption, (uint)(0x00000000L | 0x00000010L));
            Debug.Log("Игра закрылась");
            Application.Quit();    // закрыть приложение
        }
        catch (Exception ex) { }
    }
}
