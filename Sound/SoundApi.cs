using System;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.VirtualTexturing;
using WORD = System.UInt16;
using DWORD = System.UInt32;
//DEBUG!
using IDirectSound = System.Object;
using IDirectSoundBuffer = System.Object;
using static CppFunctionEmulator;
using Unity.VisualScripting;



public class SoundApi : ISound, CommLink
{
    public const string sm_volume = "volume_master";
    public const uint cm_volume = 0x8683F079;

    public const string seditor = "sound_editor";
    public const uint ceditor = 0x6C1C7B86;
    public const uint SOUND_ENGINE_VERSION = 0x00000008;
    private ILog stdout;
    private CommandsApi cmd;
    public Sound sound;

    private int saved_volume;
    private int m_vol_ue;

    // 3d sounds
    private int num_3dsounds;
    private I3DSound manager;

    // voices engine
    public VoiceEng voice_eng;

    private void RegisterCmd()
    {
        cmd.RegisterVariable(sm_volume, this, VType.VAR_INT, "master sound volume");
        cmd.RegisterCommand(seditor, this, 0, "starts sound event editor");

    }
    private void UnRegisterCmd()
    {
        cmd.UnRegister(this);
    }

    public SoundApi()
    {
        //TODO корректно назначать голосовой движок
        //voice_eng = null;
        voice_eng = new VoiceEng(this);
        stdout = null;
        cmd = null;
        sound = null;
        manager = null;
        m_vol_ue = 8;
    }

    public void Initialize(Sound snd, SoundConfig c, ILog logout, CommandsApi pcmd)
    {
        stdout = logout;
        sound = snd;

        if ((cmd = pcmd) != null)
        {
            cmd.AddRef();
            RegisterCmd();
        }

        num_3dsounds = System.Math.Clamp((int)c.channels, 2, 32);

        // save initial volume
        //sound.Listener().GetVolume(saved_volume);

        //sound.Listener().SetDopplerFactor(0.1);

        //char buf64[64];
        //const char* shr = sound->Listener().GetTextFormat(buf64);
        //if (shr)
        //    stdout->Message("SOUND: Could not retrieve sound output format: %s", shr);
        //else
        //    stdout->Message("Sound format: %s", buf64);
    }

    public void Destroy()
    {
        voice_eng.Destroy();

        // 3d sound engine maybe null
        //SafeRelease(manager);
        manager = null;

        if (cmd != null)
        {
            UnRegisterCmd();
            cmd.Release();
        }

        //if (sound->Listener()())
        //    sound->Listener().SetVolume(saved_volume);

        //delete sound;
        sound = null;
    }

    // api
    public virtual void Update(float scale)
    {
        voice_eng.Update(scale);
    }

    public virtual I3DSound Create3D(int volume, bool can_edit = true)
    {
        if (manager == null)
        {
            SManager mngr = new SManager(sound, num_3dsounds, volume);
            if (!mngr.Initialize(can_edit))
            {
                mngr.Release();
                manager = DummySound.CreateDummy3DSound();
            }
            else manager = mngr;
        }

        manager.AddRef();
        return manager;
    }
    public virtual IStreamSoundData OpenWaveFile(string name, bool loop)
    {
        return OpenMappedFile(name, loop);

    }
    public IStreamAudio CreateStreamAudio(IStreamSoundData data, int nSeconds)
    {
        //return CreateStreamingSB(sound.Driver(), data, nSeconds);
        return CreateStreamWavUnity(data,nSeconds);
    }

    public IStreamAudio CreateStreamWavUnity(IStreamSoundData data, int nSeconds)
    {
        StreamingWavUnity ssb = new StreamingWavUnity();
        if (ssb.Initialize(data, nSeconds))
            return ssb;
        ssb.Release();
        return null;
    }
    public virtual IVoice CreateVoice(bool fake)
    {
        if (fake)
        {
            //return CreateDummyVoice(); //TODO возвращать "пустой" голос при необходимости
            return null;
        }
        else
        {
            return voice_eng;
        }
    }
    public virtual float convertVolume(int vol)
    {
        return VolumeMeter.Int2DB(vol);
    }


    public virtual void SetMasterVolume(int vol)
    {
        m_vol_ue = System.Math.Clamp(vol, 0, 10);
        sound.SetMVolume(VolumeMeter.Int2DB(m_vol_ue));
    }
    public virtual int GetMasterVolume()
    {
        //int real_value;
        //sound.Listener().GetVolume(ref real_value);
        //return m_vol_ue;
        return 10;
    }

    //commands and variables
    public virtual object OnVariable(uint var_id, object data)
    {
        int return_value = 0;
        if (data != null)
        { // set
            switch (var_id)
            {
                case cm_volume:
                    SetMasterVolume(return_value = (int)data);
                    break;
            }
        }
        else
        {
            switch (var_id)
            {
                case cm_volume:
                    return_value = GetMasterVolume();
                    break;
            }
        }
        return return_value;
    }
    public virtual void OnCommand(uint cmd_id, string arg1, string arg2)
    {
        switch (cmd_id)
        {
            case ceditor:
                if (manager != null)
                    manager.OpenEditor();
                break;
        }
    }

    public static IStreamAudio CreateStreamingSB(object ignore, IStreamSoundData i, int nSeconds)
    {
        return null;
    }
    public static IStreamSoundData OpenMappedFile(string name, bool loop)
    {
        WaveFile wf = new WaveFile();
        if (wf.Initialize(name, loop))
        {
            return wf;
        }
        wf.Release();
        return null;
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }
}
public interface I3DSoundEventController : IObject, IDisposable
{
    public Vector3 GetPosition();
    public Vector3 GetVelocity();
    public DWORD GetID();

    //public struct UpdateData
    //{
    //    public int volume;      // -10000...0  (-600 - half volume)
    //    public float frequency;   // [0..1] 
    //};

    //public bool Update(UpdateData u) { return false; }
    public bool Update(ref UpdateData u);
    void SetPosition(Vector3 inWorld);
}
public interface I3DSound : IRefMem
{
    public void SetSceneDetail(int Deltail);
    public int GetSceneDetail();

    public void Update(float scale);   // 3d sounds
    public void SetCamera(MATRIX Pos, Vector3 Speed, int mode, DWORD plr = 0);
    public void SetMode(int i, DWORD d);  // SM_COCKPIT or SM_EXTERN
    public void SetVolume(int i);
    public int GetVolume();

    public void OpenEditor();

    public I3DSoundEvent LoadEvent(
          string cls,
          string obj,
          string evt,
          bool looped, bool rnd_pitch, I3DSoundEventController i);
    //public AudioClip LoadEvent(
    //      string cls,
    //      string obj,
    //      string evt,
    //      bool looped, bool rnd_pitch, I3DSoundEventController i);
}

public interface I3DSoundEvent : IObject
{
    public void Start();
    public void Stop();
    public bool IsPlaying();
    void setName(string name);
    void UpdateController(Vector3 inWorld);
}
public class StormWaveToAudioclipWrapper : IWave
{
    private AudioClip myClip;
    private AudioSource mySource;
    private GameObject mySoundEmitter;
    public string name
    {
        get
        {
            if (myClip != null) return "Sound source for " + myClip.name;
            return mySource.name + "[no sound wave set]";
        }
    }

    public StormWaveToAudioclipWrapper(AudioClip sb)
    {
        myClip = sb;
        mySoundEmitter = new GameObject("Sound emitter");
        mySoundEmitter.transform.parent = Engine.UnityCamera.transform;
        mySoundEmitter.transform.localPosition = Vector3.zero;
        mySource = mySoundEmitter.AddComponent<AudioSource>();
        mySource.clip = myClip;
        mySoundEmitter.name = name;
        mySoundEmitter.SetActive(false);
    }

    public void AddRef()
    {
        return;
    }

    public bool IsPlaying()
    {
        return mySource.isPlaying;
    }

    public void Play(bool looped, int vol)
    {
        if (mySource.isPlaying) return;
        mySoundEmitter.SetActive(true);
        mySource.Play();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        GameObject.Destroy(mySoundEmitter);
        return 0;
    }

    public void SetFrequecy(float frq)
    {

        mySource.pitch = frq;
    }

    public void Stop()
    {
        mySource.Stop();
        mySoundEmitter.SetActive(false);
    }
}

struct WaveUpd
{
    float t;
    public WaveUpd(float _t)
    {
        t = _t;
    }
    //void operator() (WaveLE* w) { w->Update(t);}
};


#region mmeapi.h
//TODO перенести в отдельный namespace или класс
public struct WAVEFORMATEX
{
    public WORD wFormatTag;
    public WORD nChannels;
    public DWORD nSamplesPerSec;
    public DWORD nAvgBytesPerSec;
    public WORD nBlockAlign;
    public WORD wBitsPerSample;
    public WORD cbSize;
}

public struct WAVEFORMAT
{
    WORD wFormatTag;
    WORD nChannels;
    DWORD nSamplesPerSec;
    DWORD nAvgBytesPerSec;
    WORD nBlockAlign;
}

public struct PCMWAVEFORMAT
{
    WAVEFORMAT wf;
    WORD wBitsPerSample;
}
#endregion

public class StreamingWavUnity : IStreamAudio
{
    /// <summary>
    /// Максимальное значение громкости в Шторме в целых значениях. Надо подсмотреть в исходниках, каково максимальное значение.
    /// </summary>
    const int maxIntValue = 10; //TODO - Надо подсмотреть в исходниках, каково максимальное значение и указать его тут
    private WaveFile wave;
    private AudioSource source;
    private GameObject MusicPlayer;
    public void AddRef()
    {
        throw new NotImplementedException();
    }

    public int GetVolume()
    {
        return (int) source.volume * maxIntValue;
    }

    public bool Play()
    {
        MusicPlayer.SetActive(true);
        source.Play();
        return true;
    }

    public int Release()
    {
        return 0;
    }

    public void SetVolume(int vol)
    {
        source.volume = (float)vol/ maxIntValue; 
    }

    public void Stop()
    {
        if (source.isPlaying) source.Stop();

        MusicPlayer.SetActive(false);
    }

    public bool Update()
    {
        //Следовать за камерой, если плеер музыки - отдельный объект
        //MusicPlayer.transform.position = Engine.UnityCamera.transform.position;
        return true;
    }


    public bool Initialize(IStreamSoundData data, int nSeconds)
    {
        wave = (WaveFile) data;

        if (MusicPlayer !=null) GameObject.Destroy(MusicPlayer);
        Debug.Log("Music player created and attached to camera");
        MusicPlayer = new GameObject("Music player");
        MusicPlayer.transform.parent = Engine.UnityCamera.transform;
        MusicPlayer.transform.localPosition= Vector3.zero;
        source = MusicPlayer.AddComponent<AudioSource>();
        //source=Engine.UnityCamera.AddComponent<AudioSource>();
        source.clip = wave.wave;
        source.loop = wave.loop;
        return true;
    }
}