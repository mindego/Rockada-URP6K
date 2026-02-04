using crc32 = System.UInt32;
using PCWaveData = WaveData;
/*===========================================================================*\
|  VoicesDB implementation                                                    |
\*===========================================================================*/

using UnityEngine;

public class _VoicesDB : VoicesDB
{

    IMappedDb db;
    //IDirectSoundBuffer** cache;
    AudioClip[] cache;
    Sound mpSound;
    bool is_3d;

    public _VoicesDB()
    {
        db = null;
        //cache = null;
        mpSound = null;
        is_3d = false;
    }
    public void Initialize(IMappedDb _db, Sound s, bool _is_3d)
    {
        db = _db; mpSound = s; is_3d = _is_3d;
        int size = db.GetNumRecords();
        if (size != 0)
        {
            Debug.Log("Voice cache size: " + size);
            cache = new AudioClip[size];
            for (int i = 0; i < size; ++i) cache[i] = null;
        }
    }
    void Destroy()
    {
        FreeCache();
        if (cache != null) cache = null; ;
        db.Release();
    }

    AudioClip DuplicateCachedVoice(AudioClip buf)
    {
        //TODO Реализовать копирования звука
        //if (false && ::RefCount(buf) == 1)
        //{
        //    // slog->Message( "addref to mpSound %s", db->CompleteObjId(code).name);
        //    buf->AddRef();
        //}
        //else
        //{
        //    // slog->Message( "duplicate mpSound %s", db->CompleteObjId(code).name);
        //    buf = mpSound->Duplicate3dSound(buf);
        //}
        return buf;
    }

    // API {
    public virtual object Query(uint cls_id)
    {
        switch (cls_id)
        {
            case IObject.ID:
            case VoicesDB.ID: return this;
            default: return null;
        }
    }

    public virtual bool TestWave(crc32 cCode, bool Load)
    {
        WaveData wd = db.GetBlock(cCode).Convert<WaveData>();
        if (wd != null && Load) wd.Load();

        return wd !=null;
    }
    public virtual AudioClip CreatePhrace(crc32[] pcCode, int Count)
    {
        WaveData[] wd_list = new WaveData[Count];

        for (int i = 0; i != Count; ++i)
        {
            wd_list[i] = db.GetBlock(pcCode[i]).Convert<WaveData>();
            Debug.Log(wd_list[i]);
            if (wd_list[i] == null)
            {
                //slog.Message("_VoicesDB::CreatePhrace ERROR: Invalid Code list (wave does not exists)");
                Debug.Log("_VoicesDB::CreatePhrace ERROR: Invalid Code list (wave does not exists)");
            }
        }

        return mpSound.CreateAudioClip(wd_list, Count, is_3d);
    }

    public virtual AudioClip LoadVoice(crc32 code, bool bCache)
    {
        int num = db.GetRecordNumber(code);
        ObjId obj_id = db.CompleteObjId(code);
        if (num >= 0)
        {
            AudioClip buf = cache[num];

            if (buf!=null)
            { // do duplicate from cache
                buf = DuplicateCachedVoice(buf);
            }
            else
            {  // not in cache
                //PCWaveData wData[1] = { db->GetBlock(code).Convert<WaveData>() };
                WaveData[] wData = new WaveData[1];
                wData[0] = db.GetBlock(code).Convert<WaveData>();
                //buf = mpSound->CreateBuffer(wData, 1, is_3d);

                buf = mpSound.CreateAudioClip(wData, 1, is_3d);
                buf.name = obj_id.name;
                if (buf && bCache)
                {
                    cache[num] = buf;
                    buf = DuplicateCachedVoice(buf);
                }
            }

            //if (buf!=null) // again
                //buf.SetCurrentPosition(0);
                

            return buf;
        }
        return null;
    }
    public virtual void FreeCache()
    {
        int size = db.GetNumRecords();
        for (int i = 0; i < size; ++i) if (cache[i] != null)
            {
                cache[i] = null;
            }
    }
    public virtual IMappedDb GetDB()
    {
        return db;
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


    // }


}