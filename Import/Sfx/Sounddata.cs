using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class WaveData : IStormImportable<WaveData>
{
    public int fDataFormat;
    public int nDataSize;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0)]
    public byte[] pData;

    public override string ToString()
    {
        return "WaveData size: " + nDataSize + "/" + pData.Length;
    }

    WaveData IStormImportable<WaveData>.Import(Stream st)
    {
        WaveData waveData = StormFileUtils.ReadStruct<WaveData>(st);
        waveData.pData = new byte[waveData.nDataSize];
        //            StormFileUtils.ReadStruct<byte[]>(ms, ms.Position);
        st.Read(waveData.pData);

        return waveData;
    }


    //public char[] pData;

    public void Load() {
        if (pData.Length == 0) return;
        //void Load() { LoadDataBlock(MemBlock(pData, nDataSize)); }
        MemoryStream st = new MemoryStream(pData);
        WaveData waveData = StormFileUtils.ReadStruct<WaveData>(st);
        waveData.pData = new byte[waveData.nDataSize];
        //            StormFileUtils.ReadStruct<byte[]>(ms, ms.Position);
        st.Read(waveData.pData);
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class GSoundData : IStormImportable<GSoundData>
{
    public uint wave_code; 
    public int mvolume; 
    public float rate_mod1; 
    public float rate_mod2; 
    public float min_d; 
    public float max_d; 
    public float g_coeff1;     // Int(t) = 1 - t + g_coeff*t;   t in [0..1]
    public int detail;

    public GSoundData Import(Stream st)
    {
        return StormFileUtils.ReadStruct<GSoundData>(st);
    }

    public override string ToString()
    {
        return "GSoundData " + wave_code.ToString("X8");
    }
}
public struct Sfx
{
    public int sample_rate;
    public int n_channels;
    public int n_bits;

    //Sfx(int rate, int chls, int bits) : sample_rate(rate), n_channels(chls), n_bits(bits) { }


    public Sfx(int fmt)
    {
        int sf_code = fmt / 10000;
        sample_rate = sf_code == 8 ? 8000 : (sf_code / 11) * 11025;
        n_bits = fmt % 100;
        n_channels = (fmt % 1000) / 100;
    }

    public Sfx(int sample_rate, int n_channels, int n_bits)
    {
        this.sample_rate = sample_rate;
        this.n_channels = n_channels;
        this.n_bits = n_bits;
    }

    int Format()
    {
        return (sample_rate == 8000 ? 8 : (sample_rate / 11025) * 11) * 10000 + n_bits + n_channels * 100;
    }

    public override string ToString()
    {
        //return "%u Hz, %u-bit %s", sample_rate, n_bits, n_channels == 1 ? "Mono" : "Stereo");
        return string.Format("{0} Hz, {1}-bit {2}", sample_rate, n_bits, n_channels == 1 ? "Mono" : "Stereo");
    }

    //    void ToWFX(WAVEFORMATEX &wfx) const {
    //    wfx.nSamplesPerSec  = sample_rate;
    //wfx.wBitsPerSample = n_bits;
    //wfx.nChannels = n_channels;
    //wfx.nBlockAlign = n_channels* n_bits / 8;
    //    wfx.nAvgBytesPerSec = sample_rate* wfx.nBlockAlign;
    //    wfx.wFormatTag = WAVE_FORMAT_PCM;
    //wfx.cbSize = 0;
    //  }

    //Sfx(const WAVEFORMATEX &wfx) {
    //    if (wfx.wFormatTag != WAVE_FORMAT_PCM || wfx.wBitsPerSample % 8) __asm int 3;
    //    sample_rate = wfx.nSamplesPerSec;
    //    n_bits = wfx.wBitsPerSample;
    //    n_channels = wfx.nChannels;
    //}


    //void ToText(char* buf)
    //{
    //    //wsprintf(buf, "%u Hz, %u-bit %s", sample_rate, n_bits, n_channels == 1 ? "Mono" : "Stereo");
    //}

}
