using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using crc32 = System.UInt32;

public class VChannel : List<IUpdWave>, IUpdWave
{
    TokenParser mPars;
    int mPlaying;
    int volume;
    VoicesDB db;
    StormWaveToAudioclipWrapper current;

    void Initialize(VoicesDB db, string str)
    {
        this.db = db;
        mPars.Init(str);
        current = null;
    }

    public VChannel()
    {
        mPars = new TokenParser(TokenParser.PhraseSpacial);
    }

    public static VChannel CreateVChannel(VoicesDB db, string str)
    {
        VChannel ch = new VChannel();
        ch.Initialize(db, str);
        return ch;
    }

    public void Play(bool looped, int vol)
    {
        //Debug.Log(string.Format("Playing {0} {1}  vol: {2} {3}", mPars, looped ? "Looped" : "Oneshot", vol, current != null ? current : "Empty"));
        if (vol != 0)
        {
            volume = VolumeMeter.Int2DB(vol);
            mPars.Reset(); mPlaying = 1;  // was setup
        }
        else
            Stop();
    }

    public void Stop()
    {
        mPars.Finish(); mPlaying = 0;
        Debug.Log(string.Format("Stopped {0}", mPars));
    }

    public void SetFrequecy(float frq)
    {
        throw new System.NotImplementedException();
    }

    public bool IsPlaying()
    {
        //Debug.Log(this + " is playing " + (current == null? "nothing":current));
        //if (current == null) return false;
        //Debug.Log(this + "is playing" + mPlaying + " " + current.name);
        return mPlaying != 0;
    }

    public void AddRef()
    {
        return;
    }

    public int RefCount()
    {
        return 1;
    }

    public int Release()
    {
        if (current != null) current.Release();
        return 0;
    }

    public void Update(float f)
    {
        if (current != null)
        {
            //  mPars.IsFull() => user call play so restart
            if (mPlaying == 0 || mPars.IsFull())
                ReleaseCurrent(mPlaying != 0);
            else
            {
                if (IsPlaying(current))
                    return;
                ReleaseCurrent();
            }
        }

        if (mPlaying != 0)
            if (LoadNext())
                PlayNext();
            else
                mPlaying = 0;
    }

    private void PlayNext()
    {
        ///hr = current->Play(0, 0, 0);
        //IDirectSoundBuffer::Play
        // HRESULT Play( DWORD dwReserved1, DWORD dwReserved2, DWORD dwFlags );
        current.Play(false, volume);
    }

    private bool LoadNext()
    {
        Assert.IsNull(current);

        do
        {
            Token tok = mPars.GetNextToken();

            if (tok.IsEmpty())
                return false; // stop channel

            TokList list = new TokList(tok.GetCode());
            TokList tail = list;
            int cnt = 1;

            Token pls = mPars.GetNextToken();

            while (pls.GetChar(0) == '+')
            {
                tok = mPars.GetNextToken();

                if (tok.IsEmpty())
                    return false;

                tail = tail.next = new TokList(tok.GetCode()); ++cnt;

                pls = mPars.GetNextToken();
            }

            mPars.SetPointer(pls);

            if (cnt == 1)
            {
                current = new StormWaveToAudioclipWrapper(db.LoadVoice(list.code, false));
            }
            else
            {
                crc32[] phr = new crc32[cnt];
                int index = 0;
                for (tail = list; tail != null; tail = tail.next) phr[index++] = tail.code;
                current = new StormWaveToAudioclipWrapper(db.CreatePhrace(phr, cnt));
            }

        } while (current == null);
        return true;
    }


    bool IsPlaying(StormWaveToAudioclipWrapper b)
    {
        //DWORD status; b->GetStatus(&status);
        //return status & DSBSTATUS_PLAYING;
        //TODO Реализовать работу со звуком
        return b.IsPlaying();
        //return true;
    }

    void ReleaseCurrent(bool stop = false)
    {
        // Debug.Log("ReleaseCurrent stop " + stop);
        Assert.IsNotNull(current);
        if (stop) current.Stop();
        current.Release();
        //current = 0;
        current = null;
    }
}

public class TokList
{
    public crc32 code;
    public TokList next;

    public TokList(crc32 _code)
    {
        code = _code;
        next = null;
    }
};