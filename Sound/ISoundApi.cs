using System;
using System.Collections.Generic;
using UnityEngine;
using DWORD = System.UInt32;
public class ISoundApi
{
    public const int SOUND_ENGINE_VERSION = 0x00000008;
    //private static string SOUND_TARGET =  "DirectSound";
    private const string SOUND_TARGET = "Unity sound";

    //private static string HelloWorld = "Sound engine: " SOUND_TARGET " " __TIME__ " " __DATE__;
    private const string HelloWorld = "Sound engine: " + SOUND_TARGET;//+ " " __TIME__ " " __DATE__;
    public static ISound CreateSInstance(object discard, ILog stdout, CommandsApi cmd, bool fake=false, int Version = SOUND_ENGINE_VERSION)
    {

        if (Version != SoundApi.SOUND_ENGINE_VERSION)
            return null;

        SoundConfig cfg = new SoundConfig(true);

        if (fake || cfg.no_sound == 1)
            return DummySound.CreateDummySound();

        //// initialize DirectSound
        Sound sound = new Sound();
        string err = sound.Initialize(discard, cfg);

        if (err != null)
        {
            stdout.Message("Sound initialization failed: %s", err);

            return DummySound.CreateDummySound();
        }

        stdout.Message(HelloWorld);

        // create and initialize general sound engine
        SoundApi isnd = new SoundApi();
        isnd.Initialize(sound, cfg, stdout, cmd);

        return isnd;
    }
}

public class Sound
{
    int def_play_flag;
    //Класс-заглушка для звука
    internal string Initialize(object discard, SoundConfig cfg)
    {
        def_play_flag = 0;
        return null;
    }

    internal AudioClip CreateAudioClip(WaveData[] wData, int v, bool is_3d)
    {
        AudioClip[] clips = new AudioClip[v];
        int total = 0;
        List<float> samples = new List<float>();
        int n_channels = 0;
        int sample_rate = 0;
        for (int i = 0; i < v; i++)
        {
            clips[i] = CreateAudioClip(wData[i], is_3d);
            
            float[] buffer = new float[clips[i].samples];
            clips[i].GetData(buffer, 0);
            total+=buffer.Length;
            samples.AddRange(buffer);
            n_channels = clips[i].channels;
            sample_rate = clips[i].frequency;

        }
        AudioClip res= AudioClip.Create("Phrase", total, n_channels, sample_rate, false);
        
        res.SetData(samples.ToArray(),0);
        return res;
    }
    internal AudioClip CreateAudioClip(WaveData wData, bool is_3d)
    {
        Sfx sfx = new Sfx(wData.fDataFormat);
        //Debug.Log(sfx);
        //NumSamples = NumBytes / (NumChannels * BitsPerSample / 8)
        int BytePerSample = sfx.n_bits / 8;
        int samplesCount = wData.nDataSize / BytePerSample;
        float[] samples = new float[samplesCount];
        byte[] sample = new byte[BytePerSample];
        for (int i = 0; i < samplesCount; i++)
        {
            Array.Copy(wData.pData, i * sample.Length, sample, 0, sample.Length);
            switch (sfx.n_bits)
            {
                /*
                 * Zn=X?min(X)/max(X)?min(X)
                 */
                case 8:

                    samples[i] = 2 *(float)sample[0] / 255-1;//normalize to [-1,1]




                    break;
                case 16:
                    //samples[i] = BitConverter.ToUInt16(sample) / (float)ushort.MaxValue; //normalize to [0,1]
                    //samples[i] = (samples[i] * 2) - 1; //scale to [-1f,1f]
                    samples[i] = BitConverter.ToInt16(sample) / (float)(short.MaxValue - short.MinValue); //normalize to [-1,1]
                    //samples[i] = (samples[i] * 2) - 1; //scale to [-1f,1f]
                    break;
                default:
                    Debug.Log("Unknown sample rate of " + sfx.n_bits);
                    break;
            }
        }
        //Исправить генерацию имени аудиоклипа на нормальный.
        AudioClip audioClip = AudioClip.Create("default", samplesCount, sfx.n_channels, sfx.sample_rate, false);
        audioClip.SetData(samples, 0);
        return audioClip;
    }

    internal void SetMVolume(int v)
    {
        throw new NotImplementedException();
    }

    internal object Driver()
    {
        return null;
    }

    internal void SetVolumeMod(int v)
    {
        throw new NotImplementedException();
    }

    internal int DefaultPlayFlag()
    {
        return def_play_flag;
    }
}
public struct SoundConfig
{
    //TODO: Переделать структуру-заглушку SoundConfig и реально её использовать.
    public const string SoundRegValue = "Sound";
    public DWORD default_device;       // Use the DSound default device? (bool)
    public DWORD exclusive_level;      // Use DSSCL_EXCLUSIVE ( elsewhere DSSCL_PRIORITY) (bool)
    public DWORD preferred_format;     // Preferred output format
    public DWORD speaker_config;       // Speaker Config
    public DWORD locdefer_flag;        // DSBCAPS_LOCDEFER option (bool)
    //GUID preferred_device;     // GUID of preferred device, if not default
    //GUID soft_3d_algorothm;    // soft 3D algorithm_

    public DWORD channels;             // max channels for use [SoundDialog::Init::min_channels,SoundDialog::Init::max_channels]
    public DWORD no_sound;

    public SoundConfig(bool Load = true)
    {
        default_device = 0;
        exclusive_level = 0;
        preferred_format = 0;
        speaker_config = 0;
        locdefer_flag = 0;
        channels = 0;
        no_sound = 0;
    }

    void SetDefault() { }
    bool Save() { return true; }
};
