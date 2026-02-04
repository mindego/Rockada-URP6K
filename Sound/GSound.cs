using System;
using UnityEngine;
using static CppFunctionEmulator;
using DWORD = System.UInt32;
//using HRESULT = System.UInt32;

public class GSound : I3DSoundEvent
{
    public const int GSF_LOOPED = 0x00000001;
    public const int GSF_RND_RATE = 0x00000002;

    public const int GSF_ALL = (~(GSF_LOOPED | GSF_RND_RATE));
    public const int GSF_INDLL = 0x10000000;     // managed
    /// <summary>
    /// Эмуляция D3Dsound
    /// </summary>
    public const int DSBPLAY_LOOPING = 1; 

    // debug flags
#if _DEBUG
public const uint GSF_MODEL_OK     =        0x00000010;
#endif

    //Эмуляция DirectSound3D
    public const uint DS3D_DEFERRED = 0x1;

    public void setName(string name)
    {
        //if (SoundEmitter != null) SoundEmitter.name = name + " " + sample[0].h.clip.name;
    }
    // commands
    public const int GSC_RESTART = 0x00010000;    // command to restart (if playing ...)
    public const int GSC_STOP = 0x00100000;   // command to stop

    //typedef I3DSoundEventController::UpdateData UpdateData;

    //Отключено ввиду исползования List<GSound> вместо <GSound>
    //public GSound next;       // pointer to next 
    //                          // 1. in start queue
    //                          // 2. in looped list

    //private GameObject FlagsShowerHolder;
    //private ShowFlags FlagsShower;
    public class ShowFlags : MonoBehaviour
    {
        public bool GSF_LOOPED, GSF_RND_RATE, GSF_INDLL, GSC_RESTART, GSC_STOP;
        
        public void UpdateFlags(Flags flags)
        {
            GSF_LOOPED = flags.Get(GSound.GSF_LOOPED) != 0;
            GSF_RND_RATE= flags.Get(GSound.GSF_RND_RATE) != 0;
            GSF_INDLL = flags.Get(GSound.GSF_INDLL) != 0;
            GSC_RESTART = flags.Get(GSound.GSC_RESTART) != 0;
            GSC_STOP = flags.Get(GSound.GSC_STOP) != 0;

        }
    }
    public GSound()
    {
        //FlagsShowerHolder = new GameObject("Flag shower ");
        //FlagsShower = FlagsShowerHolder.AddComponent<ShowFlags>();
        //next = null;
        gsparams.volume = 0;
        gsparams.frequency = 1;
    }

    public void Init(I3DSoundEventController ct, bool loop, bool rndp, SManager sm, GSoundData[] sd, int def_play)
    {

        flags = new Flags();
        flags.Set(GSF_LOOPED, loop);
        flags.Set(GSF_RND_RATE, rndp);

        ctr = ct; ctr.AddRef(); mngr = sm;

        def_play |= loop ? DSBPLAY_LOOPING : 0;


        for (int i = 0; i < 3; ++i)
        {
            sample[i] = new Sample3D();
            sample[i].InitData(sd[i], def_play);
        }

#if _DEBUG
            mCurMode = -1;
#endif
    }
    void Destroy()
    {
        Debug.LogFormat("CMD: Release GSound  \"{0}\"", mngr.GetGSName(this));
# if _DEBUG
        slog.Message("CMD: Release GSound  \"%s\"", mngr->GetGSName(this));
#endif
        Debug.Log(string.Format("CMD: Release GSound  \"{0}\" {1}", mngr.GetGSName(this), flags));
        if (flags.Get(GSF_INDLL) != 0)
            mngr.RemoveManaged(this);

        for (int i = 0; i < 3; ++i)
        {

# if _DEBUG
            if (!mbSampleInit[i] && sample[i].data)
                slog->Message("Sample %s not used", mngr->GetWaveCode(sample[i].data));
#endif

            sample[i].Done();
        }

        ctr.Release();
        mngr.gs_counter--;
    }


    // api to SMan class update methods

    /// <summary>
    /// sample can be played
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    public bool IsSample(int mode)
    {
        return sample[mode].data != null;
    }

    bool LoadSample2(int n)
    {
        mbSampleInit[n] = true;
        Asserts.Assert(sample[n].data != null);

        if (sample[n].InitBuffer(mngr.sfx_db, 0, 0, Looped() != 0))
            return true;
        else
            sample[n].InitData(null, 0);
        return false;
    }

    bool LoadSample(int n)
    {
        return mbSampleInit[n] ? true : LoadSample2(n);
    }

    public int Looped() { return flags.Get(GSF_LOOPED); }
    public int PlayingInDirectSound(int n) { return 1; }

    string[] IntExt = { "INT", "EXT", "HUL" };
    string GetGSModeName(int mode)
    {
        return (mode >= 0 && mode <= 2) ? IntExt[mode] : "ERR";
    }

    // Direct Sound commands
    public bool Play()
    {
        int mode = GetSoundMode();
        //Debug.LogFormat("Playing sound {0} m:{1}\n", mngr.GetGSName(this), GetGSModeName(mode));
        if (LoadSample(mode))
        {
            HRESULT hr;

            if (Looped() == 0)
            {
                hr = sample[mode].SetFrequencyMod(flags.Get(GSF_RND_RATE) != 0 ? mngr.Random() : 0);
                hr = sample[mode].SetCurrentPosition(0);
            }

            Set3DParams(mode, true, D3DEMULATION.DS3D_IMMEDIATE);


            if (SUCCEEDED(hr = sample[mode].Play()))
                return true;

            //slog->Message("ERROR: FAILED to PlayFirst: %s\n", DSErr(hr));
        }
        else
        {
            Debug.Log("Error load sample");
            //slog.Message("Error load sample");
        }
        return false;
    }
    void Stop(int n)
    {
        UnityEngine.Debug.LogFormat("Stopping sound {0} mode {1} flags {2}", mngr.GetGSName(this), n,flags);
        //Asserts.Assert(mCurMode == n);
        sample[n].Stop();
# if _DEBUG
        mCurMode = -1;
#endif
    }
    public void StopLcl()
    {
        Stop(GetSoundMode());
    }

    // setup
    void Set2DParams(int n)
    {
        UpdateData upd = gsparams;
        if (ctr.Update(ref upd))
        {
            if (upd.frequency != gsparams.frequency)
            {
                gsparams.frequency = upd.frequency * .25f + gsparams.frequency * .75f;
                //params.frequency=upd.frequency;

                Asserts.Assert(gsparams.frequency >= 0 && gsparams.frequency <= 1);

                gsparams.frequency = Mathf.Clamp(gsparams.frequency, 0, 1);

                HRESULT hr = sample[n].SetFrequencyMod(gsparams.frequency);
                Asserts.Assert(SUCCEEDED(hr));
            }

            // validate params ...
            if (upd.volume > 0)
            {
                // slog->Message("  ??? Lim Excided: vol = %d", upd.volume);
                upd.volume = 0;
            }

            if (upd.volume != gsparams.volume)
            {
                HRESULT hr = sample[n].SetVolumeMod((gsparams.volume = upd.volume) + mngr.volume);
                Asserts.Assert(SUCCEEDED(hr));
            }
        }
    }
    void Set3DParams(int n, bool GameUpdate, DWORD Flag)
    {
        //Asserts.Assert(mCurMode == n);
        //Asserts.Assert(flags.Get(GSF_MODEL_OK));

        sample[n].SetPosition(mPosition, Flag);
        sample[n].SetVelocity(mVelocity, Flag);


        if (GameUpdate)
            Set2DParams(n);

        //SetController();
    } // DS3D_DEFERRED or DS3D_IMMEDIATE

    //private bool UpdateSoundEmitter()
    //{

    //    if (SoundEmitter == null || SoundSource == null) return false;
    //    //if (!IsPlaying()) return false;

    //    //Vector3 localPos = ctr.GetPosition() - Engine.EngineCamera.Org;
    //    //Vector3 localPos = Engine.ToCameraReference(mPosition);
    //    Vector3 localPos = mPosition;
    //    float distance = localPos.magnitude;
    //    if (Camera.main.farClipPlane < distance) return false;

    //    SoundEmitter.transform.position = Engine.ToCameraReference(ctr.GetPosition()); 
    //    return true;
    //}


    // validate modeling vars (mPos, mVel, ... )
    public bool Set3D(int n)
    {
        Sample3D smpl = sample[n];
        Vector3 pos = ctr.GetPosition();
        float d2 = (mngr.camera.Org - pos).sqrMagnitude;
        //if (d2 > Mathf.Pow(smpl.data.max_d, 2))
        //    return false;

        float _int = smpl.Int(playtime);
        k = smpl.IntMod(d2) * _int;
        ampl = smpl.Ampl(Mathf.Pow(d2, 2)) * _int;

        // В оригинальном коде координаты устанавливаются относительно звуковой камеры
        // В Unity проще ставить источник звука "как есть" относительно главной камеры (к которой привязан listener) или по координатам звуковой камеры
        //mPosition = mngr.camera.ExpressPoint(pos);
        //mVelocity = mngr.camera.ExpressVector(ctr.GetVelocity() - mngr.speed);

        mPosition = pos - mngr.camera.Org;
        mVelocity = ctr.GetVelocity() - mngr.speed;

        //# ifdef _DEBUG
        //        slog->Append(" ampl = %f", ampl);
        //        Assert(ampl < 20);
        //#endif //_DEBUG
        //FlagsShowerHolder.transform.position = Engine.ToCameraReference(pos);
        return true;
    }
    public void ResetPlaytime() { playtime = 0; }

    // update returning false if sound was stopped

    public bool UpdatePlaying(float scale, bool GameUpdate)
    {
        //FlagsShower.UpdateFlags(flags);

        int n = GetSoundMode();
        Asserts.Assert(GSF_INDLL != 0);

# if _DEBUG
        slog.Message("GS::UpdatePlaying(%d)(%s)", n, mngr->GetGSName(this));
#endif //_DEBUG

        if (flags.Get(GSC_STOP) != 0)
        {
# if _DEBUG
            slog.Append(" stopped");
#endif //_DEBUG
            Stop(n);
            return false;
        }

        if (flags.Get(GSC_RESTART) != 0)
        {
            //pragma message "Restart sound phase No 1"
            Stop(n);
            ResetPlaytime();
            return Set3D(n);
        }
        else
        {
            playtime += scale;
            bool DistOk = Set3D(n);
            bool PlayOk = DistOk && (Looped() != 0 || PlayingInDirectSound(n) != 0);


            if (PlayOk)
            {
                Set3DParams(n, GameUpdate, DS3D_DEFERRED);
                // slog->Append(" - ok");
            }
            else
            {
                Stop(n);

                // slog->Append(" STOP: Set3D(n)=%d", DistOk);
                // if (DistOk)
                //   slog->Append(", PlayingInDirectSound(n)=%d",  PlayOk);

            }
            return PlayOk;
        }
    }


    public int GetSoundMode()
    {
        return mngr.GetSoundMode((int)ctr.GetID());
    }
    float GetK() { return k; }


    public Flags flags;
    public I3DSoundEventController ctr;
    SManager mngr;

    bool[] mbSampleInit = new bool[3];
    public Sample3D[] sample = new Sample3D[3];     // samples

    // if (Set3D returns ture, this parameters are updated)
    /// <summary>
    /// Положение источника звука. В оригинальном коде - _относительно звуковой камеры_
    /// </summary>
    Vector3 mPosition;
    Vector3 mVelocity;
    float k, ampl;


    UpdateData gsparams;
    float playtime;
#if _DEBUG
    int mCurMode;
#endif

    //TODO Корректно реализовать проигрывание звуков
    // api
    public virtual void Start()
    {
        if (flags.Get(GSF_INDLL) == 0)
        {
            //next = mngr.queue; mngr.queue = this;
            mngr.queue.Add(this);
            flags.Set(GSF_INDLL, true);
            flags.Set(GSC_STOP, false);
        }
        else
        {
            if (Looped() == 0)
            {
                flags.Set(GSC_RESTART, true);
            }
        }
        //Debug.Log("Sound set to start for " + mngr.GetGSName(this) + " " + flags);
    }


    public virtual void Stop()
    {
        flags.Set(GSC_STOP, true);
        //Debug.Log("Sound set to stop for " + mngr.GetGSName(this) );
    }
    public virtual bool IsPlaying() { return flags.Get(GSC_STOP) == 0 && flags.Get(GSF_INDLL) != 0; }

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
        Stop();
        Destroy();

        return 0;
    }

    public void UpdateController(Vector3 inWorld)
    {
        ctr.SetPosition(inWorld);
    }
}

public struct UpdateData
{
    public int volume;      // -10000...0  (-600 - half volume)
    public float frequency;   // [0..1] 
};
public class Sample3D
{

    public const float log10 = 2.30258509299404568401799145468436f;
    const float sc_oo2000 = log10 / 2000f;
    const float sc_2000 = 2000f / log10;
    const float silence = -10000;
    const float ampl_sl = 0.00001f; // Db2Ampl(silence);

    private float Db2Ampl(float db) { return Mathf.Exp(db * sc_oo2000); }
    private float Ampl2Db(float am) { return sc_2000 * Mathf.Log(am); }
    private int ValidVolume(int vol) { return vol < -10000 ? -10000 : vol; }
    public const int DS3D_DEFERRED = 0;
    public const int DS3D_IMMEDIATE = 1;
    public Storm2Audiclip h;
    //public StormSound3Dparams l;
    //IDirectSound3DBuffer* l;

    float oo_life_time;        // 1/sec
    float frequency;           // sample rate Hz

    float a1, a2;

    DWORD ds_status;           // GetStatus information
    int def_play_flag;       // default play flag

    public GSoundData data;

    public Sample3D()
    {
        h = new Storm2Audiclip(null);
        //h = null;
        //l = null;
        //l = new StormSound3Dparams();
        data = null;
    }

    public void InitData(GSoundData gsd, int def_play)
    {
        data = gsd;
        def_play_flag = def_play;

        if (data != null)
        {
            float mind = data.min_d;
            float maxd = data.max_d;

            if ((maxd - mind) < 1)
            {
                //slog.Message("invalid data");
                maxd = mind + 1;
            }

            double ampl_min = Db2Ampl(data.mvolume);
            double ampl_max = ampl_sl; // Db2Ampl(silence);
            double _ = Db2Ampl(silence);
            double temp = (ampl_min - ampl_max) * mind / (maxd - mind);
            a1 = (float)(ampl_max - temp);
            a2 = (float)(temp * maxd);
        }
        else
            a1 = a2 = 0;


        // fake init length = 1c
        oo_life_time = 1;
    }
    public bool InitBuffer(VoicesDB db, int VolumeMod, float dFreq, bool rand)
    {
        AudioClip clip = db.LoadVoice(data.wave_code, true);
        float mind = data.min_d;
        float maxd = data.max_d;
        if ((maxd - mind) < 1) // just test
            maxd = mind + 1;

        dFreq = Mathf.Clamp(dFreq, 0, 1);

        int nErrors =
              (FAILED(SetMinDist(mind)) ? 1 : 0) +
              (FAILED(SetMaxDist(maxd)) ? 1 : 0) +
              (FAILED(SetMode(D3DEMULATION.DS3DMODE_HEADRELATIVE)) ? 1 : 0) +
              (FAILED(SetVolumeMod(VolumeMod)) ? 1 : 0) +
              (FAILED(SetFrequencyMod(dFreq)) ? 1 : 0);


        if (nErrors != 0) { throw new Exception("Failed to set 3d settings"); }

        h.clip = clip;
        h.init(rand);
        return h != null;
    }

    public void Done()
    {
        //
        //SafeRelease(l); 
        SafeRelease(h);
    }

    private void SafeRelease(Storm2Audiclip i)
    {
        //TODO Имхо звук должен освобождаться не так.
        if (i != null) { i.Stop(); i.Dispose(); }
        
        return;
    }

    // modeling ...

    public float Int(float t) { return 1 - t * (1 - data.g_coeff1) * oo_life_time; }
    public float IntMod(float d2) { return data.min_d / (d2 + 1); }
    public float Ampl(float dist) { return a1 + a2 / Mathf.Clamp(dist, data.min_d, data.max_d); }

    public HRESULT Play(DWORD Flag = 0) { return h.Play(0, 0, Flag | (uint)def_play_flag); }
    public HRESULT Stop()
    {
        if (h == null) return HRESULT.E_INVALIDARG;
        return h.Stop();
    }

    // modifiers
    public HRESULT SetVolumeMod(long vol_mod)
    {
        int vol = ValidVolume((int)(vol_mod + data.mvolume));
        //Assert(vol >= -10000 && vol <= 0);
        return h.SetVolume(vol);
    }

    // status
    HRESULT GetStatus() { return h.GetStatus(ds_status); }
    DWORD DSStatus() { return ds_status; }

    // smaple current position
    public HRESULT SetCurrentPosition(int pos)
    {
        return h.SetCurrentPosition(pos);
    }

    // 3d
    public HRESULT SetPosition(Vector3 v, DWORD Flag = DS3D_DEFERRED)
    {

        return h.SetPosition(v.x, v.y, v.z, Flag);
    }
    public HRESULT SetVelocity(Vector3 v, DWORD Flag = DS3D_DEFERRED) { return h.SetVelocity(v.x, v.y, v.z, Flag); }

    HRESULT SetMaxDist(float d) { return h.SetMaxDistance(d, DS3D_IMMEDIATE); }
    HRESULT SetMinDist(float d) { return h.SetMinDistance(d, DS3D_IMMEDIATE); }
    HRESULT SetMode(DWORD Mode) { return h.SetMode(Mode, DS3D_IMMEDIATE); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="t">// t in [0..1]</param>
    /// <returns></returns>
    public HRESULT SetFrequencyMod(float t)
    {
        //TODO Вернуть (при необходимости) изменение частоты звука.
        return HRESULT.S_OK;
        int frq = (int)((data.rate_mod1 * (1 - t) + data.rate_mod2 * t) * frequency);
        //Частота - в Герцах
        Debug.Log("Freq set: " + frq + " out of "+ t);
        return h.SetFrequency(frq);
    }
}

/// <summary>
/// эмуляция IDirectSound3DBuffer
/// </summary>
public struct StormSound3Dparams
{
    public Vector3 position;
    public Vector3 velocity;

    public float MaxDistance, MinDistance;

    internal HRESULT SetPosition(float x, float y, float z, uint flag)
    {
        //SoundSource.transform.position = Engine.UnityCamera.transform.position + new Vector3(x, y, z);
        //Debug.DrawRay(Engine.UnityCamera.transform.position, Engine.UnityCamera.transform.rotation * new Vector3(x, y, z) ,Color.yellow);
        //Debug.LogFormat("DEBUG! Setting position for {0} to {1}", clip.name, new Vector3(x,y,z));
        position = new Vector3(x, y, z);
        //SoundSource.transform.position = Engine.UnityCamera.transform.position + (new Vector3(x, y, z));
        return HRESULT.S_OK;
    }

    internal HRESULT SetVelocity(float x, float y, float z, uint flag)
    {
        //Debug.LogFormat("DEBUG! Setting velocity for {0} to {1}", clip.name, new Vector3(x, y, z));
        velocity = new Vector3(x, y, z);
        return HRESULT.S_OK;
    }

    internal HRESULT SetMaxDistance(float d, int dS3D_IMMEDIATE)
    {
        MaxDistance = d;
        return HRESULT.S_OK;
    }

    internal HRESULT SetMinDistance(float d, int dS3D_IMMEDIATE)
    {
        MinDistance = d;
        return HRESULT.S_OK;
    }

    internal HRESULT SetMode(uint mode, int dS3D_IMMEDIATE)
    {
        throw new System.NotImplementedException();
    }

}
public class Storm2Audiclip
{
    private static GameObject SoundHolder = null;
    public AudioClip clip;
    AudioSource SoundSource;
    GameObject SoundEmitter;

    public Storm2Audiclip(AudioClip clip)
    {
        this.clip = clip;
        //init();
    }

    public void init(bool looped=false)
    {
        if (SoundHolder == null) SoundHolder = new GameObject("Sound holder");
        SoundEmitter = new GameObject("Sound Emitter " + (clip != null ? clip.name : "NO SOUND"));
        SoundEmitter.transform.parent = SoundHolder.transform;
        SoundEmitter.transform.position = position;

        SoundSource = SoundEmitter.AddComponent<AudioSource>();
        SoundSource.clip = clip;
        SoundSource.loop = looped;
        SoundSource.spatialBlend = 1;
        SoundSource.rolloffMode = AudioRolloffMode.Logarithmic;
        SoundSource.playOnAwake = false;
        SoundSource.maxDistance = MaxDistance;
        SoundSource.minDistance = MinDistance;
        //SoundEmitter.SetActive(false);
    }
    public HRESULT SetFrequency(int frq)
    {
        if (SoundSource == null) return HRESULT.E_ABORT;
        SoundSource.pitch = frq;
        
        return HRESULT.S_OK;
    }

    public HRESULT Start()
    {
        Debug.LogFormat("Starting SoundSource {0} SoundEmitter {1}", SoundSource, SoundEmitter);
        if (SoundEmitter != null) SoundEmitter.SetActive(true);
        if (SoundSource != null) SoundSource.Play();
        return HRESULT.S_OK;
    }

    public HRESULT Stop()
    {
        Debug.LogFormat("Stopping SoundSource {0} SoundEmitter {1}", SoundSource, SoundEmitter);
        if (SoundSource != null) SoundSource.Stop();
        if (SoundEmitter != null) SoundEmitter.SetActive(false);
        return HRESULT.S_OK;
    }

    internal HRESULT GetStatus(uint ds_status)
    {
        throw new System.NotImplementedException();
    }


    const uint DSBPLAY_LOOPING = 0x00000001;
    /// <summary>
    /// Эмуляция IDirectSoundBuffer::Play
    /// </summary>
    /// <param name="dwReserved1">This parameter is reserved. Its value must be 0.</param>
    /// <param name="dwReserved2">This parameter is reserved. Its value must be 0</param>
    /// <param name="dwFlags">Flags specifying how to play the buffer. The following flag is defined:
    /// DSBPLAY_LOOPING 	Once the end of the audio buffer is reached, play restarts at the beginning of the buffer. Play continues until explicitly stopped. This flag must be set when playing primary sound buffers.
    /// </param>
    /// <returns></returns>
    internal HRESULT Play(int dwReserved1, int dwReserved2, uint dwFlags)
    {
        
        //if (!SoundEmitter.activeSelf) SoundEmitter.SetActive(true);
        SoundEmitter.transform.position = position;
        SoundSource.loop = (dwFlags & DSBPLAY_LOOPING) != 0;
        //Debug.Log(SoundEmitter.name + " Flags: " + dwFlags.ToString("X8"));
        //SoundSource.Play();
        Start();
        return HRESULT.S_OK;
    }

    internal HRESULT SetCurrentPosition(int pos)
    {
        if (SoundEmitter == null) return HRESULT.E_FAIL;
        SoundSource.time = pos; //TODO неправильно. pos это оффсет в _байтах_
        return HRESULT.S_OK;
    }

    internal HRESULT SetVolume(int vol)
    {
       return HRESULT.S_OK;
    }

    Vector3 position = Engine.FarFarAway;
    internal HRESULT SetPosition(float x, float y, float z, uint flag)
    {
        //SoundSource.transform.position = Engine.UnityCamera.transform.position + new Vector3(x, y, z);
        //Debug.DrawRay(Engine.UnityCamera.transform.position, Engine.UnityCamera.transform.rotation * new Vector3(x, y, z) ,Color.yellow);
        //Debug.LogFormat("DEBUG! Setting position for {0} to {1}", clip.name, new Vector3(x,y,z));
        //position = new Vector3(x, y, z);
        position = Engine.UnityCamera.transform.position + (new Vector3(x, y, z));
        if (SoundSource != null) SoundSource.transform.position = position;
        return HRESULT.S_OK;
    }

    internal HRESULT SetVelocity(float x, float y, float z, uint flag)
    {
        //Debug.LogFormat("DEBUG! Setting velocity for {0} to {1}", clip.name, new Vector3(x, y, z));
        //velocity = new Vector3(x, y, z);
        return HRESULT.S_OK;
    }

    float MaxDistance, MinDistance;
    internal HRESULT SetMaxDistance(float d, int dS3D_IMMEDIATE)
    {
        MaxDistance = d;
        if (SoundSource!=null)  SoundSource.maxDistance = d;
        return HRESULT.S_OK;
    }

    internal HRESULT SetMinDistance(float d, int dS3D_IMMEDIATE)
    {
        MinDistance = d;
        if (SoundSource != null) SoundSource.minDistance = d;
        return HRESULT.S_OK;
    }

    internal HRESULT SetMode(uint mode, int dS3D_IMMEDIATE)
    {
        return HRESULT.S_OK;
    }

    internal void Dispose()
    {
        GameObject.Destroy(SoundEmitter); SoundEmitter = null;
    }
}
