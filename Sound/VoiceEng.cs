using System.Collections.Generic;
using UnityEngine;

public class VoiceEng : IVoice
{
    SoundApi sound_eng;
    //WaveRoot waves;
    List<IUpdWave> waves = new List<IUpdWave>();


    public VoiceEng(SoundApi ps)
    {
        sound_eng = ps;
    }

    public virtual void AddRef()
    {
        sound_eng.AddRef();
    }
    public virtual int Release()
    {
        return sound_eng.Release();
    }

    public virtual IObject OpenVoiceDB(string name)
    {
        return VoicesDB.CreateVoicesDb(name, sound_eng.sound, false);
    }

    /// <summary>
    /// load from disk
    /// </summary>
    /// <param name="db"></param>
    /// <param name="nm"></param>
    /// <returns></returns>
    public virtual bool CachePhrase(IObject db, string nm)
    {
        VoicesDB vdb = VoicesDB.QueryVDB(db);
        return vdb != null ? TestAndLoadPhrase(vdb, nm, true) != 0 : false;
    }

    /// <summary>
    /// One buffer
    /// </summary>
    /// <param name="db"></param>
    /// <param name=""></param>
    /// <returns></returns>
    public virtual IWave CreateWave(IObject db, string nm)
    {
        VoicesDB vdb = VoicesDB.QueryVDB(db);

        AudioClip sb = vdb != null ? vdb.LoadVoice(Hasher.HshString(nm), true) : null;
        //return sb != null ? CreateWaveSB(sb) : 0;
        //Реализовать создание звукового файла, возможно изменив его тип на audioclip
        //return null;
        return sb != null ? CreateWaveSB(sb) : null;
    }

    private IWave CreateWaveSB(AudioClip sb)
    {
        return new StormWaveToAudioclipWrapper(sb);
    }
    public virtual IWave CreatePhrase(IObject db, string str)
    {
        //slog->Message("VoiceEng::CreatePhrase: %s", str);
        VoicesDB vdb = VoicesDB.QueryVDB(db);

        if (vdb != null && TestAndLoadPhrase(vdb, str + '\0', false) != 0) //'\0' т.к. исходный код "Шторма" ориентируется по \0 как концу строки
        {
            //LpList<IUpdWave>* ch = CreateVChannel(vdb, str);
            //waves.Insert(ch);
            //return ch;
            VChannel ch = VChannel.CreateVChannel(vdb, str);
            waves.Add(ch);
            return ch;
            //TODO Исправить и реализовать создание фразы радиосообщения
        }
        return null;
    }

    //TODO Реализовать обновления и отключение и удаление голоса
    public void Update(float scale) {
        foreach (var u in waves)
        {
            u.Update(scale);
        }
    }
    public void Destroy() { }

    public static bool TryLoadPhrase(VoicesDB vdb, string str, bool load)
    {
        return TestAndLoadPhrase(vdb, str, load) != 0;
    }
    public static int TestAndLoadPhrase(VoicesDB vdb, string str, bool load)
    {
        TokenParser mPars = new TokenParser(TokenParser.PhraseSpacial, str);
        int nSuccess = 0;

        while (true)
        {
            Token t = mPars.GetNextToken();
            if (t.IsEmpty()) break;
            if (t.GetChar(0) != '+' && !vdb.TestWave(t.GetCode(), load))
            {
                //char* name = ANewN(char, t.GetLength());
                //t.CopyTo(name);
                //slog->Message("VoiceEng::CreatePhrase failed load sound \"%s\", db=%p", name, vdb);
                Debug.Log(string.Format("VoiceEng::CreatePhrase failed load sound \"{0}\", db={1}", t, vdb));
                return 0;
            }
            ++nSuccess;
        }
        return nSuccess;
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }
}


public interface IUpdWave : IWave
{
    public void Update(float t);
};

