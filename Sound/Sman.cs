using System.Collections.Generic;
using UnityEngine;
using crc32 = System.UInt32;
using DWORD = System.UInt32;
using static GSound;
/*===========================================================================*\
|   class  SManager - main 3d sound manager                                   |
\*===========================================================================*/


//typedef Pool<GSound>::pelem PelemGS;

public class SManager : I3DSound
{
    private const string sounds_db = "sounds.dat";
    private const string gsfx_db = "game.sfx";
    public static string[] IntExt = { "INT", "EXT", "HUL" };
    private const string FailedData = "  ERROR: couldn`t find sound data : {0}";
    private const string FailedLoad = "  ERROR: faled load sound sample specified in event {0}";
    public static string GetGSModeName(int mode)
    {
        return (mode >= 0 && mode <= 2) ? IntExt[mode] : "ERR";
    }
    Sound sound; // driver

    // datas
    public VoicesDB sfx_db;
    IMappedDb game_sounds;
    int mSceneDetail;

    //  GUI edit
    bool can_edit;
    //IToolWindow sound_editor;

    // debug
    public int gs_counter;


    // queues
    Queue<GSound> pool;// n - sounds (attribute GSF_PLAYING must be set)
    List<GSound> playing;
    List<GSound> toplay;

    //GSound muted;    // looped sounds, not playing now
    List<GSound> muted;    // looped sounds, not playing now
    //GSound dropped;  // temp recycle for muted
    List<GSound> dropped;  // temp recycle for muted
    //public GSound queue;    // game commands
    public List<GSound> queue;

    // temp variables (search hints)
    GSound[] min_in_new;
    GSound[] min_playing;   // temp variable


    float update_time;
    float update_min;  // real game sound update delay in sec
    bool update_flag; // we are need to call supdate callback

    int mode;        // 0 - cockpit, 1 - external, 2 - hull
    int mode_change; // cocpit mode change

    DWORD player;
    DWORD player_change;

    public int volume;
    int volume_ue;
    public MATRIX camera;
    public Vector3 speed;

    // random table
    float[] rand_tbl = new float[64];
    int rand_ptr;
    void InitRandomTable()
    {
        for (int i = 0; i < rand_tbl.Length; ++i)
            rand_tbl[i] = RandomGenerator.Rand01();
        rand_ptr = 0;
    }
    public float Random() { return rand_tbl[++rand_ptr & 63]; }

    // update stuff
    void Reject(GSound s)
    {
        ///ker->Log->Message("Dropped sound %p (%s) ", s, s->Looped()?"looped":"");

        if (s.Looped() != 0 && s.flags.Get(GSC_STOP) == 0)
        {
            dropped.Add(s);
            //s.next = dropped; dropped = s;
            //Debug.LogFormat("Moved {0} to looped list", s);
            ///ker->Log->Message(" - to  looped list\n");
        }
        else
        {
            s.flags.Set(GSF_ALL, false);
            //s.next = null;
            ///ker->Log->Message(" - forever\n");
        }
    }

    void UpdatePlaying(float scale)
    {
        List<GSound> list1 = playing;
        List<GSound> list2 = new List<GSound>();
        List<GSound> list3 = new List<GSound>();

        foreach (GSound snd in list1)
        {
            if (snd.UpdatePlaying(scale, update_flag))
            {
                list2.Add(snd);
            }
            else
            {
                list3.Add(snd);
                Reject(snd);
            }
        }
        playing = list2;

        //TODO восстановить работу обновления играемого сэмпла
        //# if _DEBUG
        //        int n = 0;
        //#endif //_DEBUG

        //        GSound list1 = playing;
        //        GSound list2 = null;

        //        for (; list1 != null;)
        //        {
        //            GSound snd = list1.data;
        //            GSound nx = list1.next;

        //            if (snd.UpdatePlaying(scale, update_flag))
        //            {

        //                list1.next = list2;
        //                list2 = list1;
        //# if _DEBUG
        //                n++;
        //#endif //_DEBUG

        //            }
        //            else
        //            {
        //                //pool.Delete(list1);
        //                pool.
        //                Reject(snd);
        //            }
        //            list1 = nx;
        //        }

        //        playing = list2;


        //# if _DEBUG
        //        slog->Message("number of playing sounds = %d\n", n);
        //#endif //_DEBUG
    }
    //void RemoveAllPlaying()
    //{ // if modechange
    //  //# if _DEBUG
    //  //        slog->Message("reset all playing ... ");
    //  //#endif //_DEBUG
    //  //        PelemGS* list = playing;

    //    //        for (; list;)
    //    //        {
    //    //            GSound q = list->data;
    //    //            PelemGS* nx = list->next;

    //    //            q.StopLcl();

    //    //# ifdef _DEBUG
    //    //            slog->Message("\"%s\" stopped", GetGSName(q));
    //    //#endif //_DEBUG

    //    //            pool.Delete(list);
    //    //            Reject(q);

    //    //            list = nx;
    //    //        }

    //    //        playing = 0;
    //    while (pool.Count > 0)
    //    {
    //        GSound q = pool.Dequeue();
    //        q.StopLcl();
    //    }
    //    pool.Clear();
    //}
    void RemoveAllPlaying()
    {
        List<GSound> list = playing;

        foreach (GSound q in list)
        {
            q.StopLcl();
            Reject(q);
        }

        //playing = null;
        playing.Clear();
    }


    void ProcessQueue(List<GSound> queue)
    {
        foreach (GSound snd in queue)
        {
            int cmode = snd.GetSoundMode();
            snd.ResetPlaytime();
            if (snd.flags.Get(GSC_STOP) == 0 && snd.IsSample(cmode) && snd.Set3D(cmode))
            {
                toplay.Add(snd);
            }
        }
        //GSound next;
        //do
        //{
        //    next = queue.next; queue.next = null;
        //    int cmode = queue.GetSoundMode();

        //    queue.ResetPlaytime();
        //    if (queue.flags.Get(GSC_STOP) == 0 && queue.IsSample(cmode) && queue.Set3D(cmode))
        //    {
        //        //queue.Play();
        //        toplay.Add(queue);
        //        continue;
        //    }
        //    else
        //    {

        //    }
        //    //if (next!=null) next.Play();

        //    //if (next != null) Debug.Log("PLaying " + next + " Flags " + next.flags);
        //} while ((queue = next) != null);


    }
    void AjustVolume()
    {
        //TODO Реализовать управление громкостью
    }

    void ProcessPlayQueue()
    {
        int nplaying = 0;

        // restart sounds have to restart
        foreach (GSound s in playing)
        {
            if (s.flags.Get(GSC_RESTART) != 0)
            {
//#warning "Restart sound phase No 2"
                s.Play();
                s.flags.Set(GSC_RESTART, false);
            }
        }

        // starting new sounds
        foreach (GSound s in toplay)
        {
            bool PlayOk = s.Play();

            if (!PlayOk)
            {
                // delete element
                Reject(s);
                //pool.Delete(s);
            }
            else
            {
                // add to playing
                playing.Add(s);
                ++nplaying;
            }
        }
        // clear new play list
        toplay.Clear();
    }

    float MinKInNew()
    {
        //TOD Реализовать получение минимального K в новом
        float min_k = 100000f;

        return min_k;
    }
    float MinKInPlaying()
    {
        //TOD Реализовать получение минимального K в проигрываемом
        float min_k = 100000f;

        return min_k;
    }

    public int GetSoundMode(int id) { return mode == 1 ? 1 : id == player ? 2 : 0; }

    GSoundData GetData(uint code) { return game_sounds.GetBlock(code).Convert<GSoundData>(); }

    private int nsounds; //TODO - возможно стоит хранить число 
    // constructor / destructor
    public SManager(Sound s, int nsounds, int start_vol = 9, float upd_min = 0.03f)
    {
        this.nsounds = nsounds;
        // DirectSound API
        sound = s;

        // debug and testing
        gs_counter = (0);

        // data source
        sfx_db = null;
        game_sounds = null;
        //sound_editor = null;
        can_edit = false;

        // current settings
        mSceneDetail = 0;
        volume_ue = start_vol;
        mode = (int)SoundApiDefines.SM_EXTERN;     //  mode_change - uninitialized
        player = 0xFFFFFFFF;  //  player_change - uninitialized

        // active soounds
        pool = new Queue<GSound>(nsounds);
        playing = new List<GSound>();
        toplay = new List<GSound>();
        muted = new List<GSound>();
        queue = new List<GSound>();
        dropped = new List<GSound>();

        // update parameters
        update_time = 0;
        update_min = upd_min;

    }
    public bool Initialize(bool can_ed)
    {
        //game_sounds = CreateMappedDb(*(int*)"GSND");
        game_sounds = IMappedDb.CreateMappedDb(IMappedDb.DBFORMAT_NAKED);


        if ((sfx_db = VoicesDB.CreateVoicesDb(gsfx_db, sound, true)) != null)
        {
            can_edit = can_ed;
            int ret = game_sounds.Open(ProductDefs.GetPI().getHddFile(sounds_db), can_edit);

            if (ret != DBDef.DB_OK && can_edit)
            {
                can_edit = false;
                ret = game_sounds.Open(ProductDefs.GetPI().getHddFile(sounds_db));
            }

            if (ret == DBDef.DB_OK)
            {
                //pool.Initialize(); //TODO Нужно ли инициализировать очередь?
                SetVolume(volume_ue);
                InitRandomTable();
                return true;
            }
        }
        Debug.Log("3D Sound Engine error: no data");
        return false;
    }
    ~SManager()
    {
        sound.SetVolumeMod(0);
        Asserts.Assert(playing == null);
        //SafeRelease(sound_editor);
        //SafeRelease(sfx_db);
        //SafeRelease(game_sounds);
        sfx_db.Release();
        game_sounds.Release();
    }

    // name resolving
    string GetWaveCode(GSoundData gs_data)
    {
        if (gs_data == null)
            return ">> No Data <<";
        string nm = sfx_db.GetDB().CompleteObjId(new ObjId((uint)gs_data.wave_code)).name;

        return nm != null ? nm : ">> No Wave <<";
    }
    public string GetGSName(GSound s)
    {
        return GetWaveCode(s.sample[GetSoundMode((int)s.ctr.GetID())].data);
    }

    // info
    int NChannels() { return pool.Count; }
    int FreeChannels() { return nsounds - pool.Count; }
    int NPlaying()
    {
        //TODO реализовать подсчёт звуков в проигрывании
        return 0;
    }
    int NPlayCommand()
    {
        //TODO реализовать подсчёт звуков в проигрывании
        return 0;
    }

    // API

    // detail level does not changes during scene updates...
    public virtual void SetSceneDetail(int Detail)
    {
        mSceneDetail = Detail;
    }
    public virtual int GetSceneDetail()
    {
        return mSceneDetail;
    }

    public virtual void Update(float scale)
    {
        update_time += scale;

        if (update_time > update_min)
        {
            update_time = 0;
            update_flag = true;
        }
        else
            update_flag = false;

        dropped = new List<GSound>();
        //toplay = null;
        toplay.Clear();

        if (mode_change != mode || player != player_change)
        {
            RemoveAllPlaying();

            if (dropped.Count != 0)
            {
                ConnectList(ref muted, dropped);
                dropped = new List<GSound>();
                mode = mode_change;
                player = player_change;
            }
        }
        else
        {
            //------------update currently playing: --------------
            UpdatePlaying(scale);
        }

        // ------------ process muted sounds: ------------------
        if (muted.Count != 0)
        {
            ProcessQueue(muted);
        }

        // ------------process new sounds:------------
        if (queue.Count != 0)
        {
            ProcessQueue(queue);
            queue.Clear();
        }

        ProcessPlayQueue();

        muted = dropped;
    }

    /// <summary>
    /// Добавляет к концу списка list содержимое списка queue
    /// </summary>
    /// <param name="list"></param>
    /// <param name="queue"></param>
    void ConnectList(ref List<GSound> list, List<GSound> queue)
    {
        //if (list==null)
        //{
        //    list = queue;
        //    return;
        //}
        //for (; list.next != null; list = list.next) ;
        //list.next = queue;
        list.AddRange(queue);
    }

    GameObject mySoundCamera;
    AudioListener myAudioListener;
    public virtual void SetCamera(MATRIX m, Vector3 v, int cmode, DWORD plr)
    {
        if (mySoundCamera == null)
        {
            mySoundCamera = new GameObject("Sound Camera");
            myAudioListener = Engine.UnityCamera.GetComponent<AudioListener>();
            myAudioListener.enabled = false;
            myAudioListener = mySoundCamera.AddComponent<AudioListener>();
        }

        camera = new MATRIX(m);
        speed = v;

        //Assert(cmode == SM_COCKPIT || cmode == SM_EXTERN || cmode == SM_NOCHAGE);

        if ((DWORD)cmode != SoundApiDefines.SM_NOCHAGE)
        {
            mode_change = cmode;
            player_change = plr;

            if (mode_change == SoundApiDefines.SM_COCKPIT)
                camera.Org += camera.Dir * 2;

        }

        mySoundCamera.transform.position = Engine.ToCameraReference(camera.Org);
    }
    public virtual void SetMode(int m, DWORD plr)
    {
        //Asserts.Assert(m == SM_COCKPIT || m == SM_EXTERN);
        mode_change = m;
    }
    public virtual void SetVolume(int vol)
    {
        volume_ue = System.Math.Clamp(vol, 0, 10);
        volume = VolumeMeter.Int2DB(volume_ue);
    }
    public virtual int GetVolume() { return volume_ue; }

    public virtual I3DSoundEvent LoadEvent(string cls, string obj, string evt, bool loop, bool rndp, I3DSoundEventController ctr)
    {
#if _DEBUG
        slog->Message("Loading gs \"%s_%s_%s\"", cls, obj, evt);
#endif //_DEBUG

        if (ctr == null)
        {
# if _DEBUG
            slog->Append(" no controller !");
#endif //_DEBUG
            return null;
        }

        EventName ev = new EventName(cls, obj, evt);

        GSoundData[] data = new GSoundData[3];

        for (int i = 0; i < 3; ++i)
        {
            Debug.Log("Loading sound " + ev.Name(i) + " " + ev.Code(i).ToString("X8"));
            data[i] = GetData(ev.Code(i));
            //Debug.Log((data[i], data[i].detail, mSceneDetail));

            //TODO! Восстановить удаление звука при отсутствии детальности сцены.
            //if (data[i] != null && data[i].detail < mSceneDetail)
            //    data[i] = null; 

            if (data[i] != null && !sfx_db.TestWave(data[i].wave_code, false))
            {
#if _DEBUG
                if (mSceneDetail == 0)
                    slog->Message(data[i] ? FailedLoad : FailedData, ev.Name(i));
#endif
                Debug.Log(string.Format(data[i] == null ? FailedLoad : FailedData, ev.Name(i) + " " + data[i].wave_code.ToString("X8")));
                data[i] = null;
            }

        }

# if _DEBUG
        slog->Message(" Load rezult: ( %s, %s, %s )",
          GetWaveCode(data[0]), GetWaveCode(data[1]), GetWaveCode(data[2]));
#endif

        if (data[0] != null || data[1] != null || data[2] != null)
        {

            GSound gs = new GSound();
            //Debug.Log((ctr, loop, rndp, this, data, sound));
            gs.Init(ctr, loop, rndp, this, data, sound.DefaultPlayFlag());
            gs_counter++;

            playing.Add(gs);

# if _DEBUG
            slog->Append(" this = %p", gs);
#endif
            return gs;

        }
        return null;
    }

    /*
    virtual GSound *Load(const char *cls, const char *obj, const char *evt, const GSParam &p);
    virtual void    Free(GSound *);
    virtual void    Start    ( GSound *s );
    virtual void    Stop     ( GSound *s );
    virtual bool    IsPlaying( GSound *s );
    */

    //TODO Реализовать удаление звука из списка играемого
    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    /// <returns>true if removed</returns>
    bool RemoveFromPlaying(GSound s)
    {
        List<GSound> ClearedList = new List<GSound>();
        bool removed = false;
        foreach (var z in playing)
        {
            if (z != s) {
                ClearedList.Add(z);
                continue;
            }
            removed = true;
            s.StopLcl();
        }
        if (removed) playing = ClearedList;
        return removed;
    }  // return true if removed
    public void RemoveManaged(GSound s)
    {
        Debug.Log("Removing sound " + s);
        bool managed = RemoveFromPlaying(s);

        if (!managed && s.Looped() != 0)
        {
            //Log->Message("manager can`t remove && looping\n");
            //RemoveFromList(muted, s);
        }

        //RemoveFromList(&queue, s);

        //Assert(managed || s->next == 0);
    }

    public void OpenEditor() { }

    public void AddRef()
    {
        //Doing nothing.
    }

    public int RefCount()
    {
        return 0;
    }

    public int Release()
    {
        return 0;
    }
}



public class EventName
{
    string name;
    crc32 code;

    /// <summary>
    /// Генерирует имя события с поддержкой получения CRC32/jamcrc для всех трёх видов звуков (Наружного, внутренного и корпуса?)
    /// </summary>
    /// <param name="cls">Класс собылия (Hangar, Weapon, Explosion)</param>
    /// <param name="obj">Тип объекта, вызвавшего событие (имя из описания gdata)</param>
    /// <param name="evt">Тип события (движение, вращение, создание и тп.)</param>
    public EventName(string cls, string obj, string evt)
    {
        name = string.Join("_", new string[] { cls, obj, evt });

    }
    internal uint Code(int i)
    {
        return Hasher.HshString(Name(i));
    }

    internal string Name(int i)
    {
        return name + SManager.IntExt[i];
    }
}