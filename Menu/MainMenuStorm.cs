#define _PYTHON_MENU_
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using static DLLEmulation;
using DWORD = System.UInt32;


public class MainMenuStorm_dll : DLLEmulation
{
    //HANDLE _hdll = 0;
    static HMODULE _hdll = null;
    public const string TexturesPath = "Graphics\\textures.dat";

    public static int DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID aborted)
    {
        switch (ul_reason_for_call)
        {
            case DLL_PROCESS_ATTACH:
# if _DEBUG
                dumpSoundPath();
#endif
                //DisableThreadLibraryCalls(hModule);
                //HeapInit();
                _hdll = hModule;
                //return registerClasses() &&
                return PythonIntegrationW.init("Python") && PythonIntegrationW.addPath("Modules") ? 1:0;

            case DLL_PROCESS_DETACH:
# if _PYTHON_MENU_
                PythonIntegrationW.done();
#endif
                //unregisterClasses();
                //HeapDone(hModule);
                break;
        }
        return 1;
    }

# if _DEBUG
    void dumpSoundPath()
    {
        char name[MAX_PATH];
        if (GetModuleFileName(GetModuleHandle("sound.dll"), name, sizeof(name)))
            GetLog().Message("Sound: %s", name);
        else
            GetLog().Message("Sound: $$$invalid$$$");
    }
#endif  

    public static IStreamSoundData openMusic(string name, ISound s)
    {
        EngineDebug.DebugConsoleFormat("Playing music {0} using ISound {1}", ProductDefs.GetPI().getHddFile(name),s == null? "null":s);
        IStreamSoundData d = s.OpenWaveFile(ProductDefs.GetPI().getHddFile(name), true);
        Debug.LogFormat("Data {0}", d==null? "null":d);
        return d!=null ? d : s.OpenWaveFile(ProductDefs.GetPI().getCdFile(name), true);
    }
}

public class WaveFile : IStreamSoundData //TODO переименовать во что-то более внятное
{
    public AudioClip wave { get; private set; }


    int sam_size;  // sizeof(sample) in bytes
    int file_size; // size of wave in samples
    int cursor;    // in samples

    public bool loop { get; private set; }

    public bool OpenWaveFile(string name)
    {
        name = "file://" + name;
        //TODO на далёкое будущее - реализовать возможность загрузки музыки с удалённого сервера.
        var www = UnityWebRequestMultimedia.GetAudioClip(name, AudioType.WAV);
        www.SendWebRequest();
        //TODO во время ожидания можно было бы сделать что-то полезное.
        while (!www.isDone)
        {
            //wait for it...
        }
        Debug.LogFormat("Loading music name {0} res {1}",name, www.result);
        if (www.result != UnityWebRequest.Result.Success) return false;
        wave = DownloadHandlerAudioClip.GetContent(www);
        return true;
    }
    public bool FindWaveData() { return false; } //TODO при необходимости реализовать

    public bool Initialize( string name , bool _loop)
    {
        if (!File.Exists(name)) return false;
        
        loop = _loop;
        return OpenWaveFile(name);
    }

    public WaveFile()
    {
        //file(0)
        //wave(0),
        //fmt(0),
        //data(0),
        //file_size(0),
        //cursor(0),
        //sam_size(0),
        loop = false;
    }
    public void Destroy()
    {
        //Ничего не делаем, пускай сборщик мусора мусор убирает.
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int GetData(int num_samples, object pdata, int cbytes)
    {
        throw new System.NotImplementedException();
    }

    public void GetDataFormat(ref PCMWAVEFORMAT data)
    {
        throw new System.NotImplementedException();
    }

    public void GetStreamInfo(ref SSDInfo i)
    {
        i.Length = file_size;
    }

    public int Release()
    {
        return 0;
    }

    public int SkipData(int num_samples)
    {
        throw new System.NotImplementedException();
    }
}