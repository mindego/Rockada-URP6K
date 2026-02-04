using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SoundImport
{
    public static AudioClip GetAudioClip(Stream ms, string name)
    {
        WaveData waveData = StormFileUtils.ReadStruct<WaveData>(ms);
        waveData.pData = new byte[waveData.nDataSize];
        //            StormFileUtils.ReadStruct<byte[]>(ms, ms.Position);
        ms.Read(waveData.pData);

        Sfx sfx = new Sfx(waveData.fDataFormat);
        //Debug.Log(sfx);
        //NumSamples = NumBytes / (NumChannels * BitsPerSample / 8)
        int BytePerSample = sfx.n_bits / 8;
        int samplesCount = waveData.nDataSize / BytePerSample;
        float[] samples = new float[samplesCount];
        byte[] sample = new byte[BytePerSample];
        for (int i=0;i<samplesCount;i++)
        {
            Array.Copy(waveData.pData, i * sample.Length, sample, 0, sample.Length);
            switch (sfx.n_bits)
            {
                /*
                 * Zn=X?min(X)/max(X)?min(X)
                 * */
                case 8:
                    samples[i] = (float) BitConverter.ToChar(sample) / (float) Char.MaxValue;
                    break;
                case 16:
                    //samples[i] = BitConverter.ToUInt16(sample) / (float)ushort.MaxValue; //normalize to [0,1]
                    //samples[i] = (samples[i] * 2) - 1; //scale to [-1f,1f]
                    samples[i] = BitConverter.ToInt16(sample) / (float)(short.MaxValue - short.MinValue); //normalize to [0,1]
                    //samples[i] = (samples[i] * 2) - 1; //scale to [-1f,1f]
                    break;
                default:
                    Debug.Log("Unknown sample rate of " + sfx.n_bits);
                    break;
            }
        }
        AudioClip audioClip = AudioClip.Create(name, samplesCount, sfx.n_channels, sfx.sample_rate, false);
        audioClip.SetData(samples,0);
        //StormFileUtils.SaveXML<float[]>("debugsound.xml", samples);
        return audioClip;

    }

    public static float[] ConvertAudio(WaveData waveData)
    {
        return default;
    }
}
